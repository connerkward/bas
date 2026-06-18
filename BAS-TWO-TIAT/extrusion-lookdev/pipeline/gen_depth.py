#!/usr/bin/env python3
"""Generate depth/height maps for ONE relief image across several engines, for the
extrusion-lookdev comparison studio.

Convention (matches the studio): output PNG is grayscale, white = nearest = tallest
relief, black = deepest background. Each engine writes depths/<engine>.png and an
entry in depths/manifest.json (kind: "depth-net" = true monocular depth net,
"generative" = a large image model's *guess* at a height map — looks plausible, not
metric).

Local-first: da2 / da3 run on-device (MPS). Hosted engines (fal, openai) run only
when explicitly requested, per the prefer-local-inference rule.

Usage:
  python gen_depth.py --src source.jpg da2 da3                 # local only
  python gen_depth.py --src source.jpg marigold midas nanobanana chatgpt
  python gen_depth.py --src source.jpg all
"""
import sys, os, json, time, base64, io, argparse, pathlib, urllib.request

HERE = pathlib.Path(__file__).resolve().parent
DEPTHS = HERE.parent / "depths"        # root; one subdir per source
DEPTHS.mkdir(exist_ok=True)
OUT = DEPTHS                           # current source's output dir (set per --key in main)
MANIFEST = OUT / "manifest.json"

# ---- keys from the gitignored central/.env (never hardcode, never log values) ----
def load_env():
    env = {}
    p = pathlib.Path.home() / "dev/central/.env"
    if p.exists():
        for line in p.read_text().splitlines():
            line = line.strip()
            if line and not line.startswith("#") and "=" in line:
                k, v = line.split("=", 1)
                env[k.strip()] = v.strip().strip('"').strip("'")
    return env
ENV = load_env()

def log(*a): print(*a, flush=True)

def save_entry(engine, file, kind, model, elapsed, extra=None):
    # per-engine sidecar — no shared-file race when engines run as parallel processes
    (OUT / f"{engine}.entry.json").write_text(json.dumps(
        {"engine": engine, "file": file, "kind": kind, "model": model,
         "elapsed_s": round(elapsed, 1), **(extra or {})}, indent=2))
    rebuild_manifest()

def rebuild_manifest():
    engines = {}
    for sc in sorted(OUT.glob("*.entry.json")):
        try:
            e = json.loads(sc.read_text()); engines[e["engine"]] = e
        except Exception: pass
    src = None
    if (OUT / "source.json").exists():
        try: src = json.loads((OUT / "source.json").read_text()).get("source")
        except Exception: pass
    MANIFEST.write_text(json.dumps({"source": src, "engines": engines}, indent=2))

def set_source(path, title=None):
    j = {}
    if (OUT / "source.json").exists():
        try: j = json.loads((OUT / "source.json").read_text())
        except Exception: pass
    j["source"] = str(path)
    if title: j["title"] = title
    (OUT / "source.json").write_text(json.dumps(j, indent=2))
    rebuild_manifest()

def build_index():
    """Scan depths/<key>/manifest.json -> depths/index.json (the source list for the studio)."""
    srcs = []
    for d in sorted(DEPTHS.iterdir()):
        if not d.is_dir(): continue
        sj = d / "source.json"
        if not (d / "manifest.json").exists(): continue
        title = d.name
        if sj.exists():
            try: title = json.loads(sj.read_text()).get("title", d.name)
            except Exception: pass
        srcs.append({"key": d.name, "title": title,
                     "thumb": f"{d.name}/source.jpg" if (d / "source.jpg").exists() else None})
    (DEPTHS / "index.json").write_text(json.dumps({"sources": srcs}, indent=2))

# ---- normalization: array -> grayscale PNG with white=near ----
def save_depth(arr, engine, invert=False):
    import numpy as np
    from PIL import Image
    a = np.asarray(arr, dtype="float32")
    lo, hi = np.percentile(a, 1), np.percentile(a, 99)   # robust to outliers
    a = (a - lo) / max(hi - lo, 1e-6)
    a = np.clip(a, 0, 1)
    if invert: a = 1.0 - a
    img = Image.fromarray((a * 255).astype("uint8"), mode="L")
    fp = OUT / f"{engine}.png"
    img.save(fp)
    log(f"  -> wrote {fp.name} ({img.width}x{img.height})")
    return fp.name

