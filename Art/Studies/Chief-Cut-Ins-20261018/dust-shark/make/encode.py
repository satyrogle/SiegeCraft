"""Render the three takes once and write MP4s: each take at 2x, and A B C side by side at 1x."""
import importlib
from fractions import Fraction

import av
import numpy as np
from PIL import Image, ImageDraw, ImageFont
import fxkit as fx

CROP = (770, 250, 1290, 790)


def mp4(frames, path, ms=fx.FRAME_MS, hold_last=600):
    c = av.open(str(path), mode="w")
    s = c.add_stream("libx264", rate=Fraction(1000, ms))
    s.width, s.height = frames[0].size
    s.pix_fmt = "yuv420p"
    s.codec_context.time_base = Fraction(1, 1000)
    s.options = {"crf": "14", "preset": "slow"}
    t = 0
    seq = list(frames) + [frames[-1]] * (hold_last // ms)
    for im in seq:
        vf = av.VideoFrame.from_ndarray(np.asarray(im.convert("RGB")), format="rgb24")
        vf.pts = t
        vf.time_base = Fraction(1, 1000)
        for pkt in s.encode(vf):
            c.mux(pkt)
        t += ms
    for pkt in s.encode():
        c.mux(pkt)
    c.close()


takes = []
for mod, folder, name, note, n in (("sceneA", "A-leaper", "A. Leaper", "fin, arc, bite, dive", 43),
                                   ("sceneB", "B-breach", "B. Breach from below", "cracks, jaws up, bite, sink", 41),
                                   ("sceneC", "C-maw", "C. Maw", "ripples, pit, ring of teeth, clamp", 43)):
    m = importlib.import_module(mod)
    frames = [m.frame(f).crop(CROP) for f in range(n)]
    big = [im.resize((im.width * 2, im.height * 2), Image.NEAREST) for im in frames]
    path = fx.STUDY / folder / ("dust-shark-" + folder + "-2x.mp4")
    mp4(big, path)
    print(path.name, path.stat().st_size // 1024, "KB")
    takes.append((name, note, frames))

F = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 26)
f2 = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 17)
N = max(len(t[2]) for t in takes)
w, h = takes[0][2][0].size
out = []
for i in range(N):
    canvas = Image.new("RGB", (3 * w + 16, h + 56), (24, 22, 28))
    d = ImageDraw.Draw(canvas)
    for j, (name, note, frames) in enumerate(takes):
        x = j * (w + 8)
        canvas.paste(frames[min(i, len(frames) - 1)], (x, 56))
        d.text((x + 8, 4), name, font=F, fill=(236, 228, 218))
        d.text((x + 8, 34), note, font=f2, fill=(170, 160, 150))
    out.append(canvas)
path = fx.STUDY / "dust-shark-A-B-C-side-by-side.mp4"
mp4(out, path)
print(path.name, path.stat().st_size // 1024, "KB", out[0].size)
