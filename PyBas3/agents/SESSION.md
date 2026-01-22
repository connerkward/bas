# Session Handoff

**Last updated:** 2026-01-14 15:15

> **Commands:** "spin up" / "spin down" → see `AGENT_0_SHARED.md`  
> **Agent roster:** see `AGENT_0_SHARED.md`

## Quick Start Commands

```bash
cd PyBas3

# Run the full system (Vision + Scoring + Dashboard)
uv run python orchestrator.py --dashboard

# Run without dashboard
uv run python orchestrator.py

# Keep participants across restarts
uv run python orchestrator.py --dashboard --persist

# Run integration tests
uv run python tests/test_integration.py

# Run individual modules
uv run python mediapipe/multi_person_detector.py
uv run python scoring/pose_scorer.py
uv run python mediapipe/live_dashboard.py
```

## Architecture Status (as of: 2026-01-14)

```mermaid
flowchart LR
    subgraph Vision
        MP["MediaPipe ✅"]
        NDI["NDI Streams ✅"]
        SHM["Shared Memory ✅"]
        DB["participants_db ✅"]
        MP --> NDI
        MP --> SHM
        MP --> DB
    end
    
    subgraph Scoring
        SC["Score Calculator ✅"]
        JSON["score JSON ✅"]
        SHM --> SC
        SC --> JSON
    end
    
    subgraph TouchDesigner
        TDN["TD Network ✅"]
        NDI --> TDN
        JSON --> TDN
    end
```

**Legend:** ✅ done | ⚠️ partial | ❌ not started

## What changed last session
- 🎨 TouchDesigner integration complete:
  - Created `td_execute.py` - Execute DAT script for live file sync
  - Fixed NDI streaming (BGRX format, proper line stride)
  - Installed TouchDesigner MCP server for direct TD control from Cursor
  - Created NDI In TOPs receiving participant video streams
  - Score data accessible via `parent().fetch('pybas3_scores')`
- 📦 Added `ndi-python` dependency to pyproject.toml

## Current state
- **👁️ Iris**: MediaPipe detection + pHash + shared memory + per-participant NDI streams ✅; zone UI with sliders + click-to-set ✅
- **🎯 Judge**: Scoring module complete (reader + scorer + JSON writer) ✅
- **🎨 Canvas**: TD integration complete - NDI video streams + score data flowing to TouchDesigner ✅

## Tasks

> **Single source of truth:** All active tasks live here. TODOs in code/docs reference this file.

### 🎨 TouchDesigner (Canvas)
**Backlog:**
- Visual effects (compositing, overlays, score display)
- Auto-update NDI sources when participants change

### 📹 Recording
**Backlog:**
- Capture participant streams from NDI with JSON metadata

### 🎬 Pre-render Pipeline
**Backlog:**
- Fix green screen edge artifacts in `depth_blend_video.py`
- Refine depth map threshold parameters (soft scaling approach)

**Status:**
- ✅ Chronophoto generation implemented (see `pre_render/CHRONOPHOTO_CONTEXT.md`)
  - Three blend modes: `long_exposure`, `hero_ghost`, `lighten_add`
  - Parallel processing, frame reuse optimization

### ✅ Completed (Recent)
**Core System:**
- MediaPipe multi-person detection + pHash UUIDs
- Shared memory protocol (`common/` module)
- Per-participant NDI streams (`BAS_Participant_<UUID>`)
- Zone UI with sliders + click-to-set corners + Z-depth visualization
- Orchestrator launcher script

**Scoring:**
- Shared memory reader + score calculator
- Per-UUID score JSON writer

**TouchDesigner:**
- NDI stream discovery + UUID parsing
- Score JSON file watcher
- Unified ParticipantManager
- Execute DAT integration script
- NDI video streams (BGRX format)
- TouchDesigner MCP server installed
- Score data + NDI streams flowing to TD

**Infrastructure:**
- UV package manager setup
- Integration tests (4/4 passing)
- Pre-render scripts + TD project migrated

## Blockers
- None
