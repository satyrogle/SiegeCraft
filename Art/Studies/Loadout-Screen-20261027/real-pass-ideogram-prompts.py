# Builds the Ideogram 4 API graphs for the loadout screen real pass, 27 Oct 2026.
# Same graph as the talent tree round 3 (and the 18 Oct chief probe). Three takes, one combo taught big.
# bbox is [top, left, bottom, right] on a 0 to 1000 grid.
#   python -I real-pass-ideogram-prompts.py TAKE SEED          prints the API graph
#   python -I real-pass-ideogram-prompts.py submit TAKE SEED   queues it on ComfyUI 8191 and prints the prompt id
import json, sys, urllib.request

STYLE = {
    "medium": "detailed 16-bit pixel art with crisp hard-edged pixels, dithering and a limited palette, like a 1990s dark fantasy strategy game screen, chunky readable shapes and strong silhouettes",
    "aesthetics": ("1980s dark fantasy film mood with a deadpan paranormal feel. Two worlds meet on this screen: "
                   "every frame, card and panel belongs to an advanced alien cult, sleek matte black slabs with thin precise glowing edges and sharp bevelled corners, "
                   "minimal, disciplined and very expensive looking; the fighters belong to a dusty desert tribe, hooded and bandaged, in sepia and umber cloth with small ember orange glows."),
    "lighting": "near darkness; the strongest light is the green glow of the two linked pieces and the combo they make; everything that is not part of the combo stays dim",
    "text_rule": ("The only text on the screen is exactly the text elements given, spelled exactly as written, in a clean pixel font that is easy to read. "
                  "No other words, letters, numbers, labels, buttons or captions anywhere."),
    "color_palette": ["#0b0a10", "#1c1a24", "#3a3442", "#6b5a48", "#c88a4a", "#ff7a2a", "#3ef08a", "#b8ffd0", "#e8e2d4"],
}

# The words agreed on 27 Oct: Type IV name, a plain line under 12 words, and what it combos into.
NAME_A, PLAIN_A, KIND_A = "STRIP AND REPRINT", "Your hits peel off enemy armour and save it.", "BUTCHER AUGMENT"
NAME_B, PLAIN_B, KIND_B = "MATTER PRINTER", "Heals print new tissue, so they land bigger.", "GLUTTON TALENT"
COMBO, PLAIN_C = "FLESH FOUNDRY", "The Marksman strips armour. The Medic heals with it."

# Our fighters as the Blood Rot sprites look, and the Pals as round 3 drew them, fully robed.
MARKSMAN = "a lean tribal marksman with a dusty head wrap and long dreadlocks, bandaged shins and ragged sepia cloth, aiming a long old wooden rifle"
DEVICE = "a sleek black alien device with a glowing green core clamped onto the rifle barrel"
MEDIC = "a thin hooded tribal medic in ragged sepia robes and bandages, holding a tall staff hung with a small lantern"
WARDEN = "a big hooded tribal warden in dusty umber cloth and bandages, holding a tall battered wooden shield"
BUTCHER = "a hulking bald grey-skinned brute with bone spikes growing from his forearms, wearing a ragged dark leather harness and a heavy cloth wrap over his chest"
GLUTTON = "a huge fat grey stone-skinned ogre with a tiny bald head, fast asleep, wearing a ragged brown tunic"
COMBO_SCENE = ("a tribal marksman shoots an enemy raider in heavy dark iron plate armour; the raider's iron plates peel off as glowing green flakes; "
               "the flakes stream in a green ribbon over the fighters' heads to a thin hooded tribal medic, who turns them into green light wrapped around "
               "a big hooded tribal warden with a tall battered wooden shield at the front, healing him")

NAME_FONT = "bold chunky pixel font, bone white with a dark outline"
PLAIN_FONT = "small clean pixel font, bone white, plain and easy to read"
KIND_FONT = "small pixel font in pale green capitals, letter spaced"
COMBO_FONT = "big bold chunky pixel font, bright green with a dark outline and a soft green glow"


def text(bbox, words, desc):
    return {"type": "text", "bbox": bbox, "text": words, "desc": desc}


def obj(bbox, desc):
    return {"type": "obj", "bbox": bbox, "desc": desc}


