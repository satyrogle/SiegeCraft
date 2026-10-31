# Four Pals, tribal technology and chain moves

## Revision 4 — owner locks reconciled through 30 October 2026

**Current reference for Jacob and Claude. Butcher is implemented in rules 15/save 23; the other three redesigned Pals still await sim implementation and owner play.** This revision follows the 28 October Pal sections and the three later 29 October sections in [DESIGN_LOCKS.md](../DESIGN_LOCKS.md): **Four Pal fixes from the model**, **The four losing combos rethought** and **The combos' hooks**, in that order. Later decisions win. It replaces revision 3's Folded Volley, Spread/Concentrate, Shade shot reservation, Mobile Aim and parked-bonds direction. Earlier revisions and their research remain collapsed below as historical evidence. Updating a paper does not implement a mechanic or validate its balance.

### The four contributions

The advanced cult improves each tribe's own fighting kit. A Marksman remains a Marksman and a Medic remains a Medic. None of the 17 classes receives another class's spell. Tribal weapons, rituals and equipment keep their identity; the Pal technology changes how that existing action lands.

| Pal | What it supplies | What it does not replace |
|---|---|---|
| **Butcher** | One fighter's class move plus Gravity Well, Fold Door or Time Stop makes one visible combo. | It does not need a second fighter's action to become a combo. |
| **Shade** | Routes for Parry, Fall Back, Focus Fire and Unleash, at most two enhanced buttons. | It does not reserve another unit's shot or own automatic paired behaviour. |
| **Whisperer** | Habits, pairs, born/earned traits and ranks inside a class. | It does not grow a fighter into another class or sell Mobile Aim again. |
| **Glutton** | Universal talents make existing effects bigger, longer, faster or cheaper; Feed doubles one owned talent for a run. | It adds no separate attack/effect or Spread/Concentrate combo branch. |

**The road builds this run.** The existing road places a Pal on one fight per row. Winning it gives a choice of three eligible things instead of that fight's materials, with no bounty increase. Butcher now offers combos for current squad classes, excluding combos already held; Shade is designed to offer button forms, Whisperer a habit/pair/rank, and Glutton an unowned talent loan. Glutton's planned offer includes plain universal talents, not only boosts requiring an installed augment. Picks end with the run. Butcher's three old button buffs have been replaced; the other Pals' old button favours remain placeholders. The proposed different Pal on every fight is held while Jacob resolves its removal of gear drops, stripping and bounty; do not present it as the current road.

### Butcher's fifteen moves and their costs

All three augments are known from the start, so all fifteen class/augment combinations can enter the first run's eligible pool. A combo occupies its fighter's gear slot and carries its pact. A rank-3 fighter can carry two augments: both combos fire and both pacts apply; no third combo is created. Trophy/augment slot competition must be shown explicitly, not silently resolved in the UI.

| Class | Gravity Well — purple | Fold Door — blue | Time Stop — gold |
|---|---|---|---|
| Marksman | **Black Hole Round:** Aimed Shot drags in nearby enemies and crushes them; a Snare-cashing crit also strengthens the crush. | **Around the Corner:** every Aimed Shot portals behind and bypasses Guard; keep Focus then Snare priority before healer fallback. A Medic takes 200 percent, a Snared target 300 percent. | **Stopped Bullets:** each hanging bullet pins for 2 seconds and leaves an Opening. The third releases all three, cannot miss and bypasses Guard. |
| Skirmisher | **Collapse:** every second strike, and any strike ripping a Bleed, pins its victim under a black hole for 2 seconds. | **Execution Corridor:** a strike cashing an Opening triggers the portal follow-up. It no longer triggers on parry. | **Frozen Flurry:** stored hits land at thaw; an Opening then is cashed by the thaw and doubles the kept hit. |
| Warden | **Event Horizon:** every fourth swing pulls and cleaves; the victims hit receive his Bleed marks. | **Wrong Door:** hits on allies next to his Guard recipient arrive on his shield and leave Bleed on their attacker. | **Still Wall:** a shield-hit attacker freezes for 2 seconds and receives his Opening, subject to the effect's lockout. |
| Trapper | **Singularity Net:** the triggering victim and dragged neighbours all receive Snare; neighbours do not receive an extra Bleed. | **Doorway:** shooters can also spring her trap, including its normal Snare. | **Time Snare:** half-speed turns for the hold plus 6 seconds; every big move begun in the slow winds up twice as long. Its Bleed lasts that whole slow. |
| Medic | **Mercy Well:** Treatment heals patient neighbours for half, unused neighbour healing flows back once to the patient; each bleeding enemy adds 20 percent healing, up to three. | **Evac:** saves a falling fighter out of reach and leaves an Opening on the attacker whose blow caused it. | **Rewind:** restores up to 3 seconds of damage and leaves an Opening on the enemy whose blow last hit that ally. |

Latest pacts are the **halved 29 October pacts**: **Heavy** makes the spell's own clock 1.25 times as long; on the Warden and Medic it slows their turns. **Unnerving** means an eighth less nerve per wearer each fight, taken from their share of the side's bar where fighters have no personal nerve. **Borrowed time** means an eighth less health. These supersede 1.5 times/quarter prices. Nearby is the same row and next lane; future Neutron Mass reaches two lanes. A gravity pull selects logical victims and crushes them, with no general movement system. Pins/freezes do not generally create Snare marks, but **Singularity Net deliberately snares dragged neighbours** because its wearer is the Trapper.

The hooks feed the existing engines: Singularity Net/Doorway supply Snare for the shooters; Stopped Bullets/Still Wall/Evac/Rewind supply Openings for Execution Corridor/Frozen Flurry; Event Horizon/Wrong Door/Time Snare supply Bleed for Collapse/Mercy Well. Mercy Well reads bleeding enemies without consuming their Bleeds or damaging them. Chief protection still rejects Snare and Opening while the band stands. Only the Trapper applies Snare; a Warden or Marksman gravity pin does not gain that status.

Rank 2's extra offensive hit is plain: no augment combo and no mark cash. This prevents a rank from duplicating every payoff. The exact triggers, recipient validation, eligible neighbours and pacts must arrive through authoritative sim state/events.

### Shade's buttons

The [reconciled Shade paper](Shade%20command%20specialisations.md) carries the eight routes and their detailed follow-ups. Mirror ends in **Pierce**; Ghost Step ends in **Shatter**. Rift Mine is **Mine → Shock → Wide Blast**: the first enemy attack after Fall Back triggers it, Shock delays the next attack, and the last form hits two additional neighbours. No tether or physical pull remains.

Jacob accepted all nine audit repairs. Relay has a separate finite shared reserve; Rupture borrows nearly ready attacks and repays recovery; follow-ups consume their own stored payload once without re-firing normal hooks; Echo accumulates a share of every eligible hit in one visible echo per fighter; natural Ghost Step return keeps the decoy while only manual Recall bursts it. Before a chief, an enhanced button may be parked as its base form for that fight. A new route unlock grants one safe-screen swap.

Both enhanced buttons share second-form tier 3 and final-form tier 5 gates. The 8 Dec slice opens seconds at campaign tier 2. Command Relay now halves one enhanced button's weakness; its one-tier-early benefit is withdrawn. The treatment of categorical weaknesses and safe replacement at the two-button cap must be explicit in the sim contract.

### Whisperer's fighters and teamwork

| Class | First habit option | Second habit option | Rank 2 |
|---|---|---|---|
| Warden | **Bodyguard:** Guard the Marksman while he aims. | **Rescuer:** Guard the least-healthy ally. | Guard two allies. |
| Skirmisher | **Breaker:** Strike a big-move windup. | **Avenger:** Strike whoever last hit an ally. | Strike twice; extra hit plain. |
| Marksman | **Counter Shot:** Shoot whoever last hit the back row. | **Spotter:** Shoot the big-move windup; target choice only. | Two shots per aim; extra hit plain. |
| Trapper | **Ambusher:** Wait for an un-parried heavy, springing before its blow lands. | **Gatekeeper:** Back-row-attacker habit retained for later content, excluded from slice road picks. | The one trap springs twice before rearming, not two simultaneous traps. |
| Medic | **Triage:** Wait for an ally below a quarter health. | **Front First:** Treat the front row first. | Keep one Treatment in hand to treat twice in succession. |

Which habit per class enters run 1 is still open; this table's column order is not that choice. Rank 3 opens the second slot. A fighter who falls in a lost fight loses the top rank, consistent with lives/no capture. The other twelve classes get content later under the same class boundary.

The three offensive pairs **Pin and Shoot** (Trapper/Marksman), **Open and Cut** (Marksman/Skirmisher) and **Snare and Rip** (Trapper/Skirmisher) bond their classes immediately from the road pick for the rest of the run. Natural bonds still form after three cashes as before. The latest pair answer is **free**: the second fighter answers without spending its own spell or changing its normal spell clock, at most once per cycle of that clock and at least 1.5 seconds apart. This replaces the earlier spent-action/borrowed-recovery catch for pairs. It does not change Rupture's separate borrowed-attack repayment rule.

**Mend and Hold** Guards both the treated fighter and the Medic, preserving the Medic's increased healing under Guard. The old claim that the Medic loses Guard is withdrawn. A pair still costs the Whisperer road pick instead of a habit or rank from that fight. These are designed rules, not yet the redesigned Whisperer's implementation. They create no new Shade button or relationship meter; bonds are no longer parked.

The **one answer/pull per spell cycle** limit and no recursive pull still apply; a free pair answer must not be described as debt or as spending the next scheduled spell. Target priority is Hook drag, Focus Fire, combo target, habit, pair, then plain, with Around the Corner's latest Focus/Snare preference respected. Habits and Mend and Hold replace only the Guard-the-Medic step in Guard targeting. Claude's latest provisional reading is one habit per class and one squad pair, with a new pick replacing the old; that interpretation is not promoted here to a separate owner lock. Ambusher's heavy is provisionally the telegraphed parryable big hit; the discarded rule that a successfully parried heavy also springs the trap is not retained.

Born quirks remain one rolled quirk per fighter; a reroll costs 25 run materials and can be worse. A kept road scar costs 75 at camp and applies to the whole class, one kept scar per class. This is retained permanent learning, distinct from run ranks/habits/pairs.

### Glutton's permanent tree and run loans

The locked tree has seven limbs: Front line, Strikers, Shooters, Control, Healers, Command and War chest. Each has a tier-1 node, tier-2 node, one of two tier-3 nodes and a keystone. Costs are 1, 2, 3 and 5 codex points: 11 for a complete chosen path, 77 for seven. The codex must grant fewer than 77, with its total still open. Points come from first records, including tribes met, beast parts broken and chief moves seen, never repeat farming. Tree tiers open with campaign tiers; keystones open at tier 4. In the slice, tier-3 talents open at campaign tier 2 and keystones at tier 3.

The locks contain all 35 talents. Apply their latest corrections when presenting them: Gravity Mesh strengthens Singularity Net crush; Probability Lock makes Aimed Shot on a Snared target aim at once; Synth Armour doubles healing on a Guarding Warden; Slow Light Rounds adds one held bullet to Stopped Bullets; Answer the Parry doubles Mirror's return; Time Lock doubles the Time Snare slow's duration; Matter Printer adds a third to Mercy Well healing; Strip doubles Opening duration; Glancing Fold is **Glancing Blow**. Stasis Wind Up expands Singularity Net from two neighbours to four. These replace the earlier duplicated effects.

Feed doubles one owned talent for the run; the prior same-limb sacrifice clause is not explicitly repeated or withdrawn by that later wording and needs a handback before the UI promises a price. Reset is only on camp arrival before purchases. A road loan is an unowned talent improving something currently held, not a second permanent point purchase. **Node Cache and Synthesis Discount need new wording** because augments are no longer owned/purchased. Do not retain their old augment-copy/shop text as active tooltips.

### What is kept, what is picked

| Permanent camp/codex fact | Current-run content |
|---|---|
| Forge: tabard, trophies and chief gear forged once. Tabard remains 50 materials. | Butcher combo/augment installations, chosen from eligible current squad content. No augment purchase. |
| One kept scar per class, 75. Glutton tree paid by first codex entries. | Shade's held routes/forms, at most two buttons. No first-form purchase. |
| Later Shade forms unlock the road pool for 150/250; class second habit 75; each pair 75. | Whisperer habit, pair and rank pickups. No rank purchase. |
| Recipes and unlocked offer pools survive the run. Buying access does not equip the effect. | Glutton talent loans and Feed; run selections clear at the end. |

