#!/usr/bin/env python3
"""
NDI video streamer for participant video outputs.

Creates separate NDI streams named `BAS_Participant_<UUID>` for each detected participant.
"""

import cv2
import numpy as np
from typing import Dict, Optional
from pathlib import Path

try:
    import NDIlib as ndi
    NDI_AVAILABLE = True
except ImportError:
    NDI_AVAILABLE = False
    print("Warning: ndi-python not available. NDI streaming disabled.")


class NDIStreamer:
    """Manages per-participant NDI video streams."""
    
    def __init__(self):
        self.streams: Dict[str, dict] = {}  # uuid -> {sender, video_frame, ...}
        self._initialized = False
        
        if not NDI_AVAILABLE:
            print("NDI streaming disabled (ndi-python not installed)")
            return
        
        # Initialize NDI library
        if not ndi.initialize():
            print("Failed to initialize NDI")
            return
        
        self._initialized = True
        print("NDI streamer initialized")
    
    def create_stream(self, uuid: str, width: int, height: int, fps: float = 30.0) -> bool:
        """
        Create a dedicated NDI stream for a participant.
        
        Args:
            uuid: Participant UUID
            width: Video width
            height: Video height
            fps: Frame rate
        
        Returns:
            True if stream created successfully
        """
        if not self._initialized:
            return False
        
        if uuid in self.streams:
            return True  # Already exists
        
        stream_name = f"BAS_Participant_{uuid}"
        
        # Create dedicated sender for this participant
        send_create = ndi.SendCreate()
        send_create.ndi_name = stream_name
        send_create.clock_video = False  # Don't block on send
        
        sender = ndi.send_create(send_create)
        if not sender:
            print(f"Failed to create NDI sender for {stream_name}")
            return False
        
        # Create video frame descriptor
        video_frame = ndi.VideoFrameV2()
        video_frame.xres = width
        video_frame.yres = height
        video_frame.FourCC = ndi.FOURCC_VIDEO_TYPE_BGRA
        video_frame.frame_rate_N = int(fps * 1000)
        video_frame.frame_rate_D = 1000
        video_frame.picture_aspect_ratio = width / height
        
        # Allocate buffer for BGRA frame
        video_frame.data = np.zeros((height, width, 4), dtype=np.uint8)
        
        self.streams[uuid] = {
            "name": stream_name,
            "sender": sender,
            "video_frame": video_frame,
            "width": width,
            "height": height
        }
        
        print(f"Created NDI stream: {stream_name} ({width}x{height})")
        return True
    
    def send_frame(self, uuid: str, frame_bgra: np.ndarray) -> bool:
        """
        Send a frame to a participant's NDI stream.
        
        Args:
            uuid: Participant UUID
            frame_bgra: Frame in BGRA format (height, width, 4)
        
        Returns:
            True if frame sent successfully
        """
        if not self._initialized:
            return False
        
        if uuid not in self.streams:
            return False
        
        stream = self.streams[uuid]
        sender = stream["sender"]
        video_frame = stream["video_frame"]
        
        # Ensure frame matches stream dimensions
        h, w = frame_bgra.shape[:2]
        if h != stream["height"] or w != stream["width"]:
            frame_bgra = cv2.resize(frame_bgra, (stream["width"], stream["height"]))
        
        # Ensure BGRA format (4 channels)
        if len(frame_bgra.shape) == 2:
            frame_bgra = cv2.cvtColor(frame_bgra, cv2.COLOR_GRAY2BGRA)
        elif frame_bgra.shape[2] == 3:
            frame_bgra = cv2.cvtColor(frame_bgra, cv2.COLOR_BGR2BGRA)
        
        # Copy frame data to NDI buffer
        np.copyto(video_frame.data, frame_bgra)
        
        # Send frame through this participant's sender
        ndi.send_send_video_v2(sender, video_frame)
        return True
    
    def remove_stream(self, uuid: str):
        """Remove and destroy an NDI stream for a participant."""
        if uuid in self.streams:
            stream = self.streams[uuid]
            if stream.get("sender"):
                ndi.send_destroy(stream["sender"])
            del self.streams[uuid]
            print(f"Removed NDI stream: BAS_Participant_{uuid}")
    
    def close(self):
        """Clean up all NDI resources."""
        if not NDI_AVAILABLE:
            return
        
        # Destroy all participant senders
        for uuid in list(self.streams.keys()):
            self.remove_stream(uuid)
        
        # Destroy NDI library
        if self._initialized:
            ndi.destroy()
            self._initialized = False
        
        print("NDI streamer closed")


if __name__ == "__main__":
    # Test: create streams and verify they appear
    import time
    
    streamer = NDIStreamer()
    
    # Create test stream
    streamer.create_stream("test1234", 640, 480)
    
    # Send some frames
    print("Sending test frames for 3 seconds...")
    for i in range(90):
        # Create gradient test pattern
        frame = np.zeros((480, 640, 4), dtype=np.uint8)
        frame[:, :, 0] = i * 2 % 256  # Blue varies
        frame[:, :, 1] = 100  # Green constant
        frame[:, :, 2] = 50   # Red constant
        frame[:, :, 3] = 255  # Alpha
        streamer.send_frame("test1234", frame)
        time.sleep(1/30)
    
    streamer.close()
    print("Test complete")
