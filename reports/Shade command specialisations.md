# Shade: four commands, eight specialisation routes

Updated 30 October 2026 against the dated Pal sections in [DESIGN_LOCKS.md](../DESIGN_LOCKS.md), through the 29 October **Four Pal fixes from the model**, **four losing combos rethought** and **combos' hooks**. This paper incorporates Jacob's accepted F1-F9 repairs and later audit/road changes. The locks govern any conflict. The original 27 October numerical ranges remain prototype proposals where the owner did not choose a value.

**Status: Shade decisions reconciled; Shade implementation and play validation pending.** Butcher is now implemented in rules 15/save 23, including the rethinks/hooks; the other three redesigned Pals still await their sim handback. The [audit and re-audit](Shade%20routes%20audit%20and%20re-audit.md) preserves the original evidence and records each design resolution. Its old REWORK REQUIRED verdict describes the 27 October proposal, not unanswered handovers. Gate D still requires an authoritative Shade prototype and Jacob's play. This document edit is not a gameplay pass.

## The idea

Shade develops the command itself. Enhance at most TWO of the four buttons, with one route on each, and deepen them through three substantial forms. Each form adds something you can see and use in a fight. The other buttons retain their normal functions. The current run acquires its forms from Shade's road rewards; camp purchases unlock eligibility for later forms rather than equipping them permanently.

The fantasy is an advanced cult commander bending how their war party fights: catching an attack, leaving a false formation, opening a rupture with concentrated blows, or gathering attack echoes for a deliberate release. These are temporary command effects. They do not give a Medic a rifle, replace native tribal equipment or make ordinary class cooperation an unlock.

Butcher supplies augmented class moves. Glutton's universal tree makes existing effects bigger, longer, faster or cheaper. Whisperer supplies habits, pairs, traits and ranks. Shade changes the buttons. The rejected Sequence/shot-reservation design is withdrawn. **Bonds are no longer parked:** the existing chains/bonds are part of the latest rules, and Whisperer's offensive pairs bond their two classes from pickup for the rest of the run. A Shade route never requires that pair to function.

## The eight routes at a glance

Each row is one route, read left to right. Development is cumulative unless the final form explicitly replaces an earlier release.

| Button and route | First form | Developed form | Final form | Continuing price or weakness |
|---|---|---|---|---|
| Parry: **Mirror** | Return an echo of the enemy's final blow. | Catch the earlier clean beats too, strengthening that return. | **Pierce** sends the return through its attacker into one enemy behind them. | A partial or miss forfeits the enhanced return for that sequence. |
| Parry: **Sanctuary** | A clean parry creates brief individual shields across the squad. | **Relay** can help a broken shield from a separate finite shared reserve. | Once per battle, the active field can shatter to prevent one fighter's death. | Give up the normal final-parry stagger while this route is equipped. |
| Fall Back: **Ghost Step** | Leave a false formation that catches one eligible enemy attack. | Press again to return early, ending dodge protection. | **Shatter** bursts a surviving decoy only on manual Recall; natural return leaves it catching hits. | Each activation has a longer lockout before another Fall Back. |
| Fall Back: **Rift Mine** | **Mine** blasts the first enemy who attacks after Fall Back. | **Shock** delays that enemy's next attack. | **Wide Blast** also hits two nearby enemies. | Longer return time; no eligible attack before expiry wastes the mine. |
| Focus Fire: **Rupture** | Squad hits open a weak point; press Focus again to rupture it. | A fully prepared rupture cracks armour for follow-up attacks. | Nearly ready offensive fighters attack early, then repay the borrowed time in recovery. | Preparation belongs to one enemy and is lost when you change target. |
| Focus Fire: **Death Warrant** | Killing the marked enemy passes Focus to another nearby enemy. | The marked death also produces a local blast. | Excess damage from the killing hit carries into the next marked enemy. | A shorter Focus window; no kill means none of these rewards. |
| Unleash: **Breach** | A command shock front hits and pushes the enemy front line. | It opens a brief gap in enemy cover. | Press again to spend the remaining buff on one larger ram. | Your living fighters take more incoming damage during the commitment. |
| Unleash: **Echo** | Each fighter's one echo accumulates a share of every eligible hit during the window. | Press again to release the stored echoes early. | A manual release can converge the echoes on your active Focus target. | Half the normal immediate Unleash bonus; the extra damage is delayed and can be wasted. |

## Progression and changing your mind

**The build comes from the road.** Win a Shade fight and choose one of three eligible button forms instead of that fight's materials. The offer uses routes open at the current tier. A first form comes with the chosen route, without a camp purchase. Picking a route already held grants its next form, subject to its tier gate and permanent pool unlock. Every acquired form ends with the run. No first-form purchase, automatic two-route starting loadout or permanent fully developed enhancement positions remain.

