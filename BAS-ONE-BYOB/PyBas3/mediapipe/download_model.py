#!/usr/bin/env python3
"""
Download MediaPipe Pose Landmarker model file.
"""

import os
import urllib.request
from pathlib import Path


MODEL_URL = "https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_full/float16/1/pose_landmarker_full.task"
MODEL_PATH = Path(__file__).parent / "pose_landmarker_full.task"


def download_model(force=False):
    """Download MediaPipe Pose Landmarker model if not present."""
    if MODEL_PATH.exists() and not force:
        print(f"Model already exists at {MODEL_PATH}")
        return str(MODEL_PATH)
    
    print(f"Downloading model from {MODEL_URL}...")
    print(f"Destination: {MODEL_PATH}")
    
    try:
        urllib.request.urlretrieve(MODEL_URL, MODEL_PATH)
        print(f"Model downloaded successfully: {MODEL_PATH}")
        return str(MODEL_PATH)
    except Exception as e:
        print(f"Error downloading model: {e}")
        raise


if __name__ == "__main__":
    download_model()
