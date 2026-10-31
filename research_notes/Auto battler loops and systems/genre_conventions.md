# Auto battler genre conventions and core loops, with dated numbers

Notes for Kindred Siege. All pages were fetched on 12 Oct 2026 unless a date is given. "Snippet" means the page itself would not open (403 or 404) or was not opened, and the figure comes from the search engine's summary of that page, so it is weaker. These games change every patch, so each number carries its set, patch, or page date. The web search budget for this session ran out before every gap could be chased, so some gaps below could be closed with more searching.

## 1. Shop and draft: shop size, reroll cost, unit tiers, odds by level, copies to upgrade, shared or private pools

### Takeaway
Every game on the list makes the player buy from a random or partly random offer, then grow what they already own. The "chess" family (Auto Chess, Underlords, TFT) shares one template: 5 cost tiers, a 5 slot shop, tier odds tied to player level, 3 copies for a 2 star and 9 for a 3 star, and one pool shared by all 8 players. Later hits cut it down: Hearthstone Battlegrounds sells every minion for 3 and refreshes for 1, Super Auto Pets sells every pet for 3 and rolls for 1 with tiers unlocking by turn, Backpack Battles shows 5 items with 1 gold rerolls, The Bazaar swaps the shop for merchants and events, and Mechabellum drops the random unit shop and keeps randomness only in reinforcement picks.

### Cited Findings

