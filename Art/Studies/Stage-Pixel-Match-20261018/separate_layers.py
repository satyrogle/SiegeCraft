"""Separate reviewed static pixels for the fixed 1x battle camera; no resampling."""
from PIL import Image,ImageDraw,ImageChops
from pathlib import Path
import json,hashlib
ROOT=Path(__file__).resolve().parent
DEST=ROOT.parents[2]/'Assets/StreamingAssets/CampaignArt/RegionalReview'
regions={
 'skullpeak':{'rear':[(0,0),(1376,0),(1376,399),(1310,370),(1250,340),(1160,320),(1060,305),(970,300),(900,286),(815,295),(710,278),(600,286),(480,294),(380,302),(270,298),(185,315),(100,348),(0,390)],
             'front':[[(0,625),(54,653),(92,693),(137,726),(174,768),(0,768)],[(1376,668),(1330,694),(1292,737),(1270,768),(1376,768)]]},
 'hunters':{'rear':[(0,0),(1376,0),(1376,306),(1310,296),(1240,282),(1160,280),(1070,268),(960,275),(860,269),(760,267),(640,267),(530,275),(420,278),(310,275),(200,288),(100,294),(0,307)],
            'front':[[(0,678),(48,695),(97,725),(155,768),(0,768)],[(1376,714),(1350,720),(1321,749),(1310,768),(1376,768)]]}}
for name,layout in regions.items():
    target=DEST/name;target.mkdir(parents=True,exist_ok=True)
    source=Image.open(ROOT/name/(name+'-review.png')).convert('RGBA')
    rear_mask=Image.new('L',source.size);ImageDraw.Draw(rear_mask).polygon(layout['rear'],fill=255)
    front_mask=Image.new('L',source.size)
    for polygon in layout['front']:ImageDraw.Draw(front_mask).polygon(polygon,fill=255)
    rear=source.copy();rear.putalpha(rear_mask)
    front=source.copy();front.putalpha(front_mask)
    floor=source.copy()
    # Hidden underpaint gives complete opaque coverage under fixed scenery pieces.
    under=Image.new('RGBA',source.size,source.getpixel((700,550)))
    floor.paste(under,(0,0),ImageChops.lighter(rear_mask,front_mask))
    for filename,img in [('ground.png',floor),('rear.png',rear),('foreground.png',front)]:img.save(target/filename)
    composite=Image.alpha_composite(Image.alpha_composite(floor,rear),front)
    assert composite.tobytes()==source.tobytes(),name+' pixels changed'
    composite.save(ROOT/name/'layered-composite.png')
    metadata={'region':name,'dimensions':source.size,'stage_origin':[32,66],'scale':1,'camera':'fixed',
      'static_layers':['ground.png','rear.png'],'above_fighters':'foreground.png',
      'source_sha256':hashlib.sha256(source.tobytes()).hexdigest(),'recomposition_matches':True,
      'ambient':'Existing Bloodridge ash sheets, unmodified, clipped to distant scenery and tinted by presenter. Claude review pending.',
      'masks':layout}
    (target/'layers.json').write_text(json.dumps(metadata,indent=2),encoding='utf-8')
    print(name+': exact static recomposition; native 1376x768')
