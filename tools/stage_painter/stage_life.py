import json
import math
import subprocess
import sys
from collections import deque
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
import stage_region as SR

SP, RG = SR.SP, SR.RG

# stage_life.py [STAGE ...] | repaint NAME ...
# Jacob, 1 Nov 2026, on the five Bloodridge stills (stage_region.py region): "Move on to the moving layer now (flyers,
# other tribes, catapult shots, sun boats) and fix the stills after." Earlier the same night he asked for stages "like
# camp scene ... with layers cool things happening in the back like camp animated stuff and much more and fucking draw
# new species of flying things and add more variety unseen tribes or other designed tribes can also be in the
# background", and for the hybrid of Claude's three region plans. What moves in each stage is that plan's "moves".
# Built the way camp 5's war loop was (town_anim.py, town_fix.py): the tribes' own fighter sheets shrunk to the
# distance, the camp's PixelLab bursts, flames and smoke, the camp's sun boats; nothing paid. New here: six flying
# species and three foreign tribes, painted on Jacob's GPU as pose sheets (Ideogram 4, like town_paint.py's ships)
# and cut by the flat grey around them. Everything that moves stays above row 285, the game's atmosphere clip
# (CampaignArtRegionalStage.cs). Positions were read off the five stills Jacob pasted on 1 Nov (half size), so they are
# good to a few pixels. A loop is 12 s at 12 fps and every schedule repeats inside it, so it loops without a seam.
W, H = SP.W, SP.H
FPS, SEC = 12, 12
N = FPS * SEC
TICK = 1000.0 / FPS
TOP = 285
OUT = SR.OUT
LIFE = OUT / "life"
SHEETS = HERE.parents[1] / "Assets/Resources/CampaignArt"  # the fighters' sheets, in this checkout
CAMP = RG.REPO / "Art/Studies/Camp-War-20261028"  # camp 5's effects and sun boats, in Jacob's main folder
FF = getattr(SR.BL, "FF", "ffmpeg")
GREY = (200, 200, 204)
SLATE = (44, 48, 58)  # pale creatures (bones, the whale, gold moths) go on dark slate, so the cut can find them
BG = {"moth": SLATE, "kite": SLATE, "whale": SLATE}

UP = "seen from the side flying to the left, its wings raised high above its back"
MID = "seen from the side flying to the left, its wings spread out level"
DOWN = "seen from the side flying to the left, its wings pushed down below its body"
CREATURES = {
    "vulture": dict(seed=2026110101, poses=[UP, MID, DOWN],
                    desc="an ash vulture: a big carrion bird with ragged ash grey and black feathers, a bald pale "
                         "head, a long neck wrapped in burial bandages and two small ember eyes"),
    "scarab": dict(seed=2026110102, poses=["seen from the side flying to the left, its thin wings raised",
                                           "seen from the side flying to the left, its thin wings lowered"],
                   desc="a flying scarab: a big beetle with a corroded bronze shell split open, thin amber wings "
                        "and its legs tucked under it"),
    "moth": dict(seed=2026110103, poses=[UP, MID, DOWN],
                 desc="a sun moth: a big moth with broad pale gold wings, each marked with a dark red sun disc, and "
                      "a furry ash grey body"),
    "bat": dict(seed=2026110104, poses=[UP, MID, DOWN],
                desc="an ember bat: a small black bat with ragged leathery wings and two glowing ember eyes"),
    "kite": dict(seed=2026110105, poses=[UP, MID, DOWN],
                 desc="a bone kite: a long thin flying creature made of pale bones bound with strips of sepia rag, "
                      "long rag strips trailing from its wings and tail"),
    "whale": dict(seed=2026110106, poses=["seen from the side swimming to the left through the air"],
                  desc="a sky whale: a huge pale grey whale floating in the air, long trailing fins, a long tail and "
                       "patches of corroded bronze barnacles on its back"),
}
# Three tribes nobody has met yet, seen small and far off. Placeholder looks; their real design is Jacob's call.
SAILOR = ("a storm sailor of the Thundercoast, a foreign tribe: a long oiled dark blue sea coat, a brass helmet with a "
          "thin lightning rod on top")
REED = "a marsh walker of the Echo Reed, a foreign tribe: a woven reed mask and a cloak of long green reeds"
PRIEST = ("a blood priest of Stoneveil, a foreign tribe: a dark violet hooded robe with shards of glowing red crystal "
          "on the shoulders")
STAND = "one person standing, seen from the side facing left"
KNEEL = "one person kneeling in prayer with the head bowed, seen from the side facing left"
FOLK = dict(seed=2026110107, desc="six people of three foreign tribes",
            names=["sailor_stand", "sailor_kneel", "reed_stand", "reed_kneel", "priest_stand", "priest_kneel"],
            poses=[SAILOR + ", " + STAND, SAILOR + ", " + KNEEL, REED + ", " + STAND, REED + ", " + KNEEL,
                   PRIEST + ", " + STAND, PRIEST + ", " + KNEEL])
FLAP = {1: [0], 2: [0, 1], 3: [0, 1, 2, 1]}

