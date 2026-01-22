# PyBas3 Technical Reference

> **Task tracking lives in:** `agents/SESSION.md`  
> **Agent protocols:** `agents/AGENTIC_INSTRUCTIONS.md`

## Installation Context
**Dark gallery environment** - low ambient light, primary illumination from projection.
- **RGB camera only** (no hardware depth sensors)
- **MediaPipe Pose z-coordinates** for depth filtering (no extra model)
- Depth filter applied BEFORE segmentation to reduce false positives
- pHash identity relies on silhouette more than color (works in low light)

## Overview

Modular system for interactive projection mapping with multi-person pose tracking.

**Build Priority:** MediaPipe → Scoring → TD integration

**Duration:** Multi-hour continuous runtime required

---

## System Architecture

### Processes
1. **Process 1:** MediaPipe vision (writes to shared memory + NDI)
2. **Process 2:** Scoring (reads from shared memory)
3. **Process 3:** Strava connector *(LOW PRIORITY)*
4. **Process 4:** Recording *(LOW PRIORITY)*
5. **TouchDesigner:** Reads NDI streams and files

### Communication
| Channel | Method | Use |
|---------|--------|-----|
| Video | NDI streams `BAS_Participant_<UUID>` | Vision → TD |
| Pose data | Shared memory `bas_pose_data` | Vision → Scoring |
| Scores | JSON files `participant_<uuid>_score.json` | Scoring → TD |
| Identity | `participants_db.json` | Persist UUIDs across restarts |

### Participant Tracking
- Perceptual hash (pHash) of upper body region
- Max 3 simultaneous participants
- UUID assigned on first detection, matched on return
- Persisted via `participants_db.json` (atomic writes)

---

## Project Structure

```
PyBas3/
├── TECHNICAL_REFERENCE.md          # This file
├── requirements.txt
├── agents/                         # Agent coordination
│   ├── SESSION.md                  # Task tracking + handoff
│   └── ...
├── common/                         # Shared protocols & constants
│   ├── __init__.py
│   ├── protocols.py                # Data structures & constants
│   └── shared_memory.py            # Binary protocol encoding/decoding
├── mediapipe/                      # Process 1
│   ├── multi_person_detector.py
│   ├── ndi_streamer.py
│   ├── shared_memory_writer.py
│   ├── zone_config.json
│   └── participants_db.json
├── scoring/                        # Process 2
│   ├── pose_scorer.py
│   ├── reference_builder.py
│   ├── shared_memory_reader.py
│   └── output/participant_<uuid>_score.json
├── pre_render/                     # Offline pipeline (migrated)
│   ├── depth_blend_video.py
│   └── frames_to_video.py
```
└── td_scripts/                     # TouchDesigner (migrated)
    └── ConnerTD/ConnerTD.toe
```

---

## Module 1: MediaPipe Vision (Process 1)

### Purpose
Multi-person detection with unique participant tracking, zone isolation, segmentation, pose tracking, shared memory output, and NDI streaming.

### Reliability Gates (reduce false positives)
- 2D screen region check (taped box)
- Pose visibility threshold
- Segmentation mask area threshold
- Optional: pose z-range heuristic

All thresholds live-tunable via `zone_config.json`

### Zone Configuration
```json
{
  "screen_region": { "x": 0.3, "y": 0.4, "width": 0.4, "height": 0.5 },
  "z_range": { "min": -0.5, "max": 0.5 },
  "max_people": 3
}
```

### Outputs
- **NDI:** `BAS_Participant_<UUID>` per participant
- **Shared Memory:** `bas_pose_data` buffer
- **File:** `participants_db.json`

### pHash Implementation
```python
class ParticipantTracker:
    def __init__(self, hash_size=8, threshold=10):
        self.hash_size = hash_size
        self.threshold = threshold  # Hamming distance
        self.participants = {}  # uuid -> phash
        
    def compute_phash(self, image):
        resized = cv2.resize(image, (self.hash_size + 1, self.hash_size))
        gray = cv2.cvtColor(resized, cv2.COLOR_BGR2GRAY) if len(resized.shape) == 3 else resized
        diff = gray[:, 1:] > gray[:, :-1]
        return diff.flatten()
    
    def match_or_create(self, frame, pose_landmarks):
        crop = self.get_upper_body_crop(frame, pose_landmarks, frame.shape)
        if crop.size == 0:
            return None
        new_hash = self.compute_phash(crop)
        
        for uuid, stored_hash in self.participants.items():
            if self.hamming_distance(new_hash, stored_hash) < self.threshold:
                self.participants[uuid] = new_hash  # Update
                return uuid
        
        new_uuid = str(uuid4())[:8]
        self.participants[new_uuid] = new_hash
        return new_uuid