The form-unlock cost granularity and how the earlier free discovery swap maps onto road replacements need explicit contracts. Do not revive universal permanent Shade capacities. Camp makes future variety possible; it does not guarantee a particular offer or complete build next run.

### One fight using the latest example

**Authored illustration, not a played fight or measured timing.** Earlier in this run, the Marksman received **Black Hole Round** from a Butcher road reward. A later Whisperer choice supplied **Pin and Shoot**. Glutton lent **Light Stake** because the shot's black hole was already held. Shade then offered **Rupture**, occupying one enhanced button; other buttons stay ordinary. This order is an example, not a guarantee that the road deals those picks.

1. At zero bars the squad already fights. Trapper's snare can set up the bonded Marksman's real Aimed Shot. Black Hole Round crushes its valid nearby victims; Light Stake doubles the crush. The Heavy pact lengthens the spell clock. No parry is required to enable the combo.
2. The enemy gives three readable beats. Correct, partial, correct leaves two bars and a wound for the Medic. The correct beats defend and charge; the partial does not charge. Held bars add +20 percent to Butcher combos. Shade does not add another meter.
3. Focus an enemy. Ordinary eligible attacks build Rupture preparation. Pin and Shoot's planned answer is free once per spell cycle, separated by at least 1.5 seconds; the Marksman's own scheduled spell keeps running. If Focus conflicts with a lower-priority habit, Focus wins.
4. After sufficient preparation, an accepted second Focus spends that Rupture payload once. Nearly ready eligible attacks may come early and repay recovery. Pair and finisher responses must obey the shared cycle/non-recursion limits without deleting the pair's normal scheduled spell. Support fighters keep their class jobs.
5. Choose ordinary Fall Back for a later covered attack. It avoids the eligible damage and gives no bar; lost attacking time is the trade. There is no Mobile Aim promise, shot reservation or automatic mine because that route was not selected.
6. A later correct parry fills bar three. If a chief ultimate was legitimately acquired, the player can retain the +30 percent combo boost or spend all three on that ultimate. It does not cast itself, and spending does not remove the installed combo, pair or talent loan. Exact per-battle charge initialisation and interrupted-cast handling still need the accepted sim contract.

This demonstrates four different jobs in one fight without four actions before every shot: Butcher changes the shot, Whisperer sets up teamwork, Glutton magnifies the held effect, and Shade changes a command's decision. Actual enemy timing, combo eligibility, survival and player clarity need implementation and owner play.

### Chief, engineering boundary and next evidence

Execute is removed. A chief kill happens only if he falls before kneeling, and gives an immediate choice of three from any Pal in place of his gear. Spare gives tribe, order and recipe. The chief choice supplies different power/allies for this run; permanent recipe/forge knowledge remains distinct. A kneeling chief is not a qualifying ordinary kill for cascades.

The 29 October handbacks implement **all fifteen Butcher combos**, their four rethinks and latest hooks in rules 15/save 23. `FavourRules.IsCombo`/`ComboBend`, `Campaign.PalPickOffered`, seeded eligible offers, `CanTakeFavour`/`TryTakeFavour`, automatic first-eligible equip and `TryEquip`/`TryUnequip` exist. Held combos live in `Campaign.Gear`, not ordinary stacked `Campaign.Favours`; if no eligible fighter has a free combo slot, the piece waits unworn. The redesigned Shade, Whisperer and Glutton still use their old favours. Their real routes/habits/ranks/tree/loans/Feed, three-bar ultimate, permanent pool unlocks and chief big pick remain backend dependent. Existing chains, earned bonds, lives, streak and leader protection remain foundations, not proof that all four new Pals are built.

Evidence must stay dated. The 29 October throwaway-model loser list is superseded for implemented Butcher by real-sim measurements, then rethinks, then hooks. Claude reports 482 scratch-harness EditMode tests passing, 7 skipped; historical rules-14 comparisons match his reported 114 fights/12 runs/32 mid-fight saves. Those are his reports, not a fresh Unity or Codex packaged verdict. Reusing rules 15/save 23 across unshipped revisions means early scratch snapshots may not match final hooks; use fresh isolated review data and preserve older owner saves.

Later real-sim sweeps did not establish broad multiplicative run synergy. The 30 October wearer-duel run shows duelist choice dominates the sampled results: plain Trapper won 100 percent of sampled duels with either pilot, Skirmisher 95/100 percent, while the default Warden won 3/22 percent. Sending Trapper/Skirmisher lifted sampled plain run wins substantially, shrinking many apparent combo advantages. Event Horizon notably helps the weak Warden duelist. These are pilot/seed/settings results reported by Claude; pilots never parried road heavies on beat, and harder settings were experimental. The possible fixed-answer Trapper duel is an open design/balance risk for Claude and Jacob, not authority for a UI or tuning repair. Nothing here claims owner play or a fresh sweep.

Claude owns effects, offer eligibility/seeds, recipients/replacements, rule-version/snapshot/save contracts. Codex consumes them for reward/assignment screens, button state, visible effect receipts, save calls, builds and independent QA. No UI may reconstruct combat rules or invent unavailable rewards. Historical replays keep their original behaviour.

Explicit open owner-design items: run-1 habit per class; Node Cache/Synthesis Discount revisions; Ambusher heavy-blow definition; total codex points below 77. Integration questions to settle before dependent screens include two-button replacement/held-form retention, unlock purchase granularity, Command Relay on categorical weaknesses and Feed's retained sacrifice rule. The 25 October truthful-parry/accepted-input issue remains separate; new Pal designs do not prove that old cue reliable.

Prototype gate: one real reward through choice, legal recipient/slot decision, visible effect in combat, save/resume, deterministic replay and run clearing. Then audit interactions against the latest locks, re-audit repairs and let Jacob play. No balance pass, clean gameplay verdict, artwork approval or background work is implied by this document reconciliation.

**30 October status addendum — duels:** Jacob approved the new **Duels** lock in [DESIGN_LOCKS.md](../DESIGN_LOCKS.md): parry-driven nerve duels, champion yield, one Trapper catch and an explicit player duelist choice. Claude handed back the rules-16 backend while this sprint's final build was being packaged. Live source now declares rules 16; Claude reports 488 scratch tests passed, but no Unity validation or play. The bounded Butcher delivery retains the independently tested rules-15 simulation in an isolated build snapshot and a separate save. The no-default-duelist screen and independent rules-16 validation are the next dependent integration job. Rules 16 also writes evidence format 22, which older binaries refuse. Its measured half-parry win rates still exceed the locked target, so the design is not a balance pass. Earlier sweep results above remain dated pre-redesign evidence.

<details>
<summary>Revision 3 — historical 27 October proposal; superseded by the owner locks above</summary>

## Revision 3 — four distinct Pal jobs and three-bar parry, 27 October 2026

**Current paper for Jacob and Claude. Plan only; no implementation or gameplay validation.** This revision replaces the active recommendation in revisions 1 and 2. Their text remains collapsed below as history. The latest owner calls are in [HANDOFF_LOG.md](../HANDOFF_LOG.md), in Claude's 27 October handover and the following Codex acknowledgments. Where they differ from the older role descriptions in [DESIGN_LOCKS.md](../DESIGN_LOCKS.md), Jacob's newer calls govern this proposal; the locks file has not been edited.

**The fight:** you time the defence and command the war party. Fighters perform their own class actions. Native tribal equipment enhanced by cult technology creates chain moves. Successful parries strengthen those chains and fill three segments for an inherited chief ultimate. Bonds are parked.

### 1. What is settled and what this paper is proposing

**Owner direction:** Butcher augments weapons and class kit, Glutton owns the talent tree, Shade enhances command buttons, Whisperer upgrades units. The cult is a disciplined, darkly humorous type-4 civilisation; its enhancements must be more imaginative than generic elemental coatings. Tribal identity remains the foundation. Each correct parry earns one of three segments; three enable an inherited chief ultimate. Parrying also improves chains. Partial parries reduce incoming damage, full parries negate it, and enemies can have one-, two- or three-beat attacks. Wait-out/fake-first timing tricks go. Kneeling chiefs are spared; Execute is removed in the newer design.

**Proposed here, not approved details:** the specific fold augment, talent branches, Focus enhancement and unit upgrade below; retained segments determining chain strength; the ultimate spending all three and removing that extra strength; keeping Fall Back as the safer dodge; exact cancellation and charge rules. No timing, damage bonus, progression price or implementation version is approved by this paper.

**Parked:** bond acquisition/levels/paired perks, the additional enemy full-break bar, and a separate tribe-set activation system. No bond prerequisite or bond event is used in this example. Parking these proposals does not remove historical rules from existing saved fights. Revision 2's bond gate is no longer an active next step.

The four roles do not mean four talent trees. Recommend one Glutton-managed talent system with relevant class/tribe branches; its exact organisation and persistence remain open. Butcher's augments, Shade's command enhancements and Whisperer's unit upgrades have their own jobs without each needing another tree or currency.

### 2. Four Pals building one fighting style

The following is a **candidate Clarth build**, not a new approved technology catalogue. It uses their tooth net and spine rifle. The working name for its chain is **Folded Volley**.

| Pal and latest role | Candidate contribution | What changes during combat | What you give up |
|---|---|---|---|
| **Butcher — weapon/kit augments** | Fits spatial-fold anchors to the tooth net and a compatible path-control assembly to the spine rifle. The real rifle shot interacts with the landed net to release its teeth along folded trajectories. | Two actual class attacks create one major attack whose materials still come from Clarth equipment. | Releasing the net ends its restraint. Real shot readiness and net deployment are consumed; there are no infinite copied projectiles. |
| **Glutton — talent tree** | A mutually exclusive branch chooses **Concentrate** or **Spread** for this compatible interaction. This example selects Spread. | Concentrate directs the volley into one victim; Spread distributes its attack across the trapped victim and one nearby enemy. | Spread sacrifices concentrated damage. Concentrate sacrifices coverage. Neither selection retains the released snare. |
| **Shade — button enhancements** | An enhanced **Focus Fire** can hold the Marksman's next eligible aimed shot briefly for the Trapper's next eligible net on the chosen enemy. | One familiar command coordinates the order of two real actions. The chain releases automatically when both are valid. | Waiting delays that shot. Focus expiry, a new target, retreat or invalid participants cancels the pending coordination. |
| **Whisperer — unit upgrades** | The Marksman receives **Mobile Aim**: a short Fall Back preserves his existing aim progress so he resumes it after returning. | The squad can escape a threat without making this fighter start his shot preparation from zero. | Choose it instead of the competing stationary-aim upgrade. It grants no firing during retreat, extra shot or free cooldown reset. |

The labels are working names. Butcher and Glutton produce the distinctive attack interaction. Shade makes deliberate execution easier; Whisperer makes the fighter more capable when the situation changes. **The chain must not require four completed Pal progression tracks before it works.** A compatible, ready pair can perform its basic chain automatically without Shade's enhancement or Whisperer's upgrade; the selected enhancements give the commander more control and flexibility.

This keeps each Pal's decision separate: **what the equipment does; which talent changes its behaviour; how a button directs it; what the individual fighter can sustain or recover from.** Whisperer does not become a bond vendor or return to cosmetic-only regalia. Glutton no longer owns the old containment-armour role from revision 2.

Butcher's mandate still covers the whole class kit, not only attacks. A future application of the same proposed fold technology could redirect a Warden's existing protection toward a valid ally or deliver a Medic's real Treatment across a constrained path. Those are research candidates, not extra mechanics required for this fight. Medic remains a healer; only Trapper performs Snare. The four technology families Claude is investigating remain unapproved; this paper does not select all four or claim that dimension folding is real-world engineering.

### 3. One meter, three visible segments

For this walkthrough, let **P** be the party's filled segments, from 0 to 3. Recommend starting at 0 each battle; cross-battle storage is not settled. These are proposed state rules for review:

| Event | Damage result | Segment result |
|---|---|---|
| Correct parry of one real beat | Negates that beat's incoming attack damage | P becomes min(3, P + 1) |
| Partial parry | Reduced damage; reduction not yet chosen | P unchanged |
| Miss | Normal full attack damage, without an additional failure multiplier | P unchanged |
| Successful dodge/Fall Back | Avoids the covered attack; loses some attacking time | P unchanged |
| Squad attack or chain | Its normal effect, with the current chain bonus where applicable | Does not generate or consume P |
| Valid inherited ultimate release | Executes that acquired chief move | Requires P = 3; spends all three |

