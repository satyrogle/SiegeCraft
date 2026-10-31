"""Export take A (the leaper) as one bite: a layer behind the bitten fighter and a layer in front of it,
anchored at the fighter's feet, 80 ms frames, plus the engine cues (tints, pop, shake, freeze)."""
import json

import numpy as np
from PIL import Image, ImageDraw

import fxkit as fx
import sceneA as A

N = 24
WORK = (680, 460)
FEET = (360, 340)
HEIGHT = 100
COLS = 6
OUT = fx.STUDY / "fx"

t = dict(i=-1, x=FEET[0], feet=FEET[1], h=HEIGHT, start=0, seed=505, bx=FEET[0] - 70, dx=FEET[0] + 70)
bits = fx.Bits(56)

CUES = [
    {"frame": 10, "victim": "white", "shake": [4, -2], "freezeVictimMs": 240, "jitterPx": 1, "damageNumber": True},
    {"frame": 11, "victim": "red", "shake": [-4, 2]},
    {"frame": 12, "victim": "normal", "shake": [2, -1]},
    {"frame": 13, "victim": "white", "victimDy": -3},
    {"frame": 14, "victim": "red", "victimDy": -1},
    {"frame": 15, "victim": "normal", "victimDy": 0},
]


def draw_shark(layer, k):
    key, (px, py) = A.pivot_for(t, k)
    spr, piv = A.POSE[key]
    fx.place(layer, spr, piv, px, py, clip=A.ground_clip(t))
    dd = ImageDraw.Draw(layer)
    if 7 <= k <= 8:
        fx.front_lip(dd, t["bx"], t["feet"], *A.HR)
    if 14 <= k <= 15:
        fx.front_lip(dd, t["dx"], t["feet"], *A.HR)
    if k == 7:
        fx.spikes(dd, t["bx"] - 4, t["feet"], t["seed"], 44, dirs=[-170, -148, -125, -100])
    if k == 15:
        fx.spikes(dd, t["dx"], t["feet"], t["seed"] + 1, 36, dirs=[-160, -130, -50, -20])


def render():
    behind, front = [], []
    y = t["feet"]
    for k in range(N):
        fxp = t["bx"] - 72 + 12 * min(k, 6)
        if 0 <= k <= 5:
            bits.chunks(fxp - 6, y - 2, y, 2, 6, spread=0.35, aim=-120)
        if k == 7:
            bits.chunks(t["bx"], y - 3, y, 14, 12, spread=0.8, aim=-75)
            bits.dust(t["bx"], y - 2, 3, spread=16, rise=0.9, size=(3, 6))
        if k in (8, 9):
            kx, ky = A.pivot_for(t, k)[1]
            bits.chunks(kx - 10, ky + 6, y, 4, 2, spread=1.0, aim=90)
        if k == 15:
            bits.chunks(t["dx"], y - 3, y, 12, 11, spread=0.8, aim=-90)
            bits.dust(t["dx"], y - 2, 3, spread=16, rise=0.9, size=(3, 6))
        bits.step()

        b = Image.new("RGBA", WORK, (0, 0, 0, 0))
        d = ImageDraw.Draw(b)
        if 0 <= k <= 5:
            A.wake(d, fxp, y, k)
        if k == 6:
            fx.ell(d, t["bx"], y, A.HR[0] + 3, A.HR[1] + 1, fill=fx.OUT)
        if 7 <= k <= 15:
            fx.hole(d, t["bx"], y, *A.HR)
        if 14 <= k <= 16:
            fx.hole(d, t["dx"], y, *A.HR)
        if k >= 16:
            fx.crater(d, t["bx"], y, A.HR[0], A.HR[1], dark=k < 22)
        if k >= 17:
            fx.crater(d, t["dx"], y, A.HR[0], A.HR[1], dark=k < 23)
        bits.draw(d, kinds=("dust", "speck"))
        if 0 <= k <= 5:
            A.fin(d, fxp, y)
        if k in (7, 8, 9, 13, 14, 15):
            draw_shark(b, k)

        f = Image.new("RGBA", WORK, (0, 0, 0, 0))
        if 10 <= k <= 12:
            draw_shark(f, k)
        bits.draw(ImageDraw.Draw(f), kinds=("chunk",))
        behind.append(b)
        front.append(f)
    return behind, front


def union_box(frames):
    box = None
    for im in frames:
        bb = im.getbbox()
        if bb:
            box = bb if box is None else (min(box[0], bb[0]), min(box[1], bb[1]), max(box[2], bb[2]), max(box[3], bb[3]))
    return box


def write_sheet(frames, box, name, layer):
    w, h = box[2] - box[0], box[3] - box[1]
    rows = (len(frames) + COLS - 1) // COLS
    sheet = Image.new("RGBA", (COLS * w, rows * h), (0, 0, 0, 0))
    meta = {"frames": [], "anchorX": float(FEET[0] - box[0]), "anchorY": float(FEET[1] - box[1]),
            "impactFrame": 10, "layer": layer, "fighterHeight": HEIGHT, "frameMs": fx.FRAME_MS, "cues": CUES}
    for i, im in enumerate(frames):
        x, y = (i % COLS) * w, (i // COLS) * h
        sheet.paste(im.crop(box), (x, y))
        meta["frames"].append({"filename": "%s_%02d" % (name, i), "frame": {"x": x, "y": y, "w": w, "h": h},
                               "rotated": False, "trimmed": False,
                               "spriteSourceSize": {"x": 0, "y": 0, "w": w, "h": h}, "sourceSize": {"w": w, "h": h},
                               "duration": fx.FRAME_MS})
    OUT.mkdir(exist_ok=True)
    sheet.save(OUT / (name + ".png"))
    (OUT / (name + ".json")).write_text(json.dumps(meta, indent=1), encoding="utf-8")
    return sheet.size, (w, h), (meta["anchorX"], meta["anchorY"])


if __name__ == "__main__":
    behind, front = render()
    bb, fb = union_box(behind), union_box(front)
    box = (min(bb[0], fb[0]) - 2, min(bb[1], fb[1]) - 2, max(bb[2], fb[2]) + 2, max(bb[3], fb[3]) + 2)
    for frames, name, layer in ((behind, "dust_shark_leap_behind", "behind"), (front, "dust_shark_leap_front", "front")):
        print(name, *write_sheet(frames, box, name, layer))
