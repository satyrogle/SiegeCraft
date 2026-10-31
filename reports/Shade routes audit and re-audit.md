# Shade routes: audit and re-audit

## Current disposition, updated 30 October 2026

**Shade decisions accepted; Shade runtime validation pending. Butcher is implemented.** Jacob approved the F1-F9 repairs in [DESIGN_LOCKS.md](../DESIGN_LOCKS.md), **The Butcher, Glutton and Shade locked**. The later audit/road sections and three 29 October sections take precedence: halved pacts, free pair answers, combo rethinks and hooks. The [Shade paper](Shade%20command%20specialisations.md) reflects that sequence. The historical proposal/findings below are dated evidence, not the current implementation specification or a claim that today's Butcher is unbuilt.

| Finding | Accepted design resolution | Still needs implementation evidence |
|---|---|---|
| F1: Relay drains a vulnerable ally | A separate small shared reserve made with the field; never other fighters' personal shields. | Reserve budget, stable simultaneous-hit order, expiry/lockout, lethal-save allowance and reload. |
| F2: final volley has no usable ready window | Rupture brings nearly ready attacks forward and repays the borrowed time afterward. | Eligibility threshold and original due-time/debt preservation. Cycle/non-recursion checks must also respect the later free pair answer: at most once per own-spell cycle, 1.5 seconds apart, normal spell clock unspent. Pair answers do not acquire Rupture debt. |
| F3: follow-up activation ambiguity | A follow-up spends its parent activation's stored payload once and never fires ordinary command hooks again. | Parent/payload IDs, accepted release state, exact restart policy, rejected inputs and replay/save traces. The once-only rule does not choose every timer. |
| F4: missing spatial system | No movement system. Mine triggers on the first enemy attack after Fall Back. Push means delay the enemy's next attack. Rift forms are Mine, Shock and Wide Blast (+2 nearby enemies). | Attack-event ordering, boss eligibility, bounded stable neighbours and no retroactive trigger or sprite-derived gameplay. |
| F5: one-hit Echo penalises fast attackers | One echo per fighter accumulates a share of every eligible hit during the original window. | Eligible source/target categories, caps and rounding, original-window limits, target invalidation and the actual damage trade. |
| F6: final Ghost Step loses natural-return defence | Only manual Recall bursts the surviving decoy. Natural return leaves it catching hits for its remaining original lifetime. Final form renamed Shatter. | No natural-return burst, no double catch/burst after consumption, unchanged original expiry and longer reuse. |
| F7: dead chief specialisations | Before a chief fight, park an enhanced button and use its base form for that fight. | Entire route benefit/drawback removed before combat; no third route, lingering payload or live respec. Actual chief attack coverage still needs play evidence. |
| F8: discovery unusable until run ends | A route unlock grants one free swap at the next safe screen. | One-use saved entitlement and explicit reconciliation with the later road-pick/replacement flow at the two-button cap. Unlocking cannot silently grant an unoffered form. |
| F9: prescribed universal-slot depth | Both buttons share form gates: second from tier 3, final from tier 5; slice seconds from campaign tier 2. Road picks now choose actual depth. Camp unlocks later-form eligibility for 150/250 materials. | Seeded offers, prerequisites, purchase granularity, replacement/retention and run clearing. No inherited permanent slot capacity. Command Relay now halves one weakness and no longer opens a form one tier early. |

Mirror's last form is **Pierce**, replacing Fold. Sanctuary's parry still counts as parried even though it gives up stagger. Black-hole pins/time freezes do not generally create Snare marks; **Singularity Net is the deliberate Trapper exception**, applying Snare to dragged neighbours without extra Bleed. Only bars and Shade routes directly reward a parry; the accepted held-bar boost to Butcher combos remains +10 percent per bar. Whisperer's offensive pairs bond classes from pickup, with free answers rather than spent spells. Mend and Hold Guards both patient and Medic, and Gatekeeper is excluded from slice picks. The earlier parked-bonds and lost-Medic-Guard assumptions are withdrawn.

**Evidence boundary:** these are owner resolutions recorded in locks/handovers, not independently tested Shade repairs. Claude's early Pal bots used a throwaway model. Later **real-sim Butcher** tests/sweeps supersede that model's loser list: all fifteen combos, rethinks and hooks are handed back in rules 15/save 23; redesigned Shade/Whisperer/Glutton still await code. Heavy is 1.25 times the clock and nerve/health pacts cost an eighth. Original audit arithmetic and 24-pair inspection remain dated 27 October evidence, not a new audit of current code. This reconciliation runs document checks only.

