# Item drops in roguelikes and loot games: where items come from, how offers are chosen, and how they steer builds

Source labels used below: **official** (developer or publisher), **fan wiki** (community wiki, usually data mined from game files), **guide** (press or strategy site), **forum** (player post), **blog** (opinion). Fandom wikis refused the fetch tool (HTTP 402), so a few Slay the Spire, Diablo and Monster Hunter numbers come from search engine extracts of Fandom pages; those are marked "via search extract". Kindred Siege facts come from the repo's `DESIGN_LOCKS.md` and `CLAUDE.md`, cited by line.

## 1. Reward shapes: pick one of three, boss and elite drops, shops, carousels, priced chests, crafting, item rooms

### Takeaway
Proven games layer three shapes: a small pick after most fights (usually one of three), a guaranteed bigger pick at elites, bosses or fixed times that often comes from its own pool, and a spend point (shop, priced chest, printer) where currency buys a targeted item. Crafting (TFT components, Vampire Survivors evolutions) gives direction without forcing, because two ordinary items the player chose become one build defining item.

### Cited Findings

#### Kindred Siege today (local design locks)
- A won path fight lets you strip one piece of the beaten side's gear or bank materials that go home. Banked materials can be swapped for one piece of their gear before the next fight. ([DESIGN_LOCKS.md lines 277, 319](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))
- Gear has three tiers: war parties drop Pauper, trials and duels drop Mid, elites and chiefs drop Princess. One gear slot per fighter. Each piece raises one stat (damage, health, Hit or Avoid) by 5, 10 or 15 (percent for damage and health, points for Hit and Avoid). A Princess piece also carries its chief's special. Run gear is lost at run end; materials come home. ([DESIGN_LOCKS.md lines 283, 318](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))
- Kill the chief and you take his Princess gear. Capture him and his whole tribe joins you instead. ([DESIGN_LOCKS.md lines 275, 276, 319](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))
- Two fighters of one tribe in the squad give its set bonus; three also bring its doctrine. ([DESIGN_LOCKS.md lines 285, 317](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))
- Each tribe's road is a small map of 3 lanes and 3 rows, then the chief. Every node shows what it is and what it drops. Events give 2 or 3 choices and cost one fight on that road. Objectives raise a fight's drop one tier. Omens add 25% materials each, and two or more raise every gear drop a tier. ([DESIGN_LOCKS.md lines 322 to 325](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))
- A won fight or an event never offers a piece you already hold (same tribe, stat and tier); it moves on to the next stat. ([DESIGN_LOCKS.md lines 336, 347](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))
- Pals forge gear from what you won, and the Pal who broke a chief learns that chief's gear recipe. ([DESIGN_LOCKS.md line 88](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))
- Starting values: about two fights before each chief, a run of 30 to 40 minutes. A run meets one tribe from tier 1, one from tier 2, then the hunters. ([DESIGN_LOCKS.md lines 208, 395, 405](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))

