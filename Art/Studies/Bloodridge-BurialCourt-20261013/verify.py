from pathlib import Path
from PIL import Image
import json, hashlib
R=Path(__file__).parent
P=Path('C:/Users/jacob/KindredSiege-Clean')
m=json.loads((R/'source-manifest.json').read_text())
results=[]
for i,(key,folder,w,h,ax,ay,x,y) in enumerate(m['fighters']):
    path=P/'Assets/Resources'/folder/(key+'.png')
    src=Image.open(path).convert('RGBA')
    exported=Image.open(R/f'fighter-{i}.png').convert('RGBA')
    mismatch=0
    for sy in range(h):
        for sx in range(w):
            a=src.getpixel((sx,sy)); b=exported.getpixel((x-ax+sx,y-ay+sy))
            if a[3]!=b[3] or (a[3] and a!=b):mismatch+=1
    results.append(dict(fighter=i,mismatched_pixels=mismatch,source_sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
ui=Image.open(R/'05-ui.png').convert('RGBA')
bad=0
for y in range(0,900,2):
    for x in range(0,1440,2):
        if len({ui.getpixel((x+dx,y+dy)) for dx in (0,1) for dy in (0,1)})!=1:bad+=1
unchanged=all(hashlib.sha256((P/s['path']).read_bytes()).hexdigest()==s['sha256'] for s in m['uiSources'])
layers={p.name:dict(size=list(Image.open(p).size),alpha_range=Image.open(p).convert('RGBA').getchannel('A').getextrema()) for p in R.glob('0*.png')}
result=dict(fighters=results,ui_nonuniform_2x_blocks=bad,ui_sources_hash_unchanged=unchanged,layers=layers,passed=all(r['mismatched_pixels']==0 for r in results) and bad==0 and unchanged)
(R/'pixel-verification.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result))