# What moves in each stage. Flyers fly an orbit (cx, cy, rx, ry) or a closed path; boats cross, orbit or follow
# keys (t, x, y); fighters stand at (x, feet) and fight, or walk keys over a span of frames starting at offset; folk
# are the foreign tribes, kneeling ones bow; shots fly from a catapult to a fort and burst; glow boxes flicker their
# ember pixels; eyes blink.
STAGE_FX = {
    "1": dict(
        tint=(1.0, 0.92, 0.92),
        flyers=[dict(kind="vulture", orbit=(700, 92, 150, 22), laps=1, w=30, phase=0.0),
                dict(kind="vulture", orbit=(700, 92, 150, 22), laps=1, w=26, phase=2.8),
                dict(kind="vulture", orbit=(300, 66, 90, 14), laps=-1, w=22, phase=1.0)],
        boats=[dict(sprite="barque_mid", cross=(78, -1)),
               dict(sprite="barque_far", orbit=(1050, 116, 200, 8), laps=1, phase=1.0)],
        folk=[dict(name="sailor_stand", h=18, walk=[(0, -20, 244), (N, 420, 244)], span=N, offset=o)
              for o in (0, 48, 96)],
        fighters=[dict(base="clarth_skirmisher_sw", at=(1238, 171), h=15, face=1),
                  dict(base="clarth_trapper_sw", at=(1256, 170), h=15, face=1),
                  dict(base="blood_rot_warden_se", at=(1278, 169), h=16, face=-1),
                  dict(base="blood_rot_skirmisher_se", at=(1296, 170), h=15, face=-1),
                  dict(base="blood_rot_trapper_se", at=(1314, 172), h=15, face=-1)],
        fires=[(450, 284, 0.55), (700, 266, 0.55), (918, 284, 0.55)],
        glow=[(540, 60, 625, 290), (775, 45, 860, 290), (400, 90, 445, 290), (930, 170, 985, 290),
              (1115, 190, 1165, 290)]),
    "2": dict(
        tint=(1.0, 1.0, 1.0),
        shots=[dict(at=0, src=(522, 72), dst=(500, 150), peak=10, dur=30),  # on the forts, clear of the barrels
               dict(at=36, src=(1042, 104), dst=(1062, 150), peak=24, dur=30),
               dict(at=72, src=(522, 72), dst=(496, 168), peak=6, dur=30),
               dict(at=108, src=(1042, 104), dst=(1056, 164), peak=20, dur=30)],
        flashes=[(485, 140, 610, 200), (905, 135, 1075, 195)],
        burning=[(88, 214), (150, 228), (335, 212)],
        flyers=[dict(kind="scarab", path=[(380, 60), (520, 30), (680, 80), (840, 40), (1100, 70), (880, 110),
                                          (640, 60), (450, 105)], laps=1, w=16, phase=p) for p in (0.0, 0.33, 0.66)],
        flies=[(505, 236, 16, 10), (980, 240, 14, 8)],
        fighters=[dict(base="clarth_warden_sw", h=26, walk=[(0, 1400, 268), (60, 1236, 268)], span=60, offset=o,
                       face=-1) for o in (0, 18)] +
                 [dict(base="clarth_medic_sw", h=24, walk=[(0, 1400, 268), (60, 1236, 268)], span=60, offset=o,
                       face=-1) for o in (72, 90)] +
                 [dict(base="blood_rot_warden_se", h=28, walk=[(0, 1400, 268), (60, 1236, 268)], span=60, offset=o,
                       face=-1) for o in (36, 108)],
        glow=[(260, 80, 310, 280), (440, 80, 480, 280), (600, 80, 640, 280), (770, 70, 805, 280),
              (815, 90, 850, 280), (1095, 95, 1130, 280), (395, 250, 440, 285), (865, 245, 905, 285)]),
    "3": dict(
        tint=(1.05, 0.95, 0.85),
        boats=[dict(sprite="barque_far", orbit=(850, 110, 260, 8), laps=-1, phase=0.5),
               dict(sprite="barque_mid", keys=[(0, W + 80, 100), (40, 1255, 176), (90, 1255, 176), (130, 600, 30),
                                               (143, 420, -60), (N, W + 80, 100)])],
        folk=[dict(name=n, h=16, at=at, bow=b) for n, at, b in
              (("priest_kneel", (100, 246), 0), ("reed_kneel", (128, 248), 9), ("sailor_kneel", (156, 246), 18),
               ("priest_stand", (200, 247), None), ("reed_kneel", (262, 250), 27), ("sailor_kneel", (292, 248), 36),
               ("sailor_stand", (334, 248), None))],
        fighters=[dict(base="clarth_warden_sw", h=34, walk=[(0, 590, 284), (24, 688, 284), (48, 690, 248)], span=48,
                       offset=o, face=1) for o in (0, 72)] +
                 [dict(base="clarth_skirmisher_sw", h=32, walk=[(0, 590, 284), (24, 688, 284), (48, 690, 248)],
                       span=48, offset=o, face=1) for o in (16, 88)] +
                 [dict(base="blood_rot_warden_se", h=36, walk=[(0, 590, 284), (24, 688, 284), (48, 690, 248)],
                       span=48, offset=o, face=1) for o in (32, 104)],
        fires=[(312, 268, 0.55), (563, 226, 0.5), (690, 220, 0.6), (820, 224, 0.5), (1062, 268, 0.55)],
        flyers=[dict(kind="moth", orbit=(312, 254, 16, 8), laps=3, w=10, phase=0.0, jitter=3),
                dict(kind="moth", orbit=(1062, 254, 18, 9), laps=-3, w=10, phase=1.0, jitter=3),
                dict(kind="moth", orbit=(690, 204, 22, 10), laps=2, w=11, phase=2.0, jitter=3),
                dict(kind="moth", orbit=(563, 212, 14, 7), laps=-4, w=9, phase=3.0, jitter=2)],
        glow=[(600, 60, 790, 235), (470, 60, 510, 290), (560, 60, 600, 230), (780, 60, 820, 230),
              (870, 60, 910, 290), (920, 100, 970, 290), (1015, 140, 1055, 290)]),
    "4": dict(
        tint=(0.62, 0.68, 0.9),
        eyes=[(525, 150, 575, 205), (815, 150, 870, 205)],
        fighters=[dict(base="blood_rot_warden_se", at=(1040, 238), h=14, face=1),
                  dict(base="blood_rot_skirmisher_se", at=(1056, 239), h=13, face=1),
                  dict(base="skull_warden_sw", at=(1076, 238), h=14, face=-1),
                  dict(base="skull_marksman_sw", at=(1094, 239), h=13, face=-1)],
        boats=[dict(sprite="barque_mid", cross=(104, 1), lanterns=True)],
        lanterns=(0, 130, W, 285),
        glow=[(0, 215, W, 262)],
        flyers=[dict(kind="bat", path=[(420, 40), (520, 15), (640, 60), (560, 95), (460, 80)], laps=2, w=11,
                     phase=p, jitter=2) for p in (0.0, 0.25, 0.5, 0.75)] +
               [dict(kind="bat", path=[(900, 40), (1050, 70), (1200, 30), (1100, 90)], laps=-2, w=10, phase=p,
                     jitter=2) for p in (0.1, 0.6)] +
               [dict(kind="kite", orbit=(1000, 70, 200, 18), laps=1, w=26, phase=0.0),
                dict(kind="kite", orbit=(250, 60, 150, 14), laps=-1, w=22, phase=2.0)],
        folk=[dict(name="reed_stand", h=22, at=(628, 262)), dict(name="priest_stand", h=22, at=(752, 262))]),
    "5": dict(
        tint=(1.0, 0.62, 0.55),
        ring=(295, 10, 400, 112),
        embers=70,
        fighters=[dict(base=b, at=(x, 279), h=22, face=1) for b, x in
                  (("blood_rot_warden_se", 470), ("blood_rot_skirmisher_se", 492), ("blood_rot_trapper_se", 514),
                   ("blood_rot_medic_se", 536))] +
                 [dict(base=b, at=(x, 279), h=22, face=-1) for b, x in
                  (("blood_rot_marksman_se", 846), ("blood_rot_warden_se", 868), ("blood_rot_skirmisher_se", 890),
                   ("blood_rot_trapper_se", 912))] +
                 [dict(base=b, at=(x, 133), h=16, face=-1) for b, x in
                  (("hunter_warden_sw", 1070), ("hunter_captain_sw", 1094), ("hunter_skirmisher_sw", 1120))],
        flyers=[dict(kind="whale", drift=(1120, 62, 30, 3), w=170)],
        fires=[(335, 248, 0.55), (583, 250, 0.55), (808, 252, 0.55), (1055, 248, 0.55)],
        glow=[(670, 170, 730, 255), (430, 160, 470, 285), (570, 120, 610, 285), (790, 120, 830, 285),
              (930, 160, 970, 285)]),
}


