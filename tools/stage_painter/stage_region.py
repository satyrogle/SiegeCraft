import json
import math
import subprocess
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
import barren_life as BL
import regions as RG
import stage_paint as SP
import town_fix as TF

# stage_region.py STAGE [paint] [life]
# Jacob, 30 Oct 2026, on the first five Bloodridge stages: "i said use same artstyle layering etc not copy paste the
# same shit and call it a day make bloodridge have more flavour and varirty if you compare where is camp and wheres
# this dog shit", then "yes go" to Claude's plan: five places of their own (red dunes at sunrise, their siege camp at
# noon, the ember canal at dusk, the necropolis by moonlight, the eclipse court), each with its own sky, ground and
# living layer, made the camp's way (same painter and palette rules, three layers, a loop). Stage 1 first, as the
# template. Bloodridge's look is their 3 Oct lock: Medjai desert guardians in Beksinski's mood, rotted sepia rags,
# burial bandages, corroded bronze, lava glowing through cracks. Layout and lanes as stage_paint.py.
OUT = RG.REPO / "Art/Stages/Bloodridge"  # not ignored, so results can be pushed for review (31 Oct)
W, H, N, FPS = SP.W, SP.H, 96, 12
WHO = ("A battle stage in the land of the Bloodridge, Egyptian desert guardians after the Medjai, painted in the eerie "
       "dreamlike mood of Zdzislaw Beksinski: rotted sepia rags, burial bandages, corroded bronze, ashen stone and ember "
       "light glowing through cracks. Only the few things named for this place stand here, along the far edge; nothing "
       "else is built anywhere in the picture.")
