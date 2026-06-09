# Desync – TouchDesigner Chronophoto Project

Real-time chronophotographic effect with sync/desync cycle using image sequences.

## What It Does

- **3 timelines** play the same image sequence with phase offsets
- **Cycle**: 5s sync → 10s desync (cosine ramp) → 5s sync, repeating (20s total)
- **Composite**: chained min/max blend (toggle for black/white background)
- **Cross dissolve**: smooth 30-frame fade at loop boundaries
- **Outputs**: `out_color`, `out_mono`, `out_invert`

## Project Layout (`/project1/desync`)

| Column | Nodes |
|--------|-------|
| **Controls** | `sync_hold`, `desync_duration`, `spread`, `baseline_fps`, `fade_zone`, `invert_mode` |
| **Sequence** | `seq_select` (constantCHOP, channel `seq` 0–4), `seq_config` (textDAT: one path per row) |
| **Desync** | `desync_val` (constantCHOP with expression) |
| **Timelines** | `timeline0`, `timeline2`, `timeline4` (moviefileinTOP) |
| **Wraps** | `wrap0`, `wrap2`, `wrap4` (for cross dissolve) |
| **Crosses** | `cross0`, `cross2`, `cross4` (crossTOP) |
| **Monos** | `mono0`, `mono2`, `mono4`, `background` |
| **Composites** | `comp0` → `comp1` → `comp_final` (chained) |
| **Post** | `post_mono`, `invert` |
| **Outputs** | `out_color`, `out_mono`, `out_invert` |
| **Record** | `record_out` (moviefileoutTOP) – image sequence or movie |

## Offline render (Python, frame-perfect, no TD)

For export that matches the preview and is not laggy, use the Python renderer (no TouchDesigner):

```bash
cd PyBas3
uv run python pre_render/render_desync.py path/to/chroma_dithered -o artifacts/desync_render --duration 20
```

- **Input:** Directory with `frame_0000.png` … `frame_0599.png` (e.g. `pre_render/outputs/.../chroma_dithered`).
- **Output:** PNGs in `-o` dir, then `desync_output.mp4` (30 fps, H.264, faststart). Use `--no-video` to only write PNGs; `--video-path` to set the MP4 path; `--invert 1` for max blend + black bg.
- Logic matches TD: desync ramp, 3 timelines (±65 spread), 30-frame cross dissolve, luminance then min/max composite, `int()` frame index (no round).
- **If the render doesn’t match the preview:** use `--compare-sec 10` (or another time) to write a single frame to `compare.png`. In TD, pause at that time (e.g. 10s), export `out_color` to an image, and compare. Use `--output-fps 60` or `120` for faster playback to match preview feel.

## Recording in TouchDesigner (images then video)

1. **In TouchDesigner:** Set `record_out` to **Type = Image Sequence**, **Image Type = PNG**, **File** = `artifacts/desync_frames/frame` (or `project.folder + '/desync_frames/frame'`). Set **Limit Length** = 600 frames if you want one 20s cycle.
2. **Put TD in Play (F5).** Turn **Record** on on `record_out`. Wait ~20 seconds. Turn **Record** off. Frames are written as `frame.0.0000.png`, `frame.0.0001.png`, … (under `artifacts/desync_frames/` or `ConnerTD/desync_frames/`).
3. **Encode to video** (smooth, no real-time encoding drops):
```bash
# From repo root or ConnerTD:
./PyBas3/td_scripts/ConnerTD/encode_desync_frames.sh
# Or with custom paths:
./encode_desync_frames.sh /path/to/frames /path/to/output.mp4
```
Script uses H.264, main profile, faststart. For desync_random use folder `desync_random_frames` and the same script with that path.

## Control Parameters

| Node | Channel | Default | Meaning |
|------|---------|---------|---------|
| `sync_hold` | sec | 5 | Seconds of full sync at start/end of cycle |
| `desync_duration` | sec | 10 | Seconds of desync phase |
| `spread` | frames | 65 | Max frame offset at full desync |
| `baseline_fps` | fps | 30 | Playback speed |
| `fade_zone` | frames | 60 | Reference (actual crossfade is 30 frames) |
| `invert_mode` | invert | 0/1 | **0** = min blend + white bg, **1** = max blend + black bg |

