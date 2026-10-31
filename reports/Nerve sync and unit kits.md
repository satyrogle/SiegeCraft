# Nerve sync and unit kits

Claude's paper, 25 Oct 2026. Jacob: "put them in logs ad write the paper go". His list is in HANDOFF_LOG line 11139, and this paper takes it in his order of importance, nerve first. Nothing here is built or locked. Every number is a starting value, and your play is the gate. The squad synergy paper (reports/Squad synergy and the Hold load.md) still stands, with your calls on it, and it slots in at step 3 below.

## 1. Nerve: sync the bar with the units

### What happens today

- **One bar per side**, 1000 points. Every hit on the side costs nerve: losing the side's whole health to hits costs 70% of a bar. Each death costs 200 on top, and an Unleash scare costs 100. Under rules 9, a parried big beat cracks 100 to 200, and a cashed mark cracks 40.
- **At zero the whole side breaks and the fight ends.** A chief kneels instead.
- **Snaps:** three times a fight, as the bar passes each quarter, the most hurt fighter either panics and flees, or rises and goes berserk (your 13 Oct lock).
- So a band of five can break with three still standing. Your rules 9 chief broke at 19 s with 957 HP left. The fight ends before the squad's play even shows, and players read it as "they gave up for no reason".

### The fix: every fighter has his own nerve, and the bar is their sum

- **Each fighter carries his own nerve**, 200 to start, so a band of five still adds up to today's 1000.
- **What a fighter goes through hits his own nerve:**
  - hits on him, by the health he loses;
  - a comrade falling beside him;
  - a scare (an Unleash shakes every fighter in front of you);
  - a parry of his move, which lands its crack on him;
  - a cashed mark on him.
- **When an enemy's nerve runs out, he snaps,** as your 13 Oct lock already says. He either panics and runs off the field alive, or he rises and fights harder until he dies.
- **You win when no enemy is left fighting,** each one dead or fled. The bar is the sum of their nerve, so it drops exactly as they fall and run. What you see and what the bar says always match.
- **The chief keeps his own bar** (your 21 Oct lock, unchanged). Every move you answer cracks it, and he kneels when it runs out. Spare him and his whole tribe is yours, everyone, as your 13 Oct rule says. Who died or ran in the fight doesn't matter. That's your point 2 as well: recruits only ever come from his kneel after the last fight.
- **Your side never runs.** Your call, 25 Oct: "there's no running only death". Your fighters carry nerve the same way, but they never flee. You lose the fight when none of yours is left standing. What one of yours does when his nerve runs out is call 1 below.
- **Your whole squad comes back every fight.** A death only costs you inside that fight. Losing a fight costs one of your 3 lives a run, like Hades' Death Defiance (your call, 25 Oct). This replaces the 13 Oct roster as lives, where the winners captured one of yours.
- **The hunt and Hold the Line:** "break their nerve" becomes "rout or kill them all". Beasts still have no nerve.

Risk, reward, punish:
- **Reward:** scaring a fighter off counts the same as killing him, and against a tough one it's quicker.
- **Risk:** fights run longer, so the enemy's beats keep coming.
- **Punish, for you:** your own fighters snap too, and with my pick for call 1, one who snaps stops taking your orders.
- **Punish, for him:** his band comes apart one fighter at a time, where you can see it.

What this changes in your locks:
- The 2 Oct break now happens fighter by fighter, and the side breaks when nobody is left fighting.
- The 13 Oct capture and the 21 Oct kneel stay as they are.
- The 13 Oct snap keeps both halves for enemies. On your side the panic that runs goes.
- Grudge (25% more damage on the tribe that captured one of yours) has no trigger once nobody of yours is captured. It needs a new one.
- Rules 9's "the squad breaks nerve too" stays, but each crack lands on the fighter it hits instead of the whole side.

## 2. A kit of 3 for every unit

Your words: "3 attacks/defensekit/magic boost etc to each unit that theyuse in proper intervals of time".

- **Each class keeps the spell it has** and gets two more of its own: a defence move it uses to save itself, and a boost.
- **Each move runs on its own timer**, and the fighter uses it when it fits: the defence when he's in danger, the boost when the squad is fighting.
- **Your class rule holds.** Every move belongs to that class only, and none copies another class's spell or any of the 17 signatures in CLASS_ROSTER.json.

The five classes we have now. The new moves are placeholders, yours to change:

