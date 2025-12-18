#!/usr/bin/env python3
"""
Extract frames from video, run Depth Anything V2, overlay depth maps with blend modes.
"""

import argparse
import os
import cv2
import numpy as np
import torch
from PIL import Image
from transformers import pipeline


def chroma_key_green(frame: np.ndarray, soft_edge: bool = True) -> tuple[np.ndarray, np.ndarray]:
    """Remove green screen, return (frame with black bg, mask)."""
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    # Tighter green range for softer edges
    lower_green = np.array([35, 40, 40])
    upper_green = np.array([85, 255, 255])
    mask = cv2.inRange(hsv, lower_green, upper_green)
    
    if soft_edge:
        # Softer edge processing - less aggressive dilation
        kernel = np.ones((3, 3), np.uint8)
        mask = cv2.dilate(mask, kernel, iterations=1)
        # Blur for soft edge
        mask = cv2.GaussianBlur(mask, (5, 5), 0)
    else:
        kernel = np.ones((5, 5), np.uint8)
        mask = cv2.dilate(mask, kernel, iterations=2)
    
    # Invert mask (foreground = white)
    mask_inv = cv2.bitwise_not(mask)
    # Apply mask - black background
    result = cv2.bitwise_and(frame, frame, mask=mask_inv)
    return result, mask_inv


def extract_frames(video_path: str, num_frames: int, output_dir: str, target_fps: float = None) -> list[str]:
    """Extract evenly spaced frames from video."""
    cap = cv2.VideoCapture(video_path)
    if not cap.isOpened():
        raise ValueError(f"Cannot open video: {video_path}")
    
    video_fps = cap.get(cv2.CAP_PROP_FPS)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    duration = total_frames / video_fps if video_fps > 0 else 0
    
    # Calculate num_frames based on target_fps if provided
    if target_fps is not None:
        num_frames = int(duration * target_fps)
        print(f"Video duration: {duration:.2f}s, extracting at {target_fps} fps = {num_frames} frames")
    
    frame_indices = np.linspace(0, total_frames - 1, num_frames, dtype=int)
    
    os.makedirs(output_dir, exist_ok=True)
    frame_paths = []
    
    for idx, frame_num in enumerate(frame_indices):
        cap.set(cv2.CAP_PROP_POS_FRAMES, frame_num)
        ret, frame = cap.read()
        if ret:
            # Apply chroma key to remove green screen
            frame, _ = chroma_key_green(frame)
            frame_path = os.path.join(output_dir, f"frame_{idx:04d}.png")
            cv2.imwrite(frame_path, frame)
            frame_paths.append(frame_path)
    
    cap.release()
    print(f"Extracted {len(frame_paths)} frames (green screen removed)")
    return frame_paths


