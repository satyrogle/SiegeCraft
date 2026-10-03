# Agents working in Kindred Siege

## Locked owner design
- Read `DESIGN_LOCKS.md` before design or implementation work. Jacob's final conditions, locked on 2026-09-30, supersede conflicting older proposals and documents. Preserve the 24 tribes, 7 tiers, 17 progression-unlocked classes, roguelike fast-paced action, stated top priorities and 2.5D battle screen. Only the explicitly open details remain to be designed.

Two assistants work in this repo: Claude and Codex. Jacob decides everything. Read this file at the start of every session.

## Branch
- Work on `game-one`. Do not push, force push, commit, merge or rebase unless Jacob asks in that session.
- Never `git add -A`. Commit only the paths you own.

## Who owns what
- Claude owns `Assets/Scripts/Sim/` and `Assets/Tests/`. The deterministic core and its tests.
- Codex owns everything else, unless Jacob says otherwise for a task.
- Never edit a file in the other agent's folders. If you need a change there, write the request in `HANDOFF_LOG.md`.

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
