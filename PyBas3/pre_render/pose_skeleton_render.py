#!/usr/bin/env python3
"""
Render pose skeletons from video onto black background at original resolution.
Output can be overlaid on original video.
"""

import argparse
import cv2
import numpy as np
import os
import sys
from pathlib import Path
from tqdm import tqdm

# Add parent for mediapipe imports
sys.path.insert(0, str(Path(__file__).parent.parent / "mediapipe"))

import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from download_model import download_model


# Pose connections (33 landmarks)
POSE_CONNECTIONS = [
    (0, 1), (1, 2), (2, 3), (3, 7),
    (0, 4), (4, 5), (5, 6), (6, 8),
    (9, 10),
    (11, 12), (11, 13), (13, 15), (15, 17), (15, 19), (15, 21), (17, 19),
    (12, 14), (14, 16), (16, 18), (16, 20), (16, 22), (18, 20),
    (11, 23), (12, 24),
    (23, 24), (23, 25), (25, 27), (27, 29), (27, 31),
    (24, 26), (26, 28), (28, 30), (28, 32)
]


def draw_skeleton(frame_shape: tuple, landmarks, color=(255, 255, 255), thickness=2, circle_radius=4) -> np.ndarray:
    """Draw skeleton on black background."""
    h, w = frame_shape[:2]
    canvas = np.zeros((h, w, 3), dtype=np.uint8)
    
    if not landmarks or len(landmarks) < 33:
        return canvas
    
    # Draw connections
    for start_idx, end_idx in POSE_CONNECTIONS:
        if start_idx < len(landmarks) and end_idx < len(landmarks):
            start = landmarks[start_idx]
            end = landmarks[end_idx]
            if hasattr(start, 'x') and hasattr(end, 'x'):
                start_pt = (int(start.x * w), int(start.y * h))
                end_pt = (int(end.x * w), int(end.y * h))
                cv2.line(canvas, start_pt, end_pt, color, thickness)
    
    # Draw joints
    for lm in landmarks:
        if hasattr(lm, 'x') and hasattr(lm, 'y'):
            x = int(lm.x * w)
            y = int(lm.y * h)
            cv2.circle(canvas, (x, y), circle_radius, color, -1)
    
    return canvas


def main():
    parser = argparse.ArgumentParser(description="Render pose skeletons from video")
    parser.add_argument("video_path", type=str, help="Input video path")
    parser.add_argument("--output-dir", type=str, default=None, help="Output directory")
    parser.add_argument("--color", type=str, default="white", choices=["white", "green", "red", "blue"],
                        help="Skeleton color")
    parser.add_argument("--thickness", type=int, default=2, help="Line thickness")
    parser.add_argument("--num-poses", type=int, default=3, help="Max poses to detect")
    parser.add_argument("--fps", type=float, default=None, help="Output fps (default: match source)")
    args = parser.parse_args()
    
    # Colors
    colors = {"white": (255, 255, 255), "green": (0, 255, 0), "red": (0, 0, 255), "blue": (255, 0, 0)}
    color = colors[args.color]
    
    # Output dir
    if args.output_dir is None:
        script_dir = Path(__file__).parent
        video_name = Path(args.video_path).stem
        args.output_dir = str(script_dir / "outputs" / f"{video_name}_skeleton")
    
    frames_dir = Path(args.output_dir) / "frames"
    frames_dir.mkdir(parents=True, exist_ok=True)
    
    # Open video
    cap = cv2.VideoCapture(args.video_path)
    if not cap.isOpened():
        print(f"Cannot open video: {args.video_path}")
        sys.exit(1)
    
    video_fps = cap.get(cv2.CAP_PROP_FPS)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    
    output_fps = args.fps if args.fps else video_fps
    print(f"Video: {width}x{height} @ {video_fps}fps, {total_frames} frames")
    print(f"Output: {args.output_dir}")
    
    # Init MediaPipe
    model_path = download_model()
    base_options = python.BaseOptions(model_asset_path=model_path)
    options = vision.PoseLandmarkerOptions(
        base_options=base_options,
        running_mode=vision.RunningMode.VIDEO,
        num_poses=args.num_poses,
        min_pose_detection_confidence=0.5,
        min_pose_presence_confidence=0.5,
        min_tracking_confidence=0.5,
        output_segmentation_masks=False
    )
    landmarker = vision.PoseLandmarker.create_from_options(options)
    
    # Process frames
    frame_idx = 0
    pbar = tqdm(total=total_frames, desc="Processing", unit="frame")
    
    while True:
        ret, frame = cap.read()
        if not ret:
            break
        
        # Detect pose
        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb)
        timestamp_ms = int(frame_idx * 1000 / video_fps)
        
        result = landmarker.detect_for_video(mp_image, timestamp_ms)
        
        # Draw all detected skeletons
        canvas = np.zeros((height, width, 3), dtype=np.uint8)
        if result and result.pose_landmarks:
            for landmarks in result.pose_landmarks:
                skeleton = draw_skeleton((height, width), landmarks, color, args.thickness)
                canvas = np.maximum(canvas, skeleton)  # Lighten blend
        
        # Save frame
        out_path = frames_dir / f"frame_{frame_idx:04d}.png"
        cv2.imwrite(str(out_path), canvas)
        
        frame_idx += 1
        pbar.update(1)
    
    pbar.close()
    cap.release()
    landmarker.close()
    
    print(f"Saved {frame_idx} frames to {frames_dir}")
    
    # Create video
    videos_dir = Path(args.output_dir) / "videos"
    videos_dir.mkdir(exist_ok=True)
    video_out = videos_dir / "skeleton.mp4"
    
    print(f"Creating video at {output_fps}fps...")
    import subprocess
    cmd = [
        "ffmpeg", "-y", "-framerate", str(output_fps),
        "-i", str(frames_dir / "frame_%04d.png"),
        "-c:v", "libx264", "-pix_fmt", "yuv420p", "-crf", "18",
        str(video_out)
    ]
    subprocess.run(cmd, capture_output=True)
    print(f"Video: {video_out}")


if __name__ == "__main__":
    main()
