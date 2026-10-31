# Squad synergy and the Hold load (v2)

Claude's paper, revised 25 Oct 2026 against Codex's audit (HANDOFF_LOG line 11117). Jacob: "Yes go revise it". Nothing here is built or locked. Every number is a starting value, and your play is the gate.

## What changed from v1

- **The order.** Squad builds come first. Changing Hold becomes a separate test afterwards (audit point 1).
- **Teamwork before any bond.** A new base rule makes a fresh squad's chain work (point 2).
- **Exact limits.** Both the base rule and bonds get them (point 3).
- **Your call on the bond rule.** Your "same enemy" wording is now a question for you, with numbers (point 4).
- **Formation rules.** Exact neighbour rules, triage and a rearm rule (point 5).
- **A proof plan you can watch** (point 6).
- **Facts fixed.** The Marksman aims in half the time at a snared enemy, Quickdraw's aim can't be broken, and a small parry can miss its timing.

## The goal

Two squad builds you can tell apart, picked before the fight. Their payoffs fire on their own and you can see them, and your orders redirect, boost or rescue them. Hold stays for the big beats.

Northgard, Warpack and Time Walker all work this way. Time Walker's real hook is skill sets you assign across heroes. Your class rule bans that, so our version is gear bends.

## What the chain does today

I measured 18 rules 9 road fights against Bloodridge with the reference bot. This was a scratch probe, and no game code changed.

| Mark | Made | Cashed | Ran out | Target fell |
|---|---|---|---|---|
| Snare (Trapper) | 58 | 11 | 47 | 0 |
| Bleed (Trapper) | 58 | 26 | 32 | 0 |
| Opening (Marksman) | 47 | 11 | 33 | 3 |

Why: the snare holds 2 s, but the Aimed Shot comes only every 5 s. The Opening lasts 3 s, but the strike comes every 4 s, so most marks die waiting for a clock. In the five duels, no pair landed a single combo.

### A real first fight today, unbonded

This was the first war party of one run, won at 21.8 s.
- 0.5 s: the trap springs on raider 1, leaving a snare and a bleed. The snare runs out unshot. The Skirmisher rips the bleed at 2.1 s.
- 4.2 s: the Marksman leaves an Opening on raider 5. The Skirmisher strikes it at 6.1 s.
- 5.8 s and 10.7 s: two more snares and bleeds on raider 3. All four run out.
- 15.8 s: snare and bleed on raider 0. The snare runs out, and the bleed is ripped at 18.1 s.
- 18.4 s: an Opening on raider 0 runs out.
- 21.5 s: a last snare and bleed. The fight ends before anyone cashes them.

Totals: 5 snares and none shot, 5 bleeds and 2 ripped, 2 Openings and 1 struck.

What the base rule below should change here, on paper and not measured: the Marksman starts with his clock half run, so the 0.5 s snare would pull him. He'd crit by about 1.5 s and leave an Opening, and the Skirmisher would be pulled to strike it. Every later snare that comes while his clock is half run gets the same answer.

## Part 1: teamwork before any bond

### The base rule: a fresh mark readies its casher (my proposal)

When one of your fighters leaves a mark, the fighter who cashes it gets their spell early enough to use it while the mark is still on.

- **Who.** The first fighter in your squad order who cashes that mark and can reach the enemy. A Marksman cashes a snare. A Skirmisher cashes an Opening, then a bleed.
- **How early.** Early enough to land inside the mark. The Marksman is ready within 1 s of a snare, since his aim at a snared enemy takes 0.75 s. The Skirmisher is ready within 2 s of an Opening and 4 s of a bleed.
- **Limit.** A spell can only be pulled once its clock is at least half run. So the shot comes at most every 2.5 s and the strike every 2 s, and only when marks are there.
- **Not when** the casher is snared, held, panicking, already aiming, or can't reach, like a melee fighter during Fall Back.
- **Your order wins.** If you Focus Fire someone, cashers go there first, as now. The pull only helps when the Focus target carries the mark.
- **A mark pays once.** Each mark pays its point and its 40 nerve once, whoever cashes it. Today a snare can pay twice, if two Marksmen crit it.
- **Order.** Marks resolve in the order they happen. On one tick, fighters act in squad order, and each fighter answers one mark at a time.

