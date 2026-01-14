#!/usr/bin/env python3
"""
Extract frames from video, run Depth Anything V2, overlay depth maps with blend modes.
Supports both green screen and regular videos with auto-detection.
"""

import argparse
import os
import cv2
import numpy as np
import torch
from PIL import Image
from transformers import pipeline
from concurrent.futures import ThreadPoolExecutor
import time
from tqdm import tqdm


def log(step: str, detail: str = "", frame: int = None, total: int = None):
    """Unified logging with step/frame info."""
    timestamp = time.strftime("%H:%M:%S")
    if frame is not None and total is not None:
        print(f"[{timestamp}] [{step}] ({frame}/{total}) {detail}")
    else:
        print(f"[{timestamp}] [{step}] {detail}")


def detect_green_screen(frame: np.ndarray, threshold: float = 0.15) -> bool:
    """Auto-detect if frame has green screen background."""
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    # Green range
    lower_green = np.array([35, 40, 40])
    upper_green = np.array([85, 255, 255])
    mask = cv2.inRange(hsv, lower_green, upper_green)
    green_ratio = np.sum(mask > 0) / mask.size
    return green_ratio > threshold


def analyze_video_for_greenscreen(video_path: str, sample_frames: int = 5) -> tuple[bool, float]:
    """Sample video to detect green screen. Returns (has_greenscreen, green_ratio)."""
    cap = cv2.VideoCapture(video_path)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    indices = np.linspace(0, total_frames - 1, sample_frames, dtype=int)
    
    green_ratios = []
    for idx in indices:
        cap.set(cv2.CAP_PROP_POS_FRAMES, idx)
        ret, frame = cap.read()
        if ret:
            hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
            lower_green = np.array([35, 40, 40])
            upper_green = np.array([85, 255, 255])
            mask = cv2.inRange(hsv, lower_green, upper_green)
            green_ratios.append(np.sum(mask > 0) / mask.size)
    cap.release()
    
    avg_green = np.mean(green_ratios) if green_ratios else 0
    return avg_green > 0.15, avg_green


def chroma_key_green(frame: np.ndarray, soft_edge: bool = True) -> tuple[np.ndarray, np.ndarray]:
    """Remove green screen, return (frame with black bg, mask)."""
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    lower_green = np.array([35, 40, 40])
    upper_green = np.array([85, 255, 255])
    mask = cv2.inRange(hsv, lower_green, upper_green)
    
    if soft_edge:
        kernel = np.ones((3, 3), np.uint8)
        mask = cv2.dilate(mask, kernel, iterations=1)
        mask = cv2.GaussianBlur(mask, (5, 5), 0)
    else:
        kernel = np.ones((5, 5), np.uint8)
        mask = cv2.dilate(mask, kernel, iterations=2)
    
    mask_inv = cv2.bitwise_not(mask)
    result = cv2.bitwise_and(frame, frame, mask=mask_inv)
    return result, mask_inv


def extract_subject_mask(frame: np.ndarray, has_greenscreen: bool) -> np.ndarray:
    """Extract subject mask - adaptive for greenscreen vs regular video."""
    if has_greenscreen:
        _, mask = chroma_key_green(frame)
        return mask
    else:
        # For regular videos: use edge detection + brightness
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        # Adaptive threshold based on frame content
        mean_val = np.mean(gray)
        std_val = np.std(gray)
        # Dynamic threshold: darker scenes get lower threshold
        thresh = max(10, min(mean_val - std_val * 0.5, 80))
        _, mask = cv2.threshold(gray, thresh, 255, cv2.THRESH_BINARY)
        # Clean up mask
        kernel = np.ones((3, 3), np.uint8)
        mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, kernel)
        mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)
        return mask


def process_frame_for_output(frame: np.ndarray, has_greenscreen: bool) -> np.ndarray:
    """Process frame for output - apply chroma key only if green screen detected."""
    if has_greenscreen:
        result, _ = chroma_key_green(frame)
        return result
    else:
        return frame.copy()


