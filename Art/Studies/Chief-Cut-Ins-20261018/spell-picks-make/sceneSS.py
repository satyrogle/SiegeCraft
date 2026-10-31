"""Steel Storm B: a blade storm. Shards fly up off the Wardens' shields and orbit the Skullpeak band (rise), the ring
spins while the wall stands (a seamless 24 frame loop), a blow lands on it (a spark, the blades bounce, no real
damage), and Unleash smashes it: a dark frame, a white frame, the blades blow outward, clatter onto the snow and
break into specks. Blades behind the band draw behind the fighters, blades in front draw in front."""
import math
import random

from PIL import ImageDraw

import capkit as ck
import fxkit as fx

OUTL, DARKS, MIDS, LIGHTS, GLINT = (22, 19, 18), (46, 56, 64), (84, 100, 108), (150, 160, 158), (240, 238, 230)
TEAL, TEALD = (39, 214, 201), (22, 122, 118)
SNOW = [(236, 240, 244), (200, 208, 216), (150, 160, 170)]
CX, CY, RX, RY = 1000, 560, 215, 165
OMEGA, LOOP = 15, 24
TIERS = [(26, 1.0, 0.0), (80, 0.95, 22.5), (134, 0.88, 11.0)]
stage = ck.Stage("skullpeak", "snow")
THEM = [u for u in stage.units if u["side"] == "them"]
WARDENS = [u for u in THEM if u["x"] < 900]
SHIELDS = [(u["x"] + 25, u["y"] - 70) for u in WARDENS]
L0, RISE, STAND = 2, 4, 30
SMASH = L0 + RISE + STAND
BLOWS = [(L0 + RISE + 8, 180.0, 80), (L0 + RISE + 19, 160.0, 26)]
rnd = random.Random(7)
BLADES = []
for t, (h, rs, ph) in enumerate(TIERS):
    for j in range(8):
        BLADES.append(dict(tier=t, h=h + rnd.uniform(-5, 5), rs=rs, a0=ph + j * 45 + rnd.uniform(-6, 6),
                           L=rnd.uniform(22, 30), tilt=rnd.uniform(-18, 18), glint=rnd.random() < 0.25,
                           spin=rnd.choice((-1, 1)) * rnd.uniform(25, 45), v=rnd.uniform(9, 15), drop=rnd.uniform(-10, 24)))


def orbit(b, k):
    th = math.radians(b["a0"] + OMEGA * k)
    gx = CX + RX * b["rs"] * math.cos(th)
    gy = CY + RY * b["rs"] * math.sin(th)
    tx, ty = -RX * math.sin(th), RY * math.cos(th)
    return gx, gy, gy - b["h"], math.atan2(ty, tx) + math.radians(b["tilt"]), th


def blade(d, x, y, ang, L, front, glint, tint=None, smear=True):
    ux, uy = math.cos(ang), math.sin(ang)
    nx, ny = -uy, ux
    w = 3.6
    tip = (x + ux * L * 0.5, y + uy * L * 0.5)
    tail = (x - ux * L * 0.5, y - uy * L * 0.5)
    if smear:
        d.line([(round(tail[0]), round(tail[1])), (round(tail[0] - ux * 16), round(tail[1] - uy * 16))], fill=TEAL if front else TEALD, width=2)
    body = tint or (MIDS if front else DARKS)
    edge = tint or (GLINT if front else LIGHTS)
    poly = [tip, (x + nx * w, y + ny * w), tail, (x - nx * w, y - ny * w)]
    d.polygon([(round(a), round(b)) for a, b in poly], fill=body, outline=OUTL if tint is None or tint != OUTL else OUTL)
    d.line([(round(tip[0]), round(tip[1])), (round(x + nx * w * 0.6), round(y + ny * w * 0.6))], fill=edge)
    if glint and tint is None:
        d.point((round(tip[0]), round(tip[1])), fill=TEAL if front else GLINT)