#### Teamfight Tactics (TFT), Set 18 "Enchanted Wilds", live since 26 Aug 2026
- Set 18 launched on 26 Aug 2026 with patch 18.1, with a 4 week PBE because of a game engine move ([Liquipedia, Patch TFT18.1](https://liquipedia.net/tft/Patch_TFT18.1)). Wikipedia says Set 18 moved TFT from the Hextech engine to Unreal Engine in Aug 2026 ([Wikipedia, Teamfight Tactics](https://en.wikipedia.org/wiki/Teamfight_Tactics)).
- Riot planned three sets in 2026: Set 17 launched 15 Apr 2026, Set 18 in summer, Set 19 in winter (snippet, [esports.gg, TFT patch schedule 2026](https://esports.gg/news/teamfight-tactics/tft-patch-schedule-2026/)).
- Shop: 5 slots. The pool is shared by all players; buying removes a copy, selling returns it (Set 18, [tftips, shop specs](https://tftips.app/en/specs/shop)).
- Pool per champion, Set 18: 1 cost: 14 champions x 30 copies = 420. 2 cost: 13 x 25 = 325. 3 cost: 14 x 18 = 252. 4 cost: 14 x 10 = 140. 5 cost: 10 x 9 = 90 ([seemeta, Set 18 odds](https://seemeta.com/en/tft/pbe/odds); [tftips](https://tftips.app/en/specs/shop) gives the same counts).
- Shop odds by player level, Set 18, in percent per slot (page labelled PBE, data "taken from the game client", [seemeta](https://seemeta.com/en/tft/pbe/odds)):

| Level | 1 cost | 2 cost | 3 cost | 4 cost | 5 cost |
|---|---|---|---|---|---|
| 1 | 100 | 0 | 0 | 0 | 0 |
| 2 | 100 | 0 | 0 | 0 | 0 |
| 3 | 75 | 25 | 0 | 0 | 0 |
| 4 | 55 | 30 | 15 | 0 | 0 |
| 5 | 45 | 33 | 20 | 2 | 0 |
| 6 | 30 | 40 | 25 | 5 | 0 |
| 7 | 19 | 30 | 40 | 10 | 1 |
| 8 | 15 | 20 | 32 | 30 | 3 |
| 9 | 10 | 17 | 25 | 33 | 15 |
| 10 | 5 | 10 | 20 | 40 | 25 |
| 11 | 1 | 2 | 12 | 50 | 35 |

- Conflict: [tftips](https://tftips.app/en/specs/shop) (Set 18) matches the table above except level 7, which it gives as 16/30/43/10/1, and it has no level 11 row.
- Upgrading: 3 copies make a 2 star, 3 two stars (9 copies) make a 3 star ([Wikipedia](https://en.wikipedia.org/wiki/Teamfight_Tactics)).
- Levels: the unit cap equals your level. Buying XP costs 4 gold for 4 XP, and you get 2 XP free at the end of each round ([League wiki, TFT:Experience](https://wiki.leagueoflegends.com/en-us/TFT:Experience)).
- XP to reach each level now (after the 14 Sep 2026 mid patch): 2, 2, 6, 10, 20, 36, 56, 68, 68 for levels 2 through 10, so 268 XP from level 1 to 10 ([tftips, player specs](https://tftips.app/en/specs/player)). Patch 18.2 (9 Sep 2026) cut level 7 to 8 from 60 to 56 and levels 8 to 9 and 9 to 10 from 68 to 64. The 14 Sep mid patch put 9 and 10 back to 68 ([Riot, patch 18.2 notes](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-18-2/)).
- A Jan 2026 blog says patch 16.3 raised level 8 to 9 from 68 to 76 (snippet, low quality source, [dtgre](https://www.dtgre.com/2026/01/tft-patch-16-3-meta-shift-fast-9-dead.html?m=0)).
- Carousel (a shared draft round where players run to pick a unit with an item): removed in Set 17 (live 15 Apr 2026) and replaced by "Realm of the Gods" (snippet, [dtgre, Set 17 patch 17.1](https://www.dtgre.com/2026/04/tft-set-17-patch-17-1-changes-buffed-nerfed-broken-guide.html)). It came back in 18.1, with a chance of more or higher cost champions ([Riot, patch 18.1 notes](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-18-1/)).
- Set 18 "Wisps": extra buyable offers that show up in every other shop during planning, in 7 categories (Champion, Combat, Misc, Shop, Gold/XP, Risky, Item). After stage 5, every other Wisp is a Combat Wisp ([Riot 18.1](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-18-1/)). After 18.2 they cost 0 to 8 gold. For example Backrow Star went from 3 to 1, and All Fives from 10 to 8 ([Riot 18.2](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-18-2/)).
- Augments: a choice of 3 on set rounds, in silver, gold and prismatic tiers, added in Set 6 ([Wikipedia](https://en.wikipedia.org/wiki/Teamfight_Tactics)).
- Item offers: a Completed Item Anvil shows 5 choices and always includes one attack, one magic and one tank item. A Component Anvil shows 4. If you do not pick in time, the rightmost is picked for you (Set 18, [tftips, items specs](https://tftips.app/en/specs/items)).

#### Dota Underlords (Valve)
- Copies per hero by tier 1 to 5: 45, 30, 25, 20, 10, shared by all players ([esportstales, last updated 31 Oct 2019](https://www.esportstales.com/dota-underlords/hero-pool-size-and-unit-odds-by-level)).
- Odds per shop slot by level, as printed. Rows 3 and 9 do not add up to 100 on the page ([esportstales](https://www.esportstales.com/dota-underlords/hero-pool-size-and-unit-odds-by-level)):

| Level | T1 | T2 | T3 | T4 | T5 |
|---|---|---|---|---|---|
| 1 | 100 | 0 | 0 | 0 | 0 |
| 2 | 70 | 30 | 0 | 0 | 0 |
| 3 | 60 | 35 | 0 | 0 | 0 |
| 4 | 50 | 35 | 15 | 0 | 0 |
| 5 | 40 | 35 | 25 | 0 | 0 |
| 6 | 35 | 30 | 30 | 5 | 0 |
| 7 | 25 | 30 | 35 | 10 | 0 |
| 8 | 22 | 27 | 35 | 15 | 1 |
| 9 | 20 | 25 | 30 | 20 | 3 |
| 10 | 15 | 21 | 28 | 30 | 6 |
| 11 (item only) | 15 | 20 | 25 | 30 | 10 |

- Three identical heroes combine. Items are handed out after certain rounds. After round 10 each player picks 1 of 4 "Lords". Early access was 20 Jun 2019 and full release 25 Feb 2020 ([Wikipedia, Dota Underlords](https://en.wikipedia.org/wiki/Dota_Underlords)).

#### Dota Auto Chess and Auto Chess (Drodo Studio)
- Copies per piece by cost 1 to 5: 45, 30, 25, 15, 10 ([esportstales, 4 Feb 2019](https://www.esportstales.com/dota-2/auto-chess-hero-units-draw-chances)).
- 5 gold pieces need courier level 8 or more. Draw chance for a 5 gold piece is 1.00% at level 8 and 6.00% at level 10, with no copies taken ([esportstales](https://www.esportstales.com/dota-2/auto-chess-hero-units-draw-chances)).
- Three identical pieces make a 2 star, three 2 stars make a 3 star. The bench holds 8. The pool is shared, so fought over synergies are harder to build ([Wikipedia, Dota Auto Chess](https://en.wikipedia.org/wiki/Dota_Auto_Chess)).

#### Hearthstone Battlegrounds (Blizzard), wiki edited 14 Apr 2026
- Every minion costs 3 coins whatever its tier, sells for 1, and a refresh costs 1. Freeze is free, up to 5 times per recruit phase ([Hearthstone wiki, Battlegrounds](https://hearthstone.wiki.gg/wiki/Battlegrounds)).
- Minions shown: 3 at tier 1, plus 1 at tiers 2, 4 and 6, so 3, 4, 4, 5, 5, 6 ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds); [Tavern Tier page](https://hearthstone.wiki.gg/wiki/Tier-related)).
- Tavern upgrade base cost: tier 2 is 5, tier 3 is 7, tier 4 is 8, tier 5 is 11, tier 6 is 11. The price drops by 1 for each round you stay on the same tier ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)).
- Shared pool, copies of each minion: tier 1 is 15, 2 is 15, 3 is 13, 4 is 11, 5 is 9, 6 is 7, 7 is 5. Distinct minions per tier: 24, 37, 45, 62, 56, 35, 13 ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)). An older, undated PC Gamer list gave 18, 15, 13, 11, 9, 6 (snippet, [PC Gamer](https://pcgamer.com/hearthstone-battlegrounds-cards-units-minions)).
- Tavern spell copies by tier: 5, 7, 9, 11, 9, 7 ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)). Spells were added in patch 28.2 (snippet, [Blizzard 28.2 notes](https://hearthstone.blizzard.com/news/24008697/28-2-patch-notes)).
- Tripling: 3 copies fuse into a golden minion with double stats. Playing it gives a reward spell that discovers a minion from the next tavern tier ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)).
- Season 12 (patch 34.2, 2 Dec 2025) added the Timewarped Tavern on turns 6 and 9. It gives 2 "Chronum" per visit to spend on cards costing 1 or 2, and anything unspent is lost (snippet, [Pocket Gamer](https://www.pocketgamer.com/hearthstone/battlegrounds-season-12-timewarped)). Season 13 (patch 35.2, 14 Apr 2026) brought Trinkets back (snippet, [Hearthstone wiki, Patches](https://hearthstone.wiki.gg/wiki/Patches)).

#### Super Auto Pets (Team Wood Games)
- 10 gold each turn. A pet costs 3, food costs 3 (Sleeping Pill and Strawberry Jam cost 1), a roll costs 1 ([SAP wiki, The Basics, edited 11 Jul 2025](https://superautopets.wiki.gg/wiki/The_Basics)).
- A pet sells for its level: 1, 2 or 3 gold ([SAP wiki, Shop, 2021](https://superautopets.wiki.gg/wiki/Shop); [Two Average Gamers, updated 4 Jun 2026](https://www.twoaveragegamers.com/ultimate-guide-to-super-auto-pets-game-mechanics/)).
- Shop tier unlocks: tier 2 on turn 3, tier 3 on turn 5, tier 4 on turn 7, tier 5 on turn 9, tier 6 on turn 11 ([The Basics](https://superautopets.wiki.gg/wiki/The_Basics)).
- Leveling: 2 XP for level 2, then 3 more for level 3, so 6 copies in total. Each level gives +1 attack and +1 health, plus a choice of two pets from the next tier ([The Basics](https://superautopets.wiki.gg/wiki/The_Basics)).
- Freezing keeps a pet or food through rolls and into the next turn, but it uses up a shop slot. The team has 5 slots ([The Basics](https://superautopets.wiki.gg/wiki/The_Basics)).
- Packs set the pet pool: Turtle 60 pets, Puppy 50, Star 83, Golden 74, Unicorn 96, Danger 61 ([Wikipedia, Super Auto Pets](https://en.wikipedia.org/wiki/Super_Auto_Pets)). A weekly pack is generated every Monday ([Steam page](https://store.steampowered.com/app/1714040/Super_Auto_Pets/)).

#### Storybook Brawl (Good Luck Games, servers shut 1 May 2023)
- Triple reward: choose 1 of 3 Treasures at the level of the tripled unit. You can hold 3 Treasures, and passing on one gives 2 gold ([HSReplay, 23 Sep 2022](https://articles.hsreplay.net/2022/09/23/storybook-brawl-the-perfect-game-for-battlegrounds-players-2/)).
- Leveling: 1 XP per turn plus bonuses, a new level every 3 XP, 6 tiers. Gold cannot buy levels. The tavern offers one buyable spell per turn, and the hand holds 4 cards ([HSReplay](https://articles.hsreplay.net/2022/09/23/storybook-brawl-the-perfect-game-for-battlegrounds-players-2/)).

#### The Bazaar (Tempo)
- There is no single shop. A day has 6 hours. Hours 0, 1, 3 and 4 are merchants or events, hour 2 is a PvE fight, hour 5 is a PvP fight, and each hour gives 1 XP (snippet, 2025, [Mobalytics day guide](https://mobalytics.gg/the-bazaar/guides/day-guide)).
- Item quality runs bronze, silver, gold, diamond. Buying a duplicate of the same quality upgrades the one you own. You cannot merge duplicates you already hold, and diamond is the cap ([Bazaar wiki, Items, edited 9 Apr 2025](https://thebazaar.wiki.gg/wiki/Items)).
- Prices double per tier. Butter costs 2, 4, 8, 16 and is worth 1, 2, 4, 8 (snippet, [bazaardb](https://www.bazaardb.gg/card/5vfx8cve075h2mh0hgme8m3m0/Butter)). Enchanted items cost double at merchants ([Items](https://thebazaar.wiki.gg/wiki/Items)).
- A level every 8 XP. Level rewards include items, loot, upgrades, skill trainers and enchant options ([Bazaar wiki, Level Up, edited 4 Apr 2025](https://thebazaar.wiki.gg/wiki/Level_Up)).

#### Backpack Battles (PlayWithFurcifer), wiki edited 20 Sep 2026
- Shop of 5 items. Rerolls cost 1 gold for the first 4, then 2 gold. You can lock an item ("Reserved"). Each item has a 10% chance to be on sale at 50% off, rounded up ([Backpack Battles wiki, Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)).
- Rarity odds by round, in percent ([Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)):

| Rarity | R1 | R2 | R3 | R4 | R5 | R6 | R7 | R8 | R9 | R10 | R11 | R12 to 18 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Common | 90 | 84 | 75 | 64 | 45 | 29 | 20 | 20 | 20 | 20 | 20 | 20 |
| Rare | 10 | 15 | 20 | 25 | 35 | 40 | 35 | 30 | 28 | 25 | 23 | 20 |
| Epic | 0 | 1 | 5 | 10 | 15 | 20 | 25 | 25 | 25 | 25 | 23 | 20 |
| Legendary | 0 | 0 | 0 | 1 | 5 | 10 | 15 | 15 | 15 | 15 | 17 | 20 |
| Godly | 0 | 0 | 0 | 0 | 0 | 1 | 5 | 10 | 12 | 15 | 17 | 20 |

- Upgrades come from recipes, not duplicates. Adjacent items combine after the fight. For example, a Sword with two Whetstones becomes a Hero Longsword dealing 10 to 12 damage every 1.6 s, against 2 to 4 every 1.4 s for the Sword ([Wikipedia, Backpack Battles](https://en.wikipedia.org/wiki/Backpack_Battles)).
- 7 classes with 5 subclasses each ([Steam page](https://store.steampowered.com/app/2427700/Backpack_Battles/)). The subclass unlocks in round 8 ([Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)).

#### Mechabellum (Game River)
- There is no random unit shop. Any unit can be unlocked and bought. You can unlock 1 unit per round and deploy 2 units per turn by default ([gameplay.tips guide, 10 Mar 2023, early access era](https://gameplay.tips/guides/mechabellum-guide-to-unit-and-mechanism-details.html)).
- Tier 1 units cost 100 supply with no unlock fee. Tier 2 units cost 200 plus a 50 unlock fee. Tier 3 are giant units ([gameplay.tips](https://gameplay.tips/guides/mechabellum-guide-to-unit-and-mechanism-details.html)).
- Reinforcements from round 2: 4 options each round, both players see the same options, each picks 1, and a picked option does not come back. They come in 4 tiers costing 0, 50, 100 and 200. Tier 3 options appear from round 3 and tier 4 from round 4 ([gameplay.tips](https://gameplay.tips/guides/mechabellum-guide-to-unit-and-mechanism-details.html)).
- Each extra tech bought for the same unit costs 200 more ([gameplay.tips](https://gameplay.tips/guides/mechabellum-guide-to-unit-and-mechanism-details.html)).

### Inferences
- The chess family template (5 tiers, 5 slot shop, odds by level, 3 copies, shared pool) is the genre's original DNA. The card games and async games keep "random offer plus upgrade by duplicates" but collapse the price to one number (3 in HS BG and SAP) and swap "buy XP" for turn based unlocks (SAP, Storybook Brawl).
- Price nearly always signals power: TFT cost equals tier, Bazaar prices double per tier, Backpack Battles uses rarity colours that rise by round. The exceptions are HS BG and SAP, where the flat price of 3 makes the shop easy to read, and tier access does the gating instead.
- Shared pools only show up where all 8 players draft live at the same time (TFT, Auto Chess, Underlords, HS BG). The async ghost games (SAP Arena, Backpack Battles, The Bazaar) and PvE modes cannot share a pool in real time, so their offers are private. No page I opened says this outright, so it is an inference.
- Every game gives an upgrade path for things already owned: triples, merges, buying a duplicate, recipes, or unit XP. A single player run would use a private offer like the async games do.
- Mechabellum shows that a fully open roster can work when the randomness moves to a small draft (4 reinforcement options a round) and the decision is about counters, not luck.

### Gaps
- TFT reroll cost (commonly 2 gold), bench size and board hex count were not found on any page I opened.
- TFT level 7 odds disagree between two Set 18 sources (19/30/40/10/1 against 16/30/43/10/1).
- SAP shop slot counts by turn (pets and food) were not found. One guide says up to 6 pets can be shown.
- Underlords and Auto Chess shop size, reroll cost and XP cost were not found on the pages opened.
- The Bazaar merchant offer sizes, reroll rules, and whether a legendary item tier exists in current builds were not checked.
- Storybook Brawl gold per turn and unit cost were not found.

## 2. Economy: income, interest, streaks, selling, levelling costs

### Takeaway
Every game hands out a fixed budget each round and makes the player trade power now against power later. The chess family adds interest (TFT: 1 gold per 10 held, capped at 5) and streak gold (TFT: +1, +2, +3), but several hits have no interest at all. HS BG gives 3 coins rising to 10 that vanish each turn, SAP gives a flat 10 that vanishes, Backpack Battles follows a scripted gold table, and Mechabellum gives both players the same supply, which carries over. Interest and streaks are optional. A per round budget with a spend now or save choice is not.

### Cited Findings

#### TFT
- Base income: 5 gold per planning phase, with less early on: 2 in rounds `1-2` and `1-3`, 3 in `1-4`, 4 in `2-1` ([League wiki, TFT:Gold, edited 29 Mar 2026](https://wiki.leagueoflegends.com/en-us/TFT:Gold)).
- Interest: +1 gold per 10 held, counted before base income, capped at 5 (reached at 50 gold). Some augments raise the cap or remove interest ([TFT:Gold](https://wiki.leagueoflegends.com/en-us/TFT:Gold)). Set 18 table: 0 to 9 gold gives 0, 10 to 19 gives 1, and so on, with 50 or more giving 5 ([tftips, player specs](https://tftips.app/en/specs/player)).
- Winning a PvP round gives +1 gold ([TFT:Gold](https://wiki.leagueoflegends.com/en-us/TFT:Gold)).
- Streaks (wins or losses in a row): the League wiki's current table says 3 to 4 gives +1, 5 gives +2, 6 or more gives +3 ([TFT:Gold](https://wiki.leagueoflegends.com/en-us/TFT:Gold)). tftips (Set 18) says 2 to 4 gives +1, 5 gives +2, 6 or more gives +3 ([tftips](https://tftips.app/en/specs/player)). The two disagree on whether a 2 streak pays. Streak tiers were already retuned in patches V10.6 and V10.8 ([TFT:Gold history](https://wiki.leagueoflegends.com/en-us/TFT:Gold)).
- Selling: since V10.12 (2020), 1 star 1 cost units sell back for full price and others take a 1 gold sell back penalty. For example, a 2 star 3 cost sells for 8 ([TFT:Gold](https://wiki.leagueoflegends.com/en-us/TFT:Gold)).
- Levelling: 4 gold buys 4 XP, plus 2 XP free per round ([TFT:Experience](https://wiki.leagueoflegends.com/en-us/TFT:Experience)). Thresholds are in section 1.
- Hyper Roll (a TFT mode from 14 Apr 2021): no interest, no streak gold, 2 gold per win instead of 1, no XP buying (everyone levels at the start of each stage), and base income "a bit higher" than normal ([Riot, Hyper Roll](https://teamfighttactics.leagueoflegends.com/en-sg/news/game-updates/teamfight-tactics-hyper-roll)). It was retired in May 2025 ([Wikipedia](https://en.wikipedia.org/wiki/Teamfight_Tactics)).
- Patch 17.5 (10 Jun 2026) went after the economy heavy augments that were dominating (snippet, [esports.gg schedule and patch coverage](https://esports.gg/news/teamfight-tactics/tft-patch-schedule-2026/)).

#### Dota Underlords
- Interest: +1 per 10 gold, up to +5 at 50, locked in when combat starts. A win gives 1 bonus gold (snippet, [PCGamesN](https://pcgamesn.com/dota-underlords/gold-how-to-make-money-economy)). A Dec 2019 patch cut the interest cap to 3, reached at 30 gold (snippet, [Liquipedia, 18 Dec 2019 patch](https://www.liquipedia.net/underlords/2019-12-18_Patch)).

#### Auto Chess
- Base income of 5 gold (snippet, [SteelSeries guide](https://steelseries.com/en-gb/blog/fundamentals-dota-auto-chess-101)). Wikipedia says interest is one gold per multiple of ten banked, "up to forty", which would cap it at 4 ([Wikipedia, Dota Auto Chess](https://en.wikipedia.org/wiki/Dota_Auto_Chess)). The SteelSeries guide instead advises holding 50 or more gold to max income (snippet), so the cap is disputed.
- Win or loss streaks of 5 or more pay bonus gold ([Wikipedia](https://en.wikipedia.org/wiki/Dota_Auto_Chess)). At 9 or more the bonus is 3 gold a round (snippet, [SteelSeries](https://steelseries.com/en-gb/blog/fundamentals-dota-auto-chess-101)). A win pays 1 gold at once, before interest is counted ([Wikipedia](https://en.wikipedia.org/wiki/Dota_Auto_Chess)).

#### Hearthstone Battlegrounds
- You start with 3 coins and gain 1 per round, up to 10. Unspent coins do not carry over. Buy 3, sell 1, refresh 1, and tavern upgrades cost 5 to 11 as listed in section 1 ([Battlegrounds wiki, 14 Apr 2026](https://hearthstone.wiki.gg/wiki/Battlegrounds)). The rules page lists no interest and no streak gold.
- A 2019 review of the beta noted that coins do not carry over and everything costs the same, so spending is steady rather than a choice between rushing early and saving for later ([Hearthstone Top Decks, 4 Nov 2019](https://www.hearthstonetopdecks.com/how-does-hearthstone-battlegrounds-compare-to-other-auto-battlers/)).

#### Super Auto Pets
- 10 gold each shop phase, no carryover. Pet 3, roll 1, and a pet sells for its level ([Two Average Gamers, 4 Jun 2026](https://www.twoaveragegamers.com/ultimate-guide-to-super-auto-pets-game-mechanics/); [SAP wiki](https://superautopets.wiki.gg/wiki/The_Basics)).

#### Storybook Brawl
- Passing on a Treasure pays 2 gold ([HSReplay, 2022](https://articles.hsreplay.net/2022/09/23/storybook-brawl-the-perfect-game-for-battlegrounds-players-2/)). Other numbers were not found.

#### The Bazaar
- Heroes start with 8 gold and 5 Income. That is the stated start for Dooley after hotfix 1.0.1 on 16 Apr 2025 ([Bazaar wiki, Version 1.0.1](https://thebazaar.wiki.gg/wiki/Version_1.0.1)), and a guide says every hero starts this way. An "Income Start" option gives 20 gold and 7 Income (snippet, [Mobalytics start of run guide](https://mobalytics.gg/the-bazaar/guides/start-of-run-guide)).
- PvE gold and XP scale with encounter tier: bronze 2 gold and 3 XP, silver 3 and 3, gold 4 and 3, legendary 6 gold and 4 XP (snippet, [Mobalytics PvE encounters](https://mobalytics.gg/the-bazaar/guides/pve-encounters-and-drops)).
- Item prices double per tier (snippet, [bazaardb](https://www.bazaardb.gg/card/5vfx8cve075h2mh0hgme8m3m0/Butter)).

#### Backpack Battles (wiki edited 20 Sep 2026)
- Gold by round: 13, 13, 15, 10, 11, 11, 12, 22, 13, 18, 14, 14, 15, 15, 16, 16, 16, 16 for rounds 1 to 18. Round 8 carries a bonus because the subclass unlocks then ([Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)).
- An older wiki version starts at 12 gold and says leftover gold carries to the next round (snippet, [Backpack Battles fandom wiki](https://backpack-battles.fandom.com/wiki/Game_Mechanics)). A 2024 Steam guide also gives 12 gold in round 1 (snippet, [Steam discussion](https://steamcommunity.com/app/2427700/discussions/0/4290313152625938174)).

#### Mechabellum
- Both players get the same base supply each round, so neither can build an economic lead (snippet, [GamePressure beginner guide](https://www.gamepressure.com/newsroom/mechabellum-guide-tips-and-tricks-for-beginners/z5566d)). Unspent supply carries into the next round ([gameplay.tips, Mar 2023](https://gameplay.tips/guides/mechabellum-guide-to-unit-and-mechanism-details.html)).
- The 2023 guide gives income as "number of turns x 200" with no practical cap ([gameplay.tips](https://gameplay.tips/guides/mechabellum-guide-to-unit-and-mechanism-details.html)). Current income figures were not confirmed.

### Inferences
- There are two economy families and both hold hits. "Bank" economies let you save: TFT, Auto Chess, Underlords, Backpack Battles (older rules), Mechabellum, and The Bazaar, which has a growing Income stat instead of interest. "Use it or lose it" economies wipe the purse each turn: HS BG and SAP. The use it or lose it games are the easiest to learn (see section 7).
- In bank games, the risk comes from saving. Holding 50 gold in TFT means not spending it on board strength, so you can lose health while you wait. Streaks pay either way, so a losing player is still paid for a plan.
- Async games script their income (the Backpack Battles table, the HS BG +1 per turn ramp, The Bazaar's per tier encounter gold). That pacing control matters more when there is no live lobby to balance against.
- Kindred Siege currently has no money decisions, so none of the genre's economic risk (save or spend, roll or level, sell or keep) is present.

### Gaps
- When The Bazaar pays Income (start of day or per hour) was not confirmed.
- Whether Backpack Battles or The Bazaar have any interest style rule was not confirmed.
- Current Mechabellum income per round was not confirmed. A Steam forum thread posts a community formula, but I could not tie it to a specific thread.
- Underlords streak values after its late 2019 to 2020 patches were not confirmed.
- No source gives Storybook Brawl income or prices.

## 3. Synergies: traits, origins and classes, item crafting, tags, enchantments, adjacency, recipes, tribes, tripling

### Takeaway
Every game has a "build identity" layer, but it takes four different shapes: threshold traits (TFT, Auto Chess, Underlords), tag or tribe text without thresholds (HS BG tribes, The Bazaar tags and enchantments, SAP abilities), space based combos (Backpack Battles adjacency and recipes), and counters plus tech (Mechabellum). Items or equipment show up in almost every game. Crafting items from components is optional.

### Cited Findings

#### TFT
- Champions get bonuses when deployed alongside others that share traits. The PC tutorial teaches this as its second step ([League wiki, TFT:Tutorial](https://wiki.leagueoflegends.com/en-us/TFT:Tutorial)).
- Each unit has 3 item slots, and components combine into completed items ([Wikipedia](https://en.wikipedia.org/wiki/Teamfight_Tactics)). Lead designer David Abecassis said at GDC 2020 that letting any two components combine into a final item was a deliberate simplification of Auto Chess's item system ([Pocket Tactics, GDC 2020 talk summary](https://www.pockettactics.com/teamfight-tactics-design)).
- Spatula and Frying Pan are special components that never show up in Component Anvils (Set 18, [tftips items](https://tftips.app/en/specs/items)).
- Augments became a defining mechanic from Set 6, according to Alex Cole, TFT head of gameplay. He also said Set 5's forced risk and reward items drew criticism, so later risk and reward mechanics were made opt in ([GamesBeat, 25 Jun 2026](https://gamesbeat.com/teamfight-tactics-turns-7-riot-games-discusses-lessons-learned-and-the-road-ahead-exclusive-interview/)).
- Each set adds its own mechanic. Set 18's is Wisps, sorted into 7 colour coded categories ([Riot 18.1](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-18-1/)).

#### Auto Chess and Underlords
- Auto Chess pieces belong to races and classes. Enough of one type triggers a synergy with several tiers of buffs, and the shared pool makes fought over synergies harder. Items drop from neutral creeps and can sometimes be combined. Every tenth round, creeps drop relics that bend the rules ([Wikipedia, Dota Auto Chess](https://en.wikipedia.org/wiki/Dota_Auto_Chess)).
- In Underlords, two or more heroes of the same alliance give a bonus to all members. Items are granted after certain rounds, and a Lord is picked after round 10 ([Wikipedia, Dota Underlords](https://en.wikipedia.org/wiki/Dota_Underlords)).

#### Hearthstone Battlegrounds
- Minion types in the pool: Beast, Demon, Dragon, Elemental, Mech, Murloc, Naga, Pirate, Quilboar, Undead, plus neutral ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)).
- At launch it had no items and no alliance bonuses. Synergy came from card text ([Hearthstone Top Decks, 2019](https://www.hearthstonetopdecks.com/how-does-hearthstone-battlegrounds-compare-to-other-auto-battlers/)). Later seasons added layers such as Tavern spells (28.2), Trinkets, Anomalies, Quests, Buddies and the Timewarped Tavern ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds); snippet, [Pocket Gamer](https://www.pocketgamer.com/hearthstone/battlegrounds-season-12-timewarped)).
- The triple reward (discover a minion from the next tier) works as a built in power spike ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)).

#### Super Auto Pets
- No traits. Pets have abilities, and stats are just attack and health (snippet, [Indie Hell Zone review, Oct 2021](https://indiehellzone.com/2021/10/26/super-auto-pets/)). Food buffs pets. Each pack brings its own mechanic, such as Toys (Puppy pack), push (Star), Trumpets (Golden), mana (Unicorn), and Jump Attacks and Transform (Danger) ([Wikipedia](https://en.wikipedia.org/wiki/Super_Auto_Pets)).

#### The Bazaar
- Item tags: Apparel, Aquatic, Core, Dinosaur, Dragon, Food, Friend, Loot, Potion, Property, Ray, Reagent, Relic, Tech, Tool, Toy, Vehicle, Weapon. Triggers: Cooldown, On condition, Start of Combat, Start of Day, Each Hour ([Bazaar wiki, Items](https://thebazaar.wiki.gg/wiki/Items)).
- Enchantments, one per item, where a new one overwrites the old: Deadly (+50% crit chance), Fiery (Burn), Heavy (Slow 2 items for 1 s), Icy (Freeze 1 item for 1 s), Radiant (cannot be Frozen, Slowed or Destroyed), Restorative (Heal), Shielded (Shield), Shiny (+1 Multicast), Toxic (Poison), Turbo (Haste 2 items for 2 s), Obsidian (double damage) ([Items](https://thebazaar.wiki.gg/wiki/Items)).
- Skills come from level rewards and Skill Trainers ([Bazaar wiki, Level Up](https://thebazaar.wiki.gg/wiki/Level_Up)).

#### Backpack Battles
- Many items work off neighbouring items, marked by Star or Diamond slots. Items in Storage do nothing until placed in the inventory ([Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)). The Pan hits harder next to food items ([Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)).
- Status stacks with fixed values: Empower +1 weapon damage per stack, Heat 2% faster triggers per stack, Cold 2% slower per stack, Luck +5% accuracy per stack, Blind minus 5% accuracy per stack, Regeneration and Poison 1 per stack every 2 s, Block absorbs 1 damage per stack ([Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)).
- Recipes combine adjacent items after the fight (see section 1). There are 7 classes with 5 subclasses each ([Steam](https://store.steampowered.com/app/2427700/Backpack_Battles/)).

#### Mechabellum
- Units fall into three roles that counter each other: cheap chaff that soaks fire, chaff clear, and expensive carries (snippet, [Final Boss beginner guide](https://finalboss.io/mechabellum-beginner-guide-to-unit-roles-deployment-and)).
- Each unit has 1 equipment slot. Units gain XP from kills, and stats scale in a straight line, so a level 3 unit has 3 times the level 1 stats ([gameplay.tips](https://gameplay.tips/guides/mechabellum-guide-to-unit-and-mechanism-details.html)). Tech trees are set up in a hangar, and upgrades unlock mid match to counter the enemy ([Steam](https://store.steampowered.com/app/669330/Mechabellum/)).

#### Storybook Brawl
- Support units buff the two units in front of them. Ranged and flying units can attack the back row. Treasures and a chosen hero's passive add build layers ([HSReplay, 2022](https://articles.hsreplay.net/2022/09/23/storybook-brawl-the-perfect-game-for-battlegrounds-players-2/); [Wikipedia, Storybook Brawl](https://en.wikipedia.org/wiki/Storybook_Brawl)).

### Inferences
- Threshold traits (2, 4, 6 style) are a chess family habit, not a genre rule. HS BG, SAP, The Bazaar, Backpack Battles and Mechabellum all succeed without them, and get build identity from text that names a tag, from space, or from counters.
- Every synergy system here is shown to the player: traits in the tutorial, tags and enchantment names on items, Star and Diamond markers on items. None of the hits builds its identity layer on hidden combos. Kindred Siege's two hidden combos are the opposite of this pattern.
- Crafting (TFT components, Backpack Battles recipes) lets a player build identity with few units. That is the shape closest to a game with a small roster, but this is a structural observation, not a recommendation.
- Power spikes are built in everywhere: triples, merges, recipe upgrades, Level Up rewards, the HS BG triple discover. They create the reward half of risk and reward.

### Gaps
- Set 18 TFT trait thresholds (exact breakpoints) were not opened, nor was the number of TFT components and recipes (commonly 9 components and 45 or more recipes, not checked).
- How many HS BG tribes appear in one lobby was not confirmed.
- Backpack Battles recipe count and how recipes are shown to new players: the recipe page returned 404.
- Which Bazaar items use board adjacency was not confirmed.

## 4. Positioning: board shapes, rows, front and back lines, how much it matters

### Takeaway
Positioning ranges from decisive free placement (Mechabellum), through grid boards (TFT hexes, the Auto Chess and Underlords 8x8), two rows (Storybook Brawl) and a single ordered line (HS BG, SAP), to inventory space where items, not units, are arranged (Backpack Battles, The Bazaar). Not one of these games lets the player control units during the fight. That is the clearest convention in the genre.

### Cited Findings
- TFT uses a hexagonal board ([Wikipedia](https://en.wikipedia.org/wiki/Teamfight_Tactics)), and the tutorial teaches that positioning affects results ([TFT:Tutorial](https://wiki.leagueoflegends.com/en-us/TFT:Tutorial)). Set 18 overtime starts 30 s into combat and lasts up to 15 s. Damage and attack speed grow by x1.1 per second. Healing, shields and crowd control drop 10% per second, down to minus 70%. Units are immune to crowd control from 38 s ([tftips, battle specs](https://tftips.app/en/specs/battle)). Since 18.1, units no longer retarget after recovering from crowd control ([Riot 18.1](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-18-1/)).
- In Auto Chess, pieces go in the first four rows of the board, and players cannot control them in combat ([Wikipedia, Dota Auto Chess](https://en.wikipedia.org/wiki/Dota_Auto_Chess)). Underlords uses chess like positioning on an 8x8 grid ([Wikipedia, Dota Underlords](https://en.wikipedia.org/wiki/Dota_Underlords)).
- HS BG: the side with more minions attacks first, minions attack left to right, targets are random unless Taunt is present, and the board holds 7 ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)). A 2019 review said you cannot position as precisely as in other auto battlers ([Hearthstone Top Decks](https://www.hearthstonetopdecks.com/how-does-hearthstone-battlegrounds-compare-to-other-auto-battlers/)).
- Storybook Brawl: 7 slots split into 4 front and 3 back ([HSReplay](https://articles.hsreplay.net/2022/09/23/storybook-brawl-the-perfect-game-for-battlegrounds-players-2/)).
- SAP: start of battle abilities fire first, then the two front pets hit each other at the same time, and fainted pets are replaced by the next in line ([SAP wiki, The Basics](https://superautopets.wiki.gg/wiki/The_Basics)). One guide says pets act from highest attack down ([Two Average Gamers](https://www.twoaveragegamers.com/ultimate-guide-to-super-auto-pets-game-mechanics/)).
- Backpack Battles: classes start with 12 to 14 of 63 possible tiles. Bags add tiles, and items can be moved and rotated ([Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)). Fights run without direct control ([Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)).
- The Bazaar: items take 1, 2 or 3 board slots ([Bazaar wiki, Items](https://thebazaar.wiki.gg/wiki/Items)). A beginner guide advises refining positioning every run (snippet, [fdaytalk](https://www.fdaytalk.com/the-bazaar-beginner-guide/)).
- Mechabellum: placed units cannot be moved or directly controlled ([Wikipedia](https://en.wikipedia.org/wiki/Mechabellum)). From round 2, units can deploy on the enemy's side as a flank. They take 10 s to deploy, during which their health rises from 50% ([gameplay.tips](https://gameplay.tips/guides/mechabellum-guide-to-unit-and-mechanism-details.html)). The Steam page says victory comes from your wits, not your hands ([Steam](https://store.steampowered.com/app/669330/Mechabellum/)).

### Inferences
- How much positioning weighs, ranked by what each game lets you change: Mechabellum (free field, flanks, decisive) is highest, then TFT, Auto Chess and Underlords (grid, front and back), then Storybook Brawl (two rows), then HS BG and SAP (order only), with Backpack Battles and The Bazaar off this axis because they arrange items, not fighters.
- No game here gives orders during the fight. Kindred Siege's mid fight orders have no genre template to copy among these nine games, and the genre puts all of its player agency into the build phase.
- Overtime rules (TFT) or fatigue (Backpack Battles) are the standard tool to stop stalled fights.

### Gaps
- TFT board dimensions (commonly 4 rows of 7 hexes per player) and bench size were not opened.
- No numbers were found on how often positioning swings fights in any of these games.

## 5. Round structure, health or lives, damage rules, match length

### Takeaway
There are two loss models. Health pool games (TFT, HS BG, Underlords, Mechabellum, Storybook Brawl) charge damage that grows with the stage and with how badly you lost. Lives games (SAP, Backpack Battles, The Bazaar's Prestige, TFT's PvE Tocker's Trials) charge a set loss and end at a win target, usually 10 wins. The async and single player formats all use lives plus a win target. Live 8 player games run 20 to 40 minutes. Async runs last 10 to 18 fights.

### Cited Findings

#### TFT
- Stage 1 has 4 rounds: `1-1` is a shared draft, and `1-2` to `1-4` are PvE with no player damage. From stage 2, each stage has 7 rounds: PvP in rounds 1, 2, 3, 5 and 6, the carousel in round 4, and PvE in round 7 ([tft.ninja, stages, updated 28 Sep 2026](https://tft.ninja/guides/game-mechanics/stages)).
- Damage on a loss is the stage base plus 1 per surviving enemy unit, whatever its star level ([tft.ninja, damage, patch 18.1, updated 6 Oct 2026](https://tft.ninja/guides/game-mechanics/player-damage-calculation)).
- Stage base damage, with two Set 18 sources that disagree. tft.ninja (18.1): 0, 2, 5, 8, 10, 12, 17 for stages 1 to 7 and later ([tft.ninja](https://tft.ninja/guides/game-mechanics/player-damage-calculation)). tftips (Set 18): 0, 2, 6, 7, 10, 12, 17, and 150 at stage 8 ([tftips player specs](https://tftips.app/en/specs/player)). tftflow (Set 18) also has 6 and 7 for stages 3 and 4 (snippet, [tftflow](https://tftflow.com/tables/set18/player-damage-calculator)).
- History: patch 14.8 changed stage damage from 0, 0, 3, 5, 7, 9, 15, 150 to 0, 2, 5, 7, 9, 11, 17, 150, and damage per surviving unit from 2, 2, 2, 1, 1 to a flat 1 (snippet, [GGRecon](https://www.ggrecon.com/guides/tft-update-14-8-patch-notes/)).
- Length: 30 to 40 minutes. The first player is usually out around stage 4 (15 to 20 minutes in), and most games end in stage 6 (30 to 35 minutes) ([tft.ninja stages](https://tft.ninja/guides/game-mechanics/stages)). Riot's Alex Cole said in Jun 2026 that the core game is still about 30 to 35 minutes ([GamesBeat](https://gamesbeat.com/teamfight-tactics-turns-7-riot-games-discusses-lessons-learned-and-the-road-ahead-exclusive-interview/)).
- Hyper Roll (2021): 20 health, with flat damage per loss of 2 in stages 1 to 4, 4 in stages 5 to 7, and 6 from stage 8, so the size of the loss does not matter. Riot said a game would almost certainly finish in 20 minutes ([Riot, Hyper Roll](https://teamfighttactics.leagueoflegends.com/en-sg/news/game-updates/teamfight-tactics-hyper-roll)).
- Double Up: 4 teams of 2 sharing one health bar ([Wikipedia](https://en.wikipedia.org/wiki/Teamfight_Tactics)).
- Tocker's Trials (27 Aug to 24 Sep 2024, PvE only): 30 rounds, 6 bosses, 3 lives in place of health, no timers, points scored each round. It used the set's champions and augments, and gold and levelling worked like a normal game ([Riot, Tocker's Trials](https://teamfighttactics.leagueoflegends.com/en-gb/news/game-updates/new-tft-workshop-mode-tockers-trials)).

#### Hearthstone Battlegrounds
- 8 players. Heroes start with 30 health (Patchwerk has 60), and some heroes start with armor ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)).
- Damage is your tavern tier plus the tiers of your surviving minions, with tokens counting as tier 1. If no minions survive on either side, it is a draw ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)).
- Damage cap in solo games from patch 31.2 (3 Dec 2024): 5 at first, 10 from turn 4, 15 from turn 8, and no cap once you are in the top 4 ([Hearthstone wiki, Patch 31.2](https://hearthstone.wiki.gg/wiki/Patch_31.2.0.212197)). The original cap from patch 21.2 was a flat 15 until the first player died (snippet, [Blizzard 21.2 notes](https://playhearthstone.com/en-us/news/23712592/21-2-patch-notes)).
- The recruit timer is 60 s in round 1, rising 15 s per round to 120 s. Duos has 4 teams of 2 sharing one life total ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)).

#### Underlords and Auto Chess
- Underlords classic mode starts at 100 health, which drops by the number of surviving enemy heroes after each lost fight. It also has a faster Knockout mode and Duos with shared health ([Wikipedia](https://en.wikipedia.org/wiki/Dota_Underlords)).
- Auto Chess runs several dozen rounds, each with income, preparation and battle phases ([Wikipedia](https://en.wikipedia.org/wiki/Dota_Auto_Chess)).

#### Super Auto Pets
- Arena: win 10 fights (10 trophies). Lives: Normal 5, Easy 7, Hard 5. Versus mode has 6 lives and is last player standing. Each loss costs 1 life, a draw costs nothing, and you get 1 life back on turn 3 if you have lost any ([SAP wiki, The Basics, Jul 2025](https://superautopets.wiki.gg/wiki/The_Basics); [Wikipedia](https://en.wikipedia.org/wiki/Super_Auto_Pets)). An older review describes 10 lives (snippet, undated, [Rubicon](https://www.rubiconline.com/?p=71188)).
- The itch.io page lists an average session of about half an hour (snippet, [itch.io](https://teamwood.itch.io/super-auto-pets)).

#### Storybook Brawl
- The winner deals their tavern level, plus 1 per surviving unit, plus 3 per surviving golden unit, for a maximum of 27 ([HSReplay](https://articles.hsreplay.net/2022/09/23/storybook-brawl-the-perfect-game-for-battlegrounds-players-2/)).

#### The Bazaar
- You start with 20 Prestige, and a PvP loss costs more Prestige on later days (snippet, Mar 2025, [Mobalytics Prestige guide](https://mobalytics.gg/the-bazaar/guides/prestige)). One summary says the loss equals the day number. Not confirmed.
- 10 PvP wins is the goal for the top rewards. Opponents are ghosts of players on the same day (snippet, [Mobalytics day guide](https://mobalytics.gg/the-bazaar/guides/day-guide)). Levels raise max health ([Level Up](https://thebazaar.wiki.gg/wiki/Level_Up)).

#### Backpack Battles
- Up to 18 rounds. Win 10 fights before losing 5. After 10 wins you can carry on in Survival Mode to round 18, where 5 losses can leave you a lower rank ([Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)).
- Player health by round: 25, 35, 45, 55, 70, 85, 100, 115, 130, 150, 170, 190, 210, 230, 260, 290, 320, 350 for rounds 1 to 18 ([Game Mechanics, 20 Sep 2026](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)).
- Fights usually last 10 to 20 s ([Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)). Fatigue begins at "nightfall" and deals rising damage to both players ([Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)).

#### Mechabellum
- Base health is 4000, adjusted by your specialist and starting pack. The loser takes damage equal to the base supply cost of the enemy's surviving units. A 100 supply pack deals 100, 200 deals 200, 300 deals 250, 400 deals 350, 500 deals 400, and 800 deals 500. Summoned, produced and hacked units deal a quarter. If both players drop below 0 in the same round, the one less far below wins. Tournaments are capped at 13 rounds ([Mechabellum wiki, Player Hit Points, edited 17 Sep 2026](https://mechabellum.wiki.gg/wiki/Player_Hit_Points)).

### Inferences
- Damage formulas almost always include a "how badly you lost" term: surviving units in TFT, HS BG, Underlords, Storybook Brawl and Mechabellum. The flat loss models (SAP lives, Backpack Battles losses, Hyper Roll) drop it for simplicity.
- Damage rises over time in every health pool game (stage base, tavern tier, supply value), so early losses are cheap and late losses decide the game. HS BG and TFT both had to add early caps or low early numbers to keep early games from ending too soon.
- Async and PvE formats settle on a win target of about 10 and a small lives pool: SAP 10 wins with 5 lives, Backpack Battles 10 wins before 5 losses, The Bazaar 10 wins with 20 Prestige, Tocker's Trials 3 lives over 30 rounds. A SAP Normal run therefore lasts between 10 and about 15 turns. That is my arithmetic from the rules above.
- A Kindred Siege run (camp, tribes of two fights plus a chief, then a final) has fewer fights than these runs unless there are many tribes. The number of fights sets how many shop and upgrade beats a run can hold.

### Gaps
- TFT starting health (commonly 100) was not on any page I opened.
- HS BG average game length, Storybook Brawl health, The Bazaar's exact Prestige loss table and day count, and Mechabellum's typical round count were not found.

## 6. Which conventions are mandatory and which are optional

### Takeaway
Six things appear in every game studied: a build phase followed by a fight the player does not control, a scarce currency topped up each round, a random or partly random offer, a way to upgrade what you own, a visible synergy hook, and a loss budget. Interest, streak gold, shared pools, XP buying, threshold traits, item crafting, grid positioning and live opponents are all missing from at least one well reviewed game, so they are optional.

### Cited Findings
- No direct control in combat: Auto Chess ([Wikipedia](https://en.wikipedia.org/wiki/Dota_Auto_Chess)), Mechabellum ([Wikipedia](https://en.wikipedia.org/wiki/Mechabellum)), Backpack Battles ([Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)), and TFT, where champions fight automatically ([TFT:Tutorial](https://wiki.leagueoflegends.com/en-us/TFT:Tutorial)).
- Hits without interest: HS BG, where coins do not carry over ([Battlegrounds wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)). SAP, where gold does not carry over ([Two Average Gamers](https://www.twoaveragegamers.com/ultimate-guide-to-super-auto-pets-game-mechanics/)), with 91% positive of 25,080 Steam reviews ([Steam](https://store.steampowered.com/app/1714040/Super_Auto_Pets/)). TFT Hyper Roll, with no interest or streaks, ran from Apr 2021 to May 2025 ([Riot](https://teamfighttactics.leagueoflegends.com/en-sg/news/game-updates/teamfight-tactics-hyper-roll); [Wikipedia](https://en.wikipedia.org/wiki/Teamfight_Tactics)).
- A hit without a random unit shop: Mechabellum, where any unit can be bought and the only random offers are reinforcement picks ([gameplay.tips](https://gameplay.tips/guides/mechabellum-guide-to-unit-and-mechanism-details.html)). It has 84% positive of 9,984 Steam reviews ([Steam](https://store.steampowered.com/app/669330/Mechabellum/)), and Rock Paper Shotgun named it one of its favourite games of 2024 ([Wikipedia](https://en.wikipedia.org/wiki/Mechabellum)).
- Hits without trait thresholds: HS BG, which had no alliances or items at launch ([Hearthstone Top Decks](https://www.hearthstonetopdecks.com/how-does-hearthstone-battlegrounds-compare-to-other-auto-battlers/)). SAP, with abilities only ([Wikipedia](https://en.wikipedia.org/wiki/Super_Auto_Pets)). Backpack Battles, with adjacency and recipes, which sold 640,000 copies in its first month and has 92% positive of 5,598 Steam reviews ([Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles); [Steam](https://store.steampowered.com/app/2427700/Backpack_Battles/)). The Bazaar, with tags and enchantments ([Bazaar wiki, Items](https://thebazaar.wiki.gg/wiki/Items)).
- Hits without XP buying: SAP, where tiers unlock by turn ([SAP wiki](https://superautopets.wiki.gg/wiki/The_Basics)), and Storybook Brawl, where gold cannot buy levels ([HSReplay](https://articles.hsreplay.net/2022/09/23/storybook-brawl-the-perfect-game-for-battlegrounds-players-2/)).
- Games without live opponents: SAP Arena, Backpack Battles and The Bazaar all fight asynchronous ghosts ([SAP Steam](https://store.steampowered.com/app/1714040/Super_Auto_Pets/); [Backpack Battles Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles); [The Bazaar Steam](https://store.steampowered.com/app/1617400/The_Bazaar/)). TFT's Tocker's Trials was PvE only ([Riot](https://teamfighttactics.leagueoflegends.com/en-gb/news/game-updates/new-tft-workshop-mode-tockers-trials)). Underlords offers single player matches against bots ([Wikipedia](https://en.wikipedia.org/wiki/Dota_Underlords)). Mechabellum has solo or cooperative Survival against AI waves ([Steam](https://store.steampowered.com/app/669330/Mechabellum/)).
- A single player roguelike auto battler: Despot's Game (Konfa Games, published by tinyBuild, released 29 Sep 2022). It has shops for units and weapons, a food meter that rises with army size, up to 49 units, and procedural labyrinths. It scores 73 on Metacritic ([Wikipedia, Despot's Game](https://en.wikipedia.org/wiki/Despot%27s_Game)).
- Signs of scale: TFT had over 33 million monthly players by Sep 2019 ([Wikipedia](https://en.wikipedia.org/wiki/Teamfight_Tactics)). Dota Auto Chess had over 8 million players by May 2019 ([Wikipedia](https://en.wikipedia.org/wiki/Dota_Auto_Chess)). Underlords passed 200,000 concurrent Steam players at launch ([Wikipedia](https://en.wikipedia.org/wiki/Dota_Underlords)). The Bazaar's Steam reviews are 78% positive of 6,225 overall but 46% of the last 321, and it launched on Steam on 13 Aug 2025 ([Steam](https://store.steampowered.com/app/1617400/The_Bazaar/)). Storybook Brawl's servers shut on 1 May 2023 after its owner FTX went bankrupt ([Wikipedia](https://en.wikipedia.org/wiki/Storybook_Brawl)).
- Riot's lesson after Set 5: risk and reward mechanics land better as opt in choices than as forced ones ([GamesBeat](https://gamesbeat.com/teamfight-tactics-turns-7-riot-games-discusses-lessons-learned-and-the-road-ahead-exclusive-interview/)).

### Inferences
Convention matrix, built from the cited findings in sections 1 to 6. "?" means not found. "inf" means my inference.

| Convention | TFT | Underlords | Auto Chess | HS BG | SAP | Storybook Brawl | The Bazaar | Backpack Battles | Mechabellum |
|---|---|---|---|---|---|---|---|---|---|
| Build phase, then fight with no control | yes | yes | yes | yes | yes | yes | yes | yes | yes |
| Random offer to choose from | shop of 5 | shop | shop | 3 to 6 minions | shop | tavern | merchants, events | shop of 5 | 4 reinforcements only |
| Pool shared with rivals | yes | yes | yes | yes | no (inf) | ? | no (inf) | no (inf) | shared reinforcement options |
| One flat unit price | no | no | no | yes, 3 | yes, 3 | ? | no | no | no |
| Gold carries over | yes | yes | yes | no | no | ? | ? | yes (older wiki) | yes |
| Interest | 1 per 10, cap 5 | cap 5, later 3 | cap 4 or 5 | no | no | ? | Income stat instead | none found | none found |
| Streak gold | yes | yes | 5 or more | none listed | none listed | ? | none found | none found | none found |
| Buy XP or levels | yes | yes | yes | buy tavern tiers | no, by turn | no, by turn | no, by hour | no | 1 unlock per round |
| Merge duplicates | 3 and 9 | 3 | 3 and 9 | golden plus discover | merge plus next tier pet | treasure choice | buy duplicate to upgrade | recipes instead | unit XP levels |
| Threshold traits | yes | yes | yes | no | no | ? | no | no | no |
| Items or equipment | crafted, 3 slots | granted | creep drops | spells, trinkets later | food | treasures | items are the build | items are the build | equipment, techs |
| Positioning | hex grid | 8x8 grid | 8x8, 4 rows | order in a line | order in a line | 2 rows | board slots | inventory grid | free field |
| Loss model | health, stage damage | 100 health | ? | 30 health, early caps | 5 lives, 10 wins | damage max 27 | 20 Prestige, 10 wins | 5 losses, 10 wins | 4000 health |

- Near universal (every column has it): build then a fight with no control, a random or partly random offer, a scarce per round currency, an upgrade path for owned pieces, a visible synergy hook, and a loss budget.
- Optional (at least one hit lacks it): interest, streak gold, shared pool, a flat price, XP buying, threshold traits, crafted items, grid positioning, live opponents.
- Mapping Kindred Siege against this (neutral, not a design proposal): its fights already run without unit control (convention met), and its player orders go beyond the genre. On the six near universal blocks, the brief describes one item, no builds, two hidden combos and few decisions. So the offer, currency, upgrade path and visible synergy blocks look absent or thin, and the loss budget is unclear. TFT's own single player mode (Tocker's Trials) kept gold, levelling, the shop and augments when it dropped PvP. That suggests these blocks, not the opponents, are what make a PvE auto battler still feel like one (inference).

### Gaps
- No source quantifies how much each convention adds to retention or reviews. "Optional" here only means "a well reviewed game exists without it".
- Underlords' decline and end of updates (it stopped being updated around 2020) was not sourced in this session.

## 7. What makes them intuitive: onboarding, concepts in the first game, readability tools

### Takeaway
The easiest games to pick up cut the numbers down to a handful (SAP: 10 gold, 3 per pet, 1 per roll, 5 slots; HS BG: 3 to buy, 1 to sell, 1 to refresh), use one flat price, show every ability in the shop, drop timers (async play), reuse content players already know (Hearthstone cards, League champions), and teach in three steps (TFT's tutorial: recruit, deploy, battle). TFT's guiding rule from its lead designer was "tough choices, not touch execution".

### Cited Findings
- TFT GDC 2020 (David Abecassis, lead designer): the team capped how many units can be on the field and let matching units merge to free space, so juggling became quick choices rather than fiddly execution. Items were simplified so that any two components make an item, and the team embraced extreme outcomes that come out of simple systems. TFT was built in about 18 weeks ([Pocket Tactics](https://www.pockettactics.com/teamfight-tactics-design)). Wikipedia says it began as a 12 person project with 8 weeks of prototyping and 10 weeks to release ([Wikipedia](https://en.wikipedia.org/wiki/Teamfight_Tactics)).
- TFT PC tutorial, 3 steps: Recruit (buy with gold, drag from bench to board, 3 copies make a 2 star), Deploy (shared traits give bonuses, positioning matters), Battle (auto fight, your avatar takes damage on a loss, out at 0) ([League wiki, TFT:Tutorial, edited 28 Nov 2024](https://wiki.leagueoflegends.com/en-us/TFT:Tutorial)).
- TFT mobile tutorial (patch 11.19, 22 Sep 2021): a smaller roster from Set 1 and a limited item pool ([League wiki, V11.19](https://wiki.leagueoflegends.com/en-us/V11.19_(Teamfight_Tactics))).
- TFT readability: since V9.24 the Buy XP tooltip shows how much gold is needed for the next level ([TFT:Gold](https://wiki.leagueoflegends.com/en-us/TFT:Gold)). Set 18 Wisps each have their own colour and icon, shown by the tactician's health bar after purchase ([Riot 18.1](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-18-1/)).
- Hyper Roll's stated goal was a simpler economy so players focus on building the strongest team and rerolling ([Riot](https://teamfighttactics.leagueoflegends.com/en-sg/news/game-updates/teamfight-tactics-hyper-roll)). Tocker's Trials had no timers, and coverage called it a good way for new players to learn (snippet, [Pocket Tactics](https://www.pockettactics.com/tft/tockers-trials)).
- HS BG was built to be easy to pick up by reusing Hearthstone cards. It had no items and no alliances, and coins do not carry over while everything costs the same. The same 2019 review said other auto battlers take tens of hours to get comfortable with ([Hearthstone Top Decks](https://www.hearthstonetopdecks.com/how-does-hearthstone-battlegrounds-compare-to-other-auto-battlers/)).
- SAP: stats are only attack and health (snippet, [Indie Hell Zone](https://indiehellzone.com/2021/10/26/super-auto-pets/)). Every pet and food ability is shown in game (snippet, [Two Average Gamers beginners guide](https://www.twoaveragegamers.com/above-average-super-auto-pets-beginners-guide/)). Arena has no timers and is played at your own pace ([Steam](https://store.steampowered.com/app/1714040/Super_Auto_Pets/)). Engadget (21 Mar 2022) headlined it as an intro to auto battlers ([Wikipedia references](https://en.wikipedia.org/wiki/Super_Auto_Pets)).
- Underlords: IGN (Jul 2019) called it the easiest auto battler for novices, partly thanks to its interface. Polygon (Jun 2019) found it more appealing to newcomers ([Wikipedia](https://en.wikipedia.org/wiki/Dota_Underlords)).
- Auto Chess mobile: VentureBeat found it confusing for players who had not played the PC version ([Wikipedia](https://en.wikipedia.org/wiki/Dota_Auto_Chess)).
- Storybook Brawl: simpler than HS BG, with fewer keywords and less information about opponents ([HSReplay](https://articles.hsreplay.net/2022/09/23/storybook-brawl-the-perfect-game-for-battlegrounds-players-2/)). PC Gamer called it crowded, with so many random elements that games varied widely ([Wikipedia](https://en.wikipedia.org/wiki/Storybook_Brawl)).
- Backpack Battles: IGN (6/10, 15 Mar 2024) found the shop and packing phase very satisfying but said there were too few items to keep runs fresh. A combat log was part of the early access plan ([Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)). Unranked has no timers ([Steam](https://store.steampowered.com/app/2427700/Backpack_Battles/)).
- Mechabellum: guides tell beginners to start against bots (snippet, [TheGamer](https://www.thegamer.com/mechabellum-beginner-tips-tricks/)) and to check how much damage each unit dealt last round before upgrading it (snippet, [Pro Game Guides](https://progameguides.com/mechabellum/mechabellum-tips-guide-reinforcements-research-points-specialists/)).
- The Bazaar's Steam page sells async play as having no real time timers to worry about ([Steam](https://store.steampowered.com/app/1617400/The_Bazaar/)).

### Inferences
- Rough count of separate systems a new player meets in a first full game (my count from the rules above, not a published figure): SAP about 9 (gold, buy, sell, roll, freeze, merge, food, line order, lives and trophies). HS BG about 10 (coins ramp, buy, sell, refresh, freeze, tavern upgrade with falling price, triple, hero power, tribes, attack order). Mechabellum about 10. Backpack Battles and The Bazaar about 14 to 15. TFT about 18 (shop, reroll, gold, interest, streaks, XP and levels, odds, star ups, bench, traits, components and items, augments, carousel, PvE rounds, hex positioning, player damage, overtime, plus the set mechanic). The games praised as easy to learn sit at the low end.
- Easy games share a pattern: one flat price, five or fewer numbers to remember, abilities printed in the shop, no clock, and gradual unlocks (SAP tiers every 2 turns, Backpack Battles rarity odds rising by round, the TFT tutorial's 3 steps). Hard games pile up systems and lean on guides and outside tools.
- Readability helpers found: XP cost hints (TFT), colour coded offer categories (TFT Wisps), per unit damage stats (Mechabellum), visible abilities (SAP), and a combat log (Backpack Battles). Kindred Siege's two hidden combos run against this pattern, since the hits make their synergies visible.

### Gaps
- No page opened describes TFT's trait tracker, post fight damage chart, or item recipe sheet, though these are standard in the client. Not checked.
- No sources found on HS BG's or The Bazaar's in game tutorials, or on how Backpack Battles teaches recipes.
- No published playtest data on how many concepts new players can absorb in a first game. The counts above are my own.
