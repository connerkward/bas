#!/usr/bin/env python3
"""
Extract frames from video, run Depth Anything V2, overlay depth maps with blend modes.
Supports both green screen and regular videos with auto-detection.
"""

import argparse
import os
import cv2
import numpy as np
import torch
from PIL import Image
from transformers import pipeline
from concurrent.futures import ThreadPoolExecutor, as_completed
import time
from tqdm import tqdm
import json
import pickle


def log(step: str, detail: str = "", frame: int = None, total: int = None):
    """Unified logging with step/frame info."""
    timestamp = time.strftime("%H:%M:%S")
    if frame is not None and total is not None:
        print(f"[{timestamp}] [{step}] ({frame}/{total}) {detail}")
    else:
        print(f"[{timestamp}] [{step}] {detail}")


def create_video_from_folder(folder_path: str, folder_name: str, videos_dir: str, target_fps: float):
    """Create a video from a folder of frames. Returns True if successful."""
    if not os.path.exists(folder_path):
        return False
    
    frame_files = sorted([f for f in os.listdir(folder_path) if f.startswith("frame_") or f.startswith("depth_")])
    if not frame_files:
        return False
    
    first_frame = cv2.imread(os.path.join(folder_path, frame_files[0]))
    if first_frame is None:
        first_frame = cv2.imread(os.path.join(folder_path, frame_files[0]), cv2.IMREAD_GRAYSCALE)
        if first_frame is None:
            return False
        first_frame = cv2.cvtColor(first_frame, cv2.COLOR_GRAY2BGR)
    
    h, w = first_frame.shape[:2]
    video_path = os.path.join(videos_dir, f"{folder_name}.mp4")
    fourcc = cv2.VideoWriter_fourcc(*'mp4v')
    out = cv2.VideoWriter(video_path, fourcc, target_fps, (w, h))
    
    if not out.isOpened():
        log("VIDEO", f"Failed to open video writer for {folder_name}")
        return False
    
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
    log("VIDEO", f"{folder_name}.mp4 done")
    return True


def detect_green_screen(frame: np.ndarray, threshold: float = 0.15) -> bool:
    """Auto-detect if frame has green screen background."""
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    # Green range
    lower_green = np.array([35, 40, 40])
    upper_green = np.array([85, 255, 255])
    mask = cv2.inRange(hsv, lower_green, upper_green)
    green_ratio = np.sum(mask > 0) / mask.size
    return green_ratio > threshold


def analyze_video_for_greenscreen(video_path: str, sample_frames: int = 5) -> tuple[bool, float]:
    """Sample video to detect green screen. Returns (has_greenscreen, green_ratio)."""
    cap = cv2.VideoCapture(video_path)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    indices = np.linspace(0, total_frames - 1, sample_frames, dtype=int)
    
    green_ratios = []
    for idx in indices:
        cap.set(cv2.CAP_PROP_POS_FRAMES, idx)
        ret, frame = cap.read()
        if ret:
            hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
            lower_green = np.array([35, 40, 40])
            upper_green = np.array([85, 255, 255])
            mask = cv2.inRange(hsv, lower_green, upper_green)
            green_ratios.append(np.sum(mask > 0) / mask.size)
    cap.release()
    
    avg_green = np.mean(green_ratios) if green_ratios else 0
    return avg_green > 0.15, avg_green


def chroma_key_green(frame: np.ndarray, params: dict = None) -> tuple[np.ndarray, np.ndarray]:
    """Remove green screen with improved masking. Returns (frame with black bg, mask)."""
    if params is None:
        params = {
            'hue_low': 35, 'hue_high': 85,
            'sat_low': 40, 'sat_high': 255,
            'val_low': 40, 'val_high': 255,
            'erode_iterations': 1,
            'dilate_iterations': 2,
            'blur_size': 5,
            'edge_smooth': 3
        }
    
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    lower_green = np.array([params['hue_low'], params['sat_low'], params['val_low']])
    upper_green = np.array([params['hue_high'], params['sat_high'], params['val_high']])
    mask = cv2.inRange(hsv, lower_green, upper_green)
    
    # Improved mask processing
    # Erode to remove small green specks
    if params['erode_iterations'] > 0:
        kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
        mask = cv2.erode(mask, kernel, iterations=params['erode_iterations'])
    
    # Dilate to fill holes
    if params['dilate_iterations'] > 0:
        kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
        mask = cv2.dilate(mask, kernel, iterations=params['dilate_iterations'])
    
    # Smooth edges with multiple blur passes for better quality
    if params['blur_size'] > 0:
        mask = cv2.GaussianBlur(mask, (params['blur_size'] * 2 + 1, params['blur_size'] * 2 + 1), 0)
        if params['edge_smooth'] > 0:
            # Additional edge smoothing
            mask = cv2.bilateralFilter(mask, params['edge_smooth'] * 2 + 1, 50, 50)
    
    mask_inv = cv2.bitwise_not(mask)
    # Use alpha blending for smoother edges
    mask_inv_f = mask_inv.astype(np.float32) / 255.0
    result = (frame.astype(np.float32) * mask_inv_f[:, :, np.newaxis]).astype(np.uint8)
    return result, mask_inv


