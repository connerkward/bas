#!/usr/bin/env python3
"""
Convert frame sequences from depth blend output back into videos.
"""

import argparse
import os
import subprocess
import glob
import tempfile
import shutil
import sys


def frames_to_video(frames_dir: str, output_path: str, fps: float = 12.0):
    """Convert a sequence of frames to a video using ffmpeg."""
    # Get all frame files sorted
    frame_pattern = os.path.join(frames_dir, "frame_*.png")
    frame_files = sorted(glob.glob(frame_pattern))
    
    if not frame_files:
        raise ValueError(f"No frames found in {frames_dir}")
    
    print(f"Found {len(frame_files)} frames in {frames_dir}")
    
    # Use ffmpeg for reliable video encoding
    # Create a temporary directory with sequentially numbered frames
    with tempfile.TemporaryDirectory() as temp_dir:
        # Copy frames to temp dir with sequential naming
        for idx, frame_path in enumerate(frame_files):
            temp_frame = os.path.join(temp_dir, f"frame_{idx:06d}.png")
            shutil.copy2(frame_path, temp_frame)
        
        # Use ffmpeg to create video
        input_pattern = os.path.join(temp_dir, "frame_%06d.png")
        cmd = [
            "ffmpeg",
            "-y",  # Overwrite output
            "-framerate", str(fps),
            "-i", input_pattern,
            "-c:v", "libx264",
            "-pix_fmt", "yuv420p",
            "-crf", "23",
            output_path
        ]
        
        print(f"Encoding video with ffmpeg...")
        result = subprocess.run(cmd, capture_output=True, text=True)
        
        if result.returncode != 0:
            print(f"ffmpeg error: {result.stderr}")
            raise RuntimeError(f"ffmpeg failed: {result.stderr}")
    
    print(f"Created video: {output_path} ({len(frame_files)} frames @ {fps} fps)")


def main():
    # Check if ffmpeg is available
    try:
        subprocess.run(["ffmpeg", "-version"], capture_output=True, check=True)
    except (subprocess.CalledProcessError, FileNotFoundError):
        print("Error: ffmpeg is required but not found. Please install ffmpeg.")
        sys.exit(1)
    
    parser = argparse.ArgumentParser(description="Convert frame sequences to video")
    parser.add_argument("input_dir", type=str, help="Input directory (blend output folder)")
    parser.add_argument("--blend-mode", type=str, default="lighten_add",
                        choices=["lighten", "add", "lighten_add", "screen", "average", "darken"],
                        help="Which blend mode folder to use")
    parser.add_argument("--output", type=str, default=None,
                        help="Output video path (default: {input_dir}_{blend_mode}.mp4)")
    parser.add_argument("--fps", type=float, default=12.0,
                        help="Output video fps (default: 12.0 for stop motion)")
    parser.add_argument("--use-chronophoto", action="store_true",
                        help="Use chronophoto.png instead of frame sequence")
    
    args = parser.parse_args()
    
    # Determine frames directory
    if args.use_chronophoto:
        frames_dir = args.input_dir
        frame_path = os.path.join(args.input_dir, args.blend_mode, "chronophoto.png")
        if not os.path.exists(frame_path):
            raise ValueError(f"chronophoto.png not found in {os.path.join(args.input_dir, args.blend_mode)}")
        # For single image, we'll just duplicate it
        print("Using chronophoto.png (will create 1-second video)")
        frames_dir = os.path.join(args.input_dir, args.blend_mode)
    else:
        frames_dir = os.path.join(args.input_dir, args.blend_mode)
        if not os.path.exists(frames_dir):
            raise ValueError(f"Blend mode folder not found: {frames_dir}")
    
    # Determine output path
    if args.output is None:
        input_basename = os.path.basename(args.input_dir.rstrip('/'))
        output_name = f"{input_basename}_{args.blend_mode}.mp4"
        args.output = output_name
    
    if args.use_chronophoto:
        # Special handling for single chronophoto image using ffmpeg
        chrono_path = os.path.join(frames_dir, "chronophoto.png")
        if not os.path.exists(chrono_path):
            raise ValueError(f"Could not read {chrono_path}")
        
        duration = 1.0  # 1 second
        cmd = [
            "ffmpeg",
            "-y",
            "-loop", "1",
            "-i", chrono_path,
            "-t", str(duration),
            "-c:v", "libx264",
            "-pix_fmt", "yuv420p",
            "-r", str(args.fps),
            "-crf", "23",
            args.output
        ]
        
        print(f"Encoding chronophoto video with ffmpeg...")
        result = subprocess.run(cmd, capture_output=True, text=True)
        
        if result.returncode != 0:
            print(f"ffmpeg error: {result.stderr}")
            raise RuntimeError(f"ffmpeg failed: {result.stderr}")
        
        print(f"Created video: {args.output} (1 second @ {args.fps} fps)")
    else:
        frames_to_video(frames_dir, args.output, args.fps)


if __name__ == "__main__":
    main()

