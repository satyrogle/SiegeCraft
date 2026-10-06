# Kindred Siege - Final owner design constraints

Status: LOCKED by Jacob on 2026-09-30.

Owner instruction: "these are my final corrections and will not be changed ever".

These conditions govern subsequent design, datasets, art planning and implementation. They supersede conflicting earlier proposals and older design documents. Existing code is implementation history, not permission to change these conditions. Do not reopen, reduce, expand or silently reinterpret the locked counts, tier structure, genre, priorities or battle presentation.

## The ten locked conditions

1. **24 tribes in total:** 23 tribes plus the Sovereign's tribe as **Alpha**.
2. **7 tiers** divide the tribe rosters.
3. **Tiers 1-3:** starting rosters; they provide most of the narrative.
4. **Tiers 4-5:** midgame rosters; they pave the path to endgame.
5. **Tier 6:** pre-endgame; unlocks most of the stuff in the game.
6. **Tier 7:** Sovereign.
7. **Genre and pace:** roguelike, fast-paced action.
8. **17 classes:** unlocked through progression. How class unlocks connect to tiers remains to be decided.
9. **Top priority:** "Items, Pals, upgrades and tribes codex" (owner wording, spacing normalised).
10. **Battle screen:** 2.5D.

The Sovereign's Alpha tribe is included in the total of 24, not added on top of 24. Tier 7 is the Sovereign tier; allocation of the other 23 tribes across tiers 1-6 is not yet specified.

## Owner clarification: Pals and the science-fiction premise

Clarified by Jacob on 2026-09-30: Pals are the main character's trusted inner circle. After the opening fight wipes out their force, the main character and a few trusted friends survive. Those surviving people are the Pals, and they serve as upgraders.

The game's tribal roster must be developed in connection with the group's advanced science-fiction technology. Pals are the people through whom upgrades are developed/applied; their specific identities, specialties, costs and upgrade rules remain to be designed. Do not assume that being an upgrader also defines whether a Pal fights, resides at a hub, or occupies a recruitable combat-class slot.

The earlier assistant interpretation of Pals as collectible creatures, pets or handler-controlled combat companions was incorrect. The proposed Beastwarden class depended on that mistake and its definition was withdrawn. In the final catalogue, its reserved C15 slot is Disruptor. Tribal identity does not imply a generic medieval or primitive setting. The class catalogue grounds combat equipment in future technology adapted to tribal capabilities.

## Owner clarification: Rivalry operates alongside progression

On 2026-09-30 Jacob rejected the revised class-tier proposal and reminded Codex: "we also need to make sure to run rivalry engine parallelly". The Rivalry Engine must remain part of the design alongside tribe recruitment, class unlocks, items and Pal upgrades. Class progression must account for its interaction with rival encounters and adaptation; it cannot be finalized in isolation.

The two preceding class-tier distributions are unapproved and must not be implemented as owner decisions. This clarification does not change the 24 tribes, seven tiers, 17 classes or other ten locked conditions. Detailed rivalry rules and their mapping to the current classes still require review; existing prototype code is not proof of correct runtime integration or approval of every historical mechanic.

## Final class catalogue

At Jacob's instruction "lets finalise the classes", the complete class definitions were finalized on 2026-09-30 in `CLASS_ROSTER.json`. This is the current design catalogue; `CLASS_ROSTER_DRAFT.json` is superseded history.

The 17 classes are Warden, Vanguard, Duelist, Berserker, Marksman, Skirmisher, Artillerist, Infiltrator, Trapper, Recon, Medic, Herald, Chemist, Field Engineer, Disruptor, Binder and Conduit.

Each has a distinct combat role, a tribal skill foundation, technological equipment, a signature action and a weakness. Pals remain the surviving inner-circle upgraders. Field Engineer's battlefield deployment role does not replace that Pal function. Tier assignments, progression requirements, numerical balance, tribe availability and detailed equipment/upgrade recipes remain open. Finalized design definitions do not assert implementation, playtest results or approval of a visual asset.

## Decisions still open within those constraints

- The complete tribe roster and the number of tribes in each of tiers 1-6.
- The 17 classes' progression requirements and their connection to tiers; names and core roles are defined in `CLASS_ROSTER.json`.
- Which class/tribe combinations exist. Do not assume every tribe fields every class.
- Detailed rules, content and presentation for Items, Pals, upgrades and the tribes codex, while retaining their top priority.
- Which unlocks belong to which stage, including what Tier 6's "most of the stuff" specifically covers.
- Camera, rendering and asset choices within the locked 2.5D battle presentation. 2.5D alone does not select an isometric camera or require a particular sprite/mesh pipeline.

Item 3 in the owner's message ends with an unfinished "and". Only the supplied complete statement about starting rosters and most of the narrative is recorded as a condition; no missing requirement has been invented.

## How this changes the preceding discussion

