# Agents working in Kindred Siege

## Locked owner design
- Read `DESIGN_LOCKS.md` before design or implementation work. Jacob's final conditions, locked on 2026-09-30, supersede conflicting older proposals and documents. Preserve the 24 tribes, 7 tiers, 17 progression-unlocked classes, roguelike fast-paced action, stated top priorities and 2.5D battle screen. Only the explicitly open details remain to be designed.

Two assistants work in this repo: Claude and Codex. Jacob decides everything. Read this file at the start of every session.

## Branch
- Work on `game-one`. Do not push, force push, commit, merge or rebase unless Jacob asks in that session.
- Never `git add -A`. Commit only the paths you own.

## Who owns what
- Owner assignment, 2026-10-17, replacing the 2026-10-06 split (log entry 8443), amended by Jacob on 2026-10-18: Codex now owns backgrounds. Jacob's 17 Oct addition still applies: "give codex balance and tests, QA, putting fighter art into fights as well".
- Jacob decides everything: character designs, the picks, every art and design approval. Pals remain Jacob's drawings.
- Claude owns the sim and its rules (the logic in `Assets/Scripts/Sim/` and the authoritative gameplay state APIs), characters, all animation and artwork other than backgrounds (`Art/`, `Assets/Resources/CampaignArt/`, except background assets now owned by Codex; all PixelLab and ComfyUI spending remains with Claude, only with Jacob's OK), and sound, music and trailers (`CampaignArtSound.cs`, `CampaignArtTrailerParry.cs`, the recorder in `CampaignArtBloodridgeReview.cs`).
- Codex owns screens, HUD, menus, input and saves on screen (the rest of `Assets/Scripts/Slice/`); builds (`Assets/Editor/`, `Builds/`); QA (play every build by hand, log what breaks with steps, check Claude's work before Jacob sees it); balance (the numbers: class stats in `SliceClasses.cs` and `CampaignTuning` in `CampaignRules.cs`; the rules logic stays Claude's); tests (`Assets/Tests/`); and putting fighter art into fights (`ArtKey` and clip loading in `CampaignArtBattle.cs`).
- Codex also owns background creation, integration and testing, including background assets within `Art/` and `Assets/Resources/CampaignArt/`. Jacob approves the art; Claude keeps characters and all animation. Coordinate shared files through the handoff log.
- Backgrounds are the only exception to Codex's no-art assignment. Codex spends nothing on PixelLab; the ownership change does not authorize paid generation.
- Handovers: Claude puts finished sheets in `Assets/Resources/CampaignArt/` (the tribes' sheet format: one shared 32 colour palette per unit, SE/SW facings, the size ladder) and posts their keys in the log; Codex binds them and builds. Sim changes come with a log note so Codex can test them.
- Balance follows Jacob's 2026-10-12 rule: no tuning or bot sweeps until he has played the build and says so.
- The frontend reads the sim's state and results and sends commands through the existing simulation APIs. It must not duplicate combat rules, invent rewards, or silently repair backend behaviour in the UI.
- Before touching the other agent's file, claim it in `HANDOFF_LOG.md` and hand it back after. Do not interrupt the other agent or change their running Unity session.

## How we talk
- `HANDOFF_LOG.md` at the repo root. Read it first. Append one entry at the end of every session: date, agent, what changed, what is not verified, what the other agent needs to know. Newest at the bottom.
- Owner instruction, 2026-10-12: Jacob wants direct Codex/Claude coordination through this shared log and does not want to act as a mediator. Record substantive work, findings, decisions, questions, blockers, review requests and replies there, addressed to the responsible agent with exact paths, evidence and the next action. Read new entries before dependent work and reply in the log; do not ask Jacob to copy messages between agents. Keep Jacob informed of meaningful progress and bring him the art/design approvals or other decisions that require him. A logged request is pending until acknowledged; do not claim Claude has read it. Preserve the existing ownership and non-interruption rules.

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
