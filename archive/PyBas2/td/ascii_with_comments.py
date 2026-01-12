"""
ASCII Art using Strava Comments as characters

Instead of ". : # @", uses actual Strava comments to form the ASCII art
"""

import numpy as np
import random

# Strava comments sorted by length (short = dark, long = bright)
COMMENTS_SHORT = [
    "💪", "🔥", "🐐", "How", "Stop", "Nice", "PR!",
]

COMMENTS_MEDIUM = [
    "Beast!", "Strong!", "Legend", "Killed it", "Crushing",
]

COMMENTS_LONG = [
    "Great pace!", "Keep it up!", "Looking strong", "Making it look easy",
]

COMMENTS_VERYLONG = [
    "You're an animal!", "Built different", "Legs of steel",
    "That elevation gain 📈", "Save some for the rest of us",
]

# Combine into brightness levels
BRIGHTNESS_LEVELS = [
    COMMENTS_SHORT,
    COMMENTS_MEDIUM, 
    COMMENTS_LONG,
    COMMENTS_VERYLONG,
]


def video_to_ascii_comments(video_top, width=60, height=30):
    """
    Convert video to ASCII art using Strava comments as characters
    
    Args:
        video_top: TD operator (e.g., op('moviefilein1'))
        width: ASCII output width in "characters" (actually comment blocks)
        height: ASCII output height
    
    Returns:
        String of ASCII art made from comments
    """
    # Get the TOP
    if isinstance(video_top, str):
        top = op(video_top)
    else:
        top = video_top
    
    if top is None:
        return "ERROR: Video TOP not found"
    
    # Get numpy array
    try:
        img_array = top.numpyArray(delayed=False)
    except:
        return "ERROR: Could not get numpy array"
    
    if img_array is None or img_array.size == 0:
        return "ERROR: Empty image"
    
    h, w, c = img_array.shape
    
    # Calculate cell sizes
    cell_h = h // height
    cell_w = w // width
    
    if cell_h == 0 or cell_w == 0:
        return "ERROR: Resolution too high"
    
    # Build ASCII using comments
    ascii_lines = []
    
    for row in range(height):
        line_parts = []
        for col in range(width):
            # Sample cell
            y_start = row * cell_h
            y_end = min(y_start + cell_h, h)
            x_start = col * cell_w
            x_end = min(x_start + cell_w, w)
            
            # Get cell
            cell = img_array[y_start:y_end, x_start:x_end, :]
            
            # Brightness
            gray = np.mean(cell[:, :, :3])
            gray = np.clip(gray * 1.5, 0, 1)  # Boost
            
            # Pick comment based on brightness
            level_index = int(gray * (len(BRIGHTNESS_LEVELS) - 1))
            level_index = max(0, min(level_index, len(BRIGHTNESS_LEVELS) - 1))
            
            # Random comment from that brightness level
            comment = random.choice(BRIGHTNESS_LEVELS[level_index])
            line_parts.append(comment)
        
        # Join with spaces
        ascii_lines.append(" ".join(line_parts))
    
    return "\n".join(ascii_lines)


def generate(video_top_path='moviefilein4', output_dat_path='ascii_output', 
             width=40, height=20):
    """
    Generate ASCII art from video using Strava comments
    
    Args:
        video_top_path: Path to video TOP
        output_dat_path: Path to output Text DAT
        width: Width in comment blocks
        height: Height in comment blocks
    """
    ascii_art = video_to_ascii_comments(video_top_path, width, height)
    
    output_dat = op(output_dat_path)
    if output_dat is None:
        print(f"ERROR: Text DAT '{output_dat_path}' not found")
        return
    
    output_dat.text = ascii_art
    print(f"Generated ASCII art from {video_top_path}")


def run():
    """Default run - call this from timer"""
    generate('moviefilein4', 'ascii_output', width=40, height=20)