def spark(d, x, y, size, n):
    """A steel hit: a white star with a teal rim, hard edged."""
    for j in range(n):
        a = math.radians(j * 360 / n + 15)
        L = size * (1.0 if j % 2 == 0 else 0.6)
        w = size * 0.16
        tip = (x + math.cos(a) * L, y + math.sin(a) * L)
        nx, ny = -math.sin(a) * w, math.cos(a) * w
        d.polygon([(round(x + nx), round(y + ny)), (round(tip[0]), round(tip[1])), (round(x - nx), round(y - ny))], fill=GLINT, outline=TEAL)
    fx.ell(d, x, y, size * 0.22, size * 0.22, fill=GLINT)


def puff(d, x, y, r):
    fx.ell(d, x, y + 1, r, r * 0.8, fill=SNOW[2])
    fx.ell(d, x, y, r - 0.5, r * 0.75, fill=SNOW[1])
    fx.ell(d, x - r * 0.3, y - r * 0.3, r * 0.45, r * 0.4, fill=SNOW[0])


def wind_items(k):
    items = []
    for t, (h, rs, ph) in enumerate(TIERS):
        for a in (0, 120, 240):
            start = ph + a + OMEGA * k + 12
            pts = []
            for s in range(0, 55, 5):
                th = math.radians(start + s)
                gx = CX + RX * rs * 1.04 * math.cos(th)
                gy = CY + RY * rs * 1.04 * math.sin(th)
                pts.append((gx, gy, gy - h - 3))
            for (x0, g0, y0), (x1, g1, y1) in zip(pts, pts[1:]):
                items.append(((g0 + g1) / 2, ("line", (x0, y0, x1, y1), (g0 + g1) / 2 > CY)))
    return items


DEBRIS = []


def smash_start(k):
    for b in BLADES:
        gx, gy, y, ang, th = orbit(b, k)
        dx, dy = gx - CX, gy - CY
        n = math.hypot(dx, dy) or 1
        DEBRIS.append(dict(b=b, gx=gx, gy=gy, z=b["h"], vx=dx / n * b["v"], vy=dy / n * b["v"] * 0.6, vz=7.0,
                           ang=ang, rest=0, bounced=False, land=gy + b["drop"] * 0.3))


def smash_step(puffs):
    for p in DEBRIS:
        if p["rest"]:
            p["rest"] += 1
            continue
        p["gx"] += p["vx"]
        p["gy"] += p["vy"]
        p["vx"] *= 0.86
        p["vy"] *= 0.86
        p["z"] += p["vz"]
        p["vz"] -= 2.4
        p["ang"] += math.radians(p["b"]["spin"])
        if p["z"] <= 0:
            p["z"] = 0
            if not p["bounced"]:
                p["bounced"] = True
                p["vz"] = 3.0
                puffs.append([p["gx"], p["gy"], 5.0])
            else:
                p["rest"] = 1
                p["ang"] = round(p["ang"] / math.pi) * math.pi + math.radians(p["b"]["tilt"] * 0.6)


