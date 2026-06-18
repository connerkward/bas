#!/usr/bin/env python3
"""Tune the DA2 (global) + Nano-Banana (detail) fusion for relief quality.
Renders HILLSHADED relief previews (raking light shows micro-relief) of parameter
variants into a labeled montage for visual judging. Knobs:
  alpha    detail strength
  sigma    detail high-pass cutoff (px) — smaller = only the finest detail
  clamp    soft tanh limit on detail amplitude (kills Nano's hard spikes)
  dgamma   detail gamma (<1 boosts faint skin/muscle detail, >1 suppresses)
  bgsupp   background detail suppression (0 keep all, 1 none where base is low/background)
  beta     base-global mix: leak some Nano low-freq into the DA2 base
  unsharp  final unsharp-mask amount
  histm    histogram-match Nano to DA2 before fusing (bool)
"""
import sys, json, pathlib, numpy as np
from PIL import Image, ImageDraw
from scipy.ndimage import gaussian_filter

D = pathlib.Path(__file__).resolve().parent.parent / "depths" / "selfmade"
def load(name):
    return np.asarray(Image.open(D/name).convert("L"), dtype="float32")/255.0
DA2 = load("da2.png"); NANO = load("nanobanana.png")
# resize nano to da2 grid
if NANO.shape != DA2.shape:
    from scipy.ndimage import zoom
    NANO = zoom(NANO, (DA2.shape[0]/NANO.shape[0], DA2.shape[1]/NANO.shape[1]), order=1)
H, W = DA2.shape
SIG = lambda f: max(1.0, W*f)

def n01(a):
    lo, hi = np.percentile(a,1), np.percentile(a,99)
    return np.clip((a-lo)/max(hi-lo,1e-6),0,1)

def histmatch(src, ref):
    s=src.ravel(); r=ref.ravel()
    so=np.argsort(s); ro=np.sort(r)
    out=np.empty_like(s); out[so]=ro[(np.linspace(0,len(r)-1,len(s))).astype(int)]
    return out.reshape(src.shape)

def fuse(alpha=0.6, sigma=1/50, clamp=0.0, dgamma=1.0, bgsupp=0.0, beta=0.0, unsharp=0.0, histm=False, mlo=0.45, mhi=0.72):
    base = DA2.copy()
    det  = histmatch(NANO, DA2) if histm else NANO
    s = SIG(sigma)
    hp = det - gaussian_filter(det, s)              # nano fine detail
    if dgamma != 1.0:
        hp = np.sign(hp)*np.power(np.abs(hp)*2, dgamma)/2
    if clamp > 0:
        hp = np.tanh(hp/clamp)*clamp                # soft-knee limit
    if bgsupp > 0:
        # sharp figure mask: ~0 on the mid-gray panel/background, ~1 on the raised figure
        mb = gaussian_filter(base, SIG(1/40))
        lo, hi = mlo, mhi
        mask = np.clip((mb-lo)/max(hi-lo,1e-6), 0, 1); mask = mask*mask*(3-2*mask)  # smoothstep
        hp = hp * (1 - bgsupp*(1-mask))
    mixedBase = (1-beta)*base + beta*gaussian_filter(det, s)
    fused = mixedBase + alpha*hp
    if unsharp > 0:
        fused = fused + unsharp*(fused - gaussian_filter(fused, SIG(1/80)))
    return n01(fused)

def hillshade(h, z=14, light=(-0.5,0.7,0.55)):
    gy,gx = np.gradient(h*z*W/512)
    nx,ny,nz = -gx,-gy,np.ones_like(h)
    n = np.sqrt(nx*nx+ny*ny+nz*nz)
    lx,ly,lz = light; ln=(lx*lx+ly*ly+lz*lz)**0.5
    sh = (nx*lx+ny*ly+nz*lz)/(n*ln)
    amb = 0.25
    return np.clip(amb + (1-amb)*np.clip(sh,0,1), 0, 1)

def montage(variants, path, cols=4, cell=300):
    rows=(len(variants)+cols-1)//cols
    sheet=Image.new("RGB",(cols*cell,rows*(cell+26)),(18,20,26)); d=ImageDraw.Draw(sheet)
    for i,(label,h) in enumerate(variants):
        sh=(hillshade(h)*255).astype("uint8")
        im=Image.fromarray(sh,"L").convert("RGB"); im.thumbnail((cell,cell))
        cx=(i%cols)*cell; cy=(i//cols)*(cell+26)
        sheet.paste(im,(cx+(cell-im.width)//2,cy+(cell-im.height)//2))
        d.text((cx+4,cy+cell+6), label, fill=(210,210,210))
    sheet.save(path); print("wrote",path,sheet.size)

if __name__ == "__main__":
    mode = sys.argv[1] if len(sys.argv)>1 else "base"
    out = "/tmp/tune_%s.png" % mode
    V=[]
    if mode=="base":
        V.append(("DA2 only", DA2.copy()))
        V.append(("Nano only", NANO.copy()))
        V.append(("a0.6 s1/50 (current)", fuse(0.6,1/50)))
        for a in (0.4,0.8,1.2):
            V.append((f"alpha {a}", fuse(a,1/50)))
        for s in (1/120,1/70,1/30):
            V.append((f"sigma {s:.3f}", fuse(0.8,s)))
    elif mode=="clamp":
        for c in (0.0,0.04,0.08,0.15):
            V.append((f"clamp {c}", fuse(0.9,1/60,clamp=c)))
        for g in (0.6,0.8,1.0,1.3):
            V.append((f"dgamma {g}", fuse(0.9,1/60,dgamma=g,clamp=0.08)))
    elif mode=="bg":
        for b in (0.0,0.4,0.7,1.0):
            V.append((f"bgsupp {b}", fuse(0.9,1/60,clamp=0.08,bgsupp=b)))
        for u in (0.0,0.4,0.8,1.2):
            V.append((f"unsharp {u}", fuse(0.9,1/60,clamp=0.08,bgsupp=0.6,unsharp=u)))
    elif mode=="final":
        V.append(("DA2 only", DA2.copy()))
        V.append(("current a0.6", fuse(0.6,1/50)))
        V.append(("CANDIDATE", fuse(**json.loads(sys.argv[2]))))
        V.append(("Nano only", NANO.copy()))
    montage(V, out, cols=4)
