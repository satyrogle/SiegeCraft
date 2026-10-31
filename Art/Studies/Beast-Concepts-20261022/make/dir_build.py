"""Three directions per beast, the Dust Shark way: key each picked FLUX picture, shrink it to sprite pixels locked to
its tribe's palette (fxkit for the Clarth shark, the Skullpeak Warden and Medic sheets for the bear), stand it at real
size on the cleared real fight (dir_stage.py), then one board per beast with the web refs each direction learned from.

python -I dir_build.py   (run dir_stage.py first)
"""
import math
import pathlib
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFont

HERE = pathlib.Path(__file__).resolve().parent
KIT = HERE.parents[1] / "Chief-Cut-Ins-20261018" / "dust-shark" / "make"
sys.path[:0] = [str(KIT), str(HERE)]
import fxkit as fx  # noqa: E402
import art  # noqa: E402
from pixel_board import SKULL, pixelize  # noqa: E402

DIR = HERE.parent / "directions"
RAW, FIGHT, SPRITES = DIR / "raw", DIR / "fight", DIR / "sprites"
WEB = pathlib.Path("C:/Users/jacob/.claude/projects/C--Users-jacob-KindredSiege-Clean/"
                   "ea710571-4dc8-4ad1-b2bc-674ede1b90b7/tool-results")
BLOB = {k: WEB / ("mcp-Claude_Browser-blob-" + v) for k, v in {
    "bowmouth": "1792747520990-54rfu2.jpg", "goblin": "1792747528237-5zcm2v.jpg",
    "terraria": "1792747542605-5lk0lr.jpg", "calamity": "1792747554621-0tvbnp.jpg",
    "skink": "1792747567831-j65akn.jpg", "cleric": "1792747578859-vbcd54.jpg",
    "colossus": "1792747588829-welslt.jpg", "dancing": "1792747600153-0nw8ef.jpg",
    "gorilla": "1792747615324-lzf6em.jpg", "pixelbears": "1792747631434-t6w5lp.jpg"}.items()}
SKULL_DARK = tuple(int(v) for v in SKULL[np.argmin(SKULL.sum(1))])

SAND_LINE = 604
FEET = 662
CX = 960
CROPS = {"shark": (520, 300, 1160, 700), "bear": (500, 285, 1140, 685)}
DIVE_CROP = (780, 530, 1160, 660)

SHARK = dict(
    key="shark", stage="alphahunt", title="Sand shark: three directions",
    brief="Hunger under the sand. The first beast of every run, fast and easy to read: the ground is not safe. "
          "Jaw, fin and wedge head, spine row, tail. Locked to the Clarth palette, about 2.5 Wardens long.",
    near="Lookalike check: Calamity's Great Sand Shark is long, tan and covered in pangolin plates. "
         "All three keep smooth red scales so they don't drift into it.",
    rows=[
        dict(id="A", name="Horn rack", raw="sharkA-2", length=312,
             refs=[("terraria", (8, 165, 277, 305)), ("dustshark", None)],
             learned="Terraria's sand shark, our own Dust Shark",
             idea="A long low torpedo, a tall fin, and a rack of horn spines behind it that it fires (Volley). "
                  "The fin and the rack are the warning.",
             good="Reads as a shark at a glance, and it is the animal the Clarth chief's Dust Shark is made of.",
             risk="The most ordinary body of the three. The rack carries all of its character."),
        dict(id="B", name="Shovel head", raw="sharkB-4", length=316,
             refs=[("bowmouth", (7, 355, 243, 512)), ("skink", (7, 355, 175, 497))],
             learned="the bowmouth guitarfish, the sandfish skink",
             idea="A long flat head that ploughs the sand (Dive), thorn ridges from the brow down the whole back, "
                  "dark skink bands across the red.",
             good="The thorn line from head to tail is the clearest warning when it swims under you.",
             risk="At game size the flat head still sits close to a normal shark's."),
        dict(id="C", name="Goblin jaw", raw="sharkC-2", length=318,
             refs=[("goblin", (7, 165, 205, 305)), ("goblin", (455, 165, 680, 305))],
             learned="the goblin shark, whose jaw shoots forward out of its head",
             idea="A blade snout over a jaw that throws forward full of needle teeth (Bite). "
                  "Horn spines on the back, a long lean body.",
             good="The strangest outline. Nobody mistakes it for a plain shark.",
             risk="The snout is thin, so it can thin out at game size."),
    ])

