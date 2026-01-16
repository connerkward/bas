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
    """Score poses based on movement with smoothing."""
    
    # Key landmarks for movement detection (full body)
    MOVEMENT_LANDMARKS = [
        0,   # nose
        11, 12,  # shoulders
        13, 14,  # elbows
        15, 16,  # wrists
        23, 24,  # hips
        25, 26,  # knees
        27, 28,  # ankles
    ]
    
    # Smoothing factor (0-1, lower = more smoothing)
    SMOOTHING_ALPHA = 0.08
    
    # Movement noise threshold (ignore tiny movements below this)
    # MediaPipe has ~1-3% jitter on stationary poses even with smoothing
    MOVEMENT_NOISE_FLOOR = 0.02  # ~2% of frame = noise
    
    def __init__(self, reference_path: str = None):
        self.reference = self._load_reference(reference_path) if reference_path else []
        self.current_frame_per_uuid: Dict[str, int] = {}
        self.previous_poses: Dict[str, List[Tuple]] = {}  # Track previous pose per UUID
        self.smoothed_scores: Dict[str, float] = {}  # Smoothed score per UUID
    
    def _load_reference(self, path: str) -> List[Optional[List[Tuple]]]:
        """Load reference poses from JSON."""
        with open(path, 'r') as f:
            data = json.load(f)
        
        # Extract just keypoints list (or None if missing)
        return [p['keypoints'] for p in data['poses']]
    
    def compute_movement(
        self,
        uuid: str,
        current_keypoints: List[Tuple]
    ) -> float:
        """
        Compute movement score based on pose change from previous frame.
        
        Returns raw movement score 0-100 (higher = more movement).
        """
        prev_keypoints = self.previous_poses.get(uuid)
        
        if not prev_keypoints or not current_keypoints:
            return 0.0
        
        total_movement = 0.0
        valid_count = 0
        
        for idx in self.MOVEMENT_LANDMARKS:
            if idx >= len(current_keypoints) or idx >= len(prev_keypoints):
                continue
            
            curr = current_keypoints[idx]
            prev = prev_keypoints[idx]
            
            # Skip if visibility too low
            if len(curr) >= 4 and curr[3] < 0.3:
                continue
            if len(prev) >= 4 and prev[3] < 0.3:
                continue
            
            # 2D distance (x, y normalized 0-1)
            dx = curr[0] - prev[0]
            dy = curr[1] - prev[1]
            movement = math.sqrt(dx * dx + dy * dy)
            
            # Apply noise floor - ignore tiny jitter
            if movement < self.MOVEMENT_NOISE_FLOOR:
                movement = 0.0
            
            total_movement += movement
            valid_count += 1
        
        if valid_count == 0:
            return 0.0
        
        avg_movement = total_movement / valid_count
        # Scale movement to 0-100: 0.015 movement per frame = 100 score
        raw_score = min(100.0, avg_movement * 6666)
        return raw_score
    
    def get_smoothed_score(self, uuid: str, raw_score: float) -> float:
        """Apply exponential moving average smoothing to score."""
        prev_smooth = self.smoothed_scores.get(uuid, raw_score)
        smoothed = prev_smooth + self.SMOOTHING_ALPHA * (raw_score - prev_smooth)
        self.smoothed_scores[uuid] = smoothed
        return round(smoothed, 1)
    
    def compute_similarity(
        self,
        live_keypoints: List[Tuple],
        ref_keypoints: List[Tuple]
    ) -> float:
        """
        Compute similarity between live pose and reference pose.
        (Legacy method, kept for compatibility)
        """
        if not ref_keypoints or not live_keypoints:
            return 0.0
        
        total_dist = 0.0
        valid_count = 0
        
        for idx in self.MOVEMENT_LANDMARKS[:9]:  # Upper body only
            if idx >= len(live_keypoints) or idx >= len(ref_keypoints):
                continue
            
            live = live_keypoints[idx]
            ref = ref_keypoints[idx]
            
            if len(live) >= 4 and live[3] < 0.3:
                continue
            if len(ref) >= 4 and ref[3] < 0.3:
                continue
            
            dx = live[0] - ref[0]
            dy = live[1] - ref[1]
            dist = math.sqrt(dx * dx + dy * dy)
            
            total_dist += dist
            valid_count += 1
        
        if valid_count == 0:
            return 0.0
        
        avg_dist = total_dist / valid_count
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
        Score a participant based on movement (smoothed).
        
        Returns dict ready to write as JSON.
        """
        # Compute raw movement score
        raw_movement = self.compute_movement(uuid, keypoints)
        
        # Apply smoothing
        smoothed_score = self.get_smoothed_score(uuid, raw_movement)
        
        # Store current pose for next frame comparison
        self.previous_poses[uuid] = keypoints.copy() if keypoints else None
        
        # Optional: also find reference frame for additional data
        ref_frame = 0
        if self.reference:
            ref_frame, _ = self.find_best_reference_frame(keypoints)
            self.current_frame_per_uuid[uuid] = ref_frame
        
        return {
            'uuid': uuid,
            'timestamp': time.time(),
            'reference_frame': ref_frame,
            'score_0_to_100': smoothed_score,
            'raw_movement': round(raw_movement, 1)
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
    
    # Reference is now optional (movement-based scoring)
    if reference_path.exists():
        scorer = PoseScorer(str(reference_path))
        print(f"Loaded reference: {reference_path}")
    else:
        scorer = PoseScorer(None)
        print("No reference loaded - using movement-based scoring only")
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
