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
| **Desync** | `desync_val` (constantCHOP with expression) |
| **Timelines** | `timeline0`, `timeline2`, `timeline4` (moviefileinTOP) |
| **Wraps** | `wrap0`, `wrap2`, `wrap4` (for cross dissolve) |
| **Crosses** | `cross0`, `cross2`, `cross4` (crossTOP) |
| **Monos** | `mono0`, `mono2`, `mono4`, `background` |
| **Composites** | `comp0` → `comp1` → `comp_final` (chained) |
| **Post** | `post_mono`, `invert` |
| **Outputs** | `out_color`, `out_mono`, `out_invert` |

## Control Parameters

| Node | Channel | Default | Meaning |
|------|---------|---------|---------|
| `sync_hold` | sec | 5 | Seconds of full sync at start/end of cycle |
| `desync_duration` | sec | 10 | Seconds of desync phase |
| `spread` | frames | 65 | Max frame offset at full desync |
| `baseline_fps` | fps | 30 | Playback speed |
| `fade_zone` | frames | 60 | Reference (actual crossfade is 30 frames) |
| `invert_mode` | invert | 0/1 | **0** = min blend + white bg, **1** = max blend + black bg |

## Image Sequence Source

Currently using pre-rendered frames:
```
/Users/CONWARD/dev/bas/PyBas3/pre_render/outputs/runside_megaslow_compressed_720p_blend_output/chroma_dithered/frame_XXXX.png
```

- 600 frames, 720×1280
- **File path is an expression** - dynamically loads correct frame each cook

Available sequences in same directory:
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

## Changing Image Sequence

To switch sequences, update the `SEQ_BASE` path in timeline and wrap file expressions:

```python
# In TD Python or via MCP:
SEQ_BASE = '/path/to/new/sequence/frame_'
for i in [0, 2, 4]:
    t = op('/project1/desync/timeline' + str(i))
    # ... update t.par.file.expr with new SEQ_BASE
```

Also update `VIDEO_LENGTH` if frame count differs.
