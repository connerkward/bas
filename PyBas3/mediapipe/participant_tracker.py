#!/usr/bin/env python3
"""
Participant tracking using perceptual hashing (pHash).

Maintains stable UUIDs across frames using upper-body image hashing.
Persists UUIDs to participants_db.json for restart persistence.
"""

import cv2
import numpy as np
import json
import os
import time
from pathlib import Path
from uuid import uuid4
from typing import Optional, Dict, List, Tuple
import mediapipe as mp


class ParticipantTracker:
    """Tracks participants using perceptual hashing."""
    
    def __init__(
        self,
        db_path: str = "participants_db.json",
        hash_size: int = 8,
        threshold: int = 10,
        max_participants: int = 3,
        hash_history_size: int = 3,
        face_threshold_floor: int = 40
    ):
        self.db_path = db_path
        self.hash_size = hash_size
        self.threshold = threshold  # Hamming distance threshold (lower = more lenient matching)
        self.max_participants = max_participants
        self.hash_history_size = max(1, hash_history_size)
        self.face_threshold_floor = face_threshold_floor
        self.participants: Dict[str, Dict] = {}  # uuid -> {"phash": array, "phash_history": list, "last_seen": timestamp}
        self._load_db()
    
    def _load_db(self):
        """Load participant database from disk."""
        if os.path.exists(self.db_path):
            try:
                with open(self.db_path, 'r') as f:
                    data = json.load(f)
                    # Handle both old format (just hash) and new format (dict with hash + timestamp)
                    for uuid, value in data.items():
                        if isinstance(value, str):
                            # Old format: just hash string
                            phash = np.array(json.loads(value), dtype=np.uint8)
                            self.participants[uuid] = {
                                "phash": phash,
                                "phash_history": [phash] if phash.size else [],
                                "last_seen": None  # Unknown timestamp
                            }
                        elif isinstance(value, dict):
                            # New format: dict with phash and last_seen
                            phash = np.array(json.loads(value.get("phash", "[]")), dtype=np.uint8)
                            raw_history = value.get("phash_history", [])
                            history: List[np.ndarray] = []
                            if isinstance(raw_history, list):
                                for item in raw_history:
                                    if isinstance(item, str):
                                        try:
                                            history.append(np.array(json.loads(item), dtype=np.uint8))
                                        except (json.JSONDecodeError, ValueError):
                                            continue
                                    else:
                                        history.append(np.array(item, dtype=np.uint8))
                            if not history and phash.size:
                                history = [phash]
                            self.participants[uuid] = {
                                "phash": phash,
                                "phash_history": history,
                                "last_seen": value.get("last_seen")
                            }
            except (json.JSONDecodeError, KeyError, ValueError) as e:
                # Invalid file, start fresh
                print(f"Warning: Could not load participants DB: {e}")
                self.participants = {}
        else:
            self.participants = {}
    
    def _save_db(self):
        """Atomically save participant database to disk."""
        # Convert to JSON-serializable format with timestamps
        data = {}
        for uuid, info in self.participants.items():
            # Handle both old format (just array) and new format (dict)
            if isinstance(info, dict):
                phash = info.get("phash", np.array([], dtype=np.uint8))
                last_seen = info.get("last_seen")
                history = info.get("phash_history", [])
            else:
                # Old format: just numpy array
                phash = info
                last_seen = None
                history = []
            
            if not isinstance(phash, np.ndarray):
                phash = np.array(phash, dtype=np.uint8)
            
            history_list: List[List[int]] = []
            for item in history:
                if isinstance(item, np.ndarray):
                    history_list.append(item.tolist())
                elif isinstance(item, list):
                    history_list.append(item)
                elif isinstance(item, str):
                    try:
                        history_list.append(json.loads(item))
                    except (json.JSONDecodeError, ValueError):
                        continue
            if not history_list and phash.size:
                history_list = [phash.tolist()]
            if len(history_list) > self.hash_history_size:
                history_list = history_list[-self.hash_history_size:]
            
            data[uuid] = {
                "phash": json.dumps(phash.tolist()),
                "phash_history": history_list,
                "last_seen": last_seen
            }
        
        temp_path = f"{self.db_path}.tmp"
        with open(temp_path, 'w') as f:
            json.dump(data, f, indent=2)
        os.replace(temp_path, self.db_path)
    
    def save(self):
        """Public method to save database."""
        self._save_db()
    
    def compute_phash(self, image: np.ndarray) -> np.ndarray:
        """
        Compute perceptual hash (dHash variant) of image.
        
        Algorithm: resize to (hash_size+1) x hash_size, convert to grayscale,
        compute horizontal differences, flatten to binary array.
        """
        if image.size == 0:
            return np.array([])
        
        # Resize image
        resized = cv2.resize(image, (self.hash_size + 1, self.hash_size))
        
        # Convert to grayscale if needed
        if len(resized.shape) == 3:
            gray = cv2.cvtColor(resized, cv2.COLOR_BGR2GRAY)
        else:
            gray = resized
        
        # Normalize to reduce lighting variance
        gray = cv2.equalizeHist(gray)
        gray = cv2.GaussianBlur(gray, (3, 3), 0)
        
        # Compute horizontal differences
        diff = gray[:, 1:] > gray[:, :-1]
        
        # Flatten to 1D boolean array
        return diff.flatten().astype(np.uint8)
    
    def hamming_distance(self, hash1: np.ndarray, hash2: np.ndarray) -> int:
        """Compute Hamming distance between two hash arrays."""
        if hash1.shape != hash2.shape:
            return float('inf')
        return np.count_nonzero(hash1 != hash2)
    
    def get_face_crop(
        self,
        frame: np.ndarray,
        landmarks,
        frame_shape: Tuple[int, int, int]
    ) -> np.ndarray:
        """
        Extract face region from frame based on pose landmarks.
        
        Uses face landmarks (nose, eyes, ears, mouth) to define crop region.
        Face landmarks: 0-10 (nose, eyes, ears, mouth)
        """
        if not landmarks:
            return np.array([])
        
        # Handle both Solutions API format (landmarks.landmark) and Tasks API format (list)
        if hasattr(landmarks, 'landmark'):
            lm = landmarks.landmark
        elif isinstance(landmarks, list) and len(landmarks) > 0:
            lm = landmarks
        else:
            return np.array([])
        
        if len(lm) == 0:
            return np.array([])
        
        h, w = frame_shape[:2]
        
        # MediaPipe Pose face landmarks (0-10)
        # 0: nose
        # 1: left eye inner, 2: left eye, 3: left eye outer
        # 4: right eye inner, 5: right eye, 6: right eye outer
        # 7: left ear, 8: right ear
        # 9: mouth left, 10: mouth right
        
        # Find bounding box from face points only
        # Use all face landmarks for better coverage
        key_points = []
        for idx in [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]:  # All face landmarks
            if idx < len(lm):
                landmark = lm[idx]
                # Handle both Solutions API (has visibility) and Tasks API (may not)
                if hasattr(landmark, 'visibility'):
                    if landmark.visibility > 0.1:  # Very low threshold for face
                        key_points.append((landmark.x * w, landmark.y * h))
                elif hasattr(landmark, 'x'):
                    key_points.append((landmark.x * w, landmark.y * h))
        
        if len(key_points) < 2:
            return np.array([])
        
        key_points = np.array(key_points)
        x_min = int(np.min(key_points[:, 0]))
        x_max = int(np.max(key_points[:, 0]))
        y_min = int(np.min(key_points[:, 1]))
        y_max = int(np.max(key_points[:, 1]))
        
        # Expand crop for face (more margin for better face capture)
        margin = 30
        x_min = max(0, x_min - margin)
        x_max = min(w, x_max + margin)
        y_min = max(0, y_min - margin)
        y_max = min(h, y_max + margin)
        
        # Ensure valid crop
        if x_max <= x_min or y_max <= y_min:
            return np.array([])
        
        crop = frame[y_min:y_max, x_min:x_max]
        return crop
    
    def match_or_create(
        self,
        frame: np.ndarray,
        landmarks,
        segmentation_mask: Optional[np.ndarray] = None
    ) -> Optional[str]:
        """
        Match detected person to existing UUID or create new one.
        
        Args:
            frame: Full frame image
            landmarks: Pose landmarks
            segmentation_mask: Optional segmentation mask (if available, uses this instead of bounding box)
        
        Returns UUID string or None if matching fails.
        """
        # If segmentation mask available, use it for more accurate pHash
        # But still crop to face region for consistency
        if segmentation_mask is not None and segmentation_mask.size > 0:
            # Get face crop first
            face_crop = self.get_face_crop(frame, landmarks, frame.shape)
            if face_crop.size > 0:
                # Use face crop directly (more stable than masked frame)
                crop = face_crop
            else:
                # Fallback: apply mask to frame
                masked_frame = cv2.bitwise_and(frame, frame, mask=segmentation_mask)
                crop = masked_frame
        else:
            # Use face crop from landmarks
            crop = self.get_face_crop(frame, landmarks, frame.shape)
        
        if crop.size == 0:
            # Debug: crop is empty, likely landmark format issue
            # This can happen if face landmarks aren't visible or detected
            return None
        
        new_hash = self.compute_phash(crop)
        if new_hash.size == 0:
            return None
        
        # Try to match existing participant
        best_match = None
        best_distance = float('inf')
        current_time = time.time()
        
        # For face-based pHash, use very lenient threshold
        # Default threshold is 10, but for face we want ~30-35 for better matching across lighting/angles/poses
        # Face crops can vary significantly between frames, so we need a high threshold
        face_threshold = max(self.threshold * 3, self.face_threshold_floor)  # Very lenient for face matching
        
        for uuid, info in self.participants.items():
            # Handle both old format (string) and new format (array)
            stored_hash = info.get("phash", np.array([], dtype=np.uint8))
            if isinstance(stored_hash, str):
                stored_hash = np.array(json.loads(stored_hash), dtype=np.uint8)
            elif not isinstance(stored_hash, np.ndarray):
                stored_hash = np.array(stored_hash, dtype=np.uint8)
            
            history = info.get("phash_history", [])
            if not history:
                history = [stored_hash] if stored_hash.size else []
            
            best_local = float('inf')
            for hist_hash in history:
                if isinstance(hist_hash, str):
                    try:
                        hist_hash = np.array(json.loads(hist_hash), dtype=np.uint8)
                    except (json.JSONDecodeError, ValueError):
                        continue
                elif not isinstance(hist_hash, np.ndarray):
                    hist_hash = np.array(hist_hash, dtype=np.uint8)
                if hist_hash.size == 0:
                    continue
                best_local = min(best_local, self.hamming_distance(new_hash, hist_hash))
            
            distance = best_local
            if distance < face_threshold and distance < best_distance:
                best_distance = distance
                best_match = uuid
        
        if best_match:
            # Update hash and timestamp for matched participant
            existing_history = self.participants[best_match].get("phash_history", [])
            history = list(existing_history) if isinstance(existing_history, list) else []
            history.append(new_hash)
            if len(history) > self.hash_history_size:
                history = history[-self.hash_history_size:]
            self.participants[best_match] = {
                "phash": new_hash,
                "phash_history": history,
                "last_seen": current_time
            }
            self._save_db()
            return best_match
        
        # Check if we've hit max participants
        # Only block if we can't match to existing participant
        if len(self.participants) >= self.max_participants and best_match is None:
            # Try to match to least recently seen participant if all slots full
            if best_match is None:
                # Find least recently seen participant to potentially replace
                oldest_uuid = min(self.participants.items(), key=lambda x: x[1].get("last_seen", 0))[0]
                oldest_time = self.participants[oldest_uuid].get("last_seen", 0)
                # If oldest participant hasn't been seen in >10 seconds, replace them
                if current_time - oldest_time > 10.0:
                    del self.participants[oldest_uuid]
                    # Now we can create new participant
                else:
                    return None
        
        # Create new participant
        new_uuid = str(uuid4())[:8]
        self.participants[new_uuid] = {
            "phash": new_hash,
            "phash_history": [new_hash],
            "last_seen": current_time
        }
        self._save_db()
        return new_uuid


if __name__ == "__main__":
    # Simple test
    tracker = ParticipantTracker()
    print(f"Loaded {len(tracker.participants)} participants from DB")
