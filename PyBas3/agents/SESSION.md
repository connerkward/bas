# Quick Reference

> **Tasks:** [Linear BAS project](https://linear.app/ckward-workspace/project/bas-d90c3a3ab597) (Personal team)  
> **Protocols:** `AGENT_0_SHARED.md` · **Tech:** `../TECHNICAL_REFERENCE.md`

## Quick Start

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

## Architecture

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
