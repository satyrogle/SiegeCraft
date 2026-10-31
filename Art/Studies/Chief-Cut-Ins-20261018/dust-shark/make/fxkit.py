"""Dust Shark kit: key and pose Comfy art, shrink every pose to sprite pixels in the Clarth palette,
draw clean pixel effects (Saint11 rules), composite over the desert hunt fight."""
import json
import math
import pathlib
import random

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage

REPO = pathlib.Path(__file__).resolve().parents[5]   # make, dust-shark, Chief-Cut-Ins, Studies, Art, repo
CA = REPO / "Assets/Resources/CampaignArt"
STUDY = REPO / "Art/Studies/Chief-Cut-Ins-20261018/dust-shark"
# Fight() only: the Codex audit render, not in the repo; drop it here to use Fight()
HUNT = REPO / "Art/Studies/Chief-Cut-Ins-20261018/refs/fight-11-AlphaHunt.png"
CLARTH_REF = REPO / "Art/Studies/Chief-Cut-Ins-20261018/refs/clarth-chief-front-approved.png"
FRAME_MS = 80
PLAY = (32, 80, 1408, 834)


def rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


OUT = rgb("#1a0909")
S0, S1, S2, S3 = rgb("#3e2520"), rgb("#5a2f26"), rgb("#8f4537"), rgb("#b8652f")
S4, S5, S6, S7 = rgb("#e18d42"), rgb("#f2aa56"), rgb("#fbd38a"), rgb("#fff1cf")
G0, G1, G2, G3 = rgb("#53201b"), rgb("#ac1917"), rgb("#f60d08"), rgb("#ffb08a")
D0, D1, D2 = rgb("#b8652f"), rgb("#eaa159"), rgb("#f7c47f")
W8 = np.array([2.0, 4.0, 3.0], np.float32)


def clarth_palette():
    im = Image.open(CLARTH_REF).convert("RGBA")
    cols = sorted({p[:3] for p in im.get_flattened_data() if p[3] > 0})
    return np.array([c for c in cols if c[0] >= c[2]], np.float32)


PAL = clarth_palette()


def lock(arr, P=PAL):
    d = (((arr[:, :, None, :3] - P[None, None]) ** 2) * W8).sum(-1)
    return P[d.argmin(-1)]


def key(path, tol=70, erode=1):
    """Cut the art out of its flat matte (flood from the border), cropped to the art."""
    a = np.asarray(Image.open(path).convert("RGB"), np.int32)
    border = np.concatenate([a[0], a[-1], a[:, 0], a[:, -1]])
    bg = np.median(border, axis=0)
    near = np.abs(a - bg).sum(2) <= tol
    lab, _ = ndimage.label(near)
    edge = set(np.unique(np.concatenate([lab[0], lab[-1], lab[:, 0], lab[:, -1]]))) - {0}
    matte = np.isin(lab, list(edge))
    fg = ~matte
    if erode:
        fg = ndimage.binary_erosion(fg, iterations=erode)
    ys, xs = np.nonzero(fg)
    box = (xs.min(), ys.min(), xs.max() + 1, ys.max() + 1)
    rgba = np.dstack([a, fg * 255]).astype(np.uint8)
    return Image.fromarray(rgba, "RGBA").crop(box)


def pose(src, angle=0.0, scale=0.2, pivot=None, flip=False, sx=1.0, sy=1.0):
    """Turn the big art about a pivot, then shrink it. Returns (small RGBA, pivot in small coords)."""
    if flip:
        src = src.transpose(Image.FLIP_LEFT_RIGHT)
    w0, h0 = src.size
    if sx != 1.0 or sy != 1.0:
        src = src.resize((max(1, round(w0 * sx)), max(1, round(h0 * sy))), Image.LANCZOS)
    w, h = src.size
    px, py = pivot if pivot else (w0 / 2, h0 / 2)
    if flip and pivot:
        px = w0 - px
    px, py = px * sx, py * sy
    R = int(math.ceil(math.hypot(max(px, w - px), max(py, h - py)))) + 8
    canvas = Image.new("RGBA", (2 * R, 2 * R), (0, 0, 0, 0))
    canvas.alpha_composite(src, (int(round(R - px)), int(round(R - py))))
    rot = canvas.convert("RGBa").rotate(angle, resample=Image.BILINEAR)
    n = max(2, int(round(2 * R * scale)))
    small = rot.resize((n, n), Image.BOX).convert("RGBA")
    return small, (n / 2.0, n / 2.0)


