# Session: depth_blend_video Fixes + Interpolation Research

Summary of work on the pre_render pipeline, real-time feasibility, and frame interpolation.

---

## 1. Real-Time Feasibility (TouchDesigner)

| Effect | Verdict | Notes |
|--------|--------|--------|
| chroma/raw atkinson, dithered | Partial | Error diffusion sequential; Bayer in GLSL is fast approx |
| chroma extract | Yes | Resolution + Threshold TOPs |
| pose skeleton mediapipe | Yes | Python/NDI or hardware body tracking |
| raw depth | Partial | Depth Anything V2 too slow; use hardware (Kinect Azure, RealSense, ZED) or NDI from external |

---

## 2. depth_blend_video.py Fixes

### 2.1 Wrong Output Directory for extract_frames

**Bug:** `extract_frames()` was called with `chroma_keyed_dir` instead of `args.output_dir`. So `raw_frames` was created under `chroma_keyed/raw_frames` while the rest of the script expected `args.output_dir/raw_frames`.

**Fix:** Pass `args.output_dir` to `extract_frames()` so both `raw_frames` and `chroma_keyed` live under `args.output_dir`.

### 2.2 Race: depth_images Check Before Population

**Bug:** Script checked for `depth_images` (and proceeded) before the background depth estimation thread finished. `depth_future.result()` was called later → RuntimeError when depth-dependent steps ran with empty list.

**Fix:** Wait on `depth_future.result()` earlier so `depth_images` is populated before any logic that depends on it.

### 2.3 Interactive Depth Tuning Removed

**Change:** User does not want UI dialogs. Call to `interactive_depth_tuning` removed; pipeline uses default depth parameters only.

### 2.4 Depth Estimation on Chroma Frames

**Change:** `run_depth_estimation` now uses chroma-keyed frame paths for depth estimation (not raw frame paths), so depth aligns with the keyed output.

### 2.5 Cleanup

- Removed corrupted output dir: `outputs/runside-megaslow-compressed_blend_output` before re-run.
- depth_blend_video re-run with effects: dithered, atkinson, extract, depth (no UI).

---

## 3. pose_skeleton_render.py

- Run on `input_videos/runside-megaslow-compressed.mp4` completed successfully.
- Output: 4585 frames in `outputs/runside-megaslow-compressed_skeleton/`.

---

## 4. Frame Interpolation

**How we do it:** Frame interpolation (e.g. for `runside_1080p.mp4`) is run as a **ComfyUI workload on the desktop**, not via local Python.

- **Workflow:** FILM VFI (same as ComfyUI-Frame-Interpolation). Workflow lives in **comfyui-workflows-bas** as `workflows/utility-frame_interpolation-film.json`.
- **Run:** Start tunnel to desktop (`scripts/comfyui-tunnel.command` from comfyui-workflows-aix), then run the workflow in ComfyUI Desktop 2026 on the Windows machine (or via MCP). Input video must be where the desktop can see it (e.g. GDrive); copy output to `pre_render/outputs/<date>/` when done.
- **Why:** Desktop has GPU; avoids MPS/CPU-only limits. No in-repo Python interpolation script.

---

## 5. Git (at Session Time)

- **Modified:** `depth_blend_video.py`, `test_all_effects.py`, .DS_Store, various `__pycache__/*.pyc`.
- **Untracked:** `DEPTH_EFFECTS_REFERENCE.md`, `td_scripts/ConnerTD/Desync.toe`, `td_scripts/desync_setup.py`, `input_videos/runside_1080p.mp4`.
- **Warning:** `.gitattributes` line 3 — `"*.blend" is not a valid attribute name`.

---

## 6. Veo Watermark Blur (2025-02-05)

### Problem
Veo "Veo" text watermark in bottom-right of generated videos. Visible on all non-chroma frames.

### Solution
- **Coords at 720p:** (660, 1245) → (703, 1262) — 43×17px box
- **Feather:** 10px margin, linear falloff from box edges
- **Blur:** Double-pass Gaussian (k=51 capped to patch size)
- **Resolution scaling:** All coords scale by `width/720` and `height/1280` for 1080p etc.

### Implementation
- `depth_blend_video.py`: `_blur_watermark_region()` — feathered blur, resolution-scaled
- `blur_watermark_raw.py`: Standalone script, same logic
- `test_watermark_blur.py`: Test script (reads frame 0 from source video, applies blur)
- **Pipeline default:** Blur enabled unless `--no-blur-watermark`
- **Pre-blur video approach:** For speed, blur source video once with cv2 (261fps), then run pipeline with `--no-blur-watermark` on the pre-blurred `-nowm.mp4`

### Constants (720p reference)
```
_VEO_BOX_X1_REF = 660   _VEO_BOX_Y1_REF = 1245
_VEO_BOX_W_REF  = 43    _VEO_BOX_H_REF  = 17
_VEO_FEATHER_REF = 10
```

## 7. Depth on Raw Frames (2025-02-05)

**Change:** Depth estimation now always uses raw frames (not chroma-keyed) when available. Better depth maps from full image with background context.

## 8. New Effects (2025-02-05)

### depth_banding_v / depth_banding_h
Directional depth contour lines using Sobel instead of Canny:
- `depth_banding_v`: Sobel(dx=0, dy=1) — horizontal contour lines
- `depth_banding_h`: Sobel(dx=1, dy=0) — vertical contour lines
Same quantization/CLAHE pipeline as `depth_banding`.

## 9. Pre-render v2 Output (2025-02-05)

`outputs/2025-02-05_runside_megaslow_compressed_720p_v2/`

Source: `input_videos/runside-megaslow-compressed-nowm.mp4` (pre-blurred watermark)

Effects: depth, raw_depth, dithered, atkinson, microres, lowres, extract, depth_banding, depth_banding_v, depth_banding_h, red_overlay, chroma_dithered, chroma_atkinson, raw_dithered, raw_atkinson, chroma_extract + chronophoto matrices + long exposure variants.

---

## References

- Chronophoto: `CHRONOPHOTO_CONTEXT.md`
- Depth/contour effects: `DEPTH_BANDING_SESSION.md`, `DEPTH_EFFECTS_REFERENCE.md`
- Pipeline script: `depth_blend_video.py`
- Watermark test: `test_watermark_blur.py`
- Standalone watermark blur: `blur_watermark_raw.py`
