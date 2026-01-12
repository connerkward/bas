#!/usr/bin/env python3
"""
NDI video streamer for participant video outputs.

Creates NDI streams named `BAS_Participant_<UUID>` for each detected participant.
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
    """Manages NDI video streams for participants."""
    
    def __init__(self):
        self.streams: Dict[str, dict] = {}  # uuid -> {sender, video_frame}
        self.ndi_find = None
        self.ndi_send = None
        
        if not NDI_AVAILABLE:
            print("NDI streaming disabled (ndi-python not installed)")
            return
        
        # Initialize NDI
        if not ndi.initialize():
            print("Failed to initialize NDI")
            return
        
        # Create NDI finder
        find_create = ndi.FindCreate()
        find_create.show_local_sources = True
        self.ndi_find = ndi.find_create_v2(find_create)
        
        # Create NDI sender
        send_create = ndi.SendCreate()
        send_create.ndi_name = "PyBas3 Vision"
        send_create.clock_video = True
        self.ndi_send = ndi.send_create(send_create)
        
        if not self.ndi_send:
            print("Failed to create NDI sender")
            return
        
        print("NDI streamer initialized")
    
    def create_stream(self, uuid: str, width: int, height: int, fps: float = 30.0):
        """
        Create an NDI stream for a participant.
        
        Args:
            uuid: Participant UUID
            width: Video width
            height: Video height
            fps: Frame rate
        """
        if not NDI_AVAILABLE or not self.ndi_send:
            return False
        
        stream_name = f"BAS_Participant_{uuid}"
        
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
            "video_frame": video_frame,
            "width": width,
            "height": height
        }
        
        print(f"Created NDI stream: {stream_name} ({width}x{height})")
        return True
    
    def send_frame(self, uuid: str, frame_bgra: np.ndarray):
        """
        Send a frame to an NDI stream.
        
        Args:
            uuid: Participant UUID
            frame_bgra: Frame in BGRA format (height, width, 4)
        """
        if not NDI_AVAILABLE or not self.ndi_send:
            return False
        
        if uuid not in self.streams:
            return False
        
        stream = self.streams[uuid]
        video_frame = stream["video_frame"]
        
        # Ensure frame matches stream dimensions
        h, w = frame_bgra.shape[:2]
        if h != stream["height"] or w != stream["width"]:
            frame_bgra = cv2.resize(frame_bgra, (stream["width"], stream["height"]))
        
        # Copy frame data to NDI buffer
        video_frame.data[:] = frame_bgra
        
        # Send frame
        ndi.send_send_video_v2(self.ndi_send, video_frame)
        return True
    
    def remove_stream(self, uuid: str):
        """Remove an NDI stream for a participant."""
        if uuid in self.streams:
            del self.streams[uuid]
            print(f"Removed NDI stream for participant {uuid[:8]}")
    
    def close(self):
        """Clean up NDI resources."""
        if not NDI_AVAILABLE:
            return
        
        if self.ndi_send:
            ndi.send_destroy(self.ndi_send)
        
        if self.ndi_find:
            ndi.find_destroy(self.ndi_find)
        
        ndi.destroy()
        print("NDI streamer closed")
