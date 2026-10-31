"""Side by side options for Jacob: each town under the real title overlay (strip, logo, list),
each camp with the four Pals standing in it. Mirrors PixelFont.Render and MakeStrip from
CampaignArtTitle.cs so the overlay matches the game. Usage: compose_options.py <art folder>"""
import json
import os
import sys
from PIL import Image, ImageDraw

ART = sys.argv[1]
RES = r"C:\Users\jacob\KindredSiege-Clean\Assets\Resources\CampaignArt"
GLYPHS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,!?:;'\"-+()0123456789/%*=_$"
NIGHT, LAMP, GLOW = (9, 17, 24, 255), (236, 156, 92, 255), (255, 214, 160, 255)
BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


class Font:
    def __init__(self, path):
        im = Image.open(path).convert("RGBA")
        self.w, self.h = im.size
        self.a = im.getchannel("A").load()
        cell = self.w // 8
        self.space = max(3, cell // 3)
        self.glyphs = {}
        cap_top, cap_bot, all_top, all_bot = cell, -1, cell, -1
        for i, ch in enumerate(GLYPHS):
            cx, cy = i % 8 * cell, i // 8 * cell
            left, right, top, bottom = cell, -1, cell, -1
            for y in range(cell):
                for x in range(cell):
                    if cy + y < self.h and self.a[cx + x, cy + y] > 127:
                        left, right, top, bottom = min(left, x), max(right, x), min(top, y), max(bottom, y)
            if right < left:
                continue
            self.glyphs[ch] = (cx + left, cy, right - left + 1)
            all_top, all_bot = min(all_top, top), max(all_bot, bottom)
            if i < 26:
                cap_top, cap_bot = min(cap_top, top), max(cap_bot, bottom)
        self.cap_top, self.cap_rows = cap_top, cap_bot - cap_top + 1

    def measure(self, text, spacing):
        width = 0
        for ch in text:
            if ch == " ":
                width += self.space
            elif ch in self.glyphs:
                width += self.glyphs[ch][2] + spacing
        return max(1, width - spacing)

    def render(self, text, fill, lower, spacing):
        top, rows = self.cap_top, self.cap_rows
        split = rows * 11 // 20
        w, h = self.measure(text, spacing) + 2, rows + 3
        ink = [[False] * w for _ in range(h)]
        pen = 1
        for ch in text:
            if ch == " ":
                pen += self.space
                continue
            if ch not in self.glyphs:
                continue
            gx, gy, gw = self.glyphs[ch]
            for y in range(rows):
                for x in range(gw):
                    if pen + x < w - 1 and gy + top + y < self.h and self.a[gx + x, gy + top + y] > 127:
                        ink[y + 1][pen + x] = True
            pen += gw + spacing

        def near(x, y):
            return any(0 <= x + dx < w and 0 <= y + dy < h and ink[y + dy][x + dx] for dy in (-1, 0, 1) for dx in (-1, 0, 1))

        out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        px = out.load()
        for y in range(h):
            for x in range(w):
                if ink[y][x]:
                    px[x, y] = lower if y - 1 >= split else fill
                elif near(x, y) or near(x, y - 1):
                    px[x, y] = NIGHT
        return out


def title_overlay(town):
    logo = Font(os.path.join(RES, "Menu", "font_logo.bytes"))
    tiny = Font(os.path.join(RES, "Menu", "font_tiny.bytes"))
    img = town.convert("RGBA").copy()
    px = img.load()
    for r in range(300):
        tex_row = (299 - r) % 4
        for x in range(152):
            gone = 0.0 if x < 128 else (x - 128 + 0.5) / 24
            if (1 - gone) * 16 > BAYER[3 - tex_row][x % 4]:
                px[x, r] = NIGHT
    kindred = logo.render("KINDRED", GLOW, LAMP, 2)
    siege = logo.render("SIEGE", GLOW, LAMP, 2)
    img.alpha_composite(kindred, (12, 14))
    img.alpha_composite(siege, (12, 16 + kindred.height))
    for i, label in enumerate(["CONTINUE", "NEW CAMPAIGN", "PALS", "MISSING ART", "QUIT"]):
        colour = LAMP if i == 0 else GLOW
        img.alpha_composite(tiny.render(label, colour, colour, 1), (24, 112 + i * 14))
    cursor = Image.new("RGBA", (3, 5), (0, 0, 0, 0))
    for y, run in enumerate([1, 2, 3, 2, 1]):
        for x in range(run):
            cursor.putpixel((x, y), LAMP)
    img.alpha_composite(cursor, (16, 112 + 2))
    return img


def camp_with_pals(scene):
    img = scene.convert("RGBA").copy()
    for i, pal in enumerate(["butcher", "glutton", "shade", "whisperer"]):
        sheet = json.load(open(os.path.join(RES, "CampLoops", pal + "_camp.json")))
        frame = sheet["frames"][0]["frame"]
        loop = Image.open(os.path.join(RES, "CampLoops", pal + "_camp.png")).convert("RGBA")
        cell = loop.crop((frame["x"], frame["y"], frame["x"] + frame["w"], frame["y"] + frame["h"]))
        img.alpha_composite(cell, (172 * i - 10 if i > 0 else 0, 300 - 8 - frame["h"]) if i > 0 else (0, 300 - 8 - frame["h"]))
    return img


def sheet(images, scale, path):
    w, h = images[0].width * scale, images[0].height * scale
    out = Image.new("RGB", (w, (h + 40) * len(images)), (20, 18, 24))
    d = ImageDraw.Draw(out)
    for i, im in enumerate(images):
        y = i * (h + 40)
        d.rectangle([0, y, 60, y + 36], fill=(214, 170, 92))
        d.text((20, y + 6), str(i + 1), fill=(20, 18, 24), font_size=26)
        out.paste(im.resize((w, h), Image.NEAREST).convert("RGB"), (0, y + 40))
    out.save(path)
    print("wrote", path, out.size)


towns = [Image.open(os.path.join(ART, n)) for n in ("town1_jaw.png", "town2_crater.png", "town3_moon.png")]
sheet([title_overlay(t) for t in towns], 2, os.path.join(ART, "options_town.png"))
camps = [Image.open(os.path.join(ART, n)) for n in ("camp1_canyon.png", "camp2_temple.png", "camp3_cave.png")]
sheet([camp_with_pals(c) for c in camps], 2, os.path.join(ART, "options_camp.png"))
