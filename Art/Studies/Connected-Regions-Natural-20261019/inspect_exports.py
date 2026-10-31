from pathlib import Path
from PIL import Image
import numpy as np,json,sys
root=Path(__file__).resolve().parent
for region in sys.argv[1:]:
 p=root/region
 im=Image.open(p/'panorama.png').convert('RGBA')
 im.crop((2064,0,3440,768)).save(p/'court-join.png')
 im.crop((688,0,2064,768)).save(p/'middle-continuity.png')
 a=np.array(im.convert('RGB')).astype(float)
 edge=np.abs(a[:,2752]-a[:,2751]).mean()
 neighbors=np.abs(a[:,2601:2900]-a[:,2600:2899]).mean(axis=(0,2))
 result={'court_join_mean_channel_step':round(float(edge),2),'nearby_edge_95th_percentile':round(float(np.percentile(neighbors,95)),2),'note':'Diagnostic only, not visual approval.'}
 (p/'join-diagnostic.json').write_text(json.dumps(result,indent=2))
 print(region,result)
