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

# === STATE ===
_scores = {}
_ndi_sources = {}
_slot_assignments = {}  # slot_num -> uuid (tracks which slot has which participant)
_initialized = False

def _poll_scores():
    """Read score JSON files."""
    global _scores
    _scores = {}
    
    if not SCORE_DIR.exists():
        return
    
    for f in SCORE_DIR.iterdir():
        if f.name.startswith('participant_') and f.name.endswith('_score.json'):
            try:
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
    
    # Poll every 10 frames (~3x/sec at 30fps)
    if frame % 10 == 0:
        _poll_scores()
        _update_ndi_slots()
        
        # NDI discovery is slow, do it less often
        if frame % 60 == 0:
            _poll_ndi()
    
    # Store data for other operators to access
    parent().store('pybas3_scores', _scores)
    parent().store('pybas3_ndi', _ndi_sources)
    parent().store('pybas3_slots', _slot_assignments.copy())
    
    # Debug output every 2 seconds
    if frame % 60 == 0:
        if _scores or _ndi_sources:
            print(f"[PyBas3] {len(_scores)} scores, {len(_slot_assignments)} slots assigned")
            for uuid, score in _scores.items():
                val = score.get('score_0_to_100', 0)
                in_zone = score.get('in_zone', False)
                # Find which slot this uuid is in
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
