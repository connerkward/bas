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

## Architecture Status (as of: <!-- YYYY-MM-DD -->)

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

### Backlog
- 🎨 TD visual effects (compositing, overlays, score display)
- 🎨 TD auto-update NDI sources when participants change
- 📹 Recording module (capture participant streams)

### In Progress
- <!-- none -->

### Done
- ✅ Migration: pre_render scripts + TD project from archive
- ✅ 👁️ MediaPipe multi-person detection + pHash UUIDs
- ✅ 👁️ SharedMemoryPoseWriter (writes to shared memory for Scoring)
- ✅ 🎯 Shared memory reader + score calculator
- ✅ 🎯 Per-UUID score JSON writer
- ✅ Common module: shared protocols & constants (`common/protocols.py`, `common/shared_memory.py`)
- ✅ 🎨 TD: NDI stream discovery + UUID parsing (`td_scripts/ndi_discovery.py`)
- ✅ 🎨 TD: Score JSON file watcher (`td_scripts/score_watcher.py`)
- ✅ 🎨 TD: Unified ParticipantManager (`td_scripts/td_participant_manager.py`)
- ✅ UV package manager setup (`pyproject.toml`)
- ✅ 🔄 Integration tests (`tests/test_integration.py`) - all 4 passing
- ✅ 👁️ Per-participant NDI streams (`BAS_Participant_<UUID>`)
- ✅ 👁️ Zone UI: sliders + click-to-set corners + Z-depth visualization
- ✅ 🔄 Launcher script (`orchestrator.py`) - starts Vision + Scoring + optional Dashboard
- ✅ 🎨 TD: Execute DAT integration script (`td_scripts/td_execute.py`)
- ✅ 🎨 TD: NDI video streams working (BGRX format fix)
- ✅ 🎨 TD: TouchDesigner MCP server installed for Cursor control
- ✅ 🎨 TD: Score data + NDI streams flowing to TouchDesigner

## Blockers
- <!-- delete when resolved -->