Risk, reward, punish:
- **Reward:** the chain you already have, firing far more often. Points and nerve from marks start to matter.
- **Risk:** shots and strikes go where the marks are, not always where you want them. Focus Fire fixes that, for a point.
- **Punish, for you:** lose the Trapper and the chain has no start. Ranged enemies never spring her trap.
- **Punish, for him:** walking into your trap costs him a crit and an Opening.

### Kin bonds (#4), on top

Your lock: "two fighters who land a combo on the same enemy three times in one fight are bonded for the run. A bonded Marksman fires at whatever his Trapper snares without aiming."

A combo between two fighters is one mark and its cash. That means a snare and the crit, an Opening and the strike, or a bleed and the rip. A Retaliator Warden's Opening counts too.

**Your call: same enemy, or any enemies?** In the 18 fights above:
- one pair landed three combos on the very same enemy in 1 fight;
- one pair landed three combos in a fight, on any enemies, in 6.

As written, bonds would be rare. My pick is any enemies, so a bond comes every few fights. The base rule will raise both numbers, but only a build can show by how much. The lock stays as it is until you choose.

What a bond does (my reading of "without aiming"):
- The bonded casher answers every one of the partner's marks, with no half clock limit.
- A bonded Marksman's answer needs no aim, so it can't be broken. It still uses his spell, and his clock starts again after it.
- A bonded Skirmisher never aims anyway, so her bond only removes the half clock limit.
- Bends keep their damage. Deadeye still hits half again as hard, and Quickdraw a quarter lighter. So Deadeye wants a bond, because its slow aim goes, and Quickdraw is better without one. That's a deliberate build choice. If you'd rather keep the aim, the other way is a bonded shot that still aims but can't be broken.
- The base rule's limits still apply: one answer at a time, the reach and disabled checks, your Focus Fire first, and a mark pays once.
- A bond ends when either fighter falls or is captured. A replacement starts unbonded.

Risk, reward, punish:
- **Reward:** a combo you can count on for the whole run.
- **Risk:** you have to point combos at shared enemies and keep both fighters alive.
- **Punish, for you:** lose one fighter and you lose the bond. The hunters learn your most used mark and shrug it off (your 20 Oct lock), and a bond makes one mark your most used.
- **Punish, for him:** every snare he walks into becomes a crit.

## Part 2: two builds from the same five

Every run starts with the Clarth's five: one Warden, one Marksman, one Skirmisher, one Trapper and one Medic. So the builds split along the road, by which pieces you strip after wins, which tribe you capture, and how you stand them.

### Build A: Pin and Burst (offence)

- **Stand:** Warden and Skirmisher in front; Trapper, Marksman and Medic behind.
- **Take:** Long Snare for the Trapper, since a snare that holds twice as long always fits the shot. Deadeye for the Marksman. Opportunist for the Skirmisher, so she hits an Opening three times as hard.
- **Capture first:** Bloodridge. Their band brings Skirmishers and Marksmen. Two of them give +10% damage each, and three bring their rally.
- **How it plays:** a raider hits your front and springs the trap, leaving a snare and a bleed. The Marksman is readied, aims 0.75 s, crits (+1 point, 40 nerve) and leaves an Opening. The Skirmisher is readied, hits the Opening twice as hard (+1, 40), then rips the bleed (+1, 40).
- **Your orders:** Focus Fire puts the crit and the strike on one target. Unleash right after a crit for burst. Hold only for big beats.
- **Why choose it:** short fights. Nerve breaks from cashes, and points come back for orders.
- **Weakness:** casters and archers never spring the trap, so the chain can only start from Aimed Shots. Healing is thin and the back row is soft.

### Build B: Wall and Mend (defence)

- **Stand:** Warden and Skirmisher in front; Medic, Trapper and Marksman behind.
- **Take:** Retaliator for the Warden, so whoever hits his Guard is left open. Surgeon or Field Kit for the Medic. Long Snare for the Trapper, to hold attackers longer.
- **Capture first:** Skullpeak. Two give +10% health each, and three bring their hunt of the most hurt.
- **How it plays:** swings at your Medic land on the Warden's Guard instead, 30% lighter. Retaliator opens the attacker, and the Skirmisher is readied and strikes the Opening. Under the Guard, the Medic heals half again as much. Most band swings don't need a Hold at all.
- **Your orders:** Hold for big beats. Fall Back for casts you can't time. Focus Fire to break a caster.
- **Why choose it:** long fights like Hold the Line and chiefs, and a thin roster, since your roster is your lives.
- **Weakness:** slow kills, so you face more of his big beats. Blasts still need Hold or Fall Back, and if you lose the Warden, the wall goes.

