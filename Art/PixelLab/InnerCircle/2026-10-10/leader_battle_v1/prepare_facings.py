from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,urllib.request,concurrent.futures
p=Path(__file__).parent
urls=json.loads((p/'rotation_urls.json').read_text())
d=p/'rotations';d.mkdir(exist_ok=True)
def get(item):
    name,url=item
    dest=d/(name+'.png')
    if not dest.exists():
        with urllib.request.urlopen(url,timeout=60) as r:dest.write_bytes(r.read())
    im=Image.open(dest).convert('RGBA');assert im.size==(192,192),(name,im.size)
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(get,urls.items()))
base=Image.open(p.parent/'corrections_v2'/'leader_front.png').convert('RGBA')
colours=[c for n,c in sorted(base.getcolors(192*192),reverse=True) if c[3]>0]
colours=[(0,0,0,0)]+colours
assert len(colours)<=32
pal=Image.new('RGBA',(32,1))
for i in range(32):pal.putpixel((i,0),colours[min(i,len(colours)-1)])
pal.save(p/'leader_palette.png')
report={}
out=Image.new('RGB',(1536,568),(43,45,52))
draw=ImageDraw.Draw(out);font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',24)
for i,(name,im) in enumerate([('APPROVED FRONT',base)]+[(k,Image.open(d/('south-east-selected.png' if k=='south-east' and (d/'south-east-selected.png').exists() else k+'.png')).convert('RGBA')) for k in ['south-east','south-west']]):
    # Add transparent room for relic motion and collapse while preserving every source pixel.
    padded=Image.new('RGBA',(256,256));padded.paste(im,(32,32))
    if i>0:
        stem='se' if name=='south-east' else 'sw'
        padded.save(d/f'leader_{stem}_start.png')
        report[stem]={'canvas':[256,256],'source_size':[192,192],'offset':[32,32],'scale':1,'bounds':padded.getbbox()}
    tile=padded.resize((512,512),Image.Resampling.NEAREST);out.paste(tile,(i*512,50),tile)
    draw.text((i*512+18,14),name,font=font,fill=(232,222,212))
out.save(p/'battle-facings.png')
(p/'pose_validation.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report))
