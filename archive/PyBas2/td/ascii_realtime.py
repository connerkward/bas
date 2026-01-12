"""
Real-time ASCII Art Generator
Runs every frame to convert video to ASCII text
"""

import numpy as np

# ASCII brightness ramp (short for speed)
ASCII_CHARS = " .:-=+*#%@"

def video_to_ascii(video_top, width=80, height=45):
    """Convert video TOP to ASCII art in real-time"""
    
    # Get TOP
    if isinstance(video_top, str):
        top = op(video_top)
    else:
        top = video_top
    
    if top is None:
        return "ERROR: Video TOP not found"
    
    # Get numpy array (no delay for real-time)
    try:
        img = top.numpyArray(delayed=False)
    except:
        return "ERROR: Could not read video"
    
    if img is None or img.size == 0:
        return "ERROR: Empty video"
    
    h, w, c = img.shape
    
    # Cell sizes
    cell_h = max(1, h // height)
    cell_w = max(1, w // width)
    
    # Build ASCII
    lines = []
    
    for row in range(height):
        chars = []
        y = min(row * cell_h, h - 1)
        
        for col in range(width):
            x = min(col * cell_w, w - 1)
            
            # Sample single pixel (faster than averaging cell)
            pixel = img[y, x, :3]
            gray = (pixel[0] * 0.299 + pixel[1] * 0.587 + pixel[2] * 0.114)
            
            # Boost contrast for depth maps
            gray = min(gray * 1.5, 1.0)
            
            # Map to character
            idx = int(gray * (len(ASCII_CHARS) - 1))
            idx = max(0, min(idx, len(ASCII_CHARS) - 1))
            
            chars.append(ASCII_CHARS[idx])
        
        lines.append(''.join(chars))
    
    return '\n'.join(lines)


def run():
    """
    Run this every frame from Timer CHOP
    Adjust resolution for performance:
    - 80x45 = good detail, ~12fps
    - 60x30 = faster, ~24fps
    - 40x20 = fastest, 60fps
    """
    ascii_art = video_to_ascii('moviefilein4', width=60, height=30)
    
    output = op('ascii_output')
    if output:
        output.text = ascii_art
