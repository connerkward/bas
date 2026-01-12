#!/usr/bin/env python3
"""
Live Dashboard - Shows participant NDI streams with scores overlaid.

Usage:
    python live_dashboard.py
"""

import cv2
import json
import time
import numpy as np
from pathlib import Path
from typing import Dict, Optional

try:
    import NDIlib as ndi
    NDI_AVAILABLE = True
except ImportError:
    NDI_AVAILABLE = False

# Paths
ZONE_CONFIG_PATH = Path(__file__).parent / "zone_config.json"
SCORE_DIR = Path(__file__).parent.parent / "scoring" / "output"


class ZoneConfig:
    """Live-editable zone configuration."""
    
    def __init__(self):
        self.load()
    
    def load(self):
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
        tmp_path = ZONE_CONFIG_PATH.with_suffix('.tmp')
        with open(tmp_path, 'w') as f:
            json.dump(self.config, f, indent=2)
        tmp_path.rename(ZONE_CONFIG_PATH)
    
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
    
    @property
    def z_min(self): return int((self.config["z_range"]["min"] + 10) * 5)  # -10 to 10 -> 0 to 100
    @z_min.setter
    def z_min(self, v): self.config["z_range"]["min"] = (v / 5.0) - 10.0
    
    @property
    def z_max(self): return int((self.config["z_range"]["max"] + 10) * 5)
    @z_max.setter
    def z_max(self, v): self.config["z_range"]["max"] = (v / 5.0) - 10.0


class NDIReceiver:
    """Receives frames from BAS_Participant NDI streams."""
    
    def __init__(self):
        self.receivers: Dict[str, any] = {}  # uuid -> receiver
        self.finder = None
        self._initialized = False
        
        if not NDI_AVAILABLE:
            return
        
        if not ndi.initialize():
            return
        
        # Create finder
        find_create = ndi.FindCreate()
        find_create.show_local_sources = True
        self.finder = ndi.find_create_v2(find_create)
        self._initialized = True
    
    def discover_sources(self) -> Dict[str, str]:
        """Find BAS_Participant streams. Returns {uuid: source_name}"""
        if not self._initialized or not self.finder:
            return {}
        
        ndi.find_wait_for_sources(self.finder, 100)  # 100ms timeout
        sources = ndi.find_get_current_sources(self.finder)
        
        participants = {}
        for source in (sources or []):
            name = source.ndi_name
            if "BAS_Participant_" in name:
                # Extract UUID from "HOSTNAME (BAS_Participant_UUID)"
                if "(" in name:
                    inner = name.split("(")[-1].rstrip(")")
                else:
                    inner = name
                if inner.startswith("BAS_Participant_"):
                    uuid = inner.replace("BAS_Participant_", "")
                    participants[uuid] = source
        
        return participants
    
    def connect(self, uuid: str, source) -> bool:
        """Connect to a participant's NDI stream."""
        if not self._initialized:
            return False
        
        if uuid in self.receivers:
            return True
        
        recv_create = ndi.RecvCreateV3()
        recv_create.color_format = ndi.RECV_COLOR_FORMAT_BGRX_BGRA
        recv_create.bandwidth = ndi.RECV_BANDWIDTH_LOWEST
        recv_create.allow_video_fields = False
        
        receiver = ndi.recv_create_v3(recv_create)
        if not receiver:
            return False
        
        ndi.recv_connect(receiver, source)
        self.receivers[uuid] = receiver
        return True
    
    def get_frame(self, uuid: str) -> Optional[np.ndarray]:
        """Get latest frame from a participant stream."""
        if uuid not in self.receivers:
            return None
        
        receiver = self.receivers[uuid]
        
        # Try to capture a frame (non-blocking)
        frame_type, video, audio, metadata = ndi.recv_capture_v2(receiver, 0)
        
        if frame_type == ndi.FRAME_TYPE_VIDEO and video is not None:
            # Convert to numpy array
            frame = np.copy(video.data)
            ndi.recv_free_video_v2(receiver, video)
            return frame
        
        return None
    
    def close(self):
        if not NDI_AVAILABLE:
            return
        
        for receiver in self.receivers.values():
            ndi.recv_destroy(receiver)
        self.receivers.clear()
        
        if self.finder:
            ndi.find_destroy(self.finder)
        
        if self._initialized:
            ndi.destroy()


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


