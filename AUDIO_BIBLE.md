# Kindred Siege audio bible

Draft 1, 14 Oct 2026, references updated 15 Oct. Written by Claude from `DESIGN_LOCKS.md` and Jacob's reactions in chat on 13 to 15 Oct, for Jacob to keep or kill line by line. Nothing below is approved until Jacob keeps it.

Tags:
- **[LOCKED]** Jacob's own words or a locked design call. The source is given.
- **[PROPOSAL]** Claude's suggestion. Keep it, change it or kill it.
- **[OPEN]** Needs Jacob's ear or a reference before anyone decides.

---

## 1. Direction

- [LOCKED] "Dark and grim but with insane humour, the whole game is like that." Jacob, chat, 13 Oct.
- [LOCKED] Funny everywhere, in the spirit of Ironhide's Kingdom Rush. Only the Sovereign is truly scary. `DESIGN_LOCKS.md`, 2 Oct.
- [LOCKED] Bloodridge is truly scary too, by Jacob's choice (3 Oct). Their titles and names stay grim (12 Oct).
- [PROPOSAL] The rule that follows: the world sounds heavy and grim, and the joke comes from timing and contrast, not from silly noises everywhere. A huge grim build up that lands on something petty. A beat of dead silence before the punchline. A hit that is a bit too meaty, a scream that goes on a bit too long.
- [OPEN] How mad. The 13 Oct batch tried three levels for every sound: grim, grim with a twist, full mad. Jacob has not picked a level.

### References

[LOCKED] Jacob, 14 and 15 Oct. Each reference owns its own part of the game, so no two styles get blended into one sound.

| Part of the game | Reference |
|---|---|
| Everywhere, underneath | Predator: Badlands. "This planet is more hostile than a grizzly bear." |
| Battles | Old school Contra and Double Dragon |
| Map, camp and menus | The Diablo 2 lofi mixes with the beats taken out, plus Fallout 1 and 2 |
| The Pals | Dave the Diver, for their personalities |
| Camp upgrades | Upgrade camps from various roguelike games |
| Tone, especially the hits | Kingdom Rush |

- [LOCKED] Retro, but no chiptune. Chiptune breaks the look. Jacob, 14 and 15 Oct.
- [PROPOSAL] Claude's reading of "old school but not chiptune": the arcade and Mega Drive sound of that era (arcade Double Dragon and Contra, Contra: Hard Corps, Streets of Rage 2). FM synth with crunchy sampled drums, not the 8 bit NES versions.
- [PROPOSAL] Diablo 2 and Fallout 1 and 2 are both late 90s with the same dark mood, so the calm screens stay one world.
- References are for feel only. Never copy their music, and never name them in an AI prompt (section 9).
- [OPEN] Fallout 1 and 2 and Diablo 2 are slow and droney, which is close to the eerie drones rejected in section 2. Jacob chose them anyway. The first takes will show which way it goes.

## 2. Already rejected

[LOCKED] Jacob's reactions on 13 Oct. Nothing on this list comes back.

- Eerie whispering and drones: "so eerie".
- Screechy, hissy top end: "straight up screechy".
- Muddy rumble where you cannot tell what the sound is: "idk what".
- Anything that sounds machine made: "feels super computer generated".
- A style guessed from documents with no direction from Jacob: "I haven't given you direction, that's why you're making shit".
- Two reference styles blended into one: "I think I chose the wrong hybrids".

