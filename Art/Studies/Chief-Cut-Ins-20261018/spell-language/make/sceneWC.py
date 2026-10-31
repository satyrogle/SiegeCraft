"""War Cry A with a shockwave. The captain draws breath (a purple glow builds in the chest), roars: a black contrast
frame, a white ring, then a shockwave rolls over the ground through the hunters' band throwing up dust, three air
rings fly from the mouth, and every hunter flashes purple and stands taller as the wave passes. It breaks into
sparks at the middle of the arena, so it never reads as a hit on your side. A second clip: a Focus Fire hit cracks
the glow in the tell and the cry is broken off."""
import math
import random

from PIL import ImageDraw

import art
import capkit as ck
import fxkit as fx

DARK, D2, MID, M2, BRIGHT, LIGHT, WHITE = ((20, 1, 77), (52, 15, 109), (77, 40, 134), (105, 44, 169),
                                           (131, 69, 191), (197, 149, 231), (255, 241, 255))
stage = ck.hunt_stage()
HUNTERS = [u for u in stage.units if u["side"] == "them"]
CAPT = [u for u in HUNTERS if u["name"] == "hunter_captain"][0]
for u in HUNTERS:
    u["H"], u["cx"] = art.unit_height(u["name"])
CH = (CAPT["x"] + CAPT["cx"] - 4, CAPT["y"] - 0.6 * CAPT["H"])
MOUTH = (CAPT["x"] + CAPT["cx"] - 16, CAPT["y"] - 0.86 * CAPT["H"])
FEET = (CAPT["x"], CAPT["y"])
L0, TELL = 2, 18
ROAR = L0 + TELL
RS = [24, 70, 140, 220, 305, 390, 470, 545]
SQ = 0.7
AIR = [1, 3, 5]
for u in HUNTERS:
    d = math.hypot(u["x"] - FEET[0], (u["y"] - FEET[1]) / SQ)
    u["hit"] = next((j for j, r in enumerate(RS) if r >= d), len(RS) - 1)


class Sparks:
    def __init__(self, seed):
        self.rnd = random.Random(seed)
        self.items = []

    def suck(self, cx, cy, n):
        for _ in range(n):
            r0 = self.rnd.uniform(36, 60)
            self.items.append(dict(kind="in", cx=cx, cy=cy, a=self.rnd.uniform(0, 2 * math.pi), r=r0, r0=r0, age=0))

    def rise(self, x, y, n):
        for _ in range(n):
            self.items.append(dict(kind="up", x=x + self.rnd.uniform(-14, 14), y=y + self.rnd.uniform(-6, 6),
                                   vx=0, vy=-self.rnd.uniform(1.2, 2.4), age=0, life=self.rnd.randint(6, 10)))

    def burst(self, x, y, n, speed, gravity=0.0):
        for _ in range(n):
            a = self.rnd.uniform(0, 2 * math.pi)
            v = speed * self.rnd.uniform(0.6, 1.1)
            self.items.append(dict(kind="shard" if gravity else "out", x=x, y=y, vx=math.cos(a) * v,
                                   vy=math.sin(a) * v * 0.8 - (3 if gravity else 0), g=gravity, age=0,
                                   life=self.rnd.randint(5, 8), ground=y + self.rnd.uniform(40, 70)))

    def step(self):
        keep = []
        for p in self.items:
            p["age"] += 1
            if p["kind"] == "in":
                p["r"] = p["r0"] * max(0.0, 1 - (p["age"] / 5) ** 1.6)
                if p["r"] > 3:
                    keep.append(p)
                continue
            if p["kind"] == "up":
                p["y"] += p["vy"]
                p["vy"] *= 0.88
            else:
                p["x"] += p["vx"]
                p["y"] += p["vy"]
                if p["kind"] == "shard":
                    p["vy"] += p["g"]
                    if p["y"] > p["ground"]:
                        p["y"], p["vy"], p["vx"] = p["ground"], 0, 0
                else:
                    p["vx"] *= 0.72
                    p["vy"] *= 0.72
            if p["age"] < p["life"]:
                keep.append(p)
        self.items = keep

    def draw(self, d):
        for p in self.items:
            if p["kind"] == "in":
                x = p["cx"] + p["r"] * math.cos(p["a"])
                y = p["cy"] + p["r"] * math.sin(p["a"]) * 0.8
                tx = p["cx"] + (p["r"] + 7) * math.cos(p["a"])
                ty = p["cy"] + (p["r"] + 7) * math.sin(p["a"]) * 0.8
                d.line([(round(tx), round(ty)), (round(x), round(y))], fill=MID)
                d.rectangle((round(x), round(y), round(x) + 1, round(y) + 1), fill=LIGHT)
                continue
            x, y = round(p["x"]), round(p["y"])
            late = p["age"] > p["life"] * 0.6
            if p["kind"] == "shard" and not late:
                d.polygon([(x, y - 3), (x + 4, y + 1), (x - 1, y + 3)], fill=LIGHT, outline=D2)
                d.point((x, y - 1), fill=WHITE)
            elif late:
                d.point((x, y), fill=BRIGHT)
            else:
                d.rectangle((x, y, x + 1, y + 1), fill=LIGHT)
                d.point((x, y), fill=WHITE)


