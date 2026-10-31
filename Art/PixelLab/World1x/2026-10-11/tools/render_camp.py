"""Render the camp at 1x (1376 x 768, the same frame as a fight's stage) from the Broken Wastes pro tileset,
the region's props and each Pal's station, then a look copy with the four Pals standing at their stations.
Usage: render_camp.py <config.json> <tileset downloads> <prop folder>... <out>

Config: {"name", "tileset", "rows", "fill", "props": [[object id, x, y, flip], ...] (top left of the visible
pixels), "pals": [[pal, feet x, feet y], ...] (stage px)}. Props are looked up in each prop folder in turn."""
import json
import os
import random
import sys
from PIL import Image

CONFIG, DL, OUT = sys.argv[1], sys.argv[2], sys.argv[-1]
FOLDERS = [DL] + sys.argv[3:-1]
LOOPS = r"C:\Users\jacob\KindredSiege-Clean\Assets\Resources\CampaignArt\CampLoops"
W, H, T = 1376, 768, 64
cfg = json.load(open(CONFIG))
os.makedirs(OUT, exist_ok=True)
ROWS = {int(k): v for k, v in cfg["rows"].items()}
VALUE = {"r": 0, "l": 1, "e": 2}


def cell(x, y):
    row = ROWS.get(y, cfg.get("fill", "r") * 26)
    if not -2 <= x <= 23:
        return -1
    return VALUE[row[x + 2]]


sheet = Image.open(os.path.join(DL, cfg["tileset"] + ".png")).convert("RGBA")
meta = json.load(open(os.path.join(DL, cfg["tileset"] + ".json")))
tiles = []
for t in meta.get("tiles") or meta["tileset_data"]["tiles"]:
    p = t["pattern_4x4"]
    b = t["bounding_box"]
    pattern = [p["row_0"], p["row_1"], p["row_2"], p["row_3"]]
    tiles.append((pattern, sum(1 for r in pattern for v in r if v != 255),
                  sheet.crop((b["x"], b["y"], b["x"] + b["width"], b["y"] + b["height"]))))
rnd = random.Random(cfg.get("seed", 20261011))
ground = Image.new("RGBA", (W + T, H + T))
misses = 0
for ty in range(0, H // T + 2):
    for tx in range(0, W // T + 2):
        around = [[cell(tx - 2 + j, ty - 2 + i) for j in range(4)] for i in range(4)]
        best, fits = -1, []
        for pattern, specific, image in tiles:
            if all(pattern[i][j] == 255 or pattern[i][j] == around[i][j] for i in range(4) for j in range(4)):
                if specific > best:
                    best, fits = specific, [image]
                elif specific == best:
                    fits.append(image)
        if not fits:
            misses += 1
            continue
        ground.alpha_composite(rnd.choice(fits), (tx * T, ty * T))
ground = ground.crop((T // 2, T // 2, T // 2 + W, T // 2 + H))
print("tiles with no match:", misses)


def prop(object_id):
    for folder in FOLDERS:
        path = os.path.join(folder, "obj_" + object_id + ".png")
        if os.path.exists(path):
            im = Image.open(path).convert("RGBA")
            im.putalpha(im.getchannel("A").point(lambda v: 255 if v >= 128 else 0))
            return im.crop(im.getbbox())
    raise SystemExit("missing prop " + object_id)


def place(img, im, x, y, flip=False):
    if flip:
        im = im.transpose(Image.FLIP_LEFT_RIGHT)
    left, top = max(0, -x), max(0, -y)
    img.alpha_composite(im.crop((left, top, min(im.width, W - x), min(im.height, H - y))), (x + left, y + top))


camp = ground.copy()
for object_id, x, y, flip in cfg["props"]:
    place(camp, prop(object_id), x, y, flip)
camp.save(os.path.join(OUT, cfg["name"] + ".png"))

look = camp.copy()
for pal, fx, fy in sorted(cfg["pals"], key=lambda p: p[2]):
    loop = json.load(open(os.path.join(LOOPS, pal + "_camp.json")))
    f = loop["frames"][0]["frame"]
    im = Image.open(os.path.join(LOOPS, pal + "_camp.png")).convert("RGBA").crop((f["x"], f["y"], f["x"] + f["w"], f["y"] + f["h"]))
    place(look, im, int(fx - loop["anchorX"]), int(fy - loop["anchorY"]))
look.save(os.path.join(OUT, cfg["name"] + "_pals.png"))
print("wrote", os.path.join(OUT, cfg["name"] + "_pals.png"))
