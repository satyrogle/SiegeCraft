"""Tide C: the water serpent. In the tell water coils up the Thundercoast chief's harpoon. Then a serpent of water
bursts from the tip, arcs over the field with its jaws shut, opens them over your back row, dives down through the
three of them (one splash each) into the sand, and bursts into rain that leaves puddles which drain away."""
import math
import random

import numpy as np
from PIL import Image, ImageDraw

import capkit as ck
import fxkit as fx

OUTW = (9, 11, 19)
DEEP, BODY, LIGHT, CYAN, FOAM, WHITE = fx.rgb("#0b5398"), fx.rgb("#2f8fa8"), fx.rgb("#81b5c0"), fx.rgb("#26ecfd"), fx.rgb("#befafb"), (240, 252, 255)
NAVY = fx.rgb("#183138")
RAW = fx.STUDY.parent / "tide" / "raw"
stage = ck.Stage("thundercoast", "sand")
YOU = [u for u in stage.units if u["side"] == "you"]
BACKROW = sorted([u for u in YOU if 380 < u["x"] < 460], key=lambda u: u["y"])
TIP, BUTT = (1135, 455), (1143, 610)
im = Image.open(fx.CA / "thundercoast_chief_sw_idle.png").convert("RGBA")
TCPAL = np.array(sorted({p[:3] for p in im.get_flattened_data() if p[3] > 0} |
                        {NAVY, DEEP, BODY, LIGHT, CYAN, FOAM, WHITE, fx.rgb("#315265"), fx.rgb("#41a2b9")}), np.float32)


def head_src(name):
    src = fx.key(RAW / name, tol=60)
    a = np.asarray(src)[..., 3] > 0
    ys, xs = np.nonzero(a)
    left = xs.min()
    snout = (float(left), float(ys[xs <= left + 3].mean()))
    bottom = ys.max()
    neck = (float(xs[ys >= bottom - 4].mean()), float(bottom))
    return src, snout, neck


OPEN_SRC, OPEN_SNOUT, OPEN_NECK = head_src("serpent-head-1.png")
SHUT_SRC, SHUT_SNOUT, SHUT_NECK = head_src("serpent-shut-1.png")
SCALE = 0.115
cache = {}


def head(which, travel_deg, scale=1.0):
    src, snout, neck = (OPEN_SRC, OPEN_SNOUT, OPEN_NECK) if which == "open" else (SHUT_SRC, SHUT_SNOUT, SHUT_NECK)
    src_deg = math.degrees(math.atan2(snout[1] - neck[1], snout[0] - neck[0]))
    rot = round(-(travel_deg - src_deg) / 5) * 5
    key_ = (which, rot, round(scale, 2))
    if key_ not in cache:
        cache[key_] = fx.native(src, angle=rot, scale=SCALE * scale, pivot=snout, outline=OUTW, pal=TCPAL)
    return cache[key_]


def bez(t, p0, c, p1):
    return ((1 - t) ** 2 * p0[0] + 2 * (1 - t) * t * c[0] + t * t * p1[0], (1 - t) ** 2 * p0[1] + 2 * (1 - t) * t * c[1] + t * t * p1[1])


P1, P2 = (450, 300), (426, 760)
_C1 = (800, 110)
_t = math.atan2(P1[1] - _C1[1], P1[0] - _C1[0])
_C2 = (P1[0] + 90 * math.cos(_t), P1[1] + 90 * math.sin(_t))
PATH = [bez(i / 200, TIP, _C1, P1) for i in range(201)] + [bez(i / 120, P1, _C2, P2) for i in range(1, 121)]
S = [0.0]
for (x0, y0), (x1, y1) in zip(PATH, PATH[1:]):
    S.append(S[-1] + math.hypot(x1 - x0, y1 - y0))
L_FLIGHT = S[200]
L_TOTAL = S[-1]
HEAD_LEN, BODY_LEN = 100, 470


