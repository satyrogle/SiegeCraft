"""Animation kit for the six spell strips (Jacob, 21 Oct 2026: "go, animate all six strips the way they are").

The shared spell language, moving at 80 ms a frame: a tribe mark snaps in under whoever the spell is about (spinning,
beating through the sim's 3 s tell, blazing in its last second), a dark contrast frame, the hero shape lands, white
and red flashes twice with a shake, then everything breaks into specks of its own pixels. Never a fade."""
import json
import math
import pathlib
import random

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage

import fxkit as fx
import mp4kit

OUT = pathlib.Path("C:/Users/jacob/KindredSiege-Clean/Art/Studies/Chief-Cut-Ins-20261018/spell-language")
RAW = OUT / "raw"
MS = 80
TELL = 38          # 3.04 s of tell, the sim's 3 s windup
GLOW_FROM = 26     # its last second blazes
WHITE, RED = (255, 250, 240), (224, 40, 30)


def palette(names, extra=()):
    cols = set(tuple(c) for c in extra)
    for n in names:
        im = Image.open(fx.CA / (n + ".png")).convert("RGBA")
        cols |= {p[:3] for p in im.get_flattened_data() if p[3] > 0}
    return np.array(sorted(cols), np.float32)


def shade(spr, mul):
    a = np.asarray(spr).astype(np.float32)
    a[..., :3] = np.clip(a[..., :3] * mul, 0, 255)
    return Image.fromarray(a.astype(np.uint8), "RGBA")


def lift(spr, toward, amt):
    a = np.asarray(spr).astype(np.float32)
    a[..., :3] = a[..., :3] + (np.array(toward, np.float32) - a[..., :3]) * amt
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8), "RGBA")


def flat(spr, col):
    a = np.asarray(spr).copy()
    a[a[..., 3] > 0, :3] = col
    return Image.fromarray(a, "RGBA")


def scaled(spr, kx, ky=None):
    ky = kx if ky is None else ky
    return spr.resize((max(1, round(spr.width * kx)), max(1, round(spr.height * ky))), Image.NEAREST)


def put(layer, spr, x, y):
    """Centre a sprite on (x, y)."""
    tmp = Image.new("RGBA", layer.size, (0, 0, 0, 0))
    tmp.paste(spr, (round(x - spr.width / 2), round(y - spr.height / 2)), spr)
    layer.alpha_composite(tmp)


def put_feet(layer, spr, x, y, clip_below=None):
    """Stand a sprite on (x, y); optionally hide everything lower than clip_below (it is coming out of the ground)."""
    if clip_below is not None:
        a = np.asarray(spr).copy()
        cut = int(round(clip_below - (y - spr.height)))
        if cut < a.shape[0]:
            a[max(0, cut):, :, 3] = 0
        spr = Image.fromarray(a, "RGBA")
    tmp = Image.new("RGBA", layer.size, (0, 0, 0, 0))
    tmp.paste(spr, (round(x - spr.width / 2), round(y - spr.height)), spr)
    layer.alpha_composite(tmp)


