#!/usr/bin/env python3
"""
Build reference poses from a video file.

Usage:
    python reference_builder.py path/to/reference_video.mp4
    
Creates reference_poses.json with normalized pose keypoints per frame.
"""

import sys
import json
import cv2
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from pathlib import Path
from typing import List, Dict

# Add parent for model download
sys.path.insert(0, str(Path(__file__).parent.parent / "mediapipe"))
from download_model import download_model


def extract_reference_poses(video_path: str) -> List[Dict]:
    """
    Extract pose keypoints from each frame of reference video.
    
    Returns list of dicts with frame_index and keypoints.
    """
    model_path = download_model()
    base_options = python.BaseOptions(model_asset_path=model_path)
    options = vision.PoseLandmarkerOptions(
        base_options=base_options,
        running_mode=vision.RunningMode.VIDEO,
        num_poses=1,
        min_pose_detection_confidence=0.5,
        min_pose_presence_confidence=0.5,
        min_tracking_confidence=0.5
    )
    
    landmarker = vision.PoseLandmarker.create_from_options(options)
    cap = cv2.VideoCapture(video_path)
    
    if not cap.isOpened():
        raise ValueError(f"Cannot open video: {video_path}")
    
    fps = cap.get(cv2.CAP_PROP_FPS)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    print(f"Processing {total_frames} frames at {fps:.1f} FPS")
    
    reference_poses = []
    frame_idx = 0
    
    while True:
        ret, frame = cap.read()
        if not ret:
            break
        
        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb)
        
        timestamp_ms = int(frame_idx * 1000 / fps)
        result = landmarker.detect_for_video(mp_image, timestamp_ms)
        
        if result.pose_landmarks and len(result.pose_landmarks) > 0:
            landmarks = result.pose_landmarks[0]
            keypoints = [(lm.x, lm.y, lm.z, lm.visibility) for lm in landmarks]
            reference_poses.append({
                'frame_index': frame_idx,
                'timestamp_ms': timestamp_ms,
                'keypoints': keypoints
            })
        else:
            # No pose detected, store empty
            reference_poses.append({
                'frame_index': frame_idx,
                'timestamp_ms': timestamp_ms,
                'keypoints': None
            })
        
        frame_idx += 1
        if frame_idx % 100 == 0:
            print(f"  Processed {frame_idx}/{total_frames}")
    
    cap.release()
    landmarker.close()
    
    valid_count = sum(1 for p in reference_poses if p['keypoints'])
    print(f"Done. {valid_count}/{len(reference_poses)} frames with valid poses")
    
    return reference_poses


def main():
    if len(sys.argv) < 2:
        print("Usage: python reference_builder.py <video_path>")
        sys.exit(1)
    
    video_path = sys.argv[1]
    output_path = Path(__file__).parent / "reference_poses.json"
    
    poses = extract_reference_poses(video_path)
    
    with open(output_path, 'w') as f:
        json.dump({
            'source_video': video_path,
            'frame_count': len(poses),
            'poses': poses
        }, f)
    
    print(f"Saved to {output_path}")


if __name__ == "__main__":
    main()
