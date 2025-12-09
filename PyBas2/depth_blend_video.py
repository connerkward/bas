#!/usr/bin/env python3
"""
Extract frames from video, run Depth Anything V2, overlay depth maps with blend modes.
"""

import argparse
import os
import cv2
import numpy as np
import torch
from PIL import Image
from transformers import pipeline


def extract_frames(video_path: str, num_frames: int, output_dir: str, target_fps: float = None) -> list[str]:
    """Extract evenly spaced frames from video."""
    cap = cv2.VideoCapture(video_path)
    if not cap.isOpened():
        raise ValueError(f"Cannot open video: {video_path}")
    
    video_fps = cap.get(cv2.CAP_PROP_FPS)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    duration = total_frames / video_fps if video_fps > 0 else 0
    
    # Calculate num_frames based on target_fps if provided
    if target_fps is not None:
        num_frames = int(duration * target_fps)
        print(f"Video duration: {duration:.2f}s, extracting at {target_fps} fps = {num_frames} frames")
    
    frame_indices = np.linspace(0, total_frames - 1, num_frames, dtype=int)
    
    os.makedirs(output_dir, exist_ok=True)
    frame_paths = []
    
    for idx, frame_num in enumerate(frame_indices):
        cap.set(cv2.CAP_PROP_POS_FRAMES, frame_num)
        ret, frame = cap.read()
        if ret:
            frame_path = os.path.join(output_dir, f"frame_{idx:04d}.png")
            cv2.imwrite(frame_path, frame)
            frame_paths.append(frame_path)
    
    cap.release()
    print(f"Extracted {len(frame_paths)} frames")
    return frame_paths


