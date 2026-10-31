from pathlib import Path
import sys,json,time,shutil
from PIL import Image,ImageDraw
sys.path.insert(0,'Art/Studies/Connected-Regions-20261018')
from outpaint import api,SOURCES
root=Path('Art/Studies/Connected-Regions-Natural-20261019')
for region in sys.argv[1:]:
 base=root/region;p=base/'flux-transition';p.mkdir(exist_ok=True)
 source=base/('clear-ground/raw.png' if region=='bloodridge' else 'flux-edit/raw.png')
 canvas=Image.open(source).convert('RGBA');canvas.paste(Image.open(SOURCES[region]).convert('RGBA'),(2752,0))
 im=canvas.crop((2112,0,3136,768));a=Image.new('L',im.size,255)
 ImageDraw.Draw(a).rectangle((64,0,895,767),fill=0)
 # Fade only the noise-mask edge into the left context, never mirror or copy terrain.
 for x in range(64,96):ImageDraw.Draw(a).line((x,0,x,767),fill=round(255*(95-x)/31))
 for x in range(864,896):ImageDraw.Draw(a).line((x,0,x,767),fill=round(255*(x-864)/31))
 im.putalpha(a);im.save('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-natural-regions-20261019/'+region+'-flux-transition.png')
 g=json.loads((base/'flux-edit/workflow-api.json').read_text())
 g['20']['inputs']['image']='ks-natural-regions-20261019/'+region+'-flux-transition.png'
 g['15']['inputs'].update(width=1024,height=768,steps=20)
 g['16']['inputs']['noise_seed']=2026101980+['bloodridge','skullpeak','hunters'].index(region)
 g['5']['inputs']['text']='pixel art environment, small crisp native pixel clusters. Repair the transition between the existing landscape on the left and the supplied original court on the right, making one natural connected landscape. One continuous horizontal ground plane: same material, pixel detail, stone scale and light across the entire image. Organic irregular worn slabs partially buried in ash, small chips, restrained low contrast weathering, a few shallow irregular cracks. Match exactly the native pixel rendering and palette of the reference. Smoothly connect the darker ground on the left to the original court ground on the right with natural irregular material changes. No straight dividing line, repeated stone pattern, thick black outlines or isolated obstructions. Naturally continue the original court edge rocks and near-edge framing into the new terrain; merge the rear wall and distant sky across the former seam. Keep the main court entrance and its identity unchanged, retain the existing landscape composition. The fighting ground stays open and level.'
 g['19']['inputs']['filename_prefix']='KS-Natural-Regions-20261019/'+region+'-flux-transition'
 (p/'workflow-api.json').write_text(json.dumps(g,indent=2))
 q=api('/queue')
 if q['queue_running'] or q['queue_pending']:raise RuntimeError('Queue occupied; untouched')
 r=api('/prompt',{'prompt':g,'client_id':'codex-natural-regions'});(p/'submission.json').write_text(json.dumps(r));print(region,r,flush=True)
 for i in range(720):
  h=api('/history/'+r['prompt_id']).get(r['prompt_id'])
  if h:
   (p/'history.json').write_text(json.dumps(h))
   ims=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
   if ims:
    v=ims[0];shutil.copyfile(Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')/v.get('subfolder','')/v['filename'],p/'raw.png')
    patch=Image.open(p/'raw.png').convert('RGBA');canvas.paste(patch.crop((64,0,896,768)),(2176,0));canvas.save(base/'candidate-flux.png')
    print('Saved '+region,flush=True);break
   if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h['status'])
  time.sleep(5)

