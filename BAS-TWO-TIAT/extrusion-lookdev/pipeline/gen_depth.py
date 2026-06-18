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
OUT  = HERE.parent / "depths"
OUT.mkdir(exist_ok=True)
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

def set_source(path):
    j = {}
    if (OUT / "source.json").exists():
        try: j = json.loads((OUT / "source.json").read_text())
        except Exception: pass
    j["source"] = str(path); (OUT / "source.json").write_text(json.dumps(j, indent=2))
    rebuild_manifest()

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

def run_da3(src):
    """Depth-Anything-3 monocular Large (local). prediction.depth is metric-ish depth: near=small,
    so invert to make near=white."""
    import torch, numpy as np, types
    t0 = time.time()
    dev = "mps" if torch.backends.mps.is_available() else "cpu"
    # DA3's api.py eagerly imports gaussian-splat export (moviepy/gsplat) and xformers,
    # none of which are needed for — or buildable on — Apple Silicon monocular depth.
    # xformers has a pure-torch fallback in the model; stub the export-only CUDA deps.
    for m in ("moviepy", "moviepy.editor", "gsplat", "xformers", "xformers.ops", "plyfile"):
        sys.modules.setdefault(m, types.ModuleType(m))
    # short-circuit the 3D-export module (glb/3DGS/video) — unused for a 2D depth map,
    # and its transitive deps don't build on Apple Silicon.
    exp = types.ModuleType("depth_anything_3.utils.export")
    exp.export = lambda *a, **k: None
    sys.modules.setdefault("depth_anything_3.utils.export", exp)
    from depth_anything_3.api import DepthAnything3
    log(f"[da3] loading DA3MONO-LARGE on {dev} ...")
    model = DepthAnything3.from_pretrained("depth-anything/DA3MONO-LARGE").to(device=dev)
    pred = model.inference([str(src)])
    d = np.asarray(pred.depth).squeeze().astype("float32")
    # DA3 mono returns depth (near=small). invert so near=white to match the studio.
    f = save_depth(d, "da3", invert=True)
    save_entry("da3", f, "depth-net", "Depth-Anything-3 MONO-Large (local, MPS)", time.time()-t0)

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

ENGINES = {"da2": run_da2, "da3": run_da3, "marigold": run_marigold,
           "midas": run_midas, "nanobanana": run_nanobanana, "chatgpt": run_chatgpt}

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--src", default=str(HERE / "source.jpg"))
    ap.add_argument("engines", nargs="+")
    a = ap.parse_args()
    src = pathlib.Path(a.src).resolve()
    set_source(src)
    todo = list(ENGINES) if a.engines == ["all"] else a.engines
    log(f"source: {src}\nengines: {todo}\n")
    for e in todo:
        if e not in ENGINES: log(f"!! unknown engine {e}"); continue
        try:
            ENGINES[e](src)
        except Exception as ex:
            log(f"!! {e} FAILED: {type(ex).__name__}: {ex}")
    log("\ndone.")

if __name__ == "__main__":
    main()