| Class | Has now | Defence (new) | Boost (new) |
|---|---|---|---|
| Warden | Guard: covers an ally | Brace: takes half from the next heavy swing at him (every 8 s) | Challenge: enemies beside him swing at him for 3 s (every 15 s) |
| Marksman | Aimed Shot | Step Back: when a melee enemy reaches him, he hops back (every 10 s) | Steady: his next Aimed Shot crits anyone (every 15 s) |
| Skirmisher | Running Strike | Sidestep: dodges the next heavy swing at her (every 10 s) | Twin Cut: two quick strikes (every 15 s) |
| Trapper | Barbed Snare | Snap Trap: snares a melee enemy who reaches her (every 12 s) | Caltrops: enemy swings in one lane come 25% slower for 4 s (every 15 s) |
| Medic | Field Treatment | Self Patch: heals herself when below half (every 12 s) | Stim: one ally's next move comes at once (every 15 s) |

The other twelve classes get their kits when they're built.

Risk, reward, punish:
- **Reward:** the squad fights smarter on its own. Brace and Sidestep take band swings without your Hold.
- **Risk:** more moving parts to read, so every move needs its own ring on the screen.
- **Punish, for you:** a move spent at the wrong moment is gone for 10 s or more.
- **Punish, for him:** his swings get blunted by fighters who can protect themselves.

What this changes in your locks: it replaces the 20 Oct count, "each class has its auto attack plus one spell", with three moves per class. It keeps "only that class's own spells".

## 3. Damage over time on basic attacks, and more combos

Your words: "how to add elemental and other kinds of dots to basic attacks".

How it works:
- **Every element is a mark,** and one class cashes it. That turns each new element into new combos (point 3) and new synergy (point 7).
- **Elements come from tribes.** A tribe's fighters carry its element on their basic attacks. When you capture a chief, his fighters bring their element to you, which ties recruiting to your build.
- **The Medic's treatment clears one stack**, so every element has an answer.

| Element | From | What it does | The combo |
|---|---|---|---|
| Bleed (have) | the snare's wound, bleeding blades | 2% health a second for 5 s | the Skirmisher rips it all at once |
| Burn (new) | Bloodridge fighters | 1% a second, stacking to 3 | the Marksman's Aimed Shot flares it, and all the burn lands at once |
| Chill (new) | Skullpeak fighters | each stack slows his swings 10%; at 3 he freezes for 1 s | a frozen enemy who swings into the Warden's Guard shatters for a big hit |

Stoneveil and Thundercoast get theirs later, poison and shock for example.

Risk, reward, punish:
- **Reward:** every basic hit sets something up, and the right squad cashes it.
- **Risk:** the enemy uses elements on you too, so Bloodridge raiders will burn you.
- **Punish, for you:** no Medic means nothing clears your stacks.
- **Punish, for him:** a stacked mark is a combo waiting to land.

The squad synergy paper's base rule, kin bonds and formation all fold in here as well: a fresh mark readies its casher, bonds make a pair's combo reliable, and formation lets the back row help the front.

## 4. Talent trees and distinctive traits

- **One tree per class:** three short branches, one for each kit move, two steps each, plus a capstone that's a distinctive trait, like Unshaken (his nerve drains half as fast) or Bloodhound (hunts the bleeding).
- **The trees live at camp.** Fighters reset when a run ends and only materials come home, so you buy tree steps with materials, through a Pal, because the Pals are the upgraders (your 30 Sep lock). Every fighter of that class gets them, every run. Which Pal trains them is yours to pick.
- **Every fighter shows two traits:** one he was born with (his quirk) and one he earns on the road (a road scar). The capstones add a third for every fighter of that class. All of them go on his card and show as icons in the fight.

Risk, reward, punish:
- **Reward:** every run starts with a stronger, more distinct squad.
- **Risk:** materials spent on trees aren't spent on the Butcher's forge.
- **Punish, for you:** a tree you've grown makes that class matter, so losing your only fighter of it hurts more.
- **Punish, for him:** a trained squad answers his moves before you have to.

## The order I'd build in

1. **Nerve per fighter** (rules 10). Rules 9 fights and saves play back as they began. You play.
2. **Kits of 3** for the five classes, with their defence moves. You play.
3. **Burn and Chill as marks**, plus the synergy paper's base rule and kin bonds. You play.
4. **Talent trees and traits** at camp.
5. **Formation**, from the synergy paper.

## Your calls

1. A fighter of yours whose nerve runs out: he rises and goes berserk, hitting harder but ignoring your orders until he dies (my pick, the half of the 13 Oct snap that doesn't run), or he just fights on as normal?
2. Which Pal trains talents?
3. Burn for Bloodridge and Chill for Skullpeak first (my pick)?
4. The kit moves above: keep them as placeholders, or do you want to name them?
