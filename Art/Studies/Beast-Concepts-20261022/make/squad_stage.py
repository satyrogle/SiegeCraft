"""Make the player's side live: erase the five baked squad fighters (their bars and labels too) from both cleared
fight frames, keep the leader, and write the five spots so the scenes can draw the Clarth squad as real sprites that
flash, get knocked back and lose health. Writes fight/<stage>-empty.png and fight/squad.json.
"""
import json
import pathlib

from PIL import Image

OUT = pathlib.Path(__file__).resolve().parents[1] / "directions" / "fight"
STAGES = {
    "alphahunt": dict(floor=(835, 585, 950, 715),
                      clear=[(376, 300, 464, 436), (540, 322, 690, 436), (360, 455, 456, 576),
                             (352, 580, 474, 738), (548, 588, 656, 716)]),
    "skullpeak": dict(floor=(1090, 470, 1240, 690),
                      clear=[(378, 312, 462, 436), (518, 284, 670, 436), (368, 632, 462, 716),
                             (546, 588, 656, 738)]),
}
SQUAD = [("clarth_medic", 418, 424), ("clarth_warden", 600, 426), ("clarth_skirmisher", 418, 564),
         ("clarth_marksman", 418, 704), ("clarth_trapper", 600, 706)]


def tile(frame, floor, box):
    patch = frame.crop(floor)
    pw, ph = patch.size
    x0, y0, x1, y1 = box
    for y in range(y0, y1, ph):
        for x in range(x0, x1, pw):
            frame.paste(patch.crop((0, 0, min(pw, x1 - x), min(ph, y1 - y))), (x, y))


def main():
    for name, s in STAGES.items():
        frame = Image.open(OUT / (name + "-clean.png")).convert("RGB")
        for box in s["clear"]:
            tile(frame, s["floor"], box)
        frame.save(OUT / (name + "-empty.png"))
    (OUT / "squad.json").write_text(json.dumps([dict(sprite=n, x=x, y=y) for n, x, y in SQUAD], indent=2), encoding="utf-8")
    print("stages and squad written")


if __name__ == "__main__":
    main()
