# PyBas3 TouchDesigner Execute DAT Script
# Sync this file to an Execute DAT with Frame Start = On

import sys
import json
import re
from pathlib import Path

# === CONFIG ===
PYBAS3_PATH = Path('/Users/CONWARD/dev/bas/PyBas3')
SCORE_DIR = PYBAS3_PATH / 'scoring' / 'output'
NDI_STREAM_PATTERN = re.compile(r'BAS_Participant_([a-f0-9]{8})')
MAX_SLOTS = 10  # Fixed NDI In TOP slots (ndi_in1 through ndi_in10)

# Speed control: avg score 0 → MIN_SPEED, avg score MAX_SCORE_FOR_FULL_SPEED → 1.0
MIN_SPEED = 0.1
MAX_SPEED = 1.0
MAX_SCORE_FOR_FULL_SPEED = 25  # Avg score needed for full speed (temp reduced from 100)

# Video TOPs to control speed on
VIDEO_TOPS = [
    'video_rainbow_trail', 'video_microres', 'video_lowres', 
    'video_dithered', 'video_depth', 'video_red_overlay', 'video_atkinson',
    'video_skeleton'
]

# === STATE ===
_scores = {}
_ndi_sources = {}
_slot_assignments = {}  # slot_num -> uuid (tracks which slot has which participant)
_initialized = False
_current_speed = MIN_SPEED  # Current playback speed (smoothed)
_target_speed = MIN_SPEED   # Target speed from score calculation
_accumulated_index = 0.0    # Smoothly accumulated video index

# Smoothing parameters: fast attack (score rises), slow decay (score falls)
ATTACK_RATE = 0.08   # How fast speed rises (0-1, higher = faster) - reduced for smoother
DECAY_RATE = 0.015   # How fast speed falls back (0-1, lower = slower decay)

def _poll_scores():
    """Read score JSON files, ignoring stale ones (>5 sec old)."""
    global _scores
    _scores = {}
    
    if not SCORE_DIR.exists():
        return
    
    import time
    now = time.time()
    
    for f in SCORE_DIR.iterdir():
        if f.name.startswith('participant_') and f.name.endswith('_score.json'):
            try:
                # Skip files older than 5 seconds
                if now - f.stat().st_mtime > 5:
                    continue
                    
                uuid = f.name.replace('participant_', '').replace('_score.json', '')
                with open(f, 'r') as fp:
                    _scores[uuid] = json.load(fp)
            except:
                pass

def _poll_ndi():
    """Discover NDI sources (requires ndi-python in TD's Python)."""
    global _ndi_sources
    _ndi_sources = {}
    
    try:
        import NDIlib as ndi
        if not ndi.initialize():
            return
        
        finder = ndi.find_create_v2()
        if finder:
            ndi.find_wait_for_sources(finder, 100)  # 100ms timeout
            sources = ndi.find_get_current_sources(finder)
            if sources:
                for s in sources:
                    match = NDI_STREAM_PATTERN.search(s.ndi_name)
                    if match:
                        _ndi_sources[match.group(1)] = s.ndi_name
            ndi.find_destroy(finder)
    except ImportError:
        pass  # NDI not available in TD
    except:
        pass

def _calculate_speed():
    """Calculate playback speed with smoothing (fast attack, slow decay)."""
    global _current_speed, _target_speed
    
    # Calculate target speed from scores
    if not _scores:
        _target_speed = MIN_SPEED
    else:
        # Only count participants who are in the zone
        in_zone_scores = [s.get('score_0_to_100', 0) for s in _scores.values() if s.get('in_zone', False)]
        
        if not in_zone_scores:
            _target_speed = MIN_SPEED
        else:
            avg_score = sum(in_zone_scores) / len(in_zone_scores)
            # Map: 0 → MIN_SPEED, MAX_SCORE_FOR_FULL_SPEED → MAX_SPEED
            normalized = min(avg_score / MAX_SCORE_FOR_FULL_SPEED, 1.0)
            _target_speed = MIN_SPEED + normalized * (MAX_SPEED - MIN_SPEED)
    
    # Apply smoothing: fast attack, slow decay
    if _target_speed > _current_speed:
        # Rising - use attack rate
        _current_speed += (_target_speed - _current_speed) * ATTACK_RATE
    else:
        # Falling - use slower decay rate  
        _current_speed += (_target_speed - _current_speed) * DECAY_RATE
    
    # Clamp to valid range
    _current_speed = max(MIN_SPEED, min(MAX_SPEED, _current_speed))
    
    return _current_speed

