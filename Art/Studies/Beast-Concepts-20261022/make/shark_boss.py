"""The sand shark as a boss in the real desert fight: its fin stalks in with a wake, the ground bulges, it breaches out
of a hole, bites a Warden (the fight-feel hit stack), sand pours off it, then it dives back and the fin swims off.

The shark is PixelLab's 40 frame bite with its glued sand cut away (anim2/shark_clean). Everything that belongs to the
beast (shark, hole, sand bank, wake, sand chunks, dust) is drawn on a half size layer and doubled, so its pixels match
the shark shown at 2x. The Warden and the stage stay at 1x. Sand moves with the Dust Shark kit's particle physics.

python -I shark_boss.py   (artlab-env python: needs scipy and av)
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

HERE = pathlib.Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parents[1] / "Chief-Cut-Ins-20261018" / "dust-shark" / "make"))
import fxkit as fx  # noqa: E402

D = HERE.parent / "directions"
CA = pathlib.Path("C:/Users/jacob/KindredSiege-Clean/Assets/Resources/CampaignArt")
TICK = 20
FLOOR_C = 205                       # floor row inside the 256 shark canvas
FY = 350                            # floor line on the half size layer (700 on screen)
CX0 = 395                           # shark canvas left edge on the half size layer
CY0 = FY - FLOOR_C                  # shark canvas top on the half size layer
HOLE = (CX0 + 170, FY, 64, 8)       # hole centre x, y, rx, ry on the half size layer
VICTIM_FEET = (824, 618)
BITE_FRAME = 22


def clip(name):
    meta = json.loads((CA / (name + "_sw_idle.json")).read_text(encoding="utf-8"))
    frs = meta["frames"] if isinstance(meta["frames"], list) else list(meta["frames"].values())
    sheet = Image.open(CA / (name + "_sw_idle.png")).convert("RGBA")
    out = [sheet.crop((f["frame"]["x"], f["frame"]["y"], f["frame"]["x"] + f["frame"]["w"],
                       f["frame"]["y"] + f["frame"]["h"])).transpose(Image.FLIP_LEFT_RIGHT) for f in frs]
    w = frs[0]["frame"]["w"]
    return out, (w - meta.get("anchorX", w / 2), meta.get("anchorY", frs[0]["frame"]["h"]))


def tint(spr, col):
    a = np.asarray(spr).copy()
    a[a[..., 3] > 0, :3] = col
    return Image.fromarray(a, "RGBA")


def redden(spr):
    a = np.asarray(spr).astype(np.float32).copy()
    on = a[..., 3] > 0
    a[on, :3] = a[on, :3] * 0.45 + np.array([224, 40, 30]) * 0.55
    return Image.fromarray(a.astype(np.uint8), "RGBA")


def near_rim(xs, ys):
    hx, hy, rx, ry = HOLE
    return ys < hy + ry * 0.55 * np.sqrt(np.clip(1 - ((xs - hx) / rx) ** 2, 0, 1))


def bank(d, k=1.0):
    """The sand piled up in front of the hole, over the shark's cut: a near half ring, a lip line on top."""
    hx, hy, rx, ry = HOLE
    rx2, ry2 = rx * k, ry * k
    pts = [(hx + (rx2 + 3) * math.cos(math.radians(a)), hy - 2 + (ry2 + 6) * math.sin(math.radians(a))) for a in range(0, 181, 6)]
    pts += [(hx + rx2 * math.cos(math.radians(a)), hy - 6 + ry2 * math.sin(math.radians(a))) for a in range(180, -1, -6)]
    d.polygon([(round(x), round(y)) for x, y in pts], fill=fx.S4)
    d.line([(round(hx + rx2 * math.cos(math.radians(a))), round(hy - 6 + ry2 * math.sin(math.radians(a))))
            for a in range(0, 181, 6)], fill=fx.S6, width=1)
    d.line([(round(hx + (rx2 + 3) * math.cos(math.radians(a))), round(hy - 2 + (ry2 + 6) * math.sin(math.radians(a))))
            for a in range(0, 181, 6)], fill=fx.S3, width=1)


def fin_sprite(frame):
    """The whole shark turned level and sunk, so only its fin, spine row and harpoons cut the sand.
    Returns the turned sprite and the row that sits on the sand line."""
    rot = frame.rotate(25, resample=Image.NEAREST, expand=True)
    rot = rot.crop(rot.getbbox())
    return rot, 34


def wake(d, x, y, n):
    L = min(40, 10 + 6 * n)
    for sgn in (-1, 1):
        d.line([(round(x + 6), round(y)), (round(x + 6 + L), round(y + sgn * 6))], fill=fx.S3, width=2)
        d.line([(round(x + 6), round(y - 1)), (round(x + 6 + L), round(y + sgn * 6 - 2))], fill=fx.S6)
    fx.ell(d, x + 2, y + 1, 12, 3, fill=fx.S5)


