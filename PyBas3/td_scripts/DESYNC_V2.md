# Desync Chronophotography Effect - Version 2

## Concept
Multiple chronophotographic timelines of a runner, each with a phase offset that animates between sync (all same frame) and desync (spread apart).

## Project Location
`/project1/DeSyncVersion2` in TouchDesigner

## Timing Cycle
```
[0-3s]     SYNC      - All 5 timelines show same frame
[3-11s]    DESYNC    - Timelines spread apart (S-curve 0→1→0)
[11-14s]   SYNC      - Return to same frame
[repeat]
```

Total cycle = `sync_hold * 2 + desync_duration` = 14 seconds default

## Current Parameters
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `sync_hold` | 3 sec | Duration of sync at start/end |
| `desync_duration` | 8 sec | Duration of spread animation |
| `spread` | 500 frames | Max frame offset at peak desync |
| `baseline_fps` | 30 fps | Playback speed |

## Node Structure

### Control CHOPs (NO WIRES - referenced via `op()` in expressions)
| Node | Channel | Default |
|------|---------|---------|
| `sync_hold` | `sec` | 3 |
| `desync_duration` | `sec` | 8 |
| `spread` | `frames` | 500 |
| `baseline_fps` | `fps` | 30 |

### Animation System
| Node | Type | Purpose |
|------|------|---------|
| `desync_mod` | scriptCHOP | Outputs `desync` channel (0→1→0) |
| `desync_mod_callbacks` | textDAT | Python callback driving desync_mod |

**CRITICAL**: `desync_mod.par.callbacks` must point to `desync_mod_callbacks` textDAT

### Video Chain (x5 timelines)
```
timeline0-4 (moviefileinTOP, index driven by expression)
    ↓
mono0-4 (monochromeTOP) 
    ↓
out (compositeTOP, operand=minimum)
    ↑
background (constantTOP, white 720x1280)
```

## Timeline Index Expression
Each timeline has `par.index.mode = 1` (expression) with:
```python
int(absTime.seconds * op('/project1/DeSyncVersion2/baseline_fps')['fps'] + MULT * op('/project1/DeSyncVersion2/spread')['frames'] * op('/project1/DeSyncVersion2/desync_mod')['desync']) % 4585
```

| Timeline | MULT |
|----------|------|
| timeline0 | -1 |
| timeline1 | -0.5 |
| timeline2 | 0 |
| timeline3 | 0.5 |
| timeline4 | 1 |

## Desync Callback Script (`desync_mod_callbacks`)
```python
def onCook(scriptOp):
    scriptOp.clear()
    t = absTime.seconds
    sh = op('/project1/DeSyncVersion2/sync_hold')
    hold = sh['sec'][0] if sh else 3
    dd = op('/project1/DeSyncVersion2/desync_duration')
    desync_len = dd['sec'][0] if dd else 8
    cycle = hold * 2 + desync_len
    pos = t % cycle
    if pos < hold or pos > (hold + desync_len):
        desync = 0
    else:
        x = (pos - hold) / desync_len
        linear = 1 - abs(2 * x - 1)
        desync = linear * linear * (3 - 2 * linear)
    ch = scriptOp.appendChan('desync')
    ch[0] = desync
```

## Visual Output
- **Minimum blend**: Darkest pixel wins → black figure shows on white
- **White background (720x1280)**: Must match video resolution
- **Monochrome conversion**: Converts color video to grayscale for clean blend
- **Result**: Black silhouette(s) on white, spreading apart during desync phase

## Video Source
`/Users/CONWARD/dev/bas/input_videos/runside-megaslow-compressed.mp4`
- Resolution: 720x1280 (portrait)
- Frames: 4585
- FPS: ~30

## Troubleshooting

### "Only see one timeline"
1. Check if currently in sync zone (pos < 3 or pos > 11)
2. Verify `desync_mod` has 1 channel outputting
3. Verify `desync_mod.par.callbacks` points to textDAT

### CHOPs "not connected"
CHOPs don't need wire connections — they're referenced via `op()` in expressions.

### No visual spread
1. Check `spread` value (higher = more visible)
2. Verify all timeline `index.expr` are set correctly
3. Check video file path is valid

## Debug Script
```python
import td
dm = op('/project1/DeSyncVersion2/desync_mod')
dm.cook(force=True)
d = dm['desync'][0]
pos = td.absTime.seconds % 14
indices = [op(f'/project1/DeSyncVersion2/timeline{i}').par.index.eval() for i in range(5)]
print(f"pos={pos:.1f}, desync={d:.3f}, spread={max(indices)-min(indices)}")
```
