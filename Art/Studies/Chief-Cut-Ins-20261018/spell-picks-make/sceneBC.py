"""Blood Call A: blood ribbons. The wounds of every bleeding fighter of yours beat red in the tell (a pulsing red
outline and a beading drop) while the priestess's blood bowl glows. Then a pointy jet tears out of every wound in one
frame, bends into a thick ribbon, flows in an arc into her bowl, coils round it, and breaks into drops that fall
and splat. Her crimsons only."""
import math
import random

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage

import capkit as ck
import fxkit as fx

C0, C1, C2, C3, C4 = fx.rgb("#440f15"), fx.rgb("#831118"), fx.rgb("#d11b1b"), fx.rgb("#d95447"), fx.rgb("#f5ada3")
stage = ck.Stage("stoneveil", "sand")
YOU = [u for u in stage.units if u["side"] == "you"]
BLEEDERS = [u for u in YOU if (u["x"], u["y"]) in ((415, 422), (420, 562), (600, 702))]
BOWL = (1199, 455)
L0, TELL = 2, 16
REL = L0 + TELL
FLOW = 7
for n, u in enumerate(BLEEDERS):
    a = np.asarray(u["spr"])[..., 3] > 0
    ys, xs = np.nonzero(a)
    H = u["y"] - (u["box"][1] + ys.min())
    u["wound"] = (u["x"] + 4, u["y"] - 0.55 * H)
    ring = ndimage.binary_dilation(a, iterations=2) & ~a
    rim = np.zeros(a.shape + (4,), np.uint8)
    rim[ring] = C2 + (255,)
    u["rim"] = Image.fromarray(rim, "RGBA")
    wx, wy = u["wound"]
    u["ctrl"] = ((wx + BOWL[0]) / 2 - 40 + 30 * n, min(wy, BOWL[1]) - 170 - 30 * n)


def bez(u, t):
    (x0, y0), (cx, cy), (x1, y1) = u["wound"], u["ctrl"], BOWL
    return ((1 - t) ** 2 * x0 + 2 * (1 - t) * t * cx + t * t * x1, (1 - t) ** 2 * y0 + 2 * (1 - t) * t * cy + t * t * y1)


