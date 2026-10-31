# Systems and production behind auto battlers

Notes for Kindred Siege. Gathered 12 Oct 2026. Labels used below: [snippet] means the claim came from a search result summary and the page itself was not opened in full; [community] means a player, fan site or fan tool, not the developer; [opinion] marks a developer or writer opinion; [recruiting] means a job ad, which says what a studio wants, not how its systems work.

## 1. Combat architecture: loop, tick rate, targeting, mana, status effects, triggers

### Takeaway
Every shipped example runs combat as a clocked simulation where things fire on timers (attack speed, item cooldowns, status ticks every 1 or 2 seconds) plus a short list of named trigger words, and every one adds a forced fight end (sandstorm at 30 seconds, fatigue at 17 seconds). No studio publishes its tick rate. The hard parts that show up in sources are trigger ordering, stacking rules and determinism, not movement or targeting.

### Cited Findings
**Teamfight Tactics (Riot)**
- Current wiki text gives mana per basic attack by unit class: 10 for Assassins, Marksmen and Fighters, 7 for Casters, 5 for Tanks. Only Tanks gain mana from damage taken: 1% of pre mitigation plus 3% of post mitigation damage, capped at 42.5 mana. Mana is granted when the attack launches, so a miss or parry still counts. Champions are mana locked briefly after casting (some longer, e.g. Brand and Karma 1.5 seconds), and overflow mana carries over up to one cast. The page does not say which set these values belong to. ([LoL Wiki, TFT:Mana, fetched Oct 2026](https://wiki.leagueoflegends.com/en-us/TFT:Mana))
- A search summary of the same wiki entry gave the older flat rule of 10 mana per attack for every unit, so the class split is a later change. [snippet] ([LoL Wiki, TFT:Mana](https://wiki.leagueoflegends.com/en-us/TFT:Mana))
- Trait effects are routinely written as combat timers: Elderwood grows every 2 seconds up to 5 stacks, Mascot heals every 2 seconds, Shurima ascends after 8 seconds, Bilgewater cannonball lands after 1.5 seconds, Dragonlord dragon strikes after 8 seconds, Rebel shield lasts 10 seconds. ([Riot, TFT 6 year bash, 26 May 2025](https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/tfts-6-year-bash/))
- TFT ran on the shared League of Legends engine until Set 18 (Enchanted Wilds, 26 Aug 2026), when Wikipedia says it moved to Unreal Engine. Not checked against a Riot post. ([Wikipedia, Teamfight Tactics](https://en.wikipedia.org/wiki/Teamfight_Tactics)) Riot TFT engine job ads ask for C++ and Unreal experience. [recruiting] ([Riot job listing via The Muse](https://www.themuse.com/jobs/riotgames/senior-software-engineer-core-tech-teamfight-tactics))
- League's game server was made deterministic so a recording of inputs replays to the exact same state. The work began to get fast, repeatable tests driven by recorded inputs (a tool called Delta Checker) and then became Chronobreak, which restores a game for esports officials. Playback is not throttled to real time: restoring to minute 40 "often takes less than 3 minutes". Timeline: team formed April 2016, minimum viable product December 2016, version 2.0 with GUI tools and automated tests May 2017. ([Riot Tech Blog, Determinism in League of Legends: Introduction, Rick Hoskinson, 20 Jun 2017](https://www.riotgames.com/en/news/determinism-league-legends-introduction))

**The Bazaar (Tempo)**
- Items fire when their cooldown fills. Charge advances a cooldown instantly; Haste makes an item charge twice as fast; Slow halves the rate; Freeze stops charging and activation. [snippet] ([Mobalytics keyword guide](https://mobalytics.gg/the-bazaar/guides/keywords-and-terms))
- Every fight has a forced end: the sandstorm normally starts at 30 seconds; the Mak item Bottled Tornado moves it to 7 seconds. [snippet] ([PCGamesN, Mak hero](https://www.pcgamesn.com/the-bazaar/mak-new-hero-season-1))
- A community guide claims a hidden internal cooldown caps any item at 4 to 5 triggers per second. [community, unverified] ([Mobalytics build guide](https://mobalytics.gg/the-bazaar/builds/fiery-pufferfish-rarran)) The wiki says internal cooldown mechanics are "still unclear". ([The Bazaar wiki, Items](https://thebazaar.wiki.gg/wiki/Items))
- The wiki lists 5 item trigger types: Cooldown, On condition, Start of Combat, Start of Day, Each Hour. Enchantments add timed statuses, e.g. Heavy applies Slow for 1 second, Icy applies Freeze for 1 second, Turbo applies Haste for 2 seconds. ([The Bazaar wiki, Items](https://thebazaar.wiki.gg/wiki/Items))
- Backend: in 2021 the project moved from "a basic server side system built for turn based play" to two backends, a real time simulation server built with .NET and a Scala server for accounts. The client moved from an Unreal prototype to Unity in 2018. [contractor's own marketing] ([N-iX Games case study](https://gamestudio.n-ix.com/case-study/the-bazaar-game-development-story/))
- A fan tracker says it captures each fight as a tick by tick replay, which implies discrete simulation ticks; no tick rate is published. [snippet, fan tool] ([BazaarDB tracker](https://sin.bazaardb.gg/tracker))

**Backpack Battles (PlayWithFurcifer)**
- Items activate on cooldowns; accuracy decides hit or miss; stamina gates items that have a stamina cost; crits deal double damage; Stun pauses all cooldowns for its duration. ([Backpack Battles wiki, Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics))
- Status effects are defined per stack: Empower +1 weapon damage; Heat makes all items activate 2% faster; Cold 2% slower; Luck +5% accuracy; Blind minus 5% accuracy; Regeneration 1 health every 2 seconds; Poison 1 damage every 2 seconds; Spikes 1 damage per melee hit taken (capped at 100% of incoming damage); Vampirism heals 1 per melee hit (capped at 100% of damage); Block absorbs 1 damage. Other named effects: Reflect, Resist, Nullify, Invulnerability, Reincarnate. ([Backpack Battles wiki, Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics))
- Fatigue ("Nightfall") starts at 17 seconds of battle, hits both players every second, starts at 1 damage and grows by `DMG(n) = DMG(n-1) + floor(DMG(n-1)/10) + 1`, switching to `/5` after 60 seconds of total match time. Example given: 29 damage at second 34, 32 at second 35. ([Backpack Battles wiki, Fatigue](https://backpackbattles.wiki.gg/wiki/Fatigue))
- Adjacency: some items act on neighbours, marked with Star or Diamond symbols; an item usually fills only one Star and one Diamond on another item. ([Backpack Battles wiki, Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics))
- Built in Godot. [snippet] ([Wikipedia, Backpack Battles](https://en.wikipedia.org/wiki/Backpack_Battles); [Game World Observer](https://gameworldobserver.com/?p=23401))

**Super Auto Pets (Team Wood Games)**
- Turn based, not timed. Your rightmost pet attacks the opponent's leftmost pet. ([SAP wiki, The Basics](https://superautopets.wiki.gg/wiki/The_Basics))
- Abilities trigger in attack order, highest attack first; the wiki gives no tie rule. Nothing that happens in battle is permanent. ([SAP wiki, Glossary](https://superautopets.wiki.gg/wiki/Glossary))
- A developer confirmed attack based ordering in a Steam thread; players report that ties and mid battle ordering are hard to predict. [snippet, community] ([Steam discussion](https://steamcommunity.com/app/1714040/discussions/0/3198118348349750330))
- Built in Unity. ([Wikipedia, Super Auto Pets](https://en.wikipedia.org/wiki/Super_Auto_Pets))

**Pokémon Auto Chess (open source, real time)**
- The sim advances with `update(dt)` where dt is in milliseconds; each unit gets `pkm.update(dt, this.board, player)`; units sit on a `Board` grid and have separate idle, moving and attacking state files; timed statuses use millisecond durations (e.g. curse effects 8000 or 30000 ms, rune protect 5000 ms); there is no maximum fight duration inside `simulation.ts`; a fight ends when one team map is empty, both empty is a draw. ([simulation.ts](https://raw.githubusercontent.com/keldaanCommunity/pokemonAutoChess/master/app/core/simulation.ts); [app/core folder](https://github.com/keldaanCommunity/pokemonAutoChess/tree/master/app/core))

### Inferences
- The minimal combat kit common to all examples: a clock, per unit or per item timers, a status table with per stack rules and tick intervals, a short list of named triggers, one explicit ordering rule, and one forced fight end. New items and combos plug into these, so they are worth locking before content grows.
- Ordering is where shipped games are vague (SAP ties, Bazaar internal cooldown). A deterministic sim has to pin it down: written tie breaks, a per trigger or per item cap to stop runaway loops, and tests for both.
- A forced fight end (Bazaar 30 seconds, Backpack Battles 17 seconds) appears in every timed example. It stops stall builds and caps how long a bot balance run can take.
- SAP shows a turn based sim can carry a hit game; TFT, The Bazaar and Backpack Battles are time based. Riot's class based mana split shows that core tuning levers live as per class data, not code.
- Riot needed a dedicated team and about 8 months to retrofit determinism into League. Kindred Siege already has it, which removes the costliest item on this list.

### Gaps
- No studio published a tick rate for TFT, The Bazaar or Backpack Battles.
- No primary source found for TFT targeting rules.
- The Bazaar internal trigger cap is a community claim only.
- No developer talk found on how Backpack Battles implements triggers in Godot.

## 2. Content data: how units, items, traits and synergies are defined and evaluated

### Takeaway
Content is a data record (id, stats, tags, tier, size, trigger, effect values) and the code supplies a fixed library of trigger and effect handlers. Synergies are counted from tags on the board against breakpoint thresholds. Public sources show the shape of this data (Riot API fields, fan extracts, wiki taxonomies, open source code) but no studio has published its pipeline or editor.

### Cited Findings
- Riot's TFT match data stores, per trait, `name`, `num_units`, `tier_current`, `tier_total`, and per unit a name and `tier` (star level). That is a breakpoint model: count units, compare to thresholds, store the tier reached. ([Riot DevRel, TFT match history, 26 Sep 2019](https://www.riotgames.com/en/DevRel/teamfight-tactics-match-history))
- Fan tools extract TFT data from game files through CommunityDragon: champions (stats, traits, abilities, costs), traits (breakpoint thresholds, scaling values, membership), items (recipe tree, stat effects), augments and shop odds. Riot no longer ships a Data Dragon pack for TFT. [community] ([tft oracle README](https://cdn.jsdelivr.net/npm/tft-oracle@0.1.0/README.md); [tft assets repo](https://github.com/ngocleek/tft-assets))
- Riot TFT gameplay engineers build new functionality for champions, traits and set mechanics, plus tools so designers and artists can work without code changes. [recruiting, snippet] ([Riot job listing via The Muse](https://www.themuse.com/jobs/riotgames/software-engineer-gameplayui-teamfight-tactics))
- TFT items: champions and the carousel give components, and any two components combine into a final item. Lead designer David Abecassis said this simplification also produced very powerful synergies, and the lesson was to embrace extreme outcomes. [opinion] ([Pocket Tactics, GDC 2020 recap](https://www.pockettactics.com/teamfight-tactics-design))
- TFT Set 16 (3 Dec 2025) unlock conditions are rules over trait and item state: 5 cost Galio unlocks at 12 star levels of Demacia units; Graves needs a Bilgewater unit holding 2 items; an unlocked unit is guaranteed in the next shop; unlocks last one game. [snippet] ([esports.gg](https://esports.gg/news/teamfight-tactics/tft-set-16-lore-and-legends-unlockable-champions/); [Mobalytics](https://mobalytics.gg/tft/guides/set-16-mechanic-unlockables))
- The Bazaar wiki taxonomy: 18 item types (Apparel, Aquatic, Core, Dinosaur, Dragon, Food, Friend, Loot, Potion, Property, Ray, Reagent, Relic, Tech, Tool, Toy, Vehicle, Weapon); 3 sizes (Small 1 slot, Medium 2, Large 3); 4 tiers (Bronze, Silver, Gold, Diamond; Diamond cannot be upgraded); 5 trigger types; 11 enchantments (Deadly, Fiery, Heavy, Icy, Radiant, Restorative, Shielded, Shiny, Toxic, Turbo, Obsidian). ([The Bazaar wiki, Items](https://thebazaar.wiki.gg/wiki/Items))
- One Bazaar card carries a value per tier: Hourglass reduces adjacent item cooldowns by 4%, 8%, 12%, 16% across its tiers. [snippet] ([BazaarDB, Hourglass](https://www.bazaardb.gg/card/1sk99jmuf77zkyxy9rt7trq8u/Hourglass)) Board sockets can also be data: the Dragons hero's Notes sockets give a passive 10% cooldown reduction to matching item types. [snippet] ([Mobalytics keyword guide](https://mobalytics.gg/the-bazaar/guides/keywords-and-terms))
- SAP content is grouped in packs (Turtle, Puppy, Star, Golden, Unicorn, Danger, plus a weekly pack), and players can build Custom Packs, i.e. a content set is a selectable list. ([Wikipedia, Super Auto Pets](https://en.wikipedia.org/wiki/Super_Auto_Pets)) SAP's glossary names 14 triggers: Buy, Buy food, Buy tier 1 pet, Eat shop food, End turn, Faint, Friend bought, Friend faints, Friend summoned, Knockout, Level up, Sell, Start of battle, Start of turn. ([SAP wiki, Glossary](https://superautopets.wiki.gg/wiki/Glossary))
- Pokémon Auto Chess keeps precomputed data in `app/models/precomputed` and the README says to run `npm run precompute` after changing pokemon or synergy data. ([repo README](https://github.com/keldaanCommunity/pokemonAutoChess)) At fight start the sim calls `applySynergyEffects` (which calls `applyEffect` per active synergy), `applyItemEffect` per held item, then `applyPostEffects`; recurring hooks such as `OnSimulationStartEffect`, `OnFieldDeathEffect` and `MonsterKillEffect` live in an `effectsSet`. ([simulation.ts](https://raw.githubusercontent.com/keldaanCommunity/pokemonAutoChess/master/app/core/simulation.ts)) Bots call `player.updateSynergies()` after building a board. ([bot.ts](https://raw.githubusercontent.com/keldaanCommunity/pokemonAutoChess/master/app/core/bot.ts))
- The `sapai` Python engine takes pet ability data from a separate community data project (`super-auto-pets-db`). [snippet] ([sapai](https://github.com/manny405/sapai))
- Riot's Game Analysis Team stores test boards as "board strings", pre designed late game boards; it made 82 for the Runeterra Reforged set. ([Riot, Talking Tactics: Game Analysis Team, 21 Jul 2023](https://teamfighttactics.leagueoflegends.com/en-gb/news/dev/talking-tactics-game-analysis-team-gat/))
- A community overlay (Bazaar Buddy) keeps its entity list by hand from public sites and reships after each game update, a sign the data churns every patch. [community, snippet] ([Reddit post via mirror](https://redlib.hbubli.cc/r/TheBazaar/duplicates/1kdamo2))

### Inferences
- Common pattern: item or unit = id + tags + tier or size + trigger + list of effect calls with numbers; synergy = count of a tag on the board compared with thresholds; the engine never special cases one card. Adding items then becomes a data job, which suits AI assisted authoring.
- A tier ladder on each card (Bazaar Hourglass) gives four balance points per item for the price of one design.
- A validation build step (Pokémon Auto Chess precompute) is cheap insurance: no item may reference an unknown tag, trigger or effect.
- Riot's "board string" fits Kindred Siege's bots directly: one line per full board, kept as a reference library per tribe and class.

### Gaps
- No studio (Riot, Tempo, PlayWithFurcifer, Team Wood) has published its item definition format or editor.
- The TFT trait rules page could not be fetched, so the unique champion counting rule is not confirmed from a source here.
- Nothing found on how The Bazaar stores card data internally.

## 3. Opponents: bots, ghosts, scripted opponents, difficulty scaling

### Takeaway
Three models exist: live players with PvE creep rounds (TFT), asynchronous ghosts of other players' saved boards matched by round and rank (SAP, Backpack Battles, The Bazaar), and scripted progressions (Pokémon Auto Chess bots, Bazaar monsters, TFT Tocker's Trials bosses). Real decision making bots are rare; Dota Underlords is the clear example with difficulty tiers. Single player difficulty in public sources is authored boards or simple curves and budgets.

### Cited Findings
- TFT: each round is against another player or computer enemies that drop loot; stage one has three computer rounds, and the last round of each later stage is against computer enemies. ([Wikipedia, Teamfight Tactics](https://en.wikipedia.org/wiki/Teamfight_Tactics))
- TFT's prototype had eight players fighting cloned armies; late in development it switched to four pairs of real players. Abecassis's lesson: don't fear late pivots when playtest data supports them. [opinion] ([Pocket Tactics, GDC 2020 recap](https://www.pockettactics.com/teamfight-tactics-design))
- Tocker's Trials (27 Aug to 24 Sep 2024): single player PvE, 30 rounds, six bosses, a three life counter replaces HP, normal and Chaos difficulty, full current set roster and augments. [snippet] ([Riot](https://teamfighttactics.leagueoflegends.com/en-gb/news/game-updates/new-tft-workshop-mode-tockers-trials); [Pocket Tactics](https://www.pockettactics.com/tft/tockers-trials))
- SAP Arena is asynchronous: the game picks an opponent from a database of teams other players built on the same turn; AI generated teams fill in when no players are available; goal is 10 wins. ([Wikipedia](https://en.wikipedia.org/wiki/Super_Auto_Pets); [SAP wiki, The Basics](https://superautopets.wiki.gg/wiki/The_Basics)) Matching reportedly uses a similar record of battles, wins and lives. [community] ([TV Tropes](https://www.tvtropes.org/pmwiki/pmwiki.php/VideoGame/SuperAutoPets)) Versus mode starts players at a score of 1500. ([Wikipedia](https://en.wikipedia.org/wiki/Super_Auto_Pets))
- Backpack Battles: you face the builds of other players; Steam tags it asynchronous multiplayer; ranked has 8 ranks tracked per character. ([Steam page](https://store.steampowered.com/app/2427700/Backpack_Battles/); [Backpack Battles wiki, Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)) Players report opponents are recent builds from the same game version, similar rank, same round and similar wins; whether bots fill gaps is unknown. [community, unverified] ([Steam discussion](https://steamcommunity.com/app/2427700/discussions/0/4353365620737430279); [Steam discussion](https://steamcommunity.com/app/2427700/discussions/0/4203616558110689260))
- The Bazaar: each in game day has one PvE fight and one fight against another player's ghost; the run aims for 10 wins; PvP losses cost Prestige, which acts as HP; ghosts can be from players hours earlier. [snippet] ([Mobalytics beginner guide](https://mobalytics.gg/blog/the-bazaar/the-bazaar-beginner-guide/); [Mobalytics review](https://mobalytics.gg/gamebase/reviews/the-bazaar-review)) Patch 13.0 (1 Apr 2026) lets players right click a monster to see its loadout before fighting. [snippet] ([Mobalytics patch tracker](https://mobalytics.gg/the-bazaar/guides/patch-notes))
- Pokémon Auto Chess bots are scripted progressions, not agents: a `BotV2` record holds `steps`, each with `roundsRequired` and a `board` of entries (`name`, `x`, `y`, optional `items`); the bot advances a step when enough rounds pass and does not react to the opponent; difficulty comes only from the authored steps. ([bot.ts](https://raw.githubusercontent.com/keldaanCommunity/pokemonAutoChess/master/app/core/bot.ts)) Bots are seeded from `db-commands/botv2.json` into MongoDB. ([repo README](https://github.com/keldaanCommunity/pokemonAutoChess))
- Dota Underlords offered offline bots from easy to hardcore, with pause and a press to advance option; players found the top tier hard. [snippet, community] ([Android Headlines](https://www.androidheadlines.com/2019/06/valve-games-mobile-games-strategy-dota-dotaunderlords.html); [Qt3 forum](https://forum.quartertothree.com/t/dota-underlords-valves-auto-chess/142504?page=3))
- Indie single player scaling patterns (anecdotal devlogs, not market leaders): fight difficulty quadratic in floor with elites about 2.25x a normal fight ([Gloople demo](https://mismatched-socks.itch.io/untitled-gloople-demo)); enemy damage exponent cut from 1.25 to 1.15 to tame late spikes ([Don't Die, Collect Loot](https://danmarchand.itch.io/dont-die-collect-loot/devlog/364982/minor-balance-change)); a capped enemy value pool as a budget ([gamedev.net devlog](https://gamedev.net/blogs/entry/2263243-dev-log-4)); time based scaling ([TOYROGUE](https://extar.itch.io/toyrogue/devlog/232912/v104)). Players complained of runaway one sided fights when enemies scaled every turn. [community] ([Goblins Can Conquer devlog](https://starea.itch.io/goblins-can-conquer/devlog/1150976/goblins-can-conquer-v03-update))

### Inferences
- For a single player game, authored boards per stage (Pokémon Auto Chess steps, Bazaar monsters with a visible loadout) are the cheapest opponent system that works, and they replay perfectly in a deterministic sim.
- A ghost is just a saved board plus round number. Kindred Siege's save system could store the player's own past boards and serve them back later as offline ghosts, without a server.
- SAP needs AI teams as a fallback even with a large player base, which suggests online ghosts are a scale problem, not a day one need.
- For difficulty: a power budget per stage plus authored elites, and a gentle curve (the devlogs show steep exponents produce unwinnable runs).

### Gaps
- No official matchmaking rules for The Bazaar or Backpack Battles; nothing on how SAP builds its AI fallback teams.
- No commercial auto battler has published a difficulty formula.
- No source found on whether TFT's PvE creep rounds or Tocker's bosses are fixed boards or generated.

## 4. Balance tooling and what developers say about balancing

### Takeaway
Riot runs a dedicated analysis team that sims ideal boards against each other daily, reads live data (round win rate, LP change), and playtests, weighing data, design intent and player perception equally. Small teams lean on very frequent patches and community stats. Sims miss pacing, and bot policy is a known blind spot.

### Cited Findings
- Riot's Game Analysis Team (GAT) is made of former analysts, coaches and pros, all Masters or higher. Sims pit ideal or near ideal comps against each other using board strings (82 for Runeterra Reforged); boards are adjusted to isolate one augment, unit or item; "theoretical strength frameworks" set expected orderings (e.g. 3 cost three star capped boards should beat 4 cost two star capped boards); sims ran daily during fine tuning with biweekly reports to designers; data used includes LP delta when a unit or trait is active, round win rate and daily meta snapshots; specific augment pairs are blacklisted (Hedge Fund followed by Rich Get Richer+); sims do not capture game pace, so playtests cover it; the team had been playtesting the next set for nearly three months. GAT works from very early design validation (before a set can be playtested) through live patches. ([Riot, Talking Tactics: GAT, 21 Jul 2023](https://teamfighttactics.leagueoflegends.com/en-gb/news/dev/talking-tactics-game-analysis-team-gat/))
- Live balance runs on roughly a two week cycle. Mortdog: problems are found through data, designer intent and player perception, with equal weight. Mortdog and designer Kent Wu ran live balance alone from partway through Set 5. Wu slices data by unit cost, star level, items, augments and traits against round and lobby win rates. Mortdog's view is that every piece of content is in the game at all times, so all of it must be well tuned. [opinion for the last point] The article shows a photo captioned as the simulator used for patch 11.24, with no detail. ([Upcomer, The Three Innovators, 8 Dec 2021](https://www.upcomer.com/the-three-innovators-how-the-tft-live-balance-team-built-patch-11-24/))
- Riot's TFT team ships across all platforms every two weeks. [recruiting, snippet] ([Riot job listing via Hitmarker](https://hitmarker.net/jobs/riot-games-senior-software-engineer-league-teamfight-tactics-513702))
- Riot opened TFT match data to third parties in 2019 and asked tools not to reduce gameplay diversity, not to make scouting too easy, and not to play for the player (no deterministic "should pick" advice or exact roll odds). ([Riot DevRel, 26 Sep 2019](https://www.riotgames.com/en/DevRel/teamfight-tactics-match-history))
- Public stats sites report average placement and pick rate; e.g. MetaTFT numbers quoted for patch 12.13 had Tempest Mage at 3.46 average placement with a 0.65 pick rate. [snippet] ([Upcomer, 12.13 tier list](https://cms.upcomer.com/?p=130252))
- Human cost: Mortdog stepped back from patch rundowns, social media and streaming after patch 14.3 (2024), citing burnout and being the target of player frustration. [snippet] ([esports.gg, patch 14.4](https://cache.esports.gg/news/teamfight-tactics/tft-patch-14-4-notes-release-date-and-more))
- Backpack Battles shipped one content patch per week from the demo launch (June 2023), 38 patches plus hotfixes by April 2024; the developers called that pace extremely taxing for a two developer team. ([GameDiscoverCo, 24 Apr 2024](https://newsletter.gamediscover.co/p/how-backpack-battles-sold-650k-copies)) Their 0.2.0 devlog planned updates every Sunday. ([itch.io devlog, Patch 0.2.0](https://playwithfurcifer.itch.io/backpack-battles/devlog/544340/patch-020)) Later they said they would slow balance updates and focus on item diversity. [snippet] ([Steam discussion](https://steamcommunity.com/app/2427700/discussions/0/4363502241742993489))
- The Bazaar is meant to get balance changes often by design. [snippet] ([Mobalytics patch tracker](https://mobalytics.gg/the-bazaar/guides/patch-notes))
- Bob's Buddy (Hearthstone Deck Tracker, Battlegrounds) simulates upwards of 10,000 combats per round and shows win, tie, loss and lethal odds. Calibration against tens of thousands of real rounds from HSReplay landed close to the Y = X line. It switches off when hidden secrets are in play. ([HSReplay, Introducing Bob's Buddy, 24 Apr 2020](https://articles.hsreplay.net/2020/04/24/introducing-bobs-buddy/)) It hides results when the machine could not finish enough sims. ([HearthSim help](https://help.hearthsim.net/en/articles/3948400-why-did-bob-s-buddy-tell-me-its-results-weren-t-accurate-enough-to-show))
- Research: Metagame Autobalancing lets designers specify a target graph of which strategies should beat which, then tunes parameters by simulation ([arXiv 2006.04419, 2020](https://arxiv.org/abs/2006.04419v1)); RuleSmith pairs LLM self play with Bayesian optimisation over rules ([arXiv 2602.06232, Feb 2026](https://arxiv.org/abs/2602.06232v1)); Cardonomicon uses simulated agents of varying skill to expose card game balance flaws ([arXiv 1908.01423, 2019](https://arxiv.org/abs/1908.01423v1)).
- Open SAP engines exist purely for AI and balance experiments: `sapai` (Python, unit tested), Gym wrappers, PPO agents, and Super ML Pets, which tests trained agents against the real game through screen reading. [snippet] ([sapai](https://github.com/manny405/sapai); [CS181SAPAI](https://github.com/HangYu04/CS181SAPAI); [super ml pets](https://github.com/andreped/super-ml-pets))
- A fan spreadsheet study of TFT Set 9.5 found units very close in balance on paper by base stats, cost and class. [community] ([Set 9.5 deconstruction](https://stephendewitt-tft.carrd.co/))

### Inferences
- Kindred Siege's balance bots are the equivalent of GAT's sims. The cheap upgrades GAT suggests: a library of reference boards, a matrix run (every board against every board over many seeds), and a written expected ordering so violations flag themselves.
- GAT pairs sims with playtests because ideal comp sims miss pacing; Cardonomicon varies agent skill. A bot that always plays well will hide traps that hurt real players.
- Blacklisting a specific combo is a legitimate lever at Riot, not a failure.
- Bob's Buddy shows 10,000 sims per fight is cheap enough to run live; a deterministic sim can do the same for odds in UI or for overnight balance runs.
- Weekly patching broke a two person team's comfort and Riot's lead designer burned out on public balance talk; patch cadence is a production cost to plan, not a default.

### Gaps
- Riot's internal simulator design is not public beyond the GAT description and a photo caption.
- No internal telemetry numbers found from Tempo or PlayWithFurcifer.
- No Super Auto Pets developer statements on balancing found.

## 5. Replays, spectating and combat logs

### Takeaway
Full in game replays are rare in auto battlers; post fight logs and damage recaps are common, and third party tools fill the gap. Riot's League work shows what seed plus inputs replay buys: fast repeatable tests, debugging and recovery.

### Cited Findings
- League's deterministic server records inputs (Server Network Recordings) and replays them faster than real time; it started as a testing tool and became Chronobreak. ([Riot Tech Blog, 20 Jun 2017](https://www.riotgames.com/en/news/determinism-league-legends-introduction))
- TFT combat recap (patch 12.6, March 2022): the S key cycles damage dealt, damage taken and healing; Tab toggles the recap on PC. [snippet] ([LoL Wiki, V12.6](https://wiki.leagueoflegends.com/en-us/V12.6_(Teamfight_Tactics)))
- Backpack Battles shows a full log of all actions after each fight with stats (which weapons dealt how much damage, when stamina ran out, when effects triggered), and has a multi stage fast forward. [snippet] ([PC Games review, Early Access](https://www.pcgames.de/Indie-Games-Thema-239276/Specials/Backpack-Battles-Builds-Early-Access-Inventar-getestet-Auto-Battler-1442869/2/)) Patch 0.2.0 added a button to show the backpacks after each battle. ([itch.io devlog](https://playwithfurcifer.itch.io/backpack-battles/devlog/544340/patch-020))
- The Bazaar: no official replay feature found; a fan tracker records each fight tick by tick, including monster and PvP opponents. [snippet, fan tool] ([BazaarDB tracker](https://sin.bazaardb.gg/tracker))
- Bob's Buddy works in Hearthstone's spectator mode. ([HSReplay](https://articles.hsreplay.net/2020/04/24/introducing-bobs-buddy/))
- Pokémon Auto Chess refreshes DPS meters every tick from each unit's damage, heal and shield counters. ([simulation.ts](https://raw.githubusercontent.com/keldaanCommunity/pokemonAutoChess/master/app/core/simulation.ts)) Its random helpers call `Math.random()` with no seed, so a fight cannot be replayed from a seed. ([random.ts](https://raw.githubusercontent.com/keldaanCommunity/pokemonAutoChess/master/app/utils/random.ts))

### Inferences
- Kindred Siege already has seed plus orders replay. The highest value addition is a structured event log emitted by the sim (time, source, target, effect, amount). One log can feed the post fight recap, a scrubbable replay, the combat log text and bot analytics.
- For server games, replays cost storage and bandwidth; for a local deterministic single player game they cost almost nothing, so the usual reasons to skip them do not apply.

### Gaps
- No developer statement found on why TFT, The Bazaar or SAP skip full replays.
- No dated patch note found for when Backpack Battles added its fight log.

## 6. UI readability

### Takeaway
Shipped games put a lot on screen but layer it: always visible bars and cooldown timers, a trait tracker with hover detail, a fixed keyword vocabulary with tooltips, and the deep numbers in a post fight recap. Riot treats readability as a design rule ("tough choices, not touch execution").

### Cited Findings
- Abecassis (GDC 2020): TFT limited board size and let units combine to cut the "juggling" of units, under the rule "tough choices, not touch execution". [opinion] ([Pocket Tactics](https://www.pockettactics.com/teamfight-tactics-design))
- TFT trait tracker got visual effects and an updated chromatic tier for rare synergies in patch 10.2. [snippet] ([Upcomer, patch 10.2](https://upcomer.com/teamfight-tactics-patch-10-2-breakdown/)) Hovering a trait and then its champion icons shows which units fit. [community] ([esports.gg UI guide](https://esports.gg/guides/teamfight-tactics/tft-tip-tuesday-how-to-utilize-the-ui-tools))
- TFT combat recap with damage dealt, taken and healing (patch 12.6). [snippet] ([LoL Wiki, V12.6](https://wiki.leagueoflegends.com/en-us/V12.6_(Teamfight_Tactics)))
- Riot's League clarity guidance: abilities should keep visual hierarchy, avoid excess noise and stay clear in chaotic fights. [snippet] ([Riot, Dev: Clarity in League](https://riotgames.com/en/news/dev-clarity-in-league)) In 2019 League damage text was cut from four colours to three for readability. [snippet] ([Surrender at 20, PBE update](https://www.surrenderat20.net/2019/06/618-pbe-update-arcade-caitlyn-pe-tft-up.html))
- Riot treats information exposure as competitive design: it asked third party tools not to make scouting other boards too easy. ([Riot DevRel](https://www.riotgames.com/en/DevRel/teamfight-tactics-match-history))
- Backpack Battles marks adjacency with Star and Diamond symbols ([wiki](https://backpackbattles.wiki.gg/wiki/Game_Mechanics)); its 1.0 update (13 Jun 2025) added status indicators and tooltips [snippet] ([Mein MMO](https://mein-mmo.de/90-prozent-positiv-auf-steam-indie-hit-backpack-battles/)); it shows a post fight log with stats [snippet] ([PC Games](https://www.pcgames.de/Indie-Games-Thema-239276/Specials/Backpack-Battles-Builds-Early-Access-Inventar-getestet-Auto-Battler-1442869/2/)).
- The Bazaar uses a small status vocabulary (Haste, Slow, Freeze, Charge and others), colour tiers Bronze to Diamond and 11 named enchantments ([wiki](https://thebazaar.wiki.gg/wiki/Items)); since patch 13.0 a monster's loadout can be inspected before the fight [snippet] ([Mobalytics](https://mobalytics.gg/the-bazaar/guides/patch-notes)).
- SAP's whole rules text rests on 14 trigger words ([SAP wiki, Glossary](https://superautopets.wiki.gg/wiki/Glossary)); its art comes from free emoji libraries (Twemoji, Noto, FxEmojis) and its itch page states no generative AI was used ([itch.io](https://teamwood.itch.io/super-auto-pets)).
- Bob's Buddy adds a per combat odds readout (win, tie, loss, lethal) on top of the game. ([HSReplay](https://articles.hsreplay.net/2020/04/24/introducing-bobs-buddy/))

### Inferences
- A fixed keyword list (SAP's 14 triggers, the Bazaar's handful of statuses) keeps every tooltip short. Each trigger and status should be named once, shown as one icon on the unit or item, and explained on hover.
- Showing the enemy board before the fight (Bazaar 13.0) and a win odds readout (Bob's Buddy) both suit a "decide, then watch" loop, and a deterministic sim can compute the odds.
- Keep the in fight screen to bars, timers and status icons; push numbers to the post fight recap, as TFT and Backpack Battles do.

### Gaps
- No primary source on the design reasoning behind TFT's damage recap or The Bazaar's UI.
- No measured guidance found on how much on screen information is too much.

## 7. Content scale, team size and development time

### Takeaway
Riot launched TFT with about 12 people in roughly 18 weeks on top of League's engine, then grew sets to about 60 champions and 25 to 28 traits (100 champions in Set 16). Backpack Battles is the clearest small team case: two developers, weekly patches, over 500 items by 2025. The Bazaar took about seven years from concept to release with an outsourced team and investor money. A single SAP pack (60 pets, 16 foods) is the smallest complete content set in this list.

### Cited Findings
**TFT**
- Began as a 12 developer project with a planned 18 weeks (8 prototyping, 10 to release); launched in the League client 26 Jun 2019; standalone mobile 19 Mar 2020; over 33 million monthly players by Sep 2019; a mid set update rotates part of the roster about every three months; 18 numbered sets listed by Aug 2026. ([Wikipedia](https://en.wikipedia.org/wiki/Teamfight_Tactics)) A GDC promo said the game was designed in less than six months. ([GDC news](https://gdconf.com/news/get-key-game-design-insights-riots-teamfight-tactics-gdc-2020))
- Set 5: 58 new champions and 27 new traits. [snippet] ([Dexerto](https://www.dexerto.com/league-of-legends/tft-set-5-all-champions-traits-more-1551047)) Set 8: 59 new units. [snippet] ([Dexerto](https://www.dexerto.com/league-of-legends/tft-set-8-champions-traits-1977898/))
- Set 15 (2025) added twelve all new origins and traits. [snippet] ([esports.gg interview](https://esports.gg/news/teamfight-tactics/interview-tft-set-15-k-o-coliseum-gets-anime-champions-traits-and-more))
- Set 16 (3 Dec 2025): 100 champions, 60 available from the start and 40 unlockable; lead set designer Julien Camaraza said past sets stuck to roughly 60. Unlockables by cost: six 2 cost, five 3 cost, thirteen 4 cost, ten 5 cost, six 7 cost. [snippet] ([esports.gg](https://esports.gg/news/teamfight-tactics/tft-set-16-lore-and-legends-unlockable-champions/); [Mobalytics](https://mobalytics.gg/tft/guides/set-16-mechanic-unlockables))

**Super Auto Pets**
- Team Wood Games, Unity; Steam 24 Sep 2021, iOS and Android Feb 2022, macOS Oct 2022. Pack sizes: Turtle 60 pets and 16 foods; Puppy (reworked June 2023) 50 new pets and 9 foods; Star 83 and 24; Golden 74 and 14; Unicorn 96 and 20 (largest); Danger 61 and 18. Seven packs live at once including a weekly pack. ([Wikipedia](https://en.wikipedia.org/wiki/Super_Auto_Pets))
- Art built from free emoji libraries; no generative AI used, per the studio. ([itch.io](https://teamwood.itch.io/super-auto-pets))

**Backpack Battles**
- Two developers, Doro and Mario (PlayWithFurcifer, Germany), publisher IndieArk; demo live June 2023; Early Access 8 Mar 2024 at $13; 38 weekly patches plus hotfixes; 640,000 units by 8 Apr 2024; all time peak 36,500 concurrent players; Super Auto Pets named as the biggest inspiration. ([GameDiscoverCo, 24 Apr 2024](https://newsletter.gamediscover.co/p/how-backpack-battles-sold-650k-copies))
- Full release 13 Jun 2025 added two classes, Mage and Adventurer. [snippet] ([Mein MMO](https://mein-mmo.de/90-prozent-positiv-auf-steam-indie-hit-backpack-battles/)) Steam now lists over 500 items, 7 classes with 5 subclasses each, ranked, unranked, custom rules and live lobbies. ([Steam](https://store.steampowered.com/app/2427700/Backpack_Battles/)) An older listing said over 400 items. [community] ([Coop GG listing](https://www.co-op.gg/dos/game/backpack-battles))
- Players start with 63 inventory tiles of which 12 to 14 are usable, 13 gold and 25 health; round 18 gives 16 gold and 350 health; shop shows 5 items; rerolls cost 1 gold for the first four, then 2; 10% chance per item of a 50% sale; subclass unlocks at round 8. ([Backpack Battles wiki, Game Mechanics](https://backpackbattles.wiki.gg/wiki/Game_Mechanics))

**The Bazaar**
- Conceived 2017 as a digital card game; announced 6 Apr 2018 ([The Bazaar wiki](https://thebazaar.wiki.gg/wiki/The_Bazaar)); N-iX Games joined in 2018 with seven people (producer, two Unity developers, two artists, a backend developer, a technical designer), growing to about 15 by 2020; Unreal prototype replaced by Unity in 2018; pivot to real time auto battling late summer 2020; new vertical slice end of 2021; core development moved in house at Tempo in 2022; N-iX scaled down in summer 2023. [contractor's own marketing] ([N-iX Games case study](https://gamestudio.n-ix.com/case-study/the-bazaar-game-development-story/))
- Release dates conflict: N-iX says Early Access in fall 2024; the wiki lists a closed beta in October 2024 and release in March 2025; Steam shows 13 Aug 2025. ([N-iX](https://gamestudio.n-ix.com/case-study/the-bazaar-game-development-story/); [wiki](https://thebazaar.wiki.gg/wiki/The_Bazaar); [Steam](https://store.steampowered.com/app/1617400/The_Bazaar/))
- Tempo raised $3.3 million to make the game. [snippet] ([ONE Esports](https://www.oneesports.gg/gaming/tempo-storm-raises-us3-3m-to-make-its-own-video-game/))
- Steam lists "over 100 unique items" and hundreds of items and combinations, with an expanding hero roster. ([Steam](https://store.steampowered.com/app/1617400/The_Bazaar/)) The wiki has item pages for six heroes: Dooley, Mak, Stelle, Pygmalien, Jules, Vanessa, plus Monster items. ([wiki, Items](https://thebazaar.wiki.gg/wiki/Items))

**Pokémon Auto Chess**
- 9,477 commits, 1.8k stars, 560 forks, GPL 3.0. ([GitHub](https://github.com/keldaanCommunity/pokemonAutoChess))

### Inferences
- Kindred Siege today (5 classes, 3 tribes, 1 item, 2 combos) is well below the smallest shipped set here. One SAP pack (60 pets, 16 foods) is the nearest benchmark for a complete, replayable content set.
- Backpack Battles is the realistic model for a tiny team: launch a public demo early, then grow content in small, frequent drops. Its weekly pace was described as extremely taxing for two people, so a solo developer should plan a slower cadence.
- TFT's 60 unit sets come from a large live team on a three month cadence with a dedicated balance team. That size is not a solo target; the takeaway is the tooling (data driven content, sims, validation) that lets content scale.
- The Bazaar's roughly seven years, a contractor team and multiple pivots are a scope warning, not a template.

### Gaps
- Team size not found for Tempo's internal team, Team Wood Games, or the current TFT team.
- Not confirmed that the Turtle pack count (60 pets) equals SAP's launch day roster.
- Backpack Battles item count at Early Access launch not found.
- No exact item count for The Bazaar; Steam's "over 100" is a marketing floor.

## 8. Open source projects and tutorials showing a minimal working architecture

### Takeaway
The most complete public reference is Pokémon Auto Chess (TypeScript, GPL 3.0, real time, multiplayer, with bots, Elo and matchmaking). For turn based SAP style sims, `sapai` is the reference. For Godot there is a 26 hour video course. No well documented Unity open source auto battler turned up in this pass.

### Cited Findings
- Pokémon Auto Chess: TypeScript, GPL 3.0; needs Node, MongoDB and Firebase; bots imported from `db-commands/botv2.json`; Pokémon rights belong to The Pokémon Company and the project can stop at any time. ([GitHub README](https://github.com/keldaanCommunity/pokemonAutoChess); [HelloGitHub listing](https://hellogithub.com/en/repository/keldaanCommunity/pokemonAutoChess))
- Its `app/core` folder maps the systems one to one: `simulation.ts`, `simulation-command.ts`, `pokemon-entity.ts`, `pokemon-state.ts`, `idle-state.ts`, `moving-state.ts`, `attacking-state.ts`, `abilities/`, `effects/`, `bot.ts`, `bot-logic.ts`, `bot-manager.ts`, `dps.ts`, `elo.ts`, `elo-engine.ts`, `matchmaking.ts`, `unit-score.ts`, `board.ts`. ([app/core](https://github.com/keldaanCommunity/pokemonAutoChess/tree/master/app/core))
- `sapai` is a Python Super Auto Pets engine built for reinforcement learning, with unit tests; class projects wrap it as a Gym environment; other repos add PPO agents and screen reading to test against the real game. [snippet] ([sapai](https://github.com/manny405/sapai); [CS181SAPAI](https://github.com/HangYu04/CS181SAPAI); [super ml pets](https://github.com/andreped/super-ml-pets))
- GodotGameLab (Adam Gulacsi) publishes a free single player TFT style autobattler course: 30 videos, about 26 hours, covering grid positioning, AI combat, abilities, combining units and equipping items. Source code availability not confirmed. ([80 Level, 15 Jul 2026](https://80.lv/articles/master-godot-4-for-free-with-over-50-hours-of-game-development-courses))
- `auto-racer-code-only` is a code only clone of a Godot 3.4.4 auto battler written mainly in C# with some GDScript. [snippet] ([GitHub](https://github.com/jake-small/auto-racer-code-only))

### Inferences
- Pokémon Auto Chess is the best map of what a full product needs (unit state machine, effect hooks, bot manager, Elo, matchmaking, DPS meter) but it is not deterministic, so it shows what Kindred Siege should not copy in its RNG.
- Its GPL 3.0 licence means copying its code into a closed commercial game would oblige releasing the game's source; use it for ideas only.
- The `auto-racer` repo is the only C# example found, and it targets Godot, not Unity.

### Gaps
- No well documented Unity auto battler repo found.
- No GDC talk found that walks through a minimal auto battler architecture.
- Did not verify whether GodotGameLab's course code is published.