def blend_lighten(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    """Lighten - take max of each pixel (like multiple exposure)."""
    return np.maximum(base, overlay)


def blend_add(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    """Additive blend - accumulate light."""
    return np.clip(base.astype(np.float32) + overlay.astype(np.float32), 0, 255).astype(np.uint8)


def blend_screen(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    """Screen blend - like projecting multiple slides."""
    base_f = base.astype(np.float32) / 255.0
    overlay_f = overlay.astype(np.float32) / 255.0
    result = 1 - (1 - base_f) * (1 - overlay_f)
    return (result * 255).astype(np.uint8)


def create_chronophotography(depth_images: list[np.ndarray], mode: str = "lighten") -> np.ndarray:
    """
    Create Marey-style chronophotography from multiple depth maps.
    All frames are composited together to show motion over time.
    """
    if len(depth_images) == 0:
        return None
    
    # Start with first frame
    result = depth_images[0].astype(np.float32)
    
    if mode == "lighten":
        # Lighten: max of all frames - preserves brightest (closest) objects
        for img in depth_images[1:]:
            result = np.maximum(result, img.astype(np.float32))
        return result.astype(np.uint8)
    
    elif mode == "add":
        # Additive: sum all frames, then normalize
        for img in depth_images[1:]:
            result += img.astype(np.float32)
        # Normalize to 0-255
        result = (result - result.min()) / (result.max() - result.min() + 1e-6) * 255
        return result.astype(np.uint8)
    
    elif mode == "screen":
        # Screen: like overlapping projections
        result = result / 255.0
        for img in depth_images[1:]:
            overlay = img.astype(np.float32) / 255.0
            result = 1 - (1 - result) * (1 - overlay)
        return (result * 255).astype(np.uint8)
    
    elif mode == "average":
        # Simple average of all frames
        for img in depth_images[1:]:
            result += img.astype(np.float32)
        result /= len(depth_images)
        return result.astype(np.uint8)
    
    elif mode == "darken":
        # Darken: min of all frames - preserves darkest (furthest) objects
        for img in depth_images[1:]:
            result = np.minimum(result, img.astype(np.float32))
        return result.astype(np.uint8)
    
    elif mode == "lighten_add":
        # Hybrid: lighten first, then add with reduced weight
        # Start with lighten (max of all frames)
        lighten_result = depth_images[0].astype(np.float32)
        for img in depth_images[1:]:
            lighten_result = np.maximum(lighten_result, img.astype(np.float32))
        
        # Then add all frames together
        add_result = depth_images[0].astype(np.float32)
        for img in depth_images[1:]:
            add_result += img.astype(np.float32)
        # Normalize additive result
        add_result = (add_result - add_result.min()) / (add_result.max() - add_result.min() + 1e-6) * 255
        
        # Blend: 70% lighten, 30% add
        hybrid = lighten_result * 0.7 + add_result * 0.3
        return np.clip(hybrid, 0, 255).astype(np.uint8)
    
    else:
        return depth_images[0]


BLEND_MODES = ["lighten", "add", "screen", "average", "darken", "lighten_add"]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("video_path", type=str)
    parser.add_argument("--num-frames", type=int, default=10)
    parser.add_argument("--target-fps", type=float, default=None, help="Extract frames at this fps (e.g., 12 for stop motion)")
    parser.add_argument("--model", type=str, default="depth-anything/Depth-Anything-V2-Small-hf")
    parser.add_argument("--device", type=str, default="cuda" if torch.cuda.is_available() else "cpu")
    parser.add_argument("--blend-modes", type=str, nargs="+", default=["lighten", "add", "lighten_add"],
                        choices=BLEND_MODES)
    parser.add_argument("--alpha", type=float, default=0.5,
                        help="Alpha blend for raw frame overlay (0.0 = depth only, 1.0 = raw only)")
    parser.add_argument("--output-dir", type=str, default=None)
    
    args = parser.parse_args()
    
    # Generate output dir from video filename if not specified
    if args.output_dir is None:
        video_basename = os.path.splitext(os.path.basename(args.video_path))[0]
        args.output_dir = f"{video_basename}_blend_output"
    
    # Extract frames
    frames_dir = os.path.join(args.output_dir, "frames")
    if args.target_fps:
        print(f"Extracting frames from {args.video_path} at {args.target_fps} fps...")
    else:
        print(f"Extracting {args.num_frames} frames from {args.video_path}...")
    frame_paths = extract_frames(args.video_path, args.num_frames, frames_dir, args.target_fps)
    
    # Load Depth Anything V2 pipeline
    print(f"Loading Depth Anything V2 ({args.model})...")
    pipe = pipeline(task="depth-estimation", model=args.model, device=args.device)
    
    # Create output dirs
    depth_maps_dir = os.path.join(args.output_dir, "depth_maps")
    os.makedirs(depth_maps_dir, exist_ok=True)
    for mode in args.blend_modes:
        os.makedirs(os.path.join(args.output_dir, mode), exist_ok=True)
    
    # Process each frame
    print("Running depth estimation...")
    depth_images = []
    for idx, frame_path in enumerate(frame_paths):
        # Load image
        image = Image.open(frame_path)
        
        # Run depth estimation
        result = pipe(image)
        depth_map = result["depth"]  # PIL Image
        
        # Convert to numpy grayscale
        depth_array = np.array(depth_map)
        
        # Normalize to 0-255
        depth_min = depth_array.min()
        depth_max = depth_array.max()
        if depth_max > depth_min:
            depth_norm = (depth_array - depth_min) / (depth_max - depth_min)
        else:
            depth_norm = np.zeros_like(depth_array, dtype=np.float32)
        
        depth_gray = (depth_norm * 255).astype(np.uint8)
        depth_images.append(depth_gray)
        
        # Save individual depth map
        path = os.path.join(depth_maps_dir, f"depth_{idx:04d}.png")
        Image.fromarray(depth_gray, mode='L').save(path)
        print(f"  Frame {idx + 1}/{len(frame_paths)}")
    
    print(f"Saved {len(depth_images)} depth maps")
    
    # Resize all depth images to same size
    target_shape = depth_images[0].shape
    resized_depths = []
    for img in depth_images:
        if img.shape != target_shape:
            img = cv2.resize(img, (target_shape[1], target_shape[0]))
        resized_depths.append(img)
    
    # Load original frames for overlay
    print("Loading original frames for overlay...")
    original_frames = []
    for frame_path in frame_paths:
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        # Resize to match depth map size
        if frame.shape[:2] != target_shape:
            frame = cv2.resize(frame, (target_shape[1], target_shape[0]))
        original_frames.append(frame)
    
    # Create chronophotography composites (Marey-style multiple exposure)
    print("Creating chronophotography composites...")
    for mode in args.blend_modes:
        # Create progressive frames showing accumulation
        for idx in range(len(resized_depths)):
            # Composite all frames up to this point
            frames_so_far = resized_depths[:idx + 1]
            depth_result = create_chronophotography(frames_so_far, mode)
            
            # Convert depth to RGB for compositing
            depth_rgb = cv2.cvtColor(depth_result, cv2.COLOR_GRAY2RGB)
            
            # Overlay with current raw frame
            if idx < len(original_frames):
                raw_frame = original_frames[idx]
                # Blend: alpha controls mix (0 = all depth, 1 = all raw)
                blended = cv2.addWeighted(depth_rgb, 1.0 - args.alpha, raw_frame, args.alpha, 0)
            else:
                blended = depth_rgb
            
            # Save progressive frame
            path = os.path.join(args.output_dir, mode, f"frame_{idx:04d}.png")
            Image.fromarray(blended).save(path)
        
        # Create final composite of ALL frames
        depth_result = create_chronophotography(resized_depths, mode)
        depth_rgb = cv2.cvtColor(depth_result, cv2.COLOR_GRAY2RGB)
        
        # Overlay with average of all raw frames
        if original_frames:
            # Average all raw frames
            avg_raw = np.mean([f.astype(np.float32) for f in original_frames], axis=0).astype(np.uint8)
            blended = cv2.addWeighted(depth_rgb, 1.0 - args.alpha, avg_raw, args.alpha, 0)
        else:
            blended = depth_rgb
        
        path = os.path.join(args.output_dir, mode, "chronophoto.png")
        Image.fromarray(blended).save(path)
        print(f"Created {mode} chronophotography")
    
    print(f"\nDone! Output: {args.output_dir}")


if __name__ == "__main__":
    main()
