# Desync – TouchDesigner Chronophoto Project

Chronophotographic runner with sync/desync cycle: multiple time-offset timelines, composite (black figures on white), red tint, and optional mono output.

## What It Does

- **5 timelines** play the same video with phase offsets. Offsets are driven by a **desync modulator**: 3s fully synced → 8s S-curve desync → 3s synced again, repeating.
- **Composite**: minimum blend → black figures on white background.
- **Red tint**: Level TOP (red only) after composite.
- **Two outputs**: `out_color` (red tinted), `out_mono` (red then grayscale).

## Project Layout (in `/project1/desync`)

| Group | Nodes |
|-------|--------|
| **CONTROLS** (left) | `sync_hold`, `desync_duration`, `spread`, `baseline_fps` (constantCHOPs) |
| **DESYNC MOD** | `desync_mod_callbacks` (textDAT), `desync_mod` (scriptCHOP) |
| **TIMELINES** | `timeline0` … `timeline4` (moviefileinTOP) |
| **LEVELS** | `level0` … `level4` (levelTOP) |
| **BACKGROUND** | `background` (constantTOP, white 1920×1080) |
| **COMPOSITE** | `out` (compositeTOP, operand = minimum) |
| **POST** | `red_tint_level` (levelTOP), `post_mono` (monochromeTOP) |

**Outputs** (in `/project1`): `out_color`, `out_mono` (nullTOP).

## Control Parameters

| Node | Channel | Default | Meaning |
|------|---------|---------|---------|
| `sync_hold` | sec | 3 | Seconds of full sync at start and end of cycle |
| `desync_duration` | sec | 8 | Seconds over which desync ramps up then down |
| `spread` | frames | 200 | Max frame offset for outer timelines at full desync |
| `baseline_fps` | fps | 30 | Playback speed (frames per second) |

## Timeline Index Expression

Each `timeline0`…`timeline4` has:

```
index.expr = int(absTime.seconds * baseline_fps + mult * spread * desync_mod['desync']) % 4585
```

`mult` is `-1, -0.5, 0, 0.5, 1` for timeline0…4. `4585` = frame count of the source video.

## Desync Modulator (`desync_mod_callbacks`)

Script CHOP callback:

- Cycle length = `2 * sync_hold + desync_duration` (e.g. 3+8+3 = 14s).
- In first and last `sync_hold` seconds: `desync = 0` (all timelines in sync).
- In the middle `desync_duration`: triangle 0→1→0, then S-curve (smoothstep) so desync ramps smoothly.

## Red Tint

`red_tint_level` (Level TOP): `highr=1`, `highg=0`, `highb=0` (and lows 0) so the composite (luminance) becomes red-only. No separate “gain” params; Level TOP uses output range.

## Video Source

- Path: `input_videos/runside-megaslow-compressed.mp4` (or full path in repo).
- Frame count: **4585** (hardcoded in index expression; change if you switch clip).

## Recreating From Scratch

1. Open TouchDesigner, create or open a project with `project1`.
2. Run the setup script in Textport:
   ```python
   exec(open('/Users/CONWARD/dev/bas/PyBas3/td_scripts/desync_setup.py').read())
   ```
3. Or use TouchDesigner MCP: create nodes and wire as above; run the same organization script for layout.
4. **Save the .toe** (File → Save). MCP cannot save the file; save manually after changes.

## Files in Repo

| File | Purpose |
|------|---------|
| `PyBas3/td_scripts/ConnerTD/Desync.toe` | TouchDesigner project (save here after editing) |
| `PyBas3/td_scripts/desync_setup.py` | Python script to create/wire Desync network in TD |
| `PyBas3/td_scripts/ConnerTD/DESYNC.md` | This documentation |

## Node Layout (Approximate)

- **Left**: Controls (sync_hold, desync_duration, spread, baseline_fps), then desync_mod_callbacks, desync_mod.
- **Center-left**: timeline0…4 (stacked).
- **Center**: level0…4, background (stacked).
- **Center-right**: composite `out`.
- **Right**: red_tint_level, post_mono; outputs out_color, out_mono in project1.

Layout is applied by the organization script (nodeCenterX/nodeCenterY). Run it again from MCP or Textport if you add nodes or move things.
