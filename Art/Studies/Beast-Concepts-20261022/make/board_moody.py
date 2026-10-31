"""The moody pass on one picture: four rows (shark on Krea 2, shark on Qwen 2.1, bear on Krea 2, bear on Qwen 2.1), each
numbered 1 to 3, the same idea in the same column. Under 2000 px both ways. A missing picture shows as a grey panel.

python -I board_moody.py
"""
import pathlib

from PIL import Image, ImageDraw, ImageFont

HERE = pathlib.Path(__file__).resolve().parent
SRC = HERE.parent / "moody"
OUT = HERE.parent / "beast-concepts-moody-board.png"
PANEL_W, PANEL_H = 560, 383
GAP = 16
LABEL = 46
ROWS = [("SAND SHARK, KREA 2", "krea-shark"), ("SAND SHARK, QWEN 2.1", "qwen-shark"),
        ("CAGE BEAR, KREA 2", "krea-bear"), ("CAGE BEAR, QWEN 2.1", "qwen-bear")]


def font(size):
    for name in ("C:/Windows/Fonts/arialbd.ttf", "C:/Windows/Fonts/arial.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            continue
    return ImageFont.load_default()


def main():
    width = 3 * PANEL_W + 4 * GAP
    height = len(ROWS) * (LABEL + PANEL_H) + (len(ROWS) + 1) * GAP
    board = Image.new("RGB", (width, height), (255, 255, 255))
    draw = ImageDraw.Draw(board)
    title, number = font(28), font(36)
    y = GAP
    for label, key in ROWS:
        draw.text((GAP, y + 8), label, fill=(20, 20, 20), font=title)
        y += LABEL
        for i in range(3):
            x = GAP + i * (PANEL_W + GAP)
            path = SRC / ("%s-%d.png" % (key, i + 1))
            if path.exists():
                board.paste(Image.open(path).convert("RGB").resize((PANEL_W, PANEL_H), Image.LANCZOS), (x, y))
            else:
                draw.rectangle((x, y, x + PANEL_W - 1, y + PANEL_H - 1), fill=(150, 150, 150))
                draw.text((x + 20, y + PANEL_H // 2), "missing", fill=(255, 255, 255), font=title)
            draw.ellipse((x + 10, y + 10, x + 62, y + 62), fill=(255, 255, 255), outline=(20, 20, 20), width=3)
            draw.text((x + 36, y + 36), str(i + 1), fill=(20, 20, 20), font=number, anchor="mm")
        y += PANEL_H + GAP
    board.save(OUT)
    print("board", OUT, board.size)


if __name__ == "__main__":
    main()