## Part 3: formation (#6), with exact rules

Your lock: "some pieces help whoever stands next to the wearer. Fall Back splits the lines and the bonus drops until they are back together."

- **Neighbours:** the fighters to your left and right in your row, in the order the squad page sets.
- **Gaps:** when a fighter falls, the row closes up, so your neighbours are the nearest living fighters on each side.
- **Fall Back** breaks every link while it runs (2.5 s), as your lock says. The links come back when it ends.
- **A Hook drag or the bear's Grab** breaks only the pulled fighter's links, and they come back when the pull ends.
- **Stacking:** a fighter gets each kind of piece once, so two Shield Walls beside one fighter don't stack. Different pieces do. Damage cuts multiply with Hold's soak and the Warden's Guard.

The pieces, as placeholders:

| Class | Piece | What the fighter beside the wearer gets |
|---|---|---|
| Warden | Shield Wall | takes a third less from heavy swings |
| Medic | Close Care | is treated first when below 90% health, unless another ally is below half |
| Trapper | Tripwire | the first melee enemy to hit them is snared; this rearms every 5 s, apart from her own trap |
| Marksman | Spotter | hits his aimed target a quarter harder |
| Skirmisher | open | yours to pick |

**When breaking the line is worth it:** a chief winds up his Eclipse on your front row and you can't time the parry. Fall Back dodges it, and under rules 9 his nerve cracks half a parry. You lose the line for 2.5 s instead of taking a blast across your whole front row.

**Does formation feed Hold?** No. Its bonuses are passive and need no order, and Shield Wall takes the sting out of band swings without a Hold.

Risk, reward, punish:
- **Reward:** the bonus while the line holds.
- **Risk:** Fall Back stops being free.
- **Punish, for you:** the bonus is off when you fall back, or when his Hook or Grab pulls someone out.
- **Punish, for him:** a held line turns his band swings into scratches.

## Part 4: Hold, later and on its own

A small parry costs a point and gives it back, so a well timed one is free: you get his stagger, an Opening and a Riposte window. It can still miss its timing, and a miss only soaks, for the point and the drain. Codex's step to make it cost its point stays a separate test, after the squad baseline, so your play can tell which change worked.

## Part 5: how we prove it

What you should see in a fight:
- **Build A:** the trap springs, the snare and bleed marks appear, and the Marksman's ring fills fast. The crit lands, the Opening shows in the Skirmisher's colour, she strikes, then the rip.
- **Build B:** a raider's swing hits the Warden's Guard, the Opening shows on the raider, the Skirmisher strikes, and the Medic's heal lands under the Guard.
- **A chain that fails says why:** the snare ran out, the trap never sprang (no melee enemy), the Opening closed, the aim broke, or the target fell.

Checks after a build go:
- Build A against Build B on the same Bloodridge road, with the same gear budget.
- A fresh squad's first fight, and a bonded pair's.
- A bond lost when a fighter falls or is captured, and the replacement starting fresh.
- A road fight against archers and casters, where the trap never springs.
- A chief fight, where marks don't take on him while his band stands.
- Two cashers on one mark, where the mark pays once.
- Saves and replays: rules 9 fights and saves play back as they began. New rules 10 saves, mid fight and between fights, keep bond pairs, mark origins, readied spells and the row order.

Your gate: whether the chain reads, whether A and B feel different, and whether Hold stops being the default. There's no Hold quota, and a lower Hold share isn't fun by itself.

## The order I'd build in

1. Rules 10: the base rule and kin bonds. Codex shows the marks, the readied ring and why a chain failed. You play A and B.
2. Formation pieces and row order. You play again.
3. The paid small parry on its own, then chief rewards set from what your plays show.

## Your calls

1. Bonds: same enemy (as locked, 1 fight in 18 today) or any enemies (6 in 18)?
2. A bonded shot: no aim (my pick), or it still aims but can't be broken?
3. Formation: left and right only (my pick), or also in front and behind?
4. The Skirmisher's formation piece.
