# Readability, cognitive load, onboarding and game feel in fast squad combat

Notes compiled 25 Oct 2026 for the Kindred Siege auto battle (2.5D pixel art, 5 or 6 fighters per side, playtest measured about 7 notable events a second, peak 28 in one second, text labels on every fighter, enemy using the same combo systems, owner: "i didnt know what was going on at all"). Every finding carries a link. Anything I worked out myself sits under Inferences or is marked (inferred). Where a claim came only from a search result summary and the page itself was not opened, it says "via search summary".

## 1. Attention science: how many things can a player track at once?

### Takeaway
People can follow about 4 moving things at once (as few as 1 when fast and crowded, up to 8 when slow), hold about 4 objects in visual working memory, and often miss a second event that lands within about 200 to 500 ms of the first. A fight with about 7 notable events a second, a peak of 28, and a word label on each of 10 to 12 fighters sits far past all three limits, so "I didn't know what was going on" is the expected human result, not a player failing (that last point is inferred).

### Cited Findings

#### Multiple object tracking (MOT)
- Pylyshyn and Storm (1988, Spatial Vision 3(3)), abstract via search summary: observers could track a subset of up to 5 targets in a field of 10 identical objects moving at random. They proposed a small pool of parallel pointers ("FINSTs"), and Pylyshyn's later theory puts the pool at around 5. [Pylyshyn and Storm 1988 (PDF)](https://ruccs.rutgers.edu/images/personal-zenon-pylyshyn/docs/storm88.pdf); [Wikipedia, Visual indexing theory](https://en.wikipedia.org/wiki/Visual_indexing_theory)
- Alvarez and Franconeri (2007, Journal of Vision 7(13)): earlier experiments put the maximum at about 4 moving objects, but capacity is not fixed. At slow speeds people tracked up to 8 objects; at fast speeds only 1 (abstract, via search summary). [Alvarez and Franconeri 2007, Kellogg record](https://www.kellogg.northwestern.edu/faculty/research/detail/2007/how-many-objects-can-you-track-evidence-for-a?p=1)
- Later work by Franconeri and colleagues (via search summary) argues speed limits tracking mainly through crowding (fast objects pass close to each other more often), not speed as such. [Franconeri et al., Psychonomic Bulletin and Review (PDF)](https://visualcognition.psych.northwestern.edu/publications/Franconeri_etal_MOT_PBR.pdf)
- Field summaries (via search summary) note the trackable number varies widely with display settings and accuracy falls gradually as targets are added, rather than hitting a hard wall. [Wikipedia, Multiple object tracking](https://en.wikipedia.org/wiki/Multiple_object_tracking)
- Green and Bavelier (2006, Cognition 101(1)), abstract via search summary: action video game players tracked approximately two more items than non players in MOT, and could enumerate about two more briefly flashed items. Training non players on an action game produced the gain, so the link is causal. [Green and Bavelier 2006, Geneva archive](https://archive-ouverte.unige.ch/unige:153732)

#### Visual working memory
- Luck and Vogel (1997, Nature 390), abstract via search summary: people can keep information about "only four colours or orientations in visual working memory at one time". Objects defined by 4 features were kept as well as single feature objects, so the unit of storage is the whole object, not the feature. [Luck and Vogel 1997, Nature](https://www.nature.com/articles/36846)

#### Time limit between events: the attentional blink
- Raymond, Shapiro and Arnell (1992, Journal of Experimental Psychology: Human Perception and Performance 18) found the attentional blink (via search summary of reviews): after noticing one target, people often fail to report a second target that appears within about 200 to 500 ms. A second target that comes straight after the first ("lag 1") is often spared. The effect has been found with letters, words, faces, colours, scenes, sounds and touch. The 500 ms edge is approximate and depends on the task. [Attentional blink review, PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC2915904)

#### Change blindness and inattentional blindness
- Rensink, O'Regan and Clark (1997, Psychological Science 8), via search summary of the paper and its precursor abstracts: in the flicker method (image shown 240 ms, blank 80 ms, then the changed image), large changes in real scenes were hard to detect even when repeated many times and observers were told to expect them, yet easy to find once a valid verbal cue pointed at them. Their conclusion: a change is only seen if attention is on the changing object. [Rensink et al. 1997 (PDF)](https://homepage.villanova.edu/diego.fernandezduque/Teaching/CognitivePsychology/Lectures_and_Labs/s3Attention/sReading/Rensink_etal_97.pdf); [Rensink, 1996 abstract](https://www2.psych.ubc.ca/~rensink/publications/abs.96.2.html)
- Simons and Chabris (1999), via search summary: about half of viewers who were counting basketball passes missed a person in a gorilla suit who walked into the scene, faced the camera and thumped their chest. [Smithsonian, Simons' own account](https://www.smithsonianmag.com/science-nature/but-did-you-see-the-gorilla-the-problem-with-inattentional-blindness-17339778/?no-ist)

#### Reading labels next to busy sprites: crowding
- Pelli and Tillman (2008, Nature Neuroscience 11), abstract via search summary: recognition breaks down when objects sit too close together ("crowding"); the spacing needed grows in proportion to distance from where the eye is pointed, and is weaker between dissimilar objects. "Observers cannot recognize objects outside of this window and its size limits the speed of reading and search." [Pelli and Tillman 2008, PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC2772078)

#### How game UX and game designers apply this
- Celia Hodent (author of The Gamer's Brain), in her talk on Fortnite's UX, reduces player cognition to three slides: "Our PERCEPTION is subjective", "Our MEMORY is fallible" with the rule "Reduce memory load", and "Our ATTENTION is scarce" with the rule "Avoid multitasking". Her Fortnite example: "We can hardly pay attention to tutorial texts when busy killing zombies". She also demos change blindness on stage. [Hodent, The UX of Fortnite, Interaction 19 slides](https://interaction19.ixda.org/download/talk-the-ux-of-fortnite-celia-hodent.pdf)
- A German write up of her talk adds that cognitive load should be spread out and moderated, that demanding attention only makes sense once players have mastered the basics, and that a HUD helps by keeping information visible so players don't have to memorise it (secondary summary, via search summary). [ComputerBase, July 2019](https://www.computerbase.de/2019-07/interface-design-fortnite/)
- id Software capped simultaneous threats in DOOM (2016) with a token system: "each type of attack" "has a limited number of tokens available at a given point in time"; "A demon needs to request a token to make their attack, then release the token back to the system after use"; each difficulty has its own token counts, "ultimately limiting how many demons can be attacking at any point in time". Demons can steal tokens from each other. No counts are published. [Tommy Thompson, Cyber Demons: The AI of DOOM 2016, Game Developer, 6 Aug 2018](https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-)

### Inferences
- 7 events a second is one every ~143 ms; the 28 peak is one every ~36 ms. Both are well inside the 200 to 500 ms blink that follows each noticed event, so most of those events cannot be consciously registered. A rough ceiling for a watcher is about 2 to 4 noticeable events a second, and fewer when the events have to be read. (inferred from the attentional blink; this is not a measured game number)
- Two squads of 5 or 6 means 10 to 12 moving fighters. MOT says a typical viewer follows about 4, more if movement is slow, fewer if fast and crowded. So at best a viewer follows 3 or 4 of their own fighters, and cannot also follow the enemy squad's combo chains at the same moment. (inferred)
- Word labels (GUARD, SNARE, BURN 3, FLARE, BRAND 1.5S, INCOMING) are the worst format for this: each needs the eye pointed straight at it, crowding makes small words beside busy sprites unreadable off centre, and each label's meaning has to be held in working memory, which holds about 4 objects. (inferred from Pelli and Tillman, Luck and Vogel, Hodent)
- Change blindness means a status flipping on a fighter the viewer is not looking at will usually be missed. State changes that matter need a short transient cue (flash, pop, motion, sound) that pulls the eye, and there must be few of them or they cancel each other out. (inferred)
- DOOM's token idea maps onto the sim: cap how many notable actions (combo finishers, status applications, telegraphed attacks) can be live per side at once, as an authoritative rule, not only as a display filter. This would be a sim rule change. (inferred)
- Lag 1 sparing suggests bundling: a hit, its status and its number that happen in the same beat should be shown as one combined event, not three separate pops. (inferred)
- Green and Bavelier's result means a practised player may handle about 2 more items, but a new player will not; the first fights must be designed for the lower number. (inferred)

### Gaps
- No peer reviewed study found that measures how many simultaneous threats players can process inside a real game; MOT, working memory and attentional blink are lab proxies.
- The Alvarez and Franconeri PDF refused the connection; their numbers here come from the abstract as quoted in search results.
- No game specific study found that compares icons against text labels for status effects in fast combat.

## 2. How auto battlers and busy games control visual and information noise

### Takeaway
The clearest written rules are Riot's: how much attention an effect grabs must match how much it matters, the most important moment on screen must be the loudest, noise stays minimal, and visuals must match hitboxes. MMOs add a hierarchy by owner (your own effects and enemy effects at full strength, other friendly effects reduced), esports add fixed team colours carried into every effect, and bullet heaven games give toggles for damage numbers, flashes and shake. No public rulebook specific to TFT clarity was found.

### Cited Findings

#### Riot, League of Legends: "Clarity in League" (12 March 2021)
- Three goals: "gameplay should be clearly conveyed"; the most important moment at any time should draw the most attention, both inside one champion's kit and between champions and other elements such as item effects; "noise should be kept minimal", for both audio and visual clutter. Big gameplay spells can be noisy "to a degree". [Riot, Clarity in League](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/)
- Same article, via search summary: visual noise adds up quickly and can make it difficult to react properly in teamfights. [Riot, Clarity in League](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/)
- Core rule: "the amount of attention an ability grabs should match its level of importance". Four things raise importance: higher damage; hard or long crowd control; dodgeability ("If something is dodgeable, it should draw more attention"); and spells that drastically change play. [same](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/)
- Hitboxes should match the visual effects; mismatch is named a major source of frustration. [same](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/)
- "Projectiles should convey the direction they're traveling, even without motion" (for example by blurring the tail end). [same](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/)
- Silhouette is "the single most important thing for champion recognition". A unique primary feature should never change, even in skins; the silhouette should show the source of power (a bow means ranged) and the facing direction; size hints at role. Every skin's abilities must be at least as clear as the base skin. [same](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/)

#### Riot, Teamfight Tactics
- On two giant summon units (Baron and a second giant summon): "Right off the bat, we knew we had to make these units stand out much more than any other." Switching the second unit's attack to a laser "allowed us to take a ton of screen real estate" "without actually diluting gameplay clarity". Baron's arrival is staged with a roar and a large area stun. So the rare, decisive unit gets the biggest footprint. [Riot, Talking Tactics dev post, 17 Oct 2023](https://teamfighttactics.leagueoflegends.com/en-gb/news/dev/talking-tactics-baron-versus-t-hex/)
- A Riot TFT gameplay VFX job listing asks artists to balance visual satisfaction and gameplay clarity and to make sure effects accurately represent a given action or state (job ad, via search summary). [Built In job listing](https://builtin.com/job/vfx-artist-ii-gameplay-teamfight-tactics/3671790)
- Riot's TFT VFX video says effects lean on shape language, colour and timing (via search summary). [Riot, Super Art Power Hour ep. 6](https://www.riotgames.com/en/news/crafting-visual-effects-in-teamfight-tactics-super-art-power-hour-ep-6)
- TFT has a post combat recap; patch 12.6 coverage lists Tab to toggle it and a key that cycles damage dealt, damage taken and healing (via search summary). [GameWatcher, TFT patch 12.6](https://www.gamewatcher.com/teamfight-tactics-patch-12-6)

#### MMOs: who owns an effect decides how loud it is
- World of Warcraft patch 11.2 (via search summary of Icy Veins) reworked "Spell Density" to act only on other friendly players in your party or raid, with levels Everything, Reduced and Essential. A reduced visual can be skipped or drawn less prominently (more transparent, smaller, or a different visual). "Your own visuals, NPC spells, and enemy player spells will always be displayed at full intensity, regardless of Spell Density settings." [Icy Veins, Patch 11.2 Spell Density](https://www.icy-veins.com/wow/news/too-many-spells-on-screen-patch-11-2-fixes-that-with-one-simple-setting/)
- At launch the setting did nothing on live servers; Blizzard confirmed a bug and promised a hotfix (via search summary). [Icy Veins, bug report](https://www.icy-veins.com/wow/news/less-visual-clutter-spell-density-feature-bugged-fix-coming/)
- Final Fantasy XIV sets battle effects separately for yourself, party members and others (plus enemy players in PvP): Show All, Show Limited, Show None, also via the /bfx command (via search summary). [FFXIV wiki, text commands](https://ffxiv.consolegameswiki.com/wiki/Configuration_text_commands); [Square Enix UI guide FAQ](https://na.finalfantasyxiv.com/uiguide/faq/faq-other/setting_bteffect.html)

#### Bullet heaven: Vampire Survivors
- Options include Damage Numbers on or off, Flashing VFX on or off (a 2023 patch made the pentagram flash obey it), Weapons ScreenShake, and Disable blood. No opacity slider for effects is listed. [Vampire Survivors wiki, Options](https://vampire.survivors.wiki/w/Options)

#### ARPG players: Path of Exile 2
- Threads on the official forum say skill effects create clutter that can hide boss mechanics, telegraphs and ground damage, and ask for an effects slider and a death recap ("what killed me?"). This is player feedback and anecdotal (via search summary). [PoE forum, The VISUAL CLARITY Issue](https://www.pathofexile.com/forum/view-thread/3906258); [PoE forum, feedback thread](https://fr.pathofexile.com/forum/view-thread/3812470)

#### Supergiant (critique only)
- A blog critique (via search summary) argues Hades II bosses use arena wide danger zones telegraphed by regions of the floor turning red "usually independently of the boss's positions", which hurts legibility (opinion, not Supergiant). [Hades II Has a Legibility Problem](https://nex-3.com/blog/hades-ii-has-a-legibility-problem/)

#### Esports: one colour per team in every effect
- Overwatch, 2017: casual viewers struggled to tell players and teams apart, and hero explosions and projectiles made the action hard to follow. Fix: team uniforms with home and away palettes that also tint hero explosions and projectiles, plus a top down map and a smart camera. [Inven Global, Kaplan on the spectator issue, 26 Oct 2017](https://www.invenglobal.com/articles/3325/jeff-kaplan-addresses-the-spectator-issue-in-overwatch-responds-with-team-uniforms-top-down-map-and-more)
- Before the fix, team colours flipped between blue and red depending on whose view the broadcast showed (via search summary). [Prototypr, Overwatch League UI](https://blog.prototypr.io/how-overwatch-leagues-ui-is-an-esports-game-changer-9db218c0d466)

#### Screen layout: Zach Gage, "Building Games That Can Be Understood at a Glance" (GDC 2018)
- Design the screen as three reads: an instant read (eye catching theme and core appeal), a secondary read (important information that is not obvious at first glance, for example Into the Breach's turn counter and terrain), and a detail read (finer rules for those who look closer). His analogy is a concert poster: artist name biggest, date and venue next, small print last. [Gigazine report of the talk](https://gigazine.net/gsc_news/en/20180501-game-design-subway-legibility); [GDC Vault listing](https://www.gdcvault.com/play/1024925/Building-Games-That-Can-Be)
- Hearthstone uses animation and colour change to bring up needed information at the right size only when it matters, so the screen doesn't overload. Slay the Spire is his counterexample: its many small icons and status numbers crowd the screen, which makes complex games hard to read for stream viewers. [Gigazine report](https://gigazine.net/gsc_news/en/20180501-game-design-subway-legibility)

#### Juice against clarity
- Pichlmair and Johansen's survey of over 200 game feel sources: "There is an adequacy to juice." Too much makes it hard to learn which parts of the interaction matter mechanically, because "decoding the actual system behind the game is cumbersome, when the whole screen is filled with wobbling particle effects". Also: "If the feedback of an event is suggesting something different to the player than the rules of the game, then there is a mismatch" and the game can feel frustrating or hard to learn. [Pichlmair and Johansen 2020, arXiv 2011.09201](https://arxiv.org/abs/2011.09201)

#### Layering one sign, then letting it land
- An analysis of Hearthstone's feedback splits each big moment into Preparation ("build anticipation & let the player know that something is about to happen"), Emphasis (the peak; Hearthstone "doesn't shy away from going 300%") and Aftermath (room to process, which keeps consecutive actions from chaining too abruptly). "Most signs & feedbacks have several layers, and the number one mistake is picking only one." [JB Oger, The Arcade Artificer, 1 Feb 2023](https://jboger.substack.com/p/hearthstone)

### Inferences
- League's four factors give a sorting key for every Kindred Siege event: damage, crowd control, whether the player can answer it, and how much it changes the fight. Everything below the top tier should be smaller, dimmer and shorter. (inferred)
- In Kindred Siege the "dodgeable" thing is anything the player can answer (a parry window, a directive). By League's rule that should be the loudest thing on screen, and routine auto hits and status ticks the quietest. (inferred)
- WoW's split suggests: enemy threats stay at full strength, the player's own decisive moments stay at full strength, and routine actions from both squads are reduced. FFXIV's self, party and others split is a ready model for an options menu. (inferred)
- One fixed colour per side carried into every effect (Overwatch) answers "whose effect is this?" with no text at all, and the colours must never swap with camera or perspective. (inferred)
- Gage's three reads map onto the fight screen: instant read = who is alive and who is winning; secondary read = what is about to hit (telegraphs); detail read = numbers, stacks and timers, shown on pause, hover or in a recap rather than floating over every fighter. (inferred)
- A recap after the fight (TFT) or a "what killed me" panel (what PoE players ask for) moves the "why" out of the live fight, where nobody can read it anyway. (inferred)

### Gaps
- No Riot document specific to TFT clarity rules (ally against enemy effects, effect budgets per unit) was found after several searches; League's article is the closest primary source.
- Nothing usable found from Supercell (Clash Royale, Brawl Stars), Mechabellum, Diablo, or Supergiant on their own clarity rules.
- A search summary said Poncle declined to add a weapon opacity slider to Vampire Survivors; not verified, so not used as a finding.
- Whether WoW's 11.2 Spell Density fix shipped was not checked.

## 3. Onboarding: introducing one mechanic at a time and layering them

### Takeaway
Nintendo, Valve and Ironhide all show one new thing on its own, then develop it, twist it and test it, and they add steps whenever playtests show too much at once. Large scale data says tutorials only pay off in complex games, and text read during action doesn't land. Roguelikes spread systems across runs and use failure as the moment that delivers new content.

### Cited Findings
- Koichi Hayashida (director, Super Mario 3D Land) on kishotenketsu: "First, you have to learn how to use that gameplay mechanic"; then the stage "will offer you a slightly more complicated scenario in which you have to use it"; then "something crazy happens that makes you think about it in a way you weren't expecting"; then "you get to demonstrate, finally, what sort of mastery you've gained over it". Each level has "a clear concept in the beginning" that is "carried through absolutely all the way". [Christian Nutt, The secret to Mario level design, Game Developer, 13 Apr 2012](https://www.gamedeveloper.com/design/the-secret-to-i-mario-i-level-design)
- Portal developer commentary. Robin Walker: "Portal is effectively an extended player training exercise", with gating so that a solid understanding of key concepts is required for success. Chris Chin: "Playtesting revealed that this puzzle introduced too many new concepts at once", so two simpler chambers were added before it. Kim Swift: "Overwhelmed players tend not to digest new information." Chris Chin: "Player training doesn't always stick, especially after the introduction of a big, new concept", so flinging is taught again later. Garret Rickey: "Portal momentum ended up being the hardest concept to convey", so it was introduced step by step. Jason Brashill: "We introduced a mandatory pause in the action" (a "gate") so players notice the first blue portal. Kim Swift: the first portal is placed so players "will invariably see themselves" through it. [Combine OverWiki, Portal developer commentary](https://combineoverwiki.net/wiki/Developer_commentary/Portal)
- Kingdom Rush, level by level (a community analysis on Game Developer, not Ironhide): level 1 brings 2 enemy types and the 4 basic towers; level 2 adds only 1 new enemy type (Wulves) plus an opening tip; level 4 uses an advice box to teach that cannons ignore half of armour; level 5 is a "Breather Level"; level 6 introduces flyers that soldiers can't block and cannons can't target; the most unique towers come last. Stated principles: "Don't be afraid to have a Breather Level." and "Introduce the most unique characters (or towers in KR's case) later than anything else." [David Harlow, Kingdom Rush: the wonderful campaign level design, Game Developer, 25 Oct 2013](https://gamedeveloper.com/design/kingdom-rush---the-wonderful-campaign-level-design)
- Andersen et al. (CHI 2012), abstract via search summary: 8 tutorial variants across 3 games tested on over 45,000 players. "Although tutorials increased play time by as much as 29% in the most complex game, they did not significantly improve player engagement in the two simpler games." The authors suggest tutorial investment may not be justified when mechanics can be discovered by experimenting. [Andersen et al., The Impact of Tutorials on Games of Varying Complexity](https://grail.cs.washington.edu/projects/game-abtesting/chi2012/)
- Hodent: "We can hardly pay attention to tutorial texts when busy killing zombies"; "Avoid multitasking"; "Reduce memory load"; and onboarding sits beside difficulty curve, pacing and learning curve in her model of game flow. [Hodent, The UX of Fortnite slides](https://interaction19.ixda.org/download/talk-the-ux-of-fortnite-celia-hodent.pdf)
- Into the Breach (Justin Ma, Subset Games): "This lead us to use of telegraphed enemy attacks as a core mechanic"; the team wanted "less randomness than FTL"; "there's no random chance in your attack options", so the game "starts to feel like a puzzle"; and it matters that "it's very clear how or why you failed so that you can feel like you can improve". [Joel Couture, Road to the IGF: Into the Breach, Game Developer, 23 Feb 2018](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-)
- Slay the Spire: PC Gamer's design award credits much of the game to how transparent it makes information and enemy intents (reviewer view, via search summary). [PC Gamer, Best Design 2019](https://www.pcgamer.com/uk/best-design-2019-slay-the-spire). The sequel shows several intents side by side instead of stacking them (guide, via search summary). [untapped.gg, reading enemy intent in Slay the Spire 2](https://sts2.untapped.gg/guides/how-to-read-enemy-intent)
- Hades (via search summary): Greg Kasavin says "dying is how you advance much of the story in Hades, so we didn't want a traditional 'easy mode'", and wanted struggling players to keep progressing through the story. [GameSpot](https://gamespot.com/articles/hades-writer-explains-script-philosophy-after-player-discovers-incredible-detail/1100-6482650/). Its Pact of Punishment (difficulty modifiers) only becomes available after the first successful escape (guide, via search summary; older guides say after defeating Hades). [Prima Games](https://primagames.com/gaming/hades-guide-pact-of-punishment-modifiers-heat)

### Inferences
- For Kindred Siege: one new status, label or combo system per early fight (or per early run). Its first appearance should be in a fight where only it happens; then it pairs with one known system; then a twist; then a test. Follow each new system with a breather fight. (inferred from Hayashida, Portal, Kingdom Rush)
- The enemy using the same combo systems is a good teaching tool only if staged: early enemies use none or one system, and the enemy gains a system only after the player has seen it work on their own side. (inferred)
- Andersen's result puts Kindred Siege in the "complex" class (many statuses, combos, directives, parry), which is exactly where staged teaching was measured to matter. (inferred)
- Hades and Slay the Spire suggest the run end or death screen is where the next system gets introduced, so new rules arrive in a calm moment, not mid fight. (inferred)
- Portal's "gate" (a forced pause so the player notices one thing) is the same idea as freezing the fight the first time a new status appears and pointing at it once. (inferred)

### Gaps
- No developer source found on how Hades stages its systems (Mirror of Night, keepsakes, Chaos, Daedalus hammers) across early runs; the Hades wiki pages refused the fetch.
- No developer statement found on Slay the Spire's intent design or its unlock pacing.
- Mark Brown's Game Maker's Toolkit video on Super Mario 3D World's 4 step design was not fetched; the Hayashida interview above is the primary source.
- Andersen et al.: whether context sensitive (just in time) help beat up front text was not verified here.
- The Kingdom Rush breakdown is a community blog, not an Ironhide statement.

## 4. Game feel: hit stop, shake, flash, knockback and sound, with numbers

### Takeaway
Hit stop is the best researched impact tool: both fighters freeze, longer for bigger hits, with a cap, and Sakurai keeps it shorter when more players share the screen. Shipped numbers run from 50 ms (Celeste's dash) through roughly 100 to 250 ms for typical Smash hits up to a 500 ms cap. Experiments warn that more juice is not better: extreme juice scored worse than medium or high, and exaggeration not tied to the player's success lowered motivation.

### Cited Findings

#### Masahiro Sakurai on hit stop (Famitsu column, fan translation)
- Hit stop freezes both characters when a hit lands to emphasise impact; "both you and your opponent freeze for the exact same amount of time". [Source Gaming, Sakurai's Famitsu column vol. 490](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/)
- "The more damage an attack inflicts, the longer the hitstop period", with a hard cap; per attack modifiers change it (Marth's sword tip gets more, the edge less; projectiles less; electric attacks more). [same](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/)
- He keeps hit stop shorter than he would like in Smash because in free for alls "that creates a chance for a third player to move in and strike". [same](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/)
- During hit stop the hit character vibrates: side to side on the ground, up and down in the air, shaking less as the stop goes on, with strength scaled to camera distance; "the characters vibrate visually, but keep the hurtbox locations static". The change from first flinch frame to hurt pose takes "four frames" inside the stop. [same](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/)
- His video on stop effects is reported to cover zooming in and slowing down when finishing someone off to build tension (via search summary of Destructoid). [Destructoid](https://www.destructoid.com/smash-boss-sakurais-big-surprise-is-a-new-youtube-channel/)

#### Smash Bros. hitlag formulas (frames, d = damage)
- Melee: floor(d / 3 + 3). Brawl and Smash 4: floor(d × 0.3846154 + 5). Ultimate: floor(d × 0.65 + 6). Multipliers: electric 1.5, crouch cancel 0.67, shielding 0.67; Ultimate also scales by player count, from 1 in 2 player matches down to 0.75 at 8 players. Caps: 20 frames in Melee, 30 frames from Brawl on. Example: a 15% move gives 8 frames (Melee), 10 (Brawl, Smash 4) and 15 (Ultimate). The attacker freezes on the frame the hit lands; the victim freezes in the first frame of its flinch while shaking. [SSBWiki, Hitlag](https://www.ssbwiki.com/Hitlag)

#### Celeste's open source player code
- `Celeste.Freeze(.05f)` (50 ms) in DashBegin and RedDashBegin; `Celeste.Freeze(.1f)` (100 ms) in ExplodeLaunch; `Celeste.Freeze(0.01f)` when smashing a crystal spinner; `DirectionalShake(DashDir, .2f)` (0.2 s, along the dash direction) in the dash coroutine; strong rumble on dash. Other constants: coyote time `JumpGraceTime = 0.1f`, `DashTime = .15f`, `DashCooldown = .2f`. [NoelFB/Celeste, Player.cs](https://raw.githubusercontent.com/NoelFB/Celeste/master/Source/Player/Player.cs)

#### Other duration figures
- An IEICE technical report (2023) on 3D action games found the best hit stop around 0.1 to 0.2 s and slow motion around 0.2 to 0.4 s (via search summary; the page refused the fetch). [IEICE technical report](https://ken.ieice.org/ken/paper/20230315OCSt/eng/)
- A community Godot tutorial (via search summary) suggests 0.04 to 0.08 s for light hits and 0.10 to 0.18 s for heavy hits (community advice, low authority). [Godot forum tutorial](https://forum.godotengine.org/t/tutorial-hit-stop-and-screen-shake-in-godot-4-autoload-trauma-camera-mit/145240)

#### Vlambeer and "Juice it or lose it"
- Jan Willem Nijman's "The Art of Screenshake" (2013) builds a plain shooter up one effect at a time; a recreation lists hit reaction, enemy knockback, permanence (debris and corpses stay), camera lerp, screen shake, player knockback and hit pause among the steps (via search summary). [DK Liao, Art of Screenshake recreation devlog](https://dkliao.itch.io/the-art-of-screenshake-recreation/devlog)
- Martin Jonasson and Petri Purho, "Juice it or lose it" (2012), via search summary: a Breakout prototype brought to life with colour, tweening and easing, squash and stretch on events, wobble on bounce, sound and music, screen shake and particles. [RPG Playground summary](https://rpgplayground.com/research-making-a-juicy-game/)

#### The game feel survey (Pichlmair and Johansen 2020)
- Hit stop is a localised pause of one or more objects; freeze frames halt the whole game; Kratos' axe in God of War freezes when it hits. "It is usual to layer several of them depending on the significance and kind of event being communicated." Slow motion "helps to communicate events that would otherwise evolve too fast to be fully perceived by the player". Screen shake along "a semantically significant direction" communicates more than random shake. Colour flashing communicates state changes, and flashing a destroyed object before it disappears gives persistence. Audio can report events that happen off screen. Instant replays, often slowed, mark pivotal moments. [Pichlmair and Johansen 2020, arXiv 2011.09201](https://arxiv.org/abs/2011.09201)

#### Experiments on how much juice
- Kao (2020, Entertainment Computing 34): four versions of the same action RPG (no, medium, high and extreme juice). Medium and high beat both extreme and none on player experience, intrinsic motivation, play time and in game performance; the takeaway is that juice must fit the situation. [Pichlmair and Johansen survey, reporting Kao](https://arxiv.org/abs/2011.09201); [Improbable Research summary](https://improbable.com/tag/rpg/)
- Kao, Ballou, Gerling, Breitsohl and Deterding (CHI 2024), preregistered, 1,699 participants, custom action RPG: success dependence (feedback tied to whether you succeeded) raised effectance, competence and curiosity; amplification unexpectedly lowered them, possibly by undermining players' sense of agency. Curiosity was the strongest predictor of enjoyment and the only predictor of play time. Juicy feel guidance depends on clear links between action and outcome and on graded success. [Imperial College record](https://spiral.imperial.ac.uk/entities/publication/253c32f5-d124-4a23-88b5-9aaaf114608d)

### Inferences
- At 60 fps one frame is about 16.7 ms, so the Smash 15% example is about 133 ms (Melee), 167 ms (Brawl, Smash 4) and 250 ms (Ultimate); the caps are about 333 ms and 500 ms. A 1 damage Ultimate hit is 6 frames (100 ms); a 10 damage hit is 12 frames (200 ms). (inferred arithmetic; SSBWiki does not state the frame rate)
- At 7 events a second, even a 50 ms global freeze per event stops the game 350 ms of every second; at the 28 peak it would need 1.4 s of freeze inside one second. So hit stop has to be local (only the two fighters involved), and whole screen freezes must be rationed to one big moment at a time. (inferred)
- Sakurai shortens hit stop as more players share the screen; with 10 to 12 fighters the same logic applies harder, and Ultimate's own player count multiplier (down to 0.75) is a shipped example of it. (inferred)
- Kao 2020 plus Kao 2024: put the biggest feedback on success the player caused (a landed parry, a directive paying off, a nerve break), and keep routine auto attacks at low juice. Juice on every routine hit is "amplification" without success dependence, the condition that lowered motivation. (inferred)
- Use Sakurai's vibrate in place on the hit sprite rather than moving hurtboxes, so presentation never changes sim positions. (inferred; fits the repo rule that presentation must not alter authoritative state)

### Gaps
- The exact hit pause ("sleep") duration Nijman uses in The Art of Screenshake was not verified; the talk video was not transcribed.
- No table of Street Fighter hit stop frames per attack strength was found.
- No Hades or Supergiant postmortem with hit stop or shake numbers was found.
- Kao 2020's original paper was not opened; its result is taken from the survey and a summary.

## 5. Spectator readability when the player mostly watches

### Takeaway
A watcher needs three things on screen: who is winning (alive counts, health per side), what is about to happen (telegraphs, intents, ultimate timers), and why something happened (the cause visible next to the effect in the moment, then a recap or replay). Games that show cause and effect one at a time read best: Hearthstone Battlegrounds resolves attacks one minion at a time, Into the Breach and Slay the Spire show enemy intent before it lands, and Overwatch added team colours, a map with ultimate timers and stun states, and kill replays.

### Cited Findings
- Reeves, Benford, O'Malley and Fraser (CHI 2005), via search summary, map spectator experiences by whether the performer's manipulations and the resulting effects are visible: expressive (both shown, the audience can follow the interaction), magical (effects shown, causes hidden), suspenseful (causes shown, effects withheld) and secretive (both hidden). [Reeves et al. 2005, Designing the Spectator Experience](https://eprints.nottingham.ac.uk/252/)
- Cheung and Huang (CHI 2011) argue StarCraft's appeal to watchers rests on information asymmetry between players and spectators, and that designers should choose carefully what to show viewers and when (secondary summary, via search summary). [Slashdot summary of Cheung and Huang](https://games.slashdot.org/story/11/05/16/1851224/why-people-watch-starcraft-instead-of-playing)
- Hearthstone Battlegrounds combat runs with no input: "There is no player input on this part of the game." "Minions attack automatically, starting in sequence from the leftmost minion to the rightmost for each player"; the side with more minions attacks first; targets are random unless Taunt applies. [Hearthstone wiki, Battlegrounds](https://hearthstone.wiki.gg/wiki/Battlegrounds)
- Battlegrounds players pull both ways on combat speed (forum, 6 Oct 2024, anecdotal). One reports "entire turns are being skipped" and quotes a Blizzard post about aiming to "normalize the time when detecting FPS drops"; another writes "I would like the combat and animations to go faster in a general sense", because lobbies wait three or more minutes on some builds. [Blizzard forums, Battlegrounds Animation Issues](https://us.forums.blizzard.com/en/hearthstone/t/battlegrounds-animation-issues/135955)
- Overwatch's 2017 spectator package: a top down map showing every player's position, ultimate timers, and whether a hero is stunned or frozen; a smart camera to smooth "the jerky tendency of humans"; instant replays from any kill in the kill feed; team palettes that tint explosions and projectiles. [Inven Global, 26 Oct 2017](https://www.invenglobal.com/articles/3325/jeff-kaplan-addresses-the-spectator-issue-in-overwatch-responds-with-team-uniforms-top-down-map-and-more)
- A community piece says VALORANT's spectator HUD made it hard to tell which agents were alive or dead, with stats hidden behind a key (community view, via search summary). [The Spike](https://www.thespike.gg/valorant/news/the-community-experiments-with-custom-esports-huds/272)
- Into the Breach builds on telegraphed enemy attacks and wants it "very clear how or why you failed". [Game Developer, 23 Feb 2018](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-)
- TFT's post combat recap shows damage dealt, damage taken and healing per unit (patch 12.6 coverage, via search summary). [GameWatcher](https://www.gamewatcher.com/teamfight-tactics-patch-12-6)
- Zach Gage designs for people who only glance at the screen (over a shoulder on the subway, on streams) and calls out crowded icon and number displays as hard for viewers. [Gigazine report](https://gigazine.net/gsc_news/en/20180501-game-design-subway-legibility)
- Slow motion and replays help viewers perceive events that happen too fast and mark pivotal moments. [Pichlmair and Johansen 2020](https://arxiv.org/abs/2011.09201)
- Hearthstone's Aftermath beat gives room to process and stops consecutive actions chaining too abruptly. [JB Oger, 1 Feb 2023](https://jboger.substack.com/p/hearthstone)

### Inferences
- In Reeves' terms, a Kindred Siege fight is "expressive" only while the cause (which fighter, which combo) is readable next to the effect. At 7 to 28 events a second it turns "magical": effects with no readable causes, which matches "i didnt know what was going on at all". (inferred)
- The minimum watcher HUD: an always visible "who is winning" read (alive count and total health per side, team coloured); a "what is next" read (intent icon over any enemy about to fire a combo finisher, and over the target); and a "why" read after the fight (a recap of damage dealt, taken and healed per fighter plus the 3 to 5 key moments). (inferred from Overwatch, Into the Breach, Slay the Spire, TFT)
- Two ways to make cause and effect followable. Sim side: sequence or rate limit notable actions (Hearthstone style resolution, DOOM style tokens) so they cannot pile up. Presentation side: a spotlight queue that gives one notable event at a time the slow motion, zoom or freeze and an aftermath beat, while everything else plays small and local. The first is a rule change (authoritative timing) and belongs to the sim owner; the second must not change any sim result. (inferred)
- Information asymmetry (Cheung and Huang) suggests a suspense tool for a watched fight: show the viewer an enemy intent that the player's fighters will "answer" a beat later, so watching becomes predicting. (inferred)
- Watch time has a cost: Battlegrounds players complain about waiting through long combats, so readability cannot come only from slowing everything down; it has to come mostly from showing less. (inferred)

### Gaps
- The Cheung and Huang paper itself was not read; only a summary.
- No public source found on how TFT or Mechabellum design their combat camera or their "who is winning" display.
- No source found that gives a good event rate for watched auto battles; the numbers here come from attention research.

## 6. Do and do not lists a HUD and a combat sim can follow

### Takeaway
The sources agree on a short list: rank every effect by importance and let only the top one be loud, colour by team everywhere, keep enemy threats and your own decisive moments at full strength and quiet everything else, telegraph before you hit, cap simultaneous threats, put numbers and words in a recap rather than over fighters, use local hit stop scaled by damage with one global moment at a time, keep juice medium and tied to success, and teach one system per fight with breathers.

### Cited Findings

#### Do
- Match each effect's attention to its gameplay importance (damage, crowd control, dodgeability, impact). [Riot, Clarity in League](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/)
- Make the most important moment at any time the loudest thing on screen; let only big gameplay spells be noisy, and only "to a degree". [Riot, Clarity in League](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/)
- Give things the player can react to more attention ("If something is dodgeable, it should draw more attention"). [Riot, Clarity in League](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/)
- Make visuals match hitboxes and rules; feedback that suggests something the rules don't do feels frustrating and hard to learn. [Riot, Clarity in League](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/); [Pichlmair and Johansen 2020](https://arxiv.org/abs/2011.09201)
- Make projectiles show direction even when still; keep each unit's primary silhouette feature fixed. [Riot, Clarity in League](https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/)
- Give each team one colour palette and carry it into explosions and projectiles. [Inven Global, Overwatch](https://www.invenglobal.com/articles/3325/jeff-kaplan-addresses-the-spectator-issue-in-overwatch-responds-with-team-uniforms-top-down-map-and-more)
- Keep your own visuals and enemy visuals at full intensity and reduce other friendly visuals (WoW); or split settings into self, party and others (FFXIV). [Icy Veins, WoW 11.2](https://www.icy-veins.com/wow/news/too-many-spells-on-screen-patch-11-2-fixes-that-with-one-simple-setting/); [FFXIV wiki](https://ffxiv.consolegameswiki.com/wiki/Configuration_text_commands)
- Offer toggles for damage numbers, flashing effects and screen shake. [Vampire Survivors wiki, Options](https://vampire.survivors.wiki/w/Options)
- Telegraph enemy attacks and remove hidden randomness so failure is explainable. [Game Developer, Into the Breach](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-)
- Cap how many enemies can attack at once with tokens per attack type, tuned per difficulty. [Game Developer, DOOM AI](https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-)
- Lay out the screen in three reads (instant, secondary, detail) and show detail only when it matters. [Gigazine report of Zach Gage](https://gigazine.net/gsc_news/en/20180501-game-design-subway-legibility)
- Layer each important sign across animation, sound, effects and voice, with a preparation beat before and an aftermath beat after. [JB Oger](https://jboger.substack.com/p/hearthstone)
- Freeze attacker and target together, longer for bigger hits, with a cap; vibrate the hit sprite while hurtboxes stay put; shorten hit stop when more players share the screen. [Source Gaming, Sakurai](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/); [SSBWiki, Hitlag](https://www.ssbwiki.com/Hitlag)
- Shake along the direction of the hit, not at random. [Pichlmair and Johansen 2020](https://arxiv.org/abs/2011.09201); [Celeste Player.cs](https://raw.githubusercontent.com/NoelFB/Celeste/master/Source/Player/Player.cs)
- Use slow motion or replays for events that happen too fast to perceive. [Pichlmair and Johansen 2020](https://arxiv.org/abs/2011.09201)
- Keep juice at medium to high, not extreme, and tie it to success. [Pichlmair and Johansen reporting Kao 2020](https://arxiv.org/abs/2011.09201); [Kao et al. 2024](https://spiral.imperial.ac.uk/entities/publication/253c32f5-d124-4a23-88b5-9aaaf114608d)
- Teach one mechanic, develop it, twist it, then test mastery. [Game Developer, Hayashida](https://www.gamedeveloper.com/design/the-secret-to-i-mario-i-level-design)
- When a playtest shows too many concepts at once, add simpler steps before it, and reteach big concepts later. [Portal commentary](https://combineoverwiki.net/wiki/Developer_commentary/Portal)
- Add breather levels; introduce one new enemy type at a time early; save the most unique pieces for later. [Harlow, Kingdom Rush analysis](https://gamedeveloper.com/design/kingdom-rush---the-wonderful-campaign-level-design)
- Reduce memory load and avoid multitasking; keep needed information visible instead of memorised. [Hodent slides](https://interaction19.ixda.org/download/talk-the-ux-of-fortnite-celia-hodent.pdf); [ComputerBase](https://www.computerbase.de/2019-07/interface-design-fortnite/)
- Give a recap of damage dealt, taken and healed after the fight, and replays of kills. [GameWatcher, TFT 12.6](https://www.gamewatcher.com/teamfight-tactics-patch-12-6); [Inven Global, Overwatch](https://www.invenglobal.com/articles/3325/jeff-kaplan-addresses-the-spectator-issue-in-overwatch-responds-with-team-uniforms-top-down-map-and-more)
- Show at all times which units are alive or dead. [The Spike, VALORANT HUD](https://www.thespike.gg/valorant/news/the-community-experiments-with-custom-esports-huds/272)

#### Do not
- Do not let team colours change with perspective. [Prototypr, Overwatch League UI](https://blog.prototypr.io/how-overwatch-leagues-ui-is-an-esports-game-changer-9db218c0d466)
- Do not let explosions and projectiles bury the action or the telegraphs (Overwatch before 2017; PoE 2 player complaints). [Inven Global](https://www.invenglobal.com/articles/3325/jeff-kaplan-addresses-the-spectator-issue-in-overwatch-responds-with-team-uniforms-top-down-map-and-more); [PoE forum](https://www.pathofexile.com/forum/view-thread/3906258)
- Do not detach a telegraph from its source (danger zones that light up independently of the boss's position are criticised as illegible). [blog critique of Hades II](https://nex-3.com/blog/hades-ii-has-a-legibility-problem/)
- Do not crowd the screen with many small icons and status numbers. [Gigazine report of Zach Gage](https://gigazine.net/gsc_news/en/20180501-game-design-subway-legibility)
- Do not expect players to read text while fighting. [Hodent slides](https://interaction19.ixda.org/download/talk-the-ux-of-fortnite-celia-hodent.pdf)
- Do not put small items close together and expect them to be recognised away from the point of gaze (crowding). [Pelli and Tillman 2008](https://pmc.ncbi.nlm.nih.gov/articles/PMC2772078)
- Do not push juice to extreme, and do not exaggerate feedback that isn't tied to success. [Pichlmair and Johansen 2020](https://arxiv.org/abs/2011.09201); [Kao et al. 2024](https://spiral.imperial.ac.uk/entities/publication/253c32f5-d124-4a23-88b5-9aaaf114608d)
- Do not fill the whole screen with particles if players must still learn what matters mechanically. [Pichlmair and Johansen 2020](https://arxiv.org/abs/2011.09201)
- Do not introduce several new concepts at once. [Portal commentary](https://combineoverwiki.net/wiki/Developer_commentary/Portal)

### Inferences
Translated to the Kindred Siege fight (all inferred; ordering and numbers are my calls built from the sources above, not values from any source):
- Labels: drop the word labels from fighters during live combat. Replace them with a small fixed set of shapes, icons and team colours (status as a coloured ring or pip, telegraph as a ground marker that grows from the attacker toward the target). Words and exact numbers (BURN 3, BRAND 1.5S) move to pause, hover and the post fight recap.
- Event budget: present no more than about 2 to 4 notable events per second to the viewer. Everything else plays as quiet local feedback (small flash, small number or none). The sim can keep its own rate; the presentation decides what is notable.
- Importance order to test first: 1) anything the player can answer (parry window, directive); 2) enemy telegraphs (the INCOMING case); 3) kills and nerve breaks; 4) the player's own combo finishers; 5) status applications; 6) routine hits and ticks.
- One spotlight at a time: whole screen freeze, zoom or slow motion only for the single top event; anything else that wants a spotlight in the same window waits (preparation, emphasis, aftermath) or gets downgraded to local feedback.
- Hit stop: local to the two fighters involved, roughly 50 to 100 ms for normal hits and 100 to 200 ms for heavy hits, with a cap; vibrate the sprite, never move positions. Global freeze only for the spotlight. (Ranges drawn from Celeste, the Smash formulas and the IEICE figure.)
- Enemy symmetry: before an enemy combo finisher fires, show an intent icon over the enemy and a marker on its target (Into the Breach, Slay the Spire). In early runs the enemy uses fewer systems than the player and gains each one only after the player has seen it. A token style cap on simultaneous enemy finishers would be a sim rule and is the sim owner's call.
- Colour: one fixed colour per side in every effect, including hit sparks and projectiles; never swap with camera.
- Options: toggles for damage numbers, flashes and shake, plus an effects density setting split by own squad and enemy squad (FFXIV and WoW pattern).
- Watcher HUD: an always visible "who is winning" bar per side, a "what's next" intent layer, and a recap after each fight with the 3 to 5 key moments and damage dealt, taken and healed per fighter.
- Ownership note from the repo's own rules: sim rule changes (token caps, sequencing, enemy system gating) sit with the sim owner; HUD and presentation work sits with the frontend owner, and the frontend must not change combat results.

### Gaps
- None of the sources gives a numeric events per second budget for watched combat; the 2 to 4 per second figure is an inference from the attentional blink research and should be checked in a playtest.
- The importance order above is mine, built from League's four factors; no source ranks these exact event types.
- No source tested hit stop durations specifically for watched multi unit auto battles; the ranges are borrowed from player controlled action games.
