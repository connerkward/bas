"""
TouchDesigner Network Setup for Dithering & Depth Banding Effects

USAGE:
1. Open TouchDesigner
2. Create a Text DAT
3. Paste this script into the Text DAT
4. Right-click the Text DAT → Run Script

This will create a network with:
- Movie file input (for depth map video)
- Bayer dithering
- Atkinson dithering (simplified)
- Depth banding
- Preview outputs
"""

# Configuration
BASE_PATH = "/Users/CONWARD/dev/bas/PyBas2/td"
DEPTH_VIDEO_PATH = "/Users/CONWARD/dev/bas/PyBas2/runside_blend_output/videos/depth.mp4"
ORIGINAL_VIDEO_PATH = "/Users/CONWARD/dev/bas/PyBas2/runside.mp4"

# Pre-rendered effect folders (alternative to real-time generation)
DEPTH_MAPS_DIR = "/Users/CONWARD/dev/bas/PyBas2/runside_blend_output/depth_maps"
DITHERED_DIR = "/Users/CONWARD/dev/bas/PyBas2/runside_blend_output/dithered"
ATKINSON_DIR = "/Users/CONWARD/dev/bas/PyBas2/runside_blend_output/atkinson"
BANDING_DIR = "/Users/CONWARD/dev/bas/PyBas2/runside_blend_output/depth_banding"
RAINBOW_DIR = "/Users/CONWARD/dev/bas/PyBas2/runside_blend_output/rainbow_trail"

# Get or create container
container = op('/project1')
if container is None:
    container = root

# Clean up existing nodes (optional - comment out if you want to keep existing work)
# for child in container.children:
#     if child.name.startswith('effect_'):
#         child.destroy()

print("Creating effect network...")
print(f"Container: {container.path}")

try:
    # ========== INPUT ==========
    # Original video
    print("Creating original video input...")
    originalMovie = container.create(moviefileinTOP, 'effect_original_video')
    originalMovie.par.file = ORIGINAL_VIDEO_PATH
    originalMovie.par.play = 1
    originalMovie.par.playmode = 'loop'
    originalMovie.nodeX = -500
    originalMovie.nodeY = 0
    print(f"  ✓ Created {originalMovie.name}")

    # Depth map video (for real-time shader effects)
    print("Creating depth map input...")
    movieIn = container.create(moviefileinTOP, 'effect_depth_input')
    movieIn.par.file = f"{DEPTH_MAPS_DIR}/frame_0000.png"  # Image sequence
    movieIn.par.play = 1
    movieIn.par.playmode = 'loop'
    movieIn.nodeX = 0
    movieIn.nodeY = 0
    print(f"  ✓ Created {movieIn.name}")

    # Pre-rendered dithered (optional - for comparison)
    print("Creating pre-rendered comparison input...")
    ditheredMovie = container.create(moviefileinTOP, 'effect_prerendered_dither')
    ditheredMovie.par.file = f"{DITHERED_DIR}/frame_0000.png"
    ditheredMovie.par.play = 1
    ditheredMovie.par.playmode = 'loop'
    ditheredMovie.nodeX = 500
    ditheredMovie.nodeY = 0
    print(f"  ✓ Created {ditheredMovie.name}")

    # Convert to grayscale if needed
    print("Creating luma converter...")
    lumaNode = container.create(lumaTOP, 'effect_luma')
    lumaNode.par.colorspace = 'rec709'
    lumaNode.nodeX = 0
    lumaNode.nodeY = -150
    lumaNode.inputConnectors[0].connect(movieIn)
    print(f"  ✓ Created {lumaNode.name}")

except Exception as e:
    print(f"❌ ERROR creating inputs: {e}")
    import traceback
    traceback.print_exc()
    raise

