"""B. Breach from below: the ground cracks under each back row hunter, jaws come straight up and bite."""
import numpy as np
from PIL import ImageDraw
import fxkit as fx
import art

S, ANG, L0 = 0.24, 85, 5
fight = fx.Fight()
op, sh = art.shark()
OPEN = fx.native(op, angle=ANG, scale=S)
SHUT = fx.native(sh, angle=ANG, scale=S)


def measure(spr, piv):
    a = np.asarray(spr).astype(int)
    on = a[..., 3] > 0
    top = int(np.nonzero(on.any(1))[0][0])
    lum = a[..., 0] * 0.3 + a[..., 1] * 0.59 + a[..., 2] * 0.11
    band = np.zeros_like(on)
    band[top + 12:top + 40] = True
    m = band & on & (lum < 75) & (a[..., 0] > a[..., 1] * 1.4)
    gx = np.nonzero(m)[1].mean() if m.any() else piv[0]
    return top - piv[1], gx - piv[0]


NOSE, GAPX = measure(*OPEN)
T = []
for k, idx in enumerate(fx.BACK):
    u = fight.units[idx]
    h, cx = art.unit_height(fx.SPOTS[idx][2])
    T.append(dict(i=idx, x=u["x"] + cx, feet=u["y"], h=h, start=L0 + 2 * k, seed=900 + idx,
                  cracks=fx.crack_paths(u["x"] + cx, u["y"] + 1, 900 + idx, n=7, length=54)))
bits = fx.Bits(77)
HOLE = (34, 12)


def shark_pos(t, k):
    px = t["x"] - GAPX
    top = t["feet"] - t["h"] - 14
    py = top - NOSE
    if k == 9:
        py -= 6
    if k == 13:
        py += 0.55 * (t["h"] + 40)
    if k == 14:
        py += 0.9 * (t["h"] + 40)
    return px, py


def clip_for(t):
    cx, cy = t["x"], t["feet"]
    rx, ry = HOLE

    def clip(xs, ys):
        inside = np.abs(xs - cx) <= rx
        gy = np.where(inside, cy + ry * np.sqrt(np.clip(1 - ((xs - cx) / rx) ** 2, 0, 1)), cy)
        return ys < gy
    return clip


def frame(f):
    shake = [0, 0]
    mods = {}
    for t in T:
        k = f - t["start"]
        if k == 9:
            bits.chunks(t["x"], t["feet"] - 4, t["feet"], 16, 13, spread=0.9)
        if k == 10:
            bits.dust(t["x"], t["feet"] - 2, 4, spread=30, rise=1.0, size=(3, 6))
            for sx, sy in ((5, -3),):
                shake[0] += sx; shake[1] += sy
        if k == 11:
            shake[0] -= 4; shake[1] += 2
        if k == 12:
            shake[0] += 2; shake[1] -= 1
        if k == 13:
            bits.dust(t["x"], t["feet"] - 2, 3, spread=24, rise=0.8, size=(3, 5))
        if k in (1, 2):
            bits.chunks(t["x"], t["feet"], t["feet"], 2, 4, spread=1.2)
        tint = {10: (255, 250, 240), 11: (224, 40, 30), 13: (255, 250, 240), 14: (224, 40, 30)}.get(k)
        mods[t["i"]] = dict(tint=tint, dy={13: -3, 14: -1}.get(k, 0))
    bits.step()

    def under(layer):
        d = ImageDraw.Draw(layer)
        for t in T:
            k = f - t["start"]
            x, y = t["x"], t["feet"]
            if 0 <= k <= 3:
                fx.draw_cracks(d, t["cracks"], grow=(k + 1) / 4)
                fx.mound(d, x, y, 14 + 6 * k, 5 + 2 * k)
            elif 4 <= k <= 7:
                fx.draw_cracks(d, t["cracks"], glow=True)
                fx.mound(d, x + (1 if k % 2 else -1), y - (1 if k % 2 else 0), 34, 12)
            elif k == 8:
                fx.draw_cracks(d, t["cracks"])
                fx.ell(d, x, y, 32, 12, fill=fx.OUT)
            elif 9 <= k <= 14:
                if k <= 12:
                    fx.draw_cracks(d, t["cracks"])
                fx.hole(d, x, y, *HOLE)
            elif 15 <= k <= 17:
                fx.crater(d, x, y, 30, 10)
                fx.hole(d, x, y, 26 - 7 * (k - 15), 9 - 2.5 * (k - 15))
            elif k > 17:
                fx.crater(d, x, y, 30, 10, dark=k < 24)
        bits.draw(d, kinds=("dust", "speck"))

    def draw_shark(layer, t, k):
        spr, piv = OPEN if k == 9 else SHUT
        px, py = shark_pos(t, k)
        fx.place(layer, spr, piv, px, py, clip=clip_for(t))
        dd = ImageDraw.Draw(layer)
        fx.front_lip(dd, t["x"], t["feet"], *HOLE)
        if k == 9:
            fx.spikes(dd, t["x"] - 6, t["feet"], t["seed"], 58, dirs=[-170, -150, -128, -52, -30, -10])
        if k == 10:
            fx.spikes(dd, t["x"] - 6, t["feet"], t["seed"] + 1, 34, dirs=[-165, -140, -40, -15], light=False)

    def before(layer, u):
        for t in T:
            if t["i"] == u["i"] and f - t["start"] == 9:
                draw_shark(layer, t, 9)

    def between(layer, u):
        for t in T:
            k = f - t["start"]
            if t["i"] == u["i"] and 10 <= k <= 14:
                draw_shark(layer, t, k)

    def over(layer):
        bits.draw(ImageDraw.Draw(layer), kinds=("chunk",))

    return fight.render(f * fx.FRAME_MS, under=under, over=over, mods=mods, shake=tuple(shake),
                        before=before, between=between)


if __name__ == "__main__":
    N = L0 + 4 + 22 + 10
    frames = [frame(f) for f in range(N)]
    out = fx.STUDY / "B-breach"
    out.mkdir(exist_ok=True)
    crop = (760, 240, 1300, 800)
    fx.save_gif(frames, [fx.FRAME_MS] * N, out / "dust-shark-B-breach-2x.gif", crop=crop, scale=2)
    fx.save_gif(frames, [fx.FRAME_MS] * N, out / "dust-shark-B-breach-full.gif")
    fx.strip(frames, list(range(L0, L0 + 24)), crop, "B-strip.png", scale=1, cols=8)
    print("frames", N, "nose", NOSE, "gapx", GAPX)