def at(s):
    s = max(0.0, min(L_TOTAL, s))
    i = int(np.searchsorted(S, s))
    i = max(1, min(len(S) - 1, i))
    t = (s - S[i - 1]) / max(1e-6, S[i] - S[i - 1])
    x = PATH[i - 1][0] + (PATH[i][0] - PATH[i - 1][0]) * t
    y = PATH[i - 1][1] + (PATH[i][1] - PATH[i - 1][1]) * t
    ang = math.degrees(math.atan2(PATH[i][1] - PATH[i - 1][1], PATH[i][0] - PATH[i - 1][0]))
    return x, y, ang


def water_ribbon(d, pts, w0, w1, k):
    n = len(pts)
    if n < 2:
        return
    for col, extra in ((OUTW, 2), (DEEP, 0), (BODY, -6)):
        for i in range(n - 1):
            w = max(1, round((w1 + (w0 - w1) * i / (n - 1)) + extra))
            d.line([(round(pts[i][0]), round(pts[i][1])), (round(pts[i + 1][0]), round(pts[i + 1][1]))], fill=col, width=w)
    for i in range(1, n - 1):
        w = w1 + (w0 - w1) * i / (n - 1)
        x0, y0 = pts[i - 1]
        x1, y1 = pts[i + 1]
        ang = math.atan2(y1 - y0, x1 - x0)
        nx, ny = math.sin(ang), -math.cos(ang)
        if ny > 0:
            nx, ny = -nx, -ny
        x, y = pts[i]
        d.point((round(x + nx * w * 0.28), round(y + ny * w * 0.28)), fill=LIGHT)
        if (i + k) % 3 == 0:
            d.rectangle((round(x + nx * w * 0.42), round(y + ny * w * 0.42), round(x + nx * w * 0.42) + 1, round(y + ny * w * 0.42) + 1), fill=FOAM)


HEAD_LEN = math.hypot(SHUT_SNOUT[0] - SHUT_NECK[0], SHUT_SNOUT[1] - SHUT_NECK[1]) * SCALE * 0.92
L0, TELL = 2, 16
REL = L0 + TELL
GROUND = 724


def coil_segments(j):
    top = BUTT[1] - (BUTT[1] - TIP[1]) * min(1.0, (j + 1) / 12)
    r = 9 if j < TELL - 4 else 7
    pts = []
    for i in range(0, 61):
        t = i / 60
        y = BUTT[1] - (BUTT[1] - top) * t
        x = BUTT[0] + (TIP[0] - BUTT[0]) * ((BUTT[1] - y) / (BUTT[1] - TIP[1]))
        ph = t * 2.5 * 2 * math.pi + j * 0.9
        pts.append((x + r * math.cos(ph), y + r * 0.35 * math.sin(ph), math.sin(ph) > 0))
    return [(a, b) for a, b in zip(pts, pts[1:])]


def splash(d, x, y, seed, size):
    rnd = random.Random(seed)
    for a in (-160, -135, -110, -90, -70, -45, -20):
        r = math.radians(a + rnd.uniform(-8, 8))
        L = size * rnd.uniform(0.6, 1.1)
        tip = (x + math.cos(r) * L, y + math.sin(r) * L)
        nx, ny = -math.sin(r) * size * 0.15, math.cos(r) * size * 0.15
        d.polygon([(round(x + nx), round(y + ny)), (round(tip[0]), round(tip[1])), (round(x - nx), round(y - ny))], fill=FOAM, outline=DEEP)
        d.line([(round(x), round(y)), (round(x + math.cos(r) * L * 0.55), round(y + math.sin(r) * L * 0.55))], fill=WHITE)
    fx.ell(d, x, y, size * 0.7, size * 0.22, outline=FOAM, width=2)