| Gate or purchase | Current rule |
|---|---|
| Tier 1 routes | Sanctuary, Ghost Step, Rupture and Breach can enter Shade's offer. |
| Tier 2 alternatives | Mirror, Rift Mine, Death Warrant and Echo can enter the offer. |
| Second forms | Campaign tier 3 normally; campaign tier 2 in the 8 Dec slice. Camp unlock: 150 materials. |
| Final forms | Campaign tier 5. Camp unlock: 250 materials. |
| Both enhanced buttons | Share these gates. No forced 2/1 then 2/2 slot-capacity schedule. Actual road picks decide which route deepens first. |
| Tier 7, the Sovereign | Still only two enhanced buttons; no third slot. Unlocking a form does not guarantee a run will offer it. |

Camp purchases permanently add eligible content to future road pools. They do not grant that form in the next battle. The earlier 30/60/100 prices, later 75-material first-form price and free permanent milestone development are superseded. The lock gives 150/250 unlock prices but does not specify whether each purchase covers one route or a wider group; do not invent that granularity in a screen.

F8 remains accepted: a newly unlocked route gives one free swap at the next safe screen, never during combat. The sim handback must reconcile that right with road acquisition and define held-route replacement when two buttons are already enhanced. A swap must not manufacture an unoffered form or clone another route's depth. F7 also remains accepted: before a chief encounter, park an enhanced button and use its base form for that fight, removing its entire benefit and drawback. Parking is not a third enhancement slot.

One button cannot carry both routes. Unequipped or parked benefits do not leak into combat. Safe-screen changes, consumed swap rights, held forms and permanent unlocks must be separate saved facts. Reload cannot grant a second pick or swap.

**Glutton's Command Relay now halves one enhanced button's weakness.** Its older one-tier-early gate bypass is withdrawn. The exact application to non-numeric weaknesses needs the authoritative handback; the UI must not choose it. Road acquisition replaces the previous permanent-power risk, but offer scarcity, replacement losses and the cost of unlocking depth still require owner play.

## Parry / Hold

These routes use the same honest parry cues and timing as the base command. Full, partial and missed beats keep their normal defensive results. A full parry still earns its normal single segment, capped at three; neither route grants an extra segment, changes the ultimate price, nor requires spending segments. Early beats never cancel the rest of a multi-hit sequence.

### Mirror: catch the attack and send it back

1. **Return.** A completely clean one-, two- or three-beat sequence ends with one damaging echo aimed at its attacker. It releases only after the final beat. One-beat attacks work on their own.
2. **Catch.** Each successful beat in that same clean sequence contributes to the return. A three-beat attack can produce a stronger counter than a single swing. Nothing is banked between sequences.
3. **Pierce.** The return travels through its original attacker and can hit ONE valid enemy behind them for reduced secondary damage. The lane is previewed; this is not an arena-wide reflection. The old name Fold is withdrawn.

**The risk:** any partial or missed beat invalidates the enhanced return for that sequence. You still receive every full parry's normal mitigation and charge, and normal final-parry stagger when eligible. There is no extra incoming-damage punishment or narrower timing window. You are wagering the offensive payoff, not your ability to defend. This route adds output but loses reliability on long strings and on enemies with no useful secondary target.

The echo is a bounded command attack inspired by the parried blow. It does not copy Eclipse, an enemy summon, an entire spell script, status application or boss phase effect. A successfully parried support beat can still defend/charge normally but contributes no damage payload. If an enemy dies or its sequence is interrupted before completion, the pending return is discarded. If the final attacker is no longer a valid target at release, the return fizzles; it cannot skip directly to an unseen enemy behind them. No overlapping sequence combines its charges with another.

**See it:** one mirrored outline catches each clean beat; it fractures on a partial/miss. On the final beat the collected shape fires back. The existing parry cue remains dominant. No second player meter.

### Sanctuary: turn a parry into a rescue window

1. **Shelter.** The first eligible full parry projects one finite shield onto each living ally, including the leader. All expire together after a brief window. They absorb damage after normal mitigation; they neither heal nor cleanse.
2. **Relay.** The field also creates one small, finite shared reserve. When a hit exhausts a fighter's personal shield, this reserve can help absorb the rest before it reaches HP. It never borrows another fighter's shield. The reserve expires with the field, cannot refill, and needs an authored budget and deterministic simultaneous-hit order.
3. **Last refuge.** Once per battle, if a hit would still kill an ally while the field is active, every remaining shield shatters to leave that one fighter at 1 HP. The field ends. Further hits, including a later beat of the same combo, can kill them. This is not a revive or temporary invulnerability.

