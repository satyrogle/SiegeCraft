"""Firebomb, animated from its strip: the hunter mark locks onto your front row (it closes in, then turns and beats
through the 3 s tell) while the captain holds a lit firebomb on his spear tip, cocks it back in the last second, flings
it in a high arc, a dark frame, it bursts between your two front fighters, fire races along the row into both marks,
burns, then dies to embers and smoke and leaves scorch marks."""
import math
import random

from PIL import ImageDraw

import anim as A
import capkit as ck
import fxkit as fx

HOT = [(255, 241, 255), (230, 179, 255), (177, 108, 240), (131, 69, 191), (77, 40, 134), (40, 18, 77)]
SMOKE = [(106, 90, 120), (70, 58, 85), (42, 34, 54)]
SCORCH = [(60, 44, 52), (40, 30, 40)]
st = ck.hunt_stage()
CAPT = [u for u in st.units if u["i"] == 106][0]
FRONT = sorted([u for u in st.units if u["side"] == "you" and u["x"] > 560], key=lambda u: u["y"])
PAL = A.palette(["hunter_captain_sw_idle", "hunter_marksman_sw_idle"], HOT)
MARK = A.Mark("mark-hunters.png", 124, PAL, steps=8, sym=45.0)
LAND = (602, 562)
IDLE_CLIP, IDLE_ANCHOR = CAPT["clip"], CAPT["anchor"]
ATTACK = [A.frame_of("hunter_captain_sw_attack", i) for i in range(40)]
COCKED, CA = ATTACK[16]
TIP_COCKED = A.tip_of(COCKED, CA, CAPT["x"], CAPT["y"], "back")
ARC0 = (TIP_COCKED[0] - 2, TIP_COCKED[1] - 8)
WIND_FROM, THROW, LANDS, BURN_OUT, END = 22, 39, 50, 65, 84
POSE = {38: 20, 39: 24, 40: 28, 41: 30, 42: 32, 43: 36}


def pose_index(f):
    if WIND_FROM <= f < WIND_FROM + 8:
        return 8 + (f - WIND_FROM)
    if WIND_FROM + 8 <= f < 38:
        return 16
    return POSE.get(f)


def arc(t):
    return (ARC0[0] + (LAND[0] - ARC0[0]) * t, ARC0[1] + (LAND[1] - ARC0[1]) * t - 230 * math.sin(math.pi * t))


def tongue(x, y, hh, ww, s, ph):
    left, right = [], []
    for i in range(15):
        t = i / 14
        hw = ww / 2 * (1 - t) * (1 + 0.45 * math.sin(math.pi * t))
        cx = x + s * t ** 1.5 + ww * 0.12 * math.sin(2.4 * math.pi * t + ph)
        left.append((cx - hw, y - hh * t))
        right.append((cx + hw, y - hh * t))
    base = [(x + ww / 2 * math.cos(a), y + ww * 0.16 * math.sin(a)) for a in [math.pi * k / 8 for k in range(9)]]
    return [(round(a), round(b)) for a, b in left + right[::-1] + base]


def flame(d, x, y, h, w, sway, ph=0.0):
    if h < 3:
        return
    d.polygon(tongue(x, y, h, w, sway, ph), fill=HOT[3])
    d.polygon(tongue(x, y + 1, h * 0.7, w * 0.62, sway * 0.7, ph + 0.6), fill=HOT[2])
    d.polygon(tongue(x, y + 2, h * 0.42, w * 0.34, sway * 0.4, ph + 1.1), fill=HOT[1])
    fx.ell(d, x, y, w * 0.14, w * 0.08, fill=HOT[0])


