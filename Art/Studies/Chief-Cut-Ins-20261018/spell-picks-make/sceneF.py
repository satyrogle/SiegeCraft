"""Firebomb C: fire rain. Shadows grow on the sand under your front row, then three small firebombs land in a beat.
Saint11 layers: contrast frame, white flash, sharp blast, fire bubbles (fast), smoke (slow). Hunter purples."""
import math
import random

from PIL import ImageDraw

import capkit as ck
import fxkit as fx

HOT = [(255, 241, 255), (230, 179, 255), (177, 108, 240), (131, 69, 191), (77, 40, 134), (40, 18, 77)]
SMOKE = [(106, 90, 120), (70, 58, 85), (42, 34, 54)]
SHADOW = (184, 101, 47)
L0, TELL, GAP = 3, 24, 3
stage = ck.hunt_stage()
YOU = [u for u in stage.units if u["side"] == "you"]
FRONT = sorted([u for u in YOU if u["x"] > 500], key=lambda u: u["y"])
SLOTS = [(600, 422), (600, 562), (600, 702)]
IMPACT = [L0 + TELL + GAP * n for n in range(3)]
OWNER = {}
for n, (x, y) in enumerate(SLOTS):
    for u in FRONT:
        if abs(u["y"] - y) < 30:
            OWNER[n] = u
bits = fx.Bits(17)


