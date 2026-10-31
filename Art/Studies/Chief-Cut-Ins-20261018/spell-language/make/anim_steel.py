"""Steel Storm, animated from its strip: a line of teal runes on the snow marks where the wall will stand (3 s tell,
the last second blazing, the Wardens glowing, steel lifting off the line), a dark frame, a barricade of the Wardens'
own shields slams up from the middle outwards, it stands (8 s in the game, 2 s here) while two of your blows spark off
it, then Unleash smashes it: dark frame, white frame, the shields blow apart, tumble, land on the snow and break up."""
import math
import random

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage

import anim as A
import capkit as ck
import fxkit as fx

OUTL, STEEL, GLINT, TEAL, TEALD = (22, 19, 18), (123, 138, 135), (240, 238, 230), (39, 214, 201), (22, 122, 118)
SNOW = [(236, 240, 244), (200, 208, 216), (150, 160, 170)]
st = ck.Stage("skullpeak", "snow")
THEM = [u for u in st.units if u["side"] == "them"]
WARDENS = [u for u in THEM if u["x"] == 840]

c = np.asarray(WARDENS[0]["spr"].crop(WARDENS[0]["spr"].getbbox())).copy()
sh = c[6:127, 59:85].copy()
r, g, b, a = [sh[..., i].astype(int) for i in range(4)]
steel = (a > 0) & ((np.maximum(np.maximum(r, g), b) - np.minimum(np.minimum(r, g), b) < 40) | ((g > r) & (b > r)))
sh[~steel, 3] = 0
lab, n = ndimage.label(sh[..., 3] > 0)
sizes = ndimage.sum(sh[..., 3] > 0, lab, range(1, n + 1))
keep = lab == (int(np.argmax(sizes)) + 1)
sh[~keep, 3] = 0
sh[ndimage.binary_dilation(keep, iterations=1) & ~keep] = OUTL + (255,)
SHIELD = A.scaled(Image.fromarray(sh, "RGBA"), 1.35, 1.15)
SLOTS = [(752 + (24 if i % 2 else 0) + 0.06 * (y - 560), y) for i, y in enumerate(range(400, 750, 38))]
LINE = [(758 + 0.06 * (y - 560), y) for y in range(395, 752, 6)]
DELAY = [int(abs(i - 4.5)) // 2 for i in range(len(SLOTS))]

REL, RISE, STAND, SMASH, SPECK, END = 38, 39, 42, 67, 80, 92
BLOWS = [(48, 4), (58, 7)]
ROT = {}


def rotated(ang):
    k = int(round(ang / 15.0)) * 15 % 360
    if k not in ROT:
        ROT[k] = SHIELD.rotate(k, resample=Image.NEAREST, expand=True)
    return ROT[k]


def rune(d, x, y, kind, col):
    x, y = round(x), round(y)
    arms = {0: ((-6, -6), (6, 6)), 1: ((-6, 6), (6, -6)), 2: ((-7, -2), (7, -2)), 3: ((-5, -9), (5, -9))}[kind % 4]
    for w, colr in ((6, OUTL), (3, col)):
        d.line([(x, y - 12), (x, y + 5)], fill=colr, width=w)
        d.line([(x + arms[0][0], y + arms[0][1] - 3), (x + arms[1][0], y + arms[1][1] - 3)], fill=colr, width=w)


def band(d, frac, glow, dark=False):
    m = len(LINE) / 2
    lo, hi = int(m - frac * m), int(math.ceil(m + frac * m))
    pts = [(round(x), round(y)) for x, y in LINE[max(0, lo):min(len(LINE), hi)]]
    if len(pts) < 2:
        return
    d.line(pts, fill=OUTL, width=10 if glow else 8)
    if dark:
        return
    d.line(pts, fill=TEAL if glow else TEALD, width=6 if glow else 4)
    if glow:
        d.line(pts, fill=GLINT, width=2)


def spark(d, x, y, size, n):
    for j in range(n):
        ang = math.radians(j * 360 / n + 15)
        L = size * (1.0 if j % 2 == 0 else 0.6)
        w = size * 0.16
        tip = (x + math.cos(ang) * L, y + math.sin(ang) * L)
        nx, ny = -math.sin(ang) * w, math.cos(ang) * w
        d.polygon([(round(x + nx), round(y + ny)), (round(tip[0]), round(tip[1])), (round(x - nx), round(y - ny))], fill=GLINT, outline=TEAL)
    fx.ell(d, x, y, size * 0.22, size * 0.22, fill=GLINT)


def puff(d, x, y, r):
    if r < 1.5:
        return
    fx.ell(d, x, y + 1, r, r * 0.8, fill=SNOW[2])
    fx.ell(d, x, y, r - 0.5, r * 0.75, fill=SNOW[1])
    fx.ell(d, x - r * 0.3, y - r * 0.3, r * 0.45, r * 0.4, fill=SNOW[0])


rnd = random.Random(11)
DEBRIS = []
for i, (x, y) in enumerate(SLOTS):
    DEBRIS.append(dict(x=x, y=y, vx=rnd.uniform(2.0, 7.0), vy=-rnd.uniform(7.0, 12.0), va=rnd.choice((-1, 1)) * rnd.uniform(12, 36),
                       ground=y + rnd.uniform(8, 30), ang=0.0, rest=False, bounced=False, land=None))
SHARDS = [dict(x=770 + rnd.uniform(-10, 10), y=575 + rnd.uniform(-120, 120), vx=rnd.uniform(-3, 9), vy=rnd.uniform(-9, -2),
               life=rnd.randint(7, 11), ground=575 + rnd.uniform(-60, 160)) for _ in range(26)]
BREAK = {}


def debris_at(f):
    """Shield positions after the smash, stepped from SMASH + 2."""
    out = []
    for dbr in DEBRIS:
        x, y, vx, vy, ang, va = dbr["x"], dbr["y"], dbr["vx"], dbr["vy"], 0.0, dbr["va"]
        bounced, land = False, None
        for k in range(f - (SMASH + 2) + 1):
            if land is not None and bounced and k - land > 2:
                break
            x += vx
            y += vy
            vy += 1.7
            ang += va
            if y >= dbr["ground"] and vy > 0:
                y = dbr["ground"]
                if not bounced:
                    vy, vx, va = -vy * 0.3, vx * 0.5, va * 0.4
                    bounced = True
                    land = k
                else:
                    vx = vy = va = 0.0
                    ang = 90.0 if math.cos(math.radians(ang)) >= 0 else -90.0
        out.append((x, y, ang, land))
    return out


def frame(f):
    mods, shake = {}, (0, 0)
    if SMASH + 1 <= f <= SMASH + 3:
        shake = A.hit_shake(f - SMASH - 1)
        for u in THEM:
            mods[u["i"]] = dict(dx={0: 3, 1: 2, 2: 1}[f - SMASH - 1])
    if f == STAND - 2:
        shake = (2, -1)

    def under(layer):
        d = ImageDraw.Draw(layer)
        if f < REL:
            band(d, {0: 0.3, 1: 0.7}.get(f, 1.0), f >= A.GLOW_FROM)
            for i, (x, y) in enumerate(SLOTS):
                if f < 3 or (f < A.GLOW_FROM and i % 2):
                    continue
                lit = f >= A.GLOW_FROM or (f // 4) % 2 == 0
                rune(d, x - 6, y - 4, i, GLINT if f >= A.GLOW_FROM else (TEAL if lit else TEALD))
        elif f == REL:
            band(d, 1.0, False, dark=True)
        elif f < SMASH:
            band(d, 1.0, True)
        for i, (x, y) in enumerate(SLOTS):
            j = f - RISE - DELAY[i]
            if 1 <= j <= 3:
                puff(d, x - 16, y + 1, 9 - 2 * j)
                puff(d, x + 16, y + 3, 8 - 2 * j)
        if f >= SMASH + 2:
            for (x, y, ang, land), (sx, sy) in zip(debris_at(f), SLOTS):
                if land is not None and f - (SMASH + 2) - land <= 3:
                    puff(d, x, y, 8 - 2 * (f - (SMASH + 2) - land))

    def before(layer, u):
        if A.GLOW_FROM <= f < REL and u in WARDENS and f % 2 == 0:
            layer.alpha_composite(A.rim(u["spr"], TEAL), u["box"][:2])

    def over(layer):
        d = ImageDraw.Draw(layer)
        if A.GLOW_FROM <= f < REL:
            rr = random.Random(f)
            for k in range(6):
                x, y = SLOTS[rr.randrange(len(SLOTS))]
                sx, sy = x + rr.uniform(-10, 10), y - 16 - ((f * 3 + k * 9) % 46)
                d.line([(round(sx), round(sy)), (round(sx + 3), round(sy - 8))], fill=OUTL, width=4)
                d.line([(round(sx), round(sy)), (round(sx + 3), round(sy - 8))], fill=GLINT, width=2)
        if RISE <= f < SMASH + 2:
            for i, (x, y) in sorted(enumerate(SLOTS), key=lambda q: q[1][1]):
                j = f - RISE - DELAY[i]
                if j < 0:
                    continue
                spr = SHIELD
                if f == SMASH:
                    spr = A.flat(SHIELD, OUTL)
                elif f == SMASH + 1:
                    spr = A.flat(SHIELD, GLINT)
                dy = {0: SHIELD.height * 0.55, 1: -4}.get(j, 0)
                dx = 0
                for bf, si in BLOWS:
                    if si == i and f - bf == 1:
                        dx = -1
                A.put_feet(layer, spr, x + dx, y + dy, clip_below=y)
                if STAND <= f < SMASH:
                    top = y - SHIELD.height
                    d.line([(round(x - 14), round(top + 4)), (round(x + 14), round(top + 1))], fill=TEAL, width=2)
                    if (f - STAND) % len(SLOTS) == i:
                        d.line([(round(x - 6), round(top + 10)), (round(x - 6), round(y - 14))], fill=GLINT, width=2)
        for bf, si in BLOWS:
            x, y = SLOTS[si]
            j = f - bf
            if j == -1:
                d.line([(round(x - 70), round(y - 104)), (round(x - 24), round(y - 92))], fill=GLINT, width=2)
            if j == 0:
                spark(d, x - 18, y - 92, 30, 7)
            if j == 1:
                spark(d, x - 18, y - 92, 16, 5)
            if j in (2, 3):
                puff(d, x - 18, y - 92, 7 - 2 * (j - 2))
        if f == SMASH + 1:
            fx.ell(d, 770, 575, 80, 150, outline=GLINT, width=3)
        if SMASH + 2 <= f < SPECK:
            for x, y, ang, land in sorted(debris_at(f), key=lambda q: q[1]):
                spr = rotated(ang)
                A.put(layer, spr, x, y - spr.height / 2 + 6)
        if f == SPECK:
            for k, (x, y, ang, land) in enumerate(debris_at(f)):
                spr = rotated(ang)
                BREAK[k] = A.Breakup(spr, x, y - spr.height / 2 + 6, 200 + k, keep=0.22, speed=1.6, up=2.0, g=0.5, life=(5, 9))
        for k, br in BREAK.items():
            br.draw(d, f - SPECK)
        if f >= SMASH + 2:
            j = f - (SMASH + 2)
            for s in SHARDS:
                if j > s["life"]:
                    continue
                x = s["x"] + s["vx"] * j
                y = min(s["ground"], s["y"] + s["vy"] * j + 0.6 * j * j)
                big = j < s["life"] * 0.6
                if big:
                    d.polygon([(round(x), round(y - 5)), (round(x + 6), round(y + 2)), (round(x - 2), round(y + 5))], fill=STEEL, outline=OUTL)
                else:
                    d.rectangle((round(x), round(y), round(x) + 1, round(y) + 1), fill=STEEL)

    return st.render(under=under, over=over, before=before, mods=mods, shake=shake)


if __name__ == "__main__":
    frames = [frame(f) for f in range(END)]
    out = A.save(frames, "steel-storm", "steel-storm-anim", (560, 200, 1408, 800), (676, 280, 996, 760),
                 [1, 18, 30, 38, 39, 40, 48, 60, 67, 68, 71, 76, 81, 86])
    print("frames", len(frames), "to", out)
