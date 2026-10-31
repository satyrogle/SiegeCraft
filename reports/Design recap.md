# Design recap, start to 25 Oct 2026

Claude, 25 Oct 2026. Jacob: "pull up every design call from the start of this project till now what stayed in game what is killed make it detailed". Sources: all of DESIGN_LOCKS.md, the sim code, HANDOFF_LOG.md and tonight's talk. Nothing here is new design.

How to read the tags:
- **Built**: in the sim and its tests pass. "Played" means you played it in a build.
- **Paper**: you locked it, but it isn't built.
- **Killed**: dropped or replaced. It says what replaced it.
- **Open**: not decided yet.
- **Tonight**: you said it on 25 Oct. It isn't in DESIGN_LOCKS yet.

"Built" means in the sim. I haven't checked most of what Codex has put on screen.

---

## Before the locks, 22 to 29 Sep

| Call | Status |
|---|---|
| Kindred Siege is game one, for Steam on Windows | Stands |
| Desk 42 | **Killed** 22 Sep. Archived to GitHub on 20 Oct |
| The old design doc's systems (TheRivalKnows mutation, phobia stacking, mythos escalation, Mercy Ledger, the old bond system) | **Killed** by the 30 Sep locks ("older documents are history"). Bonds came back on 20 Oct as kin bonds, a different idea |
| The Drowned City and Veyr | **Killed** |
| Cosmic horror tone | **Killed** 2 Oct. Replaced by funny everywhere, Kingdom Rush style |
| First Alliance prototype (Codex, 26 Sep) | **Killed**. Parked 3 Oct |
| Vendetta rule (rival grudges, too close to the Nemesis patent) | **Killed** 3 Oct. Stays out until a solicitor answers |
| Region art style B; GBA Fire Emblem style characters | Style B stands for maps. The character style moved to PixelLab on 1 Oct |
| Local Wan animation; Pixel Engine walk cycles | **Killed** 30 Sep (janky, speckle not pixel art) |

## 30 Sep, the ten locks ("will not be changed ever")

| Lock | Where it stands |
|---|---|
| 1. Tribe count | 25 since 18 Oct (was 24): 23 tribes, the Sovereign's tribe, the hunters. **Built**: 6 so far (Clarth, Bloodridge, Stoneveil, Skullpeak, Thundercoast, the hunters) |
| 2 to 6. Seven tiers (1 to 3 story, 4 to 5 midgame, 6 unlocks most things, 7 the Sovereign) | Stands. **Built**: tiers 1 and 2 on the road. Which tribes go in which tier is **Open** |
| 7. Roguelike, fast paced action | Stands |
| 8. 17 classes, unlocked through progression | Stands. **Built**: 5 (Warden, Marksman, Skirmisher, Trapper, Medic). How the other 12 unlock is **Open** |
| 9. Top priority: items, Pals, upgrades, tribes codex | Partly **built**: gear, Pal favours, the forge, codex pages. What each Pal upgrades is still mostly **Open** |
| 10. 2.5D battle screen | Stands. Codex's side |
| Pals are the surviving inner circle and the upgraders | Stands |
| The rivalry engine runs alongside progression | Stands. Since 18 Oct it is the hunters |
| Beastwarden class | **Killed**. Its slot is Disruptor |

## 1 Oct, five calls

| Call | Status |
|---|---|
| Auto battle plus orders: Hold, Fall Back, Focus Fire, Unleash, Sacrifice | **Built**. Sacrifice **killed** 10 Oct |
| Innovation claim: tribes learn from real fights, with a replay of why | **Built** (tribe minds, fight archive, replay evidence) |
| Visa slice for 8 Dec: Fallen Star, 3 tribes, 5 classes, 20 to 30 minutes | Stands. The third tribe became Skullpeak |
| Tribes and classes hand made; everything inside them procedural | Stands |
| PixelLab for sprites | Stands |

## 2 to 3 Oct, premise, loop and looks

