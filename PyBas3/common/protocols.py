"""
Shared protocols and constants for inter-process communication.

Defines data structures and constants used by both Vision (mediapipe) and
Scoring modules for shared memory communication.
"""

from dataclasses import dataclass
from typing import List, Tuple


# ============================================================================
# Constants
# ============================================================================

# Shared memory configuration
SHARED_MEMORY_BUFFER_NAME = 'bas_pose_data'
MAX_PARTICIPANTS = 3
MEDIAPIPE_LANDMARKS = 33  # MediaPipe Pose has 33 landmarks

# Binary format constants (bytes)
UUID_BYTES = 36
TIMESTAMP_BYTES = 8  # double (float64)
KEYPOINT_BYTES = 16  # 4 floats * 4 bytes each (x, y, z, visibility)
IN_ZONE_BYTES = 1

# Calculate record size (single source of truth)
POSE_RECORD_SIZE = UUID_BYTES + TIMESTAMP_BYTES + (MEDIAPIPE_LANDMARKS * KEYPOINT_BYTES) + IN_ZONE_BYTES
POSE_BUFFER_SIZE = POSE_RECORD_SIZE * MAX_PARTICIPANTS


# ============================================================================
# Data Structures
# ============================================================================

@dataclass
class PoseKeypoint:
    """Single pose keypoint with normalized coordinates."""
    x: float  # Normalized x (0-1)
    y: float  # Normalized y (0-1)
    z: float  # Depth (relative)
    visibility: float  # Visibility confidence (0-1)


@dataclass
class ParticipantPose:
    """
    Pose data structure shared between Vision and Scoring modules.
    
    This is the canonical representation of a participant's pose,
    used for both shared memory serialization and in-memory processing.
    """
    uuid: str
    timestamp: float  # Unix timestamp
    keypoints: List[PoseKeypoint]  # 33 MediaPipe landmarks
    in_zone: bool
    
    def to_tuple_list(self) -> List[Tuple[float, float, float, float]]:
        """Convert keypoints to list of tuples for binary encoding."""
        return [(kp.x, kp.y, kp.z, kp.visibility) for kp in self.keypoints]
    
    @classmethod
    def from_tuple_list(
        cls,
        uuid: str,
        timestamp: float,
        keypoints: List[Tuple[float, float, float, float]],
        in_zone: bool
    ):
        """Create from tuple list (from binary decoding)."""
        kp_list = [PoseKeypoint(x, y, z, vis) for x, y, z, vis in keypoints]
        return cls(uuid=uuid, timestamp=timestamp, keypoints=kp_list, in_zone=in_zone)
    
    def to_dict(self) -> dict:
        """Convert to dict format (for JSON/compatibility)."""
        return {
            'uuid': self.uuid,
            'timestamp': self.timestamp,
            'keypoints': self.to_tuple_list(),
            'in_zone': self.in_zone
        }
    
    @classmethod
    def from_dict(cls, data: dict):
        """Create from dict format."""
        keypoints = [PoseKeypoint(x, y, z, vis) for x, y, z, vis in data['keypoints']]
        return cls(
            uuid=data['uuid'],
            timestamp=data['timestamp'],
            keypoints=keypoints,
            in_zone=data['in_zone']
        )
