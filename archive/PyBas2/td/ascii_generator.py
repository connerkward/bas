"""
TouchDesigner ASCII Art Generator
Creates ASCII text from video input

SETUP:
1. Create a Text DAT named 'ascii_output'
2. Create a new Text DAT with this script
3. Set script to run on frame update or timer
4. Reference your video TOP in the script

USAGE:
me.run()  # Call this to generate ASCII
"""

import numpy as np

# ASCII character ramp (dark to light)
# ASCII_CHARS = " .:-=+*#%@"
ASCII_CHARS = " .'`^\",:;Il!i><~+_-?][}{1)(|\\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$"

def video_to_ascii(video_top, width=80, height=40):
    """
    Convert a video TOP to ASCII art
    
    Args:
        video_top: TD operator path or op reference (e.g., op('moviefilein1'))
        width: ASCII output width in characters
        height: ASCII output height in characters
    
    Returns:
        String of ASCII art
    """
    # Get the TOP
    if isinstance(video_top, str):
        top = op(video_top)
    else:
        top = video_top
    
    if top is None:
        return "ERROR: Video TOP not found"
    
    # Get numpy array from TOP
    # downloadNumpyArray returns RGBA as float 0-1
    try:
        img_array = top.numpyArray(delayed=False)
    except:
        return "ERROR: Could not get numpy array from TOP"
    
    if img_array is None or img_array.size == 0:
        return "ERROR: Empty image array"
    
    # img_array shape is (height, width, channels) with values 0-1
    h, w, c = img_array.shape
    
    # Calculate step sizes
    cell_h = h // height
    cell_w = w // width
    
    if cell_h == 0 or cell_w == 0:
        return "ERROR: Output resolution too large for input"
    
    # Build ASCII string
    ascii_lines = []
    
    for row in range(height):
        line = ""
        for col in range(width):
            # Sample cell
            y_start = row * cell_h
            y_end = min(y_start + cell_h, h)
            x_start = col * cell_w
            x_end = min(x_start + cell_w, w)
            
            # Get cell region
            cell = img_array[y_start:y_end, x_start:x_end, :]
            
            # Convert to grayscale (average RGB, ignore alpha)
            gray = np.mean(cell[:, :, :3])
            
            # Normalize brightness: stretch contrast to use full range
            # This helps with dark images like depth maps
            gray = np.clip(gray * 1.5, 0, 1)  # Boost brightness
            
            # Map brightness to ASCII character
            char_index = int(gray * (len(ASCII_CHARS) - 1))
            char_index = max(0, min(char_index, len(ASCII_CHARS) - 1))
            line += ASCII_CHARS[char_index]
        
        ascii_lines.append(line)
    
    return "\n".join(ascii_lines)


def generate_ascii(video_top_path='moviefilein1', output_dat_path='ascii_output', 
                   width=80, height=40):
    """
    Generate ASCII and write to a Text DAT
    
    Args:
        video_top_path: Path to video TOP (string)
        output_dat_path: Path to output Text DAT (string)
        width: ASCII width in characters
        height: ASCII height in characters
    """
    # Generate ASCII
    ascii_art = video_to_ascii(video_top_path, width, height)
    
    # Write to Text DAT
    output_dat = op(output_dat_path)
    if output_dat is None:
        print(f"ERROR: Text DAT '{output_dat_path}' not found")
        return
    
    output_dat.text = ascii_art
    

# ========== USAGE EXAMPLES ==========

# Example 1: Manual call
# generate_ascii('moviefilein1', 'ascii_output', width=80, height=40)

# Example 2: Auto-run every frame (put in a DAT Execute or Timer CHOP)
# op('script1').run()

# Example 3: Custom configuration
def run_custom():
    """Call this from a button or timer"""
    generate_ascii(
        video_top_path='moviefilein1',  # Your video TOP
        output_dat_path='ascii_output',  # Your output Text DAT
        width=100,   # Wider output
        height=60    # Taller output
    )

# Default run function
def run():
    """Default run - adjust paths as needed"""
    generate_ascii('moviefilein1', 'ascii_output', width=80, height=40)