class Fire:
    def __init__(self, x, y, seed):
        rnd = random.Random(seed)
        self.blobs = [dict(x=x + rnd.uniform(-24, 24), y=y - rnd.uniform(4, 44), r=rnd.uniform(11, 19), vy=-rnd.uniform(1.8, 4.0),
                           vx=rnd.uniform(-1.4, 1.4), heat=0) for _ in range(13)]
        self.smoke = []
        self.rnd = rnd

    def step(self, j):
        for b in self.blobs:
            b["x"] += b["vx"]
            b["y"] += b["vy"]
            b["r"] -= 1.4
            b["heat"] += 1
            if b["heat"] in (3, 5) and b["r"] > 4:
                self.smoke.append(dict(x=b["x"], y=b["y"] - 4, r=b["r"] * 0.8, age=0))
        self.blobs = [b for b in self.blobs if b["r"] > 1.5]
        for s in self.smoke:
            s["y"] -= 1.6
            s["x"] += self.rnd.uniform(-0.6, 0.6)
            s["age"] += 1
            if s["age"] > 3:
                s["r"] -= 0.7
        self.smoke = [s for s in self.smoke if s["r"] > 1.5]

    def draw(self, d):
        for s in self.smoke:
            c = SMOKE[min(2, s["age"] // 3)]
            fx.ell(d, s["x"], s["y"] + 1, s["r"], s["r"] * 0.9, fill=SMOKE[2])
            fx.ell(d, s["x"], s["y"], s["r"] - 1, s["r"] * 0.85, fill=c)
        for b in sorted(self.blobs, key=lambda q: -q["r"]):
            h = min(3, b["heat"] // 2)
            fx.ell(d, b["x"], b["y"], b["r"] + 1, b["r"] + 1, fill=HOT[5])
            fx.ell(d, b["x"], b["y"], b["r"], b["r"], fill=HOT[min(4, h + 2)])
        for b in sorted(self.blobs, key=lambda q: -q["r"]):
            h = min(3, b["heat"] // 2)
            fx.ell(d, b["x"] - 1, b["y"] - 1, b["r"] * 0.7, b["r"] * 0.7, fill=HOT[min(4, h + 1)])
            if h < 2:
                fx.ell(d, b["x"] - b["r"] * 0.25, b["y"] - b["r"] * 0.3, b["r"] * 0.38, b["r"] * 0.38, fill=HOT[0])


FIRES = {}


def bomb(d, x, y, streak):
    fx.ell(d, x, y, 5, 5, fill=(40, 18, 77), outline=fx.OUT)
    d.point((round(x - 2), round(y - 2)), fill=(230, 179, 255))
    d.line([(round(x), round(y - 6)), (round(x + 2), round(y - 9))], fill=(255, 241, 255))
    if streak:
        d.line([(round(x), round(y - 8)), (round(x), round(y - 8 - streak))], fill=(131, 69, 191), width=2)


def blast(d, x, y, seed, size):
    rnd = random.Random(seed)
    for a in (-170, -140, -115, -90, -65, -40, -10):
        a = math.radians(a + rnd.uniform(-8, 8))
        L = size * rnd.uniform(0.7, 1.15)
        w = size * 0.18
        tip = (x + math.cos(a) * L, y + math.sin(a) * L)
        nx, ny = -math.sin(a) * w, math.cos(a) * w
        d.polygon([(round(x + nx), round(y + ny)), (round(tip[0]), round(tip[1])), (round(x - nx), round(y - ny))],
                  fill=HOT[1], outline=HOT[3])
        d.line([(round(x), round(y)), (round(x + math.cos(a) * L * 0.55), round(y + math.sin(a) * L * 0.55))], fill=HOT[0])


def frame(f):
    shake = [0, 0]
    mods = {}
    for n, (x, y) in enumerate(SLOTS):
        k = f - IMPACT[n]
        if k == 2:
            FIRES[n] = Fire(x, y, 300 + n)
            bits.chunks(x, y - 3, y, 10, 10, spread=0.9, palette=(fx.S2, fx.S4, fx.S5))
        if k == 4:
            bits.dust(x, y - 2, 3, spread=18, rise=0.7, size=(3, 5))
        if n in FIRES and k >= 3:
            FIRES[n].step(k)
        for kk, sxy in ((1, (3, -2)), (2, (-3, 1)), (3, (1, 0))):
            if k == kk:
                shake[0] += sxy[0]
                shake[1] += sxy[1]
        if n in OWNER:
            t = {1: (255, 250, 240), 2: (224, 40, 30), 4: (224, 40, 30)}.get(k)
            if t:
                mods[OWNER[n]["i"]] = dict(tint=t, dx={1: -2, 2: -3, 3: -1}.get(k, 0))
            elif k in (3,):
                mods[OWNER[n]["i"]] = dict(dx=-1)
    shake = [max(-4, min(4, v)) for v in shake]
    bits.step()

    def under(layer):
        d = ImageDraw.Draw(layer)
        for n, (x, y) in enumerate(SLOTS):
            k = f - IMPACT[n]
            if L0 <= f < IMPACT[n]:
                p = (f - L0) / max(1, IMPACT[n] - L0)
                rx = 3 + 17 * p
                fx.ell(d, x, y, rx, rx * 0.35, fill=SHADOW)
            if k >= 2:
                fx.ell(d, x, y + 1, 17, 6, fill=(77, 40, 54) if k < 14 else (150, 80, 50))
                fx.ell(d, x, y, 12, 4, fill=(40, 18, 40) if k < 14 else (120, 62, 40))
        bits.draw(d, kinds=("dust", "speck"))

    def over(layer):
        d = ImageDraw.Draw(layer)
        for n, (x, y) in enumerate(SLOTS):
            k = f - IMPACT[n]
            if k == -2:
                bomb(d, x, y - 130, 20)
            if k == -1:
                bomb(d, x, y - 55, 40)
            if k == 0:
                fx.ell(d, x, y - 4, 24, 9, fill=fx.OUT)
                fx.ell(d, x, y - 18, 14, 14, fill=fx.OUT)
            if k == 1:
                fx.ell(d, x, y - 18, 22, 22, fill=HOT[0])
                for a in (0, 90, 180, 270):
                    r = math.radians(a + 45)
                    d.line([(round(x), round(y - 18)), (round(x + 40 * math.cos(r)), round(y - 18 + 40 * math.sin(r)))], fill=HOT[0], width=3)
            if k == 2:
                blast(d, x, y - 6, 500 + n, 58)
            if n in FIRES and k >= 3:
                FIRES[n].draw(d)
        bits.draw(d, kinds=("chunk",))

    return stage.render(under=under, over=over, mods=mods, shake=tuple(shake), t_ms=f * fx.FRAME_MS)


if __name__ == "__main__":
    import sys
    import mp4kit
    N = IMPACT[-1] + 20
    frames = [frame(f) for f in range(N)]
    fx.strip(frames, [IMPACT[0] - 2, IMPACT[0], IMPACT[0] + 1, IMPACT[0] + 2, IMPACT[0] + 4, IMPACT[1] + 2, IMPACT[2] + 2, IMPACT[2] + 5, IMPACT[2] + 9, IMPACT[2] + 14],
             (470, 300, 740, 760), "F-zoom.png", scale=1, cols=10)
    if "check" in sys.argv:
        sys.exit()
    out = fx.STUDY.parent / "firebomb" / "C-fire-rain"
    out.mkdir(parents=True, exist_ok=True)
    mp4kit.mp4([f.crop((150, 240, 1300, 800)) for f in frames], out / "firebomb-C-fire-rain.mp4")
    big = [f.crop((440, 290, 780, 770)) for f in frames]
    mp4kit.mp4([b.resize((b.width * 2, b.height * 2)) for b in big], out / "firebomb-C-fire-rain-2x.mp4")
    print("frames", N, out)
