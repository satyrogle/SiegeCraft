"""Targeted local correction: remove the repeated court from the starting area."""
from pathlib import Path
import json,time,shutil,sys
from PIL import Image,ImageDraw,ImageFilter
sys.path.insert(0,str(Path(__file__).resolve().parent))
from compose import api,ROOT,REPO,INPUT,OUTPUT

target=ROOT/'outskirts';target.mkdir(exist_ok=True)
canvas=Image.open(ROOT/'raw.png').convert('RGBA')
im=canvas.crop((0,0,1376,768))
alpha=Image.new('L',im.size,255)
d=ImageDraw.Draw(alpha)
d.polygon([(0,0),(680,0),(680,160),(610,240),(610,340),(490,385),(180,385),(180,768),(0,768)],fill=0)
im.putalpha(alpha);im.save(INPUT/'outskirts-edit.png')
g=json.loads((REPO/'Art/Studies/Connected-Regions-Natural-20261019/bloodridge/clear-ground/workflow-api.json').read_text())
g['6']['inputs']['image']='ks-bloodridge-composition-20261019/outskirts-edit.png'
g['9']['inputs']['prompt']='Remove the buildings, gates, stairs and monumental pillar in the LEFT half. Replace them with natural wind-eroded ash and limestone banks, a low distant barren mesa and scattered pale thorn scrub. This is the quiet outer edge of the same burial region, gradually leading toward the great hollow root tree and its pool on the RIGHT. Keep that tree, the reeds, red seedpods, pool and right half unchanged. Continue a shallow dry silt channel from the left into the existing dark pool. The near left edge has natural eroded stones and a few bone-white plants. Tiny dark moth silhouettes hover near the reeds. Match the exact existing fine pixel art style, worn organic forms, sepia and violet-brown colours and lighting. No new buildings, doors, stairs, city, humanoids, combat creatures or text.'
g['12']['inputs']['seed']=2026101992
g['14']['inputs']['filename_prefix']='KS-Bloodridge-Composition-20261019/outskirts'
(target/'workflow-api.json').write_text(json.dumps(g,indent=2))
receipt=target/'submission.json'
if receipt.exists():r=json.loads(receipt.read_text())
else:
    r=api('/prompt',{'prompt':g,'client_id':'codex-bloodridge-composition'})
    receipt.write_text(json.dumps(r,indent=2))
print(r,flush=True)
for n in range(720):
    h=api('/history/'+r['prompt_id']).get(r['prompt_id'])
    if h:
        (target/'history.json').write_text(json.dumps(h,indent=2))
        if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h['status'])
        ims=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
        if ims:
            v=ims[0];shutil.copyfile(OUTPUT/v.get('subfolder','')/v['filename'],target/'raw.png')
            edited=Image.open(target/'raw.png').convert('RGBA')
            # Native overlap feather removes the artificial input-mask boundary.
            # The tree and pool lie beyond this narrow transition and stay unchanged.
            blend=Image.eval(alpha,lambda value:255-value).filter(ImageFilter.GaussianBlur(24))
            canvas.paste(edited,(0,0),blend)
            canvas.save(ROOT/'composition.png')
            print('Saved corrected composition',flush=True);break
    if n%12==0:print('Outskirts correction queued or rendering',flush=True)
    time.sleep(5)