def pixelize(small, outline=OUT, cut=128, clean=True):
    arr = np.asarray(small).astype(np.float32)
    on = arr[..., 3] >= cut
    q = lock(arr)
    out = np.zeros(arr.shape[:2] + (4,), np.uint8)
    out[on, :3] = q[on]
    out[on, 3] = 255
    if clean:
        c = out[..., :3].astype(np.int32)
        key3 = c[..., 0] * 65536 + c[..., 1] * 256 + c[..., 2]
        lum = 0.3 * c[..., 0] + 0.59 * c[..., 1] + 0.11 * c[..., 2]
        H, Wd = key3.shape
        for y in range(1, H - 1):
            for x in range(1, Wd - 1):
                if not on[y, x] or lum[y, x] > 190:
                    continue
                if not (on[y - 1, x] and on[y + 1, x] and on[y, x - 1] and on[y, x + 1]):
                    continue
                nb = [key3[y - 1, x], key3[y + 1, x], key3[y, x - 1], key3[y, x + 1]]
                if key3[y, x] not in nb and nb.count(nb[0]) == 4:
                    out[y, x, :3] = c[y - 1, x]
    if outline is not None:
        nb = np.zeros_like(on)
        nb[1:, :] |= on[:-1, :]
        nb[:-1, :] |= on[1:, :]
        nb[:, 1:] |= on[:, :-1]
        nb[:, :-1] |= on[:, 1:]
        ring = nb & ~on
        out[ring, :3] = outline
        out[ring, 3] = 255
    return Image.fromarray(out, "RGBA")


def native(src, **kw):
    outline = kw.pop("outline", OUT)
    small, piv = pose(src, **kw)
    return pixelize(small, outline=outline), piv


def place(layer, spr, piv, x, y, clip=None):
    """Paste a native sprite so its pivot lands on (x, y). clip(xs, ys) gives the visible mask in screen coords."""
    x0, y0 = int(round(x - piv[0])), int(round(y - piv[1]))
    if clip is not None:
        a = np.asarray(spr).copy()
        ys, xs = np.mgrid[0:a.shape[0], 0:a.shape[1]]
        vis = clip(xs + x0, ys + y0)
        a[~vis, 3] = 0
        spr = Image.fromarray(a, "RGBA")
    tmp = Image.new("RGBA", layer.size, (0, 0, 0, 0))
    tmp.paste(spr, (x0, y0), spr)
    layer.alpha_composite(tmp)


def tint(spr, col):
    a = np.asarray(spr).copy()
    on = a[..., 3] > 0
    a[on, :3] = col
    return Image.fromarray(a, "RGBA")


def ell(d, cx, cy, rx, ry, fill=None, outline=None, width=1):
    if rx < 0.5 or ry < 0.5:
        if fill:
            d.point((round(cx), round(cy)), fill=fill)
        return
    d.ellipse((round(cx - rx), round(cy - ry), round(cx + rx), round(cy + ry)), fill=fill, outline=outline, width=width)


def crack_paths(cx, cy, seed, n=5, length=26, squash=0.5):
    rnd = random.Random(seed)
    paths = []
    for i in range(n):
        ang = (i + rnd.uniform(-0.3, 0.3)) * 2 * math.pi / n
        pts = [(cx, cy)]
        x, y, a = cx, cy, ang
        segs = rnd.randint(3, 4)
        for s in range(segs):
            step = length / segs * rnd.uniform(0.7, 1.3)
            a += rnd.uniform(-0.6, 0.6)
            x += math.cos(a) * step
            y += math.sin(a) * step * squash
            pts.append((x, y))
        paths.append(pts)
    return paths


