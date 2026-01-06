# Normal Lighting / Relighting Tool

Interactive relighting and tinting tool using normal maps and depth maps.

## Features

- **Normal Map Relighting**: Apply directional lighting using normal maps
- **Depth-Based Effects**: Fog and color tinting based on depth maps
- **Interactive CLI**: Real-time preview with keyboard controls
- **Portable Design**: Core processor separated from UI for easy porting to Android/TouchDesigner

## Usage

```bash
python relight.py <image_directory>
```

Example:
```bash
python relight.py images/vida
```

The script will automatically discover:
- Original image (matching pattern "Original")
- Normal maps (files starting with "Normal-")
- Depth maps (files starting with "Depth-")

## Controls

### Map Selection
- `1-9, 0` - Switch normal map (1-10)
- `N/M` - Next/Previous depth map

### Light Direction
- `Arrow Keys` - Adjust light direction (azimuth/elevation)

### Lighting Parameters
- `W/S` - Increase/Decrease light intensity
- `A/D` - Increase/Decrease ambient light
- `E/R` - Increase/Decrease normal map strength
- `I/O/P` - Adjust light color (R/G/B)

### Depth Effects
- `F` - Toggle depth fog
- `T` - Toggle depth tint
- `Z/X/C` - Adjust fog amount/color R/color G
- `V/B/H` - Adjust tint amount/near R/far R

### Other
- `Space` - Save current result
- `ESC` - Quit

## File Structure

Expected file naming:
- `Original-*.png` - Original image
- `Normal-*.png` - Normal maps (RGB, X=Red, Y=Green, Z=Blue)
- `Depth-*.png` - Depth maps (grayscale, brighter = closer)

## Architecture

The code is split into two main components:

1. **RelightProcessor** - Core processing logic (UI-agnostic)
   - Handles normal map loading and conversion
   - Applies lighting calculations
   - Depth-based effects

2. **RelightUI** - CLI interface
   - Keyboard input handling
   - Real-time preview
   - Parameter adjustment

This separation makes it easy to port to:
- **Android**: Replace `RelightUI` with Android UI, keep `RelightProcessor`
- **TouchDesigner**: Use `RelightProcessor.process()` in a Python operator

## Example Output

The script saves results as `relit_<original_name>.png` in the same directory.




