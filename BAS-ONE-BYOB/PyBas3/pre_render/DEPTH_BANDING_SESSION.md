# Depth Banding Session Documentation

Summary of work on depth-based contour/banding effects for PyBas3 pre_render pipeline.

---

## 1. Initial Problem

**Chroma depth banding** was covering both the figure and the background. Banding should isolate the figure only.

**Root cause:** Algorithm drew bands wherever `depth >= 10`. The depth map covers the whole image, so background pixels (with non-zero depth) also received bands.

---

## 2. Fixes Applied

### 2.1 Subject Mask (Initial Fix)

Mask banding to figure-only using brightness: `subject_mask = gray > 10` (chroma background is black).

### 2.2 Auto Depth Threshold (Otsu)

Replace fixed threshold with Otsu on the depth map: `depth_thresh, _ = cv2.threshold(depth_map, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)`. Subject = pixels with `depth > depth_thresh`.

### 2.3 Contour-Based Approach

Switched from horizontal scanlines to **contour lines** that follow depth topology (like a topographic map):

1. Mask depth to subject
2. CLAHE on masked depth (enhance contrast within figure)
3. Gaussian blur (low = sharper internal lines)
4. Quantize depth into N bands
5. Canny edge detect on quantized image → contour boundaries
6. Dilate for visibility
7. Mask to subject

**Key parameters:** Fewer bands (8–12) = distinct internal contours. More bands (20–30) = denser, can merge into silhouette.

---

## 3. Contour Direction Variants

Three contour styles (all non-topo):

| Name | Method | Effect |
|------|--------|--------|
| **Topo (wrap)** | Canny on quantized depth | Contours wrap around form (iso-depth lines) |
| **Horizontal** | Horizontal Sobel (d/dx) on quantized depth | Contours run left–right |
| **Vertical** | Vertical Sobel (d/dy) on quantized depth | Contours run top–bottom |

---

## 4. Red Overlay

Effects are tinted red and composited over the chroma source:

- Tint: BGR `(0, 0, 255)`
- Alpha: 0.85 on contour pixels
- Blend: `overlay * alpha + source * (1 - alpha)`

---

## 5. Saved Effect Definitions

| # | Name | Type | Params |
|---|------|------|--------|
| 1 | Topo (wrap) | Topographic contours | num_bands=12, blur=5, dilate=1 |
| 2 | Topo 20b | Same, more bands | num_bands=20, blur=5, dilate=1 |
| 3 | Horizontal contours | Left–right contours | num_bands=12, blur=5 |
| 4 | Horizontal 20b | Same, more bands | num_bands=20, blur=5 |
| 5 | Vertical contours | Top–bottom contours | num_bands=12, blur=5 |
| 6 | Vertical 20b | Same, more bands | num_bands=20, blur=5 |

**20b** = 20 bands (number of depth quantization levels).

---

## 6. Pipeline (Full)

```
1. Depth map (normalized 0–255, from Depth-Anything-V2)
2. Otsu subject mask
3. CLAHE on masked depth (clipLimit=3.0, tileGridSize=8x8)
4. Gaussian blur (kernel 3 or 5)
5. Quantize: depth_f * num_bands → uint8
6. Edge detect:
   - Topo: Canny(15, 60)
   - Horizontal: Sobel(1,0) → threshold 20
   - Vertical: Sobel(0,1) → threshold 20
7. Dilate 2x2, 1 iter
8. Mask to subject
9. (Optional) Red overlay on chroma source
```

---

## 7. Files Modified

- **`test_all_effects.py`** – Depth banding uses contour approach (12 bands, blur=3). Fallback frame from `outputs/test_frames/frame.png` when blend output missing.
- **`depth_blend_video.py`** – Same contour depth banding in main pipeline.
- **`DEPTH_EFFECTS_REFERENCE.md`** – Effect definitions and params.

---

## 8. Outputs Generated

| File | Description |
|------|-------------|
| `chroma_depth_banding_comparison.png` | Parameter sweep (bands, blur, thick) |
| `chroma_banding_contour_vs_directional.png` | Contour vs horizontal/vertical scanlines |
| `chroma_contour_directions_red_overlay.png` | Topo, Horizontal, Vertical – red overlay on chroma |
| `contour_banding_refined.png` | 12-panel parameter comparison |
| `filled_banding_patterns.png` | Scanline, crosshatch, gradient variants |
| `raw_depth_banding.png` / `chroma_depth_banding.png` | Single-frame outputs from test_all_effects |

---

## 9. Fallback Frame

When blend output paths are missing, `test_all_effects` uses:

- **Path:** `outputs/test_frames/frame.png`
- **Source:** Frame 100 from `input_videos/runside-megaslow-compressed.mp4`
- **Chroma:** `chroma_key_green()` applied to raw frame

Extract with: `ffmpeg -i input_videos/runside-megaslow-compressed.mp4 -vf "select=eq(n\,100)" -vframes 1 -update 1 outputs/test_frames/frame.png`

---

## 10. TouchDesigner Real-Time

**Banding pipeline only** (given a depth map): real-time. Blur, quantize, edge detect, dilate, composite are standard TOP ops.

**Depth estimation** (Depth-Anything-V2): not real-time (~100–500 ms/frame). Use hardware depth (RealSense, Kinect), stereo, or lighter models for live TD.