def blend_lighten(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    """Lighten - take max of each pixel (like multiple exposure)."""
    return np.maximum(base, overlay)


def blend_add(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    """Additive blend - accumulate light."""
    return np.clip(base.astype(np.float32) + overlay.astype(np.float32), 0, 255).astype(np.uint8)


def blend_screen(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    """Screen blend - like projecting multiple slides."""
    base_f = base.astype(np.float32) / 255.0
    overlay_f = overlay.astype(np.float32) / 255.0
    result = 1 - (1 - base_f) * (1 - overlay_f)
    return (result * 255).astype(np.uint8)


def create_chronophotography(depth_images: list[np.ndarray], mode: str = "lighten") -> np.ndarray:
    """
    Create Marey-style chronophotography from multiple depth maps.
    All frames are composited together to show motion over time.
    """
    if len(depth_images) == 0:
        return None
    
    # Start with first frame
    result = depth_images[0].astype(np.float32)
    
    if mode == "lighten":
        # Lighten: max of all frames - preserves brightest (closest) objects
        for img in depth_images[1:]:
            result = np.maximum(result, img.astype(np.float32))
        return result.astype(np.uint8)
    
    elif mode == "add":
        # Additive: sum all frames, then normalize
        for img in depth_images[1:]:
            result += img.astype(np.float32)
        # Normalize to 0-255
        result = (result - result.min()) / (result.max() - result.min() + 1e-6) * 255
        return result.astype(np.uint8)
    
    elif mode == "screen":
        # Screen: like overlapping projections
        result = result / 255.0
        for img in depth_images[1:]:
            overlay = img.astype(np.float32) / 255.0
            result = 1 - (1 - result) * (1 - overlay)
        return (result * 255).astype(np.uint8)
    
    elif mode == "average":
        # Simple average of all frames
        for img in depth_images[1:]:
            result += img.astype(np.float32)
        result /= len(depth_images)
        return result.astype(np.uint8)
    
    elif mode == "darken":
        # Darken: min of all frames - preserves darkest (furthest) objects
        for img in depth_images[1:]:
            result = np.minimum(result, img.astype(np.float32))
        return result.astype(np.uint8)
    
    elif mode == "lighten_add":
        # Hybrid: lighten first, then add with reduced weight
        # Start with lighten (max of all frames)
        lighten_result = depth_images[0].astype(np.float32)
        for img in depth_images[1:]:
            lighten_result = np.maximum(lighten_result, img.astype(np.float32))
        
        # Then add all frames together
        add_result = depth_images[0].astype(np.float32)
        for img in depth_images[1:]:
            add_result += img.astype(np.float32)
        # Normalize additive result
        add_result = (add_result - add_result.min()) / (add_result.max() - add_result.min() + 1e-6) * 255
        
        # Blend: 70% lighten, 30% add
        hybrid = lighten_result * 0.7 + add_result * 0.3
        return np.clip(hybrid, 0, 255).astype(np.uint8)
    
    else:
        return depth_images[0]


BLEND_MODES = ["lighten", "add", "screen", "average", "darken", "lighten_add"]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("video_path", type=str)
    parser.add_argument("--num-frames", type=int, default=10)
    parser.add_argument("--target-fps", type=float, default=None, help="Extract frames at this fps (e.g., 12 for stop motion)")
    parser.add_argument("--model", type=str, default="depth-anything/Depth-Anything-V2-Small-hf")
    parser.add_argument("--device", type=str, default="mps" if torch.backends.mps.is_available() else ("cuda" if torch.cuda.is_available() else "cpu"))
    parser.add_argument("--blend-modes", type=str, nargs="+", default=["lighten", "add", "lighten_add"],
                        choices=BLEND_MODES)
    parser.add_argument("--alpha", type=float, default=0.5,
                        help="Alpha blend for raw frame overlay (0.0 = depth only, 1.0 = raw only)")
    parser.add_argument("--output-dir", type=str, default=None)
    
    args = parser.parse_args()
    
    # Generate output dir from video filename if not specified
    if args.output_dir is None:
        video_basename = os.path.splitext(os.path.basename(args.video_path))[0]
        args.output_dir = f"{video_basename}_blend_output"
    
    # Extract frames
    frames_dir = os.path.join(args.output_dir, "frames")
    if args.target_fps:
        print(f"Extracting frames from {args.video_path} at {args.target_fps} fps...")
    else:
        print(f"Extracting {args.num_frames} frames from {args.video_path}...")
    frame_paths = extract_frames(args.video_path, args.num_frames, frames_dir, args.target_fps)
    
    # Load Depth Anything V2 pipeline
    print(f"Loading Depth Anything V2 ({args.model})...")
    pipe = pipeline(task="depth-estimation", model=args.model, device=args.device)
    
    # Create output dirs
    depth_maps_dir = os.path.join(args.output_dir, "depth_maps")
    os.makedirs(depth_maps_dir, exist_ok=True)
    for mode in args.blend_modes:
        os.makedirs(os.path.join(args.output_dir, mode), exist_ok=True)
    
    # Process each frame
    print("Running depth estimation...")
    depth_images = []
    for idx, frame_path in enumerate(frame_paths):
        # Load image
        image = Image.open(frame_path)
        frame_bgr = cv2.imread(frame_path)
        
        # Run depth estimation
        result = pipe(image)
        depth_map = result["depth"]  # PIL Image
        
        # Convert to numpy grayscale
        depth_array = np.array(depth_map)
        
        # Create mask from chroma-keyed frame - use original video frame to get proper mask
        # Re-read original and get chroma key mask (not the already-keyed frame)
        cap_temp = cv2.VideoCapture(args.video_path)
        total_video_frames = int(cap_temp.get(cv2.CAP_PROP_FRAME_COUNT))
        frame_indices_temp = np.linspace(0, total_video_frames - 1, len(frame_paths), dtype=int)
        cap_temp.set(cv2.CAP_PROP_POS_FRAMES, frame_indices_temp[idx])
        _, orig_frame = cap_temp.read()
        cap_temp.release()
        _, fg_mask = chroma_key_green(orig_frame)
        # Resize mask to match depth map if needed
        if fg_mask.shape != depth_array.shape:
            fg_mask = cv2.resize(fg_mask, (depth_array.shape[1], depth_array.shape[0]))
        
        # Normalize to 0-255
        depth_min = depth_array.min()
        depth_max = depth_array.max()
        if depth_max > depth_min:
            depth_norm = (depth_array - depth_min) / (depth_max - depth_min)
        else:
            depth_norm = np.zeros_like(depth_array, dtype=np.float32)
        
        depth_gray = (depth_norm * 255).astype(np.uint8)
        
        # Apply foreground mask to depth (black out background)
        depth_gray = cv2.bitwise_and(depth_gray, depth_gray, mask=fg_mask)
        depth_images.append(depth_gray)
        
        # Save individual depth map
        path = os.path.join(depth_maps_dir, f"depth_{idx:04d}.png")
        Image.fromarray(depth_gray, mode='L').save(path)
        print(f"  Frame {idx + 1}/{len(frame_paths)}")
    
    print(f"Saved {len(depth_images)} depth maps")
    
    # Create depth map chronophotography composite
    depth_chrono = create_chronophotography(depth_images, "lighten_add")
    path = os.path.join(depth_maps_dir, "chronophoto.png")
    Image.fromarray(depth_chrono, mode='L').save(path)
    print("Created depth maps chronophoto")
    
    # Resize all depth images to same size
    target_shape = depth_images[0].shape
    resized_depths = []
    for img in depth_images:
        if img.shape != target_shape:
            img = cv2.resize(img, (target_shape[1], target_shape[0]))
        resized_depths.append(img)
    
    # Load original frames for overlay and convert to grayscale
    print("Loading original frames for overlay (converting to B&W)...")
    original_frames = []
    for frame_path in frame_paths:
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        # Convert to grayscale then back to 3-channel for blending
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        frame = cv2.cvtColor(gray, cv2.COLOR_GRAY2BGR)
        # Resize to match depth map size
        if frame.shape[:2] != target_shape:
            frame = cv2.resize(frame, (target_shape[1], target_shape[0]))
        original_frames.append(frame)
    
    # Create dithered versions of frames
    print("Creating dithered frames...")
    dithered_dir = os.path.join(args.output_dir, "dithered")
    os.makedirs(dithered_dir, exist_ok=True)
    dithered_images = []
    for idx, frame_path in enumerate(frame_paths):
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        pil_gray = Image.fromarray(gray, mode='L')
        dithered = pil_gray.convert('1', dither=Image.Dither.FLOYDSTEINBERG)
        dithered.save(os.path.join(dithered_dir, f"frame_{idx:04d}.png"))
        dithered_images.append(np.array(dithered.convert('L')))
    
    # Create Atkinson dithered frames (Obra Dinn style)
    print("Creating Atkinson dithered frames...")
    atkinson_dir = os.path.join(args.output_dir, "atkinson")
    os.makedirs(atkinson_dir, exist_ok=True)
    for idx, frame_path in enumerate(frame_paths):
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY).astype(np.float32)
        h, w = gray.shape
        for y in range(h):
            for x in range(w):
                old_pixel = gray[y, x]
                new_pixel = 255.0 if old_pixel > 127 else 0.0
                gray[y, x] = new_pixel
                error = (old_pixel - new_pixel) / 8.0  # Atkinson divides by 8, not full diffusion
                if x + 1 < w: gray[y, x + 1] += error
                if x + 2 < w: gray[y, x + 2] += error
                if y + 1 < h:
                    if x > 0: gray[y + 1, x - 1] += error
                    gray[y + 1, x] += error
                    if x + 1 < w: gray[y + 1, x + 1] += error
                if y + 2 < h: gray[y + 2, x] += error
        result = np.clip(gray, 0, 255).astype(np.uint8)
        Image.fromarray(result, mode='L').save(os.path.join(atkinson_dir, f"frame_{idx:04d}.png"))
    
    # Create Bayer dithered frames (retro ordered dithering)
    print("Creating Bayer dithered frames...")
    bayer_dir = os.path.join(args.output_dir, "bayer")
    os.makedirs(bayer_dir, exist_ok=True)
    bayer_matrix = np.array([
        [0, 8, 2, 10],
        [12, 4, 14, 6],
        [3, 11, 1, 9],
        [15, 7, 13, 5]
    ], dtype=np.float32) / 16.0 * 255.0
    for idx, frame_path in enumerate(frame_paths):
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY).astype(np.float32)
        h, w = gray.shape
        threshold = np.tile(bayer_matrix, (h // 4 + 1, w // 4 + 1))[:h, :w]
        result = (gray > threshold).astype(np.uint8) * 255
        Image.fromarray(result, mode='L').save(os.path.join(bayer_dir, f"frame_{idx:04d}.png"))
    
    # Create pixelated extract silhouettes (retro 1-bit style)
    print("Creating pixelated extract frames...")
    extract_dir = os.path.join(args.output_dir, "extract")
    os.makedirs(extract_dir, exist_ok=True)
    pixel_scale = 8  # Downscale factor for chunky pixels
    for idx, frame_path in enumerate(frame_paths):
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        h, w = gray.shape
        # Downscale for chunky pixels
        small = cv2.resize(gray, (w // pixel_scale, h // pixel_scale), interpolation=cv2.INTER_AREA)
        # Threshold to 1-bit
        _, binary = cv2.threshold(small, 20, 255, cv2.THRESH_BINARY)
        # Upscale with nearest neighbor for hard pixel edges
        extract = cv2.resize(binary, (w, h), interpolation=cv2.INTER_NEAREST)
        Image.fromarray(extract, mode='L').save(os.path.join(extract_dir, f"frame_{idx:04d}.png"))
    
    # Create low-res pixelart style (downscale then upscale for chunky pixels)
    print("Creating low-res pixel frames...")
    lowres_dir = os.path.join(args.output_dir, "lowres")
    os.makedirs(lowres_dir, exist_ok=True)
    lowres_scale = 16  # More aggressive downscale
    for idx, frame_path in enumerate(frame_paths):
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        h, w = gray.shape
        # Downscale aggressively
        small = cv2.resize(gray, (w // lowres_scale, h // lowres_scale), interpolation=cv2.INTER_AREA)
        # Threshold to 1-bit
        _, binary = cv2.threshold(small, 20, 255, cv2.THRESH_BINARY)
        # Upscale back with nearest neighbor for chunky pixels
        lowres = cv2.resize(binary, (w, h), interpolation=cv2.INTER_NEAREST)
        Image.fromarray(lowres, mode='L').save(os.path.join(lowres_dir, f"frame_{idx:04d}.png"))
    
    # Create red-tinted dither overlay (saturated red/orange on dark base)
    print("Creating red dither overlay frames...")
    red_overlay_dir = os.path.join(args.output_dir, "red_overlay")
    os.makedirs(red_overlay_dir, exist_ok=True)
    for idx, frame_path in enumerate(frame_paths):
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        h, w = frame.shape[:2]
        
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        dark_base = np.clip(gray.astype(np.float32) * 0.3, 0, 255).astype(np.uint8)
        
        if idx < len(dithered_images):
            dith = dithered_images[idx]
            if dith.shape != (h, w):
                dith = cv2.resize(dith, (w, h))
            
            result = np.zeros((h, w, 3), dtype=np.uint8)
            result[:, :, 0] = np.clip(dark_base * 0.4, 0, 60).astype(np.uint8)
            result[:, :, 1] = np.clip(dark_base * 0.2, 0, 40).astype(np.uint8)
            result[:, :, 2] = np.clip(dark_base * 0.3, 0, 50).astype(np.uint8)
            
            dith_mask = dith > 127
            result[:, :, 2] = np.where(dith_mask, 255, result[:, :, 2])
            result[:, :, 1] = np.where(dith_mask, 80, result[:, :, 1])
            result[:, :, 0] = np.where(dith_mask, 20, result[:, :, 0])
            
            Image.fromarray(cv2.cvtColor(result, cv2.COLOR_BGR2RGB)).save(os.path.join(red_overlay_dir, f"frame_{idx:04d}.png"))
    
    # Create rainbow motion trail pass (reference style a)
    print("Creating rainbow motion trail frames...")
    rainbow_dir = os.path.join(args.output_dir, "rainbow_trail")
    os.makedirs(rainbow_dir, exist_ok=True)
    
    for idx, frame_path in enumerate(frame_paths):
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        h, w = frame.shape[:2]
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        
        # Start with dark base
        result = np.zeros((h, w, 3), dtype=np.float32)
        
        # Accumulate previous frames as motion trail with rainbow colors
        trail_frames = max(0, idx - 8)  # Look back 8 frames
        for t_idx in range(trail_frames, idx + 1):
            t_frame = cv2.imread(frame_paths[t_idx])
            if t_frame is None:
                continue
            t_gray = cv2.cvtColor(t_frame, cv2.COLOR_BGR2GRAY).astype(np.float32)
            if t_gray.shape != (h, w):
                t_gray = cv2.resize(t_gray, (w, h))
            
            # Rainbow hue based on time offset
            time_offset = (idx - t_idx) / 8.0
            hue = (time_offset * 0.7) % 1.0  # Cycle through colors
            
            # Convert hue to RGB
            if hue < 1/6:
                r, g, b = 1.0, hue * 6, 0
            elif hue < 2/6:
                r, g, b = 1 - (hue - 1/6) * 6, 1.0, 0
            elif hue < 3/6:
                r, g, b = 0, 1.0, (hue - 2/6) * 6
            elif hue < 4/6:
                r, g, b = 0, 1 - (hue - 3/6) * 6, 1.0
            elif hue < 5/6:
                r, g, b = (hue - 4/6) * 6, 0, 1.0
            else:
                r, g, b = 1.0, 0, 1 - (hue - 5/6) * 6
            
            # Fade older frames
            fade = 1.0 - (time_offset * 0.7)
            intensity = t_gray / 255.0 * fade
            
            # Add colored contribution with soft glow
            blurred = cv2.GaussianBlur(intensity, (15, 15), 0)
            result[:, :, 2] += blurred * r * 180  # R
            result[:, :, 1] += blurred * g * 180  # G
            result[:, :, 0] += blurred * b * 180  # B
        
        # Add current frame as bright highlight
        current_bright = gray.astype(np.float32) / 255.0
        bright_mask = current_bright > 0.6
        result[:, :, 0] = np.where(bright_mask, np.clip(result[:, :, 0] + current_bright * 200, 0, 255), result[:, :, 0])
        result[:, :, 1] = np.where(bright_mask, np.clip(result[:, :, 1] + current_bright * 200, 0, 255), result[:, :, 1])
        result[:, :, 2] = np.where(bright_mask, np.clip(result[:, :, 2] + current_bright * 200, 0, 255), result[:, :, 2])
        
        # Add film grain noise
        noise = np.random.normal(0, 8, (h, w, 3))
        result = np.clip(result + noise, 0, 255).astype(np.uint8)
        
        Image.fromarray(cv2.cvtColor(result, cv2.COLOR_BGR2RGB)).save(os.path.join(rainbow_dir, f"frame_{idx:04d}.png"))
    
    # Create depth-based banding dither pass (reference style b)
    print("Creating depth banding frames...")
    banding_dir = os.path.join(args.output_dir, "depth_banding")
    os.makedirs(banding_dir, exist_ok=True)
    
    for idx, frame_path in enumerate(frame_paths):
        frame = cv2.imread(frame_path)
        if frame is None:
            continue
        h, w = frame.shape[:2]
        
        # Get depth map for this frame
        if idx < len(depth_images):
            depth = depth_images[idx]
            if depth.shape != (h, w):
                depth = cv2.resize(depth, (w, h))
        else:
            depth = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        
        # Create horizontal line pattern based on depth
        result = np.zeros((h, w), dtype=np.uint8)
        
        # Number of bands based on depth value
        for y in range(h):
            for x in range(w):
                d = depth[y, x]
                if d < 10:
                    continue
                # Line spacing inversely proportional to depth (closer = denser lines)
                line_spacing = max(2, int(20 - d / 15))
                # Create wavy lines based on y position and depth
                wave = int(np.sin(y * 0.1 + d * 0.05) * 3)
                if (y + wave) % line_spacing < 2:
                    result[y, x] = 255
        
        # Add some noise/stipple
        noise_mask = np.random.random((h, w)) < (depth.astype(np.float32) / 255.0 * 0.1)
        result = np.where(noise_mask, 255, result).astype(np.uint8)
        
        Image.fromarray(result, mode='L').save(os.path.join(banding_dir, f"frame_{idx:04d}.png"))
    
    # Create chronophotography composites (Marey-style multiple exposure)
    print("Creating chronophotography composites...")
    for mode in args.blend_modes:
        # Create progressive frames showing accumulation
        for idx in range(len(resized_depths)):
            # Composite all frames up to this point
            frames_so_far = resized_depths[:idx + 1]
            depth_result = create_chronophotography(frames_so_far, mode)
            
            # Convert depth to RGB for compositing
            depth_rgb = cv2.cvtColor(depth_result, cv2.COLOR_GRAY2RGB)
            
            # Overlay with current raw frame
            if idx < len(original_frames):
                raw_frame = original_frames[idx]
                # Blend: alpha controls mix (0 = all depth, 1 = all raw)
                blended = cv2.addWeighted(depth_rgb, 1.0 - args.alpha, raw_frame, args.alpha, 0)
            else:
                blended = depth_rgb
            
            # Save progressive frame
            path = os.path.join(args.output_dir, mode, f"frame_{idx:04d}.png")
            Image.fromarray(blended).save(path)
        
        # Create final composite of ALL frames
        depth_result = create_chronophotography(resized_depths, mode)
        depth_rgb = cv2.cvtColor(depth_result, cv2.COLOR_GRAY2RGB)
        
        # Overlay with average of all raw frames
        if original_frames:
            # Average all raw frames
            avg_raw = np.mean([f.astype(np.float32) for f in original_frames], axis=0).astype(np.uint8)
            blended = cv2.addWeighted(depth_rgb, 1.0 - args.alpha, avg_raw, args.alpha, 0)
        else:
            blended = depth_rgb
        
        path = os.path.join(args.output_dir, mode, "chronophoto.png")
        Image.fromarray(blended).save(path)
        print(f"Created {mode} chronophotography")
    
    # Create videos from each pass folder
    print("Creating videos from passes...")
    videos_dir = os.path.join(args.output_dir, "videos")
    os.makedirs(videos_dir, exist_ok=True)
    
    pass_folders = ["frames", "depth_maps", "dithered", "atkinson", "bayer", "extract", "lowres", "red_overlay", "rainbow_trail", "depth_banding"]
    pass_folders.extend(args.blend_modes)
    
    for folder in pass_folders:
        folder_path = os.path.join(args.output_dir, folder)
        if not os.path.exists(folder_path):
            continue
        
        # Get all frame images (not chronophoto)
        frame_files = sorted([f for f in os.listdir(folder_path) if f.startswith("frame_") or f.startswith("depth_")])
        if not frame_files:
            continue
        
        # Read first frame to get dimensions
        first_frame = cv2.imread(os.path.join(folder_path, frame_files[0]))
        if first_frame is None:
            # Try as grayscale
            first_frame = cv2.imread(os.path.join(folder_path, frame_files[0]), cv2.IMREAD_GRAYSCALE)
            if first_frame is None:
                continue
            first_frame = cv2.cvtColor(first_frame, cv2.COLOR_GRAY2BGR)
        
        h, w = first_frame.shape[:2]
        
        # Create video writer
        video_path = os.path.join(videos_dir, f"{folder}.mp4")
        fourcc = cv2.VideoWriter_fourcc(*'mp4v')
        out = cv2.VideoWriter(video_path, fourcc, 12.0, (w, h))
        
        for frame_file in frame_files:
            frame = cv2.imread(os.path.join(folder_path, frame_file))
            if frame is None:
                frame = cv2.imread(os.path.join(folder_path, frame_file), cv2.IMREAD_GRAYSCALE)
                if frame is not None:
                    frame = cv2.cvtColor(frame, cv2.COLOR_GRAY2BGR)
            if frame is not None:
                if frame.shape[:2] != (h, w):
                    frame = cv2.resize(frame, (w, h))
                out.write(frame)
        
        out.release()
        print(f"  Created {folder}.mp4")
    
    print(f"\nDone! Output: {args.output_dir}")


if __name__ == "__main__":
    main()
