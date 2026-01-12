# Shared Contracts

> **Technical details:** `../TECHNICAL_REFERENCE.md`  
> **Year:** 2026 - All agents should be aware of current date/time context

## Agent Roster
| Emoji | Nickname | Lane | Status File |
|-------|----------|------|-------------|
| 👁️ | Iris | Vision | `status/agent_1_vision.md` |
| 🎯 | Judge | Scoring | `status/agent_2_scoring.md` |
| 🎨 | Canvas | TouchDesigner | `status/agent_3_touchdesigner.md` |

## Common Module

**Shared protocols and constants:** `../common/`

Both Vision and Scoring modules use `common/` for:
- Protocol definitions (`common/protocols.py`)
- Binary encoding/decoding (`common/shared_memory.py`)
- Data structures (`ParticipantPose`, `PoseKeypoint`)
- Constants (`SHARED_MEMORY_BUFFER_NAME`, `POSE_BUFFER_SIZE`, etc.)

**Rule:** When modifying shared memory format or data structures, update `common/` first, then update dependent modules.

## I/O Contracts

| From | To | Channel | Format |
|------|-----|---------|--------|
| 👁️ | 🎨 | NDI | `BAS_Participant_<UUID>` streams |
| 👁️ | 🎯 | Shared mem | `bas_pose_data` buffer (via `common/`) |
| 🎯 | 🎨 | Files | `scoring/output/participant_<uuid>_score.json` |
| 👁️ | restart | File | `mediapipe/participants_db.json` (atomic writes) |

## Process Boundaries
- Each module: independent `python <entrypoint>.py`
- No shared Python state
- Communicate only via: NDI, shared memory, files
- Shared code: `common/` module (protocols, constants, encoding)

## Testing Requirements
- Agents must test code changes before spinning down
- If testing not verifiable (hardware, external dependencies, integration) → flag user and pause
- Document test status in status file notes field