def body_edge_points(frame, rnd, n):
    """Points on the shark's lower outline above the sand, where loose sand pours off."""
    a = np.asarray(frame)[..., 3] > 0
    a = a[:FLOOR_C - 4]
    below = np.zeros_like(a)
    below[:-3] = a[3:]
    edge = a & ~below
    ys, xs = np.nonzero(edge)
    if not len(xs):
        return []
    idx = rnd.sample(range(len(xs)), min(n, len(xs)))
    return [(int(xs[i]), int(ys[i])) for i in idx]


def main():
    frames = [Image.open(D / "anim2" / "shark_clean" / ("%02d.png" % i)).convert("RGBA") for i in range(41)]
    stage = Image.open(D / "fight" / "alphahunt-clean.png").convert("RGBA")
    vclip, (vax, vay) = clip("clarth_warden")
    fin, fin_cut = fin_sprite(frames[0])
    rnd = random.Random(23)
    bits = fx.Bits(41)
    hx, hy, rx, ry = HOLE

    # (kind, shark frame, ms, fx) with kind: stalk, tell, breach, clip, dive, swim
    seq = []
    for k in range(14):
        seq.append(("stalk", k, 80, {}))
    for k in range(8):
        seq.append(("tell", k, 80, {}))
    for k in range(6):
        seq.append(("breach", k, 50, dict(shake=(3 - k // 2, -2 + k // 3)) if k < 4 else {}))
    seq += [("clip", i, 90, {}) for i in range(4)] + [("clip", i, 45, {}) for i in range(4, 20)]
    seq += [("clip", 20, 50, {}), ("clip", 21, 50, {})]
    seq += [("clip", 22, 40, dict(white=1, shake=(5, -3), burst=0, jit=1)),
            ("clip", 22, 40, dict(white=1, shake=(-4, 2), burst=0, kb=4, jit=1)),
            ("clip", 22, 40, dict(red=1, shake=(3, -1), burst=1, kb=8, jit=1)),
            ("clip", 22, 40, dict(red=1, shake=(-1, 0), burst=1, kb=8, jit=1)),
            ("clip", 23, 70, dict(burst=2, kb=7)), ("clip", 24, 70, dict(red=1, kb=5)),
            ("clip", 25, 70, dict(kb=3)), ("clip", 26, 70, dict(red=1, kb=2))]
    seq += [("clip", i, 70, {}) for i in range(27, 30)] + [("clip", i, 60, {}) for i in range(30, 34)]
    seq += [("clip", i, 70, {}) for i in range(34, 41)]
    for k in range(8):
        seq.append(("dive", k, 50, dict(shake=(2, 1)) if k == 6 else {}))
    for k in range(12):
        seq.append(("swim", k, 80, {}))

    out, t, last_step = [], 0, -80
    for kind, k, ms, fxs in seq:
        if kind == "breach" and k == 0:
            bits.chunks(hx - 20, hy - 4, hy, 26, 13, spread=0.9, aim=-95)
            bits.chunks(hx + 30, hy - 4, hy, 18, 11, spread=0.8, aim=-70)
            bits.dust(hx, hy - 2, 6, spread=50, rise=1.2, size=(2, 4))
        if kind == "dive" and k == 4:
            bits.chunks(hx, hy - 4, hy, 30, 12, spread=1.0, aim=-90)
            bits.dust(hx, hy - 2, 7, spread=56, rise=1.0, size=(2, 4))
        if kind == "clip" and k == BITE_FRAME and fxs.get("white") and fxs.get("shake") == (5, -3):
            bits.chunks(hx - 40, hy - 8, hy, 10, 9, spread=0.8, aim=-110)
        for _ in range(ms // TICK):
            if t - last_step >= 80:
                if kind == "stalk":
                    fxp = 640 - 15 * k
                    bits.chunks(fxp + 10, FY - 2, FY, 2, 5, spread=0.4, aim=-60)
                if kind == "tell":
                    for _ in range(2):
                        bits.chunks(hx + rnd.uniform(-20, 20), hy - 1, hy, 1, 3, spread=0.3, aim=-90)
                if kind == "clip":
                    for px, py in body_edge_points(frames[k], rnd, 3):
                        bits.chunks(CX0 + px, CY0 + py, hy + rnd.uniform(-2, 6), 1, 1.5, spread=0.3, aim=90, palette=(fx.S4, fx.S5, fx.S2))
                    if rnd.random() < 0.35:
                        bits.dust(hx + rnd.uniform(-40, 40), hy - 2, 1, spread=6, rise=0.5, size=(2, 3))
                bits.step()
                last_step = t

            layer = Image.new("RGBA", (720, 450), (0, 0, 0, 0))
            d = ImageDraw.Draw(layer)
            dy = 0
            if kind == "stalk":
                fxp = 640 - 15 * k
                wake(d, fxp + 30, FY, k)
                fx.place(layer, fin, (fin.width / 2, fin_cut), fxp, FY, clip=lambda xs, ys: ys < FY)
                fx.mound(d, fxp - fin.width / 2 + 8, FY, 14, 4)
            elif kind == "tell":
                s = (k + 1) / 8
                if k < 4:
                    fx.place(layer, fin, (fin.width / 2, fin_cut), hx, FY + 8 * k, clip=lambda xs, ys: ys < FY)
                fx.mound(d, hx, hy, 10 + 40 * s, 3 + 5 * s)
                if k >= 3:
                    fx.draw_cracks(d, fx.crack_paths(hx, hy, 5, n=6, length=26 + 24 * s), grow=min(1.0, (k - 2) / 4))
            else:
                if kind == "breach":
                    dy = [170, 110, 60, 25, 8, 0][k]
                    idx = 0
                elif kind == "dive":
                    dy = [0, 6, 20, 45, 80, 130, 190, 260][k]
                    idx = 40
                elif kind == "swim":
                    idx = None
                else:
                    idx = k
                if kind in ("breach", "clip", "dive"):
                    fx.hole(d, hx, hy, rx, ry)
                    fx.place(layer, frames[idx], (0, 0), CX0, CY0 + dy, clip=near_rim)
                    bank(ImageDraw.Draw(layer))
                if kind == "swim":
                    fx.crater(d, hx, hy, rx * 0.8, ry, dark=k < 4)
                    fxp = hx + 20 + 18 * k
                    wake(d, fxp - 40, FY, 6)
                    fx.place(layer, fin.transpose(Image.FLIP_LEFT_RIGHT), (fin.width / 2, fin_cut), fxp, FY,
                             clip=lambda xs, ys: ys < FY)
            bits.draw(ImageDraw.Draw(layer))

            f = stage.copy()
            v = vclip[(t // 100) % len(vclip)]
            if fxs.get("white"):
                v = tint(v, (255, 250, 240))
            elif fxs.get("red"):
                v = redden(v)
            jit = fxs.get("jit", 0) * (1 if (t // 40) % 2 else -1)
            f.alpha_composite(v, (int(VICTIM_FEET[0] - vax - fxs.get("kb", 0) + jit), int(VICTIM_FEET[1] - vay)))
            f.alpha_composite(layer.resize((1440, 900), Image.NEAREST))
            if "burst" in fxs:
                cx, cy = 862, 536
                dd = ImageDraw.Draw(f)
                r0 = random.Random(11)
                size = (52, 38, 24)[fxs["burst"]]
                for i in range(11):
                    ang = math.radians(-200 + i * 20 + r0.uniform(-8, 8))
                    L = size * r0.uniform(0.6, 1.1)
                    w = max(2.0, size * 0.13)
                    tip = (cx + math.cos(ang) * L, cy + math.sin(ang) * L * 0.75)
                    nx, ny = -math.sin(ang) * w, math.cos(ang) * w
                    dd.polygon([(round(cx + nx), round(cy + ny)), (round(tip[0]), round(tip[1])), (round(cx - nx), round(cy - ny))],
                               fill=(fx.S6, fx.S5, fx.S4)[(i + fxs["burst"]) % 3], outline=fx.S1)
            sx, sy = fxs.get("shake", (0, 0))
            if (sx, sy) != (0, 0):
                g = stage.copy()
                g.paste(f, (sx, sy))
                f = g
            out.append(f.convert("RGB").crop((300, 120, 1420, 840)))
            t += TICK

    path = D / "anim2" / "shark-boss.mp4"
    c = av.open(str(path), mode="w")
    s = c.add_stream("libx264", rate=Fraction(1000, TICK))
    s.width, s.height = out[0].size
    s.pix_fmt = "yuv420p"
    s.options = {"crf": "14", "preset": "slow"}
    for im in out + [out[-1]] * (500 // TICK):
        for pkt in s.encode(av.VideoFrame.from_ndarray(np.asarray(im), format="rgb24")):
            c.mux(pkt)
    for pkt in s.encode():
        c.mux(pkt)
    c.close()
    keys = [out[i] for i in (20, 60, 92, 104, 140, 200, 262, 300) if i < len(out)]
    w, h = keys[0].size
    sheet = Image.new("RGB", (4 * (w // 2) + 12, 2 * (h // 2) + 4), (20, 20, 24))
    for n, im in enumerate(keys):
        sheet.paste(im.resize((w // 2, h // 2), Image.LANCZOS), ((n % 4) * (w // 2 + 4), (n // 4) * (h // 2 + 4)))
    sheet = sheet.resize((1800, round(sheet.height * 1800 / sheet.width)), Image.LANCZOS)
    sheet.save(D / "anim2" / "shark-boss-keys.png")
    print(path.name, len(out), "frames at", TICK, "ms", path.stat().st_size // 1024, "KB")


if __name__ == "__main__":
    main()
