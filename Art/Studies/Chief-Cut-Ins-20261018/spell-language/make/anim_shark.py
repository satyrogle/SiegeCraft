"""Dust Shark, animated from its strip: the Clarth sand mark snaps in under each fighter in their back row and turns
through the 3 s tell while a fin circles inside it (a blow on your side gets eaten by sand), the last second blazes,
then one at a time, top to bottom, a dark hole, the shark (take A, the leaper) bursts out of the mark, arcs over
them, bites, dives back in on the far side, and the mark breaks into sand."""
import math
import random

from PIL import ImageDraw

import anim as A
import art
import capkit as ck
import fxkit as fx
import sceneA as SA

st = ck.hunt_stage()
MARK = A.Mark("mark-clarth.png", 128, fx.PAL, steps=8, sym=90.0)
T = []
for n, u in enumerate([u for u in st.units if u["i"] in (102, 104, 105)]):
    h, cx = art.unit_height(u["name"])
    x = u["x"] + cx
    T.append(dict(u=u, n=n, x=x, feet=u["y"], h=h, bx=x - 52, dx=x + 52))
REL, END = 38, 70
bits = fx.Bits(23)
BREAK = {}
SAND = (fx.S7, fx.S5, fx.S2)


def k_of(t, f):
    return f - (REL + 2 * t["n"]) + 6


def fin_pos(t, f):
    a = math.radians(f * 20 + t["n"] * 120)
    return t["x"] + 0.62 * MARK.rx * math.cos(a), t["feet"] + 2 + 0.62 * MARK.ry * math.sin(a), math.sin(a) >= 0


def draw_shark(layer, t, k):
    key, (px, py) = SA.pivot_for(t, k)
    spr, piv = SA.POSE[key]
    fx.place(layer, spr, piv, px, py, clip=SA.ground_clip(t))
    d = ImageDraw.Draw(layer)
    if 7 <= k <= 8:
        fx.front_lip(d, t["bx"], t["feet"], *SA.HR)
    if 14 <= k <= 15:
        fx.front_lip(d, t["dx"], t["feet"], *SA.HR)


