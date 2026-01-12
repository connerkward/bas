# Agentic Instructions

> **Task tracking:** `SESSION.md`  
> **Contracts:** `AGENT_0_SHARED.md`  
> **Technical reference:** `../TECHNICAL_REFERENCE.md`

## Commands
- **"spin up"** → Claim a role and start working
- **"spin up as Iris/Judge/Canvas"** → Claim specific role
- **"spin down"** → Update docs and commit

## "spin up" protocol

1. **Read** `SESSION.md` for context + tasks
2. **Read** `status/*` files to see who's active
3. **Claim role:**
   - User specified → use that
   - Otherwise → pick first available (`active: no` or stale >2h)
   - Still unclear → ask user
4. **Update** your status file: `active: yes`, `last_updated: <ISO>`, task
5. **Commit** status file (makes claim visible to other agents)
6. **Pick task** from backlog matching your lane
7. **Work** following `AGENT_0_SHARED.md` contracts
8. **Before completing work:** Test code or flag user if testing isn't verifiable

## "spin down" protocol

1. **Test** your code changes:
   - Run unit tests, integration tests, or manual verification as appropriate
   - If testing is not verifiable (requires hardware, external systems, etc.) → **FLAG USER AND PAUSE**
   - Document test results/status in status file
2. **Update** `SESSION.md`: what changed, current state
3. **Update** your status file: `active: no`, summary, test status
4. **Commit changes:**
   - **If in worktree:** Commit changes (commits go to your branch, safe for merge)
   - **If NOT in worktree:** Leave changes staged/unstaged for human review (never push)

## Multi-agent notes
- Status files show committed state (may be slightly stale if another agent hasn't committed yet)
- Committing early makes your claim visible faster
- If two agents claim same role before seeing each other's commit → human resolves at merge
- Works in both local and worktree Cursor modes
