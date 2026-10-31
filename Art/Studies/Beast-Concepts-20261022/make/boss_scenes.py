"""Both beasts as bosses in their real fights, the squad live and taking the hits.

Shark (desert Alpha Hunt): its fin circles the enemy half with a wake, breaks toward the squad and dives, a bulge
races under them, the sand churns and sinks under the whole squad (the tell), it bursts straight up through them and
throws everyone, bites at the sky with sand pouring off it, sinks back as a sand wave rolls out, then the fin
resurfaces and circles again.

Bear (Skullpeak trial): alone where the Skullpeak chief stands. It paces and snarls with its heart pulsing, the heart
falters and goes dark, the great skull's eyes flare and a bolt powers it up (the Battle Cat beat), it rears and roars
(a nerve hit on the squad), frost cracks race across the floor (the tell), it slams and a wall of ice spikes rolls
through the whole squad.

Beasts are PixelLab 256 clips shown at 2x; their effects are drawn on a half size layer and doubled so every beast
pixel matches. Squad and stage stay 1x. Hits and shake follow the fight-feel skill; sand debris uses the Dust Shark
kit (fxkit).

python -I boss_scenes.py [shark] [bear]   (artlab-env python: scipy, av)
"""
import json
import math
import pathlib
import random
import sys
from fractions import Fraction

import av
import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage

HERE = pathlib.Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parents[1] / "Chief-Cut-Ins-20261018" / "dust-shark" / "make"))
import fxkit as fx  # noqa: E402

D = HERE.parent / "directions"
A = D / "anim2"
CA = pathlib.Path("C:/Users/jacob/KindredSiege-Clean/Assets/Resources/CampaignArt")
TICK = 20
PLAY = (32, 80, 1408, 834)
ICE = [(236, 242, 248), (178, 196, 214), (120, 140, 168), (40, 52, 70)]
TQ, TQ_LIGHT, WHITE = (64, 224, 208), (196, 255, 246), (250, 252, 255)
FOUR = np.array([[0, 1, 0], [1, 1, 1], [0, 1, 0]])


def solid(spr, col):
    a = np.asarray(spr).copy()
    a[a[..., 3] > 0, :3] = col
    return Image.fromarray(a, "RGBA")


def blend(spr, col, k):
    a = np.asarray(spr).astype(np.float32)
    b = a.copy()
    on = a[..., 3] > 0
    b[on, :3] = a[on, :3] * (1 - k) + np.array(col, np.float32) * k
    return Image.fromarray(b.astype(np.uint8), "RGBA")


