from pathlib import Path
import json,sys,time,shutil
from PIL import Image,ImageDraw
sys.path.insert(0,'Art/Studies/Connected-Regions-20261018')
from outpaint import api,SOURCES
root=Path('Art/Studies/Connected-Regions-Natural-20261019')
region='bloodridge';base=root/region
p=base/'court-transition';p.mkdir(exist_ok=True)
canvas=Image.open(base/'clear-ground/raw.png').convert('RGBA')
canvas.paste(Image.open(SOURCES[region]).convert('RGBA'),(2752,0))
left=2112;right=3136;im=canvas.crop((left,0,right,768))
a=Image.new('L',im.size,255);ImageDraw.Draw(a).rectangle((192,0,639,767),fill=0);im.putalpha(a)
inp=Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-natural-regions-20261019')
im.save(inp/'bloodridge-transition.png')
g=json.loads((base/'clear-ground/workflow-api.json').read_text())
g['6']['inputs']['image']='ks-natural-regions-20261019/bloodridge-transition.png'
g['9']['inputs']['prompt']='Remove the vertical seam in the ground near the right third. Make this one uninterrupted continuous ancient stone courtyard. Replace the flat outlined stone paving in the middle with organically worn rounded limestone slabs with chipped edges, subtle small cracks, ash in the gaps, and mottled low-contrast pixel details matching the existing original ground on the RIGHT. Keep the ground level and the rear wall connected. Match stone scale, colours, lighting and fine native pixel art rendering across the boundary. No large dark outlines, repeated motifs, objects or new architecture. Keep both outer edges unchanged.'
g['12']['inputs']['seed']=2026101971
g['14']['inputs']['filename_prefix']='KS-Natural-Regions-20261019/bloodridge-transition'
(p/'workflow-api.json').write_text(json.dumps(g,indent=2))
q=api('/queue')
if q['queue_running'] or q['queue_pending']:raise RuntimeError('Queue occupied; untouched')
r=api('/prompt',{'prompt':g,'client_id':'codex-natural-regions'});(p/'submission.json').write_text(json.dumps(r));print(r,flush=True)
for i in range(720):
 h=api('/history/'+r['prompt_id']).get(r['prompt_id'])
 if h:
  (p/'history.json').write_text(json.dumps(h))
  ims=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
  if ims:
   v=ims[0];shutil.copyfile(Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')/v.get('subfolder','')/v['filename'],p/'raw.png')
   patch=Image.open(p/'raw.png').convert('RGBA')
   canvas.paste(patch.crop((192,0,640,768)),(left+192,0));canvas.save(base/'candidate.png')
   print('Saved',flush=True);break
  if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h['status'])
 time.sleep(5)
