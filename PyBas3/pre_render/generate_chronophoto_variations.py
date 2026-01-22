#!/usr/bin/env python3
"""
Generate chronophoto variations with different frame counts and selection strategies.
Composites onto raw frames (B&W, no chroma key) and includes hero+ghost mode.
"""

import os
import cv2
import numpy as np
from PIL import Image
from pathlib import Path
import argparse
from tqdm import tqdm


def create_chronophotography(depth_images: list[np.ndarray], mode: str = "lighten_add") -> np.ndarray:
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
    
    elif mode == "long_exposure":
        # Normalized additive blend - creates long exposure effect
        for img in depth_images[1:]:
            result += img.astype(np.float32)
        # Normalize to prevent whiteout
        result = result / len(depth_images)
        return np.clip(result, 0, 255).astype(np.uint8)
    
    elif mode == "hero_ghost":
        # Hero frame at full opacity, others as ghost underlay
        hero = depth_images[0].astype(np.float32)
        ghost = depth_images[0].astype(np.float32)
        
        # Average all frames for ghost layer
        for img in depth_images[1:]:
            ghost += img.astype(np.float32)
        ghost /= len(depth_images)
        
        # Blend: 70% hero, 30% ghost
        result = hero * 0.7 + ghost * 0.3
        return np.clip(result, 0, 255).astype(np.uint8)
    
    else:
        return depth_images[0]


def load_depth_images(depth_maps_dir: str) -> list[np.ndarray]:
    """Load all depth images from directory."""
    depth_files = sorted(Path(depth_maps_dir).glob("depth_*.png"))
    depth_images = []
    
    print(f"Loading {len(depth_files)} depth images...")
    for depth_file in tqdm(depth_files, desc="Loading depth"):
        img = cv2.imread(str(depth_file), cv2.IMREAD_GRAYSCALE)
        if img is not None:
            depth_images.append(img)
    
    return depth_images


def load_raw_frames(frames_dir: str) -> list[np.ndarray]:
    """Load raw frames and convert to B&W (no chroma key)."""
    frame_files = sorted(Path(frames_dir).glob("frame_*.png"))
    frames = []
    
    print(f"Loading {len(frame_files)} raw frames...")
    for frame_file in tqdm(frame_files, desc="Loading frames"):
        img = cv2.imread(str(frame_file), cv2.IMREAD_COLOR)
        if img is not None:
            # Convert to B&W
            gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
            # Convert back to 3-channel for compositing
            frames.append(cv2.cvtColor(gray, cv2.COLOR_GRAY2BGR))
    
    return frames


def select_frames_sequential(total_frames: int, num_frames: int) -> list[int]:
    """Select frames evenly spaced across the sequence."""
    if num_frames >= total_frames:
        return list(range(total_frames))
    
    step = total_frames / num_frames
    indices = [int(i * step) for i in range(num_frames)]
    # Ensure we include the last frame
    if indices[-1] != total_frames - 1:
        indices[-1] = total_frames - 1
    return indices


def calculate_frame_difference(img1: np.ndarray, img2: np.ndarray) -> float:
    """Calculate difference between two frames (downsampled for speed)."""
    # Downsample to 64x64 for faster computation
    small1 = cv2.resize(img1, (64, 64))
    small2 = cv2.resize(img2, (64, 64))
    # Use mean squared difference
    return np.mean((small1.astype(np.float32) - small2.astype(np.float32)) ** 2)


def select_frames_most_different(depth_images: list[np.ndarray], num_frames: int) -> list[int]:
    """Select frames that are most different from each other (greedy algorithm)."""
    if num_frames >= len(depth_images):
        return list(range(len(depth_images)))
    
    if num_frames == 1:
        return [0]
    
    # Start with first and last frame
    selected = [0, len(depth_images) - 1]
    
    print(f"Selecting {num_frames} most different frames...")
    # Greedily add frames that maximize minimum distance to already selected
    for iteration in tqdm(range(num_frames - 2), desc="Selecting frames"):
        best_idx = -1
        best_min_dist = -1
        
        for candidate in range(len(depth_images)):
            if candidate in selected:
                continue
            
            # Find minimum distance to any selected frame (compute on-the-fly)
            min_dist = min(
                calculate_frame_difference(depth_images[candidate], depth_images[sel])
                for sel in selected
            )
            
            if min_dist > best_min_dist:
                best_min_dist = min_dist
                best_idx = candidate
        
        if best_idx >= 0:
            selected.append(best_idx)
    
    return sorted(selected)


def select_frames_variance(depth_images: list[np.ndarray], num_frames: int) -> list[int]:
    """Select frames with highest variance (most change)."""
    if num_frames >= len(depth_images):
        return list(range(len(depth_images)))
    
    # Calculate variance for each frame
    variances = []
    for img in depth_images:
        var = np.var(img.astype(np.float32))
        variances.append(var)
    
    # Get indices sorted by variance (highest first)
    sorted_indices = sorted(range(len(variances)), key=lambda i: variances[i], reverse=True)
    
    # Take top N, but keep them in temporal order
    top_indices = sorted_indices[:num_frames]
    return sorted(top_indices)


