# First Alliance repair audit — 27 September 2026

**Verdict: FAILED validation; substantial repairs verified, but “all fixed” is not supported.** The Windows development player fails during startup because its runtime shader is missing. The checkpoint repair still accepts impossible state. Both independent reruns of the unchanged 51-check harness failed their salvage victory assertion after 45 passes each. No fixes or balance changes were made.

This is a focused audit of Claude’s eight repairs, the Broken-retreat correction, the commander tuning, and affected combat/persistence/UI dependencies. It is not a whole-campaign certification or approval of the prototype’s art or design.

## Confirmed defects

### FA-01 — High / confirmed: Windows player cannot initialize the arena

- **Evidence:** [AlliancePrototype.cs:476](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/AlliancePrototype.cs:476) constructs `new Material(Shader.Find("Unlit/Color"))`. The first call from `BuildArena`, line 462, throws `ArgumentNullException: shader` in the actual development player. [Startup log](C:/Users/jacob/.codex/scratch/ks-alliance-audit-20260927/standalone-startup.log). The editor can find this shader; the standalone player cannot. Runtime lookup alone did not retain it in this build.
- **Impact:** `Start` stops before `SpawnFirstFight`; this build is not a playable First Alliance executable, despite a successful build report.
- **Reproduction:** build the unchanged prototype scene with `BuildPlayerOptions`, Windows x64, `BuildOptions.Development`, and launch the resulting executable. Compilation/build: zero reported errors and warnings, 17.2045118 seconds, 98,462,979 bytes. Configured backend: Mono2x; the artifact contains Mono runtime/data. The raw build-summary options string contains additional internal flag names and is retained verbatim rather than used to infer the backend.
- **Minimal remedy:** give the prototype a build-retained reference to its material/shader, or explicitly retain the required shader through an approved configuration change. Then rebuild and inspect the actual player. A null guard alone would not provide the intended rendering.
- **Provenance:** pre-existing prototype defect, previously only a documented build risk. The original pre-Claude `Shape` also used this shader lookup. It was not introduced by the sanity or retreat repairs. No shader/configuration fix was attempted.

### FA-02 — Medium / confirmed: checkpoint validation still accepts impossible survivors and history

- **Evidence:** [AllianceMemory.cs:89](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/AllianceMemory.cs:89) permits positive health with zero sanity. Lines 94–116 constrain each event/survivor independently but do not enforce the encounter-wide rescue outcome or an eligible rescuer. [Probe results](C:/Users/jacob/.codex/scratch/ks-alliance-audit-20260927/boundary-results.txt), [loaded-state reproduction](C:/Users/jacob/.codex/scratch/ks-alliance-audit-20260927/extra-boundaries.txt).
- **Reproduction:** start with four valid, unique survivors and both guards Abandoned. Independently make (a) scout HP 130/SAN 0, (b) guard one Rescued while guard two remains Abandoned, or (c) both guards Rescued with guard two named as guard one’s rescuer. `IsValid()` returns true for all three. The scout fixture was actually saved and loaded: it was alive at SAN 0 in the debrief and alive at HP 130/SAN 10 after the patch-up.
- **Impact:** a corrupt/edited checkpoint can evade the intended Lost/casualty rule and create relationships that this encounter could never have earned. Normal first-fight play was not observed generating these fixtures.
- **Why contradictory:** zero sanity reached through `ModifySanity` triggers Lost ([UnitController.cs:749](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/UnitController.cs:749)); restoration instead assigns sanity directly ([AlliancePrototype.cs:348](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/AlliancePrototype.cs:348)). Successful rescue records rescue for every living guard and immediately ends fight one (lines 129–136); only Commander or Ember scout can earn that credit (line 129).
- **Minimal remedy:** validate the joint life-state and encounter outcome, including positive sanity for living survivors and the allowed rescuer identities. Add independent rejection fixtures without weakening the existing valid-save cases.
- **Provenance:** incomplete closure of original defect 8. The original over-max, Fell-with-HP, duplicate, resource/event mismatch and fallen/abandoned probes now reject correctly.

### FA-03 — Medium / confirmed: an intact backup is inaccessible if the primary is missing