T = ["#3e302c", "#7a6250", "#96703e", "#f07828"]  # Bloodridge's own sepia, bronze and ember
STAGES = {
    "1": dict(name="THE RED DUNES", seed=2026103011, note="the outskirts of their land at sunrise",
              sky="a vast sunrise sky, deep violet blue above fading through crimson to burning orange and gold at the "
                  "horizon, a huge low red sun",
              far="a sea of blood red dunes rolling away to the horizon, a towering dark wall of a sandstorm far away",
              light="Low warm sunrise light from the left, long violet shadows, as discrete flat pixel colour planes, "
                    "the ground in calm even light.",
              pal=["#2a2050", "#5a2a6a", "#c03a3a", "#f08040", "#f8c070", "#fff0c0", "#a83a30", "#d86a48", "#8a2a28",
                   "#5a1a1a"] + T,
              floor="flat packed blood red sand with long soft wind ripples, crisp and clear, calm and even, no dust "
                    "and no fog",
              plan=dict(sky0=[48, 36, 96], sky1=[244, 150, 84], far=[176, 70, 58], back=[160, 62, 52],
                        floor0=[200, 96, 72], floor1=[164, 72, 58], piece=[64, 40, 40]),
              back=[((0, 205, W, 290), "the crests of the nearest blood red dunes along the far edge, thin plumes of "
                                       "sand blowing off their tops"),
                    ((150, 40, 560, 290), "a colossal stone head of a hooded desert guardian half buried in the dunes, "
                                          "its face worn away, two ember eyes still glowing in the dark of the hood"),
                    ((930, 150, 1180, 290), "the tip of a giant corroded bronze khopesh blade sticking out of the "
                                            "sand at an angle")],
              near=["a dark wind cut rock", "a half buried corroded bronze shield"],
              life=dict(crest=(205, 290), birds=((960, 70, 230, 26), (420, 40, 160, 18)), embers="head")),
    "2": dict(name="THE SIEGE CAMP", seed=2026103012, note="their war camp at noon",
              sky="a hard bright noon sky, pale blue bleached almost white near the horizon, a white sun high up",
              far="far away across a gap of white clouds, a pale lavender plateau with a thin column of smoke",
              light="Hard white noon sun from straight above, short dark shadows, as discrete flat pixel colour planes, "
                    "the ground in calm even light.",
              pal=["#4a8ad0", "#cfe4f6", "#ffffff", "#e8d8b0", "#d0a868", "#a87e48", "#ece4ee", "#8a2a28"] + T,
              floor="flat trampled ochre sand with old cart ruts, crisp and clear, calm and even, no dust and no fog",
              plan=dict(sky0=[74, 138, 208], sky1=[226, 236, 244], far=[230, 232, 238], back=[200, 170, 120],
                        floor0=[216, 176, 112], floor1=[188, 146, 92], piece=[70, 52, 44]),
              back=[((0, 215, W, 290), "rows of low tents of rotted sepia rags along the far edge"),
                    ((90, 70, 470, 290), "a big ancient siege catapult of corroded bronze with a tall throwing arm and "
                                         "a sling, a heap of rough boulders beside it"),
                    ((900, 80, 1280, 290), "a second bronze siege catapult with its throwing arm cocked back, ragged "
                                           "banners with a dark red sun disc on poles")],
              near=["a heap of rough boulders", "a broken bronze wheel half buried in the sand"]),
    "3": dict(name="THE EMBER CANAL", seed=2026103013, note="a canal of lava through their dead city at dusk",
              sky="a dusk sky, deep indigo above fading through violet to burning orange at the horizon",
              far="black basalt ridges far away, their edges lit orange by the last light",
              light="Last orange dusk light from the right, deep violet shadows, as discrete flat pixel colour planes, "
                    "the ground in calm even light.",
              pal=["#1e1a3a", "#4a2a6a", "#e06a3a", "#f8b060", "#ffe08a", "#5a5a6a", "#9a96a0", "#c8c4c8"] + T,
              floor="flat pale ash grey basalt slabs with dark seams, crisp and clear, calm and even, no dust and no "
                    "fog, nothing glowing on the floor",
              plan=dict(sky0=[34, 28, 70], sky1=[236, 128, 70], far=[60, 48, 64], back=[90, 80, 92],
                        floor0=[184, 180, 186], floor1=[150, 146, 154], piece=[40, 32, 36]),
              back=[((0, 225, W, 290), "a canal of glowing orange lava running the whole width along the far edge, "
                                       "behind a low basalt wall"),
                    ((120, 60, 330, 290), "a broken black obelisk with lava glowing through its cracks on the canal's "
                                          "bank"),
                    ((620, 150, 1000, 290), "a narrow corroded bronze bridge arching over the lava canal"),
                    ((1080, 90, 1280, 290), "a second broken black obelisk leaning over the lava")],
              near=["a dark basalt boulder", "a dark basalt boulder"]),
    "4": dict(name="THE NECROPOLIS", seed=2026103014, note="their city of the dead by moonlight",
              sky="a clear night sky, deep blue with a few stars and a huge pale moon",
              far="the dark blood red ridge, its cliffs cut into rows of tomb fronts",
              light="Cold pale moonlight from the upper left, deep blue shadows, as discrete flat pixel colour planes, "
                    "the ground pale and lit so it reads clearly.",
              pal=["#0e1430", "#1e2a5a", "#3a4a8a", "#dfe6f2", "#8a2a28", "#5a1a1a", "#c8c8d4", "#9aa0b4"] + T,
              floor="flat pale moonlit flagstones with drifted sand in the seams, crisp and clear, calm and even, no "
                    "fog",
              plan=dict(sky0=[14, 20, 48], sky1=[40, 54, 104], far=[90, 30, 32], back=[110, 40, 40],
                        floor0=[196, 198, 210], floor1=[160, 162, 178], piece=[30, 24, 32]),
              back=[((0, 40, W, 290), "tall tomb fronts carved into the blood red cliff, dark doorways, burial "
                                      "bandages hanging from the lintels"),
                    ((140, 110, 330, 290), "a tall robed guardian statue in an alcove, hood with two ember eyes"),
                    ((1050, 110, 1240, 290), "a second tall robed guardian statue in an alcove, hood with two ember "
                                             "eyes")],
              near=["a toppled stone sarcophagus lid", "a dark broken stone edge"]),
    "5": dict(name="THE ECLIPSE COURT", seed=2026103015, note="the chief's court under his eclipse",
              sky="a crimson sky with a black sun in it, a burning ring of fire around the black sun",
              far="the dark ridge far away under the crimson sky",
              light="Red eclipse light from above, deep crimson shadows, as discrete flat pixel colour planes, the "
                    "ground lit so it reads clearly.",
              pal=["#2a0a10", "#6a1418", "#c02a24", "#f06a2a", "#ffc070", "#1a1012", "#e8d8b8", "#b89870"] + T,
              floor="flat polished pale sandstone with a bronze sun disc inlay near the far edge, crisp and clear, "
                    "calm and even, no fog",
              plan=dict(sky0=[60, 10, 18], sky1=[200, 50, 40], far=[90, 30, 30], back=[120, 60, 50],
                        floor0=[220, 200, 166], floor1=[190, 166, 130], piece=[40, 20, 24]),
              back=[((300, 20, 1080, 290), "a great stepped pyramid of dark stone behind the gate"),
                    ((390, 70, 990, 290), "the dark stone temple gate of two sloped pylon towers, ember light in its "
                                          "tall doorway"),
                    ((110, 120, 290, 290), "a tall pole hung with ram skulls and a bronze brazier with ember light"),
                    ((1090, 120, 1270, 290), "a tall pole hung with ram skulls and a bronze brazier with ember "
                                             "light")],
              near=["a dark broken stone edge", "a dark broken stone edge"]),
}


