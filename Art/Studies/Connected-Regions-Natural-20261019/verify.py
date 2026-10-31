from pathlib import Path
from PIL import Image
import json,sys
root=Path(__file__).resolve().parent
repo=root.parents[2];old=repo/'Art/Studies/Connected-Regions-20261018'
sys.path.insert(0,str(old));from outpaint import SOURCES
results=[]
for region in ['bloodridge','skullpeak','hunters']:
 p=root/region
 source=Image.open(SOURCES[region]).convert('RGBA')
 previous=Image.open(old/region/'panorama.png').convert('RGBA').crop((2752,0,4128,768))
 assert source.tobytes()==previous.tobytes(),region+' original source changed'
 if not (p/'panorama.png').exists():continue
 panorama=Image.open(p/'panorama.png').convert('RGBA')
 assert panorama.size==(4128,768)
 assert panorama.crop((3008,0,4128,768)).tobytes()==source.crop((256,0,1376,768)).tobytes()
 layers=[Image.open(p/(n+'.png')).convert('RGBA') for n in ['ground','rear','foreground']]
 composed=Image.alpha_composite(Image.alpha_composite(layers[0],layers[1]),layers[2])
 assert composed.tobytes()==panorama.tobytes()
 export=p/'master-export.png'
 if export.exists():assert Image.open(export).convert('RGBA').tobytes()==panorama.tobytes()
 results.append({'region':region,'native_dimensions':True,'original_source_matches_prior_review':True,'court_core_pixel_exact':True,'static_recomposition_exact':True,'reopened_master_export_exact':export.exists()})
(root/'verification.json').write_text(json.dumps(results,indent=2));print(json.dumps(results,indent=2))