BEAR = dict(
    key="bear", stage="skullpeak", title="Cage bear: three directions",
    brief="A great beast its own steel is killing. Skullpeak's beast, slow and heavy: dread, then pity when it falls. "
          "Claw, cage, roar, heart. Locked to the Skullpeak palette, about two Wardens tall.",
    near="Lookalike check: one giant arm on a hunched beast is Bloodborne's Cleric Beast. "
         "Ours stays a round heavy block with a steel claw and a caged heart, never gaunt.",
    rows=[
        dict(id="A", name="Low walker", raw="bearA-2", height=236,
             refs=[("gorilla", (7, 165, 217, 318)), ("warden", None)],
             learned="the silverback's knuckle walk, the Skullpeak Warden's claw arm",
             idea="Walks low on a long steel claw of finger bones that reaches ahead of it (Grab). "
                  "The cage sits under its chest, the heart glows inside.",
             good="The heaviest and most animal. The claw leads the whole body.",
             risk="The steel fingers are thin and may read as blades, not a grab."),
        dict(id="B", name="Rearing grab", raw="bearB-1", height=272,
             refs=[("cleric", (7, 165, 270, 310)), ("pixelbears", (127, 208, 231, 350))],
             learned="the Cleric Beast's one giant arm, menacing pixel bears",
             idea="Rears up roaring (Roar) with the steel grab claw raised high, "
                  "the cage and the heart turned to face your squad.",
             good="Shows the player the cage and the heart, the clearest weak point of the three.",
             risk="Standing up is the bear suit risk, and this claw looks bolted on, not grown."),
        dict(id="C", name="Chained", raw="bearC-2", height=240,
             refs=[("dancing", (8, 108, 110, 248)), ("colossus", (7, 165, 153, 345))],
             learned="old prints of chained dancing bears, Shadow of the Colossus",
             idea="Sits slumped with its head bowed, a huge hook claw too heavy to lift, "
                  "a cuff of steel at the wrist like a chained dancing bear.",
             good="Pity first, then dread when it rises. The closest to the brief's sorrow.",
             risk="The least threatening at a glance, and the cuff is bolted on, not grown."),
    ])


def font(size, bold=False):
    for n in (("C:/Windows/Fonts/segoeuib.ttf",) if bold else ()) + ("C:/Windows/Fonts/segoeui.ttf",
                                                                       "C:/Windows/Fonts/arial.ttf"):
        try:
            return ImageFont.truetype(n, size)
        except OSError:
            continue
    return ImageFont.load_default()


def keyed(name, kind):
    big = fx.key(RAW / (name + ".png"), tol=70 if kind == "shark" else 45)
    return art.dusty(big) if kind == "shark" else big


def shrink(big, s, kind):
    w, h = big.size
    small = big.convert("RGBa").resize((max(2, round(w * s)), max(2, round(h * s))), Image.BOX).convert("RGBA")
    pad = Image.new("RGBA", (small.width + 2, small.height + 2), (0, 0, 0, 0))
    pad.alpha_composite(small, (1, 1))
    return pixelize(pad, fx.PAL, fx.OUT) if kind == "shark" else pixelize(pad, SKULL, SKULL_DARK)


def sand(x):
    return SAND_LINE + 2 * np.sin(x / 37.0)


def cut_lip(frame, x0, x1, spray=False):
    """Where the body meets the sand: a dark parting, a light lip and a sand ridge piled along the near side,
    plus, when it is surging out, a hard edged spray of sand off its nose (Saint11's sharp shapes, no noise)."""
    d = ImageDraw.Draw(frame)
    for x in range(x0, x1):
        y = round(sand(x))
        d.point((x, y), fill=fx.S2)
        d.point((x, y + 1), fill=fx.S3)
        if x % 3:
            d.point((x, y - 1), fill=fx.S5)
    ridge = []
    for x in range(x0 - 10, x1 + 11, 4):
        k = min(1.0, (x - x0 + 10) / 50.0, (x1 + 10 - x) / 50.0)
        ridge.append((x, round(sand(x)) + 2 - round(4 * max(0.0, k))))
    poly = ridge + [(x1 + 10, round(sand(x1)) + 4), (x0 - 10, round(sand(x0)) + 4)]
    d.polygon(poly, fill=fx.S5)
    d.line(ridge, fill=fx.S6, width=1)
    for sgn in (-1, 1):
        ex = x0 if sgn < 0 else x1
        d.line([(ex, round(sand(ex))), (ex + sgn * 26, round(sand(ex)) - 9)], fill=fx.S5, width=3)
        d.line([(ex, round(sand(ex)) + 1), (ex + sgn * 30, round(sand(ex)) - 4)], fill=fx.S6, width=1)
    if spray:
        fx.spikes(d, x0 + 8, round(sand(x0)) - 1, 7, 26, dirs=[-172, -152, -132, -112])


