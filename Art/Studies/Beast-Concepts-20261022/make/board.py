"""One picture for Jacob to pick from: the sand shark's three looks on the top row, the cage bear's on the bottom, each
numbered 1 to 3 (the three-options rule). A missing picture shows as a grey panel.

python -I board.py
"""
import pathlib

from PIL import Image, ImageDraw, ImageFont

HERE = pathlib.Path(__file__).resolve().parent
RAW = HERE.parent / "raw"
OUT = HERE.parent / "beast-concepts-board.png"
PANEL = 560
GAP = 16
LABEL = 54
ROWS = [("SAND SHARK", "shark"), ("CAGE BEAR", "bear")]


def font(size):
    for name in ("C:/Windows/Fonts/arialbd.ttf", "C:/Windows/Fonts/arial.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            continue
    return ImageFont.load_default()


def main():
    width = 3 * PANEL + 4 * GAP
    height = len(ROWS) * (LABEL + PANEL) + (len(ROWS) + 1) * GAP
    board = Image.new("RGB", (width, height), (255, 255, 255))
    draw = ImageDraw.Draw(board)
    title, number = font(34), font(44)
    y = GAP
    for label, key in ROWS:
        draw.text((GAP, y + 8), label, fill=(20, 20, 20), font=title)
        y += LABEL
        for i in range(3):
            x = GAP + i * (PANEL + GAP)
            path = RAW / ("%s-%d.png" % (key, i + 1))
            if path.exists():
                board.paste(Image.open(path).convert("RGB").resize((PANEL, PANEL), Image.LANCZOS), (x, y))
            else:
                draw.rectangle((x, y, x + PANEL - 1, y + PANEL - 1), fill=(150, 150, 150))
                draw.text((x + 20, y + PANEL // 2), "missing", fill=(255, 255, 255), font=title)
            draw.ellipse((x + 12, y + 12, x + 76, y + 76), fill=(255, 255, 255), outline=(20, 20, 20), width=3)
            draw.text((x + 44, y + 44), str(i + 1), fill=(20, 20, 20), font=number, anchor="mm")
        y += PANEL + GAP
    board.save(OUT)
    print("board", OUT, board.size)


if __name__ == "__main__":
    main()