def extract_frames(video_path: str, num_frames: int, output_dir: str, 
                   target_fps: float = None, has_greenscreen: bool = False) -> list[str]:
    """Extract evenly spaced frames from video."""
    cap = cv2.VideoCapture(video_path)
    if not cap.isOpened():
        raise ValueError(f"Cannot open video: {video_path}")
    
    video_fps = cap.get(cv2.CAP_PROP_FPS)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    duration = total_frames / video_fps if video_fps > 0 else 0
    
    if target_fps is not None:
        num_frames = int(duration * target_fps)
        log("EXTRACT", f"Duration: {duration:.2f}s @ {target_fps}fps = {num_frames} frames")
    
    frame_indices = np.linspace(0, total_frames - 1, num_frames, dtype=int)
    os.makedirs(output_dir, exist_ok=True)
    frame_paths = []
    
    for idx, frame_num in enumerate(tqdm(frame_indices, desc="Extracting frames", unit="frame")):
        cap.set(cv2.CAP_PROP_POS_FRAMES, frame_num)
        ret, frame = cap.read()
        if ret:
            processed = process_frame_for_output(frame, has_greenscreen)
            frame_path = os.path.join(output_dir, f"frame_{idx:04d}.png")
            cv2.imwrite(frame_path, processed)
            frame_paths.append(frame_path)
    
    cap.release()
    log("EXTRACT", f"Done - {len(frame_paths)} frames (greenscreen={has_greenscreen})")
    return frame_paths


def compute_frame_difference(frame1: np.ndarray, frame2: np.ndarray) -> float:
    """Compute structural difference between frames."""
    if len(frame1.shape) == 3:
        gray1 = cv2.cvtColor(frame1, cv2.COLOR_BGR2GRAY)
    else:
        gray1 = frame1
    if len(frame2.shape) == 3:
        gray2 = cv2.cvtColor(frame2, cv2.COLOR_BGR2GRAY)
    else:
        gray2 = frame2
    
    small_size = (128, 128)
    g1 = cv2.resize(gray1, small_size)
    g2 = cv2.resize(gray2, small_size)
    
    abs_diff = np.mean(np.abs(g1.astype(float) - g2.astype(float)))
    edges1 = cv2.Canny(g1, 50, 150)
    edges2 = cv2.Canny(g2, 50, 150)
    edge_diff = np.mean(np.abs(edges1.astype(float) - edges2.astype(float)))
    
    return abs_diff + edge_diff * 0.3


def select_diverse_frames(frames: list[np.ndarray], num_select: int) -> list[int]:
    """Select maximally diverse frames using greedy farthest-point sampling."""
    if num_select >= len(frames):
        return list(range(len(frames)))
    
    n = len(frames)
    selected = [0]
    min_diffs = [compute_frame_difference(frames[0], frames[i]) for i in range(n)]
    min_diffs[0] = -1
    
    while len(selected) < num_select:
        best_idx = np.argmax(min_diffs)
        selected.append(best_idx)
        min_diffs[best_idx] = -1
        for i in range(n):
            if min_diffs[i] >= 0:
                d = compute_frame_difference(frames[best_idx], frames[i])
                min_diffs[i] = min(min_diffs[i], d)
    
    return sorted(selected)