def rgb_to_depth_png(rgb_bytes, engine, invert=False):
    """A returned depth image (8-bit RGB, or 16-bit single-channel) -> normalized grayscale,
    white=near. PIL.convert('L') mangles 16-bit depth to black, so read raw values for those."""
    import numpy as np
    from PIL import Image
    im = Image.open(io.BytesIO(rgb_bytes))
    arr = np.asarray(im)
    if arr.ndim == 3:                       # RGB(A) -> luminance
        arr = np.asarray(im.convert("L"))
    # else single channel (8- or 16-bit): use raw values directly
    log(f"  [{engine}] raw {im.mode} {arr.shape} {arr.dtype} min={arr.min()} max={arr.max()}")
    return save_depth(arr, engine, invert=invert)

# ============================ LOCAL ENGINES ============================
def run_da2(src):
    """Depth-Anything-V2-Large via HF transformers. Outputs inverse depth: near=large."""
    import torch, numpy as np
    from transformers import pipeline
    from PIL import Image
    t0 = time.time()
    dev = "mps" if torch.backends.mps.is_available() else "cpu"
    log(f"[da2] loading Depth-Anything-V2-Large-hf on {dev} ...")
    pipe = pipeline("depth-estimation", model="depth-anything/Depth-Anything-V2-Large-hf", device=dev)
    out = pipe(Image.open(src).convert("RGB"))
    d = out["predicted_depth"].squeeze().float().cpu().numpy()  # near = large
    f = save_depth(d, "da2", invert=False)                      # large->white = near->white
    save_entry("da2", f, "depth-net", "Depth-Anything-V2-Large (local, MPS)", time.time()-t0)

def _da3_stubs():
    # DA3's api.py eagerly imports gaussian-splat export (moviepy/gsplat) and xformers,
    # none of which are needed for — or buildable on — Apple Silicon monocular depth.
    # xformers has a pure-torch fallback in the model; stub the export-only CUDA deps.
    import types
    for m in ("moviepy", "moviepy.editor", "gsplat", "xformers", "xformers.ops", "plyfile"):
        sys.modules.setdefault(m, types.ModuleType(m))
    exp = types.ModuleType("depth_anything_3.utils.export")
    exp.export = lambda *a, **k: None
    sys.modules.setdefault("depth_anything_3.utils.export", exp)

def _run_da3(src, repo, key, label):
    """Depth-Anything-3 (local). prediction.depth is metric-ish depth (near=small),
    so invert to make near=white for the studio."""
    import torch, numpy as np
    t0 = time.time()
    dev = "mps" if torch.backends.mps.is_available() else "cpu"
    _da3_stubs()
    from depth_anything_3.api import DepthAnything3
    log(f"[{key}] loading {repo} on {dev} ...")
    model = DepthAnything3.from_pretrained(repo).to(device=dev)
    pred = model.inference([str(src)])
    d = np.asarray(pred.depth).squeeze().astype("float32")
    f = save_depth(d, key, invert=True)
    save_entry(key, f, "depth-net", label, time.time()-t0)

def run_da3(src):       # purpose-built monocular model
    _run_da3(src, "depth-anything/DA3MONO-LARGE", "da3", "Depth-Anything-3 MONO-Large (local, MPS)")

def run_da3giant(src):  # SOTA-capacity any-view model (1B), also does single-image depth
    _run_da3(src, "depth-anything/DA3-GIANT-1.1", "da3giant", "Depth-Anything-3 GIANT-1.1 1B (local, MPS)")

# ============================ FUSION ============================
# A depth net gets the GLOBAL shape right but smooths fine relief; a normal map (or a
# generative depth) carries the HIGH-frequency detail. Fuse: low-pass(reliable base) +
# high-pass(detail). Base = DA2 (must be generated first).
def _load_gray(name):
    import numpy as np
    from PIL import Image
    p = OUT / name
    if not p.exists(): raise RuntimeError(f"fusion needs {name} first (run da2 / its detail engine before fusion)")
    return np.asarray(Image.open(p).convert("L"), dtype="float32") / 255.0

def _norm01(a):
    import numpy as np
    lo, hi = np.percentile(a, 1), np.percentile(a, 99)
    return np.clip((a - lo) / max(hi - lo, 1e-6), 0, 1)

