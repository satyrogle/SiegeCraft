# PixelLab briefs: the 8 Dec slice cast

Status, 3 Oct 2026 (evening): Clarth and Bloodridge are locked, turned into 8 directions and cut to 32 colours. Skullpeak has no approved look yet. The three chiefs and the Butcher are not drawn yet. The 2 Oct beach look (sunglasses, surfboards, small horns) is dead.

## Jacob's rules

- Go easy on generations. Every generation must count, and one generation answers one question.
- Lean and toned, "hot and menacing", light battle gear, one signature piece of identity per unit. Bloodridge is the deliberate exception: truly scary.
- Each tribe is its own race, mixing several tribal or advanced races from different films and games, never a copy of one source. Check every draft for a single famous lookalike (rejected: Dek's species, an old god, a World of Warcraft character).
- The one second screenshot rule: a gamer gets the look in one second.
- Size ladder: the tank is biggest and most brutal (128 px), melee damage dealers are medium (112 px), ranged and support are small (96 px).
- Every battle sprite is cut to 32 colours, with one palette shared by all eight of its directions.

## Recipe that held up

- Front view first with `create_image_pro_flash` (5 generations at 96 px, 6 at 112 and 128). Turn only what Jacob approves with `create_character_pro_flash` and `source_image_id` (2 generations).
- To change part of an approved drawing (weapon swap, a bit more bulk), `edit_image_pro_flash` with `method: text` keeps the figure and pose (5 generations at 96 px, 6 at 112 and 128).
- To put a mood back on an approved drawing, the same edit: first list everything to keep, then the mood words, then what changes. A 128 px reference image will not fit a smaller canvas, so the smaller units get the same words instead.
- To move a unit up the size ladder, draw it again with `create_image_pro_flash` at the bigger canvas, with the old drawing as `style_image`.
- View `low top-down`, no background. Seeds: Clarth 20261006, Bloodridge 20261007 (Beksiński rounds) and 20261008 (Medjai and the locked edit), Skullpeak 20261009 onwards, one per round.
- Wording that worked: "realistic heroic proportions about seven heads tall, small head, long legs. Not chibi, not cute, not bulky", "light battle gear only, nothing heavy", "his one signature piece: ...", and "stays clearly visible from every side" when turning.
- Wording that failed: "classic handheld tactics battle sprite" drew chibi folk; naming a famous monster's traits drew that monster; weapons made from monster body parts read as orange blobs; describing an artist's style by its traits still copied his objects (for Giger: hoses, ribbed black chrome, long heads), so describe the idea instead.
- Artists are described by their traits, never named or fed in. Beksiński became "in the mood of Polish dystopian surrealist painting: decay, dread, bone, rust and ash, a dusty sepia, ochre and burnt red palette".
- Battle facings: the three quarter views (`south-east`, `south-west`). The pure side views dropped hand held weapons.
- Idle: `animate_character`, template `fight-stance-idle-8-frames`, mode `skeleton-v3`, one direction, about 3 generations, frames at 128 px. The big shapes hold; small texture flickers a little.
- Colour: `KS_PixelLocalV01/tools/aseprite_reduce.lua` in Aseprite batch mode. Stitch the 8 directions into one strip, cut it to 32 colours, slice it back, so every direction shares one palette, then check each outline against the source. Outputs: `KS_PixelLocalV01/aseprite/battle_32/` (PNG per direction plus an `.aseprite` file per unit).

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

## Skullpeak (no approved look)

Sharpshooters at war with the raiders; the chief rides a giant vulture. Jacob's direction, 3 Oct: Giger is the reference for scope, not for his actual designs. Humanoid beings who "infused mechanical properties into their body", never alien looking, "something very different".

Rounds so far, all 128 px tanks with three options each (18 generations a round):

- Round 1 (seed 20261009, `pixellab/skullpeak/`): armoured biomech brutes (skull helmet, bone crown, breath mask). "Not Giger enough."
- Round 2 (20261010, `skullpeak/round2/`): glossy black biomech creatures. "Abominations." One read close to the Alien films.
- Round 3 (20261011, `skullpeak/round3/`): a human in a Giger tube suit on the Clarth body. "Giger ain't landing." One read close to Bane.
- Round 4 (20261012, `skullpeak/round4/`): a people first with Giger only as the finish (gunslinger, steppe eagle hunter, samurai gunner). "Wasting credits."
- Round 5 (20261013, `skullpeak/round5/`): the body itself rebuilt with machine parts grown in, human face and warm skin kept: scope eye, steel bones, piston muscles. Pick not recorded yet.

## Still to make

- Skullpeak (5 units) once a look is approved.
- The three chiefs on their mounts, and the Butcher with his transform (cult, not native: our own look, war and rage traits).
- Aseprite: calm busy textures (the Clarth Trapper's checkerboard) and export sprite sheets for Unity.
- Idles for the battle sprites in three quarter view.
