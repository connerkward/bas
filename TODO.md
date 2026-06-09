# BAS — TODO

Repo layout:
- `BAS-ONE-BYOB/` — the first iteration (the multi-monitor cross / desync running figures, BYOB show). Archived; full git history preserved.
- `BAS-TWO-TIAT/` — current direction: turning the running-figure depth map into a **machinable XPS bas-relief** (CNC-milled foam), plus presentation renders.

## BAS-TWO-TIAT/ — what it is
A running-figure depth map (Muybridge athletes overlaid at staggered depths → a marching crowd) extruded into a heightfield relief sized for CNC milling out of nominal XPS foam board.

- `BAS-TWO-TIAT/extrusion-lookdev/index.html` — the **lookdev studio** (Three.js, no build). Serve the folder statically and open it. Controls: plaque size, XPS board thickness + backplate (with thin-backplate warnings), frame, depth-map remap (gamma/contrast/floor/blur/invert), crop, 4×-UltraSharp upscaled-source toggle, mesh resolution, X-ray (see backplate), perspective/ortho, Fusion-style ViewCube, presentation render, XPS sheet-layout visualizer, STL export, settings-JSON round-trip.
- `BAS-TWO-TIAT/extrusion-lookdev/depth.png` — source depth map (560×315).
- `BAS-TWO-TIAT/extrusion-lookdev/depth-up.jpg` — 4× UltraSharp upscale (2240×1260).
- `BAS-TWO-TIAT/renders/` — gallery/beauty renders (see workflow note below).

## Done
- [x] Lookdev studio (all controls above), STL export, sheet-usage visualizer.
- [x] Anamorphic outpaint of the depth map (~6.4:1 wide frieze) — `renders/depth-anamorphic.png`.
- [x] Gallery renders. **Best workflow:** render the *real* geometry for shape/material, then composite into a real reference gallery photo via gpt-image-2 (two input images: relief + reference). See `renders/galref-a.png`, `galref-b.png`. Pure-AI and pure-3D-lighting both read fake; the reference composite wins.

## TODO — toward a cut piece
- [ ] **Lock the final physical piece**: plaque size (small overdoor ~600mm vs gallery ~2000mm), source (original / upscaled / anamorphic), board thickness + backplate, frame vs flush.
- [ ] **Export CNC-ready STL** and verify: watertight, correct mm scale, smallest feature survives the endmill radius. Optionally weld the two stacked solids (relief block + backplate slab share the z=base plane) into one manifold mesh for picky CAM.
- [ ] **CAM / millability pass**: ball-endmill radius vs finest detail, steep-wall / near-vertical check (ViewCube + normal-shading), roughing + finish passes, step-over, where smoothing is needed to avoid chatter.
- [ ] **Sheet-count algorithm** — decide refinement: (A) keep grid estimate, (B) subtract factory-edge trim margin (most fab-honest), (C) show a range (area floor → grid upper bound). Currently uses the grid estimate.
- [ ] **Anamorphic frieze variant** (optional): machinable STL from the 6:1 depth map, tiled across multiple 4'×8' XPS sheets.
- [ ] Final presentation renders once the piece is locked.

## Notes / gotchas
- XPS only comes in nominal thicknesses (½"–4"); the studio enforces this. Backplate < ½" warns, < ¼" "will crumble."
- Standard XPS sheet = 4'×8' (1219×2438mm); the sheet visualizer shows count + grid + % used.
- gpt-image-2 (newest; always default to newest model) flags the bare Muybridge figures as nude — prompt them clothed/draped.
- Source video traced to: `BAS-ONE-BYOB`-era Muybridge "Attitudes of Animals in Motion (Athletes)" clip (AI/Veo running clips were a separate experiment).
