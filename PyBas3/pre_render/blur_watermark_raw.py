#!/usr/bin/env python3
"""Blur bottom-right Veo watermark on all non-chroma frames in an output dir.
Targets: raw_frames, raw_dithered, raw_atkinson. Uses 720p ref box (16x16), scaled for 1080p.
"""
import argparse
import os
import sys
import numpy as np
import cv2
from tqdm import tqdm

_VEO_REF_WIDTH, _VEO_REF_HEIGHT = 720, 1280
_VEO_BOX_X1_REF, _VEO_BOX_Y1_REF = 660, 1245
_VEO_BOX_W_REF, _VEO_BOX_H_REF = 43, 17
_VEO_FEATHER_REF = 10


def blur_watermark_region(img: np.ndarray) -> np.ndarray:
    """Same logic as depth_blend_video._blur_watermark_region."""
    h, w = img.shape[:2]
    sw, sh = w / _VEO_REF_WIDTH, h / _VEO_REF_HEIGHT
    bx1 = int(round(_VEO_BOX_X1_REF * sw))
    by1 = int(round(_VEO_BOX_Y1_REF * sh))
    bw = int(round(_VEO_BOX_W_REF * sw))
    bh = int(round(_VEO_BOX_H_REF * sh))
    margin = int(round(_VEO_FEATHER_REF * max(sw, sh)))
    x0, y0 = max(0, bx1 - margin), max(0, by1 - margin)
    x1, y1 = min(w, bx1 + bw + margin), min(h, by1 + bh + margin)
    pw, ph = x1 - x0, y1 - y0
    if pw < 4 or ph < 4:
        return img
    roi = img[y0:y1, x0:x1].astype(np.float32)
    k = max(15, min(51, pw - 1, ph - 1))
    if k % 2 == 0:
        k -= 1
    blurred = cv2.GaussianBlur(roi, (k, k), 0)
    blurred = cv2.GaussianBlur(blurred, (k, k), 0)
    yy, xx = np.meshgrid(np.arange(ph), np.arange(pw), indexing='ij')
    bx_local, by_local = bx1 - x0, by1 - y0
    dx = np.maximum(bx_local - xx, 0) + np.maximum(xx - (bx_local + bw - 1), 0)
    dy = np.maximum(by_local - yy, 0) + np.maximum(yy - (by_local + bh - 1), 0)
    dist = np.sqrt(dx.astype(np.float32) ** 2 + dy.astype(np.float32) ** 2)
    mask = np.clip(1.0 - dist / max(margin, 1), 0.0, 1.0)[:, :, np.newaxis]
    roi_out = roi * (1.0 - mask) + blurred * mask
    img[y0:y1, x0:x1] = np.clip(roi_out, 0, 255).astype(np.uint8)
    return img


def process_folder(folder_path: str) -> int:
    if not os.path.isdir(folder_path):
        return 0
    files = sorted([f for f in os.listdir(folder_path) if f.startswith("frame_") and f.endswith(".png")])
    count = 0
    for f in files:
        path = os.path.join(folder_path, f)
        img = cv2.imread(path)
        if img is None:
            continue
        blur_watermark_region(img)
        cv2.imwrite(path, img)
        count += 1
    return count


def main():
    parser = argparse.ArgumentParser(description="Blur Veo watermark on non-chroma frames")
    parser.add_argument("output_dir", type=str, help="Pre-render output dir (e.g. .../2025-02-05_..._blend_output)")
    args = parser.parse_args()
    base = args.output_dir
    folders = ["raw_frames", "raw_dithered", "raw_atkinson"]
    total = 0
    for name in folders:
        folder = os.path.join(base, name)
        n = process_folder(folder)
        if n:
            print(f"{name}: {n} frames")
            total += n
    if total == 0:
        print("No non-chroma frame folders found or all empty.", file=sys.stderr)
        sys.exit(1)
    print(f"Done. Blurred {total} frames.")


if __name__ == "__main__":
    main()