def shark_frames(row, big):
    """Surfaced: nose up out of the sand, belly under it. Under the sand: only the fin and spines show."""
    s = row["length"] / big.width
    up, piv = fx.native(big, angle=-7, scale=s)
    flat, piv2 = fx.native(big, angle=0, scale=s)
    stage = Image.open(FIGHT / "alphahunt-clean.png").convert("RGBA")
    probe = stage.copy()
    fx.place(probe, up, piv, CX, SAND_LINE - 24, clip=lambda xs, ys: ys < SAND_LINE)
    on = (np.asarray(probe.convert("RGB"), np.int32) != np.asarray(stage.convert("RGB"), np.int32)).any(2)
    cols = np.nonzero(on[SAND_LINE - 3:SAND_LINE].any(0))[0]
    x0, x1 = int(cols.min()), int(cols.max())
    hx, hrx, hry = (x0 + x1) / 2, (x1 - x0) / 2 + 12, 9
    surf = stage.copy()
    fx.hole(ImageDraw.Draw(surf), hx, SAND_LINE, hrx, hry)

    def near_rim(xs, ys):
        return ys < SAND_LINE + hry * 0.55 * np.sqrt(np.clip(1 - ((xs - hx) / hrx) ** 2, 0, 1))

    fx.place(surf, up, piv, CX, SAND_LINE - 24, clip=near_rim)
    d = ImageDraw.Draw(surf)
    fx.front_lip(d, hx, SAND_LINE, hrx, hry)
    fx.spikes(d, x0 + 4, SAND_LINE + 3, 7, 26, dirs=[-172, -152, -132, -112])
    dive = stage.copy()
    a = np.asarray(flat)
    rows_on = np.nonzero(a[..., 3].any(1))[0]
    top = rows_on.min()
    fin_rows = np.nonzero((a[..., 3] > 0).sum(1) > a.shape[1] * 0.45)[0]
    body_top = fin_rows.min() if len(fin_rows) else top + (rows_on.max() - top) // 2
    fx.place(dive, flat, piv2, CX, SAND_LINE + 4 - body_top + piv2[1], clip=lambda xs, ys: ys < sand(xs))
    don = np.asarray(dive.convert("RGB"), np.int32) != np.asarray(stage.convert("RGB"), np.int32)
    dcols = np.nonzero(don.any(2)[SAND_LINE - 4:SAND_LINE + 1].any(0))[0]
    if len(dcols):
        cut_lip(dive, int(dcols.min()), int(dcols.max()))
    return flat, surf.convert("RGB"), dive.convert("RGB")


def polish_steel(spr):
    """The brief's steel is polished, like the Warden's claw. FLUX painted it dark gunmetal, which sinks into the
    dark fur, so lift only the cool greys (blue at least equal to red, little colour) and lock them again."""
    a = np.asarray(spr).astype(np.float32)
    rgb = a[..., :3]
    mx, mn = rgb.max(-1), rgb.min(-1)
    steel = (a[..., 3] > 0) & (rgb[..., 2] >= rgb[..., 0] - 4) & (mx - mn < 30) & (mx > 28)
    rgb[steel] = 255.0 * (rgb[steel] / 255.0) ** 0.6
    rgb[steel] = lock(rgb[steel][None], SKULL)[0]
    a[..., :3] = rgb
    return Image.fromarray(a.astype(np.uint8), "RGBA")


def lock(arr, pal):
    d = (((arr[:, :, None, :3] - pal[None, None]) ** 2) * fx.W8).sum(-1)
    return pal[d.argmin(-1)]


def bear_frames(row, big):
    s = row["height"] / big.height
    spr = polish_steel(shrink(big, s, "bear"))
    stage = Image.open(FIGHT / "skullpeak-clean.png").convert("RGBA")
    bb = spr.getbbox()
    stage.alpha_composite(spr, (round(CX - (bb[0] + bb[2]) / 2), FEET - bb[3]))
    return spr, stage.convert("RGB"), None