The latest 30 October duel-wearer log changes interpretation of the real run sweeps: class sent to duel dominates sampled results, with plain Trapper winning all sampled duels under both untimed-parry pilots. Earlier default-Warden combo conclusions are not general balance verdicts. Claude reports a possible fixed-answer duel risk; neither this paper nor a frontend integration is authorised to tune it away. No fresh sweep, Unity validation or owner play is claimed here.

The original Gates A-C have accepted design directions. Exact sim contracts, numbers and the road acquisition transition still need a handback. **Gate D remains pending:** authoritative prototype, determinism/save/replay evidence, actual input/animation checks and Jacob's play verdict. No clean gameplay pass is awarded. General risks such as burst strength, reserve size and limited boss coverage survive until tested even where the old specification defect is resolved.

The latest lock explicitly leaves the run-1 habits, Node Cache and Synthesis Discount wording, Ambusher's heavy-blow definition and codex point total open. For Shade, safe road replacement, form-unlock purchase granularity and Command Relay's effect on categorical weaknesses require explicit authoritative wording. This paper records those gaps rather than inventing rules.

<details>
<summary>Historical evidence: original 27 October audit, findings and proposed gates</summary>

27 October 2026. Requested by Jacob. **Plan only. Design verdict: REWORK REQUIRED before implementation.**

Reviewed [Shade command specialisations](Shade%20command%20specialisations.md), the current owner calls and shared log, and relevant command/attack scheduling in the current source. Nine findings survive the second pass: four priority-1 rule/compatibility blockers, four priority-2 conditional or progression problems, and one priority-2 design-fit risk. Their classifications below matter: a missing contract is not a reproduced exploit, and a conditional weak choice is not universally useless.

Both passes were performed by Codex. The second pass challenged the first pass and the suggested repairs; it is not an independent reviewer or gameplay audit. All 24 legal two-button route pairings were inspected on paper. Two arithmetic examples were calculated directly; no Unity session, build, balance sweep, bot battle or save was run. The original route rules remain unchanged. Proposed repairs below are not new design locks.

## Priority 1: settle these rules first

### F1. Sanctuary's second form can kill an ally who survives with its first form

