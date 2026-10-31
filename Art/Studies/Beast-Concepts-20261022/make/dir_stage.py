"""Clear the enemy squad off two real fight frames so a beast can stand on its side: the desert Alpha Hunt (for the
sand shark) and the Skullpeak trial (for the cage bear). Each enemy, its bar and its labels are covered with tiles of
clean floor cut from the same frame, the Dust Shark kit's way. Writes directions/fight/<stage>-clean.png.
"""
import pathlib

from PIL import Image

FULL = pathlib.Path("C:/Users/jacob/.codex/scratch/ks-art-audit-fixes-20261014/full")
OUT = pathlib.Path(__file__).resolve().parents[1] / "directions" / "fight"

STAGES = {
    "alphahunt": dict(
        src="fight-11-AlphaHunt.png",
        floor=(835, 585, 950, 715),
        clear=[(744, 318, 846, 444), (734, 455, 834, 582), (976, 320, 1078, 444), (1082, 320, 1186, 444),
               (979, 455, 1081, 582), (979, 590, 1081, 716), (1169, 488, 1271, 644), (550, 780, 890, 810)],
    ),
    "skullpeak": dict(
        src="coverage-skullpeak-Trial.png",
        floor=(1090, 470, 1240, 690),
        clear=[(756, 292, 924, 476), (978, 310, 1068, 434), (958, 472, 1058, 574), (782, 572, 898, 716),
               (972, 588, 1070, 736), (380, 782, 1066, 834)],
    ),
}


def tile(frame, floor, box):
    patch = frame.crop(floor)
    pw, ph = patch.size
    x0, y0, x1, y1 = box
    for y in range(y0, y1, ph):
        for x in range(x0, x1, pw):
            piece = patch.crop((0, 0, min(pw, x1 - x), min(ph, y1 - y)))
            frame.paste(piece, (x, y))


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for name, s in STAGES.items():
        frame = Image.open(FULL / s["src"]).convert("RGB")
        for box in s["clear"]:
            tile(frame, s["floor"], box)
        frame.save(OUT / (name + "-clean.png"))
        print(name, frame.size)


if __name__ == "__main__":
    main()
