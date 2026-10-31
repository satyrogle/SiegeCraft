"""The land shark half in the sand (Jacob, 31 Oct: "put half fish in ground half on top"), built from shark_land.py.

Every land frame sits with canvas row 148 on the sand line and is cut below it, so the jaws stay up and the belly,
front fins and lower tail are under. A rim moves along the cut every tick: a rippling lip over the cut, a bow wave
of piled sand on whichever side is pushing forward (bigger the faster it goes), a furrow behind that fills back in,
and grains pouring off the body above the line. The haul still bursts out and flies; on landing it sinks to half
depth with a splash along its length. The fin walk becomes a plow. The bite is shark_bite2: gape 8 to 10, clamp at
11 (the hit, held, the front column takes it), thrash 12 to 33, open by 40. The tail sweep's blast through all five
and the head first dive stay. Clips are 40 frames (41 with the loop frame); beast and effects are drawn on the half
layer (2x pixels), squad and stage stay 1x.

python -I shark_land2.py   (python with pillow, numpy, scipy, av)
"""
import math
import pathlib
import random
import sys

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
import boss_scenes as bs  # noqa: E402
import shark_presence as sp  # noqa: E402
from boss_scenes import A, D, TICK, WHITE, fx  # noqa: E402

G = 283                       # the sand line on the half size layer (566 on screen)
CUT_ROW = 148                 # the canvas row every land frame puts on G
LAND_F, SNAP_F, SWEEP_F = 23, 11, 18
WALK_SHIFT = 50               # half size pixels the plow carries it toward the squad
SAND = (fx.S4, fx.S5, fx.S6)
SINK_MS = 300                 # landing: from belly on the sand to half depth
FREEZE = 120                  # hit stop on the snap
FILL_MS = 1300                # a furrow fills back in over this long
THRASH = (12, 33)


def clip(name):
    return [bs.strip_base(Image.open(A / name / ("%02d.png" % i)), floor=256) for i in range(41)]


def head_of(im):
    """The jaws: the leftmost solid point in the top half of the shark."""
    a = np.asarray(im)[..., 3] > 0
    ys, xs = np.nonzero(a)
    top = ys < ys.min() + 0.5 * (ys.max() - ys.min())
    x = int(xs[top].min())
    return x, int(np.median(ys[top][xs[top] < x + 6]))


def sand_spike(d, x, y, h, w=3):
    x, y, h = round(x), round(y), max(1, round(h))
    d.polygon([(x - w, y), (x, y - h), (x, y)], fill=fx.S6)
    d.polygon([(x, y), (x, y - h), (x + w, y)], fill=fx.S4)
    d.line([(x - w, y), (x, y - h), (x + w, y)], fill=fx.S2)


def spans_of(row, gap=3, least=2):
    """Runs of solid columns along one row, small gaps joined."""
    xs = np.nonzero(row)[0]
    if not len(xs):
        return []
    out, a, b = [], int(xs[0]), int(xs[0])
    for x in xs[1:]:
        x = int(x)
        if x - b <= gap:
            b = x
        else:
            out.append((a, b))
            a = b = x
    out.append((a, b))
    return [(a, b) for a, b in out if b - a + 1 >= least]