class Rain:
    def __init__(self, seed):
        self.rnd = random.Random(seed)
        self.drops, self.rings = [], []

    def add(self, x, y, n, speed, up=2.0):
        for _ in range(n):
            a = self.rnd.uniform(0, 2 * math.pi)
            v = speed * self.rnd.uniform(0.3, 1.0)
            ground = max(y + 10, self.rnd.uniform(430, 740)) if y < 430 else y + self.rnd.uniform(15, 60)
            self.drops.append(dict(x=x + self.rnd.uniform(-6, 6), y=y, vx=math.cos(a) * v, vy=math.sin(a) * v * 0.6 - up,
                                   r=self.rnd.uniform(1.5, 3.0), ground=min(GROUND + 30, ground)))

    def step(self):
        keep = []
        for p in self.drops:
            p["vy"] += 2.0
            p["x"] += p["vx"]
            p["y"] += p["vy"]
            if p["y"] >= p["ground"]:
                self.rings.append([p["x"], p["ground"], 2.0, 0])
            else:
                keep.append(p)
        self.drops = keep
        for r in self.rings:
            r[2] += 2.0
            r[3] += 1
        self.rings = [r for r in self.rings if r[3] < 3]

    def draw_air(self, d):
        for p in self.drops:
            r = p["r"]
            d.line([(round(p["x"]), round(p["y"] - r * 2.2)), (round(p["x"]), round(p["y"]))], fill=LIGHT)
            fx.ell(d, p["x"], p["y"], r * 0.8, r, fill=CYAN)
            d.point((round(p["x"]), round(p["y"] - r * 0.5)), fill=WHITE)

    def draw_ground(self, d):
        for x, y, r, age in self.rings:
            fx.ell(d, x, y, r, r * 0.35, outline=FOAM if age < 2 else LIGHT)


def serpent_points(s_snout, travel):
    s_neck = s_snout - HEAD_LEN
    s_tail = max(0.0, s_neck - BODY_LEN)
    n = max(2, int((s_neck - s_tail) / 6))
    pts = [at(s_tail + (s_neck - s_tail) * i / n)[:2] for i in range(n + 1)]
    sx, sy, _ = at(s_snout)
    neck = (sx - HEAD_LEN * math.cos(math.radians(travel)), sy - HEAD_LEN * math.sin(math.radians(travel)))
    pts.append(neck)
    return pts


def ease(x):
    x = max(0.0, min(1.0, x))
    return 1 - (1 - x) ** 2


LAST = {}


CHIEF = [u for u in stage.units if u["side"] == "them" and u["x"] > 1100][0]


