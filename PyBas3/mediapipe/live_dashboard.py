#!/usr/bin/env python3
"""
Live Dashboard - Participant streams with scores + zone config sliders.

Usage:
    python live_dashboard.py
    
Runs alongside multi_person_detector.py - reads score JSONs and shows participant feeds.
"""

import cv2
import json
import time
import numpy as np
from pathlib import Path
from typing import Dict, Optional

# Paths
ZONE_CONFIG_PATH = Path(__file__).parent / "zone_config.json"
SCORE_DIR = Path(__file__).parent.parent / "scoring" / "output"
THUMBNAILS_DIR = Path(__file__).parent / "thumbnails"


class ZoneConfig:
    """Live-editable zone configuration."""
    
    def __init__(self):
        self.load()
    
    def load(self):
        """Load config from JSON."""
        if ZONE_CONFIG_PATH.exists():
            with open(ZONE_CONFIG_PATH, 'r') as f:
                self.config = json.load(f)
        else:
            self.config = {
                "screen_region": {"x": 0.0, "y": 0.0, "width": 1.0, "height": 1.0},
                "z_range": {"min": -10.0, "max": 10.0},
                "max_people": 3,
                "min_visibility": 0.1,
                "min_landmarks": 1
            }
    
    def save(self):
        """Save config to JSON (atomic write)."""
        tmp_path = ZONE_CONFIG_PATH.with_suffix('.tmp')
        with open(tmp_path, 'w') as f:
            json.dump(self.config, f, indent=2)
        tmp_path.rename(ZONE_CONFIG_PATH)
    
    # Getters/setters for trackbars (scaled to int 0-100)
    @property
    def zone_x(self): return int(self.config["screen_region"]["x"] * 100)
    @zone_x.setter
    def zone_x(self, v): self.config["screen_region"]["x"] = v / 100.0
    
    @property
    def zone_y(self): return int(self.config["screen_region"]["y"] * 100)
    @zone_y.setter
    def zone_y(self, v): self.config["screen_region"]["y"] = v / 100.0
    
    @property
    def zone_w(self): return int(self.config["screen_region"]["width"] * 100)
    @zone_w.setter
    def zone_w(self, v): self.config["screen_region"]["width"] = max(1, v) / 100.0
    
    @property
    def zone_h(self): return int(self.config["screen_region"]["height"] * 100)
    @zone_h.setter
    def zone_h(self, v): self.config["screen_region"]["height"] = max(1, v) / 100.0
    
    @property
    def min_vis(self): return int(self.config["min_visibility"] * 100)
    @min_vis.setter
    def min_vis(self, v): self.config["min_visibility"] = v / 100.0


class ScoreWatcher:
    """Watch score JSON files."""
    
    def __init__(self):
        self.scores: Dict[str, dict] = {}
        self._mtimes: Dict[str, float] = {}
    
    def poll(self) -> Dict[str, dict]:
        """Poll for score updates."""
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


