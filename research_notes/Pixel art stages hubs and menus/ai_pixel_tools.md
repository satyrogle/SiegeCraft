# AI pixel art tools (PixelLab first) for game backgrounds, battle scenes, hub screens and title art

Notes compiled 11 Oct 2026. Research only: no PixelLab, Pixel Engine or ComfyUI generation, edit or credit spending tool was called. The only PixelLab MCP tool used was the free `search_help` docs agent, cited below as "PixelLab MCP search_help (KB, 11 Oct 2026)". It has no public URL, so those findings are tagged that way.

Source labels used in every bullet:
- **[PL]** = PixelLab's own docs, API schema, homepage or knowledge base (vendor claim).
- **[User]** = developer, player or artist report.
- **[3rd]** = third party review, blog or vendor of a different tool.
- "(snippet)" = seen only in a search result summary, page not opened.

Most PixelLab doc pages below were read through PixelLab's own compiled docs file ([llms-full.txt](https://www.pixellab.ai/llms-full.txt)). The individual page URL is given for each fact.

## Q1. What does PixelLab document about backgrounds, scenes and maps, which tools suit side or three quarter backdrops, and what sizes do they support?

### Takeaway
No PixelLab tool or guide made for side view battle backdrops, title screens, menu backgrounds or parallax layers was found, and PixelLab's own docs agent found none either. The tools that can make a whole opaque scene in one go are capped well under the 1376 x 768 stage:
- Pro / `Create Image S-XL`: up to 688 x 384 at 16:9. It takes style and reference images but has no view setting.
- Pixen: up to 512 x 512 in area, and it does have a view setting.
- PixFlux: up to 400 x 400 in area, with weak view control, init image and palette.

The map, tileset, tile and map object tools are built for tile grids that are top down (or platformer). For anything larger, PixelLab's own advice is to extend section by section with Edit Image (Pro) or Inpaint v3. Those are capped at 256 per side on tier 1 and 512 per side on tier 2 and up.

### Cited Findings

