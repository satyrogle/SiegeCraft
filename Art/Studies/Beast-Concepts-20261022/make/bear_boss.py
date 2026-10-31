"""The cage bear as Arina's war beast in the real Skullpeak fight, the Battle Cat beat: it waits dimmed beside her,
her sword sends a bolt into it, it powers up in a turquoise surge and rears roaring, then crashes down on a Warden.

PixelLab clips (anim2/idle_dim, transform, rage_slam; 256 canvases shown at 2x). Arina is the approved Skullpeak
chief sprite at 1x. The bolt, sparks, roar ring and ice shards are drawn on a half size layer and doubled to match
the bear's pixels. Hit stack after the fight-feel skill: freeze, white then red, jitter, knockback, shake.

python -I bear_boss.py   (artlab-env python: needs av)
"""
import json
import math
import pathlib
import random
from fractions import Fraction

import av
import numpy as np
from PIL import Image, ImageDraw

HERE = pathlib.Path(__file__).resolve().parent
D = HERE.parent / "directions"
A = D / "anim2"
CA = pathlib.Path("C:/Users/jacob/KindredSiege-Clean/Assets/Resources/CampaignArt")
TICK = 20
FEET_Y = 720
X0 = 800
ARINA_FEET = (1300, 560)
TQ = (64, 224, 208)
TQ_LIGHT = (196, 255, 246)
WHITE = (250, 252, 255)
ICE = [(236, 242, 248), (178, 196, 214), (120, 140, 168), (40, 52, 70)]


def clip(name, flip):
    meta = json.loads((CA / (name + "_sw_idle.json")).read_text(encoding="utf-8"))
    frs = meta["frames"] if isinstance(meta["frames"], list) else list(meta["frames"].values())
    sheet = Image.open(CA / (name + "_sw_idle.png")).convert("RGBA")
    out = []
    for f in frs:
        r = f["frame"]
        im = sheet.crop((r["x"], r["y"], r["x"] + r["w"], r["y"] + r["h"]))
        out.append(im.transpose(Image.FLIP_LEFT_RIGHT) if flip else im)
    w = frs[0]["frame"]["w"]
    ax = meta.get("anchorX", w / 2)
    return out, ((w - ax) if flip else ax, meta.get("anchorY", frs[0]["frame"]["h"]))


def load(name, n):
    return [Image.open(A / name / ("%02d.png" % i)).convert("RGBA") for i in range(n)]


def feet_row(im):
    return int(np.nonzero((np.asarray(im)[..., 3] > 0).any(1))[0].max())


def tint(spr, col):
    a = np.asarray(spr).copy()
    a[a[..., 3] > 0, :3] = col
    return Image.fromarray(a, "RGBA")


def redden(spr):
    a = np.asarray(spr).astype(np.float32).copy()
    on = a[..., 3] > 0
    a[on, :3] = a[on, :3] * 0.45 + np.array([224, 40, 30]) * 0.55
    return Image.fromarray(a.astype(np.uint8), "RGBA")


def bolt(d, p0, p1, seed, width):
    """A jagged lightning bolt from p0 to p1: a turquoise body with a white core, hard edges."""
    rnd = random.Random(seed)
    pts = [p0]
    n = 7
    for i in range(1, n):
        t = i / n
        x = p0[0] + (p1[0] - p0[0]) * t + rnd.uniform(-7, 7)
        y = p0[1] + (p1[1] - p0[1]) * t + rnd.uniform(-7, 7)
        pts.append((x, y))
    pts.append(p1)
    pts = [(round(x), round(y)) for x, y in pts]
    d.line(pts, fill=TQ, width=width + 2)
    d.line(pts, fill=WHITE, width=max(1, width - 1))


def star(d, x, y, r, col):
    d.line([(x - r, y), (x + r, y)], fill=col)
    d.line([(x, y - r), (x, y + r)], fill=col)
    d.point((x, y), fill=WHITE)


