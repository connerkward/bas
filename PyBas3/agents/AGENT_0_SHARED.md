# Shared Contracts & Protocols

> **Technical details:** `../TECHNICAL_REFERENCE.md`  
> **Task tracking:** `SESSION.md`

---

## Agent Roster

| Emoji | Nickname | Lane | Status File |
|-------|----------|------|-------------|
| 🔄 | Orchestrator | Integration (cross-lane testing) | `status/agent_0_orchestrator.md` |
| 👁️ | Iris | Vision (MediaPipe → NDI + shared mem) | `status/agent_1_vision.md` |
| 🎯 | Judge | Scoring (pose → score JSON) | `status/agent_2_scoring.md` |
| 🎨 | Canvas | TouchDesigner (NDI + JSON → visuals) | `status/agent_3_touchdesigner.md` |

### Orchestrator role
- Can read/test across all lanes (not lane-restricted)
- Cannot claim exclusive lane tasks while other agents are active on them
- Primary job: integration tests, end-to-end validation, startup scripts
- Should spin down before lane-specific agents merge (avoids conflicts)

---

## Commands

### "spin up" protocol

1. **Read** `SESSION.md` for context + tasks
2. **Read** `status/*` files to see who's active
3. **Claim role:**
   - User specified (e.g. "spin up as Iris") → use that
   - Otherwise → pick first available (`active: no` or stale >2h)
   - Still unclear → ask user
4. **Update** your status file: `active: yes`, `last_updated: <ISO>`, task
5. **Commit** status file (makes claim visible to other agents)
6. **Pick task** from backlog matching your lane
7. **Work** following contracts below
8. **Before completing:** Test code or flag user if testing isn't verifiable

### "spin down" protocol

1. **Test** your code changes:
   - Run unit tests, integration tests, or manual verification
   - If testing not verifiable (hardware, external deps) → **FLAG USER AND PAUSE**
   - Document test status in status file
2. **Update** `SESSION.md`: what changed, current state
3. **Update** your status file: `active: no`, summary, test status
4. **Commit changes:**
   - **If in worktree:** Commit (goes to your branch, safe for merge)
   - **If NOT in worktree:** Leave staged/unstaged for human review (never push)

### Multi-agent notes

- Status files show committed state (may be slightly stale)
- Committing early makes your claim visible faster
- If two agents claim same role before seeing each other's commit → human resolves at merge

---

## Common Module

**Shared protocols and constants:** `../common/`

Both Vision and Scoring modules use `common/` for:
- Protocol definitions (`common/protocols.py`)
- Binary encoding/decoding (`common/shared_memory.py`)
- Data structures (`ParticipantPose`, `PoseKeypoint`)
- Constants (`SHARED_MEMORY_BUFFER_NAME`, `POSE_BUFFER_SIZE`, etc.)

**Rule:** When modifying shared memory format or data structures, update `common/` first, then update dependent modules.

---

## I/O Contracts

| From | To | Channel | Format |
|------|-----|---------|--------|
| 👁️ | 🎨 | NDI | `BAS_Participant_<UUID>` streams |
| 👁️ | 🎯 | Shared mem | `bas_pose_data` buffer (via `common/`) |
| 🎯 | 🎨 | Files | `scoring/output/participant_<uuid>_score.json` |
| 👁️ | restart | File | `mediapipe/participants_db.json` (atomic writes) |

---

## Process Boundaries

- Each module: independent `python <entrypoint>.py`
- No shared Python state
- Communicate only via: NDI, shared memory, files
- Shared code: `common/` module (protocols, constants, encoding)

---

## Testing Requirements

- Agents must test code changes before spinning down
- If testing not verifiable (hardware, external dependencies, integration) → flag user and pause
- Document test status in status file notes field