def integrate_normals(nx, ny, nz):
    """Frankot-Chellappa: least-squares integrate a normal field to a height map (FFT)."""
    import numpy as np
    nz = np.where(np.abs(nz) < 0.05, 0.05 * np.sign(nz + 1e-9), nz)
    p, q = -nx / nz, -ny / nz                     # dz/dx, dz/dy
    H, W = p.shape
    wx = np.fft.fftfreq(W).reshape(1, W) * 2 * np.pi
    wy = np.fft.fftfreq(H).reshape(H, 1) * 2 * np.pi
    denom = wx**2 + wy**2; denom[0, 0] = 1.0
    Z = (-1j * wx * np.fft.fft2(p) - 1j * wy * np.fft.fft2(q)) / denom
    Z[0, 0] = 0
    return np.real(np.fft.ifft2(Z))

def _fuse(base, detail, key, alpha=0.6, sigma_frac=1/50.0, invert_detail=False):
    # Keep the FULL reliable base (DA2 is already crisp) and ADD the detail source's high
    # frequencies on top — augment, don't replace. (lowpass+highpass-swap muddied DA2.)
    import numpy as np
    from scipy.ndimage import gaussian_filter, zoom
    if detail.shape != base.shape:
        detail = zoom(detail, (base.shape[0]/detail.shape[0], base.shape[1]/detail.shape[1]), order=1)
    detail = _norm01(detail)
    if invert_detail: detail = 1 - detail
    sig = max(2.0, base.shape[1] * sigma_frac)
    det_hp = detail - gaussian_filter(detail, sig)   # detail's fine frequencies only
    return base + alpha * det_hp                     # full DA2 structure + injected detail

def run_fuse_normal(src):
    """DA2 global shape + fine relief recovered from Marigold surface normals (local)."""
    import torch, numpy as np
    from PIL import Image
    for m in [k for k in list(sys.modules) if k == "xformers" or k.startswith("xformers.")]:
        del sys.modules[m]
    import diffusers
    t0 = time.time()
    dev = "mps" if torch.backends.mps.is_available() else "cpu"
    log(f"[fuse_normal] Marigold-Normals (local, {dev}) + DA2 base ...")
    pipe = diffusers.MarigoldNormalsPipeline.from_pretrained(
        "prs-eth/marigold-normals-v1-1", torch_dtype=torch.float32).to(dev)
    out = pipe(Image.open(src).convert("RGB"), num_inference_steps=10)
    N = np.asarray(out.prediction).squeeze().astype("float32")   # (H,W,3) in [-1,1]
    z = integrate_normals(N[..., 0], N[..., 1], N[..., 2])
    base = _load_gray("da2.png")
    # normal-integrated detail sign can flip; pick the orientation that correlates with DA2
    from scipy.ndimage import zoom
    zr = zoom(_norm01(z), (base.shape[0]/z.shape[0], base.shape[1]/z.shape[1]), order=1)
    flip = np.corrcoef(zr.ravel(), base.ravel())[0,1] < 0
    f = save_depth(_fuse(base, z, "fuse_normal", invert_detail=flip), "fuse_normal")
    save_entry("fuse_normal", f, "fusion", "DA2 + Marigold-Normals detail (local, MPS)", time.time()-t0)

def run_fuse_nano(src):
    """DA2 global shape + Nano Banana's high-frequency detail (DA2 local + nano hosted)."""
    t0 = time.time()
    log("[fuse_nano] DA2 base + Nano Banana detail ...")
    base = _load_gray("da2.png"); detail = _load_gray("nanobanana.png")
    f = save_depth(_fuse(base, detail, "fuse_nano"), "fuse_nano")
    save_entry("fuse_nano", f, "fusion", "DA2 + Nano Banana Pro detail (hybrid)", time.time()-t0)