**The price:** while Sanctuary is equipped, your parries do not produce the normal final-parry stagger. Full damage negation and earned segments remain, and a Sanctuary parry still counts as parried for eligible effects. Only the stagger is traded away. Show this in the selection preview.

A field cannot stack, refill or extend from additional parries. Its grant lockout lasts beyond the shield window, including when the shields are broken immediately. Base parries remain available during that lockout. The death-save allowance does not refresh with the next field. If simultaneous lethal hits arrive, authoritative event ordering determines which one is saved; the UI cannot choose afterward. A fighter revived after the initial grant receives no retroactive shield.

**See it:** a separate shell around each fighter; a distinct finite reserve visibly helping a broken shell; all shells break in the single rescue. Never draw protection being stolen from another fighter. Keep the normal health bars legible.

## Fall Back

Base dodge timing, attack coverage and known counterattacks remain authoritative. Neither route dodges a whole multi-beat sequence automatically, redirects an area attack, nor grants firing while withdrawn. A second press, where available, is a follow-up to the existing activation, not another paid dodge.

### Ghost Step: make them strike a false formation

1. **Decoy.** Withdraw normally and leave an astral outline at the old line. For a short lifetime, it can catch ONE newly committed, redirectable ordinary single-target enemy attack that would otherwise aim at a living ally. A shot already avoided by the original dodge does not count as this extra catch.
2. **Recall.** After the initial dodge movement, press Fall Back again to return early. This immediately ends any remaining dodge protection; it cannot retroactively avoid a hit. The squad resumes through its ordinary action rules, with no restored aim or class readiness.
3. **Shatter.** Only manual Recall bursts the surviving decoy once. A natural return leaves it catching eligible hits for the remainder of its original lifetime, without a free burst. A consumed or expired decoy cannot explode. The old name Collapse is withdrawn.

**The price:** the next Fall Back becomes available two combat seconds later than its ordinary earliest reuse time for this activation, calculated on the original press and not shortened by Recall. This is a proposed starting delay, including when the base command has no separate cooldown. An early return buys attack time but risks meeting the enemy's next beat. Base Parry is always available when ordinarily eligible. The initial dodge window is not narrowed.

One decoy at a time. It cannot taunt a boss, redirect a named marked attack, catch area damage or erase a hit on someone already trapped by an authored mechanic. Those exceptions must be visible before relying on it. Its timer continues after it catches nothing; waiting out its lifetime gives no blast. It is a command projection, not a new unit with an equipment loadout or enemy AI priority.

**See it:** the actual retreat and an unmistakably translucent formation left behind. The button shows a return arrow only when Recall is legal. Enemy attacks visibly hit the projection if it catches them.

### Rift Mine: punish enemies for following you

1. **Mine.** Fall Back leaves one short-lived mine. The first enemy who attacks afterward triggers its blast. It responds to an authoritative attack event, not sprite movement or a crossing test. With no eligible event before expiry, it is wasted.
2. **Shock.** The blast pushes the triggering enemy: this means delaying that enemy's next attack. It does not move its logical row/lane, tether it or add a Snare mark.
3. **Wide Blast.** The same blast also hits two nearby eligible enemies, for at most three targets including the trigger. Nearby means the same row and the next lane. There is no pull, extra trigger or per-victim explosion.

**The price:** longer post-dodge return time at every form. Your fighters give up attack time even if no enemy attacks before expiry. The initial evade window and protection do not get longer as compensation. There is no stored mine to carry into the next encounter.

The mine cannot retroactively catch the attack that caused the retreat. One armed mine per side; another accepted placement replaces the old one without exploding it. No general movement system is introduced. Boss delay/attack-event eligibility must be explicit in the sim handback; do not infer immunity or immunity bypass from the art. A blast never breaks a cage, phase gate or chief protection unless that separate authored rule allows it.

**See it:** a clear astral mine, the attacking enemy triggering one blast, and a visible delay in its next windup. Wide Blast reaches its two previewed neighbours. Animation illustrates the rule; fighter sprite positions never decide it.

## Focus Fire

Target selection, normal attack range and real source readiness still matter. Selecting someone is not itself a hit or guaranteed interruption. All enhanced damage respects target eligibility and ordinary mitigation. Neither route affects a kneeling chief as a kill opportunity.

### Rupture: prepare one enemy, choose the moment

1. **Weak point.** Focus marks one enemy. Up to three ordinary squad attack actions that actually damage it open the mark further. Press Focus again on that same target to rupture the accumulated preparation; one or two hits give a smaller result. With zero preparation the release is rejected visibly, spending nothing.
2. **Crack.** A fully prepared, three-hit rupture also breaks a portion of ordinary armour for a short follow-up window. It does not remove immunity, barriers or scripted protection. A target with no armour gains no armour-break benefit.
3. **Finish.** A fully prepared rupture brings eligible, nearly ready offensive attacks forward. Each is a real attack with normal targeting, travel/aim and hit checks, then waits that much longer after. It borrows time rather than finding a bank of idle ready attacks. Already committed, interrupted, out-of-range or too-distant actions do not join; support units do not acquire an offensive spell. The accepted near-ready threshold and action/cycle identifiers must come from the sim.

