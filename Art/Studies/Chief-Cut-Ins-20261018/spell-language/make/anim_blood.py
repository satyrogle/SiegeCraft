"""Blood Call, animated from its strip: a blood mark opens under each bleeding fighter of yours and turns through the
3 s tell while their wounds beat red and blood beads and lifts out of them, the priestess's bowl pulsing; a dark
frame, a huge hand of blood bursts up out of each mark behind them (white, red, red, a shake), clenches and tears the
blood out of their wounds in a stream into its palm, then the hands burst into drops that rain down and splat, and
the pools soak into the sand. Nothing goes off at her staff."""
import math
import random

import numpy as np
from PIL import Image, ImageDraw

import anim as A
import capkit as ck
import fxkit as fx

C0, C1, C2, C3, C4 = fx.rgb("#440f15"), fx.rgb("#831118"), fx.rgb("#d11b1b"), fx.rgb("#d95447"), fx.rgb("#f5ada3")
st = ck.Stage("stoneveil", "sand")
YOU = [u for u in st.units if u["side"] == "you"]
BLEED = [u for u in YOU if (u["x"], u["y"]) in ((415, 422), (420, 562), (600, 702))]
BOWL = (1199, 455)
PAL = A.palette(["stoneveil_chief_sw_idle"], (C0, C1, C2, C3, C4))
MARK = A.Mark("mark-stoneveil.png", 120, PAL, steps=8, sym=45.0)
csrc = fx.key(A.RAW / "hero-blood-claw.png", tol=60)
REL, RIP, BURST, END = 38, 42, 49, 64
for u in BLEED:
    a = np.asarray(u["spr"])[..., 3] > 0
    ys = np.nonzero(a)[0]
    true_h = u["y"] - (u["box"][1] + ys.min())
    u["H"] = max(70, true_h)
    u["wound"] = (u["x"] + 4, u["y"] - 0.5 * true_h)
    h = round(u["H"] * 1.9)
    u["claw"] = fx.pixelize(csrc.resize((round(h * csrc.width / csrc.height), h), Image.BOX), outline=fx.OUT, pal=PAL)
    u["tall"] = A.scaled(u["claw"], 0.96, 1.1)
    u["fist"] = A.scaled(u["claw"], 1.06, 0.9)
    u["rimimg"] = A.rim(u["spr"], C2)
    u["rimdark"] = A.rim(u["spr"], C0)
    u["cx"] = u["x"] - 16
    fw, fh = u["fist"].size
    u["palm"] = (u["cx"] - fw / 2 + 0.55 * fw, u["y"] + 8 - fh + 0.32 * fh)


def drop(d, x, y, r):
    fx.ell(d, x, y, r + 1, r * 1.25 + 1, fill=C0)
    fx.ell(d, x, y, r, r * 1.25, fill=C2)
    d.point((round(x - r * 0.3), round(y - r * 0.5)), fill=C4)


def splash(d, x, y, seed, size):
    rnd = random.Random(seed)
    for a in range(-170, 11, 26):
        r = math.radians(a + rnd.uniform(-9, 9))
        L = size * rnd.uniform(0.6, 1.1)
        tip = (x + math.cos(r) * L, y + math.sin(r) * L * 0.8)
        nx, ny = -math.sin(r) * size * 0.15, math.cos(r) * size * 0.15
        d.polygon([(round(x + nx), round(y + ny)), (round(tip[0]), round(tip[1])), (round(x - nx), round(y - ny))], fill=C2, outline=C0)
        fx.ell(d, tip[0], tip[1], 2.5, 3, fill=C2)


def rising(d, u, f):
    """Blood beads lift out of the wound and pop: one drop every 6 frames, every 3 in the last second."""
    wx, wy = u["wound"]
    every = 3 if f >= A.GLOW_FROM else 6
    for k in range(2):
        born = f - (f % every) - k * every
        age = f - born
        if born < 12 or age > 5:
            continue
        x = wx + 4 * math.sin(born * 1.3 + age * 0.9)
        y = wy - 6 - 4 * age
        if age == 5:
            d.point((round(x - 2), round(y)), fill=C2)
            d.point((round(x + 2), round(y - 1)), fill=C2)
        else:
            drop(d, x, y, 2.6 - 0.25 * age)


def stream(d, u, f):
    """The rip: drops pulled out of the wound in an arc into the hand's palm."""
    wx, wy = u["wound"]
    px, py = u["palm"]
    for jd in range(10):
        t = (jd / 10.0 + (f - RIP) * 0.07) % 1.0
        x = wx + (px - wx) * t + 5 * math.sin(jd * 1.9 + f * 0.5)
        y = wy + (py - wy) * t - 18 * math.sin(math.pi * t)
        drop(d, x, y, 3.2 - 1.4 * t)


