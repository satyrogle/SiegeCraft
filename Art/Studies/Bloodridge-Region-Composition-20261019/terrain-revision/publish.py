"""Publish only the inspected terrain candidate, preserving native reference art."""
from pathlib import Path
from collections import Counter
import json, shutil
from PIL import Image, ImageDraw, ImageFilter
from revise import ROOT, PRIOR, GROVE

REPO=PRIOR.parents[2]
SOURCE=Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/comparison/bloodridge-reference.png')
DEST=Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/bloodridge-terrain')

def prepare():
    old=Image.open(PRIOR/'panorama.png').convert('RGB')
    candidate=ROOT/'ground-clearance/final-composition.png'
    assert candidate.exists(),'Inspect and collect the shelf correction before publishing'
    raw=Image.open(candidate).convert('RGB')
    court=Image.open(SOURCE).convert('RGB')
    assert raw.size==old.size==(4128,768)
    # Derive native material shades, keeping every old court and grove shade available.
    colors=list(Counter(old.getdata()))
    shades=raw.quantize(colors=100,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE).getpalette()
    for i in range(0,300,3):
        c=tuple(shades[i:i+3])
        if c not in colors and len(colors)<256:colors.append(c)
    pal=Image.new('P',(1,1));pal.putpalette([v for c in colors+[colors[0]]*(256-len(colors)) for v in c])
    final=raw.quantize(palette=pal,dither=Image.Dither.NONE).convert('RGB')
    # An interior feather hides VAE reconstruction at the protection boundary.
    mask=Image.new('L',final.size);ImageDraw.Draw(mask).polygon(GROVE,fill=255)
    mask=mask.filter(ImageFilter.MinFilter(9)).filter(ImageFilter.GaussianBlur(4))
    final.paste(old,(0,0),mask)
    final.paste(court.crop((256,0,1376,768)),(3008,0))
    final.save(ROOT/'panorama.png')
    old.save(ROOT/'previous.png');court.save(ROOT/'original-court.png')
    for i,x in enumerate([0,400,1400,2200,2752]):final.crop((x,0,x+1376,768)).save(ROOT/f'view-{i+1}.png')
    final.crop((0,0,3008,768)).save(ROOT/'new-region.png')
    court.crop((256,0,1376,768)).save(ROOT/'court-core.png')
    # Exact protected-grove test uses an eroded inner mask, excluding the blend edge.
    core=Image.new('L',final.size);ImageDraw.Draw(core).polygon(GROVE,fill=255)
    core=core.filter(ImageFilter.MinFilter(41))
    unchanged_grove=all(a==b for a,b,m in zip(final.getdata(),old.getdata(),core.getdata()) if m==255)
    assert unchanged_grove
    assert final.crop((3008,0,4128,768)).tobytes()==court.crop((256,0,1376,768)).tobytes()
    verification={'source':'ground-clearance/final-composition.png','dimensions':[4128,768],'resampled':False,'native_scale':1,'grove_core_unchanged':unchanged_grove,
        'court_core_unchanged':True,'palette_entries':len(colors),'unity_tested':False,'art_approved':False,
        'scope':'Static composition study. No gameplay, plane extraction, new animation or encounter mapping.'}
    (ROOT/'verification.json').write_text(json.dumps(verification,indent=2))
    html=(PRIOR/'index.html').read_text(encoding='utf-8')
    html=html.replace('Kindred Siege — connected battle regions','Bloodridge — terrain revision')
    html=html.replace('Bloodridge — outskirts to court.','Bloodridge — ash, roots, ravine, court.')
    html=html.replace('A composition study of ash outskirts, a root hollow and burial terraces leading into the original court. Move along the whole stretch, then inspect each place at fighter scale.','Follow the ash banks into the root hollow, along the ravine shelf and into the original court. Compare with the earlier pass at the same position.')
    html=html.replace('<button data-region="bloodridge" aria-pressed="true">Bloodridge</button>','<button id="latest" aria-pressed="true">Revised terrain</button><button id="previous" aria-pressed="false">Earlier pass</button>')
    html=html.replace('ROOT HOLLOW · BURIAL TERRACES','ROOT HOLLOW · RAVINE LEDGE')
    html=html.replace('<button id="prev">Previous view</button>','<button id="prev">Previous view</button><select id="place" aria-label="Place in region"><option value="0">Ash banks</option><option value="400">Root hollow</option><option value="1400">Ravine</option><option value="2200">Court approach</option><option value="2752">Original court</option></select>')
    html=html.replace('button{background:','select{background:#29242a;color:#d3c5b8;border:1px solid #51454c;padding:11px;font:inherit}button{background:')
    html=html.replace("let region='bloodridge',x=0,width=4128;","let region='bloodridge',x=0,width=4128,previous=false;")
    html=html.replace("document.querySelectorAll('nav button').forEach(b=>b.setAttribute('aria-pressed',b.dataset.region===r));",'')
    html=html.replace("$('overview').src=r+'/panorama.png';","$('overview').src=previous?'previous.png':r+'/panorama.png';")
    html=html.replace("$(layer==='foreground'?'front':layer).src=r+'/'+layer+'.png';","$(layer==='foreground'?'front':layer).src=previous&&layer==='ground'?'previous.png':r+'/'+layer+'.png';")
    html=html.replace("$('status').textContent=names[r]+' — composition proposal, awaiting Jacob’s visual review.';","$('status').textContent=previous?'Earlier pass — same viewport position.':'Revised Bloodridge terrain — composition proposal for Jacob’s review.';")
    html=html.replace("document.querySelectorAll('nav button').forEach(b=>b.onclick=()=>select(b.dataset.region));", "function version(p){previous=p;select('bloodridge');$('latest').setAttribute('aria-pressed',!p);$('previous').setAttribute('aria-pressed',p)}$('latest').onclick=()=>version(false);$('previous').onclick=()=>version(true);$('place').onchange=e=>{x=+e.target.value;place()};")
    html=html.replace("$('position').value=x;","$('position').value=x;$('place').value=String(x);")
    html=html.replace('Stage layer separation and Unity integration await the art review.','This is a static terrain composition. Stage layer separation and Unity integration await the art review.')
    (ROOT/'index.html').write_text(html,encoding='utf-8')
    DEST.mkdir(exist_ok=True);(DEST/'bloodridge').mkdir(exist_ok=True)
    shutil.copyfile(ROOT/'index.html',DEST/'index.html')
    shutil.copyfile(ROOT/'previous.png',DEST/'previous.png')
    for name in ['fighter.png','champion.png','ash-1.png','ash-2.png']:
        shutil.copyfile(DEST.parent/'bloodridge-composition'/name,DEST/name)
    final.save(DEST/'bloodridge/panorama.png');final.save(DEST/'bloodridge/ground.png')
    for layer in ['rear','foreground']:Image.new('RGBA',final.size).save(DEST/'bloodridge'/f'{layer}.png')
    (ROOT/'master.lua').write_text("""local root='"""+str(ROOT).replace('\\','/')+"""/'
local s=Sprite(4128,768,ColorMode.RGB)
s.layers[1].name='Revised terrain and preserved grove'
s:newCel(s.layers[1],1,Image{fromFile=root..'new-region.png'})
local court=s:newLayer();court.name='Original court core - unchanged'
s:newCel(court,1,Image{fromFile=root..'court-core.png'},Point(3008,0))
local plan=s:newLayer();plan.name='Geography guide - hidden'
s:newCel(plan,1,Image{fromFile=root..'layout-guide.png'});plan.isVisible=false
s:saveAs(root..'bloodridge-terrain.aseprite');s:close()
local reopened=app.open(root..'bloodridge-terrain.aseprite')
reopened:saveAs(root..'master-export.png');reopened:close()
""")
    print(json.dumps(verification,indent=2))
    print('http://127.0.0.1:8782/bloodridge-terrain/index.html')

if __name__=='__main__':prepare()