def create_participant_card(uuid: str, score_data: Optional[dict], thumbnail: Optional[np.ndarray]) -> np.ndarray:
    """Create a display card for a participant."""
    card_w, card_h = 320, 280
    card = np.zeros((card_h, card_w, 3), dtype=np.uint8)
    card[:] = (30, 30, 30)  # Dark gray background
    
    # Thumbnail area (top)
    thumb_h = 180
    if thumbnail is not None:
        thumb_resized = cv2.resize(thumbnail, (card_w - 20, thumb_h - 10))
        card[10:10+thumb_resized.shape[0], 10:10+thumb_resized.shape[1]] = thumb_resized
    else:
        # Placeholder
        cv2.rectangle(card, (10, 10), (card_w-10, thumb_h), (60, 60, 60), -1)
        cv2.putText(card, "No Feed", (card_w//2 - 40, thumb_h//2), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, (100, 100, 100), 2)
    
    # UUID
    cv2.putText(card, f"ID: {uuid[:8]}", (10, thumb_h + 25), 
                cv2.FONT_HERSHEY_SIMPLEX, 0.6, (200, 200, 200), 1)
    
    # Score
    if score_data:
        score = score_data.get("score_0_to_100", 0)
        in_zone = score_data.get("in_zone", False)
        
        # Score bar
        bar_x, bar_y = 10, thumb_h + 40
        bar_w, bar_h = card_w - 20, 25
        cv2.rectangle(card, (bar_x, bar_y), (bar_x + bar_w, bar_y + bar_h), (50, 50, 50), -1)
        
        # Fill based on score
        fill_w = int(bar_w * score / 100)
        color = (0, 255, 0) if in_zone else (0, 100, 255)
        cv2.rectangle(card, (bar_x, bar_y), (bar_x + fill_w, bar_y + bar_h), color, -1)
        
        # Score text
        cv2.putText(card, f"{score:.1f}", (bar_x + bar_w//2 - 20, bar_y + 18), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 255), 2)
        
        # Zone status
        zone_text = "IN ZONE" if in_zone else "OUT"
        zone_color = (0, 255, 0) if in_zone else (0, 0, 255)
        cv2.putText(card, zone_text, (10, card_h - 15), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, zone_color, 1)
    
    return card


def main():
    # Initialize
    zone_cfg = ZoneConfig()
    score_watcher = ScoreWatcher()
    
    # Create windows
    cv2.namedWindow("Participants", cv2.WINDOW_NORMAL)
    cv2.namedWindow("Zone Config", cv2.WINDOW_NORMAL)
    cv2.resizeWindow("Zone Config", 400, 300)
    
    # Trackbar callbacks
    def on_zone_x(v): zone_cfg.zone_x = v; zone_cfg.save()
    def on_zone_y(v): zone_cfg.zone_y = v; zone_cfg.save()
    def on_zone_w(v): zone_cfg.zone_w = v; zone_cfg.save()
    def on_zone_h(v): zone_cfg.zone_h = v; zone_cfg.save()
    def on_min_vis(v): zone_cfg.min_vis = v; zone_cfg.save()
    
    # Create trackbars
    cv2.createTrackbar("Zone X", "Zone Config", zone_cfg.zone_x, 100, on_zone_x)
    cv2.createTrackbar("Zone Y", "Zone Config", zone_cfg.zone_y, 100, on_zone_y)
    cv2.createTrackbar("Zone W", "Zone Config", zone_cfg.zone_w, 100, on_zone_w)
    cv2.createTrackbar("Zone H", "Zone Config", zone_cfg.zone_h, 100, on_zone_h)
    cv2.createTrackbar("Min Vis", "Zone Config", zone_cfg.min_vis, 100, on_min_vis)
    
    print("Dashboard running. Press 'q' to quit, 'r' to reload config.")
    
    try:
        while True:
            # Poll scores
            scores = score_watcher.poll()
            
            # Load thumbnails
            thumbnails = {}
            if THUMBNAILS_DIR.exists():
                for f in THUMBNAILS_DIR.glob("participant_*.jpg"):
                    uuid = f.stem.replace("participant_", "")
                    if not uuid.startswith("temp_"):
                        thumbnails[uuid] = cv2.imread(str(f))
            
            # Get active participants (those with scores)
            active_uuids = list(scores.keys())
            
            # Create participant cards
            cards = []
            for uuid in active_uuids:
                if uuid.startswith("test"):  # Skip test files
                    continue
                card = create_participant_card(
                    uuid, 
                    scores.get(uuid),
                    thumbnails.get(uuid)
                )
                cards.append(card)
            
            # Compose dashboard
            if cards:
                # Arrange cards in a row (up to 3)
                dashboard = np.hstack(cards[:3])
            else:
                # Empty state
                dashboard = np.zeros((280, 640, 3), dtype=np.uint8)
                cv2.putText(dashboard, "Waiting for participants...", (150, 140),
                            cv2.FONT_HERSHEY_SIMPLEX, 0.8, (100, 100, 100), 2)
            
            cv2.imshow("Participants", dashboard)
            
            # Zone config visualization
            config_display = np.zeros((250, 380, 3), dtype=np.uint8)
            config_display[:] = (40, 40, 40)
            
            # Draw zone preview
            preview_x, preview_y = 20, 20
            preview_w, preview_h = 340, 180
            cv2.rectangle(config_display, (preview_x, preview_y), 
                         (preview_x + preview_w, preview_y + preview_h), (80, 80, 80), -1)
            
            # Draw zone rectangle
            zx = preview_x + int(zone_cfg.config["screen_region"]["x"] * preview_w)
            zy = preview_y + int(zone_cfg.config["screen_region"]["y"] * preview_h)
            zw = int(zone_cfg.config["screen_region"]["width"] * preview_w)
            zh = int(zone_cfg.config["screen_region"]["height"] * preview_h)
            cv2.rectangle(config_display, (zx, zy), (zx + zw, zy + zh), (0, 255, 0), 2)
            
            # Labels
            cv2.putText(config_display, "Zone Preview", (preview_x, preview_y + preview_h + 20),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.5, (150, 150, 150), 1)
            
            cv2.imshow("Zone Config", config_display)
            
            # Handle keys
            key = cv2.waitKey(100) & 0xFF
            if key == ord('q'):
                break
            elif key == ord('r'):
                zone_cfg.load()
                cv2.setTrackbarPos("Zone X", "Zone Config", zone_cfg.zone_x)
                cv2.setTrackbarPos("Zone Y", "Zone Config", zone_cfg.zone_y)
                cv2.setTrackbarPos("Zone W", "Zone Config", zone_cfg.zone_w)
                cv2.setTrackbarPos("Zone H", "Zone Config", zone_cfg.zone_h)
                cv2.setTrackbarPos("Min Vis", "Zone Config", zone_cfg.min_vis)
                print("Config reloaded")
    
    except KeyboardInterrupt:
        print("\nStopping dashboard")
    finally:
        cv2.destroyAllWindows()


if __name__ == "__main__":
    main()
