# PixelLab briefs: the 8 Dec slice cast

Verified update, 6 Oct 2026 (Codex): the 15 approved fighters and all three chiefs now have breathing idles in both battle facings; all 36 sheets are present under `Assets/Resources/RivalSlice/Sheets/`. The chiefs also have eight rotations. Clarth Warden has three attack experiments in SE/SW and three additional SE idle experiments from 4-5 Oct, outside the game's four-frame idle playback. These experiments are not evidence of owner approval or in-game attack animation. Codex now owns sprite animation as well as frontend binding; see the latest animation entry in `HANDOFF_LOG.md`. The dated 3 Oct status below is historical.

Status, 3 Oct 2026 (night): Clarth, Bloodridge and Skullpeak are approved, turned into 8 directions, cut to 32 colours, given a breathing idle in both battle facings and exported as sheets for Unity (not imported into the project yet). The three chiefs are approved as 192 px fronts drawn from Jacob's own placeholder designs; they are not turned yet. Jacob draws the four Pals himself. The 2 Oct beach look (sunglasses, surfboards, small horns) is dead. 663 of 5000 generations used by 3 Oct.

## Jacob's rules

- Go easy on generations. Every generation must count, and one generation answers one question.
- Lean and toned, "hot and menacing", light battle gear, one signature piece of identity per unit. Bloodridge is the deliberate exception: truly scary.
- Each tribe is its own race, mixing several tribal or advanced races from different films and games, never a copy of one source. Check every draft for a single famous lookalike (rejected: Dek's species, an old god, a World of Warcraft character).
- The one second screenshot rule: a gamer gets the look in one second.
- Size ladder: the tank is biggest and most brutal (128 px), melee damage dealers are medium (112 px), ranged and support are small (96 px). Chiefs are 192 px.
- Every battle sprite is cut to 32 colours, with one palette shared by all eight of its directions and its idle frames.
- The Pals are Jacob's (3 Oct): "Leave the pals to me too". Agents do not draw them. The chiefs are drawn from Jacob's placeholder designs, which set each chief's identity.

## Recipe that held up

- Front view first with `create_image_pro_flash` (5 generations at 96 px, 6 at 112, 128 and 192). Turn only what Jacob approves with `create_character_pro_flash` and `source_image_id` (2 generations).
- To change part of an approved drawing (weapon swap, a bit more bulk), `edit_image_pro_flash` with `method: text` keeps the figure and pose (5 generations at 96 px, 6 at 112, 128 and 192).
- To put a mood back on an approved drawing, the same edit: first list everything to keep, then the mood words, then what changes. A 128 px reference image will not fit a smaller canvas, so the smaller units get the same words, or a 96 by 96 crop of the approved unit as the style image (that worked for the Skullpeak squad).
- To move a unit up the size ladder, draw it again with `create_image_pro_flash` at the bigger canvas, with the old drawing as `style_image`.
- From a placeholder picture (the chiefs): key out its flat background, shrink it to fit the canvas, save it as a 33 colour palette PNG (8 to 12 thousand base64 characters) and pass it as the `style_image` with a `usage_description` that says which parts to follow. Copying palette and detail gave cleaner drawings; copying all four style options stayed closer to the placeholder.
- View `low top-down`, no background. Seeds: Clarth 20261006, Bloodridge 20261007 (Beksiński rounds) and 20261008 (Medjai and the locked edit), Skullpeak 20261009 onwards, chiefs 20261020 onwards, one per round.
- Wording that worked: "realistic heroic proportions about seven heads tall, small head, long legs. Not chibi, not cute, not bulky", "light battle gear only, nothing heavy", "his one signature piece: ...", and "stays clearly visible from every side" when turning.
- Wording that failed: "classic handheld tactics battle sprite" drew chibi folk; naming a famous monster's traits drew that monster; weapons made from monster body parts read as orange blobs; describing an artist's style by its traits still copied his objects (for Giger: hoses, ribbed black chrome, long heads), so describe the idea instead.
- Artists are described by their traits, never named or fed in. Beksiński became "in the mood of Polish dystopian surrealist painting: decay, dread, bone, rust and ash, a dusty sepia, ochre and burnt red palette".
- Battle facings: the three quarter views (`south-east`, `south-west`). The pure side views dropped hand held weapons.
- Idle: `animate_character`, template `breathing-idle`, mode `skeleton-v3`, both battle facings, about 3 generations a direction. Frames come out at 192 px for the 128 px units, 160 px for 112 and 144 px for 96. `fight-stance-idle-8-frames` was tried first and dropped: it squashed the Skullpeak Warden's claw arm.
- Colour: `KS_PixelLocalV01/tools/aseprite_reduce.lua` in Aseprite batch mode. Stitch every cell of a unit (8 turns plus both idles) into one strip, cut it to 32 colours, slice it back, so the whole unit shares one palette, then check each outline against the source. Outputs: `KS_PixelLocalV01/aseprite/battle_32/` (turns) and `aseprite/battle_32_idle/<unit>/` (idles), PNG per cell plus an `.aseprite` file.
- Refine: `KS_PixelLocalV01/tools/aseprite_refine.lua` in Aseprite batch mode snaps alpha, applies hand pixel edits, cuts to a colour count with no dithering, cleans lone pixels and can add a dark outline. The lone pixel clean ate the Clarth Medic's torch sparkles, so it was turned off for the Clarth redo and for Bloodridge. With `noise=0` and a colour count above the sprite's own, it only applies the hand edits.
- Unity export: `KS_PixelLocalV01/unity_export/<tribe>/<unit>_rotations`, `<unit>_idle_se` and `<unit>_idle_sw`, each a horizontal PNG sheet with Aseprite json array data. Preview of every idle facing right: `unity_export/all_idles_facing_right.gif`.

## Clarth (approved 3 Oct)

War hunters: crested bone war masks, red cloaks, toned bodies, bronze bracers, braided hair. Signature pieces come from the sand shark they hunt.

Files: `C:/Users/jacob/ComfyUI-Shared/output/ArtLab/KS_PixelLocalV01/pixellab/clarth_redo/`, turned in `pixellab/battle_ready/clarth_*`, 32 colours in `aseprite/battle_32/clarth_*`.

| Unit | Size | Signature piece | Front image ID | Character ID |
|---|---|---|---|---|
| Warden (tank) | 128 | shark jaw shield, curved blade | 7693e229-d5d9-59c1-aada-49f4c8251599 | d917de88-8dfc-4de2-97f0-4dc46e06cb7d |
| Skirmisher | 112 | fin blades | a0ad5e96-d8aa-50bf-b70c-7aa12cb33c9a | a3aa47e7-da8b-47f5-a04d-dadb14721a65 |
| Trapper | 112 | tooth net, hooked knife | bb155952-fd78-563d-9f83-1e60bae378c0 | 44547cca-99d8-432c-bc41-ab629db97136 |
| Marksman | 96 | spine rifle | 302fa58f-1df3-5ac0-a7de-88bbb78d8f7d | 9e0233b1-4a8a-4d81-99d8-53351b1cf28d |
| Medic | 96 | tooth totem staff | c8ab70b3-7849-5916-8549-b69036c71e66 | 20b62125-093d-41ca-a5d2-eb2f15d3fd25 |

Superseded: the 96 px Warden (front `2ccec800-e64c-52d6-8749-fad02edf9ea6`, character `897259a8-5c93-4b11-9458-150e48166575`, with a west idle) stays the tribe's style reference but is no longer its battle sprite. The 96 px Skirmisher (`d3495d0f`, `63ba11a2`) and Trapper (`8c1bc2c1`, `06b5985e`) are replaced by the 112 px ones.

## Bloodridge raiders (locked 3 Oct)

Egyptian desert guardians after the Medjai, finished in the mood of Beksiński: rotted sepia and ash brown rags, burial bandages on the arms and legs, corroded bronze collars and armbands, ashen skin, lava glowing through cracks. Faceless hoods with ember eyes for the Warden and Trapper; lava tattooed faces for the Skirmisher, Marksman and Medic.

How it was made: Jacob said the clean Medjai had lost the Beksiński vibe. The approved Medjai fronts were edited three ways (recolour only, rot, burnt grave shroud), seed 20261008; he picked the rot, it went onto all five fronts with the same words, and they were turned and cut to 32 colours. "The blood ridge is fine lock it."

Files: fronts and pick sheets in `C:/Users/jacob/ComfyUI-Shared/output/ArtLab/KS_PixelLocalV01/pixellab/bloodridge/beksinski_back/`, turned in `pixellab/battle_ready/blood_rot_*` (sheet `bloodridge_rot_battle_line.png`), 32 colours in `aseprite/battle_32/blood_rot_*`.

| Unit | Size | Signature piece | Front image ID | Character ID |
|---|---|---|---|---|
| Warden (tank) | 128 | Egyptian war shield, khopesh | 41a3510d-933e-5cc2-81a7-f755053ccd81 | b3fef59f-4ba2-4f95-a526-1228e1f8b49d |
| Skirmisher | 112 | twin khopesh | 7564751a-b742-5095-8e0a-b8363c3fdc40 | a1195c8f-03c0-42f8-8f8f-135b8e98d09c |
| Trapper | 112 | hooked bronze flail, fire pots | debefb3e-f126-56f2-a3f8-b8bccbd5bc11 | 720d540c-6f12-49b8-b409-3f84e8102bf2 |
| Marksman | 96 | bronze fitted rifle | f91e1efd-7a33-5c84-baa7-08dbd98e4fd9 | 19b7cc53-fb54-45dd-95ee-433ed538dd10 |
| Medic | 96 | sceptre with an ember lantern | 7b26fe1a-5863-5bd6-b28a-8135755db763 | e7aceef9-d12c-4c53-8817-318de3265c03 |

Known weak spot: facing right (`south-east`), the Warden's shield turns edge on and shows as a thin slab. On red rock the sepia blends in more than the navy did; the bandages and lava keep them readable. Both can be tuned in Aseprite without new generations.

Superseded: the clean black, indigo and gold Medjai of 3 Oct, the sources of the edits (fronts `dc7d9243`, `4ec6c273`, `59dfbf02`, `b3aff00e`, `c401098f`; characters `f69655c8`, `71f4dfb6`, `87edc105`, `76c77a58`, `50ec2daa`; sprites in `battle_ready/blood_*` and `aseprite/battle_32/blood_*`). Their style references were the hood `57c75f52-fc38-548b-8360-09a8b49976b6` and the tattoo `ec999db7-f3e4-56ad-84c2-a13ec046fad5`. Rejected rounds are kept in `bloodridge/` (Beksiński style shrouds with crab part weapons) and `bloodridge/v2/` (the same shrouds, bulkier, with forged weapons).

## Skullpeak (approved 3 Oct)

Sharpshooters at war with the raiders; the chief rides a giant vulture. Jacob's direction: Giger is the reference for scope, not for his actual designs. Humanoid beings who "infused mechanical properties into their body", never alien looking. The approved look: warm human skin with polished steel bones showing through the chest and ribs, a steel crest that stops at the hairline so the face shows, dark slate wraps, a turquoise glow in the seams, and one bionic arm per unit after Bionic Commando, which is each unit's signature piece. "Okay these are good now next step."

Files: rounds in `C:/Users/jacob/ComfyUI-Shared/output/ArtLab/KS_PixelLocalV01/pixellab/skullpeak/` (`round3` to `round6`), squad fronts in `pixellab/skullpeak/squad/`, the refined Warden in `aseprite/skullpeak_refine/` (`warden_claw_refined_v2.aseprite`), turned in `pixellab/battle_ready/skull_*`, 32 colours in `aseprite/battle_32/skull_*`.

| Unit | Size | Signature piece | Front image ID | Character ID |
|---|---|---|---|---|
| Warden (tank) | 128 | bionic grappling claw arm, tower shield | refined front; PixelLab source c2435347-a2a8-5830-b76a-60621174e6f1 | 77cabae1-7a9e-4988-a7d0-eb23fe225939 |
| Skirmisher | 112 | bionic arm ending in a curved blade, crouched | c7b51d12-11f8-588c-90bf-ea1e6140d4b1 | 98ad2fdc-16fe-450a-874b-cf3d48561723 |
| Trapper | 112 | bionic grapple arm with a hook on a chain | 25af990f-b06a-5a78-a69b-7317d9ad5310 | 09091ede-317b-4232-82e5-0fa4413dbe41 |
| Marksman | 96 | rifle built into her arm | 101b6c50-085a-5873-9989-992df644219c | c4c87d4a-cd12-4104-bce4-09ead8c36623 |
| Medic | 96 | injector needles and turquoise vials on her arm | b8f03c88-0ae6-5933-a5b7-e9adc224ffd6 | 318bd563-2eee-4019-90e9-313323f05cb0 |

Flags: the Trapper came with a fur mantle nobody asked for, and a hook on a chain can call to mind Scorpion's spear from Mortal Kombat, though nothing else about him looks like Scorpion.

Rounds, all 128 px tanks with three options each:

- Round 1 (seed 20261009, `pixellab/skullpeak/`): armoured biomech brutes (skull helmet, bone crown, breath mask). "Not Giger enough."
- Round 2 (20261010, `skullpeak/round2/`): glossy black biomech creatures. "Abominations." One read close to the Alien films.
- Round 3 (20261011, `skullpeak/round3/`): a human in a Giger tube suit on the Clarth body. "Giger ain't landing." One read close to Bane.
- Round 4 (20261012, `skullpeak/round4/`): a people first with Giger only as the finish (gunslinger, steppe eagle hunter, samurai gunner). "Wasting credits."
- Round 5 (20261013, `skullpeak/round5/`): the body itself rebuilt with machine parts grown in, human face and warm skin kept: scope eye, steel bones, piston muscles. Jacob picked the steel bones: "2nd one add more giger and moreover you can use bionic commando for amping it up".
- Round 6 (`skullpeak/round6/`): three bionic arms on the steel bones tank (claw arm with the shield kept, rifle arm with the shield kept, an arm that is the shield). Jacob picked the claw: "Do 1 but refine it in aesperite before go". The Aseprite refine opened the face under the crest, cut 41 colours to 30 and cleaned about 330 stray pixels on the ribs and the arm, with no generations spent. Then "Make the rest let's go": the squad was drawn from a 96 by 96 crop of the Warden as the style image, turned and cut to 32 colours.

## Chiefs (approved 3 Oct)

Jacob made a placeholder for each chief ("they are shit but they are most relevant and coherent ones i found") and asked for chieftains drawn with them as the visual identity. Each front is 192 px and stands a head taller than its tribe's Warden. Not turned yet. Mounts as in the locks (sand shark, house sized crab, giant vulture).

Files: `C:/Users/jacob/ComfyUI-Shared/output/ArtLab/KS_PixelLocalV01/pixellab/chiefs_jacob/`. The approved three are copied into `approved/` as `clarth_chief.png`, `bloodridge_chief.png` and `skullpeak_chief.png`. Jacob's placeholders are kept as `jacob_placeholder_<tribe>_chief.webp`, and the keyed style references as `ref_<tribe>_chief.png`.

| Chief | Look | Image ID | How |
|---|---|---|---|
| Clarth | red ochre war paint with white stripes, crested bone crown, long black braids, white fur mantle, red cloak with white sigils, dark armour with bronze trim, long spear with a glowing red blade | d5f0d997-e137-55c0-b32a-bf889796bfb0 | round 1, seed 20261020, "clarth guy is set" |
| Bloodridge | faceless ember hood with antlers, ram skulls on one shoulder, corroded bronze collar, rotted layered rags and bandages, small skulls and charms, lava sun tabard, staff holding a burning ring | f4ebd1d1-1160-55f9-8faa-3ee68748556d | the rot edit of round 1 (aaebd49a-970b-5bc6-bdbe-6e5b0b9d213f, seed 20261022), edit seed 20261025, "option 1 for bloodridge is fine" |
| Skullpeak | a woman with a white undercut and a scarred blind eye, three breasts in a fitted black top framed by steel ribs with a turquoise glow, torn black cape, bone gauntlets, turquoise lit sword, steel bionic leg | 23e5fdd5-8cc8-5165-b2e1-88061ae91f35 | an edit of round 1 (ce281cb5-514b-550e-ad2e-73d12a1095b8, seed 20261021), edit seed 20261024, "its okay just use this one" |

Notes:
- The Clarth placeholder read as a Predator (dreadlocks with rings, a crested tusked face, a tech spear). The approved chief has a human face under a bone crown instead.
- Bloodridge round 1 was "too simple and lost his rottenness". Two fixes were drawn: the rot edit (picked) and a redraw copying the placeholder's full style (`bloodridge_chief_r2_redraw.png`, 48b9fcb8-52d3-5e56-b3c3-9de2c10858ad, not picked).
- The three breasts are Jacob's call. Claude Code's auto mode check blocked the PixelLab edit, so Jacob switched to ask mode and approved it by hand. In the approved drawing the three read only faintly. Three hand painted versions where they read clearly (black, black with a turquoise underwire, polished steel with a turquoise underwire) are kept in `KS_PixelLocalV01/aseprite/chiefs/skullpeak_chief_three_*` and were not picked.

## Set aside

- `pixellab/chiefs/` and `pixellab/butcher/`: chief and Butcher drafts made before Jacob's placeholders. Not used.

## Still to make

- Import the Unity sheets into the project.
- Aseprite: calm busy textures (the Clarth Trapper's checkerboard, the Skullpeak ribs, the chiefs' stray pixels) by hand.
- Turning, colour cuts and idles for the three chiefs, and their mounts, when Jacob asks.
- Attack, hit and death animations, once the fight needs them.