def frame(f):
    mods, shake = {}, [0, 0]
    for t in T:
        k = k_of(t, f)
        if k == 7:
            bits.chunks(t["bx"], t["feet"] - 3, t["feet"], 14, 12, spread=0.8, aim=-75)
            bits.dust(t["bx"], t["feet"] - 2, 3, spread=16, rise=0.9, size=(3, 6))
        if k in (8, 9):
            kx, ky = SA.pivot_for(t, k)[1]
            bits.chunks(kx - 10, ky + 6, t["feet"], 4, 2, spread=1.0, aim=90)
        if k == 15:
            bits.chunks(t["dx"], t["feet"] - 3, t["feet"], 12, 11, spread=0.8, aim=-90)
            bits.dust(t["dx"], t["feet"] - 2, 3, spread=16, rise=0.9, size=(3, 6))
        if 10 <= k <= 12:
            sx, sy = {10: (5, -3), 11: (-4, 2), 12: (2, -1)}[k]
            shake[0] += sx
            shake[1] += sy
        tint = {10: A.WHITE, 11: A.RED, 13: A.WHITE, 14: A.RED}.get(k)
        if tint or k in (13, 14):
            mods[t["u"]["i"]] = dict(tint=tint, dy={13: -3, 14: -1}.get(k, 0))
    if f == 17:
        bits.dust(628, 378, 3, spread=6, rise=0.6, size=(4, 7))
    bits.step()
    shake = tuple(max(-5, min(5, v)) for v in shake)

    def under(layer):
        d = ImageDraw.Draw(layer)
        for t in T:
            k = k_of(t, f)
            mx, my = t["x"], t["feet"] + 2
            if f < REL:
                A.mark_tell(layer, MARK, mx, my, f, fx.S7, spin=2)
            elif k < 16:
                spr = MARK.at(f // 2)
                spr = A.shade(spr, 0.35) if k == 6 else A.lift(spr, fx.S7, 0.3)
                A.put(layer, spr, mx, my)
            if k == 6:
                fx.ell(d, t["bx"], t["feet"], SA.HR[0] + 3, SA.HR[1] + 1, fill=fx.OUT)
            if 7 <= k <= 15:
                fx.hole(d, t["bx"], t["feet"], *SA.HR)
            if 14 <= k <= 16:
                fx.hole(d, t["dx"], t["feet"], *SA.HR)
            if k >= 16:
                fx.crater(d, t["bx"], t["feet"], SA.HR[0], SA.HR[1], dark=k < 22)
            if k >= 17:
                fx.crater(d, t["dx"], t["feet"], SA.HR[0], SA.HR[1], dark=k < 23)
            if k == 16:
                BREAK[t["n"]] = A.Breakup(MARK.at(f // 2), mx, my, 50 + t["n"], keep=0.25, speed=2.2, up=1.6, g=0.35)
            if t["n"] in BREAK:
                BREAK[t["n"]].draw(d, k - 16)
        bits.draw(d, kinds=("dust", "speck"))

    def before(layer, u):
        d = ImageDraw.Draw(layer)
        for t in T:
            if t["u"] is not u:
                continue
            k = k_of(t, f)
            if 4 <= f < REL:
                x, y, front = fin_pos(t, f)
                if not front:
                    SA.fin(d, x, y)
            if k in (7, 8, 9, 13, 14, 15):
                draw_shark(layer, t, k)

    def between(layer, u):
        d = ImageDraw.Draw(layer)
        for t in T:
            if t["u"] is not u:
                continue
            k = k_of(t, f)
            if 4 <= f < REL:
                x, y, front = fin_pos(t, f)
                if front:
                    SA.fin(d, x, y)
                rr = random.Random(f * 7 + t["n"])
                for j in range(2):
                    a = rr.uniform(0, 2 * math.pi)
                    sx = t["x"] + MARK.rx * math.cos(a)
                    sy = t["feet"] + 2 + MARK.ry * math.sin(a) - rr.uniform(4, 14)
                    d.rectangle((round(sx), round(sy), round(sx) + 1, round(sy) + 1), fill=fx.S6)
            if 10 <= k <= 12:
                draw_shark(layer, t, k)

    def over(layer):
        d = ImageDraw.Draw(layer)
        if 12 <= f <= 16:
            p = (f - 12) / 4
            x, y = 1000 + (628 - 1000) * p, 382 + (378 - 382) * p
            d.line([(round(x + 16), round(y - 2)), (round(x), round(y))], fill=(40, 30, 48), width=2)
            d.point((round(x), round(y)), fill=(230, 230, 240))
        if 17 <= f <= 19:
            j = f - 17
            d.line([(round(628 + 4), round(378 + 6 * j - 6)), (round(628 - 6), round(378 + 6 * j + 4))], fill=(40, 30, 48), width=2)
        for t in T:
            k = k_of(t, f)
            if k == 7:
                A.spikes(d, t["bx"] - 4, t["feet"], 300 + t["n"], 44, SAND, dirs=(-170, -148, -125, -100))
            if k == 15:
                A.spikes(d, t["dx"], t["feet"], 320 + t["n"], 36, SAND, dirs=(-160, -130, -50, -20))
        bits.draw(d, kinds=("chunk",))

    return st.render(under=under, over=over, before=before, between=between, mods=mods, shake=shake, t_ms=f * A.MS)


if __name__ == "__main__":
    frames = [frame(f) for f in range(END)]
    out = A.save(frames, "dust-shark", "dust-shark-anim", (560, 240, 1300, 800), (900, 300, 1160, 760),
                 [2, 10, 20, 30, 38, 39, 40, 42, 44, 46, 48, 52, 56, 62])
    print("frames", len(frames), "to", out)
