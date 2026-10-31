from pathlib import Path
import sys,json,time,shutil
from PIL import Image,ImageDraw
sys.path.insert(0,'Art/Studies/Connected-Regions-20261018')
from outpaint import api,SOURCES
root=Path('Art/Studies/Connected-Regions-Natural-20261019')
jobs=[]
for region in sys.argv[1:]:
 base=root/region;p=base/'qwen-transition';p.mkdir(exist_ok=True)
 source=base/('clear-ground/raw.png' if region=='bloodridge' else 'flux-edit/raw.png')
 canvas=Image.open(source).convert('RGBA');canvas.paste(Image.open(SOURCES[region]).convert('RGBA'),(2752,0))
 im=canvas.crop((2112,0,3136,768));a=Image.new('L',im.size,255)
 ImageDraw.Draw(a).rectangle((64,0,895,767),fill=0)
 # Fade only the noise-mask edge into the left context, never mirror or copy terrain.
 for x in range(64,96):ImageDraw.Draw(a).line((x,0,x,767),fill=round(255*(95-x)/31))
 for x in range(864,896):ImageDraw.Draw(a).line((x,0,x,767),fill=round(255*(x-864)/31))
 im.putalpha(a);im.save('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-natural-regions-20261019/'+region+'-flux-transition.png')
 g=json.loads((root/'bloodridge/clear-ground/workflow-api.json').read_text())
 g['6']['inputs']['image']='ks-natural-regions-20261019/'+region+'-flux-transition.png'
 g['12']['inputs']['seed']=2026101980+['bloodridge','skullpeak','hunters'].index(region)
 g['9']['inputs']['prompt']='Erase the abrupt vertical join between the two terrain treatments. Make this a single coherent connected pixel-art landscape. Continue the existing irregular stone and ash ground naturally across the middle. Match the same material colours, small pixel detail, worn stone scale and light on both sides. Blend the background rock bank and sky into one continuous scene. Keep the original composition and landmarks, do not add or repeat gates, buildings or pillars. Natural irregular ground, no rectangular patches, repeated floor motifs, straight dividing edges or thick dark outlines. Preserve the outer edges.'
 g['14']['inputs']['filename_prefix']='KS-Natural-Regions-20261019/'+region+'-qwen-transition'
 (p/'workflow-api.json').write_text(json.dumps(g,indent=2))
 r=api('/prompt',{'prompt':g,'client_id':'codex-natural-regions'});(p/'submission.json').write_text(json.dumps(r));print(region,r,flush=True)
 jobs.append((region,base,p,canvas,r))
for region,base,p,canvas,r in jobs:
 for i in range(720):
  h=api('/history/'+r['prompt_id']).get(r['prompt_id'])
  if h:
   (p/'history.json').write_text(json.dumps(h))
   ims=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
   if ims:
    v=ims[0];shutil.copyfile(Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')/v.get('subfolder','')/v['filename'],p/'raw.png')
    patch=Image.open(p/'raw.png').convert('RGBA');canvas.paste(patch.crop((64,0,896,768)),(2176,0));canvas.save(base/'candidate-qwen.png')
    print('Saved '+region,flush=True);break
   if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h['status'])
  time.sleep(5)

