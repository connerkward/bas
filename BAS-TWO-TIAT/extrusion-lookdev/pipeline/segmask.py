#!/usr/bin/env python3
"""Figure/ground segmentation mask for a relief (local, rembg/u2net).
Depth nets read the figure and the carved ground at the same height in low relief, so they
can't separate them. A real foreground mask lets the studio raise the figure (with detail)
over a clean recessed ground. Writes depths/<key>/figmask.png (white = figure).

Usage: python segmask.py --key selfmade [--model u2net]
"""
import argparse, pathlib, numpy as np
from PIL import Image

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--key", required=True)
    ap.add_argument("--model", default="u2net")
    a = ap.parse_args()
    d = pathlib.Path(__file__).resolve().parent.parent / "depths" / a.key
    from rembg import remove, new_session
    img = Image.open(d/"source.jpg").convert("RGB")
    m = remove(img, session=new_session(a.model), only_mask=True)
    m.save(d/"figmask.png")
    cov = 100*(np.asarray(m)>127).mean()
    print(f"wrote {d/'figmask.png'}  ({a.model}, figure coverage {cov:.0f}%)")

if __name__ == "__main__":
    main()
