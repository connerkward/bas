#!/usr/bin/env python3
"""
Test script for pose scorer with mock data.

Tests scoring logic without requiring shared memory from Vision module.
"""

import json
import time
from pathlib import Path
from pose_scorer import PoseScorer, write_score_json


def create_mock_reference(num_frames: int = 100) -> Path:
    """Create a mock reference_poses.json for testing."""
    ref_path = Path(__file__).parent / "test_reference_poses.json"
    
    # Mock reference pose (neutral standing pose, normalized coordinates)
    mock_keypoints = [
        (0.5, 0.1, 0.0, 1.0),  # nose (0)
        (0.5, 0.15, 0.0, 1.0),  # eyes
        (0.5, 0.2, 0.0, 1.0),   # head
        (0.5, 0.3, 0.0, 1.0),   # neck
        (0.45, 0.35, 0.0, 1.0),  # left shoulder (11)
        (0.55, 0.35, 0.0, 1.0),  # right shoulder (12)
        (0.4, 0.5, 0.0, 1.0),   # left elbow (13)
        (0.6, 0.5, 0.0, 1.0),   # right elbow (14)
        (0.35, 0.65, 0.0, 1.0),  # left wrist (15)
        (0.65, 0.65, 0.0, 1.0),  # right wrist (16)
        (0.48, 0.55, 0.0, 1.0),  # left hip (23)
        (0.52, 0.55, 0.0, 1.0),  # right hip (24)
    ]
    
    # Pad to 33 keypoints (MediaPipe pose has 33)
    while len(mock_keypoints) < 33:
        mock_keypoints.append((0.5, 0.5, 0.0, 0.0))
    
    poses = []
    for i in range(num_frames):
        poses.append({
            'frame_index': i,
            'timestamp_ms': i * 33,  # ~30 FPS
            'keypoints': mock_keypoints
        })
    
    with open(ref_path, 'w') as f:
        json.dump({
            'source_video': 'test_mock.mp4',
            'frame_count': num_frames,
            'poses': poses
        }, f, indent=2)
    
    return ref_path


def test_scorer():
    """Test PoseScorer with mock data."""
    print("Creating mock reference poses...")
    ref_path = create_mock_reference(100)
    
    print(f"Loading scorer with reference: {ref_path}")
    scorer = PoseScorer(str(ref_path))
    
    # Mock live pose (slightly offset from reference)
    mock_live = [
        (0.51, 0.11, 0.0, 1.0),   # nose
        (0.51, 0.16, 0.0, 1.0),
        (0.51, 0.21, 0.0, 1.0),
        (0.51, 0.31, 0.0, 1.0),
        (0.46, 0.36, 0.0, 1.0),   # left shoulder
        (0.56, 0.36, 0.0, 1.0),   # right shoulder
        (0.41, 0.51, 0.0, 1.0),   # left elbow
        (0.61, 0.51, 0.0, 1.0),   # right elbow
        (0.36, 0.66, 0.0, 1.0),   # left wrist
        (0.66, 0.66, 0.0, 1.0),   # right wrist
        (0.49, 0.56, 0.0, 1.0),   # left hip
        (0.53, 0.56, 0.0, 1.0),   # right hip
    ]
    while len(mock_live) < 33:
        mock_live.append((0.5, 0.5, 0.0, 0.0))
    
    print("Testing similarity computation...")
    score_data = scorer.score_pose("test-uuid-1234", mock_live)
    
    print(f"Result: {json.dumps(score_data, indent=2)}")
    
    # Test JSON output
    output_dir = Path(__file__).parent / "output"
    output_dir.mkdir(exist_ok=True)
    write_score_json(output_dir, "test-uuid-1234", score_data, True)
    
    output_file = output_dir / "participant_test-uuid-1234_score.json"
    if output_file.exists():
        print(f"✅ Score JSON written: {output_file}")
        with open(output_file, 'r') as f:
            written = json.load(f)
            print(f"   Content: {json.dumps(written, indent=2)}")
    else:
        print("❌ Score JSON not written")
    
    # Cleanup
    if ref_path.exists():
        ref_path.unlink()
        print(f"Cleaned up test reference: {ref_path}")


if __name__ == "__main__":
    test_scorer()