#### Slay the Spire (Mega Crit)
- Card reward base odds: normal fights 60% common, 37% uncommon, 3% rare. Elites 50%, 40%, 10%. Shop cards 54%, 37%, 9%. ([Slay the Spire wiki.gg, Cards](https://slaythespire.wiki.gg/wiki/Cards), fan wiki)
- Boss card rewards are always rare. ([Slay the Spire Fandom wiki, Card Rewards](https://slay-the-spire.fandom.com/wiki/Card_Rewards), fan wiki via search extract)
- Reward cards can arrive upgraded: base chance 25% in Act 2 and 50% in Act 3. Rares are never upgraded by chance. Ascension 12 and above halves these chances. ([Fandom Card Rewards](https://slay-the-spire.fandom.com/wiki/Card_Rewards), fan wiki via search extract; [HighGround Gaming Ascension guide](https://www.highgroundgaming.com/slay-the-spire-ascension-guide/), guide)
- Random relic rarity: Common 50%, Uncommon 33%, Rare 17%. Chests (common/uncommon/rare): small 75/25/0, medium 35/50/15, large 0/75/25. After each boss you choose 1 of 3 boss relics, and boss relics come only from bosses. In shops the third relic slot is always a Shop relic (20 Shop relics, 4 per character). Elites are a relic source with no separate odds listed. ([Slay the Spire wiki.gg, Relics](https://slaythespire.wiki.gg/wiki/Relics), fan wiki)
- The boss relic Busted Crown gives +1 energy but "Future card rewards have 2 less cards to choose from", so the size of the offer is itself a tunable reward. ([Slay the Spire wiki.gg, Relics List](https://slaythespire.wiki.gg/wiki/Relics_List), fan wiki)

#### Hades (Supergiant Games)
- Each god's boon offer gives three choices. ([Prima Games boon guide](https://primagames.com/gaming/hades-guide-olympian-god-boons-skills/2), guide)
- Chamber doors show the god's symbol, so you know whose boon is behind a door before you pick the route. Boons are lost on death. Charon's shop sells boons from random stock. Poms of Power level up boons you hold. ([Hades Fextralife wiki, Boons](https://hades.wiki.fextralife.com/Boons), fan wiki)
- Mirror of Night talents: Dark Foresight raises the chance of "Gold Laurel" chamber rewards (Boons, Daedalus Hammers, Charon's Obols, Poms of Power) by 2% per rank, 10 ranks. Fated Authority rerolls the next chamber's reward, 1 roll per rank, 8 ranks. Fated Persuasion rerolls Boon and Well of Charon choices, 1 roll per rank, 4 ranks. ([Hades Fextralife wiki, Mirror of Night](https://hades.wiki.fextralife.com/Mirror+of+Night), fan wiki)

#### Risk of Rain 2 (Hopoo Games)
- Small Chest, base $25: 79.2% common, 19.8% uncommon, 0.99% legendary. Large Chest, base $50 ("cost twice as much"): 0% common, 80% uncommon, 20% legendary, always at least uncommon. ([Risk of Rain 2 wiki.gg, Chests](https://riskofrain2.wiki.gg/wiki/Chests), fan wiki)
- After a teleporter boss, each uncommon item that would drop has an independent 15% chance to be replaced by that boss's own boss item. Bosses with no boss item (Scavenger, Horde of Many) drop only uncommons. Boss items never come from chests. ([Rogue Ranker, yellow items](https://rogueranker.com/yellow-items-ror2/), guide)
- Lunar items cannot drop from chests. They are bought with Lunar Coins from Lunar Pods or in the Bazaar Between Time. ([Pocket Tactics, lunar items](https://www.pockettactics.com/risk-of-rain-2/lunar-items), guide; [Rogue Ranker, lunar items](https://rogueranker.com/risk-of-rain-2-lunar-items/), guide)
- A 3D Printer gives the item it shows and takes a random item of the same tier from you. ([Steam forum](https://steamcommunity.com/app/632360/discussions/0/1768134097435781647), forum)

#### Teamfight Tactics (Riot Games)
- Patch 14.5: at PvE rounds "All players now get the same tier (quality) of loot, and the same amount of orbs". Contents differ but "the general value will not". Riot's reason: orbs were added for variance, and the game now has other variance (Portals, Augments, Headliner, soon Encounters), so it can "take just a small bit of variance out of the game". When one player gets a gold opener, everyone does. Orb contents scale through the game (a grey orb is 2 gold and an Item Remover early, or a 3 cost champion later). A Prismatic Orb was added as the strongest. Gold and Prismatic orbs never appear in Stage 1. ([TFT patch 14.5 notes](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-14-5-notes/), official)
- Patch 13.5, an earlier step: items stayed random for the lobby, but bonus orbs were valued by utility so everyone got about the same value. ([TFT patch 13.5 notes](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-13-5-notes/), official via search extract)
- The Masterwork Upgrade consumable "opens an armory containing Radiant versions of that unit's craftable completed items"; the player picks one to upgrade. ([TFT patch 14.5 notes](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-14-5-notes/), official)
- Carousel: once per stage; players pick in pairs, lowest health first. ([Bunny Muffins Set 8 guide](https://bunnymuffins.lol/beginner-guide-for-teamfight-tactics-set-8/), guide; [Mobalytics](https://mobalytics.gg/?p=10527), guide)
- Augments arrive at stages `2-1`, `3-2` and `4-2`. A Set 6 article describes three options per selection screen. ([Bunny Muffins Set 13 augment guide](https://bunnymuffins.lol/augment-guide-for-set-13/), guide; [Upcomer, Set 6 augments](https://upcomer.com/tft-set-6-augments-will-make-every-game-fun-and-fresh/), news)
- Two components combine into one completed item, in either order. A unit holds up to three completed items. The Spatula has no stats and mostly makes trait emblems (older set example: Spatula plus B.F. Sword made the unit an Assassin). Two Spatulas made Tactician's Crown, +1 team size. Component counts vary by set. ([Prima Games, Spatula combinations](https://primagames.com/tips/spatula-combinations-tft-guide), guide; [Metabot items guide](https://metabot.gg/en/TFT/guides/tft-items-components-recipes-how-to-combine), guide)

#### Vampire Survivors (poncle)
- Each level up offers 3 or 4 options; the 4th depends on Luck. Options are drawn without repetition. ([Vampire Survivors wiki, Level up](https://vampire.survivors.wiki/w/Level_up), fan wiki)
- Treasure chests drop from bosses. A chest usually holds 1 item with a chance of 3 or 5. Each boss sets its own odds; the Silver Bat's are 3% for five items, 10% for three, 50% for one, and a one item chest if every roll fails. Luck multiplies each roll. A chest levels a random owned weapon or passive, and a chest with several items can level the same one more than once. Chests that can evolve usually come from bosses after 10:00, and usually allow one evolution each. Tiers: Bronze (usually no evolution, before 10:00), Silver (can evolve), Gold (can give new weapons), Arcana (choose from 4 or 6 Arcanas, usually at 11:00 and 21:00), Dark. ([Vampire Survivors wiki, Treasure Chest](https://vampire.survivors.wiki/w/Treasure_Chest), fan wiki)
- An evolution needs the weapon at max level, its matching passive item in the inventory, then a chest that can evolve. ([Vampire Survivors wiki, Gift](https://vampire.survivors.wiki/w/Gift), fan wiki via search extract)

#### The Binding of Isaac (Edmund McMillen, Nicalis)
- Each room type has its own item pool: Treasure Room (largest, and the fallback), Shop, Library (actives), Curse Room, Boss (passives only), Secret Room, Devil Room, Angel Room, Golden Chest, Red Chest, several beggars, Planetarium, Ultra Secret Room, Crane Game and more. Greed mode has its own set of pools. ([Isaac wiki.gg, Item pool](https://bindingofisaacrebirth.wiki.gg/wiki/Item_pool), fan wiki)
- Devil deal items cost 1 or 2 red heart containers, or 3 soul hearts if Isaac has no red hearts. ([Isaac wiki.gg, Devil room](https://bindingofisaacrebirth.wiki.gg/wiki/Devil_room), fan wiki via search extract; [Isaac wiki.gg, Devil Room item pool](https://bindingofisaacrebirth.wiki.gg/wiki/Devil_Room_%28Item_Pool%29), fan wiki via search extract)

#### Balatro (LocalThunk)
- Default shop: 2 random cards, 2 Booster Packs, 1 Voucher. The first shop of a run guarantees a Buffoon Pack. Reroll starts at $5, rises $1 per reroll, resets each shop, and restocks only the cards. Card type weights: Joker 20 (71.4%), Tarot 4 (14.3%), Planet 4 (14.3%). Packs cost $4, $6 or $8. ([Balatro wiki, Shop](https://balatrowiki.org/w/Shop), fan wiki)

#### Monster Train (Shiny Shoe)
- Between fights you pick one of two paths with different merchants, encounters and rewards. Rewards include gold, card drafts, unit drafts, artifacts, merchants and events. Merchants spend gold on card upgrades that last the run. The champion is upgraded at the Dark Forge. A run is 9 rings with a boss every 3. ([BlueStacks beginner guide](https://www.bluestacks.com/blog/game-guides/monster-train/mstn-beginners-guide-en.html), guide)

#### Darkest Dungeon (Red Hook)
- Each expedition awards one trinket. Rarity starts at Common for short Apprentice dungeons and rises one level per extra step of difficulty or length, up to Ancestral for long Champion dungeons. Boss expeditions always award a Very Rare. ([Darkest Dungeon wiki.gg, Trinkets](https://darkestdungeon.wiki.gg/wiki/Trinkets_%28Darkest_Dungeon%29), fan wiki)

### Inferences
- Three layers recur across these games: (a) a small pick after most fights, (b) a guaranteed bigger pick at elites, bosses or fixed times, often from a separate pool (Slay the Spire boss relics only from bosses, Risk of Rain 2 boss items never from chests, Vampire Survivors evolutions only from chests after 10:00, TFT augments at three fixed stages), (c) a spend point where currency buys a targeted item (Slay the Spire and Balatro shops, Risk of Rain 2 chests and printers, Charon's shop in Hades).
- Three choices is the usual default (Slay the Spire cards and boss relics, Hades boons, TFT augments). The count is a dial: Vampire Survivors adds a 4th with Luck, Busted Crown removes 2, Hades and Balatro sell rerolls.
- Showing the reward on the route (Hades door symbols, Monster Train's two paths, Kindred Siege's node drops) turns loot into route planning. Kindred Siege already does this.
- TFT's fairness rule is "same value, different contents". It matters for competitive lobbies; for a single player run it suggests keeping the total value of each road steady while the exact pieces vary.
- Kindred Siege has far fewer reward moments per run than Slay the Spire or Hades (about 3 rows per road and 3 roads in 30 to 40 minutes, per the locks), so every pick carries more weight. A single piece with no choice wastes a moment that could be a real decision. This is my reading of DESIGN_LOCKS lines 208, 322 and 405.

#### Fit for Kindred Siege (ideas for Jacob, not decisions)
1. **Spoils as one of three.** "Strip one piece" could become pick 1 of 3 pieces the beaten side actually wore, with "bank materials" as the skip. That is the Slay the Spire reward shape, it keeps the strip fantasy, and the no duplicate rule already exists. Vampire Survivors shows a skip can still pay a little (Skip keeps some experience).
2. **Chief drop stays named and guaranteed.** The chief's Princess piece works like a Risk of Rain 2 boss item or a Darkest Dungeon boss reward: always there, always his. The Slay the Spire alternative (pick 1 of 3 boss relics) would add choice but blur the chief's identity.
3. **Pal favour nodes as Hades gods.** The node shows which Pal; that Pal offers three favours from their own themed pool; meeting the same Pal again levels a favour (like a Pom). A camp pick before the run could guarantee the first favour's Pal, like a Hades keepsake.
4. **The swap rule is already a shop.** Line 319 lets banked materials buy one piece. Risk of Rain 2 priced chests and Balatro's rising reroll show how to grow it: a priced cache node, or a reroll paid in materials that gets dearer each use. Materials are what goes home, so spending them inside the run is a real cost.
5. **Crafting through the Pal recipe lock.** A Vampire Survivors style evolution fits line 88. Example shape: a Mid piece of tribe X on a fighter of tribe X, plus that tribe's chief broken by a Pal, lets the Pal forge the chief's Princess recipe. Two things the player chose become one build defining thing.
6. **Node type pools, like Isaac rooms.** War party, trial, duel, elite and chief nodes each draw from their own list. Kindred Siege half does this already through tiers.
7. **Carousel lesson for single player.** The useful TFT idea is "the one behind picks first". After a lost fight (roster as lives), the next offer could show one extra choice. Lost fights already have locked rules (line 315), so this needs Jacob's call.

### Gaps
- The base number of cards in a Slay the Spire reward (commonly three) was not stated on the pages I could fetch. Potion drop odds were not verified.
- Hades Daedalus Hammer limits and per chamber reward odds were not found.
- Risk of Rain 2 Legendary Chest, Multishop Terminal, category chests and Lunar Pod prices were not retrieved (the page is very long and partly under construction).
- TFT carousel order and augment counts come from guides, not Riot, and can change by set.
- Monster Train draft size and rarity odds were not found.
- Balatro booster pack "choose 1 of N" rules were not on the Shop page.

## 2. Pools and weighting: steering offers toward a forming build without forcing it, and avoiding useless offers

### Takeaway
The games that steer well mostly use visible gates and narrowing, not hidden synergy weights: prerequisites (Hades Duo and Legendary boons), one boon per slot, removal of seen items (Isaac, Slay the Spire relics), conditions that block useless items (Slay the Spire Bottled relics need a matching card), early choices that shrink the pool (Monster Train's two clans), and tools the player aims (keepsakes, rerolls, banish, printers, emblems). Slay the Spire weights card rewards only by rarity and fight type and still feels steerable, because the player picks, skips and removes cards.

### Cited Findings

#### Hades
- Duo boons are offered only if you already hold a boon from each of the two gods. Each Legendary needs specific boons from its god; for example Zeus's Splitting Bolt needs Storm Lightning, Double Strike or High Voltage. ([Hades Fextralife wiki, Boons](https://hades.wiki.fextralife.com/Boons), fan wiki)
- One boon per slot (Attack, Special, Cast, Dash, Call). Taking a new boon for a filled slot means letting go of the old one. Passive boons have no slot limit. ([Hades Fextralife wiki, Boons](https://hades.wiki.fextralife.com/Boons), fan wiki)
- Olympian keepsakes (Thunder Signet for Zeus, Owl Pendant for Athena, and so on) make the next boon come from that god, once per run, and while equipped raise that god's chance of Rare or better by 10%, 15% or 20% by rank. A keepsake ranks up after 25 encounters cleared with it equipped. Keepsakes are earned by giving Nectar to characters. ([Prima Games, keepsake list](https://primagames.com/tips/hades-keepsake-list), guide; [Attack of the Fanboy, keepsakes](https://attackofthefanboy.com/guides/hades-1-0-full-keepsakes-list/), guide)
- In Hades II the Owl Pendant makes Athena boons appear and raises their rarity, and the Thunder Signet makes the first Zeus boon offered Rare or better. ([Maxroll, Hades II keepsakes](https://maxroll.gg/hades-2/guides/hades-2-keepsake-guide), guide; [Broken Builds, Hades II keepsakes](https://brokenbuilds.gg/hades-ii/tier-lists/keepsakes), guide)
- Mirror of Night rarity talents: Olympian Favor +1% chance for Rare per rank (40 ranks); Gods' Pride +1% for Epic per rank (20 ranks); Gods' Legacy +1% for Legendary or Duo "if possible" per rank (10 ranks). ([Hades Fextralife wiki, Mirror of Night](https://hades.wiki.fextralife.com/Mirror+of+Night), fan wiki)
- Guide claims, possibly outdated: Hermes appears at most three times per run, counting Charon's shop ([PC Invasion, Hermes boons](https://pcinvasion.com/hades-guide-olympian-god-boons-skills/11), guide). A Hades II early access guide says a run meets 3 Olympians by default ([Maxroll, Hades II boons and gods](https://maxroll.gg/hades-2/guides/hades-2-boons-and-gods-guide), guide).

#### Slay the Spire
- Relic spawn rules: Bottled Flame, Bottled Lightning and Bottled Tornado need an Attack, Skill or Power in the deck. Calling Bell and events that are not fights never give the Bottles or Whetstone. Only 2 of Shovel, Girya and Peace Pipe per run. Relics appear once per run. Floor limits: Ectoplasm only before floor 18 (as a Boss Swap or from the Act 1 boss chest); a long list including Maw Bank, Omamori, Prayer Wheel and Shovel only before floor 49. ([Slay the Spire wiki.gg, Relics](https://slaythespire.wiki.gg/wiki/Relics), fan wiki)
- Healing cards (Reaper, Feed, Alchemize, Self Repair, Lesson Learned, Wish, Bandage Up) can never be generated by random card effects. The only inputs the wiki lists for card reward odds are rarity, fight type and the rare offset. ([Slay the Spire wiki.gg, Cards](https://slaythespire.wiki.gg/wiki/Cards), fan wiki)
- Mega Crit's GDC 2019 talk "Slay the Spire: Metrics Driven Design and Balance" (Anthony Giovannetti) describes a metric driven approach from early development and through early access. ([GDC Vault](https://www.gdcvault.com/play/1025731), official; [Game Developer preview](https://www.gamedeveloper.com/design/learn-i-slay-the-spire-i-s-metrics-driven-approach-to-game-balancing-at-gdc-2019), press)
- Secondary summaries of that talk report the goals "every card should have a place" and "avoiding anything too warping", and that a single player game can allow rare overpowered combos without hurting other players. ([Podwise summary](https://podwise.ai/episodes/1459444), secondary; [Glasp transcript notes](https://glasp.co/youtube/7rqfbvnO_H0), secondary)

#### The Binding of Isaac
- Nearly every item has weight 1 in its pool (with 200 such items, each has a 0.5% chance). Some are lower, for example Epic Fetus 0.1 in the Treasure Room since Repentance and Mega Blast 0.2 in the Devil Room. ([Isaac wiki.gg, Item pool](https://bindingofisaacrebirth.wiki.gg/wiki/Item_pool), fan wiki)
- A seen item is removed from the Treasure Room and most pools. Some pools only reduce its weight: by 50% (Beggar, Bomb Bum, Curse Room, Devil Beggar, Key Master, Shop before Repentance), 40% (Golden Chest, Red Chest, Ultra Secret Room) or 10% (Library, Crane Game). Touching an item removes it from all pools. An empty pool falls back to the Treasure Room pool; if that is empty too, only Breakfast appears. Rerolls with The D6 consume items from pools. The Beggar pool has no quality 3 or 4 items. ([Isaac wiki.gg, Item pool](https://bindingofisaacrebirth.wiki.gg/wiki/Item_pool), fan wiki)
- Conflict: forum users and the wiki's Chaos page say a depleted pool does not borrow from another pool (with the Chaos item, a depleted pool gives Breakfast), while the Item pool page says an exhausted pool falls back to the Treasure Room pool. The fallback may differ by item or game version. ([Steam forum threads](https://steamcommunity.com/app/250900/discussions/0/352792037311226986), [more](https://steamcommunity.com/app/250900/discussions/0/357286033299484220), forum; [Isaac wiki.gg, Chaos](https://bindingofisaacrebirth.wiki.gg/wiki/Chaos), fan wiki via search extract)

#### Vampire Survivors
- Each item's chance is its rarity value over the total rarity of eligible items (with all DLC: weapons 8,130, passives 1,370). Items at max level are not offered; new items stop once 6 weapons or 6 passives are held. With nothing left to offer, a level up gives Gold or Floor Chicken. A separate check for owned items runs twice, scales with Luck and doubles on even levels (the page's formula did not render). Reroll, Skip and Banish are bought as PowerUps. ([Vampire Survivors wiki, Level up](https://vampire.survivors.wiki/w/Level_up), fan wiki)

#### Diablo III
- Loot 2.0 "Smart Loot" stops an item rolling stats useless to your class; about 15% of items roll for a random other class; it does not look at your build. ([Diablo Fandom wiki, Loot 2.0](https://diablo.fandom.com/wiki/Loot_2.0), fan wiki via search extract)

#### Monster Train
- You pick a primary and a supporting clan and get access to all cards from both. ([PlayStation Store listing](https://www.playstation.com/en-ca/games/monster-train/), official)

#### Teamfight Tactics
- Spatula items make trait emblems that add a trait to a unit, a direct "join this build" item. ([Prima Games, Spatula combinations](https://primagames.com/tips/spatula-combinations-tft-guide), guide)

#### Dead Cells
- Version 1.1 added a custom mode that keeps chosen items out of shops and chests and lets players choose starting gear. Motion Twin said in 2018 that banning unlocked weapons from the loot table was one of the most requested features. ([PCGamesN](https://www.pcgamesn.com/dead-cells/custom-game-mode-patch), news; [Nintendo Everything](https://nintendoeverything.com/dead-cells-update-out-now-version-1-1-60-fps-new-custom-game-mode-much-more/), news; [Game Pilgrim](https://gamepilgrim.com/2018/11/25/new-customizable-mode-for-dead-cells-in-alpha-testing/), news)
- The Hunter's Mirror shows a random enemy or boss that carries a blueprint you do not have yet. ([Dead Cells wiki.gg, Blueprint](https://deadcells.wiki.gg/wiki/Blueprint), fan wiki)

#### Balatro
- A player who read the code says the game rolls the rarity first, then a joker within that rarity. ([Steam forum threads](https://steamcommunity.com/app/2379780/discussions/0/604151507985670756), [more](https://steamcommunity.com/app/2379780/discussions/0/4293690852338935237), forum)

#### Kindred Siege rules that bear on this
- No duplicate pieces from fights or events. ([DESIGN_LOCKS.md lines 336, 347](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))
- Inside a run the hunters counter your sets and favourite tricks. ([DESIGN_LOCKS.md line 297](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))
- Sim rules: no `System.Random`, use `DeterministicRng`; integers only in authoritative state; no `Dictionary` or `HashSet` iteration in authoritative logic; the state snapshot must be ordered and complete. ([CLAUDE.md](C:/Users/jacob/KindredSiege-Clean/CLAUDE.md))

### Inferences
- Five steering levers show up: (1) gates (Hades prerequisites), (2) exclusion (no duplicates, no useless items, Isaac removal), (3) soft weights (keepsake rarity, Vampire Survivors owned item check), (4) player tools (rerolls, banish, printers, emblems, keepsakes), (5) narrowing through early choices (Monster Train clans, Hades slots). None of the strong examples relies on an invisible "synergy multiplier"; the rules are visible.
- Slay the Spire shows that pure random rewards still steer when the player can skip and remove. Kindred Siege's "bank materials" is the skip.
- For dead offers, the clean pattern is a known fallback (Vampire Survivors gives Gold or healing; Slay the Spire gates relics by deck content). Isaac's Breakfast is the cautionary version.

#### Fit for Kindred Siege (ideas for Jacob, not decisions)
1. **"One on build, one beside it, one wild."** The build is tribe sets (2 and 3 of a tribe), quirks (Brute, Tough, Sharp eye, Quick) and one gear slot per fighter. Slot A could favour a tribe you field 1 or 2 fighters of, or the stat a fighter's quirk already pushes; slot B a stat your squad lacks; slot C plain random. The existing no duplicate rule stays.
2. **Gate specials by use.** Offer a Princess special only when the squad can use it, like Slay the Spire's Bottled relics needing a matching card.
3. **Visible player tools.** A material priced reroll that gets dearer (Balatro), a banish for this run (Vampire Survivors), a printer style trade of a held piece for a same tier piece of another tribe (Risk of Rain 2), and maybe an emblem piece that lets one fighter count toward another tribe's set (TFT Spatula). Each is optional.
4. **The rival is a built in counterweight.** The hunters counter your sets inside a run (line 297). Steering hard into one set makes their counter bite harder. That is a good reason to keep weights soft and leave a wild slot.
5. **Pal pools like Hades gods.** Each Pal has a themed pool; a "duo favour" could appear only when you hold favours from two Pals; a camp pick could aim the first favour, like a keepsake.
6. **Determinism.** Weights should be integers in an ordered list, draws should use `DeterministicRng` from the run seed, and any pity counter or "seen" list belongs in the snapshot so replays and checksums hold.

### Gaps
- Hades base rarity odds were not found. Players often say Hades caps a run at four Olympians; I found no source confirming it.
- Whether Hades weights offers toward gods or boons you already hold is not documented in what I found.
- Isaac's item quality (0 to 4) role in selection beyond the Beggar exclusion was not found.
- The exact Vampire Survivors owned item formula did not render, and the Seal feature was not covered.
- No primary developer statement from Supergiant, Edmund McMillen or Hopoo on offer weighting was found.

## 3. Rarity, pity and bad luck protection

### Takeaway
Most games use three to five tiers with fixed odds per source and keep the top tier scarce (Risk of Rain 2 reds at 0.99% from small chests, Dead Cells rare blueprints at 0.4%, Monster Hunter rubies at about 1 to 3% per roll). Bad luck protection takes three forms: a counter that raises the odds after each miss and resets on a hit (Slay the Spire's rare offset), hard guarantees at fixed points (boss card rewards all rare, Darkest Dungeon boss expeditions always Very Rare, Hearthstone's pack guarantees), and removing duplicates (Isaac, Slay the Spire relics, Hearthstone, Darkest Dungeon uniques).

### Cited Findings
- **Slay the Spire rare offset.** It starts at −5%, rises 1% each time a common is rolled, caps at +40%, and resets to −5% when a rare is rolled. Only card rewards move it (including Orrery and Tiny House); shop and Library cards use it but do not move it. ([Fandom Card Rewards](https://slay-the-spire.fandom.com/wiki/Card_Rewards), fan wiki via search extract; [Slay the Spire wiki.gg, Cards](https://slaythespire.wiki.gg/wiki/Cards), fan wiki). An older Steam forum model starts the counter at 0 and adds it to the rare odds (rare = counter + 5% for elites); it conflicts with the wiki's −5% start and +40% cap, and I could not tell which matches current code. ([Steam forum thread A](https://steamcommunity.com/app/646570/discussions/0/594015710347314793), [thread B](https://steamcommunity.com/app/646570/discussions/0/1651043958627355887/?ctp=3), forum; the search extract did not say which of the two holds the formula)
- Slay the Spire never offers a rare on the first floor of an act, except special cases (Calling Bell, Tiny House by Boss Swap, some Neow options). ([Slay the Spire wiki.gg, Cards](https://slaythespire.wiki.gg/wiki/Cards), fan wiki)
- Hades rarity raisers: keepsakes +10/15/20% Rare or better for one god; Mirror +1% per rank for Rare (40 ranks), Epic (20), Legendary or Duo (10). ([Prima Games](https://primagames.com/tips/hades-keepsake-list), guide; [Fextralife Mirror of Night](https://hades.wiki.fextralife.com/Mirror+of+Night), fan wiki)
- **Dead Cells blueprint tiers:** Always 100% (104 blueprints), Common 10% (33), Uncommon 1.7% (53), Rare 0.4% (64), Legendary 0.03% (2). Drops are assigned at level generation to a fixed enemy. Enemies in one biome drop at most one outfit and one item blueprint; 13 "Override Limit" blueprints can drop alongside another. The Hunter's Grenade turns an enemy elite so a Blueprint Extractor can pull a random blueprint from its pool, the only way past the cap. Bosses drop outfits in a set sequence gated by Boss Stem Cell level. ([Dead Cells wiki.gg, Blueprint](https://deadcells.wiki.gg/wiki/Blueprint), fan wiki)
- **Darkest Dungeon:** expedition length and difficulty set the reward rarity; boss expeditions always give Very Rare. Ancestral, enemy (Madman, Collector) and Trophy trinkets are unique; set trinkets are limited to one copy each. Shrieker's Perch opens after losing at least eight trinkets and returns eight lost trinkets. ([Darkest Dungeon wiki.gg, Trinkets](https://darkestdungeon.wiki.gg/wiki/Trinkets_%28Darkest_Dungeon%29), fan wiki)
- **Hearthstone:** a legendary is guaranteed within 40 packs and an epic within 10 ([esports.gg](https://esports.gg/news/hearthstone/hearthstone-pity-timer/), press citing a Blizzard support article). Since Knights of the Frozen Throne, the first 10 packs of a new set contain a legendary ([TechRaptor](https://techraptor.net/content/hearthstones-card-pack-system-is-changing), press; [Hearthstone wiki.gg, card pack statistics](https://hearthstone.wiki.gg/wiki/Card_pack_statistics), fan wiki). You should not receive another copy of a legendary you own, or a third copy of another card ([Blizzard forum](https://us.forums.blizzard.com/en/hearthstone/t/incorrect-drop-rate-and-double-legendary-cards/31589), forum). Blizzard's odds disclosure for China gives an average of one legendary per 20 packs and one epic per 5 ([Hearthstone wiki.gg](https://hearthstone.wiki.gg/wiki/Card_pack_statistics), fan wiki).
- **Monster Hunter World:** Azure Rathalos High Rank Rathalos Ruby: body carve 2%, tail carve 3%, capture 2%, head break 1%, some optional quest rewards 3% ([Fextralife, Rathalos Ruby](https://monsterhunterworld.wiki.fextralife.com/Rathalos+Ruby), fan wiki). Each carve, break and capture is its own roll; one player counts about seven chances in an ideal hunt ([Steam forum](https://steamcommunity.com/app/582010/discussions/0/1735462352481591861), forum). In Wilds the Rathalos Ruby is 5% from a body carve ([Monster Hunter Fandom wiki, Rathalos carves in Wilds](https://monsterhunter.fandom.com/wiki/Rathalos_Carves/MHWilds), fan wiki via search extract).
- **Balatro:** jokers are Common 70%, Uncommon 25%, Rare 5%; Legendary jokers are never sold in the shop ([Balatro wiki, Shop](https://balatrowiki.org/w/Shop), fan wiki). A forum post says a specific rare then comes up 1 in (20 minus the rares you own), which implies owned jokers are excluded ([Steam forum threads](https://steamcommunity.com/app/2379780/discussions/0/604151507985670756), [more](https://steamcommunity.com/app/2379780/discussions/0/4293690852338935237), forum).
- **Vampire Survivors:** chest size odds per boss multiplied by Luck (above). The Clover adds 10% Luck per level, up to 50%. ([Vampire Survivors wiki, Clover](https://vampire.survivors.wiki/w/Clover), fan wiki via search extract)
- **TFT:** equal orb value per player since patch 14.5 (above), a fairness rule rather than pity. ([TFT patch 14.5 notes](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-14-5-notes/), official)
- **Diablo III:** Loot 2.0 shipped in patch 2.0.1 in February 2014 ([Shacknews](https://www.shacknews.com/article/83266/diablo-3-patch-201-deploys-with-loot-20), news). It traded quantity for quality: fewer but stronger legendaries ([Diablo Fandom wiki, Loot 2.0](https://diablo.fandom.com/wiki/Loot_2.0), fan wiki via search extract; [Game Wisdom](https://game-wisdom.com/critical/loot-2-0-and-diablos-fourth-revival), blog; the extract did not say which of the two used these words). A design blog argues the old problem was long unlucky streaks with no upgrade ([Game Wisdom](https://game-wisdom.com/critical/loot-2-0-and-diablos-fourth-revival), blog).

#### Numbers at a glance

| Game | Source of item | Top tier odds or guarantee |
|---|---|---|
| Slay the Spire | Normal fight card | 3% rare base, offset from −5% to +40% |
| Slay the Spire | Elite fight card | 10% rare base, plus a relic |
| Slay the Spire | Boss | All rare cards; pick 1 of 3 boss relics |
| Slay the Spire | Random relic | 50 / 33 / 17 (common / uncommon / rare) |
| Risk of Rain 2 | Small chest $25 | 0.99% legendary |
| Risk of Rain 2 | Large chest $50 | 20% legendary, never common |
| Risk of Rain 2 | Teleporter boss | 15% per drop to become the boss item |
| Dead Cells | Rare blueprint | 0.4% (Legendary 0.03%) |
| Monster Hunter World | Rathalos Ruby (Azure, HR) | 1% to 3% per roll |
| Balatro | Shop joker | Rare 5%, no Legendary in shop |
| Hearthstone | Card packs | Legendary within 40 packs; one in first 10 of a set |
| Darkest Dungeon | Boss expedition | Always Very Rare |
| Vampire Survivors | Silver Bat chest | 3% five items, 10% three items |

### Inferences
- My rough estimate, not a sourced figure: in Slay the Spire normal fights the rare chance only turns positive after about 4 cards, and the median gap between rares is about 19 cards, or about 6 normal rewards if each shows 3 cards. Elite and boss rewards come on top, so a run sees rare offers several times per act.
- My arithmetic from the table: a Risk of Rain 2 player needs about 70 small chests for an even chance of a red, but about 3 large chests. A Monster Hunter ruby at about 2% over seven rolls is roughly a 13% chance per hunt, so the design leans on many repeat hunts.
- Pattern: long term loot games (Monster Hunter, Dead Cells, Diablo) accept low odds and many attempts; short run games (Slay the Spire, TFT, Hades) guarantee build moments at fixed beats.

#### Fit for Kindred Siege (ideas for Jacob, not decisions)
1. Kindred Siege tiers are set by node type, so rarity is mostly guaranteed already. The excitement comes from earning a tier up (objectives and omens, lines 324 and 325), which matches Darkest Dungeon's difficulty and length raising rarity.
2. If a random tier roll is ever added (for example a war party piece rolling Mid), the Slay the Spire offset fits the Sim: an integer counter that starts below zero, adds 1 per miss, resets on a hit, and lives in the snapshot.
3. Runs are short, so prefer guarantees to low odds. A 0.4% drop works across hundreds of Dead Cells runs; a 30 to 40 minute run needs near certain build moments.
4. Open question for Jacob: a player who always captures chiefs never takes a Princess piece from them (kill gives the piece, capture gives the tribe, line 276). If Princess gear should appear in every run, an elite drop or the Pal's learned recipe could cover the capture path.
5. Jacob wants no chances shown in fights (line 291). If that extends to loot, keep odds hidden and let tiers and named specials carry the feel.

### Gaps
- Hades base rarity odds were not found.
- Diablo III legendary bad luck protection and the often quoted "85% Smart Drop" figure were not confirmed by an official source.
- The exact wording of Blizzard's Hearthstone pity article was not reached.
- My Slay the Spire estimate assumes 3 cards per reward and ignores elites, shops and events.

## 4. Region, biome and boss loot

### Takeaway
Tying loot to a place or creature makes route and target choice matter. Isaac pools items by room type; Dead Cells puts blueprints on specific enemies with one per biome; Monster Hunter builds gear from one monster's parts, hides the rarest parts behind breaks and captures, and gives set bonuses for wearing that monster's armour; Darkest Dungeon tags trinkets by dungeon and gives each boss a trophy; Risk of Rain 2 gives each teleporter boss its own item.

### Cited Findings
- **Isaac:** item pools by room type (Treasure, Shop, Library, Curse, Boss, Secret, Devil, Angel and more). ([Isaac wiki.gg, Item pool](https://bindingofisaacrebirth.wiki.gg/wiki/Item_pool), fan wiki)
- **Dead Cells:** examples of enemy tied blueprints: Magic Missiles from Arbiters in the Bank; Scavenged Bombard from Pirate Captains in Stilt Village. One outfit and one item blueprint per biome from enemies. 42 secret blueprints sit in hidden areas, puzzles, timed doors, rune locked areas or Daily Run rewards. The Hunter's Mirror shows which enemy or boss carries a blueprint you lack. ([Dead Cells wiki.gg, Blueprint](https://deadcells.wiki.gg/wiki/Blueprint), fan wiki)
- **Monster Hunter World:** set bonuses switch on with 2 or more pieces of a set, and Alpha and Beta versions share bonuses ([Fextralife, Skills](https://monsterhunterworld.wiki.fextralife.com/Skills), fan wiki). Sets with the same bonus name mix across monsters (Rathalos and Azure Starlord pieces share "Rathalos Mastery"), but the same skill under different set bonuses does not combine (Drachen and Teostra both give Master's Touch) ([Steam forum threads](https://steamcommunity.com/app/582010/discussions/0/1638668568129369162), [more](https://steamcommunity.com/app/582010/discussions/0/2590022385653163302), [more](https://steamcommunity.com/app/582010/discussions/0/1737715419897859734), forum). Rare parts come from carves, breaks and captures as separate rolls (Section 3).
- **Darkest Dungeon:** trinkets carry a Dungeon tag (Ruins, Warrens, Weald, Cove, Courtyard). Some are quest rewards from one region but are also sold at the Nomad Wagon. Champion versions of the 8 bosses from the 4 normal regions each give a Trophy exclusive to that boss. Each hero has 1 to 3 Common or Uncommon class trinkets and 1 Rare or Very Rare. Each hero also has a set of two exclusive trinkets that give a set bonus when both are worn; most come from the Courtyard. A hero carries up to two trinkets. ([Darkest Dungeon wiki.gg, Trinkets](https://darkestdungeon.wiki.gg/wiki/Trinkets_%28Darkest_Dungeon%29), fan wiki)
- **Risk of Rain 2:** each teleporter boss can turn 15% of drops into its own item; Scavenger and Horde of Many have none. ([Rogue Ranker, yellow items](https://rogueranker.com/yellow-items-ror2/), guide)
- **Vampire Survivors:** chest tier follows boss timing (Bronze before 10:00, Silver after, Arcana chests at 11:00 and 21:00), and some stages have special sources (golden chests from Zi'Appunta Belpaese's light sources). ([Vampire Survivors wiki, Treasure Chest](https://vampire.survivors.wiki/w/Treasure_Chest), fan wiki)
- **Hades:** doors show the god whose boon lies behind them. ([Hades Fextralife wiki, Boons](https://hades.wiki.fextralife.com/Boons), fan wiki)
- **Kindred Siege:** each region has a tribe and the monster that tribe hunts (task brief). Tiers 1 and 2 each hold two tribes and a run meets one at random. ([DESIGN_LOCKS.md lines 395, 405](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))

### Inferences
#### Fit for Kindred Siege (ideas for Jacob, not decisions)
1. **Tribe gear as sets.** Darkest Dungeon hero sets and Monster Hunter armour sets show gear pieces counting toward a set. Today Kindred Siege sets count fighters (line 285). Letting a tribe's pieces count too would be a new rule for Jacob to judge; it would make spoils feed sets directly.
2. **The hunted monster as a material source.** Like Monster Hunter, the monster a tribe hunts could supply the materials Pals forge with (the materials that go home). A rare part could sit behind a fight objective, the way Monster Hunter puts rare parts behind breaks, instead of a low percent roll.
3. **The chief as a boss with a trophy.** One named Princess piece (already locked) plus the recipe the Pal learns (line 88), like Darkest Dungeon boss trophies and Risk of Rain 2 boss items.
4. **Elites carry recipes, Dead Cells style.** One recipe carrier per region, with a codex hint naming which elite carries one (Hunter's Mirror).
5. **Variety is already built in.** A random tribe per tier changes the region's loot from run to run.
6. **A safety valve.** Darkest Dungeon sells region trinkets at the Nomad Wagon, so bad luck in one region does not lock out a set. Camp Pals could do the same with known pieces.
7. Capture versus kill already mirrors Monster Hunter's capture versus carve reward tables.

### Gaps
- Hades per region drop tables were not verified.
- Capcom does not publish Monster Hunter drop tables in what I found; the numbers are fan data.
- The Darkest Dungeon trinket page was truncated, so Farmstead trinkets and the full set bonus list were not read.

## 5. Meta progression: adding items to the pool across runs without making early runs weak

### Takeaway
Unlocks across runs mostly add breadth (new items in the pool) rather than raw power, and the main risk is pool dilution. Games manage it with banning tools (Dead Cells custom mode, Vampire Survivors Banish), staged unlocks per faction (Monster Train clan levels), and a small ritual that makes each unlock an event (Dead Cells' Collector). Hades openly adds power through the Mirror, and its director says that is the point.

### Cited Findings
- **Hades:** Greg Kasavin says the Mirror of Night is there to make players meaningfully more powerful from one playthrough to the next ([Tom's Guide interview](https://www.tomsguide.com/uk/features/hades-exclusive-interview-supergiant), press). Mirror talents cost Darkness per rank plus Chthonic Keys to unlock (for example Gods' Pride 100 Darkness per rank, Fated Persuasion 1000 then 2000, 3000, 4000) ([Fextralife, Mirror of Night](https://hades.wiki.fextralife.com/Mirror+of+Night), fan wiki). The skill system became boons given by each god ([GameSpot feature](https://gamespot.com/articles/hades-changes-what-it-means-to-be-a-roguelike/1100-6483420/), press).
- **Dead Cells:** a found blueprint is delivered to the Collector at a biome transition (automatic since version 2.3). You pay Cells to unlock it. Dying before delivery loses it for that run. Once paid, the item can be found in runs, chosen at the Tailor, or bought from Guillain. ([Dead Cells wiki.gg, Blueprint](https://deadcells.wiki.gg/wiki/Blueprint), fan wiki). Custom mode (version 1.1) can ban items and set starting gear (Section 2).
- **Isaac:** achievements come from challenges, milestones, hidden goals and items ([Isaac wiki.gg, Achievements](https://bindingofisaacrebirth.wiki.gg/wiki/Achievements), fan wiki). A design blog says each unlocked item joins specific pools, that every weak item dilutes the chance of a good one, and that some players skipped challenges to keep their pools clean ([On Games And Design, Binding of Isaac](https://urflow.bearblog.dev/roguelites-the-binding-of-isaac), blog).
- **Monster Train:** 5 clans (6 in the later edition), each with 10 levels that bring new cards into the pool ([PlayStation Store listing](https://www.playstation.com/en-ca/games/monster-train/), official; [Nuuvem store listing](https://www.nuuvem.com/au-en/item/monster-train), store text). A Monster Train 2 forum thread complains about the unlock grind; one reply says the needed cards unlock after a run or two ([Steam forum](https://steamcommunity.com/app/2742830/discussions/0/599653598138270985), forum, anecdote).
- **Vampire Survivors:** Reroll, Skip and Banish are bought as PowerUps. The "always Gold" or "always Floor Chicken" fallback unlocks after 100 Big Coin Bags. ([Vampire Survivors wiki, Level up](https://vampire.survivors.wiki/w/Level_up), fan wiki)
- **Risk of Rain 2:** many items must be unlocked through challenges before they appear. ([GamesRadar, how to unlock every item](https://gamesradar.com/risk-of-rain-2-items), guide)
- **Hearthstone:** pity counters and duplicate protection carry across sessions (Section 3).
- **Kindred Siege:** when a run ends, tribes that joined, their gear and chief orders reset; Pal materials, camp upgrades, the codex and what chiefs learned about you stay ([DESIGN_LOCKS.md line 201](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md)). The hunters grow stronger across runs and learn traits ([DESIGN_LOCKS.md lines 365, 376](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md)).

### Inferences
- Two families: Hades adds power you can feel (Mirror), while Dead Cells, Isaac, Monster Train and Risk of Rain 2 mostly add options. Option unlocks keep run one at full strength but can dilute pools, which is why banning tools keep appearing.
- Staged unlocks per faction (Monster Train) keep each sub pool small, so a new card lands in a pool that is already about one theme.

#### Fit for Kindred Siege (ideas for Jacob, not decisions)
1. **Chief recipes as blueprints.** Learned in the run when a Pal breaks a chief (line 88), kept only if they get home (Dead Cells loses undelivered blueprints), then unlocked with materials so they can appear in later runs. The locks do not say whether a learned recipe lasts past the run; Jacob decides.
2. **Unlock sidegrades, not floors.** New Princess specials, events or quirks widen choice without making run one weak. If power must grow, Hades style small percent perks (rarity, rerolls) are the gentlest kind.
3. **Guard against dilution.** Unlock into a tribe's own pool only, like Monster Train clan levels, so each tribe pool stays small and on theme; offer a camp ban or seal, like Dead Cells custom mode or Vampire Survivors Banish.
4. **Keep pace with the rival.** The hunters climb across runs (line 365), so player side unlocks need to keep up in options, or the rival outgrows the pool.

### Gaps
- Slay the Spire's own unlock system was not researched.
- Unlock pacing numbers (how many runs per unlock) were not found for any game.
- No data was found on whether Hades' Mirror makes early runs feel weak.

## 6. Double edged and cursed items, and giving something up to take an item

### Takeaway
The strongest trade offs are explicit, permanent or time boxed, and paid in something the player already cares about. Slay the Spire boss relics give +1 energy with a named cost; Hades Chaos gives a curse for a few encounters then a permanent blessing; Risk of Rain 2 lunar items double one thing and halve another and cost their own currency; Isaac devil deals cost max health and shut angel rooms; Dead Cells cursed chests make any hit fatal until enough kills; Darkest Dungeon trinkets pair a bonus with a malus.

### Cited Findings
- **Slay the Spire boss relics with +1 energy and a cost:** Ectoplasm (no more Gold), Sozu (no more potions), Busted Crown (card rewards show 2 fewer cards), Cursed Key (a Curse whenever you open a chest that is not a boss chest), Coffee Dripper (no Rest at Rest Sites), Fusion Hammer (no Smith), Philosopher's Stone (all enemies start combat with 1 Strength), Runic Dome (no enemy intents), Velvet Choker (no more than 6 cards per turn), Mark of Pain (2 Wounds in the draw pile each combat). Without energy: Snecko Eye (draw 2 extra each turn but start Confused) and Calling Bell (a unique Curse plus 3 relics). ([Slay the Spire wiki.gg, Relics List](https://slaythespire.wiki.gg/wiki/Relics_List), fan wiki)
- **Slay the Spire event relics with a cost:** Necronomicon (doubles the first Attack costing 2 or more each turn, plus a special Curse), Mark of the Bloom (no healing), N'loth's Hungry Face (the next chest that is not a boss chest is empty), Gremlin Visage (start each combat Weak), Mutagenic Strength (3 Strength that is lost after the first turn). ([Slay the Spire wiki.gg, Relics List](https://slaythespire.wiki.gg/wiki/Relics_List), fan wiki)
- Ectoplasm is only offered before floor 18, so its "no Gold" cost bites for most of the run. ([Slay the Spire wiki.gg, Relics](https://slaythespire.wiki.gg/wiki/Relics), fan wiki)
- Players say an extra energy is usually worth its downside and advise picking the downside you can work around; Runic Dome is often called the worst. ([Steam forum threads](https://steamcommunity.com/app/646570/discussions/0/2650805212058485290), [more](https://steamcommunity.com/app/646570/discussions/0/1742232339939829970), [more](https://steamcommunity.com/app/646570/discussions/0/594015574338624970), forum opinion)
- **Hades Chaos:** each Chaos boon comes with a curse you endure by clearing rooms before the boon is granted ([Hades Fextralife wiki, Boons](https://hades.wiki.fextralife.com/Boons), fan wiki). Hades II examples: Maimed (lose 6 HP per attack, five encounters), Excruciating (+21% damage taken, four encounters), Hobbled (slower dash, four encounters); curses count encounters that give rewards, and the blessing then lasts the rest of the run ([GameSpot, Hades II Chaos guide](https://www.gamespot.com/articles/hades-2-chaos-boons-guide/1100-6523316/), guide).
- **Hades slots:** taking a boon for a filled slot means giving up the old one. ([Hades Fextralife wiki, Boons](https://hades.wiki.fextralife.com/Boons), fan wiki)
- **Risk of Rain 2 lunar items:** each has a strong upside and a real downside. Shaped Glass roughly doubles base damage and halves max HP, stacking per copy. Brittle Crown gives gold on hit but loses gold in proportion to damage taken. Paid in Lunar Coins, never from chests. ([Pocket Tactics](https://www.pockettactics.com/risk-of-rain-2/lunar-items), guide; [Rogue Ranker](https://rogueranker.com/risk-of-rain-2-lunar-items/), guide)
- **Risk of Rain 2 printers:** you give up a random item of the same tier to take the shown item. ([Steam forum](https://steamcommunity.com/app/632360/discussions/0/1768134097435781647), forum)
- **Isaac devil deals:** 1 or 2 red heart containers, or 3 soul hearts with no red hearts. The Keeper's Bargain trinket gives a 50% chance that deals cost money instead of health; buying a devil deal still stops Angel Rooms appearing. ([Isaac wiki.gg, Devil room](https://bindingofisaacrebirth.wiki.gg/wiki/Devil_room), fan wiki via search extract; [Isaac wiki.gg, Keeper's Bargain](https://bindingofisaacrebirth.wiki.gg/wiki/Keeper%27s_Bargain), fan wiki via search extract)
- **Dead Cells curse:** while cursed, any damage kills you; each kill removes one curse point. Custom mode can set a cursed chest's curse anywhere from 0 to 999. ([Dead Cells wiki.gg, Curse](https://deadcells.wiki.gg/wiki/Curse), fan wiki via search extract)
- **Darkest Dungeon trinkets:** most normal trinkets carry both a bonus and a malus. Accuracy Stone +4 ACC, −1 SPD. Bleed Charm +20% Bleed Resist, −2 DODGE. Ethereal Crucifix +25% damage vs Eldritch, +30% Bleed Resist, −20% max HP. Trophies are mostly pure upside. ([Darkest Dungeon wiki.gg, Trinkets](https://darkestdungeon.wiki.gg/wiki/Trinkets_%28Darkest_Dungeon%29), fan wiki)
- **Kindred Siege trade offs already locked:** kill the chief for his Princess piece or capture him for his tribe (lines 275, 276); strip a piece or bank materials (line 277); beating chiefs and stripping gear raise the hunters' bounty (line 378); omens make the run harder for better drops (line 325); the shrine's Pray says "price unknown" (line 327); an event costs one fight on that road (line 323). ([DESIGN_LOCKS.md](C:/Users/jacob/KindredSiege-Clean/DESIGN_LOCKS.md))

### Inferences
- Kindred Siege already has strong trade offs at the decision level. What it lacks is items whose own text carries a cost.
- Costs that bite are paid in the game's core resource: energy and gold in Slay the Spire, health in Isaac, survival in Dead Cells. In Kindred Siege the core resources are fighters (the roster is your lives, line 279), nerve, and materials that go home.

#### Fit for Kindred Siege (ideas for Jacob, not decisions; every number is his)
1. **Princess pieces with a named cost**, Slay the Spire energy relic style: a big upside plus one plain downside on the wearer.
2. **Chaos style spoils:** a piece that is cursed for the next two fights (weaker) and then turns into its full version. Roads are short, so a two fight curse is a real gamble.
3. **Devil deal style Pal favour or shrine price:** pay with a fighter's health for the run, or a roster life. Heavy, because the roster is your lives.
4. **Lunar style currency:** parts of the hunted monster or chief memories as a separate currency for double edged gear, kept apart from normal materials.
5. **Printer trade:** give up a held piece for a same tier piece of the tribe you are building, a visible way to give something up to finish a set.
6. **Darkest Dungeon style bonus and malus on Mid pieces:** cheap to write, easy to read.
7. **Readability:** Jacob wants no chances shown (line 291), so every cost should read as plain words ("cannot Fall Back"), not a percent.

### Gaps
- Hades I Chaos curse durations were not verified; the examples above are from Hades II.
- Isaac devil and angel room chance rules were not retrieved.
- Risk of Rain 2 Lunar Coin prices, Cleansing Pools and Cauldron exchange rates were not retrieved.
- Slay the Spire Neow opening trade offs and Balatro's sticker penalties were not researched.