**Proposed chain scaling:** a fixed authored chain payoff is multiplied by a bonus selected from P: B(0) = 1, with 1 < B(1) < B(2) < B(3). Exact bonuses are intentionally unset. The added strength never changes which classes are eligible, expands target count, prolongs control or creates another attack. Support chains, when designed, need their own bounded payoff rather than turning healing into damage. Sample P at actual chain release; fragments share that same result. Do not multiply each nested hit again.

Normal chains work at zero. At three you can retain the stronger chains or spend the charge on the chief move. Casting resets only that temporary boost; all installed augments, talents and unit capabilities remain. This connection is Codex's recommendation for expressing Jacob's two parry rewards through one readable meter. Its main risk is making the ultimate feel like a penalty because it removes the boost; it needs an explicit comparison in owner play.

The three segments replace the inherited ultimate's old command-point payment in this proposal. They are not a second hidden fee. Other orders retain their separate existing resource checks pending their own design review. Hold/Fall Back costs and availability during a multi-beat sequence must be reconciled explicitly; the example assumes all depicted defensive inputs are affordable and accepted. A real implementation must never signal a valid parry while an old anti-mash rule or cost makes that press impossible.

No inherited move is granted simply because P reaches three. The showcase below takes place after legitimately obtaining Eclipse. The existing Clarth-starter-spell decision must be resolved separately; do not claim the early fifth button is necessarily empty or invent its move here. Spare-only chief rewards must replace old Execute-dependent recipe assumptions before implementation.

### 4. One fight from start to finish

**Authored scenario, not a replay:** a later road encounter after acquiring Bloodridge's Eclipse. You kept your five Clarth fighters: Warden, Trapper, Marksman, Skirmisher and Medic, plus the leader. This demonstrates the core without requiring mixed-tribe recruitment or bonds. The opposing patrol has a tough melee attacker, a second frontline fighter and a rifleman. Their attacks, survival and recovery below are illustrative, not measured balance.

**A. The squad works at zero bars: [empty / empty / empty].**

Your Warden protects the front, your Skirmisher attacks, and the Medic treats eligible wounds. A Trapper net and a ready Marksman shot can already make the selected basic Folded Volley. Nothing waits for you to earn a friendship level or land a parry. Let a basic volley resolve early so the zero-bar baseline is visible; the net and shot then recover normally.

**B. The tough enemy attacks three times: cut, cut, delayed slam.**

You correctly parry the first cut: **[filled / empty / empty]**. The second press is near the beat: a partial block deals reduced damage, and the meter stays at one. You correctly parry the delayed slam: **[filled / filled / empty]**. The final deflection creates the recovery opening. The Medic now has a real wound to treat.

Early deflections do not cancel the remaining sequence. Each beat has its own readable timing cue. A true three-for-three would earn three segments; there is no separate full-sequence meter. Normal squad actions continue, but another urgent enemy deadline must not obscure these beats.

**C. You command the chain during the recovery.**

The Trapper and Marksman's required actions have recovered by this authored moment. You select the second frontline enemy with **Focus Fire**. Shade's enhancement holds the eligible shot briefly for that target's next net. When the net lands, the shot follows.

Butcher's fold assembly sends the shot through the net's anchors and releases its tooth payload. Glutton's **Spread** branch directs the combined attack into the trapped enemy and one nearby valid enemy. The first target loses the snare. The real net and real shot are now spent. At **two bars**, the volley has the proposed two-bar strength. Charge remains at two.

You see net, shot, folded tooth paths and impacts on the actual victims. This is one chain move. No four-Pal pop-ups or separate tribal activation accompanies it.

**D. On a later exchange, you choose the safer defence.**

Your Marksman has started preparing another shot when the tough enemy telegraphs a new single heavy. You choose **Fall Back** instead of attempting the timing. The squad avoids this covered swing, briefly stops attacking and earns no segment: still **[filled / filled / empty]**.

Any pending coordinated Focus sequence is cancelled by retreat; old net state cannot fire a ghost chain. Whisperer's **Mobile Aim** preserves only the Marksman's already-earned preparation during the short retreat. On returning he still needs a live target, actual shot availability and, for another chain, a fresh valid net. You can issue a new order. He does not attack while retreating or retain a cancelled target commitment.

This is Whisperer's contribution occurring inside the fight, rather than an invisible percentage increase. Without that upgrade, the shot preparation would restart under this proposed design.

**E. You take another parry opportunity: [filled / filled / filled].**

The enemy later winds up another clear attack. You parry successfully and fill the third segment. The ultimate button becomes ready. It does not cast itself. Further parries still defend you but cannot overflow the meter.

At this point you could keep using stronger chains. In this example, the surviving group is vulnerable enough that you choose the larger immediate attack instead. You press the acquired **Eclipse**. On a valid committed release it spends all three segments and performs its authored chief-spell effect; this proposal changes its resource source, not its damage, reach or enemy-response rules. It does not automatically borrow Glutton's Spread branch or multiply itself as a chain.

**F. The squad finishes the surviving enemies at zero bars.**

The extra chain strength is gone. Butcher's fold equipment, Glutton's selected branch, Shade's Focus enhancement and Whisperer's unit upgrade still work. The Warden protects, Medic heals, and available offensive units finish the survivors. The fight does not become nonfunctional because the ultimate was spent.

The command story is short: **parry a rhythm, use the opening to coordinate a chain, dodge when you prefer safety, then spend the earned ultimate when it is useful.** The exact timings, hit counts and outcome must be proven in a playable prototype later.

### 5. How the same fighters can play differently

Change Glutton's branch to **Concentrate**, and the folded volley attacks one target instead of spreading across two. Change Whisperer's unit choice to stationary aim, and you gain that alternative's eventual defined benefit but lose preserved preparation on retreat. Shade's enhancement is a chosen way to improve Focus, not permission to turn every existing button into a mandatory multi-step input.

These are alternatives in one connected build. No inventory of new gear slots, fourth bar, relationship level or new tribal synergy meter is required. Later mixed-tribe examples should start from their own native tools and compatible enhancements; do not simply paste the Clarth tooth-net recipe onto water machinery or Bloodridge rituals. The suggested five-tribe access milestone remains a later variety test, not a restriction requiring five tribes in one party.

### 6. Rules that keep this honest

- **Actual participants:** Trapper supplies Snare; Marksman supplies the real shot. Death, interruption, invalid range, immunity or a dead target can prevent the chain. A net that already released cannot pay again.
- **Automatic action ownership:** normal class teamwork stays live. An explicit enhanced Focus can reserve one eligible upcoming shot for a bounded period; it does not confiscate a shot already launched or duplicate it. Reservation expiry releases unused readiness. A fired action pays once even if the target then dies.
- **Targets:** Spread has at most two valid enemies within an authored range; the second is previewed and chosen deterministically. If absent at release, that part does not invent a target or silently become a full-strength concentrated strike. Normal defences/immunities still apply; a fold is not universal armour bypass.
- **No duplicate payouts:** one accepted beat grants at most one segment, even if the enemy swing targets the whole squad. One parry cannot cancel an entire multi-beat string. Repeated presses cannot charge from an already-resolved attack.
- **Safe ultimate commitment:** reject invalid casts without spending; reserve while winding up, spend on committed release, return the reservation on pre-release cancellation. No charge gain above three while reserved, no post-release refund and no simultaneous second cast. Chain scaling during reservation remains the held value until the ultimate actually spends it.
- **Control and readability:** real accepted timing determines the cue. Partial blocks look/sound different from full parries. One-to-three-beat enemy sequences are followed by room to issue an order. This is a pacing hypothesis, not a proven seconds-per-threat number.
- **Existing boundaries:** a chief becoming eligible to spare cancels queued lethal follow-through. New proposals must not rewrite archived fight semantics. Ordinary enemy deaths and chief kneeling are different outcomes.

### 7. Research, review and next design gates

Primary sources checked 27 October 2026:

