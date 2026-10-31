# Synergy design in auto battlers and roguelikes: build time setup versus runtime combos

Notes for the Kindred Siege squad combat fix, researched 25 Oct 2026. Every line under Cited Findings carries a link. Lines under Inferences are my reasoning, not a source. Some pages blocked the fetch tool (403, 402 or a login wall). Where a fact came only from a search engine summary of a page, the line says "search summary".

## 1. Backpack Battles and The Bazaar: adjacency and item timing synergies, how players build, how the fight shows the payoff, how many items interact

### Takeaway
Both games put nearly all synergy into a board the player arranges before the fight (adjacency, star slots, item size, cooldown speed). A short automatic fight then fires items on timers. Backpack Battles explains the payoff after the fight with a damage log per item. The Bazaar needs an internal cooldown so runtime trigger chains cannot loop.

### Cited Findings

Backpack Battles (PlayWithFurcifer, a two person German studio; early access 8 Mar 2024, 1.0 on 13 Jun 2025)
- Structure: up to 18 rounds, each a shop phase then a battle phase. A run is won with 10 battle wins before 5 losses. [Wikipedia: Backpack Battles](https://en.wikipedia.org/wiki/Backpack_Battles)
- Items have different shapes and sizes and interact by placement. The Pan deals more damage next to food items. The Mrs. Struggles doll triggers faster when dark items are near one of its hands, so even the side of an item matters. [Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)
- Fight: no player control, items trigger at set intervals and based on nearby items, battles typically last about 10 to 20 seconds. [Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)
- Recipes are adjacency too: "For two items to combine, they must be adjacent to each other." A Hero Sword next to two Whetstones becomes the Hero Longsword. [Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles). The fusion happens after the next fight (search summary). [PC Games, page 1](https://www.pcgames.de/Indie-Games-Thema-239276/Specials/Backpack-Battles-Builds-Early-Access-Inventar-getestet-Auto-Battler-1442869/)
- Star slots: the item's description says what must sit on each empty star. Each star covers one adjacent item, and "If two or more stars go above the same item, only one star will be activated." Player tip: "at best you try to get a different item for every star to get covered", for example a pan with four stars beside four bananas gets four buffs. The asker's confusion shows stars light up when met: "sometimes one star will light up on item, sometimes not." [Steam discussion](https://steamcommunity.com/app/2427700/discussions/0/4290313152638370831)
- How the fight shows the payoff (PC Games review, translated): items fire on cooldowns with simple animations (flying pans, potions being drunk, abilities activating). Fights end within seconds. There is a speed up with several steps and pause at any time. The player cannot change the outcome mid fight, yet the reviewer found watching "very exciting", especially builds that "come into their own" over time. After each fight a full action log with statistics shows which weapons dealt how much damage, when stamina ran out and when effects triggered. Players use it to adjust what they buy. [PC Games, page 2](https://www.pcgames.de/Indie-Games-Thema-239276/Specials/Backpack-Battles-Builds-Early-Access-Inventar-getestet-Auto-Battler-1442869/2/)
- Developers (Automaton interview, translated): Super Auto Pets was the main inspiration, an auto battler you play "at your own pace". Backpack Hero and Diablo 2 informed item management, Dota 2 the item combining, and Hearthstone the feel of "objects in the game that you can touch and move". Beginner advice: "Leave your build to the shop and buy what comes up", since buy and sell prices are equal. Stamina is a brake: carry too many weapons and you cannot attack. [Automaton](https://automaton-media.com/articles/interviewsjp/20240308-285166/)
- Rarity ramp: "Rare, powerful items are more likely to appear in later rounds." [Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)
- Scale and reception: 640,000 units sold in the first month, over 36,000 concurrent players at launch. IGN (6/10) said the shop phase was "an enjoyable process that tickles the part of your brain that fires off in other games like Unpacking." [Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)

The Bazaar (created by Reynad, announced 6 Apr 2018, released March 2025)
- It began as a deck builder. That was scrapped for "Hero Builder" mechanics. [The Bazaar wiki](https://thebazaar.wiki.gg/wiki/The_Bazaar)
- Items "come in three sizes: Large (3 slots), Medium (2 slots), and Small (1 slot)." [Bazaar wiki: Items](https://thebazaar.wiki.gg/wiki/Items)
- Item trigger types: Cooldown, On condition, Start of Combat, Start of Day, Each Hour. [Bazaar wiki: Items](https://thebazaar.wiki.gg/wiki/Items)
- Brake on runtime chains: "All items seem to have an internal cooldown restricting them from triggering too quickly." The wiki's example is Trebuchet (Haste on Burn) plus Pufferfish (Poison on Haste), which could otherwise loop. [Bazaar wiki: Items](https://thebazaar.wiki.gg/wiki/Items)
- 18 item tags (Apparel, Aquatic, Core, Dinosaur, Dragon, Food, Friend, Loot, Potion, Property, Ray, Reagent, Relic, Tech, Tool, Toy, Vehicle, Weapon). One enchantment per item, for example Heavy (Slow 2 items for 1 second), Turbo (Haste 2 items for 2 seconds), Shiny (+1 Multicast). Quality tiers Bronze, Silver, Gold, Diamond, raised by buying a duplicate. [Bazaar wiki: Items](https://thebazaar.wiki.gg/wiki/Items)
- Adjacency card text: Hourglass "Adjacent items have their cooldowns reduced by 4% » 8% » 12% » 16%." [BazaarDB: Hourglass](https://www.bazaardb.gg/card/1sk99jmuf77zkyxy9rt7trq8u/Hourglass). Butter "For each adjacent Tool or Food item, this gains +1 Multicast." [BazaarDB: Butter](https://www.bazaardb.gg/card/5vfx8cve075h2mh0hgme8m3m0/Butter). Marbles "When you use an adjacent item, Charge this 1 second(s)." [BazaarDB: Marbles](https://www.bazaardb.gg/card/9z39lwd0cnasqm1y9km2vcv8z/Marbles). Multitool hastes 1, 2 or 3 other random items by tier. [BazaarDB: Multitool](https://www.bazaardb.gg/card/1gg8udwt0q7u1vrfo28pfha7b/Multitool)
- Mak's Laboratory charges enchanted items on a 3 second cooldown that slows by 1 second with each use, a chain that limits itself. [PCGamesN](https://www.pcgamesn.com/the-bazaar/mak-new-hero-season-1)
- Position: some skills buff the leftmost or rightmost item, and "many items affect or are affected by their adjacent counterparts". The goal is 10 wins, and each PvP loss lowers Prestige, which works as an HP bar (search summary, the page returned 403). [Mobalytics beginner guide](https://mobalytics.gg/blog/the-bazaar/the-bazaar-beginner-guide/)
- "Charge" advances an item's cooldown at once. Items and board slots can be Chilled (search summary). [Mobalytics keywords](https://mobalytics.gg/the-bazaar/guides/keywords-and-terms)

### Inferences
- Both games make synergy a spatial fact the player sees while building: a lit star, a neighbour with the right tag, a size that eats slots. The fight cashes in what the board already shows.
- Backpack Battles accepts that the fight is too fast to read item by item. It answers with speed controls, pause and an after fight log, so the "why did I win" story is told after the fight, not during it.
- The Bazaar's internal cooldown and Mak's self slowing Laboratory show that once items can trigger each other at runtime, designers must add brakes, or chains loop and stop being readable.
- Interaction count stays local and countable. A Bazaar item can only touch the item on each side, so it has at most two neighbours. Each Backpack Battles star touches one item.

### Gaps
- Bazaar board size is often said to be 10 slots, but no source I could open states it.
- How The Bazaar's fight shows a trigger (cooldown sweep, card pulse, numbers) is not described in any source I could open.
- No design talk or blog from The Bazaar's developers or PlayWithFurcifer about adjacency or readability was found. The Automaton interview has no remarks on readability.
- One player tip (search summary of a Steam thread) mentions green "diamond" slots in Backpack Battles. I could not confirm what they do.

## 2. Warpack, Northgard Battlegrounds and Time Walker: Dark World: how their synergies work and are shown

### Takeaway
All three of the owner's references put synergy in a layer the player builds between fights. Warpack uses a 3x3 backpack per unit with item adjacency. Northgard Battlegrounds uses building placement and adjacency bonuses in a village. Time Walker uses the choice of which of 80+ shared skills each of 4 heroes carries. Public detail on exact rules and on how payoffs are shown is thin: Warpack is unreleased (10 Nov 2026), Northgard Battlegrounds opens early access on 27 Oct 2026, and Time Walker has only fan guides.

### Cited Findings

Warpack
- Steam app 5016130 by N1ghtmare Studios. Release date 10 Nov 2026, not out yet. A demo (app 5023320) is listed. [Steam: Warpack](https://store.steampowered.com/app/5016130/)
- Pitch, verbatim: "Build a fantasy war-band, pack each unit's backpack, and feed your champions to make them monsters." "An async-PvP auto-battler: every opponent is a real player's saved army." "Five factions, shaped items, commander skills." [Nodal](https://nodal.gg/game/warpack-5016130)
- Rules from the Steam page (summarised by the fetch tool, verbatim only in quotes): each unit has its own 3x3 backpack. Items have shapes, and placing them next to compatible items activates adjacency combos. Items can be crafted into stronger ones. Spells move between units: "hand a mage's soul gem to a footman and watch him cast". Units level up by consuming other units and absorbing their traits, and ascend at max level. You draft a commander, then draft commander skills after won battles, from economy engines to army wide doctrines. Runs take about 10 minutes. Ranked play uses Elo. [Steam: Warpack](https://store.steampowered.com/app/5016130/)
- Each faction has one hook: Highcrest clerics revive fallen units. Bone Legion necromancers raise more units mid fight. Bloodhorde has berserk rage, "ROAR" momentum and regeneration. Elderwood has crit moonblades, a thorned grove wall and a drake that grows over the run. Ironclaw runesmiths permanently strengthen units after each battle. [Steam: Warpack](https://store.steampowered.com/app/5016130/)
- The developer discloses that some visual assets and the music were made with generative AI tools. [Steam: Warpack](https://store.steampowered.com/app/5016130/)

Northgard Battlegrounds
- Developer Funday Games, publisher Shiro Unlimited, announced 16 Apr 2026 as a "city-building auto-battler". Each turn you place buildings and gather resources. At the end of each round warbands fight automatically. "Construct economic buildings near resources to boost their production." Military structures recruit warriors, and mythical sites unleash creatures. "every placement shapes your clan's strength in combat." "Combine blessings with your village layout and unlock powerful synergies." Async PvP. [Gematsu](https://www.gematsu.com/2026/04/northgard-battlegrounds-announced-for-pc). Same text in the [ICO Partners press kit](https://stash.icopartners.com/mediakit/NorthgardBattlegrounds)
- 19 Oct 2026 Next Fest release: "Use powerful adjacency bonuses to boost units' effectiveness in battle and maximize a town's productivity." "Build more than 50 unique structures and pick from over 100 Fate cards." Lore builds mythical monuments that summon legendary creatures. Clan of the Wolf joins Stag and Raven in the demo. Freeplay and Ranked Ladder modes. Early access on Tuesday 27 Oct 2026. [Gamespress](https://www.gamespress.com/Shiro-Unlimited-Storms-Steam-Next-Fest-With-Frostrail-Demo-and-Northga)
- July 2026 open beta: Raven and Stag clans with their own skills, over 50 structures, Fate cards, 3 Patrons who bless the player. [Rogueliker](https://rogueliker.com/northgard-battlegrounds-open-beta/). Beta dates 9 to 15 July during Steam Tacticon (search summary). [GameWatcher](https://www.gamewatcher.com/northgard-battlegrounds-first-open-beta-arrives-next-week-as-part-of-steam-tacticon)
- Preview (Spanish, translated): two phases, build then auto battle. "The village's organization, not reflexes, decides the outcome." [After Strategy](https://after-strategy.com/es/northgard-battlegrounds-beta-abierta-auto-battler-2026/)

Time Walker: Dark World
- Developer Heartfun, publishers Leyo Games and 2P Games. Full release 29 Nov 2023 after early access from 26 Sep 2022. Very Positive, 85% of 1,435 reviews. [Steam: Time Walker](https://store.steampowered.com/app/2093910/)
- Steam text, verbatim: "Lead four Time Walkers to torture the Dark Lord and his subordinates with more than 80 skills." "Match different skill builds without class limits." "Archer healer, assassin tank or wizard warrior?" "Choose four Heroes before battles, match them with different abilities to produce amazing chain reaction." "Merge three same basic hero items into an advanced one!" "Merge two Level III abilities into a moer [sic] powerful EX ability." The summary says players "adjust their formations, plan their resources, acquire loot, and upgrade skills". [Steam: Time Walker](https://store.steampowered.com/app/2093910/)
- Store listings disagree on skill count: Steam says 80+, the Nintendo listing says 40+ (search summary). [Steam](https://store.steampowered.com/app/2093910/); [Nintendo](https://nintendo.com/store/products/time-walker-dark-world-switch)
- Each class has a scaling hook that skills feed (fan guide): Assassin/Ninja scales off crits, Archer/Marksman off attack speed, Mage/Sorcerer off casts, Berserker/Champion off taking damage and physical damage, Boxer/Firefist off fire, Knight/Lord off casting, normal attacks and ice. Two level 3 skills of one element family merge into an EX skill: Inertial Strike + Frozen Strike → Frost Blast EX; Burning Fireball + Flame Burst → Flame Burst EX; Ice Elemental Thrust + Icy Multishot → Ice Scatter EX. Suggested early team: Fighter tank with 3 defensive skills, a healer, a main DPS and an off DPS. [gameplay.tips guide](https://gameplay.tips/guides/time-walker-dark-world-general-guide-strategy-build-unlocks-and-more.html)
- Fan lineups give each hero 3 skills, and the class skill fires off how those skills are used. Berserker: "Use Triple Slash until Flame Streaks is available so the Class Skill triggers often." Boxer "becomes strong once all 3 skills reach Level 3 and the Buffer's Speed Boost reaches Level 3." Lightninger: "Her extra Broken Armor stacks help her take down the Final Boss quickly." [Steamah (DerpSmashed guide)](https://steamah.com/?p=122080)
- Community view (search summary of forum posts): one priest, one taunt tank and two pure DPS heroes is enough on Hell difficulty, while another player said you get melted without 3 healers or supports. [Steam discussion](https://steamcommunity.com/app/2093910/discussions/0/4034725629421232620)

### Inferences
- In all three, synergy is decided on a prep screen: what sits next to what (Warpack, Northgard), and which skills sit on which hero (Time Walker). No public description presents synergy as units chaining moves on their own mid fight. Time Walker's "chain reaction" is the nearest wording. The guides show it comes from skill picks made before battle that feed a class trigger (crits, attack speed, cast count, fire), plus support skills (Speed Boost, Broken Armor stacks) that lift teammates.
- Warpack's faction hooks are each one plain verb (revive, raise, rage, grow, strengthen for good). That keeps each synergy one chunk.
- Northgard Battlegrounds uses one placement decision for two payoffs, economy and battle strength.
- Time Walker turns its late payoff (the EX skill) into an item you own, made at build time from two finished skills. The "combo" is something you hold, not an event you must spot in the fight.

### Gaps
- Exact Northgard Battlegrounds adjacency rules (which building boosts which, by how much, how it is shown) are in no source found. The brief's "Marketplace style adjacency" could not be confirmed. The demo is live and early access opens 27 Oct 2026, so playing it is the only way to see it.
- Warpack's adjacency rules, item list and fight presentation are not public beyond the store page. It is the only game of that name found, so check it is the one the owner means.
- No source defines Time Walker's "chain reaction" or shows how the fight displays it. No developer interview was found.

## 3. Teamfight Tactics and Super Auto Pets: trait thresholds, trait preview and UI, trigger chains and their readability

### Takeaway
TFT makes synergy a counted property of the build: a trait switches on at published unit counts (2/4/6 style breakpoints), and Riot tunes breakpoints so a player can splash a little or commit deep. SAP shows the risk of runtime chains: its trigger ordering (start of battle, hurt, faint, summon) is its most cited readability complaint even with only five pets, and the developer admitted it was unclear.

### Cited Findings

Teamfight Tactics
- GDC 2020 talk by lead designer David Abecassis, as reported: shuffling units by hand was needless stress, so TFT capped champions on the field and merged duplicates, summed up as "tough choices, not touch execution". Any two item components combine into a finished item, which gave very strong synergies, and the lesson was "always embrace extreme outcomes". Kept from auto chess: relaxed pace, low conflict play, a roguelike arc of building toward a strong roster, low stakes for losing. [Pocket Tactics](https://www.pockettactics.com/teamfight-tactics-design)
- Design pillars named by the TFT design lead in 2019: "mastery, playful competition, and discovery" (search snippet). [Surrender at 20](https://www.surrenderat20.net/2019/06/red-post-collection-riot-pls-dev-on-tft.html)
- Breakpoint tuning: Dark Star moved from 3/6/9 to 2/4/6/8. Battlecast got "bonuses at two, four, and six units so players can be rewarded for splashing a few in their composition or deciding to run them all." [Invenglobal](https://labs.invenglobal.com/articles/11260/teamfight-tactics-galaxies-mid-set-update-includes-14-new-champions-3-unique-traits)
- Riot's Galaxies retrospective. Good state: "investing deeply in major traits (6+ pieces) or spreading your comp more widely across different minor traits." Failures: "Cybernetic required an early 3 and then you were locked into 6 or you weren't playing it"; "Star Guardian had some rough patches where it was 6 or bust"; "Chrono may as well have been a 2 or 4-piece origin only." Also: "We like traits having specific ties that make you want to optimize them." Items "too closely tied to specific traits" were a culprit: "using Warmogs with Protectors caused a larger power spike than should be allowed to exist." [Riot: Galaxies learnings](https://teamfighttactics.leagueoflegends.com/en-us/news/dev/dev-teamfight-tactics-galaxies-learnings/)
- Set 1 learnings (search snippet, the page now redirects): "We want to keep random effects in positive and controlled spaces." [Riot Nexus](https://nexus.leagueoflegends.com/en-us/2019/10/dev-tft-set-1-learnings/)
- Beginner guide: two of a trait gives a bonus and four a bigger one (Challenger example). Early on, look for simple two unit trait bonuses. Levelling up raises the team size limit. [WeCoach](https://wecoach.gg/de/blog/article/the-ultimate-beginners-guide-to-teamfight-tactics)
- Trait tiers are visible ranks. A Prismatic trait shows "a sparkling diamond icon" next to it. [Fragster](https://www.fragster.com/prismatic-traits-in-tft-teamfight-tactics-beginner-explanation/). An augment rewards each bronze rank trait, so bronze is a named tier. [SeeMeta](https://seemeta.com/de/tft/set-18/items/BronzeForLifeII)
- The "Trait Tracker" augment pays 5 random emblems once you field 7 unique traits in one combat (search summary), so 7 active traits at once is treated as a stretch goal. [Tacter](https://www.tacter.com/tft/guides/how-to-play-augment-trait-tracker-in-tft-set-14-cyber-city-4737e004)

Super Auto Pets
- Released on Steam 24 Sep 2021 by Team Wood Games (search summary). [Wikipedia: Super Auto Pets](https://en.wikipedia.org/wiki/Super_Auto_Pets)
- Five team slots. The player's rightmost pet and the opponent's leftmost pet hit each other at the same time. A pet at 0 health is removed and the next one steps up. Shop tier rises on odd turns, tier 6 from turn 11. Pets max out at level 3, and levelling up offers a choice of two pets from the next tier. [SAP wiki: The Basics](https://superautopets.wiki.gg/wiki/The_Basics)
- Each trigger is one keyword on the pet. The glossary lists 14: Buy, Buy food, Buy tier 1 pet, Eat shop food, End turn, Faint, Friend bought, Friend faints, Friend summoned, Knockout, `Level-up`, Sell, Start of battle, Start of turn. Start of battle abilities resolve in attack order, highest first. "Effects that happen in battle are not permanent." [SAP wiki: Glossary](https://superautopets.wiki.gg/wiki/Glossary)
- Hurt fires when a pet takes damage, including damage that makes it faint (search summary). [SAP wiki: Hurt](https://superautopets.wiki.gg/wiki/Hurt_(Trigger))
- Readability complaints in one Steam thread: start of battle order looked "totally random"; "Start of battle effect ordering gets really wonky", especially with hurt or summon triggers; one player asked for "visual clarity" with a left to right order; one lost because a Rhino chained kills before buffs applied; "I'm assuming ALL "start of battle"s are triggered before the faint ability does, but this also isn't very clear." Developer reply (Lau): "the ordering depends on attack power, but it sounds like it isn't clear how it works, so thank you for the feedback!" [Steam discussion](https://steamcommunity.com/app/1714040/discussions/0/3086646248547568129)

### Inferences
- A TFT trait works because it is a number the player counts before the fight. The fight is not where you discover it. Breakpoints at 2/4/6 pay a little for a small investment and more for commitment, and Riot calls "6 or bust" traits a failure.
- SAP keeps chains readable mostly through tiny scope: 5 pets, one exchange at a time, one keyword per pet, effects that reset each fight. Even so, the order of simultaneous triggers is where players get lost. A 5 class squad that chains on its own has SAP's problem without SAP's one keyword label on each unit.

### Gaps
- No Riot source found on how the in game trait tracker is laid out or why it works. From general knowledge, not verified this pass: TFT lists active traits in a column left of the board, each showing the current count against the next breakpoint, coloured by tier, and hovering shows the bonus text and that trait's champions.
- No Team Wood Games design interview or talk found. Their only on record design remark here is the Steam reply.
- No source describes SAP's in fight trigger animations.

## 4. Darkest Dungeon rank positions and Into the Breach push synergies

### Takeaway
Darkest Dungeon prints position rules on every skill as two rows of dots (where you must stand, what you can hit), so team order is a build decision you read off the tooltip. Into the Breach draws every enemy attack before it lands, and its designers say a weapon's push side effect is often its most powerful part. In both, synergy is planned on a visible board, not discovered mid fight.

### Cited Findings

Darkest Dungeon
- Tooltip dots (original game, players explaining): "yellow dots are what position your character has to be in to use that ability"; "red dots are what enemy positions you can use that move on." Hovering an enemy shows "what that attack will actually do" (hit chance, damage range, crit). [Steam discussion](https://steamcommunity.com/app/262060/discussions/0/412447613581265645)
- AoE skills link their red dots with a line (search summary of another Steam thread, not opened). [Steam discussion](https://steamcommunity.com/app/262060/discussions/0/364042703858432050)
- Darkest Dungeon II examples (4 ranks per side): Grave Robber Lunge needs ranks 3 or 4, hits enemy ranks 1 to 3 and moves her two ranks forward. [Fextralife: Lunge](https://darkestdungeon2.wiki.fextralife.com/Lunge). Jester Razor's Wit works from ranks 2 to 4, hits 1 to 3 and moves him one rank forward. [Fextralife: Razor's Wit](https://darkestdungeon2.wiki.fextralife.com/Razor's+Wit). Echoing March works from ranks 3 or 4 and hits 2 to 4. [Fextralife: Echoing March](https://darkestdungeon2.wiki.fextralife.com/Echoing+March). Bounty Hunter Come Hither works from any rank and targets enemy ranks 3 or 4. [Fextralife: Come Hither](https://darkestdungeon2.wiki.fextralife.com/Come+Hither). Occultist Binding Shadows works from 2 to 4 and hits 1 to 3. [Fextralife: Binding Shadows](https://darkestdungeon2.wiki.fextralife.com/Binding+Shadows)
- Tyler Sigman: a hero at the back is safer but limited to ranged attacks. The Bounty Hunter is "a sniper mixed with a little bit of crowd control" who can pull an enemy to the front or push one to the rear (search summary). [GameSpot](https://gamespot.com/articles/darkest-dungeon-captures-the-hellish-life-of-an-adventurer/1100-6420150/)
- Sigman on the sequel: "the side-on positional skills is just part of the game's identity, so that's staying" (search summary). [TheGamer](https://www.thegamer.com/darkest-dungeon-2-3d-combat/)
- Red Hook chose side on rank combat over an early tile based sketch partly because "you really get to see them well" (search summary). [Game Developer: Road to the IGF, Red Hook](https://www.gamedeveloper.com/audio/road-to-the-igf-red-hook-studios-i-darkest-dungeon-i-)

Into the Breach
- 8x8 grid, three mechs. "The player is shown the direction of the Vek unit's intended attack." Players can "push the Vek off a line of attack." Telegraphing "helped to hasten the pace", and a limited turn counter keeps battles short. [Wikipedia: Into the Breach](https://en.wikipedia.org/wiki/Into_the_Breach)
- Subset Games, Road to the IGF interview: "This lead us to use of telegraphed enemy attacks as a core mechanic", so that "every death felt like your own fault." "We prefer games with clear rulesets, and were interested in pursuing something with less randomness than FTL." "it's very clear how or why you failed so that you can feel like you can improve." "The first artillery attack can damage enemies, but its side effect of pushing adjacent tiles is usually more impactful." The heart of the game is "Considering the many ways weapons can interact to create original solutions." [Game Developer](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-)
- GDC 2019 "'Into the Breach' Design Postmortem" by Matthew Davis covered cutting features, borrowing ideas and difficulty. Slides are behind a login. [80.lv](https://80.lv/articles/gdc-2019-an-inside-look-at-into-the-breach); [GDC Vault](https://gdcvault.com/play/1025772/-Into-the-Breach-Design)
- Analyses (search summaries, pages blocked): the game gives "perfect information at every point" and shows a move's result before you commit. [Prototypr](https://blog.prototypr.io/into-the-breachs-ux-makes-you-feel-smart-a9cb03210757). Enemy attacks are shown with an animated tooltip, with damage previews. [Into the Spine](https://intothespine.com/2019/05/06/into-the-breach-and-imperfection/)

### Inferences
- Both games make synergy a property of position that the player reads before acting. DD's dots are a compact grammar: two rows of four dots per skill give the whole position rule.
- ITB's push combos happen at runtime, but they read well because the player causes each one with a full preview, and the enemy's intent is already drawn. That is the opposite of an automatic chain the player only watches.
- Neither game lets the enemy mirror the player's combo engine. Enemies follow their own visible rules (telegraphs, enemy ranks).

### Gaps
- The ITB GDC 2019 talk content (slides, recording) was not reachable.
- No Red Hook source found on why dots were chosen or how readability was tested.

## 5. Hades boons and duo boons, how the game tells you about synergy, and Slay the Spire card synergy

### Takeaway
Hades turns cross god synergy into a card you pick (the duo boon). It only appears after you hold qualifying boons from both gods, and the requirements are listed in the Codex and, in Hades II, in a Boon Info view where a met requirement turns green. Slay the Spire found hidden enemy moves felt unfair and that showing exact intent numbers was "more engaging".

### Cited Findings

Hades
- Duo boons need boons from two gods. Hades II wiki: "Duo Boons are offered after acquiring a minimum of two compatible boons from both gods" (Funeral Pyre, Hestia and Hera). [Fextralife: Funeral Pyre](https://hades2.wiki.fextralife.com/Funeral+Pyre). Other examples: Soul Mate (Aphrodite and Hera) [Fextralife: Soul Mate](https://hades2.wiki.fextralife.com/Soul+Mate); Master Conductor (Hephaestus and Zeus) [Fextralife: Master Conductor](https://hades2.wiki.fextralife.com/Master+Conductor)
- Hades II has 37 duos across nine gods as of v1.0, with a 12% base chance to be offered once prerequisites are met, rising to 20% in some Warden encounters (search summary). [RogueRanker](https://rogueranker.com/hades-2-duo-boons/)
- Hades (original): duo boons need tier 1 boons from two different gods (Sea Storm needs specific Zeus and Poseidon boons). Legendary boons need a tier 1 and a tier 2 boon from one god and still depend on luck once you qualify. Rarity outlines: common white, rare blue, epic purple, heroic red, legendary orange (search summaries; Prima returned 403). [Prima Games](https://primagames.com/gaming/hades-guide-legendary-heroic-duo-boons); [PC Invasion](https://pcinvasion.com/?p=222231)
- The Codex holds the requirements: "Have you unlocked the Codex yet? This will show you the requirements for boons, Duos and Legendaries." [Steam discussion](https://steamcommunity.com/app/1145350/discussions/0/4697909730619656492). A Hades 1 player: "boons have prerequisites, listed in the codex" (search summary). [Quarter to Three](https://forum.quartertothree.com/t/hades-supergiant-games/139167?page=46)
- Hades II Boon Info, as a mod author describes the base game: a required boon you already own turns from grey to white, and a met requirement's heading turns from white to light green. But "In the vanilla game, finding information for boons availability during an ongoing run is very complicated", worse once boons are banned. The mod adds colour states (picked, sacrifice, unavailable, banned) and filter pages. [Thunderstore: Improved Boon Info UI](https://thunderstore.io/c/hades-ii/p/SMarBe/Improved_Boon_Info_UI/)

Slay the Spire
- The developers accepted very strong combos because in a single player game "there would be no opponent that would feel overwhelmed by these combos." [Wikipedia: Slay the Spire](https://en.wikipedia.org/wiki/Slay_the_Spire)
- Intents: early builds hid enemy moves, which clashed with permadeath. They tried a "Next Turn" system, then an "Intents" icon system. Playtesters found exact numbers "more engaging". [Wikipedia: Slay the Spire](https://en.wikipedia.org/wiki/Slay_the_Spire)
- After a won fight the player usually picks one of three random cards. Balance used "gameplay metric reporting from the game client" to find cards rarely picked or too strong. [Wikipedia: Slay the Spire](https://en.wikipedia.org/wiki/Slay_the_Spire)
- GDC 2019 talk: "'Slay the Spire': Metrics Driven Design and Balance" by Anthony Giovannetti. [GDC Vault](https://www.gdcvault.com/browse/gdc-19/play/1025731)

### Inferences
- Hades makes synergy a reward you choose (a named, coloured duo card), not a combo you must notice in a fight. The requirement check lives in menus, before the payoff.
- SAP and Hades II agree: when players cannot predict whether a synergy will fire, they complain, even when the rules are fixed.
- StS's intent lesson maps onto an auto battle: show the enemy's next action as a number rather than surprise the watcher.

### Gaps
- No Supergiant designer interview on how boons communicate synergy was found. The duo frame colour in Hades 1 and the Hades 1 duo count were not confirmed by any page I could open.
- No Mega Crit source on the design of specific card synergies was opened.

## 6. General principles: build time synergy versus runtime combos, what reads better for a watching player, and how the payoff moment is shown

### Takeaway
In the games that are mostly watched (auto battlers), the decision and the visible synergy sit at build time and the fight cashes it in. Runtime combos read well only when the player causes them with a preview (Into the Breach) or the game stages them one at a time (Balatro). Automatic chains between units are the documented failure case (SAP ordering complaints).

### Cited Findings
- TFT's stated goal: "tough choices, not touch execution." [Pocket Tactics](https://www.pockettactics.com/teamfight-tactics-design)
- Northgard Battlegrounds preview: "The village's organization, not reflexes, decides the outcome." [After Strategy](https://after-strategy.com/es/northgard-battlegrounds-beta-abierta-auto-battler-2026/)
- An auto battler template says skill moves from combat into preparation, and depth is "invisible until a player has 10 hours in it" (search summary, low authority source). [Summer Engine](https://summerengine.com/templates/strategy/auto-battler/auto-battler)
- An indie postmortem argues a five unit line is "small enough that a person can hold the whole board in their head at once", and "position is a resource. A front card takes hits first and shields the back." (search summary, small dev blog). [Orchard Deck](https://solodevstack.com/blog/orchard-deck-deterministic-card-battler-keywords)
- Balatro: a video essay quotes LocalThunk: "My personal belief is that the game is more fun when you set up your Rube Goldberg machine and watch it go before knowing whether or not the hand will win the round." It describes the payoff: "The numbers tick up, with escalating sound effects. Each card and joker steps forward in turn to add their points to the total." It says the missing score preview is deliberate. [Amara transcript of "Balatro's Cursed Design Problem"](https://amara.org/subtitles/qZaMrU6iDy4J/en/2/download/Balatros%20Cursed%20Design%20Problem.en.srt)
- Backpack Battles tells the payoff story after the fight with an action log (damage per weapon, when effects fired), plus speed up and pause. [PC Games, page 2](https://www.pcgames.de/Indie-Games-Thema-239276/Specials/Backpack-Battles-Builds-Early-Access-Inventar-getestet-Auto-Battler-1442869/2/)
- SAP players could not read the order of simultaneous triggers, and the developer agreed it was unclear. [Steam discussion](https://steamcommunity.com/app/1714040/discussions/0/3086646248547568129)
- Slay the Spire: exact numbers on enemy intents were "more engaging". [Wikipedia](https://en.wikipedia.org/wiki/Slay_the_Spire)
- Into the Breach telegraphs so it is "very clear how or why you failed". [Game Developer](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-)
- The Bazaar needed an internal cooldown to stop trigger loops. [Bazaar wiki: Items](https://thebazaar.wiki.gg/wiki/Items)
- Hades II marks a met requirement green before you pick. [Thunderstore](https://thunderstore.io/c/hades-ii/p/SMarBe/Improved_Boon_Info_UI/)

### Inferences
- For a player who mostly watches, build time synergy reads better, because the player already knows what should happen and watches for it. Combos the player did not choose read as noise. If the enemy runs the same combos, the noise comes from both sides.
- The shared payoff pattern: one event per synergy, shown at the moment it lands, with a number and a sound, in sequence rather than all at once (Balatro, ITB previews, StS intent numbers), plus a recap after the fight (Backpack Battles log).
- On the enemy mirroring the player's combos: the async PvP games (SAP, Backpack Battles, The Bazaar, Warpack, Northgard Battlegrounds) are symmetric on purpose, because the enemy is another player's saved build and the player knows that. PvE roguelikes (StS, ITB, Hades) give enemies their own telegraphed behaviour. A PvE enemy that runs the player's synergy engine takes away the feeling that the combo belongs to the player.
- Kindred Siege specific: the current snare → aimed shot → Opening → strike chain and the bond after 3 combos are runtime events and a hidden counter. Every reference here suggests moving the decision to prep (which pairs are linked, shown on the squad screen the way TFT shows a trait count or Backpack Battles lights a star) and showing one stamp when the linked payoff lands.

### Gaps
- No controlled study found comparing how well spectators read build time versus runtime synergy. The evidence is designer statements and player complaints.
- No developer source found that says outright "build time synergy reads better for watchers". That conclusion is inferred.

## 7. How many interacting rules a player can hold, and how games scale synergy complexity across a run

### Takeaway
Working memory research puts the central limit near 4 chunks (range about 3 to 5). The games keep the live board small (SAP 5 pets, ITB 3 mechs, DD 4 ranks, Time Walker 4 heroes) and let the number of owned synergies grow slowly over a run through tiers, unlocks and prerequisites. Magic's designers name "board complexity", how cards interact on the table, as the kind that blows up fastest.

### Cited Findings
- Cowan (2001), "The magical number 4 in short-term memory": evidence for a central capacity limit averaging about four chunks, with Miller's seven treated as a rough estimate. [Cambridge, Behavioral and Brain Sciences](https://www.cambridge.org/core/journals/behavioral-and-brain-sciences/article/magical-number-4-in-shortterm-memory-a-reconsideration-of-mental-storage-capacity/44023F1147D4A1D44BDC0AD226838496). Later work mostly supports 3 to 5 chunks, and Reynolds et al. (2022) estimate about five in adults (search summary). [Journal of Cognition](https://journalofcognition.org/articles/387)
- Celia Hodent (PhD psychology, former UX director at Epic): working memory is limited, and its subsystems restrict what players can process at once (video summary). [IxDF](https://ixdf.org/literature/article/design-for-working-memory-players-will-remember-what-matters)
- Mark Rosewater, New World Order: three kinds of complexity. Board complexity is about "how they interact with one another while they are on the battlefield." "it didn't take very many cards with this style of complexity to cause problems." "Just one card, for example, can change the design tree from a few choices to a double-digit number of choices." [Wizards of the Coast](https://magic.wizards.com/en/news/making-magic/new-world-order-2011-12-05). The policy began in 2008 against complexity creep, and pushed complexity up to higher rarities (search summary). [MTG Wiki](https://mtg.wiki/page/New_World_Order)
- Live board sizes: SAP 5 pets [SAP wiki](https://superautopets.wiki.gg/wiki/The_Basics); ITB 3 mechs on 8x8 [Wikipedia](https://en.wikipedia.org/wiki/Into_the_Breach); DD 4 ranks a side [Fextralife: Lunge](https://darkestdungeon2.wiki.fextralife.com/Lunge); Time Walker 4 heroes [Steam](https://store.steampowered.com/app/2093910/), 3 skills each in fan lineups [Steamah](https://steamah.com/?p=122080); Warpack one 3x3 backpack per unit [Steam](https://store.steampowered.com/app/5016130/); Bazaar items take 1, 2 or 3 slots [Bazaar wiki](https://thebazaar.wiki.gg/wiki/Items)
- Synergy counts: TFT treats 7 unique active traits as an augment goal (search summary) [Tacter](https://www.tacter.com/tft/guides/how-to-play-augment-trait-tracker-in-tft-set-14-cyber-city-4737e004); Hades II has 37 duos over 9 gods (search summary) [RogueRanker](https://rogueranker.com/hades-2-duo-boons/)
- Scaling across a run:
  - SAP shop tier rises every odd turn to tier 6 at turn 11, and levelling up offers next tier pets. [SAP wiki](https://superautopets.wiki.gg/wiki/The_Basics)
  - Backpack Battles: rare, powerful items more likely in later rounds. [Wikipedia](https://en.wikipedia.org/wiki/Backpack_Battles)
  - TFT: levelling up raises team size [WeCoach](https://wecoach.gg/de/blog/article/the-ultimate-beginners-guide-to-teamfight-tactics), and 2/4/6 breakpoints let a player start small [Invenglobal](https://labs.invenglobal.com/articles/11260/teamfight-tactics-galaxies-mid-set-update-includes-14-new-champions-3-unique-traits)
  - Hades: duo and legendary boons need earlier boons first, then roll at 12% base in Hades II. [RogueRanker](https://rogueranker.com/hades-2-duo-boons/)
  - Time Walker: EX skills need two finished level 3 skills. [Steam](https://store.steampowered.com/app/2093910/)
  - Warpack: units ascend only at max level after eating other units. [Steam](https://store.steampowered.com/app/5016130/)
  - The Bazaar: items climb Bronze, Silver, Gold, Diamond. [Bazaar wiki](https://thebazaar.wiki.gg/wiki/Items)
  - Northgard Battlegrounds: a separate resource (Lore) builds monuments that summon legendary creatures. [Gamespress](https://www.gamespress.com/Shiro-Unlimited-Storms-Steam-Next-Fest-With-Frostrail-Demo-and-Northga)

### Inferences
- If a watcher can hold about 4 things, a fight should ask them to track about 4 live synergies at most, ideally fewer, each one chunk (one name, one colour, one stamp). A 5 class squad with three link chains, pair bond counters and an enemy doing the same goes well past that.
- The games grow complexity by adding owned, labelled synergies one at a time (a tier, a boon, an item), while the board stays fixed or grows by one unit at a time.
- Rosewater's point that one card can turn a few choices into double digits fits automatic chains: each new chain link multiplies what the watcher must follow.

### Gaps
- No game specific study found on how many synergies players actually track during a fight. The TFT figure of 7 comes from an augment's condition, not from player data.
