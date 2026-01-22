#!/usr/bin/env python3
"""Remove watermark from videos in input_videos folder."""

import cv2
import os
import sys
from pathlib import Path

def remove_watermark_from_video(input_path: str, output_path: str, watermark_region: tuple = None):
    """
    Remove watermark from video by inpainting the bottom-right corner.
    
    Args:
        input_path: Path to input video
        output_path: Path to output video
        watermark_region: (x, y, width, height) of watermark region. If None, auto-detect bottom-right corner.
    """
    cap = cv2.VideoCapture(input_path)
    if not cap.isOpened():
        print(f"Error: Cannot open {input_path}")
        return False
    
    # Get video properties
    fps = int(cap.get(cv2.CAP_PROP_FPS))
    width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    
    print(f"Processing: {os.path.basename(input_path)}")
    print(f"  Size: {width}x{height}, FPS: {fps}, Frames: {total_frames}")
    
    # Determine watermark region (bottom-right corner, ~10% of width/height)
    if watermark_region is None:
        wm_width = int(width * 0.15)  # 15% of width
        wm_height = int(height * 0.10)  # 10% of height
        x = width - wm_width
        y = height - wm_height
    else:
        x, y, wm_width, wm_height = watermark_region
    
    print(f"  Watermark region: ({x}, {y}) to ({x+wm_width}, {y+wm_height})")
    
    # Create output video writer
    fourcc = cv2.VideoWriter_fourcc(*'mp4v')
    out = cv2.VideoWriter(output_path, fourcc, fps, (width, height))
    
    if not out.isOpened():
        print(f"Error: Cannot create output video {output_path}")
        cap.release()
        return False
    
    frame_count = 0
    while True:
        ret, frame = cap.read()
        if not ret:
            break
        
        # Create mask for watermark region
        mask = np.zeros((height, width), dtype=np.uint8)
        mask[y:y+wm_height, x:x+wm_width] = 255
        
        # Inpaint the watermark region
        inpainted = cv2.inpaint(frame, mask, 3, cv2.INPAINT_TELEA)
        
        out.write(inpainted)
        frame_count += 1
        
        if frame_count % 30 == 0:
            print(f"  Processed {frame_count}/{total_frames} frames...")
    
    cap.release()
    out.release()
    print(f"  ✓ Done: {frame_count} frames processed")
    return True

def main():
    input_dir = Path("../../input_videos")
    if not input_dir.exists():
        print(f"Error: {input_dir} does not exist")
        sys.exit(1)
    
    # Find all video files
    video_extensions = ['.mp4', '.mov', '.avi', '.mkv']
    video_files = [f for f in input_dir.iterdir() 
                   if f.suffix.lower() in video_extensions and f.is_file()]
    
    if not video_files:
        print(f"No video files found in {input_dir}")
        sys.exit(1)
    
    print(f"Found {len(video_files)} video(s) to process\n")
    
    # Process each video
    for video_file in video_files:
        input_path = str(video_file)
        # Create output filename (add _nowm suffix before extension)
        output_path = str(video_file.parent / f"{video_file.stem}_nowm{video_file.suffix}")
        
        if os.path.exists(output_path):
            print(f"Skipping {video_file.name} (output already exists)")
            continue
        
        success = remove_watermark_from_video(input_path, output_path)
        if not success:
            print(f"  ✗ Failed to process {video_file.name}")
        print()
    
    print("All videos processed!")

if __name__ == "__main__":
    import numpy as np
    main()