- [Sandfall's creative director on Expedition 33 combat](https://blog.playstation.com/2024/08/28/new-clair-obscur-expedition-33-gameplay-fighting-and-exploring-the-flying-waters-region/) describes wider dodge timing, more precise parries and counters following parried sequences. This supports readable rhythm and a safer defensive alternative. The partial-block outcome, three-segment resource and retained-charge chain boost here are KS proposals, not claims about that game's rules or success.
- [Last Epoch's official skill-specialisation documentation](https://support.lastepoch.com/hc/en-us/articles/46363203944859-Skill-Specialization) describes investing in a known skill's tree and changing specialisation. This supports making Glutton's choices alter concrete actions. It does not validate our particular branches, costs or progression speed.
- Scarlet Nexus research is retained in the historical revision for future bond discussions. No SAS borrowing, bond tier or relationship unlock is introduced here.

**Claude's current review target is revision 3, not R5/R8 or section 13 of the archived drafts.** Check: (1) whether these four responsibilities match Jacob's newest calls; (2) whether the fold interaction respects Clarth equipment and class roles; (3) whether Mobile Aim duplicates an existing class bend and should instead be that bend's unit-upgrade home; (4) whether coordinated Focus has meaningful costs without starving ordinary automatic play; (5) the risk of retaining three bars forever; (6) parry-versus-chief-move exceptions. In particular, older Eclipse/Cleave proposals allowed residual damage on a successful parry, whereas the newer generic parry direction says full mitigation. This road example uses ordinary physical beats and does not silently decide that boss conflict.

Planning gates: agree this one fight and its four contributions; settle the charge/ultimate tradeoff and honest graded timing; specify one Clarth chain plus one alternative branch; then define authoritative rules, interface events, save/replay versioning and owner play evidence. **No bond gate is active.** Recipe acquisition after removing Execute, starter chief-spell selection, technology-family selection, talent persistence and unlock prices remain open.

If later approved, affected areas include Sim attacks/parry/orders/chain resolution, campaign progression and chief rewards, input/HUD/animation events, tutorials, save/replay schemas and tests. The UI must consume those authoritative states, not recreate the rules. This revision contains no executable changes, tests, balance sweep or claim of a working build. Document checking can establish consistency, not fun.

</details>

<details>
<summary>Revision 2 — historical proposal; its old Pal roles and parked-bonds direction are superseded</summary>

## Revision 2 — the tribe is the foundation, 26 October 2026

**Plan only. Latest design proposal for Jacob and Claude.** Jacob likes the connected Pal/talent/command direction but corrected its foundation: our advanced race meets the planet's different species, fighting traditions, machinery, rituals and ways of life. We enhance those existing capabilities. The interesting combinations emerge from **tribal kit meeting cult technology**, and from different classes learning to cooperate. He requested the working term **chain moves** and Scarlet Nexus as a bond reference.

This correction was already foreshadowed in the 30 September premise in [DESIGN_LOCKS.md](../DESIGN_LOCKS.md), lines 24–30. It should have governed revision 1. Its generic pressure-pulse system made the tribes incidental to the new mechanics. **That universal attack system, its specific recipes and its prototype scope are superseded.** Revision 1 is retained below as a collapsed source/review record. Its decision history and research remain useful; its proposed numbers, trees and combat content are not the current recommendation.

Nothing in this revision changes code, art, backgrounds, saves or the design locks. Examples below are new proposals, not already-approved tribe abilities. The owner's correction establishes the direction; it does not approve every example written in response.

### R1. The design relationship

**Tribe supplies the fighting tradition and native equipment. Class supplies the combat role. Pals supply the enhancement. Talents choose how it develops. Bonds let actual fighters cooperate in more ways. You command the resulting chain move during combat.**

These are different jobs, not six currencies or six preparation screens. On a fighter's page the player should see the native move, the installed enhancement, its selected talent branch and the allies it can chain with.

The advanced cult does not replace every weapon with one universal gun. Thundercoast already has water machinery and altered underwater biology; Bloodridge has ritual identity, heat and Eclipse; Clarth uses its shark-hunting equipment; Skullpeak has integrated mechanical limbs. Their solutions retain their identity when upgraded. We also do not explain away an established ritual as a fake effect or invent its true scientific origin without Jacob deciding that lore.

The existing class boundaries stay: Warden protects, Marksman shoots, Trapper snares, Medic treats. A water-related Warden and a ritual Warden can protect differently while remaining Wardens. Sharing a tribe does not grant every member its chief's complete spell. Sharing a bond does not teach a Medic to shoot or a Marksman to cast Snare.

### R2. What Scarlet Nexus actually contributes

Research checked 26 October 2026, using publisher-authored material. No new reference-game playtest.

- **Limited shared abilities and meaningful cooperation.** Bandai Namco's [launch gameplay guide](https://blog.playstation.com/2021/06/25/reach-your-full-psionic-potential-with-these-scarlet-nexus-gameplay-tips-out-now/) describes temporary SAS access to allies' powers, enhanced SAS effects as bonds grow, and increased support behaviour such as healing/revival. It also describes connecting weapon attacks with psychokinetic follow-ups. **Our inference:** a bond should change an available action or the way two actions connect, rather than only their damage. The exact KS actions below are ours.
- **Relationships express different strengths.** Scenario writer Takumi Miyajima's [official interview](https://www.bandainamcoent.com/news/scarlet-nexus-story-demo-release-special-interview-takumi-miyajima) connects the bond system to characters cooperating while retaining their individuality. **Our inference:** complementary classes should remain distinct when linked. Do not replace that relationship with a universal “everyone can borrow every spell” system.
- **Further bonds can develop ability behaviour.** The publisher's [Bond Enhancement Pack 2 announcement](https://en.bandainamcoent.eu/scarlet-nexus/news/bond-enhancement-pack-2-dlc-and-free-update-105-are-available-scarlet-nexus) describes Nexus Drive and new SAS effects, including changes to range and gauge consumption/recovery. This is DLC-specific evidence. **Our inference:** a deeper bond can add a new delivery mode or tactical option. Do not attribute those DLC features to every base-game bond.
- The official [Xbox launch tips](https://news.xbox.com/en-us/2021/06/25/8-tips-scarlet-nexus/) separately describe timed SAS use, weapon/psychokinesis interplay and stronger team support. This corroborates the relationship between cooperation and combat, not a particular KS progression speed.

We are borrowing the principle of relationships unlocking gameplay, not Scarlet Nexus's character-control model, gift loop, exact bond thresholds, supernatural powers or random support triggers. Its narrative companions are not equivalent to our procedural recruits; the adaptation needs the bounded rules in R5.

### R3. Four Pals develop tribal kit, not a parallel generic arsenal

The camp roles below are recovered decisions. The products and costs in the right columns are proposals.

| Pal | Their work on native kit | What the player changes | Meaningful cost |
|---|---|---|---|
| **Butcher: weapons and gear** | Fits cult mechanisms to a shark-spine rifle, a ritual weapon or a water-pressure tool | How the weapon delivers its own attack: a held penetrating shot, a divided volley, a net burst, an appropriate contained release | Choosing one delivery sacrifices another; the class piece still occupies the existing gear slot |
| **Glutton: armour and body augments** | Makes the fighter able to withstand or sustain that enhanced equipment: pressure seals, heat management, bracing, appropriate biological interfaces | Store an impact for a later release or use it for protection; sustain a native effect or discharge it early | Stored protection cannot also be spent offensively; carrying a limited reserve cannot supply an unlimited flood or heat source |
| **Shade: orders and techniques** | Teaches commands that coordinate the actual class moves and their enhanced versions | Direct a chain now, prepare a specific follow-through, cancel it to answer a threat, or choose a different route through a ready combination | Command points, unit readiness and the opportunity lost by committing those particular actions |
| **Whisperer: skins and regalia** | Adapts the fighters' own tribal regalia into readable signals and compatible connections between partners | Keep a committed partner/target while other orders change; coordinate a protected attack or a linked pair's release | A chosen connection can occupy a scarce coordination opportunity; separation or a disabled partner prevents its special delivery |

Functional Whisperer regalia remains a proposed extension of the locked regalia role. It is grounded in the fighters' own regalia rather than a universal cosmetic buff. Appearance choice must remain separate from combat power. Glutton's augmentation here means compatible equipment/body upgrades, not reopening the ban on item-driven extra limbs or mutations.

**Tree organisation:** retain the four Pal workshop trees as the recommendation, but make their nodes act on native class equipment. The fighter view filters those same nodes by class and tribe; it is not a second set of permanent points. Do not claim Jacob has approved the precise four-tree layout yet.

**Gear and talents do separate work:** gear gives a concrete tool or altered move; its applicable talent branch changes how you use it. A learned technique cannot conjure missing equipment. A compatible basic chain should work before the pair has a special bond. Permanent research expands the options; an active selection and the current run's equipment determine what can actually happen now.

### R4. Specific native kits and talent forks

The first column draws on established tribe identity. Every detailed enhancement/fork below is newly proposed, and is restricted to authored class/tribe combinations rather than automatically applied to all seventeen classes.

| Native foundation | Proposed cult enhancement | Proposed branch choice | What the choice changes in battle |
|---|---|---|---|
| **Clarth Trapper's tooth net and Marksman's spine rifle** | Butcher adds a controllable tension latch and a matching spine-round attachment; Shade coordinates their release | **Keep the Catch:** maintain the restraint for a precise follow-up. **Scatter the Teeth:** the shot releases the tensioned fittings as a short fragment burst | Holding one dangerous enemy versus releasing that enemy to damage a nearby group. No other class gains Snare. |
| **Thundercoast's water machinery, adapted to a Trapper's restraint tool** | A proposed water-fed snare fitting, with Glutton's sealed reserve and Butcher's controlled outlet | **Closed Circuit:** reuse a bounded water reserve for repeated restraint. **Breakwater:** spend that reserve in one larger pressure release | Reliability through a long dry-land fight versus an immediate discharge. This does not create the tribe's full arena flood with one fighter. |
| **Bloodridge's ritual weapons and heat imagery, adapted to its Marksman's rifle** | A proposed ritual-compatible heat chamber on the existing rifle; cult control makes its discharge precise | **Votive Shot:** one concentrated heated round. **Ash Fan:** a divided short spread after a longer recovery | Choose a single-target finisher or wider coverage. This is a proposed class weapon variant, not Eclipse granted to every shooter. |
| **Skullpeak's integrated rifle arm** | Butcher and Glutton fit a bracing/recoil interface to the existing mechanism | **Anchored Aim:** commit to a protected charged shot. **Running Mechanism:** keep a lighter shot available while the line changes | A high-commitment protected attack versus flexibility. Neither choice grants another class's signature. |

These forks are candidates to replace the generic Driver/Shatter branches, not extra branches to stack on top. Existing Deadeye/Quickdraw and other class bends must be reconciled explicitly: a new fork cannot quietly duplicate their benefit and multiply it again. If a new design replaces a bend, its old item/reward and historical replay semantics must be accounted for.

Elements and materials need provenance. Water comes from a defined native source/reserve; heat from the proposed equipped Bloodridge assembly or a separately earned spell; shark fragments from the Clarth tool. A tribe's colour palette is not enough to assign an element. Do not make Skullpeak ice-based because an earlier unapproved paper suggested Chill.

### R5. Bonds change cooperation between classes

Keep two ideas separate:

1. **Recipe knowledge:** the cult has discovered how compatible tools interact. This can persist in research/codex.
2. **A fighting bond:** these two particular recruits have learned to execute together during this run. Class roles determine the available cooperation; native kits and installed gear determine its result. The record belongs to the two fighters, not automatically to every Warden and Marksman everywhere.

Current source in [BattleSimulation.Chains.cs](../Assets/Scripts/Sim/Battle/BattleSimulation.Chains.cs) already forms a run bond after three eligible combo cashes by a pair across enemies, and improves response timing with eligibility and a minimum gap. Retain that as the initial acquisition baseline. The following further modes extend it; their thresholds are not decided.

| Relationship stage | Proposed gameplay change | Example |
|---|---|---|
| **Compatible, unbonded** | You can order the basic chain when both real actions are available | Clarth net plus fitted spine shot; no friendship grind required |
| **Practised pair** | Unlock a chosen coordinated execution mode, including the existing immediate bonded response where applicable | Prepare a Catch-and-Release sequence so the pair resolves its two real actions together when the net lands; you remain free to use their ordinary skills |
| **Developed pair** | Choose an alternative cooperation, rather than simply raise all its numbers | Warden–Marksman: **Sheltered Volley** keeps Guard on the shooter during a committed aim, temporarily giving up cover elsewhere. Trapper–Marksman: choose a precision restraint chain or a destructive net-release chain according to equipment |

Sheltered Volley is a support chain; the Warden still performs Guard and the Marksman performs Aimed Shot. It does not turn the Warden into a shooter. It must be checked against existing Guard/aim behaviour so it supplies a genuine command/commitment choice rather than reselling something the pair already does reliably. Its dedicated protected-delivery mode is a proposal, not an existing bond benefit.

Not every bond needs to produce an explosion. Medic–Warden can develop coordinated triage: the Medic chooses a wounded ally for her actual Treatment while the Warden commits his actual Guard to protect the vulnerable treatment source. A later branch could support moving cover, at the cost of guarding someone else. Keep this inside the existing healing/protection roles and require a demonstrably new choice before accepting the node. Medic never becomes a damage source to fill a combo table.

**Do not lock the spectacular basic chain behind the developed stage.** Bonds broaden delivery and cooperation; compatible gear and class actions already produce the first chain. An unknown partner can still work with a veteran through normal commands.

Suggested constraints for a later prototype:

- Count one legitimate cooperative resolution once. Do not count each fragment hit, each pulse or every tick of Guard as another bond event.
- Support bonds need their own meaningful event definition; offensive mark cashes alone would exclude Medic/Warden. Any new counter is proposed, not part of today's three-cash rule. No training on self-inflicted damage or repeatedly arming/cancelling orders.
- One selected advanced cooperation per pair for the first test. A fighter can know several run relationships, but can participate in only one committed chain at a time. There is no hidden free second cast.
- A fallen/disabled partner cannot contribute. For the newer squad-restoration design, recommend retaining earned pair progress when both return after the fight, while cancelling their current combat commitment. This explicitly differs from old paper language about losing a bond on any fall; historical rules remain intact.
- Run relationships reset with the run; discovery of a recipe can persist. Permanent personal bonds between recruits would be a separate owner decision because the army resets.
- No gift economy or dozens of mandatory dialogue scenes for procedural fighters. Pals retain their camp story arcs separately.

### R6. A mixed-tribe chain move in an actual fight

**Proposed example: Scalding Verdict.** The name, equipment and behaviour are new. This is an authored scenario, not a replay or balance result. It occurs after recruiting both Bloodridge and Thundercoast, such as a later encounter on that run. It is not a promise that those recruits exist before the first chief, or that one run visits every tribe.

Your squad contains a Thundercoast Trapper, a Bloodridge Marksman, a Warden, a Skirmisher and a Medic. The two ranged fighters have compatible enhancements. Their native equipment supplies both attacks; the leader commands their coordination rather than adding a generic universal attack.

1. **Thundercoast Trapper catches an enemy.** Her proposed restraint attack delivers water through its native machinery and holds the target through her own Snare. The water source/reserve is real state. No water appears merely because a Tide fighter was selected.
2. **You see a second enemy approach.** You can keep the restraint and let ordinary attacks finish the first, or order the destructive chain before the group spreads.
3. **You commit the Bloodridge Marksman's enhanced heated shot.** The source is his equipped ritual-compatible assembly; this does not spend or grant the chief's Eclipse. Both his readiness and the selected target must be valid.
4. **The shot meets the Trapper's contained water.** The enhancement releases the combined effect as a short scalding burst around the trapped enemy. This is an explicitly authored fictional device interaction, not a universal simulation where every wet target plus heat must explode.
5. **The cost happens visibly.** The restraint ends and its water reserve is spent. You traded continuing control for a major area attack. If the enemy survives, it is free to attack again.
6. **The surviving enemy winds up a heavy. You parry it.** The parry buys the squad an advantage while its equipment recovers. You could instead have retained the trap or spent another defensive action. The chain did not require a parry to exist.
7. **On a later exchange, a cast starts elsewhere while you are preparing another chain.** A practised, linked pair can finish its committed chain while other eligible fighters follow your new Focus. If the committed shooter is the only valid interrupter, cancel the chain and use him. Bonds do not duplicate the unit or its cooldown.

All four Pals contribute: Butcher makes the compatible weapon fittings; Glutton contains the pressure/heat and defines the reserve tradeoff; Shade supplies the chained order; Whisperer's proposed signals/regalia support the committed pair's coordination. They are enhancing two tribal technologies, not erasing them.

Those are development paths across the system, not four mandatory unlocks before any chain can work. Provide a safe basic fitting and direct command for the first compatible pair; reserve containment choices, prepared delivery and advanced coordination broaden it later. Each Pal must eventually offer a distinctive choice, without making every recipe require all four.

**Change one component and the outcome changes.** A contained, precision branch can turn the same water-and-heat interaction into a focused discharge on the restrained target instead of a group burst, spending the reserve and ending the restraint in either case. A different equipped source with no heat cannot produce Scalding Verdict; it can still execute the pair's ordinary pin shot or an independently authored compatible chain. Changing the class changes the available action and role, not just the colour of the explosion.

**What cannot be assumed:** full chief flood behaviour from one Trapper; infinite water on dry ground; a chief spell permanently learned by every class; full damage and full restraint together after choosing to rupture the net; a successful second hit when the first target died or became invalid.

### R7. The first road still needs this gameplay

The Clarth already supply a complete native starting kit: tooth net, spine rifle, fin blades, jaw shield and treatment staff. Use that to demonstrate chain moves before new recruits.

**Starter chain: Toothstorm, proposed.** A Clarth Trapper's net restrains the target. A Clarth Marksman's fitted spine shot strikes the net's controlled release. The teeth/fragments burst into the nearby group and the restraint ends. The player spends the command and accepts that control loss. The precision branch keeps a concentrated pin shot instead; its ordinary class interaction remains available without a new bond.

This is not a renamed universal pulse: the actual second attack is the Marksman's shot; its native equipment and the Trapper's fittings cause the interaction. Without a ready Marksman you cannot fire that chain. You may still use the net, defend, redirect another class or use the acquired tribal spell.

The Warden–Marksman protection chain offers a different opening style on the same roster. Class gear and tree selections change whether the player wants sustained restraint, a group burst or a protected shot. Bonds add coordinated execution/alternative support after legitimate cooperation. No early enemy recruitment, extra class count or pre-earned friendship is required.

### R8. Rules retained, rules replaced, and Claude's next review

Retain the earlier paper's useful contracts: automatic baseline fighting; real source/target/readiness; existing command-point competition; explicit reservation/cancellation; no false ready cue; no free class spell copying; no recursive payoff farming; valid reach/formation; deliberate sacrifices; cancellation at kneel; authoritative save/replay state; and owner play before balance claims.

Replace the universal source-B pulse. A chain recipe now declares **tribe/class action A + actual action or equipped mechanism B + required enhancements + optional bond mode = named result**. Source B may be another fighter, a properly sourced equipment discharge or an actually acquired chief order. It is not always the leader's weapon. When another unit supplies it, the chain spends that unit's action/readiness once. When equipment supplies it, consume its actual charge/cooldown. It never creates a free duplicate of either source.

The existing setup-event solution for a unit attack plus a later equipment attack remains usable where needed, but two-unit chains should use the two real unit actions directly. Do not add an extra mark layer to every move just to reuse revision 1's architecture. Base automatic reactions remain eligible; if they already spent a required move or killed the target, the proposed player chain must revalidate or cancel rather than steal their payoff.

Recipe authoring needs a source record: native tradition/tool, class role, Pal enhancement, first action, second action, resulting effect, cost, target restrictions, bond variants and visible proof. This is a development checklist, not nine things the player must read during combat.

Keep the content bounded. Author a few reusable interactions with explicit compatibility, then distinctive named recipes. Do not promise all 25 tribes × 17 classes × every gear item × every pair. A recipe needs meaningful native differences; an empty compatibility cell is preferable to a fabricated ability. No new tribe, class, elemental law or technology origin is canon merely because it appears in this paper.

**Revised planning gates, replacing revision 1's recipe scope:**

1. **Source and role gate:** recover/approve the native kit and enhancement for each proposed chain. Start with existing Clarth equipment; mark Thundercoast class-tool and Bloodridge heat-chamber details as proposals.
2. **First-road gate:** demonstrate two ways to command the same Clarth squad, including a proper two-attack Toothstorm and a distinct protection/precision choice. Both work unbonded. No new implementation is authorised now.
3. **Bond gate:** one practised pair unlocks a new selectable cooperation, with a visible give-and-take. Compare it with fresh partners; avoid free extra attacks, support-class exclusion and mandatory bond farming.
4. **Mixed-tribe gate:** after legitimate recruitment, show Scalding Verdict or another approved cross-tribe recipe. Test it with and without the required source, water/heat reserve, pair availability and relevant target immunity. Against a protected chief, a restraint recipe is unavailable where Snare is prohibited; use a legal alternative rather than disabling the rule.
5. **Road/progression/replay gate:** camp knowledge, current gear, run talents and actual pair bonds stay distinct; old fights reproduce their old rules. The first complete build has a usable early version, deeper choices and later tribe combinations.

Claude: this revision is the current review target. Please answer in the shared log: (a) any missing owner decision about native tribal tech; (b) whether each candidate enhancement preserves class/tribe identity; (c) whether the native water/heat mechanisms need different proposals; (d) what bond changes genuinely add actions beyond current automatic behaviour; (e) how gear/tree/bond progression avoids granting the same effect repeatedly; (f) exact authoritative contracts and migration implications. Review only, no code, tuning, art or background instruction. No acknowledgment is claimed.

</details>

<details>
<summary>Revision 1 — original decision ledger, research and superseded generic-kit proposal</summary>

# Revision 1: Four Pals, talent trees and commanded combos

**26 October 2026 — Codex to Jacob and Claude. Plan only.**

Jacob's brief: stay involved commanding a small war party; parry to break attacks and gain an advantage; develop talent trees and combine two different attacks into a major attack; give all four Pals meaningful contributions and costs; reconsider the controls where necessary.

**Recommendation:** the Pals develop the equipment and commands through which you combine your squad's attacks. Talent branches change the combinations, their delivery and what you sacrifice to use them. The squad keeps fighting; you choose targets, defend, interrupt, commit a combination or save its components for another opportunity.

This paper contains recovered decisions, current source observations, external research and a new design proposal. These are labelled separately. Everything from section 4 onward is **proposed**, unless explicitly identified as an existing rule. Names and numbers are working examples. No game changes, balance sweeps, builds, playtests or background work were performed. The fights below are authored walkthroughs, not measured gameplay or predictions of exact outcomes.

## 1. What the old decisions actually say

The latest direct owner correction takes precedence over older assistant recommendations. The chronology matters: an old decision can be real and subsequently replaced.

| Decision | Evidence and status | Consequence for this paper |
|---|---|---|
| Automatic fighting plus player orders; fast action; 2.5D | [Locks: original constraints and 1 October calls](../DESIGN_LOCKS.md), lines 9–22 and 66–72 | Keep the commander role. Do not introduce direct movement and basic-attack control of the leader. |
| Four surviving Pals, trusted comrades and upgraders | [Locks](../DESIGN_LOCKS.md), lines 24–30, 93–95, 116–125 | They are people with distinct jobs, not collectible pets or four interchangeable reward dispensers. |
| All four Pals stay at camp; the leader is on the battlefield | Owner correction of 10 October, [locks](../DESIGN_LOCKS.md), lines 116–125 and 154–157 | The earlier fighting/transforming Pal plan is superseded. Their technology and teachings reach the battlefield; the Pals do not appear as extra combat units. |
| Butcher: weapons and gear. Glutton: armour/body upgrades. Shade: new orders. Whisperer: skins/regalia | [Locks](../DESIGN_LOCKS.md), lines 122–125 | Preserve these responsibilities. Giving regalia a new mechanical function is an explicit proposal, not recovered approval. |
| Camp upgrades/materials persist; run gear and acquired chief orders reset | [Locks](../DESIGN_LOCKS.md), lines 176–208; later forge/trophy exceptions at 229–240 and 455–469 | Separate permanent knowledge from the current run's choices. Do not lose existing permanent forge unlocks. |
| Chief-kneel recruitment; capture and execution have different rewards | [Locks](../DESIGN_LOCKS.md), lines 269–312 and 439–453; 25 October owner list at [handoff](../HANDOFF_LOG.md), around 11139 | No new early recruitment scheme is needed to demonstrate builds with the starting five. |
| Earned command points, including successful fighting actions and a fallback trickle | [Locks](../DESIGN_LOCKS.md), lines 286–290; [FightRules.cs](../Assets/Scripts/Sim/Battle/FightRules.cs) | New commanded attacks should compete for the existing points. Do not invent four Pal mana bars. |
| Class identities remain exclusive | [Locks](../DESIGN_LOCKS.md), lines 407–438; [class catalogue](../CLASS_ROSTER.json) | Only Trapper snares. Medic heals and does not DPS. Talents develop a class's own action. |
| Snare, Bleed, Opening and Guard connect the squad | Same 20 October lock | Keep the automatic teamwork. Commanded combinations add a further decision without making normal teamwork wait for a button. |
| Class pieces bend class spells; one gear slot; visible effect/icon rather than sprite clothing | Same lock, replacing older unrestricted gear | Do not add a backpack or several equipment slots. A tree is not another hidden stack of the same gear multipliers. |
| Ten-part hybrid accepted | [Locks](../DESIGN_LOCKS.md), lines 418–431 | Retain the agenda: status web, trophy pieces, tribe sets, kin bonds, Pal duos, formation, road scars, blood pacts, order sockets, road jobs. Their presence in a lock is not proof that all are built. |
| Trophy forging uses one part into one class piece | [Locks](../DESIGN_LOCKS.md), lines 420 and 455–469 | Do not quietly replace this with a two-part crafting recipe. Two attacks combining in combat is a different system. |
| Bonds: three combos by a pair in one fight, across any enemies; bonded shot immediate | Later owner calls at [handoff](../HANDOFF_LOG.md), around 11139–11147; current [Chains.cs](../Assets/Scripts/Sim/Battle/BattleSimulation.Chains.cs) | Bonds improve reliability. The first exciting combination must work before a bond exists. Current code also has eligibility and a minimum bonded gap. |
| Formation: two front, the rest behind; back row should drive combos | Same later handover | Position and links can matter, but cross-row adjacency was an assistant interpretation. Define proposed links explicitly below. |
| Proper talent trees, more combos, distinct traits, self-preservation and three-part unit kits | Owner list at [handoff](../HANDOFF_LOG.md), around 11139–11161 | These are real design priorities. The exact tree structure and numerical nodes in Claude's paper were proposals, not all approved together. |
| Latest request: all four Pals have something to give and take; real attack combinations; controls can expand | [Owner correction](../HANDOFF_LOG.md), entry dated 26 October beginning “Owner correction: active war-party command” | Discard Codex's starting-composition-first recommendation as the answer to this brief. The two-equipped-tribal-powers suggestion was never a limit. |

### Conflicts that must stay visible

- **Pal favours:** the 18 October request authorised road favours; twelve numerical modifiers exist in [CampaignFavours.cs](../Assets/Scripts/Sim/Learning/CampaignFavours.cs) and [SideFavours.cs](../Assets/Scripts/Sim/Battle/SideFavours.cs). Jacob now explicitly rejects generic boon stacking as the answer. This paper proposes a replacement direction for new campaigns; it does not delete historical favours or silently treat their ranks as talent points.
- **Talent ownership:** [Nerve sync and unit kits](Nerve%20sync%20and%20unit%20kits.md), section 4, proposed a permanent tree per class, three branches and a capstone. Which Pal trained them was unanswered. Four Pal workshop trees below are a new structural recommendation, including class-specific nodes; they are not a previously selected layout.
- **Whisperer:** regalia ownership is locked; functional regalia are not. The proposed signal-routing role needs Jacob's design verdict. Cosmetic appearances should remain freely selectable independently of combat behaviour.
- **Parry:** rules 8, rules 9, the current implementation and the latest Claude discussion differ over block penalties, learned timing tricks and which moves are fully negated versus mitigated. The 26 October handover reports removing wait-out/fake-first and using parries to build a break opportunity. That is not verified implementation. This paper requires truthful cues and a separately agreed move-response table; it does not silently pick one damage rule for every attack.
- **Road/chief loss rules:** later owner calls ask for three lives, squad restoration and no friendly fleeing. The rules-14 owner replay still lost a chief fight through shared morale with fighters alive. Never write a tutorial that promises the later design while the running rules still behave differently. This expansion does not settle that mismatch.
- **Older expansion freeze:** [Combat fix plan](Combat%20fix%20plan.md) proposed parking talents and combos until after 30 November. The document labels itself unapproved. Jacob's current instruction expressly requests this expansion plan; that old proposed schedule is not a veto.
- **Catalogue metadata:** `CLASS_ROSTER.json` still has an old Pal-scope sentence saying location/number are undecided. The later dated owner decisions settle those. Its class signatures remain important boundaries.

### What is present in source, and what this paper adds

Current source contains automatic class actions, class bends, marks, reactions, bonds, formation-related rules, orders, earned points, favours, forge transactions and chief spells. For example, `Chains.cs` chooses an eligible recipient of a setup, respects target/reach/disabled checks and restarts the reacting spell's clock. It does not promise that every setup receives an instant response.

A targeted search of `Assets/Scripts/Sim/` found no `Talent`, `Duo`, `Socket` or `Pact` symbols. That is supporting evidence, not a complete proof that no equivalent logic exists under another name. No four-Pal tree interface or commanded fusion system was established by this review. New recipes below are not existing abilities with new labels.

## 2. Research: useful mechanisms, with their limits

Sources checked on 26 October 2026. This is document research, not fresh hands-on reference-game testing. These sources establish mechanics and design precedents; they do not prove this proposal will be fun, balanced or commercially successful.

| Reference | What the source establishes | Application to Kindred Siege — design inference |
|---|---|---|
| [Xenoblade Chronicles 3, Nintendo's game page](https://www.nintendo.com/us/store/products/xenoblade-chronicles-3-switch/) and [2022 Direct announcement](https://www.nintendo.com/us/whatsnew/nintendo-unveils-xenoblade-chronicles-3-direct-presentation-packed-with-new-footage-and-details/) | Automatic attacks coexist with player-used Arts; class-specific actions, linked attacks and Fusion Arts develop the combat. | Automatic unit fighting can support active combination play. Borrow the relationship between ordinary attacks and deliberate payoffs, not character switching, free class swapping or Ouroboros transformations. |
| [Final Fantasy VII Rebirth, Square Enix battle guide](https://www.square-enix.com/ffvii/en-us/games/rebirth/battle/) | Ordinary attacks build ATB; abilities spend it. Synergy abilities combine two characters in a visible coordinated attack and are earned through using abilities. A command menu can slow time. | Let normal fighting create opportunities for commanded combinations. Both contributors should be identifiable. A useful command should compete with another use of resources. This does not justify importing every FFVII gauge. |
| [Last Epoch, official Skill Specialization documentation](https://support.lastepoch.com/hc/en-us/articles/46363203944859-Skill-Specialization) | Specialising a known skill exposes its own tree, with a limited set of specialisation slots and a documented respecialisation route. | Put concrete changes to an action inside a tree and keep the active selection bounded. The proposed KS branches and free camp switching below are our choices, not copied Last Epoch rules. |
| [Hades, Supergiant's Big Bad Update](https://www.supergiantgames.com/blog/brace-for-the-big-bad-update/) and [Beefy Update](https://www.supergiantgames.com/blog/get-pumped-for-the-beefy-update/) | Developer notes describe new combined god powers, weapon upgrades and improvements to the core dash in the same development period. | A named interaction between two sources can justify a joint unlock; build expansion and responsive combat need to develop together. This is not a recommendation to restore generic Pal boons. These are historical development notes, not current balance data. |
| [Guild Wars 2 combo reference](https://wiki.guildwars2.com/wiki/Combo) | The official-hosted, community-maintained wiki describes fields and finishers producing additional effects; the field type and finisher type both matter. | Define exactly what the first action leaves, which second action interacts with it and what new result appears. This is supplementary reference, not developer-authored evidence. Search-index text was available; a direct fetch returned 403. |

The useful common principle is **an identifiable contribution from A, an identifiable contribution from B, and a result that changes what happens**. A damage bonus can support that result, but cannot be the whole experience. This principle is our synthesis, not a quotation or a universal rule proven by those games.

## 3. The experience in ordinary language

Your Marksman fires. His upgraded round leaves a physical anchor in the enemy's armour. You can use it now, or keep your attention on an incoming attack. You parry that attack, then command the leader's relic to drive a second pulse through the anchor. The two attacks become a heavy armour-bursting strike.

On another build, your Trapper's net is the first attack. You command a pulse through its fittings and the net bursts into a fan of fragments. You hit several enemies, but the trapped one is released: you deliberately traded control for the explosion.

The Pals created those possibilities. Your tree choices decide which version you have, what can feed it and how you deliver it. During combat you still have to choose the target and moment, preserve your fighters, answer threats and sometimes abandon the combo.

**A parry can create a safer moment to combine attacks. It is not the mandatory first ingredient of every recipe.** Successful automatic attacks, normal Guard use and deliberate commands also contribute. That is essential to avoiding another fight where the player only waits for Hold.

## 4. One connected proposal for all four Pals

All mechanics in this section are new proposals. The physical devices are explanations for the effects, not approved art briefs. Pals remain at camp.

| Pal | What they develop | What you can do during the fight | What you give up |
|---|---|---|---|
| **Butcher — weapons and gear** | Compatible fittings on a class's existing weapon, plus a pressure-pulse module controlled by the leader's relic | Combine a landed class attack with a second, commanded weapon attack. Choose a concentrated strike or a fragment burst. | A concentrated setup cannot also be the crowd-burst setup. A burst may destroy the trap that was keeping an enemy harmless. |
| **Glutton — armour/body upgrades** | A reserve layer in the Warden's armour, charged by actual Guard interceptions; an alternative plate design for retreating | Keep that reserve for the next guarded impact, or discharge it through an attack combination. | Discharging spends the extra protection. Choosing the retreat plate gives up the offensive reserve altogether. Normal Guard remains functional. |
| **Shade — orders** | A direct release order or a prepared follow-through order | Fire into an existing setup now, or commit a payload to a named unit's next eligible attack while you handle another threat. | Prepared attacks reserve points and bind a target; they can be cancelled but cannot silently follow you to a better target. |
| **Whisperer — regalia, proposed functional extension** | A target-bound sigil or a link carried in the regalia of two designated fighters | Preserve a prepared attack on one enemy while Focus moves elsewhere, or split one payload across attacks by a linked pair. | Target commitment loses flexibility; linked delivery divides power and fails when the pair is separated or disabled. |

The distinction between the last two: **Shade changes when you authorise the second attack; Whisperer changes what the authorised attack is attached to.** Butcher supplies its offensive form. Glutton lets a defensive resource become part of it. They should not all offer the same “more damage” upgrade in different colours.

The basic two-attack combination works with Butcher's fitting and Shade's release order. Glutton and Whisperer create alternatives and tradeoffs rather than four compulsory ingredients for every combo. The early road must demonstrate contributions from all four without making the player unlock four finished trees before the game begins.

### A necessary role boundary

The leader's relic issues the command and operates the equipped module. He does not gain manual basic attacks or free movement. A module does not give the Marksman a Snare, the Warden a healing spell, or the Medic an attack. A combined burst also must not erase the future Artillerist's sustained area-damage role, Recon's scanning, Binder's displacement or Disruptor's shield suppression. The proposed first recipes therefore do not add pulls, general armour debuffs, free interrupts or repeatable area stun.

## 5. Talent trees: concrete branches and progression

### Organisation

Recommend **four Pal workshop trees** with class-specific nodes inside them, plus joint nodes connecting compatible branches. This is a replacement for the earlier proposed seventeen separate class-tree screens, not an additional layer on top of them. The class filter shows only nodes that apply to that class. A Warden never receives a Marksman node because the same Pal sells both.

The tree has prerequisites and mutually exclusive branches. Owning both branches permanently does not activate both. A node must show its changed action and its loss before purchase. Avoid long prerequisite ladders of +2% damage.

| Tree | Starter capability | Branch A → later extension | Branch B → later extension |
|---|---|---|---|
| **Butcher** | **Coupled Armament:** an eligible class attack can leave one short-lived equipment setup for the relic pulse | **Driver:** Marksman Aimed Shot plants an anchor; pulse becomes **Lancebreak**, concentrated on that enemy → **Throughline:** a previewed second enemy behind it can receive the remaining projectile, with damage split | **Shatter:** Trapper Snare plants breakaway fittings; pulse becomes **Cageburst**, releasing the target and throwing fragments at a bounded nearby group → **Directed Fan:** choose the burst direction, accepting narrower coverage |
| **Glutton** | **Reserve Plate:** a real Guard interception fills one visible reserve; keeping it grants one extra guarded soak | **Return Pressure:** discharge the reserve into a Skirmisher strike for **Hammerfall**, a short cone attack, spending the defensive reserve → **Split Reserve:** two smaller stored portions; each is weaker and occupies capacity | **Carry Cover:** preserve that one extra protective layer through Fall Back for its original guarded ally → **Escort Plate:** move the remaining layer to another ally once, losing some capacity; no offensive discharge |
| **Shade** | **Release:** a commanded pulse combines with an eligible landed attack; it can also fire a modest plain pulse | **Direct:** choose and release against a current setup → **Recast:** cancel before release without losing the reserved points, but start a short retry delay | **Follow-through:** arm a named fighter's next eligible class attack on a named target, for a bounded time → **Delay Seal:** hold that landed setup briefly for a manual release instead of firing immediately |
| **Whisperer** | **Signal Regalia:** show and bind the contribution of the selected source fighter; default binding follows the active Focus target | **Hunt Sigil:** seal one prepared combo to an enemy while Focus changes elsewhere → **Pursuit Thread:** the seal follows that same enemy between rows, subject to actual attack reach | **Procession:** link two declared adjacent fighters; after both make an eligible attack, divide one pulse between their two targets → **Passing Standard:** after a successful pair release, explicitly hand the link to a different adjacent pair; no copied payload |

**Cautions on later nodes:** Throughline and Directed Fan need authoritative spatial targeting, which the present row model may not express well. They are expansion content, not prerequisites for the first paper-to-prototype gate. Procession's first version uses row slots, not guessed sprite distance. It allows neighbours within a row and the directly aligned front/back slot; this is a proposed explicit answer to the old unresolved adjacency question.

Glutton's reserve is a single equipment-state indicator, not another growing combat currency. Full means one choice: keep it for defence or discharge it. It does not fill from holding the Hold button, friendly damage, damage-over-time ticks or attacks that were completely avoided. A perfect parry does not automatically generate it. Ordinary autonomous Guard provides its useful input.

### Permanent progress versus the run

Proposed acquisition rules:

1. On the first post-tutorial camp visit, each Pal provides a starter capability and access to their first fork. Offer one complete suggested configuration, with all four choices editable, instead of requiring four purchases before the first build works. The introductory fittings, command and regalia do not require grinding or capturing a second tribe. Deeper nodes are researched later. These are additions to the tutorial/camp progression, not already implemented rewards.
2. Materials permanently research branches and joint recipes. The four Pals use the existing material pool. No four new currencies and no four independent affection grinds.
3. Before a run, choose one researched branch in each workshop. Switching researched branches at camp is free. Research prices remain to be set after the first playable comparison; this paper invents no economy target.
4. On the early road, a Pal reward offers a **specific installable node from that tree**, with its prerequisite and cost shown, or keep the materials. It changes an action. It is not an endlessly repeatable rank of a percentage buff. The current favour-node reward system would need a new versioned offer contract.
5. A run node expires with the run; researched knowledge persists. Ordinary collected gear still resets, with the existing permanent forged-item exceptions preserved. Do not reset existing forge ownership to make this model fit.
6. One deeper node is active per chosen branch in the first comparison. This is a prototype budget, not a permanent full-game limit. Joint recipes are the interactions between the selected nodes, not another loadout slot or purchase on top: choosing Return Pressure permits Hammerfall; Carry Cover excludes it. Follow-through and Hunt Sigil can coexist with Hammerfall because they modify different parts of the command.
7. Unlocking more classes and tribes broadens compatible actions and recipes over the seven tiers. The early Clarth combination already works; later content supplies alternatives.

This creates different decisions at camp and on the road: **which way to develop your command kit, then which opportunity to take with it during this run.** It does not rely only on selecting the five fighters.

### Reuse existing work without double counting it

Deadeye/Quickdraw, Retaliator/Bastion, Barbs/Long Snare and Surgeon/Field Kit already supply class-bend tradeoffs. Keep these as gear bends in the first comparison. Do not sell identical permanent talent versions that stack with them. A later decision can move a bend into a tree, but must explicitly migrate its old gear/reward role. The new talents above modify the extra attack's shape, storage, timing and routing instead.

Basic Guard, Treatment, self-preservation, class teamwork and legible targeting remain baseline functions. Buying a talent must not be required to make a Medic heal properly or a Warden protect the leader.

## 6. Proper combos: name both actions and the new result

The recipes are deliberately bounded. These are proposals, not current combos.

| Recipe | First action | Second action and its source | New result | Price and failure |
|---|---|---|---|---|
| **Lancebreak** | Marksman lands Aimed Shot with the Driver fitting, embedding an anchor | Player releases Butcher's pressure pulse through the leader's module | A distinct heavy strike drives through the anchor into the same enemy. Later Throughline can carry a split remainder into a previewed target behind. | Spends Release points and the anchor; cannot hit an unrelated enemy because Focus changed. Missed first shot means no anchor. No automatic shield bypass. |
| **Cageburst** | Trapper lands Barbed Snare with Shatter fittings | Player's pressure pulse ruptures those fittings | The net opens into a short fragment fan. One trapped attacker becomes a crowd-damage opportunity. | Ends that snare immediately, consuming its fittings; enemy can act again. No transfer of Snare to others. Against an isolated enemy it remains a focused burst but loses its crowd advantage. |
| **Hammerfall** | Skirmisher lands Running Strike | Player releases the stored Guard reserve as an armour-powered impact through the command module | The strike is followed immediately by a short pressure cone around its target | Consumes one full defensive reserve and command points. No reserve means a plain pulse, not a free Hammerfall. No new stun or knockback. |
| **Crossfire** | Two linked fighters each land their own eligible attack on two different enemies | One commanded pulse is routed through their regalia to both completed setups | Two coordinated impacts land as one visible pair attack | Splits one damage budget; needs two living, enabled contributors and an intact link. No second full-strength free attack, no extra point refund for the second impact. |

**Support interactions remain valuable without pretending they are two-attack finishers.** Warden Guard plus Medic Treatment is protection and healing. A later Medic talent may change who she can treat or what she cleanses, within her role. It should be described as a support combination. This paper does not turn her heal into a damaging detonator to include her in the offensive table.

### What the game needs to remember

Proposed equipment setup record: recipe type, originating fighter, enemy, originating attack event, expiry and whether it was spent. It is created only by the relevant landed class attack. It is a short-lived consequence of the fitting, separate from the existing Snare/Bleed/Opening payoff rights.

The starter Coupled Armament fitting can record a landed Marksman shot, Skirmisher strike or Trapper snare attack for a basic pulse or pair recipe. Driver and Shatter specialise the result of their respective class's setup; they do not disable the other classes' basic contribution. Thus a Shatter build can still combine its Trapper and Marksman in Crossfire without secretly equipping Driver as well. Medic treatment and Warden Guard are not offensive first attacks. Normal basic hits, generated pulses and damage-over-time ticks do not create these setups.

That separation addresses a real race in current combat: an automatic Skirmisher may cash Opening before the player can press anything. The new command cannot steal that ordinary class combo or depend on winning a one-frame race against it. The equipment setup survives for its own short window even after the normal Opening was cashed. Cageburst is the exception: it still requires its actual net to exist and explicitly consumes it.

For the initial proposal, one source can hold one unspent setup; creating a newer unreserved setup replaces that source's older one. A prepared command reserves a particular source/event/target. It never changes those identities silently. “Signal” here means a visible connection to a real attack, not a free vulnerability debuff that duplicates Recon.

### The execution rules

`No setup → eligible attack lands → setup available → player commits Release → second attack lands → recipe resolves → setup spent.`

For Follow-through: `player arms one source and target → points reserved → that source's next eligible attack lands before expiry → second attack follows immediately.` A queued miss is not an eligible hit; the order may wait for a later valid hit only within its original deadline. It never extends itself indefinitely.

- The second attack produces the visible fusion at its actual impact. Do not leave two completed attacks sitting in a hidden queue for seconds and then display a delayed “combo.” The optional Delay Seal is visibly player-held and requires another release.
- Use a fixed event order: resolve the initiating hit and its ordinary class reactions, then check the equipment follow-through against the surviving, still-eligible target. If the ordinary chain already killed that target, cancel the unlaunched pulse and return its reservation. The new system does not suppress a valid automatic chain to guarantee its own spectacle.
- A root/direct pulse may fire without a setup, with a modest standalone effect. The button must preview **plain pulse** versus the named combo before spending. It cannot advertise a guaranteed combo on a future random hit.
- Before launch, an invalid/dead/fled target or disabled/dead source cancels the prepared attack and returns reserved points. A short retry delay prevents free repeated arming from producing effects. Once launched, the cost is paid; target death or loss of eligibility can waste it. No silent retarget.
- A changed Focus cancels an unsealed Follow-through. Hunt Sigil explicitly preserves its original target and source, including a one-action target override for the committed fighter; other fighters obey the new Focus. The committed fighter still needs their normal readiness and reach. After that action, they follow ordinary targeting again. The seal does not give free reach, bypass a wall or force an extra attack.
- Fall Back stops incompatible melee follow-throughs. Procession links suspend while the squad is separated. No distant Skirmisher hit appears because a combo was queued earlier.
- No payoff starts another payoff automatically. Generated pulses cannot make fresh equipment setups. One setup pays once; one pair release spends one payload. No recursive chains or resource farming.
- Armour reserve and combo setups clear between fights. Bonds and run talents follow their own run lifetime; camp research persists.

### Numbers for a paper comparison, not a balance change

Use the existing command-point economy. Candidate Release cost: 2 points; setup window: 4 seconds; Follow-through expiry: 4 seconds; module recovery: 6 seconds. Test ranges later: cost 2–3, setup/queue 3–5 seconds, recovery 5–8 seconds. Longer windows allow decisions; longer recovery prevents every unit attack demanding a press. These values are assumptions to test with Jacob, not human-attention limits or measured sweet spots.

Proposed integer damage accounting:

`AdditionalPayload = floor(BasePulse × RecipeFactor / 100)`

The first attack already dealt its damage. Do not apply it again inside the combined result. For a split recipe, `partA + partB = AdditionalPayload`; deterministic remainder goes to the first declared target. The combined animation can show one total receipt, but the archive keeps both actual hits. Glutton's reserve changes the payload into Hammerfall; it is consumed once, not another multiplicative bonus applied at every stage.

A future tuning table must specify the absolute BasePulse, each recipe factor and per-target caps. This paper deliberately leaves those unset: without a real timing/damage comparison, invented numbers would imply false balance confidence. A combo's own extra pulse earns no command-point refund in the first prototype; its normal initiating class combo keeps its existing legitimate reward. That makes Release a resource choice rather than a self-funding button.

## 7. An actual road fight, decision by decision

**Authored example.** Same starting Clarth Warden, Marksman, Skirmisher, Trapper and Medic; leader commands. Warden/Skirmisher front, the others back. Opponents: a front-line attacker, a guarding fighter and a ranged caster. This is a scenario to specify and test, not a claim that this exact sequence occurred in an archive.

This early-road build has Butcher's **Driver**, Glutton's **Reserve Plate/Return Pressure**, Shade's **Follow-through**, and Whisperer's **Hunt Sigil**. This is an illustrative accessible build after early choices; it does not assume every capstone is unlocked. Its player sentence is: **“Prepare a heavy shot, keep the enemy busy while it lands, then decide whether my saved protection is worth spending on another attack.”**

| Fight moment | Squad action | Your action | Result and what you can see |
|---|---|---|---|
| Engagement | The Warden covers, Medic treats when needed, Trapper sets the trap, damage classes attack | Focus the front attacker and arm Follow-through from the Marksman on that target | A small paired preview connects the rifle to the relic pulse. Its cost is reserved. Other class actions continue. |
| Incoming heavy | Enemy raises a heavy attack at the front | Parry its genuine impact cue | The heavy is answered according to its agreed move rules. The window to attack is safer. It has not magically supplied the Marksman's half of the recipe. |
| Rifle hit | The Marksman's actual Aimed Shot lands on the selected enemy | Your already-armed command executes | His anchor and the relic pulse meet: **Lancebreak**. Rifle, pulse and target are visible. The normal automatic Opening/Skirmisher chain remains eligible separately. |
| A later enemy cast begins | A second Marksman follow-through has been armed and sealed on the front attacker; its shot has not landed yet | Move Focus to the caster and interrupt through the normal valid command/action | Hunt Sigil preserves that specific prepared shot's target while other eligible fighters follow Focus. This proposed queue override applies only to that one already-committed class action, then normal targeting resumes. Without the seal, changing Focus cancels it. If no other fighter can interrupt, cancel the sealed shot and redirect the Marksman instead. |
| Guard interception | An ordinary enemy attack lands on the Warden's Guard | No compulsory Hold press | Glutton's reserve fills once. A clear plate indicator shows extra protection is available. This came from normal squad protection. |
| Skirmisher hits | Running Strike lands and leaves its eligible equipment setup | Choose: Release now for Hammerfall, or retain the plate because another heavy is coming | Attack spends the protection and makes a cone impact. Keeping it allows the next Guard interception to use the extra layer. Neither choice prevents the Skirmisher's ordinary strike. |
| A bad moment | Marksman is disabled before a new prepared shot lands | Cancel/replan or Fall Back; do not stare at a falsely glowing finisher | Reserved points return before launch. The target/source cue explains why the combination ended. You still have defensive commands, automatic fighting and a plain pulse option. |
| End of fight | Last hostile falls or otherwise exits under the selected rules | Take a specific next tree node or bank materials | The reward shows how the next fight will change. It does not merely announce another +10% rank. |

**No-parry route through this same fight:** Focus and prepare the shot while the Warden handles an ordinary attack; dodge the dangerous heavy with Fall Back; release the valid ranged combination once retreat restrictions permit it. The offensive recipe still works. You give up the parry advantage and some attack time. This route needs actual testing; it is a required alternative, not a promised win.

### What all four Pals did here

Butcher made the anchor/pulse interaction. Glutton made a guarded hit into a defence-versus-offence choice. Shade let you commit an upcoming attack while continuing to command. Whisperer let you keep a named attack committed while responding to a different enemy. Removing any one changes a specific decision; removing Glutton or Whisperer does not make the baseline attack cease to exist.

## 8. The same five fighters, a different way to fight

Second candidate: **Shatter + Carry Cover + Direct + Procession**. This build trades the prepared single-target shot and offensive armour reserve for immediate trap bursts, safer retreats and divided attacks by a linked pair.

You let a front attacker spring the Trapper's net. Two enemies are close enough to fit Cageburst's visible area. You can leave the net intact to prevent an attack, or release the pulse now and burst it. You choose the burst because the second enemy is starting pressure on your wounded front. The trapped enemy is released; the animation and targeting state must show that cost immediately.

The enemy then marks your front with an area attack. You Fall Back. Glutton's chosen plate preserves its one extra protective layer on the original ally, but Whisperer's Procession link suspends and a melee pair attack cannot complete. You deliberately gave up a combo opportunity to keep the squad alive.

When the lines reform, the Trapper and Marksman each land an eligible attack on different targets. You spend one pulse through their restored link for Crossfire: a divided hit on both, rather than Lancebreak's concentrated one-target attack. With only a single enemy left, choose a plain pulse, a valid single-target class combo or another command; Procession has no invented second target.

| Comparison | Prepared strike build | Burst-and-retreat build |
|---|---|---|
| What you watch | A named fighter's readiness, sealed target, armour reserve | Enemy grouping, active net, whether the selected pair remains linked |
| What you commit | An upcoming shot plus reserved command points | A current net and its remaining control time |
| What makes a good moment | An interrupted/answered threat, then a reliable shot | Two enemies in a valid burst area, then room to retreat |
| What you sacrifice | Points tied up; reserve spent on attack; reduced ability to change an unsealed target | Snare protection; concentrated damage; pair link during retreat |
| What pressures this build | Source fighter disabled, a protected/invalid target, another cast demanding the reserved points | Isolated enemies, spread formations, forced separation of the linked pair |

These are different command problems on the same squad. They are hypotheses to compare, not labels attached to two damage totals.

## 9. Through a chief fight without breaking the chief's rules

Use Bloodridge as the worked case because its known problems are concrete. Treat exact timers, current move selection, parry mitigation and break thresholds as unresolved dependencies, not silently revised numbers.

1. **The chief is protected while his band stands.** Work on the band with Lancebreak or Cageburst. Do not pretend Snare or Opening can be applied to a protected chief where his rules prohibit them. Focus and ordinary damage remain subject to real reach and guard rules.
2. **He starts a Cleave.** Parry it for its agreed defensive/break benefit, or use another valid answer. A successful parry of that Cleave does not answer a different simultaneous cast. Each resolved threat needs its own receipt.
3. **A caster/interruptible chief move starts while you have an attack prepared.** Focus the valid interruption target. Hunt Sigil can preserve the earlier prepared target, but it does not refund the points you already reserved. You may cancel the attack to afford the defence. This is an actual competing decision.
4. **Sun Mantle is active.** A damaging fusion is still damage and must respect its retaliation. A glowing offensive button does not mean “safe.” Switch to the band or take another valid defensive route. A reserved attack can expire because you wisely chose not to fire into the mantle.
5. **The band is gone and a legal attack opportunity opens.** The Marksman supplies the first landed hit; release Lancebreak inside the available window. Against the chief, an equipment anchor is allowed only if the recipe/target eligibility explicitly permits it. Proposed rule: an ordinary chief damage window can accept this damage-only anchor after band protection is gone; it supplies no extra stun or bypass. This needs a new rules-version definition.
6. **If you chose the armour route,** a legitimate Guard interception can supply the reserve; a later Skirmisher hit supplies the other attack. You may keep the armour because another threat overlaps. Do not require more parries to refill the offensive recipe.
7. **At kneel,** cancel queued hostile pulses, stop new offensive combinations and present Spare/Execute. A delayed finisher must not steal the recruitment choice. Capture and execution use the existing authoritative rewards.

The paper does not claim these additions solve the current chief's shared-morale loss, misleading cue eligibility or overlapping-move presentation. Those are explicit integration gates. It also does not make ordinary HP damage drain the break bar by implication. Which combo affects health, nerve or chief break must be authored per recipe; initial proposed pulses deal health damage only.

## 10. Pal combinations: recover the old idea without restoring boon stacking

Four Pals have six possible pairings. That arithmetic is not a requirement to build all six at once. A joint interaction becomes available through **specific active tree prerequisites**, with a known combat cost and a named changed action. It is displayed at the connecting nodes, rather than sold again in an extra duo slot. Merely owning one reward from each Pal is insufficient.

| Pair | Proposed joint direction | What prevents it being a free upgrade |
|---|---|---|
| Butcher + Glutton | **Hammerfall:** offensive fittings can discharge stored armour pressure into a landed melee strike | Spends the protective reserve; needs the real Guard event and the real strike |
| Butcher + Shade | **Follow-through armament:** prepare the second attack before the unit's attack lands | Reserves points; expires; locks source/target; no auto-retarget |
| Butcher + Whisperer | **Crossfire:** one payload follows two valid attacks | Splits one budget; needs an intact declared pair |
| Glutton + Shade | **Carry Cover:** a retreat order carries the armour's remaining extra protection with its original recipient | Forgoes the offensive reserve branch; does not retain every formation bonus |
| Glutton + Whisperer | **Escort Plate:** deliberately move the one stored layer through a valid link | Original ally loses it, some capacity is lost, no copying; later expansion |
| Shade + Whisperer | **Hunt Sigil:** a prepared order remains attached to its sealed enemy when Focus changes | Cannot freely swap the sealed target; escape, death, expiry and invalid reach still matter |

This preserves the older desire for Pal duos, costly powerful upgrades and order sockets. It changes their delivery from random numerical favours to authored tree interactions. Jacob must approve that replacement before implementation. The table is a design map, not a six-feature sprint commitment.

## 11. Controls and feedback must serve these actions

Proposed first layout: keep the currently needed defensive and target commands accessible, and add a dedicated **Release** action with the actual equipped attack's name/icon. Hold/parry, Focus, Fall Back, Unleash, Release and the current tribal spell are a first layout candidate, not a permanent six-button limit. No four remote “summon Pal” buttons.

Release is not a random context lottery. It shows the recipe and target the player selected. With Follow-through it displays the named source, named enemy and reserved points. With Direct it previews the actual available result. A compact selector can choose a different valid recipe; changing it before commitment is not a combat action. Do not hide multiple simultaneous recipes behind one button that chooses its own preferred target.

The chief spell stays its own action. Dust Shark remains available from the Clarth as locked. This paper does not invent an early elemental spell because Bloodridge's Eclipse has not been acquired. Later acquired chief spells may become explicitly authored second attacks in recipes, after their real source and exclusions are defined.

On the field:

- A setup appears at the actual source and target: anchor in armour, net fittings or charged plate. Shape plus icon distinguishes them; colour alone is insufficient.
- A prepared command shows a line with the two contributors and a small expiry indicator. Unselected possibilities stay quiet.
- The two attacks visibly meet. The result has one distinct impact and a short name. Do not replace the effect with a paragraph or a large stack of labels.
- The resource bar distinguishes available points from reserved points. A warning means the order cannot be afforded; “ready” never ignores that.
- Parry cues remain readable over offensive effects. A new queued attack does not cover the enemy's impact cue or silently consume a parry input.
- Cosmetic hit-stop must not silently shorten simulation response windows. Define whether simulation time pauses before implementation and record the same command timing in replays.
- Pause can inspect and plan, but must not manufacture a valid parry: preserve the historical rule that an order given while paused does not become a perfect timed parry.
- The result/replay can identify first attack, second attack, cost, target and why a proposed combination failed. The player should not need to read that log to understand every fight.

## 12. What needs a design verdict, and the plan gates

### Decisions presented for Jacob's review

1. Four Pal workshop trees with class-specific nodes, instead of implementing the older proposed tree-per-class layout in parallel.
2. Functional Whisperer regalia as target/pair routing, with cosmetic appearance separated from combat selection.
3. The two-attack command system, including short-lived equipment setups separate from ordinary automatic combo marks.
4. Permanent research plus run installs, replacing generic new-run favour ranks; preserve historical saves/replays and permanent forge ownership.
5. Direct versus prepared release, with target commitment, resource reservation and explicit cancellation.

No new answer is required to finish this paper. These are the concrete choices the paper exposes before a later implementation go. Nothing here is entered into `DESIGN_LOCKS.md` as approved.

### Gate A — Reconcile the paper

Claude reviews the decision ledger and the concrete rules, particularly class boundaries, Whisperer's extension, the cost model and current chief targeting/mark eligibility. Resolve whether the older class-tree proposal is replaced. Keep the parry and loss-rule contradictions visible. End with one agreed rule set, not several incompatible papers quietly left active.

### Gate B — Prove two combinations in a bounded prototype, only after a build go

First playable comparison: Lancebreak and Cageburst with Direct Release, plus Hammerfall to exercise Glutton's keep-or-spend reserve. Include one Follow-through queue and Whisperer's seal so preserving a selected target through a Focus change is a usable decision, not a tiny projectile-flight trick. Demonstrate readable source/target/impact and a credible no-parry route before calling the four-Pal proposition demonstrated. That is three offensive recipes, one queue and one target-routing rule, not merely two effects. Do not begin by filling twenty-five tribes or every talent node.

This gate is larger than a HUD patch: it needs authoritative new actions, records and reward/loadout data. No hours estimate is claimed before Claude reviews those dependencies. Prototype damage and timing remain provisional until Jacob plays.

### Gate C — Add timing and formation choices

Compare the two worked builds at the same progression budget. Extend the Direct/Follow-through comparison and add the bounded Procession pair. Show one deliberate cancellation, one failed combination and one situation where retaining defence beats releasing damage. If every build is played by repeatedly pressing Release, revise the mechanics before adding capstones.

### Gate D — Make the road develop the build

One meaningful starting choice; at least two legible early reward decisions; one compatible chief opportunity; one reason to change choices on a later run. Ensure a missed reward or lost source fighter does not disable all useful commands. Recover class-gear, trophy, scar and tribe systems as distinct contributions rather than four ways to grant the same multiplier.

### Gate E — Compatibility and owner play

New campaign rules and serialized loadout/setup/queued-command/reserve state. Historical rules use their historical point, favour, cue and combo behaviour. Save/reload before launch, between both attacks and during a queued order; no changed target, duplicated hit or refund. Tests and scripted traces support the implementation; Jacob's play decides whether the interaction is enjoyable and readable.

### Acceptance examples to carry into implementation

| Situation | Required result |
|---|---|
| First eligible hit misses | No equipment setup; no false combo claim |
| Automatic Skirmisher consumes Opening | Equipment setup remains usable where its independent recipe permits; no theft or double payment of the normal combo |
| Cageburst detonates | Net ends, actual eligible enemies receive fragment hits, control loss is visible |
| Reserve kept versus spent | Next valid guard uses the reserve, or Hammerfall consumes it; never both |
| Player changes Focus | Unsealed queue cancels; sealed queue remains on its original target |
| Linked fighter dies, freezes or retreats | Pair recipe stops/suspends according to stated rules; no phantom second attack |
| Two recipes compete for one setup | The explicit selected recipe consumes it once |
| Enemy has no nerve or is boss-control immune | Damage recipe still uses authored damage eligibility; no invented crowd control or break-bar interaction |
| Chief kneels during a pending command | Hostile queued attack cancels; Spare/Execute remains the player's decision |
| No successful parries in the scenario | Normal squad play and commands can still produce valid combinations; victory is not automatically promised |
| A new tree option is chosen | Player can describe a different action or sacrifice in the next fight |
| Old archived fight is replayed | Original outcomes and checksum remain unchanged |

## 13. Dependencies and handover to Claude

**Read/review request, not implementation authorisation.** Reply in `HANDOFF_LOG.md`; Jacob should not have to carry messages between chats.

Please challenge this concrete proposal in these areas:

1. Which recovered decisions are missing or have a later owner correction? Cite the later entry rather than treating an older paper as final.
2. Do the four Pal contributions remain distinct after considering the full class catalogue? Whisperer's extension and the leader-operated pressure module especially need a verdict, not silent acceptance.
3. Can the first source/second source/result contract coexist with current automatic mark reactions without stealing their payoffs or making every proc pay twice?
4. Which current rules prevent the road/chief walkthrough from occurring? Name the precise API/state gap. Separate those gaps from disagreement with the proposed fun.
5. What would make one branch universally best? Review point reservation, cancellation, pressure farming, target-seal flexibility, adjacency and repeated area attacks.
6. Which minimum nodes must be available on the first road so this is a playable build system early, not a promise behind several captured tribes?

Likely implementation dependencies, listed for planning only:

- **Sim:** [BattleSimulation.Chains.cs](../Assets/Scripts/Sim/Battle/BattleSimulation.Chains.cs), [ClassSpells.cs](../Assets/Scripts/Sim/Battle/ClassSpells.cs), [BattleSimulation.Class.cs](../Assets/Scripts/Sim/Battle/BattleSimulation.Class.cs), commands/events, ordered battle state and explicit target eligibility. Keep class reactions and equipment reactions separately attributable.
- **Campaign/rewards:** [Campaign.cs](../Assets/Scripts/Sim/Learning/Campaign.cs), [CampaignFavours.cs](../Assets/Scripts/Sim/Learning/CampaignFavours.cs), [CampaignForge.cs](../Assets/Scripts/Sim/Learning/CampaignForge.cs), [CampaignGear.cs](../Assets/Scripts/Sim/Learning/CampaignGear.cs), [CampaignSave.cs](../Assets/Scripts/Sim/Learning/CampaignSave.cs), [ReplayEvidence.cs](../Assets/Scripts/Sim/Learning/ReplayEvidence.cs). Permanent unlocks, active branches and run installs are different saved facts.
- **Screens/input:** [CampaignArtInnerCircle.cs](../Assets/Scripts/Slice/CampaignArtInnerCircle.cs), [CampaignArtProgression.cs](../Assets/Scripts/Slice/CampaignArtProgression.cs), [CampaignArtFavours.cs](../Assets/Scripts/Slice/CampaignArtFavours.cs), [CampaignArtInput.cs](../Assets/Scripts/Slice/CampaignArtInput.cs), [CampaignArtSquadScreen.cs](../Assets/Scripts/Slice/CampaignArtSquadScreen.cs). UI reads eligibility and costs from Sim; it does not manufacture recipes or mark successes.
- **Tests/tutorial/codex:** new deterministic combo/save/replay cases, current regression suites, a demonstration of both attacks and the sacrificed benefit, and discoverable recipe history. Update approved design records only after Jacob's verdict. Character/attack effects require their usual ownership handover; no background dependency is introduced.

## 14. Evidence coverage and limits

Reviewed the current agent instructions; indexed the full handoff history for Pals, roles, talents, favours, combos, duos, sockets and pacts; read the relevant dated owner decisions, their revisions and handovers; read the applicable design-lock sections across the 668-line file; compared the class catalogue, recap, squad synergy paper, nerve/unit-kit paper, combat-fix proposal, earlier item/build research and the preceding Codex recommendation. Examined current class, chain, favour and point-rule source where needed. Earlier research inventories were treated as dated snapshots and not used as proof of current absence.

The review does not claim to have reread every unrelated art/tool log line, inaccessible private discussion or every source linked by old reports. No current native fight was observed. No complete code audit, compile, numerical balance test, independent reviewer approval or Claude acknowledgment is claimed. The paper's state transitions and two fight examples were checked for internal contradictions, but only implementation and owner play can test their feel.

The central risk is a commander UI that makes you babysit every attack. The proposed answer is automatic baseline fighting, explicit commitment of selected combinations, finite windows and meaningful alternatives. That is a testable hypothesis. If the player mostly watches expiry icons instead of the war party, this design has failed even if every recipe works technically.

</details>