## Image Sequence Source / Switching Inputs

**Sequence selection:** Set `seq_select` channel `seq` to 0–4 to switch which input sequence is used. Paths are taken from the **seq_config** text DAT (one full folder path per row). Default rows:

| seq | Folder |
|-----|--------|
| 0 | chroma_dithered |
| 1 | chroma_keyed |
| 2 | raw_frames |
| 3 | chroma_atkinson |
| 4 | composite_skeleton_dithered |

Base path (edit rows in `seq_config` to add or change sequences):
```
.../pre_render/outputs/2025-02-05_runside_megaslow_compressed_720p_blend_output/<folder>
```

- 600 frames, 720×1280 (per sequence)
- **File path is an expression** – uses `seq_config[int(seq_select.seq)]` + `/frame_XXXX.png`

Other folders you can add to `seq_config`:
- `raw_frames/` - original video frames
- `chroma_dithered/` - dithered chroma key (current)
- `chroma_atkinson/` - Atkinson dithered
- `chroma_keyed/` - clean chroma key
- `composite_skeleton_dithered/` - skeleton overlay

## Timing Cycle (20 seconds)

```
0-5s:   SYNC     desync=0, all timelines same frame
5-15s:  DESYNC   desync ramps 0→1→0 (cosine), timelines spread ±65 frames
15-20s: SYNC     desync=0, all timelines converge
```

## Key Expressions

### Desync Value (`desync_val`)
```python
(0.5 - 0.5 * math.cos(((absTime.seconds % 20) - 5) / 10 * 6.28318)) if (5 <= (absTime.seconds % 20) < 15) else 0
```

### Timeline File Path (expression on `file` parameter)
```python
'/path/to/frames/frame_' + str(int(absTime.seconds * 30 + MULT * 65 * op('/project1/desync/desync_val')['desync']) % 600).zfill(4) + '.png'
```

### Cross Blend (ramps 0→1 in final 30 frames)
```python
max(0, (frame_index % 600 - (600 - 30)) / 30)
```

## Cross Dissolve (Seamless Loop)

- `wrap0/2/4`: show frames +30 ahead (via file expression)
- `cross0/2/4`: blend timeline↔wrap
- Blend ramps from 0 to 1 in final 30 frames before loop
- Chain: `timeline + wrap → cross → mono → composites`

## Blend Mode Toggle (`invert_mode`)

| Value | Blend | Background | Use For |
|-------|-------|------------|---------|
| 0 | minimum | white | black figure on white |
| 1 | maximum | black | white figure on black |

Composite operand and background color are expressions referencing `invert_mode`.

## Performance

- **Image sequences** instead of video = instant random access
- No H.264 seeking overhead
- 6 moviefileinTOPs total (3 timeline + 3 wrap)
- Smooth 30+ FPS on M1 Mac

## Files

| File | Purpose |
|------|---------|
| `Desync.toe` | TouchDesigner project |
| `DESYNC.md` | This documentation |

---

# desync_random – Multi-Sequence Randomized Variant

Subproject at `/project1/desync_random`. Cycles through multiple visual styles with crossfade transitions, random overlays, and variable parameters.

## Architecture

**Dual Chain Design** for smooth crossfades between sequences:
- **Chain A**: Shows current sequence
- **Chain B**: Shows next sequence
- **Master Cross**: Fades A→B during last `trans_time` sec of each `seq_period`
- **Overlay System**: Random second layer blended on top

Each chain has:
- 3 timelines (offset multipliers: -1, 0, +1)
- 3 wrap TOPs (for loop crossfade)
- 3 cross TOPs (blend timeline↔wrap)
- 3 mono TOPs (grayscale conversion)
- Dynamic background (black/white based on sequence)
- Chained composites (min/max based on sequence)

## Sequences (stored in `seq_config` textDAT)

Current source: `/Users/CONWARD/dev/bas/PyBas3/pre_render/outputs/2025-02-05_runside_megaslow_compressed_720p_blend_output/`

