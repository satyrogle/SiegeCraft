"""The sand shark's game sprite sheets, cut from the approved land scene (shark_land2.py, locked 1 Nov 2026).

Every frame is half in the sand like the scene: canvas row 148 sits on the sand line and everything below it is cut, with the
moving sand rim baked in (a rippling lip over the cut, the contact shadow above it, and grains pouring off its back). The rim
is Broken Wastes sand, so the shark's fight belongs on the broken_wastes stage.

Sheets go to Assets/Resources/CampaignArt/Beasts as beast_sandshark_<clip>.png and .json in the fighters' sheet format: 256 px
cells, 8 to a row, each frame's duration, and anchorX, anchorY (the sand line under the middle of its body). The shark's
pixels are twice the fighters' pixels, so the game draws its sheets at 2x. A few keys the fighters' sheets do not have:
"loop" (false for a one shot move), "hitFrame" (where its blow lands) and "scale" (2).

  idle     the breathing clip, a loop (60 ms a frame)
  bite     shark_bite2: gape 8 to 10, the jaws clamp on frame 11 (the hit, held 230 ms), thrash, open by 40
  sweep    the tail sweep, its blast thrown on frame 18
  dive     head first back under the sand, 280 ms
  surface  bursting up half in the sand where it bites from, with the splash along its length
  fin      what shows of it under the sand: its fin, the approved patrol's piece (one frame, its own anchor on the sand line)
  shadow   its shadow under the sand, the tail sweeping, a loop

beast_sandshark_parts.json: where each part sits in the idle cell (jaw, fin, spine, tail), for the target markers.
Volley has no clip yet (PixelLab, on Jacob's PC).

python -I shark_sheets.py
"""
import json
import math
import pathlib
import random
import sys
import uuid

import numpy as np
from PIL import Image, ImageDraw

HERE = pathlib.Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
import boss_scenes as bs  # noqa: E402
import shark_land2 as s2  # noqa: E402
import shark_presence as sp  # noqa: E402
from boss_scenes import A, fx  # noqa: E402

REPO = HERE.parents[3]
OUT = REPO / "Assets" / "Resources" / "CampaignArt" / "Beasts"
CELL, COLS, CUT = 256, 8, s2.CUT_ROW
RISE = 90                        # as the scene: rows still under the sand when the surfacing starts
SAND = (fx.S4, fx.S5, fx.S6)
META_PNG = REPO / "Assets" / "Resources" / "CampaignArt" / "hunter_warden_sw_idle.png.meta"
META_JSON = REPO / "Assets" / "Resources" / "CampaignArt" / "hunter_warden_sw_idle.json.meta"
META_DIR = REPO / "Assets" / "Resources" / "CampaignArt" / "Stages.meta"


def cyc_noise(x, i, n, keys, cell, seed=0):
    """Value noise along x that morphs over the frames of a loop and comes back to where it started at frame n."""
    k = i * keys / n
    k0, g = int(k) % keys, k - int(k)
    k1 = (k0 + 1) % keys
    a, f = divmod(x, cell)
    f /= cell
    f, g = f * f * (3 - 2 * f), g * g * (3 - 2 * g)

    def h(a, k):
        return ((((int(a) * 73856093) ^ (int(k) * 19349663) ^ seed) * 2654435761) % 1000) / 500.0 - 1.0
    return (h(a, k0) * (1 - f) + h(a + 1, k0) * f) * (1 - g) + (h(a, k1) * (1 - f) + h(a + 1, k1) * f) * g


def lip_loop(a, b, i, n, amp=1.0):
    """The scene's lip over a span, as a loop of n frames: the ripples travel whole cycles and the boil returns."""
    phase = 2 * math.pi * i / n
    xs = range(a - 4, b + 5)
    hs = []
    for x in xs:
        r = (3 + amp * (1.1 * math.sin(0.29 * x - 5 * phase) + 1.3 * cyc_noise(x, i, n, 10, 6))
             + 0.5 * cyc_noise(x, i, n, 16, 2, 7))
        hs.append(max(0, min(round(r), 2 * (x - (a - 5)), 2 * ((b + 5) - x))))
    return list(xs), hs


