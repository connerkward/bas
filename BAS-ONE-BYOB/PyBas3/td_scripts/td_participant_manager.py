"""
TouchDesigner Participant Manager

Unified interface for managing participant streams and scores in TD.
Combines NDI discovery + score watching into one poll call.

Usage in TD Execute DAT:
    from td_participant_manager import ParticipantManager
    
    mgr = ParticipantManager()
    
    def onFrameStart(frame):
        participants = mgr.poll()
        for uuid, data in participants.items():
            # data['ndi_source'] = NDI stream name (or None)
            # data['score'] = score dict (or None)
            pass
"""

from typing import Dict, Optional, NamedTuple
from dataclasses import dataclass

try:
    from ndi_discovery import discover_participants, get_ndi_sources
except ImportError:
    from td_scripts.ndi_discovery import discover_participants, get_ndi_sources

try:
    from score_watcher import ScoreWatcher, DEFAULT_SCORE_DIR
except ImportError:
    from td_scripts.score_watcher import ScoreWatcher, DEFAULT_SCORE_DIR


@dataclass
class ParticipantData:
    """Data for a single participant."""
    uuid: str
    ndi_source: Optional[str] = None
    score: Optional[dict] = None
    
    @property
    def in_zone(self) -> bool:
        """Is participant currently in scoring zone?"""
        if self.score:
            return self.score.get('in_zone', False)
        return False
    
    @property
    def score_value(self) -> float:
        """Current score (0-100), or 0 if no score."""
        if self.score:
            return self.score.get('score_0_to_100', 0.0)
        return 0.0


class ParticipantManager:
    """
    Manages participant discovery from NDI and score files.
    
    Call poll() each frame to get current state.
    """
    
    def __init__(self, score_dir: Optional[str] = None):
        self.score_watcher = ScoreWatcher(score_dir)
        self._participants: Dict[str, ParticipantData] = {}
    
    def poll(self) -> Dict[str, ParticipantData]:
        """
        Poll for updates from NDI and score files.
        
        Returns dict of uuid -> ParticipantData
        """
        # Get NDI streams
        ndi_participants = discover_participants()
        
        # Get scores
        scores = self.score_watcher.poll()
        
        # Merge: a participant exists if they have NDI OR score file
        all_uuids = set(ndi_participants.keys()) | set(scores.keys())
        
        # Update participant data
        self._participants = {}
        for uuid in all_uuids:
            self._participants[uuid] = ParticipantData(
                uuid=uuid,
                ndi_source=ndi_participants.get(uuid),
                score=scores.get(uuid)
            )
        
        return self._participants
    
    def get_participant(self, uuid: str) -> Optional[ParticipantData]:
        """Get specific participant data."""
        return self._participants.get(uuid)
    
    @property
    def active_count(self) -> int:
        """Number of active participants."""
        return len(self._participants)
    
    @property
    def uuids(self) -> list:
        """List of active UUIDs."""
        return list(self._participants.keys())


# Singleton for TD
_manager: Optional[ParticipantManager] = None


def get_manager(score_dir: Optional[str] = None) -> ParticipantManager:
    """Get or create singleton manager."""
    global _manager
    if _manager is None:
        _manager = ParticipantManager(score_dir)
    return _manager


def poll() -> Dict[str, ParticipantData]:
    """Convenience - poll all participants."""
    return get_manager().poll()


if __name__ == "__main__":
    import time
    
    print("Participant Manager Test")
    print(f"Score dir: {DEFAULT_SCORE_DIR}")
    print("-" * 40)
    
    mgr = ParticipantManager()
    
    print("Polling (Ctrl+C to stop)...")
    try:
        while True:
            participants = mgr.poll()
            if participants:
                print(f"\n[{time.strftime('%H:%M:%S')}] {len(participants)} participant(s):")
                for uuid, data in participants.items():
                    ndi = "✓" if data.ndi_source else "✗"
                    score = f"{data.score_value:.1f}" if data.score else "N/A"
                    zone = "zone" if data.in_zone else "out"
                    print(f"  {uuid}: NDI:{ndi} Score:{score} ({zone})")
            else:
                print(".", end="", flush=True)
            time.sleep(1)
    except KeyboardInterrupt:
        print("\nStopped.")