def frame(f, rain):
    k = f - REL
    mods, shake = {}, (0, 0)
    state = None
    if 1 <= k <= 4:
        s = ease(k / 4) * L_FLIGHT
        state = ("shut", s, at(s)[2], 1.0)
    elif 5 <= k <= 7:
        a0, a1 = at(L_FLIGHT - 1)[2], at(L_FLIGHT + 6)[2]
        state = ("open", L_FLIGHT, a0 + (a1 - a0) * (k - 5) / 2, (1.15, 1.0, 1.0)[k - 5])
    elif 8 <= k <= 10:
        s = L_FLIGHT + (L_TOTAL - L_FLIGHT) * (k - 7) / 3
        state = ("open", s, at(s)[2], 1.0)
    for n, u in enumerate(BACKROW):
        j = k - (8 + n)
        t = {0: (255, 250, 240), 1: (224, 40, 30), 3: (224, 40, 30)}.get(j)
        if t:
            mods[u["i"]] = dict(tint=t, dx={0: -2, 1: -3}.get(j, 0))
        elif j == 2:
            mods[u["i"]] = dict(dx=-1)
        if j == 0:
            rain.add(u["x"], u["y"] - 4, 10, 9, up=8)
    if 8 <= k <= 10:
        shake = ((3, -2), (-3, 2), (3, -1))[k - 8]
    if k == 11:
        shake = (2, -1)
        for x, y in LAST.get("pts", [])[::2]:
            rain.add(x, y, 1, 4, up=1)
        sx, sy = LAST.get("snout", P2)
        rain.add(sx, min(sy, GROUND), 14, 10, up=7)
    rain.step()

    def before(layer, u):
        if u is CHIEF and L0 <= f < REL:
            d = ImageDraw.Draw(layer)
            for (x0, y0, fr0), (x1, y1, fr1) in coil_segments(f - L0):
                if not fr0:
                    d.line([(round(x0), round(y0)), (round(x1), round(y1))], fill=DEEP, width=3)

    def between(layer, u):
        if u is CHIEF and L0 <= f < REL:
            d = ImageDraw.Draw(layer)
            for (x0, y0, fr0), (x1, y1, fr1) in coil_segments(f - L0):
                if fr0:
                    d.line([(round(x0), round(y0)), (round(x1), round(y1))], fill=CYAN, width=3)
                    d.point((round(x0), round(y0) - 1), fill=WHITE)

    def under(layer):
        d = ImageDraw.Draw(layer)
        for n, u in enumerate(BACKROW):
            j = k - (8 + n)
            if j >= 0:
                rx = 30 - max(0, k - 14) * 3
                if rx > 3:
                    fx.ell(d, u["x"], u["y"] + 1, rx, rx * 0.3, fill=DEEP, outline=LIGHT)
        rain.draw_ground(d)

    def over(layer):
        d = ImageDraw.Draw(layer)
        if L0 <= f < REL:
            j = f - L0
            r = 4 + (j % 4 in (0, 1)) * 2 + 2 * j / TELL
            fx.ell(d, TIP[0], TIP[1], r, r, fill=CYAN)
            fx.ell(d, TIP[0], TIP[1], r * 0.45, r * 0.45, fill=WHITE)
        if k == 0:
            fx.ell(d, TIP[0], TIP[1], 13, 13, fill=OUTW)
        if k == 1:
            fx.ell(d, TIP[0], TIP[1], 14, 14, fill=WHITE)
            for a in (0, 90, 180, 270):
                r = math.radians(a + 45)
                d.line([(TIP[0], TIP[1]), (round(TIP[0] + 30 * math.cos(r)), round(TIP[1] + 30 * math.sin(r)))], fill=WHITE, width=3)
        if state:
            which, s, travel, sc = state
            tmp = Image.new("RGBA", layer.size, (0, 0, 0, 0))
            td = ImageDraw.Draw(tmp)
            pts = serpent_points(s, travel)
            water_ribbon(td, pts, 30, 8, k)
            if s - HEAD_LEN - BODY_LEN > 0:
                tx, ty = pts[0]
                for q in range(3):
                    fx.ell(td, tx + (q - 1) * 6, ty + (q % 2) * 3, 4 - q, 4 - q, fill=FOAM)
            spr, piv = head(which, travel, sc)
            sx, sy, _ = at(s)
            fx.place(tmp, spr, piv, sx, sy)
            if k >= 8:
                a = np.asarray(tmp).copy()
                a[GROUND:, 340:520, 3] = 0
                tmp = Image.fromarray(a, "RGBA")
            layer.alpha_composite(tmp)
            LAST["pts"], LAST["snout"] = pts, (sx, sy)
        for n, u in enumerate(BACKROW):
            j = k - (8 + n)
            if j == 0:
                splash(d, u["x"], u["y"], 400 + n, 46)
            if j == 1:
                splash(d, u["x"], u["y"], 410 + n, 26)
        if k == 11:
            splash(d, P2[0], GROUND, 499, 54)
        rain.draw_air(d)

    return stage.render(under=under, over=over, before=before, between=between, mods=mods, shake=shake, t_ms=f * fx.FRAME_MS)


if __name__ == "__main__":
    import sys
    import mp4kit
    rain = Rain(9)
    N = REL + 26
    frames = [frame(f, rain) for f in range(N)]
    pick = [L0 + 4, L0 + 12, REL, REL + 1, REL + 2, REL + 4, REL + 5, REL + 7, REL + 8, REL + 9, REL + 10, REL + 11, REL + 13, REL + 18]
    fx.strip(frames, pick, (330, 180, 1260, 780), "T-strip.png", scale=1, cols=7)
    if "check" in sys.argv:
        sys.exit()
    out = fx.STUDY.parent / "tide" / "C-water-serpent"
    out.mkdir(parents=True, exist_ok=True)
    mp4kit.mp4([fr.crop((150, 160, 1300, 800)) for fr in frames], out / "tide-C-water-serpent.mp4")
    mp4kit.mp4([fr.crop((300, 230, 700, 770)).resize((800, 1080)) for fr in frames], out / "tide-C-water-serpent-2x-your-back-row.mp4")
    print("frames", N, out, "head len", round(HEAD_LEN), "flight", round(L_FLIGHT), "total", round(L_TOTAL))