def main():
    stage = Image.open(D / "fight" / "skullpeak-clean.png").convert("RGBA")
    idle = load("idle_dim", 17)
    trans = load("transform", 41)
    slam = load("rage_slam", 41)
    big = lambda im: im.resize((512, 512), Image.NEAREST)
    Y_dim = FEET_Y - 2 * feet_row(idle[0])
    Y_tr = FEET_Y - 2 * feet_row(trans[0])
    Y_sl = Y_tr + 2 * (feet_row(trans[40]) - feet_row(slam[0]))
    arina, (aax, aay) = clip("skullpeak_chief", False)
    victim, (vax, vay) = clip("clarth_warden", True)

    hitf = np.asarray(slam[16])[..., 3] > 0
    ys, xs = np.nonzero(hitf)
    low = ys > ys.max() - 30
    impact = (X0 + 2 * int(xs[low].min()) + 24, Y_sl + 2 * int(ys.max()) - 8)
    vfeet = (impact[0] - 26, FEET_Y)
    sword = (ARINA_FEET[0] - aax + 75, ARINA_FEET[1] - aay + 165)
    back = (X0 + 2 * 150, Y_tr + 2 * 110)
    mouth = (X0 + 2 * 34, Y_tr + 2 * 72)
    print("impact", impact, "victim feet", vfeet, "sword", sword, "bear back", back)

    seq = []
    for k in range(17):
        seq.append(("idle", k, 90, {}))
    seq.append(("trans", 0, 200, {}))
    for k in range(1, 7):
        seq.append(("trans", k, 60, dict(bolt=k, flash=k == 3, shake=(3, -2) if k == 3 else (0, 0))))
    for k in range(7, 28):
        seq.append(("trans", k, 45, dict(sparks=k)))
    for k in range(28, 33):
        seq.append(("trans", k, 60, {}))
    for k in range(33, 38):
        seq.append(("trans", k, 90, dict(ring=k - 33, shake=((2, -1), (-2, 1), (1, 0), (0, 0), (0, 0))[k - 33])))
    for k in range(38, 41):
        seq.append(("trans", k, 80, {}))
    for k in range(15):
        seq.append(("slam", k, 50, {}))
    seq.append(("slam", 15, 40, {}))
    seq += [("slam", 16, 40, dict(white=1, shake=(5, -3), burst=0, jit=1)),
            ("slam", 16, 40, dict(white=1, shake=(-4, 2), burst=0, kb=4, jit=1)),
            ("slam", 16, 40, dict(red=1, shake=(3, -1), burst=1, kb=8, jit=1)),
            ("slam", 16, 40, dict(red=1, shake=(-1, 0), burst=1, kb=8, jit=1)),
            ("slam", 17, 70, dict(burst=2, kb=7)), ("slam", 18, 70, dict(red=1, kb=5)),
            ("slam", 19, 70, dict(kb=3))]
    for k in range(20, 41):
        seq.append(("slam", k, 70, dict(kb=2 if k == 20 else 0, red=k == 21)))

    rnd = random.Random(5)
    out, t = [], 0
    for kind, k, ms, fxs in seq:
        for sub in range(ms // TICK):
            f = stage.copy()
            a_spr = arina[(t // 100) % len(arina)]
            f.alpha_composite(a_spr, (int(ARINA_FEET[0] - aax), int(ARINA_FEET[1] - aay)))
            v = victim[(t // 100) % len(victim)]
            if fxs.get("white"):
                v = tint(v, (255, 250, 240))
            elif fxs.get("red"):
                v = redden(v)
            jit = fxs.get("jit", 0) * (1 if (t // 40) % 2 else -1)
            f.alpha_composite(v, (int(vfeet[0] - vax - fxs.get("kb", 0) + jit), int(vfeet[1] - vay)))
            if kind == "idle":
                f.alpha_composite(big(idle[k]), (X0, Y_dim))
            elif kind == "trans":
                bear = trans[k]
                if fxs.get("flash"):
                    bear = tint(bear, (230, 255, 252))
                f.alpha_composite(big(bear), (X0, Y_tr))
            else:
                f.alpha_composite(big(slam[k]), (X0, Y_sl))

            layer = Image.new("RGBA", (720, 450), (0, 0, 0, 0))
            d = ImageDraw.Draw(layer)
            if "bolt" in fxs:
                bolt(d, (sword[0] / 2, sword[1] / 2), (back[0] / 2, back[1] / 2), 100 + fxs["bolt"] * 3 + sub, 2)
                star(d, round(sword[0] / 2), round(sword[1] / 2), 4, TQ_LIGHT)
            if "sparks" in fxs:
                for _ in range(4):
                    sx = rnd.uniform(X0 / 2 + 20, X0 / 2 + 240)
                    sy = rnd.uniform(Y_tr / 2 + 40, FEET_Y / 2 - 6)
                    star(d, round(sx), round(sy), rnd.choice((2, 3, 4)), rnd.choice((TQ, TQ_LIGHT)))
            if "ring" in fxs:
                r = 10 + 14 * fxs["ring"]
                cx, cy = mouth[0] / 2, mouth[1] / 2
                d.ellipse((round(cx - r), round(cy - r * 0.7), round(cx + r), round(cy + r * 0.7)), outline=TQ_LIGHT, width=1)
                if r > 16:
                    d.ellipse((round(cx - r + 6), round(cy - r * 0.7 + 4), round(cx + r - 6), round(cy + r * 0.7 - 4)), outline=WHITE, width=1)
            if "burst" in fxs:
                cx, cy = impact[0] / 2, impact[1] / 2
                size = (26, 19, 12)[fxs["burst"]]
                r0 = random.Random(9)
                for i in range(11):
                    ang = math.radians(-200 + i * 20 + r0.uniform(-8, 8))
                    L = size * r0.uniform(0.6, 1.1)
                    w = max(1.0, size * 0.13)
                    tip = (cx + math.cos(ang) * L, cy + math.sin(ang) * L * 0.75)
                    nx, ny = -math.sin(ang) * w, math.cos(ang) * w
                    d.polygon([(round(cx + nx), round(cy + ny)), (round(tip[0]), round(tip[1])), (round(cx - nx), round(cy - ny))],
                              fill=ICE[(i + fxs["burst"]) % 3], outline=ICE[-1])
            f.alpha_composite(layer.resize((1440, 900), Image.NEAREST))
            sx, sy = fxs.get("shake", (0, 0))
            if (sx, sy) != (0, 0):
                g = stage.copy()
                g.paste(f, (sx, sy))
                f = g
            out.append(f.convert("RGB").crop((300, 100, 1420, 820)))
            t += TICK

    path = A / "bear-boss.mp4"
    c = av.open(str(path), mode="w")
    s = c.add_stream("libx264", rate=Fraction(1000, TICK))
    s.width, s.height = out[0].size
    s.pix_fmt = "yuv420p"
    s.options = {"crf": "14", "preset": "slow"}
    for im in out + [out[-1]] * (600 // TICK):
        for pkt in s.encode(av.VideoFrame.from_ndarray(np.asarray(im), format="rgb24")):
            c.mux(pkt)
    for pkt in s.encode():
        c.mux(pkt)
    c.close()
    marks = [10, 82, 84, 100, 150, 178, 225, 236]
    keys = [out[i] for i in marks if i < len(out)]
    w, h = keys[0].size
    sheet = Image.new("RGB", (4 * (w // 2) + 12, 2 * (h // 2) + 4), (20, 20, 24))
    for n, im in enumerate(keys):
        sheet.paste(im.resize((w // 2, h // 2), Image.LANCZOS), ((n % 4) * (w // 2 + 4), (n // 4) * (h // 2 + 4)))
    sheet = sheet.resize((1800, round(sheet.height * 1800 / sheet.width)), Image.LANCZOS)
    sheet.save(A / "bear-boss-keys.png")
    print(path.name, len(out), "frames at", TICK, "ms", path.stat().st_size // 1024, "KB")


if __name__ == "__main__":
    main()