| Index | Sequence | Prefix | Blend | Frames |
|-------|----------|--------|-------|--------|
| 0 | chroma_dithered | frame_ | max | 4585 |
| 1 | chroma_keyed | frame_ | max | 4585 |
| 2 | pose_skeleton/frames | frame_ | max | 4585 |
| 3 | raw_frames | frame_ | min | 4585 |

**Blend**: max = black bg + white fig, min = white bg + black fig

## Control Parameters

### Playback Speed
| Node | Channel | Default | Purpose |
|------|---------|---------|---------|
| `speed` | speed | 1.0 | Runner speed multiplier (0.5=half, 2.0=double) |
| `src_fps` | fps | 30 | Source recording framerate |

### Desync Timing
| Node | Channel | Default | Purpose |
|------|---------|---------|---------|
| `sync_hold` | sec | 5 | Seconds timelines stay in sync (start & end of cycle) |
| `desync_duration` | sec | 10 | Seconds of desync phase (ramp up + down) |
| `desync_val` | desync | (auto) | Current desync intensity 0→1→0 |

Total cycle = `2 × sync_hold + desync_duration`

### Spread (Frame Offset)
| Node | Channel | Default | Purpose |
|------|---------|---------|---------|
| `spread_min` | frames | 30 | Minimum frame offset between timelines |
| `spread_max` | frames | 120 | Maximum frame offset between timelines |
| `spread_val` | frames | (auto) | Current spread - varies each desync cycle |

Spread changes per cycle using golden ratio for organic variation.

### Sequence Switching
| Node | Channel | Default | Purpose |
|------|---------|---------|---------|
| `seq_period` | sec | 30 | Duration per sequence before switching |
| `trans_time` | sec | 2 | Crossfade duration between sequences |
| `fade_zone` | frames | 90 | Loop crossfade region (within sequence) |

### Overlay System
| Node | Channel | Default | Purpose |
|------|---------|---------|---------|
| `overlay_idx` | idx | (auto) | Which sequence to overlay (different from main) |
| `overlay_opacity` | opacity | (auto) | Overlay visibility 0-0.7, fades during transitions |

Overlay fades out before index change, fades in after (1.5 sec fade).

## Timing Cycle

```
Desync cycle (default 20 sec):
├─ 0-5s:    SYNC      desync_val=0, timelines aligned
├─ 5-15s:   DESYNC    desync_val ramps 0→1→0 (cosine)
└─ 15-20s:  SYNC      desync_val=0, timelines converge

Sequence cycle (default 30 sec):
├─ 0-28s:   Show sequence, overlay fades in/out randomly
└─ 28-30s:  Crossfade to next sequence
```

## Output Chain

```
Chain A/B composites
       ↓
  master_cross (A↔B crossfade)
       ↓
  overlay_comp (+ overlay layer)
       ↓
  overlay_cross (opacity control)
       ↓
     tint (orange-red, animated intensity)
       ↓
  loop_fade
   ↙     ↘
  out   preview_invert → out_invert
```
- **out** – main preview
- **out_invert** – same video, inverted (level TOP); use as second viewer

## Tint

Orange-red colorization with animated intensity (~15 sec cycle):
- Highs R: 0.65 + 0.35 × sin(t × 0.42)
- Highs G: 0.23 + 0.12 × sin(t × 0.42)
- Highs B: 0.10 + 0.05 × sin(t × 0.42)

## Project Settings

- **Cook Rate**: 90 fps (set via `project.cookRate`)
- **Resolution**: 720×1280 (9:16 vertical)

## Recording

`record_out` (moviefileoutTOP) – image sequence to `desync_random_frames/frame_XXXX.png`

Encode with ffmpeg:
```bash
ffmpeg -y -framerate 90 -i desync_random_frames/frame_%04d.png -c:v libx264 -pix_fmt yuv420p -crf 18 desync_random_output.mp4
```

## Quick Reference

| Want to... | Adjust... |
|------------|-----------|
| Slow down runner | `speed` (lower = slower) |
| Longer sync periods | `sync_hold` |
| Shorter desync | `desync_duration` |
| More spread variation | `spread_min` / `spread_max` |
| Faster sequence switching | `seq_period` |
| Smoother seq transitions | `trans_time` |
