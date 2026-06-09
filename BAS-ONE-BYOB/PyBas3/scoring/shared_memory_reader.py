#!/usr/bin/env python3
"""
Shared memory reader for pose data from Vision module.

Reads from `bas_pose_data` buffer written by SharedMemoryPoseWriter.
Uses common module for protocol definitions.
"""

from multiprocessing import shared_memory
from typing import List, Dict, Optional
import sys
from pathlib import Path

# Add parent directory to path for common module
sys.path.insert(0, str(Path(__file__).parent.parent))

from common.protocols import SHARED_MEMORY_BUFFER_NAME, MAX_PARTICIPANTS, POSE_RECORD_SIZE
from common.shared_memory import decode_pose


class SharedMemoryPoseReader:
    """Reads pose data from shared memory buffer."""
    
    def __init__(
        self,
        buffer_name: str = SHARED_MEMORY_BUFFER_NAME,
        max_participants: int = MAX_PARTICIPANTS
    ):
        self.buffer_name = buffer_name
        self.max_participants = max_participants
        self.record_size = POSE_RECORD_SIZE
        self.shm: Optional[shared_memory.SharedMemory] = None
    
    def connect(self) -> bool:
        """Connect to existing shared memory. Returns True on success."""
        try:
            self.shm = shared_memory.SharedMemory(name=self.buffer_name)
            return True
        except FileNotFoundError:
            return False
    
    def read_poses(self) -> List[Dict]:
        """
        Read all participant poses from shared memory.
        
        Returns list of dicts with keys: uuid, timestamp, keypoints, in_zone
        Each keypoint is tuple (x, y, z, visibility)
        """
        if not self.shm:
            return []
        
        poses = []
        offset = 0
        
        for _ in range(self.max_participants):
            pose = decode_pose(self.shm.buf, offset)
            if pose:
                # Convert ParticipantPose to dict format for compatibility
                poses.append(pose.to_dict())
            offset += self.record_size
        
        return poses
    
    def close(self):
        """Close shared memory connection (does not unlink)."""
        if self.shm:
            self.shm.close()
            self.shm = None
    
    def __enter__(self):
        self.connect()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        self.close()
