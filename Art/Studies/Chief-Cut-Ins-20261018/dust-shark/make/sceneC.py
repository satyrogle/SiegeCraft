"""C. Maw: ripples close into a ring under the back row, it sags, a ring of teeth rises round all three and clamps shut."""
import math
import numpy as np
from PIL import Image, ImageDraw
import fxkit as fx
import art

L0 = 5
fight = fx.Fight()
MAW = art.maw()
MW = MAW.width
T = []
for idx in fx.BACK:
    u = fight.units[idx]
    h, cx = art.unit_height(fx.SPOTS[idx][2])
    T.append(dict(i=idx, x=u["x"] + cx, feet=u["y"], h=h))
CX = round(sum(t["x"] for t in T) / 3)
CY = 565
RX, RY = 112, 172
a = np.asarray(MAW).copy()
yy, xx = np.mgrid[0:a.shape[0], 0:a.shape[1]]
rr = np.hypot((xx - a.shape[1] / 2) / (a.shape[1] / 2), (yy - a.shape[0] / 2) / (a.shape[0] / 2))
a[rr < 0.56, 3] = 0
RING = Image.fromarray(a, "RGBA")
cache = {}


def maw_native(rx, ry, ring=False):
    key = (round(rx), round(ry), ring)
    if key not in cache:
        src = RING if ring else MAW
        s = 2 * rx / MW
        cache[key] = fx.native(src, scale=s, sy=ry / rx)
    return cache[key]


def oval_pts(cx, cy, rx, ry, a0, a1, step=4):
    return [(cx + rx * math.cos(math.radians(t)), cy + ry * math.sin(math.radians(t))) for t in range(a0, a1 + 1, step)]


def wall(d, rx, ry, h):
    """The outside of the worm under the raised rim, near half only."""
    top = oval_pts(CX, CY - h, rx, ry, 0, 180)
    bot = oval_pts(CX, CY, rx, ry, 0, 180)[::-1]
    d.polygon([(round(x), round(y)) for x, y in top + bot], fill=(125, 51, 42), outline=fx.OUT)
    d.line([(round(x), round(y)) for x, y in oval_pts(CX, CY - h * 0.45, rx, ry, 10, 170)], fill=(90, 47, 38))


def ripples(d, k):
    for j, extra in enumerate((0.0, 0.28)):
        sc = max(1.0, 1.95 - 0.19 * k) + extra
        if sc > 2.3:
            continue
        for a0, a1 in ((110, 250), (-70, 70)):
            pts = [(round(x), round(y)) for x, y in oval_pts(CX, CY, RX * sc, RY * sc * 0.95, a0, a1)]
            d.line([(x, y + 2) for x, y in pts], fill=fx.S3, width=2)
            d.line(pts, fill=fx.S6, width=2)
            d.line([(x, y - 1) for x, y in pts[::3]], fill=fx.S7, width=1)


def funnel(d, k):
    for sc, col in ((1.0, fx.S4), (0.82, fx.S3), (0.64, fx.S2), (0.46, fx.S1), (0.28, fx.S0)):
        fx.ell(d, CX, CY, RX * sc, RY * sc, fill=col)
    for j in range(14):
        ang = math.radians(j * 360 / 14 + k * 17)
        r0, r1 = 0.95 - 0.1 * (k % 2), 0.7 - 0.1 * (k % 2)
        p0 = (CX + RX * r0 * math.cos(ang), CY + RY * r0 * math.sin(ang))
        p1 = (CX + RX * r1 * math.cos(ang + 0.35), CY + RY * r1 * math.sin(ang + 0.35))
        d.line([(round(p0[0]), round(p0[1])), (round(p1[0]), round(p1[1]))], fill=fx.S6)
    fx.ell(d, CX, CY, RX, RY, outline=fx.S6, width=2)


bits = fx.Bits(31)


def ring_rim(cx, cy, n=12):
    return [(cx + RX * math.cos(2 * math.pi * j / n), cy + RY * math.sin(2 * math.pi * j / n), 360 * j / n) for j in range(n)]