try:
    # ========== BAYER DITHERING ==========
    print("Creating Bayer dithering GLSL...")
    bayerGLSL = container.create(glslTOP, 'effect_bayer_dither')
    bayerGLSL.nodeX = -300
    bayerGLSL.nodeY = -300
    bayerGLSL.inputConnectors[0].connect(lumaNode)
    bayerGLSL.par.outputresolution = 'usetexture'
    print(f"  ✓ Created {bayerGLSL.name}")
    print(f"     → Shader instructions:")
    print(f"       1. Copy contents of {BASE_PATH}/bayer_dither.glsl")
    print(f"       2. Paste into the Pixel Shader parameter")
    print(f"       3. Add Uniform: 'threshold' (float, default 0.5)")
    print(f"       4. Add Uniform: 'scale' (float, default 1.0)")

    # ========== ATKINSON DITHERING ==========
    print("Creating Atkinson dithering GLSL...")
    atkinsonGLSL = container.create(glslTOP, 'effect_atkinson_dither')
    atkinsonGLSL.nodeX = 0
    atkinsonGLSL.nodeY = -300
    atkinsonGLSL.inputConnectors[0].connect(lumaNode)
    print(f"  ✓ Created {atkinsonGLSL.name}")
    print(f"     → Shader instructions:")
    print(f"       1. Copy contents of {BASE_PATH}/atkinson_dither.glsl")
    print(f"       2. Paste into the Pixel Shader parameter")
    print(f"       3. Add Uniform: 'threshold' (float, default 0.5)")

    # ========== DEPTH BANDING ==========
    print("Creating depth banding GLSL...")
    bandingGLSL = container.create(glslTOP, 'effect_depth_banding')
    bandingGLSL.nodeX = 300
    bandingGLSL.nodeY = -300
    bandingGLSL.inputConnectors[0].connect(lumaNode)
    print(f"  ✓ Created {bandingGLSL.name}")
    print(f"     → Shader instructions:")
    print(f"       1. Copy contents of {BASE_PATH}/depth_banding.glsl")
    print(f"       2. Paste into the Pixel Shader parameter")
    print(f"       3. Add Uniforms:")
    print(f"          - lineSpacing (float, default 10)")
    print(f"          - waveAmplitude (float, default 3)")
    print(f"          - waveFrequency (float, default 0.1)")
    print(f"          - noiseAmount (float, default 0.1)")
    print(f"          - depthCutoff (float, default 0.04)")

    # ========== OUTPUT PREVIEWS ==========
    print("Creating output nodes...")
    # Bayer preview
    bayerNull = container.create(nullTOP, 'effect_bayer_OUT')
    bayerNull.nodeX = -300
    bayerNull.nodeY = -450
    bayerNull.inputConnectors[0].connect(bayerGLSL)
    print(f"  ✓ Created {bayerNull.name}")

    # Atkinson preview
    atkinsonNull = container.create(nullTOP, 'effect_atkinson_OUT')
    atkinsonNull.nodeX = 0
    atkinsonNull.nodeY = -450
    atkinsonNull.inputConnectors[0].connect(atkinsonGLSL)
    print(f"  ✓ Created {atkinsonNull.name}")

    # Banding preview
    bandingNull = container.create(nullTOP, 'effect_banding_OUT')
    bandingNull.nodeX = 300
    bandingNull.nodeY = -450
    bandingNull.inputConnectors[0].connect(bandingGLSL)
    print(f"  ✓ Created {bandingNull.name}")

except Exception as e:
    print(f"❌ ERROR creating shader nodes: {e}")
    import traceback
    traceback.print_exc()
    raise

print("\n✅ Network created!")
print("\n📁 PATHS CONFIGURED:")
print(f"   Original: {ORIGINAL_VIDEO_PATH}")
print(f"   Depth Maps: {DEPTH_MAPS_DIR}")
print(f"   Pre-rendered Dither: {DITHERED_DIR}")
print(f"   Pre-rendered Atkinson: {ATKINSON_DIR}")
print(f"   Pre-rendered Banding: {BANDING_DIR}")
print("\nNEXT STEPS:")
print("1. Copy shader code into each GLSL TOP's Pixel Shader parameter")
print("2. Add the custom uniforms listed above to each GLSL TOP")
print("3. Adjust uniform values in real-time to tweak effects")
print("\nCOMPARISON MODE:")
print("  - Real-time shader outputs vs pre-rendered Python outputs")
print("  - Use Switch TOP to toggle between them")
print("\nTIP: Right-click any output NULL → 'View...' to see preview")
