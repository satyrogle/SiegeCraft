# Battle backdrops for 2D battle scenes with side view or three quarter view fighters

Scope: how JRPG, tactics and auto battle backdrops are composed and produced, aimed at Kindred Siege (two armies left versus right on one locked screen, front and back columns of up to three fighters stacked by depth plus a leader, three quarter view fighters 120 to 190 px tall at 1x, stage about 1376 x 768 on a 1440 x 900 screen).

## 1. How are the battle backgrounds of the named games built: one painted image, layered parallax, or tiles?

### Takeaway
Three families show up. Old consoles assemble each backdrop from shared tiles plus its own palette (FF6, Pokémon Emerald, GBA Fire Emblem, which also adds a separate terrain platform under the fighters). HD 2D painted games use big flat paintings, sometimes modular (Darkest Dungeon room paintings plus repeated hallway panels; Unicorn Overlord and Battle Chasers painted scenes). A third group has no separate backdrop at all: the fight happens in the field or in a 3D diorama (Chrono Trigger, Sea of Stars, Octopath, Triangle Strategy). Parallax shows up in tutorials and in scrolling scenes. None of the static battle screens studied relied on it.

### Cited Findings

#### Final Fantasy VI (SNES): tiles, one palette per backdrop
* The FF6 ROM map lists 56 battle BG data entries (6 bytes each), 56 battle BG palettes of 48 colours each, 49 compressed battle BG tile formations (up to 1140 bytes each) and 75 compressed battle BG graphics sets. ([ff6hacking ROM map](https://www.ff6hacking.com/wiki/doku.php?id=ff3:ff3us:doc:asm:rom_map))
* The same ROM map has a table giving a Dance index for each battle background, and a table of which background each Dance switches to. ([ff6hacking ROM map](https://www.ff6hacking.com/wiki/doku.php?id=ff3:ff3us:doc:asm:rom_map))
* Every backdrop belongs to one of eight terrain sets tied to Mog's Dances. Mog learns a Dance by winning a fight on a background from that set, and a Dance can switch the current battle background to its own terrain. ([Final Fantasy Wiki, Dance](https://finalfantasy.fandom.com/wiki/Dance_(Final_Fantasy_VI)))
* My observation of the Wikipedia battle screenshot (a scaled 337 x 295 copy of the 256 x 224 frame): sky with clouds and snowy peaks at the top, then a dark band of hills and pine silhouettes, then grass for the rest of the battle area. The grass starts about 30 percent down the full frame, about 43 percent down the battle area. The menu windows cover about the bottom 31 percent. Two monsters stand on the left at two depths. Four party members stand on the right in a diagonal column, each one lower and a little further right. The floor has only sparse small tufts and stones. ([Wikipedia file page](https://en.wikipedia.org/wiki/File:Final_Fantasy_VI_battle.png))
* Party sprites measure roughly 24 px tall at native scale on that screenshot (my estimate from the scaled copy). Kazuko Shibuya drew the character sprite art. ([Wikipedia FF6](https://en.wikipedia.org/wiki/Final_Fantasy_VI))

#### Chrono Trigger: no battle backdrop
* Fights happen on the map where you explore, with enemies visible on screen, not on a separate battle screen. ([Wikipedia Chrono Trigger](https://en.wikipedia.org/wiki/Chrono_Trigger))
* Akira Toriyama designed the look of each era. The overworld uses a scaled down overhead view. ([Wikipedia Chrono Trigger](https://en.wikipedia.org/wiki/Chrono_Trigger))

#### Octopath Traveler (HD 2D): 3D backgrounds under 2D sprites
* Acquire's lead artist Mika Iizuka said the team looked at PlayStation one era games, where 2D characters sat on a 3D rendered background or a single pre rendered image. ([Unreal Engine spotlight, Octopath Traveler](https://www.unrealengine.com/en-US/spotlights/octopath-traveler-s-hd-2d-art-style-and-story-make-for-a-jrpg-dream-come-true))
* Early battle effects did not look good enough. The fix was to add a point light with each visual effect, so the effect throws the characters' shadows onto the environment. ([Unreal Engine spotlight, Octopath Traveler](https://www.unrealengine.com/en-US/spotlights/octopath-traveler-s-hd-2d-art-style-and-story-make-for-a-jrpg-dream-come-true))
* Acquire on the sequel: the pixel characters look like flat sheets of paper while the backgrounds are almost entirely 3D, and making the two sit together naturally was a struggle. In the first game they sometimes had to play tricks with the lighting to make characters and background look right together. ([Unreal Engine interview, Octopath Traveler II](https://www.unrealengine.com/en-US/developer-interviews/octopath-traveler-ii-builds-a-bigger-bolder-world-in-its-stunning-hd-2d-style))
* Octopath Traveler II gave characters a smaller head to body ratio to fit more action into events and battles, raised the resolution of the maps, and rotated the camera nearly 90 degrees in some battle effects. ([Unreal Engine interview, Octopath Traveler II](https://www.unrealengine.com/en-US/developer-interviews/octopath-traveler-ii-builds-a-bigger-bolder-world-in-its-stunning-hd-2d-style))

#### Triangle Strategy (HD 2D tactics): 3D maps seen from any angle
* The producers told 4Gamer the main HD 2D problem was the camera. Octopath had a locked camera, but a tactics game needs the map to rotate 360 degrees, so maps had to look good from every angle. Handling map edges took a lot of early discussion and making maps viewable from all sides took a lot of resources. ([The Outer Haven, summarising 4Gamer](https://theouterhaven.net/2022/05/triangle-strategy-had-challenges-in-making-hd-2d-art-style-work))

#### Sea of Stars: fights in the field
* Sabotage's press kit: no random encounters and no transitions to separate battlefields. It also claims full dynamic lighting from a custom render pipeline. ([Sabotage press kit](https://sabotagestudio.com/presskits/sea-of-stars))
* Enemies sit at set places in the world and battles happen at those places. The game uses an isometric view with 2D pixel art, and the team spent six months building dynamic lighting that looked consistent with the pixel art. ([Wikipedia Sea of Stars](https://en.wikipedia.org/wiki/Sea_of_Stars))
* Thierry Boulanger: the dynamic lighting came first (Jeakim Hamel started on it in 2018), so every asset had to answer to the lighting's technical needs. ([MobileSyrup interview](https://mobilesyrup.com/2023/09/11/sea-of-stars-level-design-sabotage-studio-thierry-boulanger-interview/))

#### GBA Fire Emblem: terrain picture plus separate platform
* My observation of the FE7 English screenshot (native 240 x 160): the backdrop is an overhead, angled view of terrain, not a horizon landscape. The left half shows grass with small conifers and two tiny houses. The right half shows castle masonry with a carpeted dais. Both fighters stand on one bright grass slab with a visible front edge. Each has a dark shadow ellipse at its feet. Name banners sit in the top corners. The stats HUD fills roughly the bottom 48 px, about 30 percent of the screen. ([Wikipedia file page, FE7](https://en.wikipedia.org/wiki/File:FE_Rekka_English_gameplay.png))
* My observation of the FE8 screenshot (240 x 159): one continuous overhead terrain backdrop (grass, dirt paths, cliffs, sea at top right, houses only about 16 px tall), with both fighters on a bright grass platform. Feet sit at about y 104 to 107. The infantry fighters measure about 35 to 47 px tall including weapons. In the FE7 shot the mounted Sain is about 64 px tall to the sword tip. ([Wikipedia file page, FE8](https://en.wikipedia.org/wiki/File:SacredStonesFight.png); [Wikipedia file page, FE7](https://en.wikipedia.org/wiki/File:FE_Rekka_English_gameplay.png))
* The FE8 battle frame graphic is 256 px wide on a 240 px screen. The extra width shows during screen shake. Palettes are 16 colours with colour 0 transparent, and the frame needs 4 faction palettes (64 colours). ([FEUniverse battle screen tutorial](https://feuniverse.us/t/fe8-creating-and-inserting-battle-screens/9460))
* A hacker inserting a custom battle background used a 240 x 160 image with 15 colours plus 1 transparent. The game also needs a matching tile map (TSA), and the thread never confirms a method that worked. This is a forum post, not a spec. ([FEUniverse thread](https://feuniverse.us/t/problems-with-inserting-new-battle-backgrounds/1904))
* Platforms are tied to terrain types. One hacker suggests making a terrain type with fully transparent platforms, and warns it looks off when the two fighters are on different terrain. ([FEUniverse platforms thread](https://feuniverse.us/t/how-to-not-use-battle-platforms-in-specific-instances-fe8/12438))
* Community asset packs ship backgrounds and platforms as separate files, with names like "BB Fields 05 Evening" and "BP Plains 04 Evening", and were made to match FE8's palette. I only saw this in a search snippet and did not open the page. ([FEUniverse resource repository](https://feuniverse.us/t/fire-emblem-resource-repository-battle-animations-portraits-music-etc/3326?page=15))

#### Darkest Dungeon: painted rooms plus modular hallway panels
* Combat is shown from the side so players feel level with the characters. The party is four heroes, and rank decides who can hit whom. ([Wikipedia Darkest Dungeon](https://en.wikipedia.org/wiki/Darkest_Dungeon))
* A modder's guide: background images are PNGs in each area's Dungeons folder. Room backgrounds are 1920 x 720 and hallway backgrounds are 720 x 720. Hallway pieces are numbered, room pieces are named per dungeon (example: a weald room wall). The guide suggests darkening the borders so each image blends into the surrounding black. ([Steam community thread](https://steamcommunity.com/app/262060/discussions/4/1741106440019366509))
* Chris Bourassa: the art is drawn in Photoshop and animated in a 2D animation package. ([Game Developer, Road to the IGF](https://www.gamedeveloper.com/audio/road-to-the-igf-red-hook-studios-i-darkest-dungeon-i-))
* GameSpot's writer compared the game's layered look to Valiant Hearts. Bourassa said that, as the only artist, he built a style with rules he could write down, so he could produce consistent content fast and in effect art direct himself. ([GameSpot](https://www.gamespot.com/articles/the-gothic-sensibilities-of-darkest-dungeon/1100-6424880/))
* My observation of the Wikipedia screenshot (a 421 x 236 downscale of a close up attack): the crypt wall behind is near black and desaturated, the fighters are lit and lighter, and the UI panel fills about the bottom third. ([Wikipedia file page, Darkest Dungeon](https://en.wikipedia.org/wiki/File:Darkest-dungeon-03.jpg))

#### Unicorn Overlord: painted scene, 2x3 squads, auto resolved
* Squads sit on a two by three grid: a front rank and a back rank of three slots each. When squads meet, time pauses and the skirmish plays out automatically in initiative order. ([Zatu review](https://zatu.com/blogs/reviews/unicorn-overload-review))
* Battles play out automatically from character stats and player set behaviours. All characters, locations and sprites are 2D art. Battlefield settings include open fields and towns or cities. ([Wikipedia Unicorn Overlord](https://en.wikipedia.org/wiki/Unicorn_Overlord))
* A player account: enemy squads of three, two of them lined up one behind the other; front rows take hits, and piercing attacks are what reach the back row. ([Gigazine](https://gigazine.net/gsc_news/en/20240323-unicorn-overlord-battle))
* My observation of the Wikipedia battle screenshot (a 360 x 202 downscale of an Atlus image): a painted landscape, with sky and clouds at the top, pale blue grey mountains, dark conifers at left, a ruined stone wall at right, and a grass field with a dirt path receding into depth. The horizon sits roughly 40 percent down the frame and the floor fills the lower 55 to 60 percent. The two armies face each other from left and right under red and blue banners. Small diamond shaped 3x2 formation diagrams with HP bars sit in the bottom corners. ([Wikipedia file page, Unicorn Overlord](https://en.wikipedia.org/wiki/File:Unicorn_Overlord_battle_gameplay.jpg))

#### Battle Chasers: Nightwar: painted combat backgrounds
* Combat backgrounds are individual paintings by named artists. Phu Giang painted the airship one. Grace Liu's first one started from a thumbnail Joe Madureira doodled, and she said the shapes and a central idea gave her a starting point. The captions do not say if the pieces are 2D or 3D. ([PlayStation Blog](https://blog.playstation.com/archive/2017/09/29/20-stunning-pieces-of-battle-chasers-nightwar-artwork-by-comic-book-legend-joe-madureira-and-his-team))

#### Legend of Dragoon
* The game has three modes: area map, field, and battle screen. The field uses pre rendered backgrounds with polygon characters. I only saw this in search snippets and did not open the pages. ([Wikipedia Legend of Dragoon](https://en.wikipedia.org/wiki/The_Legend_of_Dragoon))

#### Extra GBA case: Pokémon Emerald
* The decompiled source defines 10 battle environments (grass, long grass, sand, underwater, water, pond, mountain, cave, building, plain). Each has a tileset, a tile map, an entry tileset and map, and a palette. Plain reuses the building tiles and map with its own palette. ([pokeemerald battle_bg.c](https://github.com/pret/pokeemerald/blob/master/src/battle_bg.c))
* Special arenas are mostly palette swaps of shared tiles: Groudon on cave tiles, Kyogre on water tiles, gym leaders and gyms on building tiles, and the Elite Four rooms and Champion on stadium tiles with their own palettes. Only Rayquaza gets its own tiles. The main backdrop sits on BG3 at the lowest priority, and the text box sits on BG0. ([pokeemerald battle_bg.c](https://github.com/pret/pokeemerald/blob/master/src/battle_bg.c))

#### Pixel auto battlers
* I found no reliable source that documents how a pixel art auto battler builds its battle backdrop. Unicorn Overlord (auto resolved squad fights, above) is the nearest documented match for the layout. Loop Hero's combat is automatic, but I found nothing on how its battle panel art is built.

#### Method note for my screenshot measurements
* Measurements are mine. I loaded the Wikipedia images into a browser canvas and computed approximate luma (Rec. 709 weights on sRGB values, 0 to 1) and HSV saturation per region. I report mean values and p10 to p90 spread, the spread standing in for how much internal contrast a region has. The FE images are native resolution. The FF6, Darkest Dungeon and Unicorn Overlord images are scaled or JPEG copies, so treat their numbers as rough.

### Inferences
* Static battle screens get their life from sprites and effects, not from moving the backdrop. FF6, GBA FE and Pokémon all use one locked backdrop per fight. A one screen auto battler does not need parallax to look standard.
* Tile assembly is how 16 bit and 32 bit games got 56 (FF6) or 10 plus 14 (Pokémon) backdrops cheaply. They shared graphics and layouts and gave each backdrop its own palette. FF6's ROM holds more graphics sets (75) than backdrops (56), which suggests each backdrop combines several shared graphics sets. That reading comes from the table sizes and is not documented in the source.
* Darkest Dungeon's numbered 720 px square hallway panels mean its corridors are built from repeated painted modules, a big scale version of tiles. Its rooms are one 1920 px wide painting.
* GBA FE splits backdrop and platform. The platform is a near ground slab drawn in the fighters' own view, and it is the thing the fighters actually stand on. The terrain picture behind is a far layer that only sets the place.
* Unicorn Overlord is the closest known match to Kindred Siege's layout: two squads face each other from left and right, each with a front and a back rank of three, the fight resolving on its own over a painted landscape with a receding floor.

### Gaps
* The Spriters Resource (ripped battle backdrop sheets with exact sizes and layer splits) showed a Cloudflare "verify you are human" page. I did not try to get past it, so there are no sheet sizes from there.
* No source found for FF6's exact battle backdrop pixel size. A search answer guessed 256 x 160 from memory, which is not evidence.
* No source on how many layers Darkest Dungeon's corridors use, or whether they scroll at different speeds. Red Hook's GDC talk on Darkest Dungeon II environments is members only on GDC Vault.
* No source on how Unicorn Overlord's battle backdrops are produced (painted panorama, layers, count per region). The Japanese art book Unicorn Overlord Official Design Works may cover it.
* No source on how Legend of Dragoon's battle stages are built, or on Battle Chasers' combat background count or method (2D painting versus painted 3D).
* Octopath Traveler's battle staging (which side the party stands on, how many battle backdrops) was not confirmed by a source.

## 2. What perspective does the ground use under three quarter view sprites, where is the horizon, and how much is ground? Why did the top down floor fail?

### Takeaway
Every shipped example keeps the floor, the props and the fighters in one camera. Side view JRPGs use a receding floor under a horizon about 40 percent down the battle area, with ground filling the lower 55 to 60 percent (FF6, Unicorn Overlord by my measurements). GBA Fire Emblem shows no horizon. Its high angle terrain picture is drawn small and quiet, and the fighters stand on their own bright platform in front of it. A straight down tile floor with straight down props, at the same scale as standing three quarter view fighters, breaks the one rule the tutorials agree on: never mix projections in one scene.

### Cited Findings
* Raymond Schlitter (Slynyrd) on graphical projections: "in most cases they should not be mixed in the same scene." If every element follows the same rules, even an odd projection can look good. ([Slynyrd Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1))
* The same post: in the three quarter top down projection you view the subject from about 45 degrees above and see most of the roof and the shortened front face. In three quarter side view you look at a corner from 45 degrees off centre. ([Slynyrd Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1))
* Matej Jan's example of a scene that fails: house and cars at an angle, trees fully top down, a frontal fence, a swing seen almost from above, and cars at the wrong scale. Each thing has its own perspective, so it does not fit together. He calls games that combine several projections in one image "Frankenstein projections", mostly poor workarounds for old hardware. ([Retronator, graphical projections part 1](https://medium.com/retronator-magazine/game-developers-guide-to-graphical-projections-with-video-game-examples-part-1-introduction-aa3d051c137d))
* Same article: in parallel projections things keep one size wherever they move, so artists draw each thing once. In perspective projections things shrink with distance. ([Retronator, graphical projections part 1](https://medium.com/retronator-magazine/game-developers-guide-to-graphical-projections-with-video-game-examples-part-1-introduction-aa3d051c137d))
* Height on screen is depth: the closer something sits to the horizon line, the further away it reads. ([Slynyrd Pixelblog 11](https://www.slynyrd.com/blog/2018/11/16/pixelblog-11-landscape-pixeling))
* Slynyrd's JRPG battle backdrop starts with solid horizontal bars that fix the horizon, because depth is carried mainly by colour. Sky and ground are then split into more bands. ([Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds))
* Slynyrd's top down tiles are drawn in the three quarter top down projection, 16 x 16 px is the most common size, and anything above 32 x 32 he calls excessive for pixel art. The goal is a cohesive top down game world. ([Slynyrd Pixelblog 20](https://www.slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles))
* Ground treatment flips with the view. In top down games, keep ground contrast low and save contrast for walls and trees. In side scrollers the ground should stand out to show where you can walk. ([Bregalad, NESdev pixel art thread, 2010](https://nesdev.nes.science/f21/t6892.xhtml))
* Measured proportions (my observations, method under question 1):
  * FF6: sky and far band about 30 percent of the frame, floor about 39 percent, menu about 31 percent. That puts the horizon about 43 percent down the battle area. ([Wikipedia file page, FF6](https://en.wikipedia.org/wiki/File:Final_Fantasy_VI_battle.png))
  * Unicorn Overlord: horizon roughly 38 to 46 percent down, floor about the lower 55 to 60 percent. ([Wikipedia file page, Unicorn Overlord](https://en.wikipedia.org/wiki/File:Unicorn_Overlord_battle_gameplay.jpg))
  * GBA FE: no horizon. The battle view above the HUD is about 112 px tall. Fighters' feet sit at about y 104 to 107, so near the bottom of the view, on a platform whose top face runs from about y 90 to y 104, plus a front edge. ([Wikipedia file page, FE8](https://en.wikipedia.org/wiki/File:SacredStonesFight.png))
  * Darkest Dungeon: room backgrounds are 1920 x 720 ([Steam community thread](https://steamcommunity.com/app/262060/discussions/4/1741106440019366509)), and the UI fills about the bottom third in the screenshot ([Wikipedia file page](https://en.wikipedia.org/wiki/File:Darkest-dungeon-03.jpg)).
* Octopath's makers say pixel characters look like flat sheets of paper against 3D backgrounds, which took lighting tricks to reconcile. ([Unreal Engine interview, Octopath Traveler II](https://www.unrealengine.com/en-US/developer-interviews/octopath-traveler-ii-builds-a-bigger-bolder-world-in-its-stunning-hd-2d-style))

#### Why the rejected stage (top down Wang floor, top down props, three quarter fighters) fails
* Projection mismatch: the floor and props were drawn looking straight down while the fighters were drawn turned and seen from a lower angle. That is the mixed projection both Slynyrd and Matej Jan warn against. ([Slynyrd Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1); [Retronator](https://medium.com/retronator-magazine/game-developers-guide-to-graphical-projections-with-video-game-examples-part-1-introduction-aa3d051c137d))
* No horizon means no depth cue. Without a horizon, height on screen cannot read as distance, so back row fighters do not read as further away. ([Slynyrd Pixelblog 11](https://www.slynyrd.com/blog/2018/11/16/pixelblog-11-landscape-pixeling))
* Wrong scale: top down tiles are designed at 16 to 32 px for small overworld characters ([Slynyrd Pixelblog 20](https://www.slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles)), and Matej Jan lists mismatched scale as one of the ways a mixed scene falls apart ([Retronator](https://medium.com/retronator-magazine/game-developers-guide-to-graphical-projections-with-video-game-examples-part-1-introduction-aa3d051c137d)).
* The one shipped case of an overhead terrain picture behind side view fighters, GBA Fire Emblem, keeps the terrain small (houses about 16 px next to 35 to 47 px fighters), quiet in value, and behind a separate near platform the fighters stand on, with dark contact shadows. ([Wikipedia file page, FE8](https://en.wikipedia.org/wiki/File:SacredStonesFight.png); [Wikipedia file page, FE7](https://en.wikipedia.org/wiki/File:FE_Rekka_English_gameplay.png))

### Inferences
* Camera math (mine): seen from a camera raised θ degrees above the ground, a floor square's depth shows at about sin θ of its true size. Straight down (90 degrees) shows it full size, 45 degrees about 71 percent, 30 degrees 50 percent. A straight down Wang floor therefore claims a 90 degree camera, while SE and SW facing three quarter sprites claim something lower. The floor texture, prop footprints and shadow ellipses should all be squashed to match whatever angle the fighters' feet ellipses and head tops imply.
* Two consistent families exist. (a) Low camera with a horizon: FF6, Unicorn Overlord, Darkest Dungeon. (b) High camera with no horizon in frame, where floor, props and characters are all drawn in three quarter top down: Chrono Trigger and Sea of Stars fight in their field maps, and GBA FE's terrain picture does this behind a platform. Kindred Siege fighters facing SE and SW fit either family as long as the floor and props are redrawn in that same family. The rejected stage mixed a straight down floor with turned fighters.
* Layout numbers for the 1376 x 768 stage, by analogy with FF6 and Unicorn Overlord (mine, not sourced): horizon or far band edge about 35 to 45 percent down (about 270 to 345 px), floor about 420 to 500 px tall. Three depth slots per column need distinct feet lines on that floor, back slot highest, front slot lowest, all clearly below the horizon. With 120 to 190 px fighters, feet lines about 60 to 100 px apart keep the back fighter readable behind the front one. Heads may overlap the far band. Feet should not.
* If Jacob wants the GBA FE feel, the documented trick is a platform per army: a near ground slab in the fighters' own view, carrying the feet, shadows and depth slots, set in front of a quiet, smaller scale terrain picture.

### Gaps
* No source gives the camera elevation of any of these games' battle cameras in degrees. The horizon percentages are my measurements on a few screenshots.
* No source on Kingdom Rush's stage camera angle or how its maps are painted (Ironhide's process is under question 4).
* I did not check Advance Wars or the One Piece GBA games' battle scenes.

## 3. How do artists keep fighters readable on the backdrop?

### Takeaway
Fighters carry the full value range and the strongest saturation. The floor right behind them is kept narrow in value and low in detail, and the far bands turn into flat, hazy or dark silhouettes. A floor slab plus a dark contact shadow ties each fighter down. Light and shadow come from the same direction and colour for sprites and scene.

### Cited Findings
* Atmospheric perspective: the nearest plane is the most saturated with the strongest light and shadow contrast. Each plane further back is less saturated, lighter in daylight haze, and shifts toward the sky colour. Desert haze is heavier, and the forest scene reverses the hue shift. ([Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds))
* Distant things take on the haze colour, lose saturation and use fewer colours. ([Slynyrd Pixelblog 11](https://www.slynyrd.com/blog/2018/11/16/pixelblog-11-landscape-pixeling))
* Action games such as platformers and shoot em ups may need muted or simplified backgrounds so sprites stand out. Slynyrd's turn based battle backdrop goes richer because nothing moves fast over it. ([Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds))
* Grass texture keeps one cluster scale per plane: thick blades near, one or two pixel clusters far. Clarity of what each pixel means beats extra detail. ([Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds))
* Use bright colours for sprites so they read on the background, and keep shading inside sprites light. ([Bregalad, NESdev](https://nesdev.nes.science/f21/t6892.xhtml))
* Darkest Dungeon: no dead grey or black except in the ink lines. The palette is warm overall, with even cool scenes slightly yellowed to unify everything. The strong linework lets Bourassa splash unexpected colour on objects without hurting readability. ([GameSpot](https://www.gamespot.com/articles/the-gothic-sensibilities-of-darkest-dungeon/1100-6424880/))
* Octopath ties each effect to a point light so the characters throw shadows onto the environment. ([Unreal Engine spotlight](https://www.unrealengine.com/en-US/spotlights/octopath-traveler-s-hd-2d-art-style-and-story-make-for-a-jrpg-dream-come-true))
* Measured value plans (mine, method under question 1):
  * GBA FE8: the grass behind the fighters averages luma 0.56 with a narrow spread (p10 0.43, p90 0.66). The platform top is lighter at 0.65 (0.53 to 0.76) and less saturated (0.35 against 0.43). The regions holding the fighters spread from about 0.17 to 0.77, so the fighters span about two and a half times the floor's value range. ([Wikipedia file page, FE8](https://en.wikipedia.org/wiki/File:SacredStonesFight.png))
  * GBA FE7: the grass backdrop half is 0.58 (0.48 to 0.65), again a narrow band. The castle half is 0.49 (0.27 to 0.74). ([Wikipedia file page, FE7](https://en.wikipedia.org/wiki/File:FE_Rekka_English_gameplay.png))
  * FF6: sky 0.58 (0.43 to 0.80). The far forest band is a flat dark silhouette at 0.21 (0.19 to 0.21), and the near grass is 0.52 (0.32 to 0.63). The dark band makes a hard horizon line that frames the mid value floor the sprites stand on. ([Wikipedia file page, FF6](https://en.wikipedia.org/wiki/File:Final_Fantasy_VI_battle.png))
  * Unicorn Overlord: the top sky band is 0.71 at saturation 0.26, and the far mountain band is 0.54 at saturation 0.18, the least saturated band in the frame. The lower bands, which include fighters and effects, are darker and more saturated (0.25 to 0.32, saturation 0.52 to 0.62). ([Wikipedia file page, Unicorn Overlord](https://en.wikipedia.org/wiki/File:Unicorn_Overlord_battle_gameplay.jpg))
* Contact shadows: both GBA FE screenshots show a dark ellipse under every fighter, on the platform. ([FE7](https://en.wikipedia.org/wiki/File:FE_Rekka_English_gameplay.png); [FE8](https://en.wikipedia.org/wiki/File:SacredStonesFight.png))

### Inferences
* The readable pattern is internal contrast, not just darkness. GBA FE's floor is only a little darker than its fighters on average, but its value range is about a third of theirs. A quiet floor means a narrow value band and small, even texture right where the feet go.
* Detail can be loud far away only if it is flattened into one value. FF6's busy pine skyline reads as a single dark shape with almost no internal spread.
* Kindred Siege's earlier rule (a quiet floor, layered scenery, matched light) matches what the shipped games do. The measured spreads give a checkable target: floor p10 to p90 spread well under half the fighters' spread.

### Gaps
* No artist interview gives numeric value targets. All numbers here are my measurements on low resolution fair use screenshots, three of them scaled or JPEG.
* No source from Saint11, AdamCYounis, Brandon James Greer or MortMort on battle backdrop readability turned up in search. Their material is mostly video.

## 4. What production steps do artists follow, and at what native resolutions?

### Takeaway
The documented order is: lock the stage first (canvas, horizon bars, where units stand), then colour bands, then planes back to front, then texture and detail, then haze and clouds, on a tight palette (Slynyrd uses 15 colours per backdrop). Studios start from a thumbnail or a playable sketch and only paint final art after it plays (Ironhide, Battle Chasers). Native sizes are small for pixel games (192 x 144 tutorial, 240 x 160 GBA, 256 x 224 SNES) and large for painted ones (1920 x 720 Darkest Dungeon rooms).

### Cited Findings
* Slynyrd's battle backdrop steps: (1) pick the canvas, (2) paint solid horizontal bars for the horizon, (3) break sky and ground into more colour bands, (4) add two mountain layers, moving forward from the furthest grass plane, (5) darken the nearest grass, reuse the other greens and add grass texture, (6) add a haze band at the mountain base and blue shifted shading, (7) add cloud layers, with the farthest clouds baked into the sky. ([Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds))
* Canvas 192 x 144 (4:3), chosen to fit inside 256 x 224 and 256 x 240 console screens with room for borders or UI, and because a 192 px width suits pixel perfect parallax. It was scaled x4 to 768 x 576 for a CRT demo. Valley, desert and forest versions use 15 colours each, under a self imposed limit of 16, reusing colours across layers. The concept follows Dragon Quest and Phantasy Star IV, with front facing monsters across the centre. ([Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds))
* Pixel perfect parallax method: pick a screen width with many divisors (his example is 96 px). The farthest layer moves 1 px per frame and nearer layers step to the next divisor. Slower than 1 px per frame tends to look jittery. Each layer loops in screen width copies. ([Slynyrd Pixelblog 23](https://www.slynyrd.com/blog/2019/11/12/pixelblog-23-parallax-scrolling))
* Ironhide's level process (Kingdom Rush Vengeance era): idea, then concept, then a theme and story concept, then a road map and sketch of the stage, then play testing whether it works as a map, and only then final art, with a lot of polish per stage. ([Game World Observer interview](https://gameworldobserver.com/2018/12/06/ironhide-long-fans-love-kingdom-rush-will-continue-working))
* Battle Chasers: a combat background began as a thumbnail doodle by Joe Madureira and was then painted by Grace Liu. ([PlayStation Blog](https://blog.playstation.com/archive/2017/09/29/20-stunning-pieces-of-battle-chasers-nightwar-artwork-by-comic-book-legend-joe-madureira-and-his-team))
* Darkest Dungeon: drawn in Photoshop, animated in a 2D animation package ([Game Developer](https://www.gamedeveloper.com/audio/road-to-the-igf-red-hook-studios-i-darkest-dungeon-i-)). The style had written rules so one artist could produce consistent content quickly ([GameSpot](https://www.gamespot.com/articles/the-gothic-sensibilities-of-darkest-dungeon/1100-6424880/)). Room PNGs are 1920 x 720 and hallway PNGs 720 x 720 ([Steam community thread](https://steamcommunity.com/app/262060/discussions/4/1741106440019366509)).
* FF6: 56 backdrops, each with a 48 colour palette, built from 75 graphics sets and 49 tile layouts. ([ff6hacking ROM map](https://www.ff6hacking.com/wiki/doku.php?id=ff3:ff3us:doc:asm:rom_map))
* GBA FE: a 240 x 160 backdrop at 15 colours plus transparent (forum report), 16 colour palettes, and a 256 px wide battle frame for screen shake. ([FEUniverse thread](https://feuniverse.us/t/problems-with-inserting-new-battle-backgrounds/1904); [FEUniverse tutorial](https://feuniverse.us/t/fe8-creating-and-inserting-battle-screens/9460))
* Pokémon Emerald: each environment is a tileset, tile map and palette on the lowest priority background layer, and special arenas reuse tiles with new palettes. ([pokeemerald battle_bg.c](https://github.com/pret/pokeemerald/blob/master/src/battle_bg.c))
* Sea of Stars built the lighting first, and asset production was shaped around it. ([MobileSyrup](https://mobilesyrup.com/2023/09/11/sea-of-stars-level-design-sabotage-studio-thierry-boulanger-interview/))

### Inferences
* Size check (mine): Kindred Siege's 1376 x 768 stage at 1x is about 1.06 million pixels. That is about 27 times a 240 x 160 GBA screen and about 38 times Slynyrd's 192 x 144 backdrop. At 120 to 190 px, the fighters are roughly 3 to 4 times taller than GBA FE fighters, so the stage is a large native canvas, not an upscaled small one.
* At that size, the shipped ways to stay consistent and affordable are the ones above: set bands first, repeated modules (FF6 tile layouts, Darkest Dungeon's 720 px hallway panels), and palette swaps for variants (Pokémon, FF6). A single hand pixeled 1376 x 768 painting per region is possible, but nothing in the sources shows a pixel game doing that at this size.
* A locked palette per backdrop (FF6's 48, Slynyrd's 15) shared with the fighters' approved colours keeps sprites and stage in one world.
* Ironhide and Battle Chasers both settle the composition (playable sketch, thumbnail) before any final paint. For Kindred Siege that means marking the feet lines and the horizon on the stage before any scenery is drawn.

### Gaps
* No ripped sheet sizes from The Spriters Resource (Cloudflare human check, not attempted).
* No exact native render resolution found for Sea of Stars, Octopath or Unicorn Overlord battle scenes.
* No written step by step backdrop process found from Saint11, AdamCYounis, Brandon James Greer or MortMort. Search only showed that AdamCYounis's YouTube background tutorial is widely followed ([itch.io asset page citing it](https://trashboat93.itch.io/mountains-my-first-art)).

## 5. How do games vary backdrops per region, and what makes a region recognisable in one screenshot?

### Takeaway
Games keep the composition and swap the identity. FF6 groups 56 backdrops into eight terrain families, each backdrop with its own palette. Pokémon reuses ten environment tilesets and recolours them for special places. Darkest Dungeon gives each dungeon one dominant hue with analogous and complementary accents. Slynyrd builds valley, desert and forest from the same band formula with different colours and haze. A region reads at a glance from its dominant hue, its far silhouette (peaks, pines, ruins, masonry) and its floor material.

### Cited Findings
* FF6's eight terrain families and where they appear: Wind (plains, Veldt, airship decks), Forest, Desert (desert, Imperial Camp), Love (towns, castles, mansions, interiors), Earth (mountains such as Mt. Kolts), Water (Lethe River, Serpent Trench), Twilight (caves and tombs), Snowman (the Narshe cliffs in the World of Ruin). ([Final Fantasy Wiki, Dance](https://finalfantasy.fandom.com/wiki/Dance_(Final_Fantasy_VI)))
* Each of FF6's 56 backdrops has its own 48 colour palette. ([ff6hacking ROM map](https://www.ff6hacking.com/wiki/doku.php?id=ff3:ff3us:doc:asm:rom_map))
* Pokémon Emerald: 10 environment types plus 14 special cases, almost all special cases being new palettes on shared tiles (gym, Team Magma and Aqua stadiums, each Elite Four room, Champion, Groudon, Kyogre). ([pokeemerald battle_bg.c](https://github.com/pret/pokeemerald/blob/master/src/battle_bg.c))
* Darkest Dungeon: Bourassa gave each dungeon its own dominant hue, then mixed in analogous and complementary hues for contrast and interest, to avoid the "brown and dingy" look. The whole game stays warm. ([GameSpot](https://www.gamespot.com/articles/the-gothic-sensibilities-of-darkest-dungeon/1100-6424880/))
* Darkest Dungeon's backgrounds live in per area folders, with rooms named per dungeon. ([Steam community thread](https://steamcommunity.com/app/262060/discussions/4/1741106440019366509))
* Slynyrd: the desert follows nearly the same colour band proportions as the valley but with heavier haze and a stronger colour shift. The forest implies a shorter viewing distance with less extreme scaling between layers and needs detail on every depth layer. All three use 15 colours. ([Slynyrd Pixelblog 62](https://www.slynyrd.com/blog/2026/5/27/pixelblog-62-landscape-backgrounds))
* Sea of Stars: each area of the game was meant to be distinct. ([Wikipedia Sea of Stars](https://en.wikipedia.org/wiki/Sea_of_Stars))
* Unicorn Overlord: five nations (Cornia, Drakenhold, Elheim, Bastorias, Albion) ([Gematsu](https://www.gematsu.com/2023/09/unicorn-overlord-details-story-world-characters-and-game-systems)). Battlefields range from open fields to towns and cities ([Wikipedia](https://en.wikipedia.org/wiki/Unicorn_Overlord)).
* Octopath Traveler II: the team wanted players to wonder whether the next map would be sand or snow, and added regions with an Asian look and islands of beast people. ([Unreal Engine interview](https://www.unrealengine.com/en-US/developer-interviews/octopath-traveler-ii-builds-a-bigger-bolder-world-in-its-stunning-hd-2d-style))
* GBA FE picks backdrop and platform by terrain. In the FE7 shot each half of the screen shows different terrain (grass and village on one side, castle masonry on the other). ([Wikipedia file page, FE7](https://en.wikipedia.org/wiki/File:FE_Rekka_English_gameplay.png); [FEUniverse platforms thread](https://feuniverse.us/t/how-to-not-use-battle-platforms-in-specific-instances-fe8/12438))
* Darkest Dungeon's title image deliberately shows no landmarks, to keep the time period vague. That shows how much far silhouettes carry place. ([GameSpot](https://www.gamespot.com/articles/the-gothic-sensibilities-of-darkest-dungeon/1100-6424880/))

### Inferences
* The recipe across these games: one composition template (same horizon, same feet lines, same floor band), then per region a dominant hue and light colour, a distinctive far silhouette, and a floor material. Kindred Siege's three depth slots would then land in the same place in every region, which suits an auto battler where layout must stay readable.
* Cheap variety is a palette swap of shared pieces (Pokémon, FF6). Time of day or corruption variants of one region can be palette swaps rather than new art.
* GBA FE showing each army's own terrain on its half of the screen is a precedent for siege fights where attacker and defender stand on different ground (for example open field against wall top).

### Gaps
* No source lists Darkest Dungeon's dominant hue per dungeon. I did not observe enough screenshots to state them.
* No count of unique battle backdrops for Unicorn Overlord, Octopath, Sea of Stars, Battle Chasers or Darkest Dungeon (per dungeon room and hallway counts) was found.
* GBA FE's exact rule for which terrain picture shows (per combatant half or defender only) is not documented in any source I reached. The FE7 split screen is one observed case.
