# Early build choices: research and recommendation

26 October 2026 · Codex · For Jacob and Claude

**Research and proposal only. No implementation approved or performed.** This supersedes my unsupported recommendation to add branching class talents at HANDOFF_LOG.md:11774. It does not approve Claude's rules 15 paper or change the design locks.

## My answer

**Make choosing the starting squad the first build decision. Prove two different squad strategies on the opening road before adding a talent tree, more classes, or another upgrade system.**

The player's first choice should be which fighters to take and which capabilities to leave behind. Existing class kits should supply the utility: protection, control, healing, ranged pressure and melee finishing. Those capabilities must cooperate without the player first earning a bond or pressing Hold correctly.

This is a recommendation for the next bounded prototype, not a claim that squad selection alone solves the whole roguelike. A squad selected once and left unchanged for four fights has limited evolution. For repeated build decisions before recruitment, we must eventually choose a recurring source of change: abilities, equipment, shared run modifiers, or access to different units. Renaming one of those does not make it a different system.

**My previous answer failed twice:** it presented class talents without researching the alternatives, and its example let the Marksman spread a snare even though only the Trapper may snare. Withdraw that example. Research should precede future substantive design recommendations, with verified facts separated from proposals.

## What I actually checked

I read developer/publisher descriptions for nine games, Riot's design retrospectives, Supergiant's development notes, the current Kindred Siege locks and relevant campaign/class/chain code. I compared these with Jacob's recorded rules 14 results. I did not play those reference games during this research, rewatch the three earlier videos, or run a new Kindred balance sweep. Exact minute-by-minute onboarding claims are therefore deliberately absent.

Labels used here:

- **Source fact:** a mechanic described by its developer/publisher. Marketing descriptions establish the advertised mechanic, not its quality or causal effect on retention.
- **Repository fact:** a rule or implementation found locally. Source inspection does not prove it feels good.
- **Inference:** my interpretation of those facts.
- **Proposal:** a change requiring Jacob's decision and later playtesting.

“Successful” is not used indiscriminately. The accessed Steam pages show substantial positive reception for Hades, Slay the Spire, Balatro, Monster Train, Super Auto Pets and Backpack Battles. As a dated reception proxy, their displayed English-review totals were approximately 143k/98%, 77k/97%, 105k/98%, 12k/96%, 25k/91% and 5.6k/92%, respectively. These are not sales or retention measurements and do not prove any one mechanic caused success. Time Walker is a smaller, relevant released comparison. **Warpack and Northgard Battlegrounds are design references, not proven commercial successes:** the pages accessed on 26 October list their full releases for 10 November and 27 October 2026, with demos available. Sources are linked in the comparison below.

## What the games actually let players build

The final column is my inference about transfer to Kindred, not a developer recommendation.