def ref_thumb(key, box, size=(192, 128)):
    if key == "dustshark":
        im = fx.key(KIT.parent / "raw" / "shark-open-2.png")
        im = art.dusty(im)
        bg = Image.new("RGBA", im.size, (34, 30, 34, 255))
        bg.alpha_composite(im)
        im = bg.convert("RGB")
    elif key == "warden":
        clip, _ = fx.load_clip("skull_warden")
        f = clip[0][0]
        f = f.crop(f.getbbox())
        can = Image.new("RGBA", size, (34, 30, 34, 255))
        can.alpha_composite(f, ((size[0] - f.width) // 2, size[1] - f.height))
        return can.convert("RGB")
    else:
        im = Image.open(BLOB[key]).convert("RGB").crop(box)
    im.thumbnail(size, Image.LANCZOS)
    can = Image.new("RGB", size, (34, 30, 34))
    can.paste(im, ((size[0] - im.width) // 2, (size[1] - im.height) // 2))
    return can


def wrap(d, text, f, width):
    lines, cur = [], ""
    for w in text.split():
        t = (cur + " " + w).strip()
        if d.textlength(t, font=f) <= width:
            cur = t
        else:
            lines.append(cur)
            cur = w
    return lines + [cur] if cur else lines


def para(d, xy, text, f, width, fill, gap=4):
    x, y = xy
    for line in wrap(d, text, f, width):
        d.text((x, y), line, font=f, fill=fill)
        y += f.size + gap
    return y


def top_colours(spr, n=12):
    a = np.asarray(spr).reshape(-1, 4)
    a = a[a[:, 3] > 0][:, :3]
    cols, counts = np.unique(a, axis=0, return_counts=True)
    order = np.argsort(-counts)[:n]
    return [tuple(int(v) for v in cols[i]) for i in order]


def board(spec, made):
    W, ROW, TOP = 1500, 446, 112
    H = TOP + 3 * ROW + 96
    bd = Image.new("RGB", (W, H), (24, 22, 26))
    d = ImageDraw.Draw(bd)
    ink, dim, good, bad = (238, 232, 222), (170, 164, 160), (150, 205, 140), (232, 130, 112)
    d.text((24, 16), spec["title"], font=font(34, True), fill=ink)
    para(d, (24, 62), spec["brief"], font(17), W - 48, dim)
    for i, row in enumerate(spec["rows"]):
        y = TOP + i * ROW
        d.rounded_rectangle((12, y, W - 12, y + ROW - 12), radius=10, fill=(34, 31, 36))
        d.text((28, y + 12), "%s. %s" % (row["id"], row["name"]), font=font(30, True), fill=ink)
        para(d, (28, y + 54), "Learned from: " + row["learned"], font(15), 392, dim, gap=2)
        for k, (key, box) in enumerate(row["refs"]):
            bd.paste(ref_thumb(key, box), (28 + k * 200, y + 98))
        para(d, (28, y + 238), row["idea"], font(17), 392, ink)
        sprite, fight, dive = made[row["id"]]
        bd.paste(fight.crop(CROPS[spec["key"]]), (440, y + 18))
        d.text((444, y + 18 + 402), "In the real fight, true size", font=font(14), fill=dim)
        rx = 1100
        ry = y + 18
        if dive is not None:
            bd.paste(dive.crop(DIVE_CROP), (rx, ry))
            d.text((rx, ry + 134), "Under the sand: only the fin and spines", font=font(14), fill=dim)
            ry += 170
        else:
            box = sprite.crop(sprite.getbbox())
            can = Image.new("RGBA", (380, 290), (46, 42, 48, 255))
            can.alpha_composite(box, ((380 - box.width) // 2, 290 - box.height - 6))
            bd.paste(can.convert("RGB"), (rx, ry))
            d.text((rx, ry + 294), "The sprite alone, true size", font=font(14), fill=dim)
            ry += 322
        ry = para(d, (rx, ry), "Good: " + row["good"], font(16), 376, good)
        para(d, (rx, ry + 6), "Risk: " + row["risk"], font(16), 376, bad)
    fy = TOP + 3 * ROW
    para(d, (24, fy), spec["near"], font(16), W - 48, ink)
    d.text((24, fy + 50), "Colours used:", font=font(15), fill=dim)
    x = 130
    for c in top_colours(made["A"][0]):
        d.rectangle((x, fy + 50, x + 26, fy + 72), fill=c)
        x += 30
    d.text((x + 16, fy + 52), "Refs are for learning only. Nothing from them goes in the game.", font=font(15), fill=dim)
    return bd


def main():
    SPRITES.mkdir(parents=True, exist_ok=True)
    for spec in (SHARK, BEAR):
        made = {}
        for row in spec["rows"]:
            big = keyed(row["raw"], spec["key"])
            sprite, fight, dive = (shark_frames if spec["key"] == "shark" else bear_frames)(row, big)
            tag = "%s-%s" % (spec["key"], row["id"])
            sprite.save(SPRITES / (tag + ".png"))
            fight.save(FIGHT / (tag + "-fight.png"))
            if dive is not None:
                dive.save(FIGHT / (tag + "-under-sand.png"))
            made[row["id"]] = (sprite, fight, dive)
            print(tag, "sprite", sprite.getbbox())
        b = board(spec, made)
        b.save(DIR / ("%s-three-directions.png" % spec["key"]))
        print(spec["key"], "board", b.size)


if __name__ == "__main__":
    main()
