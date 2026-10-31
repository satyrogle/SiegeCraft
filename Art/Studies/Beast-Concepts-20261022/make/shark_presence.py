"""The sand shark's boss scene with presence in its idle (Jacob, 24 Oct, from his research on Resident Evil 4's Del
Lago, Dave the Diver's sharks and Sea of Thieves' Megalodon):
1. the iceberg: its whole body as a dark shape under the sand, its tail sweeping;
2. glide and kick: it drifts and slows, then a tail kick bursts it on with a spray of sand, sinking and rising;
3. a sand furrow along its path that slowly fills back in;
4. rumble: a small shake, and the squad flinches, when it passes close;
5. it reacts: your Marksman shoots at the fin, it dives with a splash, darts away under the sand and comes up
   somewhere else, facing you.
Then the attack from boss_scenes.py: it breaks for the squad, its shadow slides under them and rounds as it turns to
breach, it bursts up through them, bites, sinks with a sand wave, and its fin comes back up to patrol.

python -I shark_presence.py   (artlab-env python: scipy, av)
"""
import math
import pathlib
import random
import sys

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
import boss_scenes as bs  # noqa: E402
from boss_scenes import A, D, TICK, WHITE, fx  # noqa: E402

SHADOW = (60, 26, 16)


def shadow(d, x, y, L, left, depth, t, kick, xscale=1.0, round_up=0.0):
    """The iceberg: the whole shark seen from above as a dark shape under the sand. The tail sweeps, harder on a
    kick; deeper is fainter and smaller; round_up squeezes it toward a blob as it turns nose up to breach."""
    s = 1 if left else -1
    body = [(-0.50, 0.0), (-0.38, -0.07), (-0.15, -0.10), (0.10, -0.085), (0.30, -0.05), (0.42, -0.02), (0.45, -0.012),
            (0.45, 0.012), (0.42, 0.02), (0.30, 0.05), (0.10, 0.085), (-0.15, 0.10), (-0.38, 0.07)]
    parts = [body,
             [(-0.12, -0.08), (0.02, -0.21), (0.06, -0.07)], [(-0.12, 0.08), (0.02, 0.21), (0.06, 0.07)],
             [(0.43, -0.01), (0.58, -0.13), (0.50, 0.0)], [(0.43, 0.01), (0.58, 0.13), (0.50, 0.0)]]
    sway = (0.03 + 0.09 * kick) * math.sin(t * 0.018)
    k = 1 - 0.12 * depth
    along = L * k * xscale * (1 - 0.62 * round_up)
    across = L * k * 0.7 * (1 + 0.9 * round_up)
    col = SHADOW + (round(150 - 70 * depth + 50 * round_up),)

    def pt(u, v, limb):
        if limb:
            v *= 1 - 0.75 * round_up
        bend = sway * max(0.0, u + 0.1) ** 2 * 2.2
        return round(x + s * u * along), round(y + (v + bend) * across)
    for n, poly in enumerate(parts):
        d.polygon([pt(u, v, n > 0) for u, v in poly], fill=col)


def glide(p0, p1, t0, dur, kicks, t):
    """Progress from p0 to p1 with tail kicks: each kick adds speed that dies away, with a little drift between.
    Returns x, y, how fresh the last kick is (1 right on it), and how far the fin has sunk since."""
    tau = min(max(0.0, t - t0), dur)

    def far(x):
        return 0.2 * x + sum(350 * (1 - math.exp(-(x - k) / 350)) for k in kicks if x >= k)
    s = far(tau) / far(dur)
    since = min([tau - k for k in kicks if k <= tau], default=9999.0)
    return p0[0] + (p1[0] - p0[0]) * s, p0[1] + (p1[1] - p0[1]) * s, math.exp(-since / 250), min(10.0, since / 110)


def splash(d, x, y, size, seed):
    bs.sand_crown(d, x, y, 14, size, True, seed)
    bs.sand_crown(d, x, y, 14, size, False, seed)


