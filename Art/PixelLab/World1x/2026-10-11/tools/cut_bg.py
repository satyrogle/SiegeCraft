"""Cut a flat background off a PixelLab object: clear every pixel joined to the border whose colour is
within a tolerance of the border colour. Usage: cut_bg.py <in> <out> <tolerance>"""
import sys
from collections import deque
from PIL import Image

src, dst, tol = sys.argv[1], sys.argv[2], int(sys.argv[3])
im = Image.open(src).convert("RGBA")
w, h = im.size
px = im.load()
corners = [px[0, 0], px[w - 1, 0], px[0, h - 1], px[w - 1, h - 1]]


def close(c):
    return c[3] < 128 or any(sum(abs(c[i] - k[i]) for i in range(3)) <= tol for k in corners)


seen = [[False] * w for _ in range(h)]
q = deque((x, y) for x in range(w) for y in (0, h - 1)) + deque((x, y) for y in range(h) for x in (0, w - 1))
while q:
    x, y = q.popleft()
    if not (0 <= x < w and 0 <= y < h) or seen[y][x] or not close(px[x, y]):
        continue
    seen[y][x] = True
    px[x, y] = (0, 0, 0, 0)
    q.extend(((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)))
im.save(dst)
print(dst, "kept bbox", im.getbbox())
