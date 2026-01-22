# Shared Contracts & Protocols

> **Tasks:** [Linear BAS project](https://linear.app/ckward-workspace/project/bas-d90c3a3ab597) · **Tech:** `../TECHNICAL_REFERENCE.md`

## I/O

| From | To | Channel | Format |
|------|-----|---------|--------|
| Vision | TouchDesigner | NDI | `BAS_Participant_<UUID>` |
| Vision | Scoring | Shared mem | `bas_pose_data` (via `common/`) |
| Scoring | TouchDesigner | Files | `scoring/output/participant_<uuid>_score.json` |
| Vision | restart | File | `participants_db.json` |

## Common Module
`common/`: protocols, shared_memory, constants. Vision + Scoring use it. Change `common/` first, then dependents.

## Process Boundaries
No shared Python state. Communicate via NDI, shared memory, files.

## Testing
Test changes before completing. If not verifiable → flag user.
