"""Shrink each beast to sprite pixels the Dust Shark way (key the flat matte, shrink, lock to the tribe palette, clean
lone pixels, a 1 px dark outline), then one board at 2x: each row starts with that tribe's real Warden for scale, then
the three directions numbered 1 to 3. Also a silhouette check (every sprite as a black shape) for reading at game size.

The shark uses the Clarth palette and the Dust Shark's dusty push (dust-shark/make/fxkit.py, art.py); the bear uses
every colour of the Skullpeak Warden and Medic sheets. Pass names to flip so they face left: python -I pixel_board.py shark-2
"""
import pathlib
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFont

HERE = pathlib.Path(__file__).resolve().parent
KIT = HERE.parents[1] / "Chief-Cut-Ins-20261018" / "dust-shark" / "make"
sys.path.insert(0, str(KIT))
import fxkit as fx  # noqa: E402
import art  # noqa: E402

STUDY = HERE.parent / "pixel"
RAW = STUDY / "raw"
SPRITES = STUDY / "sprites"
CA = fx.CA
BG = (86, 97, 108)
SCALE = 2
GAP = 24


def sheet_colours(*names):
    cols = set()
    for n in names:
        im = Image.open(CA / (n + "_sw_idle.png")).convert("RGBA")
        cols |= {p[:3] for p in im.get_flattened_data() if p[3] > 0}
    return np.array(sorted(cols), np.float32)


SKULL = sheet_colours("skull_warden", "skull_medic")


def lock(arr, pal):
    d = (((arr[:, :, None, :3] - pal[None, None]) ** 2) * fx.W8).sum(-1)
    return pal[d.argmin(-1)]


def pixelize(small, pal, outline):
    arr = np.asarray(small).astype(np.float32)
    on = arr[..., 3] >= 128
    q = lock(arr, pal)
    out = np.zeros(arr.shape[:2] + (4,), np.uint8)
    out[on, :3] = q[on]
    out[on, 3] = 255
    c = out[..., :3].astype(np.int32)
    key3 = c[..., 0] * 65536 + c[..., 1] * 256 + c[..., 2]
    H, W = key3.shape
    for y in range(1, H - 1):
        for x in range(1, W - 1):
            if not on[y, x] or not (on[y - 1, x] and on[y + 1, x] and on[y, x - 1] and on[y, x + 1]):
                continue
            nb = [key3[y - 1, x], key3[y + 1, x], key3[y, x - 1], key3[y, x + 1]]
            if key3[y, x] not in nb and nb.count(nb[0]) == 4:
                out[y, x, :3] = c[y - 1, x]
    nbm = np.zeros_like(on)
    nbm[1:, :] |= on[:-1, :]
    nbm[:-1, :] |= on[1:, :]
    nbm[:, 1:] |= on[:, :-1]
    nbm[:, :-1] |= on[:, 1:]
    ring = nbm & ~on
    out[ring, :3] = outline
    out[ring, 3] = 255
    return Image.fromarray(out, "RGBA")


def sprite(name, box, flip):
    kind = name.split("-")[0]
    big = fx.key(RAW / (name + ".png"))
    if flip:
        big = big.transpose(Image.FLIP_LEFT_RIGHT)
    if kind == "shark":
        big = art.dusty(big)
    w, h = big.size
    s = min(box[0] / w, box[1] / h)
    small = big.convert("RGBa").resize((max(2, round(w * s)), max(2, round(h * s))), Image.BOX).convert("RGBA")
    pad = Image.new("RGBA", (small.width + 2, small.height + 2), (0, 0, 0, 0))
    pad.alpha_composite(small, (1, 1))
    if kind == "shark":
        return pixelize(pad, fx.PAL, fx.OUT)
    dark = tuple(int(v) for v in SKULL[np.argmin(SKULL.sum(1))])
    return pixelize(pad, SKULL, dark)


def fighter(name):
    clip, _ = fx.load_clip(name)
    f = clip[0][0]
    return f.crop(f.getbbox())


def font(size):
    for n in ("C:/Windows/Fonts/arialbd.ttf", "C:/Windows/Fonts/arial.ttf"):
        try:
            return ImageFont.truetype(n, size)
        except OSError:
            continue
    return ImageFont.load_default()


def main():
    flips = set(sys.argv[1:])
    SPRITES.mkdir(parents=True, exist_ok=True)
    rows = []
    for label, kind, warden, box in (("SAND SHARK", "shark", "clarth_warden", (256, 170)),
                                     ("CAGE BEAR", "bear", "skull_warden", (230, 220))):
        sprs = []
        for i in range(1, 4):
            name = "%s-%d" % (kind, i)
            if (RAW / (name + ".png")).exists():
                s = sprite(name, box, name in flips)
                s.save(SPRITES / (name + ".png"))
                sprs.append(s)
            else:
                sprs.append(None)
        rows.append((label, fighter(warden), sprs, box))

    title, num, small = font(30), font(34), font(18)
    width = GAP
    for _, f, _, box in rows:
        width = max(width, GAP + f.width * SCALE + GAP + 3 * (box[0] * SCALE + GAP))
    height = GAP + sum(46 + max(box[1] + 4, 140) * SCALE + GAP for *_, box in rows)
    board = Image.new("RGB", (width, height), BG)
    sil = Image.new("RGB", (width, height), (235, 235, 235))
    d, ds = ImageDraw.Draw(board), ImageDraw.Draw(sil)
    y = GAP
    for label, f, sprs, box in rows:
        d.text((GAP, y), label, fill=(240, 240, 240), font=title)
        ds.text((GAP, y), label, fill=(20, 20, 20), font=title)
        y += 46
        row_h = max(box[1] + 4, 140) * SCALE
        base = y + row_h
        fx2 = f.resize((f.width * SCALE, f.height * SCALE), Image.NEAREST)
        board.paste(fx2, (GAP, base - fx2.height), fx2)
        d.text((GAP, base + 2), "Warden", fill=(230, 230, 230), font=small)
        x = GAP + fx2.width + GAP
        for i, s in enumerate(sprs):
            if s is not None:
                s2 = s.resize((s.width * SCALE, s.height * SCALE), Image.NEAREST)
                px = x + (box[0] * SCALE - s2.width) // 2
                board.paste(s2, (px, base - s2.height), s2)
                black = Image.new("RGB", s2.size, (0, 0, 0))
                sil.paste(black, (px, base - s2.height), s2.split()[3])
            d.ellipse((x, y, x + 48, y + 48), fill=(255, 255, 255), outline=(20, 20, 20), width=3)
            d.text((x + 24, y + 24), str(i + 1), fill=(20, 20, 20), font=num, anchor="mm")
            ds.text((x + 24, y + 24), str(i + 1), fill=(20, 20, 20), font=num, anchor="mm")
            x += box[0] * SCALE + GAP
        y = base + GAP
    board.save(STUDY.parent / "beast-concepts-pixel-board.png")
    sil.save(STUDY / "check-silhouettes.png")
    print("board", board.size, "flipped", sorted(flips))


if __name__ == "__main__":
    main()
