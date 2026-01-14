"""
TouchDesigner Integration Script

Unified interface for connecting all PyBas3 data to TouchDesigner:
- NDI video streams (from Vision module)
- Score JSON files (from Scoring module)  
- Pose data from shared memory (optional, from Vision module)

Usage in TD Execute DAT:
    import td_integration
    
    # Initialize (call once)
    td_integration.init()
    
    # In onFrameStart callback
    def onFrameStart(frame):
        data = td_integration.get_all_data()
        # data['participants'] = {uuid: ParticipantData}
        # data['poses'] = [pose_dict, ...]  # Optional
"""

import sys
from pathlib import Path
from typing import Dict, List, Optional

# Add parent directory to path for imports (do this early)
_td_scripts_dir = Path(__file__).parent
if str(_td_scripts_dir.parent) not in sys.path:
    sys.path.insert(0, str(_td_scripts_dir.parent))

# Import TD helper modules
try:
    from td_scripts.td_participant_manager import ParticipantManager, ParticipantData
except ImportError:
    try:
        from td_participant_manager import ParticipantManager, ParticipantData
    except ImportError:
        ParticipantManager = None
        ParticipantData = None

# Optional: shared memory reader for pose data
try:
    from scoring.shared_memory_reader import SharedMemoryPoseReader
except ImportError:
    SharedMemoryPoseReader = None


# Global state
_manager: Optional[ParticipantManager] = None
_pose_reader: Optional[SharedMemoryPoseReader] = None
_read_poses: bool = False


def init(score_dir: Optional[str] = None, enable_pose_data: bool = False):
    """
    Initialize TouchDesigner integration.
    
    Args:
        score_dir: Path to scoring output directory (default: auto-detect)
        enable_pose_data: If True, also read pose data from shared memory
    """
    global _manager, _pose_reader, _read_poses
    
    if ParticipantManager is None:
        print("ERROR: td_participant_manager not available")
        return False
    
    # Initialize participant manager (NDI + scores)
    try:
        _manager = ParticipantManager(score_dir)
        print("TD Integration: Participant manager initialized")
    except Exception as e:
        print(f"ERROR: Failed to initialize participant manager: {e}")
        return False
    
    # Optional: initialize pose reader
    _read_poses = enable_pose_data
    if enable_pose_data:
        if SharedMemoryPoseReader is None:
            print("WARNING: SharedMemoryPoseReader not available, pose data disabled")
            _read_poses = False
        else:
            try:
                _pose_reader = SharedMemoryPoseReader()
                if _pose_reader.connect():
                    print("TD Integration: Pose reader connected")
                else:
                    print("WARNING: Shared memory not available (Vision module not running?)")
                    _read_poses = False
            except Exception as e:
                print(f"WARNING: Failed to connect pose reader: {e}")
                _read_poses = False
    
    return True


def get_participants() -> Dict[str, ParticipantData]:
    """
    Get current participant data (NDI streams + scores).
    
    Returns dict of uuid -> ParticipantData
    """
    if _manager is None:
        return {}
    
    try:
        return _manager.poll()
    except Exception as e:
        print(f"ERROR polling participants: {e}")
        return {}


def get_poses() -> List[Dict]:
    """
    Get current pose data from shared memory.
    
    Returns list of pose dicts with keys: uuid, timestamp, keypoints, in_zone
    """
    if not _read_poses or _pose_reader is None:
        return []
    
    try:
        return _pose_reader.read_poses()
    except Exception as e:
        print(f"ERROR reading poses: {e}")
        return []


def get_all_data() -> Dict:
    """
    Get all available data (participants + poses).
    
    Returns dict with keys:
        'participants': {uuid: ParticipantData}
        'poses': [pose_dict, ...]
    """
    return {
        'participants': get_participants(),
        'poses': get_poses()
    }


def get_participant(uuid: str) -> Optional[ParticipantData]:
    """Get specific participant data by UUID."""
    if _manager is None:
        return None
    return _manager.get_participant(uuid)


def get_pose(uuid: str) -> Optional[Dict]:
    """Get pose data for specific participant UUID."""
    poses = get_poses()
    for pose in poses:
        if pose.get('uuid') == uuid:
            return pose
    return None


def cleanup():
    """Clean up resources (call on shutdown)."""
    global _pose_reader
    
    if _pose_reader:
        _pose_reader.close()
        _pose_reader = None


# TD Execute DAT callback example
def onFrameStart(frame):
    """
    Example callback for TD Execute DAT onFrameStart.
    
    Returns dict with all participant and pose data.
    """
    return get_all_data()


# Convenience functions for TD use
def get_ndi_streams() -> Dict[str, str]:
    """Get dict of uuid -> NDI stream name."""
    participants = get_participants()
    return {
        uuid: data.ndi_source 
        for uuid, data in participants.items() 
        if data.ndi_source
    }


def get_scores() -> Dict[str, float]:
    """Get dict of uuid -> score (0-100)."""
    participants = get_participants()
    return {
        uuid: data.score_value
        for uuid, data in participants.items()
        if data.score
    }


def get_active_uuids() -> List[str]:
    """Get list of active participant UUIDs."""
    participants = get_participants()
    return list(participants.keys())


if __name__ == "__main__":
    # Test script
    import time
    
    print("TouchDesigner Integration Test")
    print("=" * 50)
    
    # Initialize
    if not init(enable_pose_data=True):
        print("Failed to initialize")
        sys.exit(1)
    
    print("\nPolling for data (Ctrl+C to stop)...\n")
    
    try:
        while True:
            data = get_all_data()
            participants = data['participants']
            poses = data['poses']
            
            if participants or poses:
                print(f"\n[{time.strftime('%H:%M:%S')}]")
                
                if participants:
                    print(f"Participants: {len(participants)}")
                    for uuid, pdata in participants.items():
                        ndi = "✓" if pdata.ndi_source else "✗"
                        score = f"{pdata.score_value:.1f}" if pdata.score else "N/A"
                        zone = "zone" if pdata.in_zone else "out"
                        print(f"  {uuid}: NDI:{ndi} Score:{score} ({zone})")
                
                if poses:
                    print(f"Poses: {len(poses)}")
                    for pose in poses:
                        uuid = pose.get('uuid', 'unknown')
                        in_zone = pose.get('in_zone', False)
                        keypoints = len(pose.get('keypoints', []))
                        print(f"  {uuid}: {keypoints} keypoints, in_zone={in_zone}")
            else:
                print(".", end="", flush=True)
            
            time.sleep(1)
    
    except KeyboardInterrupt:
        print("\n\nStopped.")
    finally:
        cleanup()