def draw_participant_card(frame: Optional[np.ndarray], uuid: str, score_data: Optional[dict], card_size=(320, 240)) -> np.ndarray:
    """Draw participant card with video and score overlay."""
    card_w, card_h = card_size
    
    if frame is not None:
        # Resize frame to card size
        card = cv2.resize(frame, (card_w, card_h))
        # Convert BGRA to BGR if needed
        if card.shape[2] == 4:
            card = cv2.cvtColor(card, cv2.COLOR_BGRA2BGR)
    else:
        # No frame - dark placeholder
        card = np.zeros((card_h, card_w, 3), dtype=np.uint8)
        card[:] = (30, 30, 30)
        cv2.putText(card, "Connecting...", (card_w//2 - 60, card_h//2), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, (100, 100, 100), 2)
    
    # Overlay UUID
    cv2.rectangle(card, (0, 0), (card_w, 35), (0, 0, 0), -1)
    cv2.putText(card, f"ID: {uuid[:8]}", (10, 25), 
                cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)
    
    # Overlay score bar at bottom
    if score_data:
        score = score_data.get("score_0_to_100", 0)
        in_zone = score_data.get("in_zone", False)
        
        bar_h = 40
        cv2.rectangle(card, (0, card_h - bar_h), (card_w, card_h), (0, 0, 0), -1)
        
        # Score bar fill
        fill_w = int(card_w * score / 100)
        color = (0, 200, 0) if in_zone else (0, 100, 200)
        cv2.rectangle(card, (0, card_h - bar_h + 5), (fill_w, card_h - 5), color, -1)
        
        # Score text
        cv2.putText(card, f"{score:.1f}", (card_w//2 - 25, card_h - 12), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.8, (255, 255, 255), 2)
        
        # Zone indicator
        zone_text = "IN ZONE" if in_zone else "OUT"
        zone_color = (0, 255, 0) if in_zone else (0, 0, 255)
        cv2.putText(card, zone_text, (10, card_h - 12), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, zone_color, 2)
    
    return card


def main():
    zone_cfg = ZoneConfig()
    score_watcher = ScoreWatcher()
    ndi_receiver = NDIReceiver()
    
    # Track last frames for each participant (fallback when no new frame)
    last_frames: Dict[str, np.ndarray] = {}
    
    # Create windows
    cv2.namedWindow("Participants", cv2.WINDOW_NORMAL)
    cv2.namedWindow("Zone Config", cv2.WINDOW_NORMAL)
    cv2.resizeWindow("Zone Config", 350, 250)
    
    # Trackbar callbacks
    def on_zone_x(v): zone_cfg.zone_x = v; zone_cfg.save()
    def on_zone_y(v): zone_cfg.zone_y = v; zone_cfg.save()
    def on_zone_w(v): zone_cfg.zone_w = v; zone_cfg.save()
    def on_zone_h(v): zone_cfg.zone_h = v; zone_cfg.save()
    def on_min_vis(v): zone_cfg.min_vis = v; zone_cfg.save()
    def on_z_min(v): zone_cfg.z_min = v; zone_cfg.save()
    def on_z_max(v): zone_cfg.z_max = v; zone_cfg.save()
    
    # Create trackbars
    cv2.createTrackbar("Zone X %", "Zone Config", zone_cfg.zone_x, 100, on_zone_x)
    cv2.createTrackbar("Zone Y %", "Zone Config", zone_cfg.zone_y, 100, on_zone_y)
    cv2.createTrackbar("Zone W %", "Zone Config", zone_cfg.zone_w, 100, on_zone_w)
    cv2.createTrackbar("Zone H %", "Zone Config", zone_cfg.zone_h, 100, on_zone_h)
    cv2.createTrackbar("Min Vis %", "Zone Config", zone_cfg.min_vis, 100, on_min_vis)
    cv2.createTrackbar("Z Min", "Zone Config", zone_cfg.z_min, 100, on_z_min)
    cv2.createTrackbar("Z Max", "Zone Config", zone_cfg.z_max, 100, on_z_max)
    
    print("Dashboard running. Press 'q' to quit.")
    
    try:
        while True:
            # Poll scores
            scores = score_watcher.poll()
            
            # Discover NDI sources
            sources = ndi_receiver.discover_sources()
            
            # Connect to any new sources
            for uuid, source in sources.items():
                if uuid not in ndi_receiver.receivers:
                    ndi_receiver.connect(uuid, source)
            
            # Get frames and build cards
            cards = []
            active_uuids = set(sources.keys()) | set(scores.keys())
            
            for uuid in active_uuids:
                if uuid.startswith("test"):
                    continue
                
                # Try to get live frame
                frame = ndi_receiver.get_frame(uuid)
                if frame is not None:
                    last_frames[uuid] = frame
                else:
                    frame = last_frames.get(uuid)
                
                card = draw_participant_card(frame, uuid, scores.get(uuid))
                cards.append(card)
            
            # Compose dashboard
            if cards:
                # Arrange in grid (up to 3 per row)
                rows = []
                for i in range(0, len(cards), 3):
                    row_cards = cards[i:i+3]
                    # Pad row to 3 cards if needed
                    while len(row_cards) < 3:
                        row_cards.append(np.zeros_like(cards[0]))
                    rows.append(np.hstack(row_cards))
                dashboard = np.vstack(rows) if len(rows) > 1 else rows[0]
            else:
                dashboard = np.zeros((240, 640, 3), dtype=np.uint8)
                cv2.putText(dashboard, "Waiting for participants...", (150, 120),
                            cv2.FONT_HERSHEY_SIMPLEX, 0.8, (100, 100, 100), 2)
            
            cv2.imshow("Participants", dashboard)
            
            # Simple config display (just shows current values)
            config_display = np.zeros((200, 330, 3), dtype=np.uint8)
            config_display[:] = (40, 40, 40)
            
            y = 30
            cv2.putText(config_display, "Zone Config (sliders above)", (10, y), 
                       cv2.FONT_HERSHEY_SIMPLEX, 0.5, (150, 150, 150), 1)
            y += 35
            cv2.putText(config_display, f"X: {zone_cfg.config['screen_region']['x']:.2f}", (10, y),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.5, (200, 200, 200), 1)
            cv2.putText(config_display, f"Y: {zone_cfg.config['screen_region']['y']:.2f}", (120, y),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.5, (200, 200, 200), 1)
            y += 25
            cv2.putText(config_display, f"W: {zone_cfg.config['screen_region']['width']:.2f}", (10, y),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.5, (200, 200, 200), 1)
            cv2.putText(config_display, f"H: {zone_cfg.config['screen_region']['height']:.2f}", (120, y),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.5, (200, 200, 200), 1)
            y += 35
            cv2.putText(config_display, f"Z range: [{zone_cfg.config['z_range']['min']:.1f}, {zone_cfg.config['z_range']['max']:.1f}]", (10, y),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.5, (200, 200, 200), 1)
            y += 25
            cv2.putText(config_display, f"Min visibility: {zone_cfg.config['min_visibility']:.2f}", (10, y),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.5, (200, 200, 200), 1)
            
            cv2.imshow("Zone Config", config_display)
            
            key = cv2.waitKey(33) & 0xFF  # ~30fps
            if key == ord('q'):
                break
    
    except KeyboardInterrupt:
        pass
    finally:
        ndi_receiver.close()
        cv2.destroyAllWindows()


if __name__ == "__main__":
    main()
