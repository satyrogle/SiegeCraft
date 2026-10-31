"""Real fight captures as a stage: cut every baked fighter out by its HP bar and a floor colour test,
so effects can go behind and in front of them, and they can flash."""
import numpy as np
from PIL import Image
from scipy import ndimage

import fxkit as fx

CAP = "C:/Users/jacob/.codex/scratch/"
FRAMES = {
    "hunt": CAP + "ks-art-audit-fixes-20261014/full/fight-11-AlphaHunt.png",
    "skullpeak": CAP + "ks-art-audit-fixes-20261014/full/fight-10-Chief.png",
    "stoneveil": CAP + "ks-new-tribes-20261019/audit8/stoneveil-bleeding.png",
    "thundercoast": CAP + "ks-new-tribes-20261019/audit8/thundercoast-flood.png",
}
BAR_FILLS = {"you": [(153, 190, 172)], "them": [(218, 106, 81)]}


def find_bars(a):
    """HP bars: rows with a long run of the bar fill colour. Returns [(side, x0, x1, y)]."""
    out = []
    for side, cols in BAR_FILLS.items():
        hit = np.zeros(a.shape[:2], bool)
        for c in cols:
            hit |= (np.abs(a[..., :3].astype(int) - c).sum(-1) <= 30)
        lab, n = ndimage.label(hit)
        for sl in ndimage.find_objects(lab):
            ys, xs = sl
            w, h = xs.stop - xs.start, ys.stop - ys.start
            if 20 <= w <= 110 and h <= 8 and ys.start > 300:
                out.append((side, xs.start, xs.stop, ys.start))
    out.sort(key=lambda b: (b[0], b[3], b[1]))
    merged = []
    for b in out:
        if merged and merged[-1][0] == b[0] and abs(merged[-1][3] - b[3]) <= 4 and abs(merged[-1][1] - b[1]) <= 12:
            continue
        merged.append(b)
    return merged


def floor_mask(sub, kind):
    r, g, b = sub[..., 0].astype(int), sub[..., 1].astype(int), sub[..., 2].astype(int)
    if kind == "snow":
        mx, mn = np.maximum(np.maximum(r, g), b), np.minimum(np.minimum(r, g), b)
        return (mn > 120) & (mx - mn < 45)
    return (r > 180) & (g > 105) & (g < 205) & (b > 30) & (b < 130) & (r - b > 90)


class Stage:
    def __init__(self, key, kind="sand", drawn=None, cut_sides=("you", "them")):
        self.base = Image.open(FRAMES[key]).convert("RGBA")
        a = np.asarray(self.base)
        self.units = []
        bars = find_bars(a)
        for side, x0, x1, y in bars:
            if side not in cut_sides:
                continue
            cx = (x0 + x1) // 2
            half = max(70, (x1 - x0))
            # Stop at the HP bar of a fighter standing above in the same column.
            above = [by for s2, bx0, bx1, by in bars if by < y and abs((bx0 + bx1) // 2 - cx) < 60]
            top = max([PLAYY0, y - 240] + [by + 8 for by in above])
            box = (max(PLAYX0, cx - half), top, min(PLAYX1, cx + half), y - 1)
            sub = a[box[1]:box[3], box[0]:box[2]]
            m = ~floor_mask(sub, kind)
            m = ndimage.binary_opening(m, iterations=1)
            lab, n = ndimage.label(m)
            if n == 0:
                continue
            sizes = ndimage.sum(m, lab, range(1, n + 1))
            # The fighter is the big piece standing on this bar: prefer pieces that reach the bottom of the box.
            objs = ndimage.find_objects(lab)
            floor_ok = [i for i, sl in enumerate(objs) if sl is not None and sl[0].stop >= m.shape[0] - 14 and sizes[i] > 60]
            big = (max(floor_ok, key=lambda i: sizes[i]) if floor_ok else int(np.argmax(sizes))) + 1
            keep = lab == big
            near = ndimage.binary_dilation(keep, iterations=6)
            for k in range(1, n + 1):
                if k != big and sizes[k - 1] > 25 and (near & (lab == k)).any():
                    ys_k, xs_k = np.nonzero(lab == k)
                    hk, wk = ys_k.max() - ys_k.min() + 1, xs_k.max() - xs_k.min() + 1
                    if hk < 24 and wk > 3 * hk:
                        continue   # a text label (BLEED 5.0S, COVERED), not part of the fighter
                    keep |= lab == k
            keep = ndimage.binary_fill_holes(ndimage.binary_closing(keep, iterations=2)) & ~floor_mask(sub, kind) | ndimage.binary_fill_holes(keep) & keep
            keep = ndimage.binary_fill_holes(keep)
            ys, xs = np.nonzero(keep)
            spr = np.zeros(sub.shape[:2] + (4,), np.uint8)
            spr[keep] = sub[keep]
            spr[keep, 3] = 255
            self.units.append(dict(i=len(self.units), side=side, x=cx, y=y - 2, box=box,
                                   spr=Image.fromarray(spr, "RGBA"), top=box[1] + int(ys.min()) if len(ys) else y - 100))
        self.drawn = drawn or []

    def render(self, under=None, over=None, mods=None, shake=(0, 0), between=None, before=None, t_ms=0):
        mods = mods or {}
        f = self.base.copy()
        if under:
            under(f)
        for u in sorted(self.units, key=lambda q: q["y"]):
            m = mods.get(u["i"], {})
            if "clip" in u:
                total = sum(d for _, d in u["clip"])
                t = (t_ms + u["phase"]) % total
                spr = u["clip"][0][0]
                for s_, d_ in u["clip"]:
                    if t < d_:
                        spr = s_
                        break
                    t -= d_
                u["box"] = (int(u["x"] - u["anchor"][0]), int(u["y"] - u["anchor"][1]), 0, 0)
            else:
                spr = u["spr"]
            if m.get("tint"):
                spr = fx.tint(spr, m["tint"])
            if before:
                before(f, u)
            if not m.get("hide"):
                f.alpha_composite(spr, (u["box"][0] + m.get("dx", 0), u["box"][1] + m.get("dy", 0)))
            if between:
                between(f, u)
        if over:
            over(f)
        if shake != (0, 0):
            play = f.crop(fx.PLAY)
            g = self.base.copy()
            g.paste(play, (fx.PLAY[0] + shake[0], fx.PLAY[1] + shake[1]))
            f = g
        # Effects never draw over the HUD.
        for box in ((0, 0, 1440, fx.PLAY[1]), (0, fx.PLAY[3], 1440, 900), (0, 0, fx.PLAY[0], 900), (fx.PLAY[2], 0, 1440, 900)):
            f.paste(self.base.crop(box), box[:2])
        return f.convert("RGB")


PLAYX0, PLAYY0, PLAYX1, PLAYY1 = fx.PLAY


def hunt_stage():
    """The desert hunt: the capture with the hunter placeholders painted out, Clarth cut out by mask, hunters drawn."""
    st = Stage("hunt", "sand", cut_sides=("you",))
    fight = fx.Fight()
    st.base = fight.base
    for u in fight.units:
        st.units.append(dict(i=100 + u["i"], side="them", x=u["x"], y=u["y"], clip=u["clip"], anchor=u["anchor"],
                             phase=u["phase"], box=(0, 0, 0, 0), name=fx.SPOTS[u["i"]][2]))
    return st