def blaze(d, x, y, seed, size, f):
    rnd = random.Random(seed)
    tongues = [(-15, 46), (13, 54), (-1, 78), (-26, 30), (26, 34)]
    for i, (dx, h) in enumerate(sorted(tongues, key=lambda q: -q[1])):
        flick = 1 + 0.12 * math.sin(f * 1.9 + i * 2.1)
        flame(d, x + dx * size, y + rnd.uniform(0, 3), h * size * rnd.uniform(0.85, 1.1) * flick, 28 * size,
              rnd.uniform(-9, 9) * size + 3 * math.sin(f * 1.3 + i), rnd.uniform(0, 6) + f * 0.9)
    for j in range(4):
        ey = (f * 4 + j * 23) % 40
        fx.ell(d, x + rnd.uniform(-20, 20) * size, y - (60 + ey) * size, 2, 2.8, fill=HOT[2])


def bomb(d, x, y, r, f):
    fx.ell(d, x, y, r + 1, r + 1, fill=fx.OUT)
    fx.ell(d, x, y, r, r, fill=HOT[5])
    fx.ell(d, x - r * 0.3, y - r * 0.3, r * 0.45, r * 0.45, fill=HOT[3])
    d.point((round(x - r * 0.4), round(y - r * 0.5)), fill=HOT[1])
    d.rectangle((round(x - 2), round(y - r - 3), round(x + 2), round(y - r)), fill=SMOKE[1])
    flame(d, x + 1, y - r - 3, 12 + 3 * ((f // 2) % 2), 9, 2 * math.sin(f), f * 0.8)


def smoke(d, x, y, j, seed):
    rnd = random.Random(seed)
    for k in range(6):
        sx = x + rnd.uniform(-18, 18) + 2 * math.sin(j * 0.6 + k)
        sy = y - 18 - k * 11 - 2.5 * j
        r = rnd.uniform(6, 11) - 0.35 * j
        if r < 1.5:
            continue
        fx.ell(d, sx, sy + 1, r, r * 0.9, fill=SMOKE[2])
        fx.ell(d, sx, sy, r - 1, r * 0.85, fill=SMOKE[k % 2])
    for k in range(10):
        ex, ey = x + rnd.uniform(-26, 26), y - rnd.uniform(4, 60) - 3 * j
        if j < 14:
            d.rectangle((round(ex), round(ey), round(ex) + 1, round(ey) + 1), fill=HOT[1 + k % 2])


BREAK = {}


def frame(f):
    mods, shake = {}, (0, 0)
    pi = pose_index(f)
    if pi is None:
        CAPT["clip"], CAPT["anchor"] = IDLE_CLIP, IDLE_ANCHOR
    else:
        spr, anc = ATTACK[pi]
        CAPT["clip"], CAPT["anchor"] = [(spr, 100)], anc
    j = f - (LANDS + 1)
    if j >= 0:
        t = A.hit_tint(j)
        if t:
            for u in FRONT:
                mods[u["i"]] = dict(tint=t)
        shake = A.hit_shake(j)

    def under(layer):
        d = ImageDraw.Draw(layer)
        for n, u in enumerate(FRONT):
            mx, my = u["x"], u["y"] + 2
            if f < 38:
                A.mark_tell(layer, MARK, mx, my, f, HOT[1], spin=3, snap=(1.7, 1.4, 1.15, 1.0))
            elif f < BURN_OUT:
                spr = MARK.at(f // 3)
                spr = A.flat(spr, fx.OUT) if f == LANDS else A.lift(spr, HOT[0] if f > LANDS else HOT[1], 0.5 if f > LANDS else 0.35)
                A.put(layer, spr, mx, my)
            else:
                if f == BURN_OUT:
                    BREAK[n] = A.Breakup(MARK.at(f // 3), mx, my, 60 + n, keep=0.22, speed=2.0, up=1.2, g=0.3)
                fx.ell(d, mx, my, MARK.rx - 6, MARK.ry - 3, fill=SCORCH[0])
                fx.ell(d, mx, my, MARK.rx - 14, MARK.ry - 7, fill=SCORCH[1])
                BREAK[n].draw(d, f - BURN_OUT)
        if f == LANDS:
            fx.ell(d, LAND[0], LAND[1], 40, 15, fill=fx.OUT)
        if f > LANDS:
            fx.ell(d, LAND[0], LAND[1], 40, 15, fill=HOT[4] if f < BURN_OUT else SCORCH[0])
        if f == LANDS + 1:
            fx.ell(d, LAND[0], LAND[1], 74, 26, outline=HOT[0], width=3)
        if f >= BURN_OUT:
            for u in FRONT:
                for s in range(1, 8):
                    tt = s / 8
                    fx.ell(d, LAND[0] + (u["x"] - LAND[0]) * tt, LAND[1] + (u["y"] - LAND[1]) * tt, 6, 2.5, fill=SCORCH[0])

    def over(layer):
        d = ImageDraw.Draw(layer)
        spr, (x0, y0) = A.unit_sprite(CAPT, f * A.MS)
        if f < WIND_FROM:
            tx, ty = A.tip_of(spr, CAPT["anchor"], CAPT["x"], CAPT["y"], "top")
            bomb(d, tx - 2, ty - 10, 7, f)
        elif f < THROW:
            if f >= 38:
                tx, ty = ARC0[0] + 2, ARC0[1] + 8
            else:
                tx, ty = A.tip_of(spr, CAPT["anchor"], CAPT["x"], CAPT["y"], "back")
            bomb(d, tx - 2, ty - 8, 8 + (1 if f >= A.GLOW_FROM else 0), f)
        elif f < LANDS:
            t = (f - THROW) / (LANDS - THROW)
            for k in range(1, 9):
                tt = t - 0.035 * k
                if tt < 0:
                    break
                px, py = arc(tt)
                fx.ell(d, px, py, 2.5 - 0.2 * k, 2.5 - 0.2 * k, fill=HOT[4])
                fx.ell(d, px, py, 1.5, 1.5, fill=HOT[1] if k % 2 else HOT[2])
            bx, by = arc(t)
            bomb(d, bx, by, 10, f)
        if f == LANDS + 1:
            A.spikes(d, LAND[0], LAND[1] - 6, 12, 60, (HOT[0], HOT[1], HOT[3]))
        if f == LANDS + 2:
            A.spikes(d, LAND[0], LAND[1] - 6, 13, 36, (HOT[1], HOT[2], HOT[4]))
        if LANDS + 1 <= f < BURN_OUT + 12:
            k = f - (LANDS + 1)
            size = {0: 0.6, 1: 1.15}.get(k, 1.0)
            if f >= BURN_OUT:
                size = max(0.0, 1.0 - (f - BURN_OUT) / 11)
            reach = min(7, 3 * k)
            for u in FRONT:
                for s in range(1, reach + 1):
                    tt = s / 8
                    x, y = LAND[0] + (u["x"] - LAND[0]) * tt, LAND[1] + (u["y"] - LAND[1]) * tt
                    flame(d, x, y, (20 + 8 * (s % 2)) * size, 16 * max(0.4, size), (-1) ** s * 3, s + f * 0.8)
            if size > 0.05:
                blaze(d, LAND[0], LAND[1], 9, 0.8 * size, f)
            for n, u in enumerate(FRONT):
                if k >= 2 and size > 0.05:
                    blaze(d, u["x"], u["y"] + 2, 20 + n, 1.1 * size if k > 2 else 0.7, f)
        if f >= BURN_OUT:
            for n, u in enumerate(FRONT):
                smoke(d, u["x"], u["y"], f - BURN_OUT, 30 + n)
            smoke(d, LAND[0], LAND[1], f - BURN_OUT, 40)

    img = st.render(under=under, over=over, mods=mods, shake=shake, t_ms=f * A.MS)
    CAPT["clip"], CAPT["anchor"] = IDLE_CLIP, IDLE_ANCHOR
    return img


if __name__ == "__main__":
    frames = [frame(f) for f in range(END)]
    out = A.save(frames, "firebomb", "firebomb-anim", (440, 240, 1340, 800), (470, 300, 760, 760),
                 [1, 3, 20, 30, 51, 52, 53, 58, 66, 74])
    fx.strip(frames, [10, 26, 36, 40, 44, 48, 50, 51, 54, 70], (440, 240, 1340, 800), str(out / "firebomb-anim-check-wide.png"), scale=1, cols=2)
    print("frames", len(frames), "to", out)