```

### Shared Memory Writer
Uses `common` module for protocol definitions:

```python
from common.protocols import SHARED_MEMORY_BUFFER_NAME, POSE_BUFFER_SIZE, ParticipantPose
from common.shared_memory import encode_pose

class SharedMemoryPoseWriter:
    def __init__(self):
        from multiprocessing import shared_memory
        self.shm = shared_memory.SharedMemory(
            name=SHARED_MEMORY_BUFFER_NAME,
            create=True,
            size=POSE_BUFFER_SIZE
        )
    
    def write_poses(self, participant_poses: List[ParticipantPose]):
        buffer = bytearray(POSE_BUFFER_SIZE)
        offset = 0
        for pose in participant_poses:
            offset = encode_pose(pose, buffer, offset)
        self.shm.buf[:] = buffer
```

---

## Module 2: Scoring (Process 2)

### Purpose
Read poses from shared memory, compare to reference video, output per-participant scores.

### Pre-Processing
```bash
python reference_builder.py ../pre_render/output/videos/runside.mp4
# Creates reference_poses.json
```

### Score File Format
```json
{
  "uuid": "a1b2c3d4",
  "timestamp": 1704987825.123,
  "reference_frame": 12,
  "score_0_to_100": 52.1,
  "in_zone": true
}
```

### Shared Memory Reader
Uses `common` module for protocol definitions:

```python
from common.protocols import SHARED_MEMORY_BUFFER_NAME, MAX_PARTICIPANTS, POSE_RECORD_SIZE
from common.shared_memory import decode_pose

class SharedMemoryPoseReader:
    def __init__(self):
        from multiprocessing import shared_memory
        self.shm = shared_memory.SharedMemory(name=SHARED_MEMORY_BUFFER_NAME)
        self.record_size = POSE_RECORD_SIZE
        self.max_participants = MAX_PARTICIPANTS
    
    def read_poses(self) -> List[dict]:
        poses = []
        offset = 0
        for _ in range(self.max_participants):
            pose = decode_pose(self.shm.buf, offset)
            if pose:
                poses.append(pose.to_dict())
            offset += self.record_size
        return poses
```

---

## Module 3: Strava *(LOW PRIORITY)*

Text file outputs for TD:
- `comments.txt` - one per line
- `stats.txt` - CSV: miles_run|likes|comments
- `gps_coords.txt` - CSV: lat,lng
- `heart_rate.txt` - one BPM per line

---

## Module 4: Recording *(LOW PRIORITY)*

Per-participant video capture from NDI streams with JSON metadata.

---

## TouchDesigner Integration

**Read NDI:** NDI In TOP → `BAS_Participant_<UUID>`  
**Read Scores:** Text DAT → `scoring/output/participant_<uuid>_score.json`  
**Discover Participants:** Enumerate NDI sources, parse UUID from stream name

---

## Dependencies

**Package Manager:** UV (`uv`)

```bash
# Install dependencies
cd PyBas3 && uv sync

# Run scripts
uv run python mediapipe/multi_person_detector.py
```

Packages (defined in `pyproject.toml`):
- opencv-python>=4.8.0
- mediapipe>=0.10.0
- numpy>=1.24.0
- ndi-python>=1.1.0
- requests>=2.31.0
- Pillow>=10.0.0

---

## Pre-Render Pipeline

### Depth Map Processing Issues (2026-01-21)

**Problem:** Background showing high depth values when only figure/ground should be bright.

**Attempted fixes:**
1. Applied subject mask to regular videos (not just greenscreen) - caused figure clipping
2. Used depth percentile thresholding (75th percentile) with morphological ops - created "fin" artifacts between limbs
3. Soft depth scaling: scales background to 30% depth instead of hard thresholding
   - Uses depth percentiles (40th-70th) for smooth transition
   - Avoids morphological operations that create artifacts
   - Preserves figure depth while reducing background proportionally

**Current state:** Soft scaling implemented, needs testing/refinement.

**TODOs:**
- Fix green screen edge artifacts
- Refine depth map threshold parameters

---

## Testing

### Module Tests
```bash
# Vision
cd PyBas3/mediapipe && python test_mediapipe.py --display

# Scoring
cd PyBas3/scoring && python reference_builder.py ../pre_render/output/videos/runside.mp4
cd PyBas3/scoring && python test_scoring.py
```

### Integration Test
Run all modules in separate terminals, verify in TD:
- NDI streams appear
- Score files update
- System stable 2+ hours

---

## Success Criteria

- [ ] MediaPipe detects 1-3 people reliably
- [ ] pHash assigns stable UUIDs
- [ ] Zone detection filters correctly
- [ ] NDI streams created per participant
- [ ] Shared memory transfer working
- [ ] Scoring compares to reference
- [ ] Per-participant score files update
- [ ] System stable 3+ hours
- [ ] Zone config live-editable
- [x] Pre-render scripts migrated
- [x] TD project migrated
