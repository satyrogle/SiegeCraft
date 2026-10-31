# Hub, camp and base screens in 2D games with upgrades between runs

Notes for the report writer. Every claim carries a link. "(snippet)" marks a claim taken from a search engine summary where the page itself could not be opened, so treat those as weaker. Fandom wiki pages were read as raw wikitext through the wiki API (the pages block normal fetching), so screenshots on those pages were not seen.

## 1. How are these hub screens built and composed?

### Takeaway
They fall into two families. Walkable hubs reuse the game's own camera and projection, and every vendor has a fixed room or spot in one designed layout (Hades, Children of Morta, Moonlighter, Enter the Gungeon, Cult of the Lamb, Dead Cells, Spelunky 2, Fire Emblem Three Houses). Click hubs are one scene or menu where buildings, tents or slots are the buttons (Darkest Dungeon, The Banner Saga, Wildfrost, Loop Hero, Kingdom Rush). Even where the player places buildings (Cult of the Lamb, Loop Hero), every building is drawn by the same team, in the same camera and style, for that one ground.

### Cited Findings

#### Darkest Dungeon: the Hamlet
- The Hamlet is the main hub, reachable whenever you are not on a mission; it "contains various buildings that allow the player to form, manage, maintain and improve a roster of adventurers". ([Darkest Dungeon wiki, Town](https://darkestdungeon.wiki.gg/wiki/Town))
- Buildings: Abbey, Tavern, Blacksmith, Guild, Graveyard, Nomad Wagon, Sanitarium, Stage Coach, Survivalist, Ancestor's Memoirs, plus DLC Districts. Most buildings "must be visited to use their resources", while Districts give passive bonuses. ([Darkest Dungeon wiki, Hamlet](https://darkestdungeon.wiki.gg/wiki/Hamlet))
- Each station is a person as well as a building: the Survivalist is a "mysterious woman" and the Nomad Wagon is run by a nomad woman. ([Darkest Dungeon wiki, Hamlet](https://darkestdungeon.wiki.gg/wiki/Hamlet))
- The wiki's page template for each building has two image slots, "NPC Image" and "Building Upgrade Progression", i.e. every building has its own keeper art and its own set of upgrade stage art. ([Blacksmith](https://darkestdungeon.wiki.gg/wiki/Blacksmith); [Stage Coach](https://darkestdungeon.wiki.gg/wiki/Stage_Coach))
- The Blacksmith (the closest match to a Kindred Siege forge) has three upgrade tracks: Weaponsmithing (damage, crit, speed), Armorsmithing (max HP, dodge) and Furnace (cheaper smithing). ([Darkest Dungeon wiki, Blacksmith](https://darkestdungeon.wiki.gg/wiki/Blacksmith))
- Wikipedia summary of the hub: "The facilities can be used to cure heroes and reduce their stress, recruit new heroes, upgrade character stats, and buy new items." ([Wikipedia, Darkest Dungeon](https://en.wikipedia.org/wiki/Darkest_Dungeon))
- The Districts page's lead image caption refers to "a space where a district can be built", i.e. the town picture keeps empty plots for later buildings. ([Darkest Dungeon wiki, Districts](https://darkestdungeon.wiki.gg/wiki/Districts))

#### Wildfrost: Snowdwell
- Correction to the brief: Wildfrost is by Deadpan Games and Gaziter (artist Asaf Gazit), published by Chucklefish, built in Unity. "Deepwell" did not appear in any source. ([Wikipedia, Wildfrost](https://en.wikipedia.org/wiki/Wildfrost))
- Snowdwell is a mountain settlement "built around a large chunk of luminice". Placement runs bottom to top: the Tribe Hall "at the base", the Hot Spring "Encircling a smaller chunk of luminice", the Icebreaker Cabin with "a distinct block of ice atop it", the Frostoscope "High up the mountain", and the Challenge Shrine "At Snowdwell's peak". ([Wildfrost Wiki, Snowdwell](https://wildfrostwiki.com/Snowdwell))
- An inventor works inside the Inventor's Hut, the Pet House is the home of the pets, and travellers relax at the Hot Spring. ([Wildfrost Wiki, Snowdwell](https://wildfrostwiki.com/Snowdwell))
- The town is reached "from the map menu or 'DEAD' screen" (snippet). ([Magic Game World guide](https://www.magicgameworld.com/wildfrost-beginners-guide-8-essential-tips-for-new-players/))
- Criticism: the town system "never quite gels"; "The town's progress and unlockables never really feel like they impact your attempts too much"; the review calls town and combat "two disconnected elements". ([GGRecon review](https://www.ggrecon.com/reviews/wildfrost-review/))

#### Hades: the House of Hades
- "The game is presented in an isometric view"; "Between runs, Zagreus can explore the House of Hades"; the House "effectively acts as the game's overworld between attempts"; currency spent there buys permanent upgrades and can "order construction of new Underworld features". ([Wikipedia, Hades](https://en.wikipedia.org/wiki/Hades_(video_game)))
- Layout is a set of rooms, each with resident characters: the foyer holds the Pool of Styx "watched over by Hypnos"; then Hades' office and throne with Cerberus beside him and the House Contractor and later Orpheus next to him; a West Wing hallway that Achilles guards; an East Wing where Nyx floats by Zagreus' door; a Lounge ("expansive kitchen and dining room") with the Head Chef in the kitchen and the Wretched Broker "sequestered here in his well-stocked stall in the eastern corner"; an Administrative Chamber bought back through the Contractor; Gardens unlocked late. ([Hades wiki, House of Hades](https://hades.fandom.com/wiki/House_of_Hades))
- Station as landmark: the Mirror of Night (the main upgrade station) is "Dominating the top-left wall" of Zagreus' room, "several times Zagreus's height", with a gilded, gem set frame. ([Hades wiki, Zagreus's Room](https://hades.fandom.com/wiki/Zagreus%27s_Room))
- Source conflict: the House of Hades page says the Contractor is "To the left of Lord Hades", the House Contractor page caption says "at the desk to the right of Lord Hades". ([House of Hades](https://hades.fandom.com/wiki/House_of_Hades); [House Contractor](https://hades.fandom.com/wiki/House_Contractor))

#### Cult of the Lamb: the cult base
- Built as 2.5D in Unity: "It felt like a waste if we just stuck to normal 2D, so we decided to take advantage of that extra axis!" Artists moved flat sprites around to build fake 3D structures, chose character and camera angles in the editor, and kept the camera mostly fixed, so key elements sat toward the top or back of the screen. Lighting needed "custom solutions". ([Unity blog](https://unity.com/blog/en/games/recipe-behind-smash-hit-cult-of-the-lamb))
- Buildings are placed anywhere in the base "that isn't the central dirt area, where the Shrine sits", can be moved or demolished freely, and upgrades are built on top of the lower tier building. Decorations exist as their own category and give a slight faith boost. ([Cult of the Lamb wiki, Buildings](https://cult-of-the-lamb.fandom.com/wiki/Buildings))
- All art is hand drawn "minus a few shaders" (snippet). ([ACMI](https://www.acmi.net.au/stories-and-ideas/cult-of-the-lamb-massive-monster-take-dungeon-crawling-and-base-building-to-new-levels/))
- Tone split, creative director Julian Winton: "When you're at your base, it's kind of safe... then you go out into the forest and there is a much creepier side" (snippet). ([Stevivor preview](https://stevivor.com/previews/cult-lamb-preview-aussie-made-diabolical/))

#### The Banner Saga: camp
- Camp is where you manage the caravan, entered any time from a HUD button; you "Click 'leave' at the bottom of the camp site" to go back on the road. Functions are tents in the scene: the Heroes tent (upgrade and equip), the Rest tent, the Training tent; sometimes the Map. ([Banner Saga wiki, Camp](https://bannersaga.fandom.com/wiki/Camp))
- Art direction by Arnie Jorgensen, inspired by Eyvind Earle's Sleeping Beauty backgrounds; characters are "simple line art with flood fill colors". ([PocketGamer.biz, Making of The Banner Saga](https://www.pocketgamer.biz/the-making-of-the-banner-saga/))

#### Sea of Stars: campfire
- Camping mode starts by interacting with a campfire (or "Camp" from the menu on the world map). In it the player can save, talk with party members, swap the party leader, and use the campfire to cook or rest; Teaks can read a story. ([Sea of Stars wiki, Camping](https://seaofstars.fandom.com/wiki/Camping))
- The whole asset pipeline was built around dynamic lighting: "all of the assets that we make have to answer to the technical needs of the dynamic lighting" (Thierry Boulanger); one developer, Jeakim Hamel, worked on lighting full time from 2018 for five years. ([MobileSyrup interview](https://mobilesyrup.com/2023/09/11/sea-of-stars-level-design-sabotage-studio-thierry-boulanger-interview/))

#### Children of Morta: the Bergson house
- A two story house: a bedroom per family member, Grandma Margaret's Lab (Book of Rea upgrades), Uncle Ben's Workshop ("A small room powered by gears and wheels", equipment upgrades), kitchen, a living area with fireplace, a Library, an Animal Shelter, and the Sanctum below with a crystal and dungeon portals. Outside: a tree shrine, a hilltop lookout, ruins, a well, ponds and a Training Ground. "Between missions the player can view the home and it's occupants, and spend Morv to perform upgrades." ([Children of Morta wiki, The Bergsons' Home](https://childrenofmorta.fandom.com/wiki/The_Bergsons%27_Home); [Ben's Workshop](https://childrenofmorta.fandom.com/wiki/Ben%27s_Workshop))
- Designing the family home was artist Soheil Zarghami's favourite part; animator Arvin Garousi liked "making children play in the house". ([TechRaptor](https://techraptor.net/content/how-dead-mage-created-the-art-for-children-of-morta))

#### Moonlighter: Rynoka
- "We wanted Rynoka to feel like a quiet, friendly and safe place in contrast to the permanent aggression of the dungeons." Rynoka holds Will's shop, the other shops, visiting merchants and the dungeon access. ([Moonlighter wiki, Rynoka](https://moonlighter.fandom.com/wiki/Rynoka))
- The wiki's clickable town map places The Moonlighter (Will's shop) at the top, Vulcan's Forge on the right, The Wooden Hat bottom left, the Hawker bottom right, Le Retailer on the left, the Farm top right, two dungeon paths at the top and right edges, and the Banker and the Town Board near the middle. ([Moonlighter wiki, Rynoka](https://moonlighter.fandom.com/wiki/Rynoka))
- The Town Board is "Where Will can buy upgrades for The Moonlighter or fund new Shops". ([Moonlighter wiki, Town Board](https://moonlighter.fandom.com/wiki/Town_Board))
- CEO Javier Giménez: the team loved pixel art and it felt easier to produce than other styles when they started (snippet). ([Game World Observer](https://gameworldobserver.com/?p=1417))

#### Dead Cells: Prisoners' Quarters
- The start area is the run hub: "where you start, where you return after a run ends". It is a stone room with a glass chute that "drips water and muck from the ceiling", a chopping block and a giant's skeleton in the back. Next to it, large glass flasks for unlocked items hang from chains, the Scribe sits by the Daily Challenge door, and a large glass tube sets difficulty. Below are doors to the Tailor and the Training Room and a tunnel to the Doctor. ([Dead Cells wiki, Prisoners' Quarters](https://deadcells.wiki.gg/wiki/Prisoners%27_Quarters))
- The main upgrade vendor is not in the start room: the Collector sits "In his laboratory in Passages, between each two biomes". ([Dead Cells wiki, Collector](https://deadcells.wiki.gg/wiki/Collector))

#### Loop Hero: camp
- "The building menu is where you use the resources gained during expeditions to enhance your camp. The first thing you build is a campfire after the initial loop." 23 buildings; Farm and Forest can be built many times. ([Loop Hero wiki, Buildings](https://loophero.fandom.com/wiki/Buildings))
- The camp is a grid with adjacency rules: Lumberjack next to Forest; Forest touching "the top border of the camp" or another Forest; Library next to Intel Center; Warehouse next to Supply Depot; every building except Forest must touch an existing building; Bridge and River have fixed build spots. ([Loop Hero wiki, Buildings](https://loophero.fandom.com/wiki/Buildings))
- "As players progress through the game, they'll have a bustling camp full of people and important facilities." ([Shacknews review](https://shacknews.com/article/123099/loop-hero-review-happy-death-day))
- A player review said the camp was "visually difficult to parse" (snippet, user review). ([RAWG](https://rawg.io/games/loop-hero/reviews))

#### Inscryption: Leshy's cabin
- In Act I the cabin holds escape room puzzles you solve while the map is unrolled. You move in an L shaped space: three spots at the rear near the door, two up the left side, because Leshy and the game board fill the middle and right. New objects appear over runs (a goat skull, Goobert, a wolf statue). ([Inscryption wiki, Leshy's Cabin](https://inscryption.fandom.com/wiki/Leshy%27s_Cabin))

#### Kingdom Rush: upgrade screens
- In Kingdom Rush, Frontiers and Origins there are six upgrade trees, one per tower type and spell, each with five linear upgrades bought with stars won per level (3 stars for 18 to 20 lives left). Later games use four trees and upgrade points; Genesis gives heroes and spells their own trees. ([Kingdom Rush wiki, Upgrades](https://kingdomrushtd.fandom.com/wiki/Upgrades))

#### Fire Emblem: Three Houses: Garreg Mach (3D contrast)
- The monastery is the hub, "freely roamed and explored", with fast travel to any part already visited. ([Fire Emblem wiki, Garreg Mach Monastery](https://fireemblem.fandom.com/wiki/Garreg_Mach_Monastery))
- VGC: early chapters are "dragged down by an hour or so of housekeeping, bonding activities and lectures"; "Exploring the monastery also becomes a speedier affair once you establish a rhythm"; "you don't have to spend hours exploring". ([VGC review](https://www.videogameschronicle.com/reviews/fire-emblem-three-houses/)) TechRaptor disagreed, calling the monastery one of the reviewer's favourite parts (snippet). ([TechRaptor review](https://techraptor.net/content/fire-emblem-three-houses-review))

#### Extra pixel art hubs worth comparing
- Spelunky 2 camp: a cave you drop into by rope; it "will be dark" on the first visit with nothing but the Dwelling door, then "a large camp will slowly be constructed overtime, expanding the more characters that the player unlocks"; unlocked Spelunkers live and rest in an apartment block right of the big door. ([Spelunky wiki, Camp (2)](https://spelunky.fandom.com/wiki/Camp_(2)))
- Enter the Gungeon, the Breach: the hub where you pick characters, buy items and meet NPCs; rescued characters "move up to The Breach and offer various services"; doorways carry icons (an open book and lit bulb over the tutorial hall, a gun chamber over the elevator room); "The details in the Breach can change over the course of gameplay." ([Enter the Gungeon wiki, The Breach](https://enterthegungeon.fandom.com/wiki/The_Breach))
- Rogue Legacy 2: the skill tree starts as "a bare plot of land" and "Each skill you invest in adds a section of the castle, which expands over time". ([GGRecon guide](https://www.ggrecon.com/guides/rogue-legacy-2-skill-tree/))

### Inferences
- Navigation splits by family. Walk an avatar: Hades, Children of Morta, Moonlighter, Gungeon, Spelunky 2, Dead Cells, Cult of the Lamb, Fire Emblem (and Inscryption's tiny first person cabin). Point and click: Darkest Dungeon (buildings must be "visited" from the town screen), The Banner Saga (click tents, click "leave"), Wildfrost (entered from a menu), Loop Hero (building menu on a grid), Kingdom Rush (trees). Kindred Siege points at Pals rather than walking, so the click family is its match.
- Open space comes from giving each station its own room, zone or level of the picture, not from a bigger empty floor: Hades splits vendors across rooms and wings; Children of Morta gives each upgrader a room; Wildfrost stacks buildings up a mountain; Cult of the Lamb keeps the central dirt area clear for the Shrine. Reading the Moonlighter town map coordinates, Rynoka is a ring of shops around a central plaza with the Town Board in the middle. (Inferred from the sources above.)
- Density works when the whole space is small and every object is a puzzle or service (Inscryption's five spot cabin). Too much walking costs time (VGC on Garreg Mach), and a crowded grid of small buildings can be hard to read (Loop Hero user complaint).
- Wildfrost's own review shows the risk of a pretty town that feels "disconnected" from fights: the hub has to show what the upgrades change.

### Gaps
- No primary source found that describes the Darkest Dungeon Hamlet screen's composition or its mouse hover behaviour. Not verified, recollection only: it is one wide illustrated panorama of the town with the ruined mansion behind, buildings spread across the width, and clicking a building opens a full screen panel with that building's interior art and keeper. Check against a screenshot before relying on it.
- Wildfrost: no source on whether Snowdwell scrolls or fits one screen, or how buildings under construction look.
- Loop Hero: no reliable source on the camp's exact projection (one search summary called it top down; not confirmed on a page).
- The Banner Saga camp art itself (single painting versus layered scene) is not described in any source found.
- No GDC talk or postmortem found that covers hub art specifically for any of these games.

## 2. How does each station get its own identity, while all stations share one perspective and light?

### Takeaway
Identity comes from a keeper anchored at a recognisable place doing their job, plus one oversized landmark prop that reads at a glance (the Mirror of Night, Uncle Ben's gear driven workshop, the Broker's stall, the ice block on the Icebreaker Cabin, flasks on chains). Coherence comes from one projection, one grid and scale tied to the character sprites, and one light system that every asset was made for. Pixel artist Raymond Schlitter states the rule plainly: projections "should not be mixed in the same scene".

### Cited Findings
- Keepers anchored to spots with activities: Hypnos watches the pool, Achilles guards the hallway, Dusa dusts, the Head Chef works the kitchen, Nyx floats by Zagreus' door, Thanatos broods near a balcony, the Broker sits in "his well-stocked stall". ([Hades wiki, House of Hades](https://hades.fandom.com/wiki/House_of_Hades))
- Landmark prop scaled to read: the Mirror of Night is "several times Zagreus's height" and dominates a wall. ([Hades wiki, Zagreus's Room](https://hades.fandom.com/wiki/Zagreus%27s_Room))
- Rooms themed by the keeper's craft: Ben's workshop is "powered by gears and wheels"; Margaret's lab holds her concoctions and the Book of Rea. ([Children of Morta wiki](https://childrenofmorta.fandom.com/wiki/The_Bergsons%27_Home)) Each family member has a hobby used in the house (Lucy draws, Linda plays violin, Kevin loves flowers, Mark meditates). ([TechRaptor](https://techraptor.net/content/how-dead-mage-created-the-art-for-children-of-morta))
- Buildings marked by a signature object: the Icebreaker Cabin's block of ice, the Hot Spring around luminice, tribe flags outside the Tribe Hall. ([Wildfrost Wiki](https://wildfrostwiki.com/Snowdwell)) Gungeon doorways carry icons over the door. ([Gungeon wiki](https://enterthegungeon.fandom.com/wiki/The_Breach)) Dead Cells shows unlocks as flasks hanging on chains. ([Dead Cells wiki](https://deadcells.wiki.gg/wiki/Prisoners%27_Quarters))
- Characters designed out of their workplace: in Bloomtown, Ruth "works out of a cluttered garage/workshop", which shaped her design. ([Twin Sails dev diary](https://twin-sails.com/?p=1899))
- One projection: Schlitter says projections "should not be mixed in the same scene", and if every element follows one rule set, "any wonky projection can look good"; "uniformity takes priority over realism". In his 3/4 top down projection the viewer is about 45 degrees above, seeing about three quarters of roof and facade. ([SLYNYRD Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1))
- One light: in top down scenes the light "could come from any corner but it's most natural to come from one of the top corners"; "Use a simple palette with strong contrast from lightest to darkest colors"; you can start from "solid shapes with colors based on light source" once the palette is planned. ([SLYNYRD Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1))
- One grid and scale: furniture is designed "within a 16x16px tile map, so they mesh with the environment", and stairs are drawn "in proportion to the character sprites and wall height". ([SLYNYRD Pixelblog 35](https://www.slynyrd.com/blog/2021/11/30/pixelblog-35-top-down-interiors))
- Quiet ground: "A busy texture next to another busy texture may exhaust the eyes with too much noise"; "Negative space is your friend". ([SLYNYRD Pixelblog 20](https://www.slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles))
- One light system for all assets: Sea of Stars built every asset to the needs of its dynamic lighting. ([MobileSyrup](https://mobilesyrup.com/2023/09/11/sea-of-stars-level-design-sabotage-studio-thierry-boulanger-interview/)) Children of Morta lights pixel sprites with high resolution dynamic lights: "The puppets are pixelated but we shine them with real lights." ([TechRaptor](https://techraptor.net/content/how-dead-mage-created-the-art-for-children-of-morta))
- Modular props must be made to fit together: Children of Morta's randomly placed objects "needed to use colors and forms that could be composed in a modular way by the game algorithm". ([TechRaptor](https://techraptor.net/content/how-dead-mage-created-the-art-for-children-of-morta))
- One hand: in Hades, art director Jen Zee made characters, environments and concept art, and the pen and ink style covered all of them. ([MCV/Develop](https://mcvuk.com/?p=216051)) Darkest Dungeon's look (hard lines, pooling blacks, woodcut and illuminated manuscript references) was set by Chris Bourassa for the whole game. ([80.lv](https://80.lv/articles/red-hook-studios-talks-about-the-creation-of-darkest-dungeon))
- Keep a fixed camera in mind when placing things: Cult of the Lamb's mostly fixed camera pushed key elements toward the top or back of the screen. ([Unity blog](https://unity.com/blog/en/games/recipe-behind-smash-hit-cult-of-the-lamb))

### Inferences
#### Why the rejected Kindred Siege camp failed (applied to the brief)
- It mixed projections: a straight top down sand floor and cliff band, props generated separately in other angles, and three quarter Pal sprites. That breaks the one projection rule above, which is the single most cited principle here.
- The props were not built on a shared grid or a scale tied to the Pal sprites (Pixelblog 35), and each generator brought its own light direction and palette. No asset was made for one shared light (contrast Sea of Stars and Children of Morta).
- A tiled sand floor with a cliff band is texture, not a place: there were no zones, no landmark per Pal and no negative space, so stations read as stickers "clamped together", which is exactly what the owner said.
- Busy floor next to busy props is noise (Pixelblog 20). The reference hubs keep the ground quiet and let the station and keeper carry the detail.
#### The standard approach these games share
- One scene in one projection matching the fighters, one key light from one corner, one palette, one artist's hand or one style sheet.
- Each station is a planned zone with: a keeper anchored in it doing their job (Hades, Children of Morta), one large signature silhouette (forge chimney and glow, armour stand, shrine stones, banners for regalia), and a local light or colour accent that still obeys the shared key light.
- Space between zones is designed negative space, with a centre or path tying them together (Moonlighter plaza, Cult of the Lamb central area).

### Gaps
- No source found on hover or highlight conventions (outline, glow, name label) in any of these hubs. Not verified, recollection only: Darkest Dungeon highlights a building under the cursor and shows its name.
- No source gives numbers (pixel sizes of stations, distance between zones, percent of empty ground).

## 3. Side view panorama, isometric or top down: what do games with side or three quarter fighters use for their hub, and why?

### Takeaway
Walkable hubs use the same camera as the rest of the game. That is sourced here for Hades (isometric throughout) and Cult of the Lamb (one mostly fixed 2.5D camera); Dead Cells and Spelunky 2 (side view) and Children of Morta and Moonlighter (top down three quarter) visibly do the same, but their projection is not stated on the pages cited. Darkest Dungeon gives the clearest stated reason: its team dropped a planned top down exploration view because characters seen from above felt unrelatable, and went side view throughout. For fighters drawn in three quarter view, the matching hub is a three quarter elevated view (camera about 45 degrees above), not a straight down floor.

### Cited Findings
- Darkest Dungeon first planned an isometric or top down exploration view with a side view battle camera; Bourassa felt characters seen from above became unrelatable and the side camera's intimacy was lost; going full side view meant fewer assets and kept the presentation character centred. ([80.lv](https://80.lv/articles/red-hook-studios-talks-about-the-creation-of-darkest-dungeon)) The side view made players feel "at the same level as the characters". ([Wikipedia](https://en.wikipedia.org/wiki/Darkest_Dungeon))
- Hades "is presented in an isometric view, with the player controlling Zagreus", and Zagreus explores the House between runs. ([Wikipedia](https://en.wikipedia.org/wiki/Hades_(video_game)))
- Cult of the Lamb kept its camera mostly fixed and built its look in 2.5D. ([Unity blog](https://unity.com/blog/en/games/recipe-behind-smash-hit-cult-of-the-lamb))
- Dead Cells' hub is the start area of its first biome, the Prisoners' Quarters, where every run starts and ends. ([Dead Cells wiki](https://deadcells.wiki.gg/wiki/Prisoners%27_Quarters)) Spelunky 2's camp is the cave the Spelunker drops into by rope at the start of each adventure. ([Spelunky wiki](https://spelunky.fandom.com/wiki/Camp_(2)))
- Projection definitions: in 3/4 top down the viewer sits about 45 degrees above and sees about three quarters of both roof and facade; in 3/4 side view the viewer is 45 degrees off a building's corner and sees about three quarters of each wall. ([SLYNYRD Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1))

### Inferences
- Rule of thumb from the set: hub projection follows the fighter projection. Side view fighters get side view hubs or side on panoramas (Darkest Dungeon, Dead Cells, Spelunky). Three quarter or isometric characters get three quarter or isometric hubs (Hades, Children of Morta, Moonlighter, Cult of the Lamb). The Dead Cells, Spelunky 2, Children of Morta and Moonlighter projections come from the games as commonly known, not from the cited pages.
- For Kindred Siege (three quarter view fighters, pointer driven, no avatar), the closest fit is one composed three quarter elevated scene, framed like a stage, with the four Pal zones spread across it. A side on panorama in the Darkest Dungeon style would clash with three quarter sprites unless the ground plane is tilted to match them.
- A straight down floor under three quarter characters is the mismatch that made the rejected camp look wrong.

### Gaps
- No source found that compares hub projections across games or states a general rule; the rule above is drawn from these examples only.
- Not confirmed whether Hades' characters are real time 3D or rendered to frames (Zee's team used Maya for 3D modelling and animation and counts 942,489 character and enemy animation frames, which suggests rendered frames, but no source says so). ([MCV/Develop](https://mcvuk.com/?p=216051))

## 4. How do these screens show progression?

### Takeaway
Five devices recur: dark to lit, empty plot to building, people moving in, trophies and decor piling up, and new rooms opening. Darkest Dungeon's line sums it up: buildings go "from dark and run down to well-lit and welcoming".

### Cited Findings
- Dark to lit: "The appearance of each building changes subtly as you improve them, typically from dark and run down to well-lit and welcoming." ([Darkest Dungeon wiki, Town](https://darkestdungeon.wiki.gg/wiki/Town)) A 2016 patch added "Various lighting FX and smoke" to the Hamlet. ([17 May 2016 Patch](https://darkestdungeon.wiki.gg/wiki/17_May_2016_Patch)) Spelunky 2's camp starts dark and empty. ([Spelunky wiki](https://spelunky.fandom.com/wiki/Camp_(2)))
- Lighting bought as an upgrade: Hades' Contractor sells "Lighting, Wax Pillar", which "Illuminates the pillar-tops with long-burning flames". ([Hades wiki, House Contractor](https://hades.fandom.com/wiki/House_Contractor))
- Decor bought as an upgrade: the West Wing "can be augmented with rugs, sculptures, seating and paintings"; the Lounge with kitchen renovations, rugs and a shortcut. ([Hades wiki, House of Hades](https://hades.fandom.com/wiki/House_of_Hades)) Great Hall items are "for cosmetic purposes only" except the Music Stand; in the Lounge only the Aquarium, Bat Cage and Shortcut have mild use. ([House Contractor](https://hades.fandom.com/wiki/House_Contractor)) Zagreus' desk becomes adorned with the Fated List after a 20 gem purchase. ([Zagreus's Room](https://hades.fandom.com/wiki/Zagreus%27s_Room))
- Empty plot to building: Wildfrost's Pet House is "Under construction in a new game" and the Frostoscope is built after the first win. ([Wildfrost Wiki](https://wildfrostwiki.com/Snowdwell)) Rogue Legacy 2 starts from "a bare plot of land". ([GGRecon](https://www.ggrecon.com/guides/rogue-legacy-2-skill-tree/)) Darkest Dungeon keeps "a space where a district can be built". ([Districts](https://darkestdungeon.wiki.gg/wiki/Districts)) Moonlighter's Town Board funds new shops. ([Town Board](https://moonlighter.fandom.com/wiki/Town_Board)) Loop Hero's first build is a campfire. ([Loop Hero wiki](https://loophero.fandom.com/wiki/Buildings))
- Tiers built on top: Cult of the Lamb upgrades "can only be built on top of existing lower tier buildings". ([Cult of the Lamb wiki](https://cult-of-the-lamb.fandom.com/wiki/Buildings))
- People moving in: Gungeon NPCs "move up to The Breach". ([Gungeon wiki](https://enterthegungeon.fandom.com/wiki/The_Breach)) Spelunky 2: "More bedrooms will appear, furniture will be placed all around the area". ([Spelunky wiki](https://spelunky.fandom.com/wiki/Camp_(2))) Loop Hero ends with "a bustling camp full of people". ([Shacknews](https://shacknews.com/article/123099/loop-hero-review-happy-death-day)) Dead Cells added the Training Room, the Doctor, the Mentor Knight and the Architect to its start area in updates. ([Dead Cells wiki](https://deadcells.wiki.gg/wiki/Prisoners%27_Quarters))
- Trophies: tribe flags hung outside the Tribe Hall as tribes unlock. ([Wildfrost Wiki](https://wildfrostwiki.com/Snowdwell)) Spelunky 2 places orbs in the door creatures' mouths after boss wins, and critters appear once you have visited their area. ([Spelunky wiki](https://spelunky.fandom.com/wiki/Camp_(2))) Hades adds a statue to the Gardens after the epilogue. ([House of Hades](https://hades.fandom.com/wiki/House_of_Hades)) Inscryption adds statues and objects to the cabin over runs. ([Inscryption wiki](https://inscryption.fandom.com/wiki/Leshy%27s_Cabin))
- Story change: "A lot of what happens in the house changes by the story progression." (Soheil Zarghami) ([TechRaptor](https://techraptor.net/content/how-dead-mage-created-the-art-for-children-of-morta))
- New rooms: Hades unlocks the Lounge, the Administrative Chamber and the Gardens over time. ([House of Hades](https://hades.fandom.com/wiki/House_of_Hades))

### Inferences
- For four Pal stations, the cheapest strong signal is Darkest Dungeon's: a few art states per station (dark and bare, then lit, then fully kitted), plus trophies that match the upgrade type (orders as banners or shrine offerings, regalia hung on racks).
- Leave planned empty plots in the composition from day one (Darkest Dungeon districts, Rogue Legacy 2), so new things land in designed space instead of crowding the scene.

### Gaps
- No source gives how many art states per building Darkest Dungeon uses.

## 5. What production steps do artists describe?

### Takeaway
Brief and concept with colour first, then a block in, then points of interest and unique props, then a lighting pass that every asset was built for. Consistency comes from one art lead, one style sheet and one pipeline; mixed sources are the thing every team avoids.

### Cited Findings
- Bloomtown (pixel art town game), art director Vitaly: "It usually starts with the game designer outlining the location and its events"; then "a concept sketch that captures the palette and mood"; for larger locations "initial blocking"; then filling with "points of interest having various unique assets"; then "many different gradients and soft brushes in post-processing" to extend the palette and mood; the team tried to "minimize the visual gap between the early assets and the later ones". ([Twin Sails dev diary](https://twin-sails.com/?p=1899))
- Hades: art follows design ("first and foremost a game design-led team"); early art is "disposable until the gameplay and narrative harden up"; colour script visualisations checked the whole game; "The final Elysium concept became the basis for that biome's tileset"; pen and ink was faster than painterly for a large asset count; 1,400 environment textures; Photoshop for 2D, Maya for 3D. ([MCV/Develop](https://mcvuk.com/?p=216051))
- Children of Morta: frame by frame animation, hand painted art and modern lighting; sprites scaled about four times to full HD so lights render at high resolution; key frames first, then in betweens; combat moves roughed in and play tested before full animation. ([TechRaptor](https://techraptor.net/content/how-dead-mage-created-the-art-for-children-of-morta))
- The Banner Saga: the team tested the look by taking an Earle painting, splitting it into layers and parallaxing it on screen, then Jorgensen painted his own version and tested that. ([PocketGamer.biz](https://www.pocketgamer.biz/the-making-of-the-banner-saga/))
- Sea of Stars: lighting came first and the pipeline was shaped around it. ([MobileSyrup](https://mobilesyrup.com/2023/09/11/sea-of-stars-level-design-sabotage-studio-thierry-boulanger-interview/))
- Cult of the Lamb: fake 3D structures assembled from flat sprites in Unity, camera and character angles set in the editor, artists placing their own work straight into the project. ([Unity blog](https://unity.com/blog/en/games/recipe-behind-smash-hit-cult-of-the-lamb))
- Darkest Dungeon: custom engine and exporters; the side view decision cut the asset count. ([80.lv](https://80.lv/articles/red-hook-studios-talks-about-the-creation-of-darkest-dungeon))
- Pixel method: plan the palette, then lay "solid shapes with colors based on light source"; tiles 16x16 is the common size; props drawn on the same tile map as the floor; railings on a separate layer above stairs. ([Pixelblog 3](https://www.slynyrd.com/blog/2018/3/14/pixelblog-3-graphical-projections-1); [Pixelblog 20](https://www.slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles); [Pixelblog 35](https://www.slynyrd.com/blog/2021/11/30/pixelblog-35-top-down-interiors))

### Inferences
- A camp built the standard way would go: one brief listing the four stations and what each Pal does; one concept of the whole camp in the fighters' projection with palette and key light set; a grey block in of the four zones and paths at final scale next to a Pal sprite; then each station's landmark and props drawn by one hand to the same grid and light; then lighting and atmosphere passes; then the 192 px Pal work loops placed in their zones; then the upgrade states per station.
- Generating each prop separately and assembling them afterwards is the reverse of every process above, which all start from the whole scene.

### Gaps
- No artist breakdown found for any of the named hubs specifically (Hamlet, Snowdwell, House of Hades, cult base). The process notes above come from the games' general art pipelines and from Bloomtown.
- No source found on modular kits for hub buildings in these games.
