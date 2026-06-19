#!/usr/bin/env python3
"""Per-source relief QA: render INPUT photo next to candidate relief hillshades so we can
judge faces/detail/machineability against the source. One montage per source.
Usage: python compare_sources.py <key> [extra candidates baked in code]
"""
import sys, pathlib, numpy as np
from PIL import Image, ImageDraw
from scipy.ndimage import gaussian_filter
ROOT = pathlib.Path(__file__).resolve().parent.parent / "depths"

def load(p):
    return np.asarray(Image.open(p).convert("L"), dtype="float32")/255.0

def n01(a):
    lo, hi = np.percentile(a,1), np.percentile(a,99)
    return np.clip((a-lo)/max(hi-lo,1e-6),0,1)

def fuse(da2, nano, alpha=0.6, sigma_frac=1/50, clamp=0.0):
    W = da2.shape[1]; s = max(1.0, W*sigma_frac)
    if nano.shape != da2.shape:
        from scipy.ndimage import zoom
        nano = zoom(nano,(da2.shape[0]/nano.shape[0], da2.shape[1]/nano.shape[1]),order=1)
    hp = nano - gaussian_filter(nano, s)
    if clamp>0: hp = np.tanh(hp/clamp)*clamp
    return n01(da2 + alpha*hp)

def seg_compose(g, mask, segground=0.7, ground=0.1):
    m = gaussian_filter(mask, max(2, g.shape[1]/120))
    return m*g + (1-m)*((1-segground)*g + segground*ground)

def hillshade(h, z=10, light=(-0.5,0.7,0.55)):
    W=h.shape[1]
    gy,gx = np.gradient(h*z*W/512)
    nx,ny,nz=-gx,-gy,np.ones_like(h); n=np.sqrt(nx*nx+ny*ny+nz*nz)
    lx,ly,lz=light; ln=(lx*lx+ly*ly+lz*lz)**0.5
    sh=(nx*lx+ny*ly+nz*lz)/(n*ln)
    return np.clip(0.25+0.75*np.clip(sh,0,1),0,1)

def main():
    key = sys.argv[1]
    d = ROOT/key
    src = load(d/"source.jpg")
    da2 = load(d/"da2.png")
    nano = load(d/"nanobanana.png")
    mask = load(d/"figmask.png") if (d/"figmask.png").exists() else None
    panels = [("INPUT photo", src)]
    panels.append(("DA2", hillshade(da2)))
    panels.append(("DA2+Nano a0.6", hillshade(fuse(da2,nano,0.6))))
    panels.append(("DA2+Nano a0.9 clamp.1", hillshade(fuse(da2,nano,0.9,clamp=0.1))))
    if mask is not None and (mask>0.5).mean() < 0.8:   # only if it actually separates a figure
        panels.append(("DA2+Nano + seg-ground", hillshade(seg_compose(fuse(da2,nano,0.7), mask))))
    cols=len(panels); cell=440
    sheet=Image.new("RGB",(cols*cell, cell+26),(18,20,26)); dr=ImageDraw.Draw(sheet)
    for i,(lab,im) in enumerate(panels):
        g=Image.fromarray((np.clip(im,0,1)*255).astype('uint8'),'L').convert('RGB'); g.thumbnail((cell,cell))
        dr.rectangle([i*cell,0,i*cell+cell,cell+26],outline=(60,60,66))
        sheet.paste(g,(i*cell+(cell-g.width)//2,(cell-g.height)//2))
        dr.text((i*cell+6,cell+6),lab,fill=(220,220,220))
    out=f"/tmp/cmp_{key}.png"; sheet.save(out); print("wrote",out,sheet.size)

if __name__=="__main__":
    main()