class Shower:
    """Drops thrown out of a bursting hand: they arc up, fall, and splat on the sand."""

    def __init__(self, origin, ground, seed, n=10):
        rnd = random.Random(seed)
        self.drops = []
        for _ in range(n):
            a = rnd.uniform(math.pi * 1.05, math.pi * 1.95)
            v = rnd.uniform(3.0, 6.5)
            vx, vy = math.cos(a) * v, math.sin(a) * v * 0.8
            gy = ground + rnd.uniform(-6, 10)
            land = next((k for k in range(1, 40) if origin[1] + vy * k + 0.8 * k * k >= gy), 39)
            self.drops.append((origin[0], origin[1], vx, vy, gy, rnd.uniform(2.0, 3.2), land))

    def draw_air(self, d, k):
        for x0, y0, vx, vy, gy, r, land in self.drops:
            if 0 <= k < land:
                drop(d, x0 + vx * k, y0 + vy * k + 0.8 * k * k, r)

    def draw_ground(self, d, k):
        for x0, y0, vx, vy, gy, r, land in self.drops:
            s = k - land
            if 0 <= s < 5:
                rr = r + 2 - 0.5 * s
                fx.ell(d, x0 + vx * land, gy, rr, rr * 0.35, fill=C1)


BRK, SHOWERS = {}, {}


def frame(f):
    mods, shake = {}, (0, 0)
    j = f - (REL + 1)
    for u in BLEED:
        m = {}
        t = A.hit_tint(j) if j >= 0 else None
        if t:
            m["tint"] = t
        if RIP + 1 <= f < BURST:
            m["dx"] = 1 if f % 2 else -1
        if m:
            mods[u["i"]] = m
    if 0 <= j <= 2:
        shake = A.hit_shake(j)

    def under(layer):
        d = ImageDraw.Draw(layer)
        for n, u in enumerate(BLEED):
            mx, my = u["x"], u["y"] + 2
            if f < REL:
                A.mark_tell(layer, MARK, mx, my, f, C4, spin=3)
            elif f == REL:
                A.put(layer, A.flat(MARK.at(f // 3), C0), mx, my)
            elif f < BURST:
                A.put(layer, A.lift(MARK.at(f // 3), C4, 0.3), mx, my)
            else:
                rx = MARK.rx - 8 - 4 * max(0, f - BURST - 3)
                if rx > 2:
                    fx.ell(d, mx, my, rx, rx * 0.36, fill=C1)
                    if rx > 10:
                        fx.ell(d, mx, my, rx - 8, (rx - 8) * 0.34, fill=C0)
            if n in SHOWERS:
                SHOWERS[n].draw_ground(d, f - BURST)

    def before(layer, u):
        if u not in BLEED:
            return
        if f == REL + 1:
            A.put_feet(layer, u["claw"], u["cx"], u["y"] + 8 + 0.45 * u["claw"].height, clip_below=u["y"] + 8)
        elif f == REL + 2:
            A.put_feet(layer, u["tall"], u["cx"], u["y"] + 8)
        elif f == REL + 3:
            A.put_feet(layer, u["claw"], u["cx"], u["y"] + 8)
        elif RIP <= f < BURST:
            A.put_feet(layer, u["fist"], u["cx"], u["y"] + 8)

    def between(layer, u):
        if u not in BLEED:
            return
        d = ImageDraw.Draw(layer)
        x0, y0 = u["box"][0] + mods.get(u["i"], {}).get("dx", 0), u["box"][1]
        if f < REL:
            beat = (f // 4) % 2 == 0 if f < A.GLOW_FROM else f % 2 == 0
            if beat:
                layer.alpha_composite(u["rimimg"], (x0, y0))
            if f >= 4:
                wx, wy = u["wound"]
                drop(d, wx, wy, min(3.2, 1.6 + 0.08 * (f - 4)) + (0.6 if beat else 0.0))
            if f >= 12:
                rising(d, u, f)
        elif f == REL:
            layer.alpha_composite(u["rimdark"], (x0, y0))
        if f == REL + 1:
            splash(d, u["cx"], u["y"] + 6, int(u["x"] + u["y"]), 30)
        if f == REL + 2:
            splash(d, u["cx"], u["y"] + 6, int(u["x"] + u["y"]) + 1, 18)
        if RIP <= f < BURST:
            if f % 2 == 0:
                layer.alpha_composite(u["rimimg"], (x0, y0))
            stream(d, u, f)

    def over(layer):
        d = ImageDraw.Draw(layer)
        if f < REL:
            r = 9 + ((f // 4) % 2) * 2 + (2 if f >= A.GLOW_FROM else 0)
            fx.ell(d, BOWL[0], BOWL[1] - 2, r, r * 0.78, outline=C2, width=2)
        for n, u in enumerate(BLEED):
            if f == BURST:
                fist = u["fist"]
                BRK[n] = A.Breakup(fist, u["cx"], u["y"] + 8 - fist.height / 2, 40 + n, keep=0.16, speed=1.3, up=0.4,
                                   g=0.7, life=(5, 10))
                SHOWERS[n] = Shower(u["palm"], u["y"] + 2, 50 + n)
                splash(d, u["palm"][0], u["palm"][1], 60 + n, 26)
            if n in BRK:
                BRK[n].draw(d, f - BURST)
            if n in SHOWERS:
                SHOWERS[n].draw_air(d, f - BURST)

    return st.render(under=under, over=over, before=before, between=between, mods=mods, shake=shake)


if __name__ == "__main__":
    frames = [frame(f) for f in range(END)]
    out = A.save(frames, "blood-call", "blood-call-anim", (150, 160, 1300, 760), (290, 200, 760, 760),
                 [4, 16, 30, 38, 39, 40, 42, 45, 49, 51, 55, 60])
    print("frames", len(frames), "to", out)