# ------------------------------------------------------------------ painting and cutting the new sprites

def neg_sheet(people):
    base = json.loads(RG.L3.BASE.read_text())["5"]["inputs"]["text"].replace(" creatures,", "").replace(" letters,", "")
    if people:
        base = base.replace(" people,", "").replace(" guards,", "").replace(" human figures,", "")
    return base + (", sky, clouds, ground, grass, scenery, landscape, buildings, shadow on the ground, text, letters, "
                   "numbers, labels, frames, borders, grid, watermark, signature, blurry, photo, 3d render, different "
                   "creatures, a crowd")


def slots(n):
    """The sheet's columns: one pose each, with grey around it."""
    return [(int(i * W / n) + 6, 40, int((i + 1) * W / n) - 6, H - 40) for i in range(n)]


def paint_sheet(name, desc, poses, seed, people=False):
    LIFE.mkdir(parents=True, exist_ok=True)
    SP.OUT = LIFE
    bg = BG.get(name, GREY)
    bgname = "dark slate grey" if bg == SLATE else "pale grey"
    boxes = [(x0 + 18, 150, x1 - 18, 620) for x0, _, x1, _ in slots(len(poses))]
    init = Image.new("RGB", (W, H), bg)
    d = ImageDraw.Draw(init)
    for x0, y0, x1, y1 in boxes:
        d.ellipse((x0 + 30, y0 + 70, x1 - 30, y1 - 70), fill=(150, 146, 150) if bg == GREY else (170, 160, 140))
    same = "the same person every time" if people else "the same creature every time, the same size"
    pos = json.dumps({
        "high_level_description": "A pixel art sprite sheet for a game: " + desc + ", drawn %d times side by side, %s, "
                                  "each one whole and separate with empty space around it, on a plain flat %s "
                                  "background. " % (len(poses), same, bgname) + SP.STYLE,
        "style_description": {"medium": "pixel_art", "art_style": RG.L3.ART,
                              "aesthetics": "clean readable silhouettes, coherent pixel clusters, one design in every "
                                            "pose",
                              "lighting": "Clear daylight from the upper left, flat pixel colour planes.",
                              "color_palette": ["#3e302c", "#7a6250", "#96703e", "#f07828", "#e2d6ba", "#a89880",
                                                "#5a443a", "#c8c8cc"]},
        "compositional_deconstruction": {
            "background": "A plain flat %s background and nothing else: no sky, no clouds, no ground, no "
                          "shadows." % bgname,
            "elements": [{"type": "obj", "bbox": SP.bb(b), "desc": (p if people else desc + ", " + p)}
                         for b, p in zip(boxes, poses)]}})
    raw = SP.run("life-" + name, init, pos, seed, neg=neg_sheet(people))
    raw.save(LIFE / ("sheet-%s-raw.png" % name))
    SP.OUT = OUT


def components(m):
    """8 connected groups of a mask (no scipy needed)."""
    lab = np.zeros(m.shape, np.int32)
    h, w = m.shape
    n = 0
    for y, x in zip(*np.nonzero(m)):
        if lab[y, x]:
            continue
        n += 1
        lab[y, x] = n
        q = deque([(y, x)])
        while q:
            cy, cx = q.popleft()
            for ny in (cy - 1, cy, cy + 1):
                for nx in (cx - 1, cx, cx + 1):
                    if 0 <= ny < h and 0 <= nx < w and m[ny, nx] and not lab[ny, nx]:
                        lab[ny, nx] = n
                        q.append((ny, nx))
    return lab, n