def lump(x, t, rate=110):
    """A slow per column wobble (-1..1) that changes every rate ms, so the lip boils a little."""
    k = (t // rate) * 7919 + x * 104729
    return ((k * 2654435761) % 1000) / 500.0 - 1.0


def vnoise(x, t, cell, rate, seed=0):
    """Smooth value noise (-1..1) along x that slowly morphs over time: irregular lumps, no repeating scallops."""
    i, f = divmod(x, cell)
    k, g = divmod(t / rate, 1.0)
    f, g = f / cell, g * g * (3 - 2 * g)
    f = f * f * (3 - 2 * f)

    def h(i, k):
        return ((((int(i) * 73856093) ^ (int(k) * 19349663) ^ seed) * 2654435761) % 1000) / 500.0 - 1.0
    return (h(i, k) * (1 - f) + h(i + 1, k) * f) * (1 - g) + (h(i, k + 1) * (1 - f) + h(i + 1, k + 1) * f) * g


def lip_heights(a, b, t, amp):
    """Lip height per column over a span and 4 px past each end: travelling ripples plus a slow boil, tapered."""
    xs = range(a - 4, b + 5)
    hs = []
    for x in xs:
        r = (3 + amp * (1.1 * math.sin(0.29 * x - 0.012 * t) + 1.3 * vnoise(x, t, 6, 170))
             + 0.5 * vnoise(x, t, 2, 90, 7))
        hs.append(max(0, min(round(r), 2 * (x - (a - 5)), 2 * ((b + 5) - x))))
    return list(xs), hs


def draw_lip(d, xs, hs):
    """Sand piled against the body: the lit top is lighter than the floor so the pile reads, its near face drops
    back to floor sand, a dark foot under the bigger lumps."""
    for n, (x, h) in enumerate(zip(xs, hs)):
        if h < 1:
            continue
        left = hs[n - 1] if n else 0
        d.line([(x, G - h + 1), (x, G + 1)], fill=fx.S4)
        d.line([(x, G - h), (x, G - h + max(0, h // 2 - 1))], fill=fx.S5)
        if h >= 4 or (h > left and h >= 2):
            d.point((x, G - h), fill=fx.S6)
        if (x * 7) % 5 == 0 and h >= 3:
            d.point((x, G - 1), fill=fx.S5)
        if h >= 5:
            d.point((x, G + 2), fill=fx.S3)


def shade_above(body, xs, hs):
    """Contact shadow: darken the body in the 3 rows just above the lip, darkest next to the sand."""
    a = np.asarray(body).copy()
    for x, h in zip(xs, hs):
        if 0 <= x < a.shape[1]:
            y1 = G - max(h, 0)
            for dy, k in ((1, 0.5), (2, 0.66), (3, 0.82)):
                y = y1 - dy
                if y >= 0 and a[y, x, 3] > 0:
                    a[y, x, :3] = (a[y, x, :3] * k).astype(np.uint8)
    return Image.fromarray(a, "RGBA")


def draw_bow(d, x, h, side, t):
    """A wave of sand pushed ahead of the body: crest against the body, a long front slope; side -1 = ahead on
    the left, +1 = ahead on the right."""
    h = max(2, min(9, round(h)))
    L = 4 + round(2.4 * h)
    for k in range(-4, L + 1):
        u = max(0.0, k) / L
        hb = round(h * (1 - u * u) * (1 - 0.25 * max(0.0, -k) / 4) + 0.5 * lump(x + side * k, t, 70))
        if hb < 1:
            continue
        cx = x + side * k
        d.line([(cx, G - hb + 1), (cx, G + 1)], fill=fx.S4)
        d.line([(cx, G - hb), (cx, G - hb + max(0, round(hb * 0.6) - 1))], fill=fx.S5)
        if u < 0.4:
            d.point((cx, G - hb), fill=fx.S6)
        if hb >= 2 and u > 0.25:
            d.point((cx, G + 2), fill=fx.S3)
    if h >= 5:
        # the curl under the crest, on the face it pushes
        for j in range(2):
            d.point((x + side * (3 + j), G - h + 2 + j), fill=fx.S3)


def scene():
    stage = Image.open(D / "fight" / "alphahunt-empty.png").convert("RGBA")
    team = bs.the_squad()
    still = bs.strip_base(Image.open(A / "shark-land-256.png"), floor=256)
    haul, breathe, walk, bite, sweep = (clip(n) for n in ("shark_haul", "shark_breathe", "shark_walk", "shark_bite2",
                                                          "shark_sweep"))
    bb = still.getbbox()
    BELLY = bs.feet_row(still)
    SINK = BELLY - CUT_ROW
    X0 = round(505 - (bb[0] + bb[2]) / 2)
    Y0 = G - CUT_ROW                # half in the sand
    Y0_AIR = G - BELLY              # belly on the sand: the haul's flight
    level = Image.open(A / "shark_clean" / "00.png").convert("RGBA")
    fin = level.rotate(25, resample=Image.NEAREST, expand=True)
    fin = fin.crop(fin.getbbox())
    CUT = 40
    vis = np.nonzero((np.asarray(fin)[:CUT, :, 3] > 0).any(0))[0]
    FRONT, BACK = vis.min() - fin.width / 2, vis.max() - fin.width / 2
    base0 = np.nonzero((np.asarray(haul[0])[200:216, :, 3] > 0).any(0))[0]
    ERUPT = (X0 + (base0.min() + base0.max()) / 2 if len(base0) else X0 + 190, G + 4)
    RXH = 46
    L = 210

    # timeline, ms
    PAT = ((665, 330), (ERUPT[0] + 30, G + 6), 0, 1800, [0, 900])
    T_HAUL = 1900
    HAUL_MS = [45] * 41
    T_LANDED = T_HAUL + sum(HAUL_MS[:LAND_F])
    T_OUT = T_HAUL + sum(HAUL_MS[:12])       # the tail is clear of the hole
    T_BREATHE = T_HAUL + sum(HAUL_MS)
    T_WALK = T_BREATHE + 41 * 60
    WALK_MS = 41 * 50
    T_BITE = T_WALK + WALK_MS
    BITE_MS = [50] * 41
    BITE_MS[SNAP_F] = 50 + FREEZE + 60
    T_SNAP = T_BITE + sum(BITE_MS[:SNAP_F])
    T_SWEEP = T_BITE + sum(BITE_MS)
    SWEEP_MS = [55] * 41
    T_BLAST = T_SWEEP + sum(SWEEP_MS[:SWEEP_F])
    T_DIVE = T_SWEEP + sum(SWEEP_MS)
    DIVE_MS = 280
    T_DART = T_DIVE + DIVE_MS
    T_UP = T_DART + 450
    R0, R1 = (650, 320), (540, 312)
    SEGR = (R0, R1, T_UP, 1700, [0, 900])
    T_END = T_UP + 1700
    BLAST_MS = 520
    STOP = 180

    def at(t0, durs, t):
        u = t - t0
        for i, m in enumerate(durs):
            if u < m:
                return i
            u -= m
        return len(durs) - 1

    def mass_x(im):
        """Middle of the shark above the sand line, along x."""
        return float(np.nonzero(np.asarray(im)[:CUT_ROW, :, 3] > 0)[1].mean())
    # The fin walk clip rocks the whole body forward on each step and back on the recovery. With the fins under
    # the sand that reads as sliding backward, so the plow keeps each forward push from the clip, swaps the
    # backward slide for a slow creep, and is scaled to cover WALK_SHIFT. The clip's own rocking is cancelled by
    # placing every frame from where its body sits in the canvas.
    WALK_MX = [mass_x(w) for w in walk]
    MX0, MX1 = WALK_MX[0], mass_x(bite[0])
    push = [0.0] + [max(0.4, WALK_MX[i - 1] - WALK_MX[i]) for i in range(1, 41)]
    # body middle runs from X0 + MX0 (walk frame 0 at X0) to X0 - WALK_SHIFT + MX1 (bite frame 0 where it bites)
    PLOW = list(np.cumsum(push) * (WALK_SHIFT + MX0 - MX1) / sum(push))

    def plow(t):
        """How far the body has travelled into the plow, half size pixels, at time t."""
        f = min(40.0, max(0.0, (t - T_WALK) / 50))
        i = int(f)
        return PLOW[i] + (PLOW[min(40, i + 1)] - PLOW[i]) * (f - i)

    def shift(t):
        if t < T_WALK:
            return 0.0
        if t < T_BITE:
            return -plow(t)
        return -WALK_SHIFT

    def body_top(im):
        a = np.asarray(im)[..., 3] > 0
        return int(np.nonzero(a.sum(1) >= 40)[0].min())
    TOP = body_top(still)
    # the haul's landing frames lie flatter and lower in the canvas than the still; sink each one so the same
    # amount of shark shows above the sand as the still does at row 148, reaching row 148 exactly on frame 40
    HAUL_SINK = [max(0, min(SINK, SINK - (body_top(h) - TOP))) for h in haul]

    def sink_at(t):
        """How far the landed shark has sunk below belly-on-the-sand: fast, a little past, then settles."""
        if t < T_LANDED:
            return 0.0
        f = min(40.0, (t - T_HAUL) / HAUL_MS[0])
        i = int(f)
        target = HAUL_SINK[i] + (HAUL_SINK[min(40, i + 1)] - HAUL_SINK[i]) * (f - i)
        u = min(1.0, (t - T_LANDED) / SINK_MS)
        return target * (1 - (1 - u) ** 3) + 5 * math.sin(math.pi * u) * (1 - u)

    hx = head_of(bite[SNAP_F])[0]
    snap_a = np.asarray(bite[SNAP_F])[CUT_ROW - 12:CUT_ROW - 2, :, 3] > 0
    mx = int(np.nonzero(snap_a.any(0))[0].min())
    my = CUT_ROW - 12 + int(np.nonzero(snap_a[:, mx])[0].mean())
    JAWS = (X0 - WALK_SHIFT + mx + 4, Y0 + my)     # the hit star sits on the clamped teeth
    MOUTH_FLOOR = ((X0 - WALK_SHIFT + hx) * 2, G * 2)
    DIVE_HOLE = (X0 - WALK_SHIFT + head_of(sweep[40])[0] + 12, G)

    def blast_x(u, y):
        x0 = X0 - WALK_SHIFT + bb[0]
        xf = x0 - (x0 - STOP) * (1 - (1 - min(1.0, u / BLAST_MS)) ** 2)
        return xf + 22 * ((y - 288) / 105) ** 2

    for f in team:
        f.blow(T_LANDED, ((X0 + 128) * 2, G * 2), 4, fear=True)
        if f.x > 500:
            f.blow(T_SNAP, MOUTH_FLOOR, 8, lift=4)
        for u in range(0, BLAST_MS + 1, TICK):
            if blast_x(u, f.y / 2) * 2 <= f.x:
                f.blow(T_BLAST + u, (f.x + 80, f.y), 8, lift=6)
                break
    shakes = [(T_HAUL, 0.35), (T_LANDED, 0.6), (T_LANDED + SINK_MS // 2, 0.25), (T_SNAP, 0.65), (T_BLAST, 0.7),
              (T_DIVE + 60, 0.35)]
    bits, grains, rnd = fx.Bits(5), bs.Grains(), random.Random(12)
    furrow, spikes, out = [], [], []
    cover = np.full(720, -1e9)
    FILL_NOISE = np.random.default_rng(7).uniform(0.45, 1.15, 720)
    edge_l = edge_r = None
    bow_l = bow_r = 0.0
    streams = []
    step_at = -80

    def now(t, T):
        return t <= T < t + TICK

    def sand(n, x, y, up, spread, ground=None):
        for _ in range(n):
            grains.add(x + rnd.uniform(-spread, spread), y - 1, rnd.uniform(-1.0, 1.0), -rnd.uniform(0.6, up),
                       (ground if ground is not None else y) + rnd.uniform(-2, 6), rnd.choice(SAND))

    def top_edge(body):
        """Points on the body's upper outline above the sand line, in layer coords."""
        a = np.asarray(body)[..., 3] > 0
        a[G - 2:] = False
        up = np.zeros_like(a)
        up[1:] = a[:-1]
        ys, xs = np.nonzero(a & ~up)
        return list(zip(xs.tolist(), ys.tolist()))

    def pour(body, n, spread=0.3):
        pts = top_edge(body)
        for _ in range(n if pts else 0):
            x, y = rnd.choice(pts)
            grains.add(x + rnd.uniform(-0.4, 0.4), y, rnd.uniform(-spread, spread), rnd.uniform(-0.3, 0.2),
                       G + rnd.uniform(-1, 3), rnd.choice(SAND))

    def new_streams(body, k, dur, t):
        pts = top_edge(body)
        if not pts:
            return
        lo, hi = min(p[0] for p in pts), max(p[0] for p in pts)
        for n in range(k):
            streams.append([lo + (hi - lo) * (n + 0.5) / k + rnd.uniform(-8, 8), t, dur])

    t = 0
    while t < T_END:
        frozen = T_SNAP + 50 <= t < T_SNAP + 50 + FREEZE
        # what the shark is this tick: an under the sand state, or a land frame and where it sits
        under, frame_img, fx0, fy0 = None, None, X0 + shift(t), Y0
        if t < T_HAUL:
            x, y, kick, sink = sp.glide(*PAT, t)
            sink = max(sink, CUT * max(0.0, (t - 1650) / 250))
            under = dict(x=x, y=y, left=True, sink=sink, kick=kick, depth=sink / 14, fin=True)
        elif t < T_BREATHE:
            frame_img = haul[at(T_HAUL, HAUL_MS, t)]
            fy0 = Y0_AIR + sink_at(t)
        elif t < T_WALK:
            frame_img = breathe[min(40, (t - T_BREATHE) // 60)]
        elif t < T_BITE:
            wi = min(40, (t - T_WALK) // 50)
            frame_img = walk[wi]
            fx0 = X0 + MX0 - plow(t) - WALK_MX[wi]
        elif t < T_SWEEP:
            frame_img = bite[at(T_BITE, BITE_MS, t)]
        elif t < T_DIVE:
            frame_img = sweep[at(T_SWEEP, SWEEP_MS, t)]
        elif t < T_DART:
            frame_img = sweep[40]
        elif t < T_UP:
            u = (t - T_DART) / (T_UP - T_DART)
            e = u * u * (3 - 2 * u)
            under = dict(x=DIVE_HOLE[0] + (R0[0] - DIVE_HOLE[0]) * e, y=G + (R0[1] - G) * e, left=u > 0.8, sink=CUT,
                         kick=1.0, depth=0.8 - 0.4 * u, fin=False)
        else:
            x, y, kick, sink = sp.glide(*SEGR, t)
            under = dict(x=x, y=y, left=True, sink=max(CUT * (1 - (t - T_UP) / 160), sink), kick=kick,
                         depth=sink / 14, fin=True)
        bite_i = at(T_BITE, BITE_MS, t) if T_BITE <= t < T_SWEEP else -1
        thrash = 1.0 if THRASH[0] <= bite_i <= THRASH[1] else 0.0

        # the body, cut at the sand line
        body = bs.layer()
        if frame_img is not None:
            if t < T_OUT:
                hole_x = ERUPT[0]
                fx.place(body, frame_img, (0, 0), fx0, fy0,
                         clip=lambda xs, ys: ys < G + 4 + 6 * np.sqrt(np.clip(1 - ((xs - hole_x) / RXH) ** 2, 0, 1)))
            elif t >= T_DIVE:
                u = min(1.0, (t - T_DIVE) / DIVE_MS)
                rot = frame_img.rotate(52 * u, resample=Image.NEAREST, center=(head_of(frame_img)[0] + 10, CUT_ROW))
                fx.place(body, rot, (0, 0), fx0 - 8 * u, fy0 + 150 * u * u, clip=lambda xs, ys: ys < G)
            else:
                fx.place(body, frame_img, (0, 0), fx0, fy0, clip=lambda xs, ys: ys < G)
        in_sand = frame_img is not None and t >= T_LANDED
        spans = spans_of(np.asarray(body)[G - 1, :, 3] > 0) if in_sand else []

        # how hard each end is pushing: the outermost cut edges' speed, kicked up and dying away
        if spans and not frozen:
            l, r = spans[0][0], spans[-1][1]
            if edge_l is not None and T_BREATHE <= t < T_DIVE:
                bow_l = min(6.0, bow_l * 0.86 + min(2.5, max(0.0, edge_l - l)) * 0.8)
                bow_r = min(6.0, bow_r * 0.86 + min(2.5, max(0.0, r - edge_r)) * 0.8)
                if T_WALK <= t < T_BITE:
                    # the plow: the bow never drops below what its forward speed piles up
                    bow_l = max(bow_l, min(6.0, 2.0 + 5.0 * (shift(t - TICK) - shift(t))))
            else:
                bow_l *= 0.8
                bow_r *= 0.8
            edge_l, edge_r = l, r
        elif not spans:
            edge_l = edge_r = None
            bow_l *= 0.8
            bow_r *= 0.8
        for a, b in spans:
            cover[max(0, a):b + 1] = t

        # events
        if now(t, T_HAUL):
            bits.chunks(ERUPT[0], ERUPT[1] - 4, ERUPT[1], 26, 13, spread=0.9, aim=-110)
            sand(36, ERUPT[0], ERUPT[1], 4.0, 22)
        if now(t, T_LANDED):
            fbb = frame_img.getbbox()
            x0, x1 = fx0 + fbb[0], fx0 + fbb[2]
            for k in range(9):
                bits.chunks(x0 + (x1 - x0) * k / 8, G - 2, G, 3, 9, spread=0.9, aim=-90)
                sand(7, x0 + (x1 - x0) * k / 8, G, 3.0, 8)
        if now(t, T_LANDED + SINK_MS - 60):
            new_streams(body, 5, 1500, t)
        if now(t, T_SNAP):
            sand(20, JAWS[0], G, 2.6, 12)
            bits.chunks(JAWS[0] + 6, G - 2, G, 8, 7, spread=0.8, aim=-110)
        if now(t, T_BITE + sum(BITE_MS[:THRASH[0] + 6])):
            new_streams(body, 4, 1100, t)
        if now(t, T_BLAST):
            new_streams(body, 4, 900, t)
        if now(t, T_DIVE + DIVE_MS // 3):
            bits.chunks(DIVE_HOLE[0], G - 3, G, 22, 11, spread=0.9, aim=-90)
            sand(26, DIVE_HOLE[0], G, 3.6, 14)
        if now(t, T_UP):
            bits.chunks(R0[0], R0[1] - 2, R0[1], 10, 8, spread=0.8, aim=-90)
            sand(16, R0[0], R0[1], 3.0, 10)
        if t - step_at >= 80 and not frozen:
            if under and under["fin"] and under["sink"] < CUT * 0.6:
                bits.chunks(under["x"] + FRONT, under["y"] - 1, under["y"], 2, 4, spread=0.4, aim=-120)
            bits.step()
            step_at = t
        if spans and not frozen:
            # grains pouring off the body: a sprinkle from the outline, more after the landing and the thrash
            since_land = t - T_LANDED
            n = 2 + (10 if since_land < 250 else 6 if since_land < 700 else 0) + round(4 * thrash)
            pour(body, n)
            pts = top_edge(body)
            for s in streams:
                if s[1] <= t < s[1] + s[2] and pts and rnd.random() < 0.85:
                    near = [p for p in pts if abs(p[0] - s[0]) < 3]
                    if near:
                        x, y = min(near, key=lambda p: p[1])
                        grains.add(x + rnd.uniform(-0.5, 0.5), y, rnd.uniform(-0.15, 0.15), 0.0, G + rnd.uniform(0, 3),
                                   rnd.choice(SAND))
            # spray off a pushing bow
            for side, e, bw in ((-1, spans[0][0], bow_l), (1, spans[-1][1], bow_r)):
                if bw > 1.2:
                    for _ in range(min(6, round(bw * 0.8))):
                        grains.add(e + side * rnd.uniform(4, 6 + 1.5 * bw), G - 2 - bw * rnd.uniform(0.6, 1.1),
                                   side * rnd.uniform(0.4, 1.4), -rnd.uniform(0.6, 1.6), G + rnd.uniform(-1, 4),
                                   rnd.choice(SAND))
            if thrash and rnd.random() < 0.5:
                a, b = rnd.choice(spans)
                sand(2, rnd.uniform(a, b), G, 2.4, 2)
        # sand sliding back into the furrow
        open_cols = np.nonzero((t - cover > 0) & (t - cover < FILL_MS * 0.8))[0]
        if len(open_cols) and not frozen:
            for x in rnd.sample(list(open_cols), min(3, len(open_cols))):
                grains.add(x, G - 3, rnd.uniform(-0.2, 0.2), 0.0, G + rnd.uniform(-1, 1), rnd.choice(SAND))
        u = t - T_BLAST
        if 0 <= u <= BLAST_MS and (u // TICK) % 2 == 0:
            for y in range(186, 396, 11):
                spikes.append([blast_x(u, y) + rnd.uniform(-3, 3), y + rnd.uniform(-2, 2), t, rnd.uniform(9, 16)])
                if rnd.random() < 0.3:
                    sand(1, blast_x(u, y), y, 2.6, 2)
        spikes = [s for s in spikes if t - s[2] < 200]
        if not frozen:
            grains.step()

        # ground: shadow, furrows, wake, fin, holes
        ground = bs.layer()
        gd = ImageDraw.Draw(ground)
        if under:
            sp.shadow(gd, under["x"] + 12, under["y"] + 8, L, under["left"], under["depth"], t, under["kick"])
            furrow.append((t, under["x"], under["y"] + 1))
        furrow = [p for p in furrow if t - p[0] < 1500]
        for (t0, x0, y0), (_, x1, y1) in zip(furrow, furrow[1:]):
            age = t - t0
            if age < 700:
                w = 4 if age < 300 else 3
                gd.line([(round(x0), round(y0) - w), (round(x1), round(y1) - w)], fill=fx.S6, width=1)
                gd.line([(round(x0), round(y0) + w), (round(x1), round(y1) + w)], fill=fx.S3, width=1)
        # the land furrow: where the body was and isn't now, a trough between two ridges, closing up
        covered = np.zeros(720, bool)
        for a, b in spans:
            covered[max(0, a - 3):b + 4] = True
        for x in np.nonzero((t - cover > 0) & (t - cover < FILL_MS) & ~covered)[0]:
            w = round(2.8 * FILL_NOISE[x] * (1 - (t - cover[x]) / FILL_MS) ** 0.8)
            x = int(x)
            if w >= 2:
                gd.point((x, G - w - 1 + (x % 3 == 0)), fill=fx.S5)
                gd.line([(x, G - w + 1), (x, G + w - 2)], fill=fx.S3)
                gd.point((x, G + w - 1), fill=fx.S5)
            elif w == 1 and x % 4:
                gd.point((x, G), fill=fx.S3)
        if under and under["fin"] and under["sink"] < CUT * 0.8:
            bs.draw_wake(gd, under["x"], under["y"], under["left"], FRONT, BACK)
        if T_HAUL <= t < T_OUT:
            fx.hole(gd, ERUPT[0], ERUPT[1], RXH, 11)
        if T_OUT <= t < T_HAUL + 1500:
            fx.crater(gd, ERUPT[0], ERUPT[1], RXH - 4, 10, dark=t < T_HAUL + 900)
        if T_DART <= t < T_DART + 1300:
            fx.crater(gd, DIVE_HOLE[0], G, 34, 10, dark=t < T_DART + 600)
        if under and under["fin"] and under["sink"] < CUT:
            line = under["y"]
            bob = round(1.5 * math.sin(t * 0.012))
            fx.place(ground, fin, (fin.width / 2, CUT), under["x"], line + bob + under["sink"],
                     clip=lambda xs, ys, ln=line: ys < ln)

        # the shark and its rim
        beast = bs.layer()
        bd = ImageDraw.Draw(beast)
        splash_k = (t - T_LANDED) // TICK
        land_splash = [16, 26, 32, 28, 20, 12, 6][splash_k] if in_sand and 0 <= splash_k < 7 else 0
        dive_k = (t - T_DIVE - DIVE_MS // 3) // TICK
        dive_splash = [14, 24, 28, 22, 14, 7][dive_k] if 0 <= dive_k < 6 else 0
        if land_splash:
            for a, b in spans:
                n = max(1, (b - a) // 30)
                for k in range(n):
                    fx.spikes(bd, a + (b - a) * (k + 0.5) / n, G - 1, t // TICK * 13 + k, land_splash,
                              dirs=[-130, -110, -90, -70, -50], light=False)
        if dive_splash:
            fx.spikes(bd, DIVE_HOLE[0], G - 1, t // TICK, dive_splash, dirs=[-130, -110, -90, -70, -50], light=False)
        lips = [lip_heights(a, b, t, 1.0 + 0.8 * thrash + (0.6 if 0 <= t - T_LANDED < 600 else 0.0)) for a, b in spans]
        for xs, hs in lips:
            body = shade_above(body, xs, hs)
        beast.alpha_composite(body)
        if T_HAUL <= t < T_OUT:
            fx.front_lip(bd, ERUPT[0], ERUPT[1], RXH, 11)
            if t < T_HAUL + 4 * TICK:
                sp.splash(bd, ERUPT[0], ERUPT[1], [30, 46, 40, 26][(t - T_HAUL) // TICK], t // TICK)
        for xs, hs in lips:
            draw_lip(bd, xs, hs)
        if spans:
            if bow_l > 0.5:
                draw_bow(bd, spans[0][0] - 1, 2 + 1.15 * bow_l, -1, t)
            if bow_r > 0.5:
                draw_bow(bd, spans[-1][1] + 1, 2 + 1.15 * bow_r, 1, t)
        if land_splash:
            for a, b in spans:
                n = max(1, (b - a) // 30)
                for k in range(n + 1):
                    fx.spikes(bd, a + (b - a) * k / n, G + 1, t // TICK * 17 + k, land_splash * 0.7,
                              dirs=[-160, -135, -110, -70, -45, -20])
        if dive_splash:
            fx.spikes(bd, DIVE_HOLE[0], G + 1, t // TICK + 5, dive_splash * 0.7, dirs=[-165, -140, -40, -15])
        if T_SNAP <= t < T_SNAP + 50 + FREEZE:
            bs.star(bd, JAWS[0], JAWS[1], 12 if t < T_SNAP + 60 else 8, WHITE)
        grains.draw(bd)

        bands = [bs.layer() for _ in range(4)]
        bdraw = [ImageDraw.Draw(b) for b in bands]
        for x, y, t0, H in spikes:
            age = t - t0
            h = H * min(1.0, age / 40) if age < 90 else H * max(0.0, 1 - (age - 90) / 110)
            b = 0 if y * 2 < 425 else 1 if y * 2 < 564 else 2 if y * 2 < 705 else 3
            sand_spike(bdraw[b], x, y, h, w=4)

        over = bs.layer()
        bits.draw(ImageDraw.Draw(over))

        img = stage.copy()
        img.alpha_composite(bs.big(ground))
        img.alpha_composite(bs.big(bands[0]))
        rows = [[f for f in team if f.y < 500], [f for f in team if 500 <= f.y < 640], [f for f in team if f.y >= 640]]
        for f in rows[0]:
            f.draw(img, t)
        img.alpha_composite(bs.big(bands[1]))
        for f in rows[1]:
            f.draw(img, t)
        img.alpha_composite(bs.big(beast))
        img.alpha_composite(bs.big(bands[2]))
        for f in rows[2]:
            f.draw(img, t)
        img.alpha_composite(bs.big(bands[3]))
        img.alpha_composite(bs.big(over))
        out.append(bs.shaken(img, stage, bs.quake(shakes, t)).convert("RGB"))
        t += TICK

    bs.save_mp4(out, A / "shark-land2.mp4")
    marks = [t // TICK for t in (900, T_HAUL + 400, T_LANDED + 60, T_LANDED + SINK_MS + 200, T_BREATHE + 1200,
                                 T_WALK + 1000, T_SNAP + 60, T_BITE + sum(BITE_MS[:20]), T_BLAST + 260,
                                 T_DIVE + 140, T_DART + 300, T_UP + 700)]
    bs.key_sheet(out, marks, A / "shark-land2-keys.png")
    times = dict(landed=T_LANDED, walk=T_WALK, bite=T_BITE, snap=T_SNAP, sweep=T_SWEEP, blast=T_BLAST, dive=T_DIVE,
                 end=T_END)
    print("shark half in the sand", len(out), "frames;", " ".join("%s %d" % kv for kv in times.items()))
    return out, times


if __name__ == "__main__":
    scene()
