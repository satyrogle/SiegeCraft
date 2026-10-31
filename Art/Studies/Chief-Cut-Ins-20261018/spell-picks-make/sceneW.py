"""Dust Shark windup B: a dust devil spins on every Clarth fighter, the fin weaves between them, blows miss,
then the sand drops and the leaper (take A) bites the hunters' back row."""
import math

from PIL import ImageDraw

import capkit as ck
import fxkit as fx

BACKC, FRONTC, HIC = (107, 37, 30), (166, 58, 40), (226, 140, 96)
FORM, LOOP_END = 3, 34
stage = ck.hunt_stage()
YOU = [u for u in stage.units if u["side"] == "you"]
for u in YOU:
    u["H"] = (u["y"] - u["top"]) + 26
    u["rt"] = 36 if u["H"] < 130 else 46
    u["phase0"] = (u["x"] * 7 + u["y"] * 3) % 11
bits = fx.Bits(91)
MISSES = [(12, 1), (22, 2)]
FIN_C, FIN_A, FIN_B = (430, 575), 210, 118


def rings(u, k, part, d, bend=0.0):
    if k < 1 or k > LOOP_END:
        return
    n = 9
    show = {1: 3, 2: 6}.get(k, n)
    for i in range(show):
        t = i / (n - 1)
        y = u["y"] - 2 - t * u["H"]
        rx = 8 + (u["rt"] - 8) * t ** 1.25
        ry = max(2.5, rx * 0.3)
        x = u["x"] + math.sin(k * 0.9 + i * 0.7 + u["phase0"]) * 2.5 * t + bend * t * t * 14
        th = 2 + round(2 * t)
        for dash in range(2):
            a0 = k * 62 + i * 27 + dash * 180 + u["phase0"] * 30
            pts = []
            for a in range(int(a0), int(a0) + 141, 7):
                r = math.radians(a)
                pts.append((x + rx * math.cos(r), y + ry * math.sin(r), math.sin(r)))
            seg = [(round(px), round(py)) for px, py, sn in pts if (sn < 0) == (part == "back")]
            if len(seg) < 2:
                continue
            if part == "back":
                d.line([(px, py + 1) for px, py in seg], fill=(70, 24, 20), width=th)
                d.line(seg, fill=BACKC, width=th)
            else:
                d.line([(px, py + 1) for px, py in seg], fill=BACKC, width=th)
                d.line(seg, fill=FRONTC, width=th)
                d.line([(px, py - th // 2) for px, py in seg[1:-1]], fill=HIC, width=1)
    for j in range(3):
        a = math.radians(k * 75 + j * 120 + u["phase0"] * 20)
        h = (0.25 + 0.3 * j) * u["H"]
        rr = 10 + (u["rt"] - 8) * (h / u["H"]) ** 1.25
        px, py = u["x"] + rr * math.cos(a), u["y"] - h + rr * 0.3 * math.sin(a)
        if (math.sin(a) < 0) == (part == "back"):
            d.rectangle((round(px), round(py), round(px) + 1, round(py) + 1), fill=HIC)
    for j in range(4):
        a = math.radians(k * 40 + j * 90)
        px, py = u["x"] + 22 * math.cos(a), u["y"] + 6 * math.sin(a)
        if (math.sin(a) < 0) == (part == "back"):
            fx.ell(d, px, py, 4, 3, fill=fx.D0 if part == "back" else fx.D1)


def fin_at(k):
    s = k * 2 * math.pi / 32
    x = FIN_C[0] + FIN_A * math.sin(s)
    y = FIN_C[1] + FIN_B * math.sin(s) * math.cos(s)
    return x, y, 1 if math.cos(s) >= 0 else -1


def fin(d, x, y, face):
    pts = [(-11, 0), (-9, -9), (-8, -18), (-6, -27), (0, -20), (6, -11), (13, 0)]
    poly = [(round(x + face * px), round(y + py)) for px, py in pts]
    d.polygon(poly, fill=(125, 51, 42), outline=fx.OUT)
    d.line([(round(x + face * -5), round(y - 24)), (round(x + face * 1), round(y - 16)), (round(x + face * 6), round(y - 8))], fill=(192, 117, 92))
    for sgn in (-1, 1):
        d.line([(round(x - face * 10), round(y)), (round(x - face * 40), round(y + sgn * 7))], fill=fx.S3, width=2)
        d.line([(round(x - face * 10), round(y - 1)), (round(x - face * 40), round(y + sgn * 7 - 2))], fill=fx.S6)
    fx.ell(d, x + face * 2, y + 1, 13, 4, fill=fx.S5)


def arrow(d, x, y, ang):
    r = math.radians(ang)
    tip = (x + 7 * math.cos(r), y + 7 * math.sin(r))
    tail = (x - 7 * math.cos(r), y - 7 * math.sin(r))
    d.line([(round(tail[0]), round(tail[1])), (round(tip[0]), round(tip[1]))], fill=(40, 30, 48), width=2)
    d.point((round(tip[0]), round(tip[1])), fill=(230, 230, 240))


def frame(k):
    bends, arrows = {}, []
    for kk, ui in MISSES:
        u = YOU[ui]
        j = k - kk
        hx, hy = u["x"] + u["rt"] * 0.8, u["y"] - u["H"] * 0.55
        if j == 0:
            arrows.append((hx + 90, hy - 20, 170))
        if j == 1:
            arrows.append((hx + 40, hy - 6, 170))
        if j == 2:
            bends[ui] = -1.0
            bits.dust(hx, hy, 2, spread=4, rise=0.4, size=(3, 5))
            arrows.append((hx + 4, hy - 10, 120))
        if j == 3:
            bends[ui] = 0.6
            arrows.append((hx + 14, hy - 24, 60))
        if j == 4:
            arrows.append((hx + 22, hy - 30, -20))
        if j == 5:
            arrows.append((hx + 28, hy - 26, -80))
    if k == 0:
        for u in YOU:
            bits.dust(u["x"], u["y"] - 1, 3, spread=14, rise=0.6, size=(2, 4))
    if FORM <= k <= LOOP_END and k % 3 == 0:
        for u in YOU:
            bits.dust(u["x"], u["y"] - 1, 1, spread=12, rise=0.5, size=(2, 4))
    if k == LOOP_END + 1:
        for u in YOU:
            for t in (0.3, 0.6, 0.9):
                bits.chunks(u["x"], u["y"] - t * u["H"], u["y"], 3, 3, spread=1.0, aim=90)
            bits.dust(u["x"], u["y"] - 2, 2, spread=16, rise=0.5, size=(3, 5))
        x, y, face = fin_at(LOOP_END)
        bits.chunks(x, y - 2, y, 8, 9, spread=0.6)
    bits.step()

    def under(layer):
        d = ImageDraw.Draw(layer)
        x, y, face = fin_at(min(k, LOOP_END))
        if FORM - 1 <= k <= LOOP_END:
            fin(d, x, y, face)
        if k == LOOP_END + 1:
            fx.hole(d, x, y, 14, 5)
            fx.spikes(d, x, y, 77, 26, dirs=[-160, -120, -60, -20])
        if k > LOOP_END + 1:
            fx.crater(d, x, y, 14, 5, dark=False)
        bits.draw(d, kinds=("dust", "speck"))

    def before(layer, u):
        if u in YOU:
            rings(u, k, "back", ImageDraw.Draw(layer), bends.get(YOU.index(u), 0.0))

    def between(layer, u):
        if u in YOU:
            rings(u, k, "front", ImageDraw.Draw(layer), bends.get(YOU.index(u), 0.0))

    def over(layer):
        d = ImageDraw.Draw(layer)
        for a in arrows:
            arrow(d, *a)
        bits.draw(d, kinds=("chunk",))

    return stage.render(under=under, over=over, before=before, between=between, t_ms=k * fx.FRAME_MS)


if __name__ == "__main__":
    import sys
    NW = LOOP_END + 4
    wind = [frame(k) for k in range(NW)]
    fx.strip(wind, [0, 1, 2, 3, 8, 12, 14, 15, 22, 24, 35, 36], (170, 280, 690, 760), "W-zoom.png", cols=6)
    if "check" in sys.argv:
        sys.exit()
    import sceneA
    bite = [sceneA.frame(f) for f in range(sceneA.L0 - 1, sceneA.L0 + 4 + 24 + 10)]
    import mp4kit as encode
    allf = wind + bite
    crop = (150, 240, 1300, 800)
    out = fx.STUDY / "windup-B"
    out.mkdir(exist_ok=True)
    encode.mp4([f.crop(crop) for f in allf], out / "dust-shark-windup-B-then-bite.mp4")
    big = [f.crop((170, 280, 690, 760)) for f in wind]
    encode.mp4([b.resize((b.width * 2, b.height * 2)) for b in big], out / "dust-shark-windup-B-2x.mp4")
    print("frames", len(wind), len(bite))
