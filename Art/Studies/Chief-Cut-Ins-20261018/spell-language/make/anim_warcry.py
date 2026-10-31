"""War Cry, animated from its strip: the hunter mark lights under the captain himself (he is the one to hit) and turns
through the 3 s tell while a hound of smoke gathers over him and sparks are drawn into his chest; a dark frame, the
hound snaps up big and howls, a shockwave bursts out of the mark and rolls through their band, every hunter it passes
flashes purple and stands taller, three rings of sound fly from the jaws, then the hound and the mark break into specks.
A second take: your Focus Fire hits him in the tell, the glow cracks, the mark shatters and the hound bursts. No cry."""
import math

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage

import anim as A
import fxkit as fx
import sceneWC as W

HOT = [(255, 241, 255), (230, 179, 255), (177, 108, 240), (131, 69, 191), (77, 40, 134), (40, 18, 77)]
st = W.stage
CAPT = W.CAPT
HUNTERS = [u for u in W.HUNTERS if u is not CAPT]
PAL = A.palette(["hunter_captain_sw_idle", "hunter_marksman_sw_idle"], HOT)
MARK = A.Mark("mark-hunters.png", 150, PAL, steps=8, sym=45.0)
hsrc = fx.key(A.RAW / "hero-hound.png", tol=60)
HOUND = fx.pixelize(hsrc.resize((150, round(150 * hsrc.height / hsrc.width)), Image.BOX), outline=fx.OUT, pal=PAL)
HOUND_BIG = A.scaled(HOUND, 1.15)


def smoke_version(spr):
    a = np.asarray(spr).copy()
    on = a[..., 3] > 0
    edge = on & ~ndimage.binary_erosion(on, iterations=2)
    a[on, :3] = HOT[4]
    a[edge, :3] = HOT[3]
    return Image.fromarray(a, "RGBA")


HSMOKE = smoke_version(HOUND)
FEET = (CAPT["x"], CAPT["y"])
HPOS = (CAPT["x"] - 26, CAPT["y"] - 181 - 44)
JAWS = (HPOS[0] - 34, HPOS[1] + 12)
REL, WAVE, BREAK_UP, CHEV_OFF, END = 38, 39, 49, 58, 62
HIT, END_B = 24, 40
SMOKE = {}


