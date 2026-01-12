#!/usr/bin/env python3
"""
MediaPipe multi-person pose detection with pHash-based participant tracking.

Detects multiple people simultaneously using MediaPipe Tasks API,
assigns stable UUIDs via perceptual hashing, and filters by zone configuration.
"""

import cv2
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import numpy as np
import json
import os
import time
from pathlib import Path
from uuid import uuid4
from typing import Optional, Dict, List, Tuple

from participant_tracker import ParticipantTracker
from download_model import download_model
from ndi_streamer import NDIStreamer
from shared_memory_writer import SharedMemoryPoseWriter


class ZoneFilter:
    """Filters detections based on zone configuration."""
    
    def __init__(self, config_path: str = "zone_config.json"):
        self.config_path = config_path
        self.config = self._load_config()
        self._last_reload = time.time()
    
    def reload_if_needed(self, interval: float = 0.5):
        """Reload config from disk if interval elapsed."""
        if time.time() - self._last_reload > interval:
            self.config = self._load_config()
            self._last_reload = time.time()
    
    def _load_config(self) -> Dict:
        """Load zone configuration from JSON file."""
        default_config = {
            "screen_region": {"x": 0.0, "y": 0.0, "width": 1.0, "height": 1.0},
            "z_range": {"min": -10.0, "max": 10.0},
            "max_people": 3,
            "min_visibility": 0.1,
            "min_landmarks": 1
        }
        
        if os.path.exists(self.config_path):
            try:
                with open(self.config_path, 'r') as f:
                    config = json.load(f)
                    # Merge with defaults
                    default_config.update(config)
                    return default_config
            except (json.JSONDecodeError, KeyError):
                pass
        
        # Save default config if file doesn't exist
        with open(self.config_path, 'w') as f:
            json.dump(default_config, f, indent=2)
        
        return default_config
    
    def is_in_zone(self, landmarks) -> bool:
        """
        Check if pose landmarks are within configured zone.
        
        Args:
            landmarks: List of pose landmarks (Tasks API format)
        
        Returns:
            True if pose is in zone, False otherwise
        """
        if not landmarks or len(landmarks) < self.config["min_landmarks"]:
            return False
        
        # Check visibility (if available)
        visible_count = 0
        for lm in landmarks:
            if hasattr(lm, 'visibility') and lm.visibility >= self.config["min_visibility"]:
                visible_count += 1
            elif hasattr(lm, 'x'):  # Assume visible if has coordinates
                visible_count += 1
        
        if visible_count < self.config["min_landmarks"]:
            return False
        
        # Check screen region (x, y coordinates)
        screen_region = self.config["screen_region"]
        in_screen = False
        for lm in landmarks:
            if hasattr(lm, 'x') and hasattr(lm, 'y'):
                x, y = lm.x, lm.y
                if (screen_region["x"] <= x <= screen_region["x"] + screen_region["width"] and
                    screen_region["y"] <= y <= screen_region["y"] + screen_region["height"]):
                    in_screen = True
                    break
        
        if not in_screen:
            return False
        
        # Check z-range (depth) if available
        z_range = self.config["z_range"]
        in_z_range = False
        for lm in landmarks:
            if hasattr(lm, 'z'):
                z = lm.z
                if z_range["min"] <= z <= z_range["max"]:
                    in_z_range = True
                    break
        
        # If z is not available, assume it's in range
        return in_z_range if any(hasattr(lm, 'z') for lm in landmarks) else True


