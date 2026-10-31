import json, hashlib
from pathlib import Path
from PIL import Image
from itertools import groupby
ROOT=Path('C:/Users/jacob/KindredSiege-Clean')
paths=['Assets/Resources/CampaignArt/UI/ui_button.bytes','Assets/Resources/CampaignArt/Menu/font_tiny.bytes']+[f'Assets/Resources/CampaignArt/UI/icon_{s}.bytes' for s in ['hold','fallback','focus','unleash','eclipse']]
result=[]
for rel in paths:
    path=ROOT/rel
    im=Image.open(path).convert('RGBA')
    colors=list(dict.fromkeys(im.getdata()))
    indices={c:i for i,c in enumerate(colors)}
    rows=[]
    for y in range(im.height):
        row=[indices[im.getpixel((x,y))] for x in range(im.width)]
        rows.append([[k,len(list(g))] for k,g in groupby(row)])
    result.append(dict(path=rel,width=im.width,height=im.height,colors=['#'+bytes(c).hex() for c in colors],rows=rows,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
print(json.dumps(result,separators=(',',':')))
