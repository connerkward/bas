#!/usr/bin/env python3
"""
Shared memory reader for pose data from Vision module.

Reads from `bas_pose_data` buffer written by SharedMemoryPoseWriter.
"""

import struct
from multiprocessing import shared_memory
from typing import List, Dict, Optional


class SharedMemoryPoseReader:
    """Reads pose data from shared memory buffer."""
    
    def __init__(self, buffer_name: str = 'bas_pose_data', max_participants: int = 3):
        self.buffer_name = buffer_name
        self.max_participants = max_participants
        # Per record: uuid(36) + timestamp(8) + keypoints(33*4*4) + in_zone(1) = 573 bytes
        self.record_size = 36 + 8 + (33 * 4 * 4) + 1
        self.buffer_size = self.record_size * max_participants
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
            # Read UUID (36 bytes, null-padded)
            uuid_bytes = bytes(self.shm.buf[offset:offset + 36])
            uuid = uuid_bytes.decode('utf-8').strip('\x00')
            
            if not uuid:
                # Empty slot, skip
                offset += self.record_size
                continue
            
            offset += 36
            
            # Read timestamp (8 bytes, double)
            timestamp = struct.unpack_from('d', self.shm.buf, offset)[0]
            offset += 8
            
            # Read 33 keypoints (4 floats each: x, y, z, visibility)
            keypoints = []
            for _ in range(33):
                x, y, z, vis = struct.unpack_from('ffff', self.shm.buf, offset)
                keypoints.append((x, y, z, vis))
                offset += 16
            
            # Read in_zone flag (1 byte)
            in_zone = bool(self.shm.buf[offset])
            offset += 1
            
            poses.append({
                'uuid': uuid,
                'timestamp': timestamp,
                'keypoints': keypoints,
                'in_zone': in_zone
            })
        
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
