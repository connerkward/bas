# Depth Effects Reference

Saved effects from contour exploration. All use chroma-keyed source, depth from raw frame, red overlay.

See `DEPTH_BANDING_SESSION.md` for full session documentation.

## 1. Topo (wrap)
- **Type:** Topographic contours (perpendicular to depth gradient, wrap around form)
- **Params:** num_bands=12, blur=5, dilate=1
- **Method:** Quantize depth → Canny edge detect → mask to subject

## 2. Topo 20b
- **Type:** Same as Topo, more bands
- **Params:** num_bands=20, blur=5, dilate=1

## 3. Horizontal contours
- **Type:** Contours run left–right (from d/dx = vertical edges)
- **Params:** num_bands=12, blur=5
- **Method:** Quantize depth → horizontal Sobel (1,0) → threshold → dilate

## 4. Horizontal 20b
- **Type:** Same as Horizontal, more bands
- **Params:** num_bands=20, blur=5

## 5. Vertical contours
- **Type:** Contours run top–bottom (from d/dy = horizontal edges)
- **Params:** num_bands=12, blur=5
- **Method:** Quantize depth → vertical Sobel (0,1) → threshold → dilate

## 6. Vertical 20b
- **Type:** Same as Vertical, more bands
- **Params:** num_bands=20, blur=5

## Red overlay
- Tint: BGR (0, 0, 255)
- Alpha: 0.85 on contour pixels, blend with source

## Pipeline
1. Depth map (normalized 0–255)
2. Otsu subject mask
3. CLAHE on masked depth
4. Gaussian blur
5. Quantize to bands
6. Edge detect (Canny for topo, Sobel for horiz/vert)
7. Dilate 2x2, 1 iter
8. Red overlay on chroma source