class Mark:
    """A top down tribe mark laid flat on the floor, in spin steps."""

    def __init__(self, raw_name, width, pal, steps=8, sym=45.0, squash=0.42):
        src = fx.key(RAW / raw_name, tol=60)
        side = max(src.size)
        sq = Image.new("RGBA", (side, side), (0, 0, 0, 0))
        sq.paste(src, ((side - src.width) // 2, (side - src.height) // 2), src)
        h = max(2, round(width * squash))
        self.frames = []
        for i in range(steps):
            rot = sq.convert("RGBa").rotate(sym * i / steps, resample=Image.BILINEAR).convert("RGBA")
            self.frames.append(fx.pixelize(rot.resize((width, h), Image.BOX), outline=fx.OUT, pal=pal))
        self.w, self.h = self.frames[0].size
        self.rx, self.ry = self.w / 2, self.h / 2

    def at(self, step):
        return self.frames[step % len(self.frames)]


def mark_tell(layer, mark, x, y, f, glow, spin=2, snap=(0.35, 1.18, 1.0)):
    """The mark through the tell: snaps in with an overshoot, turns one step every `spin` frames, beats, then blazes."""
    if f < 0:
        return None
    spr = mark.at(f // spin)
    if f >= GLOW_FROM:
        spr = lift(spr, glow, 0.22 + 0.14 * ((f // 2) % 2))
    else:
        spr = shade(spr, 0.82 if (f // 4) % 2 == 0 else 0.97)
    if f < len(snap):
        spr = scaled(spr, snap[f])
    put(layer, spr, x, y)
    if f >= GLOW_FROM:
        fx.ell(ImageDraw.Draw(layer), x, y, mark.rx + 3 + (f % 2), mark.ry + 2, outline=glow, width=2)
    return spr


def rim(spr, col, it=2):
    a = np.asarray(spr)[..., 3] > 0
    ring = ndimage.binary_dilation(a, iterations=it) & ~a
    im = np.zeros(a.shape + (4,), np.uint8)
    im[ring] = col + (255,)
    return Image.fromarray(im, "RGBA")


def unit_sprite(u, t_ms=0):
    if "clip" not in u:
        return u["spr"], u["box"][:2]
    total = sum(d for _, d in u["clip"])
    t = (t_ms + u["phase"]) % total
    spr = u["clip"][0][0]
    for s, d in u["clip"]:
        if t < d:
            spr = s
            break
        t -= d
    return spr, (int(u["x"] - u["anchor"][0]), int(u["y"] - u["anchor"][1]))


def unit_height(u):
    spr, (x0, y0) = unit_sprite(u)
    ys = np.nonzero(np.asarray(spr)[..., 3] > 0)[0]
    return u["y"] - (y0 + ys.min())


def hit_tint(j):
    """Frames since a hit: white, red, normal, red, normal (the lich's two strobes)."""
    return {0: WHITE, 1: RED, 3: RED}.get(j)


def hit_shake(j, amp=4):
    return {0: (amp, -(amp // 2)), 1: (-amp, amp // 2), 2: (amp // 2, -1)}.get(j, (0, 0))


class Breakup:
    """A sprite breaking into specks of its own pixels: thrown out, falling, shrinking to single pixels, gone."""

    def __init__(self, spr, x, y, seed, keep=0.2, speed=2.5, up=1.5, g=0.45, life=(5, 9)):
        rnd = random.Random(seed)
        a = np.asarray(spr)
        ys, xs = np.nonzero(a[..., 3] > 0)
        cx, cy = spr.width / 2, spr.height / 2
        self.p = []
        for px, py in zip(xs, ys):
            if rnd.random() > keep:
                continue
            dx, dy = px - cx, py - cy
            n = math.hypot(dx, dy) or 1.0
            v = speed * rnd.uniform(0.5, 1.2)
            self.p.append((x + dx, y + dy, dx / n * v, dy / n * v * 0.7 - up * rnd.uniform(0.4, 1.0),
                           rnd.randint(*life), tuple(int(c) for c in a[py, px, :3])))
        self.g = g

    def draw(self, d, j):
        for x0, y0, vx, vy, life, col in self.p:
            if j < 0 or j > life:
                continue
            x = x0 + vx * j
            y = y0 + vy * j + 0.5 * self.g * j * j
            s = 1 if j > life * 0.6 else 2
            d.rectangle((round(x), round(y), round(x) + s - 1, round(y) + s - 1), fill=col)


def spikes(d, x, y, seed, size, cols, dirs=(-165, -140, -115, -90, -65, -40, -15)):
    """A hard edged burst: a fan of spikes, light core, dark rim. No noise."""
    light, mid, dark = cols
    rnd = random.Random(seed)
    for a in dirs:
        a = math.radians(a + rnd.uniform(-7, 7))
        L = size * rnd.uniform(0.7, 1.15)
        w = size * 0.17
        tip = (x + math.cos(a) * L, y + math.sin(a) * L)
        nx, ny = -math.sin(a) * w, math.cos(a) * w
        d.polygon([(round(x + nx), round(y + ny)), (round(tip[0]), round(tip[1])), (round(x - nx), round(y - ny))], fill=mid, outline=dark)
        d.line([(round(x), round(y)), (round(x + math.cos(a) * L * 0.55), round(y + math.sin(a) * L * 0.55))], fill=light)


def frame_of(name, i):
    m = json.loads((fx.CA / (name + ".json")).read_text(encoding="utf-8"))
    fr = m["frames"] if isinstance(m["frames"], list) else list(m["frames"].values())
    sh = Image.open(fx.CA / (name + ".png")).convert("RGBA")
    r = fr[i]["frame"]
    return sh.crop((r["x"], r["y"], r["x"] + r["w"], r["y"] + r["h"])), (m.get("anchorX", r["w"] / 2), m.get("anchorY", r["h"]))


def tip_of(spr, anchor, ux, uy, mode):
    """The topmost pixel of a sprite, or its furthest right pixel in its top 40% (a spear cocked back)."""
    a = np.asarray(spr)[..., 3] > 0
    ys, xs = np.nonzero(a)
    if mode == "top":
        i = int(np.argmin(ys))
    else:
        upper = ys < ys.min() + (ys.max() - ys.min()) * 0.4
        i = int(np.argmax(np.where(upper, xs, -1)))
    return ux - anchor[0] + xs[i], uy - anchor[1] + ys[i]


def even(box):
    x0, y0, x1, y1 = box
    return (x0, y0, x1 - ((x1 - x0) % 2), y1 - ((y1 - y0) % 2))


def save(frames, folder, name, crop1, crop2, check_idx):
    """The take over the fight at 1x, a 2x zoom, and a check strip of chosen frames (kept under 2000 px wide)."""
    out = OUT / folder
    out.mkdir(parents=True, exist_ok=True)
    crop1, crop2 = even(crop1), even(crop2)
    mp4kit.mp4([f.crop(crop1) for f in frames], out / (name + ".mp4"))
    big = [f.crop(crop2) for f in frames]
    mp4kit.mp4([b.resize((b.width * 2, b.height * 2), Image.NEAREST) for b in big], out / (name + "-2x.mp4"))
    w = crop2[2] - crop2[0]
    cols = max(1, min(len(check_idx), 1990 // (w + 4)))
    fx.strip(frames, check_idx, crop2, str(out / (name + "-check.png")), scale=1, cols=cols)
    return out
