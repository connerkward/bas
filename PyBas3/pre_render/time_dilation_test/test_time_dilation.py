#!/usr/bin/env python3
"""
Time Dilation Test App
Compare interpolation methods for slow-motion playback.
Uses OpenCV highgui - no tkinter dependency.

Controls:
  1/2/3  - Switch method (nearest/blend/optical_flow)
  LEFT/RIGHT - Adjust speed (-0.1 / +0.1)
  SPACE  - Play/Pause
  Q/ESC  - Quit
  Mouse  - Click timeline to seek
"""

import cv2
import numpy as np
import time
import argparse
from pathlib import Path


class TimeDilationTest:
    def __init__(self, video_path: str):
        self.video_path = video_path
        self.cap = cv2.VideoCapture(video_path)
        
        if not self.cap.isOpened():
            raise ValueError(f"Cannot open video: {video_path}")
        
        self.fps = self.cap.get(cv2.CAP_PROP_FPS)
        self.total_frames = int(self.cap.get(cv2.CAP_PROP_FRAME_COUNT))
        self.width = int(self.cap.get(cv2.CAP_PROP_FRAME_WIDTH))
        self.height = int(self.cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
        
        # Scale for display
        self.display_scale = min(900 / self.width, 700 / self.height, 1.0)
        self.display_w = int(self.width * self.display_scale)
        self.display_h = int(self.height * self.display_scale)
        
        # UI dimensions (taller to avoid overlap)
        self.ui_height = 160
        self.window_h = self.display_h + self.ui_height
        
        # Preload frames
        print(f"Loading {self.total_frames} frames...")
        self.frames = []
        while True:
            ret, frame = self.cap.read()
            if not ret:
                break
            self.frames.append(frame)
        self.cap.release()
        print(f"Loaded {len(self.frames)} frames @ {self.fps}fps")
        
        # Precompute optical flow
        print("Precomputing optical flow...")
        self.flows = []
        self.flows_back = []
        gray_prev = cv2.cvtColor(self.frames[0], cv2.COLOR_BGR2GRAY)
        for i in range(1, len(self.frames)):
            gray_curr = cv2.cvtColor(self.frames[i], cv2.COLOR_BGR2GRAY)
            flow = cv2.calcOpticalFlowFarneback(
                gray_prev, gray_curr, None,
                pyr_scale=0.5, levels=3, winsize=15,
                iterations=3, poly_n=5, poly_sigma=1.2, flags=0
            )
            flow_back = cv2.calcOpticalFlowFarneback(
                gray_curr, gray_prev, None,
                pyr_scale=0.5, levels=3, winsize=15,
                iterations=3, poly_n=5, poly_sigma=1.2, flags=0
            )
            self.flows.append(flow)
            self.flows_back.append(flow_back)
            if i % 20 == 0:
                print(f"  Flow {i}/{len(self.frames)-1}")
            gray_prev = gray_curr
        print("Optical flow ready\n")
        
        # State
        self.position = 0.0
        self.speed = 0.5
        self.method_idx = 2  # optical_flow
        self.methods = ["nearest", "blend", "optical_flow"]
        self.playing = True
        self.last_time = time.time()
        
        # Button positions for click detection
        self.button_rects = []
        
        # Colors
        self.bg_color = (26, 26, 26)
        self.accent = (53, 107, 255)  # Orange in BGR
        self.text_color = (224, 224, 224)
        self.dim_color = (100, 100, 100)
        
        # Window
        self.window_name = "Time Dilation Test"
        cv2.namedWindow(self.window_name, cv2.WINDOW_NORMAL)
        cv2.resizeWindow(self.window_name, self.display_w, self.window_h)
        cv2.setMouseCallback(self.window_name, self.on_mouse)
        
        print("Controls:")
        print("  1/2/3      - Method: nearest/blend/optical_flow")
        print("  LEFT/RIGHT - Speed ±0.1")
        print("  SPACE      - Play/Pause")
        print("  Q/ESC      - Quit")
        print("  Mouse      - Click timeline to seek\n")
        
    def on_mouse(self, event, x, y, flags, param):
        """Handle mouse clicks on buttons and timeline."""
        if event == cv2.EVENT_LBUTTONDOWN:
            # Check button clicks
            for i, (btn_x, btn_y, btn_w, btn_h) in enumerate(self.button_rects):
                if btn_x <= x <= btn_x + btn_w and btn_y <= y <= btn_y + btn_h:
                    self.method_idx = i
                    return
            
            # Check timeline click
            timeline_y = self.display_h + 95
            timeline_x1 = 15
            timeline_x2 = self.display_w - 15
            if abs(y - timeline_y) < 12 and timeline_x1 <= x <= timeline_x2:
                timeline_w = timeline_x2 - timeline_x1
                t = (x - timeline_x1) / timeline_w
                self.position = t * (len(self.frames) - 1)
                self.position = max(0, min(self.position, len(self.frames) - 1))
    
    def warp_frame(self, frame: np.ndarray, flow: np.ndarray, t: float) -> np.ndarray:
        """Warp frame using optical flow."""
        h, w = flow.shape[:2]
        flow_scaled = flow * t
        x, y = np.meshgrid(np.arange(w), np.arange(h))
        map_x = (x + flow_scaled[:, :, 0]).astype(np.float32)
        map_y = (y + flow_scaled[:, :, 1]).astype(np.float32)
        return cv2.remap(frame, map_x, map_y, cv2.INTER_LINEAR, borderMode=cv2.BORDER_REPLICATE)
    
    def interpolate_optical_flow(self, idx1: int, idx2: int, t: float) -> np.ndarray:
        """Bidirectional optical flow interpolation."""
        if idx1 >= len(self.frames) - 1:
            return self.frames[-1]
        if idx2 >= len(self.frames):
            idx2 = len(self.frames) - 1
        if idx1 >= len(self.flows):
            return self.frames[idx1]
            
        frame1 = self.frames[idx1]
        frame2 = self.frames[idx2]
        
        warped1 = self.warp_frame(frame1, self.flows[idx1], t)
        warped2 = self.warp_frame(frame2, self.flows_back[idx1], 1.0 - t)
        
        return cv2.addWeighted(warped1, 1.0 - t, warped2, t, 0)
    
    def get_frame(self, position: float) -> np.ndarray:
        """Get interpolated frame at position."""
        idx = int(position)
        t = position - idx
        
        idx = max(0, min(idx, len(self.frames) - 1))
        next_idx = min(idx + 1, len(self.frames) - 1)
        method = self.methods[self.method_idx]
        
        if method == "nearest":
            return self.frames[idx].copy()
        elif method == "blend":
            if idx == next_idx or t < 0.01:
                return self.frames[idx].copy()
            return cv2.addWeighted(self.frames[idx], 1.0 - t, self.frames[next_idx], t, 0)
        elif method == "optical_flow":
            if idx == next_idx or t < 0.01:
                return self.frames[idx].copy()
            return self.interpolate_optical_flow(idx, next_idx, t)
        
        return self.frames[idx].copy()
    
    def draw_ui(self, canvas: np.ndarray):
        """Draw UI controls on canvas."""
        h = self.display_h
        w = canvas.shape[1]
        
        # Background
        canvas[h:, :] = self.bg_color
        
        # Row 1: Method buttons (left side) - spaced
        method_names = ["1: Nearest", "2: Blend", "3: Optical Flow"]
        btn_w = 140
        btn_h = 32
        btn_spacing = 24
        row1_y = h + 14
        
        # Clear button rects
        self.button_rects = []
        
        for i, name in enumerate(method_names):
            btn_x = 20 + i * (btn_w + btn_spacing)
            color = self.accent if i == self.method_idx else (50, 50, 50)
            
            # Store rect for click detection
            self.button_rects.append((btn_x, row1_y, btn_w, btn_h))
            
            # Draw button
            cv2.rectangle(canvas, (btn_x, row1_y), (btn_x + btn_w, row1_y + btn_h), color, -1)
            cv2.rectangle(canvas, (btn_x, row1_y), (btn_x + btn_w, row1_y + btn_h), (80, 80, 80), 2)
            
            # Draw text
            text_size = cv2.getTextSize(name, cv2.FONT_HERSHEY_SIMPLEX, 0.42, 1)[0]
            text_x = btn_x + (btn_w - text_size[0]) // 2
            text_y = row1_y + (btn_h + text_size[1]) // 2
            cv2.putText(canvas, name, (text_x, text_y), cv2.FONT_HERSHEY_SIMPLEX, 0.42, self.text_color, 1)
        
        # Row 2: Speed display (below buttons, right aligned)
        speed_text = f"Speed: {self.speed:.2f}x"
        speed_x = w - 180
        speed_y = row1_y + btn_h + 22
        cv2.putText(canvas, speed_text, (speed_x, speed_y), cv2.FONT_HERSHEY_SIMPLEX, 0.55, self.accent, 1)
        cv2.putText(canvas, "[<- / ->]", (speed_x, speed_y + 18), cv2.FONT_HERSHEY_SIMPLEX, 0.32, self.dim_color, 1)
        
        # Row 3: Timeline
        timeline_y = h + 95
        timeline_x1 = 15
        timeline_x2 = w - 15
        timeline_w = timeline_x2 - timeline_x1
        
        # Timeline background
        cv2.rectangle(canvas, (timeline_x1, timeline_y - 3), (timeline_x2, timeline_y + 3), (50, 50, 50), -1)
        
        # Progress bar
        progress = self.position / max(1, len(self.frames) - 1)
        progress_x = int(timeline_x1 + progress * timeline_w)
        cv2.rectangle(canvas, (timeline_x1, timeline_y - 3), (progress_x, timeline_y + 3), self.accent, -1)
        
        # Playhead
        cv2.circle(canvas, (progress_x, timeline_y), 7, self.accent, -1)
        cv2.circle(canvas, (progress_x, timeline_y), 7, self.text_color, 2)
        
        # Row 4: Frame info (left) and status (right)
        row4_y = h + 135
        effective_fps = self.fps * self.speed
        info = f"Frame {int(self.position)}/{len(self.frames)} | {self.fps:.0f}fps -> {effective_fps:.1f}fps"
        cv2.putText(canvas, info, (timeline_x1, row4_y), cv2.FONT_HERSHEY_SIMPLEX, 0.36, self.dim_color, 1)
        
        status = "PLAYING" if self.playing else "PAUSED [SPACE]"
        status_color = (100, 200, 100) if self.playing else (100, 100, 200)
        status_size = cv2.getTextSize(status, cv2.FONT_HERSHEY_SIMPLEX, 0.36, 1)[0]
        cv2.putText(canvas, status, (w - status_size[0] - 15, row4_y), cv2.FONT_HERSHEY_SIMPLEX, 0.36, status_color, 1)
    
    def run(self):
        """Main loop."""
        while True:
            current_time = time.time()
            dt = current_time - self.last_time
            self.last_time = current_time
            
            if self.playing:
                frames_to_advance = dt * self.fps * self.speed
                self.position += frames_to_advance
                if self.position >= len(self.frames):
                    self.position = 0.0
            
            # Get interpolated frame
            frame = self.get_frame(self.position)
            
            # Resize for display
            frame_resized = cv2.resize(frame, (self.display_w, self.display_h))
            
            # Create canvas - use display_w consistently
            canvas = np.zeros((self.display_h + self.ui_height, self.display_w, 3), dtype=np.uint8)
            canvas[:self.display_h, :] = frame_resized
            
            # Draw UI
            self.draw_ui(canvas)
            
            cv2.imshow(self.window_name, canvas)
            
            # Handle keys
            key = cv2.waitKey(1) & 0xFF
            
            if key == ord('q') or key == 27:  # Q or ESC
                break
            elif key == ord(' '):
                self.playing = not self.playing
            elif key == ord('1'):
                self.method_idx = 0
            elif key == ord('2'):
                self.method_idx = 1
            elif key == ord('3'):
                self.method_idx = 2
            elif key == 81 or key == 2:  # LEFT arrow
                self.speed = max(0.1, self.speed - 0.1)
            elif key == 83 or key == 3:  # RIGHT arrow
                self.speed = min(2.0, self.speed + 0.1)
        
        cv2.destroyAllWindows()


def main():
    parser = argparse.ArgumentParser(description="Time dilation interpolation test")
    parser.add_argument(
        "video", type=str, nargs='?',
        default=str(Path(__file__).parent.parent.parent.parent / "input_videos" / "runside.mp4"),
        help="Path to video file"
    )
    args = parser.parse_args()
    
    app = TimeDilationTest(args.video)
    app.run()


if __name__ == "__main__":
    main()
