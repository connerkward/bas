#!/usr/bin/env python3
"""
Offline render of the Desync chronophoto effect (matches TouchDesigner logic).
Renders frame-perfect PNGs then optionally encodes to video. No real-time encoding.
"""

import argparse
import math
import os
import subprocess
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageFile

ImageFile.LOAD_TRUNCATED_IMAGES = True


# Match TD: 20s cycle, 5s sync / 10s desync (cosine) / 5s sync
CYCLE_SEC = 20.0
SYNC_HOLD_SEC = 5.0
DESYNC_DURATION_SEC = 10.0
SPREAD_FRAMES = 65
FADE_FRAMES = 30
NUM_SOURCE_FRAMES = 600
FPS = 30
# Timeline multipliers (same as timeline0, timeline2, timeline4)
MULTS = (-1, 0, 1)


def desync_val(sec: float) -> float:
    """Desync amount 0..1. Zero during sync, cosine ramp during desync phase."""
    s = sec % CYCLE_SEC
    if s < SYNC_HOLD_SEC or s >= SYNC_HOLD_SEC + DESYNC_DURATION_SEC:
        return 0.0
    x = (s - SYNC_HOLD_SEC) / DESYNC_DURATION_SEC  # 0..1 over 10s
    return 0.5 - 0.5 * math.cos(x * 2 * math.pi)


def frame_index(sec: float, mult: int) -> int:
    """Source frame index for one timeline at time sec. TD uses int() truncation."""
    idx = sec * FPS + mult * SPREAD_FRAMES * desync_val(sec)
    return int(idx) % NUM_SOURCE_FRAMES


def cross_blend_weight(frame_idx: int, num_frames: int = NUM_SOURCE_FRAMES) -> float:
    """Blend weight for wrap (0 = timeline only, 1 = wrap only) in last 30 frames."""
    if num_frames < FADE_FRAMES or frame_idx < num_frames - FADE_FRAMES:
        return 0.0
    return (frame_idx - (num_frames - FADE_FRAMES)) / FADE_FRAMES


# sRGB luminance (matches TD monochromeTOP luminance)
LUM_R, LUM_G, LUM_B = 0.299, 0.587, 0.114


def rgb_to_luminance(rgb: np.ndarray) -> np.ndarray:
    """Convert RGB (H,W,3) to luminance (H,W)."""
    return (rgb[..., 0] * LUM_R + rgb[..., 1] * LUM_G + rgb[..., 2] * LUM_B).astype(np.float32)


def count_frames(frames_dir: Path) -> int:
    """Count frame_XXXX.png files; use for modulo when sequence has != 600 frames."""
    n = 0
    for p in frames_dir.glob("frame_*.png"):
        n = max(n, int(p.stem.split("_")[-1]) + 1)
    return n if n else NUM_SOURCE_FRAMES


def load_frame(frames_dir: Path, idx: int, num_frames: int = NUM_SOURCE_FRAMES) -> np.ndarray:
    """Load one frame as RGB uint8 array (H, W, 3). idx is wrapped by num_frames."""
    idx = int(idx) % num_frames
    path = frames_dir / f"frame_{idx:04d}.png"
    if not path.exists():
        raise FileNotFoundError(f"Missing frame: {path}")
    img = Image.open(path).convert("RGB")
    return np.array(img)


