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
        self.streams: Dict[str, dict] = {}  # uuid -> {sender, width, height}
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
        """
        if not self._initialized:
            return False
        
        if uuid in self.streams:
            return True  # Already exists
        
        stream_name = f"BAS_Participant_{uuid}"
        
        # Create dedicated sender for this participant
        send_create = ndi.SendCreate()
        send_create.ndi_name = stream_name
        send_create.clock_video = False
        
        sender = ndi.send_create(send_create)
        if not sender:
            print(f"Failed to create NDI sender for {stream_name}")
            return False
        
        self.streams[uuid] = {
            "name": stream_name,
            "sender": sender,
            "width": width,
            "height": height,
            "fps": fps
        }
        
        print(f"Created NDI stream: {stream_name} ({width}x{height})")
        return True
    
    def send_frame(self, uuid: str, frame_bgra: np.ndarray) -> bool:
        """
        Send a frame to a participant's NDI stream.
        """
        if not self._initialized:
            return False
        
        if uuid not in self.streams:
            return False
        
        stream = self.streams[uuid]
        sender = stream["sender"]
        width = stream["width"]
        height = stream["height"]
        
        # Ensure frame matches stream dimensions
        h, w = frame_bgra.shape[:2]
        if h != height or w != width:
            frame_bgra = cv2.resize(frame_bgra, (width, height))
        
        # Convert to BGRX (ignore alpha, use X=255)
        if len(frame_bgra.shape) == 2:
            frame_bgrx = cv2.cvtColor(frame_bgra, cv2.COLOR_GRAY2BGRA)
        elif frame_bgra.shape[2] == 3:
            frame_bgrx = cv2.cvtColor(frame_bgra, cv2.COLOR_BGR2BGRA)
        else:
            frame_bgrx = frame_bgra.copy()
        
        # Set alpha to 255
        frame_bgrx[:, :, 3] = 255
        
        # Ensure contiguous C-order array
        frame_bgrx = np.ascontiguousarray(frame_bgrx, dtype=np.uint8)
        
        # Create fresh video frame each time
        video_frame = ndi.VideoFrameV2()
        video_frame.xres = width
        video_frame.yres = height
        video_frame.FourCC = ndi.FOURCC_VIDEO_TYPE_BGRX
        video_frame.frame_rate_N = 30000
        video_frame.frame_rate_D = 1000
        video_frame.picture_aspect_ratio = width / height
        video_frame.line_stride_in_bytes = width * 4
        video_frame.data = frame_bgrx
        
        # Send frame
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