def blend_lighten(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    return np.maximum(base, overlay)


def blend_add(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    return np.clip(base.astype(np.float32) + overlay.astype(np.float32), 0, 255).astype(np.uint8)


def blend_screen(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    base_f = base.astype(np.float32) / 255.0
    overlay_f = overlay.astype(np.float32) / 255.0
    result = 1 - (1 - base_f) * (1 - overlay_f)
    return (result * 255).astype(np.uint8)


def create_chronophotography(depth_images: list[np.ndarray], mode: str = "lighten") -> np.ndarray:
    """Create Marey-style chronophotography from multiple depth maps."""
    if len(depth_images) == 0:
        return None
    
    result = depth_images[0].astype(np.float32)
    
    if mode == "lighten":
        for img in depth_images[1:]:
            result = np.maximum(result, img.astype(np.float32))
        return result.astype(np.uint8)
    
    elif mode == "add":
        for img in depth_images[1:]:
            result += img.astype(np.float32)
        result = (result - result.min()) / (result.max() - result.min() + 1e-6) * 255
        return result.astype(np.uint8)
    
    elif mode == "screen":
        result = result / 255.0
        for img in depth_images[1:]:
            overlay = img.astype(np.float32) / 255.0
            result = 1 - (1 - result) * (1 - overlay)
        return (result * 255).astype(np.uint8)
    
    elif mode == "average":
        for img in depth_images[1:]:
            result += img.astype(np.float32)
        result /= len(depth_images)
        return result.astype(np.uint8)
    
    elif mode == "darken":
        for img in depth_images[1:]:
            result = np.minimum(result, img.astype(np.float32))
        return result.astype(np.uint8)
    
    elif mode == "lighten_add":
        lighten_result = depth_images[0].astype(np.float32)
        for img in depth_images[1:]:
            lighten_result = np.maximum(lighten_result, img.astype(np.float32))
        add_result = depth_images[0].astype(np.float32)
        for img in depth_images[1:]:
            add_result += img.astype(np.float32)
        add_result = (add_result - add_result.min()) / (add_result.max() - add_result.min() + 1e-6) * 255
        hybrid = lighten_result * 0.7 + add_result * 0.3
        return np.clip(hybrid, 0, 255).astype(np.uint8)
    
    else:
        return depth_images[0]


BLEND_MODES = ["lighten", "add", "screen", "average", "darken", "lighten_add"]


def process_dither_frame(args):
    """Thread worker for dithering."""
    idx, frame_path, output_dir, dither_type = args
    frame = cv2.imread(frame_path)
    if frame is None:
        return None
    
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    h, w = gray.shape
    
    if dither_type == "floyd":
        pil_gray = Image.fromarray(gray, mode='L')
        dithered = pil_gray.convert('1', dither=Image.Dither.FLOYDSTEINBERG)
        result = np.array(dithered.convert('L'))
    
    elif dither_type == "atkinson":
        gray_f = gray.astype(np.float32)
        for y in range(h):
            for x in range(w):
                old_pixel = gray_f[y, x]
                new_pixel = 255.0 if old_pixel > 127 else 0.0
                gray_f[y, x] = new_pixel
                error = (old_pixel - new_pixel) / 8.0
                if x + 1 < w: gray_f[y, x + 1] += error
                if x + 2 < w: gray_f[y, x + 2] += error
                if y + 1 < h:
                    if x > 0: gray_f[y + 1, x - 1] += error
                    gray_f[y + 1, x] += error
                    if x + 1 < w: gray_f[y + 1, x + 1] += error
                if y + 2 < h: gray_f[y + 2, x] += error
        result = np.clip(gray_f, 0, 255).astype(np.uint8)
    
    elif dither_type == "bayer":
        bayer_matrix = np.array([
            [0, 8, 2, 10], [12, 4, 14, 6],
            [3, 11, 1, 9], [15, 7, 13, 5]
        ], dtype=np.float32) / 16.0 * 255.0
        threshold = np.tile(bayer_matrix, (h // 4 + 1, w // 4 + 1))[:h, :w]
        result = (gray.astype(np.float32) > threshold).astype(np.uint8) * 255
    
    else:
        result = gray
    
    out_path = os.path.join(output_dir, f"frame_{idx:04d}.png")
    Image.fromarray(result, mode='L').save(out_path)
    return result


def process_pixel_frame(args):
    """Thread worker for pixelated frames with adaptive threshold."""
    idx, frame_path, output_dir, scale, frame_stats = args
    frame = cv2.imread(frame_path)
    if frame is None:
        return None
    
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    h, w = gray.shape
    
    # Adaptive threshold based on frame statistics
    mean_val, std_val = frame_stats
    # Lower threshold for darker videos, higher for brighter
    base_thresh = max(5, mean_val * 0.15)
    
    small = cv2.resize(gray, (max(1, w // scale), max(1, h // scale)), interpolation=cv2.INTER_AREA)
    
    # Use Otsu's method for automatic thresholding
    _, binary = cv2.threshold(small, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    
    result = cv2.resize(binary, (w, h), interpolation=cv2.INTER_NEAREST)
    
    out_path = os.path.join(output_dir, f"frame_{idx:04d}.png")
    Image.fromarray(result, mode='L').save(out_path)
    return result


def compute_frame_stats(frame_paths: list[str]) -> tuple[float, float]:
    """Compute mean/std brightness across all frames for adaptive thresholding."""
    values = []
    for path in frame_paths[:min(10, len(frame_paths))]:
        frame = cv2.imread(path)
        if frame is not None:
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            values.append(np.mean(gray))
    return (np.mean(values), np.std(values)) if values else (128, 50)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("video_path", type=str)
    parser.add_argument("--num-frames", type=int, default=10)
    parser.add_argument("--target-fps", type=float, default=12.0, help="Extract frames at this fps")
    parser.add_argument("--model", type=str, default="depth-anything/Depth-Anything-V2-Small-hf")
    parser.add_argument("--device", type=str, default="mps" if torch.backends.mps.is_available() else ("cuda" if torch.cuda.is_available() else "cpu"))
    parser.add_argument("--blend-modes", type=str, nargs="+", default=[],
                        choices=BLEND_MODES, help="Chronophotography blend modes (disabled by default)")
    parser.add_argument("--alpha", type=float, default=0.5)
    parser.add_argument("--effects", type=str, nargs="+", 
                        default=["rainbow_trail", "microres", "lowres", "dithered", "depth", "red_overlay", "atkinson"],
                        choices=["rainbow_trail", "microres", "lowres", "dithered", "depth", "red_overlay", "atkinson", "extract", "bayer", "depth_banding"],
                        help="Which effects to generate")
    parser.add_argument("--output-dir", type=str, default=None)
    parser.add_argument("--force-greenscreen", action="store_true", help="Force green screen mode")
    parser.add_argument("--force-no-greenscreen", action="store_true", help="Force regular video mode")
    parser.add_argument("--workers", type=int, default=4, help="Number of worker threads")
    parser.add_argument("--batch-size", type=int, default=4, help="Batch size for depth estimation")
    
    args = parser.parse_args()
    
    # Generate output dir from video filename, defaulting under pre_render/outputs
    if args.output_dir is None:
        script_dir = os.path.dirname(os.path.abspath(__file__))
        default_root = os.path.join(script_dir, "outputs")
        os.makedirs(default_root, exist_ok=True)
        video_basename = os.path.splitext(os.path.basename(args.video_path))[0]
        args.output_dir = os.path.join(default_root, f"{video_basename}_blend_output")
    
    log("INIT", f"Processing {args.video_path}")
    log("INIT", f"Device: {args.device}, Workers: {args.workers}, Batch: {args.batch_size}")
    
    # Auto-detect green screen
    if args.force_greenscreen:
        has_greenscreen = True
        log("DETECT", "Forced green screen mode")
    elif args.force_no_greenscreen:
        has_greenscreen = False
        log("DETECT", "Forced regular video mode")
    else:
        has_greenscreen, green_ratio = analyze_video_for_greenscreen(args.video_path)
        log("DETECT", f"Green ratio: {green_ratio:.2%} -> {'GREEN SCREEN' if has_greenscreen else 'REGULAR VIDEO'}")
    
    # Extract frames
    frames_dir = os.path.join(args.output_dir, "frames")
    log("EXTRACT", f"Starting @ {args.target_fps}fps...")
    frame_paths = extract_frames(args.video_path, args.num_frames, frames_dir, 
                                  target_fps=args.target_fps, has_greenscreen=has_greenscreen)
    
    # Compute frame stats for adaptive thresholding
    frame_stats = compute_frame_stats(frame_paths)
    log("STATS", f"Frame brightness: mean={frame_stats[0]:.1f}, std={frame_stats[1]:.1f}")
    
    # Load Depth Anything V2
    log("MODEL", f"Loading {args.model}...")
    pipe = pipeline(task="depth-estimation", model=args.model, device=args.device)
    
    # Create output dirs
    depth_maps_dir = os.path.join(args.output_dir, "depth_maps")
    os.makedirs(depth_maps_dir, exist_ok=True)
    for mode in args.blend_modes:
        os.makedirs(os.path.join(args.output_dir, mode), exist_ok=True)
    
    # Batch depth estimation for GPU efficiency
    log("DEPTH", f"Running depth estimation on {len(frame_paths)} frames...")
    depth_images = []
    
    # Process in batches
    num_batches = (len(frame_paths) + args.batch_size - 1) // args.batch_size
    with tqdm(total=len(frame_paths), desc="Depth estimation", unit="frame") as pbar:
        for batch_start in range(0, len(frame_paths), args.batch_size):
            batch_end = min(batch_start + args.batch_size, len(frame_paths))
            batch_paths = frame_paths[batch_start:batch_end]
            
            # Load batch images
            batch_images = [Image.open(p) for p in batch_paths]
            
            # Run depth on batch
            results = pipe(batch_images)
            
            for i, (result, frame_path) in enumerate(zip(results, batch_paths)):
                idx = batch_start + i
                depth_map = result["depth"]
                depth_array = np.array(depth_map)
                
                # Get subject mask
                orig_frame = cv2.imread(frame_path)
                fg_mask = extract_subject_mask(orig_frame, has_greenscreen)
                
                # Resize mask if needed
                if fg_mask.shape != depth_array.shape:
                    fg_mask = cv2.resize(fg_mask, (depth_array.shape[1], depth_array.shape[0]))
                
                # Normalize depth
                depth_min, depth_max = depth_array.min(), depth_array.max()
                if depth_max > depth_min:
                    depth_norm = (depth_array - depth_min) / (depth_max - depth_min)
                else:
                    depth_norm = np.zeros_like(depth_array, dtype=np.float32)
                
                depth_gray = (depth_norm * 255).astype(np.uint8)
                
                # For greenscreen: mask background. For regular: keep full depth
                if has_greenscreen:
                    depth_gray = cv2.bitwise_and(depth_gray, depth_gray, mask=fg_mask)
                
                depth_images.append(depth_gray)
                
                path = os.path.join(depth_maps_dir, f"depth_{idx:04d}.png")
                Image.fromarray(depth_gray, mode='L').save(path)
            
            pbar.update(len(batch_paths))
    
    log("DEPTH", f"Done - {len(depth_images)} depth maps")
    
    # Create depth chronophoto
    depth_chrono = create_chronophotography(depth_images, "lighten_add")
    path = os.path.join(depth_maps_dir, "chronophoto.png")
    Image.fromarray(depth_chrono, mode='L').save(path)
    log("CHRONO", "Created depth chronophoto")
    
    # Resize depths to common size
    target_shape = depth_images[0].shape
    resized_depths = []
    for img in depth_images:
        if img.shape != target_shape:
            img = cv2.resize(img, (target_shape[1], target_shape[0]))
        resized_depths.append(img)
    
    # Load original frames
    log("FRAMES", "Loading original frames...")
    original_frames = []
    for frame_path in frame_paths:
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        frame = cv2.cvtColor(gray, cv2.COLOR_GRAY2BGR)
        if frame.shape[:2] != target_shape:
            frame = cv2.resize(frame, (target_shape[1], target_shape[0]))
        original_frames.append(frame)
    
    # Parallel dithering
    dithered_images = []
    if "dithered" in args.effects or "red_overlay" in args.effects:
        log("DITHER", "Creating dithered frames (parallel)...")
        dithered_dir = os.path.join(args.output_dir, "dithered")
        os.makedirs(dithered_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            floyd_args = [(i, p, dithered_dir, "floyd") for i, p in enumerate(frame_paths)]
            floyd_results = list(tqdm(executor.map(process_dither_frame, floyd_args), 
                                      total=len(floyd_args), desc="Dithering (Floyd-Steinberg)", unit="frame"))
            dithered_images = [r for r in floyd_results if r is not None]
    
    if "atkinson" in args.effects:
        log("DITHER", "Creating Atkinson dithered frames...")
        atkinson_dir = os.path.join(args.output_dir, "atkinson")
        os.makedirs(atkinson_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            atk_args = [(i, p, atkinson_dir, "atkinson") for i, p in enumerate(frame_paths)]
            list(tqdm(executor.map(process_dither_frame, atk_args), 
                      total=len(atk_args), desc="Dithering (Atkinson)", unit="frame"))
    
    if "bayer" in args.effects:
        log("DITHER", "Creating Bayer dithered frames...")
        bayer_dir = os.path.join(args.output_dir, "bayer")
        os.makedirs(bayer_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            bayer_args = [(i, p, bayer_dir, "bayer") for i, p in enumerate(frame_paths)]
            list(tqdm(executor.map(process_dither_frame, bayer_args), 
                      total=len(bayer_args), desc="Dithering (Bayer)", unit="frame"))
    
    # Parallel pixelated frames with adaptive threshold
    if "extract" in args.effects:
        log("PIXEL", "Creating extract frames...")
        extract_dir = os.path.join(args.output_dir, "extract")
        os.makedirs(extract_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            extract_args = [(i, p, extract_dir, 8, frame_stats) for i, p in enumerate(frame_paths)]
            list(tqdm(executor.map(process_pixel_frame, extract_args), 
                      total=len(extract_args), desc="Pixelation (Extract)", unit="frame"))
    
    if "lowres" in args.effects:
        log("PIXEL", "Creating lowres frames...")
        lowres_dir = os.path.join(args.output_dir, "lowres")
        os.makedirs(lowres_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            lowres_args = [(i, p, lowres_dir, 16, frame_stats) for i, p in enumerate(frame_paths)]
            list(tqdm(executor.map(process_pixel_frame, lowres_args), 
                      total=len(lowres_args), desc="Pixelation (Lowres)", unit="frame"))
    
    if "microres" in args.effects:
        log("PIXEL", "Creating microres frames...")
        microres_dir = os.path.join(args.output_dir, "microres")
        os.makedirs(microres_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            micro_args = [(i, p, microres_dir, 24, frame_stats) for i, p in enumerate(frame_paths)]
            list(tqdm(executor.map(process_pixel_frame, micro_args), 
                      total=len(micro_args), desc="Pixelation (Microres)", unit="frame"))
    
    # Red overlay frames
    if "red_overlay" in args.effects:
        log("EFFECT", "Creating red overlay frames...")
        red_overlay_dir = os.path.join(args.output_dir, "red_overlay")
        os.makedirs(red_overlay_dir, exist_ok=True)
        for idx, frame_path in enumerate(tqdm(frame_paths, desc="Red overlay", unit="frame")):
            frame = cv2.imread(frame_path)
            if frame is None:
                continue
            h, w = frame.shape[:2]
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            dark_base = np.clip(gray.astype(np.float32) * 0.3, 0, 255).astype(np.uint8)
            
            if idx < len(dithered_images):
                dith = dithered_images[idx]
                if dith.shape != (h, w):
                    dith = cv2.resize(dith, (w, h))
                
                result = np.zeros((h, w, 3), dtype=np.uint8)
                result[:, :, 0] = np.clip(dark_base * 0.4, 0, 60).astype(np.uint8)
                result[:, :, 1] = np.clip(dark_base * 0.2, 0, 40).astype(np.uint8)
                result[:, :, 2] = np.clip(dark_base * 0.3, 0, 50).astype(np.uint8)
                
                dith_mask = dith > 127
                result[:, :, 2] = np.where(dith_mask, 255, result[:, :, 2])
                result[:, :, 1] = np.where(dith_mask, 80, result[:, :, 1])
                result[:, :, 0] = np.where(dith_mask, 20, result[:, :, 0])
                
                Image.fromarray(cv2.cvtColor(result, cv2.COLOR_BGR2RGB)).save(
                    os.path.join(red_overlay_dir, f"frame_{idx:04d}.png"))
        log("EFFECT", "Red overlay done")
    
    # Rainbow trail
    if "rainbow_trail" in args.effects:
        log("EFFECT", "Creating rainbow trail frames...")
        rainbow_dir = os.path.join(args.output_dir, "rainbow_trail")
        os.makedirs(rainbow_dir, exist_ok=True)
        
        for idx, frame_path in enumerate(tqdm(frame_paths, desc="Rainbow trail", unit="frame")):
            frame = cv2.imread(frame_path)
            if frame is None:
                continue
            h, w = frame.shape[:2]
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            result = np.zeros((h, w, 3), dtype=np.float32)
            
            trail_frames = max(0, idx - 8)
            for t_idx in range(trail_frames, idx + 1):
                t_frame = cv2.imread(frame_paths[t_idx])
                if t_frame is None:
                    continue
                t_gray = cv2.cvtColor(t_frame, cv2.COLOR_BGR2GRAY).astype(np.float32)
                if t_gray.shape != (h, w):
                    t_gray = cv2.resize(t_gray, (w, h))
                
                time_offset = (idx - t_idx) / 8.0
                hue = (time_offset * 0.7) % 1.0
                
                if hue < 1/6:
                    r, g, b = 1.0, hue * 6, 0
                elif hue < 2/6:
                    r, g, b = 1 - (hue - 1/6) * 6, 1.0, 0
                elif hue < 3/6:
                    r, g, b = 0, 1.0, (hue - 2/6) * 6
                elif hue < 4/6:
                    r, g, b = 0, 1 - (hue - 3/6) * 6, 1.0
                elif hue < 5/6:
                    r, g, b = (hue - 4/6) * 6, 0, 1.0
                else:
                    r, g, b = 1.0, 0, 1 - (hue - 5/6) * 6
                
                fade = 1.0 - (time_offset * 0.7)
                intensity = t_gray / 255.0 * fade
                blurred = cv2.GaussianBlur(intensity, (15, 15), 0)
                result[:, :, 2] += blurred * r * 180
                result[:, :, 1] += blurred * g * 180
                result[:, :, 0] += blurred * b * 180
            
            current_bright = gray.astype(np.float32) / 255.0
            bright_mask = current_bright > 0.6
            result[:, :, 0] = np.where(bright_mask, np.clip(result[:, :, 0] + current_bright * 200, 0, 255), result[:, :, 0])
            result[:, :, 1] = np.where(bright_mask, np.clip(result[:, :, 1] + current_bright * 200, 0, 255), result[:, :, 1])
            result[:, :, 2] = np.where(bright_mask, np.clip(result[:, :, 2] + current_bright * 200, 0, 255), result[:, :, 2])
            
            noise = np.random.normal(0, 8, (h, w, 3))
            result = np.clip(result + noise, 0, 255).astype(np.uint8)
            Image.fromarray(cv2.cvtColor(result, cv2.COLOR_BGR2RGB)).save(
                os.path.join(rainbow_dir, f"frame_{idx:04d}.png"))
        log("EFFECT", "Rainbow trail done")
    
    # Depth banding
    if "depth_banding" in args.effects:
        log("EFFECT", "Creating depth banding frames...")
        banding_dir = os.path.join(args.output_dir, "depth_banding")
        os.makedirs(banding_dir, exist_ok=True)
        
        for idx, frame_path in enumerate(tqdm(frame_paths, desc="Depth banding", unit="frame")):
            frame = cv2.imread(frame_path)
            if frame is None:
                continue
            h, w = frame.shape[:2]
            
            if idx < len(depth_images):
                depth = depth_images[idx]
                if depth.shape != (h, w):
                    depth = cv2.resize(depth, (w, h))
            else:
                depth = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            
            result = np.zeros((h, w), dtype=np.uint8)
            
            for y in range(h):
                for x in range(w):
                    d = depth[y, x]
                    if d < 10:
                        continue
                    line_spacing = max(2, int(20 - d / 15))
                    wave = int(np.sin(y * 0.1 + d * 0.05) * 3)
                    if (y + wave) % line_spacing < 2:
                        result[y, x] = 255
            
            noise_mask = np.random.random((h, w)) < (depth.astype(np.float32) / 255.0 * 0.1)
            result = np.where(noise_mask, 255, result).astype(np.uint8)
            Image.fromarray(result, mode='L').save(os.path.join(banding_dir, f"frame_{idx:04d}.png"))
        log("EFFECT", "Depth banding done")
    
    # Chronophotography composites (only if blend modes specified)
    if args.blend_modes:
        log("CHRONO", "Creating blend composites...")
    for mode in args.blend_modes:
        for idx in tqdm(range(len(resized_depths)), desc=f"Blend ({mode})", unit="frame"):
            frames_so_far = resized_depths[:idx + 1]
            depth_result = create_chronophotography(frames_so_far, mode)
            depth_rgb = cv2.cvtColor(depth_result, cv2.COLOR_GRAY2RGB)
            
            if idx < len(original_frames):
                raw_frame = original_frames[idx]
                blended = cv2.addWeighted(depth_rgb, 1.0 - args.alpha, raw_frame, args.alpha, 0)
            else:
                blended = depth_rgb
            
            path = os.path.join(args.output_dir, mode, f"frame_{idx:04d}.png")
            Image.fromarray(blended).save(path)
        
        depth_result = create_chronophotography(resized_depths, mode)
        depth_rgb = cv2.cvtColor(depth_result, cv2.COLOR_GRAY2RGB)
        
        if original_frames:
            avg_raw = np.mean([f.astype(np.float32) for f in original_frames], axis=0).astype(np.uint8)
            blended = cv2.addWeighted(depth_rgb, 1.0 - args.alpha, avg_raw, args.alpha, 0)
        else:
            blended = depth_rgb
        
        path = os.path.join(args.output_dir, mode, "chronophoto.png")
        Image.fromarray(blended).save(path)
        log("CHRONO", f"{mode} done")
    
    # Create videos
    log("VIDEO", "Creating videos from passes...")
    videos_dir = os.path.join(args.output_dir, "videos")
    os.makedirs(videos_dir, exist_ok=True)
    
    # Build list of folders that were actually generated
    pass_folders = ["frames"]
    if "depth" in args.effects:
        pass_folders.append("depth_maps")
    effect_to_folder = {
        "dithered": "dithered", "atkinson": "atkinson", "bayer": "bayer",
        "extract": "extract", "lowres": "lowres", "microres": "microres",
        "red_overlay": "red_overlay", "rainbow_trail": "rainbow_trail", "depth_banding": "depth_banding"
    }
    for effect, folder in effect_to_folder.items():
        if effect in args.effects:
            pass_folders.append(folder)
    pass_folders.extend(args.blend_modes)
    
    for folder in tqdm(pass_folders, desc="Creating videos", unit="video"):
        folder_path = os.path.join(args.output_dir, folder)
        if not os.path.exists(folder_path):
            continue
        
        frame_files = sorted([f for f in os.listdir(folder_path) if f.startswith("frame_") or f.startswith("depth_")])
        if not frame_files:
            continue
        
        first_frame = cv2.imread(os.path.join(folder_path, frame_files[0]))
        if first_frame is None:
            first_frame = cv2.imread(os.path.join(folder_path, frame_files[0]), cv2.IMREAD_GRAYSCALE)
            if first_frame is None:
                continue
            first_frame = cv2.cvtColor(first_frame, cv2.COLOR_GRAY2BGR)
        
        h, w = first_frame.shape[:2]
        video_path = os.path.join(videos_dir, f"{folder}.mp4")
        fourcc = cv2.VideoWriter_fourcc(*'mp4v')
        out = cv2.VideoWriter(video_path, fourcc, args.target_fps, (w, h))
        
        for frame_file in frame_files:
            frame = cv2.imread(os.path.join(folder_path, frame_file))
            if frame is None:
                frame = cv2.imread(os.path.join(folder_path, frame_file), cv2.IMREAD_GRAYSCALE)
                if frame is not None:
                    frame = cv2.cvtColor(frame, cv2.COLOR_GRAY2BGR)
            if frame is not None:
                if frame.shape[:2] != (h, w):
                    frame = cv2.resize(frame, (w, h))
                out.write(frame)
        
        out.release()
        log("VIDEO", f"{folder}.mp4 done")
    
    log("DONE", f"Output: {args.output_dir}")


if __name__ == "__main__":
    main()