**The price:** commit to one target. Changing Focus targets, expiry or target death discards the preparation. Cashing it ends that Focus activation. Retreat does not erase the mark, but its timer keeps running and withdrawn fighters cannot build it. An early rupture trades future payoff for immediate damage.

One attack action contributes at most one preparation step, regardless of projectile count or targets hit. This is Rupture's local preparation, not a new class-chain Opening mark. Damage-over-time ticks, Mirror returns, Death Warrant blasts, Echo replays and other command-generated damage cannot build it. The final volley cannot reopen the just-consumed mark. A follow-up rupture spends that activation's stored payload once and never runs ordinary Focus hooks again. A new target/new activation pays the normal cost and cannot bypass ordinary Focus availability.

Rupture may pull an eligible action forward only once per spell cycle, and a pulled spell pulls nothing else. For example, with a normal 60-tick interval and 12 ticks left, an accepted early Rupture action now must leave its following due time at tick 72, not tick 60. This illustrates conserved recovery, not a chosen 12-tick eligibility window. **Whisperer's later pair answer is different:** it is free, leaves the fighter's own spell clock running and answers at most once per own-spell cycle, at least 1.5 seconds apart. The cycle/non-recursion limit remains shared, but the pair does not inherit Rupture's recovery debt.

**See it:** one crack visibly widens on the selected enemy, then flares when fully prepared. Focus shows a detonation symbol while release is possible. Final-form participants light briefly, then visibly use their own weapons. This is not the withdrawn Sequence design: no class pair is required and no future shot is reserved.

### Death Warrant: carry the attack through a crowd

1. **Transfer.** If allied damage kills the Focus target while this activation is live, its remaining Focus time transfers to one nearby valid enemy. Preview that next enemy. No target in range means the order ends.
2. **Death burst.** A qualifying marked kill also produces one short-range blast around the victim. It can damage nearby valid enemies but grants no new Focus time.
3. **Carry the blow.** The unused portion of the actual killing hit can pass as a bounded second impact to the next marked enemy. The new target applies its own defences. A huge boss-health value is never used as the damage source.

**The price:** this route begins with a shorter Focus duration than the base command. It has no free refresh on kill. Without a timely kill, there is no transfer, burst or carried impact. It is a poor specialist choice against a lone durable chief; ordinary Focus targeting remains useful.

At most two transfers, three marked victims total, in one activation. Only one target is focused at a time. Resolve a qualifying kill by selecting the next mark, applying the old victim's burst, then applying any carried impact if that next target is still valid. A death burst or carried impact can kill, but cannot trigger another transfer, death burst or carried impact. If that extra damage kills the newly marked enemy, the order ends; unused carried damage is discarded. Explicitly selecting another target cancels the old activation and pays for a new one normally. Enemy suicide, scripted despawn, breaking a beast part and a chief kneeling do not count as a qualifying allied kill.

**See it:** the mark visibly flies to the previewed next enemy; a compact outward pulse leaves the first victim. The overkill impact has one clear source and destination. No stack of kill banners over the fighters.

## Unleash

Normal resource checks and existing one-time interactions with authored obstacles remain. An added release never repeats those obstacle interactions or resets class cooldowns. No Unleash route produces parry segments or acts as an inherited chief ultimate.

### Breach: open the line, then drive through

1. **Impact.** Accepted Unleash sends a short command shock front into the nearby enemy front line, damaging up to three eligible targets and delaying their next attacks. That delay is the locked meaning of push; it changes no logical row/lane. Its normal offensive window then continues. The leader is not physically moved in front of his protectors.
2. **Gap.** Affected enemy cover providers cannot cover other enemies for a brief interval. Their own defence remains. This does not strip immunity or an authored wall from a chief; the existing legitimate wall answer still resolves once on initial activation.
3. **Ram.** Press Unleash again while a meaningful part of its active window remains to end that buff and release one larger ram into the same forward area. More unspent duration means a stronger ram. It is a new damage pulse, not another application of the initial obstacle interaction or a renewal of the cover gap.

**The price:** during the original active window and a brief recovery afterward, allied HP damage taken is increased. Releasing Ram or issuing Fall Back does not end this exposure early. Dodging or correctly parrying still avoids eligible damage; the route never prevents a defensive input. The price applies to damage that actually gets through, not extra unavoidable HP loss.

