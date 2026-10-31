# Owner combat record: everything Jacob has said about combat, from the project's own files

Scope: local files only, read on 25 Oct 2026 up to the last line of HANDOFF_LOG.md (line 11714). Sources: all of DESIGN_LOCKS.md (668 lines), HANDOFF_LOG.md (lines 10800 to 11714 in full, plus targeted reads of earlier entries found by search), reports/ (Design recap, Nerve sync and unit kits, Squad synergy and the Hold load, Boss fight depth for chiefs, Auto battler loops and systems), Claude's memory files, and the header comments and constants of Assets/Scripts/Sim/Battle. Link labels: "DL n" is DESIGN_LOCKS.md line n, "HL n" is HANDOFF_LOG.md line n. Tags: **[JACOB]** his own words or decision; **[CLAUDE]** Claude's proposal or call; **[CODEX]** Codex's proposal, audit or paraphrase; **[BUILT rN]** in the sim behind campaign rules version N; **[NOT IN LOCKS]** decided in chat or the log but not yet written into DESIGN_LOCKS.md. Quotes keep his spelling.

## 1. Every reference Jacob named for combat, fights, bosses, synergy, pacing or feel, and what he likes about each

### Takeaway
His combat references fall into four groups: busy, funny, readable action (Kingdom Rush, "easy Pokémon", Darkest Dungeon as an auto battler); the parry and boss feel (Sekiro, World of Warcraft raid bosses, Devil May Cry's style rank, Hades' lives); visible squad synergy (Warpack, Northgard Battlegrounds, Time Walker, Mega Man Battle Network's stolen moves); and visible rival memory (Nemesis, MGS). Every reference is for one feeling or one mechanic, never a template to copy: he has rejected copies of a famous boss (DMC 3) and of WoW characters, and a "familiar-game mashup".

### Cited Findings

**Kingdom Rush (Ironhide)**
- Tone **[JACOB]**: "funny everywhere, in the spirit of Ironhide's Kingdom Rush; only the Sovereign is truly scary" (2 Oct). ([DL 81](../../DESIGN_LOCKS.md#L81))
- Pace **[JACOB]**: "Fight: auto battle. Order buttons refill during the fight so the player stays busy (Kingdom Rush)." The refill itself was killed on 13 Oct and replaced by earned points; the "keep the player busy" intent stands. ([DL 84](../../DESIGN_LOCKS.md#L84); [DL 287](../../DESIGN_LOCKS.md#L287); [Design recap 62](../../reports/Design%20recap.md#L62))
- **[JACOB]**, 4 Oct: "Kingdom Rush with Pokemon", nothing cut. ([HL 4637](../../HANDOFF_LOG.md#L4637))
- Sound **[JACOB]**: one reference per area, "Contra and Double Dragon for battles", "Kingdom Rush for hits". ([HL 10935](../../HANDOFF_LOG.md#L10935))
- Backgrounds **[JACOB]**, 21 Oct: layer 2 has "Kingdom Rush style events you click for an achievement". ([HL 10087](../../HANDOFF_LOG.md#L10087))
- Parked by him on 25 Oct: "Kingdom Rush style upgradeable spells". ([HL 11688](../../HANDOFF_LOG.md#L11688))

**Pokémon**
- Loop **[JACOB]**, 10 Oct: "The loop stays a roguelike and stays simple, 'like easy Pokémon': go in, fight a few times, beat the chief." ([DL 181](../../DESIGN_LOCKS.md#L181))
- The first tribe is the leader's "starter Pokémon" (8 Oct story). ([HL 5138](../../HANDOFF_LOG.md#L5138))
- For winning tribes: "Pokémon Legends: Arceus research tasks (varied feats per creature) and Shadow of Mordor's visual cues on returning enemies". ([HL 5140](../../HANDOFF_LOG.md#L5140))

**Sekiro**
- **[JACOB]**, 18 Oct: "parry always wins thats the whole point but look at sekiro you dont have to attack at all just riposte". This produced the heavy swings. ([HL 8731](../../HANDOFF_LOG.md#L8731); [BattleSimulation.Heavy.cs 5](../../Assets/Scripts/Sim/Battle/BattleSimulation.Heavy.cs#L5))
- **[JACOB]**, 24 Oct: "parry is the ultimate move it negates anything and everything like sekiro you cant make it a side mechanic bro everything should be intertwined between them". ([DL 573](../../DESIGN_LOCKS.md#L573))
- Raid chief, 25 Oct: the chief's bar works "like Sekiro's posture" and mashing Hold never refreshes the parry window "the way mashing block does in Sekiro" (Claude's build of his raid calls). ([BattleSimulation.Raid.cs 8](../../Assets/Scripts/Sim/Battle/BattleSimulation.Raid.cs#L8); [HL 11182](../../HANDOFF_LOG.md#L11182))
- Claude's memory of how he judges mechanics: "He judges designs Sekiro style: the safe answer must pay less than the risky one, and a miss must cost something." ([memory: feedback_risk_reward_punish](C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/memory/feedback_risk_reward_punish.md))

**World of Warcraft raid bosses**
- **[JACOB]**, 25 Oct: "lets imagine our whole run like a raid bosses from World of Warcraft", "only chief and his mechanics where some you have to parry, some you have to mitigate", "hold is universal mechanic tht doesnt require points". ([BattleSimulation.Raid.cs 5-8](../../Assets/Scripts/Sim/Battle/BattleSimulation.Raid.cs#L5); [FightRules.cs 114](../../Assets/Scripts/Sim/Battle/FightRules.cs#L114))
- **[JACOB]**: "there's only chief and his mechanics". ([HL 11170](../../HANDOFF_LOG.md#L11170))
- On 20 Oct he called Claude's Bloodridge chief page "perfect"; the lock itself describes that page as written like a raid boss page. ([DL 441](../../DESIGN_LOCKS.md#L441))
- WoW is never a look to copy: the WoW dragonkin hunters were a "copyright violation waiting to happen". ([DL 357](../../DESIGN_LOCKS.md#L357))

**Devil May Cry**
- Glory, locked 20 Oct as part of the chief direction: "Glory, the hook, like Devil May Cry's style rank: a big letter from D to S." (Claude's page; Jacob locked the direction.) ([DL 446](../../DESIGN_LOCKS.md#L446))
- Rejected 22 Oct: "you made him exactly like dmc 3 boss" (the hunt captain as a rival duelist with mirror copies, a three cut combo and an escape). ([DL 511](../../DESIGN_LOCKS.md#L511))
- Premise, September: "just like dmc 5 the very first fight is the major boss the final boss but you are wiped completely". ([HL 1277](../../HANDOFF_LOG.md#L1277))

**Hades (and Absolum)**
- Losing a run "works like Hades and Absolum": run gear lost, materials come home. ([DL 188](../../DESIGN_LOCKS.md#L188))
- 3 lives a run: "yea like hades death defiance bro" (25 Oct). ([HL 11271](../../HANDOFF_LOG.md#L11271))
- 11 Oct: the fight choice was "too much reading next to Hades' one icon". ([HL 6152](../../HANDOFF_LOG.md#L6152))
- Pal favours on road nodes are "Hades boons from the four Pals" (his 18 Oct pick, built). ([HL 9286](../../HANDOFF_LOG.md#L9286); [Design recap 160](../../reports/Design%20recap.md#L160))

**Darkest Dungeon**
- **[JACOB]**, 6 Oct, first playtest, liked: "it's like Darkest Dungeon but an auto battler". ([HL 4778](../../HANDOFF_LOG.md#L4778))
- Rejected 1 Oct: camera close ups, "its like zooming in and out nothing like darkest dungeon"; he said "try it yes" to a short freeze, small shake, flash and big number instead, then called the hit stop "too subtle". ([HL 4530](../../HANDOFF_LOG.md#L4530); [HL 4536](../../HANDOFF_LOG.md#L4536))
- Darkest Dungeon's ranks are the Thundercoast chief's tactical piece ("moves that hit places, not people") and its Heart of Darkness victim mark is in the tactical list (both from Claude's boss research, inside the direction Jacob locked). ([DL 451](../../DESIGN_LOCKS.md#L451); [DL 545](../../DESIGN_LOCKS.md#L545))

**Mega Man Battle Network, Summon Night, He-Man, Octopath**
- MMBN **[JACOB]**, 2 Oct: orders come from "the signature moves taken from broken tribes (Mega Man Battle Network)". This became each chief's one order on the 5th button. ([DL 84](../../DESIGN_LOCKS.md#L84); [DL 185](../../DESIGN_LOCKS.md#L185); [Design recap 63](../../reports/Design%20recap.md#L63))
- Summon Night: Swordcraft Story: the Pal who broke a chief learns his gear recipe. ([DL 88](../../DESIGN_LOCKS.md#L88))
- He-Man: gear changes a unit's look like Snake Armor (killed 20 Oct, pieces now show as effects and icons); the Battle Cat transform (Pal transform killed 10 Oct; reused for the cage bear's powered form, "when tiger becomes powered by greyskull"). ([DL 88](../../DESIGN_LOCKS.md#L88); [DL 416](../../DESIGN_LOCKS.md#L416); [DL 646](../../DESIGN_LOCKS.md#L646))
- Octopath Traveler: Pal story arcs (not combat). ([DL 96](../../DESIGN_LOCKS.md#L96))

**Nemesis, MGS, Shadow of War, ECHO, Assassin's Creed Odyssey (the rival)**
- **[JACOB]**, 24 Oct: "the beauty of nemesis or mgs is you can actually see the visual feedback ... dont mimic but something new like a hybrid". ([DL 573](../../DESIGN_LOCKS.md#L573))
- The locked showcase hybrid: Shadow of War's gang you can see, MGS V's adaptation you can see, ECHO's learning you can shape, plus a replay of your real fight. ([DL 618](../../DESIGN_LOCKS.md#L618))
- Hunters as mercenaries "like those of Assassin's Creed Odyssey" with names and titles. ([DL 373-375](../../DESIGN_LOCKS.md#L373))
- Patent flag: the banner, trophies and memories sit closest to the Nemesis patent (US 10,926,179). ([DL 642](../../DESIGN_LOCKS.md#L642))

**Into the Breach, Slay the Spire, Advance Wars, XCOM 2 (tactical pieces for chiefs)**
- From Claude's 20 Oct boss research, inside the direction Jacob locked ("lock this direction"): seeing his next move 6 s early (Into the Breach, Slay the Spire), moves that hit places (Darkest Dungeon), a band with jobs, a war bar (Advance Wars), a tax on order spam (Slay the Spire's Time Eater), a victim mark, a rolled strength and weakness (XCOM 2's Chosen). "The first chief gets one piece, later chiefs more." ([DL 451](../../DESIGN_LOCKS.md#L451))
- Built per chief: Bloodridge forecast, Skullpeak band jobs, Stoneveil Blood Price (the spam tax), Thundercoast places not people. ([DL 478](../../DESIGN_LOCKS.md#L478); [DL 491](../../DESIGN_LOCKS.md#L491); [DL 533](../../DESIGN_LOCKS.md#L533); [DL 545](../../DESIGN_LOCKS.md#L545))
- Caution: a Slay the Spire intent style proposal ("every enemy move gets a visible warning with one matching answer button") was rejected on 6 Oct as a chore, and he said of deck builders "they suck i hate them". ([memory: feedback_fight_chore](C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/memory/feedback_fight_chore.md); [HL 5140](../../HANDOFF_LOG.md#L5140))

**Warpack, Northgard Battlegrounds, Time Walker (squad synergy videos, 24 Oct)**
- **[JACOB]** supplied three YouTube links with timestamps: Northgard Battlegrounds (t=1212s), Warpack (t=244s), a roundup at the Time Walker Dark World chapter (t=190s). ([HL 11113](../../HANDOFF_LOG.md#L11113))
- **[CODEX]** read of the frames: Northgard, a Marketplace "producing 2 Krowns plus 1 per adjacent ECONOMY" (placement and adjacency); Warpack, "unit/item synergy panels, per-unit backpack, formation and commander"; Time Walker, "the four combatants and individual skill bars", "cross-class skill builds and merging heroes/abilities". ([HL 11113](../../HANDOFF_LOG.md#L11113))
- **[CODEX]** inference: "prioritise a legible, deliberately assembled, autonomously functioning squad build before changing Hold rewards ... let active orders redirect/amplify/rescue without requiring a timing prompt for each synergy." ([HL 11115](../../HANDOFF_LOG.md#L11115))
- **[CLAUDE]**: "Time Walker's real hook is skill sets you assign across heroes. Your class rule bans that, so our version is gear bends." ([Squad synergy 19](../../reports/Squad%20synergy%20and%20the%20Hold%20load.md#L19))

**Presentation references used for fights**
- GBA Fire Emblem: his love of Sacred Stones "concerned its pixel-art/animation feel; it does not automatically approve Fire Emblem's grid, turn structure or campaign design". ([HL 3770](../../HANDOFF_LOG.md#L3770)) Claude framed Hit and Avoid as "Fire Emblem style" with shown chances; Jacob cut the numbers: "Just show damage number". ([HL 7144](../../HANDOFF_LOG.md#L7144); [DL 291](../../DESIGN_LOCKS.md#L291))
- One Piece GBA: "a reference for its art direction and how the game presents its premise, not for its plot". ([HL 5133](../../HANDOFF_LOG.md#L5133)) He was offered Fire Emblem, One Piece and Castlevania HUD boards and never picked one. ([HL 7938](../../HANDOFF_LOG.md#L7938); [HL 10932](../../HANDOFF_LOG.md#L10932))
- The lich boss video (sent 18 Oct): he wants hand made looking spells like it; image model spells were "very static and noisy and weird". Claude's read of the lich: "one big readable icon (the skull) held for half a second". ([HL 9747](../../HANDOFF_LOG.md#L9747); [HL 9751](../../HANDOFF_LOG.md#L9751))
- DBZ FighterZ for chief cut ins: "a comic strip like thing popping up on the screen with chieftain ... like dbz fighter z when you call you friend a popup with the character face and body shows up and vanishes". He then picked the Advance Wars / Wargroove style look. ([HL 8850](../../HANDOFF_LOG.md#L8850); [HL 9035](../../HANDOFF_LOG.md#L9035))
- Premise references (not fight mechanics): Clair Obscur: Expedition 33 for the wipe, Predator: Badlands, Outriders; Lord Boros (One Punch Man) for the Sovereign's threat. ([DL 80](../../DESIGN_LOCKS.md#L80); [DL 367](../../DESIGN_LOCKS.md#L367))

**Against mashups**
- "Recent Fallout Tactics / Fire Emblem / Total War comparisons were exploratory ... Jacob objected to drifting into a familiar-game mashup." ([HL 171](../../HANDOFF_LOG.md#L171); [HL 2267](../../HANDOFF_LOG.md#L2267))
- Feel principle **[JACOB]**, 2 Oct: "An absurd, addictive loop that players can beat in many ways comes before niche artistry." ([DL 82](../../DESIGN_LOCKS.md#L82))

### Inferences
- The two strongest current pulls are Sekiro (the parry as the ultimate move, posture, riposte) for bosses, and the Warpack/Time Walker/Northgard style (a squad you assemble whose synergy fires on its own, visibly) for road fights. His 25 Oct calls split them that way: raid chief built around a free Hold and parry; road fights built around autonomous kits and chains.
- Kingdom Rush is the constant for pace, tone and "the player stays busy"; Pokémon and Hades are the constants for loop simplicity and loss rules.
- References are for one feeling each; before showing any boss or system, check that it does not read as a famous game's version (his DMC 3 and WoW reactions; Claude's own rule in [memory: feedback_no_boss_copies](C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/memory/feedback_no_boss_copies.md)).

### Gaps
- Jacob's own words on what he likes in Warpack, Northgard Battlegrounds and Time Walker are not in any file; only Codex's muted frame sampling is recorded.
- The 6 Oct quote "bro no go this is still a chore" exists only in Claude's memory file, not in the log.
- The quotes in the task brief ("i already told you no point askig me again", "12k lines of log and atleast 2k lines of design locks") are not in any local file; they are from the live chat.
- Nothing beyond the one lock line records what he likes about Mega Man Battle Network.

## 2. Every rejection about fights, and why

### Takeaway
Across 1 to 25 Oct the same few failures recur: he cannot see what the enemy or his own orders are doing (unreadable, too fast, too much text); one answer or one button wins (a chore, Simon says, spam); fights end on a bar before units visibly die; agents invent new loops or copy famous designs instead of building what he said; effects look flat or pasted; and, most recently, too many new systems at once on both sides with nothing teaching them.

### Cited Findings
Chronological, his words first, the recorded cause after.

1. **1 Oct, impact feel.** Close ups: "its like zooming in and out nothing like darkest dungeon"; hit stop "too subtle" and "death animation sucks". Fix he okayed: freeze, shake, flash, big number. ([HL 4530](../../HANDOFF_LOG.md#L4530); [HL 4536](../../HANDOFF_LOG.md#L4536))
2. **4 Oct.** "the UI looks AI made, the scene is bad, there are no real animations ... and the chief's retreat needs a reward you can see". ([HL 4657](../../HANDOFF_LOG.md#L4657))
3. **6 Oct, first playtest (liked the loop, then problems).** He "cannot see what anything does: what the Medic heals, how much the Warden hits for, what a trap or a cover is ... He did not see what Hold, Fall Back, Focus Fire, Unleash or Eclipse did. Everyone hits the Butcher"; the chief was "too powerful"; gear "was never a design decision"; the UI "sucks to the core"; "There is no feedback for what he does"; "the auto battle needs variations to be fun, and 'those buttons... not making any sense'". Also: "i cant see the feedback to determine what the enemy is doing". ([HL 4781-4784](../../HANDOFF_LOG.md#L4781); [HL 4871](../../HANDOFF_LOG.md#L4871))
4. **6 Oct, the chore.** Claude proposed "every enemy move gets a visible warning with one matching answer button, Slay the Spire intent style"; Jacob: "bro no go this is still a chore". The build before it was also a chore: "a three button script (Fall Back on the glow, Unleash whenever possible, Frenzy once) won 300 of 300 test runs." Claude's recorded lesson: "one warning with one right button is Simon says. Any set of buttons that never changes converges to a script." ([memory: feedback_fight_chore](C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/memory/feedback_fight_chore.md))
5. **8 Oct.** No deck builders ("they suck i hate them"); "gamers are very impatient": show 3 options, they choose 1 and go. ([HL 5140](../../HANDOFF_LOG.md#L5140); [HL 5180](../../HANDOFF_LOG.md#L5180))
6. **10 Oct, the respect campaign.** "He could not see the loop on screen, and it 'didn't make sense'." Respect bar, taboos and Sacrifice dropped. ([DL 178-184](../../DESIGN_LOCKS.md#L178)) To Claude: "youre inventing so much new stuff youre forgetting about the old one you derail me and yourself", then "youre going into a rabbit hole bro". ([memory: feedback_design_from_his_decisions](C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/memory/feedback_design_from_his_decisions.md))
7. **11 Oct, three fights.** "he never knew why he won or lost, that spamming Focus Fire on their healer 'did nothing', that the tutorial never says it is one, that the fight choice is too much reading next to Hades' one icon". Cause from the rules: their healer stands in the back row, unreachable while their front stands; their Warden covers the focused target; their Medic heals. ([HL 6152](../../HANDOFF_LOG.md#L6152))
8. **12 Oct, "click and play".** The slice was "click and play". ([DL 271](../../DESIGN_LOCKS.md#L271)) His seven complaints, as Claude's report lists them: no risk, no reward, no mitigation, no builds, no items, no combos, no synergy. ([Auto battler report 92-102](../../reports/Auto%20battler%20loops%20and%20systems.md#L92)) On bots: "promise me and add in the hook that you will only optimise after build like how you ran those many bots to just see what happens when you spam bro make the core strong later we will see". ([memory: feedback_build_first](C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/memory/feedback_build_first.md))
9. **13 Oct.** "repetition is a chore ... we need to find how diff tribes act differently to threat" (gave tribe reactions). No hit chance anywhere: "Just show damage number". ([TribeDoctrines.cs 5](../../Assets/Scripts/Sim/Learning/TribeDoctrines.cs#L5); [DL 291](../../DESIGN_LOCKS.md#L291))
10. **14 Oct (Claude's own play, for Jacob).** "One answer wins. Every fight was decided by parry then Riposte ... or by our Eclipse ... Once you know the timing it is too easy ... That is close to Jacob's 6 Oct note that one answer is a chore." ([HL 7831](../../HANDOFF_LOG.md#L7831)) Diagnosis: parry alone won 8 of 8; "The loop is free. Hold costs 1 and the parry pays 2"; "No habit answers Hold". ([HL 8012-8020](../../HANDOFF_LOG.md#L8012))
11. **18 Oct.** Chief spells must be "a real threat" ("think big", then "tune the existing ones not rewrite the whole thing"). Claude's audit found Steel Storm was the Eclipse renamed. ([DL 391](../../DESIGN_LOCKS.md#L391); [HL 8739](../../HANDOFF_LOG.md#L8739))
12. **19 Oct, the arena.** "the whole arena has to be like an actual fight ... they have to be popping up, they have to be unique. You just can't have some pixels fighting each other." ([HL 9784](../../HANDOFF_LOG.md#L9784))
13. **20 Oct, chief fights.** Turned down "a 3 second kneel window ('very punishing'), chiefs who use your own four orders back at you ('copy paste of the same formula'), and bosses written as one key for one lock ('you can only kill a boss linearly')". His rule: "Boss fights, everything should be different. And how you tackle that is all the gameplay." Yield, bodyguards and a second phase are "the basics that should already be in the game, and ... the game needs more than them." ([DL 441](../../DESIGN_LOCKS.md#L441); [DL 437](../../DESIGN_LOCKS.md#L437))
14. **19 to 23 Oct, spell effects.** Image model stills "look like pasted shit"; maw take "i hate it"; animated geometry "bro directions is right but they look sshit"; "youre doing flat images on top down game wow really ?bro ?"; the Eclipse fire column "no its just fire need to redo it can you do it like the meteor". Liked: the leaper "very fluid and cool", the meteor "really cool bravo", the meteor Eclipse "lovely bro", "BRO LOCK HEX SHIELD". ([memory: feedback_spell_effects_route](C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/memory/feedback_spell_effects_route.md); [HL 10634](../../HANDOFF_LOG.md#L10634))
15. **22 Oct, the hunt captain.** "you made him exactly like dmc 3 boss"; the replacement built from real hunting: "yea go i like it". ([DL 511](../../DESIGN_LOCKS.md#L511))
16. **24 Oct, risk and reward.** On the parry paper: "where is the punish and where is reward where is risk ?" (a Hold too early still soaked for free). ([memory: feedback_risk_reward_punish](C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/memory/feedback_risk_reward_punish.md))
17. **24 Oct, the bear.** "he should be doing party wide damage not to 1 unit he should be where 3 tit is and there should be no other sprite apart the beast"; "beasts have to be size of combined canvas size of all the fighters". ([DL 646](../../DESIGN_LOCKS.md#L646))
18. **24 Oct, Gates 0 to 2 build (rules 8).** "he cannot win, presses Hold without seeing an effect, and cannot follow combat because it is too fast." ([HL 10819](../../HANDOFF_LOG.md#L10819))
19. **24 Oct, rules 8 rejected** (Codex's record of his feedback): "constant parrying crowds out Focus Fire and other decisions; learning immediately counters successful play; too much text and a tiny nerve/timing display make the fight difficult to follow. He wants hard, satisfying combat with meaningful squad synergy, not a game centred on studying an adaptive opponent and repeatedly pressing Hold." His own request: "bro put the combat redesign in the handover for claude, tell him what needs changing". ([HL 10865-10868](../../HANDOFF_LOG.md#L10865); [DL 657](../../DESIGN_LOCKS.md#L657))
20. **25 Oct, nerve bar.** After rules 9 (his chief broke at 19 s with 957 HP left): "usually games are won when enemies die but here we have fighter game like bar which is confusing to most players our game is won before all the enemies die we need to make sure to sync nerve bar and all units together otherwise we aint seeing any proper gameplay". ([HL 11150](../../HANDOFF_LOG.md#L11150))
21. **25 Oct, bonded combo timing.** "right away is combo rest doesnt feel like a combo". ([HL 11145](../../HANDOFF_LOG.md#L11145))
22. **25 Oct, the boss detour.** "when i said comabt why did you jump right into boss fight bro we have so much to do with base combat"; to Codex: "bro help claude with combat design i told everything and he is still giving me shit". ([HL 11216](../../HANDOFF_LOG.md#L11216); [HL 11227](../../HANDOFF_LOG.md#L11227))
23. **25 Oct, rules 12 playtest.** "i didnt know what was going on at all". Claude's cause: "about twelve new systems went into one fight in a day, both sides get all of them, and nothing teaches them." ([HL 11658](../../HANDOFF_LOG.md#L11658); [HL 11667](../../HANDOFF_LOG.md#L11667))

### Inferences
- The root causes repeat: unreadable cause and effect (3, 7, 18, 19, 23), one dominant answer (4, 10, 19), invisible endings (20), and agents adding systems he did not ask for or copying famous shapes (6, 13, 15, 22).
- Every fix that worked for him was either a visible moment (freeze, flash, big number, meteor, hex shield) or a simplification he named himself (easy Pokémon loop, set path, Hades lives, Hold free).
- The enemy should not mirror the player's full kit: rule 12 gave both sides chains, boosts and frenzy, and his own Warden was flared and frenzied at 6.5 s while enemy combos looked the same as his ([HL 11662-11663](../../HANDOFF_LOG.md#L11662)).

### Gaps
- No recorded reaction from him to the rules 10 raid chief build or the rules 11 road build, apart from the redirect at HL 11216.
- He did not say why he hated the maw take (Claude's memory says so).
- Speed setting, rejected clicks and reading time in his plays are not persisted ([HL 10857](../../HANDOFF_LOG.md#L10857); [HL 11645](../../HANDOFF_LOG.md#L11645)).

## 3. His locked combat decisions in DESIGN_LOCKS.md, and which still stand

### Takeaway
The locks carry: auto battle plus a few orders; tribes that learn; a nerve bar; one chief order on the 5th button; earned points; Perfect Hold as the parry; Hit and Avoid with no numbers shown; tribe reactions; nerve snaps; class spells and marks; chief kits with Glory and a kneel; beasts; the hunt; the parry at the center (rules 8, experience rejected); and the three squad first picks (rules 9). DESIGN_LOCKS.md stops at 24 Oct; everything from 25 Oct is only in the log (section 4), and several locks are already overridden there.

### Cited Findings
| Lock (date, line) | Jacob's words or the lock | Status on 25 Oct |
|---|---|---|
| Genre, 30 Sep ([DL 17](../../DESIGN_LOCKS.md#L17)) | "roguelike, fast-paced action"; battle screen 2.5D ([DL 20](../../DESIGN_LOCKS.md#L20)) | Stands |
| Fight model, 1 Oct ([DL 68](../../DESIGN_LOCKS.md#L68)) | "Auto battle + directives". Units fight on their own; the player spends directive points (Hold, Fall Back, Focus Fire, Unleash, Sacrifice), with pause and speed | Stands. Sacrifice cut 10 Oct ([DL 184](../../DESIGN_LOCKS.md#L184)) |
| Innovation, 1 Oct ([DL 69](../../DESIGN_LOCKS.md#L69)) | units and tribes learn from real fights, with a replay of why; rivalry stays ("there will be no narrative without this") | Stands; narrowed by rules 9 (one lesson a fight, on one move, chief learns only from his fights) ([DL 661](../../DESIGN_LOCKS.md#L661)) |
| Orders refill, 2 Oct ([DL 84](../../DESIGN_LOCKS.md#L84)) | buttons refill so the player stays busy (Kingdom Rush) | Killed 13 Oct by earned points ([DL 287](../../DESIGN_LOCKS.md#L287)) |
| Stolen moves, 2 Oct ([DL 84](../../DESIGN_LOCKS.md#L84)) | moves taken from broken tribes (MMBN) | Became "Each chief gives you one main order, like the Eclipse" ([DL 185](../../DESIGN_LOCKS.md#L185); [DL 200](../../DESIGN_LOCKS.md#L200)) |
| Nerve break, 2 Oct ([DL 85](../../DESIGN_LOCKS.md#L85)) | each tribe has a nerve bar; at zero it gives up; "kill fewer, keep more" | Side bar built. Joining rule replaced (nobody joins until the chief, [DL 199](../../DESIGN_LOCKS.md#L199); capture the chief, [DL 275](../../DESIGN_LOCKS.md#L275)). Road fights: replaced by nerve per fighter on 25 Oct, not in locks |
| Hold exploit answer, 2 Oct ([DL 89](../../DESIGN_LOCKS.md#L89)) | holding drains your own nerve; chiefs learn to flank a holding line | Drain built; removed in road fights under rules 11 ([HL 11440](../../HANDOFF_LOG.md#L11440)). Hold costs no point under rules 10 |
| Death, 2 Oct and 10 Oct ([DL 90](../../DESIGN_LOCKS.md#L90); [DL 188](../../DESIGN_LOCKS.md#L188)) | army, gear, converts reset; Hades rule for materials | Stands |
| Loop, 10 Oct ([DL 181-187](../../DESIGN_LOCKS.md#L181)) | "like easy Pokémon"; each tribe a set path ("neat and easy"); each chief gives one main order; runs start and end at camp | Stands |
| Unleash drain, 11 Oct ([DL 226](../../DESIGN_LOCKS.md#L226)) | Unleash drains your own nerve at half Hold's rate | Stands outside road fights; no drain in road fights under rules 11 |
| Fixed lineups and Medium difficulty, 11 Oct ([DL 251-254](../../DESIGN_LOCKS.md#L251)) | same squad per fight kind; a sensible player wins one first run in three, most by run 3 | Lineups replaced by three squads per fight shown on the node (his 18 Oct call; the lock text was not edited) ([HL 9284](../../HANDOFF_LOG.md#L9284)) |
| Capture the chief, 13 Oct ([DL 273-276](../../DESIGN_LOCKS.md#L273)) | "its not easy"; kill him and take his Princess gear | Stands; since 20 to 21 Oct it is the kneel with Spare or Execute ([DL 449](../../DESIGN_LOCKS.md#L449)) |
| Roster as lives, 13 Oct ([DL 279-281](../../DESIGN_LOCKS.md#L279)) | lose and the winners capture one of yours | Replaced 25 Oct by 3 lives, squad back every fight (not in locks) |
| Earned points, 13 Oct ([DL 287](../../DESIGN_LOCKS.md#L287)) | "Jacob's idea to kill spam": points from a parry, a Fall Back dodge, a Focus Fire interrupt, a kill, a nerve break, a combo; small trickle; spent on Focus Fire, Unleash and the chief's order | Stands, changed by rules 9 (parry refunds only its point; a cashed mark earns 1; trickle every 8 s) and rules 10 (Hold free, parry pays no points) |
| Perfect Hold, 13 Oct ([DL 289](../../DESIGN_LOCKS.md#L289)) | Hold in the last half second before a big hit: no damage, attacker staggers, Unleash after is a combo. "No new button" | Stands; grew into rules 8, 9 and 10 |
| Hit and Avoid, 13 Oct ([DL 291](../../DESIGN_LOCKS.md#L291)) | "Just show damage number"; MISS shown; Focus Fire raises Hit, Hold raises Avoid | Stands, built |
| Tribe reactions, 13 Oct ([DL 293](../../DESIGN_LOCKS.md#L293)) | "repetition is a chore"; 3 or 4 reactions per tribe | Stands, built as doctrines |
| Nerve snaps, 13 Oct ([DL 295](../../DESIGN_LOCKS.md#L295)) | panic (flee) or rise (berserk), both sides | Road fights: replaced on 25 Oct (melee frenzy, enemy ranged flee, your ranged freeze; "there's no running only death") |
| Build first, 12 Oct ([DL 301](../../DESIGN_LOCKS.md#L301)) | build until it plays and he has played it; no bot sweeps or tuning before that | Stands |
| Tribes learn your parry, 14 Oct ([DL 349](../../DESIGN_LOCKS.md#L349)) | wait out your Hold | Stands, narrowed by rules 9 |
| Chief spells are real threats, 18 Oct ([DL 391](../../DESIGN_LOCKS.md#L391)) | "think big": 30/40/50% of each target's health, 350 nerve | Stands |
| Dust Shark, 18 Oct ([DL 384-391](../../DESIGN_LOCKS.md#L384)) | Clarth spell, answered by Fall Back | Stands |
| New tribes, 18 Oct ([DL 395-405](../../DESIGN_LOCKS.md#L395)) | bloodletting (Stoneveil), the flood (Thundercoast); both chief spells answered by Fall Back; no prisoners | Stands; no prisoners is flavour only under rules 11 |
| Class rule, 20 Oct ([DL 411](../../DESIGN_LOCKS.md#L411)) | "classes have auto attacks then have specific spells only they can use tank cant heal and heal cant dps" | Stands; 25 Oct kit of 3 adds a defence move and a boost per class |
| Marks and pieces, 20 Oct ([DL 414-416](../../DESIGN_LOCKS.md#L414)) | Snare, Bleed, Opening, Guard; every piece bends only its class's spell; pieces shown as effects and icons | Stands, built |
| The hybrid ten, 20 Oct ([DL 417-427](../../DESIGN_LOCKS.md#L417)) | "All are okay a hybrid of all 3 wuld be super cool", "put all ten into the hybrid" | Built: marks, trophies, tribe sets, road scars; kin bonds (rules 11, on any enemies). Paper: Pal duos, formation pieces, blood pacts, order sockets, road jobs, envoys ([Design recap 170-180](../../reports/Design%20recap.md#L170)) |
| Chief fights, 20 Oct ([DL 441-453](../../DESIGN_LOCKS.md#L441)) | "Boss fights, everything should be different. And how you tackle that is all the gameplay."; "more flashy and addictive"; "lock this direction". Own moveset, moves on their own timers, overlapping, each with more than one answer; Glory D to S; one hidden chain; kneel with no timer; Spare or Execute | Built rules 3 to 7; Bloodridge became the raid chief under rules 10 (not in locks) |
| Beasts, 21 to 24 Oct ([DL 455-507](../../DESIGN_LOCKS.md#L455); [DL 646-653](../../DESIGN_LOCKS.md#L646)) | shark and cage bear, parts, no nerve; the bear "party wide damage", alone, as big as the squad | Built; the shark is "far too easy"; the party wide slam is not a sim move ("Open") |
| The hunt, 22 Oct ([DL 509-521](../../DESIGN_LOCKS.md#L509)) | Track, Drive, Corner, Kill; the Pack; Turned Hunt | Built rules 6; the hunters are parked by Jacob on 25 Oct ([HL 11688](../../HANDOFF_LOG.md#L11688)) |
| Parry at the center, 24 Oct ([DL 571-642](../../DESIGN_LOCKS.md#L571)) | "parry is the ultimate move it negates anything and everything like sekiro"; every beat a duel for nerve; nothing unparryable; paused Hold never parries; kit answers stay as safe answers; Gate 4 lessons as baits; combo cues | Built rules 8, experience rejected the same day. Still standing: nothing unparryable, paused Hold never parries ([Design recap 208](../../reports/Design%20recap.md#L208)) |
| Squad first, 24 Oct ([DL 655-668](../../DESIGN_LOCKS.md#L655)) | his three picks: band swings are small beats every 5 s; no block cost (Hold a stance again); a chief learns only from fights against him, one lesson per fight, on one move | Locked and built rules 9. The rest of rules 9 (points and nerve from the squad, honest reads) is Claude's proposal, built, not locked ([DL 668](../../DESIGN_LOCKS.md#L668)) |

Other lock lines a designer needs:
- Open since 10 Oct: "making one button spam lose". ([DL 208](../../DESIGN_LOCKS.md#L208))
- 14 Oct acceptance for the repair round: "one normal player build shows representative Bloodridge and Skullpeak fights with real command choices and passes a look and readability review plus save and replay checks. Test counts alone do not settle feel or art." ([DL 343](../../DESIGN_LOCKS.md#L343))
- Medium difficulty is measured on a "reading bot" that reacts only to what the screen shows. ([DL 253](../../DESIGN_LOCKS.md#L253))

### Inferences
- The durable core from 1 to 2 Oct is intact: auto battle with a few orders, a chief order on the 5th button, learning rivals, and a busy player. What keeps changing is how the player spends points and how fights end.
- DESIGN_LOCKS.md is now a day behind the owner: the shared nerve bar, roster as lives, the 13 Oct snaps, Hold's point cost and the 20 Oct "auto attack plus one spell" count are all overridden in the log on 25 Oct.

### Gaps
- No 25 Oct section exists in DESIGN_LOCKS.md; the log entries in section 4 are the only record.
- The fixed lineups line was never edited after the 18 Oct three squads call ([HL 9284](../../HANDOFF_LOG.md#L9284)); Claude's 25 Oct recap still marks "Fixed lineups; Medium difficulty" as Built ([Design recap 115](../../reports/Design%20recap.md#L115)). The two records conflict.

## 4. His 25 Oct decisions, in HANDOFF_LOG.md and not in DESIGN_LOCKS.md

### Takeaway
On 25 Oct Jacob made the raid chief (rules 10), set a nine point base combat list, approved road combat (rules 11) and the road kit (rules 12), saw the rules 12 fight overload him, handed all open work to Codex, and then approved Claude's six fixes as rules 13, which Codex is building. The answers below are already given and should not be asked again.

### Cited Findings

**Calls on the squad synergy paper v2 (morning)** ([HL 11143-11147](../../HANDOFF_LOG.md#L11143))
- Kin bonds form from three combos by one pair in one fight, on any enemies (changes the 20 Oct "same enemy" wording).
- A bonded shot fires right away: "right away is combo rest doesnt feel like a combo".
- Formation: two in front, the rest behind; "back row is constant combo doers" (Claude reads this as yes, back row pieces can help the fighter in front; unconfirmed).
- The Skirmisher's formation piece (Blood Scent, the fighter beside her hits bleeding enemies harder): "sure why not".
- "when we recruit the chief the whole place is ours". ([memory: project_squad_synergy](C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/memory/project_squad_synergy.md))

**His list, "do these first then we move on"** ([HL 11149-11158](../../HANDOFF_LOG.md#L11149))
1. Sync the nerve bar with the units (quote in section 2, item 20).
2. "recruit comes only from chief when he kneels for us after final fight".
3. More combos. 4. Proper talent trees. 5. Distinctive traits. 6. Self preserving gameplay. 7. Unit synergy.
8. Elemental and other kinds of damage over time on basic attacks.
9. "3 attacks/defensekit/magic boost etc to each unit that theyuse in proper intervals of time".

**Raid chief, rules 10** ([HL 11168-11190](../../HANDOFF_LOG.md#L11168); [BattleSimulation.Raid.cs 5-16](../../Assets/Scripts/Sim/Battle/BattleSimulation.Raid.cs#L5))
- **[JACOB]**: "show me in game those boss fights we will refine when i see them do 1 pass"; the chief fights alone ("there's only chief and his mechanics"); Hold is free and universal ("hold is universal mechanic tht doesnt require points"); a parry pays in his bar, not in points; interruptible casts show a cast bar and parry moves do not; his ultimate gets the cut in and flash on the parry moment.
- **[CLAUDE]** numbers: 400% of a chief's health; his bar is posture, cracked only by answers, refilling at 30 a second times his health share; below 30% health his Eclipse comes every 12.5 s; Sun Mantle and Burning Ring broken by Focus Fire; his Eclipse only parried or dodged; Spare brings his tribe's lineup under fresh names. Bloodridge only; the other three chiefs keep their bands.
- Then **[JACOB]** redirected: "when i said comabt why did you jump right into boss fight bro we have so much to do with base combat". The raid build stays a preview. ([HL 11216](../../HANDOFF_LOG.md#L11216))

**Road combat, rules 11: his calls in his words** ([HL 11268-11280](../../HANDOFF_LOG.md#L11268))
- Nerve synced with the units, so fights end when units are visibly beaten.
- "there's no running only death": your units never run.
- Your whole squad comes back every fight; 3 lives a run ("yea like hades death defiance bro").
- Recruits only from a chief's kneel.
- Frenzy is a melee last stand: "frenzy last about 30 seconds till they collapse", then "frenzy is last stand not all only skirmisher tank and any other melee dps".
- The streak: "streak is 3 with multplier die loose the multiploier".
- Bonds on any enemies; the bonded shot right away. Two in front, the rest behind; back row "constant combo doers". Blood Scent for the Skirmisher.
- A kit of 3 per unit, "attacks/defensekit/magic boost", used at proper intervals. Damage over time and elements on basic attacks.
- Hold free (rules 10). Fall Back keeps its 1 point. Points pay for Focus Fire, Unleash and the chief spell.
- **[JACOB]** "yes to all 3" to Claude's defaults: your Marksman, Trapper and Medic freeze 3 s at zero nerve and recover; the x3 streak multiplies the fight's materials; Grudge fires on the tribe that took one of your lives. ([HL 11406](../../HANDOFF_LOG.md#L11406))
- Build go: "go, build rules 11 in the sim as written in 11263, 11358 and 11406, and hand it to Codex". ([HL 11414](../../HANDOFF_LOG.md#L11414))
- **[CLAUDE]** (built as written, not Jacob's words): nerve 200 per fighter and its drain sources, the half clock chain eligibility, 1.5 s bond gap, the five defence moves and their numbers, frenzy only below half health, freeze back at 50 nerve, leaders never snap, time out rules. ([HL 11282](../../HANDOFF_LOG.md#L11282); [HL 11296-11325](../../HANDOFF_LOG.md#L11296); [HL 11358-11404](../../HANDOFF_LOG.md#L11358))

**Road kit, rules 12: his five** ([HL 11526-11533](../../HANDOFF_LOG.md#L11526))
1. Fall Back stays, as a pure dodge ("alright bro lets keep it as dodge and build these 5 lets go").
2. The streak multiplies a won fight's materials, x1 to x3.
3. Recruits bring their own element ("depends what adds more variety"); only Burn is built.
4. Bloodridge's road trash carries small versions of the chief's moves: "the trash before the boss is divided in such a way that one of the boss ability is given to one of a mob in a trash pack", then "1. We can do that". ([BattleSimulation.Carriers.cs 6-7](../../Assets/Scripts/Sim/Battle/BattleSimulation.Carriers.cs#L6))
5. Every class gets an automatic boost: "these are great as long as they re code and what player has not to worry". ([BattleSimulation.Boosts.cs 5-6](../../Assets/Scripts/Sim/Battle/BattleSimulation.Boosts.cs#L5))

**Handover and rules 13**
- **[JACOB]**: "handover everything to codex i dont have limit on you i have fresh reset on codex". ([HL 11649](../../HANDOFF_LOG.md#L11649))
- Claude's six fixes after his rules 12 play, written as "NOT APPROVED ... Jacob asked to talk them through, then was too tired to answer": (1) only the player's side reacts in chains and boosts, enemies keep basic attacks, Bloodridge Burn and carried moves; (2) one new system per road fight: combos, then defence moves, then boosts, carried moves last before the chief; (3) the player's frenzy only below a quarter of health; (4) the Steady pierce fix; (5) icons, not words, on fighters, one big stamp per player combo, no enemy combo banner, a trimmed header; (6) the carried Cleave beat and the Brand countdown as the loud moments. ([HL 11669-11676](../../HANDOFF_LOG.md#L11669))
- Then **[JACOB]**: "go bro build rules 13 with those changes, fix the Steady pierce bug, audit it then re audit it, keep old replays working and let me play, leave bgs alone". Codex recorded the approved scope as those six, with new Bloodridge runs needing four combat steps to introduce four systems. In progress at the end of the log. ([HL 11713-11714](../../HANDOFF_LOG.md#L11713))

**Other 25 Oct calls** ([HL 11682-11687](../../HANDOFF_LOG.md#L11682))
- Hold is free and a parry pays in the chief's bar, not points. Points pay for Focus Fire, Unleash and the chief spell; Fall Back keeps its 1 point. Recruits only from a chief's kneel. Thundercoast's no prisoners is flavour only. The trash before a boss carries the boss's moves.
- Parked by him: the hunters; beast hunts and hunter fights as revisits of a cleared region; beast levels 1 to 10, then tame or let go; the Sovereign raiding allied tribes with random objectives; omens chosen up front; Kingdom Rush style upgradeable spells; the other three chiefs as raid bosses. ([HL 11688](../../HANDOFF_LOG.md#L11688))

**Already answered, do not ask him again**
- Bonds: any enemies; bonded answer right away ([HL 11144-11145](../../HANDOFF_LOG.md#L11144)). Your units never run; squad restored each fight; 3 lives; recruits only from the kneel; melee frenzy about 30 s then collapse; streak to x3 lost on a loss; ranged freeze 3 s; streak multiplies materials; Grudge on the life taker ([HL 11268-11280](../../HANDOFF_LOG.md#L11268); [HL 11406](../../HANDOFF_LOG.md#L11406)). Fall Back a pure dodge; recruits bring their own element; trash carries boss moves; automatic boosts the player never has to worry about ([HL 11529-11533](../../HANDOFF_LOG.md#L11529)). Hold free ([HL 11280](../../HANDOFF_LOG.md#L11280)). Rules 13's six fixes ([HL 11714](../../HANDOFF_LOG.md#L11714)).

### Inferences
- His 25 Oct shape: road fights are squad fights where units protect themselves and combo on their own ("what player has not to worry"), the player's orders steer, exploit or rescue, and fights end when enemies are visibly dead or gone; bosses are raid fights around a free, universal Hold and a posture bar.
- Rules 13 is the owner approved answer to "i didnt know what was going on at all": simpler enemies, systems introduced one per fight, fewer words, louder key beats.
- Codex's base combat target, which Claude acknowledged, is the clearest written statement of the goal: "A fresh squad should visibly protect itself and complete teamwork before a bond, talent tree or perfect parry is required ... He should be able to explain why one enemy fell and why one ally survived without opening Details." ([HL 11232](../../HANDOFF_LOG.md#L11232); [HL 11265](../../HANDOFF_LOG.md#L11265))

### Gaps
- None of 25 Oct is in DESIGN_LOCKS.md.
- Still unresolved per the log: back row adjacency for formation pieces ([HL 11146](../../HANDOFF_LOG.md#L11146); [HL 11235](../../HANDOFF_LOG.md#L11235)); element inheritance scope, recruits' own element versus everyone getting it ([HL 11404](../../HANDOFF_LOG.md#L11404)); Long Snare cannot save a trap set right after a shot, fix (recheck unclaimed setups) not built ([HL 11499](../../HANDOFF_LOG.md#L11499)); talent trees, traits and other elements are unbuilt and unspecified beyond Claude's paper ([Nerve sync 96-106](../../reports/Nerve%20sync%20and%20unit%20kits.md#L96)).
- Rules 13's result is not in the log yet.

## 5. The playtest evidence

### Takeaway
Jacob's recorded plays: 6 Oct (liked the loop, could not read the fight), 10 Oct (respect campaign made no sense), 11 Oct (never knew why he won or lost), 24 Oct three times (rules 8 build too fast and Hold showed no effect; the learned counter chief fight; then a full rules 9 Bloodridge route won but 68% Hold), and 25 Oct (rules 12 war party lost, "i didnt know what was going on at all"). The data says the parry carried every win, nerve ended fights before units died, and rules 12 put about 7 notable events a second on screen with both sides running the same combos.

### Cited Findings

**24 Oct, Gates 0 to 2 build (rules 8)**: "he cannot win, presses Hold without seeing an effect, and cannot follow combat because it is too fast". Speeds offered were 1x, 2x, 4x, manual fights reset to 1x; the parry window is 15 ticks (half a second). ([HL 10819-10821](../../HANDOFF_LOG.md#L10819)) Codex then made 0.5x the default and added 0.25x. ([HL 10839](../../HANDOFF_LOG.md#L10839))

**24 Oct, his chief fight in the learning build (rules 8), Codex's replay** ([HL 10857-10861](../../HANDOFF_LOG.md#L10857))
- War party won in 855 ticks: 10 Holds, 9 parries. Those parries armed the chief: WaitsOutParry and Feints both true.
- Chief fight lost at 22.53 s: his nerve 0, the enemy's 506; all six of his fighters alive; leader 360/360; chief 1026/1026 HP. Eight accepted commands, all Hold, zero parries; no Focus, Unleash or Fall Back.
- Six of eight Holds were inside the flagged parry window, but feints and wait outs turned them into point spend and nerve loss.
- Claude's read ([HL 10950-10954](../../HANDOFF_LOG.md#L10950)): 40 to 50 beats a minute in a Bloodridge chief fight, every one a duel for nerve; "In Jacob's chief fight 9 blocks took 480 of his 1000 nerve in 22 s"; the chief "feinted 13 times in 22 s, three moves at once at 18.97 s"; "Squad play barely moves nerve, apart from deaths".

**24 Oct, full rules 9 Bloodridge route, Codex's audit** ([HL 11099-11103](../../HANDOFF_LOG.md#L11099))
- Champion duel won 30.87 s (6 parries); Eclipse trial won 30.77 s (4); war party won 27.40 s (4); Bloodridge chief won and recruited at 19.00 s (6 parries). No deaths on his side. Final nerve 730, 631, 353, 553 of 1000.
- Chief: Focus on their Medic at 2.00 s, Unleash 4.53 s (scare blocked, damage counted), the Medic killed at 9.10 s, all six Holds parried (Heavy x3, Cleave x2, Brand x1), the chief yielded at 18.97 s with 957 HP left.
- "26 Hold orders out of 38 combat orders (68.4%); 20 successful parries. Chief: 6 Holds out of 8 combat orders." "the observed action mix remains Hold-heavy."

**25 Oct, rules 12 war party, Codex's audit** ([HL 11645-11647](../../HANDOFF_LOG.md#L11645))
- Squad wiped at tick 1229 (41 s), all six enemies alive, two lives left.
- Four orders: Hold 6.23 s, Hold 16.07 s, Unleash 20.53 s, Hold 40.33 s. No Focus, no Fall Back, no parry. Unleash did 162 damage but the enemy auto Held on the same tick and blocked its scare.
- Bosk frenzied at 6.53 s, Brakka at 8.30 s; frenzied units cannot boost (TryBoost excludes them), so his front line lost Shield Bash and Twin Cut. Karru froze at 13.47 s and died at 15.30 s; Brakka 19.30 s; Bosk 19.70 s from a Flare; leader 25.97 s; Fenno 34.23 s; Gesh 40.97 s.
- Enemy Burn and Flare: 781 damage. Recorded hits: his side 1688, the enemy's 3348. Enemy bonds formed; none on his side.

**25 Oct, Claude's audit of the same fight** ([HL 11658-11667](../../HANDOFF_LOG.md#L11658))
- "About 7 notable events a second, with a peak of 28 in the one second from 1.0 s: traps both ways, snares, bleeds, aims, Snap Trap, Burn."
- "Their Trapper, Marksman and Skirmisher ran snare, crit, Opening, rip on his Skirmisher inside 0.4 s. Enemy combos look the same as his."
- "Their Marksman flared the Burn on his Warden: he dropped below half health and frenzied at 6.5 s, ignoring orders from then on." 57 Burn stacks set. His own Openings and bleeds were often ".unready".
- Codex's captures: "words on every body, four header lines, an enemy combo banner."

**Earlier plays and agent plays**
- 6 Oct first playtest: see section 2, item 3. After Claude's fixes, bot runs: "Hold always" fell from 98 to 0 wins of 300; Smart 284 to 300. ([HL 4843-4846](../../HANDOFF_LOG.md#L4843))
- 11 Oct: his log was war party won, Hold the line held, Bloodridge chief lost and the run ended. ([HL 6152](../../HANDOFF_LOG.md#L6152))
- 12 Oct Claude's hand play: Eclipse trial lost at 30.0 s with all 6 standing after one order; duel lost after one missed Eclipse cost 36% nerve; "A won fight gives +10 materials and a Continue button. There is nothing to choose." ([HL 6994-7012](../../HANDOFF_LOG.md#L6994))
- 14 Oct Claude's play: 6 of 6 won; parried 5 of 5 casts "once I pressed Hold about 2.6 s after a cast started". ([HL 7831-7838](../../HANDOFF_LOG.md#L7831))
- 18 Oct: "Jacob's own test sessions only reached the title" in that combined build. ([HL 8736](../../HANDOFF_LOG.md#L8736))

**Bot probes on the new rules (not human evidence)**
- Rules 9 marks in 18 road fights: snares 58 made, 11 cashed; bleeds 58, 26; Openings 47, 11; "most marks die waiting for a clock". ([Squad synergy 25-31](../../reports/Squad%20synergy%20and%20the%20Hold%20load.md#L25))
- Rules 10 raid chief: 9 kneels at 22 to 50 s, 1 kill at 50 s, 4 to 6 parries each. ([HL 11186](../../HANDOFF_LOG.md#L11186))
- Rules 11: reference bot won 60 of 60 Bloodridge road fights (average 35 s); 60 bonds; 675 reactions, 535 unready setups, 394 bonded answers; 112 fled, 188 frenzies, 6 freezes, 4 collapses; Brace 89, Sidestep 5, Step Back 110, Snap Trap 95, Self Patch 2; 584 orders, 64 Holds. Fall Back spam won 23 of 40 (ceiling 20%). ([HL 11419-11489](../../HANDOFF_LOG.md#L11419))
- Rules 12: 56 of 72 won (average 37 s); 517 boosts; 121 flares; 268 carried moves told, 229 landed or met; 14 fights reached x3. Unclaimed setups outnumber combos 909 to 633 in a bot sweep. ([HL 11579-11584](../../HANDOFF_LOG.md#L11579); [HL 11703](../../HANDOFF_LOG.md#L11703))
- Open bug: a Steady pierce shot still lands during Fall Back (fix given at [HL 11654](../../HANDOFF_LOG.md#L11654)). ([HL 11620-11622](../../HANDOFF_LOG.md#L11620))

### Inferences
- His one full win (rules 9) was carried by parries and ended on nerve with the chief nearly unhurt, which is exactly what he then asked to change (nerve synced with units, squad synergy).
- His two losses since 24 Oct came from things he could not read: learned feints and wait outs on every beat (rules 8), and twelve simultaneous systems mirrored on the enemy (rules 12). Both fail his 6 Oct complaint, "i cant see the feedback to determine what the enemy is doing".
- At about 7 notable events per simulation second, even the 0.5x default gives roughly 3 or 4 per real second; whether he played at 0.5x is not recorded.

### Gaps
- Which speed he played, which clicks were rejected and how long he read are not persisted ([HL 10857](../../HANDOFF_LOG.md#L10857); [HL 11645](../../HANDOFF_LOG.md#L11645)).
- No play of the rules 10, 11 or 13 builds by him is recorded; no outside player has played yet ([HL 10586](../../HANDOFF_LOG.md#L10586)).

## 6. The combat systems in the sim today

### Takeaway
The sim (Assets/Scripts/Sim/Battle, about 17,000 lines) stacks every rule behind versioned switches: rules 8 parry at the center, 9 squad first, 10 raid chief, 11 road combat, 12 road kit, and 13 (Codex, in progress) staged road. A new campaign plays the newest; older fights replay exactly. Below is each system, what it does and its key numbers (all starting values, 30 ticks a second).

### Cited Findings

**Base fight** ([BattleSimulation.cs 35-64](../../Assets/Scripts/Sim/Battle/BattleSimulation.cs#L35); [SliceClasses.cs 67-106](../../Assets/Scripts/Sim/Battle/SliceClasses.cs#L67); [SliceRules.cs 15-18](../../Assets/Scripts/Sim/Learning/SliceRules.cs#L15))
- Five classes (health, power, seconds per action): Warden 160/8/1.2 front; Skirmisher 100/10/1.0 front; Marksman 90/18/1.5 back, ranged; Trapper 95/9/1.3 back, ranged; Medic 85, heals 16, 1.4 back. Units at 400% health in the slice. Hit 85 to 95, Avoid 4 to 14 by class; a blow lands between 40% and 98%.
- Size ladder and look are art only (Warden 128 px, melee 112, ranged and support 96, chiefs 192). ([DL 107](../../DESIGN_LOCKS.md#L107))

**Orders** ([BattleTypes.cs 1042-1055](../../Assets/Scripts/Sim/Battle/BattleTypes.cs#L1042); [BattleSimulation.cs 35-64](../../Assets/Scripts/Sim/Battle/BattleSimulation.cs#L35); [FightRules.cs 184-230](../../Assets/Scripts/Sim/Battle/FightRules.cs#L184))
- Hold: 1 point (0 under rules 10), lasts 5 s, holding front row takes 70%, +15 Avoid, drains own nerve (about 50 for a full Hold in the campaign; none in rules 11 road fights).
- Fall Back: 1 point, 2.5 s, dodges casts and heavy swings; rules 12 makes it a pure dodge (nobody on the falling side attacks).
- Focus Fire: 1 point, 5 s, +10 Hit on the target, picks the Marksman's mark, breaks a cast when the focusing side takes 4% of the caster's health in the windup.
- Unleash: 2 points, 4 s, 150% damage, 100 nerve scare (blocked by an enemy Hold), drains own nerve at half Hold's rate (none in rules 11 road fights).
- 5th button, chief spell: 3 points. Sacrifice is cut.
- Earned points: parry 2 (1 under rules 9, 0 under rules 10), Fall Back dodge 1, interrupt 1, kill 1, panic 1, combo 1, cashed mark 1 (rules 9); trickle every 12 s (8 s under rules 9). Starting points were 5 in Claude's 14 Oct test ([HL 8013](../../HANDOFF_LOG.md#L8013)).
- Combos: Riposte (Unleash within 2 s of a parry: 12% of max health and 120 nerve); Ambush (Fall Back then Unleash within 3 s hits their back row); Pyre (Focus Fire, Unleash, then the chief spell within 6 s). ([Moves.cs 16-18](../../Assets/Scripts/Sim/Battle/Moves.cs#L16))

**Nerve, side bar** ([BattleTypes.cs 413-416](../../Assets/Scripts/Sim/Battle/BattleTypes.cs#L413); [BattleSimulation.cs 54-60](../../Assets/Scripts/Sim/Battle/BattleSimulation.cs#L54); [FightRules.cs 236-247](../../Assets/Scripts/Sim/Battle/FightRules.cs#L236))
- 1000 per side; losing the side's whole health costs 70%; a death costs 200; Unleash 100. At zero the side breaks and the fight ends (a chief kneels instead).
- Snaps at each lost quarter, 3 a fight: the most hurt fighter panics or rises (150% damage) for 3 s; 50% rise chance, +25 while holding, minus 25 under enemy Unleash.

**Parry** ([FightRules.cs 216-230](../../Assets/Scripts/Sim/Battle/FightRules.cs#L216); [BattleSimulation.Heavy.cs 20-35](../../Assets/Scripts/Sim/Battle/BattleSimulation.Heavy.cs#L20); [BattleSimulation.Parry.cs 37-93](../../Assets/Scripts/Sim/Battle/BattleSimulation.Parry.cs#L37))
- Perfect Hold: a Hold within 15 ticks (0.5 s) of a beat parries; the doer staggers 2 s and is open; a Hold given while paused never parries.
- Heavy swings: a band takes turns winding up a 200% blow with a 1 s tell, every 2.5 s, every 5 s under rules 9; parried costs their side 40 nerve under rules 8, nothing under rules 9 (a small beat).
- Rules 8 duels for nerve: safe answers crack 25% of a parry (50% under rules 9); each parry in a row +50%, cap 300% (200% under rules 9); feints come 30 ticks before the real beat; a wait out holds at least 1 s under rules 9.
- Rules 9: a cashed mark earns 1 point and cracks 40 nerve; a stale Hold only soaks; one learned lesson a fight, on one move; a chief has his own mind and learns only from his fights.

**Tribe behaviour and learning**
- Doctrines (never learned): Bloodridge rally and guard their caster; Skullpeak hunt your most hurt and hold formation below half nerve; Clarth fall back from your Unleash and pick off your back row; Stoneveil and Thundercoast mixes. ([TribeDoctrines.cs 5-12](../../Assets/Scripts/Sim/Learning/TribeDoctrines.cs#L5))
- Learned habits: cover whoever you focus, Hold when you Unleash, Unleash when you Fall Back, wait out your parry, feint; "Shoots past your Warden" cut. ([DL 600-609](../../DESIGN_LOCKS.md#L600))
- Stoneveil blades bleed and Blood Call rips the bleeding; Thundercoast floods (you slow and lose nerve per level, they hit harder). ([BattleSimulation.Tribes.cs 5-21](../../Assets/Scripts/Sim/Battle/BattleSimulation.Tribes.cs#L5))
- Hunters: named, a ladder of 10, bounty 1 to 5, 17 traits (Delay, Double Swing, Feint, Long Reach, Thick Hide, Unshakable and more), band traits. Parked by Jacob on 25 Oct. ([DL 373-380](../../DESIGN_LOCKS.md#L373); [DL 611-616](../../DESIGN_LOCKS.md#L611); [BattleSimulation.Band.cs 5-31](../../Assets/Scripts/Sim/Battle/BattleSimulation.Band.cs#L5))

**Class spells, marks and pieces** ([ClassSpells.cs 51-110](../../Assets/Scripts/Sim/Battle/ClassSpells.cs#L51); [HL 9929-9935](../../HANDOFF_LOG.md#L9929))
- Warden Guard: covers an ally (the Medic by default), blows land on him 30% lighter; treatment under his Guard heals 150%.
- Marksman Aimed Shot: every 5 s, aims 1.5 s, 250%, leaves an Opening; at a snared enemy aims 0.75 s and crits 200%; a blow during the aim breaks it, retry after 2 s.
- Skirmisher Strike: every 4 s, 200%, reaches the back row; twice as hard on an Opening; rips a bleed (all remaining bleed at once).
- Trapper Barbed Snare: trap holds whoever springs it 2 s, bleed 2% a second for 5 s; the springing blow never lands.
- Medic Treatment: never attacks; heals the most hurt.
- Marks: Snare and Opening last 2 s and 3 s. Bends, two per class: Bastion, Retaliator, Deadeye, Quickdraw, Opportunist (Opening x3), Bloodrush, Barbs, Long Snare (snare x2), Surgeon, Field Kit. ([ClassSpells.cs 11-38](../../Assets/Scripts/Sim/Battle/ClassSpells.cs#L11))
- Pal favours: 12 favours that bend one order's numbers each (Butcher Unleash, Glutton Hold, Shade Fall Back, Whisperer Focus Fire). ([SideFavours.cs 5-66](../../Assets/Scripts/Sim/Battle/SideFavours.cs#L5); [HL 9286-9290](../../HANDOFF_LOG.md#L9286))

**Chief spells** ([Moves.cs 21-34](../../Assets/Scripts/Sim/Battle/Moves.cs#L21); [DL 391](../../DESIGN_LOCKS.md#L391))
- Eclipse: 3 s windup, then the front row loses 30/40/50% of health by grade and the side 350 nerve; Hold soaks half, Fall Back dodges, Focus Fire interrupts; the caster is worn out for 4 s after, taking double damage. Others: Steel Storm (wall, 10% through, 8 s, Unleash smashes it for 300 nerve), Dust Shark, Blood Call, Tide, Firebomb, War Cry.

**Chief fights** ([ChiefKits.cs 5-35](../../Assets/Scripts/Sim/Battle/ChiefKits.cs#L5); [ChiefKits.cs 259-298](../../Assets/Scripts/Sim/Battle/ChiefKits.cs#L259); [ChiefKits.cs 526-537](../../Assets/Scripts/Sim/Battle/ChiefKits.cs#L526); [BattleSimulation.Chief.cs 6-17](../../Assets/Scripts/Sim/Battle/BattleSimulation.Chief.cs#L6))
- Base: second phase at half health (reserves join, spell recast); bodyguards while his band stands (Wardens guard, Medic treats him first, marks do not take on him); kneel when his nerve runs out, no timer: Hold spares (tribe joins), Focus Fire executes (his piece).
- Bloodridge: Cleave every 6 s (1 s glow), Brand every 15 s (4 s ring, 20% of health on the mark and neighbours), Eclipse every 25 s, Burning Ring every 12 s (1.5 s spin, 25%), Sun Mantle every 18 s (5 s, burns attackers 10%); Blood Sun at half health; 6 s forecast; Sunbreaker chain (350 nerve, 4 s stun). Takes 3 of 5 moves a run.
- Skullpeak (Steel Storm 30 s, Hook 9 s, Rifle Line 15 s, Iron Advance 20 s, Overclock 25 s, Lockdown), Stoneveil (Blood Call 20 s, Basin 15 s, Leeches 12 s, Open Veins 18 s, Transfusion 16 s, Blood Price every 6th order), Thundercoast (Tide 25 s, Sluice Gate 15 s, Riptide 18 s, Tidal Bore 20 s, Storm Surge 30 s, the water clock). ([DL 482-549](../../DESIGN_LOCKS.md#L482))
- Glory D to S: 1000 points, 200 a rank; parry 150, break 120, smash 120, snare 100, dodge 80, switch 80, guard, heal and brace 60; a different answer pays half again, the same answer twice half; eating a big move drops a rank; at A answers pay x1.5 points, at S x2 and the 5th button hits twice as hard; the grade pays materials.

**Raid chief, rules 10** ([BattleSimulation.Raid.cs 21-30](../../Assets/Scripts/Sim/Battle/BattleSimulation.Raid.cs#L21)): Bloodridge alone at 400% health; posture regenerates 30 a second at full health, scaled by health; only answers crack it; Hold free; a Hold within 1 s of the last keeps the old window; below 30% health the spell comes twice as often; Mantle and Ring are cast bars Focus Fire breaks.

**Beasts and the hunt** ([BeastKits.cs 36-48](../../Assets/Scripts/Sim/Battle/BeastKits.cs#L36); [DL 457-504](../../DESIGN_LOCKS.md#L457); [DL 512-519](../../DESIGN_LOCKS.md#L512)): shark (Bite, Dive, Volley, Sweep; four breakable parts, no nerve); cage bear (Grab, Roar, Pulse, ribcage halves hits); the hunt (Track 5 s, Drive 4 s, Corner, Kill 1.5 s; the Pack at half health).

**Road combat, rules 11** ([BattleSimulation.Road.cs 21-27](../../Assets/Scripts/Sim/Battle/BattleSimulation.Road.cs#L21); [Chains.cs 22-23](../../Assets/Scripts/Sim/Battle/BattleSimulation.Chains.cs#L22); [Defence.cs 18-21](../../Assets/Scripts/Sim/Battle/BattleSimulation.Defence.cs#L18); [HL 11424-11471](../../HANDOFF_LOG.md#L11424))
- Nerve per fighter, 200; drains from health lost (70% of 200 over his health), a comrade falling (40), your Unleash scare (half on each of their front row), a mark cashed on him (40). No shared bar; the fight ends when a side has nobody left fighting.
- Snap once a fight: melee below half health frenzies (+50% damage, ignores Fall Back and Focus, collapses and dies at 30 s; above half health nerve holds at 1); enemy ranged flee; your ranged freeze 3 s, back at 50 nerve, never below 1 again; leaders never snap.
- Lives: 3 a run, squad restored every fight, no captures, Grudge on the tribe that took a life.
- Chains: a mark readies its casher (Marksman for a snare, Skirmisher for an Opening or a bleed) if his last cast was at least half his clock ago (2.5 s and 2 s); he reacts out of turn; otherwise ".unready". Each mark pays once. Three cashes by one pair in a fight bond them for the run; a bonded answer is instant with no aim, at least 1.5 s apart.
- Defence moves, 10 s cooldown spent only when they fire: Brace (a band heavy swing on the Warden lands half and leaves the swinger an Opening), Sidestep (Skirmisher dodges a band heavy swing), Step Back (Marksman dodges a melee blow), Snap Trap (a melee enemy who hits the Trapper is snared), Self Patch (Medic below half heals 25%). Brace and Sidestep never fire with a Hold up.

**Road kit, rules 12** ([BattleSimulation.Boosts.cs 23-27](../../Assets/Scripts/Sim/Battle/BattleSimulation.Boosts.cs#L23); [Burn.cs 13-16](../../Assets/Scripts/Sim/Battle/BattleSimulation.Burn.cs#L13); [Carriers.cs 24-37](../../Assets/Scripts/Sim/Battle/BattleSimulation.Carriers.cs#L24); [HL 11541-11566](../../HANDOFF_LOG.md#L11541))
- Fall Back as a pure dodge. Streak x1 to x3 on a won fight's materials, reset by a loss.
- Automatic boosts every 15 s, only with a real target: Shield Bash (50% power, stagger, Opening), Steady (unbreakable shot that pierces to the enemy behind for 50%), Twin Cut (two 60% cuts that leave a bleed), Rearm (trap rearms), Stim (the Marksman or Skirmisher furthest from his spell gets it now, when nobody is below half).
- Burn: every Bloodridge fighter except the chief (and your Bloodridge recruits) stacks Burn, 1% of health a second per stack, up to 3, for 5 s after the last blow; an Aimed Shot flares it all at once, paying as a cashed mark.
- Carried moves: the first two Bloodridge road fighters carry small chief moves on their own clocks from 4 s: Cleave (every 6 s, 1 s tell, 200%), Brand (every 15 s, 4 s, 10% on the mark and row neighbours), Burning Ring (every 12 s, 1.5 s cast bar, 15% to both fighters in a lane).

**Rules 13, in progress (Codex)** ([FightRules.cs 131-133](../../Assets/Scripts/Sim/Battle/FightRules.cs#L131); [BattleSimulation.ClearRoad.cs](../../Assets/Scripts/Sim/Battle/BattleSimulation.ClearRoad.cs); [HL 11714](../../HANDOFF_LOG.md#L11714)): a ClearRoadStage per fight (1 combos, 2 defence, 3 boosts, 4 carriers) and "SimpleEnemy" for the enemy side in road fights; player frenzy below 25%; the pierce fix. Code present but not verified.

**Screen reads** (no rules, for Codex): ranked threats with beat timing and who answers ([HL 10530](../../HANDOFF_LOG.md#L10530)); order results with misses and reasons ([HL 10602](../../HANDOFF_LOG.md#L10602)); fake, delayed and real beats and rejected orders ([HL 11033-11035](../../HANDOFF_LOG.md#L11033)); lesson firings and marks ([HL 10751-10753](../../HANDOFF_LOG.md#L10751)).

**Versions and tests**: rules 12, save version 21, replay format 19 ([HL 11567-11570](../../HANDOFF_LOG.md#L11567)); Unity 438 total, 431 passed, 7 skipped; Codex's added pierce test fails (439 total, 1 failed) ([HL 11639](../../HANDOFF_LOG.md#L11639)). Nothing committed ([HL 11690](../../HANDOFF_LOG.md#L11690)).

### Inferences
- Pieces that match his stated wants and already exist: the class spells and marks (his synergy), defence moves and boosts (his kit of 3, "what player has not to worry"), per fighter nerve and lives (his nerve sync and Hades rule), chief kits with Glory (his boss rule), the raid chief's free Hold.
- Pieces the evidence ties to his confusion: learned feints and wait outs on every beat (rules 8, already cut back), enemies running the full player kit (rules 12, cut by rules 13), early frenzy disabling front line boosts, many word labels.

### Gaps
- I did not run the sim or tests; numbers come from constants, header comments and the handbacks.
- Rules 13 behaviour and its tests are not handed back yet.

## 7. The 8 Dec 2026 visa slice: scope and deadline

### Takeaway
The slice is due 8 Dec 2026 for the Innovator Founder endorsement (pack ready 1 Dec), with a player test gate on 30 Nov and a freeze on 7 Dec in the 13 Oct plan. Its scope grew from "core loop plus one transform" to a full run with a road map and two tiers of random tribes; on 23 Oct Claude and Codex proposed freezing new mechanics until the readability gates pass, which the 24 and 25 Oct rule work then overtook.

### Cited Findings
- 1 Oct **[JACOB]**: "The 8 Dec visa slice: Fallen Star; three tribes (the Clarth, a raider tribe whose chief is the rival, a third tribe at war with the raiders); five classes (Warden, Marksman, Skirmisher, Trapper, Medic); about 20 to 30 minutes." ([DL 70](../../DESIGN_LOCKS.md#L70))
- 2 Oct: "the core loop plus one transform: fight, nerve break, convert, one Pal (the Butcher), one forged gear piece, and a chief who runs and comes back smarter." ([DL 113](../../DESIGN_LOCKS.md#L113))
- 10 Oct: "The 8 Dec slice is one run: camp, Bloodridge, Skullpeak, then the first Alpha hunters." ([DL 198](../../DESIGN_LOCKS.md#L198)) 18 Oct: a run now meets Bloodridge or the cave tribe, then Skullpeak or the sea tribe, then the hunters. ([DL 405](../../DESIGN_LOCKS.md#L405))
- 13 Oct additions by his picks: path map, events, formation, chief taunts, objectives, omens, arena hazards, camp upgrades if there is room; sprints to 22 Nov, tuning 23 to 29 Nov, "Gate 5, 30 Nov: the player test. Gate 6, 7 Dec: freeze"; "If late, cut arena hazards first, then camp upgrades, then omens." ([DL 299-310](../../DESIGN_LOCKS.md#L299))
- 8 Oct flag: "building the full loop by 8 Dec is the biggest risk on the list". ([HL 5208](../../HANDOFF_LOG.md#L5208))
- 23 Oct plan Jacob asked to log: Gates 0 to 6 in order "with new mechanics frozen until they pass. No road sprint, no more bosses or beasts"; Jacob plays the locked route after Gates 1 and 2, "then finds five people who have never seen the game, before 8 Dec"; two strangers play after Gate 2. ([HL 10586](../../HANDOFF_LOG.md#L10586))
- Codex's 24 Oct estimate: 24 to 40 working hours for an integration ready current slice; "The literal full DESIGN_LOCKS scope cannot honestly be promised as one wiring sprint." ([HL 10791](../../HANDOFF_LOG.md#L10791))
- Visa: endorsement submission target 2026-12-08, pack ready 2026-12-01, current visa expires 2027-02-06; endorser Innovator International; evidence method "one challengeable claim tested three ways", including player evidence (understanding, deliberate use, value). ([memory: project_innovator_visa](C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/memory/project_innovator_visa.md))
- Innovation claim to show: tribes that learn from real fights with a replay of why. ([DL 69](../../DESIGN_LOCKS.md#L69))

### Inferences
- From 25 Oct there are about six weeks to 8 Dec and five to the 30 Nov player test; the combat must read for strangers by then, since player understanding is part of the visa evidence.
- The 23 Oct freeze on new mechanics was overtaken by rules 8 to 13 in two days; the hunters, other raid chiefs and several 20 Oct hybrids are now parked or paper, which shrinks the realistic slice to the Bloodridge route plus Skullpeak.

### Gaps
- No updated slice scope after the 25 Oct parking of the hunters (the 10 Oct and 18 Oct locks still end a run on the hunters).
- Whether the five outside players have been found is not recorded.