TAKES = {
    # 1. A sum, read left to right: card + card = combo card, and the battle strip under them shows it happen.
    "sum": {
        "high_level_description": ("A pixel art build screen for a dark fantasy roguelike auto battler that teaches one combo at a glance. It reads left to right like a sum: "
                                   "a card, a big plus sign, a second card, a big equals sign, then a bigger glowing combo card. "
                                   "Under them a wide battle strip shows the combo happening on the fighters."),
        "background": "Almost black void with faint dust. Plenty of empty dark space between the cards so each one reads clearly.",
        "elements": [
            obj([50, 25, 600, 285], "A tall sleek matte black card with thin glowing green edges. Its top half is a pixel art picture: a long old wooden tribal rifle with "
                "a sleek black alien device with a glowing green core clamped onto its barrel; a bullet strikes an iron breastplate and the plate peels off in curls of glowing green flakes. "
                "A tiny portrait of " + BUTCHER + " sits in the card's top left corner."),
            text([345, 40, 385, 270], KIND_A, KIND_FONT),
            text([390, 40, 450, 270], NAME_A, NAME_FONT),
            text([460, 45, 570, 265], PLAIN_A, PLAIN_FONT),
            text([250, 298, 370, 352], "+", COMBO_FONT),
            obj([50, 365, 600, 625], "A tall sleek matte black card with thin glowing green edges. Its top half is a pixel art picture: a tribal medic's bandaged hand glowing green, "
                "printing fresh pink flesh and sinew onto a wounded arm in thin lines of green light. A tiny portrait of " + GLUTTON + " sits in the card's top left corner."),
            text([345, 380, 385, 610], KIND_B, KIND_FONT),
            text([390, 380, 450, 610], NAME_B, NAME_FONT),
            text([460, 385, 570, 605], PLAIN_B, PLAIN_FONT),
            text([250, 638, 370, 692], "=", COMBO_FONT),
            obj([20, 705, 640, 975], "The hero of the screen: a bigger sleek matte black card with a thick glowing green frame and a soft green aura. "
                "Its top half is a pixel art picture of green armour flakes swirling into a glowing green heart of new flesh."),
            text([360, 720, 450, 960], COMBO, COMBO_FONT),
            text([470, 725, 600, 955], PLAIN_C, PLAIN_FONT),
            obj([670, 15, 985, 985], "A wide strip of night battlefield seen from slightly above and to the side, with small pixel art fighters like a strategy game: "
                + COMBO_SCENE + ". Everything except the green combo is dim."),
        ],
    },
    # 2. The fighters are the screen: the two pieces glow on two of them, a thread of light joins them up to the combo's name.
    "camp": {
        "high_level_description": ("A pixel art build screen for a dark fantasy roguelike auto battler: the player's squad of five tribal fighters stands in a battle line at a night camp, "
                                   "and the screen shows one combo between two of them. Two glowing green pieces on two fighters are joined by a bright green thread of light "
                                   "that arcs up to the combo's name."),
        "background": "A dusty desert camp at night, almost black, a low campfire glow on the sand, ragged tents far behind in the dark.",
        "elements": [
            obj([30, 5, 470, 210], "Far back in the dark, " + BUTCHER + ", standing and barely lit, watching the marksman."),
            obj([90, 830, 470, 995], "Far back in the dark, " + GLUTTON + ", sitting slumped and barely lit, near the medic."),
            obj([30, 330, 230, 670], "A sleek matte black plate with a thick glowing green frame, hanging at the top of the green thread's arc."),
            text([45, 345, 125, 655], COMBO, COMBO_FONT),
            text([130, 350, 215, 650], PLAIN_C, PLAIN_FONT),
            obj([200, 300, 470, 680], "A thick bright glowing green thread of light arcs up from the marksman's rifle device to the plate and down to the medic's glowing hand, "
                "green flakes drifting along it."),
            text([255, 40, 305, 300], NAME_A, NAME_FONT + ", on a small sleek black tag with thin green edges"),
            text([310, 45, 385, 295], PLAIN_A, PLAIN_FONT),
            text([255, 700, 305, 960], NAME_B, NAME_FONT + ", on a small sleek black tag with thin green edges"),
            text([310, 705, 385, 955], PLAIN_B, PLAIN_FONT),
            obj([400, 190, 960, 400], "Big and clear, " + MARKSMAN + ", with " + DEVICE + ". The device glows bright green."),
            obj([420, 590, 960, 780], "Big and clear, " + MEDIC + ". His free hand glows bright green."),
            obj([470, 20, 960, 990], "Three dim tribal fighters stand in the same line: on the far left " + WARDEN + "; in the middle, between the marksman and the medic, "
                "a crouched hooded skirmisher with two curved blades; on the far right a tall hooded trapper with a coiled chain. They are dim and quiet."),
        ],
    },
    # 3. The grid you accepted from BlazBlue, drawn our way: a row per trigger, tiers I to III, a combo socket, and one big panel that does the reading.
    "grid": {
        "high_level_description": ("A pixel art build board for a dark fantasy roguelike auto battler. On the left, a grid: one row for each trigger, three sockets per row for tiers I, II and III, "
                                   "then a last combo socket. Two lit green sockets are joined by a glowing green line to a lit combo socket. "
                                   "On the right, a tall panel explains that combo with a picture."),
        "background": ("The board is cut into a wall of black alien glass with a faint fine grid of thin dark lines. Dusty tribal charms, beads and small bone fetishes hang off "
                       "its top edge on cords. Near darkness."),
        "elements": [
            text([140, 30, 220, 150], "HIT", NAME_FONT),
            text([290, 30, 370, 150], "BLOCK", NAME_FONT),
            text([440, 30, 520, 150], "PARRY", NAME_FONT),
            text([590, 30, 670, 150], "HEAL", NAME_FONT),
            text([740, 30, 820, 150], "ORDER", NAME_FONT),
            obj([60, 160, 900, 620], ("Five rows of sockets, one row level with each trigger word. Each row has three octagonal sockets of sleek matte black with thin silver rims, "
                                      "small roman numerals I, II and III above the columns, then a fourth socket set apart at the end of the row. "
                                      "Most sockets are empty and dark, a few hold dim grey icons. Two sockets glow bright green: the first socket of the HIT row holds a tiny rifle "
                                      "with a glowing green device, and the first socket of the HEAL row holds a tiny glowing green hand. A thick glowing green line joins those two "
                                      "sockets and runs to the end socket of the HIT row, which glows green with a tiny green heart of new flesh. "
                                      "The end sockets of the other rows show a dim question mark.")),
            obj([40, 650, 960, 985], "A tall sleek matte black panel with a thin glowing green frame. Its top half is a pixel art picture: " + COMBO_SCENE + "."),
            text([520, 670, 600, 965], COMBO, COMBO_FONT),
            text([610, 670, 680, 965], NAME_A + " + " + NAME_B, NAME_FONT),
            text([690, 675, 800, 960], PLAIN_C, PLAIN_FONT),
        ],
    },
}
NEG = ("nude, nudity, topless, bare chest, breasts, cleavage, revealing clothes, "
       "photo, photograph, 3d render, smooth vector art, flat corporate infographic, modern web interface, blurry, soft airbrush, "
       "garbled letters, misspelled words, gibberish text, extra text, random letters, watermark, signature, logo, baby, cute, chibi, anime")


