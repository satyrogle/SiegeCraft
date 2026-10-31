"""Tide, animated from its strip: a whirlpool mark opens under each fighter in your back row and spins through the
3 s tell, bubbling, the wet sand spreading, while water coils up the Thundercoast chief's harpoon; a dark frame, a
wave rears up out of each whirlpool right behind them and hangs at the top of its curl, crashes down on them (white,
red, red, a splash crown, a shake), floods round their feet and drains back into the whirlpools, which spin shut."""
import math
import random

import numpy as np
from PIL import Image, ImageDraw

import anim as A
import fxkit as fx
import sceneT as T

WHITE = A.WHITE
FOAM, CYAN, DEEP, LIGHT, OUTW = T.FOAM, T.CYAN, T.DEEP, T.LIGHT, T.OUTW
st = T.stage
YOU = [u for u in st.units if u["side"] == "you"]
BACK = sorted([u for u in YOU if 380 < u["x"] < 460], key=lambda u: u["y"])
CHIEF = T.CHIEF
MARK = A.Mark("mark-thundercoast.png", 124, T.TCPAL, steps=8, sym=45.0)
RX, RY = MARK.rx * 1.15, MARK.ry * 1.15
SLOPE = 1.7
REL, RISE, CRASH, DRAIN, GONE, END = 38, 39, 44, 48, 58, 64
wsrc = fx.key(A.RAW / "hero-wave.png", tol=60)
wsrc = wsrc.crop((0, 0, wsrc.width, round(wsrc.height * 0.86)))
for u in BACK:
    a = np.asarray(u["spr"])[..., 3] > 0
    ys = np.nonzero(a)[0]
    u["H"] = max(80, u["y"] - (u["box"][1] + ys.min()))
    h = round(u["H"] * 1.6)
    u["wave"] = fx.pixelize(wsrc.resize((round(h * wsrc.width / wsrc.height), h), Image.BOX), outline=OUTW, pal=T.TCPAL)
    u["crash"] = A.scaled(u["wave"], 1.08, 0.55)
    u["flat"] = A.scaled(u["wave"], 1.16, 0.3)


def base_y(xs, mx, my):
    dx = np.abs(xs - mx)
    inside = dx <= RX
    return np.where(inside, my + RY * np.sqrt(np.clip(1 - (dx / RX) ** 2, 0, 1)), my - (dx - RX) * SLOPE)


def mound_clip(mx, my):
    def clip(xs, ys):
        return ys < base_y(xs, mx, my)
    return clip


def wave_on(layer, u, spr, dy=0.0):
    mx, my = u["x"], u["y"] + 2
    y0 = my + RY * 0.6 - spr.height + dy
    fx.place(layer, spr, (0, 0), mx - 0.6 * spr.width, y0, clip=mound_clip(mx, my))
    return y0


def spray(d, mx, my, seed, top):
    rnd = random.Random(seed)
    pts = [(mx + RX * math.cos(math.radians(t)), my + RY * math.sin(math.radians(t))) for t in range(0, 181, 6)]
    d.line([(round(x), round(y)) for x, y in pts], fill=FOAM, width=3)
    d.line([(round(x), round(y) - 1) for x, y in pts[2:-2]], fill=WHITE, width=1)
    for side in (-1, 1):
        for j in range(16):
            ex = rnd.uniform(0, 60)
            x = mx + side * (RX + ex)
            y = my - ex * SLOPE
            if y < top:
                continue
            fx.ell(d, x + rnd.uniform(-3, 3), y + rnd.uniform(-2, 2), rnd.uniform(1.5, 3.2), rnd.uniform(1.5, 3.2),
                   fill=FOAM if j % 3 else WHITE)


def crown(d, x, y, seed, size):
    rnd = random.Random(seed)
    for a in (-170, -150, -128, -108, -90, -72, -52, -30, -10):
        r = math.radians(a + rnd.uniform(-6, 6))
        L = size * rnd.uniform(0.6, 1.15)
        tip = (x + math.cos(r) * L, y + math.sin(r) * L)
        nx, ny = -math.sin(r) * size * 0.13, math.cos(r) * size * 0.13
        d.polygon([(round(x + nx), round(y + ny)), (round(tip[0]), round(tip[1])), (round(x - nx), round(y - ny))], fill=FOAM, outline=DEEP)
        d.line([(round(x), round(y)), (round(x + math.cos(r) * L * 0.55), round(y + math.sin(r) * L * 0.55))], fill=WHITE)
        fx.ell(d, tip[0], tip[1] - 2, 2.5, 2.5, fill=FOAM)


def bubbles(d, mx, my, f, n):
    for b in range(n):
        cyc, age = divmod(f + b, 3)
        rnd = random.Random(int(mx) * 1000 + int(my) * 7 + b * 31 + cyc * 977)
        a = rnd.uniform(0, 2 * math.pi)
        rr = rnd.uniform(0.2, 0.85)
        x = mx + MARK.rx * rr * math.cos(a)
        y = my + MARK.ry * rr * math.sin(a) - age * 2
        if age < 2:
            fx.ell(d, x, y, 1.5 + age, 1.5 + age, outline=FOAM)
        else:
            d.point((round(x - 2), round(y - 1)), fill=FOAM)
            d.point((round(x + 2), round(y - 2)), fill=WHITE)


def foam_drops(d, mx, my, k, seed):
    rnd = random.Random(seed)
    for b in range(8):
        a = rnd.uniform(math.pi * 1.1, math.pi * 1.9)
        v = rnd.uniform(2.5, 4.5)
        x = mx + math.cos(a) * v * k * 2
        y = my + math.sin(a) * v * k * 1.4 + 0.8 * k * k
        if k < 6 and y <= my + 8:
            fx.ell(d, x, y, 1.5, 1.8, fill=FOAM)


