"""A. Leaper: a fin cuts the sand to each back row hunter, the shark arcs out, bites, dives back in."""
import numpy as np
from PIL import ImageDraw
import fxkit as fx
import art

S, L0 = 0.15, 5
fight = fx.Fight()
op, sh = art.shark()
POSE = {}
for ang in (42, 16, 0):
    POSE[("o", ang)] = fx.native(op, angle=ang, scale=S)
for ang in (-12, -35, -60, -80):
    POSE[("s", ang)] = fx.native(sh, angle=ang, scale=S)


def bite_point(spr, piv, white):
    a = np.asarray(spr).astype(int)
    lum = a[..., 0] * 0.3 + a[..., 1] * 0.59 + a[..., 2] * 0.11
    on = a[..., 3] > 0
    cols = np.nonzero(on.any(0))[0]
    front = np.zeros_like(on)
    front[:, cols.max() - 28:] = True
    m = front & on & ((lum > 200) if white else ((lum < 75) & (a[..., 0] > a[..., 1] * 1.4)))
    ys, xs = np.nonzero(m)
    return xs.mean() - piv[0], ys.mean() - piv[1]


BITE_S = bite_point(*POSE[("s", -12)], True)
BITE_O = bite_point(*POSE[("o", 0)], False)
hist = {}
for c in np.asarray(POSE[("s", -12)][0]).reshape(-1, 4):
    if c[3]:
        hist[tuple(c[:3])] = hist.get(tuple(c[:3]), 0) + 1
HIDE = max(hist, key=hist.get)

T = []
for k, idx in enumerate(fx.BACK):
    u = fight.units[idx]
    h, cx = art.unit_height(fx.SPOTS[idx][2])
    ux, feet = u["x"] + cx, u["y"]
    T.append(dict(i=idx, x=ux, feet=feet, h=h, start=L0 + 2 * k, seed=500 + idx,
                  bx=ux - 70, dx=ux + 70))
bits = fx.Bits(55)
HR = (18, 7)


def fin(d, x, y):
    pts = [(-11, 0), (-9, -9), (-8, -18), (-6, -27), (0, -20), (6, -11), (13, 0)]
    poly = [(round(x + px), round(y + py)) for px, py in pts]
    d.polygon(poly, fill=HIDE, outline=fx.OUT)
    d.line([(round(x - 5), round(y - 24)), (round(x + 1), round(y - 16)), (round(x + 6), round(y - 8))], fill=(192, 117, 92))


def wake(d, x, y, n):
    L = min(46, 12 + 8 * n)
    for sgn in (-1, 1):
        d.line([(round(x - 10), round(y)), (round(x - 10 - L), round(y + sgn * 8))], fill=fx.S3, width=2)
        d.line([(round(x - 10), round(y - 1)), (round(x - 10 - L), round(y + sgn * 8 - 2))], fill=fx.S6)
    fx.ell(d, x + 2, y + 1, 13, 4, fill=fx.S5)


def ground_clip(t):
    feet = t["feet"]
    rx, ry = HR

    def clip(xs, ys):
        g = np.full(xs.shape, float(feet))
        for hx in (t["bx"], t["dx"]):
            inside = np.abs(xs - hx) <= rx
            g = np.where(inside, feet + ry * np.sqrt(np.clip(1 - ((xs - hx) / rx) ** 2, 0, 1)), g)
        return ys < g
    return clip


def pivot_for(t, k):
    ux, feet, h = t["x"], t["feet"], t["h"]
    by = feet - 0.62 * h
    top = feet - h
    if k == 7:
        return ("o", 42), (t["bx"] + 6, feet - 24)
    if k == 8:
        return ("o", 16), (ux - 58, top - 22)
    if k == 9:
        return ("o", 0), (ux - 16 - BITE_O[0], top + 2 - BITE_O[1])
    if 10 <= k <= 12:
        return ("s", -12), (ux - BITE_S[0], by - BITE_S[1])
    if k == 13:
        return ("s", -35), (ux + 24, feet - 44)
    if k == 14:
        return ("s", -60), (t["dx"] - 2, feet - 20)
    if k == 15:
        return ("s", -80), (t["dx"] + 4, feet + 4)
    return None, None