def cut_cell(im, dx=0, dy=0, rotate=0.0, pivot=None):
    """One frame placed in a cell with its row 148 on the sand line (moved by dx, dy and turned about pivot), cut below it."""
    if rotate:
        im = im.rotate(rotate, resample=Image.NEAREST, center=pivot)
    cell = Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))
    cell.paste(im, (round(dx), round(dy)), im)
    a = np.asarray(cell).copy()
    a[CUT:, :, 3] = 0
    return Image.fromarray(a, "RGBA")


class Pour:
    """Grains pouring off its back, worked out per frame so a loop's last frame runs into its first."""

    def __init__(self, seed, loop_ms):
        self.rnd = random.Random(seed)
        self.loop = loop_ms
        # streams fall from fixed spots along the back; each sends a grain every few frames, offset from the others
        self.streams = [(self.rnd.uniform(0.15, 0.9), self.rnd.uniform(0, 1), self.rnd.choice((120, 150, 200, 240))) for _ in range(7)]

    def draw(self, d, body, t_ms):
        a = np.asarray(body)[..., 3] > 0
        a[CUT - 2:] = False
        cols = np.nonzero(a.any(0))[0]
        if not len(cols):
            return
        lo, hi = cols.min(), cols.max()
        for n, (u, off, every) in enumerate(self.streams):
            x = int(lo + (hi - lo) * u)
            col = np.nonzero(a[:, x])[0]
            if not len(col):
                continue
            top = col.min()
            for k in range(6):
                age = ((t_ms - off * every) % every) + k * every
                y = top + 0.5 * 0.0011 * age * age   # the scene's grains fall at 0.22 px a tick, squared, 20 ms ticks
                if y < CUT - 1:
                    d.point((x + (k % 2), round(y)), fill=SAND[(n + k) % 3])


def finish(frame, lips, pour, d_t, bow=None):
    """Shade above the lip, then the lip and the grains, on a cut frame."""
    for xs, hs in lips:
        frame = s2_shade(frame, xs, hs)
    d = ImageDraw.Draw(frame)
    for xs, hs in lips:
        s2.draw_lip(d, xs, hs)
    if pour is not None:
        pour.draw(d, frame, d_t)
    return frame


def s2_shade(body, xs, hs):
    return s2.shade_above(body, xs, hs)


def spans(frame):
    return s2.spans_of(np.asarray(frame)[CUT - 1, :, 3] > 0)


