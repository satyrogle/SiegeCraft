# Combat fix plan

25 Oct 2026, Claude. Built from [the research report](Fun%20readable%20squad%20combat.md), your words in the log and the locks, and the sim on disk tonight. Nothing here is approved or built.

## The answer

Put road fights on one beat: one enemy threat at a time, about every 6 s, that you can parry, and your squad cashes your parry with one combo you can see.

The fight never lacked systems. It lacks space between them.

## Why, in numbers

| What | Number | Where from |
|---|---|---|
| Notable events a person takes in | about 2 to 4 a second | the report, attention section |
| Your rules 12 fight | about 7 a second, 28 at the peak | [log 11658](../HANDOFF_LOG.md#L11658) |
| Telegraphed threats in a full war party | about 26 a minute, each on its own clock | band swing every 5 s, Cleave every 6 s, Brand every 15 s ([DL 659](../DESIGN_LOCKS.md#L659); [Carriers.cs 27](../Assets/Scripts/Sim/Battle/BattleSimulation.Carriers.cs#L27)); worked out, not measured |
| Threats a person can answer | one at a time, with about 1 s of warning | the report, orders section |
| Your parries, rules 9 route (threats about 5 s apart) | 20 of 26 Holds | [log 11101](../HANDOFF_LOG.md#L11101) |
| Your parries, rules 8 chief (beats every 1 to 2 s, with feints) | 0 of 8 Holds | [log 10857](../HANDOFF_LOG.md#L10857) |
| Your parries, rules 12 war party | 0 of 3 Holds | [log 11645](../HANDOFF_LOG.md#L11645) |
| Setups that never paid (rules 12 bot sweep) | 909, against 633 combos | [log 11703](../HANDOFF_LOG.md#L11703) |
| Overlaps still in rules 14 | a Brand, a cast and a Cleave up at once in fight four, in Codex's own checks | [log 11740](../HANDOFF_LOG.md#L11740) |

Two routes agree. The research says people answer one cue at a time. Your own plays say you parry when threats are spaced and miss everything when they overlap.

Rules 13 showed the other side. Cutting hard left fight one with nothing to parry ([log 11728](../HANDOFF_LOG.md#L11728)). So the fix is spacing, not cutting.

## One minute of the fixed fight, on paper

- **0 to 4 s.** Both lines trade blows. Small hits, small numbers, quiet.
- **4 s.** Their Cleaver lifts his axe. He glows, a line points at your Warden, a sound starts. Nothing else big starts while he winds up.
- **5 s.** You press Hold in the last half second. Clang, flash, freeze. He staggers for 2 s and is open.
- **5 s.** Your Skirmisher hits the opening at once. One stamp: her face, his face, the number. A point for the combo.
- **5 to 7 s.** Your call: Unleash now for the Riposte (2 points), or keep the points for the chief spell (3).
- **7 to 11 s.** Quiet. Your snare springs and your Marksman shoots the snared one. One stamp, one point.
- **11 s.** The next threat.

Miss the parry and the Cleave lands for double on your Warden. Fall Back instead and it misses, but it costs a point, your squad stops hitting for 2.5 s and nothing follows. The risky answer pays more, the safe one pays less, and the miss hurts.

That's 5 or 6 threats a minute, each with a choice after it. The research target is one readable beat every 5 to 10 s.

## The four rules (rules 15, after Codex's rules 14)

Codex delivered rules 14 tonight: your leader takes no damage while anyone else stands, every parry beat has a ring on the attacker and a line to his target, the Hold button lights only when there's a beat, combos show who set them up and who finished, and band swings are back in fight one ([log 11735](../HANDOFF_LOG.md#L11735)). You haven't played it yet. These rules sit on top of it. Old fights and saves keep playing back exactly behind the new version, like every bump so far.

**1. One threat at a time.**
- What: road fights get one threat slot. A band swing, a carried Cleave, a Brand or a Burning Ring must take the slot to start its tell. When it lands or is answered, the slot stays shut 3 s. Then the threat that has waited longest takes it, ties in setup order. A waiting clock stops, so nothing piles up.
- Chief fights keep their overlapping moves, as you locked on 20 Oct ([DL 441](../DESIGN_LOCKS.md#L441)).
- Why: Doom's attack tokens cap how many demons attack at once. WoW cut its raids to one standard warning set. Your rules 9 numbers above.
- Test: in a stage 4 road fight no two tells overlap, and tells drop from about 26 a minute to about 10.

**2. Every setup pays, and your parry's pays first.**
- What: a parry already leaves the swinger open ([Parry.cs 338](../Assets/Scripts/Sim/Battle/BattleSimulation.Parry.cs#L338)). But your Skirmisher only takes an opening if she hasn't struck in the last 2 s, so most of the time nothing follows your parry. Under rule 2 a mark waits for its casher: it stays up until he's ready, then he cashes it at once. The snare's hold stays 2 s; only the mark waits. If he can't (dead, frozen, falling back), it runs out as now. This is the "recheck unclaimed setups" fix from [log 11499](../HANDOFF_LOG.md#L11499), never built.
- One side's payoffs land at least 1 s apart, so each gets its own stamp. The opening from your parry goes first in line.
- Why: your words, "parry is the ultimate move ... everything should be intertwined between them" ([DL 573](../DESIGN_LOCKS.md#L573)), and "right away is combo rest doesnt feel like a combo" ([log 11145](../HANDOFF_LOG.md#L11145)).
- Risk, reward, punish: the half second is the risk. The reward is no damage, a stagger, a combo and its point. A miss takes the full hit. Fall Back stays the safe answer and pays nothing.
- Test: every parried swinger who lives gets struck on his opening. No ".unready" setups, and no two payoffs from one side inside 1 s.

**3. No counter lands on the same tick as your order.**
- What: a learned answer to your order (their Hold against your Unleash, covering whoever you focus) waits half a second and shows its tell first.
- Why: in your rules 12 fight their Hold blocked your Unleash's scare on the same tick, so the order looked like it did nothing ([log 11645](../HANDOFF_LOG.md#L11645)). A person needs about a quarter second just to see a cue. Your lock says a learned answer should bait an opening ([DL 600](../DESIGN_LOCKS.md#L600)).
- The learning stays. It just becomes something you can see, which the visa claim needs anyway.
- Test: a learned Hold never blocks an Unleash given in the same half second.

**4. The sim tells the screen what matters.** No new rules, only reads for Codex:
- the threat in the slot and the next two, for a queue on screen;
- a tier on every hit (light, heavy, finisher, chief), so only the top one gets the freeze and flash;
- what each order would do right now (a cast bar up: Focus Fire breaks it; an enemy staggered: Unleash is a Riposte; 3 points: the chief spell). Lit means possible, not "press this";
- each enemy's move, for the screen before the fight;
- a recap after the fight: parries made and missed, combos by pair, who killed whom, the threat that hurt most.

## What the screen does (Codex)

- One loud thing at a time. Rank every event: parry, threat tell, kill, combo stamp, status, plain hit. Only the top one gets the freeze, flash, big number and sound. The rest play small, on the two fighters only.
- Keep what rules 14 added: the ring on the attacker, the line to his target, the Hold button that lights only for a beat, and the setup and finisher trails on combos.
- A perfect Hold and an early Hold look and sound different: clang against thud.
- The stagger after a parry shows as a window over him, so the Riposte has its moment.
- No words on bodies. One colour per side in every spark and shot.
- Before the fight: each enemy's move as an icon, and lit lines between your fighters who can combo.
- After the fight: the recap, three to five moments.
- Save your speed and rejected clicks with each fight, so your plays can be read.
- Once rule 1 is in, try 1x as the default again. Half speed was a patch for the pile up. Your call after you play.

## Keep, cut, park

| Keep (your calls) | Cut from road fights | Park until after 30 Nov (my call) |
|---|---|---|
| Auto battle, four orders, the chief spell | Overlapping threats (rule 1) | More combos, talent trees, traits (your list, items 3 to 5) |
| Hold free, Fall Back a 1 point dodge, points from combos | Setups that never pay (rule 2) | Elements past Burn |
| Nerve per fighter, 3 lives, the x3 streak | Same tick counters (rule 3) | Hunters, beasts, other raid chiefs, spell upgrades (you parked these) |
| Melee frenzy as a last stand, yours below a quarter health | Enemy combos and boosts (gone in rules 13) | |
| Class spells, marks, bonds, defence moves, boosts | | |
| Trash carrying the chief's moves | | |
| Simple enemies (13), leader last (14), one new system per fight | | |
| The raid chief (rules 10) | | |

The fight order stays as you approved it in rules 13, with band swings back in fight one (rules 14): combos, then defence moves, then boosts, then the carried moves, then the chief.

## Build order

| Step | What | Who | Target |
|---|---|---|---|
| 0 | Play rules 14: Builds/KindredSiege-Road14-20261025, Play Bloodridge Road.cmd | you | now |
| A | Rules 1 to 4, one quick test each | whoever you give the sim (Codex since your handover) | this week |
| B | The screen list | Codex | next week |
| C | You play the whole Bloodridge road and the chief; fix only what you name | both | 16 to 29 Nov |
| D | Strangers play | you | 30 Nov |

Targets, not promises. No bot sweeps or tuning before you've played A, under your 12 Oct rule.

## Your calls

1. One threat at a time in road fights, 3 s apart. Chief fights keep overlapping moves. My call: yes. Yes, or change it?
2. Every setup pays, your parry's first, one payoff a second. My call: yes, it's the one link between your parry and your squad. Yes, or change it?
3. Learned counters wait half a second and show a tell. My call: yes. Yes, or change it?
4. Park more combos, talent trees and traits until after 30 Nov. My call: park, because adding systems is what broke rules 12. Yes, or change it?

## How sure

- LIKELY on the cause, spacing: the research and your own parry numbers agree, by different routes.
- GUESS on every number (6 s, 3 s, 1 s, half a second). Your play sets them.
- What would change my mind: if build A reads but bores you, the gap is too long. If it still doesn't read with one threat at a time, the fault is the screen, not the sim.
- Holes I couldn't close:
  - A free Hold kept up all fight soaks 30% of front row damage for nothing ([BattleSimulation.cs 1794](../Assets/Scripts/Sim/Battle/BattleSimulation.cs#L1794)). That's your rules 9 pick, Hold a stance again. Mashing can't parry, though: road fights already run the raid guard, where a Hold pressed within 1 s of your last press keeps the old window ([Raid.cs 57](../Assets/Scripts/Sim/Battle/BattleSimulation.Raid.cs#L57)). If you catch yourself holding all fight, the stance needs a cost.
  - The chief fight is the one place I left alone. If strangers can't read it on 30 Nov, the same slot can go there, two at a time.
- Not checked: rules 14. Codex handed it back tonight ([log 11735](../HANDOFF_LOG.md#L11735)), but I haven't run it and you haven't played it. I ran nothing. The threat count comes from the sim's timings.