def frame(f):
    shake = [0, 0]
    mods = {}
    for t in T:
        k = f - t["start"]
        fxp = t["bx"] - 72 + 12 * min(k, 6)
        if 0 <= k <= 5:
            bits.chunks(fxp - 6, t["feet"] - 2, t["feet"], 2, 6, spread=0.35, aim=-120)
        if k == 7:
            bits.chunks(t["bx"], t["feet"] - 3, t["feet"], 14, 12, spread=0.8, aim=-75)
            bits.dust(t["bx"], t["feet"] - 2, 3, spread=16, rise=0.9, size=(3, 6))
        if k in (8, 9):
            kx, ky = pivot_for(t, k)[1]
            bits.chunks(kx - 10, ky + 6, t["feet"], 4, 2, spread=1.0, aim=90)
        if k == 10:
            shake[0] += 5; shake[1] -= 3
        if k == 11:
            shake[0] -= 4; shake[1] += 2
        if k == 12:
            shake[0] += 2; shake[1] -= 1
        if k == 15:
            bits.chunks(t["dx"], t["feet"] - 3, t["feet"], 12, 11, spread=0.8, aim=-90)
            bits.dust(t["dx"], t["feet"] - 2, 3, spread=16, rise=0.9, size=(3, 6))
        tint = {10: (255, 250, 240), 11: (224, 40, 30), 13: (255, 250, 240), 14: (224, 40, 30)}.get(k)
        mods[t["i"]] = dict(tint=tint, dy={13: -3, 14: -1}.get(k, 0))
    bits.step()

    def under(layer):
        d = ImageDraw.Draw(layer)
        for t in T:
            k = f - t["start"]
            y = t["feet"]
            fxp = t["bx"] - 72 + 12 * min(k, 6)
            if 0 <= k <= 5:
                wake(d, fxp, y, k)
            if k == 6:
                fx.ell(d, t["bx"], y, HR[0] + 3, HR[1] + 1, fill=fx.OUT)
            if 7 <= k <= 15:
                fx.hole(d, t["bx"], y, *HR)
            if 14 <= k <= 16:
                fx.hole(d, t["dx"], y, *HR)
            if k >= 16:
                fx.crater(d, t["bx"], y, HR[0], HR[1], dark=k < 22)
            if k >= 17:
                fx.crater(d, t["dx"], y, HR[0], HR[1], dark=k < 23)
        bits.draw(d, kinds=("dust", "speck"))

    def draw_shark(layer, t, k):
        key, (px, py) = pivot_for(t, k)
        spr, piv = POSE[key]
        fx.place(layer, spr, piv, px, py, clip=ground_clip(t))
        dd = ImageDraw.Draw(layer)
        if 7 <= k <= 8:
            fx.front_lip(dd, t["bx"], t["feet"], *HR)
        if 14 <= k <= 15:
            fx.front_lip(dd, t["dx"], t["feet"], *HR)
        if k == 7:
            fx.spikes(dd, t["bx"] - 4, t["feet"], t["seed"], 44, dirs=[-170, -148, -125, -100])
        if k == 15:
            fx.spikes(dd, t["dx"], t["feet"], t["seed"] + 1, 36, dirs=[-160, -130, -50, -20])

    def before(layer, u):
        d = ImageDraw.Draw(layer)
        for t in T:
            k = f - t["start"]
            if t["i"] != u["i"]:
                continue
            if 0 <= k <= 5:
                fin(d, t["bx"] - 72 + 12 * k, t["feet"])
            if k in (7, 8, 9, 13, 14, 15):
                draw_shark(layer, t, k)

    def between(layer, u):
        for t in T:
            k = f - t["start"]
            if t["i"] == u["i"] and 10 <= k <= 12:
                draw_shark(layer, t, k)

    def over(layer):
        bits.draw(ImageDraw.Draw(layer), kinds=("chunk",))

    return fight.render(f * fx.FRAME_MS, under=under, over=over, mods=mods, shake=tuple(shake),
                        before=before, between=between)


if __name__ == "__main__":
    N = L0 + 4 + 24 + 10
    frames = [frame(f) for f in range(N)]
    out = fx.STUDY / "A-leaper"
    out.mkdir(exist_ok=True)
    crop = (760, 240, 1300, 800)
    fx.save_gif(frames, [fx.FRAME_MS] * N, out / "dust-shark-A-leaper-2x.gif", crop=crop, scale=2)
    fx.save_gif(frames, [fx.FRAME_MS] * N, out / "dust-shark-A-leaper-full.gif")
    s = L0 + 2
    fx.strip(frames, [s + k for k in (2, 5, 6, 7, 8, 9, 10, 11, 13, 14, 15, 18)], (860, 380, 1160, 640), "A-mid-zoom.png", scale=2, cols=6)
    fx.strip(frames, [L0 + 7, L0 + 9, L0 + 11, L0 + 13], crop, "A-row.png", cols=4)
    print("frames", N, "biteS", BITE_S, "biteO", BITE_O, "hide", HIDE)
