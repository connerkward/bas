#!/usr/bin/env python3
"""
Pose Scorer - Process 2

Reads poses from shared memory, compares to reference video,
writes per-participant score JSON files.

Usage:
    python pose_scorer.py [--reference reference_poses.json]
"""

import argparse
import json
import math
import os
import time
from pathlib import Path
from typing import Dict, List, Optional, Tuple

from shared_memory_reader import SharedMemoryPoseReader


class PoseScorer:
    """Compare live poses to reference and compute similarity scores."""
    
    # Key landmarks for scoring (upper body focus)
    SCORING_LANDMARKS = [
        0,   # nose
        11, 12,  # shoulders
        13, 14,  # elbows
        15, 16,  # wrists
        23, 24,  # hips
    ]
    
    def __init__(self, reference_path: str):
        self.reference = self._load_reference(reference_path)
        self.current_frame_per_uuid: Dict[str, int] = {}
    
    def _load_reference(self, path: str) -> List[Optional[List[Tuple]]]:
        """Load reference poses from JSON."""
        with open(path, 'r') as f:
            data = json.load(f)
        
        # Extract just keypoints list (or None if missing)
        return [p['keypoints'] for p in data['poses']]
    
    def compute_similarity(
        self,
        live_keypoints: List[Tuple],
        ref_keypoints: List[Tuple]
    ) -> float:
        """
        Compute similarity between live pose and reference pose.
        
        Uses weighted Euclidean distance on normalized coordinates.
        Returns score 0-100 (higher = more similar).
        """
        if not ref_keypoints or not live_keypoints:
            return 0.0
        
        total_dist = 0.0
        valid_count = 0
        
        for idx in self.SCORING_LANDMARKS:
            if idx >= len(live_keypoints) or idx >= len(ref_keypoints):
                continue
            
            live = live_keypoints[idx]
            ref = ref_keypoints[idx]
            
            # Skip if visibility too low (threshold 0.3)
            if len(live) >= 4 and live[3] < 0.3:
                continue
            if len(ref) >= 4 and ref[3] < 0.3:
                continue
            
            # 2D distance (x, y normalized 0-1)
            dx = live[0] - ref[0]
            dy = live[1] - ref[1]
            dist = math.sqrt(dx * dx + dy * dy)
            
            total_dist += dist
            valid_count += 1
        
        if valid_count == 0:
            return 0.0
        
        avg_dist = total_dist / valid_count
        # Convert distance to score: 0 distance = 100, 0.5+ distance = 0
        score = max(0.0, min(100.0, (1.0 - avg_dist * 2) * 100))
        return round(score, 1)
    
    def find_best_reference_frame(
        self,
        live_keypoints: List[Tuple],
        search_window: int = 30
    ) -> Tuple[int, float]:
        """
        Find the reference frame that best matches the live pose.
        
        Returns (frame_index, score).
        """
        best_frame = 0
        best_score = 0.0
        
        for i, ref_kp in enumerate(self.reference):
            if ref_kp is None:
                continue
            score = self.compute_similarity(live_keypoints, ref_kp)
            if score > best_score:
                best_score = score
                best_frame = i
        
        return best_frame, best_score
    
    def score_pose(self, uuid: str, keypoints: List[Tuple]) -> Dict:
        """
        Score a participant's pose against reference.
        
        Returns dict ready to write as JSON.
        """
        ref_frame, score = self.find_best_reference_frame(keypoints)
        self.current_frame_per_uuid[uuid] = ref_frame
        
        return {
            'uuid': uuid,
            'timestamp': time.time(),
            'reference_frame': ref_frame,
            'score_0_to_100': score
        }


def write_score_json(output_dir: Path, uuid: str, score_data: Dict, in_zone: bool):
    """Atomically write score JSON for a participant."""
    score_data['in_zone'] = in_zone
    
    filename = f"participant_{uuid}_score.json"
    final_path = output_dir / filename
    temp_path = output_dir / f".{filename}.tmp"
    
    with open(temp_path, 'w') as f:
        json.dump(score_data, f, indent=2)
    
    os.replace(temp_path, final_path)


def main():
    parser = argparse.ArgumentParser(description='Pose Scorer')
    parser.add_argument(
        '--reference',
        default='reference_poses.json',
        help='Path to reference poses JSON'
    )
    parser.add_argument(
        '--output-dir',
        default='output',
        help='Directory for score JSON files'
    )
    parser.add_argument(
        '--poll-rate',
        type=float,
        default=30.0,
        help='Scoring updates per second'
    )
    args = parser.parse_args()
    
    reference_path = Path(__file__).parent / args.reference
    output_dir = Path(__file__).parent / args.output_dir
    output_dir.mkdir(exist_ok=True)
    
    if not reference_path.exists():
        print(f"Reference not found: {reference_path}")
        print("Run: python reference_builder.py <video_path>")
        return
    
    scorer = PoseScorer(str(reference_path))
    reader = SharedMemoryPoseReader()
    
    print("Waiting for shared memory buffer 'bas_pose_data'...")
    while not reader.connect():
        time.sleep(1.0)
    
    print(f"Connected. Scoring at {args.poll_rate} Hz, output: {output_dir}")
    poll_interval = 1.0 / args.poll_rate
    
    try:
        while True:
            start = time.time()
            
            poses = reader.read_poses()
            
            for pose in poses:
                score_data = scorer.score_pose(pose['uuid'], pose['keypoints'])
                write_score_json(output_dir, pose['uuid'], score_data, pose['in_zone'])
            
            elapsed = time.time() - start
            sleep_time = poll_interval - elapsed
            if sleep_time > 0:
                time.sleep(sleep_time)
    
    except KeyboardInterrupt:
        print("\nStopping scorer")
    finally:
        reader.close()


if __name__ == "__main__":
    main()
