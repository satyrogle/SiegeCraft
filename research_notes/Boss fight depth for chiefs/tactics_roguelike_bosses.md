# How tactics games, roguelikes and deckbuilders make boss fights deep (notes for Kindred Siege chief fights)

How to read the sources: every bullet ends with its link. A link marked [search] means the claim came from a search engine summary of that page, because the page blocked my fetcher (Fandom wikis returned HTTP 402, several guide sites returned 403). Treat [search] items as secondhand. Links without the tag were opened and read. Wiki numbers are community documented, not published by the developers.

## 1. Slay the Spire: intents, the named bosses, and what Mega Crit said

### Takeaway
Slay the Spire shows every enemy's next move as an intent icon, so its bosses get depth by bending the rules a deck relies on (card count, Power cards, debuffs, burst damage) and by alternating different threat shapes, not by hiding information. The boss is a scouted test: its portrait sits on the map from the start of the act, it is drawn from a pool of three, and Ascension escalates it with stats first and new movesets later.

### Cited Findings
Intent display and scouting
- Intents are icons: attack (seven icons by damage band: 0 to 4, 5 to 9, 10 to 14, 15 to 19, 20 to 24, 25 to 29, 30 or more), defend, attack plus defend, debuff, strong debuff ("a Powerful Debuff"), attack plus debuff, defend plus debuff, buff, attack plus buff, defend plus buff, escape, sleep, stunned, and Unknown. Unknown covers splits, explosions, summons and the Awakened One's Rebirth. ([StS wiki: Intent](https://slaythespire.wiki.gg/wiki/Intent))
- Information is itself a tradeable resource: the Runic Dome relic shows all intents as Unknown in exchange for an extra Energy each turn. The Writhing Mass "Changes intent randomly when dealt attack damage." ([StS wiki: Intent](https://slaythespire.wiki.gg/wiki/Intent))
- "Every boss has a unique portrait shown at the top of the Spire map." Each act draws one boss from three: Act 1 Slime Boss, The Guardian or Hexaghost (floor 16); Act 2 Bronze Automaton, The Collector or The Champ (floor 33); Act 3 Awakened One, Time Eater or Donu and Deca (floor 50); Act 4 is always the Corrupt Heart (floor 55). ([StS wiki: Bosses](https://slaythespire.wiki.gg/wiki/Bosses))
- The node before a boss is always a Rest Site (the Heart is preceded by an elite instead). Act 1 and Act 2 bosses offer a choice of three Boss Relics. After each boss the player heals 100% of missing HP (75% at Ascension 5). ([StS wiki: Bosses](https://slaythespire.wiki.gg/wiki/Bosses))
- Escalation: bosses get more HP and damage at Ascension 4 and 9 and "more challenging movesets" at Ascension 19. At Ascension 20 two different bosses are fought back to back at the end of Act 3. The Heart is only available after killing the Act 3 boss at least once with each of the Ironclad, Silent and Defect. ([StS wiki: Bosses](https://slaythespire.wiki.gg/wiki/Bosses))

Hexaghost (Act 1)
- 250 HP (264 at Ascension 9). Opens with Activate (does nothing), then Divider, then loops Sear, Tackle, Sear, Inflame, Tackle, Sear, Inferno. ([StS wiki: Hexaghost](https://slaythespire.wiki.gg/wiki/Hexaghost))
- Divider deals (N+1) x 6, where N is the player's HP at the start of the turn divided by 12, rounded down (12 to 23 HP gives N = 1, so 12 damage). The big early hit scales with how healthy you are, so it is answered by Block, not by HP. ([StS wiki: Hexaghost](https://slaythespire.wiki.gg/wiki/Hexaghost))
- Sear: 6 damage plus 1 Burn (2 Burns at Ascension 19). Tackle 5x2 (6x2 at A4). Inflame: 12 Block and 2 Strength (3 at A19). Inferno adds 3 upgraded Burns and upgrades every Burn in the discard pile; after Inferno, Sear's Burns are upgraded for the rest of the fight. The wiki is inconsistent on Inferno damage (move table 2x6, 3x6 at A4; Hints section 24, 30 at A4, 36 at A19). ([StS wiki: Hexaghost](https://slaythespire.wiki.gg/wiki/Hexaghost))

The Champ (Act 2)
- 420 HP (440 at A9). Phase 1: Taunt (2 Weak, 2 Vulnerable) every 4 turns; otherwise Heavy Slash 45% (16, 18 at A4), Face Slap 25% (12 plus 2 Frail and 2 Vulnerable), Defensive Stance 15% (15 Block and 5 Metallicize, rising to 20 and 7 at A19), Gloat 15% (2 Strength, 3 at A4, 4 at A19). No move twice in a row. ([StS wiki: The Champ](https://slaythespire.wiki.gg/wiki/The_Champ))
- When he drops below 50% HP he uses Anger on his next turn: removes all his debuffs and gains 6 Strength (9 at A4, 12 at A19). The following turn he uses Execute (10x2). For the rest of the fight he repeats two random moves, then Execute. ([StS wiki: The Champ](https://slaythespire.wiki.gg/wiki/The_Champ))

Awakened One (Act 3)
- 300 HP (320 at Ascension), fought alongside Cultists. Curiosity: gains 1 Strength whenever the player plays a Power card (2 at A19). Regenerate: heals 10 HP at the end of its turn (15 at A19). Phase 1 opens with Slash (20), then Soul Strike 6x4 (25%) or Slash (75%). ([StS wiki: Awakened One](https://slaythespire.wiki.gg/wiki/Awakened_One))
- At 0 HP it Reborns: heals to full HP, removes all debuffs, loses Curiosity, but keeps the Strength Curiosity gave it. Phase 2 opens with Dark Echo (40), then 50/50 Tackle 10x3 or Sludge (18 damage plus a Void into the draw pile). The Cultists flee when it dies in phase 2. ([StS wiki: Awakened One](https://slaythespire.wiki.gg/wiki/Awakened_One))

Time Eater (Act 3)
- 456 HP (480 at A9). Time Warp: "Whenever you play a certain number of cards, ends your turn and gains X Strength." Curses and statuses that play themselves do not count. An early patch said "Time Eater now gains less Strength from Time Warp but base damage for attacks increased." ([StS wiki: Time Warp](https://slaythespire.wiki.gg/wiki/Time_Warp)); ([StS wiki: Time Eater](https://slaythespire.wiki.gg/wiki/Time_Eater))
- The values are every 12 cards and 2 Strength, per a guide; I could not confirm them on a wiki page. ([Pro Game Guides: Time Eater](https://progameguides.com/slay-the-spire/how-to-beat-the-time-eater-boss-in-slay-the-spire-strategies-deck-synergies-and-more/)) [search]
- Haste: once per fight, when below half HP, it removes all its debuffs and heals to 50% HP (and gains 32 Block at A19). Moves: Reverberate 7x3 (45%), Head Slam 26 plus 1 Draw Reduction (35%, A19 also adds 2 Slimed), Ripple 20 Block plus 1 Vulnerable and 1 Weak (20%, A19 adds 1 Frail). Never Reverberate three times in a row, never Head Slam or Ripple twice in a row. ([StS wiki: Time Eater](https://slaythespire.wiki.gg/wiki/Time_Eater))

Donu and Deca (Act 3)
- Each has 250 HP (265 at A9) and 2 Artifact (3 at A19), which negates debuffs. Donu alternates Circle of Power (all enemies gain 3 Strength) and Beam (10x2). Deca alternates Beam (10x2 plus 2 Dazed into the discard pile) and Square of Protection (all enemies gain 16 Block, plus 3 Plated Armor at A19). ([StS wiki: Donu and Deca](https://slaythespire.wiki.gg/wiki/Donu_and_Deca))

Corrupt Heart (Act 4)
- 750 HP (800 at A9). Invincible: it can lose at most 300 HP per turn (200 at A19). Beat of Death: the player takes 1 damage for every card played (2 at A19). ([StS wiki: Corrupt Heart](https://slaythespire.wiki.gg/wiki/Corrupt_Heart))
- Always opens with Debilitate: 2 Vulnerable, 2 Weak, 2 Frail, and adds a Burn, Dazed, Slimed, Void and Wound to the draw pile. Then it alternates, in random order, Blood Shots (2 damage x12 hits, x15 at A4) and Echo (40, 45 at A4). ([StS wiki: Corrupt Heart](https://slaythespire.wiki.gg/wiki/Corrupt_Heart))
- Every 3 turns from turn 4 it uses Buff: removes negative Strength, gains 2 Strength, plus one extra buff in this order: 2 Artifact, then 1 more Beat of Death, then Painful Stabs, then 10 Strength, then 50 Strength. The wiki does not say whether the list repeats. ([StS wiki: Corrupt Heart](https://slaythespire.wiki.gg/wiki/Corrupt_Heart))

What Mega Crit said
- Anthony Giovannetti: "We wanted a wide gamut of enemies that would challenge different strategies in unique ways." Also: "It was important to us that there would not just be a single dominant strategy that the player could always do", and "Since we are a single player card game, we don't have to worry about the typical downside of strong combos". ([Game Developer, Road to the IGF: Slay the Spire](https://www.gamedeveloper.com/disciplines/road-to-the-igf-mega-crit-games-i-slay-the-spire-i-))
- Giovannetti gave the GDC 2019 talk "Slay the Spire: Metrics Driven Design and Balance" on a metrics focus in early development and data driven balancing through early access. I could not open the talk or a transcript. ([GDC news](https://gdconf.com/news/learn-slay-spires-successful-metrics-driven-approach-game-balancing-gdc-2019)) [search]; ([GDC Vault](https://www.gdcvault.com/browse/gdc-19/play/1025731)) [search]
- Podcast show notes (written by the host, not a transcript): "The core design of Slay the Spire is risk versus reward." ([Think Like A Game Designer](https://justingarydesign.substack.com/p/anthony-giovannetti-crafting-slay))

Slay the Spire 2 (Early Access 2026; fan guide snapshot, numbers may change)
- Early Access was out by March 2026. ([Daily Campus](https://dailycampus.com/2026/03/13/slay-the-spire-2-early-access-fully-delivers/)) [search]. The boss is shown as an icon at the top of the map from the moment you enter a zone. ([KeenGamer guide](https://www.keengamer.com/articles/guides/slay-the-spire-2-beginners-guide-to-help-you-get-started/)) [search; attribution between this and the allthings.how guide uncertain]
- Twelve bosses across four zones, per a fan guide: Ceremonial Beast (252 HP) gains Strength each turn; dropping it below 150 HP stuns it and clears that Strength; its phase 2 uses Ringing, which limits you to one card that turn. Vantom (173 HP) starts with 9 Slippery charges that cut each of your hits to 1 damage. Waterfall Giant (250 HP) stacks Steam Eruption; at 0 HP it is stunned and invulnerable for a turn, then detonates for the stored damage. The Insatiable (321 HP) starts a four turn countdown to instant death on turn one; Frantic Escape cards extend it, each costing one more energy than the last. Knowledge Demon (379 HP): each debuff lets you choose one of two negative effects from a rotating pool. Kaiser Crab: if one claw dies the survivor gains 5 Strength and 99 Block that turn. Doormaker: when its Door dies it is stunned a turn, then retreats into a new Door with 20 more max HP. The Queen (400 HP): Chains of Binding makes your first three draws each turn Bound so you can play only one of them; her Amalgam ally flees when she dies. Test Subject has three lives (100, 200, 300 HP) with a new rule each: gains 2 Strength whenever you play a Skill, then adds Wounds when you take unblocked damage, then gains Intangible every other turn and adds Burns. ([allthings.how: every StS2 boss](https://allthings.how/every-boss-in-slay-the-spire-2-organized-by-act-and-location/))

### Inferences
- The three Act 3 bosses are three different engine checks: Time Eater taxes how many cards you play per turn, Awakened One taxes Power cards and wipes debuffs at rebirth, Donu and Deca block debuffs with Artifact and buff each other (target priority). Because the boss is random but visible on the map, players draft toward the boss they can see, and no single deck answer is safe across runs.
- Three bosses (Champ at 50%, Time Eater at 50%, Awakened One at 0) have a scripted reset that clears the player's debuffs and changes the pattern. The threshold becomes a planning point: hold debuffs and burst for after the reset, or burst through it.
- The Heart combines a burst cap (Invincible), a card tax (Beat of Death), and a fixed escalation clock (+2 Strength every 3 turns, then +10, then +50). It punishes both slow decks and decks that win with one giant turn, so it tests the whole deck rather than one stat.
- Alternating threat shapes (Heart: 12 small hits versus one 40 hit; Time Eater: 7x3 versus 26) mean the same defence is not always right, because multi hit attacks scale badly against Strength and well against flat Block.
- Slay the Spire 2's fan reported designs push further toward rule changers: per hit damage caps (Vantom), card per turn limits (Ringing, Chains of Binding), death timers you buy time against (Insatiable), and a player choice of which punishment to take (Knowledge Demon).

### Gaps
- No Mega Crit statement about how a specific boss (Time Eater, Awakened One, the Heart) was designed. The GDC 2019 talk is members only; a transcript summary site returned 403.
- The 12 card and 2 Strength values for Time Warp come only from a guide via search.
- Whether the attack number on an intent already includes Strength, Weak and Vulnerable was not confirmed in this session (from memory, unverified: it does).
- How the Heart is unlocked by keys (from memory, unverified: three keys collected in Acts 1 to 3) was not confirmed; the wiki page I read only gave the three character Act 3 requirement.

## 2. Into the Breach: telegraphed attacks, perfect information, and its final fight

### Takeaway
Into the Breach shows every Vek attack's direction and target before the player moves, so each turn is a puzzle about pushing, blocking or absorbing known threats; Subset designed it so "every death felt like your own fault". Its bosses are normal Vek types with one rule bent, and its final mission is a protect the bomb objective rather than a damage race.

### Cited Findings
- Justin Ma: "This lead us to use of telegraphed enemy attacks as a core mechanic." "the game starts to feel like a puzzle." "We prefer games with clear rulesets, and were interested in pursuing something with less randomness than FTL." "We wanted to make something where every death felt like your own fault." "it's very clear how or why you failed so that you can feel like you can improve." ([Game Developer, Road to the IGF: Into the Breach](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-))
- Ma on difficulty: "Balancing the difficulty of the game has been a numbers game." "A single additional enemy turns a battle from 'a fun challenge' to 'completely impossible'". ([Game Developer, Road to the IGF: Into the Breach](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-))
- Ma on avoiding one answer and on objectives: "we wouldn't want the player to find a single tactic and never have any incentive to try something else." "Just surviving, with minimal damage, until the end of the battle is often more important than eliminating the enemy." "you may choose to sacrifice a mech to save the city." ([Game Developer, Road to the IGF: Into the Breach](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-))
- "The player is shown the direction of the Vek unit's intended attack," along with each unit's remaining moves and health. Missions give "a fixed number of turns". "The limited turn counter was used to keep battles short" and "telegraphing the Vek's movements further helped to hasten the pace" (Wikipedia citing Shacknews). ([Wikipedia: Into the Breach](https://en.wikipedia.org/wiki/Into_the_Breach))
- Matthew Davis said part of the vision was a push to be more deterministic in moment to moment play. ([PC Gamer preview](https://pcgamer.com/into-the-breach-preview)) [search]
- Davis gave "Into the Breach Design Postmortem" at GDC 2019, covering what to cut, borrowing from other games and choosing the difficulty. I could not see its content. ([80.lv](https://80.lv/articles/gdc-2019-an-inside-look-at-into-the-breach)); ([GDC Vault](https://gdcvault.com/play/1025772/-Into-the-Breach-Design)) [search]
- Reviewers note almost no randomness apart from a small chance for attacks on buildings to miss the Power Grid. ([RPG Codex review](https://rpgcodex.net/content.php?id=10843)) [search; attribution among the reviews returned is uncertain]
- Final mission: the Volcanic Hive is a single two part mission that opens after you secure at least two islands. ([Giant Bomb: Volcanic Hive](https://giantbomb.com/wiki/Locations/Volcanic_Hive)) [search]. The underground half asks you to protect power coils and a large bomb until its timer reaches zero; the detonation ends the campaign. ([Gameranx guide](https://gameranx.com/features/id/141981/article/into-the-breach-an-easier-campaign-victory-guide-custom-squad-build/)) [search]. Players report that a destroyed bomb is replaced by a new bomb with a new timer, and the mission only fails if all grid power or all mechs are lost. ([Steam discussion](https://steamcommunity.com/app/590380/discussions/0/3041607080062373266)) [search; player reported]
- Bosses: Vek Leaders are boss type enemies, including Psion and Goo variants and robotic Bot Leaders. Psions are support Vek with 2 HP and 2 move whose passive buffs other Vek or debuffs your units. ([ludo.guide: Veks](https://www.ludo.guide/guide/into-the-breach/veks)) [search]
- The Scorpion Leader webs and attacks all four adjacent tiles in a cross and, unlike other Scorpions, pushes its targets; it has very high health but low movement and only a melee attack. ([ITB wiki: Scorpion Leader](https://intothebreach.fandom.com/wiki/Scorpion_Leader)) [search]
- The Shell Psion gives all other Vek Hardened Carapace (weapon damage reduced by 1), so players kill it first. ([ITB wiki: Shell Psion](https://intothebreach.fandom.com/wiki/Shell_Psion?oldid=4632)) [search]

### Inferences
- Into the Breach gets boss depth from the same rules as every fight: a Leader is a known Vek with one rule bent (cross attack plus push), and a Psion is a band buffer whose death strips a global buff. That is the same shape as Kindred Siege's bodyguard rule.
- The climax changes the win condition from "kill" to "protect an object for N turns", with a forgiving fallback (a replacement bomb). The final test is planning under pressure, not damage.
- Ma's warning that one extra enemy flips a fight from fun to impossible means reinforcement waves are the most sensitive tuning knob in a boss fight.

### Gaps
- Exact HP, attacks and rules of each Vek Leader, the final mission's turn counts and the identity of its boss units could not be verified (Fandom pages blocked).
- Content of the GDC 2019 postmortem is not public to me.

## 3. Darkest Dungeon 1 and 2: bosses as puzzles and composition checks

### Takeaway
Red Hook's bosses each turn one normal rule into a puzzle aimed at a party assumption: light (Shambler), rank 4 reach behind obstacles plus marked ranks (Prophet), a hero taken out (Hag), a hero turned against you (Siren), a target you must not hit (Swine Prince), and a forced sacrifice at HP thresholds (Heart of Darkness). Most counters use existing tools differently (Guard stops charm, zero damage stuns, debuffs on a once per round attacker), and the wiki lists preferred and discouraged heroes per boss, which is a composition check.

### Cited Findings
The Prophet (telegraph by marking ranks)
- He stands in rank 4 behind wooden pews on ranks 1 to 3 (small pew HP 25/38/51, large 55/83/113 by level). Breaking pews drops loot worth "up to 7500 gold if all three Pews are broken"; pews left standing vanish, with the reward, when he dies. ([DD wiki: Prophet](https://darkestdungeon.wiki.gg/wiki/Prophet))
- Each round, Prognostication (free action, start of round) marks one or two ranks, not heroes. Rubble of Ruin (free action, end of round) hits the marked ranks for 11 to 23, 15 to 30 or 23 to 45 by level. In between he takes one normal action: Eye On You (stun plus 11 stress) or Fulminate (blight on all four ranks). If a hero is above 50 stress, Eye On You's chance doubles on Veteran and triples on Champion. ([DD wiki: Prophet](https://darkestdungeon.wiki.gg/wiki/Prophet))
- Counters: he takes only one real turn per round, so debuffs are very effective (stacked Intimidate and Weakening Curse can reduce his damage to almost nothing). Defenders can Guard the targeted hero, though if Rubble hits both the guard's rank and the guarded rank, both take damage. ([DD wiki: Prophet](https://darkestdungeon.wiki.gg/wiki/Prophet))

The Shambler (rule: no light)
- It appears at 0 torchlight (ambush chance 1%, 8%, 12% by difficulty while travelling at 0 light) or when a torch is used on its altar. While it lives, light is locked at 0, heroes take 40% more stress, its abilities gain 25% damage, 5% crit and 12.5 accuracy, and the party is always surprised (formation scrambled). ([DD wiki: Shambler](https://darkestdungeon.wiki.gg/wiki/Shambler))
- Each round it summons 2 Tentacles, even if its attack misses. Tentacles stay after it dies, prefer Marked heroes, and each Clapperclaw stacks a self buff (+20 accuracy, +5% crit, +50% damage, +3 speed, +25% protection). Stentorious Lament (10 stress plus a party shuffle) is only used while a Tentacle lives. Any difficulty drops one of five Ancestral trinkets, so low level Shamblers are the efficient farm. ([DD wiki: Shambler](https://darkestdungeon.wiki.gg/wiki/Shambler))

The Hag (rule: one hero removed)
- On turn 1 she puts a hero in the Cauldron. That hero cannot act and takes 8.75% of max HP every time anyone acts (about 12 actions, roughly 2.5 rounds, from full to 0). Breaking the pot frees them into the front rank, but the pot respawns and she usually puts someone back in. The pot tips over by itself when the hero reaches Death's Door. A hero who dies in the pot counts as dead. ([DD wiki: Hag](https://darkestdungeon.wiki.gg/wiki/Hag))
- She acts twice per round; two of her skills (a debuff and a self heal of 6/10/14) are only used while someone is in the pot. The advice is to kill her fast rather than free the pot, and build to fight with three heroes. Preferred: high single target damage and Mark users. Discouraged: Antiquarian, Vestal, Plague Doctor, Crusader. A solo Leper cannot be potted. ([DD wiki: Hag](https://darkestdungeon.wiki.gg/wiki/Hag))

The Siren (rule: a hero is turned)
- Song of Desire has a 70% chance to take control of its target; the target can dodge, and even if hit has a flat 30% chance to resist regardless of debuff resistance. It is guaranteed on her first turn each round if nobody is charmed, at least one hero is not at Death's Door and the party has at least two heroes; about 25% on later turns. She targets the lowest debuff resistance hero 75% of the time and never the strictly highest. A charmed hero fights for her for "usually two full rounds". A crit on a charmed hero sends them back (with stress to the party); fleeing while a hero is charmed kills that hero. ([DD wiki: Siren](https://darkestdungeon.wiki.gg/wiki/Siren))
- Counters: a Guard (Houndmaster's Guard Dog, Man at Arms's Defender) on a hero stops that hero being charmed; swap out skills that would heal or buff her if your hero is charmed; change who she targets with debuff resistance (Holy Water); she acts twice per round, so marks and debuffs must be reapplied. ([DD wiki: Siren](https://darkestdungeon.wiki.gg/wiki/Siren))

The Collector (rule: your own past heroes)
- A roaming miniboss with a 3%, 4% or 5% chance by difficulty, only when the inventory is at least 79% full (13 slots). On turn 1 he summons 3 Collected heads of hero types: Highwayman (most dangerous, hits any rank, 3x more likely to target Marked heroes), Man at Arms (guards an ally, +25% protection, stuns), Vestal (heals and buffs, prefers the Collector). All Collected die with him; he resummons more often as they die. ([DD wiki: The Collector](https://darkestdungeon.wiki.gg/wiki/The_Collector_(Darkest_Dungeon)))
- Counters: stun him on turn 1 before he summons (low stun resistance), kill Highwaymen first, keep heroes above HP floors that survive a maximum Headhunt plus a bleed tick (20, 26, 34 by level). ([DD wiki: The Collector](https://darkestdungeon.wiki.gg/wiki/The_Collector_(Darkest_Dungeon)))

The Swine Prince (rule: do not hit the pet)
- The Prince fills ranks 1 to 3; Wilbur (17/26/35 HP) is in rank 4. Each round Wilbur marks one or two heroes; the Prince's Obliterate skills deal +100% damage to Marked heroes and clear the Mark; with no marks he uses the much weaker Wild Flailing. On Veteran and Champion, Wilbur also stuns heroes as a free action at the start of each round. ([DD wiki: Swine Prince](https://darkestdungeon.wiki.gg/wiki/Swine_Prince))
- Any damaging attack on Wilbur makes the Prince immediately use Enraged Destruction on all heroes (high damage, can stun); if Wilbur dies, the Prince uses it every round. Killing the Prince ends the quest, and you may retreat without killing Wilbur. Counters: kill the Prince first, use zero damage stuns on Wilbur (Bounty Hunter, Plague Doctor), clear Marks (Arbalest, Musketeer), and avoid Riposte because it can kill Wilbur by accident. ([DD wiki: Swine Prince](https://darkestdungeon.wiki.gg/wiki/Swine_Prince))

The Heart of Darkness (rule: thresholds demand a sacrifice)
- 252 HP (303 on Stygian/Bloodmoon). On its turn, if its HP is at or below 168 (two thirds) with 4 heroes alive, or at or below 84 (one third) with 3 alive, it uses Come Unto Your Maker: one hero dies instantly, bypassing HP and deathblow resistance, and the player chooses which. Stun does not prevent it; the only prevention is to take the Heart from above a threshold to dead before it acts. The wiki's strategy section gives 170 and 83 instead, unreconciled. ([DD wiki: Heart of Darkness](https://darkestdungeon.wiki.gg/wiki/Heart_of_Darkness))
- Advice: let healers or support heroes be the ones that die; buff one high damage hero and stack defence debuffs to burst through a threshold. The wiki documents runs where all four heroes survived. ([DD wiki: Heart of Darkness](https://darkestdungeon.wiki.gg/wiki/Heart_of_Darkness))

Escalation in DD1
- Bosses come in three levels with scaled stats, for example the Prophet has 105, 158 or 215 HP and speed 0, 1 or 2, and the Hag 66, 99 or 135 HP with speed 5, 6 or 7. ([DD wiki: Prophet](https://darkestdungeon.wiki.gg/wiki/Prophet)); ([DD wiki: Hag](https://darkestdungeon.wiki.gg/wiki/Hag))

Darkest Dungeon 2
- Confession bosses are not Ordained but receive "a set of buffs even more powerful than the usual Ordainment buffs", themed to the boss. ([DD wiki: Ordainment](https://darkestdungeon.wiki.gg/wiki/Ordainment)) [search]. Reaching maximum Loathing (a meter that builds while travelling) triggers penalties including +10% Confession boss health; winning fights lowers it. ([DD wiki: Loathing](https://darkestdungeon.wiki.gg/wiki/Loathing)) [search]
- DD2 bosses are described as "especially difficult enemies", often with large damage or stress, high HP, multiple actions per round, or unique mechanics. ([DD wiki: Bosses (DD2)](https://darkestdungeon.wiki.gg/wiki/Bosses_(Darkest_Dungeon_II))) [search]
- The Obsession boss's eyestalks apply a "seen" status that focuses all the eyes' attacks on that hero; the Ambition boss's plan is to "constantly buff itself up" across three phases; the final boss has three phases. ([Bogleech: DD2 Confession bosses](https://bogleech.com/halloween/hall23-darkestdungeon2bosses))
- Players describe the first Confession boss (Denial) as one where chains neutralize the party's buffing and healing. ([Steam discussion](https://steamcommunity.com/app/1940340/discussions/0/4407417073562911828)) [search; player opinion; attribution between two Steam threads uncertain]

Developer commentary
- Chris Bourassa on a cut DD2 region where every fight was one huge four slot enemy: "every fight would be a massive drawn-out slug fest", "a neat twist on the combat", and "if there's only one target you're able to really set-up and execute combos." ([GameBanshee, excerpting VG247](https://www.gamebanshee.com/94hfc))

### Inferences
- Darkest Dungeon telegraphs by placing a mark one phase before damage resolves (Prophet marks ranks at round start and hits at round end; Wilbur marks heroes for the Prince). The player answers by moving, guarding, clearing the mark, or killing the marker. That is the turn based version of a cast bar with a target.
- Each boss deletes one assumption: that you control all four heroes (Hag, Siren), that you may hit anything (Swine Prince), that light and formation are stable (Shambler), or that HP is the only clock (Heart of Darkness). The fixes are mostly skill choice and targeting discipline, not bigger numbers.
- The Heart of Darkness and the Prophet's pews both put a choice inside the fight: whom to lose, and whether to spend actions on loot before the kill.
- Bourassa's point implies a design tension: a lone boss target makes combos easy, so bosses with adds or bodyguards keep combos contested.

### Gaps
- No Red Hook commentary on DD1 boss design was found. A Designer Notes episode with Tyler Sigman exists but I did not open it. ([Designer Notes 34: Tyler Sigman, Part 2](https://www.designer-notes.com/?p=1385)) [search]
- Move lists for DD2's Confession bosses were not found.

## 4. Hades and Hades 2: phases, recurring bosses, Extreme Measures and the Vow of Rivals

### Takeaway
Supergiant's bosses recur every run, so depth comes from an opt in remix paid for with Heat: Extreme Measures gives each region's boss new rules per rank (a three Fury fight, a hazard arena, Theseus on a chariot with a new moveset, a third phase in darkness for Hades). Hades 2's Vow of Rivals repeats the idea with hazards, two on one fights and new attack patterns across twice as many Guardians.

### Cited Findings
- Extreme Measures ranks map to regions: rank 1 the Furies (Tartarus), rank 2 the Bone Hydra, rank 3 Theseus and Asterius, rank 4 the final boss (listed as "Redacted"). ([TrueAchievements walkthrough](https://www.trueachievements.com/game/Hades/walkthrough/7)) [search]
- Rank 1: one Fury is the main boss and her two sisters support her; the support sisters only have a couple of attacks and cannot be damaged. ([PC Invasion](https://www.pcinvasion.com/?p=221630)) [search]; ([Prima Games: Furies](https://primagames.com/?p=313614)) [search]
- Rank 2: the Bone Hydra fight moves to a room where much of the floor is lava. ([The Mary Sue](https://www.themarysue.com/?p=561766)) [search; attribution uncertain]
- Rank 3: Theseus rides a bronze chariot, max HP 12,000 instead of 9,000, loses all his normal attacks and gains Cluster Bombs and Center Dive. ([Hades wiki: Theseus](https://hades.fandom.com/wiki/Theseus?oldid=9683)) [search]. Asterius's slam makes a large shockwave and he does it up to three times. ([GameSkinny](https://www.gameskinny.com/fppcw/how-to-beat-theseus-and-the-minotaur-in-hades)) [search]. In the normal fight, if Asterius dies first, Theseus enrages and moves and attacks faster. ([Hades wiki: Theseus](https://hades.fandom.com/wiki/Theseus?oldid=9683)) [search]
- Rank 4: bought from the House Contractor for 100 gems after the true ending; the guide says activating it "pushes you all the way to 10 Heat". In the Hades fight, after you drop him the second time a third phase begins in which he shrouds the arena in darkness. ([Prima Games: true ending and Extreme Measures 4](https://primagames.com/gaming/hades-true-ending-persephone-extreme-measures-4)) [search]
- Supergiant's own patch notes show the remixes being tuned both ways: "The Fury Sisters: increased health and attack speed under Extreme Measures (Pact)"; "Theseus: reduced damage of explosive attack under Extreme Measures (Pact)"; a new prophecy, "Sea-God's Spite", rewards beating Theseus under Extreme Measures; the Bone Hydra was reworked so "the main head now has 4 new variants". ([Supergiant: The Blood Price update patch notes](https://www.supergiantgames.com/blog/hades-the-blood-price-update-patch-notes))
- Hades 2: The Unseen Update (PC, 18 June 2025) added the Oath of the Unseen's Vow of Rivals, "powered-up versions" of the Guardians with unique mechanics. ([Final Weapon](https://finalweapon.net/2025/06/18/hades-ii-the-unseen-update-is-now-available/)) [search]; ([Windows Central](https://www.windowscentral.com/gaming/pc-gaming/hades-2-unseen-update)) [search]
- Vow of Rivals ranks unlock in order: rank 1 after beating Hecate twice and Polyphemus once, then speaking to Hecate with Fear; rank 2 after beating Hecate and Polyphemus with the Vow active, Charybdis once, and Scylla and Eris three times. ([Hades wiki: Oath of the Unseen](https://hades.fandom.com/wiki/Oath_of_the_Unseen)) [search]. At the higher rank a rival joins the boss for a two on one fight; for example Chronos joins the summit fight, cannot be damaged, and replaces Zeus's stun with Time Bubbles. ([RogueRanker: Vow of Rivals guide](https://rogueranker.com/?p=12237)) [search]
- A critic says hazardous environments and two on one fights return from Extreme Measures, Guardians get "a lot of new attack patterns", it applies to both routes, and there are twice as many Guardians as in the first game; the critic calls it "a much harder and more improved version of Extreme Measures". ([Gfinity](https://www.gfinityesports.com/article/hades-2s-vow-of-rivals-outshines-the-originals-extreme-measures))

### Inferences
- Supergiant's escalation for recurring bosses is "new rules for a boss you already know", chosen by the player and priced in Heat. Its tools are a second body (a support sister, a rival), a hazard arena, a replaced moveset, and an extra phase. Health still rises (Theseus +3,000), but it is not the main change.
- The patch notes show that remixed moves needed tuning in both directions, so a remix is new content to balance, not a free difficulty dial.

### Gaps
- Exact Heat cost per Extreme Measures rank and the full rank 4 Hades moveset (Fandom pages blocked, guide pages returned 403). From memory, unverified: ranks cost 1, 2, 3 and 4 Heat, which would match the "10 Heat" figure.
- The normal phase structure of each Hades boss was not verified in this session.
- No Supergiant designer commentary on why Extreme Measures exists was found.

## 5. Inscryption, Monster Train, Wildfrost, Griftlands, Fire Emblem, Advance Wars and XCOM 2

### Takeaway
Outside Slay the Spire the same tools recur: bosses that rewrite one board rule (Inscryption), visible countdowns and waves (Wildfrost), escalation ladders (Monster Train's 25 Covenant ranks), breakable armour that stuns when fully broken (Fire Emblem: Three Houses), objective boss maps (Advance Wars), and a nemesis that adapts across a campaign with rolled strengths and weaknesses (XCOM 2's Chosen). XCOM's Alien Rulers also show a reaction rule that was too strong and had to be scaled back.

### Cited Findings
Inscryption (Daniel Mullins)
- Each Act 1 cabin boss has two phases. The Prospector's pack mule moves across his row and blocks attacks; killing it adds random creature cards to your hand; in phase 2 he turns your played cards into gold. The Angler's hook steals your most recently played card; phase 2 brings Great Whites. The Trapper's traps destroy your card opposite them when destroyed and your creatures become pelts; in phase 2 he becomes the Trader, whose full board you buy with pelts. ([Pocket Tactics: Leshy](https://www.pockettactics.com/inscryption/leshy)) [search]; ([TechRaptor boss guide](https://Techraptor.net/gaming/guides/inscryption-boss-battle-guide)) [search]; ([Pro Game Guides: Trapper and Trader](https://progameguides.com/inscryption/how-to-beat-the-trapper-and-trader-in-inscryption/)) [search]
- In the later Leshy fight (Scrybe of Beasts), when one of your cards dies he uses his camera to capture aspects of it. ([Pro Game Guides: Scrybe of Beasts](https://progameguides.com/inscryption/how-to-beat-the-scrybe-of-beasts-in-inscryption/)) [search]
- Mullins's GDC 2022 talk was "Sacrifices Were Made: The Inscryption Post-Mortem"; I found no boss specific commentary. ([Wikipedia: Inscryption](https://en.wikipedia.org/wiki/Inscryption)) [search]

Monster Train (Shiny Shoe)
- There are 25 Covenant levels "that have increasing difficulty modifiers like increased enemy hit points, debuffs to your troops, and other nasty effects." ([Seasoned Gaming review](https://seasonedgaming.com/2021/03/30/review-monster-train-can-you-keep-hell-from-freezing-over/)) [search]
- Players list enemy stat boosts and higher shop costs as Covenant levers; an earlier Covenant 10 dazed units entering the floor below the Pyre. ([Steam discussion](https://steamcommunity.com/app/1102190/discussions/0/2527030866856060084)). A later patch, quoted by a player, changed it so "Covenant Rank 10 now gives Emberdrain to the first unit played to the top floor each turn" (one less ember next turn). ([Steam discussion](https://steamcommunity.com/app/1102190/discussions/0/3092275748073975130)) [search; player quoting a patch note; exact thread uncertain]
- Normal enemies get one round of combat per floor before ascending, but a boss fight is all or nothing. ([Gamereactor beta impressions](https://ipv6.gamereactor.eu/monster-train-beta-impressions/)) [search]. In Monster Train 2 players describe the Relentless boss stage as the boss moving up floors and dealing Pyre damage as it advances past floors it has cleared. ([Steam discussion, Monster Train 2](https://steamcommunity.com/app/2742830/discussions/0/591764116171932268)) [search; player reported]
- The final boss Seraph has several versions with different rules; one applies 3 Sap to your units on a floor every turn and places Lightwings each turn, another, in one player's words, "attacks himself every turn AND casts melee weakness on your guys". ([Monster Train wiki](https://monster-train.fandom.com/wiki/File:Stemperant.png)) [search]; ([Steam discussion](https://steamcommunity.com/app/1102190/discussions/0/2953754543766072756/?ctp=4)) [search; player reported; exact thread uncertain]

Wildfrost
- Enemies act on visible counters, and enemy counters count down first; the wave deployer shows how many turns until the next wave. In boss battles the battle ends as soon as the boss dies, whatever else is left on the board. ([Wildfrost wiki: Bosses](https://wildfrostwiki.com/Bosses)) [search]; ([Wildfrost wiki: Enemies](https://wildfrostwiki.com/Enemies)) [search]
- The usual final boss (Eye of the Storm) is the Frost Guardian, always with 3 other enemies and 2 more waves waiting; whenever it loses any health it grants attack to itself and all allies currently in battle. The true final fight needs the Lumin Vase and starts with 6 enemies and no waves. ([Wildfrost wiki: Eye of the Storm](https://wildfrostwiki.com/Eye_of_the_Storm)) [search]; ([Wildfrost wiki: Heart of the Storm](https://wildfrostwiki.com/Heart_of_the_Storm)) [search]

Griftlands (Klei)
- Negotiation is a second card fight: you attack the opponent's core argument (resolve); side arguments give ongoing buffs or damage, and some are invulnerable. ([GameSkinny](https://www.gameskinny.com/sqa55/griftlands-guide-what-are-negotiations-and-how-do-they-work)) [search]
- Opponents broadcast their attacks one move in advance, so you can plan defences. ([PC Gamer, early access](https://www.pcgamer.com/uk/griftlands-complex-double-deck-building-shows-promise-in-early-access)) [search]
- Each day ends in a big boss fight; boss encounters cannot be avoided, but parleying can make them less difficult. ([Turn Based Lovers review](https://turnbasedlovers.com/review/griftlands-tbl/)) [search]; ([TechRaptor preview](https://techraptor.net/content/griftlands-preview)) [search]

Fire Emblem: Three Houses monsters
- Each tile a monster occupies has its own barrier. Attacks on an intact barrier tile deal 50% less damage (70% less for Dragon Scale Wall and Essence Barrier). A barrier breaks after up to two rounds of combat, or instantly if it is the primary target of a gambit or takes damage from a weapon or art it is weak to. Breaking one barrier stops the monster counterattacking; breaking all of them causes an armor break, so it cannot act in its next phase (once per monster). It can repair barriers, usually after preparing a Staggering Blow. ([Fire Emblem Wiki: Barrier](https://fireemblemwiki.org/wiki/Enclosing_Wall))
- Monsters have several health bars ("stocks") and sometimes build up power, then unleash a very strong attack the following turn; barriers not fully broken by then regenerate. ([Gameranx](https://gameranx.com/features/id/178155/article/fire-emblem-three-houses-how-to-defeat-giant-monsters-battle-guide/)) [search]

Advance Wars
- Boss maps change the objective: in Advance Wars 2's Black Hole Citadel you must destroy three Black Cannons at the back of Sturm's army, choosing three COs with no two from the same army. ([Let's Play Archive](https://lparchive.org/Advance-Wars-2/Update%2047)) [search]
- In Dual Strike, Black Crystals heal nearby Black Hole units, and the Black Obelisk looks like a Black Cannon but works like a Black Crystal. ([Wikipedia: Advance Wars: Dual Strike](https://en.wikipedia.org/wiki/Advance_Wars:_Dual_Strike)) [search]

XCOM 2: the Chosen and the Alien Rulers (Firaxis)
- The three Chosen (Assassin: stealth and melee; Hunter: a long range rifle whose bullets track targets; Warlock: psionics that can control several of your units at once) are tasked with recapturing the Commander. They "gain new exclusive abilities over time", can eventually attack the Avenger directly, keep returning after defeat, and can only be killed for good once their stronghold is destroyed. ([Wikipedia: XCOM 2: War of the Chosen](https://en.wikipedia.org/wiki/XCOM_2:_War_of_the_Chosen))
- Their strengths and weaknesses are procedurally generated per campaign and shown when a Chosen reveals itself; each is weak to one of the three Resistance factions and has weak points such as extra damage from explosives. Their main aim is to daze and kidnap soldiers to raise a knowledge bar, which grants more battlefield strengths and strategy layer pressure; a full bar lets them attack your base. An adjacent ally can revive a dazed soldier before capture. ([Gameranx: beating the Chosen](https://gameranx.com/features/id/118207/article/xcom-2-war-of-the-chosen-how-to-beat-the-chosen-boss-tips-tricks/)) [search]; ([PC Gamer review](https://pcgamer.com/xcom-2-war-of-the-chosen-review)) [search]
- Jake Solomon has said the nemesis style enemies were a late pivot after a giant mech prototype did not work with the camera. ([PCGamesN](https://www.pcgamesn.com/xcom-2/mechs-war-of-the-chosen)) [search]
- Alien Rulers (Alien Hunters DLC) originally took an action after every single action your soldiers performed, including free actions like a reload or a Battle Scanner. ([ludo.guide: Alien Rulers](https://www.ludo.guide/guide/xcom-2/dlc-content-overview/alien-rulers-alien-hunters-dlc)) [search]. War of the Chosen scaled this back: they only get a free move in response to an attack or a movement, and only within their line of sight; reloads and some abilities no longer trigger them; and they guard their own hideouts. ([GameBanshee, quoting IGN](https://www.gamebanshee.com/kdawd)) [search; written before release]

### Inferences
- Inscryption and Wildfrost use bosses that rewrite one rule of the board (your cards turn to gold, your played card is stolen, every point of damage buffs the enemy side), which changes how good each of your existing cards is rather than how big the numbers are.
- Three Houses gives a second answer to a telegraphed big attack: break every barrier during the warning turn and the monster loses its action; fail and the barriers come back. The right tool (gambits, effective weapons) breaks barriers instantly, so a composition with those tools has a cheaper answer.
- XCOM's rolled strengths and weaknesses stop one memorised answer from working every campaign. The Rulers' every action reaction rule shows how a reaction trigger can feel unfair, and the fix was to narrow the triggers to attacks and moves in sight.
- Advance Wars and Into the Breach both make the climax about an objective (destroy the cannons, protect the bomb), which changes which units matter.

### Gaps
- Monster Train: the exact Relentless rule, each Seraph version's abilities and the Covenant ranks that affect bosses were not verified (Fandom blocked), and no Shiny Shoe designer commentary was found.
- Inscryption: the Leshy final phase card (the Moon) and the opponent's queued back row could not be sourced.
- Griftlands: no specific boss negotiation was found.
- XCOM 2: exact knowledge thresholds, the strength and weakness pools, and Firaxis commentary on adaptation were not found.
- Advance Wars: Black Cannon damage and ranges not found.

## 6. Across these games: telegraphs without reflexes, rewarding planning over stats, escalation, and avoiding one dominant answer

### Takeaway
In games without real time reflexes, the boss's next move is shown as who or where, how big, and when, and good telegraphs leave at least two answers with different costs. Planning beats stats when the boss caps or taxes the raw power plan (damage caps, card taxes, debuff cleanses at thresholds, reprisals for the wrong target) and when thresholds force a choice of what to lose. Escalation mostly adds rules, bodies and phases to known bosses; dominant answers are avoided with boss pools, rolled traits and bosses that each favour a different composition.

### Cited Findings
How the next move is shown
- Icon plus a damage band for the next action, with Unknown reserved for transformations (Slay the Spire). ([StS wiki: Intent](https://slaythespire.wiki.gg/wiki/Intent))
- Exact direction and target tiles (Into the Breach). ([Wikipedia: Into the Breach](https://en.wikipedia.org/wiki/Into_the_Breach))
- Turn counters on every enemy and on the next wave (Wildfrost). ([Wildfrost wiki: Bosses](https://wildfrostwiki.com/Bosses)) [search]
- A mark placed a phase before the damage: Prophet marks ranks at the start of the round and hits them at the end; Wilbur marks heroes for the Prince's double damage. ([DD wiki: Prophet](https://darkestdungeon.wiki.gg/wiki/Prophet)); ([DD wiki: Swine Prince](https://darkestdungeon.wiki.gg/wiki/Swine_Prince))
- A one turn charge before a huge attack (Three Houses monsters). ([Gameranx](https://gameranx.com/features/id/178155/article/fire-emblem-three-houses-how-to-defeat-giant-monsters-battle-guide/)) [search]
- One move ahead intents in a second card fight (Griftlands). ([PC Gamer, early access](https://www.pcgamer.com/uk/griftlands-complex-double-deck-building-shows-promise-in-early-access)) [search]
- The boss itself shown before the act starts (Slay the Spire, Slay the Spire 2). ([StS wiki: Bosses](https://slaythespire.wiki.gg/wiki/Bosses)); ([KeenGamer](https://www.keengamer.com/articles/guides/slay-the-spire-2-beginners-guide-to-help-you-get-started/)) [search]
- Hidden information as a paid trade (Runic Dome: no intents for an extra Energy each turn). ([StS wiki: Intent](https://slaythespire.wiki.gg/wiki/Intent))

Rules that reward planning over stats
- Damage cap per turn: Heart Invincible 300 (200 at A19). Card tax: Heart Beat of Death 1 (2) per card; Time Eater ends your turn after a set number of cards. Power tax: Awakened One gains Strength per Power. ([StS wiki: Corrupt Heart](https://slaythespire.wiki.gg/wiki/Corrupt_Heart)); ([StS wiki: Time Warp](https://slaythespire.wiki.gg/wiki/Time_Warp)); ([StS wiki: Awakened One](https://slaythespire.wiki.gg/wiki/Awakened_One))
- Threshold cleanses: Champ's Anger and Time Eater's Haste at 50%, Awakened One's Rebirth all remove the player's debuffs. ([StS wiki: The Champ](https://slaythespire.wiki.gg/wiki/The_Champ)); ([StS wiki: Time Eater](https://slaythespire.wiki.gg/wiki/Time_Eater)); ([StS wiki: Awakened One](https://slaythespire.wiki.gg/wiki/Awakened_One))
- Debuff shields: Donu and Deca's 2 (3) Artifact. HP scaled hit: Hexaghost's Divider. ([StS wiki: Donu and Deca](https://slaythespire.wiki.gg/wiki/Donu_and_Deca)); ([StS wiki: Hexaghost](https://slaythespire.wiki.gg/wiki/Hexaghost))
- Reprisal for the wrong target (Swine Prince), a hero removed (Hag) or turned (Siren), and a player chosen sacrifice at thresholds unless you burst past them (Heart of Darkness). ([DD wiki: Swine Prince](https://darkestdungeon.wiki.gg/wiki/Swine_Prince)); ([DD wiki: Hag](https://darkestdungeon.wiki.gg/wiki/Hag)); ([DD wiki: Siren](https://darkestdungeon.wiki.gg/wiki/Siren)); ([DD wiki: Heart of Darkness](https://darkestdungeon.wiki.gg/wiki/Heart_of_Darkness))
- Damage taken buffs the whole enemy side (Wildfrost's Frost Guardian); per hit damage cut to 1 for 9 charges (StS2 Vantom); a death countdown you pay to extend (StS2 Insatiable). ([Wildfrost wiki: Eye of the Storm](https://wildfrostwiki.com/Eye_of_the_Storm)) [search]; ([allthings.how](https://allthings.how/every-boss-in-slay-the-spire-2-organized-by-act-and-location/))
- Breakable armour where the right tool breaks it instantly and a full break costs the boss its next action (Three Houses). ([Fire Emblem Wiki: Barrier](https://fireemblemwiki.org/wiki/Enclosing_Wall))

Escalation across a run or a ladder
- Slay the Spire Ascension: stats at A4 and A9, new movesets at A19, two bosses at A20. ([StS wiki: Bosses](https://slaythespire.wiki.gg/wiki/Bosses))
- Hades Extreme Measures and Hades 2 Vow of Rivals: per boss remixes bought with Heat. ([Supergiant patch notes](https://www.supergiantgames.com/blog/hades-the-blood-price-update-patch-notes)); ([Gfinity](https://www.gfinityesports.com/article/hades-2s-vow-of-rivals-outshines-the-originals-extreme-measures))
- DD1 boss levels with scaled stats; DD2 Loathing adds 10% boss health at maximum. ([DD wiki: Prophet](https://darkestdungeon.wiki.gg/wiki/Prophet)); ([DD wiki: Loathing](https://darkestdungeon.wiki.gg/wiki/Loathing)) [search]
- Monster Train's 25 Covenant ranks; XCOM's Chosen gain abilities as knowledge rises. ([Seasoned Gaming](https://seasonedgaming.com/2021/03/30/review-monster-train-can-you-keep-hell-from-freezing-over/)) [search]; ([Wikipedia: War of the Chosen](https://en.wikipedia.org/wiki/XCOM_2:_War_of_the_Chosen))

Avoiding one dominant answer
- Stated goals: Giovannetti wanted enemies "that would challenge different strategies in unique ways" and no "single dominant strategy"; Ma did not want players "to find a single tactic and never have any incentive to try something else." ([Game Developer: Slay the Spire](https://www.gamedeveloper.com/disciplines/road-to-the-igf-mega-crit-games-i-slay-the-spire-i-)); ([Game Developer: Into the Breach](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-))
- Random boss from a visible pool of three per act (Slay the Spire); rolled strengths and weaknesses per campaign (XCOM); several versions of the same boss (Monster Train's Seraph, Hades' Bone Hydra with 4 head variants). ([StS wiki: Bosses](https://slaythespire.wiki.gg/wiki/Bosses)); ([Gameranx: Chosen](https://gameranx.com/features/id/118207/article/xcom-2-war-of-the-chosen-how-to-beat-the-chosen-boss-tips-tricks/)) [search]; ([Supergiant patch notes](https://www.supergiantgames.com/blog/hades-the-blood-price-update-patch-notes))
- Different bosses favour different parties: the Hag punishes healers and slow damage, the Prophet rewards debuffers and Guard, the Swine Prince rewards zero damage stuns and punishes Riposte. ([DD wiki: Hag](https://darkestdungeon.wiki.gg/wiki/Hag)); ([DD wiki: Prophet](https://darkestdungeon.wiki.gg/wiki/Prophet)); ([DD wiki: Swine Prince](https://darkestdungeon.wiki.gg/wiki/Swine_Prince))
- The player picks the punishment: StS2's Knowledge Demon offers a choice of two negative effects. ([allthings.how](https://allthings.how/every-boss-in-slay-the-spire-2-organized-by-act-and-location/))

### Inferences
- A good telegraph in these games answers three questions (who or where, how big, when) and is paired with more than one valid answer at different costs: move, block, guard, cleanse the mark, kill the marker, or break armour in time.
- Depth comes from bosses that change the value of the player's existing tools. The same deck or party is great against one boss and weak against another, and the boss is visible early enough to plan for.
- Randomness is kept before the decision (which boss, which move, shown in advance) and kept out of the result, which is why Slay the Spire's weighted random move choice (Time Eater 45/35/20 with no repeat rules) still reads as fair.

### Gaps
- No developer source found that compares telegraph styles directly; the cross game pattern here is my synthesis.

## 7. What could transfer to Kindred Siege's chief fights

### Takeaway
Kindred Siege already has the core of a Slay the Spire or Into the Breach style boss: a 3 second tell answered by an order. The biggest gains would come from (1) tells that show target and shape so different orders answer different shapes, (2) one rule per chief that taxes a specific mark or order, so each build meets good and bad chiefs, (3) decisions at the existing half and quarter health thresholds, and (4) escalating chiefs across tiers by remixing moves rather than adding health. Everything below is my inference from the sources above, not tested in the Kindred Siege sim.

### Cited Findings
- Slay the Spire shows the boss on the map from the start of the act and draws it from three per act. ([StS wiki: Bosses](https://slaythespire.wiki.gg/wiki/Bosses))
- The Heart alternates 12 small hits with one 40 hit and caps its HP loss at 300 per turn. ([StS wiki: Corrupt Heart](https://slaythespire.wiki.gg/wiki/Corrupt_Heart))
- A Guard on a Darkest Dungeon hero prevents the Siren's charm. ([DD wiki: Siren](https://darkestdungeon.wiki.gg/wiki/Siren))
- Hitting Wilbur triggers the Swine Prince's Enraged Destruction, and Riposte can kill Wilbur by accident. ([DD wiki: Swine Prince](https://darkestdungeon.wiki.gg/wiki/Swine_Prince))
- Three Houses monsters lose their next action when every barrier breaks, and barriers regenerate after a charged attack. ([Fire Emblem Wiki: Barrier](https://fireemblemwiki.org/wiki/Enclosing_Wall))
- XCOM's Ruler reaction to every soldier action was scaled back to attacks and moves in sight. ([GameBanshee, quoting IGN](https://www.gamebanshee.com/kdawd)) [search]
- "A single additional enemy turns a battle from 'a fun challenge' to 'completely impossible'". ([Game Developer: Into the Breach](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-))
- "if there's only one target you're able to really set-up and execute combos." ([GameBanshee, excerpting VG247](https://www.gamebanshee.com/94hfc))

### Inferences
1. Scout the chief. Show the chief, his chief spell and his one special rule at the start of the road (Slay the Spire's map portrait), so drafting gear and classes is the first counter.
2. The tell should say target and shape, not just "something is coming". Mark which fighter or area is hit and whether it is one big hit, a lunge, a flurry or a grab (StS intents, Prophet's marked ranks, Wilbur's marks). Then let different orders answer different shapes: a Hold parry beats one big hit but not a flurry, Fall Back beats a lunge or bite, a Warden Guard soaks a flurry on one ally. Give each answer a cost, so it is a choice and not a quiz.
3. Alternate two threat shapes per chief, as the Heart and Time Eater do, so "always Hold" loses and the player has to read the tell.
4. Give each chief one rule that taxes one engine. Examples: gains nerve armour whenever a Bleed ticks (taxes Trapper builds); ignores the first few Snares like Artifact; every hit on him buffs his band (Frost Guardian, taxes many small Skirmisher hits); each hit is cut to a small cap for a number of charges (Vantom, taxes big Aimed Shots); gains power whenever the player spends an order (Beat of Death, taxes order spam). Because chiefs are known in advance, a Bleed squad fears one chief and loves another.
5. Make the half health phase a decision, not just a stat jump. At half health the chief could cleanse marks (Champ, Time Eater, Awakened One) unless an Unleash lands during a short window, or demand a toll where the player picks which fighter takes it (Heart of Darkness). Saving Unleash for the threshold then becomes a plan.
6. Cap health loss per time window (Heart's Invincible) but leave nerve damage uncapped, so burst builds cannot delete a chief and the capture route becomes the elegant answer for strong squads.
7. A pet or banner you must not hit (Swine Prince). Since fighters pick their own targets, this turns Focus Fire on the chief and Hold into tools of restraint. Bleed ticks and auto attacks are the Riposte risk, so the targeting AI must be readable or the rule will feel unfair.
8. Take or turn a fighter (Hag's pot, Siren's charm). During the tell, a Warden Guard on the marked fighter prevents it, which reuses an existing mark as the counter; Focus Fire on the cage frees them early. Check that freeing is not always the wrong move, as it is against the Hag.
9. Breakable plates with a stagger (Three Houses). During the 3 second tell, an Aimed Shot or a Snare breaks a plate; break them all before the tell ends and the chief spell is cancelled. That gives a build based second answer next to the order based one.
10. Optional greed objectives (the Prophet's pews pay up to 7,500 gold but vanish when he dies). Captives or totems in the chief's camp that pay out only if freed before he breaks.
11. Escalate by remix at higher tiers (Ascension 19 movesets, Extreme Measures, Vow of Rivals): a changed chief spell, a hazard in the 2.5D arena, or a rival chief from another tribe joining at phase 2. Supergiant still had to tune each remix both ways.
12. Rolled traits (XCOM Chosen): each chief rolls one strength and one weakness from a pool keyed to marks and classes, shown on the road. Keep any reaction trigger narrow and visible, as the Rulers' fix shows.
13. Reinforcements are the touchiest knob (Ma). Test the two phase 2 reserves across builds with the autopilot before adding more.
14. When the band falls, combos open (Bourassa). Since marks do not land on the chief while his band stands, a short window right after the last bodyguard falls (or after a full plate break) where marks land extra hard would reward timing.
15. Hidden information as a gear trade (Runic Dome): a gear piece that gives an extra order point per fight but shortens or hides chief tells.
16. Determinism: the chief's choice of spell can stay seeded by the deterministic RNG, because Slay the Spire shows that weighted random moves read as fair when the result is shown before the player commits.

### Gaps
- None of these ideas has been tested in the Kindred Siege simulation, and I did not check them against DESIGN_LOCKS.md; they should be checked against the locked design before any pass.
- Whether 3 seconds is long enough to read a target plus a shape and pick an order in a fast paced battle was not researched (Hades, a real time game, is the closer reference and its telegraph timings were not covered).
- How readable the auto targeting is matters for ideas 7 and 8 and is unknown from here.