- The eight classes found in the existing code are not the final class catalogue. Use the 17 definitions in `CLASS_ROSTER.json`; runtime enum, data, save and behaviour migration remains implementation work.
- The earlier seven-tribe sketch and three-record/guard-scout examples were proposals or examples. They do not define the final roster size.
- The earlier request for a small, grounded dataset remains relevant to implementation: use bounded authored records and shared systems to realise the fixed scope. Smaller development tests do not change the final totals.
- Preserve "roguelike, fast-paced action" as the genre direction. Do not substitute a different genre based on the older GDD or prototype.

This record locks the owner's design decisions. It does not claim that the game, 24-tribe dataset, 17-class roster, codex or 2.5D battle screen has already been implemented or validated.

## Owner calls, 1 Oct 2026 (Jacob's answers, recorded from HANDOFF_LOG.md)

1. Fight model: "Auto battle + directives". Units fight on their own; the player spends directive points (Hold, Fall Back, Focus Fire, Unleash, Sacrifice), with pause and speed. On 2 Oct the orders became refilling buttons (below).
2. Innovation claim: units and tribes that learn from real fights, including the wars between tribes, with a replay of why. Rivalry stays ("there will be no narrative without this"), running on the learning system.
3. The 8 Dec visa slice: Fallen Star; three tribes (the Clarth, a raider tribe whose chief is the rival, a third tribe at war with the raiders); five classes (Warden, Marksman, Skirmisher, Trapper, Medic); about 20 to 30 minutes.
4. Counts: "yes lock it". Tribes and classes are hand made and unchanging (24 and 17 remain the full game); everything inside them is procedural: individual fighters, traits, leaders, what they learned, and the wars between tribes.
5. Art: PixelLab for sprites and animation.

## Owner calls, 2 Oct 2026 (Jacob in chat with Claude)

These add to the ten locked conditions; none of the ten changes. Items marked "Claude's detail" were proposed by Claude and not objected to, but not confirmed word for word.

**Premise and tone**
- The player's people are a cult of cyborg flesh beings ("cybernetic bio organisms"; not synths, not ethereal) who worship old gods and follow their teachings. Cunning and vile, sure they are the top predator.
- They land on a rogue death planet where everything alive fights (references: Genna in Predator: Badlands, the hubris of Outriders). The opening god tier army is wiped out by the Sovereign in one second. This replaces "crash landed" wherever the wording differs.
- Tone: funny everywhere, in the spirit of Ironhide's Kingdom Rush; only the Sovereign is truly scary.
- The "three tit rule": every character and region needs one look a gamer gets in one second from a screenshot. Lore can be clever; the look must be instantly appealing. An absurd, addictive loop that players can beat in many ways comes before niche artistry.

**Loop**
- Fight: auto battle. Order buttons refill during the fight so the player stays busy (Kingdom Rush). Orders come from the leading Pal's god (Claude's detail) plus the signature moves taken from broken tribes (Mega Man Battle Network).
- Break: each tribe has a nerve bar. Hits, scary orders and gear drain it; at zero the tribe gives up and everyone still alive joins the cult (kill fewer, keep more). This replaces "breaks below half strength".
- The chief of a broken tribe runs away with what he learned and comes back later leading a splinter group.
- Upgrades: Pals forge gear from what you won, and the Pal who broke a chief learns that chief's gear recipe (Summon Night: Swordcraft Story). Gear enhances a unit's look, like He Man's Snake Armor. No mutations and no extra limbs.
- The answer to the "focus their Medic and Hold" exploit: holding drains your own nerve over time, and chiefs can also learn to flank a holding line.
- Death: army, gear and converts reset; chief memories, the codex and Pal stories stay. Our own units keep learning.
- Modes: a campaign big enough to feel endless, plus an endless mode (the same world with the ending switched off). Battles are generated by the rivalry engine inside the 24 tribe and 17 class locks. Tribes are still collected in the codex.

**Pals**
- Five survivors: the leader (the player, kept quiet) and four Pals. Pals are still the upgraders, and a Pal leads each fight as the hero. Claude's detail: one Pal per fight, the fight is lost if the Pal falls, and only Pals transform mid fight (the Cringer into Battle Cat moment).
- The four Pals take Warhammer Chaos god traits only, never Games Workshop's looks: the Butcher (war, rage), the Glutton (rot, endurance), the Shade (change, tricks), the Whisperer (excess, charm). Names are placeholders. They do not hate each other.
- Personalities are cheeky, in the spirit of the Dave the Diver cast. Each has a backstory of what the Sovereign's one second did to them, and their final chapters land at the Sovereign rematch (Octopath Traveler style arcs).