# ============================ FAL ENGINES ============================
def _fal_run(endpoint, payload):
    key = ENV.get("FAL_KEY")
    if not key: raise RuntimeError("FAL_KEY not in central/.env")
    req = urllib.request.Request(
        f"https://fal.run/{endpoint}",
        data=json.dumps(payload).encode(),
        headers={"Authorization": f"Key {key}", "Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=300) as r:
        return json.loads(r.read())

def _data_uri(src):
    b = pathlib.Path(src).read_bytes()
    return "data:image/jpeg;base64," + base64.b64encode(b).decode()

def _download(url):
    with urllib.request.urlopen(url, timeout=300) as r:
        return r.read()

def _first_image_url(res):
    if isinstance(res, dict):
        if "image" in res and isinstance(res["image"], dict): return res["image"]["url"]
        if "images" in res and res["images"]: return res["images"][0]["url"]
        if "depth" in res and isinstance(res["depth"], dict): return res["depth"]["url"]
    raise RuntimeError(f"no image url in fal response: {str(res)[:300]}")

def run_marigold(src):
    """Marigold depth LOCALLY via diffusers (MPS). Open weights — no need to go hosted.
    prediction is affine-invariant depth in [0,1] with 0=near; invert for white=near."""
    import torch, numpy as np
    from PIL import Image
    # DA3 (if it ran earlier in this process) stubs a fake xformers in sys.modules whose
    # __spec__ is None; diffusers' xformers probe crashes on that. xformers isn't actually
    # installed, so drop the stubs and let diffusers correctly see it as absent.
    for m in [k for k in list(sys.modules) if k == "xformers" or k.startswith("xformers.")]:
        del sys.modules[m]
    import diffusers
    t0 = time.time()
    dev = "mps" if torch.backends.mps.is_available() else "cpu"
    log(f"[marigold] diffusers MarigoldDepthPipeline (local, {dev}) ...")
    pipe = diffusers.MarigoldDepthPipeline.from_pretrained(
        "prs-eth/marigold-depth-lcm-v1-0", torch_dtype=torch.float32).to(dev)
    out = pipe(Image.open(src).convert("RGB"), num_inference_steps=4)
    d = np.asarray(out.prediction).squeeze().astype("float32")   # [0,1], 0=near
    f = save_depth(d, "marigold", invert=True)                   # -> white=near
    save_entry("marigold", f, "depth-net", "Marigold LCM (local, MPS, diffusers)", time.time()-t0)

def run_marigold_fal(src):
    t0 = time.time()
    log("[marigold-fal] fal-ai/imageutils/marigold-depth ...")
    res = _fal_run("fal-ai/imageutils/marigold-depth", {"image_url": _data_uri(src)})
    f = rgb_to_depth_png(_download(_first_image_url(res)), "marigold", invert=False)
    save_entry("marigold", f, "depth-net", "Marigold (fal, hosted)", time.time()-t0)

def run_midas(src):
    """MiDaS (DPT-Hybrid) LOCALLY via transformers (MPS). Open weights. near=large."""
    import torch, numpy as np
    from transformers import pipeline
    from PIL import Image
    t0 = time.time()
    dev = "mps" if torch.backends.mps.is_available() else "cpu"
    log(f"[midas] DPT-Hybrid-MiDaS (local, {dev}) ...")
    pipe = pipeline("depth-estimation", model="Intel/dpt-hybrid-midas", device=dev)
    d = pipe(Image.open(src).convert("RGB"))["predicted_depth"].squeeze().float().cpu().numpy()
    f = save_depth(d, "midas", invert=False)                     # near=large -> white=near
    save_entry("midas", f, "depth-net", "MiDaS / DPT-Hybrid (local, MPS)", time.time()-t0)

def run_midas_fal(src):
    t0 = time.time()
    log("[midas-fal] fal-ai/imageutils/depth ...")
    res = _fal_run("fal-ai/imageutils/depth", {"image_url": _data_uri(src)})
    f = rgb_to_depth_png(_download(_first_image_url(res)), "midas", invert=False)
    save_entry("midas", f, "depth-net", "MiDaS (fal, hosted)", time.time()-t0)

DEPTH_PROMPT = ("Convert this photograph of a sculpted bas-relief into a precise GRAYSCALE "
    "DEPTH MAP for 3D extrusion. The surfaces closest to the viewer / highest relief must be "
    "PURE WHITE; the deepest recessed background must be PURE BLACK; everything between is a "
    "smooth continuous gray gradient that follows the true sculpted height of the form. "
    "No color, no text, no lighting or shadows — only height encoded as brightness. "
    "Keep the same framing and proportions as the input.")

def run_nanobanana(src):
    t0 = time.time()
    log("[nanobanana] fal-ai/gemini-3-pro-image-preview/edit (generative depth guess) ...")
    res = _fal_run("fal-ai/gemini-3-pro-image-preview/edit",
                   {"prompt": DEPTH_PROMPT, "image_urls": [_data_uri(src)]})
    f = rgb_to_depth_png(_download(_first_image_url(res)), "nanobanana", invert=False)
    save_entry("nanobanana", f, "generative", "Nano Banana Pro / Gemini-3-Pro-Image (fal)", time.time()-t0)

# ============================ OPENAI ENGINE ============================
def run_chatgpt(src):
    t0 = time.time()
    key = ENV.get("OPENAI_API_KEY")
    if not key: raise RuntimeError("OPENAI_API_KEY not in central/.env")
    log("[chatgpt] OpenAI image edit (generative depth guess) ...")
    # multipart/form-data to /v1/images/edits
    import mimetypes, uuid
    boundary = uuid.uuid4().hex
    img = pathlib.Path(src).read_bytes()
    parts = []
    def field(name, value):
        parts.append(f"--{boundary}\r\nContent-Disposition: form-data; name=\"{name}\"\r\n\r\n{value}\r\n".encode())
    field("model", os.environ.get("OPENAI_IMAGE_MODEL", "gpt-image-1"))
    field("prompt", DEPTH_PROMPT)
    field("size", "1024x1024")
    parts.append((f"--{boundary}\r\nContent-Disposition: form-data; name=\"image\"; "
                  f"filename=\"src.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n").encode() + img + b"\r\n")
    parts.append(f"--{boundary}--\r\n".encode())
    body = b"".join(parts)
    req = urllib.request.Request("https://api.openai.com/v1/images/edits", data=body,
        headers={"Authorization": f"Bearer {key}", "Content-Type": f"multipart/form-data; boundary={boundary}"})
    with urllib.request.urlopen(req, timeout=300) as r:
        res = json.loads(r.read())
    d = res["data"][0]
    raw = base64.b64decode(d["b64_json"]) if d.get("b64_json") else _download(d["url"])
    f = rgb_to_depth_png(raw, "chatgpt", invert=False)
    save_entry("chatgpt", f, "generative", "ChatGPT Image / gpt-image-1 (hosted)", time.time()-t0)

ENGINES = {"da2": run_da2, "da3": run_da3, "da3giant": run_da3giant, "marigold": run_marigold,
           "midas": run_midas, "midas_fal": run_midas_fal, "marigold_fal": run_marigold_fal,
           "nanobanana": run_nanobanana, "chatgpt": run_chatgpt,
           "fuse_normal": run_fuse_normal, "fuse_nano": run_fuse_nano}

DEFAULT_SET = ["da2", "da3giant", "marigold", "midas", "nanobanana", "chatgpt"]

def main():
    global OUT, MANIFEST
    ap = argparse.ArgumentParser()
    ap.add_argument("--src", default=str(HERE / "source.jpg"))
    ap.add_argument("--key", default="default", help="source key -> depths/<key>/")
    ap.add_argument("--title", default=None, help="human label for the source")
    ap.add_argument("engines", nargs="*", default=["set"],
                    help="engine names, or 'all' / 'set' (the default comparison set)")
    a = ap.parse_args()
    src = pathlib.Path(a.src).resolve()
    OUT = DEPTHS / a.key; OUT.mkdir(parents=True, exist_ok=True)
    MANIFEST = OUT / "manifest.json"
    # stash a downscaled copy of the source for the studio's thumbnail + plaque aspect
    try:
        from PIL import Image
        im = Image.open(src).convert("RGB"); im.thumbnail((1400, 1400))
        im.save(OUT / "source.jpg", quality=92)
    except Exception as ex:
        log(f"!! could not stage source thumb: {ex}")
    set_source(src, a.title or a.key)
    engs = a.engines or ["set"]
    todo = DEFAULT_SET if engs == ["set"] else (list(ENGINES) if engs == ["all"] else engs)
    log(f"key: {a.key}\nsource: {src}\nengines: {todo}\n")
    for e in todo:
        if e not in ENGINES: log(f"!! unknown engine {e}"); continue
        try:
            ENGINES[e](src)
        except Exception as ex:
            log(f"!! {e} FAILED: {type(ex).__name__}: {ex}")
    build_index()
    log("\ndone.")

if __name__ == "__main__":
    main()