def smoke_pose(f):
    """The smoke hound gathers from a wisp at his chest to full size over his head."""
    p = min(1.0, max(0.0, (f - 6) / 22.0))
    s = round(0.25 + 0.75 * p, 2)
    if s not in SMOKE:
        SMOKE[s] = A.scaled(HSMOKE, s)
    x = W.CH[0] + (HPOS[0] - W.CH[0]) * p + (1 if (f // 2) % 2 else -1)
    y = W.CH[1] - 20 + (HPOS[1] - (W.CH[1] - 20)) * p
    return SMOKE[s], x, y


def wisps(d, f):
    for k in range(5):
        ph = (f * 0.11 + k / 5.0) % 1.0
        sx, sy = CAPT["x"] - 18 + 9 * k, CAPT["y"] - 130
        tx, ty = HPOS[0] - 30 + 15 * k, HPOS[1] + 30
        x = sx + (tx - sx) * ph + 3 * math.sin(f * 0.7 + k)
        y = sy + (ty - sy) * ph
        r = 5 - 3 * ph
        fx.ell(d, x, y, r + 1, r + 1, fill=HOT[5])
        fx.ell(d, x, y, r, r, fill=HOT[4])


def chevron(d, x, y):
    for k in range(2):
        yy = y - k * 7
        pts = [(x - 8, yy + 5), (x, yy - 3), (x + 8, yy + 5)]
        d.line(pts, fill=fx.OUT, width=5)
        d.line(pts, fill=HOT[1] if k == 0 else HOT[0], width=3)


def bolt(d, x, y, ang):
    r = math.radians(ang)
    tip = (x + 9 * math.cos(r), y + 9 * math.sin(r))
    tail = (x - 9 * math.cos(r), y - 9 * math.sin(r))
    seg = [(round(tail[0]), round(tail[1])), (round(tip[0]), round(tip[1]))]
    d.line(seg, fill=(40, 30, 48), width=3)
    d.line(seg, fill=(236, 228, 218), width=1)
    d.point(seg[1], fill=(255, 255, 255))


def frame(f, sparks, bits, state, hit_at=None):
    broken = hit_at is not None
    j = f - hit_at if broken else -99
    w = f - WAVE if not broken else -99
    live = f < REL and (not broken or j < 3)
    mods, shake = {}, (0, 0)
    if 2 <= f < REL and f % 2 == 0 and (not broken or j < 0):
        sparks.suck(W.CH[0], W.CH[1], 3)
    if not broken:
        if 0 <= w < len(W.RS):
            for x, y in W.ring_points(W.RS[w], 14):
                bits.dust(x, y - 1, 1, spread=5, rise=0.9, size=(3, 6))
        for u in HUNTERS:
            jj = f - (WAVE + u["hit"])
            if jj == 0:
                mods[u["i"]] = dict(tint=HOT[2])
                sparks.rise(u["x"], u["y"] - u["H"] - 4, 3)
            elif jj in (1, 2):
                mods[u["i"]] = dict(dy={1: -3, 2: -1}[jj], dx={1: -2, 2: -1}[jj])
        if f in (36, 37):
            mods[CAPT["i"]] = dict(dy=-1)
        if w == 0:
            mods[CAPT["i"]] = dict(tint=A.WHITE, dx=3)
        if w == 1:
            mods[CAPT["i"]] = dict(dx=1)
        if 0 <= w <= 2:
            shake = A.hit_shake(w)
    else:
        t = A.hit_tint(j) if j >= 0 else None
        if t:
            mods[CAPT["i"]] = dict(tint=t, dx={0: 2, 1: 3}.get(j, 0))
        elif 2 <= j <= 6:
            mods[CAPT["i"]] = dict(dx=1 if j % 2 else -1)
        if j == 0:
            shake = (3, -1)
        if j == 3:
            sparks.burst(W.CH[0], W.CH[1], 10, 12, gravity=1.8)
            sparks.burst(W.CH[0], W.CH[1], 6, 6)
    sparks.step()
    bits.step()
    mx, my = FEET[0], FEET[1] + 2

    def under(layer):
        d = ImageDraw.Draw(layer)
        if live:
            A.mark_tell(layer, MARK, mx, my, f, HOT[1], spin=3)
        elif not broken and f == REL:
            A.put(layer, A.flat(MARK.at(f // 3), fx.OUT), mx, my)
        elif not broken and f < BREAK_UP:
            A.put(layer, A.lift(MARK.at(f // 3), HOT[0], 0.5 if w == 0 else 0.3), mx, my)
            fx.ell(d, mx, my, MARK.rx + 3, MARK.ry + 2, outline=HOT[1], width=2)
        if "mark" not in state and ((not broken and f >= BREAK_UP) or (broken and j >= 3)):
            state["mark"] = (f, A.Breakup(MARK.at(f // 3), mx, my, 5, keep=0.3 if broken else 0.22,
                                         speed=2.6 if broken else 2.0, up=1.2, g=0.3))
        if "mark" in state:
            state["mark"][1].draw(d, f - state["mark"][0])
        if broken and 3 <= j <= 9:
            d.line([(mx - 60, my - 6), (mx - 10, my + 4), (mx + 20, my - 4), (mx + 66, my + 6)], fill=fx.OUT, width=3)
        if not broken:
            if 0 <= w < len(W.RS):
                W.shock(d, W.RS[w], thin=w >= len(W.RS) - 2)
                if w >= 1:
                    W.shells(d, W.RS[w], 16)
            if w == len(W.RS):
                W.shock_specks(d, W.RS[-1] + 14, 14)
            if w == len(W.RS) + 1:
                W.shock_specks(d, W.RS[-1] + 24, 30)
        bits.draw(d, kinds=("dust", "speck"))

    def over(layer):
        d = ImageDraw.Draw(layer)
        if 6 <= f and live:
            spr, x, y = smoke_pose(f)
            if f >= A.GLOW_FROM:
                spr = A.lift(spr, HOT[2], 0.25 if f % 2 else 0.08)
            wisps(d, f)
            A.put(layer, spr, x, y)
        if broken and j == 3 and "smoke" not in state:
            spr, x, y = smoke_pose(f)
            state["smoke"] = (f, A.Breakup(spr, x, y, 11, keep=0.3, speed=2.6, up=0.8, g=0.2))
        if "smoke" in state:
            state["smoke"][1].draw(d, f - state["smoke"][0])
        if live:
            W.glow(d, min(17, f * 17 // 37), crack=broken and j >= 1)
        if broken:
            k = f - (hit_at - 4)
            if 0 <= k < 4:
                for sx, sy in ((640, 400), (640, 560), (640, 700)):
                    p = (k + 1) / 4.0
                    x, y = sx + (W.CH[0] - sx) * p, sy + (W.CH[1] - sy) * p
                    bolt(d, x, y, math.degrees(math.atan2(W.CH[1] - sy, W.CH[0] - sx)))
            if j == 0:
                A.spikes(d, W.CH[0] - 6, W.CH[1], 4, 26, (A.WHITE, (255, 230, 200), A.RED),
                         dirs=(-180, -120, -60, 0, 60, 120))
            if j == 3:
                fx.ell(d, W.CH[0], W.CH[1], 16, 16, fill=fx.OUT)
                A.spikes(d, W.CH[0], W.CH[1], 812, 30, (HOT[0], HOT[1], HOT[3]), dirs=(-150, -100, -40, 20, 80, 140))
            if j == 4:
                fx.ell(d, W.CH[0], W.CH[1], 12, 10, fill=(70, 58, 85))
        else:
            if f == REL:
                fx.ell(d, W.CH[0], W.CH[1], 14, 14, fill=fx.OUT)
            if w == 0:
                fx.ell(d, W.CH[0], W.CH[1], 26, 26, outline=HOT[0], width=3)
                fx.ell(d, W.CH[0], W.CH[1], 10, 10, fill=HOT[0])
            if w == 1:
                fx.ell(d, W.CH[0], W.CH[1], 40, 40, outline=HOT[1], width=2)
            if 0 <= w < BREAK_UP - WAVE:
                A.put(layer, HOUND_BIG if w == 0 else HOUND, HPOS[0], HPOS[1] - (1 if (f // 2) % 2 else 0))
            if w == 0:
                fx.ell(d, JAWS[0], JAWS[1], 26, 26, outline=HOT[0], width=3)
                A.spikes(d, JAWS[0], JAWS[1], 3, 34, (HOT[0], HOT[1], HOT[3]), dirs=(-200, -170, -140, -110, -80))
            for a0 in (1, 3, 5):
                age = w - a0
                if 0 <= age < 6:
                    W.air_ring(d, JAWS[0] - 30 - 64 * age, JAWS[1], 18 + 17 * age, age)
            if f >= BREAK_UP and "hound" not in state:
                state["hound"] = (f, A.Breakup(HOUND, HPOS[0], HPOS[1], 9, keep=0.2, speed=2.2, up=1.6, g=0.3))
            if "hound" in state:
                state["hound"][1].draw(d, f - state["hound"][0])
            if f < CHEV_OFF:
                for u in HUNTERS:
                    if f - (WAVE + u["hit"]) >= 1:
                        chevron(d, u["x"], u["y"] - u["H"] - 14)
        sparks.draw(d)

    return st.render(under=under, over=over, mods=mods, shake=shake, t_ms=f * A.MS)


if __name__ == "__main__":
    sp, bt, state = W.Sparks(41), fx.Bits(42), {}
    frames = [frame(f, sp, bt, state) for f in range(END)]
    out = A.save(frames, "war-cry", "war-cry-anim", (560, 240, 1408, 800), (900, 300, 1300, 720),
                 [6, 20, 34, 38, 39, 40, 42, 44, 46, 49, 53, 59])
    fx.strip(frames, [30, 39, 41, 43, 45, 50], (560, 240, 1408, 800), str(out / "war-cry-anim-check-wide.png"), scale=1, cols=2)
    sp, bt, state = W.Sparks(43), fx.Bits(44), {}
    bframes = [frame(f, sp, bt, state, hit_at=HIT) for f in range(END_B)]
    A.save(bframes, "war-cry", "war-cry-broken-by-focus-fire", (560, 240, 1408, 800), (900, 300, 1300, 720),
           [10, 21, 23, 24, 25, 27, 29, 33])
    print("frames", len(frames), len(bframes), "to", out)
