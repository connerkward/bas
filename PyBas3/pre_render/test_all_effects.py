#!/usr/bin/env python3
"""Test script: run all effects on a single raw frame and chroma-keyed frame for comparison."""

import cv2
import numpy as np
from PIL import Image
from transformers import pipeline
import torch
import os
import sys

# Import functions from main script
sys.path.insert(0, os.path.dirname(__file__))
from depth_blend_video import (
    process_dither_frame, process_pixel_frame, 
    chroma_key_green, extract_subject_mask,
    blend_lighten, blend_add, blend_screen
)

def apply_all_effects(raw_frame, chroma_frame, depth_map, frame_stats):
    """Apply all effects to both frames and return results."""
    h, w = raw_frame.shape[:2]
    results = {}
    
    # Ensure depth is same size
    if depth_map.shape != (h, w):
        depth_map = cv2.resize(depth_map, (w, h))
    
    # Dithering effects
    print("  Applying dithering effects...")
    for frame_type, frame in [("raw", raw_frame), ("chroma", chroma_frame)]:
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        
        # Floyd-Steinberg
        pil_gray = Image.fromarray(gray, mode='L')
        dithered = pil_gray.convert('1', dither=Image.Dither.FLOYDSTEINBERG)
        results[f"{frame_type}_dithered"] = np.array(dithered.convert('L'))
        
        # Atkinson
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
        results[f"{frame_type}_atkinson"] = np.clip(gray_f, 0, 255).astype(np.uint8)
        
        # Bayer
        bayer_matrix = np.array([
            [0, 8, 2, 10], [12, 4, 14, 6],
            [3, 11, 1, 9], [15, 7, 13, 5]
        ], dtype=np.float32) / 16.0 * 255.0
        threshold = np.tile(bayer_matrix, (h // 4 + 1, w // 4 + 1))[:h, :w]
        results[f"{frame_type}_bayer"] = (gray.astype(np.float32) > threshold).astype(np.uint8) * 255
    
    # Pixelation effects
    print("  Applying pixelation effects...")
    for frame_type, frame in [("raw", raw_frame), ("chroma", chroma_frame)]:
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        
        # Extract (scale 8)
        small = cv2.resize(gray, (max(1, w // 8), max(1, h // 8)), interpolation=cv2.INTER_AREA)
        _, binary = cv2.threshold(small, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
        results[f"{frame_type}_extract"] = cv2.resize(binary, (w, h), interpolation=cv2.INTER_NEAREST)
        
        # Lowres (scale 16)
        small = cv2.resize(gray, (max(1, w // 16), max(1, h // 16)), interpolation=cv2.INTER_AREA)
        _, binary = cv2.threshold(small, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
        results[f"{frame_type}_lowres"] = cv2.resize(binary, (w, h), interpolation=cv2.INTER_NEAREST)
        
        # Microres (scale 24)
        small = cv2.resize(gray, (max(1, w // 24), max(1, h // 24)), interpolation=cv2.INTER_AREA)
        _, binary = cv2.threshold(small, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
        results[f"{frame_type}_microres"] = cv2.resize(binary, (w, h), interpolation=cv2.INTER_NEAREST)
    
    # Red overlay (needs dithered)
    print("  Applying red overlay...")
    for frame_type, frame in [("raw", raw_frame), ("chroma", chroma_frame)]:
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        dark_base = np.clip(gray.astype(np.float32) * 0.3, 0, 255).astype(np.uint8)
        dith = results.get(f"{frame_type}_dithered", gray)
        
        result = np.zeros((h, w, 3), dtype=np.uint8)
        result[:, :, 0] = np.clip(dark_base * 0.4, 0, 60).astype(np.uint8)
        result[:, :, 1] = np.clip(dark_base * 0.2, 0, 40).astype(np.uint8)
        result[:, :, 2] = np.clip(dark_base * 0.3, 0, 50).astype(np.uint8)
        
        dith_mask = dith > 127
        result[:, :, 2] = np.where(dith_mask, 255, result[:, :, 2])
        result[:, :, 1] = np.where(dith_mask, 80, result[:, :, 1])
        result[:, :, 0] = np.where(dith_mask, 20, result[:, :, 0])
        results[f"{frame_type}_red_overlay"] = result
    
    # Depth banding (only on chroma since depth is from raw)
    print("  Applying depth banding...")
    for frame_type, frame in [("raw", raw_frame), ("chroma", chroma_frame)]:
        result = np.zeros((h, w), dtype=np.uint8)
        for y in range(h):
            for x in range(w):
                d = depth_map[y, x]
                if d < 10:
                    continue
                line_spacing = max(2, int(20 - d / 15))
                wave = int(np.sin(y * 0.1 + d * 0.05) * 3)
                if (y + wave) % line_spacing < 2:
                    result[y, x] = 255
        noise_mask = np.random.random((h, w)) < (depth_map.astype(np.float32) / 255.0 * 0.1)
        result = np.where(noise_mask, 255, result).astype(np.uint8)
        results[f"{frame_type}_depth_banding"] = result
    
    return results

def main():
    # Get frame paths
    base_dir = "outputs/runside-megaslow-compressed_blend_output/chroma_keyed"
    frame_num = "0100"
    
    raw_path = os.path.join(base_dir, "raw_frames", f"frame_{frame_num}.png")
    chroma_path = os.path.join(base_dir, "chroma_keyed", f"frame_{frame_num}.png")
    
    # Try nested structure if needed
    if not os.path.exists(chroma_path):
        chroma_path = os.path.join(base_dir, "chroma_keyed", "chroma_keyed", f"frame_{frame_num}.png")
    
    if not os.path.exists(raw_path) or not os.path.exists(chroma_path):
        print(f"Error: Frames not found")
        print(f"  Raw: {raw_path} ({'exists' if os.path.exists(raw_path) else 'missing'})")
        print(f"  Chroma: {chroma_path} ({'exists' if os.path.exists(chroma_path) else 'missing'})")
        sys.exit(1)
    
    print("Loading frames...")
    raw_frame = cv2.imread(raw_path)
    chroma_frame = cv2.imread(chroma_path)
    
    if raw_frame is None or chroma_frame is None:
        print("Failed to load frames")
        sys.exit(1)
    
    print(f"Raw frame shape: {raw_frame.shape}")
    print(f"Chroma frame shape: {chroma_frame.shape}")
    
    # Load depth model and run on raw frame
    device = "mps" if torch.backends.mps.is_available() else ("cuda" if torch.cuda.is_available() else "cpu")
    print(f"\nLoading depth model on {device}...")
    pipe = pipeline(task="depth-estimation", model="depth-anything/Depth-Anything-V2-Small-hf", device=device)
    
    print("Running depth estimation on raw frame...")
    raw_image = Image.fromarray(cv2.cvtColor(raw_frame, cv2.COLOR_BGR2RGB))
    result = pipe([raw_image])[0]
    depth_map = result["depth"]
    depth_array = np.array(depth_map)
    
    # Normalize depth
    depth_min, depth_max = depth_array.min(), depth_array.max()
    if depth_max > depth_min:
        depth_norm = (depth_array - depth_min) / (depth_max - depth_min)
    else:
        depth_norm = np.zeros_like(depth_array, dtype=np.float32)
    depth_gray = (depth_norm * 255).astype(np.uint8)
    
    # Compute frame stats (for pixelation)
    gray = cv2.cvtColor(chroma_frame, cv2.COLOR_BGR2GRAY)
    frame_stats = (np.mean(gray), np.std(gray))
    
    print("\nApplying all effects...")
    results = apply_all_effects(raw_frame, chroma_frame, depth_gray, frame_stats)
    
    # Save results
    output_dir = "outputs/test_all_effects"
    os.makedirs(output_dir, exist_ok=True)
    
    print(f"\nSaving results to {output_dir}/...")
    
    # Save originals
    cv2.imwrite(os.path.join(output_dir, "00_raw_original.png"), raw_frame)
    cv2.imwrite(os.path.join(output_dir, "00_chroma_original.png"), chroma_frame)
    Image.fromarray(depth_gray, mode='L').save(os.path.join(output_dir, "00_depth_map.png"))
    
    # Save all effects
    effect_order = [
        "dithered", "atkinson", "bayer",
        "extract", "lowres", "microres",
        "red_overlay", "depth_banding"
    ]
    
    for effect in effect_order:
        for frame_type in ["raw", "chroma"]:
            key = f"{frame_type}_{effect}"
            if key in results:
                result = results[key]
                if len(result.shape) == 2:  # Grayscale
                    Image.fromarray(result, mode='L').save(
                        os.path.join(output_dir, f"{frame_type}_{effect}.png"))
                else:  # Color
                    cv2.imwrite(os.path.join(output_dir, f"{frame_type}_{effect}.png"), 
                               cv2.cvtColor(result, cv2.COLOR_RGB2BGR) if result.shape[2] == 3 else result)
    
    # Create comparison grids
    print("Creating comparison grids...")
    
    # Dithering comparison
    dither_comparison = np.hstack([
        cv2.cvtColor(results["raw_dithered"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["chroma_dithered"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["raw_atkinson"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["chroma_atkinson"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["raw_bayer"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["chroma_bayer"], cv2.COLOR_GRAY2BGR)
    ])
    cv2.imwrite(os.path.join(output_dir, "comparison_dithering.png"), dither_comparison)
    
    # Pixelation comparison
    pixel_comparison = np.hstack([
        cv2.cvtColor(results["raw_extract"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["chroma_extract"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["raw_lowres"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["chroma_lowres"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["raw_microres"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["chroma_microres"], cv2.COLOR_GRAY2BGR)
    ])
    cv2.imwrite(os.path.join(output_dir, "comparison_pixelation.png"), pixel_comparison)
    
    # Special effects comparison
    special_comparison = np.hstack([
        results["raw_red_overlay"],
        results["chroma_red_overlay"],
        cv2.cvtColor(results["raw_depth_banding"], cv2.COLOR_GRAY2BGR),
        cv2.cvtColor(results["chroma_depth_banding"], cv2.COLOR_GRAY2BGR)
    ])
    cv2.imwrite(os.path.join(output_dir, "comparison_special.png"), special_comparison)
    
    # Full comparison (all effects side by side)
    all_effects = []
    for effect in effect_order:
        all_effects.append(cv2.cvtColor(results[f"raw_{effect}"], cv2.COLOR_GRAY2BGR) if len(results[f"raw_{effect}"].shape) == 2 else results[f"raw_{effect}"])
        all_effects.append(cv2.cvtColor(results[f"chroma_{effect}"], cv2.COLOR_GRAY2BGR) if len(results[f"chroma_{effect}"].shape) == 2 else results[f"chroma_{effect}"])
    
    # Create a grid (2 columns: raw, chroma)
    rows = []
    for i in range(0, len(all_effects), 2):
        if i + 1 < len(all_effects):
            rows.append(np.hstack([all_effects[i], all_effects[i+1]]))
    full_comparison = np.vstack(rows)
    cv2.imwrite(os.path.join(output_dir, "comparison_all.png"), full_comparison)
    
    print(f"\n✓ Done! Results saved to {output_dir}/")
    print("  - Individual effect files: raw_*.png, chroma_*.png")
    print("  - Comparison grids: comparison_*.png")
    print("  - Full comparison: comparison_all.png")

if __name__ == "__main__":
    main()