def draw_cracks(d, paths, grow=1.0, glow=False):
    for pts in paths:
        k = max(2, int(round(1 + (len(pts) - 1) * grow)))
        seg = [(round(x), round(y)) for x, y in pts[:k]]
        if glow:
            d.line(seg, fill=G0, width=3)
            d.line(seg, fill=G2, width=1)
        else:
            d.line(seg, fill=S0, width=1)
    if glow:
        d.point([(round(p[0][0]), round(p[0][1])) for p in paths], fill=G3)


def mound(d, cx, cy, rx, ry):
    ell(d, cx, cy + 1, rx, ry, fill=S3)
    ell(d, cx, cy, rx, ry, fill=S5)
    ell(d, cx - rx * 0.15, cy - ry * 0.35, rx * 0.6, ry * 0.45, fill=S6)


def hole(d, cx, cy, rx, ry):
    ell(d, cx, cy, rx + 2, ry + 1, fill=S5)
    ell(d, cx, cy, rx, ry, fill=S1)
    ell(d, cx, cy + 1, rx - 1, ry - 1, fill=OUT)


def front_lip(d, cx, cy, rx, ry):
    """The near rim of a hole, drawn over whatever comes out of it."""
    pts = []
    for i in range(0, 181, 6):
        t = math.radians(i)
        pts.append((cx + rx * math.cos(t), cy + ry * math.sin(t)))
    for i in range(180, -1, -6):
        t = math.radians(i)
        pts.append((cx + (rx + 3) * math.cos(t), cy + (ry + 3) * math.sin(t) + 1))
    d.polygon([(round(x), round(y)) for x, y in pts], fill=S4)
    d.line([(round(cx + rx * math.cos(math.radians(i))), round(cy + ry * math.sin(math.radians(i))))
            for i in range(0, 181, 6)], fill=S6, width=1)


def crater(d, cx, cy, rx, ry, dark=True):
    ell(d, cx, cy + 1, rx + 1, ry + 1, outline=S3)
    ell(d, cx, cy, rx, ry, outline=S5)
    if dark:
        ell(d, cx, cy + 1, rx * 0.55, ry * 0.5, fill=S3)


def spikes(d, cx, cy, seed, size, dirs=None, light=True):
    """The sharp blast: a fan of hard edged spikes, no noise."""
    rnd = random.Random(seed)
    dirs = dirs or [-150, -125, -105, -90, -75, -55, -30]
    for a in dirs:
        a = math.radians(a + rnd.uniform(-6, 6))
        L = size * rnd.uniform(0.7, 1.15)
        w = size * 0.16
        tip = (cx + math.cos(a) * L, cy + math.sin(a) * L)
        nx, ny = -math.sin(a) * w, math.cos(a) * w
        poly = [(cx + nx, cy + ny), tip, (cx - nx, cy - ny)]
        d.polygon([(round(x), round(y)) for x, y in poly], fill=S5 if light else S3, outline=S2)
        if light:
            d.line([(round(cx), round(cy)), (round(cx + math.cos(a) * L * 0.6), round(cy + math.sin(a) * L * 0.6))], fill=S7)