**Tribes and planet**
- Character look (Jacob, 3 Oct; this replaces the earlier "one native species" and "small horns" calls): every native is lean and toned, never bulky, "hot and menacing", in light battle gear with one signature piece of identity. Each tribe is its own race, mixing several tribal or advanced races from different films and games, never a copy of one source (Jacob rejected drafts that read as Dek's species, an old god and a World of Warcraft character). Realistic heroic proportions, not chibi. The three tit rule always applies.
- Each tribe also has the monster it hunts and the wreck it scavenged. Chiefs ride their region's monster. Converts keep their look and add the cult sigil and their Pal's gear.
- The Clarth (Broken Wastes), approved 3 Oct: war hunters with crested bone war masks, red cloaks, toned bodies, bronze bracers and braided hair; the slice squad is three women and two men. Their signature pieces come from the sand shark they hunt: jaw shield (Warden), spine rifle (Marksman), fin blades (Skirmisher), tooth net (Trapper), tooth totem staff (Medic). Their chief surfs a giant sand shark.
- The Bloodridge raiders (the rival's tribe), locked by Jacob on 3 Oct: Egyptian desert guardians after the Medjai (the black robed guardians of The Mummy and the real Medjay), finished in the mood of Zdzisław Beksiński's paintings. Rotted sepia and ash brown rags, burial bandages on the arms and legs, corroded bronze collars and armbands, ashen skin, lava glowing through cracks. Two kinds of head: faceless hoods with ember eyes for the Warden and Trapper, lava tattooed faces under hoods and headwraps for the Skirmisher, Marksman and Medic. Signature pieces: tall Egyptian war shield and khopesh (Warden), bronze fitted rifle (Marksman), twin khopesh (Skirmisher), hooked bronze flail (Trapper), sceptre with an ember lantern (Medic). Their chief rides a crab the size of a house. By Jacob's choice this tribe is truly scary, an exception to "only the Sovereign is scary". Rejected on the way: Beksiński style shrouds with crab part weapons, the same shrouds with forged weapons, and a clean black, indigo and gold Medjai that lost the Beksiński mood.
- Skullpeak (at war with the raiders: sharpshooters, chief on a giant vulture), approved by Jacob on 3 Oct: humanoid beings who grew machine parts into their own bodies, with H. R. Giger as the reference for scope, not for his actual designs, never alien looking and never a copy of the xenomorph. Warm human skin with polished steel bones showing through the chest and ribs, a steel crest that stops at the hairline so the face shows, dark slate wraps and a turquoise glow in the seams. Each unit's signature piece is one bionic arm, after Bionic Commando: grappling claw arm and tower shield (Warden), arm ending in a curved blade (Skirmisher), grapple arm with a hook on a chain (Trapper), rifle built into the arm (Marksman), injector needles and turquoise vials (Medic). Rejected on the way: armoured biomech brutes, black biomech creatures, a human in a Giger suit, other peoples with a Giger finish, a scope eye and piston muscles.
- Chiefs (Jacob, 3 Oct): Jacob made a placeholder design for each chief and had them drawn in PixelLab from those, as 192 px fronts that stand a head taller than their Wardens. Approved: the Clarth chief, a lean war chief with red ochre war paint and white stripes, a crested bone crown, long black braids, a white fur mantle, a red cloak with white sigils, dark armour with bronze trim and a long spear with a glowing red blade ("clarth guy is set"; the placeholder's Predator style face was dropped for a human one). The Bloodridge chief, a faceless ember hood with antlers, ram skulls on one shoulder, a corroded bronze collar, rotted layered rags and bandages, small skulls and charms, a lava sun tabard and a staff holding a burning ring ("option 1 for bloodridge is fine"; the first, cleaner round was too simple and lost the rot). The Skullpeak chief, a woman with a white undercut and a scarred blind eye, three breasts in a fitted black top framed by steel ribs with a turquoise glow, a torn black cape, bone gauntlets, a turquoise lit sword and a steel bionic leg (the three breasts are Jacob's call; in the approved drawing they read only faintly). Their mounts stay as above.
- Pals (Jacob, 3 Oct): Jacob draws the four Pals himself. Agents do not draw them.
- Colour (Jacob, 3 Oct): every battle sprite is cut to 32 colours in Aseprite, with one palette shared by all eight of its directions and its idle frames.
- Size ladder (Jacob, 3 Oct): tanks are drawn biggest and most brutal, melee damage dealers medium, ranged and support small, so every squad reads big to small. In the slice: Warden 128 px; Skirmisher and Trapper 112 px; Marksman and Medic 96 px.
- Regions keep the concept map names (Broken Wastes, Bloodridge, Stoneveil, Thundercoast, Skullpeak, Silvertree, Echo Reed), each with one screenshot hook. The planet is littered with the wrecks of earlier armies the Sovereign beat; that joke lives in the codex and the Pals' lines, not in the look.

**Slice scope and art**
- The 8 Dec slice is the core loop plus one transform: fight, nerve break, convert, one Pal (the Butcher), one forged gear piece, and a chief who runs and comes back smarter.
- Characters are drawn in PixelLab, not the local pipeline (the local drafts of 2 Oct were rejected). Battle sprites follow the size ladder (128, 112 and 96 px) and fight in the three quarter views. Each tribe's approved units are the style reference for the rest of that tribe. Briefs and asset IDs live in `PIXELLAB_BRIEFS_SLICE.md`. Every generation must count.
