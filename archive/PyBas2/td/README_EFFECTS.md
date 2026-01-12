# TouchDesigner Depth Effects

Dithering and depth banding effects for TouchDesigner using pre-rendered depth maps.

## Files

- `bayer_dither.glsl` - Ordered dithering using 4x4 Bayer matrix
- `atkinson_dither.glsl` - Simplified Atkinson error diffusion
- `depth_banding.glsl` - Wavy horizontal line effect based on depth
- `setup_effects.py` - Automated network builder

## Quick Start

### Automatic Setup

1. Open TouchDesigner
2. Create a **Text DAT**
3. Copy contents of `setup_effects.py` into the Text DAT
4. Edit the paths at top of script:
   ```python
   BASE_PATH = "/path/to/your/td/folder"
   DEPTH_VIDEO_PATH = "/path/to/depth_maps.mp4"
   ```
5. Right-click Text DAT → **Run Script**
6. Follow the printed instructions to paste shaders

### Manual Setup

#### 1. Input Chain
```
Movie File In → Luma TOP
```
- **Movie File In**: Load your depth map video
- **Luma TOP**: Convert to grayscale (if not already)

#### 2. Bayer Dithering
```
Luma → GLSL TOP (Bayer) → Null (preview)
```

**GLSL TOP Setup:**
1. Create `glslTOP`
2. Connect input to Luma
3. Paste `bayer_dither.glsl` into **Pixel Shader**
4. Add uniforms (Uniform Name / Type / Default):
   - `threshold` / float / 0.5
   - `scale` / float / 1.0

**Parameters:**
- `threshold`: 0-1, controls dither intensity
- `scale`: 1-4, scales the Bayer pattern

#### 3. Atkinson Dithering
```
Luma → GLSL TOP (Atkinson) → Null (preview)
```

**GLSL TOP Setup:**
1. Create `glslTOP`
2. Connect input to Luma
3. Paste `atkinson_dither.glsl` into **Pixel Shader**
4. Add uniform:
   - `threshold` / float / 0.5

**Note:** This is a simplified shader approximation. True Atkinson requires sequential pixel processing.

#### 4. Depth Banding
```
Luma → GLSL TOP (Depth Banding) → Null (preview)
```

**GLSL TOP Setup:**
1. Create `glslTOP`
2. Connect input to Luma
3. Paste `depth_banding.glsl` into **Pixel Shader**
4. Add uniforms:
   - `lineSpacing` / float / 10.0
   - `waveAmplitude` / float / 3.0
   - `waveFrequency` / float / 0.1
   - `noiseAmount` / float / 0.1
   - `depthCutoff` / float / 0.04

**Parameters:**
- `lineSpacing`: 2-20, distance between lines (affected by depth)
- `waveAmplitude`: 0-10, wave height
- `waveFrequency`: 0-1, wave oscillation speed
- `noiseAmount`: 0-1, scattered noise intensity
- `depthCutoff`: 0-1, filters dark/close areas (0.04 = 4%)

## Generating Depth Maps

Use the Python pipeline to generate depth videos first:

```bash
cd /Users/CONWARD/dev/bas/PyBas2

# Run depth extraction
python depth_blend_video.py runside.mp4 \
  --output-dir runside_blend_output \
  --skip-chronophotography
```

This creates:
- `runside_blend_output/depth_maps/` - Frame sequence for TD input
- `runside_blend_output/dithered/` - Pre-rendered Floyd-Steinberg dither
- `runside_blend_output/atkinson/` - Pre-rendered Atkinson dither
- `runside_blend_output/depth_banding/` - Pre-rendered banding effect
- `runside_blend_output/videos/` - Compiled videos of all effects

## Available Pre-rendered Assets

The `runside_blend_output/` folder contains:
- **depth_maps/** - Grayscale depth frames
- **dithered/** - Floyd-Steinberg dithered frames
- **atkinson/** - Atkinson dithered frames
- **depth_banding/** - Depth banding effect frames
- **rainbow_trail/** - Rainbow motion trail effect
- **red_overlay/** - Red channel overlay effect
- **frames/** - Original video frames
- **videos/** - All effects as compiled MP4s

## Tips

- **Performance**: GLSL shaders run on GPU, real-time at HD/4K
- **Compositing**: Use Composite TOP to blend dithered output with original video
- **Control**: Map MIDI/OSC to uniform parameters for live tweaking
- **Recording**: Use Movie File Out TOP to render final output

## Advanced: Combining Effects

Create a switch or composite chain:

```
[Bayer OUT] ──┐
              ├── Switch TOP → Composite TOP → Original Video
[Banding OUT]─┘
```

Use `Composite TOP` blend modes (Add, Screen, Multiply) to layer effects.
