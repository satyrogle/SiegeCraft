"""Render a 1x battle stage (1376 x 768, the fight's stage area at 1x) from a PixelLab pro tileset and map
objects, then a copy with both squads standing where CampaignArtBattle.cs puts them.
Usage: render_stage.py <config.json> <downloads> <out>

Config: {"name", "tileset", "rows": {"-2": "llll..."} (26 cells per row, columns -2..23, l upper, r lower,
e transition), "fill": "r", "props": [[object id, x, y, flip], ...] (x and y are the top left of the prop's
visible pixels), "top": first row's feet in screen px}.
A tile is decided by the 4 x 4 cells around it, the way PixelLab picks them; 255 means any."""
import json
import os
import random
import sys
from PIL import Image

CONFIG, DL, OUT = sys.argv[1], sys.argv[2], sys.argv[3]
RES = r"C:\Users\jacob\KindredSiege-Clean\Assets\Resources"
W, H, T = 1376, 768, 64
STAGE_X, STAGE_Y = 32, 66  # the stage's top left on the 1440 x 900 screen
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
    pattern = [p["row_0"], p["row_1"], p["row_2"], p["row_3"]]
    b = t["bounding_box"]
    image = sheet.crop((b["x"], b["y"], b["x"] + b["width"], b["y"] + b["height"]))
    tiles.append((pattern, sum(1 for r in pattern for v in r if v != 255), image))

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
        tile = rnd.choice(fits)
        # A floor with one plain tile repeats every 64 px; "flip_lower" mirrors plain floor tiles at random.
        if cfg.get("flip_lower") and all(v in (0, 255) for r in around for v in r) and all(v == 0 for r in around for v in r):
            if rnd.random() < 0.5:
                tile = tile.transpose(Image.FLIP_LEFT_RIGHT)
            if cfg["flip_lower"] == "hv" and rnd.random() < 0.5:
                tile = tile.transpose(Image.FLIP_TOP_BOTTOM)
        # Tiles sit half a tile up and left; draw on a canvas padded by half a tile, then crop.
        ground.alpha_composite(tile, (tx * T, ty * T))
ground = ground.crop((T // 2, T // 2, T // 2 + W, T // 2 + H))
print("tiles with no match:", misses)


def prop(object_id):
    im = Image.open(os.path.join(DL, "obj_" + object_id + ".png")).convert("RGBA")
    # Hard edges only: no half see through pixels against the ground (pixel-scenes rule 8).
    a = im.getchannel("A").point(lambda v: 255 if v >= 128 else 0)
    im.putalpha(a)
    return im.crop(im.getbbox())


def place(img, im, x, y, flip=False):
    if flip:
        im = im.transpose(Image.FLIP_LEFT_RIGHT)
    left, top = max(0, -x), max(0, -y)
    img.alpha_composite(im.crop((left, top, im.width, im.height)), (x + left, y + top))


stage = ground.copy()
for object_id, x, y, flip in cfg["props"]:
    place(stage, prop(object_id), x, y, flip)
stage.save(os.path.join(OUT, cfg["name"] + ".png"))


def frame0(path):
    sheet_json = json.load(open(path + ".json"))
    f = sheet_json["frames"][0]["frame"]
    im = Image.open(path + ".png").convert("RGBA").crop((f["x"], f["y"], f["x"] + f["w"], f["y"] + f["h"]))
    return im, sheet_json["anchorX"], sheet_json["anchorY"]


# The formation from BuildBattle, in screen px: rows from "top" to 700, your front 600 and back 420, their
# front 840 and back 1020, your leader at 250, 640.
top = cfg.get("top", 380)
col3 = [top, (top + 700) // 2, 700]
# The enemy squad: "enemy" is a sprite prefix and its folder, e.g. "ClarthStudy/blood_rot" or "CampaignArt/skull".
foe = cfg.get("enemy", "ClarthStudy/blood_rot")
spots = [
    ("ClarthStudy/clarth_warden_se_idle", 600, top), ("ClarthStudy/clarth_skirmisher_se_idle", 600, 700),
    ("ClarthStudy/clarth_marksman_se_idle", 420, col3[0]), ("ClarthStudy/clarth_trapper_se_idle", 420, col3[1]),
    ("ClarthStudy/clarth_medic_se_idle", 420, col3[2]), ("CampaignArt/Leader/leader_se_idle", 250, 640),
    (foe + "_warden_sw_idle", 840, top), (foe + "_skirmisher_sw_idle", 840, 700),
    (foe + "_marksman_sw_idle", 1020, col3[0]), (foe + "_trapper_sw_idle", 1020, col3[1]),
    (foe + "_medic_sw_idle", 1020, col3[2]),
]
look = stage.copy()
for key, x, y in sorted(spots, key=lambda s: s[2]):
    im, ax, ay = frame0(os.path.join(RES, key.replace("/", os.sep)))
    place(look, im, int(round(x - STAGE_X - ax)), int(round(y - STAGE_Y - ay)))
look.save(os.path.join(OUT, cfg["name"] + "_squads.png"))
print("wrote", os.path.join(OUT, cfg["name"] + "_squads.png"))
