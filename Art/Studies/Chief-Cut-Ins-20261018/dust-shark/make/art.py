"""The posed creature art: keyed Comfy pictures, pushed toward the Clarth dusty reds before the palette lock."""
import numpy as np
from PIL import Image
import fxkit as fx

RAW = fx.STUDY / "raw"


def dusty(img, sat=0.75, hue=0.015, val=0.93):
    a = np.asarray(img).copy()
    hsv = np.asarray(img.convert("RGB").convert("HSV")).astype(np.float32)
    hsv[..., 0] = (hsv[..., 0] + hue * 255) % 255
    hsv[..., 1] *= sat
    hsv[..., 2] = np.clip(hsv[..., 2] * val, 0, 255)
    rgbm = np.asarray(Image.fromarray(hsv.astype(np.uint8), "HSV").convert("RGB"))
    a[..., :3] = rgbm
    return Image.fromarray(a, "RGBA")


def shark():
    op = fx.key(RAW / "shark-open-2.png")
    sh = fx.key(RAW / "shark-shut-2.png")
    return dusty(op), dusty(sh)


def maw():
    return dusty(fx.key(RAW / "maw-open-1.png"), sat=0.7)


def mouth_centre(spr):
    """Centroid of the dark mouth inside a pixelized open shark."""
    a = np.asarray(spr).astype(int)
    on = a[..., 3] > 0
    lum = a[..., 0] * 0.3 + a[..., 1] * 0.59 + a[..., 2] * 0.11
    red = (a[..., 0] > a[..., 1] * 1.6) & (lum < 70) & on
    ys, xs = np.nonzero(red)
    return float(xs.mean()), float(ys.mean())


def unit_height(name):
    clip, (ax, ay) = fx.load_clip(name)
    bb = clip[0][0].getbbox()
    return ay - bb[1], (bb[0] + bb[2]) / 2 - ax


if __name__ == "__main__":
    op, sh = shark()
    hunter = fx.load_clip("hunter_medic")[0][0][0]
    hunter = hunter.crop(hunter.getbbox())
    tiles = [hunter]
    for ang, src in ((40, op), (20, op), (5, op), (-5, sh), (-30, sh), (-55, sh)):
        spr, piv = fx.native(src, angle=ang, scale=0.15)
        tiles.append(spr.crop(spr.getbbox()))
    for ang, src in ((85, op), (85, sh)):
        spr, piv = fx.native(src, angle=ang, scale=0.26)
        if src is op:
            print("B mouth centre rel pivot", [round(v - p) for v, p in zip(mouth_centre(spr), piv)])
        tiles.append(spr.crop(spr.getbbox()))
    m, _ = fx.native(maw(), scale=0.36, sx=1.0, sy=1.0)
    tiles.append(m.crop(m.getbbox()))
    W = sum(t.width for t in tiles) + 12 * len(tiles)
    H = max(t.height for t in tiles) + 20
    sheet = Image.new("RGBA", (W, H), (225, 141, 66, 255))
    x = 6
    for t in tiles:
        sheet.alpha_composite(t, (x, H - 10 - t.height))
        x += t.width + 12
    sheet = sheet.resize((sheet.width * 2, sheet.height * 2), Image.NEAREST)
    sheet.convert("RGB").save("pose-test-2.png")
    for n in ("hunter_marksman", "hunter_medic", "hunter_warden"):
        print(n, unit_height(n))
