# Environment pixel art: tilesets, props, and tile maps versus painted stages

Scope: production practice for tiles, props and environment pieces, aimed at Kindred Siege (one static screen, stage about 1376 x 768 at 1x, three quarter view fighters about 120 to 190 px tall). Every claim has a link. Claims marked "via search summary" come from a search result summary where the page itself was not opened or refused access, so treat them as weaker. Inferences are kept apart from findings. Sources are paraphrased, not quoted.

## 1. When do games build a scene from tiles, and when do they paint the whole screen as one piece? Which suits a static single screen battle stage or hub with three quarter view characters?

### Takeaway
Tiles are a memory, reuse and editing tool for large, scrolling, grid based worlds. Screens that are seen whole and never scroll (JRPG battle backdrops, adventure game rooms, fighting game stages) are composed as one scene, usually split into a floor layer and a back layer and drawn for the battle camera. For a static battle stage with three quarter fighters, the documented standard is a composed backdrop, not a top down terrain tileset.

### Cited Findings
- Tilemaps exist for memory and performance: a level is built from small images used many times instead of one big image, and the same grid doubles as a logical grid for collision, pathfinding and level editors. [MDN, Tiles and tilemaps overview](https://developer.mozilla.org/en-US/docs/Games/Techniques/Tilemaps)
- MDN calls a tilemap that fits the screen static and gives grid gameplay examples for it (Pacman, Arkanoid, Sokoban); it also lists prerendering a map into one texture, or into prerendered chunks used as big tiles, as rendering options. [MDN, Tiles and tilemaps overview](https://developer.mozilla.org/en-US/docs/Games/Techniques/Tilemaps)
- RPG Maker MZ builds field maps from 48 x 48 tiles, but its battle backgrounds are whole images: a battleback is 1000 x 740 px, the battlebacks1 folder holds mainly floors and battlebacks2 mainly walls, combined freely; the top of the image is cropped in front view battles and the bottom in side view battles. [RPG Maker MZ Help, Asset Standards](https://rpgmakerofficial.com/product/MZ_help-en/01_11_01.html)
- Slynyrd's battle background lesson for a turn based JRPG builds the backdrop as one landscape on a 192 x 144 canvas: solid horizontal colour bars set the horizon, then sky and ground are split into more bands; monsters face front across the middle, and the bottom is left open for heroes seen from behind. [Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds)
- Same lesson: backgrounds should serve the scene without distracting from play; real time action games may need muted or simplified backgrounds so sprites contrast, while turn based RPGs can lean toward rich scenery. Each scene used 15 colours under a self imposed 16 colour cap, and depth is carried mostly by colour (nearest plane most saturated and contrasty; further planes less saturated, lighter, and shifted toward the sky hue). [Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds)
- Slynyrd says sizing assets to one tile grid helps cohesion even when the environment does not use actual tiles. [Slynyrd Pixelblog 55](https://www.slynyrd.com/blog/2025/3/24/pixelblog-55-top-down-character-animation)
- Arcade fighting game stages were composed scenes stored in hardware tiles: the CPS1 board has three tilemap layers (8 x 8, 16 x 16 and 32 x 32 tiles) and one sprite layer; SF2's Ryu stage uses nearly 200 sprite tiles for detail, and Guile's stage draws part of the jet with sprite tiles, which the author thinks happened because the artists ran out of background tile space. SF2 used the 16 x 16 layer for a per line floor parallax by scrolling tiles sideways line by line. [Fabien Sanglard, CPS1 graphics](https://fabiensanglard.net/cps1_gfx/index.html)
- A modding forum post says SF2 can warp its floor freely because both fighters always stand on the same depth line; the trick breaks in games where characters move in depth. [Chronocrash forum, via search summary](https://chronocrash.com/forum/threads/scroll-property-elaboration.6047/post-82669)
- Single screen adventure rooms are the classic painted background case: Mark Ferrari was background artist on Loom and did the Monkey Island 1 backgrounds, and was brought in to do most Thimbleweed Park backgrounds (early rooms were still wireframes). [Thimbleweed Park blog](https://blog.thimbleweedpark.com/mark_ferrari.html)
- Modern pixel games add a lighting layer over the art: Eastward's Feng Ye says the lighting is general deferred shading so designers can put more lights in a level and tune ambient tones for mood, and that sprites are placed and oriented to look 3D [Red Bull, Eastward interview](https://www.redbull.com/my-en/eastward-interview); Eastward's scenes are laid out in Pixpil's own engine, Gii, which has a Unity like scene editor [Game Developer, Road to the IGF: Eastward](https://www.gamedeveloper.com/disciplines/road-to-the-igf-pixpil-s-i-eastward-i-).
- Sea of Stars' plans before release listed Unity, pixel perfect rendering, and fully dynamic sprite lighting with normal maps and shadow receiving (plans, not confirmed final features). [Niche Gamer, via search summary](https://nichegamer.com/2020/03/19/the-messenger-prequel-rpg-sea-of-stars-enters-kickstarter-launches-2020-for-pc/)
- A forum reply (low authority) says memory for large painted sprite images should not be an issue on modern computers. [Unity Discussions](https://discussions.unity.com/t/confused-by-tile-map-design-lighting-and-map-background-design-for-2d-rpgs/675265)

### Inferences
- Rule of thumb from the above: use tiles when the space scrolls, is large, is reused across many maps, or needs a gameplay grid; compose when the screen is static and read as a picture. Tile based RPGs switch to composed battlebacks for exactly this case (RPG Maker).
- For Kindred Siege the nearest documented models are the JRPG battleback (a floor image plus a back image, composed for the battle camera) and the fighting game stage (one composed scene, with extra sprites for animated detail). If a grid is used, it is for assembly or storage, as on CPS1, not the visible look.
- Kindred Siege fighters stand in rows at different depths, so the SF2 floor warp does not apply, and with a static camera there is no parallax to fake. A still, receding floor painted for the fighters' camera is enough, with ambient animation as separate small sprites.
- Cost check: 1376 x 768 is about 7 times wider and 5 times taller than Slynyrd's 192 x 144 battle canvas, roughly 38 times the area. A fully hand pixeled single image is a big job; the middle path the sources document is large unique chunks, or prerendered chunks treated as big tiles (MDN), stacked in layers (MDN, Stardew), with sprites for detail (CPS1).
- A hub that is one static screen follows the same logic as a battle stage; the adventure game room is the model.

#### Why the rejected pass fails, in these terms (inference)
- A top down Wang terrain set is the scrolling world tool. Used for a static stage, it brings the tile look (grid, repeats, transition bands) and none of the benefits (no scrolling, no level editing at scale, no memory pressure at this size).
- The composed stage approach draws floor, plateau and props together, in one camera, one light and one palette, around where the fighter rows stand.

### Gaps
- No primary developer source found on how Hyper Light Drifter, Sea of Stars, Celeste or Octopath Traveler split tiles and unique painted pieces; no GDC Vault or 80.lv source was retrieved.
- No source found on Garou or Street Fighter III background production; FF6 battle background sheets on The Spriters Resource were not located.
- No source recommends a tile size or chunk size for a 1x stage at 1376 x 768.

## 2. How do artists avoid visible repetition (variant tiles, macro patterns, clutter and decal layers, lighting overlays)?

### Takeaway
Hide the pattern inside calm, even textures; keep fill tiles near flat and put detail at edges and in rare variants; place variants randomly with low weights and flips; stack partial textures and props on separate layers that cross tile seams; use larger scale noise or non periodic (Wang) tilings so patches do not line up; then lay lighting and colour variation over the top.

### Cited Findings
- Slynyrd: tile making is harder than it looks; the job is hiding patterns inside consistent, non distracting textures; keep visual weight even across the tile; let clusters overlap the edges and wrap around to hide seams; define a few key clusters and repeat them for consistency; mirror tiles for new orientations; one grass tile alone is monotonous, so mix flat areas with textured ones; add occasional flower variants; negative space helps; do not place key clusters against each other (corner to corner contact is fine). [Slynyrd Pixelblog 20](https://www.slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles)
- Slynyrd: a busy texture next to another busy texture tires the eye, so pair busy with airy; a few colours make a rich texture, too many make it blurry; strongly contrasting colours in a pattern make noise. [Slynyrd Pixelblog 20](https://www.slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles)
- Slynyrd: watch seams live while drawing and keep a zoomed out preview open at normal viewing distance. [Slynyrd Pixelblog 20](https://www.slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles)
- Slynyrd: start from a base texture that loops on all four sides; cut side and corner pieces by shaving the base; remove backgrounds from partial textures (grass, rocky dirt) and stack them as layers rather than baking every combination into tiles. [Slynyrd Pixelblog 43](https://slynyrd.com/blog/2023/3/26/pixelblog-43-top-down-tiles-part-2)
- LPC style guide: keep tile detail simple with more detail at edges; centre tiles should be one colour or a very subtle pattern; add occasional detail tiles to break a repeating tile; avoid noise; dither sparingly if at all. [LPC Style Guide](https://lpc.opengameart.org/static/LPC-Style-Guide/build/styleguide.html)
- Tiled: when several tiles share a terrain configuration, the terrain brush picks among them with equal chance by default; each tile has a probability; the manual's example gives decorative variants 0.01 (100 times rarer than a normal tile); flips and rotation can be switched on to multiply variants, but vertical flip should stay off when tiles carry vertical shadows. [Tiled manual, Using Terrains](https://doc.mapeditor.org/en/stable/manual/terrain/)
- LDtk added perlin noise to auto layer rules so a rule fires only in random areas (patches at a larger scale than one tile). [LDtk 0.1.5 release notes, via search summary](https://github.com/deepnight/ldtk/releases/tag/v0.1.5-alpha)
- Graphics research: Wang tiles (squares with coloured edges that must match their neighbours) fill the plane with non repeating texture from a small set; tiles can carry texture, point distributions or geometry; coding the corners lets objects cross tile edges; tiles can be prelit. [Cohen et al., Wang Tiles for Image and Texture Generation, SIGGRAPH 2003, via search summary](https://graphics.uni-konstanz.de/publikationen/Cohen2003WangTilesImage/index.html)
- Layers: a rock or bush can sit on its own layer over any terrain instead of needing a tile per combination, giving a richer world with fewer tiles; sprites drawn between layers can walk behind trees. [MDN, Tiles and tilemaps overview](https://developer.mozilla.org/en-US/docs/Games/Techniques/Tilemaps)
- Stardew Valley maps (16 x 16 tiles, edited in Tiled) use layers Back (terrain, water), Buildings, Paths (flooring, grass, debris), Front (drawn over the player when the player is north of it, behind when south) and AlwaysFront (always over the player, used for foliage cover); extra numbered layers can be added for rendering only. [Stardew Valley Wiki, Modding:Maps](https://stardewvalleywiki.com/Modding:Maps)
- Lighting over tiles: Eastward's deferred lighting lets designers add many lights and tune ambient tones. [Red Bull, Eastward interview](https://www.redbull.com/my-en/eastward-interview) A player analysis of Hyper Light Drifter reads its look as soft light over colour limited pixel art, with the screen glow done by shaders (community opinion, not the developer). [Steam discussion, via search summary](https://steamcommunity.com/app/257850/discussions/0/357284767236620083)

### Inferences
- The fill tile repeats the most, so it must be the quietest tile in the set; the character of the ground goes into edges, rare variants and the decoration layer.
- Kindred Siege numbers: 1376 / 64 = 21.5 columns and 768 / 64 = 12 rows, about 258 cells. A two terrain corner set has exactly one all sand tile (one of its 16 corner combinations), so with no variants every interior cell is the same image, and any feature in it forms a lattice across the stage.
- A 64 px tile holds 16 times the pixels of a 16 px tile, so it has more room for recognisable features; the bigger the tile, the harder the job of hiding the pattern.
- Decorations placed off grid as free sprites (pebbles, cracks, bones, sand ripples) hide seams better than detail baked into tiles, because they cross tile edges (the same reason Cohen's corner coding exists).

### Gaps
- No source gives a target number of fill variants or ideal weights beyond Tiled's 0.01 example.
- No primary source found on Celeste's decals or on how Celeste's autotiler picks variants.
- No source measures at what scale or coverage repetition becomes visible.

## 3. How do props stay consistent in perspective and lighting so they do not look like stickers? What are the rules for shadows, outlines, palette and scale?

### Takeaway
One projection, one light, one palette logic and one size grid for everything. Props need shadows (drop and contact) coloured from the ground they sit on. Characters get the strongest outline and their own palette so they separate; props and tiles use softer, coloured outlines.

### Cited Findings
#### Perspective
- Slynyrd: many projections exist, but in most cases they should not be mixed in one scene; uniformity beats realism; if every element follows the same rules, even an odd projection looks good. [Slynyrd Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1)
- LPC: the camera is top down at about 60 degrees and orthographic (things do not shrink with distance); perspective tricks on props or tiles are wrong; characters are drawn in the same perspective as the tiles. [LPC Style Guide](https://lpc.opengameart.org/static/LPC-Style-Guide/build/styleguide.html)
- A gamedev.net critique of JRPG assets: trees read as flat billboards; express the camera angle in every sprite, use one camera angle for every object, and draw from plan views so a tree, a character and a castle can be measured against each other. [gamedev.net thread, via search summary (page refused access)](https://gamedev.net/forums/topic/676165-critique-my-jrpg-graphics/5277204/)

#### Light
- Slynyrd: most depth comes from lighting; light can come from any corner, but a top corner is most natural. [Slynyrd Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1)
- LPC: light mainly from above, any side light from the left but only a little; a large lighting difference between the top and the sides of objects. [LPC Style Guide](https://lpc.opengameart.org/static/LPC-Style-Guide/build/styleguide.html)

#### Shadows and grounding
- Slynyrd: keep cliff drop shadows conservative so they do not fight sprites; one or two wall faces only, minding the light source, no longer than one tile, all the same length whatever the wall height (not realistic, but convincing); shadows help walls blend into dirt, grass and sand. [Slynyrd Pixelblog 43](https://slynyrd.com/blog/2023/3/26/pixelblog-43-top-down-tiles-part-2)
- LPC: drop shadows are one colour (#322125 at 60% opacity), characters included; props should have shadows or they will not look part of the scene. [LPC Style Guide](https://lpc.opengameart.org/static/LPC-Style-Guide/build/styleguide.html)
- A commercial 16 x 16 top down prop pack ships every prop twice: transparent, and grounded with a contact shadow on its ground. Its reasoning: a shadow must darken something, so on a transparent prop it can only be an opaque dark blob, which under a bush reads as a hole; the shadow colour comes from the material it falls on (that material's darkest tone, darker and cooler), so a shadow on sand cannot come out green. [csaf, Verdant Props](https://csaf.itch.io/verdant-props)

#### Outlines and separation
- LPC: tiles and objects are outlined with a darker version of the local colour, not black; characters use black or near black outlines with no selective outlining; each character gets its own palette to stand out from the background; props vary colour, brightness and saturation so they do not melt into the ground. [LPC Style Guide](https://lpc.opengameart.org/static/LPC-Style-Guide/build/styleguide.html)

#### Palette
- LPC: colour ramps should not share one hue; shadows shift slightly toward purple, highlights toward yellow; no pure colours; interiors cooler with less contrast, underground doubly so. [LPC Style Guide](https://lpc.opengameart.org/static/LPC-Style-Guide/build/styleguide.html)
- Slynyrd: the whole top down series uses one palette (Mondo) [Slynyrd Pixelblog 35](https://www.slynyrd.com/blog/2021/11/30/pixelblog-35-top-down-interiors); landscapes reuse colours across layers at 15 colours per scene [Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds); use a simple palette with strong light to dark contrast [Slynyrd Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1).
- The gamedev.net critique tied a perspective mismatch to a brightness and saturation imbalance that stopped the pieces feeling like one whole. [gamedev.net thread, via search summary](https://gamedev.net/forums/topic/676165-critique-my-jrpg-graphics/5277204/)

#### Scale
- LPC: 32 x 32 grid; a square object such as a chair fits a 32 x 32 area; a character base fits 32 x 48 and is about two and a half heads tall. [LPC Style Guide](https://lpc.opengameart.org/static/LPC-Style-Guide/build/styleguide.html)
- Slynyrd: a top down character of 26 x 32 px is roughly 1 x 2 tiles of 16 x 16 [Slynyrd Pixelblog 55](https://www.slynyrd.com/blog/2025/3/24/pixelblog-55-top-down-character-animation); stairs are scaled to the character sprites and wall height, and furniture sits on the same 16 x 16 grid [Slynyrd Pixelblog 35](https://www.slynyrd.com/blog/2021/11/30/pixelblog-35-top-down-interiors).
- Stardew Valley: 16 x 16 tiles [Stardew Valley Wiki, Modding:Maps](https://stardewvalleywiki.com/Modding:Maps); farmer sprites are 16 x 32 [Stardew Valley Wiki, Modding:Farmer sprite, via search summary](https://stardewvalleywiki.com/Modding:Farmer_sprite).
- RPG Maker MZ: 48 x 48 tiles, character frames normally 48 x 48. [RPG Maker MZ Help, Asset Standards](https://rpgmakerofficial.com/product/MZ_help-en/01_11_01.html)

### Inferences
- A prop reads as a sticker when any of these differ from the fighters and the ground: projection, light direction, palette and contrast, grounding (no contact shadow), or scale. Props generated one by one, each with its own view, light and palette, will miss several of these at once unless they share a written brief and one palette.
- Character to tile ratios in the sources run from 1 to 2 tiles tall (RPG Maker frame 1, LPC 1.5, Stardew and Slynyrd 2). Kindred Siege fighters at 120 to 190 px on 64 px tiles are 1.9 to 3 tiles tall, so the tile size was in a normal range; the failure sits in projection, repetition and prop integration, not in tile size.
- For Kindred Siege props (rock spires, totems, obelisk, braziers, skull, skeleton): draw them showing their front faces at the fighters' camera angle, lit from the side the fighters' baked light comes from, coloured from the stage palette, with contact shadows tinted from the sand or stone under them, and sized against a fighter silhouette used as a ruler.
- Keep the strongest outline and contrast for fighters; give props and ground softer coloured outlines (LPC), and keep the ground right behind fighter rows quiet (Slynyrd 62).

### Gaps
- No pixel art source gives numbers for contact shadow size or shape at large sprite scales; only LPC's drop shadow colour and the Verdant method were found.
- No professional source gives prop to character scale ratios beyond grid conventions.

## 4. What does a top down camera tileset imply for characters drawn in three quarter or side view? Is mixing them a known mistake? [PERSPECTIVE MISMATCH, FLAGGED]

### Takeaway
Yes. Mixing projections is a named, known failure: side facing characters on top down maps read as lying on the ground, and objects that ignore the camera angle read as flat billboards. The Zelda and Stardew 3/4 top down look works only because ground, walls, props and characters share one cheat: you see the top of the ground and the front of everything upright. Side view and front view JRPG battles avoid the clash by cutting to a separate backdrop composed for the battle camera.

### Cited Findings
- [MISMATCH] Slynyrd: projections should in most cases not be mixed in one scene; all elements must follow the same rules. [Slynyrd Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1)
- Slynyrd's 3/4 top down: like viewing a house from 45 degrees above, seeing about three quarters of the roof and the facade; the front is shortened while roof angles are kept [Slynyrd Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1). For characters, the 3/4 top down view shows a large part of the front plus the top of objects, and characters are drawn front, back, side and angled [Slynyrd Pixelblog 55](https://www.slynyrd.com/blog/2025/3/24/pixelblog-55-top-down-character-animation).
- [MISMATCH] LPC: characters are drawn in the same perspective as the tiles (about 60 degrees, orthographic). [LPC Style Guide](https://lpc.opengameart.org/static/LPC-Style-Guide/build/styleguide.html)
- [MISMATCH] An itch.io user on a top down 16 x 16 asset pack found it odd that the environments were top down while only the characters faced sideways; the creator answered by making four direction top down characters to match. [itch.io comment](https://itch.io/post/12327712)
- [MISMATCH] Another itch.io user: most graphics top down but characters from the side makes the player look like they are lying on the ground. [itch.io comment](https://itch.io/post/4524338)
- [MISMATCH] gamedev.net critique: objects that do not express the camera angle look like flat billboards; use one camera angle for all objects. Another reply described designing sprites for the chosen camera angle, with a front view and a side view tilted to that angle and cross referenced to place features. [gamedev.net thread, via search summary](https://gamedev.net/forums/topic/676165-critique-my-jrpg-graphics/5277204/)
- In a 3/4 cheat world, screen height is depth: Stardew draws Front layer objects over the player when the player is north of them and behind the player when south. [Stardew Valley Wiki, Modding:Maps](https://stardewvalleywiki.com/Modding:Maps)
- RPG Maker MZ keeps its 3/4 field maps (48 px tiles, four direction characters) separate from battles, which use battleback images cropped differently for front view and side view battles. [RPG Maker MZ Help, Asset Standards](https://rpgmakerofficial.com/product/MZ_help-en/01_11_01.html)
- Slynyrd's front view JRPG battle composition: monsters front facing across the middle, heroes at the bottom seen from behind, the backdrop built from horizon bands. [Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds)
- SF2's floor was praised for pixel level single point perspective scrolling [EventHubs quoting Ars Technica, via search summary](https://www.eventhubs.com/news/2010/dec/27/masterpiece-street-fighter-ii-ars-technica-article), done with per line tile scrolling [Fabien Sanglard, CPS1 graphics](https://fabiensanglard.net/cps1_gfx/index.html).

### Inferences
- A top down floor with three quarter characters is fine only inside the 3/4 cheat: ground drawn from above, every upright thing showing its front at the same angle, characters drawn for the same angle. It breaks when props are true plan views (seen straight down, showing the tops of skulls, obelisk tips or the back of a skeleton) beside fighters seen from the front, or when floor detail implies a straight down camera that the fighters contradict.
- Kindred Siege fighters (120 to 190 px, three quarter view, rows at several depths) imply a lower camera than a Stardew field map. Their natural stage is a receding ground plane (texture rows getting smaller and tighter toward the back) with upright props showing their fronts, like a JRPG battleback or a fighting game stage, not a map tileset.
- A square flagstone grid with no foreshortening is the worst case: a perfect lattice is exactly the pattern Slynyrd and LPC say to hide, and an unforeshortened grid declares a straight down camera that the fighters deny.
- Ways out (inference): (a) compose the stage in the fighters' camera as one scene or a few big layers; (b) if tiles stay, draw them as 3/4 cheat tiles with front faces, and foreshorten the floor bands by depth row; (c) redraw every prop in the fighters' view.

### Gaps
- No single professional source compares the Zelda or Stardew field map camera with side view JRPG battle backdrops; the comparison is assembled from RPG Maker docs, Slynyrd and LPC.
- Video guidance from Saint11, AdamCYounis and Brandon James Greer on perspective could not be read (see question 5).

## 5. What workflows and tools do professionals use (Aseprite, Pyxel Edit, Tiled, LDtk), and what do Slynyrd, Saint11, AdamCYounis, Pixel Pete and Brandon James Greer say about environments?

### Takeaway
Draw tiles in an editor that shows seams live (Pyxel Edit, Aseprite tilemap layers or Tiled Mode), rough out every connection first and detail after, then lay out maps in Tiled or LDtk with terrain rules (16 tile corner or edge sets, 47 tile blob) plus weighted random variants and separate decoration layers. Slynyrd is the best written source; Saint11's tutorials are images; AdamCYounis and Brandon James Greer are video only; no Pixel Pete source was found.

### Cited Findings
#### Autotile set sizes
- Tiled: corner sets and edge sets with two terrains need 16 tiles each; mixed sets need 256, but reduced sets such as the 47 tile blob work; with rotation the blob drops to 15 tiles. [Tiled manual, Using Terrains](https://doc.mapeditor.org/en/stable/manual/terrain/)
- Stagecast (cr31, mirrored by Boris the Brave): the blob is a 47 tile subset of the two edge, two corner Wang set (256 tiles is too many to make and use); it packs into a 7 x 7 sheet with three copies of the empty tile, or a 6 x 8 sheet with one extra solid tile; the layouts were found by Caeles by exhaustive search; the empty tile is often drawn without a blob. [cr31 blob page](https://boristhebrave.com/permanent/24/06/cr31/stagecast/wang/blob.html)
- Boris the Brave: corner sets (marching squares style) and edge sets are 16 tiles each; the corner set with rotation is 6; the blob with rotation and mirroring is 16; drop in variant tiles do not change a set's structure. [Boris the Brave, Classification of Tilesets](https://www.boristhebrave.com/2021/11/14/classification-of-tilesets/) Conflict: Tiled says the blob is 15 with rotation, Boris says 16 with rotation and mirroring; the counting rules differ and neither page reconciles them.
- RPG Maker MZ autotiles are built from 6 tiles (representative, boundary and group patterns), in sheets A1 (water, waterfalls), A2 (ground), A3 (buildings), A4 (walls) and A5 (normal tiles). [RPG Maker MZ Help, Asset Standards](https://rpgmakerofficial.com/product/MZ_help-en/01_11_01.html)
- A paid Aseprite extension, AutoBlob, generates the full 47 tile blob set (or the 16 tile edge set) from a 2 x 3 template, in the layout Godot, Tiled and GameMaker expect. [Aseprite community, via search summary](https://community.aseprite.org/t/extension-autoblob-generate-the-full-47-tile-blob-autotile-set-from-6-template-tiles/28606)

#### Editors
- Aseprite: tilemap layers since v1.3; Draw Pixels and Draw Tiles modes; Manual, Auto (the default) and Stack sub modes. [Aseprite docs, Tilemap](https://www.aseprite.org/docs/tilemap/)
- Slynyrd: in Aseprite, use View then Tiled Mode on a tile sized canvas to see repeats live; Pyxel Edit is built for tiles and updates placed tiles live; Photoshop lacks this. [Slynyrd Pixelblog 20](https://www.slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles) Pyxel Edit shows all tile connections in real time. [Slynyrd Pixelblog 35](https://www.slynyrd.com/blog/2021/11/30/pixelblog-35-top-down-interiors)
- Tiled: terrain brush and fill with per tile probability and flips [Tiled manual, Using Terrains](https://doc.mapeditor.org/en/stable/manual/terrain/); Stardew Valley's maps are edited in Tiled [Stardew Valley Wiki, Modding:Maps](https://stardewvalleywiki.com/Modding:Maps).
- LDtk: auto layers paint tiles from rules that match IntGrid values; pure auto layers can render separate foreground and background layers from one IntGrid. [LDtk docs, Auto layers](https://ldtk.io/docs/general/auto-layers/)

#### Slynyrd workflow and numbers
- 16 x 16 is the most common tile size; anything above 32 x 32 seems overkill for pixel art. [Slynyrd Pixelblog 20](https://www.slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles)
- Wall sets: start with a front face that loops on all sides; redraw it at 45 degrees for angled sides and reuse parts for the bottom and top; mirror and colour swap for the opposite side; rough a template of all tiles and connections first, then detail while watching the connections; expect lots of trial and error on tiles with many connections. [Slynyrd Pixelblog 43](https://slynyrd.com/blog/2023/3/26/pixelblog-43-top-down-tiles-part-2)
- Sand: long, angled S shaped clusters, a bit irregular; too many wiggles look like noodles. Water: two frames. [Slynyrd Pixelblog 43](https://slynyrd.com/blog/2023/3/26/pixelblog-43-top-down-tiles-part-2)

#### Saint11 (Pedro Medeiros)
- His tutorial index includes image tutorials named Tiles, Rock, Sand, Water, Ice, Wood, Vegetation (three parts), Parallax, Illumination Techniques, Outlines, Top Down Houses, Ruins, City and Indoors; the content lives inside the images, so it could not be read as text. [Saint11 tutorials](https://saint11.art/blog/pixel-art-tutorials/)
- Lospec lists his Making Tiles (2017, tips for seamless tiles), Environmental Hazards (animated tiles) and a UI 9 slice tutorial. [Lospec, tiles tag](https://lospec.com/pixel-art-tutorials/tags/tiles); [Saint11, Making Tiles](https://twitter.com/saint11/status/884809251725639680)

#### AdamCYounis
- Pixel Art Class videos: Tile Set Art, made while working on Project Apollo [YouTube](https://www.youtube.com/watch?v=Q-vZ6ZvvSys); Grassy Platform Tileset Tutorial [YouTube](https://www.youtube.com/watch?v=jQZ12jmwx7w); Isometric Tile Basics [YouTube](https://www.youtube.com/watch?v=OqwQBWEzcxU); Fully Lit Isometric Tiles in Unity [YouTube](https://www.youtube.com/watch?v=xpL44Ztyy1Q). Titles and descriptions only.

#### Brandon James Greer
- Channel [YouTube](https://www.youtube.com/@BJGpixel). Relevant titles: Top Down Pixel Art Practice, a timelapse with style discussion [YouTube](https://www.youtube.com/watch?v=9u535Rr4KJM); How to draw pixel art from 3D reference [YouTube](https://www.youtube.com/watch?v=dc2QgpVmEOk); Designing pixel art characters and game scenes [YouTube](https://www.youtube.com/watch?v=lIhpsoSFtBo). Titles only.

#### Pixel Pete
- No matching tutorials found in three searches.

### Inferences
- The shared professional loop: rough all connections, test live in a tiled preview, judge at 1x game scale, then add variants and a decoration layer in the map editor.
- For Kindred Siege, if a tile layer survives anywhere (for example a sand to stone transition), a 16 tile corner set is enough; the 47 tile blob only earns its cost for detailed concave shapes. The bigger gains are the variant, decoration and lighting layers, and drawing everything in the fighters' camera.

### Gaps
- No transcripts for AdamCYounis or Brandon James Greer videos were reachable (a transcript site returned 403), so their advice is not summarised here.
- Saint11's Making Tiles content sits in an image on Twitter and was not readable.
- Pixel Pete could not be identified; the name may be misremembered.
- Celeste, Hyper Light Drifter and Sea of Stars tile workflows are not sourced; The Spriters Resource sheets were not checked.