class Bits:
    """Debris and dust, one particle at a time. Heavy chunks fall fast, dust floats."""

    def __init__(self, seed):
        self.rnd = random.Random(seed)
        self.items = []

    def chunks(self, cx, cy, ground, n, speed, up=1.0, spread=1.0, palette=(S2, S4, S5), aim=-90):
        for _ in range(n):
            a = math.radians(aim + self.rnd.uniform(-70, 70) * spread)
            v = speed * self.rnd.uniform(0.5, 1.1)
            self.items.append(dict(kind="chunk", x=cx + self.rnd.uniform(-6, 6), y=cy - self.rnd.uniform(0, 6),
                                   vx=math.cos(a) * v, vy=math.sin(a) * v * up, g=2.4,
                                   ground=ground + self.rnd.uniform(-4, 8), size=self.rnd.choice((2, 3, 3, 4, 5)),
                                   col=self.rnd.choice(palette), bounces=0, rest=0, life=self.rnd.randint(9, 14), age=0))

    def dust(self, cx, cy, n, spread=18, rise=1.2, size=(4, 9)):
        for _ in range(n):
            self.items.append(dict(kind="dust", x=cx + self.rnd.uniform(-spread, spread), y=cy - self.rnd.uniform(0, 8),
                                   vx=self.rnd.uniform(-1.6, 1.6), vy=-self.rnd.uniform(0.3, rise),
                                   r=self.rnd.uniform(*size), grow=2, life=self.rnd.randint(6, 9), age=0))

    def step(self):
        keep = []
        for p in self.items:
            p["age"] += 1
            if p["kind"] == "chunk":
                if p["rest"]:
                    p["rest"] += 1
                else:
                    p["vy"] += p["g"]
                    p["x"] += p["vx"]
                    p["y"] += p["vy"]
                    if p["y"] >= p["ground"] and p["vy"] > 0:
                        p["y"] = p["ground"]
                        if p["bounces"] == 0 and p["size"] <= 3:
                            p["vy"] *= -0.3
                            p["vx"] *= 0.5
                            p["bounces"] = 1
                        else:
                            p["rest"] = 1
                if p["age"] < p["life"] + 3:
                    keep.append(p)
            elif p["kind"] == "dust":
                p["x"] += p["vx"]
                p["y"] += p["vy"]
                p["vx"] *= 0.85
                if p["grow"] > 0:
                    p["r"] += 1
                    p["grow"] -= 1
                elif p["age"] > p["life"] * 0.5:
                    p["r"] -= 1.2
                if p["r"] > 1.5:
                    keep.append(p)
                elif p["age"] < p["life"] + 3:
                    p["kind"] = "speck"
                    p["specks"] = [(p["x"] + self.rnd.uniform(-3, 3), p["y"] + self.rnd.uniform(-3, 2)) for _ in range(3)]
                    p["sage"] = 0
                    keep.append(p)
            elif p["kind"] == "speck":
                p["sage"] += 1
                p["specks"] = [(x + self.rnd.uniform(-1, 1), y - 0.5) for x, y in p["specks"]][: max(0, 3 - p["sage"])]
                if p["specks"]:
                    keep.append(p)
        self.items = keep

    def draw(self, d, kinds=("chunk", "dust", "speck")):
        for p in sorted([q for q in self.items if q["kind"] in kinds], key=lambda q: q.get("y", 0)):
            if p["kind"] == "chunk":
                s = p["size"]
                if p["age"] > p["life"]:
                    s = max(1, s - (p["age"] - p["life"]))
                x, y = round(p["x"]), round(p["y"])
                d.rectangle((x, y - s + 1, x + s - 1, y), fill=p["col"])
                if s >= 3:
                    d.point((x, y - s + 1), fill=S6)
                    d.line((x + 1, y, x + s - 1, y), fill=S1)
            elif p["kind"] == "dust":
                r = p["r"]
                ell(d, p["x"], p["y"] + 1, r, r * 0.85, fill=D0)
                ell(d, p["x"], p["y"], r - 0.5, r * 0.8, fill=D1)
                ell(d, p["x"] - r * 0.3, p["y"] - r * 0.3, r * 0.4, r * 0.35, fill=D2)
            elif p["kind"] == "speck":
                d.point([(round(x), round(y)) for x, y in p["specks"]], fill=D1)


SPOTS = [(795, 430, "hunter_warden"), (780, 565, "hunter_skirmisher"), (1030, 430, "hunter_marksman"),
         (1135, 430, "hunter_trapper"), (1030, 565, "hunter_medic"), (1030, 700, "hunter_warden"),
         (1220, 632, "hunter_captain")]
BACK = [2, 4, 5]