def composite_on_frame(depth_chrono: np.ndarray, raw_frame: np.ndarray, alpha: float = 0.5) -> np.ndarray:
    """Composite depth chronophoto onto raw frame."""
    # Convert depth to RGB
    depth_rgb = cv2.cvtColor(depth_chrono, cv2.COLOR_GRAY2BGR)
    
    # Ensure same size
    if depth_rgb.shape != raw_frame.shape:
        depth_rgb = cv2.resize(depth_rgb, (raw_frame.shape[1], raw_frame.shape[0]))
    
    # Blend: alpha controls depth visibility
    blended = cv2.addWeighted(raw_frame, 1.0 - alpha, depth_rgb, alpha, 0)
    return blended


def main():
    parser = argparse.ArgumentParser(description="Generate chronophoto variations")
    parser.add_argument(
        "--depth_maps_dir",
        type=str,
        default="outputs/runside-megaslow_blend_output/depth_maps",
        help="Directory containing depth map images"
    )
    parser.add_argument(
        "--frames_dir",
        type=str,
        default="outputs/runside-megaslow_blend_output/frames",
        help="Directory containing raw frame images"
    )
    parser.add_argument(
        "--frame_counts",
        type=int,
        nargs="+",
        default=[3, 5, 7, 10],
        help="Number of frames to use for each variation"
    )
    parser.add_argument(
        "--composite_alpha",
        type=float,
        default=0.5,
        help="Alpha for compositing depth onto raw frames (0-1)"
    )
    
    args = parser.parse_args()
    
    # Resolve paths
    script_dir = Path(__file__).parent
    depth_maps_dir = script_dir / args.depth_maps_dir
    frames_dir = script_dir / args.frames_dir
    output_dir = depth_maps_dir / "chronophotos"
    output_dir.mkdir(exist_ok=True)
    
    # Load all depth images and raw frames
    depth_images = load_depth_images(str(depth_maps_dir))
    raw_frames = load_raw_frames(str(frames_dir))
    total_frames = len(depth_images)
    
    print(f"\nLoaded {total_frames} depth images and {len(raw_frames)} raw frames")
    print(f"Output directory: {output_dir}\n")
    
    # Blend modes to try
    blend_modes = ["lighten_add", "long_exposure", "hero_ghost"]
    
    # Generate variations
    for num_frames in args.frame_counts:
        if num_frames > total_frames:
            num_frames = total_frames
        
        print(f"\n=== Generating variations with {num_frames} frames ===")
        
        # Sequential selection
        seq_indices = select_frames_sequential(total_frames, num_frames)
        seq_selected = [depth_images[i] for i in seq_indices]
        
        # Most different selection (only for small counts)
        if num_frames <= 10:
            diff_indices = select_frames_most_different(depth_images, num_frames)
            diff_selected = [depth_images[i] for i in diff_indices]
        else:
            diff_indices = None
            diff_selected = None
        
        # Variance-based selection
        var_indices = select_frames_variance(depth_images, num_frames)
        var_selected = [depth_images[i] for i in var_indices]
        
        # Generate for each blend mode
        for mode in blend_modes:
            # Sequential
            seq_chrono = create_chronophotography(seq_selected, mode)
            seq_path = output_dir / f"chrono_seq_{num_frames:04d}_{mode}.png"
            Image.fromarray(seq_chrono, mode='L').save(seq_path)
            
            # Composite onto raw frame (use middle frame as base)
            if raw_frames and len(seq_indices) > 0:
                mid_idx = seq_indices[len(seq_indices) // 2]
                if mid_idx < len(raw_frames):
                    composite = composite_on_frame(seq_chrono, raw_frames[mid_idx], args.composite_alpha)
                    comp_path = output_dir / f"composite_seq_{num_frames:04d}_{mode}.png"
                    Image.fromarray(composite).save(comp_path)
            
            # Most different
            if diff_selected:
                diff_chrono = create_chronophotography(diff_selected, mode)
                diff_path = output_dir / f"chrono_diff_{num_frames:04d}_{mode}.png"
                Image.fromarray(diff_chrono, mode='L').save(diff_path)
                
                # Composite
                if raw_frames and len(diff_indices) > 0:
                    mid_idx = diff_indices[len(diff_indices) // 2]
                    if mid_idx < len(raw_frames):
                        composite = composite_on_frame(diff_chrono, raw_frames[mid_idx], args.composite_alpha)
                        comp_path = output_dir / f"composite_diff_{num_frames:04d}_{mode}.png"
                        Image.fromarray(composite).save(comp_path)
            
            # Variance
            var_chrono = create_chronophotography(var_selected, mode)
            var_path = output_dir / f"chrono_var_{num_frames:04d}_{mode}.png"
            Image.fromarray(var_chrono, mode='L').save(var_path)
            
            # Composite
            if raw_frames and len(var_indices) > 0:
                mid_idx = var_indices[len(var_indices) // 2]
                if mid_idx < len(raw_frames):
                    composite = composite_on_frame(var_chrono, raw_frames[mid_idx], args.composite_alpha)
                    comp_path = output_dir / f"composite_var_{num_frames:04d}_{mode}.png"
                    Image.fromarray(composite).save(comp_path)
        
        print(f"  Generated {len(blend_modes)} blend modes × 3 selection strategies")
    
    print(f"\n✓ All variations saved to {output_dir}")


if __name__ == "__main__":
    main()