There is one Impact and at most one Ram per activation. A tiny residual buff cannot purchase a full-strength Ram. Ram ends the remaining attack buff; allies cannot retain its full duration and also receive the cash-out. No stacking/restart while this activation or its exposure recovery remains live. Boss delay/cover eligibility remains authored; no phase is bypassed by moving presentation sprites.

**See it:** a single force front travels from the squad, opening a visible gap. The second press compresses the remaining glow into the ram. Exposure is communicated on the Unleash button, not by covering every fighter with text.

### Echo: prepare a second strike, decide where it lands

1. **Record.** During the original Unleash window, each living offensive fighter's one visible echo stores a share of every eligible hit, rather than only its first hit. At the end the accumulated echo repeats against its original eligible target. The real fighter continues normal combat; no future action is held for the recording. Exact eligible-hit categories, per-target storage and caps belong in the authoritative handback.
2. **Release.** Press Unleash again to replay the available echoes immediately and end the remaining buff. Wait for more fighters to contribute or release before a dangerous target escapes. There is no new release button.
3. **Converge.** On a manual release, if there is a live, legal active Focus target, all available echoes can converge on it instead of their original victims. The ordinary, unenhanced Focus button is enough; a second enhanced button is not required. With no legal Focus target, they replay to their original targets as before. The release preview must show which outcome will happen.

**The price:** halve the ordinary immediate Unleash bonus at all forms. If base Unleash adds 50 percent damage, this would add 25 percent, not halve total damage. The eventual echoes are the compensation. Waiting can lose targets; a stunned or dead fighter may never record an attack at all. This is weaker when immediate damage is needed before a target's next action.

Each fighter has at most one visible stored echo per activation, accumulating eligible offensive damage from that activation's original window. Heals, Guard, ultimates and generated command damage do not feed it. Echoes do not carry status procs, create ordinary chain marks, refill resources or trigger one another. An already recorded echo is a command projection and may release after its fighter falls; it does not revive or give that fighter another real action. Dead/untargetable original victims waste their stored contribution unless a legal Converge redirects it. No automatic retargeting to a chief behind protection. A manual release records the previewed mode and target: if that target becomes invalid before input acceptance, reject the Converge without spending or silently switching modes; after a committed release, invalid impacts are lost. With no stored payload, a manual release is unavailable. Early release cannot renew the original recording/reuse budget.

**See it:** one translucent weapon trace appears for each recording fighter. On release, the traces visibly repeat their paths or converge. Native shots and swings supply the visual vocabulary; no new generic gun replaces the tribe's kit.

## What two selected routes feel like

- **Rupture + Breach:** expose a defended enemy, build the weak point, then choose a real volley or an early ram. Strong single-target commitment, dangerous while exposed. Giving up Sanctuary and Ghost Step has a visible cost.
- **Rift Mine + Death Warrant:** retreat, punish the next eligible enemy attack with Wide Blast, then exploit a qualifying kill to carry the attack through nearby enemies. It creates no physical grouping; no attacks before mine expiry and no timely kill waste the effects. Neither enhancement requires a parry.
- **Sanctuary + Echo:** use a clean defence to protect the squad while you prepare a delayed burst. Safer against chip damage, but gives up parry stagger and immediate Unleash output; missed timing or no shield coverage still hurts.

These are interactions between ordinary effects. No special pair bonus, bond level, combo recipe unlock or third enhancement is added. Six possible button pairs and four route combinations per pair provide 24 fully unlocked pair/route configurations; that count does not establish 24 equally useful builds.

## One fight, with two enhancements

**Authored illustration, not a replay or balance result.** A developed squad meets a late-run patrol: an advancing melee enemy, a second frontline enemy and a ranged enemy. Equip final-form Rift Mine and Rupture. Parry and Unleash are ordinary. Assume legal targets, available command resources, susceptible patrol enemies and actual attack readiness where described.

1. Focus the advancing melee enemy. Normal squad hits begin opening its weak point. Nobody waits for a parry to start fighting.
2. It attacks three times. Parry the first beat, partially block the second, and parry the last. The charge ledger is 0 -> 1 -> 1 -> 2. Damage from the partial block remains; the Medic still has a real job. No Mirror or Sanctuary occurs because neither is equipped.
3. It later starts a covered rushing attack. Fall Back to dodge that attack and leave the mine. Charge stays at two. The mine does not catch this already-committed rush retroactively.
4. The first subsequent eligible enemy attack triggers the mine. Shock delays that enemy's next attack, and Wide Blast also hits two eligible neighbours in the same row/adjacent lane. Nobody crosses a simulated zone or gets pulled into a group. Your longer return time has cost attacking time; the Focus timer has continued to run.
5. After returning, a real third attack completes the still-live Rupture preparation. Press Focus on that target again. The rupture cracks ordinary armour; eligible nearly ready offensive fighters borrow their next real attacks and repay the time afterward. The Medic does not fire a rifle, a shot already in flight is not duplicated, and no new chain mark is invented. Finisher and pair responses obey the shared cycle/non-recursion limit; a free pair answer does not spend that fighter's next normal spell. If the Focus window expired during retreat, this prepared rupture is unavailable.
6. Use ordinary Unleash if the grouped enemies and current threat make its normal payoff worthwhile. It is not Breach or Echo. This choice must remain meaningful even though it is one of the two unenhanced buttons.
7. A later genuine parry can fill segment three. If an inherited chief move was legitimately acquired earlier in this run, choose whether to spend the three segments on its valid release. Held bars give Butcher combos +10 percent each, up to +30 percent; spending them removes that temporary boost. Shade creates neither the chief move nor an extra segment and does not add that combo multiplier to every command effect.

