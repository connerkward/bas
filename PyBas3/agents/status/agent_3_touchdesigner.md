# 🎨 Canvas (TouchDesigner) Status

**active:** yes
**last_updated:** 2026-01-12T22:45:00Z
**task:** TD helper scripts complete
**test_status:** ✅ All scripts tested
  - ndi_discovery.py: UUID parsing works, NDI finder initializes
  - score_watcher.py: mtime-based polling works
  - td_participant_manager.py: unified interface works

**notes:** 
- ✅ `td_scripts/ndi_discovery.py` - discovers BAS_Participant_<UUID> NDI streams
- ✅ `td_scripts/score_watcher.py` - watches score JSON files with efficient mtime caching
- ✅ `td_scripts/td_participant_manager.py` - unified ParticipantManager for TD
- ✅ UV package manager configured (pyproject.toml, Python 3.10)
- Next: integrate scripts into ConnerTD.toe (requires TD GUI)