def frame(f, puffs):
    k = f - L0
    ks = f - SMASH
    mods, shake = {}, (0, 0)
    items = []
    if 0 <= k < RISE:
        t = (k + 1) / RISE
        e = 1 - (1 - t) ** 2
        for b in BLADES:
            gx, gy, y, ang, th = orbit(b, k + 1)
            sx, sy = min(SHIELDS, key=lambda s: (s[0] - gx) ** 2 + (s[1] - gy) ** 2)
            x = sx + (gx - sx) * e
            yy = sy + (y - sy) * e - 34 * math.sin(math.pi * t)
            g = sy + 70 + (gy - sy - 70) * e
            items.append((g, ("blade", x, yy, ang + math.radians(b["spin"] * 3 * (1 - t)), b, None, g > CY)))
    elif RISE <= k and ks < 2:
        kk = min(k, SMASH - L0) + 1
        for b in BLADES:
            gx, gy, y, ang, th = orbit(b, kk)
            tint, off = None, 0.0
            for fb, deg, h in BLOWS:
                j = f - fb
                close = abs((math.degrees(th) - deg + 180) % 360 - 180) < 32 and abs(b["h"] - h) < 40
                if close and j == 0:
                    tint = GLINT
                if close and j == 1:
                    off = 7.0
            if ks == 0:
                tint = OUTL
            if ks == 1:
                tint = GLINT
            x = gx + off * math.cos(th)
            yy = y + off * 0.6 * math.sin(th)
            items.append((gy, ("blade", x, yy, ang, b, tint, gy > CY)))
        if ks < 0:
            items += wind_items(kk)
    else:
        if ks == 2:
            smash_start(SMASH - L0 + 1)
        smash_step(puffs)
        for p in DEBRIS:
            if ks >= 16:
                continue
            front = p["gy"] > CY
            if ks >= 13:
                items.append((p["gy"], ("specks", p["gx"], p["gy"] - p["z"], 16 - ks)))
            else:
                items.append((p["gy"], ("blade", p["gx"], p["gy"] - p["z"], p["ang"], p["b"], DARKS if p["rest"] else None, front)))
    for pp in puffs:
        pp[2] -= 1.0
    puffs[:] = [pp for pp in puffs if pp[2] > 1.5]
    if ks in (1, 2, 3):
        shake = {1: (4, -2), 2: (-4, 2), 3: (2, -1)}[ks]
        for u in THEM:
            mods[u["i"]] = dict(dx={1: 3, 2: 2, 3: 1}[ks])
    items.sort(key=lambda it: it[0])
    state = {"i": 0}

    def draw_item(d, it):
        kind = it[1][0]
        if kind == "blade":
            _, x, y, ang, b, tint, front = it[1]
            blade(d, x, y, ang, b["L"], front, b["glint"], tint=tint, smear=tint is None and ks < 2)
        elif kind == "line":
            _, (x0, y0, x1, y1), front = it[1]
            d.line([(round(x0), round(y0)), (round(x1), round(y1))], fill=TEAL if front else TEALD, width=2)
        elif kind == "specks":
            _, x, y, n = it[1]
            for s in range(n):
                d.rectangle((round(x + s * 4 - 4), round(y - s), round(x + s * 4 - 3), round(y - s + 1)), fill=DARKS)

    def before(layer, u):
        d = ImageDraw.Draw(layer)
        while state["i"] < len(items) and items[state["i"]][0] < u["y"]:
            draw_item(d, items[state["i"]])
            state["i"] += 1

    def under(layer):
        d = ImageDraw.Draw(layer)
        for x, y, r in puffs:
            puff(d, x, y, r)
        if 0 <= ks <= 1:
            fx.ell(d, CX, CY, RX * 1.02, RY * 1.02, outline=GLINT if ks == 1 else OUTL, width=2)

    def over(layer):
        d = ImageDraw.Draw(layer)
        while state["i"] < len(items):
            draw_item(d, items[state["i"]])
            state["i"] += 1
        if k == -1 or k == 0:
            for sx, sy in SHIELDS:
                spark(d, sx, sy, 12 if k == -1 else 20, 4)
        for fb, deg, h in BLOWS:
            j = f - fb
            px, py = CX + RX * math.cos(math.radians(deg)), CY + RY * math.sin(math.radians(deg)) - h
            if j == 0:
                spark(d, px, py, 26, 7)
            if j == 1:
                fx.ell(d, px, py, 6, 6, fill=GLINT)

    return stage.render(under=under, over=over, before=before, mods=mods, shake=shake)


if __name__ == "__main__":
    import sys
    import mp4kit
    puffs = []
    N = SMASH + 20
    frames = [frame(f, puffs) for f in range(N)]
    pick = [L0 - 1, L0 + 1, L0 + 3, L0 + 8, BLOWS[0][0], BLOWS[0][0] + 1, SMASH - 1, SMASH, SMASH + 1, SMASH + 3, SMASH + 6, SMASH + 12]
    fx.strip(frames, pick, (740, 230, 1260, 790), "SS-strip.png", scale=1, cols=6)
    if "check" in sys.argv:
        sys.exit()
    out = fx.STUDY.parent / "steel-storm" / "B-blade-storm"
    out.mkdir(parents=True, exist_ok=True)
    mp4kit.mp4([fr.crop((560, 200, 1408, 800)) for fr in frames], out / "steel-storm-B-blade-storm.mp4")
    mp4kit.mp4([fr.crop((760, 280, 1240, 760)).resize((960, 960)) for fr in frames], out / "steel-storm-B-blade-storm-2x.mp4")
    print("frames", N, out)
