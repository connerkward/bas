#!/usr/bin/env python3
"""
Day/Night relighting with depth of field using normal maps and depth maps.
"""

import argparse
import cv2
import numpy as np
import time
from pathlib import Path
from typing import Optional, Tuple, List


class RelightProcessor:
    """Core relighting processor."""
    
    def __init__(self, original: np.ndarray, normal: Optional[np.ndarray] = None, 
                 depth: Optional[np.ndarray] = None):
        self.original = original.astype(np.float32) / 255.0
        self.normal = self._load_normal(normal) if normal is not None else None
        self.depth = self._load_depth(depth) if depth is not None else None
        
        # Ensure all images are same size
        h, w = original.shape[:2]
        if self.normal is not None and self.normal.shape[:2] != (h, w):
            self.normal = cv2.resize(self.normal, (w, h))
        if self.depth is not None and self.depth.shape[:2] != (h, w):
            self.depth = cv2.resize(self.depth, (w, h))
    
    def _load_normal(self, normal: np.ndarray) -> np.ndarray:
        """Convert normal map to [-1, 1] range."""
        if len(normal.shape) == 2:
            return None
        normal_float = normal.astype(np.float32) / 255.0
        normal_xyz = (normal_float * 2.0 - 1.0)
        if normal_xyz.shape[2] >= 3:
            normal_xyz[:, :, 2] = np.abs(normal_xyz[:, :, 2])
        return normal_xyz
    
    def _load_depth(self, depth: np.ndarray) -> np.ndarray:
        """Normalize depth map to [0, 1] range."""
        if len(depth.shape) == 3:
            depth = cv2.cvtColor(depth, cv2.COLOR_BGR2GRAY)
        return depth.astype(np.float32) / 255.0
    
    def _get_sun_moon_params(self, hours: float, brightness: float = 1.0) -> dict:
        """
        Cinematic lighting based on hour of day (0-24).
        Direct hour mapping for intuitive control.
        """
        # Wrap hours to 0-24
        h = hours % 24
        
        # Sun elevation based on hour (peaks at noon)
        # Simple approximation: -90 at midnight, +90 at noon
        if h <= 12:
            elevation = -90 + (h / 12) * 180  # -90 to 90
        else:
            elevation = 90 - ((h - 12) / 12) * 180  # 90 to -90
        
        azimuth = (h / 24) * 360
        
        # Direct hour-based color grading (BGR format for OpenCV!)
        # Extended transition zones for smoother animation
        
        if h < 3 or h >= 23:  # NIGHT: 23:00 - 03:00 (4 hours)
            # Pure cinematic blue (BGR: max B, near-zero G/R)
            light_color = (1.0, 0.1, 0.0)
            night_dim = 0.95
            is_night = True
            
        elif h < 7:  # BLUE TO GOLD MORNING: 03:00 - 07:00 (4 hour transition)
            # Smooth transition pure blue to gold
            blend = (h - 3) / 4.0
            blend = blend * blend * (3 - 2 * blend)  # Smoothstep
            light_color = (
                1.0 - blend * 0.7,   # B: 1.0 -> 0.3
                0.1 + blend * 0.5,   # G: 0.1 -> 0.6
                0.0 + blend * 1.0    # R: 0.0 -> 1.0
            )
            night_dim = 0.95 + blend * 0.0
            is_night = False
            
        elif h < 10:  # GOLDEN HOUR TO DAY: 07:00 - 10:00 (3 hour transition)
            # Transition from gold to neutral
            blend = (h - 7) / 3.0
            blend = blend * blend * (3 - 2 * blend)  # Smoothstep
            light_color = (
                0.3 + blend * 0.62,  # B: 0.3 -> 0.92
                0.6 + blend * 0.37,  # G: 0.6 -> 0.97
                1.0                   # R: stays 1.0
            )
            night_dim = 0.95 + blend * 0.05
            is_night = False
            
        elif h < 14:  # DAY: 10:00 - 14:00 (4 hours)
            # Neutral warm (BGR)
            light_color = (0.92, 0.97, 1.0)
            night_dim = 1.0
            is_night = False
            
        elif h < 17:  # DAY TO GOLDEN EVENING: 14:00 - 17:00 (3 hour transition)
            # Transition from neutral to gold
            blend = (h - 14) / 3.0
            blend = blend * blend * (3 - 2 * blend)  # Smoothstep
            light_color = (
                0.92 - blend * 0.62,  # B: 0.92 -> 0.3
                0.97 - blend * 0.37,  # G: 0.97 -> 0.6
                1.0                    # R: stays 1.0
            )
            night_dim = 1.0 - blend * 0.05
            is_night = False
            
        elif h < 23:  # GOLD TO BLUE EVENING: 17:00 - 23:00 (6 hour transition)
            # Smooth transition gold to pure blue
            blend = (h - 17) / 6.0
            blend = blend * blend * (3 - 2 * blend)  # Smoothstep
            light_color = (
                0.3 + blend * 0.7,   # B: 0.3 -> 1.0
                0.6 - blend * 0.5,   # G: 0.6 -> 0.1
                1.0 - blend * 1.0    # R: 1.0 -> 0.0
            )
            night_dim = 0.95
            is_night = blend > 0.8
            
        else:  # Fallback
            light_color = (1.0, 0.1, 0.0)  # Pure blue in BGR
            night_dim = 0.95
            is_night = True
        
        base_intensity = 1.0
        base_ambient = 0.3
        
        return {
            'azimuth': azimuth,
            'elevation': elevation,
            'light_color': light_color,
            'intensity': base_intensity * night_dim * brightness,
            'ambient': base_ambient * night_dim * brightness,
            'is_night': is_night
        }
    
    def _apply_depth_of_field(self, image: np.ndarray, focus_depth: float, 
                               dof_amount: float) -> np.ndarray:
        """Apply depth-based blur (depth of field effect) - optimized."""
        if self.depth is None or dof_amount <= 0:
            return image
        
        # Calculate blur amount based on distance from focus depth
        depth_diff = np.abs(self.depth - focus_depth)
        
        # Simple two-level blur for speed - increased max blur
        blur_size = max(3, int(dof_amount * 61))
        if blur_size % 2 == 0:
            blur_size += 1
        
        # Single blur pass
        blurred = cv2.GaussianBlur(image, (blur_size, blur_size), 0)
        
        # Blend based on depth difference
        blend = np.clip(depth_diff * 2, 0, 1)[:, :, np.newaxis]
        
        return image * (1 - blend) + blurred * blend
    
    def process(self, hours: float = 12.0, focus_depth: float = 0.5,
                dof_amount: float = 0.0, normal_strength: float = 1.0,
                brightness: float = 1.0, ambient_color: tuple = (0.5, 0.5, 0.5),
                cutout_x: float = 0.5, cutout_y: float = 0.5, 
                cutout_size: float = 0.0,
                override_light: dict = None) -> np.ndarray:
        """Process with day/night cycle, depth of field, and dashboard ambient.
        
        override_light: dict with 'azimuth', 'elevation', 'color' (BGR 0-1) to override lighting.
        """
        
        # Get sun/moon parameters
        params = self._get_sun_moon_params(hours, brightness)
        
        # Override with custom light if provided
        if override_light:
            if 'azimuth' in override_light:
                params['azimuth'] = override_light['azimuth']
            if 'elevation' in override_light:
                params['elevation'] = override_light['elevation']
            if 'color' in override_light:
                params['light_color'] = override_light['color']
            if 'intensity' in override_light:
                params['intensity'] = override_light['intensity']
        
        result = self.original.copy()
        
        # Color tint from time of day (always applied)
        ambient_rgb = np.array(ambient_color, dtype=np.float32).reshape(1, 1, 3)
        natural_rgb = np.array(params['light_color'], dtype=np.float32).reshape(1, 1, 3)
        
        # Apply normal map lighting if available
        if self.normal is not None and normal_strength > 0:
            # Calculate light direction from azimuth/elevation
            az_rad = np.deg2rad(params['azimuth'])
            el_rad = np.deg2rad(params['elevation'])
            
            light_dir = np.array([
                np.cos(el_rad) * np.sin(az_rad),
                np.sin(el_rad),
                np.cos(el_rad) * np.cos(az_rad)
            ], dtype=np.float32)
            
            # Normalize
            light_dir = light_dir / (np.linalg.norm(light_dir) + 1e-6)
            
            # Get normals
            normals = self.normal[:, :, :3].copy()
            normals[:, :, 1] = -normals[:, :, 1]  # Flip Y
            
            # Dot product
            dot = np.sum(normals * light_dir, axis=2, keepdims=True)
            dot = np.clip(dot, -1.0, 1.0)
            
            # Lighting calculation with enhanced contrast
            raw_lighting = dot * 0.5 + 0.5  # 0 to 1
            # Gamma for shadow falloff
            raw_lighting = np.power(raw_lighting, 0.7)
            lighting = params['ambient'] + (1.0 - params['ambient']) * raw_lighting
            lighting = lighting * normal_strength + (1.0 - normal_strength)
        else:
            # No normal map - use flat lighting
            lighting = np.ones((1, 1, 1), dtype=np.float32) * 0.8
        
        # Color grading based on time of day
        if params['is_night']:
            # Night: strong blue tint
            combined_color = natural_rgb * 0.9 + ambient_rgb * 0.1
        else:
            # Day: mostly natural color
            combined_color = natural_rgb * 0.95 + ambient_rgb * 0.05
        
        lighting_rgb = lighting * combined_color * params['intensity']
        result = result * lighting_rgb
        
        # Apply depth of field
        if dof_amount > 0:
            result = self._apply_depth_of_field(result, focus_depth, dof_amount)
        
        # Apply album art cutout (restore original in center area)
        if cutout_size > 0:
            h, w = result.shape[:2]
            min_dim = min(h, w)
            
            # Calculate cutout bounds
            size_px = int(cutout_size * min_dim)
            cx = int(cutout_x * w)
            cy = int(cutout_y * h)
            
            x1 = max(0, cx - size_px // 2)
            x2 = min(w, cx + size_px // 2)
            y1 = max(0, cy - size_px // 2)
            y2 = min(h, cy + size_px // 2)
            
            # Restore original in cutout area
            result[y1:y2, x1:x2] = self.original[y1:y2, x1:x2]
        
        # Clamp and convert
        result = np.clip(result * 255.0, 0, 255).astype(np.uint8)
        
        # Draw cutout border for visibility (after uint8 conversion)
        if cutout_size > 0:
            h, w = result.shape[:2]
            min_dim = min(h, w)
            size_px = int(cutout_size * min_dim)
            cx = int(cutout_x * w)
            cy = int(cutout_y * h)
            x1 = max(0, cx - size_px // 2)
            x2 = min(w, cx + size_px // 2)
            y1 = max(0, cy - size_px // 2)
            y2 = min(h, cy + size_px // 2)
            cv2.rectangle(result, (x1, y1), (x2, y2), (100, 100, 100), 1)
        
        return result


class RelightUI:
    """Simple trackbar UI."""
    
    def __init__(self, image_dir: str, original_name: str = "Original"):
        self.image_dir = Path(image_dir)
        self.original_path = self._find_file(original_name)
        
        if self.original_path is None:
            raise ValueError(f"Could not find original image in {image_dir}")
        
        # Discover maps
        self.normal_maps = self._discover_maps("Normal")
        self.depth_maps = self._discover_maps("Depth")
        
        # Load original
        self.original = cv2.imread(str(self.original_path))
        if self.original is None:
            raise ValueError(f"Could not load: {self.original_path}")
        
        # Current state - default to normal map 1, depth map 4
        self.current_normal_idx = min(1, len(self.normal_maps) - 1) if len(self.normal_maps) > 0 else -1
        self.current_depth_idx = min(4, len(self.depth_maps) - 1) if len(self.depth_maps) > 0 else -1
        
        # Parameters
        self.hours = 12.0  # Noon (0-24)
        self.focus_depth = 0.5  # Mid-depth (0.0-1.0)
        self.dof_amount = 0.0
        self.normal_strength = 1.0  # Full normal map effect (0.0-1.0)
        self.brightness = 1.0  # Default to 1.0 (original brightness)
        
        # Animation
        self.animating = False
        self.anim_start_time = 0
        
        # Mercedes ambient animation
        self.merc_animating = False
        self.merc_start_time = 0
        # Subtle Mercedes interior colors (BGR format) - warm, understated
        self.merc_colors = [
            (0.85, 0.9, 0.95),   # Warm white
            (0.5, 0.6, 0.85),    # Soft amber
            (0.75, 0.65, 0.55),  # Muted ice blue
        ]
        self.merc_color_names = ["Warm White", "Soft Amber", "Ice Blue"]
        self._merc_status = ""
        
        # Dashboard ambient color (0.0-1.0 for each channel)
        self.ambient_r = 0.5
        self.ambient_g = 0.5
        self.ambient_b = 0.5
        
        # Album art cutout - disabled (0.0-1.0 for position/size)
        self.cutout_x = 0.0
        self.cutout_y = 0.0
        self.cutout_size = 0.0
        
        # Cache for display
        self._cached_normal_display = None
        self._cached_depth_display = None
        self._cached_normal_idx = -1
        self._cached_depth_idx = -1
        
        self.processor = None
        self._update_processor()
        
        print(f"\n=== Day/Night Relighting ===")
        print(f"Original: {self.original_path.name}")
        print(f"Normal maps: {len(self.normal_maps)}")
        print(f"Depth maps: {len(self.depth_maps)}")
        print(f"[Space] Save | [P] Play day/night | [M] Mercedes ambient | [ESC] Quit\n")
    
    def _find_file(self, pattern: str) -> Optional[Path]:
        matches = list(self.image_dir.glob(f"*{pattern}*.png"))
        # Filter out relit versions
        matches = [m for m in matches if "relit" not in m.name.lower()]
        return sorted(matches)[0] if matches else None
    
    def _discover_maps(self, prefix: str) -> List[Path]:
        # Find all files containing the prefix (handles leading spaces)
        maps = sorted([
            p for p in self.image_dir.glob(f"*{prefix}*.png")
            if "relit" not in p.name.lower()
        ])
        return maps
    
    def _update_processor(self):
        normal = None
        if 0 <= self.current_normal_idx < len(self.normal_maps):
            normal = cv2.imread(str(self.normal_maps[self.current_normal_idx]))
        
        depth = None
        if 0 <= self.current_depth_idx < len(self.depth_maps):
            depth = cv2.imread(str(self.depth_maps[self.current_depth_idx]))
        
        self.processor = RelightProcessor(self.original, normal, depth)
    
    def _get_current_normal_image(self) -> Optional[np.ndarray]:
        if 0 <= self.current_normal_idx < len(self.normal_maps):
            return cv2.imread(str(self.normal_maps[self.current_normal_idx]))
        return None
    
    def _get_current_depth_image(self) -> Optional[np.ndarray]:
        if 0 <= self.current_depth_idx < len(self.depth_maps):
            return cv2.imread(str(self.depth_maps[self.current_depth_idx]))
        return None
    
    def run(self):
        window_name = "Relighting"
        controls_window = "Controls"
        
        cv2.namedWindow(window_name, cv2.WINDOW_NORMAL)
        cv2.namedWindow(controls_window, cv2.WINDOW_AUTOSIZE)
        
        # Minimal image for trackbars
        cv2.imshow(controls_window, np.zeros((1, 400, 3), dtype=np.uint8))
        cv2.waitKey(1)
        
        def on_trackbar(val):
            pass
        
        # Create trackbars
        if len(self.normal_maps) > 0:
            cv2.createTrackbar("Normal Map", controls_window, self.current_normal_idx, 
                             max(1, len(self.normal_maps) - 1), on_trackbar)
        if len(self.depth_maps) > 0:
            cv2.createTrackbar("Depth Map", controls_window, self.current_depth_idx, 
                             max(1, len(self.depth_maps) - 1), on_trackbar)
        
        cv2.createTrackbar("Hour", controls_window, 120, 240, on_trackbar)  # 0-240 = 0-24 hours (10x for precision)
        cv2.createTrackbar("Brightness", controls_window, 100, 200, on_trackbar)
        cv2.createTrackbar("Focus Depth", controls_window, 50, 100, on_trackbar)
        cv2.createTrackbar("DOF Amount", controls_window, 0, 100, on_trackbar)
        cv2.createTrackbar("Normal Str", controls_window, 100, 100, on_trackbar)
        cv2.createTrackbar("Ambient R", controls_window, 50, 100, on_trackbar)
        cv2.createTrackbar("Ambient G", controls_window, 50, 100, on_trackbar)
        cv2.createTrackbar("Ambient B", controls_window, 50, 100, on_trackbar)
        cv2.createTrackbar("Cutout X", controls_window, 0, 100, on_trackbar)
        cv2.createTrackbar("Cutout Y", controls_window, 0, 100, on_trackbar)
        cv2.createTrackbar("Cutout Size", controls_window, 0, 50, on_trackbar)
        
        while True:
            # Read trackbars
            if len(self.normal_maps) > 0:
                new_idx = cv2.getTrackbarPos("Normal Map", controls_window)
                if new_idx != self.current_normal_idx:
                    self.current_normal_idx = new_idx
                    self._update_processor()
            
            if len(self.depth_maps) > 0:
                new_idx = cv2.getTrackbarPos("Depth Map", controls_window)
                if new_idx != self.current_depth_idx:
                    self.current_depth_idx = new_idx
                    self._update_processor()
            
            # Check for Mercedes ambient animation
            override_light = None
            merc_status = ""
            if self.merc_animating:
                now = time.time()
                elapsed = now - self.merc_start_time
                color_duration = 5.0  # 5 seconds per color
                cycle_duration = color_duration * len(self.merc_colors)
                
                if elapsed >= cycle_duration:
                    self.merc_animating = False
                else:
                    # Calculate which color and transition
                    color_idx = int(elapsed / color_duration) % len(self.merc_colors)
                    next_idx = (color_idx + 1) % len(self.merc_colors)
                    
                    # Smooth blend between colors
                    t = (elapsed % color_duration) / color_duration
                    t = t * t * (3 - 2 * t)  # Smoothstep
                    
                    c1 = self.merc_colors[color_idx]
                    c2 = self.merc_colors[next_idx]
                    blended = (
                        c1[0] * (1 - t) + c2[0] * t,
                        c1[1] * (1 - t) + c2[1] * t,
                        c1[2] * (1 - t) + c2[2] * t,
                    )
                    
                    # More dramatic light movement
                    light_rotations = 6.0  # Full rotations during animation
                    azimuth = (elapsed / cycle_duration) * 360 * light_rotations
                    # Slower, wider elevation sweep
                    elevation = 35 + 30 * np.sin(elapsed * 0.8)
                    
                    override_light = {
                        'azimuth': azimuth,
                        'elevation': elevation,
                        'color': blended,
                        'intensity': 1.0
                    }
                    merc_status = f" [{self.merc_color_names[color_idx]}]"
            
            # Check for day/night animation
            if self.animating:
                now = time.time()
                elapsed = now - self.anim_start_time
                cycle_duration = 21.0
                
                if elapsed >= cycle_duration:
                    self.animating = False
                    self.hours = 0
                    cv2.setTrackbarPos("Hour", controls_window, 0)
                    cv2.setTrackbarPos("DOF Amount", controls_window, 100)
                else:
                    # Smoothstep for butter-smooth animation
                    t = elapsed / cycle_duration
                    # Apply smoothstep: 3t² - 2t³
                    smooth_t = t * t * (3 - 2 * t)
                    self.hours = smooth_t * 24.0
                    
                    # DOF: only at night when sun is down (23:00 - 03:00)
                    h = self.hours
                    if h < 3:  # Night (before dawn)
                        dof_factor = 1.0
                    elif h < 4:  # Quick fade out at dawn
                        t = (h - 3) / 1.0
                        dof_factor = 1.0 - t * t * (3 - 2 * t)
                    elif h < 22:  # Day - no blur
                        dof_factor = 0.0
                    elif h < 23:  # Quick fade in at dusk
                        t = (h - 22) / 1.0
                        dof_factor = t * t * (3 - 2 * t)
                    else:  # Night (after dusk)
                        dof_factor = 1.0
                    self.dof_amount = dof_factor
            elif not self.merc_animating:
                self.hours = cv2.getTrackbarPos("Hour", controls_window) / 10.0
                self.dof_amount = cv2.getTrackbarPos("DOF Amount", controls_window) / 100.0
            
            # Always read these values
            self.brightness = cv2.getTrackbarPos("Brightness", controls_window) / 100.0
            self.focus_depth = cv2.getTrackbarPos("Focus Depth", controls_window) / 100.0
            self.normal_strength = cv2.getTrackbarPos("Normal Str", controls_window) / 100.0
            self.ambient_r = cv2.getTrackbarPos("Ambient R", controls_window) / 100.0
            self.ambient_g = cv2.getTrackbarPos("Ambient G", controls_window) / 100.0
            self.ambient_b = cv2.getTrackbarPos("Ambient B", controls_window) / 100.0
            self.cutout_x = cv2.getTrackbarPos("Cutout X", controls_window) / 100.0
            self.cutout_y = cv2.getTrackbarPos("Cutout Y", controls_window) / 100.0
            self.cutout_size = cv2.getTrackbarPos("Cutout Size", controls_window) / 100.0
            
            # Process
            result = self.processor.process(
                hours=self.hours,
                focus_depth=self.focus_depth,
                dof_amount=self.dof_amount,
                normal_strength=self.normal_strength,
                brightness=self.brightness,
                ambient_color=(self.ambient_r, self.ambient_g, self.ambient_b),
                cutout_x=self.cutout_x,
                cutout_y=self.cutout_y,
                cutout_size=self.cutout_size,
                override_light=override_light
            )
            
            # Store merc status for display
            self._merc_status = merc_status
            
            # Build composite display
            display = self._build_display(result)
            
            cv2.imshow(window_name, display)
            cv2.imshow(controls_window, np.zeros((1, 400, 3), dtype=np.uint8))
            
            # Minimal delay during animation for maximum smoothness
            wait_time = 8 if (self.animating or self.merc_animating) else 30
            key = cv2.waitKey(wait_time) & 0xFF
            if key == 27:  # ESC
                break
            elif key == ord(' '):
                output_path = self.image_dir / f"relit_{self.original_path.stem}.png"
                cv2.imwrite(str(output_path), result)
                print(f"Saved: {output_path}")
            elif key == ord('p'):  # Play day/night animation
                self.animating = True
                self.merc_animating = False  # Stop Mercedes if running
                self.anim_start_time = time.time()
                print("Playing day/night cycle (21 seconds)...")
            elif key == ord('m'):  # Play Mercedes ambient animation
                self.merc_animating = True
                self.animating = False  # Stop day/night if running
                self.merc_start_time = time.time()
                print("Playing Mercedes ambient (15 seconds)...")
        
        cv2.destroyAllWindows()
    
    def _build_display(self, result: np.ndarray) -> np.ndarray:
        """Build display with result + normal/depth maps below."""
        h, w = result.shape[:2]
        
        # Scale maps to fit below
        map_h = h // 3
        map_w = w // 2
        
        # Cache normal display
        if self._cached_normal_idx != self.current_normal_idx:
            normal_img = self._get_current_normal_image()
            if normal_img is not None:
                self._cached_normal_display = cv2.resize(normal_img, (map_w, map_h))
                if 0 <= self.current_normal_idx < len(self.normal_maps):
                    name = self.normal_maps[self.current_normal_idx].stem[:30]
                    cv2.putText(self._cached_normal_display, name, (5, 20), 
                               cv2.FONT_HERSHEY_SIMPLEX, 0.4, (255, 255, 255), 1)
            else:
                self._cached_normal_display = np.zeros((map_h, map_w, 3), dtype=np.uint8)
            self._cached_normal_idx = self.current_normal_idx
        
        # Cache depth display
        if self._cached_depth_idx != self.current_depth_idx:
            depth_img = self._get_current_depth_image()
            if depth_img is not None:
                self._cached_depth_display = cv2.resize(depth_img, (map_w, map_h))
                if 0 <= self.current_depth_idx < len(self.depth_maps):
                    name = self.depth_maps[self.current_depth_idx].stem[:30]
                    cv2.putText(self._cached_depth_display, name, (5, 20), 
                               cv2.FONT_HERSHEY_SIMPLEX, 0.4, (255, 255, 255), 1)
            else:
                self._cached_depth_display = np.zeros((map_h, map_w, 3), dtype=np.uint8)
            self._cached_depth_idx = self.current_depth_idx
        
        # Use cached displays
        normal_display = self._cached_normal_display
        depth_display = self._cached_depth_display
        
        # Handle size mismatch on first run
        if normal_display is None:
            normal_display = np.zeros((map_h, map_w, 3), dtype=np.uint8)
        if depth_display is None:
            depth_display = np.zeros((map_h, map_w, 3), dtype=np.uint8)
        
        # Combine maps horizontally
        maps_row = np.hstack([normal_display, depth_display])
        
        # Resize result to match maps width
        result_resized = cv2.resize(result, (map_w * 2, h))
        
        # Add time indicator
        time_str = self._get_time_string()
        cv2.putText(result_resized, time_str, (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 
                   0.7, (255, 255, 255), 2)
        
        # Stack vertically
        return np.vstack([result_resized, maps_row])
    
    def _get_time_string(self) -> str:
        """Convert hours (0-24) to readable time."""
        # Mercedes animation takes priority in display
        if self.merc_animating:
            merc_status = getattr(self, '_merc_status', '')
            return f"Mercedes Ambient{merc_status}"
        
        h = int(self.hours) % 24
        m = int((self.hours - int(self.hours)) * 60)
        
        if h < 5 or h >= 21:
            period = "Night"
        elif h < 7:
            period = "Dawn"
        elif h < 12:
            period = "Morning"
        elif h < 17:
            period = "Afternoon"
        elif h < 18.5:
            period = "Golden Hour"
        else:
            period = "Dusk"
        
        anim = " [PLAYING]" if self.animating else ""
        return f"{h:02d}:{m:02d} - {period}{anim}"


def main():
    parser = argparse.ArgumentParser(description="Day/Night relighting with DOF")
    parser.add_argument("image_dir", type=str)
    parser.add_argument("--original", type=str, default="Original")
    args = parser.parse_args()
    
    ui = RelightUI(args.image_dir, args.original)
    ui.run()


if __name__ == "__main__":
    main()
