# Agent 0: Orchestrator

**active:** no  
**last_updated:** 2026-01-12T23:50:00Z  
**task:** Integration testing + NDI fixes complete

## Summary
- Fixed NDI API compatibility (`ndi.FindCreate()`, `video_frame.data`)
- Per-participant NDI streams working
- Zone UI integrated into detector (sliders, click-to-set, Z-depth viz)
- Segmentation mask tuned for tighter body contour
- All 4 integration tests passing

## Test Status
```
4/4 tests passed
  ✓ Shared Memory Round-Trip
  ✓ Scoring Reads Shared Memory
  ✓ TD Score Watcher
  ✓ TD NDI UUID Parsing
```

## Files Touched
- `mediapipe/multi_person_detector.py` (zone UI, segmentation tuning, --persist flag)
- `mediapipe/ndi_streamer.py` (API fixes, per-participant streams)
- `common/shared_memory.py` (UUID strip fix)
- `.cursor/rules/pybas3.mdc` (removed commit instructions, added test rules)
- `agents/AGENT_0_SHARED.md` (removed commit instructions)