| Game and primary source | What the player chooses or assembles | What connects the pieces | What transfers; what does not |
|---|---|---|---|
| [Hades — developer's Steam page](https://store.steampowered.com/app/1145360/Hades/) | Weapons and god boons enhance the actions the player uses. Permanent progression also exists. | Powers modify an already playable action kit; different choices produce different combinations. | Put a useful kit in the player's hands first. Copying its boon structure would still mean choosing a boon-like system; it would not answer Jacob's objection merely by changing the name. |
| [Slay the Spire — Mega Crit](https://store.steampowered.com/app/646570/Slay_the_Spire/) | Cards, relics and a route with different risks and rewards. | Cards work together; relics alter their value and interactions. | The player chooses ingredients repeatedly, rather than merely watching a preset combo. Do not import a card interface or an entire deckbuilder without a reason. |
| [Balatro — developer/publisher listing](https://store.steampowered.com/app/2379780/Balatro/) | Poker hands, Jokers and several kinds of run-changing cards/tools. | Jokers and card changes interact with how hands score. | A familiar base action can support many builds when the player can alter its consequences. Another combat resource is not automatically necessary. |
| [Monster Train — developer/publisher listing](https://store.steampowered.com/app/1102190/Monster_Train/) | Primary and supporting clans, route benefits, cards and upgrades. | The two clan pools combine; upgrades and deployment across three floors alter their use. | Starting composition and later evolution are distinct decisions. Kindred needs to decide both; “the roster is the build” is only half an answer. |
| [Super Auto Pets — Team Wood Games](https://store.steampowered.com/app/1714040/Super_Auto_Pets/) | Teams of pets with individual abilities; available pools differ between standard, custom and weekly packs. | Mixing compatible abilities produces team combinations. | Let choosing the team matter before combat. Do not assume a player's single fixed team provides the same agency as a team they assemble. |
| [Backpack Battles — developer/publisher listing](https://store.steampowered.com/app/2427700/Backpack_Battles/) | Purchased and crafted items, their placement, and a class. | Item shapes and arrangement influence a build's strength. | Preparation contains actual decisions with visible objects. Its inventory game is central; removing equipment management removes much of the reference's build mechanism. |
| [Time Walker: Dark World — developer/publisher listing](https://store.steampowered.com/app/2093910/Time_Walker_Dark_World/) | Four heroes with assigned abilities, resource spending, merging and reforging; talents also unlock content. | Abilities can combine without class limits. | Borrow the readable connection between selected abilities and automatic results. Its unrestricted skill assignment directly conflicts with Kindred's class-role lock. |
| [Warpack — developer listing](https://store.steampowered.com/app/5016130/Warpack/) | Army composition, each unit's 3×3 equipment layout, unit consumption and commander skills. | Adjacency, transferred traits and drafted commander effects shape the army. | Jacob correctly identified substantial preparation. Copying it requires accepting some of those decisions; it is not evidence that fixed kits alone create equivalent variety. Its cross-role equipment also conflicts with Kindred's class lock. |
| [Northgard Battlegrounds — developer/publisher listing](https://store.steampowered.com/app/4487060/Northgard_Battlegrounds/) | Clan, settlement placement, economic/military buildings and divine blessings. | Buildings, resources, warband production and blessings interact. | Its preparation is a settlement-building game. Two combat rows are not equivalent to that spatial/economic system. Importing the whole thing would be a major scope change. |

### What the developers say about the choices

Riot's [2021 Hextech Augment explanation](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/gizmos-gadgets-set-mechanic-overview-hextech-augments/) describes repeated choose-one-of-three decisions that shape strategy. Its [2022 examples](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/augments-augmented/) include Double Trouble, which rewards exactly two copies of a unit. This is an example of a modifier changing what team the player wants, rather than just increasing every team's damage. These are historical examples, not claims about the current TFT set.

Riot's [Gizmos & Gadgets retrospective](https://teamfighttactics.leagueoflegends.com/en-au/news/dev/dev-teamfight-tactics-gizmos-gadgets-learnings/) credits clear decision moments and match variance, while acknowledging dead item components, inconsistent offers and power-balance problems. **Inference:** offering choices is not enough. A choice must be useful for the available squad, and the offer system must not repeatedly deliver unusable options. This retrospective is developer testimony, not an independent experiment.

Supergiant's [Beefy Update](https://www.supergiantgames.com/blog/get-pumped-for-the-beefy-update/) discusses both core dash improvements and additional weapon/boon combinations. Its [Big Bad Update](https://www.supergiantgames.com/blog/brace-for-the-big-bad-update/) adds duo powers. **Inference:** satisfying execution and interesting build combinations are separate responsibilities. One cannot compensate reliably for broken feedback in the other.

## The finding that changes my recommendation

**These games expose something the player can assemble, then repeatedly show the consequences. They do not get their variety merely by having many named classes or by automatically triggering combos.** This is a synthesis of the comparisons, not a universal law or a claim that every successful game uses the same formula.

For Kindred, distinguish three things:

| Layer | Player question | Our current problem |
|---|---|---|
| Functional kit | What can these fighters do? | Existing protection, healing, control and mark/cash rules are hard to read and can fail to connect. |
| Build choice | Which strengths do I bring, and what do I give up? | The opening squad supplies very little composition choice. Automatic teamwork is being presented as player-authored variety. |
| Run evolution | What do I take now that changes my next decision? | Several reward systems exist, but the owner has rejected using more gear slots, ordinary kit functions sold as boons, and bonds-like picks as the answer. No replacement has been chosen. |

A talent tree can be one delivery mechanism for the third layer. It does not establish the other two. That is why my first response was inadequate.

## What Kindred already has, and where the assumptions break

**Repository facts:**

1. [Campaign.cs](../Assets/Scripts/Sim/Learning/Campaign.cs), `TopUpReserve`, creates fresh Clarth fighters by cycling the starter roster. `BeginFight` already selects a squad through `DefaultSquad` or `PickSquad`. A selected squad is therefore an existing concept; an opening composition selector is not already a finished feature.
2. The campaign describes the Clarth as allies after the tutorial and fresh squad providers between runs. Choosing the composition they send is conceptually different from capturing enemy recruits. **It would still change the opening campaign contract and needs approval.** Do not assert that kneel-only recruitment already authorizes it.
3. [ClassSpells.cs](../Assets/Scripts/Sim/Battle/ClassSpells.cs) already contains choices such as Deadeye versus Quickdraw, Retaliator, Barbs, Long Snare, Surgeon and Field Kit. For example, a stronger slow aim and a weaker fast uninterruptible aim already exist as class bends. A new talent tree could duplicate existing behaviour.
4. [BattleSimulation.Chains.cs](../Assets/Scripts/Sim/Battle/BattleSimulation.Chains.cs), `React`, chooses one eligible casher, with bonded/ready priority. Two Marksmen are **not** automatically two simultaneous pin payoffs. Existing eligibility, targeting and cooldown rules may make duplicates compete instead of cooperate. Inspect and demonstrate this before selling a “double gunner combo.”
5. [DESIGN_LOCKS.md](../DESIGN_LOCKS.md), “The squad's spells, marks and class pieces,” preserves class ownership: the Trapper snares; the Medic does not become a damage dealer. The earlier proposal to have a Marksman distribute snares crossed that boundary.
6. The same lock records class-bound gear and order sockets. Those are existing design history, not permission to disregard Jacob's newer rejection of an equipment-heavy answer. Any reuse or replacement must be stated plainly.
7. The roster also represents lives. Giving the player seven real reserve fighters to choose five increases their loss buffer. A composition prototype must preserve the starting headcount rather than quietly grant extra lives.

**Actual owner-play evidence**, recorded in [HANDOFF_LOG.md](../HANDOFF_LOG.md) under “Owner rules 14 gameplay diagnosis”: four road wins; one of those wins had zero parries and no allied deaths. Five inputs landed within the HUD's gold conditions but retained an older Hold timestamp. The chief defeat was a shared morale break with five survivors and the chief at 118 HP. These findings establish feedback and loss-condition problems in this run. They do not establish a universal difficulty balance.

## Which direction fits the brief best?

| Candidate | What it genuinely adds | Cost or conflict | My recommendation |
|---|---|---|---|
| Starting squad composition | An immediate choice of capabilities, redundancies and omissions | Changes starter selection; duplicate classes need functional support; fixed starting choice does not create continuous run growth | **Prototype first.** Closest to the squad-commanding fantasy and the content already present. |
| Branching class talents | More ability variants and repeat choices | My previous answer; potentially duplicates class bends and adds UI/content debt | Do not build a tree before proving what decisions are missing. |
| More per-fighter equipment | A potentially strong combination system | Explicitly rejected as the direction; more inventory burden | Do not smuggle it back as “modules.” |
| Shared run relics/doctrines | Global rules that can alter composition incentives | Mechanically still a modifier/boon system; needs its own justification and approval | An honest possible growth system, not a novel loophole around the rejection. |
| Formation alone | Exposure, reach and positional tradeoffs | Two rows have much less decision space than an economy/placement game; may produce a solved layout | Support the build; do not promise it supplies all build variety. |
| Earlier enemy recruitment | More available units and tribes | Changes the kneel-only acquisition rule | Unnecessary for the first composition experiment. |
| Player-built deployment machinery | A whole new preparation game | Assets, economy, space rules and a new screen; competes with the current combat repair | Too large a detour now. |

There is no research-backed trick that supplies an evolving build while holding the roster, abilities, equipment, run rules and deployment all fixed. At least one must be a meaningful choice. The question is which one deserves the player's attention.

## What I recommend doing next

### Gate 0 — Make the fight trustworthy

Repair and verify the gold/last-Hold mismatch and the delayed-beat contradiction against Jacob's archived inputs. If removing wait-out and fake-first is the approved direction from his Claude discussion, reconcile that decision before changing rules. Repeated-input eligibility still needs attention even after those two tricks disappear.

Show Focus acceptance and its actual target clearly; show a cast interruption when it happens. Make the chief's shared morale danger and the exact loss cause explicit. Resolve the difference between the older “parry negates” rules and the newly discussed “Cleave/Eclipse parry mitigates” proposal before teaching either. Version rule changes so historical replays retain their actual outcomes.

**Pass:** the shown instruction predicts the recorded outcome; a loss with living allies explains why; no promise that successful parry answered a second simultaneous threat.

### Gate 1 — Compare two starting compositions on paper, then in a bounded prototype

Proposed test compositions, using only the five starting classes and the normal five fighters plus leader:

| Candidate | Five fighters | Intended strength to test | Capability sacrificed |
|---|---|---|---|
| Ranged pressure | Warden, Medic, Trapper, Marksman, Marksman | Protection and control buy time for sustained ranged pressure; a second shooter may cover readiness gaps | No Skirmisher to cash Opening/Bleed through the existing melee reactions |
| Melee finishing | Warden, Medic, Trapper, Skirmisher, Skirmisher | Control/bleed feed close-range finishers; a second finisher may cover readiness gaps | No specialist Marksman for pin shots and aimed-shot utility; more melee exposure |

These are **test hypotheses, not balanced builds**. Keep the current mixed squad as the comparison baseline. The always-present leader also matters: a leader who already supplies enough shooting could make the “no Marksman” tradeoff much smaller than its label implies. Inspect that rather than hide it.

The first deliverable is an annotated trace showing what each candidate can actually do with current rules, which setups go unused and what becomes stronger or weaker. If duplicates only compete for the same mark, do not fix it with arbitrary damage buffs and claim synergy. Either give their existing kit behaviour a clear, role-correct contribution in a separately approved change, or reject that candidate. A boss must remain answerable through the universal command kit; a missing specialist may change efficiency but must not create an undisclosed impossible encounter.

For a playable comparison, let the player choose the five Clarth sent **before the first real road fight**, without changing tutorial survivors into different classes. Treat this as a proposed post-tutorial muster step, not a retrofit to historical saves. Unselected candidates are not extra reserve lives. No new tribe, class, gear slot or combat button is required by this test.

**Pass:** Jacob can describe why he chose each squad, identify one visible payoff and one disadvantage, and wants to try a different lineup. Merely winning with both does not pass. If the two squads feel the same, stop and revise the kits or reject the composition hypothesis; do not build menus around a failed distinction.

### Gate 2 — Choose the run-growth system explicitly

Once the first choice is enjoyable, decide what the player receives between fights that changes the plan. Do not pretend the first composition selection already solves this.

My preference is to use the smallest already-acceptable reward mechanism that strengthens an observed squad strategy. Existing class bends are a candidate for reuse, but their delivery currently involves gear and Jacob has challenged that direction. Moving them into class choices would still be a talent/ability-selection system. A shared modifier would still be boon-like. These costs must appear in the decision, not disappear under fiction or terminology.

For the growth-system paper, require a complete example: starting choice → first reward and rejected alternative → changed next fight → second reward → chief application. Every reward must work with the actual starting roster. No essential three-part combo that becomes functional only after the first chief; no crowd-only build whose entire payoff vanishes against a solo boss.

**Pass:** at least two decisions genuinely change the next choice or combat priority, and the owner prefers making them to skipping straight to battle. The count of two is a proposed small test scope, not a research-derived optimum. If the owner wants full Hades-style run growth immediately, this gate is a real design decision that cannot be skipped.

### Gate 3 — Expand only the part that proves enjoyable

Then add more starting options, rewards and later captured tribes. Preserve the locked 25 tribes and 17 progression-unlocked classes. Their total count is future content scope, not evidence that the opening is already varied.

Before production, propagate an approved decision through campaign creation, squad validation, roster/life accounting, save snapshots, replay versions, reward eligibility, UI previews, tutorials and tests. No numbers, timings or win-rate targets in this report are an authorized tuning pass.

## What Claude should review

1. Do the current reaction rules and leader kit permit a genuine ranged-versus-melee tradeoff with these starting classes? Bring exact traces, including unclaimed marks and a solo chief, not a description of intended synergy.
2. Is post-tutorial Clarth muster acceptable within the narrative and latest owner rules? Keep enemy recruitment at chief kneels; flag any broader acquisition-rule conflict.
3. Which existing reward mechanism, if any, is still acceptable after Jacob's latest rejection? Do not implement a renamed gear or favour system without that decision.
4. Which prior parry decisions are superseded by the latest discussion? The counter timing, repeated Hold stamp and overlapping answers must agree with the displayed cue.

This is my revised recommendation to review with Jacob. It is not an instruction for Claude to start building.

## Source and evidence notes

All web sources above were accessed 26 October 2026. Steam product descriptions are first-party descriptions; displayed review statistics are platform aggregates. Historical Riot/Supergiant articles are explicitly dated examples, not current balance references. No quotes or claims were adopted from the unaffiliated SEO guides returned in search. No universal “humans can process X events per second” limit is used to set this game's pacing.

Local inspection: `AGENTS.md`; `DESIGN_LOCKS.md` class/gear/formation and squad-first sections; `HANDOFF_LOG.md` latest owner diagnosis and Claude's 26 October question; `reports/Combat fix plan.md`; relevant portions of `reports/Fun readable squad combat.md`; `Campaign.cs`; `ClassSpells.cs`; `BattleSimulation.Chains.cs`. Claude's papers are context and proposals, not independent validation of their cited science or proof of current gameplay.

Confidence: **high** that the existing cue contract needs repair and that the compared games expose explicit build choices; **moderate** that starting composition is the cheapest promising fit for this brief; **unproven** that the two proposed compositions will be enjoyable or sufficient. The latter requires the bounded comparison and Jacob's play. No new gameplay, build, native visual, performance or balance result is claimed.
