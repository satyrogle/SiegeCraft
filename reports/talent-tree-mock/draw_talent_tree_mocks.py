#!/usr/bin/env python3
"""Kindred Siege talent tree: three mock takes.

Design mock only. The game does not read anything in this folder.
Everything is drawn at 1x (640 x 360 game screen) and saved at 3x with
nearest neighbour scaling.

Needs Pillow, numpy and the X11 bitmap fonts from the xfonts-cyrillic
package (koi5x8, koi6x13b, koi7x14b, koi9x15b).
"""
import gzip
import io
import math
import os
import random
import tempfile

import numpy as np
from PIL import Image, ImageDraw, ImageFont, PcfFontFile

SCALE = 3
OUT = os.path.dirname(os.path.abspath(__file__))
FONT_DIR = "/usr/share/fonts/X11/cyrillic"

# ---------------------------------------------------------------- palette

P = dict(
    ink=(10, 8, 16), night=(18, 14, 28), deep=(28, 22, 42), panel=(40, 31, 58),
    panel2=(56, 44, 78), edge=(84, 68, 110), mist=(132, 120, 150),
    parch=(234, 216, 170), parch2=(196, 172, 120), parch3=(140, 116, 76),
    gold=(244, 196, 80), gold2=(196, 140, 44), gold3=(120, 82, 26),
    cyan=(96, 240, 232), cyan2=(40, 168, 186), cyan3=(20, 92, 110),
    red=(236, 84, 76), red2=(156, 40, 52), red3=(84, 22, 32),
    green=(164, 210, 92), green2=(96, 134, 50), green3=(48, 72, 32),
    purple=(190, 136, 252), purple2=(116, 70, 180), purple3=(60, 36, 100),
    blue=(116, 188, 255), blue2=(60, 112, 198), blue3=(30, 56, 112),
    white=(255, 248, 228), skin=(214, 150, 104), skin2=(156, 96, 62),
    brown=(112, 70, 42), brown2=(72, 44, 28), brown3=(44, 28, 20),
    locked=(64, 58, 78), locked2=(98, 90, 114),
)

RAMP = dict(
    gold=(P["white"], P["gold"], P["gold3"]),
    dim=(P["mist"], P["locked2"], P["locked"]),
    open=(P["parch"], P["parch2"], P["parch3"]),
    cyan=(P["white"], P["cyan"], P["cyan3"]),
    green=(P["white"], P["green"], P["green3"]),
    red=(P["white"], P["red"], P["red3"]),
    purple=(P["white"], P["purple"], P["purple3"]),
    blue=(P["white"], P["blue"], P["blue3"]),
)

# ---------------------------------------------------------------- fonts


def load_fonts():
    tmp = tempfile.mkdtemp()
    fonts = {}
    for key, name in dict(s="koi5x8", m="koi6x13b", l="koi7x14b", xl="koi9x15b").items():
        data = gzip.open(os.path.join(FONT_DIR, name + ".pcf.gz")).read()
        PcfFontFile.PcfFontFile(io.BytesIO(data), "iso8859-1").save(os.path.join(tmp, name))
        fonts[key] = ImageFont.load(os.path.join(tmp, name + ".pil"))
    return fonts


F = load_fonts()


def tw(s, f="s"):
    return int(F[f].getlength(s))


def text(d, x, y, s, f="s", col=P["parch"], sh=P["ink"], anchor="l", outline=False):
    w = tw(s, f)
    if anchor == "c":
        x -= w // 2
    elif anchor == "r":
        x -= w
    if outline:
        for ox, oy in ((-1, 0), (1, 0), (0, -1), (0, 1), (1, 1), (-1, -1), (1, -1), (-1, 1)):
            d.text((x + ox, y + oy), s, font=F[f], fill=sh)
    elif sh is not None:
        d.text((x + 1, y + 1), s, font=F[f], fill=sh)
    d.text((x, y), s, font=F[f], fill=col)
    return w


def wrap(s, width, f="s"):
    lines, cur = [], ""
    for word in s.split():
        trial = (cur + " " + word).strip()
        if tw(trial, f) <= width:
            cur = trial
        else:
            lines.append(cur)
            cur = word
    if cur:
        lines.append(cur)
    return lines


def para(d, x, y, s, width, col=P["parch"], f="s", lh=9):
    for line in wrap(s, width, f):
        text(d, x, y, line, f, col)
        y += lh
    return y

# ---------------------------------------------------------------- pixels


def plot(img, x, y, col):
    if 0 <= x < img.width and 0 <= y < img.height:
        img.putpixel((int(x), int(y)), col)


def line_pts(x0, y0, x1, y1):
    x0, y0, x1, y1 = int(round(x0)), int(round(y0)), int(round(x1)), int(round(y1))
    pts = []
    dx, dy = abs(x1 - x0), -abs(y1 - y0)
    sx, sy = (1 if x0 < x1 else -1), (1 if y0 < y1 else -1)
    err = dx + dy
    while True:
        pts.append((x0, y0))
        if x0 == x1 and y0 == y1:
            break
        e2 = 2 * err
        if e2 >= dy:
            err += dy
            x0 += sx
        if e2 <= dx:
            err += dx
            y0 += sy
    return pts


def pline(img, pts, col, width=1, dash=None, phase=0):
    """Polyline. dash=(on, off) draws a dashed line."""
    i = phase
    for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
        seg = line_pts(x0, y0, x1, y1)
        steep = abs(y1 - y0) > abs(x1 - x0)
        for (x, y) in seg:
            if dash is None or (i % (dash[0] + dash[1])) < dash[0]:
                plot(img, x, y, col)
                if width >= 2:
                    plot(img, x + (1 if steep else 0), y + (0 if steep else 1), col)
            i += 1


def circle_pts(cx, cy, r, steps=None):
    steps = steps or max(24, int(2 * math.pi * r * 1.5))
    seen, pts = set(), []
    for k in range(steps):
        a = 2 * math.pi * k / steps
        p = (int(round(cx + r * math.cos(a))), int(round(cy + r * math.sin(a))))
        if p not in seen:
            seen.add(p)
            pts.append(p)
    return pts


def ring(img, cx, cy, r, col, dash=None):
    for i, (x, y) in enumerate(circle_pts(cx, cy, r)):
        if dash is None or (i % (dash[0] + dash[1])) < dash[0]:
            plot(img, x, y, col)


def disc(d, cx, cy, r, col):
    d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=col)


def sprite(img, x, y, rows, pal):
    for j, row in enumerate(rows):
        for i, ch in enumerate(row):
            if ch in ". ":
                continue
            plot(img, x + i, y + j, pal[ch])


def mirror(rows):
    return [r + r[::-1] for r in rows]


BAYER = np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]) / 16.0