- **Evidence:** [AllianceMemory.cs:147](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/AllianceMemory.cs:147) gates the backup attempt on `File.Exists(path)`.
- **Reproduction:** save a valid checkpoint, replace it with another valid checkpoint to create `.bak`, corrupt the primary: fallback succeeds and preserves the corrupt file. Move only that primary into the audit evidence directory, leaving the same valid `.bak`: loading now fails with “No prototype checkpoint yet.” All of this used an isolated scratch path.
- **Impact:** a missing/inaccessible primary can make a recoverable run appear lost. Corrupt-but-existing primary recovery works; this is a narrower residual failure.
- **Minimal remedy:** try and validate the backup after any unsuccessful primary load, including absence. Preserve diagnostic distinctions and do not overwrite the only valid recovery copy with rejected bytes.
- **Provenance:** limitation of the newly added fallback, not evidence that ordinary saves are being deleted or corrupted.

### FA-04 — Medium / confirmed: the displayed pulse loss is not always the actual loss

- **Evidence:** [AlliancePrototype.cs:253](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/AlliancePrototype.cs:253) tops up through `ModifySanity`; [UnitController.cs:664](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/UnitController.cs:664) honors Stalwart immunity; [AlliancePrototype.cs:267](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/AlliancePrototype.cs:267) still prints a fixed “48 damage, 20 sanity.”
- **Reproduction:** Commander starts at SAN 51; `Random.InitState(6)` makes its stress roll grant Stalwart. An uncovered pulse removes 48 HP, the ordinary-hit portion takes SAN 51→49, Stalwart then blocks the remaining 18. The status still says 20 sanity. This used the actual ordinary stress roll, not a permanently altered class/talent asset.
- **Impact:** the player cannot infer the actual result from the message, and the unconditional exact-total claim is false. The ordinary no-modifier double-counting defect itself is repaired: all four normal receiver/cover fixtures gave exactly 48/20 or 24/6.
- **Minimal remedy:** describe these as base costs and show the actual applied HP/sanity changes and relevant immunity. Preserve intended virtues unless Jacob explicitly changes that design. Low-health/dead receivers and sanity clamping are also exceptions to an unconditional loss guarantee.
- **Provenance:** inherited virtue behavior plus pre-existing fixed wording; not evidence that the commander’s 0.5 multiplier is incorrectly applied to the pulse top-up.

### FA-05 — Low / confirmed: outcome/history text still contradicts casualties

- **Evidence:** [AlliancePrototype.cs:218](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/AlliancePrototype.cs:218) always announces “You survived together.” [AllianceMemory.cs:71](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/AllianceMemory.cs:71) says “Will cover them”; line 73 says “Will not return for fight two” even on an F2 death.
- **Reproduction:** rescue seeds 202 and 303 won with Ash guard 1 dead and still emitted the together line. The recorded F2 Fell event is described using the same fight-one-only sentence. Both contradictions are also visible together in the fresh [second-harness victory frame](C:/Users/jacob/.codex/scratch/ks-alliance-audit-20260927/independent-harness-02/05-rescue-result.png), opened and inspected. “Them” is ambiguous while guard behavior protects the Commander, including when the recorded rescuer was the scout.
- **Impact/remedy:** make victory wording depend on survivors, death text depend on its fight, and cover wording identify who is protected. These are small copy/condition changes; none was implemented.
- **Provenance:** explicitly acknowledged and left open in Claude’s repair handoff; unchanged by the repairs.

## Original defects and tuning: independent disposition

