"""
Depth Map Blip Transition
Shows a brief flash of depth map at end of video clip

SETUP:
1. Run this every frame from Timer CHOP → DAT Execute
2. Uses a Switch TOP to toggle between main video and depth map
3. Or uses Composite TOP opacity for blend
"""

# Configuration
MAIN_VIDEO = 'moviefilein4'      # Your main video TOP
DEPTH_VIDEO = 'moviefilein3'     # Your depth map video TOP
SWITCH_TOP = 'switch1'           # Switch TOP to control (or use Composite)
COMPOSITE_TOP = 'composite1'     # If using composite method

BLIP_START_FRAME = 90            # Frame to start blip (video has 96 frames)
BLIP_DURATION = 6                # How many frames the blip lasts
BLIP_INTENSITY = 1.0             # 0-1, how strong the depth map shows


def get_current_frame():
    """Get current playback frame from main video"""
    video_top = op(MAIN_VIDEO)
    if video_top is None:
        return 0
    return int(video_top.par.index)


def control_via_switch():
    """
    Method 1: Use Switch TOP to toggle between videos
    Switch TOP input 0 = main video
    Switch TOP input 1 = depth video
    """
    current_frame = get_current_frame()
    switch = op(SWITCH_TOP)
    
    if switch is None:
        print(f"ERROR: Switch TOP '{SWITCH_TOP}' not found")
        return
    
    # Check if we're in the blip range
    if BLIP_START_FRAME <= current_frame < BLIP_START_FRAME + BLIP_DURATION:
        # Show depth map
        switch.par.index = 1
    else:
        # Show main video
        switch.par.index = 0


def control_via_composite():
    """
    Method 2: Use Composite TOP opacity
    Composite input 0 = main video
    Composite input 1 = depth video (fades in/out)
    """
    current_frame = get_current_frame()
    comp = op(COMPOSITE_TOP)
    
    if comp is None:
        print(f"ERROR: Composite TOP '{COMPOSITE_TOP}' not found")
        return
    
    # Calculate opacity for depth layer
    frames_into_blip = current_frame - BLIP_START_FRAME
    
    if frames_into_blip < 0 or frames_into_blip >= BLIP_DURATION:
        # Outside blip range - no depth visible
        opacity = 0.0
    else:
        # Inside blip range
        # Optional: Fade in/out within the blip
        progress = frames_into_blip / BLIP_DURATION
        
        # Triangle fade (fade in first half, fade out second half)
        if progress < 0.5:
            opacity = (progress * 2.0) * BLIP_INTENSITY  # Fade in
        else:
            opacity = ((1.0 - progress) * 2.0) * BLIP_INTENSITY  # Fade out
    
    # Apply opacity to depth layer (assuming it's on operand 1)
    # Adjust based on your composite setup
    comp.par.value0 = opacity  # Or whatever parameter controls blend


def control_via_level():
    """
    Method 3: Control a Level TOP's opacity that sits over the video
    Most flexible - put depth through a Level TOP
    """
    current_frame = get_current_frame()
    level = op('level_depth')
    
    if level is None:
        return
    
    frames_into_blip = current_frame - BLIP_START_FRAME
    
    if frames_into_blip < 0 or frames_into_blip >= BLIP_DURATION:
        opacity = 0.0
    else:
        progress = frames_into_blip / BLIP_DURATION
        # Quick flash: peak in middle
        opacity = 1.0 - abs(progress * 2.0 - 1.0)
        opacity *= BLIP_INTENSITY
    
    level.par.opacity = opacity


def run():
    """
    Main function - choose your method:
    Uncomment the one you want to use
    """
    
    # METHOD 1: Hard switch (instant cut)
    # control_via_switch()
    
    # METHOD 2: Composite blend (smooth fade)
    control_via_composite()
    
    # METHOD 3: Level TOP opacity
    # control_via_level()


# Debug: print current state
def debug():
    frame = get_current_frame()
    print(f"Frame: {frame}, In blip range: {BLIP_START_FRAME <= frame < BLIP_START_FRAME + BLIP_DURATION}")
