"""
Score JSON File Watcher for TouchDesigner

Usage in TD:
- Create ScoreWatcher instance with output directory
- Call poll() each frame to get updated scores
- Or use watch() for a blocking watcher (not for TD main thread)

Score file format: scoring/output/participant_<uuid>_score.json
"""

import json
import os
import re
from pathlib import Path
from typing import Dict, Optional

# Pattern for score files
SCORE_FILE_PATTERN = re.compile(r'^participant_([a-f0-9]{8})_score\.json$')

# Default path relative to PyBas3
DEFAULT_SCORE_DIR = Path(__file__).parent.parent / "scoring" / "output"


class ScoreWatcher:
    """
    Watches score JSON files for updates.
    
    Example:
        watcher = ScoreWatcher()
        scores = watcher.poll()  # Returns {uuid: score_dict}
    """
    
    def __init__(self, score_dir: Optional[str] = None):
        self.score_dir = Path(score_dir) if score_dir else DEFAULT_SCORE_DIR
        self._last_mtimes: Dict[str, float] = {}
        self._cached_scores: Dict[str, dict] = {}
    
    def _get_score_files(self) -> Dict[str, Path]:
        """
        Find all score files and extract UUIDs.
        Returns {uuid: file_path}
        """
        files = {}
        if not self.score_dir.exists():
            return files
        
        for f in self.score_dir.iterdir():
            match = SCORE_FILE_PATTERN.match(f.name)
            if match:
                files[match.group(1)] = f
        
        return files
    
    def poll(self) -> Dict[str, dict]:
        """
        Check for score updates. Returns all current scores.
        Only re-reads files that have changed.
        
        Returns {uuid: score_dict}
        """
        score_files = self._get_score_files()
        current_uuids = set(score_files.keys())
        cached_uuids = set(self._cached_scores.keys())
        
        # Remove scores for participants that left
        for uuid in cached_uuids - current_uuids:
            del self._cached_scores[uuid]
            self._last_mtimes.pop(uuid, None)
        
        # Check each file
        for uuid, filepath in score_files.items():
            try:
                mtime = filepath.stat().st_mtime
                
                # Only read if new or modified
                if uuid not in self._last_mtimes or mtime > self._last_mtimes[uuid]:
                    with open(filepath, 'r') as f:
                        self._cached_scores[uuid] = json.load(f)
                    self._last_mtimes[uuid] = mtime
                    
            except (json.JSONDecodeError, OSError) as e:
                # File might be mid-write, skip this poll
                continue
        
        return self._cached_scores.copy()
    
    def get_score(self, uuid: str) -> Optional[dict]:
        """Get score for specific participant."""
        return self._cached_scores.get(uuid)
    
    def get_active_uuids(self) -> list:
        """Get list of UUIDs with score files."""
        return list(self._get_score_files().keys())


# Singleton for TD use
_watcher: Optional[ScoreWatcher] = None


def get_watcher(score_dir: Optional[str] = None) -> ScoreWatcher:
    """Get or create singleton watcher."""
    global _watcher
    if _watcher is None:
        _watcher = ScoreWatcher(score_dir)
    return _watcher


def poll_scores() -> Dict[str, dict]:
    """Convenience function - poll all scores."""
    return get_watcher().poll()


def get_score(uuid: str) -> Optional[dict]:
    """Convenience function - get single score."""
    return get_watcher().get_score(uuid)


# For TD Execute DAT
def onFrameStart(frame):
    """
    Example TD callback - poll scores each frame.
    """
    return poll_scores()


if __name__ == "__main__":
    import time
    
    print(f"Watching: {DEFAULT_SCORE_DIR}")
    watcher = ScoreWatcher()
    
    print("Polling scores (Ctrl+C to stop)...")
    try:
        while True:
            scores = watcher.poll()
            if scores:
                print(f"\n[{time.strftime('%H:%M:%S')}] Active participants:")
                for uuid, score in scores.items():
                    print(f"  {uuid}: {score.get('score_0_to_100', 'N/A'):.1f}")
            time.sleep(0.5)
    except KeyboardInterrupt:
        print("\nStopped.")