def cut_sheet(name, n):
    """Each pose cut off the flat grey: pixels that differ from the column's border colour, the biggest group only
    (a stray shadow or speck is dropped), cropped tight. Saved as name-i.png, full size."""
    a = np.asarray(Image.open(LIFE / ("sheet-%s-raw.png" % name)).convert("RGB")).astype(np.float64)
    out = []
    for i, (x0, y0, x1, y1) in enumerate(slots(n)):
        reg = a[y0:y1, x0:x1]
        edge = np.concatenate([reg[:8].reshape(-1, 3), reg[-8:].reshape(-1, 3), reg[:, :8].reshape(-1, 3),
                               reg[:, -8:].reshape(-1, 3)])
        m = np.abs(reg - np.median(edge, 0)).sum(2) > 54
        m = np.asarray(Image.fromarray(m.astype(np.uint8) * 255).filter(ImageFilter.MedianFilter(3))) > 127
        small = np.asarray(Image.fromarray(m.astype(np.uint8) * 255).resize((m.shape[1] // 3, m.shape[0] // 3),
                                                                             Image.BOX)) > 40
        lab, k = components(small)
        if k == 0:
            out.append(None)
            continue
        big = lab == 1 + np.bincount(lab.ravel())[1:].argmax()
        big = np.asarray(Image.fromarray(big.astype(np.uint8) * 255).resize((m.shape[1], m.shape[0]), Image.NEAREST)
                         .filter(ImageFilter.MaxFilter(7))) > 127
        keep = m & big
        ys, xs = np.nonzero(keep)
        crop = (slice(ys.min(), ys.max() + 1), slice(xs.min(), xs.max() + 1))
        rgba = np.dstack([reg[crop], keep[crop] * 255.0]).astype(np.uint8)
        Image.fromarray(rgba, "RGBA").save(LIFE / ("%s-%d.png" % (name, i)))
        out.append(rgba)
    return out


def creatures(repaint=()):
    """Paints any sheet not painted yet (or named in repaint), then cuts every sheet."""
    jobs = [(k, v["desc"], v["poses"], v["seed"], False) for k, v in CREATURES.items()]
    jobs.append(("folk", FOLK["desc"], FOLK["poses"], FOLK["seed"], True))
    for name, desc, poses, seed, people in jobs:
        if name in repaint or not (LIFE / ("sheet-%s-raw.png" % name)).exists():
            paint_sheet(name, desc, poses, seed, people)
        cut_sheet(name, len(poses))
    look_creatures()


def look_creatures():
    """Every cut pose at the size it flies in the loops and at 4x, one row a species: look-creatures.png."""
    rows = [(k, CREATURES[k]["poses"], None) for k in CREATURES] + [("folk", FOLK["poses"], FOLK["names"])]
    tiles = []
    for name, poses, names in rows:
        row = []
        for i in range(len(poses)):
            p = LIFE / ("%s-%d.png" % (name, i))
            if p.exists():
                rgb, al = shrink(np.asarray(Image.open(p).convert("RGBA")), width=USE_W.get(name, 22))
                im = Image.fromarray(np.dstack([rgb, al * 255]).clip(0, 255).astype(np.uint8), "RGBA")
                row.append(im.resize((im.width * 4, im.height * 4), Image.NEAREST))
        tiles.append((name, row))
    hgt = sum(max([t.height for t in r] + [20]) + 30 for _, r in tiles)
    s = Image.new("RGB", (1376, hgt), (40, 36, 44))
    d = ImageDraw.Draw(s)
    y = 0
    for name, row in tiles:
        d.text((8, y + 6), name, fill=(239, 223, 190), font=font(18))
        x = 8
        for t in row:
            s.paste(t, (x, y + 30), t)
            x += t.width + 24
        y += max([t.height for t in row] + [20]) + 30
    s.save(OUT / "look-creatures.png")


USE_W = {"vulture": 30, "scarab": 16, "moth": 11, "bat": 11, "kite": 26, "whale": 170, "folk": 12}


def font(size):
    try:
        return ImageFont.truetype("arial.ttf", size)
    except OSError:
        try:
            return ImageFont.load_default(size)
        except TypeError:
            return ImageFont.load_default()


# ------------------------------------------------------------------ shrinking sprites to the distance

def shrink(rgba, width=None, scale=None, ncol=12, outline=True):
    """Box shrink with the colours cut to a few, ember and bone accents kept, and a dark one pixel outline, the way
    town_anim.shrink_fixed shrinks the camp's fighters."""
    a = rgba.astype(np.float64)
    al = a[..., 3] > 127
    s = scale if scale is not None else width / a.shape[1]
    w, h = max(1, round(a.shape[1] * s)), max(1, round(a.shape[0] * s))

    def down(x):
        return np.asarray(Image.fromarray(x.astype(np.float32)).resize((w, h), Image.BOX)).astype(np.float64)

    den = down(al)
    src = a[..., :3]
    col = np.dstack([down(src[..., c] * al) for c in range(3)]) / np.maximum(den[..., None], 1e-6)
    m = den >= 0.45
    if al.sum() > 4:
        px = np.ascontiguousarray(src[al]).astype(np.uint8)
        q = Image.fromarray(px[None]).quantize(colors=ncol, method=Image.Quantize.MEDIANCUT)
        qp = np.array(q.getpalette()[:3 * ncol], np.float64).reshape(-1, 3)
        col = qp[((col[:, :, None, :] - qp[None, None]) ** 2).sum(3).argmin(2)]
    else:
        qp = np.array([[30.0, 24, 28]])
    r, g, b = src[..., 0], src[..., 1], src[..., 2]
    lu = r * 0.299 + g * 0.587 + b * 0.114
    for acc, need in ((al & (lu > 190) & (np.abs(r - b) < 40), 0.3),
                      (al & (r > 180) & (g > 50) & (g < 175) & (b < 100) & (lu > 105), 0.07)):
        frac = down(acc)
        mean = np.dstack([down(src[..., c] * acc) for c in range(3)]) / np.maximum(frac[..., None], 1e-6)
        use = (frac >= need) & m
        col[use] = mean[use]
    if not outline:
        return col, m
    out = np.zeros((h + 2, w + 2, 3))
    out[1:-1, 1:-1] = col
    mm = np.zeros((h + 2, w + 2), bool)
    mm[1:-1, 1:-1] = m
    ring = (np.asarray(Image.fromarray((mm * 255).astype(np.uint8)).filter(ImageFilter.MaxFilter(3))) > 0) & ~mm
    out[ring] = qp[qp.sum(1).argmin()] * 0.7
    return out, mm | ring


CACHE = {}


def species(kind, w):
    """A species' poses shrunk to one width, all on one canvas size so it does not jump between wing beats."""
    key = ("sp", kind, w)
    if key not in CACHE:
        n = len(CREATURES[kind]["poses"])
        raw = [LIFE / ("%s-%d.png" % (kind, i)) for i in range(n)]
        raw = [np.asarray(Image.open(p).convert("RGBA")) for p in raw if p.exists()]
        if not raw:
            CACHE[key] = None
            return None
        s = w / max(r.shape[1] for r in raw)
        fr = [shrink(r, scale=s) for r in raw]
        hh, ww = max(f[1].shape[0] for f in fr), max(f[1].shape[1] for f in fr)
        out = []
        for rgb, al in fr:
            c, m = np.zeros((hh, ww, 3)), np.zeros((hh, ww), bool)
            y0, x0 = (hh - al.shape[0]) // 2, (ww - al.shape[1]) // 2
            c[y0:y0 + al.shape[0], x0:x0 + al.shape[1]] = rgb
            m[y0:y0 + al.shape[0], x0:x0 + al.shape[1]] = al
            out.append((c, m))
        CACHE[key] = out
    return CACHE[key]


def folk(name, h):
    """One of the foreign tribe sprites; h is the standing height, so a kneeling one comes out lower."""
    key = ("folk", name, h)
    if key not in CACHE:
        p = LIFE / ("folk-%d.png" % FOLK["names"].index(name))
        ref = LIFE / ("folk-%d.png" % FOLK["names"].index(name.replace("_kneel", "_stand")))
        if not p.exists() or not ref.exists():
            CACHE[key] = None
            return None
        a = np.asarray(Image.open(p).convert("RGBA"))
        CACHE[key] = shrink(a, scale=h / Image.open(ref).height, ncol=8)
    return CACHE[key]


def sheet_frames(base, anim):
    meta = json.loads((SHEETS / ("%s_%s.json" % (base, anim))).read_text())["frames"]
    meta = meta if isinstance(meta, list) else list(meta.values())
    sheet = np.asarray(Image.open(SHEETS / ("%s_%s.png" % (base, anim))).convert("RGBA"))
    return [sheet[q["y"]:q["y"] + q["h"], q["x"]:q["x"] + q["w"]] for q in (f["frame"] for f in meta)], \
        np.array([f["duration"] for f in meta], float)


def fighter(base, anim, h):
    """town_anim.clip: one sheet shrunk so the idle stance is h tall, every frame cut by the union box so the feet
    stay put."""
    key = ("ft", base, anim, h)
    if key not in CACHE:
        idle, _ = sheet_frames(base, "idle")
        ys = np.nonzero(idle[0][..., 3] > 127)[0]
        s = h / (ys.max() - ys.min() + 1)
        frames, durs = sheet_frames(base, anim)
        fy, fx = np.nonzero(frames[0][..., 3] > 127)
        uy, ux = np.nonzero(np.any([f[..., 3] > 127 for f in frames], axis=0))
        y0, y1, x0, x1 = uy.min(), uy.max() + 1, ux.min(), ux.max() + 1
        CACHE[key] = dict(frames=[shrink(f[y0:y1, x0:x1], scale=s, ncol=10) for f in frames], cum=np.cumsum(durs),
                          total=float(durs.sum()), ground=round((fy.max() + 1 - y0) * s) + 1,
                          cx=round(((fx.min() + fx.max()) / 2 - x0) * s) + 1)
    return CACHE[key]


SMOKE = np.array([[62.0, 56, 60], [96, 88, 92], [132, 126, 130]])  # town_fix.SMOKE: the camp's pale puffs, darkened


def fxload(name, scale=1.0, ramp=None):
    """A camp effect, shrunk; with a ramp its colours are replaced by the ramp's three by brightness."""
    key = ("fx", name, scale, ramp is None)
    if key not in CACHE:
        meta = json.loads((CAMP / "fx" / (name + ".json")).read_text())["frames"]
        meta = meta if isinstance(meta, list) else list(meta.values())
        im = np.asarray(Image.open(CAMP / "fx" / (name + ".png")).convert("RGBA"))
        fr = [shrink(im[q["y"]:q["y"] + q["h"], q["x"]:q["x"] + q["w"]], scale=scale, ncol=10, outline=False)
              for q in (f["frame"] for f in meta)]
        if ramp is not None:
            px = np.vstack([rgb[al] for rgb, al in fr if al.any()])
            qs = np.percentile(px @ np.array([0.299, 0.587, 0.114]), [33, 66])
            fr = [(ramp[np.digitize(rgb @ np.array([0.299, 0.587, 0.114]), qs)], al) for rgb, al in fr]
        pv = meta[0].get("pivot", {"x": 0.5, "y": 0.5})
        h, w = fr[0][1].shape
        CACHE[key] = dict(frames=fr, ox=pv["x"] * w, oy=pv["y"] * h)
    return CACHE[key]


def boat(name):
    key = ("boat", name)
    if key not in CACHE:
        p = CAMP / ("layer-2-ship-%s.png" % name.replace("_", "-"))
        a = np.asarray(Image.open(p).convert("RGBA")).astype(np.float64) if p.exists() else None
        CACHE[key] = None if a is None else (a[..., :3], a[..., 3] > 127)
    return CACHE[key]


# ------------------------------------------------------------------ drawing

def blit(cv, rgb, al, x0, y0, flip=False, tint=None):
    if flip:
        rgb, al = rgb[:, ::-1], al[:, ::-1]
    h, w = al.shape
    x0, y0 = int(round(x0)), int(round(y0))
    xa, ya, xb, yb = max(0, x0), max(0, y0), min(W, x0 + w), min(TOP, y0 + h)
    if xa >= xb or ya >= yb:
        return
    sub = al[ya - y0:yb - y0, xa - x0:xb - x0]
    src = rgb[ya - y0:yb - y0, xa - x0:xb - x0]
    if tint is not None:
        src = src * np.array(tint)
    cv[ya:yb, xa:xb][sub] = src[sub]


def dot(cv, x, y, col):
    x, y = int(round(x)), int(round(y))
    if 0 <= x < W and 0 <= y < TOP:
        cv[y, x] = col


def ember_mask(base, box, yellow=False, dark=125):
    """Ember (or lantern yellow) pixels inside a box, only where the area around them is dark: cracks in stone,
    eyes in a hood, lights in a doorway. Bright sky of the same colour never flickers."""
    x0, y0, x1, y1 = box
    r, g, b = base[..., 0], base[..., 1], base[..., 2]
    lu = r * 0.299 + g * 0.587 + b * 0.114
    if yellow:
        e = (r > 215) & (g > 165) & (b < 150)
    else:
        e = (r > 190) & (g > 50) & (g < 200) & (b < 120) & (lu > 100)
    if dark:
        e &= np.asarray(Image.fromarray(lu.clip(0, 255).astype(np.uint8)).filter(ImageFilter.BoxBlur(6))) < dark
    m = np.zeros((H, W), bool)
    m[y0:min(y1, TOP), x0:x1] = e[y0:min(y1, TOP), x0:x1]
    return m


def ring_mask(base, box):
    """The burning ring round the black sun: ember pixels in a band just outside the dark disc."""
    x0, y0, x1, y1 = box
    lu = base[y0:y1, x0:x1].mean(2)
    ys, xs = np.nonzero(lu < 40)
    m = np.zeros((H, W), bool)
    if len(ys) < 20:
        return m, None
    cy, cx = ys.mean(), xs.mean()
    rad = math.sqrt(len(ys) / math.pi)
    yy, xx = np.mgrid[y0:y1, x0:x1]
    dist = np.hypot(yy - (y0 + cy), xx - (x0 + cx))
    e = ember_mask(base, box, dark=0)[y0:y1, x0:x1]
    m[y0:y1, x0:x1] = e & (dist > rad - 2) & (dist < rad + 12)
    return m, (x0 + cx, y0 + cy, rad)


def path_at(pts, u):
    """A point u (0 to 1) of the way round a closed path, and which way it is heading."""
    seg = [math.dist(pts[i], pts[(i + 1) % len(pts)]) for i in range(len(pts))]
    d = (u % 1.0) * sum(seg)
    for i, s in enumerate(seg):
        if d <= s or i == len(seg) - 1:
            a, b = pts[i], pts[(i + 1) % len(pts)]
            f = d / s if s else 0
            return a[0] + (b[0] - a[0]) * f, a[1] + (b[1] - a[1]) * f, (1 if b[0] > a[0] else -1)
        d -= s


def keys_at(keys, t):
    for (t0, x0, y0), (t1, x1, y1) in zip(keys, keys[1:]):
        if t0 <= t <= t1:
            f = (t - t0) / max(1, t1 - t0)
            return x0 + (x1 - x0) * f, y0 + (y1 - y0) * f, (1 if x1 > x0 else -1)
    return keys[-1][1], keys[-1][2], 1


def schedule(rng, base, h):
    """A fighter's loop: idle, attack and hit in turns, filling the loop exactly."""
    segs, t = [], 0
    while t < N:
        anim = str(rng.choice(["idle", "attack", "hit"], p=[0.45, 0.4, 0.15]))
        if anim == "idle":
            segs.append((t, "idle", int(rng.integers(0, 4000))))
            t += int(rng.integers(8, 30))
        else:
            segs.append((t, anim, 0))
            t += int(math.ceil(fighter(base, anim, h)["total"] / TICK))
    return segs


def frame_of(c, anim, ms):
    ms = ms % c["total"] if anim == "idle" else min(ms, c["total"] - 1)
    return min(int(np.searchsorted(c["cum"], ms, side="right")), len(c["frames"]) - 1)


def draw_fighter(cv, f, t, tint):
    side = f["base"].split("_")[-1]
    flip = f["face"] != (1 if side == "se" else -1)
    if "walk" in f:
        u = (t - f["offset"]) % N
        if u >= f["span"]:
            return
        x, y, _ = keys_at(f["walk"], u)
        c = fighter(f["base"], "idle", f["h"])
        rgb, al = c["frames"][frame_of(c, "idle", u * TICK)]
        y -= (u // 3) % 2  # walking is a slide with a 1 px bob (camp, 28 Oct)
    else:
        x, y = f["at"]
        t0, anim, phase = [s for s in f["segs"] if s[0] <= t][-1]
        c = fighter(f["base"], anim, f["h"])
        rgb, al = c["frames"][frame_of(c, anim, (t - t0) * TICK + phase)]
    cx = al.shape[1] - 1 - c["cx"] if flip else c["cx"]
    blit(cv, rgb, al, x - cx, y - c["ground"], flip, tint)


def flyer_at(f, t):
    if "orbit" in f:
        cx, cy, rx, ry = f["orbit"]
        ang = 2 * math.pi * f["laps"] * t / N + f["phase"]
        x, y = cx + rx * math.cos(ang), cy + ry * math.sin(ang)
        face = 1 if -math.sin(ang) * f["laps"] > 0 else -1
    elif "path" in f:
        x, y, face = path_at(f["path"], f["laps"] * t / N + f["phase"])
        if f["laps"] < 0:
            face = -face
    else:
        cx, cy, ax, ay = f["drift"]
        x = cx + ax * math.sin(2 * math.pi * t / N)
        y = cy + ay * math.sin(4 * math.pi * t / N)
        face = 1 if math.cos(2 * math.pi * t / N) > 0 else -1
    j = f.get("jitter", 0)
    if j:
        x += j * math.sin(2 * math.pi * 7 * t / N + f["phase"] * 3)
        y += j * math.cos(2 * math.pi * 11 * t / N + f["phase"] * 5)
    return x, y, face


def boat_at(b, t, w):
    if "cross" in b:
        y, d = b["cross"]
        span = W + w + 60
        x = (W + w / 2 + 30 - span * t / N) if d < 0 else (-w / 2 - 30 + span * t / N)
        return x, y + 2 * math.sin(2 * math.pi * 6 * t / N), d
    if "orbit" in b:
        cx, cy, rx, ry = b["orbit"]
        ang = 2 * math.pi * b["laps"] * t / N + b["phase"]
        return cx + rx * math.cos(ang), cy + ry * math.sin(ang), (1 if -math.sin(ang) * b["laps"] > 0 else -1)
    x, y, d = keys_at(b["keys"], t)
    return x, y + 2 * math.sin(2 * math.pi * 6 * t / N), d


BOULDER = np.array([[64.0, 48, 40], [112, 88, 70], [150, 124, 100]])
TURQ = np.array([[120.0, 255, 240], [60, 210, 200]])


def render(k):
    """The stage's living loop over its still: stage-k-life.mp4 (2x, three loops) and the frames for the look."""
    o = STAGE_FX[k]
    base = np.asarray(Image.open(OUT / ("stage-%s.png" % k)).convert("RGB")).astype(np.float32)
    rng = np.random.default_rng(int(SR.STAGES[k]["seed"]))
    tint = o.get("tint")
    for f in o.get("fighters", ()):
        if "at" in f:
            f["segs"] = schedule(rng, f["base"], f["h"])
    glow = np.zeros((H, W), bool)
    for box in o.get("glow", ()):
        glow |= ember_mask(base, box)
    gy, gx = np.nonzero(glow)
    gph = rng.uniform(0, 2 * math.pi, (H // 8 + 1, W // 8 + 1))[gy // 8, gx // 8]
    lan = ember_mask(base, o["lanterns"], yellow=True) if "lanterns" in o else np.zeros((H, W), bool)
    ly, lx = np.nonzero(lan)
    lph = rng.uniform(0, 2 * math.pi, (H // 6 + 1, W // 6 + 1))[ly // 6, lx // 6]
    eyes = np.zeros((H, W), bool)
    for box in o.get("eyes", ()):
        eyes |= ember_mask(base, box, dark=110)
    ring, sun = ring_mask(base, o["ring"]) if "ring" in o else (np.zeros((H, W), bool), None)
    flame = fxload("ks-camp-town-flame", 1.0)
    burst = fxload("ks-camp-war-explosion-20", 1.2)
    puff = fxload("ks-camp-smoke-puff", 0.6, SMOKE)
    fire = fxload("ks-camp-building-fire", 0.5)
    column = fxload("ks-camp-smoke-column", 0.5, SMOKE)
    emb = [dict(x=float(rng.uniform(0, W)), y0=float(rng.uniform(0, TOP)), k=int(rng.integers(1, 3)),
                sw=float(rng.uniform(2, 6)), ph=float(rng.uniform(0, 6.3)),
                c=[(255, 170, 60), (255, 220, 120), (230, 90, 40)][int(rng.integers(0, 3))])
           for _ in range(o.get("embers", 0))]
    flies = [(cx, cy, r, float(rng.uniform(0, 6.3)), int(rng.integers(3, 7))) for cx, cy, r, n in o.get("flies", ())
             for _ in range(n)]
    sparks = [(float(rng.uniform(0, 6.3)), int(rng.integers(0, N))) for _ in range(14)] if "ring" in o else []
    frames = []
    for t in range(N):
        cv = base.copy()
        if len(gy):
            cv[gy, gx] = np.clip(base[gy, gx] * (1 + 0.16 * np.sin(2 * math.pi * 3 * t / N + gph))[:, None], 0, 255)
        if len(ly):
            cv[ly, lx] = np.clip(base[ly, lx] * (1 + 0.12 * np.sin(2 * math.pi * 5 * t / N + lph))[:, None], 0, 255)
        if eyes.any():
            shut = (t % 48) in (40, 41, 42) or (t % 72) in (20, 21)
            cv[eyes] = base[eyes] * (0.3 if shut else 1.0 + 0.15 * math.sin(2 * math.pi * 4 * t / N))
        if ring.any():
            cv[ring] = np.clip(base[ring] * (1.05 + 0.2 * math.sin(2 * math.pi * 6 * t / N)), 0, 255)
            cx, cy, rad = sun
            for ang, t0 in sparks:
                u = (t - t0) % N
                if u < 16:
                    rr = rad + 6 + u * 1.5
                    dot(cv, cx + rr * math.cos(ang), cy + rr * math.sin(ang) + u * 0.4,
                        (255, 220, 120) if u < 6 else (230, 90, 40))
        for b in o.get("boats", ()):
            sp = boat(b["sprite"])
            if sp is None:
                continue
            rgb, al = sp
            x, y, d = boat_at(b, t, al.shape[1])
            blit(cv, rgb, al, x - al.shape[1] / 2, y - al.shape[0] / 2, d > 0, tint)
            if b.get("lanterns") and (t // 3) % 4:
                dot(cv, x - al.shape[1] * 0.3, y, (255, 214, 120))
                dot(cv, x + al.shape[1] * 0.3, y - 1, (255, 190, 90))
        for f in o.get("flyers", ()):
            fr = species(f["kind"], f["w"])
            if fr is None:
                continue
            x, y, face = flyer_at(f, t)
            seq = FLAP[len(fr)]
            i = seq[(t // (2 if f["w"] < 14 else 3) + int(f.get("phase", 0) * 5)) % len(seq)]
            rgb, al = fr[i]
            blit(cv, rgb, al, x - al.shape[1] / 2, y - al.shape[0] / 2, face > 0, tint)
        for s in o.get("shots", ()):
            u = (t - s["at"]) % N
            (x0, y0), (x1, y1) = s["src"], s["dst"]
            if u < s["dur"]:
                f = u / s["dur"]
                x = x0 + (x1 - x0) * f
                y = y0 + (y1 - y0) * f - (4 * f * (1 - f)) * (min(y0, y1) - s["peak"])
                r = 5 - 3 * f
                for q in range(3, 0, -1):
                    f2 = max(0, (u - q * 2) / s["dur"])
                    dot(cv, x0 + (x1 - x0) * f2, y0 + (y1 - y0) * f2 - (4 * f2 * (1 - f2)) * (min(y0, y1) - s["peak"]),
                        (150, 124, 100))
                yy, xx = np.mgrid[-4:5, -4:5]
                for dy, dx in zip(yy[(xx ** 2 + yy ** 2) <= r * r], xx[(xx ** 2 + yy ** 2) <= r * r]):
                    dot(cv, x + dx, y + dy, BOULDER[0] if dy > 0 else (BOULDER[2] if dy < -r / 2 else BOULDER[1]))
            elif u < s["dur"] + len(burst["frames"]):
                rgb, al = burst["frames"][u - s["dur"]]
                blit(cv, rgb, al, x1 - burst["ox"], y1 - burst["oy"])
            elif u < s["dur"] + len(burst["frames"]) + 3 * len(puff["frames"]):
                v = u - s["dur"] - len(burst["frames"])
                rgb, al = puff["frames"][(v // 3) % len(puff["frames"])]
                blit(cv, rgb, al, x1 - puff["ox"], y1 - puff["oy"] - v * 0.5)
        for i, box in enumerate(o.get("flashes", ())):
            x0, y0, x1, y1 = box
            for j in range(3):
                u = (t + 5 * i + 3 * j) % 9
                if u < 3:
                    rr = np.random.default_rng(((t - u) % N) * 31 + i * 7 + j).random(2)
                    x, y = x0 + rr[0] * (x1 - x0), y0 + rr[1] * (y1 - y0)
                    for dx, dy in ((0, 0), (1, 0), (-1, 0), (0, 1), (0, -1)):
                        dot(cv, x + dx, y + dy, TURQ[0] if (dx, dy) == (0, 0) or u == 0 else TURQ[1])
                    for q in range(2, 5 + 3 * u):
                        dot(cv, x - q * 0.6, y + q * 1.2, TURQ[1])
        for x, y, sc in o.get("fires", ()):
            fl = fxload("ks-camp-town-flame", sc)
            rgb, al = fl["frames"][(t // 2 + int(x)) % len(fl["frames"])]
            blit(cv, rgb, al, x - fl["ox"], y - fl["oy"])
        for x, y in o.get("burning", ()):
            rgb, al = column["frames"][(t // 2 + int(x)) % len(column["frames"])]
            blit(cv, rgb, al, x - column["ox"], y - 10 - column["oy"])
            rgb, al = fire["frames"][(t // 2 + int(x)) % len(fire["frames"])]
            blit(cv, rgb, al, x - fire["ox"], y - fire["oy"])
        for cx, cy, r, ph, sp in flies:
            dot(cv, cx + r * math.cos(2 * math.pi * sp * t / N + ph), cy + r * 0.5 * math.sin(2 * math.pi * sp * 2 * t / N + ph),
                (30, 26, 28))
        for e in emb:
            y = (e["y0"] + TOP * e["k"] * t / N) % TOP
            dot(cv, e["x"] + e["sw"] * math.sin(2 * math.pi * 2 * t / N + e["ph"]), y, e["c"])
        for f in o.get("folk", ()):
            sp = folk(f["name"], f["h"])
            if sp is None:
                continue
            rgb, al = sp
            if "walk" in f:
                u = (t - f["offset"]) % N
                if u >= f["span"]:
                    continue
                x, y, d = keys_at(f["walk"], u)
                y -= (u // 3) % 2
            else:
                (x, y), d = f["at"], -1
                if f.get("bow") is not None and (t + f["bow"]) % 45 < 8:
                    y += 1
            blit(cv, rgb, al, x - al.shape[1] / 2, y - al.shape[0], d > 0, tint)
        for f in o.get("fighters", ()):
            draw_fighter(cv, f, t, tint)
        frames.append(cv.clip(0, 255).astype(np.uint8))
    dst = OUT / ("stage-%s-life.mp4" % k)
    p = subprocess.Popen([FF, "-v", "error", "-y", "-f", "rawvideo", "-pix_fmt", "rgb24", "-s", "%dx%d" % (W, H),
                          "-r", str(FPS), "-i", "-", "-vf", "scale=%d:%d:flags=neighbor" % (W * 2, H * 2),
                          "-c:v", "libx264", "-pix_fmt", "yuv420p", "-crf", "16", str(dst)], stdin=subprocess.PIPE)
    for _ in range(3):
        for f in frames:
            p.stdin.write(f.tobytes())
    p.stdin.close()
    p.wait()
    print("life", k, dst.name, flush=True)
    return frames[0], frames[N // 2]


def look(shots):
    """The top of every stage (where things move) at two moments, half size, one row a stage: look-life.png."""
    th, bar = 150, 26
    s = Image.new("RGB", (2 * (W // 2) + 8, len(shots) * (th + bar)), (18, 14, 20))
    d = ImageDraw.Draw(s)
    for r, (k, fr) in enumerate(shots):
        y = r * (th + bar)
        d.text((8, y + 4), "%s   %s" % (k, SR.STAGES[k]["name"]), fill=(239, 223, 190), font=font(16))
        for c, f in enumerate(fr):
            s.paste(Image.fromarray(f[:300]).resize((W // 2, th), Image.BOX), (c * (W // 2 + 8), y + bar))
    s.save(OUT / "look-life.png")


if __name__ == "__main__":
    args = sys.argv[1:]
    if args[:1] == ["repaint"]:
        creatures(repaint=args[1:])
        sys.exit()
    creatures()
    ks = args or sorted(STAGE_FX)
    look([(k, render(k)) for k in ks])
    print("look-life.png and look-creatures.png", flush=True)
