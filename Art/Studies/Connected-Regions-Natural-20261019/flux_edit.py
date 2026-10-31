from pathlib import Path
import sys,json,time,shutil
from PIL import Image,ImageDraw
import numpy as np
sys.path.insert(0,'Art/Studies/Connected-Regions-20261018')
from outpaint import api,SOURCES
root=Path('Art/Studies/Connected-Regions-Natural-20261019')
subjects={
'bloodridge':'ancient burial lands, muted ash-brown limestone and violet-brown shadows, dry gnarled roots, worn pale stone and small burial niches',
'skullpeak':'mountain clan land, cool grey natural slate banks and ash, weathered dark iron, sparse ivory snow only on rear ledges, broken fortification braces',
'hunters':'alien hunter wastes, ash-grey basalt, dusty mauve ground, low craggy rock banks and buried starship wreck fragments, a few dark metal ribs and dull violet mineral seams'}
for region in sys.argv[1:]:
 p=root/region/'flux-edit';p.mkdir(parents=True,exist_ok=True)
 receipt=p/'submission.json'
 if receipt.exists():r=json.loads(receipt.read_text())
 else:
  q=api('/queue')
  if q['queue_running'] or q['queue_pending']:raise RuntimeError('Queue occupied; untouched')
  court=Image.open(SOURCES[region]).convert('RGB')
  color=tuple(np.median(np.array(court)[420:620,400:1000],axis=(0,1)).astype(int))
  guide=Image.new('RGBA',(4128,768),(*color,0));guide.paste(court,(2752,0))
  inp=Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-natural-regions-20261019')
  guide.save(inp/(region+'-flux-guide.png'));court.save(inp/(region+'-court.png'))
  g=json.loads(Path('Art/Studies/Bloodridge-Champion-Flux-20261014/workflow-2026101454.json').read_text())
  g['2']['inputs']['strength_model']=0.7
  g['7']['inputs']['image']='ks-natural-regions-20261019/'+region+'-court.png'
  g.pop('8');g['9']['inputs']['pixels']=['7',0]
  prompt='''pixel art environment, crisp fine pixel clusters, subdued limited palette. Extend the reference scene horizontally to the LEFT into ONE very wide continuous battle landscape. The supplied original court fills the RIGHT THIRD of the new canvas. Preserve it there. The new LEFT TWO THIRDS is a natural approach in the same region: '''+subjects[region]+'''. A low sparse bank at far left becomes gradually more enclosed toward the right court. Unique irregular landmarks along the upper quarter, no second court. An entirely empty flat fighting floor occupies the lower two thirds. All buildings, walls, rubble, ledges and large stones are confined above y=250. The lower area contains ONLY shallow ground texture, same oblique projection and ground height as the reference. Quiet weathered earth with irregular embedded stone slabs and shallow natural fissures, varying sizes and spacing. The paving gradually becomes more complete near the original court on the right. Continue the floor material across the boundary naturally. Every part of the landscape is unique, no repeating texture patches or mirrored scenery. One coherent uninterrupted scene with continuous ground and rear banks, no panels, walls crossing the floor, floating islands, foreground pillars, people or UI. Match the original art's small native pixel details, organic chipped rock shapes and restrained ash colours.'''
  g['5']['inputs']['text']=prompt
  g['15']['inputs'].update(steps=20,width=4128,height=768)
  g['16']['inputs']['noise_seed']=2026101940+list(subjects).index(region)
  g['20']={'class_type':'LoadImage','inputs':{'image':'ks-natural-regions-20261019/'+region+'-flux-guide.png'}}
  g['21']={'class_type':'VAEEncode','inputs':{'pixels':['20',0],'vae':['4',0]}}
  g['22']={'class_type':'SetLatentNoiseMask','inputs':{'samples':['21',0],'mask':['20',1]}}
  g['17']['inputs']['latent_image']=['22',0]
  g.pop('12')
  g['19']['inputs']['filename_prefix']='KS-Natural-Regions-20261019/'+region+'-flux'
  (p/'workflow-api.json').write_text(json.dumps(g,indent=2));(p/'prompt.txt').write_text(prompt)
  r=api('/prompt',{'prompt':g,'client_id':'codex-natural-regions'});receipt.write_text(json.dumps(r))
 print(region,r,flush=True)
 for i in range(720):
  h=api('/history/'+r['prompt_id']).get(r['prompt_id'])
  if h:
   (p/'history.json').write_text(json.dumps(h))
   ims=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
   if ims:
    v=ims[0];shutil.copyfile(Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')/v.get('subfolder','')/v['filename'],p/'raw.png');print('Saved '+region,flush=True);break
   if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h['status'])
  time.sleep(5)