def glow(d, k, crack=False):
    r = 2 + 7 * min(1.0, k / (TELL - 4))
    if k % 4 in (1, 2):
        r += 1
    x, y = CH
    fx.ell(d, x, y, r + 2, r + 2, fill=D2)
    fx.ell(d, x, y, r, r, fill=BRIGHT)
    fx.ell(d, x - r * 0.25, y - r * 0.25, r * 0.5, r * 0.5, fill=LIGHT)
    d.point((round(x - r * 0.3), round(y - r * 0.35)), fill=WHITE)
    if k >= TELL - 6:
        fx.ell(d, x, y, r + 6 + (k % 2) * 2, r + 6 + (k % 2) * 2, outline=M2)
    if crack:
        d.line([(round(x - r), round(y - 2)), (round(x - 2), round(y + 1)), (round(x + 1), round(y - 3)), (round(x + r), round(y + 2))],
               fill=DARK, width=2)


def shock(d, R, thin=False):
    cx, cy = FEET
    band = ((-15, MID, 3), (-12, BRIGHT, 4), (-7, LIGHT, 4), (-2, WHITE, 3)) if not thin else ((-6, BRIGHT, 2), (-3, LIGHT, 2), (0, WHITE, 2))
    for dr, col, w in band:
        r = R + dr
        if r > 3:
            d.ellipse((round(cx - r), round(cy - r * SQ), round(cx + r), round(cy + r * SQ)), outline=col, width=w)


def shells(d, R, n):
    """Sand thrown up where the wave hits the floor: hard edged shells leaning away from the captain."""
    cx = FEET[0]
    for x, y in ring_points(R + 2, n):
        lean = -1 if x < cx else 1
        h = 16 + (int(x + y) % 3) * 4
        poly = [(x - 8, y), (x + lean * 2, y - h * 0.55), (x + lean * 12, y - h), (x + lean * 6, y - h * 0.4), (x + 8, y)]
        d.polygon([(round(a), round(b)) for a, b in poly], fill=fx.S6, outline=fx.S2)
        d.line([(round(x + lean * 2), round(y - h * 0.55)), (round(x + lean * 12), round(y - h))], fill=fx.S7)


def shock_specks(d, R, step):
    cx, cy = FEET
    n = max(8, int(2 * math.pi * R / step))
    for j in range(n):
        a = 2 * math.pi * j / n
        x, y = cx + R * math.cos(a), cy + R * SQ * math.sin(a)
        d.rectangle((round(x), round(y), round(x) + (1 if step < 20 else 0), round(y) + (1 if step < 20 else 0)), fill=LIGHT)


def ring_points(R, n, xmin=640):
    cx, cy = FEET
    pts = []
    for j in range(n):
        a = 2 * math.pi * (j + 0.5) / n
        x, y = cx + R * math.cos(a), cy + R * SQ * math.sin(a)
        if xmin <= x <= fx.PLAY[2] and fx.PLAY[1] + 40 <= y <= fx.PLAY[3]:
            pts.append((x, y))
    return pts


def air_ring(d, x, y, ry, age):
    rx = max(4, ry * 0.36)
    box = (round(x - rx), round(y - ry), round(x + rx), round(y + ry))
    d.arc(box, 270, 90, fill=MID, width=2)
    d.arc(box, 90, 270, fill=WHITE if age < 3 else LIGHT, width=4 if age < 2 else 3)
    inner = (box[0] + 5, box[1] + 8, box[2] - 3, box[3] - 8)
    if age < 4 and inner[2] > inner[0] + 2 and inner[3] > inner[1] + 2:
        d.arc(inner, 105, 255, fill=LIGHT, width=2)


