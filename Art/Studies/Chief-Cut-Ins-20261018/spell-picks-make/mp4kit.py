"""Pixel previews to MP4 (libx264, millisecond timestamps). No side effects on import."""
from fractions import Fraction

import av
import numpy as np

import fxkit as fx


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
