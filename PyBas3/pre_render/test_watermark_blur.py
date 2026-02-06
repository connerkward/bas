#!/usr/bin/env python3
"""Test watermark blur: read first frame from source video, apply blur+feather, save.
Usage: uv run python pre_render/test_watermark_blur.py [video_path] [out_path]
Defaults: input_videos/runside-megaslow-compressed.mp4 -> pre_render/outputs/watermark_blur_feather_test.png
"""
import os
import sys

import cv2

# Assume run from PyBas3/ (uv run python pre_render/test_watermark_blur.py)
PRE_RENDER = os.path.dirname(os.path.abspath(__file__))
REPO_ROOT = os.path.dirname(PRE_RENDER)  # PyBas3
if os.path.basename(REPO_ROOT) == "PyBas3":
    REPO_ROOT = os.path.dirname(REPO_ROOT)

sys.path.insert(0, PRE_RENDER)
from depth_blend_video import _blur_watermark_region


def main():
    video = sys.argv[1] if len(sys.argv) > 1 else os.path.join(REPO_ROOT, "input_videos", "runside-megaslow-compressed.mp4")
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(PRE_RENDER, "outputs", "watermark_blur_feather_test.png")
    if not os.path.isfile(video):
        print(f"Video not found: {video}", file=sys.stderr)
        sys.exit(1)
    cap = cv2.VideoCapture(video)
    ok, img = cap.read()
    cap.release()
    if not ok or img is None:
        print(f"Could not read frame from {video}", file=sys.stderr)
        sys.exit(1)
    _blur_watermark_region(img)
    os.makedirs(os.path.dirname(out) or ".", exist_ok=True)
    cv2.imwrite(out, img)
    print(f"Wrote {out} (frame 0 from video)")


if __name__ == "__main__":
    main()