Victory is not guaranteed by this script. A dead target wastes Rupture preparation; no eligible attack before expiry wastes the mine; delaying the rupture can lose the window; inability to safely issue Unleash can make holding it wiser. This illustration assumes the mark stays live long enough; it is not a timing guarantee. Those are decisions to evaluate in a prototype.

## Shared rules and bounded starting parameters

The following are **provisional prototype values/ranges only**. They are not balance changes, measured results or authorisation for tuning. All time is simulation time. Learning-speed presentation must stretch cues and motion together. Use integer ticks and fixed-point arithmetic in any eventual authoritative implementation.

| Knob | Candidate / bounded exploration | Purpose |
|---|---|---|
| Sanctuary shield / window / grant lockout | 15-25 percent max HP per ally; 3-4 s; 8-12 s | Brief rescue coverage, not continuous shielding. Lockout never shorter than window. |
| Sanctuary Relay | Finite separate reserve; authored size and stable recipient order still needed | Never drain another fighter's personal shield or refill the reserve. |
| Ghost Step decoy lifetime / additional reuse delay | 3-4 s; +2 s beyond the ordinary earliest reuse time | A readable bait with a real gap before reuse even if base cooldown is zero. Recall cannot shorten it. |
| Rift Mine lifetime / Shock delay / return | 4-6 s; delay amount not chosen; 1.5x base return | Punishes the first eligible attack after retreat without extending evade protection. |
| Rupture preparation / armour crack | Cap 3 attack actions; 30-50 percent ordinary armour for 2-3 s | Clear buildup and follow-through; no mandatory perfect-parry dependency. |
| Death Warrant duration / transfers | 60-75 percent base Focus; at most 2 transfers | A short kill opportunity, not permanent targeting or an infinite cascade. |
| Breach exposure / cover gap / post-window recovery | +15-25 percent incoming HP damage; 1-2 s gap; 1-2 s recovery | Committed offence with available defensive answers. Exposure lasts to original buff end plus recovery. |
| Echo record / fraction | Share of every eligible hit in the original window, one echo per fighter; former 30-60 percent range needs re-evaluation | No fixed first-hit budget that penalises faster fighters. A storage cap cannot leave an unpriced ongoing penalty. |

Damage coefficients and areas for Mirror, Shatter, mine, Rupture, Death Warrant and Breach need actual squad/encounter measurements. No arbitrary final damage table is asserted here. Define the coefficients and displayed areas together; a paper route is not ready to implement with missing numbers silently guessed in UI.

Structural formulas and ordering:

- **Rupture:** preparation h = min(3, eligible attack actions during the live mark). Raw rupture = floor(K_r * h / 3), where K_r is an authored positive per-route damage value for the appropriate content band. Only h = 3 enables the armour crack and final-form volley. Apply normal target defence to the rupture first; then apply the crack for subsequent eligible attacks. It does not increase its own preceding damage.
- **Mirror:** let C be the authored cap for a captured beat, and p_i that beat's eligible raw offensive payload. A clean first-form sequence returns a coefficient times min(C, p_final). The developed form uses the sum of min(C, p_i) across at most three beats. Scale once, then apply each victim's defence. Secondary line damage is a fraction of the primary raw return. C and coefficients must prevent boss payloads from becoming boss-health-sized counters. Utility-only payloads are zero.
- **Echo:** the candidate accumulation is stored raw payload per fighter = sum(floor(q * eligible hit raw damage)) over the original activation window, 0 < q < 1. That replaces one recorded action. Capture each realised hit's offensive amount before its original victim's mitigation; do not apply the old victim's armour twice. Echoes receive no second Unleash multiplier. New victims still apply their own defences. The exact fixed-point rounding/cap and handling of hits on several original targets remain implementation contracts, not settled coefficients.
- **Death Warrant:** excess = max(0, resolved HP-directed damage of the killing hit - victim HP immediately before that hit). Carried raw impact = min(authored cap, floor(c * excess)), 0 < c <= 1; the new target then mitigates it. This deliberately cannot reconstruct damage absorbed by the old victim's armour/shield. Burst and carried damage carry a no-cascade flag.
- **Ram:** raw ram = K_b * remaining buff ticks / original buff ticks, with fixed integer rounding and a minimum accepted remaining window. K_b is bounded authored damage. No remaining duration means no legal release. Exposure's end time remains the original end plus recovery.
- **Damage interaction:** applicable source bonuses are resolved once. Breach exposure applies once to incoming HP-directed damage before Sanctuary shields absorb it. Shield overflow reaches HP. Last refuge checks the resulting lethal hit once. Extra command effects do not produce parry segments, duplicate command-refund events or trigger themselves. Ordinary eligible real attacks still use the core's normal rules.