#### What PixelLab says about scenes and backgrounds in general
- [PL] The homepage says it generates detailed pixel art scenes and environments from text, and makes animated environments and dynamic backgrounds. It also says it makes maps, tilesets and texture assets for both top down and side scrolling games. It names no games or studios and claims 3,000+ indie developers. [PixelLab homepage](https://www.pixellab.ai/)
- [PL] Asked directly, PixelLab's docs agent found no tool or guide for parallax layers, title screens, menu backgrounds or side view battle backdrops. PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] KB FAQ entry on large 16:9 scenes: use `Create Image S-XL (New)` rather than "Create Image High Quality", typically at 512 x 288 for 16:9. Past 512, extend with Edit Image (Pro) section by section. The entry suggests init image strength of about 430 to 530. PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] Conflict inside PixelLab's own KB:
  - One FAQ gives S-XL an init strength.
  - Another says Pro tools have no init strength control.
  - A third says the web `Create Image M-XL (PixFlux)` takes no init image.
  - But the API schemas for PixFlux list `init_image`. [API v1 schema](https://api.pixellab.ai/v1/openapi.json); [API v2 schema](https://api.pixellab.ai/v2/openapi.json); PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] KB FAQ on map size: keep the map workshop canvas to 256 x 256 or smaller. Anything bigger is experimental and less stable. PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] The older "Create large image" doc calls that tool useful for backgrounds because it made the highest resolution images. It is the only tool page found that says so outright. Its limits are minimum 100 x 100 and area at most 200 x 200 (example 265 x 150 for 16:9). The page sends maps to the map tools. [PL docs: Create large image](https://www.pixellab.ai/docs/tools/create-large-image)
- [PL] The PixFlux doc (Create M to XL image) recommends roughly 16:9 for scenery (for example 160 x 100) and 2:3 for characters. Minimum is 32 x 32. Area cap is 200 x 200 on the free tier, 320 x 320 on tier 1 and 400 x 400 on tier 2 and up (snippet plus llms-full). [PL docs: Create image (pixflux)](https://www.pixellab.ai/docs/tools/create-image-flux)
- [PL] PixelLab tutorial videos exist, titled "Creating a pixel art sidescroller map and tilesets with PixelLab" and "Generate map tiles for your sidescroller game with PixelLab". Their content could not be read. [YouTube H-dPJKmKr1E](https://www.youtube.com/watch?v=H-dPJKmKr1E); [YouTube XVlbYNMI3xg](https://www.youtube.com/watch?v=XVlbYNMI3xg)
- [PL] In June 2025 PixelLab announced two new map tools on X: isometric tiles and sidescroller tilesets (snippet). [PixelLab on X](https://x.com/PixelLabAI/status/1933877847015264353)
- [3rd] 80.lv (13 Dec 2023) described early PixelLab as an Aseprite extension that turns a sketch plus prompt into pixel art. It said the tool was especially good for pixel maps and scenery. [80.lv](https://80.lv/articles/easily-create-pixel-art-based-on-sketch-with-this-ai-tool-for-aseprite/)

#### Camera views (what "side" and "three quarter" map to)
- [PL] The camera options are:
  - `high top-down`: camera at about 35 degrees.
  - `low top-down`: camera at about 20 degrees.
  - `side`: sidescroller view.

  PixelLab says view and direction controls are weak and work better with an init image. [PL docs: Camera](https://www.pixellab.ai/docs/options/camera)
- [PL] The v1 API schema calls `view`, `direction` and `isometric` "weakly guiding" on PixFlux. [API v1 schema](https://api.pixellab.ai/v1/openapi.json)

#### Tool by tool: purpose, max size, view, style inputs, background default
Scene capable generators:
- [PL] **Create S-XL image (Pro)** (API `generate-image-v2`, MCP `create_image_pro`):
  - Makes several variations from text. Minimum 16 px.
  - Square goes up to 512 x 512. Wider ratios go bigger, up to 688 x 384 at 16:9, and the closest supported ratio is picked automatically.
  - Takes up to 4 reference images plus an optional style image. The style image sets palette, outline, detail and shading.
  - Has a No Background toggle. The page does not mention scenes or a view option.

  [PL docs: Create S-XL image (Pro)](https://www.pixellab.ai/docs/tools/create-sl-image-pro)
- [PL] The v2 schema for `generate-image-v2` says background removal is automatic. Output count by size: up to 42 px gives 64 images, 43 to 85 px gives 16, 86 to 170 px gives 4, above 170 px gives 1. [API v2 schema](https://api.pixellab.ai/v2/openapi.json)
- [PL] The MCP docs agent says `create_image_pro` can make scenes if `no_background=False` is set, and lists no view or direction settings. PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **Pixen** (`create-image-pixen`):
  - Minimum side 16. Area at most 512 x 512. Width and height must divide by 4, and it must be square below 32 px.
  - Has `view` and `direction`. No style image, reference image or palette input.
  - Opaque background by default.

  [API v2 schema](https://api.pixellab.ai/v2/openapi.json); PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **PixFlux** (`create-image-pixflux`):
  - v2 area is 32 x 32 to 400 x 400. Supports init image, forced palette (`color_image`) and transparent background. v1 also caps each side at 16 to 400.
  - `init_image_strength` runs 1 to 999 (default 300).
  - The v1 schema notes a blank background above 200 x 200 area for `no_background`; the wording is unclear.

  [API v2 schema](https://api.pixellab.ai/v2/openapi.json); [API v1 schema](https://api.pixellab.ai/v1/openapi.json)
- [PL] **Naming trap:** `create-image-pixflux-background` is not a background scene tool. It takes the same parameters as PixFlux and runs as an async background job. [API v2 schema](https://api.pixellab.ai/v2/openapi.json)
- [PL] **BitForge** (`create-image-bitforge`, the style reference model):
  - Area at most 200 x 200 (v1: 16 to 200 per side).
  - `style_image` with `style_strength` 0 to 100 (default 0).
  - Inpainting with a black and white mask, init image, `color_image` forced palette, plus view and direction.

  [API v1 schema](https://api.pixellab.ai/v1/openapi.json)
- [PL] **Pro Flash**:
  - `create_image_pro_flash` is at most 256 per side, in multiples of 4. It takes a style image and style options and is transparent by default.
  - `edit_image_pro_flash` is at most 256 per side at the native canvas size, with a reference image mode.
  - `inpaint_image_pro_flash` is at most 256 per side. It keeps unmasked pixels exactly and uses a context image plus a bounding box.

  PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **Consistent style (Pro)** / `generate-with-style-v2`:
  - Takes 1 to 4 style images. Output is always square, sized from the largest style image (16 to 512), so it is not a 16:9 backdrop tool.
  - The doc advises several style images that share a look.

  [PL docs: Consistent style](https://www.pixellab.ai/docs/tools/consistent-style); [API v2 schema](https://api.pixellab.ai/v2/openapi.json)

Editing, extending and conversion:
- [PL] **Edit image (Pro)**:
  - Edits one image from text or a reference. Minimum 32 x 32.
  - Maximum 256 x 256 on tier 1; tier 2 and up can go larger, with cost bands listed up to 512 px.
  - Cost is 10, 15 or 25 generations for outputs of 1 to 256, 257 to 314 and 315 to 512 px.
  - The page does not mention masks or outpainting. Tier 1 or above is needed.

  [PL docs: Edit image (Pro)](https://www.pixellab.ai/docs/tools/edit-image-pro)
- [PL] **Inpaint v3** (MCP `inpaint_image`, API `/v2/inpaint-v3`):
  - Minimum 32 x 32. Maximum 256 per side on tier 1 and 512 per side on tier 2 and up. The limit is per side, so 512 x 64 and 512 x 512 both work on tier 2.
  - "Use Selection" sets the working area on canvases bigger than 256 x 256. "Crop to Mask" focuses on the mask.
  - Costs 10 generations per use and needs a paid tier.

  [PL docs: Inpaint v3](https://www.pixellab.ai/docs/tools/inpaint-v3)
- [PL] The older **Inpaint**:
  - Docs give 32 x 32 minimum, and 100 x 100 max on tier 1 or 160 x 160 on tier 2 and up.
  - The v1 API caps it at 200 x 200 area.

  [PL docs: Inpaint](https://www.pixellab.ai/docs/tools/inpaint); [API v1 schema](https://api.pixellab.ai/v1/openapi.json)
- [PL] **Image to pixel art**:
  - Input 16 to 2048 px. Output 16 to 512 px at about one quarter of input size, keeping the ratio.
  - `fixer=true` plus a higher `init_image_strength` gives a more faithful result.
  - The Pro version picks its own size and scales inputs over 2048 px down.
  - The Pro Flash version turns transparent areas white.

  [API v2 schema](https://api.pixellab.ai/v2/openapi.json); PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **Unzoom** puts upscaled pixel art back on its true pixel grid. It is not outpainting. The doc gives a 256 x 256 to 2048 x 2048 canvas, Aseprite and Pixelorama only. The v2 API also lists `/unzoom`. [PL docs: Unzoom](https://www.pixellab.ai/docs/tools/unzoom-pixelart); [API v2 llms.txt](https://api.pixellab.ai/v2/llms.txt)
- [PL] **Reduce colors**:
  - Area at most 512 x 512. The PixelLab docs page itself pitches the tool for matching a palette.
  - It quantises to an exact count or to a palette image of up to 256 colours, not both at once.

  [PL docs: Reduce colors](https://www.pixellab.ai/docs/tools/reduce-colors); PixelLab MCP search_help (KB, 11 Oct 2026)

Map, tile and object tools:
- [PL] **Create map (pixflux)** makes maps in top down or sidescroller view, with the same area caps as PixFlux (200 / 320 / 400). Options are description, camera view, outline, shading, detail, init image and target palette. [PL docs: Create map](https://www.pixellab.ai/docs/tools/create-map)
- [PL] By contrast, the MCP `create_map` is seeded with a top down tileset, has no side view option, and grows as terrain is painted. PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **Extend map** caps area at 140 x 140 (free and tier 1) or 200 x 200 (tier 2 and up). **Extend map v2** is experimental, at 180 x 180, and needs tier 2. [PL docs: Extend map](https://www.pixellab.ai/docs/tools/extend-map); [PL docs: Extend map v2](https://www.pixellab.ai/docs/tools/extend-map-v2)
- [PL] **Tilesets**:
  - `create_topdown_tileset` makes Wang tilesets for corner based terrain transitions. Tiles are 16 or 32 px (64 in Pro mode), in low or high top down view.
  - `create_sidescroller_tileset` makes 16 or 32 px platformer tiles with transparent backgrounds.

  PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] The Create Tileset doc lists Top Down (High Top Down) and Sidescroller types. Sidescroller asks for a center tile and a top tile description (snippet plus llms-full). [PL docs: Create tileset](https://www.pixellab.ai/docs/tools/create-tileset)
- [PL] **Create tiles (Pro)** makes square tiles from 16 to 128 px. View is `low top-down` (default), `high top-down` or `side`, and it takes style images. [PL docs: Create tiles (Pro)](https://www.pixellab.ai/docs/tools/create-tiles-pro)
- [PL] **Isometric tile** size conflicts. The doc says 32 x 32 or 16 x 16; the MCP docs agent says 64 x 64. [PL docs: Create isometric tile](https://www.pixellab.ai/docs/tools/create-isometric-tile); PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **Map objects** (`create_map_object`):
  - Canvas 32 to 400 px. View is `side`, `low top-down` or `high top-down` (default high top down).
  - An optional background image is used for style matching. Inpainting mode is capped at 192 x 192, and the default mask is an oval covering 60%.
  - It returns a new transparent object. The background is only context and is not edited.
  - The docs do not promise matched lighting or perspective.

  PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **UI assets** (`create_ui_asset` / Create UI elements Pro) are panels, not illustrations. Sizes: 512 x 512 square, 688 x 384 at 16:9, 600 x 448 at 4:3. They take a palette hint and a style image and are transparent by default. [PL docs: Create UI elements (Pro)](https://www.pixellab.ai/docs/tools/create-ui-elements-pro); PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **Title text**: the KB suggests generating a decorative frame or banner, then adding the text by hand in Aseprite with its Text Tool and a pixel font. Pro tools may produce readable text when prompted. PixelLab MCP search_help (KB, 11 Oct 2026)

#### Third party size claims (conflict with the official ones)
- [3rd] GameDev AI Hub (2026) gives PixelLab up to 320 x 320 on the Pixel Apprentice tier and 512 x 512 on Pixel Artisan, with animation capped at 128 x 128. It gives Retro Diffusion's website up to 276 x 276 per credit. [GameDev AI Hub](https://gamedevaihub.com/retro-diffusion-vs-pixellab/)
- [3rd] Review sites still repeat a 400 x 400 PixFlux cap for backgrounds (snippet). The official Pro limit of 688 x 384 supersedes it. [Knowara review](https://knowara.com/ai-tools/image/pixellab-review/); [PL docs: Create S-XL image (Pro)](https://www.pixellab.ai/docs/tools/create-sl-image-pro)

#### Retro Diffusion (the main alternative), for size comparison
- [3rd vendor] **RD Pro styles** are 12 to 256 px. They include `default`, `painterly`, `fantasy`, `horror`, `scifi`, `simple`, `isometric`, `topdown` and `platformer`. `rd_pro__typography` and `dungeon_map` are 64 to 256 px.
- [3rd vendor] **RD Plus** is 64 to 384 px, with styles including `environment`, `isometric` and `topdown_map`.
- [3rd vendor] Overall width and height are 12 to 512 px, and most styles top out at 256 or 384. `scene_object` is 64 to 384 px.

  [Retro Diffusion api-examples README](https://github.com/Retro-Diffusion/api-examples)
- [3rd] Scenario's guide:
  - RD Plus is meant for game scenes and was trained at 256 x 256 or below.
  - Going beyond native size can cause pixel duplication or colour bleed.
  - RD Tile is not meant for full scenes.

  [Scenario KB: Retro Diffusion essentials](https://help.scenario.com/en/articles/retro-diffusion-models-the-essentials/)

### Inferences
- **No PixelLab tool fills 1376 x 768 at 1x in one go.** The nearest is Pro at 688 x 384, which is exactly half the stage each way. So a Pro scene shown at 2x fills the stage with a clean whole number scale. But its pixels would then be twice the size of the 1x character sprite pixels (120 to 190 px tall). That is the mixed pixel size problem critics flag (see Q3).
- **512 x 288 does not scale cleanly to the stage.** It is the KB's typical recommendation, but 1376 / 512 = 2.6875, so it needs recomposing.
- **A 1x master means many overlapping windows** (Inpaint v3, Edit Image Pro):
  - On tier 2 (512 per side) with about 64 px overlap: about 6 windows (3 x 2).
  - On tier 1 (256 per side): about 28 windows.

  Each seam is a chance for style drift. This is arithmetic, not a PixelLab statement.
- **Scene friendly versus grid tools.** Pro has the largest canvas but no view parameter; Pixen has view control up to 512 x 512 area; PixFlux has view, init image and palette but a smaller area. The tilesets (Wang, sidescroller, tiles Pro, isometric) and `create_map` are built for navigable tile grids.
- **Why the rejected pass failed (likely).** It built battle stages and the camp tile by tile from top down Wang tilesets plus separately made map objects. That is how PixelLab's top down map pipeline is designed to work. It is not built for a composed side or three quarter vista with a horizon, which nothing in PixelLab's map tooling is documented to make.
- **"Three quarter" mapping.** For the Kindred Siege sprites this most likely means `low top-down` (about 20 degrees) or `high top-down` (about 35 degrees). Only Pixen, PixFlux, BitForge, map objects and Tiles Pro expose that setting; Pro does not.

### Gaps
- What the two PixelLab YouTube tutorials actually show, and any PixelLab Discord threads on backgrounds. Both are not indexed or readable here.
- The per side maximum for PixFlux in API v2 (only the 400 x 400 area is documented). The docs agent also found nothing.
- Whether `Create Image S-XL (New)` really takes an init image or sketch. PixelLab's own KB answers conflict.
- The real tier 2 and up maximum for Edit Image (Pro) beyond the 512 cost band.
- Credit cost of a full 688 x 384 Pro image (the doc only says cost depends on grid content size and ratio).
- Retro Diffusion's exact live limit for `rd_plus__environment` (the README points to a live style catalogue).

## Q2. How do developers keep one consistent style across many generated pieces?

### Takeaway
PixelLab's documented levers are:
- style images (Pro: one style image plus up to 4 references; Consistent Style: 1 to 4 images sharing a look);
- a target palette with "Force colors";
- init images (strength 400 to 600 for variations);
- inpainting with part of the selection left unpainted so the model sees the existing style.

Users and reviewers still report unifying palettes by hand afterwards. The most robust practice reported is to edit one master image rather than assemble many independent generations.

### Cited Findings

#### PixelLab's own claims
- [PL] The style image in Pro sets colour palette, outline, detail and shading, with options for which to copy. Up to 4 reference images can guide content. A fixed seed reproduces results. [PL docs: Create S-XL image (Pro)](https://www.pixellab.ai/docs/tools/create-sl-image-pro)
- [PL] The Consistent Style doc advises using several style images that share a similar look for more consistent results. [PL docs: Consistent style](https://www.pixellab.ai/docs/tools/consistent-style)
- [PL] The homepage claims its tools adapt to your reference images to keep sprites consistent, and lists style consistent generation and reference based UI. [PixelLab homepage](https://www.pixellab.ai/)
- [PL] Colour options:
  - "Limit colors used" restricts output to a chosen palette or the current image's palette.
  - "Force colors" makes output follow the Target Palette.
  - PixFlux and BitForge take a `color_image` as a forced palette in the API.

  [PL docs: Color](https://www.pixellab.ai/docs/options/color); [API v1 schema](https://api.pixellab.ai/v1/openapi.json)
- [PL] Init images:
  - They often give a better result and help colours and perspective.
  - Strength 400 to 600 suits variations of an existing image; 600 to 900 suits detail work.
  - For Extend map, very rough init images are called extremely useful, and lower strength gives the model more freedom.

  [PL docs: Init image](https://www.pixellab.ai/docs/options/init-image); [PL docs: Extend map](https://www.pixellab.ai/docs/tools/extend-map)
- [PL] Inpainting master scene method (Create Map guide):
  - The model only sees what is inside the selection.
  - Select an area that overlaps existing art and sketch roughly inside it.
  - Paint the inpaint layer only where new content should go, leaving the rest as a style reference for smooth transitions.
  - Describe what is in the middle of the selection.
  - Repeat, then use inpainting again for final fixes.

  [PL guide: Using the Create Map tool](https://www.pixellab.ai/docs/guides/map-tiles)
- [PL] Inpainting layer: you draw black on the "Inpainting" layer where changes should happen. Inpaint, Style, Map, Rotate and Animation all support it. [PL docs: Inpainting](https://www.pixellab.ai/docs/options/inpainting)
- [PL] Extension guidance:
  - The mask marks pixels to regenerate and transparent areas stay frozen.
  - The selection controls what the model sees, so include context.
  - Oversized work should be done in overlapping patches.

  PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] For map objects, a background image can be supplied for style matching. Output is still a separate transparent object. PixelLab MCP search_help (KB, 11 Oct 2026)

#### Users and third parties
- [User/3rd] Jonathan Yu's hands on review (31 Dec 2025) found the style reference feature made matching existing art very easy, with consistent style across characters. He did not test backgrounds. Most features live in the Aseprite extension, which he calls the best way to use PixelLab. [Jonathan Yu review](https://www.jonathanyu.xyz/2025/12/31/pixellab-review-the-best-ai-tool-for-2d-pixel-art-games/)
- [3rd] GameDev AI Hub (2026) says with either PixelLab or Retro Diffusion you still nudge frames, fix sliding feet and unify palettes by hand. It also says PixelLab's newer models cost about 40 credits each. [GameDev AI Hub](https://gamedevaihub.com/retro-diffusion-vs-pixellab/)
- [3rd vendor] **Retro Diffusion consistency tools**:
  - `input_palette` locks output to a palette image.
  - RD Pro takes up to 9 `reference_images`.
  - img2img `strength` runs 0 to 1 (default 0.75).
  - `tile_x` and `tile_y` give seamless tiling.

  [Retro Diffusion api-examples README](https://github.com/Retro-Diffusion/api-examples)
- [3rd vendor] **Retro Diffusion extension and website**:
  - The extension adds Palettize and Color Style Transfer.
  - It uses different models from the website, and the page warns you cannot get website results from it.
  - The extension is being phased out for a native editor.

  [Retro Diffusion itch page](https://astropulse.itch.io/retrodiffusion)
- [3rd tool vendors] Aseprite plugins aimed at this step:
  - Spriteloom inpaints only inside the selection and can pin output to the project palette (snippet). [Spriteloom devlog](https://vkarach.itch.io/spriteloom/devlog/1616156/spriteloom-v100-local-ai-pixel-art-generator-for-aseprite)
  - DeAI PixelKit targets fuzzy edges, uneven block sizes and noisy backgrounds, and snaps fake pixel grids back to true resolution. It is in development (snippet). [DeAI PixelKit](https://onlyjasoncando.itch.io/deai-pixelkit)
- [3rd vendor] Sprite AI's background guide (updated 11 Aug 2026):
  - Background and character pixel scale should match.
  - Derive 8 to 16 desaturated background colours from the sprite palette and use palette transfer.
  - AI output is roughly 80% there and needs refining in Aseprite. The figure comes with no data.

  [Sprite AI blog](https://www.sprite-ai.art/blog/pixel-art-backgrounds)

### Inferences
- The levers that work across a whole set are:
  - the same style image or images on every call;
  - one locked palette (a target palette on generation, then `reduce_colors` with a palette image, or Aseprite's own palette remap);
  - growing one master canvas by overlapping inpaint windows, rather than pasting independent generations.
- Pro has the best style control but the weakest perspective control (no `view`). A rough init or blockout is the documented way to pin perspective, so it matters most for scenes.
- `reduce_colors` caps at 512 x 512 area, so a 1376 x 768 master would need palette unification in Aseprite or done in pieces.

### Gaps
- No public account was found of anyone holding PixelLab style across many separate backdrop images, as opposed to characters.
- No measured data on how much drift builds up over repeated inpaint extensions.

## Q3. What do experienced pixel artists and developers say about AI pixel art for environments, and what are the limits and failure modes?

### Takeaway
Two kinds of tell come up:
- **Craft tells:** uneven pixel or block size ("mixels"), grid drift, wasteful palettes, stray pixels and half transparent edge pixels.
- **Scene logic tells:** perspective, lighting and proportion that do not agree across a set, and objects that make no physical sense.

PixelLab itself concedes weak view control, small canvases, experimental large maps and unreliable background removal on map editor objects. Shipped examples that worked used AI images only as a base and were heavily hand edited.

### Cited Findings

#### PixelLab's own admitted limits
- [PL] View and direction controls are weak, so pair them with an init image. [PL docs: Camera](https://www.pixellab.ai/docs/options/camera)
- [PL] Map canvases over 256 x 256 are experimental and less stable. First results may need several retries. [PL guide: Using the Create Map tool](https://www.pixellab.ai/docs/guides/map-tiles); PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] The map model is trained on 16 x 16 tiles, and the guide works at 128 x 128 (an 8 x 8 tile area). [PL guide: Using the Create Map tool](https://www.pixellab.ai/docs/guides/map-tiles)
- [PL] A KB FAQ says objects generated in the map editor can get cut off or show odd background removal artifacts. PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] Default background behaviour differs by tool. A scene needs the transparent default switched off; an object made with an opaque default tool needs its background removed.
  - Pro removes backgrounds automatically.
  - Pro Flash is transparent by default.
  - Pixen and PixFlux are opaque by default.

  [API v2 schema](https://api.pixellab.ai/v2/openapi.json); PixelLab MCP search_help (KB, 11 Oct 2026)

#### Artists and developers
- [User] Hacker News thread on GameTorch, a different AI sprite tool, June 2025:
  - Commenters said block size varies within and between images, so it is not real pixel art.
  - Nearest neighbour scaling to the grid fixes many images but not all.
  - SXX (whose company shipped three pixel art titles) said consistent good results are not possible, and that matching style, lighting and camera perspective is near impossible.
  - lesostep noted wildly different proportions and nearly transparent floating pixels that complicate collision.
  - kubb said there were not enough assets to build environments.

  [Hacker News](https://news.ycombinator.com/item?id=44207304)
- [User] An r/PixelArt thread (Apr 2025) noted that mixels, strays and wasteful palettes are AI features but also common beginner mistakes. It warned against false accusations and suggested asking for timelapses or layered files instead. [r/PixelArt via redlib](https://redlib.ssps.io/r/PixelArt/comments/1k26vck/i_dont_use_ai)
- [User] On the Steam page for The Rangers In The South (Jan 2025):
  - Players flagged intro backgrounds as AI because they did not make physical sense, citing a couch backed against a fireplace and dozens of lanterns at random sizes.
  - The solo developer said Photoshop's fill tool was used on some images.
  - At least one player said the AI disclosure stopped a purchase.

  [Steam discussion](https://steamcommunity.com/app/3074110/discussions/0/595137684887881384/)
- [User] An itch.io commenter who uses PixelLab said it can reproduce a commercial pixel art building pack's style, but with less detail (snippet). [itch.io comments](https://pixelslime99.itch.io/buildingforge-urban-edition/comments)
- [User] A visual novel background seller (not pixel art) said AI still gets architecture and furniture details wrong, along with the consistency of light and shadow, and advised using AI images as drafts or overpaint bases (snippet). [itch.io comments](https://lisadikaprio.itch.io/109-visual-novel-backgrounds/comments)
- [User] Another non pixel art seller listed the fixes needed on AI backgrounds: deformed lamps, windows, text and chairs, poor lighting, and blurry upscales. They were fixed with repainting, added shadows, layering and gradients to hide flaws. [Rachel Chen on itch.io](https://rachelchen.itch.io/ai-made-backgrounds-vs-rachels-backgrounds-the-difference)
- [3rd] A blog guide (authority unclear) says consistency is AI's weakest skill and grid based assets like tilesets are where AI fails. It adds that cleanup can take longer than drawing for some assets (snippet). [qwe.edu.pl](https://www.qwe.edu.pl/tutorial/create-pixel-art-with-ai-tools/)
- [3rd vendor] The Retro Diffusion developer said the extension's PixFix cleanup filter is not like doing it by hand, and called 720 x 1024 much too high resolution for pixel art. [Retro Diffusion itch page](https://astropulse.itch.io/retrodiffusion)
- [3rd] Scenario says Retro Diffusion was trained with experienced pixel artists. It says enlarging past native size (256 x 256 or below for RD Plus) causes pixel duplication or colour bleed. [Scenario KB](https://help.scenario.com/en/articles/retro-diffusion-models-the-essentials/)
- [3rd] Jonathan Yu found PixelLab clearly weaker at small sizes (16 x 16 or less). He said the docs go out of date fast, so you burn credits experimenting. [Jonathan Yu review](https://www.jonathanyu.xyz/2025/12/31/pixellab-review-the-best-ai-tool-for-2d-pixel-art-games/)

### Inferences
- Of the failure modes in the brief:
  - **Inconsistent perspective between generations:** directly supported. PixelLab calls its own view control weak, and artists name perspective and lighting as the hardest thing to match.
  - **Style drift:** supported indirectly. The map guide's rule to keep unpainted context exists to prevent it, and reviewers unify palettes by hand.
  - **Size limits:** documented above.
  - **Opaque or cut off object backgrounds:** acknowledged in PixelLab's KB.
- A stage built from separately generated objects risks the scene logic tells players catch: objects that do not share a light source, ground plane or scale. That is worse than craft tells on a fixed screen viewed for whole fights.

### Gaps
- **Flat or empty backgrounds:** no source specifically documents PixelLab scene outputs being flat or empty. Only general remarks (kubb on HN) and the vendor advice to use 3 or more layers.
- No named, well known pixel artist's published critique of PixelLab environments specifically was found. Reddit threads were not indexed by the search tools.

## Q4. Which shipped or in development games used PixelLab or similar tools for environments, and how?

### Takeaway
No shipped game was found that publicly credits PixelLab or Retro Diffusion for environments. PixelLab's own homepage names none. The documented public cases are Steam AI disclosures that do not name a tool. The Great Rebellion used AI images only as a starting base for some level backgrounds and props, then heavily hand edited them to match its pixel art.

### Cited Findings
- [PL] PixelLab's homepage names no games or studios. It only claims to be liked by over 3,000 indie developers. [PixelLab homepage](https://www.pixellab.ai/)
- [PL] PixelLab's FAQ covers Steam's AI disclosure survey and says AI assets are allowed in games (snippet). [PL docs: FAQ](https://www.pixellab.ai/docs/faq)
- [User/disclosure] **The Great Rebellion** (pixel art, 99% positive of 280 reviews at time of writing):
  - Its Steam disclosure says some backgrounds in a few levels, and a few props, were based on AI images.
  - The images were used only as a base and then "heavily edited manually afterwards" to fit the game's pixel art style.
  - The tool is not named.

  [Totally Human Media](https://www.totallyhuman.io/blog/the-surprising-number-of-steam-games-that-use-genai)
- [3rd] The same article found about 1,000 Steam games disclosing GenAI use, sorted into eight usage categories, including background and environment art. [Totally Human Media](https://www.totallyhuman.io/blog/the-surprising-number-of-steam-games-that-use-genai)
- [User/disclosure] The Rangers In The South disclosed AI use. The developer said Photoshop's generative fill helped with some images, and players disputed how far it went (see Q3). [Steam discussion](https://steamcommunity.com/app/3074110/discussions/0/595137684887881384/)
- [3rd] itch.io asset packs tagged AI generated pixel art backgrounds describe AI first passes corrected and refined by hand for consistency, without naming the tool (snippet). [itch.io tag listing](https://itch.io/game-assets/newest/released/tag-ai-generated/tag-backgrounds/tag-pixel-art)
- [3rd] Steam rewrote its disclosure form on 16 Jan 2026. The rule now covers AI content that ships in the game and reaches players, including store page and marketing assets (snippet). [IndieForGames](https://indieforgames.com/steam-ai-disclosure-rules/)

### Inferences
- Any PixelLab output that ships in Kindred Siege's stages, hub or title screen would fall under Steam's player facing disclosure. That is a reputational factor, given the backlash shown on The Rangers In The South page.
- The one positive public precedent (The Great Rebellion) fits a "base layer, then heavy hand repaint" pipeline, not shipping raw generations.

### Gaps
- No game was found that credits PixelLab by name for backgrounds. Steam disclosures rarely name tools. Searches of Steam, itch.io, Reddit and X turned up none.
- The Great Rebellion's tool and exact workflow are not documented.

## Q5. What is a sensible pipeline, and what are practical routes to a coherent battle backdrop, hub scene and title screen?

### Takeaway
PixelLab's own documented route for big scenes is:
1. Rough init or sketch first.
2. Generate a base at the largest scene size (S-XL / Pro).
3. Extend or repair it section by section with Edit Image (Pro) or Inpaint v3, using overlapping selections that keep existing pixels as context.
4. Finish with inpainting.

Text goes on by hand in Aseprite. Users add palette unification and hand cleanup afterwards, and a vendor guide adds splitting the scene into layers at one pixel scale. Nothing in PixelLab's docs supports building backdrops from top down tilesets plus separate objects.

### Cited Findings
- [PL] **Composition first.** Init images help colours and perspective. View control is weak without them. Very rough init images are extremely useful when extending. [PL docs: Init image](https://www.pixellab.ai/docs/options/init-image); [PL docs: Camera](https://www.pixellab.ai/docs/options/camera); [PL docs: Extend map](https://www.pixellab.ai/docs/tools/extend-map)
- [PL] **Base generation.** S-XL (New) is the recommended start for big 16:9 scenes (512 x 288 typical, init strength about 430 to 530). Pro goes up to 688 x 384 with a style image and references. Set `no_background=False` for backdrops. [PL docs: Create S-XL image (Pro)](https://www.pixellab.ai/docs/tools/create-sl-image-pro); PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **Extension and repair.**
  - Extend past 512 with Edit Image (Pro) section by section, plus partial inpainting.
  - Inpaint v3 works on a selection of a larger canvas: up to 512 per side on tier 2 and up, at 10 generations per use.
  - Leave part of each selection unpainted for style context and describe the middle of the selection.

  PixelLab MCP search_help (KB, 11 Oct 2026); [PL docs: Inpaint v3](https://www.pixellab.ai/docs/tools/inpaint-v3); [PL guide: Using the Create Map tool](https://www.pixellab.ai/docs/guides/map-tiles)
- [PL] **Objects into a scene.** Map objects can take the scene as a style reference but come back as separate transparent objects. Inpaint mode is capped at 192 x 192. PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **Concept painting to pixels.** `image-to-pixelart` takes input up to 2048 px and outputs up to 512 px at about a quarter of the input size. `unzoom` restores the grid of upscaled pixel art. [API v2 schema](https://api.pixellab.ai/v2/openapi.json); [PL docs: Unzoom](https://www.pixellab.ai/docs/tools/unzoom-pixelart)
- [PL] **Palette.** Target palette with Force colors during generation. `reduce_colors` with a palette image (up to 256 colours) afterwards, at most 512 x 512 area. [PL docs: Color](https://www.pixellab.ai/docs/options/color); [PL docs: Reduce colors](https://www.pixellab.ai/docs/tools/reduce-colors); PixelLab MCP search_help (KB, 11 Oct 2026)
- [PL] **Title text.** Generate a frame or banner, then set the logo text by hand in Aseprite with a pixel font. PixelLab MCP search_help (KB, 11 Oct 2026)
- [3rd vendor] **Layering.**
  - Use a sky layer plus far, mid and near layers, with an optional foreground. Three is the sweet spot and five is the practical maximum.
  - Use the same pixel scale for background and characters.
  - Lower contrast and saturation with distance.
  - Keep the background palette smaller than the sprites' and derived from it.

  [Sprite AI blog](https://www.sprite-ai.art/blog/pixel-art-backgrounds)
- [User/disclosure] **Finish by hand.** In The Great Rebellion, AI was the base and the result was heavily hand edited to match the pixel art. GameDev AI Hub reports palettes still unified by hand. [Totally Human Media](https://www.totallyhuman.io/blog/the-surprising-number-of-steam-games-that-use-genai); [GameDev AI Hub](https://gamedevaihub.com/retro-diffusion-vs-pixellab/)
- [3rd] **Seamless tiling.** Runware notes Retro Diffusion's directional seamless tiling as useful for game backgrounds (snippet). On Kindred Siege's fixed screen it matters only for wrapping sky or far layers. [Runware blog](https://runware.ai/blog/retro-diffusion-creating-authentic-pixel-art-with-ai-at-scale)

### Inferences
These are synthesis for Kindred Siege, not sourced claims. Stage: 1376 x 768 at 1x. Sprites: 120 to 190 px tall, three quarter view.

**Order of work.** Composition comes first and is free:
- a flat value or colour blockout of the whole stage at final size;
- the horizon, ground plane and lane where fighters stand, a quiet floor under them, and the sky / far / mid / near split;
- a check at true size with real sprites placed.

This matches PixelLab's "init image first" advice and the owner's standing "mock free first" rule. A tile by tile build has no such whole scene step.

**Route A, battle backdrop at 2x.** Generate one Pro scene at 688 x 384 with `no_background=False`. Feed in the blockout as reference, and use one locked style image for every stage. Show it at exactly 2x.
- Pros: cheapest and most coherent, since it is one generation with one light and one perspective.
- Con: background pixels are twice the sprite pixel size. That breaks the "one pixel size" rule critics use as the main AI tell.
- Fit: works only if the owner accepts a chunkier backdrop behind finer sprites, or if sprites are redrawn at half scale.

**Route B, battle backdrop at 1x.** Keep one 1376 x 768 master canvas in Aseprite:
1. Place the blockout.
2. Fill it with Inpaint v3 windows of up to 512 per side (tier 2), each overlapping existing pixels as context, in about 6 windows.
3. Unify the palette, hand clean, then split into layers.

- Pro: highest fidelity.
- Cons: the most credits, and the highest risk of seams and drift, which PixelLab's own map guide warns about. On tier 1 (256 per side) it needs about 28 windows, which is probably too many.

**Route C, paint over.** Paint a high resolution concept or render (Blender is installed), convert it with image to pixel art (512 px output cap, so split into regions or accept a 2x result), then repaint by hand.
- This is closest to how the one positive Steam precedent describes its process.
- Many critics say cleanup is where the time goes.

**Hub or camp.** Treat it as one composed scene, not a tiled map, using the same route as the battle backdrop. The interactive buildings or stations should be cut from the master as layers, not generated as separate map objects, so they share light and perspective. Map objects only match style through a reference image, not by being drawn into the scene.

**Title screen.** Compose the logo space into the illustration from the start. Do not paste the logo on a strip afterwards; that was the rejected 480 x 300 approach. PixelLab has no title tool, and its KB says to hand set text in Aseprite. Retro Diffusion's `rd_pro__typography` style (64 to 256 px) exists, but its quality is untested here.

**Credit discipline.** Pro class calls cost much more (about 40 credits per GameDev AI Hub; Inpaint v3 is 10 generations per use). So lock composition, palette and style image on free mocks before any paid call.

### Gaps
- No published end to end case study was found of a side or three quarter battle backdrop, hub or title screen made with PixelLab. All routes above are inferred from PixelLab's tool docs, its KB and general cleanup reports.
- Untested: whether Pro output at 688 x 384 holds detail and perspective well enough at 2x behind 1x sprites. That needs a free mock before any credit spend.
- No source gives realistic hand cleanup hours per AI backdrop.