def render_frame(
    frames_dir: Path,
    sec: float,
    invert_mode: int,
    num_frames: int = NUM_SOURCE_FRAMES,
) -> np.ndarray:
    """
    Render one output frame at time sec. Matches TD: cross -> mono (luminance) -> comp (min/max).
    Returns RGB uint8 (H,W,3) with R=G=B (grayscale for out_color).
    num_frames: wrap frame indices for sequences with != 600 frames.
    """
    layers = []
    for mult in MULTS:
        idx = frame_index(sec, mult) % num_frames
        w = cross_blend_weight(idx, num_frames)
        if w <= 0:
            rgb = load_frame(frames_dir, idx, num_frames)
        else:
            wrap_idx = (idx + FADE_FRAMES) % num_frames
            tl = load_frame(frames_dir, idx, num_frames).astype(np.float32)
            wr = load_frame(frames_dir, wrap_idx, num_frames).astype(np.float32)
            rgb = ((1 - w) * tl + w * wr).astype(np.uint8)
        # Monochrome TOP: luminance (same as mono0/mono2/mono4)
        layers.append(rgb_to_luminance(rgb))

    L0, L1, L2 = layers[0], layers[1], layers[2]
    # comp0 = op(mono0, mono2), comp1 = op(comp0, mono4), comp_final = op(comp1, background)
    # operand: minimum when invert_mode 0, maximum when invert_mode 1
    if invert_mode == 0:
        comp = np.minimum(np.minimum(L0, L1), L2)
    else:
        comp = np.maximum(np.maximum(L0, L1), L2)
    # comp_final with background: min(comp, 1) or max(comp, 0) in TD; we output comp as 0-255
    out_uint8 = np.clip(comp.round(), 0, 255).astype(np.uint8)
    # out_color is RGB with R=G=B (grayscale)
    return np.stack([out_uint8, out_uint8, out_uint8], axis=-1)


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Offline render Desync effect (matches TD). Output PNGs then optional MP4."
    )
    parser.add_argument(
        "input_dir",
        type=str,
        help="Directory containing frame_0000.png .. frame_0599.png, or base dir when using --sequences",
    )
    parser.add_argument(
        "--sequences",
        type=str,
        default=None,
        metavar="NAME1,NAME2,...",
        help="Comma-separated subfolder names under input_dir to cycle through (e.g. chroma_dithered,chroma_keyed,raw_frames). Each shown for --seq-period sec.",
    )
    parser.add_argument(
        "--seq-period",
        type=float,
        default=30.0,
        help="Seconds to show each sequence before switching (default: 30). Used with --sequences.",
    )
    parser.add_argument(
        "--trans-time",
        type=float,
        default=2.0,
        help="Crossfade duration in seconds when switching sequences (default: 2). Use 0 for hard cuts.",
    )
    parser.add_argument(
        "-o",
        "--output-dir",
        type=str,
        default=None,
        help="Output directory for PNGs (default: input_dir/../desync_render)",
    )
    parser.add_argument(
        "--duration",
        type=float,
        default=20.0,
        help="Output duration in seconds (default: 20 = one cycle)",
    )
    parser.add_argument(
        "--invert",
        type=int,
        default=0,
        choices=[0, 1],
        help="0 = min blend + white bg, 1 = max blend + black bg (default: 0)",
    )
    parser.add_argument(
        "--no-video",
        action="store_true",
        help="Only write PNGs, do not run ffmpeg",
    )
    parser.add_argument(
        "--video-path",
        type=str,
        default=None,
        help="Output video path (default: output_dir/desync_output.mp4)",
    )
    parser.add_argument(
        "--output-fps",
        type=float,
        default=30.0,
        help="Video playback fps (default: 30). Use 60 or 120 for faster playback to match TD preview feel.",
    )
    parser.add_argument(
        "--compare-sec",
        type=float,
        default=None,
        metavar="SEC",
        help="Only render one frame at this time (sec) and save as compare.png for side-by-side with TD export.",
    )
    args = parser.parse_args()

    base_dir = Path(args.input_dir)
    if not base_dir.is_dir():
        print(f"Error: not a directory: {base_dir}", file=sys.stderr)
        sys.exit(1)

    if args.sequences:
        seq_names = [s.strip() for s in args.sequences.split(",") if s.strip()]
        sequence_dirs = [base_dir / name for name in seq_names]
        for d in sequence_dirs:
            if not d.is_dir():
                print(f"Error: sequence dir not found: {d}", file=sys.stderr)
                sys.exit(1)
            if not (d / "frame_0000.png").exists():
                print(f"Error: expected frame_0000.png in {d}", file=sys.stderr)
                sys.exit(1)
        frames_dirs = sequence_dirs
        frame_counts = [count_frames(d) for d in frames_dirs]
        print(f"Multi-sequence: {len(frames_dirs)} sequences, {args.seq_period}s each: {seq_names}")
        print(f"  Frame counts: {frame_counts}")
    else:
        if not (base_dir / "frame_0000.png").exists():
            print(f"Error: expected frame_0000.png in {base_dir}", file=sys.stderr)
            sys.exit(1)
        frames_dirs = [base_dir]
        frame_counts = [count_frames(base_dir)]

    out_dir = Path(args.output_dir) if args.output_dir else base_dir.parent / "desync_render"
    out_dir.mkdir(parents=True, exist_ok=True)

    if args.compare_sec is not None:
        sec = args.compare_sec
        frames_dir = frames_dirs[0]
        nf = frame_counts[0]
        idx0 = frame_index(sec, MULTS[0]) % nf
        idx1 = frame_index(sec, MULTS[1]) % nf
        idx2 = frame_index(sec, MULTS[2]) % nf
        print(f"Compare frame at t={sec}s: desync_val={desync_val(sec):.4f}, timeline indices {idx0}, {idx1}, {idx2}")
        out_arr = render_frame(frames_dir, sec, args.invert, nf)
        compare_path = out_dir / "compare.png"
        Image.fromarray(out_arr).save(compare_path)
        print(f"Saved {compare_path} – export the same frame from TD out_color and compare.")
        return

    num_out_frames = int(round(args.duration * FPS))
    n_seq = len(frames_dirs)
    print(f"Rendering {num_out_frames} frames @ {FPS} fps (duration {args.duration}s) -> {out_dir}")

    trans_time = max(0.0, args.trans_time) if n_seq > 1 else 0.0
    if n_seq > 1 and trans_time > 0:
        print(f"  Crossfade: {trans_time}s between sequences")

    for i in range(num_out_frames):
        t_sec = i / FPS
        if n_seq > 1:
            seq_idx = int(t_sec / args.seq_period) % n_seq
            sec_in_segment = t_sec % args.seq_period
            next_idx = (seq_idx + 1) % n_seq
            # During last trans_time seconds of segment, blend to next sequence
            if trans_time > 0 and sec_in_segment >= args.seq_period - trans_time:
                blend = (sec_in_segment - (args.seq_period - trans_time)) / trans_time
                next_sec = sec_in_segment - (args.seq_period - trans_time)  # 0..trans_time into next
                curr = render_frame(
                    frames_dirs[seq_idx], sec_in_segment, args.invert, frame_counts[seq_idx]
                ).astype(np.float32)
                next_arr = render_frame(
                    frames_dirs[next_idx], next_sec, args.invert, frame_counts[next_idx]
                ).astype(np.float32)
                out_arr = (1 - blend) * curr + blend * next_arr
                out_arr = np.clip(out_arr.round(), 0, 255).astype(np.uint8)
            else:
                # Content time: after first segment, add trans_time so we don't jump when
                # crossfade ends (we were showing next at 0..trans_time, now continue from trans_time)
                if t_sec < args.seq_period:
                    content_sec = sec_in_segment  # first segment plays from 0
                else:
                    content_sec = trans_time + sec_in_segment
                out_arr = render_frame(
                    frames_dirs[seq_idx], content_sec, args.invert, frame_counts[seq_idx]
                )
        else:
            frames_dir = frames_dirs[0]
            sec = t_sec
            num_frames = frame_counts[0]
            out_arr = render_frame(frames_dir, sec, args.invert, num_frames)
        out_path = out_dir / f"frame_{i:04d}.png"
        Image.fromarray(out_arr).save(out_path)
        if (i + 1) % 100 == 0:
            print(f"  {i + 1}/{num_out_frames}")

    print(f"Wrote {num_out_frames} PNGs to {out_dir}")

    if args.no_video:
        return

    video_path = args.video_path or str(out_dir / "desync_output.mp4")
    pattern = str(out_dir / "frame_%04d.png")
    out_fps = args.output_fps
    cmd = [
        "ffmpeg", "-y", "-framerate", str(out_fps), "-i", pattern,
        "-c:v", "libx264", "-pix_fmt", "yuv420p", "-profile:v", "main",
        "-movflags", "+faststart", "-crf", "20", video_path,
    ]
    duration_sec = num_out_frames / out_fps
    print(f"Encoding video: {video_path} @ {out_fps} fps ({duration_sec:.1f}s playback)")
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        print(result.stderr, file=sys.stderr)
        sys.exit(1)
    print(f"Done: {video_path}")


if __name__ == "__main__":
    main()