def load_clip(name):
    meta = json.loads((CA / (name + "_sw_idle.json")).read_text(encoding="utf-8"))
    sheet = Image.open(CA / (name + "_sw_idle.png")).convert("RGBA")
    frames = meta["frames"] if isinstance(meta["frames"], list) else list(meta["frames"].values())
    out = []
    for f in frames:
        r = f["frame"]
        out.append((sheet.crop((r["x"], r["y"], r["x"] + r["w"], r["y"] + r["h"])), f.get("duration", 100)))
    fr0 = frames[0]["frame"]
    return out, (meta.get("anchorX", fr0["w"] / 2), meta.get("anchorY", fr0["h"]))


class Fight:
    def __init__(self):
        hunt = Image.open(HUNT).convert("RGBA")
        patch = hunt.crop((840, 590, 940, 705))
        for x, y, _ in SPOTS:
            hunt.paste(patch, (x - 50, y - 110))
        self.base = hunt
        self.units = []
        for i, (x, y, name) in enumerate(SPOTS):
            clip, anchor = load_clip(name)
            self.units.append(dict(i=i, x=x, y=y, clip=clip, anchor=anchor, phase=(i * 137) % 400))

    def unit_sprite(self, u, t_ms):
        total = sum(d for _, d in u["clip"])
        t = (t_ms + u["phase"]) % total
        for spr, d in u["clip"]:
            if t < d:
                return spr
            t -= d
        return u["clip"][0][0]

    def render(self, t_ms, under=None, over=None, mods=None, shake=(0, 0), between=None, before=None):
        """under(layer) draws on the ground; between(layer, unit) draws right after a unit;
        over(layer) draws over everything; mods[i] = dict(tint, dx, dy, clip_y, hide)."""
        mods = mods or {}
        f = self.base.copy()
        if under:
            under(f)
        for u in sorted(self.units, key=lambda q: q["y"]):
            m = mods.get(u["i"], {})
            spr = self.unit_sprite(u, t_ms)
            if m.get("tint"):
                spr = tint(spr, m["tint"])
            ax, ay = u["anchor"]
            x0 = int(u["x"] - ax + m.get("dx", 0))
            y0 = int(u["y"] - ay + m.get("dy", 0))
            if m.get("clip_y") is not None:
                a = np.asarray(spr).copy()
                cut = int(m["clip_y"] - y0)
                if cut < a.shape[0]:
                    a[max(0, cut):, :, 3] = 0
                spr = Image.fromarray(a, "RGBA")
            if before:
                before(f, u)
            if not m.get("hide"):
                f.alpha_composite(spr, (x0, y0))
            if between:
                between(f, u)
        if over:
            over(f)
        if shake != (0, 0):
            play = f.crop(PLAY)
            g = self.base.copy()
            g.paste(play, (PLAY[0] + shake[0], PLAY[1] + shake[1]))
            for box in ((0, 0, 1440, PLAY[1]), (0, PLAY[3], 1440, 900), (0, 0, PLAY[0], 900), (PLAY[2], 0, 1440, 900)):
                g.paste(self.base.crop(box), box[:2])
            f = g
        return f.convert("RGB")


def save_gif(frames, durs, path, crop=None, scale=1):
    out = []
    for fr in frames:
        im = fr.crop(crop) if crop else fr
        if scale != 1:
            im = im.resize((im.width * scale, im.height * scale), Image.NEAREST)
        out.append(im.quantize(colors=255, method=Image.MEDIANCUT, dither=Image.NONE))
    out[0].save(path, save_all=True, append_images=out[1:], duration=durs, loop=0, disposal=1, optimize=False)


def strip(frames, idx, crop, path, scale=1, cols=6):
    tiles = [frames[i].crop(crop) for i in idx]
    w, h = tiles[0].size
    rows = (len(tiles) + cols - 1) // cols
    sheet = Image.new("RGB", (cols * w * scale + (cols - 1) * 4, rows * h * scale + (rows - 1) * 4), (20, 18, 22))
    for k, t in enumerate(tiles):
        if scale != 1:
            t = t.resize((w * scale, h * scale), Image.NEAREST)
        sheet.paste(t, ((k % cols) * (w * scale + 4), (k // cols) * (h * scale + 4)))
    sheet.save(path)