class MultiPersonDetector:
    """MediaPipe multi-person pose detector with participant tracking."""
    
    def __init__(
        self,
        zone_config_path: str = "zone_config.json",
        participants_db_path: str = "participants_db.json",
        min_detection_confidence: float = 0.5,
        min_tracking_confidence: float = 0.5,
        enable_segmentation: bool = False,
        num_poses: int = 3
    ):
        
        self.zone_filter = ZoneFilter(zone_config_path)
        self.tracker = ParticipantTracker(participants_db_path, max_participants=num_poses)
        self.num_poses = num_poses
        
        # Download model file if needed and use MediaPipe Tasks API for multi-person detection
        model_path = download_model()
        base_options = python.BaseOptions(model_asset_path=model_path)
        options = vision.PoseLandmarkerOptions(
            base_options=base_options,
            running_mode=vision.RunningMode.VIDEO,
            num_poses=num_poses,
            min_pose_detection_confidence=0.5,  # Restored to 0.5 for better robustness
            min_pose_presence_confidence=0.5,  # Restored to 0.5
            min_tracking_confidence=0.5,  # Restored to 0.5
            output_segmentation_masks=True  # Enabled for segmentation overlay
        )
        
        self.landmarker = vision.PoseLandmarker.create_from_options(options)
        
        # Initialize NDI streamer
        self.ndi_streamer = NDIStreamer()
        self.stream_resolution = (640, 480) # NDI stream resolution
        
        # Shared memory writer for Scoring module
        self.shared_memory_writer = SharedMemoryPoseWriter()
        
        # Pose connections for drawing (MediaPipe pose has 33 landmarks)
        self.POSE_CONNECTIONS = [
            (0, 1), (1, 2), (2, 3), (3, 7),
            (0, 4), (4, 5), (5, 6), (6, 8),
            (9, 10),
            (11, 12), (11, 13), (13, 15), (15, 17), (15, 19), (15, 21), (17, 19),
            (12, 14), (14, 16), (16, 18), (16, 20), (16, 22), (18, 20),
            (11, 23), (12, 24),
            (23, 24), (23, 25), (25, 27), (27, 29), (27, 31),
            (24, 26), (26, 28), (28, 30), (28, 32)
        ]
        
        self.current_participants: Dict[str, Dict] = {}
        self.frame_counter = 0
        self.start_time = time.time()
        self.last_segmentation_masks = None  # Store soft masks from pose landmarker
        self.last_hard_masks = None  # Store hard-edged binary masks (thresholded at 0.5)
        
        # Thumbnail output directory
        self.thumbnails_dir = Path(__file__).parent / "thumbnails"
        self.thumbnails_dir.mkdir(exist_ok=True)
        self.thumbnail_size = (160, 120)  # Low-res thumbnail size (width, height)
        self.saved_thumbnails = set()  # Track which UUIDs have thumbnails saved (save only once)
    
    def detect(self, frame: np.ndarray) -> List[Dict]:
        """
        Detect poses in frame and return list of participant data.
        
        Returns list of dicts with keys: uuid, landmarks, in_zone, timestamp
        """
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        
        # Process frame (synchronous VIDEO mode)
        timestamp_ms = int((time.time() - self.start_time) * 1000)
        detection_result = self.landmarker.detect_for_video(mp_image, timestamp_ms)
        
        detected = []
        
        # Debug: Check what MediaPipe returned
        if not detection_result:
            if self.frame_counter % 60 == 0:
                print(f"DEBUG: No detection_result returned")
            return detected
        
        # Check if poses detected
        has_poses = hasattr(detection_result, 'pose_landmarks') and detection_result.pose_landmarks
        if not has_poses:
            if self.frame_counter % 60 == 0:
                print(f"DEBUG: detection_result exists but no pose_landmarks")
            return detected
        
        # Process all detected poses (Tasks API supports multiple people)
        pose_landmarks_list = detection_result.pose_landmarks
        segmentation_masks = detection_result.segmentation_masks if hasattr(detection_result, 'segmentation_masks') else None
        
        # Store segmentation masks for visualization
        if segmentation_masks:
            self.last_segmentation_masks = segmentation_masks
        else:
            self.last_segmentation_masks = None
        
        # Create hard-edged binary masks by thresholding at 0.5
        # This converts soft alpha-blended edges to crisp binary (0 or 1) masks
        hard_masks = []
        if segmentation_masks:
            for seg_mask in segmentation_masks:
                try:
                    mask_np = seg_mask.numpy_view() if hasattr(seg_mask, 'numpy_view') else seg_mask
                    # Ensure 2D mask
                    if len(mask_np.shape) == 3 and mask_np.shape[2] == 1:
                        mask_np = mask_np.squeeze(axis=2)
                    
                    # Convert to uint8 for processing
                    mask_uint8 = (mask_np * 255).astype(np.uint8)
                    
                    # Apply Gaussian blur to smooth edges before thresholding
                    mask_blurred = cv2.GaussianBlur(mask_uint8, (5, 5), 0)
                    
                    # Apply threshold for hard edges (more robust: use Otsu's method or fixed threshold)
                    # Increased threshold to 140 for tighter edges
                    _, mask_binary = cv2.threshold(mask_blurred, 140, 255, cv2.THRESH_BINARY)
                    
                    # Clean up mask: remove small noise, fill holes with larger kernels and iterations
                    kernel_close = np.ones((7, 7), np.uint8) # Larger kernel for closing
                    kernel_open = np.ones((5, 5), np.uint8)  # Larger kernel for opening
                    
                    mask_binary = cv2.morphologyEx(mask_binary, cv2.MORPH_CLOSE, kernel_close, iterations=2) # Fill holes more aggressively
                    mask_binary = cv2.morphologyEx(mask_binary, cv2.MORPH_OPEN, kernel_open, iterations=2)  # Remove noise more aggressively
                    
                    # Find the largest contour and keep only that (removes stray blobs)
                    contours, _ = cv2.findContours(mask_binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
                    if contours:
                        largest_contour = max(contours, key=cv2.contourArea)
                        mask_largest_contour = np.zeros_like(mask_binary)
                        cv2.drawContours(mask_largest_contour, [largest_contour], -1, 255, -1)
                        mask_binary = mask_largest_contour
                    
                    # Convert back to float32 (0.0 or 1.0)
                    binary_mask = (mask_binary / 255.0).astype(np.float32)
                    hard_masks.append(binary_mask)
                except Exception as e:
                    if self.frame_counter <= 5:
                        print(f"DEBUG: Thresholding mask error: {e}")
                    pass
        self.last_hard_masks = hard_masks if hard_masks else None
        
        # Process each detected pose
        for idx, landmarks in enumerate(pose_landmarks_list):
            # Get corresponding segmentation mask if available
            seg_mask = None
            if segmentation_masks and idx < len(segmentation_masks):
                seg_mask_np = segmentation_masks[idx]
                if hasattr(seg_mask_np, 'numpy_view'):
                    seg_mask = seg_mask_np.numpy_view()
                else:
                    seg_mask = seg_mask_np
                # Ensure 2D mask
                if len(seg_mask.shape) == 3 and seg_mask.shape[2] == 1:
                    seg_mask = seg_mask.squeeze(axis=2)
            
            # Check if pose is in zone
            in_zone = self.zone_filter.is_in_zone(landmarks)
            
            # Match or create UUID using face-based pHash
            # Pass landmarks directly (list format) - participant_tracker handles both formats
            uuid = self.tracker.match_or_create(frame, landmarks, seg_mask)
            
            if uuid:
                if self.frame_counter <= 5 or self.frame_counter % 30 == 0:
                    print(f"DEBUG: Matched/created UUID {uuid[:8]} for pose {idx} (frame {self.frame_counter})")
            else:
                # If UUID creation failed, still add pose for visualization (temporary UUID)
                temp_uuid = f"temp_{idx}_{self.frame_counter}"
                if self.frame_counter <= 5 or self.frame_counter % 30 == 0:
                    print(f"DEBUG: UUID match failed for pose {idx}, using temp UUID {temp_uuid} (frame {self.frame_counter})")
                    print(f"DEBUG:   - Participants in DB: {len(self.tracker.participants)}")
                    print(f"DEBUG:   - Max participants: {self.tracker.max_participants}")
            
            detected.append({
                "uuid": uuid if uuid else temp_uuid,
                "landmarks": landmarks,
                "in_zone": in_zone,
                "timestamp": time.time()
            })
        
        # Write poses to shared memory for Scoring module (only real UUIDs, skip temp)
        real_participants = [p for p in detected if not p["uuid"].startswith("temp_")]
        if real_participants:
            self.shared_memory_writer.write_poses(real_participants)
        
        self.frame_counter += 1
        return detected
    
    def _extract_bounding_box_crop(self, frame: np.ndarray, landmarks, w: int, h: int) -> np.ndarray:
        """Fallback: extract bounding box crop if segmentation mask fails."""
        if not landmarks:
            return np.zeros((h, w, 3), dtype=np.uint8)
        
        # Get bounding box from landmarks
        x_coords = [lm.x * w for lm in landmarks if hasattr(lm, 'x')]
        y_coords = [lm.y * h for lm in landmarks if hasattr(lm, 'y')]
        
        if not x_coords or not y_coords:
            return np.zeros((h, w, 3), dtype=np.uint8)
        
        x_min = max(0, int(min(x_coords) - 20))
        x_max = min(w, int(max(x_coords) + 20))
        y_min = max(0, int(min(y_coords) - 20))
        y_max = min(h, int(max(y_coords) + 20))
        
        if x_max <= x_min or y_max <= y_min:
            return np.zeros((h, w, 3), dtype=np.uint8)
        
        crop = frame[y_min:y_max, x_min:x_max]
        return crop
    
    def create_participant_stream_frame(self, frame: np.ndarray, uuid: str, landmarks, hard_mask: Optional[np.ndarray] = None) -> Optional[np.ndarray]:
        """
        Create a composite frame for NDI streaming: hard-edged segmented person + skeleton overlay.
        
        Args:
            frame: Full frame
            uuid: Participant UUID
            landmarks: Pose landmarks
            hard_mask: Binary segmentation mask (0.0 or 1.0)
        
        Returns:
            BGRA frame with segmented person + skeleton, or None if failed
        """
        try:
            h, w = frame.shape[:2]
            
            # Start with black background
            output = np.zeros((h, w, 3), dtype=np.uint8)
            
            # Apply hard-edged segmentation mask to extract person
            if hard_mask is not None:
                try:
                    # Ensure mask is 2D and matches frame size
                    mask_uint8 = (hard_mask * 255).astype(np.uint8)
                    if mask_uint8.shape[:2] != frame.shape[:2]:
                        mask_uint8 = cv2.resize(mask_uint8, (w, h), interpolation=cv2.INTER_NEAREST)
                    
                    # Ensure mask is binary (hard edges) - threshold again for robustness
                    _, mask_binary = cv2.threshold(mask_uint8, 127, 255, cv2.THRESH_BINARY)
                    
                    # Apply morphological operations to clean up mask (remove noise, fill holes)
                    kernel = np.ones((3, 3), np.uint8)
                    mask_binary = cv2.morphologyEx(mask_binary, cv2.MORPH_CLOSE, kernel)
                    mask_binary = cv2.morphologyEx(mask_binary, cv2.MORPH_OPEN, kernel)
                    
                    # Apply mask to extract person (hard edges)
                    output = cv2.bitwise_and(frame, frame, mask=mask_binary)
                except Exception as e:
                    if self.frame_counter <= 5:
                        print(f"DEBUG: Mask application error for {uuid[:8]}: {e}")
                    # Fallback: use bounding box
                    output = self._extract_bounding_box_crop(frame, landmarks, w, h)
            else:
                # Fallback: use bounding box if no mask
                output = self._extract_bounding_box_crop(frame, landmarks, w, h)
            
            # Draw skeleton overlay on segmented person
            if landmarks and len(landmarks) >= 33:
                color = (0, 255, 0)  # Green skeleton
                
                # Draw connections (skeleton)
                for connection in self.POSE_CONNECTIONS:
                    start_idx, end_idx = connection
                    if start_idx < len(landmarks) and end_idx < len(landmarks):
                        start = landmarks[start_idx]
                        end = landmarks[end_idx]
                        if hasattr(start, 'x') and hasattr(end, 'x'):
                            start_pt = (int(start.x * w), int(start.y * h))
                            end_pt = (int(end.x * w), int(end.y * h))
                            cv2.line(output, start_pt, end_pt, color, 3)
                
                # Draw key points (joints)
                for landmark in landmarks:
                    if hasattr(landmark, 'x') and hasattr(landmark, 'y'):
                        x = int(landmark.x * w)
                        y = int(landmark.y * h)
                        cv2.circle(output, (x, y), 5, color, -1)
            
            # Convert to BGRA for NDI streaming
            bgra_frame = cv2.cvtColor(output, cv2.COLOR_BGR2BGRA)
            return bgra_frame
        except Exception as e:
            if self.frame_counter <= 5:
                print(f"DEBUG: Stream frame creation error for {uuid[:8]}: {e}")
            return None
    
    def save_participant_thumbnail(self, frame: np.ndarray, uuid: str, landmarks, hard_mask: Optional[np.ndarray] = None):
        """
        Save a thumbnail that matches the NDI stream output: segmented person + skeleton overlay.
        
        Args:
            frame: Full frame
            uuid: Participant UUID (MUST be real UUID, not temp - caller should filter)
            landmarks: Pose landmarks
            hard_mask: Binary segmentation mask
        """
        # Only save once per UUID (caller should already filter temp UUIDs)
        if uuid in self.saved_thumbnails:
            return
        
        try:
            # Create the same composite frame as NDI stream (segmented + skeleton)
            stream_frame = self.create_participant_stream_frame(frame, uuid, landmarks, hard_mask)
            if stream_frame is None:
                return
            
            # Convert BGRA to BGR for saving
            bgr_frame = cv2.cvtColor(stream_frame, cv2.COLOR_BGRA2BGR)
            
            # Resize to thumbnail size
            thumbnail = cv2.resize(bgr_frame, self.thumbnail_size, interpolation=cv2.INTER_AREA)
            
            # Save thumbnail
            thumbnail_path = self.thumbnails_dir / f"participant_{uuid}.jpg"
            cv2.imwrite(str(thumbnail_path), thumbnail)
            self.saved_thumbnails.add(uuid)
            
        except Exception as e:
            if self.frame_counter <= 5:
                print(f"DEBUG: Thumbnail save error for {uuid[:8]}: {e}")
            import traceback
            traceback.print_exc()
    
    def close(self):
        """Clean up resources."""
        if self.landmarker:
            self.landmarker.close()
        self.ndi_streamer.close()
        self.shared_memory_writer.close()
        self.tracker.save()


def main():
    """Main function with video source support."""
    import argparse
    parser = argparse.ArgumentParser(description="Multi-person pose detector")
    parser.add_argument("--source", "-s", default="0",
                        help="Video source: camera index (default: 0) or video file path")
    parser.add_argument("--loop", "-l", action="store_true",
                        help="Loop video file (for testing)")
    parser.add_argument("--persist", "-p", action="store_true",
                        help="Keep participants_db.json from previous run (default: clear)")
    args = parser.parse_args()
    
    # Clear participants by default (unless --persist)
    if not args.persist:
        db_path = Path(__file__).parent / "participants_db.json"
        if db_path.exists():
            db_path.unlink()
            print("Cleared participants_db.json")
        # Clear thumbnails
        thumb_dir = Path(__file__).parent / "thumbnails"
        if thumb_dir.exists():
            for f in thumb_dir.glob("participant_*.jpg"):
                f.unlink()
            print("Cleared thumbnails")
        # Clear score files
        score_dir = Path(__file__).parent.parent / "scoring" / "output"
        if score_dir.exists():
            for f in score_dir.glob("participant_*_score.json"):
                if "test" not in f.name:
                    f.unlink()
            print("Cleared score files")
    
    detector = MultiPersonDetector(num_poses=3)
    
    # Parse source - int for camera, string for file
    source = int(args.source) if args.source.isdigit() else args.source
    cap = cv2.VideoCapture(source)
    
    # Set lower resolution for faster processing (reduces lag)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
    
    # Verify resolution was set
    actual_w = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    actual_h = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    print(f"Camera resolution: {actual_w}x{actual_h}")
    
    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                if args.loop and not str(source).isdigit():
                    cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
                    continue
                break
            
            participants = detector.detect(frame)
            detector.frame_counter += 1
            
            # Save thumbnails and create NDI streams for each detected participant
            # ONLY for real UUIDs (skip temp UUIDs)
            hard_masks = detector.last_hard_masks
            for idx, p in enumerate(participants):
                uuid = p["uuid"]
                
                # SKIP temp UUIDs completely - no thumbnails, no streams
                if uuid.startswith("temp_"):
                    continue
                
                landmarks = p["landmarks"]
                # Get corresponding hard mask if available
                hard_mask = hard_masks[idx] if hard_masks and idx < len(hard_masks) else None
                
                # Save thumbnail (once per participant, only real UUIDs)
                detector.save_participant_thumbnail(frame, uuid, landmarks, hard_mask)
                
                # Create NDI stream if it doesn't exist
                if uuid not in detector.ndi_streamer.streams:
                    detector.ndi_streamer.create_stream(
                        uuid,
                        detector.stream_resolution[0],
                        detector.stream_resolution[1],
                        fps=30.0
                    )
                
                # Create composite frame (segmented person + skeleton) and send to NDI
                stream_frame = detector.create_participant_stream_frame(frame, uuid, landmarks, hard_mask)
                if stream_frame is not None:
                    # Resize to stream resolution
                    stream_frame_resized = cv2.resize(
                        stream_frame,
                        detector.stream_resolution,
                        interpolation=cv2.INTER_LINEAR
                    )
                    detector.ndi_streamer.send_frame(uuid, stream_frame_resized)
            
            # Reload zone config (for live slider updates)
            detector.zone_filter.reload_if_needed()
            
            # Get segmentation masks for overlay
            seg_masks = detector.last_segmentation_masks
            
            # Draw pose skeleton on left side
            annotated = frame.copy()
            h, w = frame.shape[:2]
            
            # Draw zone rectangle with semi-transparent fill
            zone = detector.zone_filter.config["screen_region"]
            zx1 = int(zone["x"] * w)
            zy1 = int(zone["y"] * h)
            zx2 = int((zone["x"] + zone["width"]) * w)
            zy2 = int((zone["y"] + zone["height"]) * h)
            # Draw semi-transparent cyan fill
            overlay = annotated.copy()
            cv2.rectangle(overlay, (zx1, zy1), (zx2, zy2), (255, 255, 0), -1)
            cv2.addWeighted(overlay, 0.15, annotated, 0.85, 0, annotated)
            # Draw thick border
            cv2.rectangle(annotated, (zx1, zy1), (zx2, zy2), (0, 255, 255), 3)
            # Label
            cv2.putText(annotated, "ZONE", (zx1 + 5, zy1 + 25), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 255), 2)
            
            if participants:
                for p in participants:
                    landmarks = p["landmarks"]
                    if not landmarks or len(landmarks) < 33:
                        continue
                    
                    color = (0, 255, 0) if p["in_zone"] else (0, 0, 255)
                    
                    # Draw connections (skeleton) - thicker lines
                    for connection in detector.POSE_CONNECTIONS:
                        start_idx, end_idx = connection
                        if start_idx < len(landmarks) and end_idx < len(landmarks):
                            start = landmarks[start_idx]
                            end = landmarks[end_idx]
                            if hasattr(start, 'x') and hasattr(end, 'x'):
                                start_pt = (int(start.x * w), int(start.y * h))
                                end_pt = (int(end.x * w), int(end.y * h))
                                cv2.line(annotated, start_pt, end_pt, color, 4)
                    
                    # Draw key points (joints) - larger circles
                    for landmark in landmarks:
                        if hasattr(landmark, 'x') and hasattr(landmark, 'y'):
                            x = int(landmark.x * w)
                            y = int(landmark.y * h)
                            cv2.circle(annotated, (x, y), 6, color, -1)
                    
                    # Draw UUID label
                    if len(landmarks) > 0 and hasattr(landmarks[0], 'x'):
                        nose = landmarks[0]
                        uuid_text = f"{p['uuid'][:8]} {'IN' if p['in_zone'] else 'OUT'}"
                        cv2.putText(annotated, uuid_text, (int(nose.x * w), int(nose.y * h) - 20),
                                   cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)
            
            # Get hard-edged binary masks (thresholded from pose segmentation)
            hard_masks = detector.last_hard_masks
            
            # Create segmentation overlay on right side with hard edges
            seg_overlay = frame.copy()
            if hard_masks:
                for idx, hard_mask in enumerate(hard_masks):
                    try:
                        # Convert binary mask (0.0 or 1.0) to uint8 (0 or 255) - hard edges!
                        mask_uint8 = (hard_mask * 255).astype(np.uint8)
                        
                        # Ensure mask matches frame dimensions (use nearest neighbor to preserve hard edges)
                        if mask_uint8.shape[:2] != frame.shape[:2]:
                            mask_uint8 = cv2.resize(mask_uint8, (w, h), interpolation=cv2.INTER_NEAREST)
                        
                        # Create colored overlay (green tint) with hard edges
                        color_mask = np.zeros_like(frame)
                        color_mask[:, :, 1] = mask_uint8  # Green channel
                        seg_overlay = cv2.addWeighted(seg_overlay, 0.4, color_mask, 0.6, 0)  # 60% green blend
                        
                        # Draw skeleton on segmentation overlay if we have participants
                        if idx < len(participants):
                            p = participants[idx]
                            landmarks = p["landmarks"]
                            if landmarks and len(landmarks) >= 33:
                                color = (0, 255, 0) if p["in_zone"] else (0, 0, 255)
                                for connection in detector.POSE_CONNECTIONS:
                                    start_idx, end_idx = connection
                                    if start_idx < len(landmarks) and end_idx < len(landmarks):
                                        start = landmarks[start_idx]
                                        end = landmarks[end_idx]
                                        if hasattr(start, 'x') and hasattr(end, 'x'):
                                            start_pt = (int(start.x * w), int(start.y * h))
                                            end_pt = (int(end.x * w), int(end.y * h))
                                            cv2.line(seg_overlay, start_pt, end_pt, color, 3)
                    except Exception as e:
                        if detector.frame_counter <= 5:
                            print(f"DEBUG: Hard mask visualization error: {e}")
                        pass
            
            # Combine side by side
            combined = np.hstack([annotated, seg_overlay])
            
            # Add labels
            cv2.putText(combined, "Pose Skeleton", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (255, 255, 255), 2)
            cv2.putText(combined, "Segmentation", (w + 10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (255, 255, 255), 2)
            
            # Show combined view
            cv2.imshow("Pose Detection + Segmentation", combined)
            
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
    finally:
        cap.release()
        cv2.destroyAllWindows()
        detector.close()


if __name__ == "__main__":
    main()
