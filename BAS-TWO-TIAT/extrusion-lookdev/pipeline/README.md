# Depth pipeline for the extrusion lookdev

Generates depth/height maps for ONE head-on relief image across several engines, into
`../depths/<engine>.png` + `../depths/manifest.json`. The studio (`../index.html`) reads
the manifest and extrudes each engine's map — single-view switch or side-by-side compare.

**Convention:** every output is grayscale, **white = nearest = tallest relief**, black =
deepest background.

## Engines

Local-first (this machine, Apple-Silicon MPS) — see the `prefer-local-inference-rule`.
Only the closed-weight image models are hosted.

| engine | kind | where | model |
|---|---|---|---|
| `da2` | depth net | **local** | Depth-Anything-V2-Large (HF transformers) |
| `da3` | depth net | **local** | Depth-Anything-3 MONO-Large (ByteDance repo) |
| `marigold` | depth net | **local** | Marigold-Depth-LCM (diffusers) |
| `midas` | depth net | **local** | MiDaS / DPT-Hybrid (HF transformers) |
| `nanobanana` | generative guess | hosted | Nano Banana Pro / Gemini-3-Pro-Image (fal) — closed weights |
| `chatgpt` | generative guess | hosted | ChatGPT Image / gpt-image-1 (OpenAI) — closed weights |
| `fuse_normal` | fusion | **local** | DA2 global shape + fine relief from Marigold surface-normals (Frankot-Chellappa integrated) |
| `fuse_nano` | fusion | hybrid | DA2 global shape + Nano Banana's high-frequency detail |
| `meshy` | image→3D | hosted | Meshy-6 single-image-to-3D (fal) → GLB, front-depth rendered via `glb_depth.html` |

**Fusion** (`fuse_*`): a depth net gets the *global* shape right but smooths detail; the
fused maps keep DA2's full structure and ADD a detail source's high frequencies
(`base + α·highpass(detail)`). `fuse_normal` is fully local (Marigold-Normals → integrate
→ inject); `fuse_nano` injects Nano Banana's detail. Both need `da2.png` (and the detail
engine) generated first.

**Image-to-3D** (`meshy`): direct single-image-to-3D reconstructs a *full rounded object*,
not a relief — run via `meshy_gen.py` (fal queue → GLB), then rendered to a front-facing
depth by `glb_depth.html` (headless three.js, MeshDepthMaterial). Included as the
"best/likeliest" direct-3D comparison; it tends to produce a shallow, smoothed slab.

"generative guess" = an image model *prompted* for a height map. It looks plausible but
is **not** metric depth (gpt-image-1 in particular tends to ignore the instruction and
return a near-flat emboss). Kept as a comparison axis, labelled as such in the studio.

## Run

Each source lives in its own `../depths/<key>/` (depth maps + `manifest.json` + a
`source.jpg` thumbnail); `../depths/index.json` lists all sources and the studio cycles
them. `da3giant` (DA3-GIANT-1.1, 1B) is the default DA3 — higher-capacity than the
purpose-built `da3` (DA3MONO-LARGE); both run locally at ~the same speed.

```bash
# generate a new source (default 6-engine comparison set)
PYTHONPATH=da3repo/src .venv/bin/python gen_depth.py \
  --key woodpanel --title "Carved wood narrative panel" --src /path/to/relief.jpg set

uv venv --python 3.12 .venv && source .venv/bin/activate
uv pip install torch torchvision transformers diffusers accelerate pillow numpy requests \
  safetensors huggingface_hub einops
# DA3 (local) needs its repo on PYTHONPATH; xformers/gsplat/moviepy are CUDA-only and are
# stubbed at import time in gen_depth.py (DA3 has a pure-torch SwiGLU fallback):
git clone --depth 1 https://github.com/ByteDance-Seed/Depth-Anything-3 da3repo
VIRTUAL_ENV=.venv uv pip install -e da3repo --no-deps
VIRTUAL_ENV=.venv uv pip install omegaconf addict imageio scipy plyfile evo matplotlib tqdm opencv-python trimesh

# all engines (hosted keys read from ~/dev/central/.env: FAL_KEY, OPENAI_API_KEY)
PYTHONPATH=da3repo/src .venv/bin/python gen_depth.py --src source.jpg all
# or a subset
PYTHONPATH=da3repo/src .venv/bin/python gen_depth.py --src source.jpg da2 da3 marigold midas
```

Swap the source by pointing `--src` at any frontal relief image (e.g. a muser pick under
`~/ideas-syncthing/proj-bas/`). Each engine writes its own `../depths/<engine>.entry.json`
sidecar; the manifest is rebuilt from those, so engines can run as parallel processes
without clobbering a shared file.