def dither(t, cols):
    """Map a 0..1 field to a list of colours with ordered (Bayer) dithering."""
    n = len(cols) - 1
    v = np.clip(t, 0, 1) * n
    base = np.floor(v).astype(int)
    frac = v - base
    h, w = t.shape
    th = np.tile(BAYER, (h // 4 + 1, w // 4 + 1))[:h, :w]
    idx = np.clip(base + (frac > th), 0, n)
    return Image.fromarray(np.array(cols, dtype=np.uint8)[idx])


def panel(d, x0, y0, x1, y1, fill=P["deep"], edge=P["gold2"], rivets=True, tech=False):
    d.rectangle([x0, y0, x1, y1], fill=P["ink"])
    d.rectangle([x0 + 1, y0 + 1, x1 - 1, y1 - 1], fill=edge)
    d.rectangle([x0 + 2, y0 + 2, x1 - 2, y1 - 2], fill=P["ink"])
    d.rectangle([x0 + 3, y0 + 3, x1 - 3, y1 - 3], fill=fill)
    if tech:
        d.line([x0 + 5, y1 - 4, x1 - 5, y1 - 4], fill=P["cyan3"])
        d.line([x0 + 5, y1 - 4, x0 + 9, y1 - 4], fill=P["cyan2"])
    if rivets:
        for cx, cy in ((x0 + 1, y0 + 1), (x1 - 1, y0 + 1), (x0 + 1, y1 - 1), (x1 - 1, y1 - 1)):
            d.rectangle([cx - 1, cy - 1, cx + 1, cy + 1], fill=P["ink"])
            d.point((cx, cy), fill=P["gold"])

# ---------------------------------------------------------------- icons (9x9, k=outline 1=main 2=light 3=shade)


ICONS = dict(
    shield=[
        "kkkkkkkkk",
        "k2211113k",
        "k2111113k",
        "k2111113k",
        "k1111113k",
        ".k11113k.",
        ".k11133k.",
        "..k133k..",
        "...kkk...",
    ],
    sword=[
        "......kkk",
        ".....k22k",
        "....k21k.",
        "...k21k..",
        "k.k21k...",
        "kk21k....",
        ".k3kk....",
        "k3k.kk...",
        "kk.......",
    ],
    target=[
        "..kkkkk..",
        ".k12221k.",
        "k1kkkkk1k",
        "k2k111k3k",
        "k2k1k1k3k",
        "k2k111k3k",
        "k1kkkkk1k",
        ".k13331k.",
        "..kkkkk..",
    ],
    knot=[
        ".kkk.kkk.",
        "k221k221k",
        "k1kk1kk1k",
        ".k1k1k1k.",
        "..k111k..",
        ".k1k1k1k.",
        "k1kk1kk3k",
        "k133k133k",
        ".kkk.kkk.",
    ],
    drop=[
        "....k....",
        "...k2k...",
        "...k1k...",
        "..k211k..",
        ".k21111k.",
        ".k21111k.",
        ".k11113k.",
        "..k133k..",
        "...kkk...",
    ],
    eye=[
        "....k....",
        "...k2k...",
        "..k212k..",
        ".k1kkk1k.",
        "k12k2k13k",
        ".k1kkk3k.",
        "..k133k..",
        "...k3k...",
        "....k....",
    ],
    chest=[
        ".kkkkkkk.",
        "k2222221k",
        "k1111113k",
        "kkkk2kkkk",
        "k111k113k",
        "k111k113k",
        "k1111113k",
        "k3333333k",
        "kkkkkkkkk",
    ],
)

BONE = [
    ".kk.....kk.",
    "k22kkkkk21k",
    ".k2222221k.",
    "k21kkkkk13k",
    ".kk.....kk.",
]

LOCK = [
    "..111..",
    ".1...1.",
    ".1...1.",
    "2222222",
    "2222222",
    "222k222",
    "222k222",
    "3333333",
]

CHEVRON = [
    "k....",
    "1k...",
    "11k..",
    "111k.",
    "1111k",
    "111k.",
    "11k..",
    "1k...",
    "k....",
]


def icon(img, x, y, name, ramp):
    sprite(img, x, y, ICONS[name], {"k": P["ink"], "1": ramp[1], "2": ramp[0], "3": ramp[2]})


def bone(img, x, y):
    sprite(img, x, y, BONE, {"k": P["ink"], "1": P["parch2"], "2": P["white"], "3": P["parch3"]})


def lock(img, x, y, ramp=None):
    hi, main, lo = ramp or RAMP["dim"]
    sprite(img, x, y, LOCK, {"k": P["ink"], "1": lo if ramp is None else main, "2": main, "3": lo})

# ---------------------------------------------------------------- Pal portraits (24x24, left half mirrored)


BUTCHER = mirror([
    "............",
    "........kkkk",
    "......kkSsss",
    ".....kSsssss",
    "....kSssssss",
    "....kSssssss",
    "....kSkkkkss",
    "....kSsekkss",
    "....kSssssss",
    "....kSsssssS",
    "...kkShhhhhh",
    "...khhhhSsss",
    "....khkSsRRR",
    ".....kSsssss",
    "......kSSSSS",
    "....kkkkSSSS",
    "..kkwwwwkkkk",
    ".kwwwwwwwwww",
    ".kwwrwwwwwww",
    "kwwwrrwwwwww",
    "kwwwwwwWwwww",
    "kwwwwwwWwwww",
    "kWwwwwwWwwww",
    "kkkkkkkkkkkk",
])
CLEAVER = [
    ".kkkkk.",
    "kxxxxxk",
    "kxkxxek",
    "kxxxxek",
    "kxxxxek",
    "kxxxxek",
    ".kkxkk.",
    "..kbk..",
    "..kbk..",
    "..kbk..",
    "..kbk..",
    "..kkk..",
]
GLUTTON = mirror([
    "............",
    "............",
    ".......kkkkk",
    ".....kklllll",
    "....kllloooo",
    "...klloooooo",
    "..kllooooooo",
    "..kloooooooo",
    ".klookkkkooo",
    ".klooookoooo",
    ".kloOoooooooo"[:12],
    ".kOOoooooooo",
    ".kOooooookkk",
    ".kOooooookmm",
    ".kOOoooookkk",
    "..kOOooooooo",
    "..kOOOoooooo",
    ".kkOOOOOoooo",
    "kooOOOOOOOOO",
    "koooOOOOOOOO",
    "kooooooooooo",
    "kOoooooooooo",
    "kOOooooooooo",
    "kkkkkkkkkkkk",
])
SHADE = mirror([
    "........kkkk",
    "......kkqqqq",
    ".....kqqpppp",
    "....kqpppppp",
    "...kqppppkkk",
    "...kqppkkkkk",
    "..kqppkkkkkk",
    "..kqpkkkkkkk",
    "..kqpkkEekkk",
    "..kqpkkkkkkk",
    "..kqppkkkkkk",
    "..kpppkkkkkk",
    "..kppppkkkkk",
    ".kqpppppkkkk",
    ".kqppppppppp",
    ".kqppppppppp",
    "kqpppPpppppp",
    "kqppPPpppppp",
    "kqpPPppppppp",
    "kppPpppppPpp",
    "kpPPpppppPpp",
    "kPPppppppPPp",
    "kPpppppppPPp",
    "kkkkkkkkkkkk",
])
WHISPERER = mirror([
    "........kkkk",
    "......kkllll",
    ".....kllbbbb",
    "....klbbbbbb",
    "...klbbbbkkk",
    "...kbbbbksss",
    "...kbbbkssss",
    "..kbbbbkSkkk",
    "..kbbbbksekS",
    "..kbbbbkssss",
    "..kbbbbkvvvv",
    "..kbbbbkvvvv",
    "..kbbbbkVvvv",
    "..kbbbbbkvvv",
    ".kbbbbbbbkvv",
    ".kbbbbbbbbkk",
    "kbbbBbbbbbbb",
    "kbbBBbbbbbbb",
    "kbBBbbbblbbb",
    "kbBbbbbblbbb",
    "kBBbbbbblbbb",
    "kBbbbbbbbbbb",
    "kBbbbbbbbbbb",
    "kkkkkkkkkkkk",
])
FINGER = [
    ".kk.",
    "kssk",
    "kssk",
    "kssk",
    "kSsk",
    "kSSk",
]

PORTRAIT_PAL = {
    "k": P["ink"], "s": P["skin"], "S": P["skin2"], "h": P["brown2"], "w": P["parch"], "W": P["parch2"],
    "r": P["red"], "R": P["red2"], "e": P["white"], "x": P["mist"], "b": P["brown"],
    "o": P["green2"], "O": P["green3"], "l": P["green"], "m": P["red3"],
    "q": P["purple"], "p": P["purple2"], "P": P["purple3"], "E": P["cyan"],
}
WHISPER_PAL = dict(PORTRAIT_PAL, b=P["blue2"], B=P["blue3"], l=P["blue"], v=P["parch2"], V=P["parch3"])


def portrait(img, x, y, who):
    if who == "butcher":
        sprite(img, x, y, BUTCHER, PORTRAIT_PAL)
        sprite(img, x + 17, y + 1, CLEAVER, PORTRAIT_PAL)
    elif who == "glutton":
        sprite(img, x, y, GLUTTON, PORTRAIT_PAL)
    elif who == "shade":
        sprite(img, x, y, SHADE, PORTRAIT_PAL)
    elif who == "whisperer":
        sprite(img, x, y, WHISPERER, WHISPER_PAL)
        sprite(img, x + 10, y + 9, FINGER, WHISPER_PAL)


def framed_portrait(img, d, x, y, who):
    col = PALS[who]["col"]
    d.rectangle([x - 3, y - 3, x + 26, y + 26], fill=P["ink"])
    d.rectangle([x - 2, y - 2, x + 25, y + 25], fill=PALS[who]["ramp"][2])
    d.rectangle([x - 1, y - 1, x + 24, y + 24], fill=P["night"])
    d.rectangle([x - 2, y - 2, x + 25, y - 2], fill=col)
    portrait(img, x, y, who)
    if who == "glutton":
        text(d, x + 18, y - 9, "z", "s", P["green"])
        text(d, x + 23, y - 15, "Z", "m", P["green"])

# ---------------------------------------------------------------- the talent data (same save in every take)


PALS = dict(
    butcher=dict(name="BUTCHER", col=P["red"], ramp=RAMP["red"], job="Fits augments",
                 gets="A fighter carries a 2nd augment", via="TWIN FITTING"),
    glutton=dict(name="GLUTTON", col=P["green"], ramp=RAMP["green"], job="Keeps the tree",
                 gets="Overfed this run: WARM HANDS 4/3", via="WARM HANDS"),
    shade=dict(name="SHADE", col=P["purple"], ramp=RAMP["purple"], job="Enhances buttons",
               gets="A button takes a 2nd enhancement", via="SECOND EDGE"),
    whisperer=dict(name="WHISPERER", col=P["blue"], ramp=RAMP["blue"], job="Upgrades units",
                   gets="Upgrades cost 15% less per rank", via="CHEAP LESSON"),
)

# Each branch: key, name, who it is for, icon, ramp, six talents.
# A talent: name, max rank, rank in this save, Pal link, what it does.
# Talents 0 and 1 are tier 1, 2 and 3 are tier 2, 4 and 5 are tier 3.
TREE = [
    ("command", "COMMAND", "You", "eye", "cyan", [
        ("Wide Window", 3, 3, None, "The parry window is wider."),
        ("Spare Order", 3, 2, None, "More order points per fight."),
        ("Deep Meter", 2, 1, None, "The parry meter holds a 4th segment."),
        ("Second Edge", 1, 0, "shade", "One Shade button takes a second enhancement."),
        ("Chief Calm", 1, 0, None, "A missed parry costs no meter once a fight."),
        ("Last Order", 1, 0, None, "Unleash refunds half its points."),
    ]),
    ("front", "FRONT LINE", "Tanks", "shield", "gold", [
        ("Brace Again", 3, 3, None, "Defensive cooldowns return faster."),
        ("Thick Hide", 3, 2, None, "More armour."),
        ("Shield Wall", 2, 1, None, "Tanks guard the ally behind them."),
        ("Hold Fast", 2, 0, None, "Tanks cannot be knocked back."),
        ("Unmovable", 1, 0, None, "First lethal hit leaves 1 HP."),
        ("Last Wall", 1, 0, None, "Tanks taunt when an ally drops low."),
    ]),
    ("shoot", "SHOOTERS", "Ranged", "target", "gold", [
        ("Steady Aim", 3, 2, None, "Aimed shots charge faster."),
        ("Long Draw", 3, 0, None, "Longer range."),
        ("Pin Shot", 2, 0, None, "Aimed shots slow the target."),
        ("Volley", 2, 0, None, "Every 4th shot splits in three."),
        ("Dead Eye", 1, 0, None, "Aimed shots on an opening crit."),
        ("Sun Shot", 1, 0, None, "Shots blind for 1 second."),
    ]),
    ("chest", "WAR CHEST", "The run", "chest", "gold", [
        ("Scrap Eye", 3, 2, None, "More materials after a fight."),
        ("Deep Pockets", 3, 1, None, "Carry more materials."),
        ("Cheap Lesson", 2, 1, "whisperer", "Whisperer upgrades cost less."),
        ("Salvager", 2, 0, None, "Broken augments return materials."),
        ("Lucky Streak", 1, 0, None, "The run streak survives one loss."),
        ("Trophy Wall", 1, 0, None, "Bosses drop an extra Marrow."),
    ]),
    ("heal", "HEALERS", "Support", "drop", "gold", [
        ("Warm Hands", 3, 3, "glutton", "Heals land bigger."),
        ("Clean Water", 3, 1, None, "Heals also clear a bleed."),
        ("Overflow", 2, 1, None, "Overheal becomes a shield."),
        ("Field Mend", 2, 0, None, "Heal a downed ally once a fight."),
        ("Second Wind", 1, 0, None, "Healers act twice after a parry."),
        ("Rise Again", 1, 0, None, "Revive one fighter per run."),
    ]),
    ("control", "CONTROL", "Snares", "knot", "gold", [
        ("Tight Knot", 3, 1, None, "Snares hold longer."),
        ("Sticky Sand", 3, 0, None, "Traps slow nearby enemies."),
        ("Deep Snare", 2, 0, None, "Snared enemies take more damage."),
        ("Trap Field", 2, 0, None, "Place one more trap."),
        ("Time Lock", 1, 0, None, "A snare freezes the target fully."),
        ("Long Net", 1, 0, None, "Snares chain to a second enemy."),
    ]),
    ("strike", "STRIKERS", "Melee", "sword", "gold", [
        ("Open Wound", 3, 3, None, "Strikes on an opening hit harder."),
        ("Quick Step", 3, 1, None, "Strikers close gaps faster."),
        ("Gap Finder", 2, 0, None, "Openings last longer."),
        ("Bleed Out", 2, 0, None, "Opening strikes cause a bleed."),
        ("Execution", 1, 0, None, "Kill enemies under 10% HP."),
        ("Twin Fitting", 1, 0, "butcher", "A fighter carries a second Butcher augment."),
    ]),
]
BRANCH = {b[0]: b for b in TREE}
OVERFED = ("heal", 0)          # Glutton fed Warm Hands past its max this run
SELECTED = ("command", 3)      # the talent the cursor is on (Second Edge)
COST = [1, 1, 2, 2, 3, 3]      # Marrow per rank, by slot
MARROW = 4                     # Marrow in the pocket in this save


def tier_of(i):
    return i // 2


def talent(bkey, i):
    return BRANCH[bkey][5][i]


def link_talents():
    out = []
    for b in TREE:
        for i, t in enumerate(b[5]):
            if t[3]:
                out.append((b[0], i, t[3]))
    return out

# ---------------------------------------------------------------- shared screen parts

W, H = 640, 360


def top_bar(img, d, title, sub):
    d.rectangle([0, 0, W - 1, 21], fill=P["ink"])
    d.rectangle([0, 0, W - 1, 19], fill=P["panel"])
    d.line([0, 19, W - 1, 19], fill=P["gold2"])
    d.line([0, 20, W - 1, 20], fill=P["gold3"])
    d.line([0, 0, W - 1, 0], fill=P["panel2"])
    icon(img, 5, 5, "eye", RAMP["cyan"])
    text(d, 18, 3, title, "m", P["gold"])
    text(d, 18 + tw(title, "m") + 8, 7, sub, "s", P["mist"])
    # Marrow counter
    text(d, W - 8, 7, "MARROW %d" % MARROW, "s", P["parch"], anchor="r")
    bone(img, W - 8 - tw("MARROW %d" % MARROW) - 15, 8)


LINK_TALENT = dict(butcher=("strike", 5), glutton=None, shade=("command", 3), whisperer=("chest", 2))


def link_source(who):
    return LINK_TALENT[who] or OVERFED


def is_active(st):
    return st in ("max", "owned", "over")


def status_word(st):
    if is_active(st):
        return "LINKED"
    return "CAN BUY" if st == "open" else "SEALED"


def link_line(img, pts, who, st):
    """Solid: the Pal is fed now. Dashed: buy the talent to feed it. Dotted: sealed for now."""
    col = PALS[who]["col"]
    if is_active(st):
        pline(img, pts, col, width=2)
    elif st == "open":
        pline(img, pts, col, width=2, dash=(4, 2))
    else:
        pline(img, pts, col, width=1, dash=(2, 3))


def pal_box(img, d, x0, y0, x1, y1, who, st, flip=False):
    pal = PALS[who]
    active = is_active(st)
    panel(d, x0, y0, x1, y1, fill=P["deep"], edge=pal["ramp"][2])
    px = x1 - 30 if flip else x0 + 7
    framed_portrait(img, d, px, y0 + 8, who)
    tx = x0 + 7 if flip else x0 + 37
    text(d, tx, y0 + 6, pal["name"], "m", pal["col"])
    text(d, tx, y0 + 20, pal["job"], "s", P["mist"])
    text(d, tx, y0 + 29, status_word(st), "s", pal["col"] if st != "locked" else P["locked2"])
    text(d, x0 + 7, y0 + 41, "FROM " + pal["via"], "s", pal["col"] if active else P["parch2"])
    return para(d, x0 + 7, y0 + 51, pal["gets"], x1 - x0 - 14, P["parch"] if active else P["mist"])


def pal_card(img, d, x0, y0, x1, y1, who, st, compact=False):
    """Pal box used by the board and the star chart: name and status on top, link below."""
    pal = PALS[who]
    active = is_active(st)
    panel(d, x0, y0, x1, y1, fill=P["deep"], edge=pal["ramp"][2])
    framed_portrait(img, d, x0 + 8, y0 + 8, who)
    text(d, x0 + 39, y0 + 5, pal["name"], "m", pal["col"])
    text(d, x1 - 6, y0 + 7, status_word(st), "s", pal["col"] if st != "locked" else P["locked2"], anchor="r")
    y = y0 + 19
    if not compact:
        text(d, x0 + 39, y, pal["job"], "s", P["mist"])
        y += 10
    text(d, x0 + 39, y, "FROM " + pal["via"], "s", pal["col"] if active else P["parch2"])
    return para(d, x0 + 39, y + 9, pal["gets"], x1 - x0 - 45, P["parch"] if active else P["mist"])


def strip(img, d, y0, h, heading, panels):
    """Bottom strip: how the player moves through the tree, in three steps."""
    d.rectangle([0, y0, W - 1, y0 + h - 1], fill=P["night"])
    text(d, 8, y0 + 4, heading, "m", P["gold"])
    pw = 200
    for k, (title, lines, mini) in enumerate(panels):
        x0 = 8 + k * (pw + 16)
        py = y0 + 21
        panel(d, x0, py, x0 + pw, py + h - 26, fill=P["deep"], edge=P["edge"], rivets=False)
        mini(img, d, x0 + 6, py + 5, k)
        tx = x0 + 84
        text(d, tx, py + 6, title, "s", P["gold"])
        y = py + 17
        for ln in lines:
            y = para(d, tx, y, ln, pw - 88, P["parch"])
            y += 2
        if k < 2:
            ax = x0 + pw + 6
            ay = py + (h - 26) // 2
            sprite(img, ax, ay - 4, CHEVRON, {"k": P["gold3"], "1": P["gold"]})


def header(img, d, num, title, idea):
    d.rectangle([0, 0, W - 1, 33], fill=P["night"])
    d.rectangle([4, 4, 29, 29], fill=P["ink"])
    d.rectangle([5, 5, 28, 28], fill=P["gold"])
    d.rectangle([7, 7, 26, 26], fill=P["gold2"])
    text(d, 17, 9, str(num), "xl", P["ink"], sh=None, anchor="c")
    text(d, 37, 4, title, "l", P["white"])
    text(d, 37, 21, idea, "s", P["parch2"])


def compose(num, title, idea, screen, strip_fn):
    img = Image.new("RGB", (W, 34 + H + 118), P["night"])
    d = ImageDraw.Draw(img)
    header(img, d, num, title, idea)
    img.paste(screen, (0, 34))
    d.rectangle([0, 34 + H, W - 1, 34 + H], fill=P["gold3"])
    strip_fn(img, d, 34 + H + 1, 117)
    return img

# ---------------------------------------------------------------- node drawing (takes 1 and 3 share the state logic)


def rank_state(bkey, i, open_rule):
    t = talent(bkey, i)
    if (bkey, i) == OVERFED:
        return "over"
    if t[2] >= t[1]:
        return "max"
    if t[2] > 0:
        return "owned"
    return "open" if open_rule(bkey, i) else "locked"


STAMP = (P["parch"], P["brown2"], P["brown3"])

STATE_LOOK = dict(
    max=(P["white"], P["gold2"], STAMP),
    owned=(P["gold"], P["deep"], RAMP["gold"]),
    open=(P["parch2"], P["ink"], RAMP["open"]),
    locked=(P["locked"], P["ink"], RAMP["dim"]),
    over=(P["green"], P["green3"], RAMP["green"]),
)


def pips(img, cx, y, bkey, i, horizontal=True):
    t = talent(bkey, i)
    n = t[1] + (1 if (bkey, i) == OVERFED else 0)
    x = cx - (n * 3 - 1) // 2
    for k in range(n):
        if k >= t[1]:
            col = P["green"]
        elif k < t[2]:
            col = P["gold"]
        else:
            col = P["locked"]
        for dx in range(2):
            for dy in range(2):
                plot(img, x + k * 3 + dx, y + dy, col)


def round_node(img, d, cx, cy, bkey, i, state, ic):
    border, fill, ramp = STATE_LOOK[state]
    if bkey == "command" and state == "owned":
        border, ramp = P["cyan"], RAMP["cyan"]
    if bkey == "command" and state == "max":
        border, fill = P["white"], P["cyan2"]
    disc(d, cx, cy, 8, P["ink"])
    disc(d, cx, cy, 7, border)
    disc(d, cx, cy, 6, P["ink"])
    disc(d, cx, cy, 5, fill)
    if state == "locked" and tier_of(i) == 2:
        lock(img, cx - 3, cy - 4)
    else:
        icon(img, cx - 4, cy - 4, ic, ramp)
    if state == "over":
        ring(img, cx, cy, 10, P["green"], dash=(2, 2))

# ================================================================ TAKE 1: PARRY WHEEL


T1_C = (320, 191)
T1_R = [50, 84, 118]
T1_ORDER = ["command", "front", "shoot", "chest", "heal", "control", "strike"]


def t1_angle(k):
    return math.radians(-90 + k * 360 / 7)


def t1_open(bkey, i):
    return tier_of(i) <= 1      # rings 1 and 2 are open in Act 2


def t1_node_pos(k, i, c=T1_C, rings=T1_R, sep=9):
    r = rings[tier_of(i)]
    a = t1_angle(k) + (-1 if i % 2 == 0 else 1) * math.asin(sep / r)
    return c[0] + r * math.cos(a), c[1] + r * math.sin(a)


def take1_screen():
    img = Image.new("RGB", (W, H))
    yy, xx = np.mgrid[0:H, 0:W]
    dist = np.hypot(xx - T1_C[0], (yy - T1_C[1]) * 1.2) / 330
    img.paste(dither(1 - dist, [P["ink"], P["night"], P["deep"], P["panel"]]), (0, 0))
    d = ImageDraw.Draw(img)
    cx, cy = T1_C

    # carved drum
    disc(d, cx, cy, 143, P["ink"])
    disc(d, cx, cy, 142, P["gold3"])
    disc(d, cx, cy, 141, P["gold2"])
    disc(d, cx, cy, 140, P["ink"])
    disc(d, cx, cy, 139, P["deep"])
    ring(img, cx, cy, 136, P["cyan3"], dash=(6, 3))
    for k in range(28):
        a = math.radians(k * 360 / 28)
        plot(img, cx + 141 * math.cos(a), cy + 141 * math.sin(a), P["gold"])
    # ring tracks
    ring(img, cx, cy, T1_R[0], P["edge"])
    ring(img, cx, cy, T1_R[1], P["edge"])
    ring(img, cx, cy, T1_R[2], P["locked"], dash=(2, 3))

    # spokes and twigs
    for k, bkey in enumerate(T1_ORDER):
        a = t1_angle(k)
        tal = BRANCH[bkey][5]
        for tier in range(3):
            r0 = 29 if tier == 0 else T1_R[tier - 1]
            r1 = T1_R[tier]
            got = any(tal[j][2] > 0 for j in (2 * tier, 2 * tier + 1))
            col = (P["cyan"] if bkey == "command" else P["gold"]) if got else (P["edge"] if tier < 2 else P["locked"])
            p0 = (cx + r0 * math.cos(a), cy + r0 * math.sin(a))
            p1 = (cx + r1 * math.cos(a), cy + r1 * math.sin(a))
            pline(img, [p0, p1], col, width=2 if got else 1, dash=None if tier < 2 else (2, 2))
            for j in (2 * tier, 2 * tier + 1):
                nx, ny = t1_node_pos(k, j)
                pline(img, [p1, (nx, ny)], col)

    # Pal links (drawn under the nodes)
    def at(bkey, i):
        x, y = t1_node_pos(T1_ORDER.index(bkey), i)
        return int(round(x)), int(round(y))

    tf, se, cl, wh = at("strike", 5), at("command", 3), at("chest", 2), at(*OVERFED)
    links = dict(
        butcher=[tf, (tf[0], 64), (134, 64)],
        shade=[se, (352, 84), (352, 56), (506, 56)],
        whisperer=[cl, (420, 290), (506, 290)],
        glutton=[wh, (302, 318), (134, 318)],
    )
    for who, pts in links.items():
        st = rank_state(*link_source(who), t1_open)
        link_line(img, pts, who, st)
        ex, ey = pts[-1]
        sprite(img, ex - 2, ey - 2, ["..k..", ".k1k.", "k111k", ".k1k.", "..k.."],
               {"k": P["ink"], "1": PALS[who]["col"]})

    # ring 3 seal in the bottom gap
    tx, ty = cx, cy + T1_R[2]
    d.rectangle([tx - 6, ty - 6, tx + 6, ty + 6], fill=P["ink"])
    d.rectangle([tx - 5, ty - 5, tx + 5, ty + 5], fill=P["deep"])
    lock(img, tx - 3, ty - 4, RAMP["gold"])

    # nodes
    for k, bkey in enumerate(T1_ORDER):
        b = BRANCH[bkey]
        for i in range(6):
            nx, ny = at(bkey, i)
            st = rank_state(bkey, i, t1_open)
            t = talent(bkey, i)
            if t[3] and t[3] != "glutton":
                ring(img, nx, ny, 10, PALS[t[3]]["col"], dash=(2, 1))
            round_node(img, d, nx, ny, bkey, i, st, b[3])
            pips(img, nx, ny + 10, bkey, i)
        # name plaque nailed to the drum rim
        a = t1_angle(k)
        r = 157 if k == 0 else 162
        px, py = int(round(cx + r * math.cos(a))), int(round(cy + r * math.sin(a)))
        name, role = b[1], b[2]
        ramp = RAMP["cyan"] if bkey == "command" else RAMP["gold"]
        w = max(tw(name, "m"), tw(role)) + 10
        x0, y0 = px - w // 2, py - 12
        d.rectangle([x0, y0, x0 + w, y0 + 24], fill=P["ink"])
        d.rectangle([x0 + 1, y0 + 1, x0 + w - 1, y0 + 23], fill=P["panel"])
        d.line([x0 + 1, y0 + 1, x0 + w - 1, y0 + 1], fill=ramp[1])
        text(d, px, y0 + 2, name, "m", ramp[1], anchor="c")
        text(d, px, y0 + 14, role, "s", P["mist"], anchor="c", sh=None)

    # centre: the parry meter
    disc(d, cx, cy, 29, P["ink"])
    disc(d, cx, cy, 28, P["cyan3"])
    disc(d, cx, cy, 27, P["ink"])
    for s in range(3):
        a0 = -90 + s * 120 + 5
        a1 = -90 + (s + 1) * 120 - 5
        col = P["cyan"] if s < 2 else P["cyan3"]
        d.pieslice([cx - 26, cy - 26, cx + 26, cy + 26], a0, a1, fill=col)
        if s < 2:
            d.pieslice([cx - 26, cy - 26, cx + 26, cy + 26], a0, a0 + 18, fill=P["white"])
    disc(d, cx, cy, 19, P["ink"])
    disc(d, cx, cy, 18, P["night"])
    text(d, cx, cy - 11, "PARRY", "s", P["cyan"], anchor="c")
    text(d, cx, cy - 2, "2/3", "m", P["white"], anchor="c")

    # selection cursor on Second Edge
    cursor(img, se[0], se[1], 11)

    top_bar(img, d, "TALENTS", "ACT 2   MISSION 9   GLUTTON'S TENT")

    # left column: Butcher, key, Glutton
    pal_box(img, d, 4, 26, 132, 100, "butcher", rank_state(*link_source("butcher"), t1_open))
    legend(img, d, 4, 106, 132, 236)
    y = pal_box(img, d, 4, 242, 132, 356, "glutton", rank_state(*OVERFED, t1_open))
    para(d, 11, y + 4, "Wake him to spend or reset. He hates it. Reset is free.", 114, P["mist"])
    # right column: Shade, tooltip, Whisperer
    pal_box(img, d, 508, 26, 636, 100, "shade", rank_state(*link_source("shade"), t1_open), flip=True)
    tooltip(img, d, 508, 106, 636, 244, "command", 3, "Ring 2. Open now.")
    pal_box(img, d, 508, 254, 636, 340, "whisperer", rank_state(*link_source("whisperer"), t1_open), flip=True)
    return img


def cursor(img, x, y, r):
    for sx in (-1, 1):
        for sy in (-1, 1):
            for k in range(4):
                plot(img, x + sx * r - sx * k, y + sy * r, P["white"])
                plot(img, x + sx * r, y + sy * r - sy * k, P["white"])


def legend(img, d, x0, y0, x1, y1):
    panel(d, x0, y0, x1, y1, fill=P["deep"], edge=P["edge"], rivets=False)
    text(d, x0 + 7, y0 + 6, "KEY", "m", P["gold"])
    rows = [
        ("max", "Maxed"),
        ("owned", "Bought, more ranks"),
        ("open", "Can buy now"),
        ("locked", "Sealed until Act 3"),
        ("over", "Overfed by Glutton"),
    ]
    dd = ImageDraw.Draw(img)
    y = y0 + 24
    for st, label in rows:
        border, fill, ramp = STATE_LOOK[st]
        disc(dd, x0 + 14, y + 3, 6, P["ink"])
        disc(dd, x0 + 14, y + 3, 5, border)
        disc(dd, x0 + 14, y + 3, 3, fill)
        if st == "over":
            ring(img, x0 + 14, y + 3, 8, P["green"], dash=(2, 2))
        text(d, x0 + 27, y, label, "s", P["parch"])
        y += 14
    y += 1
    for st, label in (("max", "Pal fed now"), ("open", "Buy it to feed"), ("locked", "Link still sealed")):
        link_line(img, [(x0 + 7, y + 3), (x0 + 22, y + 3)], "butcher", st)
        text(d, x0 + 27, y, label, "s", P["parch"])
        y += 10


def tooltip(img, d, x0, y0, x1, y1, bkey, i, where):
    t = talent(bkey, i)
    b = BRANCH[bkey]
    panel(d, x0, y0, x1, y1, fill=P["panel"], edge=P["gold2"], tech=True)
    icon(img, x0 + 7, y0 + 7, b[3], RAMP["cyan"] if bkey == "command" else RAMP["gold"])
    text(d, x0 + 20, y0 + 5, t[0].upper(), "m", P["white"])
    text(d, x0 + 7, y0 + 21, "%s  TIER %d" % (b[1], tier_of(i) + 1), "s", P["mist"])
    text(d, x0 + 7, y0 + 32, "RANK %d/%d" % (t[2], t[1]), "s", P["parch"])
    text(d, x1 - 20, y0 + 32, "%d" % COST[i], "s", P["parch"], anchor="r")
    bone(img, x1 - 18, y0 + 33)
    y = para(d, x0 + 7, y0 + 46, t[4], x1 - x0 - 14, P["parch"])
    if t[3]:
        y += 3
        text(d, x0 + 7, y, "FEEDS " + PALS[t[3]]["name"], "s", PALS[t[3]]["col"])
        y += 10
    text(d, x0 + 7, y, where, "s", P["mist"])
    if t[3] == "shade":
        # what it changes: one button, two enhancement sockets
        y += 14
        bx, by = x0 + 7, y
        d.rectangle([bx, by, bx + 30, by + 14], fill=P["ink"])
        d.rectangle([bx + 1, by + 1, bx + 29, by + 13], fill=P["panel2"])
        d.line([bx + 1, by + 1, bx + 29, by + 1], fill=P["parch2"])
        text(d, bx + 15, by + 3, "HOLD", "s", P["parch"], anchor="c")
        for k in range(2):
            sx = bx + 40 + k * 16
            gem = ["...k...", "..k1k..", ".k121k.", "k12111k", ".k111k.", "..k1k..", "...k..."]
            if k == 0:
                sprite(img, sx, by + 3, gem, {"k": P["ink"], "1": P["purple"], "2": P["white"]})
            else:
                sprite(img, sx, by + 3, ["...1...", "..1.1..", ".1...1.", "1..2..1", ".1...1.", "..1.1..", "...1..."],
                       {"1": P["purple2"], "2": P["purple"]})
        text(d, bx + 74, by + 3, "+1 SLOT", "s", P["purple"])
    # buy button
    bx, by = x0 + 7, y1 - 20
    d.rectangle([bx, by, bx + 52, by + 12], fill=P["ink"])
    d.rectangle([bx + 1, by + 1, bx + 51, by + 11], fill=P["gold2"])
    text(d, bx + 26, by + 3, "HOLD: BUY", "s", P["ink"], sh=None, anchor="c")


def mini_wheel(img, d, x0, y0, stage):
    cx, cy = x0 + 36, y0 + 37
    rings = [11, 21, 31]
    disc(d, cx, cy, 35, P["ink"])
    disc(d, cx, cy, 34, P["night"])
    for r_i, r in enumerate(rings):
        open_ring = r_i <= stage
        ring(img, cx, cy, r, P["edge"] if open_ring else P["locked"], dash=None if open_ring else (1, 2))
    disc(d, cx, cy, 5, P["cyan3"] if stage == 0 else P["cyan"])
    for k in range(7):
        a = t1_angle(k)
        for r_i, r in enumerate(rings):
            for side in (-1, 1):
                aa = a + side * math.asin(3 / r)
                x, y = int(round(cx + r * math.cos(aa))), int(round(cy + r * math.sin(aa)))
                if r_i < stage:
                    col = P["gold"]
                elif r_i == stage:
                    col = P["parch2"] if (k + side) % 3 else P["gold"]
                else:
                    col = P["locked"]
                d.rectangle([x - 1, y - 1, x + 1, y + 1], fill=col)
    # Pal link dots light up with the rings
    for who, (k, r_i) in dict(shade=(0, 1), whisperer=(3, 1), butcher=(6, 2)).items():
        if r_i <= stage:
            a = t1_angle(k)
            x, y = cx + rings[r_i] * math.cos(a), cy + rings[r_i] * math.sin(a)
            ex, ey = (x0 + 70 if x > cx else x0 + 2), y
            pline(img, [(x, y), (ex, ey)], PALS[who]["col"], dash=(2, 1))


def take1_strip(img, d, y0, h):
    strip(img, d, y0, h, "HOW YOU MOVE THROUGH IT", [
        ("START OF THE CAMPAIGN", ["Ring 1 is open on all 7 spokes.",
                                   "Spend Marrow at camp after each mission."], mini_wheel),
        ("AFTER ACT 1", ["Ring 2 opens on every spoke.",
                         "Shade and Whisperer links sit here."], mini_wheel),
        ("AFTER ACT 2", ["Ring 3 opens: one big talent per spoke.",
                         "Butcher link sits on the outer ring."], mini_wheel),
    ])


def take1():
    return compose(1, "PARRY WHEEL",
                   "Radial tree. The parry meter is the hub. Rings open act by act, on every spoke at once.",
                   take1_screen(), take1_strip)

# ================================================================ TAKE 2: WAR BOARD


T2_ORDER = ["front", "strike", "shoot", "control", "heal", "command", "chest"]
T2_X0, T2_CW, T2_CARD = 10, 89, 83
T2_ROWS = [52, 78, 117, 143, 182, 208]
T2_CH = 24


def t2_open(bkey, i):
    tier = tier_of(i)
    if tier == 0:
        return True
    if tier == 2:
        return False                      # sealed until Act 3
    spent = sum(talent(bkey, j)[2] for j in (0, 1))
    return spent >= 3


def t2_need(bkey):
    return max(0, 3 - sum(talent(bkey, j)[2] for j in (0, 1)))


def card(img, d, x, y, bkey, i):
    b = BRANCH[bkey]
    t = talent(bkey, i)
    st = rank_state(bkey, i, t2_open)
    w, h = T2_CARD, T2_CH
    if st == "locked":
        d.rectangle([x, y, x + w - 1, y + h - 1], fill=P["ink"])
        d.rectangle([x + 1, y + 1, x + w - 2, y + h - 2], fill=P["red3"])
        for k in range(0, w + h, 6):
            pline(img, [(x + 2 + k, y + 2), (x + 2 + k - (h - 5), y + h - 3)], P["brown3"])
        d.rectangle([x + 3, y + 3, x + w - 4, y + h - 4], outline=P["red2"])
        if tier_of(i) == 1:
            need = t2_need(bkey)
            lock(img, x + 6, y + 8, RAMP["open"])
            text(d, x + 16, y + 4, t[0], "s", P["parch3"], outline=True)
            text(d, x + 16, y + 13, "%d MORE ABOVE" % need, "s", P["parch2"], outline=True)
        return
    if st in ("max", "owned", "over"):
        face = P["parch"] if st != "over" else (200, 222, 150)
        d.rectangle([x, y, x + w - 1, y + h - 1], fill=P["ink"])
        d.rectangle([x + 1, y + 1, x + w - 2, y + h - 2], fill=P["gold"] if st == "max" else P["parch3"])
        d.rectangle([x + 2, y + 2, x + w - 3, y + h - 3], fill=face)
        d.line([x + 2, y + h - 3, x + w - 3, y + h - 3], fill=P["parch2"] if st != "over" else P["green2"])
        ink = P["ink"]
        ramp = RAMP["cyan"] if bkey == "command" else (RAMP["green"] if st == "over" else RAMP["gold"])
        if st == "over":
            d.rectangle([x + 1, y + 1, x + w - 2, y + h - 2], outline=P["green"])
        icon(img, x + 4, y + 4, b[3], ramp)
        text(d, x + 16, y + 3, t[0], "s", ink, sh=None)
        if st == "max":
            sprite(img, x + w - 9, y + 3, ["..k..", ".k1k.", "k111k", ".k1k.", "..k.."],
                   {"k": P["gold3"], "1": P["gold"]})
    else:  # open
        d.rectangle([x, y, x + w - 1, y + h - 1], fill=P["ink"])
        d.rectangle([x + 1, y + 1, x + w - 2, y + h - 2], fill=P["gold2"])
        d.rectangle([x + 2, y + 2, x + w - 3, y + h - 3], fill=P["panel"])
        icon(img, x + 4, y + 4, b[3], RAMP["open"])
        text(d, x + 16, y + 3, t[0], "s", P["parch"])
    # rank boxes: bought ranks are solid, empty ones are faint slots
    n = t[1] + (1 if (bkey, i) == OVERFED else 0)
    for k in range(n):
        bx = x + 16 + k * 7
        by = y + 14
        if k < t[2] or k >= t[1]:
            fill = P["green"] if k >= t[1] else (P["cyan2"] if bkey == "command" else P["gold2"])
            d.rectangle([bx, by, bx + 5, by + 4], fill=P["ink"])
            d.rectangle([bx + 1, by + 1, bx + 4, by + 3], fill=fill)
            plot(img, bx + 1, by + 1, P["white"])
        elif st == "open":
            d.rectangle([bx, by, bx + 5, by + 4], outline=P["locked2"])
        else:
            d.rectangle([bx, by, bx + 5, by + 4], outline=P["parch3"])
    if st != "max":
        bone(img, x + w - 15, y + 15)
        text(d, x + w - 17, y + 13, str(COST[i]), "s", P["ink"] if st != "open" else P["parch"],
             sh=None if st != "open" else P["ink"], anchor="r")


def pal_effect(img, d, x, y, who, active):
    """A tiny picture of what the link changes in that Pal's system."""
    col = PALS[who]["col"]
    dim = PALS[who]["ramp"][2]
    gem = ["...k...", "..k1k..", ".k121k.", "k12111k", ".k111k.", "..k1k..", "...k..."]
    empty = ["...1...", "..1.1..", ".1...1.", "1..2..1", ".1...1.", "..1.1..", "...1..."]
    d.rectangle([x - 2, y - 3, x + 137, y + 12], fill=P["night"])
    if who in ("butcher", "shade"):
        if who == "butcher":
            icon(img, x, y, "sword", RAMP["open"])
            label = "WEAPON"
        else:
            d.rectangle([x, y, x + 9, y + 8], fill=P["ink"])
            d.rectangle([x + 1, y + 1, x + 8, y + 7], fill=P["panel2"])
            label = "BUTTON"
        text(d, x + 13, y + 1, label, "s", P["mist"])
        sprite(img, x + 50, y + 1, gem, {"k": P["ink"], "1": col, "2": P["white"]})
        sprite(img, x + 62, y + 1, empty, {"1": col if active else dim, "2": col if active else dim})
        text(d, x + 74, y + 1, "+1 SLOT", "s", col if active else dim)
    elif who == "glutton":
        for k in range(4):
            fill = P["gold2"] if k < 3 else P["green"]
            d.rectangle([x + k * 8, y + 1, x + k * 8 + 6, y + 6], fill=P["ink"])
            d.rectangle([x + k * 8 + 1, y + 2, x + k * 8 + 5, y + 5], fill=fill)
        text(d, x + 36, y + 1, "ONE PAST MAX", "s", col)
    else:
        text(d, x, y + 1, "UPGRADE", "s", P["mist"])
        w = text(d, x + 42, y + 1, "40", "s", P["mist"])
        d.line([x + 41, y + 4, x + 42 + w, y + 4], fill=P["red"])
        sprite(img, x + 56, y, CHEVRON[2:7], {"k": P["ink"], "1": col})
        text(d, x + 64, y + 1, "34", "s", col)
        bone(img, x + 76, y + 2)


def take2_screen():
    img = Image.new("RGB", (W, H))
    yy, xx = np.mgrid[0:H, 0:W]
    grain = (np.sin(xx * 0.09 + np.sin(yy * 0.31) * 2.0) * 0.5 + 0.5) * 0.35
    plank = ((yy % 22) < 1).astype(float)
    t = 0.45 + grain - plank * 0.6 - np.hypot(xx - 320, yy - 180) / 900
    img.paste(dither(t, [P["ink"], P["brown3"], P["brown2"], P["brown"]]), (0, 0))
    d = ImageDraw.Draw(img)
    rnd = random.Random(7)
    for y in range(22, H, 22):
        for x in range(rnd.randint(0, 40), W, 97):
            plot(img, x, y + 3, P["parch3"])

    # the board itself
    panel(d, 4, 24, W - 5, 236, fill=P["brown3"], edge=P["gold3"])
    for k, bkey in enumerate(T2_ORDER):
        b = BRANCH[bkey]
        x = T2_X0 + k * T2_CW
        spent = sum(t[2] for t in b[5])
        total = sum(t[1] for t in b[5])
        ramp = RAMP["cyan"] if bkey == "command" else RAMP["gold"]
        d.rectangle([x, 28, x + T2_CARD - 1, 48], fill=P["ink"])
        d.rectangle([x + 1, 29, x + T2_CARD - 2, 47], fill=P["panel2"])
        d.line([x + 1, 29, x + T2_CARD - 2, 29], fill=ramp[1])
        icon(img, x + 3, 32, b[3], ramp)
        text(d, x + 14, 30, b[1] if len(b[1]) < 10 else b[1].split()[0] + " " + b[1].split()[1][0:4],
             "s", ramp[1])
        text(d, x + 14, 39, b[2] if len(b[2]) < 12 else b[2][:11], "s", P["mist"])
        text(d, x + T2_CARD - 4, 39, "%d/%d" % (spent, total), "s", P["parch"], anchor="r")

    # tier bars
    for y, label, col in ((104, "TIER 2   A COLUMN FLIPS WHEN 3 RANKS ARE BOUGHT ABOVE IT", P["parch2"]),
                          (169, "TIER 3   SEALED UNTIL ACT 3", P["red"])):
        d.rectangle([8, y, W - 9, y + 10], fill=P["ink"])
        d.line([8, y + 5, W - 9, y + 5], fill=P["gold3"])
        lw = tw(label) + 10
        d.rectangle([320 - lw // 2, y, 320 + lw // 2, y + 10], fill=P["ink"])
        text(d, 320, y + 1, label, "s", col, anchor="c")

    for k, bkey in enumerate(T2_ORDER):
        x = T2_X0 + k * T2_CW
        for i in range(6):
            card(img, d, x, T2_ROWS[i], bkey, i)

    # wax seal over tier 3
    sx, sy = 320, 207
    disc(d, sx, sy, 10, P["ink"])
    disc(d, sx, sy, 9, P["red2"])
    disc(d, sx, sy, 7, P["red"])
    lock(img, sx - 3, sy - 4, RAMP["open"])
    for side in (-1, 1):
        d.rectangle([sx + side * 12 - (0 if side > 0 else 150), sy - 2,
                     sx + side * 12 + (150 if side > 0 else 0), sy + 2], fill=P["ink"])
        d.rectangle([sx + side * 12 - (0 if side > 0 else 150), sy - 1,
                     sx + side * 12 + (150 if side > 0 else 0), sy + 1], fill=P["brown"])
    disc(d, sx, sy, 10, P["ink"])
    disc(d, sx, sy, 9, P["red2"])
    disc(d, sx, sy, 7, P["red"])
    lock(img, sx - 3, sy - 4, RAMP["open"])

    # Pal stations
    stations = [("butcher", 4), ("glutton", 162), ("shade", 324), ("whisperer", 482)]
    sw = 154
    for who, x in stations:
        st = rank_state(*link_source(who), t2_open)
        pal_card(img, d, x, 262, x + sw, 356, who, st)
        pal_effect(img, d, x + 8, 337, who, is_active(st))

    # routes from cards to stations
    def cx_(k):
        return T2_X0 + k * T2_CW

    routes = {
        "butcher": [(cx_(1) + 41, T2_ROWS[5] + T2_CH), (cx_(1) + 41, 244), (80, 244), (80, 262)],
        "glutton": [(cx_(4) + T2_CARD, T2_ROWS[0] + 12), (cx_(4) + T2_CARD + 3, T2_ROWS[0] + 12),
                    (cx_(4) + T2_CARD + 3, 248), (240, 248), (240, 262)],
        "shade": [(cx_(5) + T2_CARD, T2_ROWS[3] + 12), (cx_(5) + T2_CARD + 3, T2_ROWS[3] + 12),
                  (cx_(5) + T2_CARD + 3, 252), (400, 252), (400, 262)],
        "whisperer": [(cx_(6) + T2_CARD, T2_ROWS[2] + 12), (W - 7, T2_ROWS[2] + 12),
                      (W - 7, 256), (560, 256), (560, 262)],
    }
    for who, pts in routes.items():
        st = rank_state(*link_source(who), t2_open)
        col = PALS[who]["col"]
        link_line(img, pts, who, st)
        ex, ey = pts[-1]
        sprite(img, ex - 2, ey - 4, ["k...k", "k1.1k", ".k1k.", "..k.."], {"k": P["ink"], "1": col})
        # mark the card edge
        sx, sy = pts[0]
        d.rectangle([sx - 1, sy - 1, sx + 1, sy + 1], fill=col)

    # cursor on Second Edge
    x = cx_(5)
    y = T2_ROWS[3]
    for (px, py, dx, dy) in ((x - 2, y - 2, 1, 1), (x + T2_CARD + 1, y - 2, -1, 1),
                             (x - 2, y + T2_CH + 1, 1, -1), (x + T2_CARD + 1, y + T2_CH + 1, -1, -1)):
        for k in range(4):
            plot(img, px + dx * k, py, P["white"])
            plot(img, px, py + dy * k, P["white"])

    top_bar(img, d, "TALENTS", "ACT 2   MISSION 9   THE WAR BOARD")
    bx = W - 8 - tw("MARROW 4") - 26 - 96
    d.rectangle([bx, 3, bx + 92, 15], fill=P["ink"])
    d.rectangle([bx + 1, 4, bx + 91, 14], fill=P["green3"])
    text(d, bx + 46, 6, "RESET: WAKE HIM", "s", P["green"], sh=None, anchor="c")
    # small tooltip strip under the top bar? keep the board clean; the strip below explains
    return img


def mini_board(img, d, x0, y0, stage):
    d.rectangle([x0, y0, x0 + 71, y0 + 73], fill=P["brown3"])
    d.rectangle([x0, y0, x0 + 71, y0 + 73], outline=P["gold3"])
    # how many ranks each column has above tier 2 at this stage
    above = [[1, 1, 0, 0, 1, 2, 1], [3, 4, 2, 1, 4, 5, 3], [5, 4, 2, 1, 4, 5, 3]][stage]
    for c in range(7):
        x = x0 + 3 + c * 10
        for r in range(6):
            y = y0 + 4 + r * 11 + (3 if r >= 2 else 0) + (3 if r >= 4 else 0)
            tier = r // 2
            if tier == 0:
                col = P["parch"] if (c + r + stage) % 3 else P["panel"]
                if stage == 0:
                    col = P["panel"] if (c + r) % 2 else P["parch"]
            elif tier == 1:
                col = P["panel"] if above[c] >= 3 else P["red3"]
                if above[c] >= 3 and stage >= 1 and (c + r) % 2 == 0:
                    col = P["parch"]
            else:
                col = P["red3"] if stage < 2 else P["panel"]
            d.rectangle([x, y, x + 8, y + 8], fill=P["ink"])
            d.rectangle([x + 1, y + 1, x + 7, y + 7], fill=col)
            if col == P["panel"]:
                d.rectangle([x + 1, y + 1, x + 7, y + 7], outline=P["gold2"])
    if stage < 2:
        disc(d, x0 + 36, y0 + 59, 4, P["red"])


def take2_strip(img, d, y0, h):
    strip(img, d, y0, h, "HOW YOU MOVE THROUGH IT", [
        ("START OF THE CAMPAIGN", ["Tier 1 cards are face up in all 7 columns.",
                                   "Buy ranks with Marrow after missions."], mini_board),
        ("3 RANKS IN ONE COLUMN", ["That column's tier 2 cards flip over.",
                                   "Each role deepens on its own."], mini_board),
        ("AFTER ACT 2", ["The wax seal breaks: tier 3 opens.",
                         "Butcher link card waits here."], mini_board),
    ])


def take2():
    return compose(2, "WAR BOARD",
                   "Iron Marines style. Cards by role. A column deepens when you spend in it.",
                   take2_screen(), take2_strip)

# ================================================================ TAKE 3: STAR CHART


T3_ORDER = ["strike", "front", "heal", "command", "control", "shoot", "chest"]
T3_CX = dict(strike=70, front=150, heal=232, command=320, control=408, shoot=490, chest=570)
# (x offset from the constellation's centre, y) for slots 0..5. Low stars are tier 1, high ones tier 3.
T3_SHAPE = dict(
    strike=[(0, 272), (-14, 248), (12, 208), (-4, 174), (10, 96), (-6, 62)],
    front=[(-8, 268), (12, 250), (-14, 204), (10, 180), (-10, 98), (12, 70)],
    heal=[(6, 270), (-12, 244), (14, 210), (-8, 176), (8, 94), (-10, 66)],
    command=[(-12, 266), (12, 266), (-16, 198), (16, 178), (-8, 90), (8, 66)],
    control=[(8, 272), (-10, 250), (12, 210), (-14, 182), (6, 98), (-8, 64)],
    shoot=[(-6, 270), (14, 244), (-12, 206), (12, 174), (-10, 96), (10, 68)],
    chest=[(8, 268), (-12, 250), (12, 204), (-10, 178), (12, 96), (-8, 66)],
)
T3_EDGES = [(0, 1), (0, 2), (1, 3), (2, 3), (2, 4), (3, 5), (4, 5)]
T3_BRIDGES = [(("strike", 2), ("front", 2)), (("heal", 2), ("command", 2)), (("control", 2), ("shoot", 2))]
T3_CLOUD = 112          # cloud base in Act 2: everything above is uncharted
T3_HORIZON = 288


def t3_pos(bkey, i):
    dx, y = T3_SHAPE[bkey][i]
    return T3_CX[bkey] + dx, y


def t3_charted(bkey, i):
    return t3_pos(bkey, i)[1] > T3_CLOUD


def t3_lit(node):
    return talent(*node)[2] > 0


def t3_neighbours(bkey, i):
    out = [(bkey, b if a == i else a) for a, b in T3_EDGES if i in (a, b)]
    for p, q in T3_BRIDGES:
        if p == (bkey, i):
            out.append(q)
        if q == (bkey, i):
            out.append(p)
    return out


def t3_open(bkey, i):
    if not t3_charted(bkey, i):
        return False
    if i == 0:
        return True                       # the first star rises from the horizon
    return any(t3_lit(nb) for nb in t3_neighbours(bkey, i))


def half(col):
    return tuple(v // 2 + 8 for v in col)


def star(img, x, y, arm, col, core=P["white"]):
    """Four point sparkle. arm 1..2 is a dim speck, 3+ gets a white core and a soft glow."""
    for k in range(1, arm + 1):
        if arm >= 3 and k == 1:
            c = core
        elif arm >= 3 and k == arm:
            c = half(col)
        else:
            c = col
        for dx, dy in ((k, 0), (-k, 0), (0, k), (0, -k)):
            plot(img, x + dx, y + dy, c)
    if arm >= 3:
        for dx, dy in ((1, 1), (-1, -1), (1, -1), (-1, 1)):
            plot(img, x + dx, y + dy, col)
    if arm >= 5:
        for dx, dy in ((2, 1), (1, 2), (-2, 1), (-1, 2), (2, -1), (1, -2), (-2, -1), (-1, -2)):
            plot(img, x + dx, y + dy, half(col))
    plot(img, x, y, core)


def draw_star(img, d, x, y, bkey, i, st):
    t = talent(bkey, i)
    arm = tier_of(i) + 3
    col = P["cyan"] if bkey == "command" else P["gold"]
    if t[3] and t[3] != "glutton":
        ring(img, x, y, 8, PALS[t[3]]["col"], dash=(2, 1))
    if st == "over":
        ring(img, x, y, 7, P["green"])
        star(img, x, y, 5, P["green"])
    elif st == "max":
        star(img, x, y, arm + 1, col)
    elif st == "owned":
        star(img, x, y, arm, col)
    elif st == "open":
        ring(img, x, y, 4, P["ink"])
        ring(img, x, y, 3, P["parch"])
        plot(img, x, y, P["white"])
    else:
        star(img, x, y, 2, P["locked2"], P["locked2"])
    if st != "locked":
        pips(img, x, y + (arm + 3 if st in ("max", "owned", "over") else 6), bkey, i)


def clouds(img, base_y, top=22):
    """Scalloped cloud bank from the top of the sky down to base_y."""
    h, w = img.height, img.width
    yy, xx = np.mgrid[0:h, 0:w]
    edge = base_y - 7 + 7 * np.abs(np.sin(xx * math.pi / 26 + 0.6)) + 2 * np.sin(xx * 0.07)
    depth = edge - yy                          # pixels above the cloud's lower edge
    mask = (depth > 0) & (yy >= top)
    th = np.tile(BAYER, (h // 4 + 1, w // 4 + 1))[:h, :w]
    shade = np.clip(1 - depth / 26, 0, 1)
    arr = np.array(img)
    cols = np.array([P["night"], P["deep"], P["panel"], P["panel2"], P["edge"]], dtype=np.uint8)
    v = shade * (len(cols) - 1)
    base = np.floor(v).astype(int)
    idx = np.clip(base + ((v - base) > th), 0, len(cols) - 1)
    # thin mist under the lower edge
    mist = (depth <= 0) & (depth > -6) & (th > 0.8 + depth / 30)
    arr[mask] = cols[idx][mask]
    arr[mist] = np.array(P["deep"], dtype=np.uint8)
    return Image.fromarray(arr)


def hills(img, d):
    for layer, (base, amp, col) in enumerate(((278, 7, P["night"]), (289, 4, P["ink"]))):
        pts = [(0, H)]
        for x in range(0, W + 1, 2):
            y = base + amp * math.sin(x * 0.021 + layer * 2) + amp * 0.6 * math.sin(x * 0.067 + 1 + layer)
            pts.append((x, int(round(y))))
        pts.append((W, H))
        d.polygon(pts, fill=col)
    # tents by the fire
    for tx, s in ((282, 9), (364, 11), (248, 7)):
        ty = 300
        d.polygon([(tx - s, ty), (tx, ty - s - 2), (tx + s, ty)], fill=P["ink"])
        d.line([(tx, ty - s - 2), (tx + s, ty)], fill=P["gold3"] if tx < 320 else P["red3"])


def campfire(img, d, x, y):
    yy, xx = np.mgrid[0:H, 0:W]
    glow = np.clip(1 - np.hypot(xx - x, (yy - y) * 1.6) / 34, 0, 1)
    th = np.tile(BAYER, (H // 4 + 1, W // 4 + 1))[:H, :W]
    arr = np.array(img)
    m1 = (glow > 0.1) & (glow * 0.9 > th) & (arr.sum(axis=2) < 120)
    arr[m1] = np.array(P["red3"], dtype=np.uint8)
    m2 = (glow > 0.45) & (glow * 0.8 > th)
    arr[m2] = np.array(P["gold3"], dtype=np.uint8)
    img.paste(Image.fromarray(arr), (0, 0))
    flame = [
        "....k....",
        "...k1k...",
        "..k121k..",
        "..k1221k.",
        ".k12321k.",
        ".k12331k.",
        "k1233321k",
        "k1233321k",
        ".k12321k.",
        "kbbkkkbbk",
        "bbbbbbbbb",
    ]
    sprite(img, x - 4, y - 9, flame, {"k": P["red3"], "1": P["red"], "2": P["gold"], "3": P["white"],
                                     "b": P["brown"]})


def take3_screen():
    img = Image.new("RGB", (W, H))
    yy, xx = np.mgrid[0:H, 0:W]
    rng = np.random.default_rng(5)
    neb = np.zeros((H, W))
    for _ in range(8):
        bx, by = rng.uniform(0, W), rng.uniform(100, 280)
        s = rng.uniform(30, 90)
        neb += rng.uniform(0.3, 0.8) * np.exp(-((xx - bx) ** 2 + ((yy - by) * 1.8) ** 2) / (2 * s * s))
    neb = neb / neb.max()
    t = 0.08 + 0.4 * (yy / H) ** 1.6 + 0.24 * neb * (1 - 0.7 * yy / H)
    img.paste(dither(t, [P["ink"], P["night"], P["deep"], P["blue3"], P["purple3"]]), (0, 0))
    d = ImageDraw.Draw(img)
    rnd = random.Random(11)
    for _ in range(300):
        x, y = rnd.randrange(W), rnd.randrange(22, T3_HORIZON)
        plot(img, x, y, rnd.choice([P["edge"], P["mist"], P["locked2"], P["blue2"], P["edge"]]))

    def pos(node):
        return t3_pos(*node)

    # constellation lines
    for bkey in T3_ORDER:
        line_col = P["cyan"] if bkey == "command" else P["gold"]
        root = t3_pos(bkey, 0)
        pline(img, [root, (root[0], T3_HORIZON + 2)], P["gold3"], dash=(1, 2))
        for a, c in T3_EDGES:
            na, nc = (bkey, a), (bkey, c)
            la, lc = t3_lit(na), t3_lit(nc)
            if la and lc:
                pline(img, [pos(na), pos(nc)], line_col)
            elif (la or lc) and t3_charted(*nc):
                pline(img, [pos(na), pos(nc)], P["gold2"], dash=(2, 2))
            else:
                pline(img, [pos(na), pos(nc)], P["locked"], dash=(1, 2))
    for p, q in T3_BRIDGES:
        pp, pq = pos(p), pos(q)
        lit = t3_lit(p) or t3_lit(q)
        pline(img, [pp, pq], P["parch2"] if lit else P["edge"], dash=(1, 1))
        mx, my = (pp[0] + pq[0]) // 2, (pp[1] + pq[1]) // 2
        sprite(img, mx - 2, my - 2, ["..k..", ".k1k.", "k1.1k", ".k1k.", "..k.."],
               {"k": P["ink"], "1": P["parch2"] if lit else P["edge"]})

    # the cloud bank hides tier 3
    img.paste(clouds(img, T3_CLOUD), (0, 0))
    d = ImageDraw.Draw(img)
    text(d, 320, T3_CLOUD + 5, "THE CLOUDS PART AFTER ACT 2", "s", P["mist"], anchor="c", outline=True)

    # horizon, camp and the Pals
    hills(img, d)
    campfire(img, d, 320, 330)

    # Pal links: a thread of light from the star down to the Pal at camp
    def at(bkey, i):
        return t3_pos(bkey, i)

    tf, se, cl, wh = at("strike", 5), at("command", 3), at("chest", 2), at(*OVERFED)
    links = dict(
        butcher=[tf, (14, tf[1]), (14, 302)],
        glutton=[wh, (wh[0], 302)],
        shade=[se, (362, se[1]), (362, 302)],
        whisperer=[cl, (626, cl[1]), (626, 302)],
    )
    for who, pts in links.items():
        st = rank_state(*link_source(who), t3_open)
        link_line(img, pts, who, st)

    # stars
    for bkey in T3_ORDER:
        b = BRANCH[bkey]
        for i in range(6):
            x, y = t3_pos(bkey, i)
            if not t3_charted(bkey, i):
                text(d, x, y - 4, "?", "s", P["mist"], anchor="c", outline=True)
                t = talent(bkey, i)
                if t[3]:
                    ring(img, x, y, 7, PALS[t[3]]["col"], dash=(1, 2))
                continue
            draw_star(img, d, x, y, bkey, i, rank_state(bkey, i, t3_open))
        ramp = RAMP["cyan"] if bkey == "command" else RAMP["gold"]
        text(d, T3_CX[bkey], 140, b[1], "s", ramp[1], outline=True, anchor="c")
        text(d, T3_CX[bkey], 149, b[2], "s", P["mist"], outline=True, anchor="c")

    se_x, se_y = se
    cursor(img, se_x, se_y, 9)

    # Pals sit at camp, under the sky
    for who, x0, x1 in (("butcher", 6, 150), ("glutton", 156, 300), ("shade", 340, 484), ("whisperer", 490, 634)):
        st = rank_state(*link_source(who), t3_open)
        pal_card(img, d, x0, 302, x1, 358, who, st, compact=True)
        lx = links[who][-1][0]
        sprite(img, lx - 2, 299, ["k...k", "k1.1k", ".k1k.", "..k.."], {"k": P["ink"], "1": PALS[who]["col"]})

    # info ribbon: the selected star and the key
    d.rectangle([0, 21, W - 1, 52], fill=P["ink"])
    d.rectangle([0, 21, W - 1, 51], fill=P["panel"])
    d.line([0, 51, W - 1, 51], fill=P["gold3"])
    icon(img, 6, 25, "eye", RAMP["cyan"])
    text(d, 19, 23, "SECOND EDGE", "m", P["white"])
    x = text(d, 92, 27, "COMMAND  TIER 2  RANK 0/1  COST 2", "s", P["mist"]) + 94
    bone(img, x, 28)
    x += 16
    x += text(d, x, 27, "One Shade button takes a second enhancement.", "s", P["parch"]) + 8
    text(d, x, 27, "FEEDS SHADE", "s", P["purple"])
    # key row
    kx, ky = 8, 39
    text(d, kx, ky, "KEY", "s", P["gold"])
    kx += 24
    star(img, kx + 3, ky + 3, 3, P["gold"])
    kx += text(d, kx + 9, ky, "Lit", "s", P["parch"]) + 18
    ring(img, kx + 3, ky + 3, 3, P["parch2"])
    plot(img, kx + 3, ky + 3, P["parch"])
    kx += text(d, kx + 9, ky, "Can light", "s", P["parch"]) + 18
    star(img, kx + 3, ky + 3, 1, P["locked2"], P["locked2"])
    kx += text(d, kx + 9, ky, "Dark", "s", P["parch"]) + 18
    text(d, kx + 3, ky, "?", "s", P["mist"], anchor="c")
    kx += text(d, kx + 9, ky, "In the clouds", "s", P["parch"]) + 18
    pline(img, [(kx, ky + 3), (kx + 10, ky + 3)], P["parch2"], dash=(1, 1))
    kx += text(d, kx + 14, ky, "Bridge", "s", P["parch"]) + 22
    star(img, kx + 3, ky + 3, 3, P["green"])
    kx += text(d, kx + 9, ky, "Overfed", "s", P["parch"]) + 18
    pline(img, [(kx, ky + 3), (kx + 10, ky + 3)], P["purple"], width=2, dash=(4, 2))
    text(d, kx + 14, ky, "Thread to a Pal", "s", P["parch"])

    top_bar(img, d, "TALENTS", "ACT 2   MISSION 9   THE NIGHT SKY OVER CAMP")
    return img


def mini_sky(img, d, x0, y0, stage):
    w, h = 72, 74
    yy, xx = np.mgrid[0:h, 0:w]
    sky = dither(0.15 + 0.5 * (yy / h), [P["ink"], P["night"], P["deep"], P["purple3"]])
    img.paste(sky, (x0, y0))
    base = [44, 25, 6][stage]
    th = np.tile(BAYER, (h // 4 + 1, w // 4 + 1))[:h, :w]
    for y in range(h):
        for x in range(w):
            edge = base - 3 + 3 * abs(math.sin(x * math.pi / 9))
            dep = edge - y
            if dep > 0:
                v = min(1, dep / 12)
                plot(img, x0 + x, y0 + y, P["panel2"] if v < 0.4 or v < th[y, x] * 0.5 else P["panel"])
    lit_by_stage = [[0, 1], [0, 1, 2, 3], [0, 1, 2, 3, 4, 5]]
    for k in range(7):
        cx = x0 + 6 + k * 10
        ys = [y0 + 58, y0 + 52, y0 + 42, y0 + 34, y0 + 18, y0 + 11]
        dxs = [0, -3, 3, -2, 2, -2]
        prev = None
        for j in range(6):
            x, y = cx + dxs[j], ys[j]
            if y - y0 < base + 2:
                plot(img, x, y, P["mist"])
                continue
            lit = j in lit_by_stage[stage] and (k * 3 + j) % 4 != 3
            if lit:
                if prev:
                    pline(img, [prev, (x, y)], P["gold3"])
                star(img, x, y, 2 if j < 4 else 3, P["cyan"] if k == 3 else P["gold"])
                prev = (x, y)
            else:
                plot(img, x, y, P["parch2"])
    # horizon and fire
    d.rectangle([x0, y0 + 64, x0 + w - 1, y0 + h - 1], fill=P["ink"])
    sprite(img, x0 + 34, y0 + 62, [".k.", "k1k", "121"], {"k": P["red"], "1": P["gold"], "2": P["white"]})
    d.rectangle([x0, y0, x0 + w - 1, y0 + h - 1], outline=P["edge"])


def take3_strip(img, d, y0, h):
    strip(img, d, y0, h, "HOW YOU MOVE THROUGH IT", [
        ("START OF THE CAMPAIGN", ["Every constellation rises from the horizon, so its first star is open.",
                                   "Light any star that touches a lit one."], mini_sky),
        ("AFTER ACT 1", ["The clouds lift. Tier 2 stars show.",
                         "Bridges let you hop to the next branch."], mini_sky),
        ("AFTER ACT 2", ["The sky clears. Big tier 3 stars show.",
                         "The Butcher thread waits up there."], mini_sky),
    ])


def take3():
    return compose(3, "STAR CHART",
                   "The night sky over camp. Constellations rise from the horizon. Clouds part act by act.",
                   take3_screen(), take3_strip)


# ================================================================ output


def up(img):
    return img.resize((img.width * SCALE, img.height * SCALE), Image.NEAREST)


def combined(takes):
    gap, m, top = 12, 12, 40
    w = m * 2 + sum(t.width for t in takes) + gap * (len(takes) - 1)
    h = top + max(t.height for t in takes) + m
    img = Image.new("RGB", (w, h), P["ink"])
    d = ImageDraw.Draw(img)
    text(d, m, 8, "KINDRED SIEGE   TALENT TREE   THREE TAKES", "l", P["gold"])
    text(d, m, 25, "Same 42 talents and the same save in all three: Act 2, mission 9, 4 Marrow in the pocket. "
         "Mock only. Drawn with Python at 1x, shown at 3x.", "s", P["mist"])
    x = m
    for t in takes:
        d.rectangle([x - 2, top - 2, x + t.width + 1, top + t.height + 1], fill=P["gold3"])
        img.paste(t, (x, top))
        x += t.width + gap
    return img


def main():
    t1, t2, t3 = take1(), take2(), take3()
    for img, name in ((t1, "take-1-parry-wheel"), (t2, "take-2-war-board"), (t3, "take-3-star-chart")):
        up(img).save(os.path.join(OUT, name + ".png"), optimize=True)
    up(combined([t1, t2, t3])).save(os.path.join(OUT, "talent-tree-three-takes.png"), optimize=True)


if __name__ == "__main__":
    main()