def _get_effective_speed():
    """Get speed value, checking for manual override."""
    global _current_speed
    ctrl = op('/project1/speed_control')
    if ctrl and len(ctrl.chans()) >= 2:
        # Access by index since chan() returns value not channel object
        override_val = ctrl[1].eval()  # override is channel 1
        manual_val = ctrl[0].eval()    # manual_speed is channel 0
        if override_val > 0.5:
            return max(MIN_SPEED, min(MAX_SPEED, manual_val))
    return _current_speed

def _update_video_speeds():
    """Update video index - speed=1.0 plays 1 video frame per TD frame (60fps playback)."""
    effective_speed = _get_effective_speed()
    
    # Get current accumulated index
    current_index = op('/project1').fetch('pybas3_index', 0.0)
    
    # At speed=1.0, advance 1 video frame per TD frame (60fps playback)
    # At speed=0.1, advance 0.1 video frames per TD frame (6fps = slow motion)
    frame_increment = effective_speed
    new_index = current_index + frame_increment
    
    # Store for video TOPs to read
    op('/project1').store('pybas3_index', new_index)
    op('/project1').store('pybas3_speed', effective_speed)

def _update_score_display():
    """Update the score_display Text TOP with current stats."""
    display = op('/project1/score_display')
    if not display:
        return
    
    effective = _get_effective_speed()
    ctrl = op('/project1/speed_control')
    override = 0
    if ctrl and len(ctrl.chans()) >= 2:
        override = ctrl[1].eval()  # override is channel 1
    mode = "MANUAL" if override > 0.5 else "auto"
    
    lines = []
    lines.append(f"Speed: {effective:.2f}x [{mode}]")
    lines.append(f"(auto: {_current_speed:.2f}x target: {_target_speed:.2f}x)")
    lines.append("")
    
    if _scores:
        in_zone = [s for s in _scores.values() if s.get('in_zone', False)]
        in_zone_scores = [s.get('score_0_to_100', 0) for s in in_zone]
        avg = sum(in_zone_scores) / len(in_zone_scores) if in_zone_scores else 0
        lines.append(f"In Zone: {len(in_zone)}  Avg: {avg:.1f}")
        lines.append("-" * 25)
        for uuid, score in _scores.items():
            val = score.get('score_0_to_100', 0)
            zone = "ZONE" if score.get('in_zone', False) else "out"
            slot = next((s for s, u in _slot_assignments.items() if u == uuid), None)
            slot_str = f"[{slot}]" if slot else ""
            lines.append(f"{uuid[:8]} {slot_str}: {val:5.1f} {zone}")
    else:
        lines.append("No participants")
    
    display.par.text = "\\n".join(lines)

def _update_ndi_slots():
    """Auto-assign/clear NDI In TOP slots based on active participants."""
    global _slot_assignments
    
    # Get all active UUIDs (from scores, since NDI discovery may not work in TD)
    active_uuids = set(_scores.keys())
    
    # Clear slots for participants that left
    for slot_num, uuid in list(_slot_assignments.items()):
        if uuid not in active_uuids:
            ndi_op = parent().op(f'ndi_in{slot_num}')
            if ndi_op and hasattr(ndi_op.par, 'name'):
                ndi_op.par.name = ""  # Clear source
                print(f"[PyBas3] Cleared ndi_in{slot_num} (participant {uuid} left)")
            del _slot_assignments[slot_num]
    
    # Find UUIDs that need a slot
    assigned_uuids = set(_slot_assignments.values())
    unassigned_uuids = active_uuids - assigned_uuids
    
    # Find available slots
    used_slots = set(_slot_assignments.keys())
    available_slots = [i for i in range(1, MAX_SLOTS + 1) if i not in used_slots]
    
    # Assign unassigned UUIDs to available slots
    for uuid in unassigned_uuids:
        if not available_slots:
            break  # No more slots
        
        slot_num = available_slots.pop(0)
        _slot_assignments[slot_num] = uuid
        
        # Build the NDI source name (matches Python backend format)
        # Format: "HOSTNAME (BAS_Participant_UUID)"
        # We'll set it to just the stream name pattern and let TD find it
        source_name = f"BAS_Participant_{uuid}"
        
        # Check if we have the full source name from NDI discovery
        if uuid in _ndi_sources:
            source_name = _ndi_sources[uuid]
        
        ndi_op = parent().op(f'ndi_in{slot_num}')
        if ndi_op and hasattr(ndi_op.par, 'name'):
            current = ndi_op.par.name.eval()
            if current != source_name:
                ndi_op.par.name = source_name
                print(f"[PyBas3] Assigned ndi_in{slot_num} -> {uuid}")