def ribbon(d, pts, wmax):
    n = len(pts)
    if n < 2:
        return
    for layer, col, extra in ((0, C0, 2), (1, C1, 0), (2, C2, -2)):
        for i in range(n - 1):
            w = max(1, round((wmax + extra) * (0.35 + 0.65 * (i + 1) / n)))
            d.line([(round(pts[i][0]), round(pts[i][1])), (round(pts[i + 1][0]), round(pts[i + 1][1]))], fill=col, width=w)
    hl = [(round(x), round(y - max(1, wmax * 0.25))) for x, y in pts[n // 3:]]
    if len(hl) >= 2:
        d.line(hl, fill=C4, width=1)


def jet(d, x, y, ang, L):
    ux, uy = math.cos(ang), math.sin(ang)
    nx, ny = -uy, ux
    tip = (x + ux * L, y + uy * L)
    poly = [(x + nx * 4, y + ny * 4), tip, (x - nx * 4, y - ny * 4)]
    d.polygon([(round(a), round(b)) for a, b in poly], fill=C2, outline=C0)
    d.line([(round(x), round(y)), (round(x + ux * L * 0.6), round(y + uy * L * 0.6))], fill=C4)


def splash(d, x, y, seed, size):
    rnd = random.Random(seed)
    for a in range(-170, 11, 30):
        r = math.radians(a + rnd.uniform(-10, 10))
        L = size * rnd.uniform(0.6, 1.1)
        tip = (x + math.cos(r) * L, y + math.sin(r) * L)
        nx, ny = -math.sin(r) * size * 0.16, math.cos(r) * size * 0.16
        d.polygon([(round(x + nx), round(y + ny)), (round(tip[0]), round(tip[1])), (round(x - nx), round(y - ny))], fill=C2, outline=C0)


class Drops:
    def __init__(self, seed):
        self.rnd = random.Random(seed)
        self.items = []
        self.splats = []

    def add(self, x, y, n, speed, up=0.0):
        for _ in range(n):
            a = self.rnd.uniform(0, 2 * math.pi)
            v = speed * self.rnd.uniform(0.4, 1.0)
            self.items.append(dict(x=x, y=y, vx=math.cos(a) * v, vy=math.sin(a) * v * 0.7 - up, r=self.rnd.uniform(1.5, 3.2),
                                   ground=y + self.rnd.uniform(120, 190) if up else y + self.rnd.uniform(20, 60)))

    def step(self):
        keep = []
        for p in self.items:
            p["vy"] += 1.6
            p["x"] += p["vx"]
            p["y"] += p["vy"]
            if p["y"] >= p["ground"]:
                self.splats.append([p["x"], p["ground"], p["r"] + 2, 0])
            else:
                keep.append(p)
        self.items = keep
        for s in self.splats:
            s[3] += 1
        self.splats = [s for s in self.splats if s[3] < 9]

    def draw_air(self, d):
        for p in self.items:
            r = p["r"]
            fx.ell(d, p["x"], p["y"], r, r * 1.15, fill=C1)
            d.point((round(p["x"] - 0.5), round(p["y"] - r * 0.5)), fill=C4)

    def draw_ground(self, d):
        for x, y, r, age in self.splats:
            if age < 6:
                fx.ell(d, x, y, r + 1, (r + 1) * 0.4, fill=C0)
                fx.ell(d, x, y, r, r * 0.35, fill=C1)
            else:
                d.point((round(x), round(y)), fill=C1)


def ease(x):
    x = max(0.0, min(1.0, x))
    return 1 - (1 - x) ** 2


def frame(f, drops):
    k = f - REL
    mods, shake = {}, (0, 0)
    tint = {0: (255, 250, 240), 1: (224, 40, 30), 3: (224, 40, 30)}.get(k)
    for u in BLEEDERS:
        m = {}
        if tint:
            m["tint"] = tint
        if k in (1, 2):
            m["dx"] = {1: 2, 2: 1}[k]
        mods[u["i"]] = m
    if k == 0:
        shake = (3, -2)
    if k == 1:
        shake = (-3, 1)
    if 1 <= k <= FLOW:
        for u in BLEEDERS:
            tail = max(0.0, ease(k / FLOW) - 0.45)
            drops.add(*bez(u, tail), 1, 2)
    if k == FLOW + 5:
        drops.add(BOWL[0], BOWL[1] - 4, 18, 9, up=6)
    drops.step()

    def between(layer, u):
        if u not in BLEEDERS:
            return
        d = ImageDraw.Draw(layer)
        if L0 <= f < REL:
            j = f - L0
            if j % 4 in (0, 1):
                layer.alpha_composite(u["rim"], (u["box"][0] + mods.get(u["i"], {}).get("dx", 0), u["box"][1]))
            r = 2.5 + 3 * j / TELL + (1.5 if j % 4 in (0, 1) else 0)
            wx, wy = u["wound"]
            fx.ell(d, wx, wy + r * 0.4, r, r * 1.2, fill=C2, outline=C0)
            d.point((round(wx - r * 0.3), round(wy - r * 0.3)), fill=C4)

    def under(layer):
        drops.draw_ground(ImageDraw.Draw(layer))

    def over(layer):
        d = ImageDraw.Draw(layer)
        if L0 <= f < REL:
            j = f - L0
            r = 8 + (j % 4 in (0, 1)) * 2 + 3 * j / TELL
            fx.ell(d, BOWL[0], BOWL[1] - 2, r, r * 0.8, outline=C2, width=2)
        if k == 0:
            for u in BLEEDERS:
                wx, wy = u["wound"]
                cx, cy = bez(u, 0.12)
                jet(d, wx, wy, math.atan2(cy - wy, cx - wx), 28)
                splash(d, wx, wy, int(wx), 16)
        if 1 <= k <= FLOW + 2:
            for u in BLEEDERS:
                head = ease(k / FLOW) if k <= FLOW else 1.0
                tail = max(0.0, head - 0.45) if k <= FLOW else min(1.0, 0.7 + 0.15 * (k - FLOW))
                if k == 1:
                    head, tail = 0.16, 0.0
                pts = [bez(u, tail + (head - tail) * s / 15) for s in range(16)]
                ribbon(d, pts, 10)
                hx, hy = pts[-1]
                fx.ell(d, hx, hy, 6, 6, fill=C2, outline=C0)
                d.point((round(hx - 1), round(hy - 2)), fill=C4)
        if FLOW + 1 <= k <= FLOW + 4:
            j = k - FLOW - 1
            r = (9, 15, 12, 12)[j]
            for s in range(3):
                a0 = j * 50 + s * 120
                box = (BOWL[0] - 24, BOWL[1] - 12 - r, BOWL[0] + 24, BOWL[1] - 12 + 10 - r)
                d.arc(box, a0, a0 + 140, fill=C0, width=4)
                d.arc(box, a0, a0 + 140, fill=C2, width=2)
            fx.ell(d, BOWL[0], BOWL[1] - 6 - r, r + 1, r + 1, fill=C0)
            fx.ell(d, BOWL[0], BOWL[1] - 6 - r, r, r, fill=C1)
            fx.ell(d, BOWL[0] - 2, BOWL[1] - 8 - r, r * 0.7, r * 0.7, fill=C2)
            fx.ell(d, BOWL[0] - r * 0.35, BOWL[1] - 6 - r * 1.45, r * 0.25, r * 0.25, fill=C4)
        if k == FLOW + 5:
            fx.ell(d, BOWL[0], BOWL[1] - 18, 16, 16, fill=C0)
            splash(d, BOWL[0], BOWL[1] - 18, 9, 30)
        drops.draw_air(d)

    return stage.render(under=under, over=over, between=between, mods=mods, shake=shake, t_ms=f * fx.FRAME_MS)


if __name__ == "__main__":
    import sys
    import mp4kit
    drops = Drops(5)
    N = REL + FLOW + 18
    frames = [frame(f, drops) for f in range(N)]
    pick = [L0 + 2, L0 + 4, REL, REL + 1, REL + 2, REL + 4, REL + 6, REL + FLOW + 1, REL + FLOW + 2, REL + FLOW + 5, REL + FLOW + 7, REL + FLOW + 11]
    fx.strip(frames, pick, (330, 200, 1300, 760), "BC-strip.png", scale=1, cols=4)
    if "check" in sys.argv:
        sys.exit()
    out = fx.STUDY.parent / "blood-call" / "A-blood-ribbons"
    out.mkdir(parents=True, exist_ok=True)
    mp4kit.mp4([fr.crop((150, 160, 1300, 760)) for fr in frames], out / "blood-call-A-ribbons.mp4")
    mp4kit.mp4([fr.crop((330, 300, 730, 740)).resize((800, 880)) for fr in frames], out / "blood-call-A-ribbons-2x-your-side.mp4")
    print("frames", N, out)