def prompt(o):
    els = [{"type": "obj", "bbox": SP.bb((0, 0, W, 170)), "desc": o["sky"]},
           {"type": "obj", "bbox": SP.bb((0, 120, W, 250)), "desc": o["far"]}]
    els += [{"type": "obj", "bbox": SP.bb(b), "desc": d} for b, d in o["back"]]
    els += [{"type": "obj", "bbox": SP.bb((0, SP.FLOOR_Y, W, H)), "desc": o["floor"]},
            {"type": "obj", "bbox": SP.bb(SP.FRAMEBOX[0]), "desc": o["near"][0] + " in the near foreground, in shadow"},
            {"type": "obj", "bbox": SP.bb(SP.FRAMEBOX[1]), "desc": o["near"][1] + " in the near foreground, in shadow"}]
    return json.dumps({"high_level_description": WHO + " " + SP.STYLE + " No people anywhere, the fighters are added "
                                                 "later. This place: " + o["name"].lower() + ", " + o["note"] + ".",
                       "style_description": {"medium": "pixel_art", "art_style": RG.L3.ART, "aesthetics": SP.AES,
                                             "lighting": o["light"], "color_palette": o["pal"]},
                       "compositional_deconstruction": {"background": SP.LAY, "elements": els}})


def paint(k):
    o = STAGES[k]
    SP.OUT, SP.PLAN = OUT, o["plan"]
    OUT.mkdir(parents=True, exist_ok=True)
    init = SP.plan(o)
    init.save(OUT / ("stage-%s-init.png" % k))
    raw = SP.run("region-%s" % k, init, prompt(o), o["seed"])
    raw.save(OUT / ("stage-%s-raw.png" % k))
    RG.clean(raw).save(OUT / ("stage-%s.png" % k))
    SP.test_fight(k)


def layers(k, a):
    """The game's three files (CampaignArtRegionalStage.cs): ground, the whole painting; rear, everything above the
    floor line; foreground, the dark near corner pieces cut out of the floor."""
    d = OUT / ("stage-%s-layers" % k)
    d.mkdir(exist_ok=True)
    rgba = np.dstack([a, np.full((H, W), 255, np.uint8)])
    Image.fromarray(rgba).save(d / "ground.png")
    rear = rgba.copy()
    rear[SP.FLOOR_Y:, :, 3] = 0
    Image.fromarray(rear).save(d / "rear.png")
    lu = a.astype(float) @ np.array([0.299, 0.587, 0.114])
    floor_lu = np.median(lu[SP.FLOOR_Y + 40:H - 140])
    fg = np.zeros((H, W, 4), np.uint8)
    for x0, y0, x1, y1 in SP.FRAMEBOX:
        x0, y0, x1, y1 = max(0, x0 - 40), y0 - 60, min(W, x1 + 40), y1
        m = lu[y0:y1, x0:x1] < floor_lu - 45
        fg[y0:y1, x0:x1][m] = rgba[y0:y1, x0:x1][m]
    Image.fromarray(fg).save(d / "foreground.png")


CALM = {"1": (0.7, (198, 168, 123), 0.4)}  # stage: (colour kept, the sand it leans to, how far). The red dunes'
# floor came out one loud red and Clarth's red cloaks sank into it (30 Oct look); the dunes behind keep their red.


def calm(k):
    """The stage's painting, its fight floor calmed where CALM says, faded in below the floor line. Always from the
    raw paint, so running it twice changes nothing."""
    if k not in CALM:
        return np.asarray(Image.open(OUT / ("stage-%s.png" % k)).convert("RGB"))
    keep, col, kk = CALM[k]
    f = np.asarray(RG.clean(Image.open(OUT / ("stage-%s-raw.png" % k)).convert("RGB"))).astype(float)
    lu = (f @ np.array([0.299, 0.587, 0.114]))[..., None]
    m = (lu + (f - lu) * keep) * (1 - kk) + np.array(col, float) * kk
    ramp = np.clip((np.arange(H) - (SP.FLOOR_Y - 4)) / 24.0, 0, 1)[:, None, None]
    a = np.asarray(RG.clean(Image.fromarray((f * (1 - ramp) + m * ramp).clip(0, 255).round().astype(np.uint8))))
    Image.fromarray(a).save(OUT / ("stage-%s.png" % k))
    SP.OUT = OUT
    SP.test_fight(k)
    return a


