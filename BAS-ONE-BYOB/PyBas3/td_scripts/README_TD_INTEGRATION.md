# TouchDesigner Integration Guide

This guide explains how to connect PyBas3 data (video streams, scores, pose data) to TouchDesigner.

## Overview

PyBas3 provides three data sources for TouchDesigner:
1. **NDI Video Streams**: `BAS_Participant_<UUID>` streams from Vision module
2. **Score JSON Files**: `scoring/output/participant_<uuid>_score.json` from Scoring module
3. **Pose Data** (optional): Shared memory buffer `bas_pose_data` from Vision module

## Quick Start

### 1. Setup Python Path in TouchDesigner

TouchDesigner needs to find the PyBas3 modules. In your TD Execute DAT:

**Option A: Add to Python path (recommended)**
```python
import sys
from pathlib import Path

# Add PyBas3 directory to path
sys.path.insert(0, r'/path/to/PyBas3')
```

**Option B: Use td_scripts directory**
```python
import sys
from pathlib import Path

# If Execute DAT is in td_scripts directory
td_scripts_dir = Path(op('.').path).parent / 'td_scripts'
sys.path.insert(0, str(td_scripts_dir.parent))
```

### 2. Use the Integration Script

Create an **Execute DAT** in TouchDesigner with Python mode:

```python
import td_integration

# Initialize once (in Start callback or first frame)
def onStart():
    td_integration.init(
        score_dir=None,  # Auto-detect, or specify: r'/path/to/scoring/output'
        enable_pose_data=False  # Set True to also read pose data
    )

# Poll data each frame
def onFrameStart(frame):
    data = td_integration.get_all_data()
    
    participants = data['participants']  # {uuid: ParticipantData}
    poses = data['poses']  # [pose_dict, ...]
    
    # Example: Get NDI stream names
    for uuid, pdata in participants.items():
        if pdata.ndi_source:
            # pdata.ndi_source = "BAS_Participant_<uuid>"
            # Use this name in NDI In TOP
            pass
    
    # Example: Get scores
    for uuid, pdata in participants.items():
        if pdata.score:
            score = pdata.score_value  # 0-100
            in_zone = pdata.in_zone  # bool
            pass
```

## Data Structures

### ParticipantData

```python
@dataclass
class ParticipantData:
    uuid: str
    ndi_source: Optional[str]  # NDI stream name (e.g., "BAS_Participant_abc12345")
    score: Optional[dict]  # Score JSON dict
    
    # Properties:
    in_zone: bool  # Is participant in scoring zone?
    score_value: float  # Score 0-100
```

### Score JSON Format

```json
{
  "uuid": "a1b2c3d4",
  "timestamp": 1704987825.123,
  "reference_frame": 12,
  "score_0_to_100": 52.1,
  "in_zone": true
}
```

### Pose Data Format

```python
{
    'uuid': 'a1b2c3d4',
    'timestamp': 1704987825.123,
    'keypoints': [(x, y, z, visibility), ...],  # 33 MediaPipe landmarks
    'in_zone': True
}
```

## API Reference

### Initialization

```python
td_integration.init(score_dir=None, enable_pose_data=False)
```

Initialize the integration. Call once at startup.

- `score_dir`: Optional path to scoring output directory (default: auto-detect)
- `enable_pose_data`: If True, also reads pose data from shared memory

### Getting Data

```python
# Get all data
data = td_integration.get_all_data()
# Returns: {'participants': {...}, 'poses': [...]}

# Get participants only (NDI + scores)
participants = td_integration.get_participants()
# Returns: {uuid: ParticipantData}

# Get poses only (optional)
poses = td_integration.get_poses()
# Returns: [pose_dict, ...]

# Get specific participant
pdata = td_integration.get_participant('abc12345')
# Returns: ParticipantData or None

# Get specific pose
pose = td_integration.get_pose('abc12345')
# Returns: pose dict or None
```

### Convenience Functions

```python
# Get NDI stream names
streams = td_integration.get_ndi_streams()
# Returns: {uuid: stream_name}

# Get scores
scores = td_integration.get_scores()
# Returns: {uuid: score_0_to_100}

# Get active UUIDs
uuids = td_integration.get_active_uuids()
# Returns: ['uuid1', 'uuid2', ...]
```

## TouchDesigner Network Setup

### NDI Video Streams

1. Add **NDI In TOP** to your network
2. In the Execute DAT, get stream names:
   ```python
   streams = td_integration.get_ndi_streams()
   # streams = {'abc12345': 'BAS_Participant_abc12345', ...}
   ```
3. Set NDI In TOP's source to the stream name (can be driven by CHOP or parameter)
4. Auto-update source name when participants change

### Score Data

1. Use `td_integration.get_scores()` to get scores
2. Drive parameters, CHOPs, or visualizations with score values
3. Use `in_zone` property to trigger effects

### Pose Data (Optional)

1. Enable pose data: `td_integration.init(enable_pose_data=True)`
2. Get poses: `poses = td_integration.get_poses()`
3. Extract keypoints for CHOPs or geometry

## Example: Complete Setup

```python
# Execute DAT: Initialize
def onStart():
    import td_integration
    td_integration.init(enable_pose_data=False)
    
    # Store in TD global storage
    me.store('td_int', td_integration)

# Execute DAT: Poll Data (each frame)
def onFrameStart(frame):
    td_int = me.fetch('td_int')
    data = td_int.get_all_data()
    
    participants = data['participants']
    
    # Update NDI In TOP sources (example)
    for uuid, pdata in participants.items():
        if pdata.ndi_source:
            # Find or create NDI In TOP for this UUID
            # Set source parameter to pdata.ndi_source
            pass
    
    # Drive score parameters (example)
    scores = td_int.get_scores()
    for uuid, score in scores.items():
        # Update parameter or CHOP
        # op('score_param')[uuid] = score
        pass
```

## Troubleshooting

### "td_participant_manager not available"
- Ensure PyBas3 directory is in Python path
- Check that `td_scripts/` directory exists

### "Shared memory not available"
- Vision module must be running
- This is expected if pose data is disabled (default)

### NDI streams not appearing
- Check that Vision module is running
- Verify NDI library is installed: `pip install ndi-python`
- Check network connectivity

### Score files not updating
- Check that Scoring module is running
- Verify score directory path: `scoring/output/`
- Check file permissions

## Dependencies

Required:
- `ndi-python` (for NDI discovery)
- Python standard library

Optional:
- Shared memory access (for pose data)

Install dependencies in TouchDesigner's Python environment or system Python if TD uses system Python.
