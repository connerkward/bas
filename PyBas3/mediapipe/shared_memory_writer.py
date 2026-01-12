#!/usr/bin/env python3
"""
Shared memory writer for pose data to Scoring module.

Writes participant poses to `bas_pose_data` buffer for inter-process communication.
Uses common module for protocol definitions.
"""

import sys
from pathlib import Path
from multiprocessing import shared_memory
from typing import List, Optional
import time

# Add parent directory to path for common module
sys.path.insert(0, str(Path(__file__).parent.parent))

from common.protocols import (
    SHARED_MEMORY_BUFFER_NAME,
    MAX_PARTICIPANTS,
    POSE_BUFFER_SIZE,
    POSE_RECORD_SIZE,
    ParticipantPose,
    PoseKeypoint,
)
from common.shared_memory import encode_pose


class SharedMemoryPoseWriter:
    """Writes pose data to shared memory buffer for Scoring module."""
    
    def __init__(
        self,
        buffer_name: str = SHARED_MEMORY_BUFFER_NAME,
        max_participants: int = MAX_PARTICIPANTS
    ):
        self.buffer_name = buffer_name
        self.max_participants = max_participants
        self.record_size = POSE_RECORD_SIZE
        self.shm: Optional[shared_memory.SharedMemory] = None
        self._create_or_connect()
    
    def _create_or_connect(self):
        """Create or connect to shared memory buffer."""
        try:
            # Try to connect to existing buffer first
            self.shm = shared_memory.SharedMemory(name=self.buffer_name)
        except FileNotFoundError:
            # Create new buffer if it doesn't exist
            self.shm = shared_memory.SharedMemory(
                name=self.buffer_name,
                create=True,
                size=POSE_BUFFER_SIZE
            )
    
    def write_poses(self, participants: List[dict]):
        """
        Write participant poses to shared memory.
        
        Args:
            participants: List of dicts with keys: uuid, landmarks, in_zone, timestamp
                - uuid: str (participant UUID)
                - landmarks: List of MediaPipe landmark objects (33 landmarks)
                - in_zone: bool
                - timestamp: float (optional, defaults to current time)
        """
        if not self.shm:
            return
        
        # Clear buffer first (fill with zeros)
        buffer = bytearray(POSE_BUFFER_SIZE)
        
        # Convert participants to ParticipantPose and encode
        offset = 0
        for participant in participants[:self.max_participants]:
            pose = self._convert_to_participant_pose(participant)
            if pose:
                offset = encode_pose(pose, buffer, offset)
        
        # Write to shared memory (atomic write)
        # Use memoryview to write bytearray to shared memory buffer
        self.shm.buf[:POSE_BUFFER_SIZE] = memoryview(buffer)
    
    def _convert_to_participant_pose(self, participant: dict) -> Optional[ParticipantPose]:
        """
        Convert participant dict to ParticipantPose.
        
        Args:
            participant: Dict with uuid, landmarks, in_zone, timestamp
        
        Returns:
            ParticipantPose or None if conversion fails
        """
        try:
            uuid = participant.get("uuid")
            landmarks = participant.get("landmarks")
            in_zone = participant.get("in_zone", False)
            timestamp = participant.get("timestamp", time.time())
            
            if not uuid or not landmarks:
                return None
            
            # Convert MediaPipe landmarks to PoseKeypoint list
            keypoints = []
            for lm in landmarks:
                if hasattr(lm, 'x') and hasattr(lm, 'y'):
                    x = lm.x
                    y = lm.y
                    z = lm.z if hasattr(lm, 'z') else 0.0
                    visibility = lm.visibility if hasattr(lm, 'visibility') else 1.0
                    keypoints.append(PoseKeypoint(x=x, y=y, z=z, visibility=visibility))
                else:
                    # Fallback: assume tuple/list format
                    if isinstance(lm, (tuple, list)) and len(lm) >= 2:
                        x, y = lm[0], lm[1]
                        z = lm[2] if len(lm) > 2 else 0.0
                        visibility = lm[3] if len(lm) > 3 else 1.0
                        keypoints.append(PoseKeypoint(x=x, y=y, z=z, visibility=visibility))
                    else:
                        # Invalid landmark, skip
                        continue
            
            # Ensure we have exactly 33 landmarks (MediaPipe pose format)
            if len(keypoints) < 33:
                # Pad with zero keypoints if needed
                while len(keypoints) < 33:
                    keypoints.append(PoseKeypoint(x=0.0, y=0.0, z=0.0, visibility=0.0))
            elif len(keypoints) > 33:
                # Truncate if too many
                keypoints = keypoints[:33]
            
            return ParticipantPose(
                uuid=uuid,
                timestamp=timestamp,
                keypoints=keypoints,
                in_zone=in_zone
            )
        except Exception as e:
            print(f"DEBUG: Error converting participant to ParticipantPose: {e}")
            return None
    
    def close(self):
        """Close shared memory connection (does not unlink - Scoring module may still be using it)."""
        if self.shm:
            self.shm.close()
            self.shm = None
    
    def unlink(self):
        """Unlink shared memory (removes it from system). Only call when no other processes are using it."""
        if self.shm:
            self.shm.close()
            self.shm.unlink()
            self.shm = None
    
    def __enter__(self):
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        self.close()


if __name__ == "__main__":
    # Test: Create writer and write sample poses
    writer = SharedMemoryPoseWriter()
    
    # Create dummy landmarks
    class DummyLandmark:
        def __init__(self, x, y, z=0.0, visibility=1.0):
            self.x = x
            self.y = y
            self.z = z
            self.visibility = visibility
    
    test_participants = [
        {
            "uuid": "test1234",
            "landmarks": [DummyLandmark(0.5, 0.5, 0.0, 1.0) for _ in range(33)],
            "in_zone": True,
            "timestamp": time.time()
        }
    ]
    
    writer.write_poses(test_participants)
    print(f"✓ Wrote {len(test_participants)} pose(s) to shared memory buffer '{SHARED_MEMORY_BUFFER_NAME}'")
    
    writer.close()
