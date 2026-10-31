"""Local background extraction only. Preserve all retained RGB pixels and source PNG."""
from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np
root=Path(__file__).resolve().parent
source=Image.open(root/'bloodridge-whole-region.png').convert('RGB')
rgb=np.array(source)
# The model baked a neutral checker into the exterior. Only remove neutral
# pixels connected to a canvas edge, never enclosed material within the region.
neutral=(rgb.max(axis=2).astype(int)-rgb.min(axis=2).astype(int)<=8)&(rgb.min(axis=2)>95)
mask=Image.fromarray((neutral*255).astype('uint8')).copy()
for p in [(0,0),(source.width-1,0),(0,source.height-1),(source.width-1,source.height-1)]:
    if mask.getpixel(p)==255:
        ImageDraw.floodfill(mask,p,128,thresh=0)
alpha=np.where(np.array(mask)==128,0,255).astype('uint8')
out=source.convert('RGBA')
out.putalpha(Image.fromarray(alpha))
out.save(root/'region-extracted.png')
assert np.array_equal(np.array(out)[:,:,:3],rgb)
print('Native pixels retained; exterior removed:',int((alpha==0).sum()))
