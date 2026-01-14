# 🎨 Canvas (TouchDesigner) Status

**active:** no
**last_updated:** 2026-01-14T15:15:00Z
**task:** TD integration complete
**test_status:** ✅ All integration tests passing (4/4)

**notes:** 
- ✅ `td_scripts/td_execute.py` - Execute DAT script for live file sync
- ✅ `td_scripts/ndi_discovery.py` - NDI discovery + UUID parsing
- ✅ `td_scripts/score_watcher.py` - score JSON watcher
- ✅ `td_scripts/td_participant_manager.py` - unified TD interface
- ✅ NDI video streams working (fixed BGRX format, line stride)
- ✅ TouchDesigner MCP server installed (`touchdesigner-mcp/`)
- ✅ Score data accessible via `parent().fetch('pybas3_scores')`
- Next: Visual effects, auto-update NDI sources on participant change
