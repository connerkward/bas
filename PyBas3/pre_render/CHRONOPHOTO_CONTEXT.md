# Chronophoto Generation Context

## What We're Doing

Generating chronophotos - ghostly composites of raw video frames that create long-exposure trail effects. These are NOT depth map composites, but direct composites of the actual video frames themselves.

## Implementation

### Location
- **Script:** `PyBas3/pre_render/depth_blend_video.py`
- **Output:** `{output_dir}/chronophoto/chronophoto_{mode}.png`
- **Integration:** Runs as a separate pass after depth maps and raw frames are loaded

### Blend Modes
Three blend modes are generated in parallel:

1. **`long_exposure`**: Normalized additive blend - averages all frames to create continuous slug/trail effect without whiteout
2. **`hero_ghost`**: One hero frame at 70% opacity with ghost underlay (averaged frames) at 30% opacity
3. **`lighten_add`**: Hybrid of lighten (max) and normalized add - 70% lighten, 30% add

### Process Flow
1. Extract frames from video (or reuse existing frames if found)
2. Generate depth maps (if `depth` effect enabled)
3. Load raw frames and convert to grayscale
4. **Chronophoto pass** (if `chronophoto` effect enabled):
   - Convert all frames to grayscale
   - Process each blend mode in parallel using ThreadPoolExecutor
   - Generate ghostly composite using all frames
   - Save as RGB PNG files

### Frame Reuse Optimization
- Added check in `extract_frames()` to detect existing frames
- If frames found in output directory, skips extraction and reuses them
- Logs: "Found {N} existing frames, skipping extraction"

## What We Tried

1. **Initial approach**: Created chronophoto variations script with different frame counts (3, 5, 7, 10) and selection strategies (sequential, most different, high variance)
2. **Depth map composites**: Initially tried compositing depth maps onto raw frames, but user wanted raw frame composites only
3. **Pipeline integration**: Added as separate pass that runs after prerequisite passmes (depth maps, frame loading)
4. **Parallelization**: Process blend modes in parallel for efficiency
5. **Full frame usage**: For long_exposure mode, use ALL frames to get continuous trail effect (not just subset)

## Current Status

- ✅ Chronophoto pass integrated into pipeline
- ✅ Three blend modes implemented (long_exposure, hero_ghost, lighten_add)
- ✅ Parallel processing by blend mode
- ✅ Frame reuse optimization added
- ✅ Implementation complete

**Note:** For active tasks, see `agents/SESSION.md`

## Output Files

For video `runside-megaslow-compressed.mp4`:
- `outputs/runside-megaslow-compressed_blend_output/chronophoto/chronophoto_long_exposure.png`
- `outputs/runside-megaslow-compressed_blend_output/chronophoto/chronophoto_hero_ghost.png`
- `outputs/runside-megaslow-compressed_blend_output/chronophoto/chronophoto_lighten_add.png`
