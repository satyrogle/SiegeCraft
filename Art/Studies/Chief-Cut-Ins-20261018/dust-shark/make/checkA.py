"""Play the exported A sheets the way the game would (behind layer, fighter, front layer, cues) and put them next to the take."""
import json

from PIL import Image

import fxkit as fx

FXD = fx.STUDY / "fx"


def clip(name):
    meta = json.loads((FXD / (name + ".json")).read_text(encoding="utf-8"))
    sheet = Image.open(FXD / (name + ".png")).convert("RGBA")
    frames = [sheet.crop((f["frame"]["x"], f["frame"]["y"], f["frame"]["x"] + f["frame"]["w"], f["frame"]["y"] + f["frame"]["h"]))
              for f in meta["frames"]]
    return frames, (meta["anchorX"], meta["anchorY"]), meta["cues"]


BEHIND, ANCHOR, CUES = clip("dust_shark_leap_behind")
FRONT, _, _ = clip("dust_shark_leap_front")
TINT = {"white": (255, 250, 240), "red": (224, 40, 30)}
fight = fx.Fight()
L0, STAGGER = 5, 2
targets = [(i, L0 + STAGGER * n) for n, i in enumerate(fx.BACK)]


def frame(f):
    mods, shake = {}, [0, 0]
    for i, start in targets:
        k = f - start
        for c in CUES:
            if c["frame"] == k:
                m = mods.setdefault(i, {})
                if c.get("victim") in TINT:
                    m["tint"] = TINT[c["victim"]]
                if "victimDy" in c:
                    m["dy"] = c["victimDy"]
                if "shake" in c:
                    shake[0] += c["shake"][0]
                    shake[1] += c["shake"][1]

    def put(layer, u, frames):
        for i, start in targets:
            k = f - start
            if i == u["i"] and 0 <= k < len(frames):
                im = frames[k]
                layer.alpha_composite(im, (int(u["x"] - ANCHOR[0]), int(u["y"] - ANCHOR[1])))

    return fight.render(f * fx.FRAME_MS, mods=mods, shake=tuple(shake),
                        before=lambda layer, u: put(layer, u, BEHIND),
                        between=lambda layer, u: put(layer, u, FRONT))


if __name__ == "__main__":
    import sceneA
    pick = [L0 + 2 + k for k in (3, 7, 8, 9, 10, 13, 14, 18)]
    crop = (840, 330, 1180, 640)
    take = [sceneA.frame(f) for f in range(max(pick) + 1)]
    rebuilt = [frame(f) for f in range(max(pick) + 1)]
    tiles = [take[i] for i in pick] + [rebuilt[i] for i in pick]
    fx.strip(tiles, list(range(len(tiles))), crop, "checkA.png", scale=1, cols=len(pick))
    print("ok", len(BEHIND), ANCHOR)
