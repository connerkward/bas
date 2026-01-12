# 👁️ Iris (Vision) — Process 1

> **Technical details:** `../TECHNICAL_REFERENCE.md` § Module 1

## Goal
Multi-person detection with stable UUIDs, zone gating, segmentation, NDI streaming, shared-memory pose output.

## Owns
`PyBas3/mediapipe/`: `multi_person_detector.py`, `ndi_streamer.py`, `shared_memory_writer.py`, `zone_config.json`, `participants_db.json`

## Inputs
- Webcam / capture device
- `zone_config.json` (live-tunable)
- `participants_db.json` (optional, for UUID persistence)

## Outputs
- **NDI:** `BAS_Participant_<UUID>` per participant
- **Shared mem:** `bas_pose_data` buffer (via `common/shared_memory.py`)
- **File:** `participants_db.json`

## Common Module
Uses `common/` module for shared memory protocol definitions. When implementing `SharedMemoryPoseWriter`, import from `common/protocols.py` and `common/shared_memory.py`.

## Done check
- 0 people → no participant NDI streams
- 1–3 in zone → stable UUIDs, matching NDI streams
- Restart → UUIDs persist via `participants_db.json`
