#!/usr/bin/env python3
"""
Live Score Dashboard - Shows per-participant thumbnails and scores.

Usage:
    python live_dashboard.py
"""

import cv2
import json
import numpy as np
from pathlib import Path
from typing import Dict, Optional

SCORE_DIR = Path(__file__).parent.parent / "scoring" / "output"
THUMBNAIL_DIR = Path(__file__).parent / "thumbnails"


class ScoreWatcher:
    """Watch score JSON files."""
    
    def __init__(self):
        self.scores: Dict[str, dict] = {}
        self._mtimes: Dict[str, float] = {}
    
    def poll(self) -> Dict[str, dict]:
        if not SCORE_DIR.exists():
            return self.scores
        
        for f in SCORE_DIR.glob("participant_*_score.json"):
            try:
                mtime = f.stat().st_mtime
                uuid = f.stem.replace("participant_", "").replace("_score", "")
                
                if uuid not in self._mtimes or mtime > self._mtimes[uuid]:
                    with open(f, 'r') as fh:
                        self.scores[uuid] = json.load(fh)
                    self._mtimes[uuid] = mtime
            except:
                continue
        
        return self.scores


class ThumbnailCache:
    """Cache participant thumbnails."""
    
    def __init__(self):
        self.thumbnails: Dict[str, np.ndarray] = {}
        self._mtimes: Dict[str, float] = {}
    
    def get(self, uuid: str) -> Optional[np.ndarray]:
        """Get thumbnail for participant, reload if updated."""
        thumb_path = THUMBNAIL_DIR / f"participant_{uuid}.jpg"
        
        if not thumb_path.exists():
            return None
        
        try:
            mtime = thumb_path.stat().st_mtime
            if uuid not in self._mtimes or mtime > self._mtimes[uuid]:
                img = cv2.imread(str(thumb_path))
                if img is not None:
                    self.thumbnails[uuid] = img
                    self._mtimes[uuid] = mtime
            return self.thumbnails.get(uuid)
        except:
            return None


def draw_score_card(uuid: str, score_data: dict, thumbnail: Optional[np.ndarray], card_size=(320, 160)) -> np.ndarray:
    """Draw a score card with thumbnail and score."""
    card_w, card_h = card_size
    card = np.zeros((card_h, card_w, 3), dtype=np.uint8)
    card[:] = (25, 25, 25)
    
    score = score_data.get("score_0_to_100", 0)
    in_zone = score_data.get("in_zone", False)
    
    # Thumbnail on left side
    thumb_size = card_h - 20
    thumb_x, thumb_y = 10, 10
    
    if thumbnail is not None:
        # Resize thumbnail to fit
        h, w = thumbnail.shape[:2]
        scale = thumb_size / max(h, w)
        new_w, new_h = int(w * scale), int(h * scale)
        resized = cv2.resize(thumbnail, (new_w, new_h))
        
        # Center in thumbnail area
        y_off = thumb_y + (thumb_size - new_h) // 2
        x_off = thumb_x + (thumb_size - new_w) // 2
        card[y_off:y_off+new_h, x_off:x_off+new_w] = resized
    else:
        # Placeholder
        cv2.rectangle(card, (thumb_x, thumb_y), (thumb_x + thumb_size, thumb_y + thumb_size), (50, 50, 50), -1)
        cv2.putText(card, "?", (thumb_x + thumb_size//2 - 15, thumb_y + thumb_size//2 + 20),
                    cv2.FONT_HERSHEY_SIMPLEX, 2.0, (80, 80, 80), 3)
    
    # Right side content
    right_x = thumb_x + thumb_size + 20
    
    # UUID
    cv2.putText(card, f"ID: {uuid[:8]}", (right_x, 35),
                cv2.FONT_HERSHEY_SIMPLEX, 0.6, (200, 200, 200), 2)
    
    # Zone status
    zone_text = "IN ZONE" if in_zone else "OUT"
    zone_color = (0, 200, 100) if in_zone else (100, 100, 200)
    cv2.putText(card, zone_text, (right_x, 60),
                cv2.FONT_HERSHEY_SIMPLEX, 0.5, zone_color, 2)
    
    # Large score
    score_text = f"{score:.0f}"
    cv2.putText(card, score_text, (right_x, 115),
                cv2.FONT_HERSHEY_SIMPLEX, 2.0, (255, 255, 255), 3)
    
    # Score bar
    bar_x = right_x
    bar_w = card_w - bar_x - 15
    bar_y = card_h - 20
    cv2.rectangle(card, (bar_x, bar_y), (bar_x + bar_w, card_h - 8), (50, 50, 50), -1)
    fill_w = int(bar_w * score / 100)
    bar_color = (0, 200, 100) if in_zone else (100, 150, 200)
    cv2.rectangle(card, (bar_x, bar_y), (bar_x + fill_w, card_h - 8), bar_color, -1)
    
    return card


def main():
    score_watcher = ScoreWatcher()
    thumb_cache = ThumbnailCache()
    
    cv2.namedWindow("Participants", cv2.WINDOW_NORMAL)
    print("Score Dashboard running. Press 'q' to quit.")
    
    try:
        while True:
            scores = score_watcher.poll()
            
            # Build score cards
            cards = []
            for uuid, score_data in scores.items():
                if uuid.startswith("test"):
                    continue
                thumbnail = thumb_cache.get(uuid)
                card = draw_score_card(uuid, score_data, thumbnail)
                cards.append(card)
            
            # Compose dashboard
            if cards:
                dashboard = np.vstack(cards)
            else:
                dashboard = np.zeros((160, 320, 3), dtype=np.uint8)
                dashboard[:] = (25, 25, 25)
                cv2.putText(dashboard, "Waiting for participants...", (30, 85),
                            cv2.FONT_HERSHEY_SIMPLEX, 0.6, (100, 100, 100), 2)
            
            cv2.imshow("Participants", dashboard)
            
            key = cv2.waitKey(100) & 0xFF
            if key == ord('q'):
                break
    
    except KeyboardInterrupt:
        pass
    finally:
        cv2.destroyAllWindows()


if __name__ == "__main__":
    main()
