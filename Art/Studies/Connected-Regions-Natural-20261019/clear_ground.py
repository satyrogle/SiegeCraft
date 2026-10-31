from pathlib import Path
import json,sys,time,shutil
from PIL import Image,ImageDraw
sys.path.insert(0,'Art/Studies/Connected-Regions-20261018')
from outpaint import api
root=Path('Art/Studies/Connected-Regions-Natural-20261019/bloodridge')
p=root/'clear-ground';p.mkdir(exist_ok=True)
g=json.loads((root/'workflow-api.json').read_text())
im=Image.open(root/'flux-edit/raw.png').convert('RGBA');a=Image.new('L',im.size,255)
ImageDraw.Draw(a).rectangle((0,285,2751,767),fill=0);im.putalpha(a)
im.save('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-natural-regions-20261019/bloodridge-clear.png')
g['6']['inputs']['image']='ks-natural-regions-20261019/bloodridge-clear.png'
for n in ['9','10']:g[n]['inputs'].pop('image2',None)
g.pop('8')
g['9']['inputs']['prompt']='Remove the large square stepped temple that blocks the foreground near the middle-right. Remove the raised boulders in the lower left. Replace these two obstructions with flat walkable worn limestone paving and quiet dusty ground, continuing the surrounding floor naturally. Keep the ground all on one level, with small unique irregular stone slabs and subtle cracks. No new buildings, walls, pits or obstacles in the lower two thirds. Keep the distant rear ruins and original court on the right completely unchanged. Same small crisp pixel art details, organic stone shapes, subdued brown and violet palette. One seamless continuous ground plane.'
g['12']['inputs']['seed']=2026101951
g['14']['inputs']['filename_prefix']='KS-Natural-Regions-20261019/bloodridge-clear'
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
