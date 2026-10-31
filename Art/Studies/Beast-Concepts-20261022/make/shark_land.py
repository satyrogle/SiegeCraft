"""The sand shark that breathes out of water (Jacob, 24 Oct: "if i told you the shark can breathe out of water").
Like an epaulette shark it hauls out and walks on its front fins. Its fin and shadow patrol under the sand; it
bursts out and crashes down on its belly; it breathes on land, gills flaring; it walks at the squad on its fins; it
lunges and bites the front of the squad; it spins and its tail sweeps a blast of sand through all five; then it
dives back under head first and its fin patrols again.

PixelLab clips in anim2 (shark_haul, shark_breathe, shark_walk, shark_bite, shark_sweep; 256 canvases shown at 2x),
the presence idle from shark_presence.py, the squad and the hit stack from boss_scenes.py.

python -I shark_land.py   (artlab-env python: scipy, av)
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

G = 283                       # the land shark's belly line, half size (566 on screen)
LAND_F, SNAP_F, SWEEP_F = 22, 20, 18
WALK_SHIFT = 50               # half size pixels the walk carries it toward the squad
SAND = (fx.S4, fx.S5, fx.S6)


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


def scene():
    stage = Image.open(D / "fight" / "alphahunt-empty.png").convert("RGBA")
    team = bs.the_squad()
    still = bs.strip_base(Image.open(A / "shark-land-256.png"), floor=256)
    haul, breathe, walk, bite, sweep = (clip(n) for n in ("shark_haul", "shark_breathe", "shark_walk", "shark_bite",
                                                          "shark_sweep"))
    bb = still.getbbox()
    BELLY = bs.feet_row(still)
    X0 = round(505 - (bb[0] + bb[2]) / 2)
    Y0 = G - BELLY
    LEN = bb[2] - bb[0]
    level = Image.open(A / "shark_clean" / "00.png").convert("RGBA")
    fin = level.rotate(25, resample=Image.NEAREST, expand=True)
    fin = fin.crop(fin.getbbox())
    CUT = 40
    vis = np.nonzero((np.asarray(fin)[:CUT, :, 3] > 0).any(0))[0]
    FRONT, BACK = vis.min() - fin.width / 2, vis.max() - fin.width / 2
    base0 = np.nonzero((np.asarray(haul[0])[200:216, :, 3] > 0).any(0))[0]
    ERUPT = (X0 + (base0.min() + base0.max()) / 2 if len(base0) else X0 + 190, G + 4)
    L = 210

    # timeline, ms
    PAT = ((665, 330), (ERUPT[0] + 30, G + 6), 0, 1800, [0, 900])
    T_HAUL = 1900
    HAUL_MS = [45] * 41
    T_LANDED = T_HAUL + sum(HAUL_MS[:LAND_F])
    T_BREATHE = T_HAUL + sum(HAUL_MS)
    T_WALK = T_BREATHE + 41 * 60
    T_BITE = T_WALK + 41 * 50
    BITE_MS = [50] * 41
    BITE_MS[SNAP_F] = 150
    T_SNAP = T_BITE + sum(BITE_MS[:SNAP_F])
    T_SWEEP = T_BITE + sum(BITE_MS)
    SWEEP_MS = [55] * 41
    T_BLAST = T_SWEEP + sum(SWEEP_MS[:SWEEP_F])
    T_DIVE = T_SWEEP + sum(SWEEP_MS)
    DIVE_MS = 220
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

    def shift(t):
        if t < T_WALK:
            return 0.0
        u = min(1.0, (t - T_WALK) / (41 * 50))
        return -WALK_SHIFT * (u + 0.05 * math.sin(2 * math.pi * 4 * u))

    hx, hy = head_of(bite[SNAP_F])
    JAWS = (X0 - WALK_SHIFT + hx, Y0 + hy)
    MOUTH_FLOOR = ((X0 - WALK_SHIFT + hx) * 2, G * 2)
    DIVE_HOLE = (X0 - WALK_SHIFT + head_of(sweep[40])[0] + 12, G)

    def blast_x(u, y):
        x0 = X0 - WALK_SHIFT + bb[0]
        xf = x0 - (x0 - STOP) * (1 - (1 - min(1.0, u / BLAST_MS)) ** 2)
        return xf + 22 * ((y - 288) / 105) ** 2

    for f in team:
        f.blow(T_LANDED, ((X0 + 128) * 2, G * 2), 4, fear=True)
        if f.x > 500:
            f.blow(T_SNAP, MOUTH_FLOOR, 18, lift=6)
        for u in range(0, BLAST_MS + 1, TICK):
            if blast_x(u, f.y / 2) * 2 <= f.x:
                f.blow(T_BLAST + u, (f.x + 80, f.y), 14, lift=10)
                break
    shakes = [(T_HAUL, 0.35), (T_LANDED, 0.55), (T_SNAP, 0.6), (T_BLAST, 0.7), (T_DIVE + 60, 0.3)]
    bits, grains, rnd = fx.Bits(5), bs.Grains(), random.Random(12)
    furrow, drag, spikes, out = [], [], [], []
    step_at = -80

    def now(t, T):
        return t <= T < t + TICK

    def sand(n, x, y, up, spread, ground=None):
        for _ in range(n):
            grains.add(x + rnd.uniform(-spread, spread), y - 1, rnd.uniform(-1.0, 1.0), -rnd.uniform(0.6, up),
                       (ground if ground is not None else y) + rnd.uniform(-2, 6), rnd.choice(SAND))

    t = 0
    while t < T_END:
        # what the shark is this tick: an under the sand state, or a land frame and where it stands
        under, frame_img, fx0 = None, None, X0 + shift(t)
        if t < T_HAUL:
            x, y, kick, sink = sp.glide(*PAT, t)
            sink = max(sink, CUT * max(0.0, (t - 1650) / 250))
            under = dict(x=x, y=y, left=True, sink=sink, kick=kick, depth=sink / 14, fin=True)
        elif t < T_BREATHE:
            frame_img = haul[at(T_HAUL, HAUL_MS, t)]
        elif t < T_WALK:
            frame_img = breathe[min(40, (t - T_BREATHE) // 60)]
        elif t < T_BITE:
            frame_img = walk[min(40, (t - T_WALK) // 50)]
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

        # events
        if now(t, T_HAUL):
            bits.chunks(ERUPT[0], ERUPT[1] - 4, ERUPT[1], 22, 13, spread=0.9, aim=-110)
            sand(30, ERUPT[0], ERUPT[1], 4.0, 16)
        if now(t, T_LANDED):
            bits.chunks(X0 + bb[0] + LEN * 0.5, G - 2, G, 16, 9, spread=1.0, aim=-90)
            for k in range(8):
                sand(5, X0 + bb[0] + LEN * k / 7, G, 2.6, 6)
        if now(t, T_SNAP):
            sand(16, JAWS[0], G, 2.4, 10)
        if now(t, T_DIVE):
            bits.chunks(DIVE_HOLE[0], G - 3, G, 20, 11, spread=0.9, aim=-90)
            sand(24, DIVE_HOLE[0], G, 3.6, 14)
        if now(t, T_UP):
            bits.chunks(R0[0], R0[1] - 2, R0[1], 10, 8, spread=0.8, aim=-90)
            sand(16, R0[0], R0[1], 3.0, 10)
        if t - step_at >= 80:
            if under and under["fin"] and under["sink"] < CUT * 0.6:
                bits.chunks(under["x"] + FRONT, under["y"] - 1, under["y"], 2, 4, spread=0.4, aim=-120)
            bits.step()
            step_at = t
        if frame_img is not None and T_BREATHE <= t < T_DIVE:
            edge = bs.edge_points(frame_img, 256)
            for _ in range(2):
                cx, cy = rnd.choice(edge)
                if cy < BELLY - 6:
                    grains.add(fx0 + cx, Y0 + cy, rnd.uniform(-0.3, 0.3), 0.0, G + rnd.uniform(-2, 6), rnd.choice(SAND))
        if T_WALK <= t < T_BITE and (t - T_WALK) % 250 < TICK:
            sand(6, fx0 + bb[0] + 34, G, 1.8, 5)
        u = t - T_BLAST
        if 0 <= u <= BLAST_MS and (u // TICK) % 2 == 0:
            for y in range(186, 396, 11):
                spikes.append([blast_x(u, y) + rnd.uniform(-3, 3), y + rnd.uniform(-2, 2), t, rnd.uniform(9, 16)])
                if rnd.random() < 0.3:
                    sand(1, blast_x(u, y), y, 2.6, 2)
        spikes = [s for s in spikes if t - s[2] < 200]
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
        if under and under["fin"] and under["sink"] < CUT * 0.8:
            bs.draw_wake(gd, under["x"], under["y"], under["left"], FRONT, BACK)
        if frame_img is not None and t >= T_LANDED:
            fbb = frame_img.getbbox()
            cx = fx0 + (fbb[0] + fbb[2]) / 2
            gd.ellipse((round(cx - (fbb[2] - fbb[0]) * 0.46), round(G - 5), round(cx + (fbb[2] - fbb[0]) * 0.46),
                        round(G + 7)), fill=sp.SHADOW + (110,))
        if t >= T_WALK:
            x_now = fx0 + bb[0] + 40
            x_start = X0 + bb[0] + 40
            if x_now < x_start - 2:
                gd.line([(round(x_now), G + 2), (round(x_start), G + 2)], fill=fx.S3, width=1)
                gd.line([(round(x_now), G - 1), (round(x_start), G - 1)], fill=fx.S6, width=1)
        if T_HAUL <= t < T_HAUL + 8 * TICK:
            fx.hole(gd, ERUPT[0], ERUPT[1], 34, 10)
        if T_HAUL + 8 * TICK <= t < T_HAUL + 1400:
            fx.crater(gd, ERUPT[0], ERUPT[1], 30, 9, dark=t < T_HAUL + 700)
        if T_DIVE <= t < T_DART + 120:
            fx.hole(gd, DIVE_HOLE[0], G, 40, 11)
        if T_DART + 120 <= t < T_DART + 1400:
            fx.crater(gd, DIVE_HOLE[0], G, 34, 10, dark=t < T_DART + 600)
        if under and under["fin"] and under["sink"] < CUT:
            line = under["y"]
            bob = round(1.5 * math.sin(t * 0.012))
            fx.place(ground, fin, (fin.width / 2, CUT), under["x"], line + bob + under["sink"],
                     clip=lambda xs, ys, ln=line: ys < ln)

        # the shark on land
        beast = bs.layer()
        bd = ImageDraw.Draw(beast)
        if frame_img is not None:
            if t < T_HAUL + 4 * TICK:
                hole_x = ERUPT[0]
                fx.place(beast, frame_img, (0, 0), fx0, Y0,
                         clip=lambda xs, ys: ys < G + 4 + 6 * np.sqrt(np.clip(1 - ((xs - hole_x) / 34) ** 2, 0, 1)))
                fx.front_lip(bd, ERUPT[0], ERUPT[1], 34, 10)
                sp.splash(bd, ERUPT[0], ERUPT[1], [26, 40, 34, 22][(t - T_HAUL) // TICK], t // TICK)
            elif t >= T_DIVE:
                u = min(1.0, (t - T_DIVE) / DIVE_MS)
                rot = frame_img.rotate(52 * u, resample=Image.NEAREST, center=(head_of(frame_img)[0] + 10, BELLY))
                fx.place(beast, rot, (0, 0), fx0 - 8 * u, Y0 + 150 * u * u,
                         clip=lambda xs, ys: ys < G + 6 * np.sqrt(np.clip(1 - ((xs - DIVE_HOLE[0]) / 40) ** 2, 0, 1)))
                fx.front_lip(bd, DIVE_HOLE[0], G, 40, 11)
                if t < T_DIVE + 5 * TICK:
                    sp.splash(bd, DIVE_HOLE[0], G, [24, 36, 30, 20, 12][(t - T_DIVE) // TICK], t // TICK)
            else:
                beast.alpha_composite(frame_img, (round(fx0), round(Y0)))
            if T_SNAP <= t < T_SNAP + 2 * TICK:
                bs.star(bd, JAWS[0], JAWS[1], 12, WHITE)
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

    bs.save_mp4(out, A / "shark-land.mp4")
    marks = [t // TICK for t in (900, T_HAUL + 300, T_LANDED + 60, T_BREATHE + 1200, T_WALK + 1200, T_SNAP + 20,
                                 T_BLAST + 260, T_DIVE + 120, T_UP + 700)]
    bs.key_sheet(out, marks, A / "shark-land-keys.png")
    print("shark on land", len(out), "frames; landed", T_LANDED, "snap", T_SNAP, "blast", T_BLAST, "end", T_END)


if __name__ == "__main__":
    scene()