| Call | Status |
|---|---|
| We are a cult of cyborg flesh beings who worship old gods | Stands |
| Prologue part 1 (7 Oct): the raid, half the army gone in a second, the four first officers become the Pals | Stands. Why she spares the leader, and part 2, are **Open** |
| Funny everywhere, only the Sovereign scary | Stands. Bloodridge are scary too, by your choice |
| The three tit rule: one look a gamer gets in one second | Stands |
| Order buttons refill over time | **Killed** 13 Oct. Replaced by points you earn |
| Orders come from moves taken off broken tribes (Mega Man Battle Network) | Became "each chief gives you his one order", the 5th button. **Built** |
| Nerve bar: a tribe gives up at zero and everyone alive joins (kill fewer, keep more) | The bar is **built**. "Everyone alive joins" was **killed**, three times over: 10 Oct nobody joins until the chief, 11 Oct only his standing fighters, then 13 Oct capture the chief and the whole tribe joins |
| A broken chief runs off and comes back leading a splinter group | **Killed**. Became the chief who learns from you, shown on the fight card (11 Oct) |
| Pals forge gear from what you won; the Pal who broke a chief learns his recipe | **Built** for one piece: breaking the Bloodridge chief teaches the Butcher the Lava Sun Tabard |
| Gear changes how a fighter looks (He Man's Snake Armor) | **Killed** 20 Oct. Pieces show as an effect and an icon instead |
| No mutations, no extra limbs | Stands |
| Holding drains your own nerve | **Built**. Rules 9 keeps it as the Hold's drain |
| Death: army, gear and converts reset; chief memories, codex and Pal stories stay | Stands, plus the 10 Oct Hades rule below |
| A campaign that feels endless, plus an endless mode | **Paper** |
| A Pal leads every fight as the hero and transforms (Battle Cat) | **Killed** 10 Oct. The Pals stay at camp, and the leader is on the field |
| Four Pals with Chaos god traits only: Butcher, Glutton, Shade, Whisperer | Stands. The names are placeholders |
| Chiefs ride their region's monster | **Killed** 9 Oct. No riding |
| Every tribe hunts a monster | Became the beasts (20 Oct). **Built** for two: the shark and the bear |
| Clarth look: war hunters, bone masks, red cloaks, shark part weapons | Stands. The surfer and horned chibi drafts were **killed** |
| Bloodridge look: Medjai guardians in a Beksiński mood | Stands. The crab weapons and the clean Medjai were **killed** |
| Skullpeak look: steel bones through warm skin, one bionic arm each | Stands. Four Giger rounds were **killed** |
| Chiefs: Clarth, Bloodridge, Skullpeak (Arina) at 192 px | Stands |
| Size ladder 128, 112, 96 px; 32 colours per sprite | Stands |
| Region names from the concept map | Stands |

## 8 to 10 Oct, the loop

| Call | Status |
|---|---|
| The respect campaign (8 Oct answers: pick a tribe, 3 fight cards, respect bar, taboos, recruits, assists) | **Killed** 10 Oct, after you played it and it "didn't make sense" |
| Taboos | **Killed** |
| Each tribe is a set road of a few fights, then its chief | **Built** |
| Every run starts and ends at camp, even when you lose | **Built** |
| Losing like Hades: run gear is lost, materials come home and pay for camp upgrades | **Built** |
| A run crosses tiers, one tribe per tier, toward the Sovereign | **Built** for two tiers |
| Nobody joins until the chief | **Built** |
| Each chief gives you his one order for the run | **Built** |
| Every tribe you win over goes into the codex for good | **Built** |
| The hunt grows inside a run and resets with it | **Killed** 18 Oct. The hunters now get stronger across runs |
| Leader look locked; a neural bracer moves a relic over his palm | Stands |
| Pals at camp: Butcher forges weapons and gear, Shade unlocks orders, Glutton armour and body, Whisperer skins and regalia | Roles stand. Only the Butcher's forge is **built** |
| Pal looks: Glutton v3 locked; Butcher v4 and Shade v4 provisional; Whisperer still being redone | **Open** for three of them. The first Pal sheets and the first sprite pass were **killed** |
| A physical camp where you click a Pal and they work at their station | **Paper**. No art route approved |

## 11 Oct

| Call | Status |
|---|---|
| The world at 1x | Stands as a rule. The whole 1x art pass (camp, menus, three stages, tilesets) was **killed** that night |
| Joke intro names (Kindred Freeze, Cheese, Bees) | **Killed** 14 Oct |
| Unleash drains your own nerve at half Hold's rate | **Built** |
| No floor on recruits | **Killed** 13 Oct by capture |
| Lava Sun Tabard, 50 materials, the first Warden wears it | **Built** |
| One last fight against the hunters after the last chief | **Built**. Since 22 Oct it's the hunt |
| The hunters don't learn between runs | **Killed** 18 Oct |
| A war between tribes at camp | **Killed**. The 1 Oct code still sits in TribeWar.cs, unused |
| The fight card says what a tribe learned from you, and from which fight | **Built** |
| Fixed lineups; Medium difficulty | **Built** |

## 12 to 13 Oct, the big design

| Call | Status |
|---|---|
| A name generator: Egyptian names for Bloodridge, deadpan chief titles, 1 in 20 cheeky names | **Built** |
| The hunters stay faceless | **Killed** 18 Oct. They have names now |
| Capture the chief and the whole tribe joins; kill him and you take his Princess piece | **Built**. Since 21 Oct it's the kneel with Spare or Execute |
| Your roster is your lives: the winners capture one of yours; the run ends below 3 fighters | **Built**. **Tonight** you replaced it with 3 lives, below |
| Gear tiers: Pauper, Mid, Princess, one slot each | **Built**. Since 20 Oct every piece belongs to one class |
| Quirks (Grudge, Brute, Tough, Sharp eye, Quick, Steady) and tribe sets | **Built**. Grudge fires on the tribe that captured one of yours, so tonight's change breaks its trigger |
| Orders cost points you earn (parries, dodges, breaks, kills, combos, plus a trickle) | **Built** |
| The parry is Perfect Hold: Hold just before a big hit | **Built**. It grew into rules 8 and 9 |
| Hit and Avoid, no chances shown, just damage numbers and MISS | **Built** |
| Tribes react under threat (Bloodridge rally, Skullpeak hunt the hurt, Clarth kite) | **Built** |
| Nerve snaps: panic or go berserk, both sides | **Built** |
| Path map: 3 lanes, 3 rows, then the chief | **Built** |
| Events, objectives, omens | **Built** |
| Formation: set each fighter front or back on the squad page | **Built** |
| Chief taunts | **Built** as lines at the big moments. The words are yours to write |
| Arena hazards | **Paper**. First to cut if late |
| Build first, optimise later | Stands |
| Sprints 4 and 5 | Parked 14 Oct. Their content came back later in other forms |

## 14 to 16 Oct

| Call | Status |
|---|---|
| A tribe that gets parried learns to wait out your Hold | **Built**. Rules 9 cut it to one move, and only the chief learns it |
| Title: our leader at the ship's bay over her planet, COMMENCE, the deadpan readouts | **Built** |
| The dive from orbit into her place | **Open**. The storyboards were **killed** |
| Hunters as dragonkin (15 Oct) | **Killed** 16 Oct. Manta ray reptilians picked instead. The masked hunters were **killed** too |
| Bloodridge champion: two handed great khopesh, near Warden size | Stands |

## 18 Oct

| Call | Status |
|---|---|
| The hunters are the 25th tribe and the rival, not the Sovereign's army | Stands |
| The Sovereign's tribe is the 57: light warriors corrupted by a dark surge; look option 2 | Stands. Their classes and her place are **Open** |
| Hunters: names and titles, learn a trait when they beat you, ambush on the road, bounty 1 to 5, a ladder of 10 | **Built** |
| Clarth chief spell: Dust Shark | **Built** |
| Chief spells doubled ("think big") | **Built** |
| Two new tribes: Stoneveil (cave, bloodletting) and Thundercoast (sea, flood); one random tribe per tier | **Built** with placeholder figures. No art yet |
| Pal favours on road nodes, like Hades boons | **Built**: 12 favours, 3 per Pal, each bending one button |

## 19 to 20 Oct

| Call | Status |
|---|---|
| Class rule: an auto attack plus one spell only that class has | **Built**. **Tonight**'s kit of 3 would change the count |
| Skirmisher is melee main damage; the Trapper's snare also bleeds | **Built** |
| Marks: Snare, Bleed, Opening, Guard, each cashed by another class | **Built** |
| Every piece bends only its own class's spell; after a win you strip one of three | **Built** |
| Hybrid 1, status web | **Built** (the marks) |
| Hybrid 2, trophy pieces from beast parts | **Built** |
| Hybrid 3, tribe stacking inside a run | **Built** (sets) |
| Hybrid 4, kin bonds | **Paper**. **Tonight**: on any enemy, and a bonded shot fires right away |
| Hybrid 5, Pal duos | **Paper** |
| Hybrid 6, formation pieces that help the neighbour | **Paper**. **Tonight**: the back row are the constant combo doers |
| Hybrid 7, road scars | **Built** (Jawbreaker and the rest) |
| Hybrid 8, blood pacts | **Paper** |
| Hybrid 9, order sockets | **Paper** |
| Hybrid 10, road jobs | **Paper** |
| Envoys, stages that read what you carry | **Paper** |
| Beasts as optional bosses beside the road | **Built** |
| Chief fights: his own moveset, Glory D to S, a hidden chain, a kneel with no timer, Spare or Execute | **Built** (rules 3) |
| A 3 second kneel window, chiefs using your orders back at you, one key per lock | **Killed** |
| The kneel at a quarter health | **Killed**. Replaced by the nerve kneel |
| One tactical piece per chief (Bloodridge sees moves early, Skullpeak band jobs, Stoneveil Blood Price, Thundercoast places not people) | Locked with each kit and built with it, per the locks |

## 21 to 23 Oct

| Call | Status |
|---|---|
| Sand shark: no nerve, four parts, trophies, scars, two phases | **Built** (rules 4). Far too easy at default strength |
| Bloodridge chief finished (5 moves); Skullpeak chief kit | **Built** (rules 5) |
| Cage bear | **Built** |
| Hunt captain as a rival duelist | **Killed** ("exactly like dmc 3 boss") |
| The hunt: Track, Drive, Corner, Kill | **Built** (rules 6) |
| Stoneveil and Thundercoast chiefs | **Built** (rules 7) |
| Spell effects rise out of a floor ring in the top down camera; hex shield | Locked. Six cut. Flat card effects **killed** |
| Bear look, PixelLab, 256 px, alone on its side; the party wide slam | Look locked 24 Oct. The slam lives only in the video. Whether it becomes a sim move is **Open** |

## 24 Oct, rules 8 and rules 9

| Call | Status |
|---|---|
| Rules 8, the parry at the center | **Built**, then you played it and rejected the feel |
| Every band swing is a duel for nerve | **Killed** by rules 9. Only big beats are duels now |
| A block cost: an early Hold cracks your nerve | **Killed** by rules 9 |
| Chiefs learn from your road fights; wait out and feint on every beat | **Killed** by rules 9 |
| Nothing is unparryable; a Hold given while paused never parries | Stands |
| Gate 4: every lesson a rival learns opens a play for you; hunters have 17 traits | **Built** |
| "Shoots past your Warden" | **Killed** |
| Hunt Banner, lesson marks on bodies, the memory replay | **Paper**. Closest to the Nemesis patent, so the solicitor sees them first |
| Combo cues: colour by finisher, glow when ready, stamps | **Paper** on the screen side. Codex's |
| Rules 9, the squad first: your three picks | **Built** and played (Codex's audit, log 11095). Locked |
| Rules 9 extras: parry refunds a point, a cashed mark gives a point and nerve, a trickle, chain cap 200% | **Built** and played. Not locked |

## 25 Oct, tonight

All of this is **Tonight**: said, but not locked or built.

| Call | Notes |
|---|---|
| Bonds form on any enemies; a bonded shot fires right away | Changes the 20 Oct "same enemy" wording |
| Formation: two in front, the rest behind; the back row are the constant combo doers | My reading of your words, still to confirm |
| Skirmisher's piece Blood Scent: the fighter beside her hits bleeding enemies harder | "sure why not" |
| Your nine point list, first: nerve synced with units, recruits only from the chief, more combos, talent trees, distinctive traits, self preserving units, unit synergy, elements on basic attacks, a kit of 3 per unit | Paper: reports/Nerve sync and unit kits.md |
| When the chief kneels and you take him, his whole tribe is yours | Already true since 13 Oct. Fixed in the paper |
| Your units never run, they only die | **Clash**: the nerve paper still says your fighters can flee. It needs that line changed |
| Your whole squad comes back every fight | Replaces "your fallen stay down" |
| 3 lives a run, like Hades Death Defiance | Replaces the roster as lives and the winners capturing one of yours. Grudge needs a new trigger |

---

## The big things still open

- The Sovereign: why she spares the leader, prologue part 2, her place, the 57's classes.
- Which tribes sit in which tier. How the 17 classes unlock. 12 classes unbuilt.
- What each Pal upgrades, beyond the forge and the favours.
- The camp, the battle stages and the menus: no approved art route since the 11 Oct rejection.
- Renaming the hunt in code and on screen.
- The Nemesis patent question, open since 8 Oct.
- Endless mode.
- Tonight's list, in your order, nerve first.