# === TD CALLBACKS ===

def onStart():
    """Called when Execute DAT starts."""
    global _initialized
    _initialized = True
    print(f"[PyBas3] Initialized - watching {SCORE_DIR}")
    print(f"[PyBas3] Managing {MAX_SLOTS} NDI slots")
    _poll_scores()
    _poll_ndi()
    _update_ndi_slots()
    print(f"[PyBas3] Found {len(_scores)} scores, {len(_ndi_sources)} NDI streams")

def onFrameStart(frame):
    """Called every frame."""
    global _initialized
    
    # Initialize on first frame if onStart didn't run
    if not _initialized:
        onStart()
    
    # Update video index EVERY frame for smooth playback
    _update_video_speeds()
    
    # Poll scores/NDI less often (~6x/sec at 60fps)
    if frame % 10 == 0:
        _poll_scores()
        _update_ndi_slots()
        _calculate_speed()
        _update_score_display()
        
        # NDI discovery is slow, do it less often
        if frame % 60 == 0:
            _poll_ndi()
    
    # Store data for other operators to access
    op('/project1').store('pybas3_scores', _scores)
    op('/project1').store('pybas3_ndi', _ndi_sources)
    op('/project1').store('pybas3_slots', _slot_assignments.copy())
    # pybas3_speed is stored in _update_video_speeds with effective speed
    
    # Debug output every 2 seconds
    if frame % 60 == 0:
        if _scores or _ndi_sources:
            avg = sum(s.get('score_0_to_100', 0) for s in _scores.values()) / max(len(_scores), 1)
            print(f"[PyBas3] {len(_scores)} participants, avg:{avg:.1f}, speed:{_current_speed:.2f}x")
            for uuid, score in _scores.items():
                val = score.get('score_0_to_100', 0)
                in_zone = score.get('in_zone', False)
                slot = next((s for s, u in _slot_assignments.items() if u == uuid), None)
                slot_str = f"slot {slot}" if slot else "no slot"
                print(f"  {uuid}: {val:.0f} {'(zone)' if in_zone else ''} [{slot_str}]")

# === HELPER FUNCTIONS (call from other DATs via op('pybas3_exec').module) ===

def get_scores():
    """Get all scores: {uuid: score_dict}"""
    return _scores.copy()

def get_ndi_sources():
    """Get NDI sources: {uuid: stream_name}"""
    return _ndi_sources.copy()

def get_slot_assignments():
    """Get slot assignments: {slot_num: uuid}"""
    return _slot_assignments.copy()

def get_participant_uuids():
    """Get list of active UUIDs."""
    return list(set(_scores.keys()) | set(_ndi_sources.keys()))

def get_score(uuid):
    """Get score for specific UUID (0-100)."""
    return _scores.get(uuid, {}).get('score_0_to_100', 0)

def get_in_zone(uuid):
    """Check if participant is in zone."""
    return _scores.get(uuid, {}).get('in_zone', False)

def get_slot_for_uuid(uuid):
    """Get slot number for a UUID, or None if not assigned."""
    for slot, u in _slot_assignments.items():
        if u == uuid:
            return slot
    return None

def get_uuid_for_slot(slot_num):
    """Get UUID assigned to a slot, or None if empty."""
    return _slot_assignments.get(slot_num)

def get_playback_speed():
    """Get current playback speed (0.1 to 1.0 based on avg score)."""
    return _current_speed

def get_avg_score():
    """Get average score across all participants."""
    if not _scores:
        return 0
    return sum(s.get('score_0_to_100', 0) for s in _scores.values()) / len(_scores)
