# Combat resolution and determinism in commercial auto battlers

Label key used on every finding. [Official] = developer or publisher page, patch note, store page or press release. [Official, designer opinion] = a developer's stated view. [Wiki] = community wiki or Wikipedia describing mechanics. [Press] = journalist article, review or guide site. [Community tool] = third party simulator or tracker and its docs. [Community measurement] = numbers players found by testing. [Player claim] = forum post, not confirmed by the studio. [Opinion] = a subjective view. "(search summary only)" = I saw the claim in a search result summary but could not open the page, so treat it as weaker. Research date: 12 Oct 2026. The session's web search budget ran out before every gap could be chased, so the Gaps lists are partly "not searched further", not only "does not exist".

## 1. How each game resolves combat: server or client, seed, simultaneous or turn by turn, tick, exact replay

### Takeaway
No studio I found publicly documents its server or client split, RNG seeding or tick rate for any game in this set. What is public splits the games into two models: turn by turn queues (Hearthstone Battlegrounds, Super Auto Pets, Storybook Brawl) and fights that run on a clock of attack timers or item cooldowns (TFT, The Bazaar, Backpack Battles and Hero's Hour by their sources; Underlords and Mechabellum are only described as automatic). Three clock games have a documented timer that ends stalls (Bazaar sandstorm at 30 s, Backpack Battles fatigue at 17 s, TFT overtime at 30 s per a fan sim). Only Mechabellum advertises replays you can take over from a past moment, and the brief's premise that Mechabellum rounds are deterministic is not confirmed by any source I found.

### Cited Findings
**Hearthstone Battlegrounds (Blizzard, Nov 2019 onward)**
- [Official, 1 Nov 2019] Minions take turns attacking, starting with each player's leftmost minion. The attacked enemy "will be completely random" unless the opponent has Taunt, and Taunt minions must be attacked first. Source: [Blizzard, Introducing Hearthstone Battlegrounds](https://hearthstone.blizzard.com/en-us/news/23156373)
- [Wiki] "The player with the greater number of minions attacks first." If counts are equal, "one of the players is randomly selected to attack first." Attacks run leftmost to rightmost for each player, then repeat, and newly summoned minions join the sequence. A minion with 0 attack skips its attack. Combat runs until one or both players have no minions. Source: [Hearthstone Wiki, Battlegrounds](https://hearthstone.wiki.gg/wiki/Battlegrounds)
- [Wiki] "The Combat Phase does not actually last any time." Recruit Phase timer: 60 seconds in round one, plus 15 seconds each round, capped at 120 seconds. Boards reset after each combat (all minions restored, combat tokens removed). Source: [Hearthstone Wiki, Battlegrounds](https://hearthstone.wiki.gg/wiki/Battlegrounds)
- [Player claim, 6 Jul 2021] "The results of the battle are calculated as soon as your turn ends so skipping the animation has no impact on the outcome." No Blizzard staff reply in the thread. Source: [Blizzard forums, Battlegrounds turn timer](https://us.forums.blizzard.com/en/hearthstone/t/battlegrounds-turn-timer/65935)

**Super Auto Pets (Team Wood)**
- [Wiki] Battle opens with a Start of Battle phase where all such abilities fire. Then "The player's right-most Pet attacks the opponent's left-most Pet simultaneously." Fainted pets are removed, the next pet steps up, and this repeats until a team is empty. Both empty is a draw. Source: [Super Auto Pets Wiki, The Basics](https://superautopets.wiki.gg/wiki/The_Basics)
- [Wiki, 2021 revision] "Abilities trigger in attack order: the pet with the most attack goes first, then the second most, etc." Also: "Nothing that happens in battle is permanent." Source: [Super Auto Pets Wiki, Glossary](https://superautopets.wiki.gg/wiki/Glossary)
- [Press, undated guide] Equal attack is broken by higher HP. Source: [Two Average Gamers guide](https://twoaveragegamers.com/super-auto-pets-win-more-consistently-with-this-ultimate-guide/). Contradicted in part by [Player claim, 2021 to 2022, search summary only] a player saw a Dodo and Jaguar resolve in a different order with the same setup, and others were unsure whether ties are coin flips. Source: [Steam thread](https://steamcommunity.com/app/1714040/discussions/0/3086646248547568129)

**Teamfight Tactics (Riot)**
- [Wiki] Crit is a chance per attack (base 25%), see section 2. Source: [League of Legends Wiki, TFT Critical strike](https://wiki.leagueoflegends.com/en-us/TFT:Critical_strike)
- [Community tool, README tracks Set 6.5 so roughly 2022] A fan simulator models units that pathfind into range, auto attack, generate mana, cast abilities, and fight "until one team is left standing". It lists a "30 second overtime limit" as not yet built and says "Some values must be experimentally determined by manually timing or measuring values from the original game." Source: [tacticians-academy/teamfight-simulator](https://github.com/tacticians-academy/teamfight-simulator)
- [Press, 17 Jun 2026] TFT moved from Riot's Hextech engine to Unreal Engine with the Enchanted Wilds set (live 11 Aug 2026, public beta from 14 Jul). Riot's lead said "Moving things on the board uses the exact same code algorithm in Unreal as it did in Hextech." The article says nothing about servers, netcode, determinism or replays. Source: [GamesBeat, TFT to Unreal](https://gamesbeat.com/inside-riot-games-decision-to-move-development-of-teamfight-tactics-to-unreal-engine/)

**Dota Underlords (Valve)**
- [Wiki, Wikipedia] Heroes on an 8x8 grid "automatically fight the opposing team without any further direct input from the player." Early access 20 Jun 2019, full release 25 Feb 2020. Early access added "a replay system". Over 1.5 million downloads and over 200,000 concurrent Steam players within days. Source: [Wikipedia, Dota Underlords](https://en.wikipedia.org/wiki/Dota_Underlords)

**Auto Chess (Drodo)**
- [Wiki, Wikipedia] Drodo Studio released Dota Auto Chess in January 2019 and it had over 8 million players by May 2019. Drodo and Valve both made standalone versions later in 2019. No combat resolution detail found. Source: [Wikipedia, Auto battler](https://en.wikipedia.org/wiki/Auto_battler)

**The Bazaar (Tempo, Reynad)**
- [Press, 29 Mar 2025] The sandstorm normally starts 30 seconds into a fight. Mak's Bottled Tornado starts it at 7 seconds. Items run on cooldowns (example: Laboratory starts at a 3 second cooldown and drops 1 second per use). Source: [PCGamesN, Mak](https://www.pcgamesn.com/the-bazaar/mak-new-hero-season-1)
- [Press guide, search summary only] Haste makes an item charge twice as fast, Slow half as fast, Freeze stops charging. Crit chance applies only to items with a cooldown and doubles active effects (damage, heal, shield, burn, poison). Source: [Mobalytics, Bazaar keywords](https://mobalytics.gg/the-bazaar/guides/keywords-and-terms)
- [Community tool, search summary only] The BazaarDB tracker (beta, Windows) saves each fight with its opponent and offers "tick by tick" combat replays, which suggests the fight runs on discrete ticks. Tick length not found. Source: [BazaarDB tracker](https://bazaardb.gg/tracker)

**Backpack Battles (PlayWithFurcifer)**
- [Press, 13 Mar 2024, German, my translation] Duels run fully automatically, items trigger on cooldowns, a multi step fast forward exists, and you cannot influence the result during the action. Source: [PC Games](https://www.pcgames.de/Indie-Games-Thema-239276/Specials/Backpack-Battles-Builds-Early-Access-Inventar-getestet-Auto-Battler-1442869/2/)
- [Wiki] Fatigue ("Nightfall") starts at 17 seconds. Damage starts at 1 and grows by `DMG(n) = DMG(n-1) + floor(DMG(n-1)/10) + 1`. After 60 seconds of fight time the divisor drops to 5. Worked example: 29 damage at 34 s, 32 at 35 s. The Power of the Moon item makes fatigue start 6 s earlier and stacks if both players have it. The wiki notes "minor variation" in when the 60 s change shows, cause unknown. Source: [Backpack Battles Wiki, Fatigue](https://backpackbattles.wiki.gg/wiki/Fatigue)

**Mechabellum (Game River, published by Dreamhaven since 25 Mar 2025)**
- [Wiki, Wikipedia] Armies fight over several rounds. Each round players get supplies to buy and place units. "Once placed, the units cannot be moved, nor can they be directly controlled." Winning a round lowers the opponent's HP. Early access 11 May 2023, full release 26 Sep 2024. Source: [Wikipedia, Mechabellum](https://en.wikipedia.org/wiki/Mechabellum)
- [Press, Japanese, search summary only] Units placed in earlier rounds are restored at the start of each round and cannot be repositioned. A battle runs until one side is wiped, and surviving units set the damage. Source: [Automaton](https://automaton-media.com/articles/newsjp/pvp-autobattler-mechabellum-20240927-312417/)
- [Official, 25 Mar 2025] Replay system: "Take control of past battles to examine precise moments and explore alternative strategies." Source: [Dreamhaven press release](https://pressreleases.triplepointpr.com/2025/03/25/dreamhaven-announces-partnership-with-game-river-to-publish-tactical-auto-battler-mechabellum/)
- [Player claim, 1 Jun 2023] Missile launcher spread seemed to vary a lot, and fortress anti air killed all six bombers in one round but only one in another. Other players replied that RNG is "very minimal". No developer reply. Source: [Steam thread, rng in this game](https://steamcommunity.com/app/669330/discussions/0/3833172420309249835)

**Storybook Brawl (Good Luck Games, servers shut 1 May 2023)**
- [Press, 23 Sep 2022] Front row of 4, back row of 3. "Your front row will be the units attacking first." Ranged units deal damage without taking any. Ranged and flying units can hit the back row. Support units buff the two units in front of them. Source: [HSReplay, Storybook Brawl part 2](https://articles.hsreplay.net/2022/09/23/storybook-brawl-the-perfect-game-for-battlegrounds-players-2/)
- [Wiki, Wikipedia] Combat phases were "automated fights against other players' boards". Early access 18 Jun 2021, studio bought by FTX 22 Mar 2022, servers shut down 1 May 2023. Source: [Wikipedia, Storybook Brawl](https://en.wikipedia.org/wiki/Storybook_Brawl)

**Hero's Hour (ThingOnItsOwn), included as a mid fight input example**
- [Official] "A fast turn-based strategy RPG with real-time combat." In battles "your units will do the best to fight for themselves in real time". Source: [Steam store](https://store.steampowered.com/app/1656780/Heros_Hour/)

### Inferences
- The HS BG wiki line that combat "does not actually last any time", plus the player claim, points to "compute the whole fight instantly, then play an animation". That is the same split Kindred Siege uses. Blizzard has not confirmed it in anything I found.
- Three of the clock games have a documented stall breaker: Bazaar sandstorm at 30 s, Backpack Battles fatigue at 17 s with an escalating formula, TFT overtime at 30 s per the fan sim README. No timer source was found for Underlords, Mechabellum or Hero's Hour.
- Mechabellum's "take control of past battles" fits either a deterministic resimulation from recorded inputs or saved state snapshots. Sources do not say which. The 2023 player reports of variable missile spread fit either seeded randomness inside a replayable sim or real variance. I cannot tell which.
- Summary matrix (my synthesis of the findings above; "not stated" means no source found):

| Game | Where resolved | Seed | Order model | Tick | Exact replay |
|---|---|---|---|---|---|
| HS Battlegrounds | not stated (player claim: at end of turn) | not stated | alternating, leftmost first; more minions starts; tie random | combat "does not last any time" | no official BG replay found |
| Super Auto Pets | not stated | not stated (fan sim uses seeds) | front pets hit at the same time; start of battle by attack | not stated | not found |
| TFT | not stated | not stated | clock (attacks, mana, pathing) | not stated | no replay found; spectate since 14.1 |
| Underlords | not stated | not stated | automatic, model not stated | not stated | replay system added in early access |
| Auto Chess | not found | not found | not found | not found | not found |
| The Bazaar | not stated | not stated | clock, item cooldowns | "tick by tick" per third party tracker | third party tracker only |
| Backpack Battles | not stated | not stated | clock, item cooldowns plus stamina | not stated | post fight action log (2024) |
| Mechabellum | not stated | not stated | automatic per round, model not stated | not stated | official replays with take over |
| Storybook Brawl | shut down | not stated | front row first | not stated | not found |

### Gaps
- No technical source (GDC talk, engineering blog, dev interview) found for server versus client, RNG seeding or tick rate in any of these games. The 2026 TFT engine article is silent on it. Search budget ran out before trying GDC Vault, Riot Tech Blog, Valve dev posts or Game River talks.
- Storybook Brawl: order inside a row and which player starts were not found.
- Auto Chess and Underlords: combat randomness and determinism not found.
- Mechabellum determinism: unconfirmed either way.

## 2. Which randomness each game keeps or removes, and what designers have said about randomness versus predictability

### Takeaway
Shop and loot randomness is everywhere. Combat randomness survives in a few narrow channels: random targets (HS BG, some SAP abilities), crit rolls (TFT, The Bazaar, Backpack Battles), hit or miss rolls (TFT dodge and Blind, Backpack Battles accuracy) and, per players, projectile spread and unit drops in Mechabellum. The clearest designer statement is Riot's from 2019: keep randomness "in positive and controlled spaces", and a disable is fine when "it was predictable and could be played around". I found no public statement from Team Wood, Tempo or Reynad, PlayWithFurcifer, Game River or Valve on combat randomness.

### Cited Findings
- [Official, 2019] HS BG targets are random unless Taunt. [Wiki] first attacker is random when minion counts tie. Sources: [Blizzard](https://hearthstone.blizzard.com/en-us/news/23156373), [Hearthstone Wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)
- [Official, 2019] HS BG opponents are randomly chosen. Your next opponent shows on the leaderboard, but "you'll have to wait until the battle begins to see your next opponent's Minions and positioning." Source: [Blizzard](https://hearthstone.blizzard.com/en-us/news/23156373)
- [Community tool, 24 Apr 2020] A BG combat's outcome "can swing heavily based on just a few attacks" (example: a stray Soul Juggler ping). Source: [HSReplay, Introducing Bob's Buddy](https://articles.hsreplay.net/2020/04/24/introducing-bobs-buddy/)
- [Wiki] SAP Mosquito: "Start of battle: Deal 1 damage to one random enemy" (two at level 2, three at level 3). Multiple Mosquitos can hit the same enemy. Start of battle abilities "cannot be interrupted" and fire even after the pet faints. Source: [Super Auto Pets Wiki, Mosquito](https://superautopets.wiki.gg/wiki/Mosquito)
- [Wiki] SAP: start of battle order is set by attack (section 1). Tie handling is disputed (section 1).
- [Wiki] TFT crit: base 25% chance and 40% bonus crit damage (140% total). Launch patch 9.13 lists 25% chance and "50% critical strike damage" (read as 150% total, which matches the next note). The patch 11.12 note says crit damage went to 130% from 150%. The current page says 140%, so either it rose again later or the page is stale; I could not confirm which. Most abilities cannot crit on their own. Source: [League of Legends Wiki, TFT Critical strike](https://wiki.leagueoflegends.com/en-us/TFT:Critical_strike)
- [Wiki] TFT dodge ("An attack that is dodged deals no damage") appears in sources from Set 1 through Set 18 items (Quicksilver, Sparring Gloves, Thief's Gloves are listed). Blind ("Attack might miss") appears in Sets 4 and 4.5. Set 2's Cloud Hex gave 20% extra dodge. I found no patch that removed dodge. Source: [League of Legends Wiki, TFT Dodge](https://wiki.leagueoflegends.com/en-us/TFT:Dodge)
- [Press, patch summaries, search summary only] Yordle dodge went from 30%/55% to 35%/60% in patch 9.17 (2019). The So Small augment went from 25% to 30% dodge in patch 12.6 (2022). Sources: [NerfPlz 9.17](https://www.nerfplz.com/2019/08/team-fight-tactics-official-patch-917.html), [NME 12.6](https://www.nme.com/news/teamfight-tactics-12-6-patch-notes-reveal-major-attack-damage-reductions-3190700)
- [Official, designer opinion, Riot_Mort, 28 Oct 2019] "We still strongly believe that utilizing randomness in the correct way creates a wider variety of experiences." Two piece Imperial was "a very random effect that boiled down to a coin flip" on a carry's double damage, accepted because players opted into it. Phantom created exciting moments at a cost "likely too costly for the overall experience". Item drops were reworked "so that the outcomes were more controlled than before". Zephyr worked because "it was predictable and could be played around". Goals: fewer disables, softer counters, randomness in "positive and controlled spaces". Source: [Riot, /dev: TFT Set 1 Learnings](https://teamfighttactics.leagueoflegends.com/en-us/news/dev/dev-tft-set-1-learnings)
- [Press, search summary only] Mortdog unbanned matchmaking trackers and said tracking opponents on paper is "not a skill test TFT is interested in testing". Source: [Upcomer](https://upcomer.com/tft-dev-announces-ban-lift-on-matchmaking-add-ons/)
- [Press, 2025] Bazaar: some item effects pick randomly (Quicksilver duplicates "a random small item on your board"). Source: [PCGamesN](https://www.pcgamesn.com/the-bazaar/mak-new-hero-season-1). [Press guide, search summary only] Bazaar crit is a chance on cooldown items and doubles active effects; Haste, Slow and Freeze change charge speed by set multiples (2x, 0.5x, 0). Source: [Mobalytics](https://mobalytics.gg/the-bazaar/guides/keywords-and-terms)
- [Wiki, old fandom wiki no longer updated, search summary only] Backpack Battles: accuracy decides hit or miss; each Luck stack adds 5% accuracy and each Blind stack removes 5%; running out of stamina stops a weapon acting. A player estimated that an item eating 7 Luck costs about 35% accuracy ([Community measurement], derived, not official). Sources: [Fandom, Accuracy](https://backpack-battles.fandom.com/wiki/Accuracy), [Fandom, Critical hits](https://backpack-battles.fandom.com/wiki/Critical_hits)
- [Wiki] Backpack Battles fatigue is a pure formula once started (section 1). Source: [Backpack Battles Wiki, Fatigue](https://backpackbattles.wiki.gg/wiki/Fatigue)
- [Player claim and opinion, 1 Jun 2023] Mechabellum: missile spread and anti air results vary; "unit unlocks have no RNG, unlike other auto battlers such as TFT"; "RNG is an asset in these games, not a problem" (argued that precise calculation makes games boring). Source: [Steam thread](https://steamcommunity.com/app/669330/discussions/0/3833172420309249835)
- [Official, search summary only] Mechabellum update 0.8 (Apr 2024) added rare units and "major changes to the unit drop system". Source: [Mechabellum Wiki, Update 0.8](https://mbwiki.dreamhaven.com/updates/update-08-rare-units-and-ui-changes). [Opinion, early 2024, search summary only] a player with about 500 hours said "every single game is decided by luck at higher MMR now". Source: [Steam thread](https://steamcommunity.com/app/669330/discussions/0/4289187252719404872/?ctp=2)
- [Press, quoted on Wikipedia] PC Gamer on Storybook Brawl: the "sheer number of random elements causes games to vary wildly from one to the next". Source: [Wikipedia, Storybook Brawl](https://en.wikipedia.org/wiki/Storybook_Brawl)

### Inferences
- Riot's rule of thumb from 2019 is the closest thing to a genre principle: randomness is tolerated where players opted in (Imperial) or can plan around it (Zephyr), and cut where it overrides decisions (Phantom, Hextech).
- Queue games keep randomness in who gets hit; clock games keep it in whether a hit lands or crits. In the documented clock games, cooldown timing itself changes only by set multipliers (Haste, Slow, Freeze), not by random jitter. That last point is inferred from the keyword definitions, not stated by any studio.
- For a seeded sim like Kindred Siege, the evidence says the question is whether variance is visible and plannable, not whether it exists.

### Gaps
- No designer statements found from Team Wood (SAP fixed order), Tempo or Reynad (Bazaar cooldowns and crit), PlayWithFurcifer, Game River (Mechabellum) or Valve on combat randomness. Not searched: dev Discords, Reddit AMAs, podcasts.
- TFT targeting rules (closest unit, tie breaks) not found in any source.
- No source on whether TFT ever removed dodge; the wiki suggests it still exists on items through Set 18.

## 3. How asynchronous games use recorded opponent builds (ghosts)

### Takeaway
Super Auto Pets Arena, The Bazaar and Backpack Battles are built on asynchronous ghosts: you fight a stored copy of another player's board taken at the same point in their run (same turn, same day, same round). That removes turn timers and lets runs pause. Live games (TFT, HS BG) only use a ghost to fill an odd player count. Riot also tested a single player, PvE only TFT mode in 2024.

### Cited Findings
- [Wiki] SAP Arena: "Arena Mode features untimed, asynchronous battles." Opponents come from "a database of teams created by other players on the same turn." Goal is 10 wins. Lives: 5 normal, 7 easy, 5 hard. One life back at turn 3 if you lost any. Versus mode is a 2 to 8 player room with a turn timer and 6 lives. Source: [Super Auto Pets Wiki, The Basics](https://superautopets.wiki.gg/wiki/The_Basics)
- [Official, search summary only] Team Wood describes Arena as "asynchronous match ups, no turn timer!" Source: [Team Wood itch.io page](https://teamwood.itch.io/super-auto-pets)
- [Official] The Bazaar: you "face real players during their journey", tagged Asynchronous Multiplayer and PvP. Steam release date 13 Aug 2025. Source: [Steam, The Bazaar](https://store.steampowered.com/app/1617400/The_Bazaar/). [Wiki] closed beta October 2024, "released in March 2025", first announced by Reynad on 6 Apr 2018. Source: [The Bazaar Wiki](https://thebazaar.wiki.gg/wiki/The_Bazaar). Note: the Steam date and the wiki date differ; the wiki may mean the launch on Tempo's own launcher (not confirmed).
- [Press guide, search summary only] Bazaar matchmaking pairs you with an opponent from the same day, and you fight "images" of opponents who played up to several hours earlier. Sources: [Mobalytics beginner guide](https://mobalytics.gg/blog/the-bazaar/the-bazaar-beginner-guide/), [Mobalytics day guide](https://mobalytics.gg/the-bazaar/guides/day-guide)
- [Opinion, forum, search summary only] Async lets the game match you against a snapshot of someone's deck while they are offline, avoiding queues and quitters seen in HS BG. Source: [RPGHQ forum](https://rpghq.org/forums/viewtopic.php?p=170024)
- [Official] Backpack Battles: "Face the builds of other players in different game modes" and "See what they came up with, and counter them." Ranked: "Gain points against equal opponents, but still at your own pace." Unranked has no timers; Lobbies are live with a timer. Early access 8 Mar 2024, release 13 Jun 2025. Source: [Steam, Backpack Battles](https://store.steampowered.com/app/2427700/Backpack_Battles/)
- [Press, 13 Mar 2024] Backpack Battles opponents are builds of players who reached the same rank in their own runs, and fights can be paused. Source: [PC Games](https://www.pcgames.de/Indie-Games-Thema-239276/Specials/Backpack-Battles-Builds-Early-Access-Inventar-getestet-Auto-Battler-1442869/2/). [Press, Mar 2024, search summary only] you face "the character and bag of another real player", and everyone seems to start with the same gold and health. Source: [GamingOnLinux](https://www.gamingonlinux.com/2024/03/backpack-battles-is-another-clear-indie-steam-hit)
- [Wiki] HS BG: Kel'Thuzad "takes the board and tavern tier of a previously defeated player but has 0 actual health". Nobody faces the same player or Kel'Thuzad more than once in any 3 combat rounds (unless two players remain). Source: [Hearthstone Wiki](https://hearthstone.wiki.gg/wiki/Battlegrounds)
- [Player claim, 23 May 2024] Who gets the BG ghost seemed tied to lobby creation order. No staff reply. Source: [Blizzard forums, Ghost algorithm](https://us.forums.blizzard.com/en/hearthstone/t/ghost-algorithm-in-battlegrounds/128995)
- [Press guides, search summary only] TFT: Mobalytics says beating a ghost deals no damage to its owner, but losing to it still hurts you. Source: [Mobalytics TFT terms](https://mobalytics.gg/?p=31664). MagicGameWorld adds that the duplicate deals damage if it wins as if it were real, and that at 3 players the highest health player always fights the duplicate. Source: [MagicGameWorld](https://www.magicgameworld.com/?p=35264). The two agree that a ghost hurts you when it wins; only MagicGameWorld makes the 3 player claim.
- [Official, Aug 2024] TFT Tocker's Trials: "highly-experimental PvE game mode", solo, 30 rounds including 6 bosses, 3 lives in place of HP, no timers, a Chaos Mode, live 27 Aug to 24 Sep 2024 only. The team prototyped "what TFT could be with only these PvE rounds" and "weren't sure if it was something we'd even end up shipping". Source: [Riot, Tocker's Trials](https://teamfighttactics.leagueoflegends.com/en-gb/news/game-updates/new-tft-workshop-mode-tockers-trials)

### Inferences
- In these three async games, ghosts buy no turn timer, pausable runs and no waiting on other players. Matching on the same turn, day or round is how they keep power levels comparable.
- For a single player game such as Kindred Siege, recorded or authored boards act as content rather than as a fairness tool. Tocker's Trials (lives instead of HP, boss boards "you'd never encounter anywhere else") is the nearest big studio test of that format.

### Gaps
- No official doc on how The Bazaar picks a ghost (when the snapshot is taken, any MMR filter). SAP and Backpack Battles pool filters beyond "same turn" and "same rank" not found. Riot has no official ghost damage rule in anything I opened.

## 4. How players predict outcomes: previews, community simulators and calculators, in game forecasts

### Takeaway
I found no game in this set that shows an official win forecast before combat. Prediction lives in third party tools, and the best known one (Bob's Buddy) only shows odds after the board is locked. In game, studios give information instead of odds: The Bazaar lets you inspect a monster's loadout before choosing it, HS BG shows who you face but hides their board until combat, and Mechabellum's units carry over between rounds (per a Japanese press summary), which should let you see much of the army you will face (my inference).

### Cited Findings
- [Community tool, 24 Apr 2020] Bob's Buddy (bundled with Hearthstone Deck Tracker) "simulates upwards of 10,000 combats" and shows win, tie and loss odds plus lethal odds. It runs at the start of combat and "only provides you with information about your combat performance after combat has begun." Calibrated against "tens of thousands of combat rounds" from HSReplay uploads, with points "very close to the Y = X line". It switches off for Akazamzarak secrets. Source: [HSReplay, Introducing Bob's Buddy](https://articles.hsreplay.net/2020/04/24/introducing-bobs-buddy/)
- [Community tool, search summary only] Bob's Buddy warns when your PC could not run enough simulations, and opponent Secrets are not modelled because they are hidden information. Source: [HearthSim help](https://help.hearthsim.net/en/articles/3948400-why-did-bob-s-buddy-tell-me-its-results-weren-t-accurate-enough-to-show)
- [Community tool, search summary only] Open source BG simulators run a battle many times and report statistics; hsbg-sim reports win, tie, loss shares and score percentiles. Sources: [twanvl simulator](https://github.com/twanvl/hearthstone-battlegrounds-simulator), [hsbg-sim](https://pypi.org/project/hsbg-sim)
- [Community tool, search summary only] SAP: the saptest Rust crate builds teams, assigns seeds "for repeatable results" and simulates fights, using data scraped from the fan wiki; sapai is a Python counterpart. Source: [saptest crate](https://crates-io.ganymede-static.progval.net/crates/saptest/0.4.5)
- [Community tool] TFT fan simulator pulls stats from CommunityDragon and had to measure undocumented values by timing the live game (section 1). Source: [teamfight-simulator](https://github.com/tacticians-academy/teamfight-simulator)
- [Press guide, search summary only] Bazaar patch 13.0 added the ability to "right click a Monster to see its loadout prior to fighting them". Source: [Mobalytics patch notes](https://mobalytics.gg/the-bazaar/guides/patch-notes)
- [Official, 2019] HS BG shows the next opponent on the leaderboard, but their minions and positioning stay hidden until battle starts. Source: [Blizzard](https://hearthstone.blizzard.com/en-us/news/23156373)
- [Official, 2025] Mechabellum markets the game as positioning units and trying to predict or counter the opponent's moves. Source: [Dreamhaven press release](https://pressreleases.triplepointpr.com/2025/03/25/dreamhaven-announces-partnership-with-game-river-to-publish-tactical-auto-battler-mechabellum/)
- [Press, 2026, search summary only] Riot began blocking unknown third party apps from reading game memory by default across League, TFT and VALORANT, part of a security push that also restricted League replays; Riot then delayed some restrictions. This affects community tools. Sources: [Sheep Esports](https://www.sheepesports.com/kr/articles/riot-delays-league-of-legends-replay-restrictions/en), [Dot Esports](https://dotesports.com/league-of-legends/news/riot-delays-league-replay-changes-patch-26-21)
- [Player opinion, 6 Jul 2021] "The actual fight is purely there for you to figure out how the algorithm determined who won." Source: [Blizzard forums](https://us.forums.blizzard.com/en/hearthstone/t/battlegrounds-turn-timer/65935)

### Inferences
- The one widely used forecast (Bob's Buddy) arrives after positioning is locked, so it teaches rather than decides. That is a boundary Kindred Siege can copy (odds after commit) or break on purpose (odds before commit), with the risk that a visible pre commit forecast turns each choice into "pick the highest number".
- Studios share inputs (enemy loadout, enemy identity, persistent armies) and leave the maths to the player. Community tools exist because combat is computable; Riot's tracker stance shows a studio choosing to make computable info free for all rather than reward note taking.

### Gaps
- Firestone's BG simulator repository was not located (guessed URL returned 404). Legion TD 2's rumoured per wave value guide not confirmed. No Bazaar or Backpack Battles fight simulator found. I may have missed in game forecast features added in 2025 to 2026 because the search budget ran out.

## 5. Player agency when the player can act during or between combat rounds, and designer statements on mid fight input

### Takeaway
The genre default is zero input during combat; decisions sit between rounds. Mechabellum sells exactly that as RTS depth "without the actions-per-minute (APM) requirement". A few games add small mid fight input: Hero's Hour lets the hero cast spells and give attack orders in real time, and Marvel Rivals' 2025 auto battler mode gave the player a drone with a blaster that can chip, heal, and damage the other team once per round. I found no designer statement that measures the value of mid fight input.

### Cited Findings
- [Wiki, Wikipedia] Genre definition: units "automatically battle each other, typically without player input". Source: [Wikipedia, Auto battler](https://en.wikipedia.org/wiki/Auto_battler)
- [Wiki, Wikipedia] Underlords heroes fight "without any further direct input from the player". Source: [Wikipedia, Dota Underlords](https://en.wikipedia.org/wiki/Dota_Underlords)
- [Press, 2024] Backpack Battles: you cannot influence the result during the action. Source: [PC Games](https://www.pcgames.de/Indie-Games-Thema-239276/Specials/Backpack-Battles-Builds-Early-Access-Inventar-getestet-Auto-Battler-1442869/2/)
- [Wiki, Wikipedia] Mechabellum: "Once placed, the units cannot be moved, nor can they be directly controlled." Supplies also buy temporary and permanent upgrades. Source: [Wikipedia, Mechabellum](https://en.wikipedia.org/wiki/Mechabellum)
- [Official, 25 Mar 2025] Mechabellum: "fighting as a team–without micro-management" and "Decision-making and tactical gameplay are at the heart of each battle." Source: [Dreamhaven press release](https://pressreleases.triplepointpr.com/2025/03/25/dreamhaven-announces-partnership-with-game-river-to-publish-tactical-auto-battler-mechabellum/)
- [Official, publisher opinion, 25 Mar 2025] Mike Morhaime (Dreamhaven CEO, not the developer) said Mechabellum "captures all the strategic depth of the things we love about real-time strategy" "without the actions-per-minute (APM) requirement". The article narrates that "you can pause to think after each turn". Source: [GamesBeat](https://gamesbeat.com/mechabellum-gets-a-second-chance-as-dreamhaven-relaunches-the-mech-autobattler/)
- [Official] Hero's Hour: "Cast spells and give attack orders to swing the tide of battle - or just sit back and watch as your might unfolds." Source: [Steam store](https://store.steampowered.com/app/1656780/Heros_Hour/)
- [Official devlog, search summary only] Hero's Hour line spells such as Beam of Frost and Sunbeam fire from the hero and "cannot be used if your hero is dead". Source: [itch.io devlog v1.4](https://thingonitsown.itch.io/heros-hour/devlog/288150/fanfare-update-heros-hour-v-14). [Opinion, search summary only] one player called the spells "not very strategic, and fairly weak unless you specifically play a mage". Source: [Steam discussion](https://steamcommunity.com/app/1656780/discussions/0/3184612855376310116)
- [Press, 6 Jun 2025] Marvel Rivals, Ultron's Battle Matrix Protocol (limited time, until 23 Jun 2025): you "use your blaster to chip away at enemy players, plus heal your own teammates, and damage the other team once per round". Source: [Destructoid](https://www.destructoid.com/marvel-rivals-new-limited-time-mode-is-easily-the-best-way-to-play-the-game-since-its-most-recent-update/). [Press, search summary only] after each round players pick one of four buff cards. Source: [Beebom](https://beebom.com/marvel-rivals-ultron-battle-matrix-protocol-auto-chess-mode-explained/amp/)
- [Official, 2019] HS BG agency is board position between rounds, for Poisonous, Deathrattle and Divine Shield. Source: [Blizzard](https://hearthstone.blizzard.com/en-us/news/23156373)
- [Official, designer opinion, 2019] Riot judged disables by whether they were "predictable and could be played around" (Zephyr good; old Demon, which could drain all mana in one hit, bad). Source: [Riot, Set 1 Learnings](https://teamfighttactics.leagueoflegends.com/en-us/news/dev/dev-tft-set-1-learnings)

### Inferences
- Where mid fight input exists it is small and limited: the damage action is once per round (Marvel Rivals), or spells are tied to a hero who can die (Hero's Hour). The automatic fight stays the main event.
- Mechabellum gets decision density with no in fight input at all: decisions happen between rounds, against an enemy army that carries over (per the Japanese press summary). Reading the opponent is the decision.
- For Kindred Siege's "one order spam wins" problem, these games suggest three levers, all my inference: limit how often an order can fire per fight (not just its point cost), tie orders to something that can be lost, and make each order's effect predictable and counterable in Riot's Zephyr sense so the enemy setup changes which order is right.

### Gaps
- No GDC talk or interview found where Riot, Valve, Blizzard, Tempo or Game River explain why mid fight input was cut or kept. Mortdog's reaction to the Marvel Rivals mode could not be read (page returned 403). No source quantified engagement or win rate effects of mid fight input.

## 6. Replay, spectate and combat log features

### Takeaway
Official replays are rare. Mechabellum has them (with take over), Underlords added them in 2019 early access, TFT added spectating in patch 14.1 (Jan 2024) but no replays I could find, Backpack Battles shows a post fight log of every action with stats, and The Bazaar's replays come from a third party tracker. Replays built from recorded inputs break across balance patches, as seen in another strategy game.

### Cited Findings
- [Official, 2025] Mechabellum replays let you take control of past battles. Source: [Dreamhaven press release](https://pressreleases.triplepointpr.com/2025/03/25/dreamhaven-announces-partnership-with-game-river-to-publish-tactical-auto-battler-mechabellum/). [Official, search summary only] a hotfix note reads "Fixed an issue where replays would not play correctly." Source: [PatchBot, Mechabellum](https://patchbot.io/games/mechabellum)
- [Wiki, Wikipedia] Underlords early access added a replay system. Source: [Wikipedia](https://en.wikipedia.org/wiki/Dota_Underlords). [Press, Aug 2019, search summary only] at that point replays were still listed as missing; the August update 213 fixed spectator issues and let spectators see party members' shops; a Combat Stats Kills panel credits summons' kills to the summoner. Source: [PCGamesN, Underlords update](https://www.pcgamesn.com/dota-underlords/update). These fit if replays arrived between Aug 2019 and Feb 2020.
- [Press, search summary only] TFT spectator mode shipped in patch 14.1 on 10 Jan 2024, after a PBE test that began 6 Dec 2023. Source: [SI, TFT patch 14.1](https://www.si.com/esports/tft/patch-notes-14-1)
- [Press, 13 Mar 2024, my translation] Backpack Battles: at the end of a fight you can view a full log of all actions with statistics: which weapons dealt how much damage, when stamina ran out, when effects triggered. Fights can be paused and fast forwarded. Source: [PC Games](https://www.pcgames.de/Indie-Games-Thema-239276/Specials/Backpack-Battles-Builds-Early-Access-Inventar-getestet-Auto-Battler-1442869/2/)
- [Community tool, search summary only] BazaarDB tracker saves each fight and offers tick by tick replays plus run timelines and win rates. Source: [BazaarDB tracker](https://bazaardb.gg/tracker)
- [Community tool, 2020] Bob's Buddy works in Hearthstone's Spectator Mode. Source: [HSReplay](https://articles.hsreplay.net/2020/04/24/introducing-bobs-buddy/)
- [Player explanation, other genre, search summary only] Company of Heroes 2 replays are a log of commands, events and random outcomes that the game reruns; minor builds show sync errors and big balance changes make old replays misbehave or crash. Source: [COH2.org forum](https://www.coh2.org/topic/29774/how-to-make-old-replays-work/post/307165)

### Inferences
- Kindred Siege's seed plus orders replay is the same model as COH2's command log, so a replay needs the sim version stored with it or it will drift after balance changes.
- Backpack Battles' per item damage and timing log is the cheapest "why did I lose" tool in this set, and it serves the same job players use Bob's Buddy and BazaarDB for.

### Gaps
- No official replay or combat log found for Super Auto Pets, HS Battlegrounds or The Bazaar. TFT's post combat damage chart was not looked up. A search summary credited MetaTFT with saying TFT lets you spectate players who are not your friends, but I could not find that page, so it is left out of the findings. Search budget ran out before checking these.