class Fighter:
    """A live Clarth fighter facing the beast: white frame, red flickers, jitter, knockback and a hop per blow."""

    def __init__(self, name, x, y):
        meta = json.loads((CA / (name + "_sw_idle.json")).read_text(encoding="utf-8"))
        frs = meta["frames"] if isinstance(meta["frames"], list) else list(meta["frames"].values())
        sheet = Image.open(CA / (name + "_sw_idle.png")).convert("RGBA")
        self.frames = [sheet.crop((f["frame"]["x"], f["frame"]["y"], f["frame"]["x"] + f["frame"]["w"],
                                   f["frame"]["y"] + f["frame"]["h"])).transpose(Image.FLIP_LEFT_RIGHT) for f in frs]
        w = frs[0]["frame"]["w"]
        self.ax, self.ay = w - meta.get("anchorX", w / 2), meta.get("anchorY", frs[0]["frame"]["h"])
        self.x, self.y = x, y
        self.blows, self.tremble = [], []
        self.phase = (x * 7 + y * 3) % 400

    def blow(self, t, src, push, lift=0, fear=False):
        dx, dy = self.x - src[0], self.y - src[1]
        n = math.hypot(dx, dy) or 1.0
        self.blows.append((t, dx / n, dy / n, push, lift, fear))

    def corner(self, t):
        ox = oy = 0.0
        for t0, ux, uy, push, lift, fear in self.blows:
            u = t - t0
            if u < 0:
                continue
            e = u / 60 if u < 60 else 1.0 if u < 180 else max(0.0, 1 - (u - 180) / 520) ** 2
            ox += ux * push * e
            oy += uy * push * e * 0.5
            if lift and u < 420:
                oy -= lift * math.sin(math.pi * u / 420)
            if u < 200 and not fear:
                ox += 1 if (u // 40) % 2 else -1
        for t0, t1 in self.tremble:
            if t0 <= t < t1:
                ox += 1 if (t // 60) % 2 else -1
        return round(self.x - self.ax + ox), round(self.y - self.ay + oy)

    def look(self, t):
        spr = self.frames[((t + self.phase) // 100) % len(self.frames)]
        for t0, _, _, _, _, fear in self.blows:
            u = t - t0
            if fear:
                if 0 <= u < 320 and (u // 80) % 2 == 0:
                    spr = blend(spr, (40, 60, 120), 0.55)
            elif 0 <= u < 40:
                return solid(spr, (255, 250, 240))
            elif 80 <= u < 160 or 240 <= u < 320:
                spr = blend(spr, (224, 40, 30), 0.55)
        return spr

    def draw(self, frame, t):
        frame.alpha_composite(self.look(t), self.corner(t))


def the_squad():
    spots = json.loads((D / "fight" / "squad.json").read_text(encoding="utf-8"))
    return sorted((Fighter(s["sprite"], s["x"], s["y"]) for s in spots), key=lambda f: f.y)


def quake(events, t, top=7):
    """Trauma shake: each event adds trauma that decays about 1.6 a second; offset grows with trauma squared."""
    tr = sum(max(0.0, a - (t - t0) * 0.0016) for t0, a in events if t >= t0)
    s = top * min(1.0, tr) ** 2
    return (round(s * (0.7 * math.sin(t * 0.061) + 0.3 * math.sin(t * 0.137 + 1.3))),
            round(s * (0.7 * math.sin(t * 0.083 + 0.7) + 0.3 * math.sin(t * 0.171 + 2.1))))


def shaken(frame, stage, off):
    """Shake moves the play area in whole pixels; the HUD stays put."""
    if off == (0, 0):
        return frame
    out = frame.copy()
    out.paste(stage.crop(PLAY), PLAY[:2])
    out.paste(frame.crop(PLAY), (PLAY[0] + off[0], PLAY[1] + off[1]))
    for box in ((0, 0, 1440, PLAY[1]), (0, PLAY[3], 1440, 900), (0, 0, PLAY[0], 900), (PLAY[2], 0, 1440, 900)):
        out.paste(frame.crop(box), box[:2])
    return out


def layer():
    return Image.new("RGBA", (720, 450), (0, 0, 0, 0))


def big(lay):
    return lay.resize((1440, 900), Image.NEAREST)


def save_mp4(frames, path, hold=500):
    box = av.open(str(path), mode="w")
    st = box.add_stream("libx264", rate=Fraction(1000, TICK))
    st.width, st.height = frames[0].size
    st.pix_fmt = "yuv420p"
    st.options = {"crf": "15", "preset": "slow"}
    for im in list(frames) + [frames[-1]] * (hold // TICK):
        for pkt in st.encode(av.VideoFrame.from_ndarray(np.asarray(im), format="rgb24")):
            box.mux(pkt)
    for pkt in st.encode():
        box.mux(pkt)
    box.close()


def key_sheet(frames, marks, path):
    picks = [frames[m] for m in marks if m < len(frames)]
    w, h = picks[0].width // 2, picks[0].height // 2
    sheet = Image.new("RGB", (4 * w + 12, ((len(picks) + 3) // 4) * (h + 4)), (20, 20, 24))
    for n, im in enumerate(picks):
        sheet.paste(im.resize((w, h), Image.LANCZOS), ((n % 4) * (w + 4), (n // 4) * (h + 4)))
    sheet.resize((1800, round(sheet.height * 1800 / sheet.width)), Image.LANCZOS).save(path)


class Grains:
    """Loose sand or ice bits that fall every tick and vanish shortly after landing."""

    def __init__(self):
        self.items = []

    def add(self, x, y, vx, vy, ground, col):
        self.items.append([x, y, vx, vy, ground, col, 0])

    def step(self):
        keep = []
        for p in self.items:
            if p[6] == 0:
                p[3] += 0.22
                p[0] += p[2]
                p[1] += p[3]
                if p[1] >= p[4]:
                    p[1], p[6] = p[4], 1
            else:
                p[6] += 1
            if p[6] < 6:
                keep.append(p)
        self.items = keep

    def draw(self, d):
        for x, y, _, _, _, col, _ in self.items:
            d.point((round(x), round(y)), fill=col)


def edge_points(im, below):
    a = np.asarray(im)[..., 3] > 0
    a[below:] = False
    edge = a & ~ndimage.binary_erosion(a, structure=FOUR)
    ys, xs = np.nonzero(edge)
    return list(zip(xs.tolist(), ys.tolist()))


def clip_frames(name, n):
    return [Image.open(A / name / ("%02d.png" % i)).convert("RGBA") for i in range(n)]


def feet_row(im):
    return int(np.nonzero((np.asarray(im)[..., 3] > 0).any(1))[0].max())


# ---------------------------------------------------------------- shark

def strip_base(im, floor=208):
    """Cut the baked sand, rocks and anything below the floor row; keep the shark itself."""
    a = np.asarray(im.convert("RGBA")).copy()
    ys = np.mgrid[0:a.shape[0], 0:a.shape[1]][0]
    on = a[..., 3] > 0
    r, g, b = (a[..., k].astype(int) for k in range(3))
    sand = on & (r > 150) & (g > 90) & (g > r * 0.55) & (b < g) & ((r - b) > 90) & (g < 225)
    a[(ys >= floor) | (sand & (ys >= 140)), 3] = 0
    on = a[..., 3] > 0
    lab, n = ndimage.label(on, structure=np.ones((3, 3)))
    if n:
        sizes = ndimage.sum(on, lab, range(1, n + 1))
        a[on & (lab != 1 + int(np.argmax(sizes))), 3] = 0
    return Image.fromarray(a, "RGBA")


def sand_crown(d, cx, cy, rx, size, back, seed):
    """The eruption: jagged sand spikes round the hole, dark behind the shark, light in front, re-cut every tick."""
    rnd = random.Random(seed * 2 + back)
    for deg in ((190, 215, 240, 270, 300, 325, 350) if back else (10, 35, 60, 90, 120, 145, 170)):
        a = math.radians(deg)
        ox, oy = cx + rx * math.cos(a), cy + 14 * math.sin(a)
        for _ in range(3):
            ang = math.radians(-90 + 40 * math.cos(a) + rnd.uniform(-28, 28))
            L = size * rnd.uniform(0.45, 1.1) * (0.75 if back else 1.0)
            w = max(1.5, L * 0.09)
            tip = (ox + math.cos(ang) * L, oy + math.sin(ang) * L)
            nx, ny = -math.sin(ang) * w, math.cos(ang) * w
            d.polygon([(round(ox + nx), round(oy + ny)), (round(tip[0]), round(tip[1])), (round(ox - nx), round(oy - ny))],
                      fill=fx.S3 if back else fx.S5, outline=fx.S2)
            if not back:
                d.line([(round(ox), round(oy)), (round(ox + math.cos(ang) * L * 0.6), round(oy + math.sin(ang) * L * 0.6))],
                       fill=fx.S6)


def sinkhole(d, cx, cy, u):
    """The tell: ripples sliding inward and sand grains pulled toward a growing pit."""
    for i in range(3):
        r = 92 - ((u * 0.11 + i * 30) % 92)
        if r < 8:
            continue
        box = (round(cx - r), round(cy - r * 0.7), round(cx + r), round(cy + r * 0.7))
        d.arc(box, 180, 360, fill=fx.S6, width=1)
        d.arc(box, 0, 180, fill=fx.S3, width=1)
    pr = 5 + 15 * min(1.0, u / 800)
    fx.ell(d, cx, cy, pr + 3, (pr + 3) * 0.6, fill=fx.S3)
    fx.ell(d, cx, cy, pr, pr * 0.6, fill=fx.S1)
    fx.ell(d, cx, cy + 1, pr * 0.6, pr * 0.35, fill=fx.OUT)
    for k in range(18):
        a = k * 2 * math.pi / 18 + 0.3
        rr = 88 * (1 - ((u * 0.0016 + k * 0.137) % 1))
        d.point((round(cx + rr * math.cos(a)), round(cy + rr * 0.7 * math.sin(a))), fill=fx.S6 if k % 2 else fx.S5)


def wave_rx(u, dur):
    return (30 + 220 * (1 - (1 - u / dur) ** 2)) / 2


def sand_wave(d, cx, cy, u, dur):
    """The ring of sand that rolls out when the shark sinks; ry is 0.7 rx to match the floor."""
    rx = wave_rx(u, dur)
    ry = rx * 0.7
    w = max(1, round(3 * (1 - u / dur)))
    d.ellipse((round(cx - rx - 1), round(cy - ry - 1), round(cx + rx + 1), round(cy + ry + 1)), outline=fx.S3, width=1)
    d.ellipse((round(cx - rx), round(cy - ry), round(cx + rx), round(cy + ry)), outline=fx.S5, width=w + 1)
    d.arc((round(cx - rx + 1), round(cy - ry + 1), round(cx + rx - 1), round(cy + ry - 1)), 180, 360, fill=fx.S6, width=1)


def draw_wake(d, x, y, left, front, back):
    s = 1 if left else -1
    fx_, bx = x + (front if left else -front), x + (back if left else -back)
    for side in (-1, 1):
        d.line([(round(bx), round(y)), (round(bx + s * 34), round(y + side * 7))], fill=fx.S3, width=2)
        d.line([(round(bx), round(y - 1)), (round(bx + s * 34), round(y + side * 7 - 1))], fill=fx.S6)
    fx.ell(d, fx_ - s * 2, y + 1, 9, 3, fill=fx.S5)
    fx.ell(d, fx_ - s * 2, y, 6, 2, fill=fx.S6)


def shark_scene(snap=8):
    stage = Image.open(D / "fight" / "alphahunt-empty.png").convert("RGBA")
    team = the_squad()
    level = Image.open(A / "shark_clean" / "00.png").convert("RGBA")
    fin = level.rotate(25, resample=Image.NEAREST, expand=True)
    fin = fin.crop(fin.getbbox())
    fin_r = fin.transpose(Image.FLIP_LEFT_RIGHT)
    CUT = 40
    vis = np.nonzero((np.asarray(fin)[:CUT, :, 3] > 0).any(0))[0]
    FRONT, BACK = vis.min() - fin.width / 2, vis.max() - fin.width / 2
    vert = [strip_base(Image.open(A / "shark_vbite" / ("%02d.png" % i))) for i in range(41)]
    base = np.nonzero((np.asarray(vert[0])[198:207, :, 3] > 0).any(0))[0]
    BASE_X = (base.min() + base.max()) / 2
    RX = (base.max() - base.min()) / 2 + 10
    edges = [edge_points(v, 200) for v in vert]

    E = (248, 288)
    EF = (E[0] * 2, E[1] * 2)
    C, R = (505, 270), (115, 55)
    W = 2 * math.pi / 2600
    DIVE_AT = (410, 288)
    T_BREAK, T_DIVE, T_UNDER, T_TELL, T_BREACH = 2800, 3150, 3310, 3550, 4450
    RISE = [200, 130, 60, 14, -10, -6, 0]
    T_BITE = T_BREACH + len(RISE) * TICK + 100
    BITE_MS = 45
    T_SINK = T_BITE + 41 * BITE_MS
    SINK = [round(230 * (k / 8) ** 2) for k in range(9)] + [230] * 3
    T_GONE = T_SINK + len(SINK) * TICK
    T_WAVE, WAVE_MS = T_SINK + 4 * TICK, 600
    T_RESURF = T_SINK + 700
    T_END = T_RESURF + 2000
    CROWN = [26, 48, 64, 58, 44, 28]

    def ring(th):
        return C[0] + R[0] * math.cos(th), C[1] + R[1] * math.sin(th)

    def fin_at(t):
        if t < T_BREAK:
            th = math.pi / 2 + W * t
        elif t < T_DIVE:
            bx, by = ring(math.pi / 2 + W * T_BREAK)
            s = ((t - T_BREAK) / (T_DIVE - T_BREAK)) ** 2
            return bx + (DIVE_AT[0] - bx) * s, by + (DIVE_AT[1] - by) * s, True, 0.0
        elif t < T_UNDER:
            return DIVE_AT[0], DIVE_AT[1], True, CUT * (t - T_DIVE) / (T_UNDER - T_DIVE)
        elif t >= T_RESURF:
            th = W * (t - T_RESURF)
        else:
            return None
        x, y = ring(th)
        sink = max(0.0, CUT * (1 - (t - T_RESURF) / 160)) if t >= T_RESURF else 0.0
        return x, y, ring(th + 0.01)[0] < x, sink

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

    for f in team:
        f.tremble.append((T_TELL, T_BREACH))
        f.blow(T_BREACH + 2 * TICK, EF, 30, lift=24)
        ru = math.hypot(f.x - EF[0], (f.y - EF[1]) / 0.7)
        k = 1 - (ru - 30) / 220
        if 0 <= k <= 1:
            f.blow(T_WAVE + WAVE_MS * (1 - math.sqrt(k)), EF, 10, lift=8)
    shakes = [(T_TELL, 0.25), (T_TELL + 450, 0.2), (T_BREACH, 1.0), (T_BITE + snap * BITE_MS, 0.35), (T_WAVE, 0.7)]
    cracks = fx.crack_paths(E[0], E[1], 11, n=7, length=44, squash=0.7)
    bits, grains, rnd = fx.Bits(41), Grains(), random.Random(3)
    trail, out, step_at = [], [], -80

    def now(t, T):
        return t <= T < t + TICK

    t = 0
    while t < T_END:
        st = fin_at(t)
        if now(t, T_DIVE):
            bits.chunks(DIVE_AT[0], DIVE_AT[1] - 2, DIVE_AT[1], 8, 7, spread=0.7, aim=-90)
        if now(t, T_BREACH):
            bits.chunks(E[0] - 20, E[1] - 6, E[1], 26, 14, spread=0.9, aim=-100)
            bits.chunks(E[0] + 20, E[1] - 6, E[1], 22, 13, spread=0.9, aim=-80)
        if now(t, T_WAVE):
            bits.chunks(E[0], E[1] - 4, E[1], 24, 12, spread=1.0, aim=-90)
        if now(t, T_RESURF):
            sx, sy = ring(0)
            bits.chunks(sx, sy - 2, sy, 10, 8, spread=0.8, aim=-90)
        if t - step_at >= 80:
            if st and st[3] < CUT:
                bow = st[0] + (FRONT if st[2] else -FRONT)
                bits.chunks(bow, st[1] - 1, st[1], 2, 4, spread=0.4, aim=-120 if st[2] else -60)
            if T_UNDER <= t < T_TELL:
                s = (t - T_UNDER) / (T_TELL - T_UNDER)
                bits.chunks(DIVE_AT[0] + (E[0] - DIVE_AT[0]) * s, E[1] - 2, E[1], 3, 5, spread=0.5, aim=-90)
            if T_TELL <= t < T_BREACH:
                for _ in range(3):
                    gx = E[0] + rnd.uniform(-18, 18)
                    grains.add(gx, E[1] - 1, rnd.uniform(-0.5, 0.5), rnd.uniform(-2.2, -1.0), E[1] + rnd.uniform(-2, 3),
                               rnd.choice((fx.S4, fx.S5, fx.S6)))
            bits.step()
            step_at = t
        if T_BITE <= t < T_SINK:
            idx = min(40, (t - T_BITE) // BITE_MS)
            for _ in range(5):
                cx, cy = rnd.choice(edges[idx])
                grains.add(E[0] - BASE_X + cx, E[1] + 4 - 207 + cy, rnd.uniform(-0.6, 0.6), rnd.uniform(-0.4, 0.3),
                           E[1] + rnd.uniform(-3, 10), rnd.choice((fx.S4, fx.S5, fx.S6)))
        if T_WAVE <= t < T_WAVE + WAVE_MS:
            rx = wave_rx(t - T_WAVE, WAVE_MS)
            for _ in range(5):
                a = rnd.uniform(0, 2 * math.pi)
                gx, gy = E[0] + rx * math.cos(a), E[1] + rx * 0.7 * math.sin(a)
                grains.add(gx, gy, 0.5 * math.cos(a), -1.3, gy + 1, rnd.choice((fx.S5, fx.S6)))
        grains.step()

        ground = layer()
        gd = ImageDraw.Draw(ground)
        if st and st[3] < CUT:
            trail.append((t, st[0], st[1]))
        trail = [p for p in trail if t - p[0] < 700]
        for n, (_, x, y) in enumerate(trail):
            if n % 3 == 0:
                gd.point((round(x), round(y) + 1), fill=fx.S3)
        if st and st[3] < CUT:
            draw_wake(gd, st[0], st[1], st[2], FRONT, BACK)
        if T_UNDER <= t < T_TELL:
            s = (t - T_UNDER) / (T_TELL - T_UNDER)
            fx.mound(gd, DIVE_AT[0] + (E[0] - DIVE_AT[0]) * s, E[1], 12, 4)
        if T_TELL <= t < T_BREACH:
            sinkhole(gd, E[0], E[1], t - T_TELL)
            fx.draw_cracks(gd, cracks, grow=min(1.0, (t - T_TELL) / 700))
        if T_BREACH <= t < T_GONE:
            fx.hole(gd, E[0], E[1], RX, 14)
        if T_GONE <= t < T_GONE + 1400:
            fx.crater(gd, E[0], E[1], RX * 0.8, 12, dark=t < T_GONE + 700)
        if T_WAVE <= t < T_WAVE + WAVE_MS:
            sand_wave(gd, E[0], E[1], t - T_WAVE, WAVE_MS)
        if st:
            spr = fin if st[2] else fin_r
            line = st[1]
            bob = round(1.5 * math.sin(t * 0.012))
            fx.place(ground, spr, (spr.width / 2, CUT), st[0], line + bob + st[3],
                     clip=lambda xs, ys, ln=line: ys < ln)

        beast = layer()
        bd = ImageDraw.Draw(beast)
        if T_BREACH <= t < T_GONE:
            idx, dy = shark_pose(t)
            kc, ks = (t - T_BREACH) // TICK, (t - T_WAVE) // TICK
            size = CROWN[kc] if kc < len(CROWN) else (CROWN[ks] * 0.7 if 0 <= ks < len(CROWN) else 0)
            if size:
                sand_crown(bd, E[0], E[1], RX, size, True, t // TICK)
            fx.place(beast, vert[idx], (BASE_X, 207), E[0], E[1] + 4 + dy, clip=near_rim)
            fx.front_lip(bd, E[0], E[1], RX, 14)
            if size:
                sand_crown(bd, E[0], E[1], RX, size, False, t // TICK)
                if kc >= 3 or ks >= 3:
                    for _ in range(10):
                        grains.add(E[0] + rnd.uniform(-RX - 24, RX + 24), E[1] - rnd.uniform(8, size * 0.8),
                                   rnd.uniform(-0.9, 0.9), rnd.uniform(-1.2, 0.4), E[1] + rnd.uniform(-2, 12),
                                   rnd.choice((fx.S4, fx.S5, fx.S6)))
        grains.draw(bd)

        over = layer()
        bits.draw(ImageDraw.Draw(over))

        frame = stage.copy()
        frame.alpha_composite(big(ground))
        for f in team:
            if f.y < EF[1]:
                f.draw(frame, t)
        frame.alpha_composite(big(beast))
        for f in team:
            if f.y >= EF[1]:
                f.draw(frame, t)
        frame.alpha_composite(big(over))
        out.append(shaken(frame, stage, quake(shakes, t)).convert("RGB"))
        t += TICK

    save_mp4(out, A / "shark-boss-v2.mp4")
    marks = [t // TICK for t in (600, 3000, 3400, 4100, T_BREACH + 120, T_BITE + 400, T_WAVE + 200, T_RESURF + 600)]
    key_sheet(out, marks, A / "shark-boss-v2-keys.png")
    print("shark", len(out), "frames, breach at", T_BREACH, "ms, end", T_END, "ms")


# ---------------------------------------------------------------- bear

def pulse(im, k):
    """The caged heart beats: turquoise pixels brighten toward the light turquoise."""
    if k <= 0:
        return im
    a = np.asarray(im).astype(np.float32)
    b = a.copy()
    r, g, bl = a[..., 0], a[..., 1], a[..., 2]
    m = (a[..., 3] > 0) & (g > 110) & (bl > 110) & (r < 0.75 * g)
    b[m, :3] = a[m, :3] * (1 - k) + np.array(TQ_LIGHT, np.float32) * k
    return Image.fromarray(b.astype(np.uint8), "RGBA")


def bolt(d, p0, p1, seed, width=2):
    rnd = random.Random(seed)
    pts = [p0]
    for i in range(1, 7):
        s = i / 7
        pts.append((p0[0] + (p1[0] - p0[0]) * s + rnd.uniform(-6, 6), p0[1] + (p1[1] - p0[1]) * s + rnd.uniform(-6, 6)))
    pts.append(p1)
    pts = [(round(x), round(y)) for x, y in pts]
    d.line(pts, fill=TQ, width=width + 2)
    d.line(pts, fill=WHITE, width=max(1, width - 1))


def star(d, x, y, r, col):
    x, y, r = round(x), round(y), max(1, round(r))
    d.line([(x - r, y), (x + r, y)], fill=col)
    d.line([(x, y - r), (x, y + r)], fill=col)
    q = max(1, round(r * 0.5))
    d.line([(x - q, y - q), (x + q, y + q)], fill=col)
    d.line([(x - q, y + q), (x + q, y - q)], fill=col)
    d.point((x, y), fill=WHITE)


def ice_spike(d, x, y, h, w=3):
    x, y, h = round(x), round(y), max(1, round(h))
    d.polygon([(x - w, y), (x, y - h), (x, y)], fill=ICE[0])
    d.polygon([(x, y), (x, y - h), (x + w, y)], fill=ICE[1])
    d.line([(x - w, y), (x, y - h), (x + w, y)], fill=ICE[3])


def bear_scene():
    stage = Image.open(D / "fight" / "skullpeak-empty.png").convert("RGBA")
    team = the_squad()
    pace = clip_frames("pace", 41)
    lit = Image.open(A / "bear-256.png").convert("RGBA")
    dark = Image.open(A / "bear-dim-256.png").convert("RGBA")
    sag = clip_frames("idle_dim", 17)
    trans = clip_frames("transform", 41)
    slam = clip_frames("rage_slam", 41)
    CX, FEET = 1160, 640
    bb = pace[0].getbbox()
    X0 = round(CX - (bb[0] + bb[2]))
    Y_PACE = FEET - 2 * feet_row(pace[0])
    Y_SAG = FEET - 2 * feet_row(sag[0])
    Y_TR = FEET - 2 * feet_row(trans[0])
    Y_SL = Y_TR + 2 * (feet_row(trans[40]) - feet_row(slam[0]))
    GEM, EYES = (494, 86), ((482, 106), (502, 111))

    PACE_MS = 64
    T_FALTER = 41 * PACE_MS
    FALTER = [(80, 1), (40, 0), (90, 1), (60, 0), (50, 1), (110, 0), (40, 1), (130, 0), (30, 1), (60, 0)]
    T_SAG = T_FALTER + sum(m for m, _ in FALTER)
    T_SKULL = T_SAG + 480
    T_TRANS = T_SKULL + 520
    TR_MS = [120] + [60] * 6 + [45] * 21 + [60] * 5 + [70] * 8
    T_ROAR = T_TRANS + sum(TR_MS[:33])
    T_SLAM = T_TRANS + sum(TR_MS)
    SL_MS = [60] * 9 + [50] * 6 + [40] + [120] + [60] * 24
    T_IMPACT = T_SLAM + sum(SL_MS[:16])
    T_WAVE, WAVE_MS = T_IMPACT + 40, 560
    T_HOLD = T_SLAM + sum(SL_MS)
    T_END = T_HOLD + 900
    LEFT_STOP = 360

    def schedule(t0, durs, t):
        u = t - t0
        for i, m in enumerate(durs):
            if u < m:
                return i
            u -= m
        return len(durs) - 1

    a = np.asarray(slam[16])[..., 3] > 0
    ys, xs = np.nonzero(a)
    low = ys > ys.max() - 30
    IMP = (X0 + 2 * int(xs[low].min()) + 20, Y_SL + 2 * int(ys.max()))
    a = np.asarray(trans[36])[..., 3] > 0
    ys, xs = np.nonzero(a)
    top = ys < ys.min() + 0.45 * (ys.max() - ys.min())
    MOUTH = (X0 + 2 * int(xs[top].min()), Y_TR + 2 * int(np.median(ys[top][xs[top] < xs[top].min() + 6])))
    a = np.asarray(trans[2])[..., 3] > 0
    ys, xs = np.nonzero(a)
    HUMP = ((X0 + 2 * float(np.median(xs[ys < ys.min() + 6]))) / 2, (Y_TR + 2 * (int(ys.min()) + 8)) / 2)
    print("bear canvas", (X0, Y_PACE), "impact", IMP, "mouth", MOUTH, "hump", HUMP)

    def front_x(u, y):
        xf = IMP[0] - (IMP[0] - LEFT_STOP) * (1 - (1 - min(1.0, u / WAVE_MS)) ** 2)
        return xf + 46 * ((y - 575) / 215) ** 2

    for f in team:
        r_u = math.hypot(f.x - MOUTH[0], (f.y - MOUTH[1]) / 0.75)
        f.blow(T_ROAR + (r_u - 20) / 0.9, MOUTH, 5, fear=True)
        for u in range(0, WAVE_MS + 1, TICK):
            if front_x(u, f.y) <= f.x:
                f.blow(T_WAVE + u, (f.x + 80, f.y), 16, lift=12)
                break
    shakes = [(T_TRANS + 240, 0.5), (T_ROAR, 0.4), (T_ROAR + 280, 0.25), (T_IMPACT, 1.0)]
    rnd = random.Random(8)
    cracks = []
    for i in range(7):
        ty = 190 + i * 33
        pts, x, y = [], IMP[0] / 2 - 4, IMP[1] / 2 + (i - 3) * 3
        while x > LEFT_STOP / 2:
            pts.append((x, y))
            x -= rnd.uniform(7, 13)
            y += (ty - y) * 0.3 + rnd.uniform(-6, 6)
        cracks.append(pts)
    flakes = [(rnd.uniform(16, 704), rnd.uniform(40, 417), rnd.uniform(-0.5, -0.15), rnd.uniform(0.35, 0.8),
               rnd.choice((ICE[0], WHITE))) for _ in range(70)]
    spikes, frost, grains = [], [], Grains()
    rows = [[f for f in team if f.y < 500], [f for f in team if 500 <= f.y < 640], [f for f in team if f.y >= 640]]
    out = []

    def now(t, T):
        return t <= T < t + TICK

    t = 0
    while t < T_END:
        if t < T_FALTER:
            spr, y0 = pulse(pace[min(40, t // PACE_MS)], 0.3 * (0.5 + 0.5 * math.sin(t * 2 * math.pi / 700))), Y_PACE
        elif t < T_SAG:
            spr, y0 = (lit if FALTER[schedule(T_FALTER, [m for m, _ in FALTER], t)][1] else dark), Y_PACE
        elif t < T_TRANS:
            spr, y0 = sag[min(16, (t - T_SAG) // 80)], Y_SAG
        elif t < T_SLAM:
            i = schedule(T_TRANS, TR_MS, t)
            spr, y0 = trans[i], Y_TR
            if i == 3 and t - T_TRANS < 280:
                spr = solid(spr, (230, 255, 252))
        else:
            if t < T_HOLD:
                i = schedule(T_SLAM, SL_MS, t)
            else:
                k = ((t - T_HOLD) // 90) % 14
                i = 33 + (k if k < 8 else 14 - k)
            spr, y0 = pulse(slam[i], 0.25 * (0.5 + 0.5 * math.sin(t * 2 * math.pi / 500))), Y_SL

        if now(t, T_IMPACT):
            for _ in range(36):
                grains.add(IMP[0] / 2 + rnd.uniform(-8, 8), IMP[1] / 2 - 2, rnd.uniform(-2.6, 1.0), rnd.uniform(-4.2, -1.4),
                           IMP[1] / 2 + rnd.uniform(-4, 12), rnd.choice(ICE[:3]))
        if now(t, T_WAVE + WAVE_MS):
            for y in range(190, 396, 10):
                for _ in range(3):
                    grains.add(front_x(WAVE_MS, y * 2) / 2 + rnd.uniform(-3, 3), y - 2, rnd.uniform(-1.6, 0.4),
                               rnd.uniform(-3, -1), y + rnd.uniform(-2, 4), rnd.choice(ICE[:3]))
        u = t - T_WAVE
        if 0 <= u <= WAVE_MS and (u // TICK) % 2 == 0:
            for y in range(366, 792, 22):
                spikes.append([front_x(u, y) + rnd.uniform(-5, 5), y + rnd.uniform(-4, 4), t, rnd.uniform(28, 44)])
        live = []
        for s in spikes:
            if t - s[2] < 180:
                live.append(s)
            else:
                frost.append((s[0], s[1], t))
        spikes = live
        frost = [p for p in frost if t - p[2] < 1200]
        grains.step()

        sky = layer()
        sd = ImageDraw.Draw(sky)
        if T_SKULL <= t < T_TRANS + 600:
            r = min(7.0, 1 + 6 * (t - T_SKULL) / 520)
            star(sd, GEM[0], GEM[1], r + (t // 40) % 2, TQ_LIGHT)
            for ex, ey in EYES:
                fx.ell(sd, ex, ey, r * 0.5, r * 0.4, fill=TQ_LIGHT)
                sd.point((ex, ey), fill=WHITE)
            for _ in range(2):
                star(sd, GEM[0] + rnd.uniform(-30, 30), GEM[1] + rnd.uniform(-20, 40), rnd.choice((1, 2)), TQ)

        ground = layer()
        gd = ImageDraw.Draw(ground)
        if T_SLAM + 200 <= t < T_IMPACT + 1500:
            g = min(1.0, (t - T_SLAM - 200) / (T_IMPACT - T_SLAM - 200))
            for pts in cracks:
                seg = [(round(x), round(y)) for x, y in pts[:max(2, round(len(pts) * g))]]
                gd.line([(x, y + 1) for x, y in seg], fill=ICE[3], width=1)
                gd.line(seg, fill=ICE[1], width=1)
                for x, y in seg[::2]:
                    gd.point((x, y), fill=ICE[0])
        for x, y, _ in frost:
            gd.point((round(x / 2), round(y / 2)), fill=ICE[1])
            gd.point((round(x / 2) + 2, round(y / 2) + 1), fill=ICE[0])

        bands = [layer() for _ in range(4)]
        bdraw = [ImageDraw.Draw(b) for b in bands]
        for x, y, t0, H in spikes:
            age = t - t0
            h = H * min(1.0, age / 40) if age < 80 else H * max(0.0, 1 - (age - 80) / 100)
            b = 0 if y < 425 else 1 if y < 564 else 2 if y < 705 else 3
            ice_spike(bdraw[b], x / 2, y / 2, h / 2, w=5)

        over = layer()
        od = ImageDraw.Draw(over)
        if T_TRANS + 120 <= t < T_TRANS + 480:
            mid = ((GEM[0] + HUMP[0]) / 2, min(GEM[1], HUMP[1]) - 24)
            bolt(od, GEM, mid, 300 + t // 40, 2)
            bolt(od, mid, HUMP, 400 + t // 40, 2)
            star(od, HUMP[0], HUMP[1], 4 + (t // 40) % 3, TQ_LIGHT)
        if T_ROAR <= t < T_SLAM + 400:
            for k in range(4):
                uu = t - (T_ROAR + k * 140)
                rr = (20 + 0.9 * uu) / 2
                if uu >= 0 and rr < 330:
                    box = (round(MOUTH[0] / 2 - rr), round(MOUTH[1] / 2 - rr * 0.75),
                           round(MOUTH[0] / 2 + rr), round(MOUTH[1] / 2 + rr * 0.75))
                    od.ellipse(box, outline=TQ_LIGHT, width=1)
                    od.arc((box[0] + 1, box[1] + 1, box[2] - 1, box[3] - 1), 120, 240, fill=WHITE, width=1)
        if T_IMPACT <= t < T_IMPACT + 200:
            sc = 1 - (t - T_IMPACT) / 200
            for k in range(-4, 5):
                ice_spike(od, IMP[0] / 2 + k * 5, IMP[1] / 2 + abs(k), (34 - 5 * abs(k)) * sc, w=4)
            if t < T_IMPACT + 40:
                star(od, IMP[0] / 2, IMP[1] / 2 - 6, 14, WHITE)
        grains.draw(od)
        for x0, y0f, vx, vy, col in flakes:
            od.point((round(16 + (x0 + vx * t / TICK - 16) % 688), round(40 + (y0f + vy * t / TICK - 40) % 377)), fill=col)

        frame = stage.copy()
        frame.alpha_composite(big(sky))
        frame.alpha_composite(big(ground))
        frame.alpha_composite(spr.resize((512, 512), Image.NEAREST), (X0, y0))
        frame.alpha_composite(big(bands[0]))
        for b in range(3):
            for f in rows[b]:
                f.draw(frame, t)
            frame.alpha_composite(big(bands[b + 1]))
        frame.alpha_composite(big(over))
        out.append(shaken(frame, stage, quake(shakes, t)).convert("RGB"))
        t += TICK

    save_mp4(out, A / "bear-boss-v2.mp4")
    marks = [t // TICK for t in (1200, T_FALTER + 300, T_SKULL + 400, T_TRANS + 260, T_ROAR + 200, T_SLAM + 600,
                                 T_WAVE + 200, T_WAVE + 420)]
    key_sheet(out, marks, A / "bear-boss-v2-keys.png")
    print("bear", len(out), "frames, impact at", T_IMPACT, "ms, end", T_END, "ms")


if __name__ == "__main__":
    which = sys.argv[1:] or ["shark", "bear"]
    if "shark" in which:
        shark_scene()
    if "bear" in which:
        bear_scene()
