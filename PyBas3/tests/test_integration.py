#!/usr/bin/env python3
"""
Integration tests for PyBas3 pipeline.

Tests:
1. Vision → Scoring shared memory communication
2. Scoring → TD JSON output
3. TD helper scripts
"""

import json
import time
import tempfile
from pathlib import Path
from multiprocessing import shared_memory

# Add parent to path for imports
import sys
sys.path.insert(0, str(Path(__file__).parent.parent))

from common.protocols import (
    SHARED_MEMORY_BUFFER_NAME,
    POSE_BUFFER_SIZE,
    POSE_RECORD_SIZE,
    MAX_PARTICIPANTS,
    ParticipantPose,
    PoseKeypoint,
)
from common.shared_memory import encode_pose, decode_pose


def make_mock_pose(uuid: str, in_zone: bool = True) -> ParticipantPose:
    """Create a mock pose for testing."""
    keypoints = [PoseKeypoint(0.5, 0.5, 0.0, 1.0) for _ in range(33)]
    # Set some distinct values for shoulders/hips
    keypoints[11] = PoseKeypoint(0.4, 0.4, 0.0, 1.0)  # left shoulder
    keypoints[12] = PoseKeypoint(0.6, 0.4, 0.0, 1.0)  # right shoulder
    return ParticipantPose(
        uuid=uuid,
        timestamp=time.time(),
        keypoints=keypoints,
        in_zone=in_zone
    )


def test_shared_memory_roundtrip():
    """Test writing and reading from shared memory."""
    print("=" * 60)
    print("TEST: Shared Memory Round-Trip")
    print("=" * 60)
    
    shm = None
    try:
        # Create shared memory
        try:
            shm = shared_memory.SharedMemory(name=SHARED_MEMORY_BUFFER_NAME)
            shm.close()
            shm.unlink()
        except FileNotFoundError:
            pass
        
        shm = shared_memory.SharedMemory(
            name=SHARED_MEMORY_BUFFER_NAME,
            create=True,
            size=POSE_BUFFER_SIZE
        )
        print(f"✓ Created shared memory: {SHARED_MEMORY_BUFFER_NAME}")
        print(f"  Size: {POSE_BUFFER_SIZE} bytes ({MAX_PARTICIPANTS} participants)")
        
        # Write mock poses
        poses = [
            make_mock_pose("abc12345", in_zone=True),
            make_mock_pose("def67890", in_zone=False),
        ]
        
        buffer = bytearray(POSE_BUFFER_SIZE)
        offset = 0
        for pose in poses:
            offset = encode_pose(pose, buffer, offset)
        shm.buf[:POSE_BUFFER_SIZE] = memoryview(buffer)
        print(f"✓ Wrote {len(poses)} poses to shared memory")
        
        # Read back
        read_poses = []
        offset = 0
        for _ in range(MAX_PARTICIPANTS):
            pose = decode_pose(shm.buf, offset)
            if pose:
                read_poses.append(pose)
            offset += POSE_RECORD_SIZE
        
        print(f"✓ Read {len(read_poses)} poses from shared memory")
        
        # Verify
        assert len(read_poses) == len(poses), f"Expected {len(poses)}, got {len(read_poses)}"
        for i, (orig, read) in enumerate(zip(poses, read_poses)):
            if orig.uuid != read.uuid:
                print(f"  DEBUG: orig.uuid={repr(orig.uuid)}, read.uuid={repr(read.uuid)}")
            assert orig.uuid == read.uuid, f"UUID mismatch at {i}: {orig.uuid} != {read.uuid}"
            assert orig.in_zone == read.in_zone, f"in_zone mismatch at {i}"
            assert len(read.keypoints) == 33, f"Keypoints count mismatch at {i}"
        
        print("✓ Data integrity verified")
        print("PASS: Shared memory round-trip")
        return True
        
    except Exception as e:
        print(f"FAIL: {e}")
        return False
    finally:
        if shm:
            shm.close()
            shm.unlink()