def scene():
    stage = Image.open(D / "fight" / "alphahunt-empty.png").convert("RGBA")
    team = bs.the_squad()
    level = Image.open(A / "shark_clean" / "00.png").convert("RGBA")
    fin = level.rotate(25, resample=Image.NEAREST, expand=True)
    fin = fin.crop(fin.getbbox())
    CUT = 40
    vis = np.nonzero((np.asarray(fin)[:CUT, :, 3] > 0).any(0))[0]
    FRONT, BACK = vis.min() - fin.width / 2, vis.max() - fin.width / 2
    vert = [bs.strip_base(Image.open(A / "shark_vbite" / ("%02d.png" % i))) for i in range(41)]
    base = np.nonzero((np.asarray(vert[0])[198:207, :, 3] > 0).any(0))[0]
    BASE_X = (base.min() + base.max()) / 2
    RX = (base.max() - base.min()) / 2 + 10
    edges = [bs.edge_points(v, 200) for v in vert]
    L = 210

    E = (248, 288)
    EF = (E[0] * 2, E[1] * 2)
    DIVE_AT = (410, 288)
    P0, P1, P1B, P2, P3, P4 = (665, 322), (395, 330), (388, 312), (600, 250), (560, 345), (470, 322)
    SEG1 = (P0, P1, 0, 1900, [0, 950])
    T_TURN, TURN_MS = 1900, 240
    SEG2 = (P1B, P2, T_TURN + TURN_MS, 1700, [0, 760])
    T_FIRE, T_HIT = 2840, 3000
    T_DART, DART_MS = 3100, 500
    T_UP = T_DART + DART_MS
    SEG3 = (P3, P4, T_UP, 1200, [100, 700])
    T_BREAK = T_UP + 1200
    T_DIVE, T_UNDER, T_TELL = T_BREAK + 350, T_BREAK + 510, T_BREAK + 750
    T_BREACH = T_TELL + 900
    RISE = [200, 130, 60, 14, -10, -6, 0]
    T_BITE = T_BREACH + len(RISE) * TICK + 100
    BITE_MS = 45
    T_SINK = T_BITE + 41 * BITE_MS
    SINK = [round(230 * (k / 8) ** 2) for k in range(9)] + [230] * 3
    T_GONE = T_SINK + len(SINK) * TICK
    T_WAVE, WAVE_MS = T_SINK + 4 * TICK, 600
    T_RESURF = T_SINK + 700
    R0, R1 = (650, 300), (520, 300)
    SEGR = (R0, R1, T_RESURF, 2000, [0, 1000])
    T_END = T_RESURF + 2000
    CROWN = [26, 48, 64, 58, 44, 28]
    MUZZLE = (229, 322)
    HIT_AT = glide(*SEG2, T_HIT)[:2]
    KICKS = [(SEG1, 0), (SEG1, 950), (SEG2, 0), (SEG2, 760), (SEG3, 100), (SEG3, 700), (SEGR, 0), (SEGR, 1000)]

    def state(t):
        """The fin (x, y, facing, sink, kick, x scale, shown) and the shadow (depth, rounding) at time t."""
        if t < T_TURN:
            x, y, kick, sink = glide(*SEG1, t)
            return dict(x=x, y=y, left=True, sink=sink, kick=kick, xs=1.0, depth=sink / 14, rnd=0.0, fin=True)
        if t < T_TURN + TURN_MS:
            u = (t - T_TURN) / TURN_MS
            return dict(x=P1[0] + (P1B[0] - P1[0]) * u - 10 * math.sin(math.pi * u), y=P1[1] + (P1B[1] - P1[1]) * u,
                        left=u < 0.5, sink=2.0, kick=0.3, xs=max(0.15, abs(math.cos(math.pi * u))), depth=0.2, rnd=0.0,
                        fin=True)
        if t < T_HIT:
            x, y, kick, sink = glide(*SEG2, t)
            return dict(x=x, y=y, left=False, sink=sink, kick=kick, xs=1.0, depth=sink / 14, rnd=0.0, fin=True)
        if t < T_DART:
            u = (t - T_HIT) / (T_DART - T_HIT)
            return dict(x=HIT_AT[0], y=HIT_AT[1], left=False, sink=CUT * u, kick=1.0, xs=1.0, depth=0.3 + 0.5 * u,
                        rnd=0.0, fin=True)
        if t < T_UP:
            u = (t - T_DART) / DART_MS
            e = u * u * (3 - 2 * u)
            return dict(x=HIT_AT[0] + (P3[0] - HIT_AT[0]) * e, y=HIT_AT[1] + (P3[1] - HIT_AT[1]) * e, left=u > 0.8,
                        sink=CUT, kick=1.0, xs=0.3 if 0.7 < u < 0.9 else 1.0, depth=0.9 - 0.5 * u, rnd=0.0, fin=False)
        if t < T_BREAK:
            x, y, kick, sink = glide(*SEG3, t)
            return dict(x=x, y=y, left=True, sink=max(CUT * (1 - (t - T_UP) / 160), sink), kick=kick, xs=1.0,
                        depth=sink / 14, rnd=0.0, fin=True)
        if t < T_DIVE:
            s = ((t - T_BREAK) / (T_DIVE - T_BREAK)) ** 2
            return dict(x=P4[0] + (DIVE_AT[0] - P4[0]) * s, y=P4[1] + (DIVE_AT[1] - P4[1]) * s, left=True, sink=0.0,
                        kick=1.0, xs=1.0, depth=0.0, rnd=0.0, fin=True)
        if t < T_UNDER:
            u = (t - T_DIVE) / (T_UNDER - T_DIVE)
            return dict(x=DIVE_AT[0] - 10 * u, y=DIVE_AT[1], left=True, sink=CUT * u, kick=0.6, xs=1.0, depth=0.5 * u,
                        rnd=0.0, fin=True)
        if t < T_TELL:
            u = (t - T_UNDER) / (T_TELL - T_UNDER)
            x0 = DIVE_AT[0] - 10
            return dict(x=x0 + (E[0] - x0) * u, y=DIVE_AT[1] + (E[1] - DIVE_AT[1]) * u, left=True, sink=CUT, kick=1.0,
                        xs=1.0, depth=0.5 - 0.2 * u, rnd=0.0, fin=False)
        if t < T_BREACH:
            u = (t - T_TELL) / (T_BREACH - T_TELL)
            return dict(x=E[0], y=E[1], left=True, sink=CUT, kick=0.5, xs=1.0, depth=0.3 * (1 - u),
                        rnd=min(1.0, u * 1.4), fin=False)
        if t < T_RESURF:
            return None
        x, y, kick, sink = glide(*SEGR, t)
        return dict(x=x, y=y, left=True, sink=max(CUT * (1 - (t - T_RESURF) / 160), sink), kick=kick, xs=1.0,
                    depth=sink / 14, rnd=0.0, fin=True)

    def shark_pose(t):
        if t < T_BITE:
            k = (t - T_BREACH) // TICK
            return 0, RISE[k] if k < len(RISE) else 0
        if t < T_SINK:
            return min(40, (t - T_BITE) // BITE_MS), 0
        return 40, SINK[min(len(SINK) - 1, (t - T_SINK) // TICK)]

    def near_rim(xs, ys):
        inside = np.clip(1 - ((xs - E[0]) / RX) ** 2, 0, 1)
        return ys < E[1] + 14 * 0.55 * np.sqrt(inside)

    close = (P1[0] * 2, P1[1] * 2)
    for f in team:
        f.tremble.append((T_UNDER, T_BREACH))
        f.blow(T_BREACH + 2 * TICK, EF, 30, lift=24)
        ru = math.hypot(f.x - EF[0], (f.y - EF[1]) / 0.7)
        k = 1 - (ru - 30) / 220
        if 0 <= k <= 1:
            f.blow(T_WAVE + WAVE_MS * (1 - math.sqrt(k)), EF, 10, lift=8)
        if math.hypot(f.x - close[0], f.y - close[1]) < 420:
            f.tremble.append((1500, 1900))
            f.blow(1750, close, 6, fear=True)
    shakes = [(1450, 0.18), (1750, 0.22), (T_HIT, 0.15), (T_TELL, 0.25), (T_TELL + 450, 0.2), (T_BREACH, 1.0),
              (T_BITE + 8 * BITE_MS, 0.35), (T_WAVE, 0.7)]
    cracks = fx.crack_paths(E[0], E[1], 11, n=7, length=44, squash=0.7)
    bits, grains, rnd = fx.Bits(41), bs.Grains(), random.Random(3)
    furrow, out, step_at = [], [], -80

    def now(t, T):
        return t <= T < t + TICK

    def sand(n, x, y, up, spread):
        for _ in range(n):
            grains.add(x + rnd.uniform(-spread, spread), y - 1, rnd.uniform(-1.0, 1.0), -rnd.uniform(0.6, up),
                       y + rnd.uniform(-2, 6), rnd.choice((fx.S4, fx.S5, fx.S6)))

    t = 0
    while t < T_END:
        st = state(t)
        for seg, k in KICKS:
            if now(t, seg[2] + k):
                kx, ky = glide(*seg, seg[2] + k)[:2]
                left = seg[1][0] < seg[0][0]
                tail = kx + (0.5 * L if left else -0.5 * L)
                bits.chunks(tail, ky - 2, ky, 6, 6, spread=0.6, aim=-60 if left else -120)
                sand(14, tail, ky, 3.2, 10)
        if now(t, T_HIT):
            bits.chunks(HIT_AT[0], HIT_AT[1] - 3, HIT_AT[1], 12, 9, spread=0.8, aim=-90)
            sand(24, HIT_AT[0], HIT_AT[1], 3.6, 12)
        if now(t, T_UP):
            bits.chunks(P3[0], P3[1] - 2, P3[1], 10, 8, spread=0.8, aim=-90)
            sand(16, P3[0], P3[1], 3.0, 10)
        if now(t, T_DIVE):
            bits.chunks(DIVE_AT[0], DIVE_AT[1] - 2, DIVE_AT[1], 8, 7, spread=0.7, aim=-90)
        if now(t, T_BREACH):
            bits.chunks(E[0] - 20, E[1] - 6, E[1], 26, 14, spread=0.9, aim=-100)
            bits.chunks(E[0] + 20, E[1] - 6, E[1], 22, 13, spread=0.9, aim=-80)
        if now(t, T_WAVE):
            bits.chunks(E[0], E[1] - 4, E[1], 24, 12, spread=1.0, aim=-90)
        if now(t, T_RESURF):
            bits.chunks(R0[0], R0[1] - 2, R0[1], 10, 8, spread=0.8, aim=-90)
            sand(16, R0[0], R0[1], 3.0, 10)
        if t - step_at >= 80:
            if st and st["fin"] and st["sink"] < CUT * 0.6:
                bow = st["x"] + (FRONT if st["left"] else -FRONT)
                bits.chunks(bow, st["y"] - 1, st["y"], 2, 4, spread=0.4, aim=-120 if st["left"] else -60)
            if T_UNDER <= t < T_TELL:
                bits.chunks(st["x"], st["y"] - 2, st["y"], 3, 5, spread=0.5, aim=-90)
            if T_TELL <= t < T_BREACH:
                sand(3, E[0], E[1], 2.2, 18)
            bits.step()
            step_at = t
        if T_BITE <= t < T_SINK:
            idx = min(40, (t - T_BITE) // BITE_MS)
            for _ in range(5):
                cx, cy = rnd.choice(edges[idx])
                grains.add(E[0] - BASE_X + cx, E[1] + 4 - 207 + cy, rnd.uniform(-0.6, 0.6), rnd.uniform(-0.4, 0.3),
                           E[1] + rnd.uniform(-3, 10), rnd.choice((fx.S4, fx.S5, fx.S6)))
        if T_WAVE <= t < T_WAVE + WAVE_MS:
            rx = bs.wave_rx(t - T_WAVE, WAVE_MS)
            for _ in range(5):
                a = rnd.uniform(0, 2 * math.pi)
                gx, gy = E[0] + rx * math.cos(a), E[1] + rx * 0.7 * math.sin(a)
                grains.add(gx, gy, 0.5 * math.cos(a), -1.3, gy + 1, rnd.choice((fx.S5, fx.S6)))
        grains.step()

        ground = bs.layer()
        gd = ImageDraw.Draw(ground)
        if st:
            sx = st["x"] + (12 if st["left"] else -12)
            shadow(gd, sx, st["y"] + 8, L, st["left"], st["depth"], t, st["kick"], st["xs"], st["rnd"])
            if st["sink"] < CUT or not st["fin"]:
                furrow.append((t, st["x"], st["y"] + 1))
        furrow = [p for p in furrow if t - p[0] < 1500]
        for (t0, x0, y0), (_, x1, y1) in zip(furrow, furrow[1:]):
            age = t - t0
            if age < 700:
                w = 4 if age < 300 else 3
                gd.line([(round(x0), round(y0) - w), (round(x1), round(y1) - w)], fill=fx.S6, width=1)
                gd.line([(round(x0), round(y0) + w), (round(x1), round(y1) + w)], fill=fx.S3, width=1)
            elif (round(x0) * 3 + round(y0)) % 4 == 0:
                gd.point((round(x0), round(y0) - 2), fill=fx.S6)
                gd.point((round(x0), round(y0) + 2), fill=fx.S3)
        if st and st["fin"] and st["sink"] < CUT * 0.8:
            bs.draw_wake(gd, st["x"], st["y"], st["left"], FRONT, BACK)
        if T_HIT <= t < T_HIT + 5 * TICK:
            splash(gd, HIT_AT[0], HIT_AT[1], [20, 30, 26, 18, 10][(t - T_HIT) // TICK], t // TICK)
        if T_UP <= t < T_UP + 4 * TICK:
            splash(gd, P3[0], P3[1], [18, 24, 18, 10][(t - T_UP) // TICK], t // TICK)
        if T_RESURF <= t < T_RESURF + 4 * TICK:
            splash(gd, R0[0], R0[1], [18, 24, 18, 10][(t - T_RESURF) // TICK], t // TICK)
        if T_UNDER <= t < T_TELL:
            fx.mound(gd, st["x"] - 70, st["y"], 12, 4)
        if T_TELL <= t < T_BREACH:
            bs.sinkhole(gd, E[0], E[1], t - T_TELL)
            fx.draw_cracks(gd, cracks, grow=min(1.0, (t - T_TELL) / 700))
        if T_BREACH <= t < T_GONE:
            fx.hole(gd, E[0], E[1], RX, 14)
        if T_GONE <= t < T_GONE + 1400:
            fx.crater(gd, E[0], E[1], RX * 0.8, 12, dark=t < T_GONE + 700)
        if T_WAVE <= t < T_WAVE + WAVE_MS:
            bs.sand_wave(gd, E[0], E[1], t - T_WAVE, WAVE_MS)
        if st and st["fin"] and st["sink"] < CUT:
            spr = fin if st["left"] else fin.transpose(Image.FLIP_LEFT_RIGHT)
            if st["xs"] < 1.0:
                spr = spr.resize((max(1, round(spr.width * st["xs"])), spr.height), Image.NEAREST)
            line = st["y"]
            bob = round(1.5 * math.sin(t * 0.012))
            fx.place(ground, spr, (spr.width / 2, CUT), st["x"], line + bob + st["sink"],
                     clip=lambda xs, ys, ln=line: ys < ln)

        beast = bs.layer()
        bd = ImageDraw.Draw(beast)
        if T_BREACH <= t < T_GONE:
            idx, dy = shark_pose(t)
            kc, ks = (t - T_BREACH) // TICK, (t - T_WAVE) // TICK
            size = CROWN[kc] if kc < len(CROWN) else (CROWN[ks] * 0.7 if 0 <= ks < len(CROWN) else 0)
            if size:
                bs.sand_crown(bd, E[0], E[1], RX, size, True, t // TICK)
            fx.place(beast, vert[idx], (BASE_X, 207), E[0], E[1] + 4 + dy, clip=near_rim)
            fx.front_lip(bd, E[0], E[1], RX, 14)
            if size:
                bs.sand_crown(bd, E[0], E[1], RX, size, False, t // TICK)
                if kc >= 3 or ks >= 3:
                    for _ in range(10):
                        grains.add(E[0] + rnd.uniform(-RX - 24, RX + 24), E[1] - rnd.uniform(8, size * 0.8),
                                   rnd.uniform(-0.9, 0.9), rnd.uniform(-1.2, 0.4), E[1] + rnd.uniform(-2, 12),
                                   rnd.choice((fx.S4, fx.S5, fx.S6)))
        grains.draw(bd)

        over = bs.layer()
        od = ImageDraw.Draw(over)
        bits.draw(od)
        if T_FIRE <= t < T_FIRE + 2 * TICK:
            bs.star(od, MUZZLE[0], MUZZLE[1], 5, fx.S6)
        if T_FIRE <= t < T_HIT:
            u = (t - T_FIRE) / (T_HIT - T_FIRE)
            hx, hy = MUZZLE[0] + (HIT_AT[0] - MUZZLE[0]) * u, MUZZLE[1] + (HIT_AT[1] - MUZZLE[1]) * u - 6
            dx, dy = HIT_AT[0] - MUZZLE[0], HIT_AT[1] - MUZZLE[1]
            n = math.hypot(dx, dy)
            od.line([(round(hx - dx / n * 22), round(hy - dy / n * 22)), (round(hx), round(hy))], fill=fx.S4, width=3)
            od.line([(round(hx - dx / n * 10), round(hy - dy / n * 10)), (round(hx), round(hy))], fill=WHITE, width=2)

        frame = stage.copy()
        frame.alpha_composite(bs.big(ground))
        for f in team:
            if f.y < EF[1]:
                f.draw(frame, t)
        frame.alpha_composite(bs.big(beast))
        for f in team:
            if f.y >= EF[1]:
                f.draw(frame, t)
        frame.alpha_composite(bs.big(over))
        out.append(bs.shaken(frame, stage, bs.quake(shakes, t)).convert("RGB"))
        t += TICK

    bs.save_mp4(out, A / "shark-boss-v3.mp4")
    marks = [t // TICK for t in (500, 1750, 2020, 2920, 3060, 3350, T_UP + 400, T_UNDER + 150, T_TELL + 600,
                                 T_BREACH + 120, T_WAVE + 200, T_RESURF + 700)]
    bs.key_sheet(out, marks, A / "shark-boss-v3-keys.png")
    print("shark", len(out), "frames; shot", T_HIT, "ms; breach", T_BREACH, "ms; end", T_END, "ms")


if __name__ == "__main__":
    scene()