For all eight routes, new activations pay the ordinary command cost and availability requirements. Included second presses (Recall, Rupture release, Ram, Echo release) consume the stored payload of their parent activation once, never pay another base activation cost, re-arm the first effect or fire normal command hooks again. Parent identity, original deadline, consumed state and the exact earliest new activation must be explicit in the sim/save contract; the owner chose the once-only rule but not every timer. Their alternate button state must be obvious. A rejected input spends nothing and shows why; ordinary defensive input cannot be invisibly denied by an enhancement timer. Route follow-ups are disabled while paused in the same way as any live-timing action; pause cannot store a valid parry or manufacture a beat.

Cross-Pal constraints: only the parry bars and Shade's own routes directly reward a parry; Execution Corridor triggers when a strike cashes an Opening, not on parry. Pins/freezes do not generally create Snare marks, with the deliberate **Singularity Net exception**: the Trapper snares dragged neighbours too, without extra Bleed. The latest Butcher hooks leave/cash Snare, Opening and Bleed; they are authoritative rules-15 effects, not new Shade procs. Heavy is now 1.25 times the spell clock, and Unnerving/Borrowed time cost an eighth of nerve/health, superseding the earlier quarter pacts. Rank 2's extra offensive hit remains plain. Target priority is Hook drag, Focus Fire, combo target, habit, pair, then plain, with Around the Corner retaining Focus/Snare before healer fallback. These belong to the sim, not frontend patches.

Whisperer's latest Mend and Hold Guards both the patient and Medic; do not retain the old lost-Guard catch. Gatekeeper is excluded from slice road picks, and Ambusher waits to stop an un-parried heavy; a successfully parried heavy does not spring it. The free pair answer, habits and ranks remain designed rather than implemented. The proposal for a different Pal on every fight is held while the gear/strip/bounty consequence is decided; no Shade screen should assume that new road layout.

No hidden damage reaches a spared/kneeling chief, target outside the encounter, despawned unit or invalid boss phase. Pending damage and queued real finishers must recheck eligibility at impact. Multi-target effects use stable target ordering and explicit maximum counts. A new enhancement never changes an old replay's command semantics.

## Presentation and teaching

At a Shade reward, show the three actual eligible forms, the button/route each changes, its current-run depth and its tradeoff. Preview **one action on actual fighters**, followed by what is lost. Show both enhanced positions clearly. At camp, label later-form purchases as permanent road-pool unlocks, never active upgrades. Do not revive the rejected eight-card art mockup or generated instruction poster as approved visual direction.

During combat, keep the same four command locations and the inherited-ultimate control. Use the existing command icon plus one clear changed state: recall, rupture, ram or echo release. A target mark, projection or weapon trace carries local preparation; no new full-width progress bars. The three-segment parry meter remains recognisable and separate. Text is for a first explanation or an optional detail view, not continuous narration over the battle.

Feedback must tell apart input accepted, effect waiting, actual effect landed and effect lost. For example, a prepared Rupture may have no eligible nearly ready fighters for its finish; preview the actual participants instead of promising a full squad attack. A wrong-timing parry cannot be drawn as successful just because Sanctuary absorbed the resulting damage.

## What Claude needs before implementation

This is the reconciled Shade paper. The 28 October owner locks supersede the earlier Ward/Purge/Expose/Overdrive ranks, the withdrawn Sequence reservation, permanent milestone capacities and camp form ownership. Butcher, Glutton and Whisperer are specified in the same dated lock sections and in the [current Four Pals revision](Four%20Pals%20talents%20and%20commanded%20combos%20-%20design%20paper.md). No new owner decisions are created by this edit.