def coil(layer, f, front):
    d = ImageDraw.Draw(layer)
    for (x0, y0, fr0), (x1, y1, fr1) in T.coil_segments(min(12, f // 3)):
        if fr0 == front:
            d.line([(round(x0), round(y0)), (round(x1), round(y1))], fill=CYAN if front else DEEP, width=3)
            if front:
                d.point((round(x0), round(y0) - 1), fill=WHITE)


def frame(f):
    mods, shake = {}, (0, 0)
    j = f - CRASH
    if j >= 0:
        t = A.hit_tint(j)
        for u in BACK:
            m = {}
            if t:
                m["tint"] = t
            if j < 3:
                m["dy"] = 2
            if m:
                mods[u["i"]] = m
        shake = A.hit_shake(j)
    tops = {}

    def under(layer):
        d = ImageDraw.Draw(layer)
        for n, u in enumerate(BACK):
            mx, my = u["x"], u["y"] + 2
            if f < REL:
                p = min(1.0, f / 30.0)
                rw = 30 + 36 * p
                fx.ell(d, mx, my + 1, rw, rw * 0.35, fill=(round(150 - 30 * p), round(92 - 12 * p), 52))
                A.mark_tell(layer, MARK, mx, my, f, CYAN, spin=1 if f >= A.GLOW_FROM else 2)
                if f >= 8:
                    bubbles(d, mx, my, f, 6 if f >= A.GLOW_FROM else 3)
            elif f == REL:
                fx.ell(d, mx, my + 1, 66, 23, fill=(120, 80, 52))
                A.put(layer, A.flat(MARK.at(f), OUTW), mx, my)
            elif f < DRAIN:
                rr = 74 if f < CRASH + 3 else 80
                fx.ell(d, mx, my + 1, rr, rr * 0.36, fill=DEEP, outline=LIGHT)
                A.put(layer, A.lift(MARK.at(f), CYAN, 0.35), mx, my)
            elif f < GONE:
                k = f - DRAIN
                rr = 80 - 8 * k
                if rr > 3:
                    fx.ell(d, mx, my + 1, rr, rr * 0.36, fill=DEEP, outline=LIGHT)
                A.put(layer, A.scaled(MARK.at(f * 2), max(0.2, 1.0 - 0.08 * k)), mx, my)
            if f == GONE:
                A.spikes(d, mx, my, 90 + n, 14, (WHITE, FOAM, DEEP), dirs=(-150, -110, -70, -30))

    def before(layer, u):
        if u is CHIEF and f < REL:
            coil(layer, f, False)
        if u in BACK and RISE <= f < CRASH:
            dy = {RISE: u["wave"].height * 0.5, RISE + 1: -8.0}.get(f, 0.0)
            tops[u["i"]] = wave_on(layer, u, u["wave"], dy)

    def between(layer, u):
        d = ImageDraw.Draw(layer)
        if u is CHIEF and f < REL:
            coil(layer, f, True)
            if f >= 20:
                r = 4 + ((f // 4) % 2) * 2 + (2 if f >= A.GLOW_FROM else 0)
                fx.ell(d, T.TIP[0], T.TIP[1], r, r, fill=CYAN)
                fx.ell(d, T.TIP[0], T.TIP[1], r * 0.45, r * 0.45, fill=WHITE)
        if u in BACK and RISE <= f < CRASH:
            spray(d, u["x"], u["y"] + 2, u["y"] + f, tops[u["i"]] + 20)

    def over(layer):
        d = ImageDraw.Draw(layer)
        if f == REL:
            fx.ell(d, T.TIP[0], T.TIP[1], 12, 12, fill=OUTW)
        if f == RISE:
            fx.ell(d, T.TIP[0], T.TIP[1], 14, 14, fill=WHITE)
            for a in (0, 90, 180, 270):
                r = math.radians(a + 45)
                d.line([(T.TIP[0], T.TIP[1]), (round(T.TIP[0] + 30 * math.cos(r)), round(T.TIP[1] + 30 * math.sin(r)))],
                       fill=WHITE, width=3)
        for n, u in enumerate(BACK):
            mx, my = u["x"], u["y"] + 2
            if f in (CRASH, CRASH + 1):
                top = wave_on(layer, u, u["crash"], 0 if f == CRASH else 6)
                spray(d, mx, my, u["y"] + f, top + 10)
                crown(d, mx + 8, my, 400 + n + f, 50 if f == CRASH else 30)
            if f == CRASH + 2:
                top = wave_on(layer, u, u["flat"], 0)
                spray(d, mx, my, u["y"] + f, top + 6)
            if CRASH + 3 <= f < DRAIN + 3:
                rnd = random.Random(f * 13 + n)
                for b in range(6):
                    fx.ell(d, mx - 34 + 13 * b + rnd.uniform(-2, 2), my - 8 - (b % 3) * 6 - rnd.uniform(0, 4), 2, 2.5, fill=FOAM)
            if GONE <= f < GONE + 6:
                foam_drops(d, mx, my, f - GONE, 80 + n)

    return st.render(under=under, over=over, before=before, between=between, mods=mods, shake=shake)


if __name__ == "__main__":
    frames = [frame(f) for f in range(END)]
    out = A.save(frames, "tide", "tide-anim", (150, 160, 1300, 800), (250, 210, 780, 760),
                 [4, 20, 34, 38, 39, 42, 44, 47, 54])
    fx.strip(frames, [20, 36, 39, 44], (300, 250, 1260, 760), str(out / "tide-anim-check-wide.png"), scale=1, cols=2)
    print("frames", len(frames), "to", out)
