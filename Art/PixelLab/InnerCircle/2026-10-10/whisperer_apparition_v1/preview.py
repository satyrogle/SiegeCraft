from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib
p=Path(__file__).parent
im=Image.open(p/'whisperer_front.png').convert('RGBA')
assert im.size==(192,192)
colours=len(im.getcolors(192*192));assert colours<=32
assert im.getpixel((0,0))[3]==0
out=Image.new('RGB',(800,864),(46,48,54))
scaled=im.resize((768,768),Image.Resampling.NEAREST)
out.paste(scaled,(16,64),scaled)
d=ImageDraw.Draw(out);font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',24)
d.text((24,18),'WHISPERER — APPARITION / REVIEW',font=font,fill=(236,226,211))
out.save(p/'phone-preview.png')
(p/'validation.json').write_text(json.dumps(dict(size=im.size,colours=colours,bounds=im.getbbox(),sha256=hashlib.sha256((p/'whisperer_front.png').read_bytes()).hexdigest()),indent=2))
print('192px transparent sprite checked; phone preview saved.')