Every batch made on 13 Oct was "not it": local Stable Audio 3 and `ACE-Step` (passes 1 and 2), then ElevenLabs three times (plain prompts; Kingdom Rush crossed with Diablo plus metal and chiptune; the grim to mad ladder). They are kept as reference in `C:\Users\jacob\ComfyUI-Shared\output\ArtLab\` (`audio`, `audio_v2`, `audio_eleven`, `audio_eleven_dir`, `audio_grim`), each with a `manifest.json` of prompts and settings.

## 3. Pillars

[PROPOSAL]

1. **Read the fight by ear.** An auto battle moves fast and nobody can watch every fighter. Every moment that changes a decision (an order ready, a big hit winding up, the parry window, a nerve snap, a chief about to break) gets a sound you would know with your eyes shut.
2. **Grim weight, comic timing.** See section 1.
3. **Every tribe sounds like itself,** so a tribe can be recognised, and collected in the codex, by ear.
4. **Easy on the ears.** A run lasts 30 to 40 minutes. Nothing heard hundreds of times may tire.

## 4. The cast

Facts are [LOCKED] from `DESIGN_LOCKS.md`. Every "sound" line is a [PROPOSAL].

**The cult, the player's people.** Cyborg flesh beings who worship old gods; cunning, vile and sure they are the top predator (2 Oct).
Sound: wet flesh plus machine. Smug. Their sounds are a little too pleased with themselves, like a satisfied servo whirr after a kill.

**The leader.** The player, kept quiet (2 Oct). His weapon is a bracer that steers a small relic over his palm with his nerve impulses (10 Oct).
Sound: never speaks. The relic gets one small hum.

**The four Pals,** all at camp as upgraders (10 Oct). Reference: Dave the Diver (section 1).
- Butcher: war and rage expressed as cold command; forges weapons and gear. Sound: a single controlled hammer strike, the forge breathing. He never shouts.
- Glutton: rot and endurance; always asleep, a patient, unsettling, carefree funny uncle; armour and body augments. Sound: his snore is his signature, and it changes when you buy something from him.
- Shade: change and tricks; always levitates; the priest shaman whose upgrades unlock orders. Sound: a low levitation hum and a ritual chime.
- Whisperer: excess and charm; wild, feral astral goth beauty; skins and regalia. Sound: warm and inviting, with one note that bends wrong. Beautiful, never a creepy whisper (see section 2).

**The Sovereign.** Truly scary; she said "boo hoo you suck" while ending half the army (2 and 7 Oct).
Sound: the one place the game goes quiet. Everything drops out before she acts. Her humour is cruel and casual, never goofy.

**The Sovereign's hunters.** A faceless mercenary company with no names (12 and 13 Oct).
Sound: uniform and mechanical, no voices. One march beat so you know they have arrived.

**Chiefs.** Each has a deadpan title (12 Oct). Chief taunts come in Sprint 4 (13 Oct).
[OPEN] Are taunts voiced, or text with a sound?

## 5. Tribe sound kits

[LOCKED] 24 tribes: 23 plus the Sovereign's Alpha tribe, across 7 tiers (30 Sep). Each tribe is its own race (3 Oct). Each reacts differently under threat (13 Oct), and three of one tribe bring its doctrine (13 Oct). Every tribe you win goes into the codex for good (10 Oct).

[PROPOSAL] Every tribe gets the same kit, so 24 tribes stay affordable:
- **Material:** what its gear is made of, which is what its hits sound like.
- **Signature:** one sound only that tribe has.
- **Battle layer:** a rhythm that joins the battle music when that tribe fights.
- **Capture sting:** plays when you capture its chief. It doubles as that tribe's codex sound.
- **Reaction cues:** one short sound per doctrine or reaction.

The three tribes designed so far. Looks are [LOCKED]; sounds are [PROPOSAL].

| Tribe | Look and behaviour (locked) | Material | Signature | Battle layer |
|---|---|---|---|---|
| Clarth | War hunters of the Broken Wastes. Gear from the sand shark they hunt: jaw shield, spine rifle, fin blades, tooth net, tooth totem staff. They kite and trap your back row. | Bone, hide, sand | A bone hunting horn | Fast hand drums |
| Bloodridge | Egyptian Medjai in Beksiński's mood: burial bandages, corroded bronze, lava through cracks. Khopesh, bronze flail, ember lantern sceptre. They rally when one falls and guard the shaman. Truly scary. | Corroded bronze, burial cloth, embers | A bronze sistrum rattle over a low ember roar | Slow, heavy frame drums |
| Skullpeak | People who grew machine parts into their own bodies: steel bones, turquoise glow, one bionic arm each (grapple claw, arm blade, hook chain, arm rifle, injectors). They hunt your most hurt fighter. | Polished steel, hydraulics, servos | A hydraulic hiss and a grapple chain zip | A tight, mechanical rhythm |

- [PROPOSAL] Bloodridge's own sounds carry no jokes, because they stay scary. The humour comes from your side reacting to them.
- [OPEN] Kits for the other 20 tribes, once those tribes are designed.

## 6. Music and the planet

- [LOCKED] **References,** 14 and 15 Oct (section 1): Predator: Badlands everywhere underneath; old school Contra and Double Dragon for battles; the Diablo 2 lofi mixes without beats, plus Fallout 1 and 2, for the map, camp and menus; Dave the Diver for the Pals; roguelike upgrade camps for camp; Kingdom Rush for tone. Retro, no chiptune.
- [PROPOSAL] **The hostile planet layer,** from the Predator: Badlands reference: an ambience under every screen, so something always feels like it is hunting nearby. Distant roars, things moving just out of sight, wildlife fighting each other. Even camp has the planet growling a little. The music sits on top of it.
- [OPEN] **The exact style** inside those references, heard on real takes. Styles tried on 13 Oct with no pick: dark fantasy ambient, light orchestral, folk, desert caravan, metal over orchestra, chiptune metal, hard rock, calm chiptune, mellow rock, grim battle metal, humppa folk metal, dark circus, slow dark march, macabre waltz, creepy carnival.

[PROPOSAL] How the music works, whatever the style:
- **One shared battle bed** for the whole game. Each tribe adds its battle layer on top (section 5).
- **Intensity in layers.** Calm, fighting, then chief or big moment. Layers fade in and out on one timeline. Never a hard cut.
- **Loops** of 45 to 90 seconds, cut on the bar, with a crossfaded seam.

[PROPOSAL] Music states, using the screens in the locks:

| State | When | Loops | Intensity |
|---|---|---|---|
| Intro | The joke names and the trailer comic | No | Builds to the Sovereign |
| Main menu | The town menu | Yes | Low |
| Camp | Between runs, with the Pals at their stations | Yes | Low |
| Path map | Planning the road through a tribe | Yes | Low to medium |
| Event | A short scene with choices | Yes | Low |
| Fight | A path fight | Yes, in layers | Medium, rising with the fight |
| Chief fight | The fight that ends a tribe's road | Yes, in layers | High, with that tribe's battle layer up front |
| Hunters | The mercenary company's fight at the end of a run | Yes | High, their march |
| The Sovereign | Any moment she appears | No | Silence first |
| Result | Won, lost, captured, killed, run over | No, stingers | Resolves |

- **Stingers:** fight won, fight lost, chief captured (the tribe's capture sting), chief killed, Princess gear, run over, new codex entry.

## 7. Sound effects by moment

Priority: **must** means the fight cannot be read without it, **should** means it adds a lot, **could** means it's nice. Every priority and note is a [PROPOSAL]. "In code" means the cue already exists in `Assets/Scripts/Slice/CampaignArtSound.cs`. Cues marked "13 Oct" were added that day and are not compiled or tested.

**Orders**

| Moment | What it tells the player | Priority | Notes | In code |
|---|---|---|---|---|
| Order accepted (Hold, Fall Back, Focus Fire, Unleash) | Your order went through | must | One sound per order, so they are told apart | Accepted, one shared sound |
| Order refused | Not enough points | must | Short and never annoying | Unavailable |
| Chief order (Eclipse, Frenzy, more to come) | You used a chief's borrowed move | must | Uses that tribe's kit | none |
| Order point earned | You earned a point by parry, dodge, interrupt, kill, break or combo | must | Tiny and satisfying | none |

**Reading the fight**

| Moment | What it tells the player | Priority | Notes | In code |
|---|---|---|---|---|
| Big hit winding up | A big hit is coming, get ready to Hold | must | Its last beat lands on the parry window (the last half second) | Cast, enemy only |
| Perfect Hold, the parry | You timed it | must | The most satisfying sound in the game | none |
| Big hit lands, dodged, halved or parried | How the cast resolved | should | | none |
| Interrupt | You broke their cast | must | | Interrupt, 13 Oct |
| Combo | Parry then Unleash, or another combo | must | Big | Combo, 13 Oct |
| Hit | A normal blow | must | Quiet, short, 3 or more variants, in the tribe's material | Hit, 13 Oct |
| Miss | A blow missed, and MISS shows on screen | must | A whoosh | none |
| Guard | A blow was blocked | should | | Shield |
| Heal | | should | | Heal, 13 Oct |
| Trap armed, trap sprung | | should | | Trap (sprung only), 13 Oct |
| Fighter falls | | must | | Kill, 13 Oct |
| Nerve snap, panic | A fighter cracked and runs | must | Different from rise | none |
| Nerve snap, rise | A fighter cracked and went berserk | must | Different from panic | none |
| Tribe reaction | The tribe is doing its thing | should | From the tribe kit | none |
| Transform, empowered | | should | | none |
| Side breaks | A side gave up | must | | Break |
| Fight over | Won or lost | must | A stinger, section 6 | Win and Lose, 13 Oct |

**The run**

| Moment | What it tells the player | Priority | Notes | In code |
|---|---|---|---|---|
| Chief captured | The catch, the heart of the game | must | The tribe's capture sting, the biggest reward sound in the game | none |
| Chief killed | No tribe, but his Princess piece | must | | none |
| One of yours captured | You lost a fighter, by name | must | | none |
| Gear drop: Pauper, Mid, Princess | How good the drop is | should | Three steps, each bigger | Reward, one sound |
| Strip gear or bank materials | | should | | none |
| Objective met or missed | | should | | none |
| Path map node picked | | should | | none |
| Event choice | | could | | none |
| Omen picked | The run just got harder | could | Grim, with a wink | none |

**Camp, menus and intro**

| Moment | Priority | Notes |
|---|---|---|
| Pal station loops | should | Forge, snore, levitation hum, regalia (section 4) |
| Upgrade bought | should | Glutton's snore changes when he sells you something. Reference: roguelike upgrade camps (section 1) |
| New codex entry | should | The tribe's capture sting, short version |
| Button click and hover | should | Subtle |
| Intro joke names | could | One gag per wrong name (an ice crack for Freeze, a cheese squelch for Cheese, a bee swarm for Bees), then one huge grim hit for Siege |
| The Sovereign | must | Silence first |

## 8. Mix and technical rules

Most of these are [PROPOSAL], from the research report `Research/reports/AI sound and music tools.md`.

- **Priority:** orders, the parry window and captures sit above everything. Hits sit lowest. No more than 2 or 3 copies of one sound at a time.
- **Ducking:** music ducks under stingers and under the Sovereign. The planet layer ducks under fights.
- **Loudness:** aim the whole game at about minus 16 to minus 18 LUFS on PC, with music a few dB under the effects. This is a suggestion, not a standard.
- **Lengths:** repeated combat sounds under 0.5 s, order sounds under 0.4 s, stingers 3 to 6 s, loops 45 to 90 s.
- **Variation:** 3 or more takes of every repeated sound, with random pitch of about 1 semitone either way and random volume of about 2 dB either way.
- **Files:** source WAV at 48 kHz. Effects mono, music stereo. Unity import: effects PCM or ADPCM with decompress on load, music Vorbis.
- **Names:** effects as `sfx_<moment>_<tribe or common>_<take>.wav`, for example `sfx_hit_bloodridge_02.wav`. Music as `mus_<state>_<layer>.ogg`, for example `mus_fight_skullpeak.ogg`.
- **Engine:** Unity 2022.3's built in audio. FMOD and Wwise aren't needed at this size.
- [LOCKED] Sound is presentation only. The deterministic Sim never reads it (`CLAUDE.md`).
- [LOCKED] The frontend in `Assets/Scripts/Slice/` is Codex's. Sound wiring goes through `HANDOFF_LOG.md` (`CLAUDE.md`, 6 Oct).
- Current loader: `Assets/Resources/CampaignArt/Audio/<Cue>.wav`, read by `CampaignArtSound.cs`. Claude's edit of 13 Oct is not compiled or tested, and the names above would replace its cue names.

## 9. Making the sounds

- [LOCKED] Placeholders now, a paid artist later. Jacob, 13 Oct.
- [LOCKED] Get Jacob's direction and references before any pass. Make three genuinely different ideas per sound; he picks by ear. Never refine a guess. Jacob, 13 Oct, and the three options rule.
- [PROPOSAL] **One brief per sound,** for AI or an artist: the moment, what it tells the player, feel words, one reference clip with a timestamp, length, number of variations, and which tribe kit it belongs to.

What the 13 Oct tests showed:
- One real, recordable event per prompt works best. Abstract words like "magic", "UI confirm" and "sparkle" gave hiss or mud on Stable Audio 3.
- The models push timbre words to the extreme. "Muffled", "deep" and "dark" gave mud, and "bright", "shimmer" and "metallic" gave screech. Voice words like "whispering" gave the eerie feel.
- Stable Audio 3 in ComfyUI only behaves when run Stability's own way: a `TrackType:` prefix, their 8 step sigma list, 6 s of padding with `seconds_total`, and a trim. Without that, hits start late and clips end in junk.
- ElevenLabs sound effects cost about 10 credits a second. They come back as interleaved stereo PCM at 48 kHz.

Legal, from the research report (13 Oct):
- Steam's AI disclosure covers sound. Any AI sound that ships must be disclosed on the store page.
- Keep a log for every file: tool, model, plan, date, prompt and edits.
- ElevenLabs paid plans allow commercial use. Their music terms cover indie games under $500k lifetime revenue.
- Stable Audio needs a free registration with Stability, and covers you while yearly revenue stays under $1M.
- Never name real artists, songs or reference games in a prompt.

## 10. Open decisions

[OPEN] Jacob's calls:

1. Music style inside the references in section 1. Needs takes Jacob can hear.
2. How mad the humour gets in sound: grim, grim with a twist, or full mad.
3. Do fighters, chiefs and Pals make vocal sounds (grunts, gibberish, voiced taunts), or stay silent?
4. The Sovereign's sound.
5. Kits for the other 20 tribes, once they are designed.
6. What fills in until the artist arrives: AI, free sound packs made by people, or silence.

## 11. Finish checklist

[PROPOSAL] Audio for the 8 Dec slice counts as finished when:

- Every "must" moment in section 7 has a sound Jacob picked.
- Clarth, Bloodridge, Skullpeak and the hunters each have their kit.
- The music style is picked, the map, camp and fight loops are seamless, and the stingers resolve.
- Nothing from section 2 is in the game.
- Every AI file is logged and the Steam disclosure is decided.
- Jacob has heard it in a real run, not just on a review page.
