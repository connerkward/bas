#!/usr/bin/env python3
"""
Apply heavy motion blur (temporal frame averaging = slow shutter) then dither;
output at specified fps (e.g. 12fps).
"""

import argparse
import os
import subprocess
import tempfile
import cv2
import numpy as np
from PIL import Image
from tqdm import tqdm


def zoom_blur(img: np.ndarray, length: float, num_samples: int = 9) -> np.ndarray:
    """Radial/linear zoom blur: smear each pixel along the ray from center (streaks toward center)."""
    h, w = img.shape[:2]
    cx, cy = w / 2.0, h / 2.0
    yy, xx = np.meshgrid(np.arange(h), np.arange(w), indexing="ij")
    dx = xx.astype(np.float32) - cx
    dy = yy.astype(np.float32) - cy
    r = np.sqrt(dx * dx + dy * dy)
    r = np.maximum(r, 1e-6)
    ux = dx / r
    uy = dy / r
    out = np.zeros_like(img, dtype=np.float64)
    for k in range(num_samples):
        t = k / max(1, num_samples - 1)
        r_sample = np.maximum(r - length * (1 - t), 0).astype(np.float32)
        map_x = (cx + ux * r_sample).astype(np.float32)
        map_y = (cy + uy * r_sample).astype(np.float32)
        out += cv2.remap(img, map_x, map_y, cv2.INTER_LINEAR, borderMode=cv2.BORDER_REFLECT)
    out = (out / num_samples).astype(np.uint8)
    return out


def main():
    parser = argparse.ArgumentParser(description="Motion blur + dither video at lower fps")
    parser.add_argument("video_path", type=str)
    parser.add_argument("--output", "-o", type=str, default=None)
    parser.add_argument("--fps", type=float, default=12.0)
    parser.add_argument("--blur-frames", type=int, default=None,
                        help="Number of frames to average for motion blur")
    parser.add_argument("--blur-seconds", type=float, default=None,
                        help="Exposure length in seconds (long exposure smudge); overrides --blur-frames")
    parser.add_argument("--spatial-blur", type=float, default=0,
                        help="Gaussian sigma (px) on blended frame before dither to merge streaks (e.g. 2–4)")
    parser.add_argument("--zoom-blur", type=float, default=0,
                        help="Radial zoom blur length in pixels (linear streaks from center)")
    args = parser.parse_args()

    if args.output is None:
        base = os.path.splitext(os.path.basename(args.video_path))[0]
        args.output = os.path.join(
            os.path.dirname(os.path.abspath(__file__)),
            "outputs", f"{base}_blend_output", "videos", f"dithered_motionblur_{int(args.fps)}fps.mp4"
        )

    os.makedirs(os.path.dirname(args.output), exist_ok=True)

    cap = cv2.VideoCapture(args.video_path)
    if not cap.isOpened():
        raise SystemExit(f"Cannot open {args.video_path}")

    src_fps = cap.get(cv2.CAP_PROP_FPS) or 24.0
    total = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    duration = total / src_fps
    out_frame_count = int(round(duration * args.fps))

    if args.blur_seconds is not None:
        args.blur_frames = max(2, int(round(args.blur_seconds * src_fps)))
        print(f"Motion blur: {args.blur_seconds}s = {args.blur_frames} frames")
    if args.blur_frames is None:
        args.blur_frames = 8
    half_win = args.blur_frames // 2

    # Load all source frames (for temporal blending we need random access)
    print(f"Loading {total} frames @ {src_fps:.1f}fps...")
    frames = []
    for _ in tqdm(range(total), desc="Read"):
        ret, f = cap.read()
        if not ret:
            break
        frames.append(f)
    cap.release()
    n_src = len(frames)
    if n_src == 0:
        raise SystemExit("No frames read")

    with tempfile.TemporaryDirectory() as tmp:
        frame_dir = os.path.join(tmp, "frames")
        os.makedirs(frame_dir, exist_ok=True)

        for i in tqdm(range(out_frame_count), desc="Motion blur + dither"):
            # Center of this output frame in source frame index
            center = (i + 0.5) / args.fps * src_fps
            idx_center = int(round(center))
            start = max(0, idx_center - half_win)
            end = min(n_src, idx_center + half_win + 1)
            window = frames[start:end]
            # Gaussian-weighted temporal average (smoother long exposure, fewer ghost edges)
            n_win = len(window)
            t = np.arange(n_win, dtype=np.float32) - (n_win - 1) / 2.0
            w = np.exp(-0.5 * (t / max(1, n_win * 0.35)) ** 2)
            w /= w.sum()
            blended = np.zeros_like(window[0], dtype=np.float64)
            for k, f in enumerate(window):
                blended += w[k] * f.astype(np.float64)
            blended = np.clip(blended, 0, 255).astype(np.uint8)
            # Optional spatial blur to merge leg streaks
            if args.spatial_blur > 0:
                ksize = int(round(args.spatial_blur * 2.5)) | 1
                blended = cv2.GaussianBlur(blended, (ksize, ksize), args.spatial_blur)
            # Optional radial zoom blur (linear streaks from center)
            if args.zoom_blur > 0:
                blended = zoom_blur(blended, args.zoom_blur)
            # Dither: Floyd-Steinberg on luminance
            gray = cv2.cvtColor(blended, cv2.COLOR_BGR2GRAY)
            pil_gray = Image.fromarray(gray, mode="L")
            dithered = pil_gray.convert("1", dither=Image.Dither.FLOYDSTEINBERG)
            result = np.array(dithered.convert("L"))
            out_path = os.path.join(frame_dir, f"frame_{i:06d}.png")
            Image.fromarray(result, mode="L").save(out_path)

        # Encode video
        input_pattern = os.path.join(frame_dir, "frame_%06d.png")
        cmd = [
            "ffmpeg", "-y", "-framerate", str(args.fps), "-i", input_pattern,
            "-c:v", "libx264", "-pix_fmt", "yuv420p", "-crf", "23",
            args.output
        ]
        subprocess.run(cmd, check=True, capture_output=True)

    print(f"Wrote {args.output} ({out_frame_count} frames @ {args.fps}fps)")


if __name__ == "__main__":
    main()
