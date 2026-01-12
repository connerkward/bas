"""
Binary protocol for shared memory pose data.

Provides encoding/decoding functions for the shared memory buffer format
used for inter-process communication between Vision and Scoring modules.
"""

import struct
from typing import List, Optional
from multiprocessing import shared_memory

from .protocols import (
    SHARED_MEMORY_BUFFER_NAME,
    MAX_PARTICIPANTS,
    POSE_RECORD_SIZE,
    POSE_BUFFER_SIZE,
    MEDIAPIPE_LANDMARKS,
    UUID_BYTES,
    TIMESTAMP_BYTES,
    KEYPOINT_BYTES,
    IN_ZONE_BYTES,
    ParticipantPose,
)


def encode_pose(pose: ParticipantPose, buffer: bytearray, offset: int) -> int:
    """
    Encode a single pose into buffer at the given offset.
    
    Args:
        pose: ParticipantPose to encode
        buffer: Bytearray buffer to write to
        offset: Starting byte offset in buffer
    
    Returns:
        New offset after encoding (offset + POSE_RECORD_SIZE)
    """
    # UUID (36 bytes, null-padded)
    uuid_bytes = pose.uuid.encode('utf-8').ljust(UUID_BYTES)[:UUID_BYTES]
    buffer[offset:offset+UUID_BYTES] = uuid_bytes
    offset += UUID_BYTES
    
    # Timestamp (8 bytes, double)
    struct.pack_into('d', buffer, offset, pose.timestamp)
    offset += 8
    
    # Keypoints (33 * 16 bytes = 528 bytes)
    for kp in pose.keypoints:
        struct.pack_into('ffff', buffer, offset, kp.x, kp.y, kp.z, kp.visibility)
        offset += 16
    
    # In zone flag (1 byte)
    buffer[offset] = 1 if pose.in_zone else 0
    offset += 1
    
    return offset


def decode_pose(buffer: memoryview, offset: int) -> Optional[ParticipantPose]:
    """
    Decode a single pose from buffer at the given offset.
    
    Args:
        buffer: Memoryview of shared memory buffer
        offset: Starting byte offset in buffer
    
    Returns:
        ParticipantPose if slot is occupied, None if empty
    """
    # Read UUID (36 bytes, space-padded)
    uuid_bytes = bytes(buffer[offset:offset+UUID_BYTES])
    uuid = uuid_bytes.decode('utf-8').strip('\x00').strip()
    
    if not uuid:
        # Empty slot
        return None
    
    offset += UUID_BYTES
    
    # Read timestamp (8 bytes, double)
    timestamp = struct.unpack_from('d', buffer, offset)[0]
    offset += 8
    
    # Read keypoints (33 * 16 bytes)
    keypoints = []
    for _ in range(MEDIAPIPE_LANDMARKS):
        x, y, z, vis = struct.unpack_from('ffff', buffer, offset)
        keypoints.append((x, y, z, vis))
        offset += 16
    
    # Read in_zone flag (1 byte)
    in_zone = bool(buffer[offset])
    
    return ParticipantPose.from_tuple_list(uuid, timestamp, keypoints, in_zone)
