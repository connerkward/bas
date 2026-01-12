# Session Handoff

**Last updated:** 2026-01-12 22:45

> **Commands:** "spin up" / "spin down" → see `AGENT_0_SHARED.md`  
> **Agent roster:** see `AGENT_0_SHARED.md`

## Architecture Status (as of: <!-- YYYY-MM-DD -->)

```mermaid
flowchart LR
    subgraph Vision
        MP["MediaPipe ✅"]
        NDI["NDI Streams ⚠️"]
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
        TDN["TD Network ⚠️"]
        NDI --> TDN
        JSON --> TDN
    end
```

**Legend:** ✅ done | ⚠️ partial | ❌ not started

## What changed last session
- 🎨 Canvas: Created `td_scripts/ndi_discovery.py` (NDI source discovery + UUID parsing)
- 🎨 Canvas: Created `td_scripts/score_watcher.py` (score JSON file watcher with mtime caching)
- 🎨 Canvas: Created `td_scripts/td_participant_manager.py` (unified interface for TD)
- 🎨 Canvas: Set up UV package manager with `pyproject.toml`
- 🎨 Canvas: Updated cursor rules + TECHNICAL_REFERENCE.md for UV

## Current state
- **👁️ Iris**: MediaPipe detection + pHash + shared memory writer ✅; NDI streams integrated (pending ndi-python install)
- **🎯 Judge**: Scoring module complete (reader + scorer + JSON writer)
- **🎨 Canvas**: TD helper scripts complete (ndi_discovery, score_watcher, td_participant_manager); needs GUI integration

## Tasks

### Backlog
- 👁️ NDI stream output per participant (code ready, needs ndi-python dependency)

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

## Blockers
- <!-- delete when resolved -->