| Claim | Result and boundary |
|---|---|
| 1. Hollow ordinary-hit collapse | **Fixed in tested scope.** Actual hit left Hollow SAN 100 while scout lost 5 and Commander lost 2. Its runtime immunity is set after `Initialise` (prototype lines 196–200). A forced prolonged-drain boundary still removed 3; Husk death still removed 15. Immunity is deliberately not immunity to all sanity death. |
| 2. Cover and alternating guards | **Fixed in tested scope.** Marked trusting guard one was followed despite wary guard two on the opposite side. Target sequence was Commander, guard one, Commander, guard two. A dead marked guard caused no pulse; a Broken marked guard could receive cover. Broken/hesitating actors are still not guaranteed to arrive in time. |
| 3. Pulse double counting | **Fixed for ordinary surviving receivers.** Covered/uncovered Commander and guard-target fixtures measured 24 HP/6 SAN and 48 HP/20 SAN. See FA-04 for inherited mitigation and clamping exceptions. |
| 4. Defeat/Lost witness consequences | **Fixed for this isolated encounter.** Ordinary allied death caused -15 once; Lost caused -20 to both sides plus -15 to its own side. Repeating lethal damage did not replay it; defeat count stayed one. Loading a dead guard and reconstructing fight two did not mourn it again. Prototype handlers match the main manager’s witness calls, not its unrelated campaign/mutation side effects. |
| 5. Pending pulse after boss death | **Fixed.** Killing Hollow during a pending warning with Husk still alive cleared the mark, applied no pulse and left the receiver’s HP unchanged. |
| 6. Repeat extraction order | **Fixed.** Repeating Salvage preserved a 1.25-second collection value; switching to Rescue reset it to zero. Real route extraction also completed. |
| 7. Runtime ownership | **Fixed in measured lifecycle.** Five prototype `UnitData` and 16 Unlit/Color materials before/after three frame-separated restarts. After Play Mode exit: zero prototype data, zero such runtime materials. A separate non-vacuous check loaded all eight shared UnitData assets: intact and serialized content unchanged. File hashes provide disk-level preservation evidence too. |
| 8. Checkpoint validation/recovery | **Partially fixed.** Original contradictions now reject and corrupt-existing-primary backup recovery works. FA-02/03 remain. |
| 9. Broken retreat sinking | **Fixed for the reported path.** 600 seeded calls to the real `TickAI` with an inert test BT, Broken guard at y=.8 and Hollow at y=1.3, kept y=.8. This was a synthetic tick probe, not a claimed ten-second real-time soak. All nine normal route snapshots also kept allied actors at y=.8. |
| 10. Commander Comprehension .5 | **Verified runtime-only.** Actual ordinary enemy hit costs 2 (Unity rounds -2.5 to even -2), not 5; other prototype units remain 1.0. `HollowPulse` is not an eldritch source, so its ordinary top-up is unscaled. Witness loss and prolonged drain are not reduced by this multiplier. The modifier would affect other eldritch sources if introduced later; the isolated scene does not run the main manager’s aura/dread systems. No shared asset was changed. |

Exact repair comparison is in the three `claude-*.cs.diff` files in the evidence directory. Only the y-flattening hunk in `UnitController` belongs to these recent repairs: its much larger Git diff includes pre-existing lifecycle work and must not all be attributed to Claude’s latest pass.

## Gameplay results and design tradeoffs

Nine additional real-time editor runs used queued Game view mouse events, normal `UnitController` ticks, normal autosaving, and initial Unity RNG seeds 101/202/303. No health, damage, trust, positions or virtues were injected into these nine runs. Boundary fixtures above are separate. Timing remained frame dependent, so a seed is **not** a deterministic replay guarantee.

“No orders” means click Begin, let fight one time out without an order, then click Begin Fight Two and leave its default Advance untouched. Rescue means choose Rescue, then Cover only when a guard warning appears, and Advance after an actual pulse. Salvage means choose Salvage, disrupt Commander warnings, Cover guard warnings, and Advance after an actual pulse. The scripted pause check resumes through its GUI fallback after queued keyboard events fail to prove legacy key input.

| Route / seed | Fight-two outcome | Seconds | Covers / pulses taken | Commander HP / SAN | Other outcome |
|---|---|---:|---:|---:|---|
| No orders / 101 | Defeat | 19.733 | 0 / 2 | 0 / 35 | Guard one dead; Hollow HP 107 |
| No orders / 202 | Defeat | 20.023 | 0 / 2 | 0 / 37 | Guard one dead; Hollow HP 94 |
| No orders / 303 | Defeat | 19.734 | 0 / 2 | 0 / 37 | Guard one dead; Hollow HP 107 |
| Rescue / 101 | Defeat | 19.115 | 2 / 2 | 0 / 66 | Hollow HP 3 |
| Rescue / 202 | Victory | 19.804 | 1 / 1 | 186 / 83 | Guard one dead |
| Rescue / 303 | Victory | 19.795 | 2 / 2 | 150 / 75 | Guard one dead |
| Salvage / 101 | Defeat | 22.148 | 1 / 1 | 0 / 60 | Hollow HP 65; one charge left |
| Salvage / 202 | Defeat | 22.135 | 1 / 1 | 0 / 62 | Hollow HP 65; one charge left |
| Salvage / 303 | Defeat | 22.268 | 1 / 1 | 0 / 60 | Hollow HP 65; one charge left |

