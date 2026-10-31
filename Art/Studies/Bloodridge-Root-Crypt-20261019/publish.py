"""Publish a single connected native panorama and inspectable composition master."""
from pathlib import Path
from collections import Counter
import json, shutil
import numpy as np
from PIL import Image, ImageFilter, ImageDraw
from edit_region import ROOT, PRIOR, COURT, EDITS

DEST=Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/bloodridge-root-crypt')
PREVIOUS=PRIOR/'terrain-revision'

def run():
    base=Image.open(ROOT/'base.png').convert('RGB')
    colors=list(Counter(base.getdata()))
    # The original court has 66 shades; the fighter limit does not apply to scenery.
    # Keep every existing base shade before adding a restrained new material ramp.
    for name in EDITS:
        candidate=ROOT/name/('refined.png' if name=='crypt' else 'cleaned.png')
        q=Image.open(candidate).convert('RGB').quantize(colors=24,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE)
        p=q.getpalette()
        for i in range(0,72,3):
            color=tuple(p[i:i+3])
            if color not in colors and len(colors)<256:colors.append(color)
    palette=Image.new('P',(1,1));palette.putpalette([v for c in colors+[colors[0]]*(256-len(colors)) for v in c])
    final=base.copy()
    for name,spec in EDITS.items():
        candidate=ROOT/name/('refined.png' if name=='crypt' else 'cleaned.png')
        edited=Image.open(candidate).convert('RGB').quantize(palette=palette,dither=Image.Dither.NONE).convert('RGB')
        edit=Image.open(ROOT/name/'edit-mask.png').filter(ImageFilter.MinFilter(7)).filter(ImageFilter.GaussianBlur(5))
        active=Image.fromarray(np.uint8(np.array(edit)>0)*255)
        layer=edited.convert('RGBA');layer.putalpha(active);layer.save(ROOT/name/'layer.png')
        final.paste(edited,spec['box'][:2],active)
    court=Image.open(COURT).convert('RGB')
    final.paste(court,(2752,0))
    # Keep the court at its original position. Its left approach floor meets the
    # existing continuous floor along an irregular ash edge, never a straight cut.
    earlier=Image.open(PRIOR/'panorama.png').convert('RGB').crop((2752,0,3008,768))
    source=court.crop((0,0,256,768))
    yy,xx=np.indices((768,256))
    edge=72+23*np.sin(yy*.027)+13*np.sin(yy*.061)-(yy-480)*.035
    tone=np.array(source).mean(axis=2)
    progress=np.clip((xx-edge+np.clip((tone-105)*.25,-10,10))/26,0,1)
    floor_weight=np.clip((yy-340)/100,0,1)
    a=1-floor_weight+floor_weight*progress
    earlier.paste(source,(0,0),Image.fromarray(np.uint8(a*255)))
    earlier=earlier.quantize(palette=palette,dither=Image.Dither.NONE).convert('RGB')
    final.paste(earlier,(2752,0))
    final.save(ROOT/'panorama.png');court.save(ROOT/'original-court.png')
    final.crop((2752,0,4128,768)).save(ROOT/'court-display.png')
    # Unchanged native reference cutouts, presented separately from the background.
    # This is a composition reference, never passed off as a gameplay capture.
    reference=final.crop((1400,0,2776,768)).convert('RGBA')
    shadows=Image.new('RGBA',reference.size);draw=ImageDraw.Draw(shadows)
    for file,x,y,flip in [('fighter.png',310,360,True),('fighter.png',470,470,True),('champion.png',850,360,False),('fighter.png',1010,470,False)]:
        draw.ellipse((x+45,y+140,x+110,y+154),fill=(24,20,33,85))
    reference.alpha_composite(shadows)
    for file,x,y,flip in [('fighter.png',310,360,True),('fighter.png',470,470,True),('champion.png',850,360,False),('fighter.png',1010,470,False)]:
        actor=Image.open(DEST.parent/'bloodridge-terrain'/file).convert('RGBA')
        if flip:actor=actor.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
        reference.alpha_composite(actor,(x,y))
    reference.convert('RGB').save(ROOT/'native-fighter-reference.png')
    for i,x in enumerate([0,400,1400,2200,2752]):final.crop((x,0,x+1376,768)).save(ROOT/f'view-{i+1}.png')
    assert final.size==(4128,768)
    assert final.crop((3008,0,4128,768)).tobytes()==court.crop((256,0,1376,768)).tobytes()
    assert final.crop((0,460,2752,768)).tobytes()==base.crop((0,460,2752,768)).tobytes()
    assert final.crop((688,0,1248,460)).tobytes()==base.crop((688,0,1248,460)).tobytes()
    meta={'dimensions':[4128,768],'native_scale':1,'resampled':False,'palette_entries':len(colors),
        'original_court_reference_unchanged':True,'original_court_core_preserved':True,'court_world_x':2752,'approach_floor_blend_x':[2752,3008],
        'fighting_floor_before_court_y460_to767_unchanged':True,
        'grove_core_preserved':True,'native_fighter_reference_is_gameplay_capture':False,'unity_tested':False,'art_approved':False}
    (ROOT/'verification.json').write_text(json.dumps(meta,indent=2))
    html=(PREVIOUS/'index.html').read_text(encoding='utf-8')
    html=html.replace('Bloodridge — terrain revision','Bloodridge — root crypt revision')
    html=html.replace('Bloodridge — ash, roots, ravine, court.','Bloodridge — into the root crypt.')
    html=html.replace('Follow the ash banks into the root hollow, along the ravine shelf and into the original court. Compare with the earlier pass at the same position.','One connected burial region: ash wash, root grove, sunken crypt and the original court. Move along the whole stretch and compare the previous terrain at the same position.')
    html=html.replace('Revised terrain','New region').replace('Earlier pass','Previous terrain')
    html=html.replace('BARE OUTSKIRTS','ASH WASH').replace('ROOT HOLLOW · RAVINE LEDGE','ROOT GROVE · SUNKEN CRYPT')
    html=html.replace('>Ash banks<','>Ash wash<').replace('>Ravine<','>Root crypt<')
    html=html.replace('Revised Bloodridge terrain — composition proposal for Jacob’s review.','Bloodridge root crypt — art proposal for Jacob’s review.')
    (ROOT/'index.html').write_text(html,encoding='utf-8')
    DEST.mkdir(exist_ok=True);(DEST/'bloodridge').mkdir(exist_ok=True)
    shutil.copyfile(ROOT/'index.html',DEST/'index.html')
    shutil.copyfile(PREVIOUS/'panorama.png',DEST/'previous.png')
    for name in ['fighter.png','champion.png','ash-1.png','ash-2.png']:shutil.copyfile(DEST.parent/'bloodridge-terrain'/name,DEST/name)
    final.save(DEST/'bloodridge/panorama.png');final.save(DEST/'bloodridge/ground.png')
    for name in ['rear','foreground']:Image.new('RGBA',final.size).save(DEST/'bloodridge'/f'{name}.png')
    (ROOT/'master.lua').write_text("""local root='"""+str(ROOT).replace('\\','/')+"""/'
local s=Sprite(4128,768,ColorMode.RGB)
s.layers[1].name='Preserved grove and native fighting floor'
s:newCel(s.layers[1],1,Image{fromFile=root..'base.png'})
local out=s:newLayer();out.name='Ash wash - native edit'
s:newCel(out,1,Image{fromFile=root..'outskirts/layer.png'})
local crypt=s:newLayer();crypt.name='Sunken root crypt - native edit'
s:newCel(crypt,1,Image{fromFile=root..'crypt/layer.png'},Point(1248,0))
local court=s:newLayer();court.name='Original court - approach floor edge blended'
s:newCel(court,1,Image{fromFile=root..'court-display.png'},Point(2752,0))
s:saveAs(root..'bloodridge-root-crypt.aseprite');s:close()
local again=app.open(root..'bloodridge-root-crypt.aseprite')
again:saveAs(root..'master-export.png');again:close()
""")
    print(json.dumps(meta,indent=2))
    print('http://127.0.0.1:8782/bloodridge-root-crypt/index.html')

if __name__=='__main__':run()