def frame(f, sparks, bits):
    k = f - ROAR
    shake = [0, 0]
    mods = {}
    if L0 <= f < ROAR and (f - L0) % 2 == 0:
        sparks.suck(CH[0], CH[1], 3)
    if 0 <= k < len(RS):
        for x, y in ring_points(RS[k], 14):
            bits.dust(x, y - 1, 1, spread=5, rise=0.9, size=(3, 6))
    for n, a0 in enumerate(AIR):
        if k == a0 + 6:
            x = MOUTH[0] - 30 - 64 * 6
            for t in range(10):
                sparks.burst(x, MOUTH[1] + (t - 4.5) * 14, 1, 3)
    for u in HUNTERS:
        if u is CAPT:
            continue
        j = k - u["hit"]
        if j == 0:
            mods[u["i"]] = dict(tint=BRIGHT)
            sparks.rise(u["x"], u["y"] - u["H"] - 4, 3)
        elif j in (1, 2):
            mods[u["i"]] = dict(dy={1: -3, 2: -1}[j], dx={1: -2, 2: -1}[j])
    if f >= ROAR - 4 and k < 0:
        mods[CAPT["i"]] = dict(dy=-1)
    if k == 1:
        mods[CAPT["i"]] = dict(tint=WHITE, dx=3)
    if k == 2:
        mods[CAPT["i"]] = dict(dx=1)
    shake = {1: [4, -2], 2: [-4, 2], 3: [2, -1]}.get(k, [0, 0])
    sparks.step()
    bits.step()

    def under(layer):
        d = ImageDraw.Draw(layer)
        if 0 <= k < len(RS):
            shock(d, RS[k], thin=k >= len(RS) - 2)
            if k >= 1:
                shells(d, RS[k], 16)
        if k == len(RS):
            shock_specks(d, RS[-1] + 14, 14)
        if k == len(RS) + 1:
            shock_specks(d, RS[-1] + 24, 30)
        bits.draw(d, kinds=("dust", "speck"))

    def over(layer):
        d = ImageDraw.Draw(layer)
        if L0 <= f < ROAR:
            glow(d, f - L0)
        if k == 0:
            fx.ell(d, CH[0], CH[1], 14, 14, fill=fx.OUT)
        if k == 1:
            fx.ell(d, CH[0], CH[1], 26, 26, outline=WHITE, width=3)
            fx.ell(d, CH[0], CH[1], 10, 10, fill=WHITE)
        if k == 2:
            fx.ell(d, CH[0], CH[1], 40, 40, outline=LIGHT, width=2)
        for n, a0 in enumerate(AIR):
            age = k - a0
            if 0 <= age < 6:
                air_ring(d, MOUTH[0] - 30 - 64 * age, MOUTH[1], 18 + 17 * age, age)
        sparks.draw(d)

    return stage.render(under=under, over=over, mods=mods, shake=tuple(shake), t_ms=f * fx.FRAME_MS)


BREAK = L0 + 10


def broken(f, sparks, bits):
    k = f - BREAK
    mods = {}
    shake = (0, 0)
    if L0 <= f < BREAK and (f - L0) % 2 == 0:
        sparks.suck(CH[0], CH[1], 3)
    if k == 2:
        sparks.burst(CH[0], CH[1], 10, 12, gravity=1.8)
        sparks.burst(CH[0], CH[1], 6, 6)
    t = {0: fx.rgb("#fff1cf"), 1: (224, 40, 30), 3: (224, 40, 30)}.get(k)
    if t:
        mods[CAPT["i"]] = dict(tint=t, dx={0: 2, 1: 3}.get(k, 0))
    elif 2 <= k <= 6:
        mods[CAPT["i"]] = dict(dx=(1 if k % 2 else -1) if k < 6 else 0)
    if k == 0:
        shake = (2, -1)
    sparks.step()
    bits.step()

    def over(layer):
        d = ImageDraw.Draw(layer)
        if L0 <= f < BREAK + 2:
            glow(d, min(f - L0, BREAK - L0 - 1), crack=k >= 1)
        if k == 0:
            fx.spikes(d, CH[0] + 10, CH[1] - 6, 811, 20, dirs=[-170, -120, -60, 0, 60, 120])
        if k == 2:
            fx.ell(d, CH[0], CH[1], 16, 16, fill=fx.OUT)
            fx.spikes(d, CH[0], CH[1], 812, 30, dirs=[-150, -100, -40, 20, 80, 140], light=True)
        if k == 3:
            fx.ell(d, CH[0], CH[1], 12, 10, fill=(70, 58, 85))
        sparks.draw(d)

    return stage.render(over=over, mods=mods, shake=shake, t_ms=f * fx.FRAME_MS)


if __name__ == "__main__":
    import sys
    import mp4kit
    N = ROAR + len(RS) + 12
    s1, b1 = Sparks(41), fx.Bits(42)
    frames = [frame(f, s1, b1) for f in range(N)]
    fx.strip(frames, [L0 + 6, ROAR - 1, ROAR, ROAR + 1, ROAR + 2, ROAR + 3, ROAR + 4, ROAR + 5, ROAR + 6, ROAR + 8, ROAR + 9, ROAR + 12],
             (600, 240, 1408, 800), "WC-strip.png", scale=1, cols=4)
    s2, b2 = Sparks(43), fx.Bits(44)
    bf = [broken(f, s2, b2) for f in range(BREAK + 12)]
    fx.strip(bf, [BREAK - 2, BREAK, BREAK + 1, BREAK + 2, BREAK + 3, BREAK + 6], (1080, 420, 1340, 680), "WC-break.png", scale=2, cols=6)
    if "check" in sys.argv:
        sys.exit()
    out = fx.STUDY.parent / "war-cry" / "A-shock-rings"
    out.mkdir(parents=True, exist_ok=True)
    mp4kit.mp4([fr.crop((560, 240, 1408, 800)) for fr in frames], out / "war-cry-A-shockwave.mp4")
    mp4kit.mp4([fr.crop((900, 330, 1300, 720)).resize((800, 780)) for fr in frames], out / "war-cry-A-shockwave-2x.mp4")
    mp4kit.mp4([fr.crop((900, 330, 1300, 720)).resize((800, 780)) for fr in bf], out / "war-cry-A-broken-by-focus-fire-2x.mp4")
    print("frames", N, len(bf), out)
