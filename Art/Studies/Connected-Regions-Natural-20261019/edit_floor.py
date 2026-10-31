from pathlib import Path
import json,sys,time,shutil
from PIL import Image,ImageDraw
sys.path.insert(0,'Art/Studies/Connected-Regions-20261018')
from outpaint import api
root=Path('Art/Studies/Connected-Regions-Natural-20261019/bloodridge')
p=root/'floor-edit';p.mkdir(exist_ok=True)
g=json.loads((root/'workflow-api.json').read_text())
im=Image.open(root/'raw.png').convert('RGBA');a=Image.new('L',im.size,255)
ImageDraw.Draw(a).rectangle((0,260,2751,767),fill=0);im.putalpha(a)
im.save('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-natural-regions-20261019/bloodridge-floor.png')
g['6']['inputs']['image']='ks-natural-regions-20261019/bloodridge-floor.png'
g['9']['inputs']['prompt']='Replace the large plain brown area across the bottom left two thirds with a detailed ancient stone courtyard FLOOR. Extend the irregular worn paving of image 2 naturally into this empty area. Every stone slab is a different organic shape and size. Smooth chipped pale limestone slabs, fine dark cracks, ash-filled gaps, a few little buried stones. Flat walkable horizontal ground seen obliquely from above, continuous with the ground on the right. The leftmost floor slowly becomes bare ash with isolated embedded stones. Match the exact pixel art style, palette, small pixel detail and light direction of the original image. Keep all background architecture and the right third unchanged. No repeating patches, mirrored patterns, tiles, walls, people, text or UI.'
g['12']['inputs']['seed']=2026101931
g['14']['inputs']['filename_prefix']='KS-Natural-Regions-20261019/bloodridge-floor'
(p/'workflow-api.json').write_text(json.dumps(g,indent=2))
r=api('/prompt',{'prompt':g,'client_id':'codex-natural-regions'});(p/'submission.json').write_text(json.dumps(r));print(r,flush=True)
for i in range(720):
 h=api('/history/'+r['prompt_id']).get(r['prompt_id'])
 if h:
  (p/'history.json').write_text(json.dumps(h))
  ims=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
  if ims:
   v=ims[0];shutil.copyfile(Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')/v.get('subfolder','')/v['filename'],p/'raw.png');print('Saved',flush=True);break
  if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h['status'])
 time.sleep(5)