def graph(key, seed, prefix):
    t = TAKES[key]
    cap = {"aspect_ratio": "16:9", "high_level_description": t["high_level_description"],
           "style_description": STYLE,
           "compositional_deconstruction": {"background": t["background"], "elements": t["elements"]}}
    w, h = 1344, 768
    return {
        "1": {"class_type": "UNETLoader", "inputs": {"unet_name": "ideogram4_int8_convrot.safetensors", "weight_dtype": "default"}},
        "2": {"class_type": "CLIPLoader", "inputs": {"clip_name": "qwen3vl_8b_fp8_scaled.safetensors", "type": "ideogram4", "device": "default"}},
        "3": {"class_type": "VAELoader", "inputs": {"vae_name": "flux2-vae.safetensors"}},
        "4": {"class_type": "CLIPTextEncode", "inputs": {"clip": ["2", 0], "text": json.dumps(cap)}},
        "5": {"class_type": "CLIPTextEncode", "inputs": {"clip": ["2", 0], "text": NEG}},
        "6": {"class_type": "CFGOverride", "inputs": {"model": ["1", 0], "cfg": 3.0, "start_percent": 0.7, "end_percent": 1.0}},
        "7": {"class_type": "DualModelGuider", "inputs": {"model": ["6", 0], "positive": ["4", 0], "cfg": 7.0, "model_negative": ["6", 0], "negative": ["5", 0]}},
        "8": {"class_type": "EmptyFlux2LatentImage", "inputs": {"width": w, "height": h, "batch_size": 1}},
        "9": {"class_type": "KSamplerSelect", "inputs": {"sampler_name": "euler"}},
        "10": {"class_type": "Ideogram4Scheduler", "inputs": {"steps": 20, "width": w, "height": h, "mu": 0.0, "std": 1.75}},
        "11": {"class_type": "RandomNoise", "inputs": {"noise_seed": seed}},
        "12": {"class_type": "SamplerCustomAdvanced", "inputs": {"noise": ["11", 0], "guider": ["7", 0], "sampler": ["9", 0], "sigmas": ["10", 0], "latent_image": ["8", 0]}},
        "13": {"class_type": "VAEDecode", "inputs": {"samples": ["12", 0], "vae": ["3", 0]}},
        "14": {"class_type": "SaveImage", "inputs": {"images": ["13", 0], "filename_prefix": prefix}},
    }


if __name__ == "__main__":
    if sys.argv[1] == "submit":
        key, seed = sys.argv[2], int(sys.argv[3])
        body = json.dumps({"prompt": graph(key, seed, "KS-Loadout-20261027/%s-%d" % (key, seed)), "client_id": "ks-loadout-real-pass"}).encode()
        req = urllib.request.Request("http://127.0.0.1:8191/prompt", data=body, headers={"Content-Type": "application/json"})
        print(key, seed, json.load(urllib.request.urlopen(req))["prompt_id"])
    else:
        key, seed = sys.argv[1], int(sys.argv[2])
        print(json.dumps(graph(key, seed, "KS-Loadout-20261027/%s-%d" % (key, seed))))