**Confirmed conditional design regression.** The [Sanctuary rules](Shade%20command%20specialisations.md#sanctuary-turn-a-parry-into-a-rescue-window) let an exhausted shield automatically borrow from every other shield. Later forms inherit Relay; the paper offers no way to keep Sanctuary while declining that behaviour.

Consider two living fighters. A has 100 HP, B has 5 HP. Each has 10 shield. One enemy area attack resolves 15 damage to A, then 10 to B. These are illustrative values, not current class statistics.

| Outcome | First form | Second form with Relay |
|---|---|---|
| Hit on A | A spends its 10 shield and loses 5 HP. | A spends its own 10 plus 5 borrowed from B; loses no HP. |
| Hit on B | Its intact 10 shield absorbs the hit; B survives at 5 HP. | Its remaining 5 shield absorbs half; the rest kills B. |

Total protection is conserved, but survival is worse. Stable event ordering makes this repeatable; determinism does not make the allocation sensible. The paper acknowledges exposure as a tradeoff, so this is not a hidden arithmetic contradiction. The problem is forcing a survival-changing tradeoff into an automatic upgrade without giving the player control. At the final form this can also needlessly consume the once-per-battle death save.

**Recommended repair:** give Relay a small, finite shared reserve created with the field. It may help a broken shield without draining another fighter's personal protection. Keep the route's lost-stagger cost. An alternative is optional redistribution with clear donor protection, but that adds control/UI complexity.

**Re-audit of the repair:** a separate reserve adds power; do not pretend it preserves the old total shield budget. Set its total explicitly, prohibit refills, share the field expiry and grant lockout, and specify who gets it during simultaneous hits. The reserve must not make Last refuge renewable. Do not claim this proposal is balanced until the total shield/health budget is measured.

### F2. Rupture's final volley depends on a readiness window the automatic combat loop does not expose

**Confirmed integration/definition gap.** [Rupture](Shade%20command%20specialisations.md#rupture-prepare-one-enemy-choose-the-moment) promises a volley from offensive fighters who are currently ready, without waiting or changing readiness.

In [BattleSimulation.Tick](../Assets/Scripts/Sim/Battle/BattleSimulation.cs), lines 660-664 at review, an eligible fighter's cooldown is decremented, its action immediately executes when due, then its next cooldown is added. [ReplayRunner](../Assets/Scripts/Sim/ReplayRunner.cs), lines 35-54, applies commands between complete ticks. [ActWithSpell](../Assets/Scripts/Sim/Battle/BattleSimulation.Class.cs) also spends a due action on a class spell when appropriate. In ordinary uninterrupted combat there is no stable bank of ready, idle attacks for the player to cash in.

**Trigger:** three real attacks prepare Rupture. Those same attacks have just entered recovery. Press the lit final-form button: the rupture damage happens, but the advertised squad volley can have no participants. Defining ready as one tick before firing still creates a tiny scheduling test, not a readable command opportunity. This is not proof that no exceptional state could ever produce an eligible fighter; it is enough to invalidate the promised reliable capstone as currently specified.

**Recommended repair:** explicitly let the command bring a bounded number of nearly ready ordinary attacks forward, then pay back the borrowed time in their next recovery. Do not borrow class casts, interrupt committed actions or give a Medic an attack. This is a new command rule for approval, not something the frontend should simulate.

**Re-audit of the repair:** with a 60-tick attack interval and 12 ticks left, the normal next attacks would be at ticks 12 and 72. A lawful borrowed strike occurs now, with the next still due at 72. Resetting recovery to 60 instead would manufacture extra tempo. Track one action/cycle and the original due time, survive save/reload, exclude unready-beyond-limit or busy units, and never clear the debt by recasting. Finisher-tagged attacks must not refill a new Rupture mark and bootstrap repeated volleys. An alternative simpler design is a brief payoff on the next natural hits, but it must not be advertised as an immediate coordinated volley.

### F3. Follow-up releases lack a complete activation and reuse contract

**Confirmed specification gap; conditional exploit risk, not a proven infinite loop.** The [shared rules](Shade%20command%20specialisations.md#shared-rules-and-bounded-starting-parameters) include free follow-up presses. Breach defines an explicit no-restart period; Echo and Rupture do not define an equivalent original activation budget after an early release. First-form Echo also lacks an explicit answer to an attempted new activation while it is already recording.

Current [CanUse/TryApplyOrder/ApplyDirective](../Assets/Scripts/Sim/Battle/BattleSimulation.cs) checks ordinary command resources and sets command timers; the inspected ordinary Focus/Unleash path has no general lockout that can be assumed to solve this. ApplyDirective also invokes normal command combos and one-time obstacle effects. Those existing facts are integration context, not permission to retain every old rule in a new version.

**Trigger:** start Echo, record one favourable attack, release early, start again as soon as normal resources allow. If ending the buff resets its quota/reuse window, repeated short recordings may beat the advertised wait-for-the-squad decision. If a release is routed through normal Unleash activation, it can also accidentally fire normal Riposte/Ambush/obstacle hooks again. The document forbids repeated utility in prose but does not specify the input/state boundary that enforces it.

**Recommended repair:** explicitly distinguish Start, Release and Retarget, with a parent activation ID, original start/deadline, consumed payload IDs and an earliest new activation time. A release consumes that parent's prepared payload once. It never invokes ordinary activation hooks. Paying for a new activation is necessary but not automatically sufficient to make repeat-burst behaviour acceptable.

**Re-audit of the repair:** reject duplicate releases, zero-payload releases, releases after target invalidation, reload duplication and changed-target reuse. A new paid activation must not inherit an old payload. Pick the actual reuse policy rather than blindly adding a long cooldown to every command. Historical replay commands retain their old meaning. This closes ambiguity on paper; it does not establish a safe burst rate without the resource/damage numbers.

### F4. Rift Mine and Breach depend on spatial rules that the paper has not defined

**Confirmed design/integration gap.** [Rift Mine](Shade%20command%20specialisations.md#rift-mine-punish-enemies-for-following-you) needs an enemy to enter an abandoned line, then move other enemies into its blast. [Breach](Shade%20command%20specialisations.md#breach-open-the-line-then-drive-through) pushes enemies and opens a gap. Neither establishes the authoritative trigger areas, movement events or target/lane selection.

The inspected [BattleUnit](../Assets/Scripts/Sim/Battle/BattleTypes.cs), lines 665-669, has a fixed index and row. [LanePosition](../Assets/Scripts/Sim/Battle/BattleSimulation.cs), lines 1901-1917, derives position across a row from setup order; dead fighters retain their places. Existing reach and specific boss exceptions are not a general move-through-a-zone contract. Visible movement in the HUD cannot establish a mine trigger.

**Trigger:** an ordinary melee enemy keeps attacking from its logical row, or a stationary chief never enters a zone. The player pays longer withdrawal, but the mine has no defined legal entry event. Conversely, faking the entry from sprite movement would make gameplay depend on presentation and threaten replays. A Breach push might move enemies away from the mine; it is not automatically a combo.

**Recommended repair:** Claude must define the minimum authoritative lane/engagement events, area selection, entry, pull and push rules before this route is approved. A smaller alternative is an explicitly event-triggered counterfield responding to an eligible enemy recommitment after withdrawal. That would be a different mechanic and must be named/taught honestly. Do not silently build general pathfinding or use animation positions as game state.

**Re-audit of the repair:** retain anchored-boss immunity, no retroactive trigger on the attack already dodged, one mine, stable target limits and one blast. Do not make every boss movable just to make the route useful. The illustrative Rift Mine/Rupture fight is not timing proof; a clearer first showcase is mine, return, then Focus, rather than assuming one short Focus window spans an entire multi-beat attack plus retreat and re-entry.

## Priority 2: avoid false upgrades and dead ends

### F5. Echo's damage trade gets worse as ordinary attack count rises

**Confirmed conditional arithmetic weakness, not a universal damage verdict.** [Echo](Shade%20command%20specialisations.md#echo-prepare-a-second-strike-decide-where-it-lands) halves the normal Unleash bonus on every hit but compensates with just one recorded hit per fighter.

Let b be ordinary damage per hit, a the normal Unleash bonus, N the number of ordinary hits during the same full window, and q the echo fraction. Assume an unchanged target, identical mitigation, all hits land and the echo arrives. Then:

- Ordinary Unleash: N * b * (1 + a).
- Echo: N * b * (1 + a/2) + q * b * (1 + a/2).
- Difference: b * [q * (1 + a/2) - N * a/2].

Using the paper's illustrative +50 percent bonus, b = 100, and its highest candidate q = 0.6:

| Real hits during window | Ordinary Unleash | Echo including successful replay |
|---|---:|---:|
| 1 | 150 | 200 |
| 2 | 300 | 325 |
| 3 | 450 | 450 |
| 4 | 600 | 575 |
| 5 | 750 | 700 |

This does not assert that a current default fighter lands four or five ordinary hits in that window. It establishes a route-scaling problem when such a speed build exists. Redirecting a stored attack may still be valuable; there is no redirection benefit against the same isolated target. Low q, lost echoes and class actions excluded from recording can worsen the trade.

**Recommended repair:** let each fighter's one visible echo accumulate a bounded fraction of its actual eligible damage during the original window. Keep one projection per fighter, not a copy of every pellet. Alternatively redesign the immediate penalty rather than penalising every hit to pay for one fixed extra hit.

**Re-audit of the repair:** only real eligible ordinary damage contributes; generated echoes, mine blasts and reflections cannot feed it. Early release cannot reset the original recording quota/reuse budget (F3). A hard storage ceiling can recreate the same speed penalty if the damage penalty continues after the bank is full; define that case explicitly. This is a candidate fix, not a chosen damage multiplier or a balance pass.

### F6. Ghost Step's last form removes part of its earlier defensive choice

**Confirmed conditional behaviour change; coverage risk.** [Ghost Step](Shade%20command%20specialisations.md#ghost-step-make-them-strike-a-false-formation) says return, whether manual or natural, collapses the decoy. Earlier forms can leave it alive until its lifetime expires. The final form therefore removes any remaining post-return interception window.

**Trigger:** withdraw from ordinary melee enemies who cannot reach the retreat. The decoy survives because they cannot hit the squad. Natural return destroys it for a blast just as they can threaten the squad again. The advertised choice to leave it baiting is not available then. Where the burst hits nobody, the higher form loses defence without even a compensating hit.

**Re-audit correction:** the first-pass broad suspicion that the decoy could never work was too strong. [MeleeHeldBack](../Assets/Scripts/Sim/Battle/BattleSimulation.cs), lines 1986-1997, explicitly exempts ranged and frenzied attackers and allows Long Reach in some circumstances. Ghost Step has real potential during withdrawal against eligible ranged attacks. Do not label the whole route useless.

**Recommended repair:** manual Recall can collapse the decoy; natural return leaves it alive for the remainder of its original lifetime. Then early offence and later interception remain real alternatives. No extra timer refresh, second decoy or extended dodge.

**Re-audit of the repair:** returning naturally must not grant both a free explosion and the retained defensive decoy. Keep the original longer reuse time; an expired or consumed decoy cannot explode. This improves agency without adding another button or making a named boss attack redirectable.

### F7. Several specialisations are worse than their base command in predictable chief encounters

**Confirmed conditional coverage gap; specialist weakness itself is allowed.** Compare the stated rules against a lone, stationary chief with no eligible ordinary decoy attack:

| Route | Lost value in that encounter | Cost that remains |
|---|---|---|
| Death Warrant | No transfer, death burst or carry while the chief lives; kneeling does not trigger them. | Shorter Focus window for the same activation cost. |
| Rift Mine | No legal entry if the chief does not advance; control immunity can remove later benefits. | Longer return and lost squad attacking time. |
| Ghost Step, first form | No eligible decoy attack if every attack is excluded by its boss/named/area rules. | Longer wait before another Fall Back. |
| Mirror, later forms | A single-beat, lone enemy supplies no earlier beat or target behind it. | No new late-form benefit in this case; the first-form return remains useful and does not make base Parry worse. |

The [camp-only switching rule](Shade%20command%20specialisations.md#progression-and-changing-your-mind) locks the choice through a multi-tribe run containing chiefs. The route may be a worthwhile crowd specialisation overall, but the player cannot opt back into the ordinary button for its dead encounter. This is especially awkward for Fall Back, whose role is the safer alternative to Parry.

**Recommended repair:** preserve specialisation weaknesses, but allow parking an enhancement at an announced safe pre-encounter point and using that button's base form. The parked position stays occupied; it cannot be filled with another fully developed route for that encounter. A more limited alternative is safe choice at each region boundary with the coming encounter types disclosed. Do not silently combine these into unrestricted free boss-by-boss optimisation.

**Re-audit of the repair:** park the complete route, both advantages and drawbacks; clear all pending payloads before the battle, never on a live button press. The unused slot is an actual opportunity cost. For Sanctuary, no shield persists while its stagger penalty disappears. Specialists need not match single-target routes against every chief. Actual boss move coverage still needs a content table; these scenarios are not claims about every named chief's final kit.

### F8. Tier-2 alternatives unlock during a run but cannot be tried in that run

**Confirmed progression/UX conflict.** The [progression table](Shade%20command%20specialisations.md#progression-and-changing-your-mind) unlocks the four alternatives when Tier 2 is first reached. The switching paragraph allows actual route changes only at camp; the new-milestone exception develops only the current pair.

**Trigger:** reach Tier 2 for the first time, unlock an exciting alternative, and continue the run. There is no legal place to equip it. Ending the run becomes the quickest way to try the reward. It is not a farming exploit or a softlock, but it interrupts the promised progression experience.

**Recommended repair:** claiming a newly unlocked route grants one safe between-fight replacement opportunity for that discovery. Keep the two-button cap and inherited development. It may also be deferred to camp. No automatic replacement of the player's selected build.

**Re-audit of the repair:** record discovery and claim separately, consume the opportunity once, and do not grant it again on reload or a repeated Tier-2 visit. A single controlled discovery choice does not require unrestricted respec after every fight. Keep this separate from any accepted F7 parking policy.

### F9. Progression develops two universal slots more than a chosen route

**Design-fit and power-curve risk, not a correctness bug.** The [slot capacity rules](Shade%20command%20specialisations.md#progression-and-changing-your-mind) prescribe 1/1, then 2/1, 2/2, 3/2 and 3/3. By Tier 6 every unlocked route can be exchanged into full development. Choices change the equipped pair and which slot develops first, but there is no decision between taking one route deep early and developing both evenly.

That only partly delivers Jacob's request to choose a side and take it to its fullest. The permanent full capacities also apply in future Tier-1 fights. This was disclosed in the original proposal; it is not a newly discovered save bug. Without final damage/control values, nobody can honestly prove that it trivialises early runs. Nor can we claim the whole game lacks loss progression: Butcher and Glutton may provide it, and their final handovers are not available here.

**Recommended direction:** keep permanent discoveries and free camp experimentation. Consider a refundable development budget that permits 3/1 versus 2/2 at a meaningful midgame milestone, with prerequisites and a maximum of two enhanced commands. That requires deciding when the final form becomes available; do not call it a repair while secretly retaining a gate that prevents the choice. Ensure both routes can still reach their final form before the Sovereign.

**Re-audit of the direction:** moving a final form earlier changes power pacing, so this is an owner design choice, not an automatic fix. Do not add forced usage grinding, irreversible purchases, a new currency, per-run loss of permanent Pal development or enemy scaling in the name of solving it. Check early-tier encounters with the developed pair and the experience of repeatedly losing before Tier 3. Retain the current capacity scheme if Jacob prefers a simple loadout system, but describe that choice honestly.

## Re-audit of all 24 legal pairings

Pairs use different buttons. Mirror + Sanctuary, Ghost Step + Rift Mine, Rupture + Death Warrant and Breach + Echo are illegal combinations and were not treated as builds. Entries below describe paper interactions, not executed fights. Every pair still inherits its individual route findings above.

| Pair | Second-pass result |
|---|---|
| Mirror + Ghost Step | Dodging a beat loses the clean-sequence return. Legitimate defensive choice, not double parry reward. |
| Mirror + Rift Mine | Mine damage earns no parry segment. Spatial trigger still unresolved. |
| Sanctuary + Ghost Step | Decoy interception consumes no HP shield and earns no parry segment. Finite layered defence is not automatically broken. |
| Sanctuary + Rift Mine | Retreat spends field lifetime and attack time; the mine cannot extend or refill the field. |
| Mirror + Rupture | Return damage cannot fill the weak point. A return kill discards the dead target's preparation. |
| Mirror + Death Warrant | An attributed return kill can qualify; Death Warrant secondary damage cannot continue a cascade. |
| Sanctuary + Rupture | Lost stagger may leave enemy cover intact. The pair does not automatically get both safe defence and an uncovered target. |
| Sanctuary + Death Warrant | Short Focus and lost stagger can make timely kills harder. Weakness to inspect, not a demonstrated exploit. |
| Mirror + Breach | Finite burst after a clean sequence. Coefficients are missing, so dominance is unproven. |
| Mirror + Echo | Reflections are not ordinary attacks and cannot be recorded. No reflection/echo recursion under the written rules. |
| Sanctuary + Breach | Shields can soften exposure, but use both enhancement positions and finite protection. This interaction is not itself a bug. |
| Sanctuary + Echo | Protection can buy time for recorded attacks; neither budget refreshes the other. F1/F5 remain. |
| Ghost Step + Rupture | Recall can restore attacking time; its blast cannot prepare Rupture. Mark expiry remains relevant. |
| Ghost Step + Death Warrant | An eligible decoy kill may transfer Focus. Secondary burst damage creates no new decoy. |
| Rift Mine + Rupture | Grouping can help real attacks, but mine damage does not charge the mark. Long setup can exhaust Focus. |
| Rift Mine + Death Warrant | Intended crowd combination; needs actual entry geometry and the same bounded transfer rules. |
| Ghost Step + Breach | Dodge legitimately avoids eligible damage but cannot cancel exposure. Keep command pulses distinct from real attacks during withdrawal. |
| Ghost Step + Echo | Real withdrawn attacks cannot add recordings under the proposal. Explicitly decide whether already stored projections may release during retreat; do not invent fresh fighter attacks. |
| Rift Mine + Breach | A forward push may drive foes away from the mine. Not an automatic combo without defined spatial relationships. |
| Rift Mine + Echo | Trap damage cannot be recorded. Longer withdrawal can cost recording opportunities. |
| Rupture + Breach | Impact/Ram do not build the weak point. One-time obstacle utility and real volley action budgets remain crucial. |
| Rupture + Echo | Converge can release before rupture. Rupture ends Focus, so the reverse order loses the Converge target until another paid Focus. Order-sensitive, not impossible. |
| Death Warrant + Breach | Primary Impact/Ram kills can qualify within the two-transfer limit. Secondary kill damage cannot make an infinite loop or repeat wall utility. |
| Death Warrant + Echo | Converge commits to its target at release. If an early echo kills it and Focus transfers, already committed echoes cannot silently retarget to the new victim. Automatic original-target replay remains a different option. |

**No infinite chain confirmed under the written caps and source exclusions.** That does not clear the unspecified activation/rearm budget in F3 or prove damage balance. No pair should secretly require a third enhanced button, a bond or a particular class talent to function.

## What the second pass changed

- Rejected the blanket claim that Ghost Step has no usable attacks to catch: ranged attacks are an explicit current-source exception. F6 remains a narrower late-form/melee coverage problem.
- Narrowed Echo from generally bad to a demonstrated attack-count problem. Its own arithmetic shows an advantage at one or two hits in the example; redirection has separate value.
- Rejected an infinite Death Warrant kill-loop finding: transfer caps and no-cascade flags already forbid it. Whether those rules are implemented later is a different question.
- Rejected the claim that Rupture and Echo cannot combine: releasing Echo before rupture is legal. Releasing rupture first loses Focus and must not receive a free target restoration.
- Did not label Sanctuary + Breach broken merely because one mitigates the other's risk. The two-slot cost, finite shields and persistent exposure matter.
- Kept the permanent-power issue as a design risk, not a proven easy-mode verdict. The new system is not implemented and its coefficients are unspecified.
- Rechecked each recommended repair for new costs: Relay adds a reserve budget; borrowed attacks need conserved recovery; Echo accumulation needs an original-window limit; a retained decoy must not also explode; parking must remove the whole route; discovery swaps must be one-use; deeper early allocation changes unlock pacing.

## Proposed correction gates

**Gate A — write usable rules.** Resolve F1-F4 first: safe shield distribution, a real finisher scheduling contract, explicit activation/release/reuse identity, and authoritative spatial events. Provide example state traces, including invalid targets and save/resume. Do not begin a HUD-only implementation of rules the sim has not defined.

**Gate B — make each route an honest choice.** Correct Echo's attack-count penalty and Ghost Step's forced late-form behaviour. List each route's useful and useless encounter conditions. Choose an explicit safe policy for parking a dead enhancement or committing by region. Preserve weaknesses instead of making every route universally strong.

**Gate C — settle progression.** Make a newly unlocked alternative usable without needing to end the run. Decide between universal slot capacity and a budget that supports early depth. Preserve two enhanced buttons, permanent discoveries and the seven-tier structure. Resolve early-run power through owner play, not assumed enemy scaling.

**Gate D — only after the design is accepted, prototype and evaluate.** One weak squad and one developed squad; crowd, lone chief and encounter with no eligible movement; missed parries and successful dodges; recorded attacks with dead targets; all four ordinary commands still useful. Later determinism, save/replay and input tests validate implementation. Owner play establishes readability and whether the routes actually feel distinct. No pass is claimed for these future gates.

## Evidence and limits

Local evidence: the linked route sections; current [DESIGN_LOCKS](../DESIGN_LOCKS.md) and [HANDOFF_LOG](../HANDOFF_LOG.md); [BattleSimulation](../Assets/Scripts/Sim/Battle/BattleSimulation.cs), [BattleSimulation.Class](../Assets/Scripts/Sim/Battle/BattleSimulation.Class.cs), [BattleSimulation.Tribes](../Assets/Scripts/Sim/Battle/BattleSimulation.Tribes.cs), [BattleTypes](../Assets/Scripts/Sim/Battle/BattleTypes.cs) and [ReplayRunner](../Assets/Scripts/Sim/ReplayRunner.cs). Current code is used to expose integration assumptions, not as proof that the newest proposed combat rules are already built.

Primary research refreshed: [Last Epoch's skill specialisation support](https://support.lastepoch.com/hc/en-us/articles/46363203944859-Skill-Specialization) supports investing in familiar skills and changing specialisation; [its Sentinel skill documentation](https://support.lastepoch.com/hc/en-us/articles/46362203855515-Sentinel-Skill-Tree) gives an example of a follow-up action with an explicit longer cooldown. Neither proves KS's balance, requires copying re-levelling, or settles our progression policy. The defect findings come from KS's own rules, source and arithmetic.

Still missing: final approved Butcher/Glutton integration contracts, final damage/area/cost numbers, a complete boss coverage table, the new authoritative timing/resource contract, and real input/animation/play evidence. This report is a bounded design audit and re-audit, not a clean gameplay pass. Findings remain open until the proposal is revised and the relevant future evidence exists.

</details>
