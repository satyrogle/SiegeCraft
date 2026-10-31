# Builds the Ideogram 4 API graphs for the talent tree takes (same graph that ran on 18 Oct).
import json, sys

STYLE = {
    "medium": "detailed 16-bit pixel art with crisp hard-edged pixels, dithering and a limited palette, like a 1990s dark fantasy game menu",
    "aesthetics": "1980s dark fantasy film mood with a deadpan paranormal feel, quiet and moody, the whole menu is one painted living object like an ink painting skill tree",
    "lighting": "near darkness, the only strong light comes from the talents that are bought",
    "text_rule": "The only text anywhere on the screen is the single title word TALENTS. No menus, lists, buttons, labels, numbers or captions.",
}
PALS = ("Three small square pixel art portrait frames stacked in a column on the right edge of the screen, each shoulders up and fully clothed, no names or labels: "
        "a hulking bald grey-skinned brute with bone spikes growing from his forearms, wearing a ragged dark leather harness and a heavy cloth wrap over his chest; "
        "a slender bald lilac-skinned woman with glowing pale eyes wearing a high-collared black hooded robe that covers her body completely, gold bands on her sleeves; "
        "a lilac-skinned woman with wild black hair wearing a long black high-necked robe that covers her body completely, small orange crescent blades floating beside her. ")
TITLE = {"type": "text", "bbox": [30, 30, 110, 330], "text": "TALENTS",
         "desc": "bold chunky pixel font, bone white with a dark outline, top left corner"}

TAKES = {
    "rot": {
        "high_level_description": "A pixel art talent tree menu screen for a dark fantasy roguelike game: a glowing mushroom tree grows out of the back of a huge grey ogre who is fast asleep, and the whole talent tree is that living fungus.",
        "palette": ["#0d0a12", "#2a2236", "#6b6058", "#f0a030", "#ffd98a", "#9cc44a"],
        "background": "Deep violet black cave darkness with a dark soil floor and a few faint floating spores. Plenty of empty dark space around the tree so it reads clearly.",
        "elements": [
            {"type": "obj", "bbox": [560, 110, 990, 470], "desc": "A huge fat grey stone-skinned ogre sitting slumped on the floor, fast asleep, tiny bald head drooping onto his chest, wearing a ragged brown tunic, a dull purple crystal shard in his lap. A thick pale fungal stalk grows out of his back."},
            {"type": "obj", "bbox": [80, 90, 760, 720], "desc": "The talent tree: the pale fungal stalk rises from the sleeping ogre's back and branches into seven long curving limbs that spread up and sideways like a bonsai. Each limb carries four mushroom caps in a row from the trunk outward. Caps near the trunk are big and glow warm amber with tiny bright spots; further out some are small pale closed buds; the outermost tips are only faint grey spores fading into the dark, not grown yet. One cap low on the left has burst open in bright sickly green with spores spraying out."},
            {"type": "obj", "bbox": [110, 770, 870, 980], "desc": PALS + "A thin dotted glowing thread of spores in a different colour (blue, red, purple) drifts from each portrait to one mushroom cap on the tree."},
            TITLE,
        ],
    },
    "candle": {
        "high_level_description": "A pixel art talent tree menu screen for a dark fantasy roguelike game: the whole talent tree is one crooked black wrought iron candle stand covered in candles, standing in a dark cult chapel.",
        "palette": ["#0d0a12", "#2e2a35", "#555061", "#dcd2bb", "#f6c24a", "#b6e05a"],
        "background": "Dark stone chapel interior, almost black, warm pools of candle light on the floor, faint cobwebs. Lots of empty dark space around the stand.",
        "elements": [
            {"type": "obj", "bbox": [70, 250, 990, 750], "desc": "A tall black wrought iron candle stand on three splayed feet with one slightly crooked stem and many uneven arms sprouting at different heights on the left and right, each curving out and then up, no two arms alike, clearly asymmetric. Each arm holds four little iron cups from the stem outward. Near the stem the candles are lit, some with tall bright flames, some with small flames; further out are fresh unlit white candles, then empty cups; the outermost arm ends are unfinished, bare bolts with a small hanging paper tag. One candle low on the left burns with a tall eerie green flame. Wax drips down the iron. On top of the stem a small iron ring with a carved eye."},
            {"type": "obj", "bbox": [690, 30, 990, 230], "desc": "A small square pixel art portrait frame of a huge fat grey stone-skinned ogre, fast asleep, tiny bald head, wearing a ragged brown tunic, holding a brass candle snuffer. He is alone in the frame."},
            {"type": "obj", "bbox": [110, 770, 870, 980], "desc": PALS + "A thin cloth prayer ribbon in a different colour (blue, red, purple) is tied to one candle cup on the stand and flutters across to each portrait."},
            TITLE,
        ],
    },
    "crack": {
        "high_level_description": "A pixel art talent tree menu screen for a dark fantasy roguelike game: the whole talent tree is one huge black monolith stone, cracked open from a strike near its top, with molten gold filling some of the cracks.",
        "palette": ["#0d0a12", "#2a2630", "#524a5c", "#e8b830", "#fff0a0", "#c8e060"],
        "background": "A dark cave void, almost black, faint dust in the air, the monolith lit softly from the left. Lots of empty dark space around it.",
        "elements": [
            {"type": "obj", "bbox": [60, 240, 980, 760], "desc": "A tall smooth black stone monolith standing on a dark floor. Near its top a small scorched impact crater where something fell from the sky and struck it. From the crater seven jagged natural rock fractures branch downward and outward across the stone face like lightning, irregular and organic, never straight lines, each splitting at four small chipped joints. Cracks near the crater are filled with bright glowing molten gold like kintsugi, some thick, some thin; further out the cracks are dark and empty with a small diamond shaped notch at each joint; the outer ends are not cracked yet, only marked with small white chalk X marks. One crack on the left overflows with green gold molten metal dripping down the stone."},
            {"type": "obj", "bbox": [690, 30, 990, 230], "desc": "A small square pixel art portrait frame of a huge fat grey stone-skinned ogre, fast asleep, tiny bald head, wearing a ragged brown tunic, holding an iron chisel. He is alone in the frame."},
            {"type": "obj", "bbox": [110, 770, 870, 980], "desc": PALS + "A thin glowing line in a different colour (blue, red, purple) runs from one crack joint on the monolith to each portrait."},
            TITLE,
        ],
    },
}
NEG = ("nude, nudity, topless, bare chest, breasts, cleavage, revealing clothes, "
       "photo, photograph, 3d render, smooth vector art, flat corporate infographic, diagram, straight lines, modern web interface, blurry, "
       "soft airbrush, menu list, buttons, labels, numbers, captions, paragraphs of text, small text, garbled letters, watermark, signature, symmetrical menorah, baby")


def graph(key, seed, prefix):
    t = TAKES[key]
    cap = {"aspect_ratio": "16:9", "high_level_description": t["high_level_description"],
           "style_description": dict(STYLE, color_palette=t["palette"]),
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
    key, seed = sys.argv[1], int(sys.argv[2])
    print(json.dumps(graph(key, seed, "KS-TalentTree-20261027/%s-%d" % (key, seed))))