def test_scoring_reads_shared_memory():
    """Test that scoring module can read from shared memory."""
    print("\n" + "=" * 60)
    print("TEST: Scoring Reads Shared Memory")
    print("=" * 60)
    
    shm = None
    try:
        from scoring.shared_memory_reader import SharedMemoryPoseReader
        
        # Clean up any existing shared memory
        try:
            existing = shared_memory.SharedMemory(name=SHARED_MEMORY_BUFFER_NAME)
            existing.close()
            existing.unlink()
        except FileNotFoundError:
            pass
        
        # Create and populate shared memory (simulating Vision module)
        shm = shared_memory.SharedMemory(
            name=SHARED_MEMORY_BUFFER_NAME,
            create=True,
            size=POSE_BUFFER_SIZE
        )
        
        mock_pose = make_mock_pose("test1234", in_zone=True)
        buffer = bytearray(POSE_BUFFER_SIZE)
        encode_pose(mock_pose, buffer, 0)
        shm.buf[:POSE_BUFFER_SIZE] = memoryview(buffer)
        print("✓ Vision mock: wrote pose to shared memory")
        
        # Read using Scoring's reader
        reader = SharedMemoryPoseReader()
        connected = reader.connect()
        assert connected, "Failed to connect to shared memory"
        poses = reader.read_poses()
        print(f"✓ Scoring reader: read {len(poses)} poses")
        
        assert len(poses) == 1, f"Expected 1 pose, got {len(poses)}"
        assert poses[0]['uuid'] == "test1234", f"UUID mismatch"
        assert poses[0]['in_zone'] == True, f"in_zone mismatch"
        
        print("✓ Data integrity verified")
        print("PASS: Scoring reads shared memory")
        reader.close()
        return True
        
    except Exception as e:
        print(f"FAIL: {e}")
        import traceback
        traceback.print_exc()
        return False
    finally:
        if shm:
            shm.close()
            shm.unlink()


def test_td_score_watcher():
    """Test that TD can watch score JSON files."""
    print("\n" + "=" * 60)
    print("TEST: TD Score Watcher")
    print("=" * 60)
    
    try:
        from td_scripts.score_watcher import ScoreWatcher
        
        with tempfile.TemporaryDirectory() as tmpdir:
            watcher = ScoreWatcher(tmpdir)
            
            # Initially empty
            scores = watcher.poll()
            assert len(scores) == 0, "Expected empty scores initially"
            print("✓ Empty directory: no scores")
            
            # Write a score file
            score_file = Path(tmpdir) / "participant_abc12345_score.json"
            score_data = {
                "uuid": "abc12345",
                "timestamp": time.time(),
                "reference_frame": 10,
                "score_0_to_100": 85.5,
                "in_zone": True
            }
            with open(score_file, 'w') as f:
                json.dump(score_data, f)
            print("✓ Wrote score file")
            
            # Poll again
            scores = watcher.poll()
            assert len(scores) == 1, f"Expected 1 score, got {len(scores)}"
            assert "abc12345" in scores, "UUID not found"
            assert scores["abc12345"]["score_0_to_100"] == 85.5
            print("✓ Score file detected and parsed")
            
            # Update score
            score_data["score_0_to_100"] = 90.0
            time.sleep(0.1)  # Ensure mtime changes
            with open(score_file, 'w') as f:
                json.dump(score_data, f)
            
            scores = watcher.poll()
            assert scores["abc12345"]["score_0_to_100"] == 90.0
            print("✓ Score update detected")
            
            print("PASS: TD score watcher")
            return True
            
    except Exception as e:
        print(f"FAIL: {e}")
        import traceback
        traceback.print_exc()
        return False


def test_td_ndi_uuid_parsing():
    """Test NDI stream name UUID parsing."""
    print("\n" + "=" * 60)
    print("TEST: TD NDI UUID Parsing")
    print("=" * 60)
    
    try:
        from td_scripts.ndi_discovery import parse_uuid_from_stream
        
        # Test cases
        tests = [
            ("BAS_Participant_abc12345", "abc12345"),
            ("HOSTNAME (BAS_Participant_def67890)", "def67890"),
            ("Some Other Stream", None),
            ("BAS_Participant_", None),  # Empty UUID
        ]
        
        for stream_name, expected in tests:
            result = parse_uuid_from_stream(stream_name)
            assert result == expected, f"For '{stream_name}': expected {expected}, got {result}"
            print(f"✓ '{stream_name}' → {result}")
        
        print("PASS: NDI UUID parsing")
        return True
        
    except Exception as e:
        print(f"FAIL: {e}")
        return False


def main():
    """Run all integration tests."""
    print("\n" + "=" * 60)
    print("PyBas3 Integration Tests")
    print("=" * 60 + "\n")
    
    results = []
    
    results.append(("Shared Memory Round-Trip", test_shared_memory_roundtrip()))
    results.append(("Scoring Reads Shared Memory", test_scoring_reads_shared_memory()))
    results.append(("TD Score Watcher", test_td_score_watcher()))
    results.append(("TD NDI UUID Parsing", test_td_ndi_uuid_parsing()))
    
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)
    
    passed = sum(1 for _, r in results if r)
    total = len(results)
    
    for name, result in results:
        status = "✓ PASS" if result else "✗ FAIL"
        print(f"  {status}: {name}")
    
    print(f"\n{passed}/{total} tests passed")
    
    return passed == total


if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)