These samples no longer support the older “do nothing still wins” observation, but do not prove no possible no-order run can win. Sanity tuning has moved the observed Commander failure toward HP exhaustion. The salvage policy here waits until a guard is marked to move into cover; the shipped harness moves into Cover much earlier, immediately after disrupting the first warning. Do not conflate those strategies or infer that salvage is unwinnable. The repaired boss survives long enough for positioning and HP exposure to matter. Whether this is satisfying difficulty is Jacob’s design decision; no numbers were tuned.

**Visual observations:** actual fresh 1046×654 editor frames were opened. The danger disk is visible; cover itself is mostly a status sentence/counter. Guard bodies still merge, and anti-overlap label displacement detaches A1/A2 labels from their bodies. HP/SAN numbers are readable in the left panel, but that panel says “Your orders” or “Will shield you” even when shared sanity logic can make a unit hesitate, retreat or attack an ally. Enemy sanity remains hidden. See [warning frame](C:/Users/jacob/.codex/scratch/ks-alliance-audit-20260927/independent-harness-01/04-earned-cover.png), [salvage debrief](C:/Users/jacob/.codex/scratch/ks-alliance-audit-20260927/independent-harness-01/06-salvage-memories.png), and [actual cover pulse](C:/Users/jacob/.codex/scratch/ks-alliance-audit-20260927/frames/03-rescue-seed101-pulse2.png).

These are medium-impact readability/design issues, confirmed in frames and [HUD behavior selection, line 530](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/AlliancePrototype.cs:530), not new regressions from the three repair files. A minimal next pass would expose current mental behavior, connect labels to actors and make the interception visually identifiable; this audit does not authorize an art or movement rewrite.

## Main-combat movement follow-up

The full-3D paths are reachable: default Warden retreat ([BTPresets.cs:32](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/BehaviourTree/BTPresets.cs:32)) calls [BattleActions.cs:235](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/BehaviourTree/BattleActions.cs:235); support/flee gambits use similar vectors ([GambitLibrary.cs:133](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/GambitLibrary.cs:133), lines 164, 255, 808); Fall Back and Sacrifice are reachable directives ([DirectiveSystem.cs:249](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/DirectiveSystem.cs:249), line 354).

However, the present main battle places units through `BattleGrid.GridToWorld` at one common y ([BattleGrid.cs:42](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/BattleGrid.cs:42); [BattleManager.cs:466](C:/Users/jacob/KindredSiege-Clean/Assets/Scripts/Battle/BattleManager.cs:466)). SampleScene’s origin y is zero, and the eight current UnitData assets have no custom prefab. Full-3D motion between these equal-height positions preserves that plane.

The actual Retreat node was probed for 120 fixed 1/60-second steps: same-height actors retained y=.8; injecting an enemy at y=1.3 drove the unit to y=-1.704396. This proves a **conditional medium risk**, not an observed default-main-battle sinking regression. If main combat later introduces unequal root heights/terrain, constrain the appropriate movement plane or use a real ground-following movement contract. Fall Back toward the unit’s own spawn is not automatically the same as retreat away from a taller enemy. No campaign battle was launched, and these files were not changed.

## Test quality and limits

