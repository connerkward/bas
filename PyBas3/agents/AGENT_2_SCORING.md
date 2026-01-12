# 🎯 Judge (Scoring) — Process 2

> **Technical details:** `../TECHNICAL_REFERENCE.md` § Module 2

## Goal
Read poses from shared memory, compare to reference video, write per-participant score JSON.

## Owns
`PyBas3/scoring/`: `pose_scorer.py`, `reference_builder.py`, `shared_memory_reader.py`, `output/`

## Inputs
- Shared mem: `bas_pose_data` (from 👁️ Iris, via `common/shared_memory.py`)
- `reference_poses.json` (built from reference video)
- `common/` module (shared protocols)

## Outputs
- `scoring/output/participant_<uuid>_score.json` (overwritten)

## Commands
```bash
# Build reference (one-time)
python reference_builder.py ../pre_render/output/videos/runside.mp4

# Run scorer
python pose_scorer.py
```

## Done check
- Participant appears → score JSON appears/updates
- Participant leaves → score file stops updating (stale timestamp)
