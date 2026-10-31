"""Native review assets for the single composition study, with source preservation."""
from pathlib import Path
from collections import Counter
import json, shutil
from PIL import Image
import numpy as np

ROOT=Path(__file__).resolve().parent
REPO=ROOT.parents[2]
SOURCE=Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/comparison/bloodridge-reference.png')
PREVIOUS=REPO/'Art/Studies/Connected-Regions-Natural-20261019'
DEST=Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/bloodridge-composition')

def prepare():
    image_source=ROOT/('composition.png' if (ROOT/'composition.png').exists() else 'raw.png')
    raw=Image.open(image_source).convert('RGB')
    court=Image.open(SOURCE).convert('RGB')
    assert raw.size==(4128,768)
    # Keep every court shade; derive the new materials' shades from the actual render.
    colors=list(Counter(court.getdata()))
    material_palette=raw.crop((0,0,3008,768)).quantize(colors=64,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE).getpalette()
    for color in [tuple(material_palette[i:i+3]) for i in range(0,192,3)]:
        if color not in colors:colors.append(color)
    palette=Image.new('P',(1,1));palette.putpalette([v for c in colors+[colors[0]]*(256-len(colors)) for v in c])
    left=raw.crop((0,0,3008,768)).quantize(palette=palette,dither=Image.Dither.NONE).convert('RGBA')
    final=Image.new('RGBA',raw.size);final.paste(left,(0,0));final.paste(court.crop((256,0,1376,768)),(3008,0))
    final.save(ROOT/'panorama.png');left.save(ROOT/'new-region.png')
    court.crop((256,0,1376,768)).save(ROOT/'court-core.png')
    court.save(ROOT/'original-court.png')
    assert final.crop((3008,0,4128,768)).convert('RGB').tobytes()==court.crop((256,0,1376,768)).tobytes()
    for i,x in enumerate([0,917,1835,2752]):final.crop((x,0,x+1376,768)).save(ROOT/f'view-{i+1}.png')
    final.crop((2064,0,3440,768)).save(ROOT/'court-approach.png')
    meta={'source':image_source.name,'dimensions':[4128,768],'native_scale':1,'source_court_colours':len(Counter(court.getdata())),
          'new_region_palette_colours':len(colors),'resampled':False,'court_core_x':3008,'court_core_preserved':True,
          'source_court_unchanged':True,'status':'Composition proposal, awaiting Jacob. No Unity binding or new animation.'}
    (ROOT/'review.json').write_text(json.dumps(meta,indent=2))
    # Reuse the established native review controls and unchanged fighter cutouts.
    html=(PREVIOUS/'index.html').read_text(encoding='utf-8')
    html=html.replace('<button data-region="skullpeak" aria-pressed="false">Skullpeak</button>','')
    html=html.replace('<button data-region="hunters" aria-pressed="false">Hunter wastes</button>','')
    html=html.replace('One region. Successive battlegrounds.','Bloodridge — outskirts to court.')
    html=html.replace('Follow the same ground from the sparse outskirts to the existing court. Drag the highlighted view along the strip, then inspect its pixels below.','A composition study of ash outskirts, a root hollow and burial terraces leading into the original court. Move along the whole stretch, then inspect each place at fighter scale.')
    html=html.replace('DEVELOPING APPROACH','ROOT HOLLOW · BURIAL TERRACES')
    html=html.replace('whole-width redraw candidate','composition proposal')
    html=html.replace('Original court assets are unchanged; their approach edges are blended only in these panorama copies.','Original court source is unchanged. Stage layer separation and Unity integration await the art review.')
    (ROOT/'index.html').write_text(html,encoding='utf-8')
    DEST.mkdir(exist_ok=True);d=DEST/'bloodridge';d.mkdir(exist_ok=True)
    shutil.copyfile(ROOT/'index.html',DEST/'index.html')
    for n in ['fighter.png','champion.png']:shutil.copyfile(PREVIOUS/n,DEST/n)
    for i in [1,2]:shutil.copyfile(REPO/'Assets/StreamingAssets/CampaignArt/Bloodridge'/f'ks-bloodridge-court-restrained-ash-review-{i}.png',DEST/f'ash-{i}.png')
    final.save(d/'panorama.png');final.save(d/'ground.png')
    blank=Image.new('RGBA',final.size)
    blank.save(d/'rear.png');blank.save(d/'foreground.png')
    (ROOT/'master.lua').write_text("""local root='"""+str(ROOT).replace('\\','/')+"""/'
local s=Sprite(4128,768,ColorMode.RGB)
s.layers[1].name='New region composition'
s:newCel(s.layers[1],1,Image{fromFile=root..'new-region.png'})
local l=s:newLayer();l.name='Original court core - unchanged'
s:newCel(l,1,Image{fromFile=root..'court-core.png'},Point(3008,0))
local plan=s:newLayer();plan.name='Input composition guide - hidden'
s:newCel(plan,1,Image{fromFile=root..'layout-guide.png'});plan.isVisible=false
s:saveAs(root..'bloodridge-composition.aseprite');s:close()
local again=app.open(root..'bloodridge-composition.aseprite')
again:saveAs(root..'master-export.png');again:close()
""")
    print(json.dumps(meta,indent=2))
    print('http://127.0.0.1:8782/bloodridge-composition/index.html')

if __name__=='__main__':prepare()