def write_sheet(name, frames, durations, anchor, loop=True, hit=None, extra=None):
    rows = (len(frames) + COLS - 1) // COLS
    w, h = frames[0].size
    sheet = Image.new("RGBA", (COLS * w if len(frames) >= COLS else len(frames) * w, rows * h), (0, 0, 0, 0))
    meta = {"frames": []}
    for k, (fr, ms) in enumerate(zip(frames, durations)):
        x, y = (k % COLS) * w, (k // COLS) * h
        sheet.paste(fr, (x, y))
        meta["frames"].append({"filename": "%s_%02d" % (name, k), "frame": {"x": x, "y": y, "w": w, "h": h}, "rotated": False,
                               "trimmed": False, "spriteSourceSize": {"x": 0, "y": 0, "w": w, "h": h},
                               "sourceSize": {"w": w, "h": h}, "duration": int(ms)})
    meta["anchorX"], meta["anchorY"] = float(anchor[0]), int(anchor[1])
    meta["scale"] = 2
    meta["loop"] = loop
    if hit is not None:
        meta["hitFrame"] = hit
    if extra:
        meta.update(extra)
    png, js = OUT / (name + ".png"), OUT / (name + ".json")
    sheet.save(png)
    js.write_text(json.dumps(meta), encoding="utf-8")
    for path, template in ((png, META_PNG), (js, META_JSON)):
        meta_path = path.with_name(path.name + ".meta")
        if not meta_path.exists():
            text = template.read_text(encoding="utf-8")
            old = text.split("guid: ")[1].split()[0]
            meta_path.write_text(text.replace(old, uuid.uuid4().hex), encoding="utf-8")
    print(name, sheet.size, len(frames), "frames")


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    dir_meta = OUT.with_name(OUT.name + ".meta")
    if not dir_meta.exists():
        text = META_DIR.read_text(encoding="utf-8")
        dir_meta.write_text(text.replace(text.split("guid: ")[1].split()[0], uuid.uuid4().hex), encoding="utf-8")

    breathe, bite, sweep = (s2.clip(n) for n in ("shark_breathe", "shark_bite2", "shark_sweep"))
    # the breathing clip's one frame specks in front of the snout, cleared as in the scene
    snout = int(np.median([np.nonzero((np.asarray(b)[95:140, :, 3] > 0).any(0))[0].min() for b in breathe]))
    for n, b in enumerate(breathe):
        a = np.asarray(b).copy()
        a[90:146, :snout - 1, 3] = 0
        breathe[n] = Image.fromarray(a, "RGBA")
    still = bs.strip_base(Image.open(A / "shark-land-256.png"), floor=256)
    bb = still.getbbox()
    anchor = ((bb[0] + bb[2]) / 2, CUT)
    s2.G = CUT                       # the scene's lip and shade work on the sand line; here that is the cell's row 148

    # idle: a loop
    n = len(breathe) - 1             # frame 40 is frame 0 again
    pour = Pour(3, n * 60)
    idle = []
    for i in range(n):
        fr = cut_cell(breathe[i])
        idle.append(finish(fr, [lip_loop(a, b, i, n) for a, b in spans(fr)], pour, i * 60))
    write_sheet("beast_sandshark_idle", idle, [60] * n, anchor)

    # bite: a one shot from the idle pose; the hit on 11, held
    ms = [50] * 41
    ms[s2.SNAP_F] = 50 + s2.FREEZE + 60
    pour = Pour(5, sum(ms))
    out, t = [], 0
    for i, im in enumerate(bite):
        fr = cut_cell(im)
        thrash = 1.0 if s2.THRASH[0] <= i <= s2.THRASH[1] else 0.0
        out.append(finish(fr, [s2.lip_heights(a, b, t, 1.0 + 0.8 * thrash) for a, b in spans(fr)], pour, t))
        t += ms[i]
    write_sheet("beast_sandshark_bite", out, ms, anchor, loop=False, hit=s2.SNAP_F)

    # sweep: a one shot; the blast thrown on 18
    ms = [55] * 41
    pour = Pour(7, sum(ms))
    out, t = [], 0
    for i, im in enumerate(sweep):
        fr = cut_cell(im)
        out.append(finish(fr, [s2.lip_heights(a, b, t, 1.0) for a, b in spans(fr)], pour, t))
        t += ms[i]
    write_sheet("beast_sandshark_sweep", out, ms, anchor, loop=False, hit=s2.SWEEP_F)

    # dive: head first under, 14 frames of 20 ms, from the idle pose
    base = breathe[0]
    pivot = (s2.head_of(base)[0] + 10, CUT)
    out = []
    for k in range(14):
        u = (k + 1) / 14
        fr = cut_cell(base, -8 * u, 150 * u * u, rotate=52 * u, pivot=pivot)
        sp_ = spans(fr)
        fr = finish(fr, [s2.lip_heights(a, b, k * 20, 1.6) for a, b in sp_], None, 0)
        if 3 <= k < 9:
            fx.spikes(ImageDraw.Draw(fr), pivot[0] - 6, CUT, k, [14, 24, 28, 22, 14, 7][k - 3] * 0.7, dirs=[-165, -140, -110, -70, -40, -15])
        out.append(fr)
    write_sheet("beast_sandshark_dive", out, [20] * 14, anchor, loop=False)

    # surface: bursts up half in the sand, 18 frames of 20 ms, the splash along its length
    out = []
    for k in range(18):
        u = (k + 1) / 18
        fr = cut_cell(bite[0], 0, RISE * (1 - u) ** 2.2)
        sp_ = spans(fr)
        fr = finish(fr, [s2.lip_heights(a, b, k * 20, 1.6) for a, b in sp_], Pour(11, 360) if k > 6 else None, k * 20)
        size = [16, 26, 32, 28, 20, 12, 6][k] if k < 7 else 0
        if size:
            d = ImageDraw.Draw(fr)
            for a, b in sp_:
                m = max(1, (b - a) // 30)
                for j in range(m + 1):
                    fx.spikes(d, a + (b - a) * j / m, CUT + 1, k * 17 + j, size * 0.7, dirs=[-160, -135, -110, -70, -45, -20])
        out.append(fr)
    write_sheet("beast_sandshark_surface", out, [20] * 18, anchor, loop=False)

    # fin: the patrol's piece over the sand line
    level = Image.open(A / "shark_clean" / "00.png").convert("RGBA")
    fin = level.rotate(25, resample=Image.NEAREST, expand=True)
    fin = fin.crop(fin.getbbox())
    a = np.asarray(fin).copy()
    a[40:, :, 3] = 0                 # the scene shows its top 40 rows above the sand
    fin = Image.fromarray(a, "RGBA").crop((0, 0, fin.width, 40))
    cell = Image.new("RGBA", (fin.width, 40), (0, 0, 0, 0))
    cell.alpha_composite(fin)
    write_sheet("beast_sandshark_fin", [cell], [100], (fin.width / 2, 40), loop=True)

    # shadow: the whole shark seen from above under the sand, the tail sweeping; 24 frames, facing left
    out = []
    for k in range(24):
        sh = Image.new("RGBA", (CELL, 96), (0, 0, 0, 0))
        t = k * 2 * math.pi / 0.018 / 24     # one whole sweep of the tail over the loop
        sp.shadow(ImageDraw.Draw(sh), CELL / 2, 48, 210, True, 0.2, t, 0.3)
        out.append(sh)
    write_sheet("beast_sandshark_shadow", out, [60] * 24, (CELL / 2, 48))

    # parts: where each sits in the idle cell
    a = np.asarray(cut_cell(breathe[0]))[..., 3] > 0
    ys, xs = np.nonzero(a)
    hx, hy = s2.head_of(breathe[0])
    # the big dorsal fin peaks between columns 155 and 185 (the spear and the bone spines stand further forward)
    tops = [np.nonzero(a[:, x])[0].min() if a[:, x].any() else 999 for x in range(155, 186)]
    dorsal = 155 + int(np.argmin(tops))
    right = xs.max()
    tail_y = int(np.nonzero(a[:, right])[0].mean())
    mid = int((hx + right) / 2)
    parts = {"cell": CELL, "scale": 2, "anchorX": anchor[0], "anchorY": anchor[1],
             "jaw": [hx + 12, hy + 22], "fin": [dorsal + 4, int(min(tops)) + 10],
             "spine": [mid, int(np.nonzero(a[:, mid])[0].min()) + 4], "tail": [int(right) - 4, tail_y]}
    (OUT / "beast_sandshark_parts.json").write_text(json.dumps(parts, indent=1), encoding="utf-8")
    meta_path = OUT / "beast_sandshark_parts.json.meta"
    if not meta_path.exists():
        text = META_JSON.read_text(encoding="utf-8")
        meta_path.write_text(text.replace(text.split("guid: ")[1].split()[0], uuid.uuid4().hex), encoding="utf-8")
    print("parts", parts)


if __name__ == "__main__":
    main()