def interactive_chroma_tuning(video_path: str, params_file: str = None) -> dict:
    """Interactive window to tune chroma key parameters with video playback. Returns tuned parameters."""
    # Default parameters
    default_params = {
        'hue_low': 35, 'hue_high': 85,
        'sat_low': 40, 'sat_high': 255,
        'val_low': 40, 'val_high': 255,
        'erode_iterations': 1,
        'dilate_iterations': 2,
        'blur_size': 5,
        'edge_smooth': 3
    }
    
    # Load saved parameters if available
    params = default_params.copy()
    if params_file and os.path.exists(params_file):
        try:
            with open(params_file, 'r') as f:
                saved = json.load(f)
                if 'chroma' in saved:
                    params.update(saved['chroma'])
                    log("TUNING", "Loaded saved chroma parameters as defaults")
        except Exception as e:
            log("TUNING", f"Could not load saved params: {e}, using defaults")
    
    # Clamp initial values to valid ranges
    params['hue_low'] = min(params['hue_low'], 179)
    params['hue_high'] = min(params['hue_high'], 179)
    
    def chroma_key_with_bg(frame: np.ndarray, params: dict, bg_color: tuple = (255, 255, 255)) -> np.ndarray:
        """Apply chroma key and composite on colored background."""
        result, mask = chroma_key_green(frame, params)
        # Create white background
        h, w = result.shape[:2]
        bg = np.full((h, w, 3), bg_color, dtype=np.uint8)
        # Use mask to blend: where mask is 0 (background), use white; where mask is 255 (foreground), use result
        mask_f = mask.astype(np.float32) / 255.0
        result_bg = (result.astype(np.float32) * mask_f[:, :, np.newaxis] + 
                     bg.astype(np.float32) * (1 - mask_f[:, :, np.newaxis])).astype(np.uint8)
        return result_bg
    
    use_manual = [True]  # Use list to allow modification in nested function
    
    def update_preview(frame: np.ndarray):
        """Update preview with current frame and parameters."""
        # Get current trackbar values
        current_params = {
            'hue_low': min(cv2.getTrackbarPos('Hue Low', 'Chroma Tuning'), 179),
            'hue_high': min(cv2.getTrackbarPos('Hue High', 'Chroma Tuning'), 179),
            'sat_low': cv2.getTrackbarPos('Sat Low', 'Chroma Tuning'),
            'sat_high': cv2.getTrackbarPos('Sat High', 'Chroma Tuning'),
            'val_low': cv2.getTrackbarPos('Val Low', 'Chroma Tuning'),
            'val_high': cv2.getTrackbarPos('Val High', 'Chroma Tuning'),
            'erode_iterations': cv2.getTrackbarPos('Erode', 'Chroma Tuning'),
            'dilate_iterations': cv2.getTrackbarPos('Dilate', 'Chroma Tuning'),
            'blur_size': cv2.getTrackbarPos('Blur', 'Chroma Tuning'),
            'edge_smooth': cv2.getTrackbarPos('Edge Smooth', 'Chroma Tuning')
        }
        
        # Get tuned result on white background
        tuned_result = chroma_key_with_bg(frame, current_params, (255, 255, 255))
        # Get auto result on white background
        auto_result_bg = chroma_key_with_bg(frame, default_params, (255, 255, 255))
        
        # Determine which one to use based on selection
        selected_result = tuned_result if use_manual[0] else auto_result_bg
        selected_params = current_params if use_manual[0] else default_params
        
        # Show composite: original | auto (white bg) | tuned (white bg) | selected (highlighted)
        h, w = frame.shape[:2]
        mask_auto = chroma_key_green(frame, default_params)[1]
        mask_tuned = chroma_key_green(frame, current_params)[1]
        mask_selected = mask_tuned if use_manual[0] else mask_auto
        
        # Highlight selected version with border
        selected_display = selected_result.copy()
        if use_manual[0]:
            cv2.rectangle(selected_display, (0, 0), (w-1, h-1), (0, 255, 0), 5)  # Green border for manual
        else:
            cv2.rectangle(selected_display, (0, 0), (w-1, h-1), (255, 255, 0), 5)  # Yellow border for auto
        
        composite = np.hstack([
            cv2.resize(frame, (w//4, h//4)),
            cv2.resize(auto_result_bg, (w//4, h//4)),
            cv2.resize(tuned_result, (w//4, h//4)),
            cv2.resize(selected_display, (w//4, h//4))
        ])
        
        # Add text labels
        label_y = 20
        cv2.putText(composite, "Original", (10, label_y), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 1)
        cv2.putText(composite, "Auto", (w//4 + 10, label_y), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 1)
        cv2.putText(composite, "Manual", (w//2 + 10, label_y), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 1)
        mode_text = "SELECTED: MANUAL" if use_manual[0] else "SELECTED: AUTO"
        color = (0, 255, 0) if use_manual[0] else (255, 255, 0)
        cv2.putText(composite, mode_text, (3*w//4 + 10, label_y), cv2.FONT_HERSHEY_SIMPLEX, 0.5, color, 2)
        
        cv2.imshow('Chroma Tuning', composite)
        return selected_params
    
    # Open video for playback
    cap = cv2.VideoCapture(video_path)
    if not cap.isOpened():
        log("TUNING", f"Could not open video: {video_path}")
        return None
    
    fps = cap.get(cv2.CAP_PROP_FPS)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    frame_delay = int(1000 / fps) if fps > 0 else 33  # ms per frame
    
    cv2.namedWindow('Chroma Tuning', cv2.WINDOW_NORMAL)
    # macOS OpenCV fix: ensure initial values are strictly less than max
    hue_low_init = min(int(params['hue_low']), 178)
    hue_high_init = min(int(params['hue_high']), 178)
    sat_low_init = min(params['sat_low'], 254)
    sat_high_init = min(params['sat_high'], 254)
    val_low_init = min(params['val_low'], 254)
    val_high_init = min(params['val_high'], 254)
    erode_init = min(params['erode_iterations'], 4)
    dilate_init = min(params['dilate_iterations'], 4)
    blur_init = min(params['blur_size'], 14)
    edge_smooth_init = min(params['edge_smooth'], 9)
    
    # Create trackbars with dummy callback (we'll update manually in loop)
    def trackbar_callback(_): pass
    cv2.createTrackbar('Hue Low', 'Chroma Tuning', hue_low_init, 179, trackbar_callback)
    cv2.createTrackbar('Hue High', 'Chroma Tuning', hue_high_init, 179, trackbar_callback)
    cv2.createTrackbar('Sat Low', 'Chroma Tuning', sat_low_init, 255, trackbar_callback)
    cv2.createTrackbar('Sat High', 'Chroma Tuning', sat_high_init, 255, trackbar_callback)
    cv2.createTrackbar('Val Low', 'Chroma Tuning', val_low_init, 255, trackbar_callback)
    cv2.createTrackbar('Val High', 'Chroma Tuning', val_high_init, 255, trackbar_callback)
    cv2.createTrackbar('Erode', 'Chroma Tuning', erode_init, 5, trackbar_callback)
    cv2.createTrackbar('Dilate', 'Chroma Tuning', dilate_init, 5, trackbar_callback)
    cv2.createTrackbar('Blur', 'Chroma Tuning', blur_init, 15, trackbar_callback)
    cv2.createTrackbar('Edge Smooth', 'Chroma Tuning', edge_smooth_init, 10, trackbar_callback)
    
    log("TUNING", "Video playback active. Adjust parameters in the window.")
    log("TUNING", "Press 'c' to confirm and save, 'q' to quit, SPACE to pause/play, 'm' to toggle auto/manual")
    log("TUNING", "Preview shows: Original | Auto (default) | Manual (your settings) | Selected (highlighted)")
    print("\n[TUNING] Video is playing. Adjust sliders and press 'c' to confirm or 'q' to cancel.\n")
    print("[TUNING] Controls:")
    print("[TUNING]   SPACE = pause/play")
    print("[TUNING]   LEFT/RIGHT = seek frame")
    print("[TUNING]   'm' = toggle between AUTO and MANUAL (selected version has border)\n")
    
    paused = False
    frame_pos = 0
    
    while True:
        if not paused:
            cap.set(cv2.CAP_PROP_POS_FRAMES, frame_pos)
            ret, frame = cap.read()
            if not ret:
                # Loop back to start
                frame_pos = 0
                cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
                ret, frame = cap.read()
                if not ret:
                    break
            frame_pos = (frame_pos + 1) % total_frames
        
        # Update preview with current frame and parameters
        selected_params = update_preview(frame)
        params = selected_params  # Keep params updated (will be either auto or manual based on selection)
        
        # Handle keyboard input
        key = cv2.waitKey(frame_delay if not paused else 30) & 0xFF
        if key == ord('c') or key == ord('C'):
            mode = "MANUAL" if use_manual[0] else "AUTO"
            print(f"[TUNING] Confirmed! Using {mode} parameters. Closing window...")
            cap.release()
            cv2.destroyAllWindows()
            cv2.waitKey(1)
            if use_manual[0]:
                log("TUNING", "Chroma parameters confirmed (MANUAL) and will be saved")
                return params
            else:
                log("TUNING", "Using AUTO parameters (defaults)")
                return None  # Return None to use defaults
        elif key == ord('q') or key == ord('Q'):
            print("[TUNING] Cancelled. Using defaults...")
            cap.release()
            cv2.destroyAllWindows()
            cv2.waitKey(1)
            log("TUNING", "Chroma tuning cancelled, using defaults")
            return None
        elif key == 27:  # ESC key
            print("[TUNING] ESC pressed. Cancelling...")
            cap.release()
            cv2.destroyAllWindows()
            cv2.waitKey(1)
            log("TUNING", "Chroma tuning cancelled (ESC), using defaults")
            return None
        elif key == ord(' ') or key == ord('m') or key == ord('M'):  # SPACE to pause/play, 'm' to toggle
            if key == ord(' '):
                paused = not paused
                print(f"[TUNING] {'Paused' if paused else 'Playing'}")
            else:  # 'm' key
                use_manual[0] = not use_manual[0]
                mode = "MANUAL" if use_manual[0] else "AUTO"
                print(f"[TUNING] Switched to {mode} mode")
        elif key == 81 or key == 2:  # LEFT arrow
            frame_pos = max(0, frame_pos - 30)
            paused = True
        elif key == 83 or key == 3:  # RIGHT arrow
            frame_pos = min(total_frames - 1, frame_pos + 30)
            paused = True


def interactive_depth_tuning(sample_depth: np.ndarray, sample_frame: np.ndarray, 
                              has_greenscreen: bool, chroma_params: dict = None) -> dict:
    """Interactive window to tune depth map parameters. Returns tuned parameters."""
    params = {
        'percentile_low': 40,
        'percentile_high': 70,
        'background_scale': 0.3,
        'mask_erode': 0,
        'mask_dilate': 0
    }
    
    def update_preview(_):
        nonlocal params
        params = {
            'percentile_low': cv2.getTrackbarPos('Percentile Low', 'Depth Tuning'),
            'percentile_high': cv2.getTrackbarPos('Percentile High', 'Depth Tuning'),
            'background_scale': cv2.getTrackbarPos('Bg Scale', 'Depth Tuning') / 100.0,
            'mask_erode': cv2.getTrackbarPos('Mask Erode', 'Depth Tuning'),
            'mask_dilate': cv2.getTrackbarPos('Mask Dilate', 'Depth Tuning')
        }
        
        # Process depth with current params
        depth_gray = sample_depth.copy()
        
        if has_greenscreen:
            fg_mask = extract_subject_mask(sample_frame, has_greenscreen, chroma_params)
            if fg_mask.shape != depth_gray.shape:
                fg_mask = cv2.resize(fg_mask, (depth_gray.shape[1], depth_gray.shape[0]))
            depth_gray = cv2.bitwise_and(depth_gray, depth_gray, mask=fg_mask)
        else:
            depth_percentile_high = np.percentile(depth_gray, params['percentile_high'])
            depth_percentile_low = np.percentile(depth_gray, params['percentile_low'])
            depth_float = depth_gray.astype(np.float32)
            soft_mask = np.clip(
                (depth_float - depth_percentile_low) / (depth_percentile_high - depth_percentile_low + 1e-6),
                0.0, 1.0
            )
            depth_gray = (depth_float * (params['background_scale'] + (1 - params['background_scale']) * soft_mask)).astype(np.uint8)
        
        # Show composite: original | depth | processed depth
        h, w = sample_frame.shape[:2]
        depth_colored = cv2.applyColorMap(depth_gray, cv2.COLORMAP_VIRIDIS)
        processed_colored = cv2.applyColorMap(depth_gray, cv2.COLORMAP_VIRIDIS)
        composite = np.hstack([
            cv2.resize(sample_frame, (w//3, h//3)),
            cv2.resize(depth_colored, (w//3, h//3)),
            cv2.resize(processed_colored, (w//3, h//3))
        ])
        cv2.imshow('Depth Tuning', composite)
    
    cv2.namedWindow('Depth Tuning', cv2.WINDOW_NORMAL)
    # macOS OpenCV fix: ensure initial values are strictly less than max
    percentile_low_init = min(params['percentile_low'], 99)
    percentile_high_init = min(params['percentile_high'], 99)
    bg_scale_init = min(int(params['background_scale'] * 100), 99)
    mask_erode_init = min(params['mask_erode'], 4)
    mask_dilate_init = min(params['mask_dilate'], 4)
    cv2.createTrackbar('Percentile Low', 'Depth Tuning', percentile_low_init, 100, update_preview)
    cv2.createTrackbar('Percentile High', 'Depth Tuning', percentile_high_init, 100, update_preview)
    cv2.createTrackbar('Bg Scale', 'Depth Tuning', bg_scale_init, 100, update_preview)
    cv2.createTrackbar('Mask Erode', 'Depth Tuning', mask_erode_init, 5, update_preview)
    cv2.createTrackbar('Mask Dilate', 'Depth Tuning', mask_dilate_init, 5, update_preview)
    
    update_preview(None)
    
    log("TUNING", "Adjust depth parameters. Press 'c' to confirm, 'q' to quit without saving.")
    while True:
        key = cv2.waitKey(1) & 0xFF
        if key == ord('c'):
            cv2.destroyAllWindows()
            log("TUNING", "Depth parameters confirmed")
            return params
        elif key == ord('q'):
            cv2.destroyAllWindows()
            log("TUNING", "Depth tuning cancelled, using defaults")
            return None


def extract_subject_mask(frame: np.ndarray, has_greenscreen: bool, chroma_params: dict = None) -> np.ndarray:
    """Extract subject mask - adaptive for greenscreen vs regular video."""
    if has_greenscreen:
        _, mask = chroma_key_green(frame, chroma_params)
        return mask
    else:
        # For regular videos: use edge detection + brightness
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        # Adaptive threshold based on frame content
        mean_val = np.mean(gray)
        std_val = np.std(gray)
        # Dynamic threshold: darker scenes get lower threshold
        thresh = max(10, min(mean_val - std_val * 0.5, 80))
        _, mask = cv2.threshold(gray, thresh, 255, cv2.THRESH_BINARY)
        # Clean up mask
        kernel = np.ones((3, 3), np.uint8)
        mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, kernel)
        mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)
        return mask


def process_frame_for_output(frame: np.ndarray, has_greenscreen: bool, chroma_params: dict = None) -> np.ndarray:
    """Process frame for output - apply chroma key only if green screen detected."""
    if has_greenscreen:
        result, _ = chroma_key_green(frame, chroma_params)
        return result
    else:
        return frame.copy()


def extract_frames(video_path: str, num_frames: int, output_dir: str, 
                   target_fps: float = None, has_greenscreen: bool = False, 
                   chroma_params: dict = None) -> list[str]:
    """Extract evenly spaced frames from video, saving raw and chroma-keyed frames."""
    # Create folders: raw_frames and chroma_keyed (no separate frames folder)
    base_dir = os.path.dirname(output_dir) if os.path.basename(output_dir) == "frames" else output_dir
    raw_frames_dir = os.path.join(base_dir, "raw_frames")
    chroma_keyed_dir = os.path.join(base_dir, "chroma_keyed")
    os.makedirs(raw_frames_dir, exist_ok=True)
    os.makedirs(chroma_keyed_dir, exist_ok=True)
    
    # Check if frames already exist
    existing_frames = sorted([f for f in os.listdir(chroma_keyed_dir) if f.startswith("frame_") and f.endswith(".png")])
    if existing_frames:
        log("EXTRACT", f"Found {len(existing_frames)} existing frames, skipping extraction")
        frame_paths = [os.path.join(chroma_keyed_dir, f) for f in existing_frames]
        return frame_paths
    
    # Extract frames from video
    cap = cv2.VideoCapture(video_path)
    if not cap.isOpened():
        raise ValueError(f"Cannot open video: {video_path}")
    
    video_fps = cap.get(cv2.CAP_PROP_FPS)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    duration = total_frames / video_fps if video_fps > 0 else 0
    
    if target_fps is not None:
        num_frames = int(duration * target_fps)
        log("EXTRACT", f"Duration: {duration:.2f}s @ {target_fps}fps = {num_frames} frames")
    
    frame_indices = np.linspace(0, total_frames - 1, num_frames, dtype=int)
    frame_paths = []
    
    for idx, frame_num in enumerate(tqdm(frame_indices, desc="Extracting frames", unit="frame")):
        cap.set(cv2.CAP_PROP_POS_FRAMES, frame_num)
        ret, frame = cap.read()
        if ret:
            # Save raw frame
            raw_frame_path = os.path.join(raw_frames_dir, f"frame_{idx:04d}.png")
            cv2.imwrite(raw_frame_path, frame)
            
            # Process and save chroma-keyed frame (or regular if no greenscreen)
            processed = process_frame_for_output(frame, has_greenscreen, chroma_params)
            chroma_path = os.path.join(chroma_keyed_dir, f"frame_{idx:04d}.png")
            cv2.imwrite(chroma_path, processed)
            frame_paths.append(chroma_path)
    
    cap.release()
    log("EXTRACT", f"Done - {len(frame_paths)} frames (raw={raw_frames_dir}, chroma_keyed={chroma_keyed_dir})")
    return frame_paths


def compute_frame_difference(frame1: np.ndarray, frame2: np.ndarray) -> float:
    """Compute structural difference between frames."""
    if len(frame1.shape) == 3:
        gray1 = cv2.cvtColor(frame1, cv2.COLOR_BGR2GRAY)
    else:
        gray1 = frame1
    if len(frame2.shape) == 3:
        gray2 = cv2.cvtColor(frame2, cv2.COLOR_BGR2GRAY)
    else:
        gray2 = frame2
    
    small_size = (128, 128)
    g1 = cv2.resize(gray1, small_size)
    g2 = cv2.resize(gray2, small_size)
    
    abs_diff = np.mean(np.abs(g1.astype(float) - g2.astype(float)))
    edges1 = cv2.Canny(g1, 50, 150)
    edges2 = cv2.Canny(g2, 50, 150)
    edge_diff = np.mean(np.abs(edges1.astype(float) - edges2.astype(float)))
    
    return abs_diff + edge_diff * 0.3


def select_diverse_frames(frames: list[np.ndarray], num_select: int) -> list[int]:
    """Select maximally diverse frames using greedy farthest-point sampling."""
    if num_select >= len(frames):
        return list(range(len(frames)))
    
    n = len(frames)
    selected = [0]
    min_diffs = [compute_frame_difference(frames[0], frames[i]) for i in range(n)]
    min_diffs[0] = -1
    
    while len(selected) < num_select:
        best_idx = np.argmax(min_diffs)
        selected.append(best_idx)
        min_diffs[best_idx] = -1
        for i in range(n):
            if min_diffs[i] >= 0:
                d = compute_frame_difference(frames[best_idx], frames[i])
                min_diffs[i] = min(min_diffs[i], d)
    
    return sorted(selected)


def blend_lighten(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    return np.maximum(base, overlay)


def blend_add(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    return np.clip(base.astype(np.float32) + overlay.astype(np.float32), 0, 255).astype(np.uint8)


def blend_screen(base: np.ndarray, overlay: np.ndarray) -> np.ndarray:
    base_f = base.astype(np.float32) / 255.0
    overlay_f = overlay.astype(np.float32) / 255.0
    result = 1 - (1 - base_f) * (1 - overlay_f)
    return (result * 255).astype(np.uint8)


def create_chronophotography(depth_images: list[np.ndarray], mode: str = "lighten") -> np.ndarray:
    """Create Marey-style chronophotography from multiple depth maps."""
    if len(depth_images) == 0:
        return None
    
    result = depth_images[0].astype(np.float32)
    
    if mode == "lighten":
        for img in depth_images[1:]:
            result = np.maximum(result, img.astype(np.float32))
        return result.astype(np.uint8)
    
    elif mode == "add":
        for img in depth_images[1:]:
            result += img.astype(np.float32)
        result = (result - result.min()) / (result.max() - result.min() + 1e-6) * 255
        return result.astype(np.uint8)
    
    elif mode == "screen":
        result = result / 255.0
        for img in depth_images[1:]:
            overlay = img.astype(np.float32) / 255.0
            result = 1 - (1 - result) * (1 - overlay)
        return (result * 255).astype(np.uint8)
    
    elif mode == "average":
        for img in depth_images[1:]:
            result += img.astype(np.float32)
        result /= len(depth_images)
        return result.astype(np.uint8)
    
    elif mode == "darken":
        for img in depth_images[1:]:
            result = np.minimum(result, img.astype(np.float32))
        return result.astype(np.uint8)
    
    elif mode == "lighten_add":
        lighten_result = depth_images[0].astype(np.float32)
        for img in depth_images[1:]:
            lighten_result = np.maximum(lighten_result, img.astype(np.float32))
        add_result = depth_images[0].astype(np.float32)
        for img in depth_images[1:]:
            add_result += img.astype(np.float32)
        add_result = (add_result - add_result.min()) / (add_result.max() - add_result.min() + 1e-6) * 255
        hybrid = lighten_result * 0.7 + add_result * 0.3
        return np.clip(hybrid, 0, 255).astype(np.uint8)
    
    elif mode == "long_exposure":
        # Normalized additive blend - creates long exposure effect
        for img in depth_images[1:]:
            result += img.astype(np.float32)
        # Normalize to prevent whiteout
        result = result / len(depth_images)
        return np.clip(result, 0, 255).astype(np.uint8)
    
    elif mode == "hero_ghost":
        # Hero frame at full opacity, others as ghost underlay
        hero = depth_images[0].astype(np.float32)
        ghost = depth_images[0].astype(np.float32)
        
        # Average all frames for ghost layer
        for img in depth_images[1:]:
            ghost += img.astype(np.float32)
        ghost /= len(depth_images)
        
        # Blend: 70% hero, 30% ghost
        result = hero * 0.7 + ghost * 0.3
        return np.clip(result, 0, 255).astype(np.uint8)
    
    else:
        return depth_images[0]


BLEND_MODES = ["lighten", "add", "screen", "average", "darken", "lighten_add", "long_exposure", "hero_ghost"]


def process_dither_frame(args):
    """Thread worker for dithering."""
    idx, frame_path, output_dir, dither_type = args
    frame = cv2.imread(frame_path)
    if frame is None:
        return None
    
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    h, w = gray.shape
    
    if dither_type == "floyd":
        pil_gray = Image.fromarray(gray, mode='L')
        dithered = pil_gray.convert('1', dither=Image.Dither.FLOYDSTEINBERG)
        result = np.array(dithered.convert('L'))
    
    elif dither_type == "atkinson":
        gray_f = gray.astype(np.float32)
        for y in range(h):
            for x in range(w):
                old_pixel = gray_f[y, x]
                new_pixel = 255.0 if old_pixel > 127 else 0.0
                gray_f[y, x] = new_pixel
                error = (old_pixel - new_pixel) / 8.0
                if x + 1 < w: gray_f[y, x + 1] += error
                if x + 2 < w: gray_f[y, x + 2] += error
                if y + 1 < h:
                    if x > 0: gray_f[y + 1, x - 1] += error
                    gray_f[y + 1, x] += error
                    if x + 1 < w: gray_f[y + 1, x + 1] += error
                if y + 2 < h: gray_f[y + 2, x] += error
        result = np.clip(gray_f, 0, 255).astype(np.uint8)
    
    elif dither_type == "bayer":
        bayer_matrix = np.array([
            [0, 8, 2, 10], [12, 4, 14, 6],
            [3, 11, 1, 9], [15, 7, 13, 5]
        ], dtype=np.float32) / 16.0 * 255.0
        threshold = np.tile(bayer_matrix, (h // 4 + 1, w // 4 + 1))[:h, :w]
        result = (gray.astype(np.float32) > threshold).astype(np.uint8) * 255
    
    else:
        result = gray
    
    out_path = os.path.join(output_dir, f"frame_{idx:04d}.png")
    Image.fromarray(result, mode='L').save(out_path)
    return result


def process_pixel_frame(args):
    """Thread worker for pixelated frames with adaptive threshold."""
    idx, frame_path, output_dir, scale, frame_stats = args
    frame = cv2.imread(frame_path)
    if frame is None:
        return None
    
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    h, w = gray.shape
    
    # Adaptive threshold based on frame statistics
    mean_val, std_val = frame_stats
    # Lower threshold for darker videos, higher for brighter
    base_thresh = max(5, mean_val * 0.15)
    
    small = cv2.resize(gray, (max(1, w // scale), max(1, h // scale)), interpolation=cv2.INTER_AREA)
    
    # Use Otsu's method for automatic thresholding
    _, binary = cv2.threshold(small, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    
    result = cv2.resize(binary, (w, h), interpolation=cv2.INTER_NEAREST)
    
    out_path = os.path.join(output_dir, f"frame_{idx:04d}.png")
    Image.fromarray(result, mode='L').save(out_path)
    return result


def check_existing_outputs(output_dir: str, effects: list, blend_modes: list) -> dict:
    """Check what outputs already exist. Returns dict of completion status."""
    status = {
        'frames': False,
        'depth_maps': False,
        'videos': {},
        'effects': {},
        'blend_modes': {}
    }
    
    # Check frames (chroma_keyed folder)
    chroma_keyed_dir = os.path.join(output_dir, "chroma_keyed")
    if os.path.exists(chroma_keyed_dir):
        existing_frames = [f for f in os.listdir(chroma_keyed_dir) if f.startswith("frame_") and f.endswith(".png")]
        status['frames'] = len(existing_frames) > 0
    
    # Check depth maps
    depth_maps_dir = os.path.join(output_dir, "depth_maps")
    if os.path.exists(depth_maps_dir):
        existing_depths = [f for f in os.listdir(depth_maps_dir) if f.startswith("depth_") and f.endswith(".png")]
        status['depth_maps'] = len(existing_depths) > 0
    
    # Check effect outputs
    effect_folders = {
        "dithered": "dithered", "atkinson": "atkinson", "bayer": "bayer",
        "extract": "extract", "lowres": "lowres", "microres": "microres",
        "red_overlay": "red_overlay", "rainbow_trail": "rainbow_trail",
        "depth_banding": "depth_banding", "chronophoto": "chronophoto"
    }
    for effect in effects:
        if effect in effect_folders:
            folder = os.path.join(output_dir, effect_folders[effect])
            if os.path.exists(folder):
                existing = [f for f in os.listdir(folder) if f.startswith("frame_") and f.endswith(".png")]
                status['effects'][effect] = len(existing) > 0
    
    # Check blend modes
    for mode in blend_modes:
        folder = os.path.join(output_dir, mode)
        if os.path.exists(folder):
            existing = [f for f in os.listdir(folder) if f.startswith("frame_") and f.endswith(".png")]
            status['blend_modes'][mode] = len(existing) > 0
    
    # Check videos
    videos_dir = os.path.join(output_dir, "videos")
    if os.path.exists(videos_dir):
        status['videos'] = {f.replace('.mp4', ''): os.path.exists(os.path.join(videos_dir, f)) 
                           for f in os.listdir(videos_dir) if f.endswith('.mp4')}
    
    return status


def compute_frame_stats(frame_paths: list[str]) -> tuple[float, float]:
    """Compute mean/std brightness across all frames for adaptive thresholding."""
    values = []
    for path in frame_paths[:min(10, len(frame_paths))]:
        frame = cv2.imread(path)
        if frame is not None:
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            values.append(np.mean(gray))
    return (np.mean(values), np.std(values)) if values else (128, 50)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("video_path", type=str)
    parser.add_argument("--num-frames", type=int, default=10)
    parser.add_argument("--target-fps", type=float, default=24.0, help="Extract frames at this fps")
    parser.add_argument("--model", type=str, default="depth-anything/Depth-Anything-V2-Small-hf")
    parser.add_argument("--device", type=str, default="mps" if torch.backends.mps.is_available() else ("cuda" if torch.cuda.is_available() else "cpu"))
    parser.add_argument("--blend-modes", type=str, nargs="+", default=[],
                        choices=BLEND_MODES, help="Chronophotography blend modes (disabled by default)")
    parser.add_argument("--alpha", type=float, default=0.5)
    parser.add_argument("--effects", type=str, nargs="+", 
                        default=["rainbow_trail", "microres", "lowres", "dithered", "depth", "red_overlay", "atkinson"],
                        choices=["rainbow_trail", "microres", "lowres", "dithered", "depth", "red_overlay", "atkinson", "extract", "bayer", "depth_banding", "chronophoto"],
                        help="Which effects to generate")
    parser.add_argument("--output-dir", type=str, default=None)
    parser.add_argument("--force-greenscreen", action="store_true", help="Force green screen mode")
    parser.add_argument("--force-no-greenscreen", action="store_true", help="Force regular video mode")
    parser.add_argument("--workers", type=int, default=4, help="Number of worker threads")
    parser.add_argument("--batch-size", type=int, default=4, help="Batch size for depth estimation")
    
    args = parser.parse_args()
    
    # Generate output dir from video filename, defaulting under pre_render/outputs
    if args.output_dir is None:
        script_dir = os.path.dirname(os.path.abspath(__file__))
        default_root = os.path.join(script_dir, "outputs")
        os.makedirs(default_root, exist_ok=True)
        video_basename = os.path.splitext(os.path.basename(args.video_path))[0]
        args.output_dir = os.path.join(default_root, f"{video_basename}_blend_output")
    
    log("INIT", f"Processing {args.video_path}")
    log("INIT", f"Device: {args.device}, Workers: {args.workers}, Batch: {args.batch_size}")
    
    # Check for existing outputs (resume capability)
    existing_status = check_existing_outputs(args.output_dir, args.effects, args.blend_modes)
    if existing_status['frames'] or existing_status['depth_maps'] or any(existing_status['effects'].values()):
        log("RESUME", "Found existing outputs - will resume from where we left off")
        if existing_status['frames']:
            log("RESUME", f"  - Frames: {existing_status['frames']} existing")
        if existing_status['depth_maps']:
            log("RESUME", f"  - Depth maps: {existing_status['depth_maps']} existing")
        for effect, exists in existing_status['effects'].items():
            if exists:
                log("RESUME", f"  - {effect}: existing")
    
    # Load or create tuning parameters
    params_file = os.path.join(args.output_dir, "tuning_params.json")
    chroma_params = None
    depth_params = None
    
    if os.path.exists(params_file):
        with open(params_file, 'r') as f:
            saved_params = json.load(f)
            chroma_params = saved_params.get('chroma')
            depth_params = saved_params.get('depth')
            log("PARAMS", "Loaded saved tuning parameters")
    
    # Auto-detect green screen
    if args.force_greenscreen:
        has_greenscreen = True
        log("DETECT", "Forced green screen mode")
    elif args.force_no_greenscreen:
        has_greenscreen = False
        log("DETECT", "Forced regular video mode")
    else:
        has_greenscreen, green_ratio = analyze_video_for_greenscreen(args.video_path)
        log("DETECT", f"Green ratio: {green_ratio:.2%} -> {'GREEN SCREEN' if has_greenscreen else 'REGULAR VIDEO'}")
    
    # Use auto/default chroma parameters (no interactive tuning)
    if has_greenscreen and chroma_params is None:
        # Load saved parameters if available, otherwise use defaults
        if os.path.exists(params_file):
            try:
                with open(params_file, 'r') as f:
                    saved_params = json.load(f)
                    chroma_params = saved_params.get('chroma')
                    if chroma_params:
                        log("PARAMS", "Loaded saved chroma parameters")
            except:
                chroma_params = None
        
        if chroma_params is None:
            log("CHROMA", "Using auto/default chroma key parameters")
            chroma_params = None  # Will use defaults in chroma_key_green function
    
    # Extract frames (saves to raw_frames and chroma_keyed folders)
    chroma_keyed_dir = os.path.join(args.output_dir, "chroma_keyed")
    raw_frames_dir = os.path.join(args.output_dir, "raw_frames")
    if not existing_status['frames']:
        log("EXTRACT", f"Starting @ {args.target_fps}fps...")
        frame_paths = extract_frames(args.video_path, args.num_frames, chroma_keyed_dir, 
                                      target_fps=args.target_fps, has_greenscreen=has_greenscreen,
                                      chroma_params=chroma_params)
    else:
        log("RESUME", "Using existing frames")
        existing_frames = sorted([f for f in os.listdir(chroma_keyed_dir) if f.startswith("frame_") and f.endswith(".png")])
        frame_paths = [os.path.join(chroma_keyed_dir, f) for f in existing_frames]
    
    # Create video for chroma_keyed frames (synchronous, not background)
    videos_dir = os.path.join(args.output_dir, "videos")
    os.makedirs(videos_dir, exist_ok=True)
    if not existing_status['videos'].get('chroma_keyed', False):
        log("VIDEO", "Creating chroma_keyed.mp4...")
        create_video_from_folder(chroma_keyed_dir, "chroma_keyed", videos_dir, args.target_fps)
    else:
        log("RESUME", "chroma_keyed.mp4 already exists")
    
    # If only extract is requested, stop here
    if args.effects == ["extract"]:
        log("DONE", f"Frame extraction complete. Output: {args.output_dir}")
        log("DONE", f"  - Raw frames: {os.path.join(args.output_dir, 'raw_frames')}")
        log("DONE", f"  - Chroma-keyed frames: {chroma_keyed_dir}")
        log("DONE", f"  - Video: {os.path.join(videos_dir, 'chroma_keyed.mp4')}")
        return
    
    # Compute frame stats for adaptive thresholding
    frame_stats = compute_frame_stats(frame_paths)
    log("STATS", f"Frame brightness: mean={frame_stats[0]:.1f}, std={frame_stats[1]:.1f}")
    
    # Load Depth Anything V2 (only if needed)
    needs_depth = "depth" in args.effects or "depth_banding" in args.effects or args.blend_modes
    pipe = None
    if needs_depth:
        log("MODEL", f"Loading {args.model}...")
        pipe = pipeline(task="depth-estimation", model=args.model, device=args.device)
    
    # Create output dirs
    depth_maps_dir = os.path.join(args.output_dir, "depth_maps")
    os.makedirs(depth_maps_dir, exist_ok=True)
    for mode in args.blend_modes:
        os.makedirs(os.path.join(args.output_dir, mode), exist_ok=True)
    
    # Interactive depth tuning (before processing all frames)
    if needs_depth and "depth" in args.effects and depth_params is None and not existing_status['depth_maps']:
        log("TUNING", "Opening interactive depth tuning...")
        # Process one sample frame for tuning (use raw frame)
        raw_frame_files = sorted([f for f in os.listdir(raw_frames_dir) if f.startswith("frame_") and f.endswith(".png")])
        sample_frame_path = os.path.join(raw_frames_dir, raw_frame_files[len(raw_frame_files) // 2])  # Middle frame
        sample_frame = cv2.imread(sample_frame_path)
        sample_image = Image.open(sample_frame_path)
        sample_result = pipe([sample_image])[0]
        sample_depth_array = np.array(sample_result["depth"])
        depth_min, depth_max = sample_depth_array.min(), sample_depth_array.max()
        if depth_max > depth_min:
            depth_norm = (sample_depth_array - depth_min) / (depth_max - depth_min)
        else:
            depth_norm = np.zeros_like(sample_depth_array, dtype=np.float32)
        sample_depth_gray = (depth_norm * 255).astype(np.uint8)
        
        depth_params = interactive_depth_tuning(sample_depth_gray, sample_frame, has_greenscreen, chroma_params)
        if depth_params:
            # Save parameters
            if not os.path.exists(params_file):
                saved_params = {}
            else:
                with open(params_file, 'r') as f:
                    saved_params = json.load(f)
            saved_params['depth'] = depth_params
            with open(params_file, 'w') as f:
                json.dump(saved_params, f, indent=2)
            log("PARAMS", "Depth parameters saved")
        else:
            # Use defaults
            depth_params = {
                'percentile_low': 40,
                'percentile_high': 70,
                'background_scale': 0.3,
                'mask_erode': 0,
                'mask_dilate': 0
            }
    
    # Use default depth params if not set
    if depth_params is None:
        depth_params = {
            'percentile_low': 40,
            'percentile_high': 70,
            'background_scale': 0.3,
            'mask_erode': 0,
            'mask_dilate': 0
        }
    
    # Define function to run depth estimation
    def run_depth_estimation():
        """Run depth estimation and return depth_images list."""
        depth_imgs = []
        if not existing_status['depth_maps']:
            # Get raw frame paths (use raw frames for depth estimation)
            raw_frame_files = sorted([f for f in os.listdir(raw_frames_dir) if f.startswith("frame_") and f.endswith(".png")])
            raw_frame_paths = [os.path.join(raw_frames_dir, f) for f in raw_frame_files]
            
            log("DEPTH", f"Running depth estimation on {len(raw_frame_paths)} raw frames...")
            
            # Process in batches
            num_batches = (len(raw_frame_paths) + args.batch_size - 1) // args.batch_size
            with tqdm(total=len(raw_frame_paths), desc="Depth estimation", unit="frame") as pbar:
                for batch_start in range(0, len(raw_frame_paths), args.batch_size):
                    batch_end = min(batch_start + args.batch_size, len(raw_frame_paths))
                    batch_paths = raw_frame_paths[batch_start:batch_end]
                    
                    # Load batch images (use raw frames for depth estimation)
                    batch_images = [Image.open(p) for p in batch_paths]
                    
                    # Run depth on batch
                    results = pipe(batch_images)
                    
                    for i, (result, raw_frame_path) in enumerate(zip(results, batch_paths)):
                        idx = batch_start + i
                        depth_map = result["depth"]
                        depth_array = np.array(depth_map)
                        
                        # Get subject mask from raw frame
                        orig_frame = cv2.imread(raw_frame_path)
                        fg_mask = extract_subject_mask(orig_frame, has_greenscreen, chroma_params)
                        
                        # Resize mask if needed
                        if fg_mask.shape != depth_array.shape:
                            fg_mask = cv2.resize(fg_mask, (depth_array.shape[1], depth_array.shape[0]))
                        
                        # Normalize depth
                        depth_min, depth_max = depth_array.min(), depth_array.max()
                        if depth_max > depth_min:
                            depth_norm = (depth_array - depth_min) / (depth_max - depth_min)
                        else:
                            depth_norm = np.zeros_like(depth_array, dtype=np.float32)
                        
                        depth_gray = (depth_norm * 255).astype(np.uint8)
                        
                        # Apply subject mask with tuned parameters
                        if has_greenscreen:
                            depth_gray = cv2.bitwise_and(depth_gray, depth_gray, mask=fg_mask)
                        else:
                            depth_percentile_high = np.percentile(depth_gray, depth_params['percentile_high'])
                            depth_percentile_low = np.percentile(depth_gray, depth_params['percentile_low'])
                            depth_float = depth_gray.astype(np.float32)
                            soft_mask = np.clip(
                                (depth_float - depth_percentile_low) / (depth_percentile_high - depth_percentile_low + 1e-6),
                                0.0, 1.0
                            )
                            depth_gray = (depth_float * (depth_params['background_scale'] + (1 - depth_params['background_scale']) * soft_mask)).astype(np.uint8)
                        
                        depth_imgs.append(depth_gray)
                        
                        path = os.path.join(depth_maps_dir, f"depth_{idx:04d}.png")
                        Image.fromarray(depth_gray, mode='L').save(path)
                    
                    pbar.update(len(batch_paths))
            
            log("DEPTH", f"Done - {len(depth_imgs)} depth maps")
            
            # Create depth chronophoto
            depth_chrono = create_chronophotography(depth_imgs, "lighten_add")
            path = os.path.join(depth_maps_dir, "chronophoto.png")
            Image.fromarray(depth_chrono, mode='L').save(path)
            log("CHRONO", "Created depth chronophoto")
        else:
            log("RESUME", "Loading existing depth maps...")
            existing_depths = sorted([f for f in os.listdir(depth_maps_dir) if f.startswith("depth_") and f.endswith(".png")])
            for depth_file in existing_depths:
                depth_path = os.path.join(depth_maps_dir, depth_file)
                depth_img = cv2.imread(depth_path, cv2.IMREAD_GRAYSCALE)
                if depth_img is not None:
                    depth_imgs.append(depth_img)
            log("RESUME", f"Loaded {len(depth_imgs)} existing depth maps")
        
        return depth_imgs
    
    # Check if depth is needed for any effects
    needs_depth = "depth" in args.effects or "depth_banding" in args.effects or args.blend_modes
    
    # Run depth estimation - will be used in parallel with independent effects
    depth_images = []
    depth_executor = None
    depth_future = None
    
    if needs_depth:
        if not existing_status['depth_maps']:
            log("PARALLEL", "Starting depth estimation (will run in parallel with independent effects)...")
            # Start depth estimation in thread (GPU work can run while CPU processes effects)
            depth_executor = ThreadPoolExecutor(max_workers=1)
            depth_future = depth_executor.submit(run_depth_estimation)
        else:
            log("RESUME", "Loading existing depth maps...")
            existing_depths = sorted([f for f in os.listdir(depth_maps_dir) if f.startswith("depth_") and f.endswith(".png")])
            for depth_file in existing_depths:
                depth_path = os.path.join(depth_maps_dir, depth_file)
                depth_img = cv2.imread(depth_path, cv2.IMREAD_GRAYSCALE)
                if depth_img is not None:
                    depth_images.append(depth_img)
            log("RESUME", f"Loaded {len(depth_images)} existing depth maps")
    else:
        log("SKIP", "Skipping depth estimation (not needed for requested effects)")
    
    # Create video for depth_maps (synchronous)
    if "depth" in args.effects and not existing_status['videos'].get('depth_maps', False):
        log("VIDEO", "Creating depth_maps.mp4...")
        create_video_from_folder(depth_maps_dir, "depth_maps", videos_dir, args.target_fps)
    elif existing_status['videos'].get('depth_maps', False):
        log("RESUME", "depth_maps.mp4 already exists")
    
    # Only require depth images if depth-related effects are needed
    needs_depth = "depth" in args.effects or "depth_banding" in args.effects or args.blend_modes
    if needs_depth and not depth_images:
        raise RuntimeError("Depth images not available but required for requested effects")
    
    # Resize depths to common size (only if we have depth images)
    resized_depths = []
    if depth_images:
        target_shape = depth_images[0].shape
        for img in depth_images:
            if img.shape != target_shape:
                img = cv2.resize(img, (target_shape[1], target_shape[0]))
            resized_depths.append(img)
    
    # Load original frames (only if needed for effects)
    original_frames = []
    needs_original_frames = needs_depth or any(effect in args.effects for effect in ["red_overlay", "rainbow_trail", "chronophoto"])
    if needs_original_frames:
        log("FRAMES", "Loading original frames...")
        for frame_path in frame_paths:
            frame = cv2.imread(frame_path)
            if frame is None:
                continue
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            frame = cv2.cvtColor(gray, cv2.COLOR_GRAY2BGR)
            if target_shape and frame.shape[:2] != target_shape:
                frame = cv2.resize(frame, (target_shape[1], target_shape[0]))
            original_frames.append(frame)
    
    # Chronophoto pass - ghostly composite of raw frames (not depth maps)
    # Runs after frames are loaded, parallelized by blend mode
    if "chronophoto" in args.effects:
        log("CHRONO", "Creating chronophoto pass (ghostly composite of raw frames)...")
        chrono_dir = os.path.join(args.output_dir, "chronophoto")
        os.makedirs(chrono_dir, exist_ok=True)
        
        if not original_frames:
            log("CHRONO", "No frames available, skipping chronophoto pass")
        else:
            # Convert frames to grayscale for chronophotography
            gray_frames = [cv2.cvtColor(f, cv2.COLOR_BGR2GRAY) for f in original_frames]
            
            # Blend modes for chronophoto pass
            chrono_modes = ["long_exposure", "hero_ghost", "lighten_add"]
            
            def generate_chronophoto(mode):
                """Generate chronophoto from raw frames for a specific mode."""
                chrono_result = create_chronophotography(gray_frames, mode)
                
                # Convert back to RGB for saving
                chrono_rgb = cv2.cvtColor(chrono_result, cv2.COLOR_GRAY2RGB)
                
                # Save the ghostly composite
                chrono_path = os.path.join(chrono_dir, f"chronophoto_{mode}.png")
                Image.fromarray(chrono_rgb).save(chrono_path)
                
                return mode
            
            # Process chronophoto modes in parallel
            with ThreadPoolExecutor(max_workers=min(len(chrono_modes), args.workers)) as executor:
                list(tqdm(executor.map(generate_chronophoto, chrono_modes), 
                         total=len(chrono_modes), desc="Chronophoto pass", unit="mode"))
            
            log("CHRONO", "Chronophoto pass done")
    
    # Process independent effects while depth estimation runs (if depth is running)
    # These effects don't need depth: dithered, atkinson, bayer, extract, lowres, microres, rainbow_trail
    dithered_images = []
    
    if depth_future:
        log("PARALLEL", "Processing independent effects while depth estimation runs...")
    
    # Parallel dithering (independent effect - runs while depth estimation continues)
    if ("dithered" in args.effects or "red_overlay" in args.effects) and not existing_status['effects'].get('dithered', False):
        log("DITHER", "Creating dithered frames (parallel)...")
        dithered_dir = os.path.join(args.output_dir, "dithered")
        os.makedirs(dithered_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            floyd_args = [(i, p, dithered_dir, "floyd") for i, p in enumerate(frame_paths)]
            floyd_results = list(tqdm(executor.map(process_dither_frame, floyd_args), 
                                      total=len(floyd_args), desc="Dithering (Floyd-Steinberg)", unit="frame"))
            dithered_images = [r for r in floyd_results if r is not None]
        
        # Create video for dithered (synchronous)
        if not existing_status['videos'].get('dithered', False):
            log("VIDEO", "Creating dithered.mp4...")
            create_video_from_folder(dithered_dir, "dithered", videos_dir, args.target_fps)
        else:
            log("RESUME", "dithered.mp4 already exists")
    elif existing_status['effects'].get('dithered', False):
        log("RESUME", "Using existing dithered frames")
        dithered_dir = os.path.join(args.output_dir, "dithered")
        existing_dithered = sorted([f for f in os.listdir(dithered_dir) if f.startswith("frame_") and f.endswith(".png")])
        for d_file in existing_dithered:
            d_path = os.path.join(dithered_dir, d_file)
            d_img = cv2.imread(d_path, cv2.IMREAD_GRAYSCALE)
            if d_img is not None:
                dithered_images.append(d_img)
    
    # Wait for depth estimation to complete (if it was running)
    if depth_future:
        log("PARALLEL", "Waiting for depth estimation to complete...")
        depth_images = depth_future.result()
        depth_executor.shutdown(wait=True)
    
    if "atkinson" in args.effects and not existing_status['effects'].get('atkinson', False):
        log("DITHER", "Creating Atkinson dithered frames...")
        atkinson_dir = os.path.join(args.output_dir, "atkinson")
        os.makedirs(atkinson_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            atk_args = [(i, p, atkinson_dir, "atkinson") for i, p in enumerate(frame_paths)]
            list(tqdm(executor.map(process_dither_frame, atk_args), 
                      total=len(atk_args), desc="Dithering (Atkinson)", unit="frame"))
        
        # Create video for atkinson (synchronous)
        if not existing_status['videos'].get('atkinson', False):
            log("VIDEO", "Creating atkinson.mp4...")
            create_video_from_folder(atkinson_dir, "atkinson", videos_dir, args.target_fps)
        else:
            log("RESUME", "atkinson.mp4 already exists")
    elif existing_status['effects'].get('atkinson', False):
        log("RESUME", "Using existing atkinson frames")
    
    if "bayer" in args.effects and not existing_status['effects'].get('bayer', False):
        log("DITHER", "Creating Bayer dithered frames...")
        bayer_dir = os.path.join(args.output_dir, "bayer")
        os.makedirs(bayer_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            bayer_args = [(i, p, bayer_dir, "bayer") for i, p in enumerate(frame_paths)]
            list(tqdm(executor.map(process_dither_frame, bayer_args), 
                      total=len(bayer_args), desc="Dithering (Bayer)", unit="frame"))
        
        # Create video for bayer (synchronous)
        if not existing_status['videos'].get('bayer', False):
            log("VIDEO", "Creating bayer.mp4...")
            create_video_from_folder(bayer_dir, "bayer", videos_dir, args.target_fps)
        else:
            log("RESUME", "bayer.mp4 already exists")
    elif existing_status['effects'].get('bayer', False):
        log("RESUME", "Using existing bayer frames")
    
    # Parallel pixelated frames with adaptive threshold
    if "extract" in args.effects and not existing_status['effects'].get('extract', False):
        log("PIXEL", "Creating extract frames...")
        extract_dir = os.path.join(args.output_dir, "extract")
        os.makedirs(extract_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            extract_args = [(i, p, extract_dir, 8, frame_stats) for i, p in enumerate(frame_paths)]
            list(tqdm(executor.map(process_pixel_frame, extract_args), 
                      total=len(extract_args), desc="Pixelation (Extract)", unit="frame"))
        
        # Create video for extract (synchronous)
        if not existing_status['videos'].get('extract', False):
            log("VIDEO", "Creating extract.mp4...")
            create_video_from_folder(extract_dir, "extract", videos_dir, args.target_fps)
        else:
            log("RESUME", "extract.mp4 already exists")
    elif existing_status['effects'].get('extract', False):
        log("RESUME", "Using existing extract frames")
    
    if "lowres" in args.effects and not existing_status['effects'].get('lowres', False):
        log("PIXEL", "Creating lowres frames...")
        lowres_dir = os.path.join(args.output_dir, "lowres")
        os.makedirs(lowres_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            lowres_args = [(i, p, lowres_dir, 16, frame_stats) for i, p in enumerate(frame_paths)]
            list(tqdm(executor.map(process_pixel_frame, lowres_args), 
                      total=len(lowres_args), desc="Pixelation (Lowres)", unit="frame"))
        
        # Create video for lowres (synchronous)
        if not existing_status['videos'].get('lowres', False):
            log("VIDEO", "Creating lowres.mp4...")
            create_video_from_folder(lowres_dir, "lowres", videos_dir, args.target_fps)
        else:
            log("RESUME", "lowres.mp4 already exists")
    elif existing_status['effects'].get('lowres', False):
        log("RESUME", "Using existing lowres frames")
    
    if "microres" in args.effects and not existing_status['effects'].get('microres', False):
        log("PIXEL", "Creating microres frames...")
        microres_dir = os.path.join(args.output_dir, "microres")
        os.makedirs(microres_dir, exist_ok=True)
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            micro_args = [(i, p, microres_dir, 24, frame_stats) for i, p in enumerate(frame_paths)]
            list(tqdm(executor.map(process_pixel_frame, micro_args), 
                      total=len(micro_args), desc="Pixelation (Microres)", unit="frame"))
        
        # Create video for microres (synchronous)
        if not existing_status['videos'].get('microres', False):
            log("VIDEO", "Creating microres.mp4...")
            create_video_from_folder(microres_dir, "microres", videos_dir, args.target_fps)
        else:
            log("RESUME", "microres.mp4 already exists")
    elif existing_status['effects'].get('microres', False):
        log("RESUME", "Using existing microres frames")
    
    # Red overlay frames
    if "red_overlay" in args.effects and not existing_status['effects'].get('red_overlay', False):
        log("EFFECT", "Creating red overlay frames...")
        red_overlay_dir = os.path.join(args.output_dir, "red_overlay")
        os.makedirs(red_overlay_dir, exist_ok=True)
        for idx, frame_path in enumerate(tqdm(frame_paths, desc="Red overlay", unit="frame")):
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
                
                Image.fromarray(cv2.cvtColor(result, cv2.COLOR_BGR2RGB)).save(
                    os.path.join(red_overlay_dir, f"frame_{idx:04d}.png"))
        log("EFFECT", "Red overlay done")
        
        # Create video for red_overlay (synchronous)
        if not existing_status['videos'].get('red_overlay', False):
            log("VIDEO", "Creating red_overlay.mp4...")
            create_video_from_folder(red_overlay_dir, "red_overlay", videos_dir, args.target_fps)
        else:
            log("RESUME", "red_overlay.mp4 already exists")
    elif existing_status['effects'].get('red_overlay', False):
        log("RESUME", "Using existing red_overlay frames")
    
    # Rainbow trail
    if "rainbow_trail" in args.effects and not existing_status['effects'].get('rainbow_trail', False):
        log("EFFECT", "Creating rainbow trail frames...")
        rainbow_dir = os.path.join(args.output_dir, "rainbow_trail")
        os.makedirs(rainbow_dir, exist_ok=True)
        
        for idx, frame_path in enumerate(tqdm(frame_paths, desc="Rainbow trail", unit="frame")):
            frame = cv2.imread(frame_path)
            if frame is None:
                continue
            h, w = frame.shape[:2]
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            result = np.zeros((h, w, 3), dtype=np.float32)
            
            trail_frames = max(0, idx - 8)
            for t_idx in range(trail_frames, idx + 1):
                t_frame = cv2.imread(frame_paths[t_idx])
                if t_frame is None:
                    continue
                t_gray = cv2.cvtColor(t_frame, cv2.COLOR_BGR2GRAY).astype(np.float32)
                if t_gray.shape != (h, w):
                    t_gray = cv2.resize(t_gray, (w, h))
                
                time_offset = (idx - t_idx) / 8.0
                hue = (time_offset * 0.7) % 1.0
                
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
                
                fade = 1.0 - (time_offset * 0.7)
                intensity = t_gray / 255.0 * fade
                blurred = cv2.GaussianBlur(intensity, (15, 15), 0)
                result[:, :, 2] += blurred * r * 180
                result[:, :, 1] += blurred * g * 180
                result[:, :, 0] += blurred * b * 180
            
            current_bright = gray.astype(np.float32) / 255.0
            bright_mask = current_bright > 0.6
            result[:, :, 0] = np.where(bright_mask, np.clip(result[:, :, 0] + current_bright * 200, 0, 255), result[:, :, 0])
            result[:, :, 1] = np.where(bright_mask, np.clip(result[:, :, 1] + current_bright * 200, 0, 255), result[:, :, 1])
            result[:, :, 2] = np.where(bright_mask, np.clip(result[:, :, 2] + current_bright * 200, 0, 255), result[:, :, 2])
            
            noise = np.random.normal(0, 8, (h, w, 3))
            result = np.clip(result + noise, 0, 255).astype(np.uint8)
            Image.fromarray(cv2.cvtColor(result, cv2.COLOR_BGR2RGB)).save(
                os.path.join(rainbow_dir, f"frame_{idx:04d}.png"))
        log("EFFECT", "Rainbow trail done")
        
        # Create video for rainbow_trail (synchronous)
        if not existing_status['videos'].get('rainbow_trail', False):
            log("VIDEO", "Creating rainbow_trail.mp4...")
            create_video_from_folder(rainbow_dir, "rainbow_trail", videos_dir, args.target_fps)
        else:
            log("RESUME", "rainbow_trail.mp4 already exists")
    elif existing_status['effects'].get('rainbow_trail', False):
        log("RESUME", "Using existing rainbow_trail frames")
    
    # Depth banding
    if "depth_banding" in args.effects and not existing_status['effects'].get('depth_banding', False):
        log("EFFECT", "Creating depth banding frames...")
        banding_dir = os.path.join(args.output_dir, "depth_banding")
        os.makedirs(banding_dir, exist_ok=True)
        
        for idx, frame_path in enumerate(tqdm(frame_paths, desc="Depth banding", unit="frame")):
            frame = cv2.imread(frame_path)
            if frame is None:
                continue
            h, w = frame.shape[:2]
            
            if idx < len(depth_images):
                depth = depth_images[idx]
                if depth.shape != (h, w):
                    depth = cv2.resize(depth, (w, h))
            else:
                depth = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            
            result = np.zeros((h, w), dtype=np.uint8)
            
            for y in range(h):
                for x in range(w):
                    d = depth[y, x]
                    if d < 10:
                        continue
                    line_spacing = max(2, int(20 - d / 15))
                    wave = int(np.sin(y * 0.1 + d * 0.05) * 3)
                    if (y + wave) % line_spacing < 2:
                        result[y, x] = 255
            
            noise_mask = np.random.random((h, w)) < (depth.astype(np.float32) / 255.0 * 0.1)
            result = np.where(noise_mask, 255, result).astype(np.uint8)
            Image.fromarray(result, mode='L').save(os.path.join(banding_dir, f"frame_{idx:04d}.png"))
        log("EFFECT", "Depth banding done")
        
        # Create video for depth_banding (synchronous)
        if not existing_status['videos'].get('depth_banding', False):
            log("VIDEO", "Creating depth_banding.mp4...")
            create_video_from_folder(banding_dir, "depth_banding", videos_dir, args.target_fps)
        else:
            log("RESUME", "depth_banding.mp4 already exists")
    elif existing_status['effects'].get('depth_banding', False):
        log("RESUME", "Using existing depth_banding frames")
    
    # Chronophotography composites (only if blend modes specified and depth available)
    if args.blend_modes and resized_depths:
        log("CHRONO", "Creating blend composites...")
        for mode in args.blend_modes:
            if not existing_status['blend_modes'].get(mode, False):
                for idx in tqdm(range(len(resized_depths)), desc=f"Blend ({mode})", unit="frame"):
                    frames_so_far = resized_depths[:idx + 1]
                    depth_result = create_chronophotography(frames_so_far, mode)
                    depth_rgb = cv2.cvtColor(depth_result, cv2.COLOR_GRAY2RGB)
                    
                    if idx < len(original_frames):
                        raw_frame = original_frames[idx]
                        blended = cv2.addWeighted(depth_rgb, 1.0 - args.alpha, raw_frame, args.alpha, 0)
                    else:
                        blended = depth_rgb
                    
                    path = os.path.join(args.output_dir, mode, f"frame_{idx:04d}.png")
                    Image.fromarray(blended).save(path)
                
                depth_result = create_chronophotography(resized_depths, mode)
                depth_rgb = cv2.cvtColor(depth_result, cv2.COLOR_GRAY2RGB)
                
                if original_frames:
                    avg_raw = np.mean([f.astype(np.float32) for f in original_frames], axis=0).astype(np.uint8)
                    blended = cv2.addWeighted(depth_rgb, 1.0 - args.alpha, avg_raw, args.alpha, 0)
                else:
                    blended = depth_rgb
                
                path = os.path.join(args.output_dir, mode, "chronophoto.png")
                Image.fromarray(blended).save(path)
                log("CHRONO", f"{mode} done")
                
                # Create video for blend mode (synchronous)
                if not existing_status['videos'].get(mode, False):
                    log("VIDEO", f"Creating {mode}.mp4...")
                    mode_dir = os.path.join(args.output_dir, mode)
                    create_video_from_folder(mode_dir, mode, videos_dir, args.target_fps)
                else:
                    log("RESUME", f"{mode}.mp4 already exists")
            else:
                log("RESUME", f"Using existing {mode} frames")
    elif args.blend_modes and not resized_depths:
        log("SKIP", "Skipping blend modes - depth maps not available")
    
    log("DONE", f"Output: {args.output_dir}")


if __name__ == "__main__":
    main()
