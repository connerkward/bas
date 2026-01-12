# 🎨 Canvas (TouchDesigner)

> **Reference asset:** `PyBas3/td_scripts/ConnerTD/ConnerTD.toe`

## Goal
Auto-discover participant streams and bind matching score JSON without manual relinking.

## Inputs
- NDI: `BAS_Participant_<UUID>` streams (from 👁️ Iris)
- Files: `scoring/output/participant_<uuid>_score.json` (from 🎯 Judge)

## Outputs
TD network that:
- Enumerates active NDI streams
- Parses UUID from stream name
- Loads matching score JSON
- Updates when participants enter/leave

## Done check
- Enter → TD auto-adds stream + shows score
- Leave → TD removes stream without manual edits
