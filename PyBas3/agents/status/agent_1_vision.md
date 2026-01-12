# 👁️ Iris (Vision) Status

**active:** no
**last_updated:** 2026-01-12T16:00:00Z
**task:** SharedMemoryPoseWriter implemented, integrated, and tested
**test_status:** ✅ All tests passed
  - MultiPersonDetector integration: working
  - Vision → Scoring shared memory communication: verified
  - Temp UUID filtering: working (only real UUIDs written)
  - Data integrity: UUIDs, keypoints (33), in_zone flags preserved

**notes:** 
- ✅ MediaPipe multi-person detection + face-based pHash UUIDs
- ✅ SharedMemoryPoseWriter (`mediapipe/shared_memory_writer.py`) - writes pose data to shared memory for Scoring module
- ✅ Integrated into MultiPersonDetector (writes after each detection, filters temp UUIDs)
- ✅ NDI streaming code integrated (pending ndi-python dependency install)
- ✅ Thumbnail generation (one per participant UUID)
- ✅ Fixed duplicate code issue (removed duplicate classes/main functions)
- ✅ Improved face-based pHash matching (threshold increased to 30 for better stability)
