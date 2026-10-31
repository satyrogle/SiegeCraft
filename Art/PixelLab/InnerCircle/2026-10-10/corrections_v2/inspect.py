from PIL import Image,ImageDraw
from pathlib import Path
p=Path(__file__).parent
src=Image.open(p/'leader_clothing.png')
out=Image.new('RGB',(768,820),(45,48,54)); im=src.convert('RGBA').resize((768,768),Image.Resampling.NEAREST);out.paste(im,(0,40),im)
d=ImageDraw.Draw(out)
for x in range(0,192,16):
 d.line((x*4,40,x*4,808),fill=(70,70,70));d.text((x*4,20),str(x),fill='white')
for y in range(0,192,16):
 d.line((0,40+y*4,768,40+y*4),fill=(70,70,70));d.text((0,40+y*4),str(y),fill='white')
out.save(p/'leader-corrected-grid.png')
print(src.getcolors())
print(src.getpalette())
