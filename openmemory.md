# OpenMemory Guide

## Overview
- PyBas3 is a multi-process system for multi-person pose tracking, scoring, and TouchDesigner integration.
- MediaPipe detection assigns stable participant UUIDs and publishes per-participant pose data to shared memory and NDI streams.

## Architecture
- Modules run as separate processes and communicate via shared memory, NDI, and files.
- `PyBas3/mediapipe` performs detection, segmentation, UUID assignment, and NDI streaming.
- `PyBas3/scoring` reads shared memory poses and writes per-participant score JSON files.
- `PyBas3/td_scripts` consumes NDI + score JSONs for TouchDesigner integration.

## User Defined Namespaces
- 

## Components
- `MultiPersonDetector` (`PyBas3/mediapipe/multi_person_detector.py`): orchestrates MediaPipe detection, zone filtering, UUID assignment, shared memory writes, and NDI streaming.
- `ParticipantTracker` (`PyBas3/mediapipe/participant_tracker.py`): computes face-based pHash, matches/creates UUIDs, persists to `participants_db.json`.
- `depth_blend_video.py` (`PyBas3/pre_render/depth_blend_video.py`): offline video processing pipeline that extracts frames, generates depth maps, applies effects (dithering, pixelation, etc.), and creates chronophoto composites.

## Patterns
- Participant UUID persistence via `participants_db.json` with atomic writes.
- Pre-render pipeline: extract frames → depth estimation → effects → chronophoto pass (runs after depth maps, uses all frames for ghostly composites).