def life(k):
    """A seamless loop of the stage's living layer: sand blowing off the dune crests (specks that break up, never
    fade), scavengers circling, and the guardian's ember eyes flickering (only ember pixels inside the dark hood; the
    first loop caught the orange sunrise behind the head too)."""
    o, L = STAGES[k], STAGES[k].get("life", {})
    a = calm(k)
    layers(k, a)
    base = a.astype(float)
    rng = np.random.default_rng(o["seed"])
    lu = base @ np.array([0.299, 0.587, 0.114])
    sand = base[SP.FLOOR_Y + 20:SP.FLOOR_Y + 120].reshape(-1, 3)
    sand = sand[np.argsort(sand @ np.array([0.299, 0.587, 0.114]))][-len(sand) // 4:]  # the light sand tones
    y0, y1 = L.get("crest", (205, 290))
    specks = [dict(x=float(rng.uniform(0, W)), y=float(rng.uniform(y0, y1)), t0=int(rng.integers(0, N)),
                   life=int(rng.integers(14, 30)), vx=float(rng.uniform(1.6, 3.0)), vy=float(rng.uniform(-0.7, -0.2)),
                   c=sand[int(rng.integers(0, len(sand)))]) for _ in range(150)]
    eyes = np.zeros((H, W), bool)
    if L.get("embers"):
        bx0, by0, bx1, by1 = o["back"][1][0]
        e = (base[..., 0] > 200) & (base[..., 1] > 70) & (base[..., 1] < 190) & (base[..., 2] < 110)
        dark = np.asarray(Image.fromarray(lu.clip(0, 255).astype(np.uint8)).filter(ImageFilter.BoxBlur(7))) < 95
        eyes[by0:by1, bx0:bx1] = (e & dark)[by0:by1, bx0:bx1]
    pt = BL.ptero_frames("far")
    sky = lu > -1
    sky[SP.FLOOR_Y - 60:] = False
    frames = []
    for t in range(N):
        cv = base.copy()
        if eyes.any():
            g = (1.0, 1.18, 1.05, 0.85, 1.12, 1.0)[(t // 2) % 6]
            cv[eyes] = np.clip(base[eyes] * g, 0, 255)
        for i, (cx, cy, rx, ry) in enumerate(L.get("birds", ())):
            for j in range(2):
                ph = 2 * math.pi * (t / N + j * 0.5 + i * 0.21)
                rgb, al = pt[(t // 2 + j * 3) % len(pt)]
                TF.put(cv, rgb[:, ::-1] if math.cos(ph) < 0 else rgb, al[:, ::-1] if math.cos(ph) < 0 else al,
                       cx + rx * math.sin(ph) - al.shape[1] / 2, cy + ry * math.cos(ph) - al.shape[0] / 2, vis=sky)
        for s in specks:
            u = (t - s["t0"]) % N
            if u >= s["life"]:
                continue
            x, y = s["x"] + s["vx"] * u, s["y"] + s["vy"] * u + 0.02 * u * u
            n = 2 if u < s["life"] * 0.5 else 1  # a short streak, then a single speck, then gone
            for q in range(n):
                xi, yi = int(x - q) % W, int(y)
                if 0 <= yi < SP.FLOOR_Y + 20:
                    cv[yi, xi] = s["c"]
        frames.append(cv.clip(0, 255).astype(np.uint8))
    dst = OUT / ("stage-%s-life.mp4" % k)
    p = subprocess.Popen([BL.FF, "-v", "error", "-y", "-f", "rawvideo", "-pix_fmt", "rgb24", "-s", "%dx%d" % (W, H),
                          "-r", str(FPS), "-i", "-", "-vf", "scale=%d:%d:flags=neighbor" % (W * 2, H * 2),
                          "-c:v", "libx264", "-pix_fmt", "yuv420p", "-crf", "16", str(dst)], stdin=subprocess.PIPE)
    for _ in range(3):
        for f in frames:
            p.stdin.write(f.tobytes())
    p.stdin.close()
    p.wait()
    strip = Image.new("RGB", (W, 3 * (H // 2) + 8))
    for i, t in enumerate((0, 32, 64)):
        strip.paste(Image.fromarray(frames[t]).resize((W // 2, H // 2), Image.BOX), ((i % 2) * (W // 2), (i // 2) * (H // 2 + 4)))
    Image.open(OUT / ("stage-%s-test-fight.png" % k)).resize((W // 2, H // 2), Image.BOX).save(OUT / "tmp.png")
    strip.paste(Image.open(OUT / "tmp.png"), (W // 2, H // 2 + 4))
    (OUT / "tmp.png").unlink()
    strip.save(OUT / ("look-stage-%s.png" % k))
    print("life", k, dst.name, "frames", N, "eye px", int(eyes.sum()), flush=True)


if __name__ == "__main__":
    k = sys.argv[1]
    if "life" not in sys.argv[2:]:
        paint(k)
    if "paint" not in sys.argv[2:]:
        life(k)