- The existing harness contains 13 memory/save assertions plus 38 assertions in one ordered runtime script, not 51 independent scenarios. It stops at the first failure. In run one, 45 assertions passed, the next assertion failed, and five later assertions never ran. Those later defeat/casualty/order boundaries were separately probed here.
- Its “can win” assertions ([AlliancePrototypeTools.cs:185](C:/Users/jacob/KindredSiege-Clean/Assets/Editor/AlliancePrototypeTools.cs:185), line 230) bind a random frame-time fight to one outcome without controlling the seed or clock. A green execution does not prove robust balance. Do not weaken these assertions merely to recover a green report; separate deterministic contract tests from recorded stochastic playtests.
- Existing checks do not assert pulse deltas, mixed-trust positioning, dead-caster cancellation, repeated extraction orders, asset counts or the new semantic save cases. This audit supplied independent in-memory probes for those points.
- The original harness disables normal persistence every tick (line 108), uses an isolated checkpoint path, and clicks only Begin/Rescue. The nine additional runs exercised queued mouse controls for salvage, second-fight start, cover, disrupt and pause, with default autosaves enabled. Default-path checkpoint Load through its UI button remains unverified; direct loading and corrupt/backup edge behavior were tested.
- Original harness cancellation safety and stale-output risks remain: `OpenPrototype` can return on a canceled save prompt while `ValidateBatch` continues (lines 37, 47–53); results are not invalidated at start; static `Captured` suppresses repeated filenames without a domain reload (line 287). These are pre-existing tooling risks. This audit used clean editor scene state and distinct, initially empty output directories.
- The added probe file reports **55 PASS, 10 FAIL, 2 OBSERVED** entries. Four failures are the semantic/recovery rejection expectations in FA-02/03. Six failures are inability to establish legacy `Input.GetKeyDown` via paired queued keyboard events; they are **not** six confirmed keyboard defects. The boundary portion alone was 38 PASS / 4 FAIL, plus a virtue observation; lifecycle and GUI checks account for the remainder.
- The added route capture driver had an evidence-timing flaw: deferred `*-result.png` captures could land after the following Restart because its phase transition reused an old elapsed value. The inspected rescue-202 result image was a briefing. Those result PNGs are excluded as terminal-state evidence. The synchronous route text, warning/pulse/debrief frames and original-harness captures remain separate evidence. The diagnostic source is retained unchanged so this limitation is reproducible.
- The 13 `KindredSiege.Sim.Tests` EditMode tests passed, zero failed/skipped, 0.5521414 seconds. That proves the existing core suite still passes, not that this prototype is connected to the authoritative deterministic core.
- A real Windows build and launch were performed. Native Computer Use saw the player and a Windows Security Alert; no security permission was granted. Two player frame capture attempts timed out. Player startup failed as FA-01 independently proves. Therefore direct OS keyboard/mouse play, standalone visuals, other resolutions, controller support, performance soak and a full campaign regression remain unverified. No security settings were changed.

## Provenance, evidence and preservation

- Checkout: **C:/Users/jacob/KindredSiege-Clean**, branch **game-one**, HEAD **9ef7cc9c3c993c7d03da2248f6223cb780df20c7**, Unity **2022.3.62f3 (96770f904ca7)**, active target **StandaloneWindows64**. Steam/Windows intent is retained. Desk 42 was not audited or changed.
- Read `AGENTS.md`, the original audit beginning at handoff line 3469 and subsequent Claude repair/test/tuning entries. The map/PixelLab entry was treated as unrelated design exploration.
- No Unity editor was running initially. Unity Hub was present. Editors started for this audit used hidden background launches; only the locally built player was intentionally opened for foreground interactive inspection. No conflicting editor was launched.
- Baseline covers **214 tracked and non-ignored untracked files**, including the important untracked prototype, core and test sources. Baseline and comparison artifacts are in [the evidence directory](C:/Users/jacob/.codex/scratch/ks-alliance-audit-20260927). Dirty status is intentional; HEAD alone does not describe this tested state.
- Pre-Claude originals: `C:/Users/jacob/AppData/Local/Temp/claude/C--Users-jacob-Desk-42/7b241809-2501-4eda-a1d8-46cf95793569/scratchpad/play/orig/`. Their hashes and comparisons were recorded. Existing `Logs/AlliancePrototypeValidation-codex-20260926` was verified present. Claude’s latest result was independently read and archived before rerunning; it was not counted as this audit’s pass.
- All custom diagnostic C#/Python files are outside the checkout under `.codex/scratch/ks-alliance-audit-20260927`; Unity compiled the probes in memory through the installed Coplay v10 bridge. Nothing was added under Assets, Assets/Tests or Assets/Scripts/Sim. No package/provider was installed or changed.
- Normal test autosaves were intentionally exercised. The LocalLow directory existed but contained no files at baseline. Preferences and UserSettings were backed up before Unity ran. Final restoration/check results and both harness outcomes follow below.

### Evidence index

All scratch evidence paths below are relative to `C:/Users/jacob/.codex/scratch/ks-alliance-audit-20260927/`:

