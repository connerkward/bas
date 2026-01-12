# Session Handoff

**Last updated:** <!-- YYYY-MM-DD HH:MM -->

> **Commands:** "spin up" → claim task & start | "spin down" → update this doc & commit

## Agent Roster
| Emoji | Nickname | Lane |
|-------|----------|------|
| 👁️ | Iris | Vision (MediaPipe → NDI + shared mem) |
| 🎯 | Judge | Scoring (pose → score JSON) |
| 🎨 | Canvas | TouchDesigner (NDI + JSON → visuals) |

## Architecture Status (as of: <!-- YYYY-MM-DD -->)

```mermaid
flowchart LR
    subgraph Vision
        MP["MediaPipe ❌"]
        NDI["NDI Streams ❌"]
        SHM["Shared Memory ❌"]
        DB["participants_db ❌"]
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
        TDN["TD Network ❌"]
        NDI --> TDN
        JSON --> TDN
    end
```

**Legend:** ✅ done | ⚠️ partial | ❌ not started

## What changed last session
- <!-- keep short -->

## Current state
- **👁️ Iris**: MediaPipe detection + pHash implemented; needs shared memory writer
- **🎯 Judge**: Scoring module complete (reader + scorer + JSON writer)
- **🎨 Canvas**: TD project copied from archive

## Tasks

### Backlog
- 👁️ NDI stream output per participant
- 👁️ **TODO: Implement SharedMemoryPoseWriter** (connects to scoring module)
- 🎨 TD: NDI stream discovery + UUID parsing
- 🎨 TD: Score JSON file watcher

### In Progress
- <!-- none -->

### Done
- ✅ Migration: pre_render scripts + TD project from archive
- ✅ 👁️ MediaPipe multi-person detection + pHash UUIDs
- ✅ 🎯 Shared memory reader + score calculator
- ✅ 🎯 Per-UUID score JSON writer

## Blockers
- <!-- delete when resolved -->
