#!/usr/bin/env python3
"""Quick test: run depth estimation on both raw and chroma-keyed frames for comparison."""

import cv2
import numpy as np
from PIL import Image
from transformers import pipeline
import torch
import os
import sys

def process_depth(frame_path, label):
    """Run depth estimation on a frame and return results."""
    print(f"\n=== Processing {label} ===")
    print(f"Loading: {frame_path}")
    
    if not os.path.exists(frame_path):
        print(f"Frame not found: {frame_path}")
        return None
    
    frame = cv2.imread(frame_path)
    if frame is None:
        print("Failed to load frame")
        return None
    
    print(f"Frame shape: {frame.shape}")
    
    # Run depth estimation
    image = Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
    result = pipe([image])[0]
    
    depth_map = result["depth"]
    depth_array = np.array(depth_map)
    
    # Normalize depth
    depth_min, depth_max = depth_array.min(), depth_array.max()
    print(f"Depth range: {depth_min:.2f} - {depth_max:.2f}")
    
    if depth_max > depth_min:
        depth_norm = (depth_array - depth_min) / (depth_max - depth_min)
    else:
        depth_norm = np.zeros_like(depth_array, dtype=np.float32)
    
    depth_gray = (depth_norm * 255).astype(np.uint8)
    depth_colored = cv2.applyColorMap(depth_gray, cv2.COLORMAP_VIRIDIS)
    
    return {
        'frame': frame,
        'depth_gray': depth_gray,
        'depth_colored': depth_colored,
        'label': label
    }

# Load depth model once
device = "mps" if torch.backends.mps.is_available() else ("cuda" if torch.cuda.is_available() else "cpu")
print(f"Loading depth model on {device}...")
pipe = pipeline(task="depth-estimation", model="depth-anything/Depth-Anything-V2-Small-hf", device=device)

# Get frame paths
base_dir = "outputs/runside-megaslow-compressed_blend_output/chroma_keyed"
frame_num = "0100"

raw_path = os.path.join(base_dir, "raw_frames", f"frame_{frame_num}.png")
chroma_path = os.path.join(base_dir, "chroma_keyed", f"frame_{frame_num}.png")

# If chroma_keyed doesn't exist, try nested structure
if not os.path.exists(chroma_path):
    chroma_path = os.path.join(base_dir, "chroma_keyed", f"frame_{frame_num}.png")
    if not os.path.exists(chroma_path):
        # Try nested structure
        chroma_path = os.path.join(base_dir, "chroma_keyed", "chroma_keyed", f"frame_{frame_num}.png")

# Process both frames
raw_result = process_depth(raw_path, "RAW")
chroma_result = process_depth(chroma_path, "CHROMA-KEYED")

if raw_result is None or chroma_result is None:
    print("Failed to process one or both frames")
    sys.exit(1)

# Save results
output_dir = "outputs/test_depth"
os.makedirs(output_dir, exist_ok=True)

# Save individual results
cv2.imwrite(os.path.join(output_dir, "raw_original.png"), raw_result['frame'])
Image.fromarray(raw_result['depth_gray'], mode='L').save(os.path.join(output_dir, "raw_depth_gray.png"))
cv2.imwrite(os.path.join(output_dir, "raw_depth_colored.png"), raw_result['depth_colored'])

cv2.imwrite(os.path.join(output_dir, "chroma_original.png"), chroma_result['frame'])
Image.fromarray(chroma_result['depth_gray'], mode='L').save(os.path.join(output_dir, "chroma_depth_gray.png"))
cv2.imwrite(os.path.join(output_dir, "chroma_depth_colored.png"), chroma_result['depth_colored'])

# Create comparison: raw vs chroma side by side
h, w = raw_result['frame'].shape[:2]
comparison = np.hstack([
    raw_result['frame'],
    raw_result['depth_colored'],
    chroma_result['frame'],
    chroma_result['depth_colored']
])
cv2.imwrite(os.path.join(output_dir, "comparison_raw_vs_chroma.png"), comparison)

# Create depth comparison only
depth_comparison = np.hstack([
    raw_result['depth_colored'],
    chroma_result['depth_colored']
])
cv2.imwrite(os.path.join(output_dir, "depth_comparison.png"), depth_comparison)

print(f"\n=== Results saved to {output_dir}/ ===")
print("RAW frame:")
print("  - raw_original.png: Original raw frame")
print("  - raw_depth_gray.png: Depth map (grayscale)")
print("  - raw_depth_colored.png: Depth map (colored)")
print("\nCHROMA-KEYED frame:")
print("  - chroma_original.png: Original chroma-keyed frame")
print("  - chroma_depth_gray.png: Depth map (grayscale)")
print("  - chroma_depth_colored.png: Depth map (colored)")
print("\nComparisons:")
print("  - comparison_raw_vs_chroma.png: Raw | Raw Depth | Chroma | Chroma Depth")
print("  - depth_comparison.png: Raw Depth | Chroma Depth")