| Evidence | Purpose |
|---|---|
| `baseline-files.json`, `baseline-git.txt`, `original-hashes.json`, `claude-*.diff` | Provenance and dirty-file preservation |
| `claude-latest-evidence/` | Untouched copy of the previous reported 51-pass run |
| `validation-independent.log`, `independent-harness-01/` | First fresh run: FAIL after 45 passes; rescue time 19.12557 s, one cover |
| `audit-editor.log`, `bridge-transcript.jsonl` | Fresh live probes, GUI routes, core test job, build and second original harness |
| `AuditProbe.cs`, `boundary-results.txt`, `extra-boundaries.txt` | Reproducible in-memory boundary tests, failures and their limitations |
| `routes-results.txt`, `routes-timeline.txt`, `routes-complete.txt`, `frames/` | Nine input-driven seeded routes; terminal PNG limitation described above |
| `lifecycle-after.json` | Zero prototype-owned data/materials after leaving Play Mode |
| `build-summary.json`, `windows-dev/`, `standalone-startup.log` | Successful build and failed actual player startup |
| `zero-sanity-checkpoint.json`, `dead-guard-checkpoint.json`, `checkpoint-probe*` | Isolated checkpoint fixtures and recovery evidence |

No repairs, balance tuning, staging, commits, pushes, resets, scene saves, production settings edits or cleanup of unrelated work were authorized or performed. The report and one append-only handoff entry are the intended repository changes.

## Final independent runs and preservation check

| Existing harness rerun | Result | Passed assertions before failure | Rescue result | Log |
|---|---|---:|---|---|
| 1, fresh hidden editor / executeMethod | FAIL at step 13, salvage victory | 45 | Victory, 19.12557 s, one cover | `validation-independent.log` |
| 2, clean Edit Mode after audit tests/build / same unchanged method | FAIL at step 13, salvage victory | 45 | Victory, 19.79437 s, one cover | `audit-editor.log` |

Neither run supplied a new 51-pass result. Each failed before its five trailing assertions. Run two's actual victory screenshot also independently confirms the misleading casualty text. The reproducible harness failure is a **confirmed medium validation issue**, but its origin is not isolated to randomness versus timing; no pre-repair executable was replayed under matched inputs. Minimal next action: retain the failing evidence, add seeded/fixed-clock contract coverage, and separately agree which player strategies should win before tuning anything.

Fresh run outputs are retained separately at:

- `C:/Users/jacob/KindredSiege-Clean/Logs/AlliancePrototypeValidation-independent-01-20260927/`
- `C:/Users/jacob/KindredSiege-Clean/Logs/AlliancePrototypeValidation-independent-02-20260927/`
- Copies also exist under the scratch evidence directory as `independent-harness-01/` and `independent-harness-02/`.

**The original `Logs/AlliancePrototypeValidation/` directory was restored byte-for-byte to Claude’s previous output. Its PASS is historical, not the result of this audit.** Its file hashes match the pre-run archive. The older `AlliancePrototypeValidation-codex-20260926/` was left alone.

Restoration evidence: [preservation-result.json](C:/Users/jacob/.codex/scratch/ks-alliance-audit-20260927/preservation-result.json). Before the handoff append, all 214 baseline files matched their SHA-256 hashes. Editor preferences and this project's editor PlayerPrefs were exported again after restoration: both `.reg` exports exactly match their before-file hashes. The standalone preference key was absent before and is absent again. UserSettings file hashes match; saved Library build/last-scene preferences were restored. A PowerShell typed-value restoration attempt failed; native import of the original registry exports then restored the complete values and was verified by those exact export hashes.

The three generated LocalLow files—checkpoint, `.bak`, and `Player.log`—were archived under `LocalLow.after-tests/` and removed from that originally empty directory. It is empty again. No campaign save was present or modified. Both audit editors and the failed player are closed. Retained generated state consists of audit reports/evidence, the development build, Unity Library/Bee/import/build caches and logs; the runtime bridge also generated its usual local status metadata. Existing source, scenes, packages and ProjectSettings content remained unchanged. The final handoff-prefix and repository checks are recorded in `final-verification.json` alongside this report.

**Recommended order, without implementation authorization:** make the Windows player initialize; close the remaining checkpoint invariants/recovery gap; show honest pulse/casualty/mental-state feedback; then have Jacob evaluate route difficulty with the now-longer boss fight. Do not expand the map, art, campaign or mechanics on the strength of these local tests.
