# Agents working in Kindred Siege

## Locked owner design
- Read `DESIGN_LOCKS.md` before design or implementation work. Jacob's final conditions, locked on 2026-09-30, supersede conflicting older proposals and documents. Preserve the 24 tribes, 7 tiers, 17 progression-unlocked classes, roguelike fast-paced action, stated top priorities and 2.5D battle screen. Only the explicitly open details remain to be designed.

Two assistants work in this repo: Claude and Codex. Jacob decides everything. Read this file at the start of every session.

## Branch
- Work on `game-one`. Do not push, force push, commit, merge or rebase unless Jacob asks in that session.
- Never `git add -A`. Commit only the paths you own.

## Who owns what
- Owner assignment, 2026-10-06: Jacob appointed Codex the frontend "wirer" for what Claude builds. This supersedes the earlier instruction parking Codex while all development moved to Claude.
- Claude owns `Assets/Scripts/Sim/`, `Assets/Tests/`, game rules, balance, and authoritative gameplay-state APIs.
- Codex owns the player-facing integration: `Assets/Scripts/Slice/`, slice UI/HUD/input, squad and reward presentation, approved-art binding, and the scene/editor wiring needed to play that frontend.
- Animation assignment, 2026-10-06: Jacob also asked Codex to take over sprite animation through PixelLab. Continue from the approved character IDs and Claude's existing clips; preserve the size ladder, shared 32-colour palettes and SE/SW battle facings. Keep experimental takes separate from production assets and record their review status. Pals remain Jacob's drawings.
- The frontend reads Claude's state/results and sends commands through the existing simulation APIs. It must not duplicate combat rules, invent rewards, alter balance, or silently repair backend behaviour in the UI.
- Each agent preserves the other's active files. Backend API needs and frontend handoffs go in `HANDOFF_LOG.md`; do not interrupt the other agent or change their running Unity session. Other folders and asset generation remain task-scoped; Jacob's art/design approvals still apply.

## How we talk
- `HANDOFF_LOG.md` at the repo root. Read it first. Append one entry at the end of every session: date, agent, what changed, what is not verified, what the other agent needs to know. Newest at the bottom.

## Rules for the deterministic core (`Assets/Scripts/Sim/`)
- No `UnityEngine` references. The asmdef has `noEngineReferences: true`.
- No `System.Random`, no `UnityEngine.Random`. Use `DeterministicRng`.
- No floats in authoritative state. Integers only. Presentation may use floats.
- No `Dictionary` or `HashSet` iteration in authoritative logic. Ordered lists only.
- Every state change enters through a `SimCommand` in the log, or through `Tick()`.
- The state snapshot must be ordered and complete. The checksum is over that snapshot, never over log text.

## How to talk to Jacob
- Plain English, short sentences, answer first.
- Never say done, fixed or working unless you ran it and looked. Otherwise say "not checked".
- No documents, commits or extra passes he did not ask for.