Claude owns the authoritative sim implementation. Hand back sequence identity/completion; accepted full/partial outcomes; command costs and follow-up states; shared-reserve and lethal-event ordering; attack-event/decoy eligibility; Rupture borrowed-action debt versus free pair-answer cycle IDs; cover eligibility; Echo payload recording; chief-safe cancellation; Shade's seeded offers/recipients/replacements; separate pool unlock and run-form persistence; and rule-version compatibility. Butcher's offer/equip/persistence handback already exists and must not be described as pending. Codex consumes authoritative states for screens, feedback, input, saves on screen and tests. No frontend reconstruction of missing rules.

Affected future areas: battle command/state/event definitions and snapshots; campaign unlock/save schema; replay rules and archived loadouts; HUD/button states and target previews; tutorial/codex copy; effect and animation requests; class/gear interaction tests; content-band damage/area tables. Changes to these belong to an implementation sprint after design review, not to this paper delivery.

Acceptance cases for that future sprint:

1. All four base commands remain usable; a third enhancement is rejected; an alternative replaces its route; no unequipped benefits or drawbacks survive.
2. Permanent pool unlocks survive defeat; acquired forms do not. Seeded offers, the selected recipient, held depth, pending swap and consumed pick survive save/resume without duplication. Camp purchases do not silently equip forms or change historical replays.
3. Correct/partial/correct in a three-beat attack gives two segments and no Mirror return. Sanctuary absorbs damage without falsely counting it as a parry. Neither cancels early beats.
4. Sanctuary Relay uses only its finite shared reserve and never drains personal shields. Grant lockout survives shields breaking; only one lethal event is saved per battle; a subsequent hit can kill. A Sanctuary parry still counts as parried.
5. Ghost Step catches at most one eligible new single-target attack; area/boss-marked attacks stay honest; Recall cannot renew evade time or reset the longer lockout.
6. Rift Mine triggers once from the first eligible post-Fall-Back attack event, can expire unused, delays the next attack at Shock and hits no more than two neighbours at Wide Blast. No sprite crossing, tether or physical pull controls gameplay.
7. Rupture counts attack actions, not pellets/DoTs/echoes; retarget/expiry removes preparation; nearly ready real attacks repay borrowed time. Shared cycle/non-recursion checks also respect the later free once-per-cycle pair answer with its 1.5-second separation and unspent normal clock. Kneeling cancels lethal follow-through.
8. Death Warrant has one live target, at most two transfers and no time refresh. Extra burst/carried damage cannot create a cascade or make kneeling count as killing.
9. Breach repeats no obstacle utility on Ram; ending its buff does not remove exposure; normal dodge/parry answers remain available.
10. Echo accumulates a share of every eligible hit within one original window and one visible echo per fighter. It applies each target's defence once and no copied procs; Converge works with unenhanced Focus and previews target changes honestly. Early release cannot reset recording eligibility.
11. Old replays retain the old costs, targets, effects and timing. New replays and save/resume preserve every relevant enhancement timer, target, payload and consumed allowance deterministically.
12. In owner play, each route changes an observable decision. Include a fight using two non-Parry enhancements, a lone chief with the parking choice, a crowd, a weak squad and an actual road-developed pair. Headless correctness would not prove fun or clarity.

Remaining validation risks: perfect-sequence Mirror encouraging excessive parry attention; Sanctuary pressure and shared-reserve budget; boss attack/delay eligibility; Rupture borrowing too much tempo despite conserved recovery; Death Warrant against no-add chiefs and the visibility of parking; Breach plus Rupture burst; Echo storage/penalty balance; and road offers starving a chosen build. Command Relay's treatment of categorical weaknesses, safe replacement at the two-button cap and unlock-cost granularity need explicit handback. None is silently decided in UI. These require implementation evidence and owner play, not another numerical promise.

## Research and evidence

Primary sources checked 27 October 2026:

- [Last Epoch: Skill Specialization](https://support.lastepoch.com/hc/en-us/articles/46363203944859-Skill-Specialization) describes specialising known skills, investing in their trees and changing specialisation. We borrow dedicated development of familiar commands, not its five slots, twenty levels or re-levelling rules.
- [Last Epoch: Sentinel Skill Tree](https://support.lastepoch.com/hc/en-us/articles/46362203855515-Sentinel-Skill-Tree) documents Javelin becoming a raining barrage, and a Battle Standard variant that adds a dash follow-up with a longer cooldown. This is the useful principle: changed delivery, a new decision on the same skill and an explicit cost. It does not validate any KS mechanic or number above.
- [ESO: ability morphs](https://help.elderscrollsonline.com/app/answers/detail/a_id/3902/) describes selecting a developed variation of an existing skill. We retain mutually exclusive routes; we do not import use-grinding or its currency.

The Last Epoch promotional skills page and newer support tree disagree on an example's projectile count. No such count is copied into this design. Research supports the design pattern, not claims that these routes will be balanced or commercially successful. No reference-game playtest was performed for this paper.
