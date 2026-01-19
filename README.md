# Bas

Movement captured. Bodies tracked. Scores rendered.

An experimental system for real-time pose analysis and projection mapping in dark gallery environments.

![Bas](./artifacts/Screenshot%202025-12-18%20at%204.45.07%20%E2%80%8FPM.png)

[Watch video](artifacts/basvid_v1.mov)

---

## Pipeline

**Multi-process architecture** — vision, scoring, and rendering run as independent processes communicating through shared memory, NDI streams, and atomic file writes.

### Vision Process
MediaPipe Pose detection with perceptual hashing for participant identity. Zone filtering via 2D screen regions and pose z-coordinates. Segmentation masks isolate participants before pose extraction. Each participant assigned a stable UUID persisted across restarts.

**Outputs:** Per-participant NDI streams (`BAS_Participant_<UUID>`), shared memory pose buffer, participant database JSON.

### Scoring Process
Reads pose data from shared memory, compares against reference video frames using landmark distance metrics. Writes per-participant score JSON files with timestamps and zone status.

**Outputs:** `participant_<uuid>_score.json` files with 0-100 scores, reference frame indices, zone flags.

### TouchDesigner Integration
Consumes NDI video streams and score JSON files. Dynamic participant discovery via NDI source enumeration. Score data accessible through DAT scripts for real-time visual effects.

---

## Technology

- **MediaPipe Pose** — 33-point landmark detection, segmentation, depth estimation
- **NDI** — Network Device Interface for low-latency video streaming
- **Shared Memory** — Binary protocol for inter-process pose data transfer
- **Perceptual Hashing** — Upper-body silhouette matching for identity persistence
- **TouchDesigner** — Real-time visual programming for projection mapping

**Environment:** RGB camera only, low ambient light, projection-based illumination. Depth filtering via MediaPipe z-coordinates, no hardware depth sensors required.

---

*Technical testing. Digital farm.*