def frame(f):
    k = f - L0
    shake = [0, 0]
    mods = {}
    for t in T:
        m = {}
        if 6 <= k <= 9:
            m["dy"] = k - 5
            m["clip_y"] = t["feet"]
        elif 10 <= k <= 15:
            m["dy"] = 5
            m["clip_y"] = t["feet"]
        elif k == 16:
            m["dy"] = -3
        elif k == 17:
            m["dy"] = -1
        m["tint"] = {13: (255, 250, 240), 14: (224, 40, 30), 16: (255, 250, 240), 17: (224, 40, 30)}.get(k)
        mods[t["i"]] = m
    if k == 11:
        for x, y, ang in ring_rim(CX, CY):
            bits.chunks(x, y - 4, y, 2, 10, spread=0.25, aim=ang - 90 if 90 < ang < 270 else -90 + (ang - 90 if ang <= 90 else ang - 450) * 0.3)
    if k == 12:
        for x, y, ang in ring_rim(CX, CY, 8):
            bits.dust(x, y - 2, 1, spread=6, rise=0.9, size=(3, 6))
    if k == 19:
        bits.chunks(CX, CY - 4, CY + 30, 14, 12, spread=0.9)
        bits.dust(CX, CY, 5, spread=40, rise=1.0, size=(3, 6))
    for kk, sxy in ((13, (6, -3)), (14, (-5, 2)), (15, (3, -1))):
        if k == kk:
            shake = list(sxy)
    bits.step()
    H = {11: 16, 12: 22}.get(k, 0)

    def raised(layer, rx, ry, h):
        d = ImageDraw.Draw(layer)
        fx.ell(d, CX, CY, rx, ry, fill=fx.OUT)
        wall(d, rx, ry, h)
        spr, piv = maw_native(rx, ry)
        fx.place(layer, spr, piv, CX, CY - h)

    def under(layer):
        d = ImageDraw.Draw(layer)
        if 0 <= k <= 5:
            ripples(d, k)
        elif 6 <= k <= 9:
            funnel(d, k)
        elif k == 10:
            fx.ell(d, CX, CY, RX + 2, RY + 2, fill=fx.S5)
            fx.ell(d, CX, CY, RX, RY, fill=fx.OUT)
        elif 11 <= k <= 12:
            sc = 1.1 if k == 11 else 1.0
            raised(layer, RX * sc, RY * sc, H)
        elif 13 <= k <= 19:
            fill = {13: fx.S1, 14: fx.S1, 15: fx.S1, 16: fx.S2, 17: fx.S2, 18: fx.S3, 19: fx.S3}[k]
            fx.ell(d, CX, CY, RX * 0.98, RY * 0.99, fill=fx.S5)
            fx.ell(d, CX, CY, RX * 0.94, RY * 0.96, fill=fill)
            fx.ell(d, CX, CY, RX * 0.94, RY * 0.96, outline=fx.S2, width=2)
            for j in range(16):
                ang = math.radians(j * 22.5 + k * 9)
                r0 = 0.92 - 0.12 * ((k + j) % 3)
                p0 = (CX + RX * r0 * math.cos(ang), CY + RY * r0 * math.sin(ang))
                p1 = (CX + RX * (r0 - 0.16) * math.cos(ang), CY + RY * (r0 - 0.16) * math.sin(ang))
                d.line([(round(p0[0]), round(p0[1])), (round(p1[0]), round(p1[1]))], fill=fx.S6)
        elif 20 <= k <= 25:
            sc = 0.95 - 0.13 * (k - 20)
            fx.ell(d, CX, CY, RX * sc, RY * sc, fill=fx.S3, outline=fx.S5)
            fx.ell(d, CX, CY, RX * 0.97, RY * 0.98, outline=fx.S3)
        elif k > 25:
            fx.ell(d, CX, CY, RX * 0.97, RY * 0.98, outline=fx.S3)
            fx.ell(d, CX, CY - 1, RX * 0.95, RY * 0.96, outline=fx.S5)
        bits.draw(d, kinds=("dust", "speck"))

    def over(layer):
        if 11 <= k <= 12:
            sc = 1.1 if k == 11 else 1.0
            tmp = Image.new("RGBA", layer.size, (0, 0, 0, 0))
            raised(tmp, RX * sc, RY * sc, H)
            arr = np.asarray(tmp).copy()
            arr[: T[2]["feet"] - 6, :, 3] = 0
            layer.alpha_composite(Image.fromarray(arr, "RGBA"))
        if 13 <= k <= 18:
            j = k - 13
            sc = (1.0, 1.0, 1.0, 0.62, 0.36, 0.16)[j]
            rx, ry = 48 * sc, RY * 1.04 * sc
            h = (24, 24, 24, 14, 6, 2)[j]
            tmp = Image.new("RGBA", layer.size, (0, 0, 0, 0))
            wall(ImageDraw.Draw(tmp), rx, ry, h)
            spr, piv = maw_native(rx, ry, ring=True)
            fx.place(tmp, spr, piv, CX, CY - h)
            if j >= 3:
                arr = np.asarray(tmp).astype(np.float32)
                arr[..., :3] *= (0.8, 0.62, 0.45)[j - 3]
                tmp = Image.fromarray(arr.astype(np.uint8), "RGBA")
            layer.alpha_composite(tmp)
        if k == 19:
            fx.spikes(ImageDraw.Draw(layer), CX, CY, 4401, 46, dirs=[-160, -130, -100, -80, -50, -20])
        bits.draw(ImageDraw.Draw(layer), kinds=("chunk",))

    return fight.render(f * fx.FRAME_MS, under=under, over=over, mods=mods, shake=tuple(shake))


if __name__ == "__main__":
    N = L0 + 30 + 8
    frames = [frame(f) for f in range(N)]
    out = fx.STUDY / "C-maw"
    out.mkdir(exist_ok=True)
    crop = (760, 240, 1300, 800)
    fx.save_gif(frames, [fx.FRAME_MS] * N, out / "dust-shark-C-maw-2x.gif", crop=crop, scale=2)
    fx.save_gif(frames, [fx.FRAME_MS] * N, out / "dust-shark-C-maw-full.gif")
    fx.strip(frames, [L0 + k for k in (2, 5, 8, 10, 11, 12, 13, 14, 16, 17, 19, 23)], (880, 330, 1200, 790), "C-zoom.png", scale=1, cols=6)
    print("frames", N, "centre", CX, CY)
