from pathlib import Path
from PIL import Image
import json,hashlib
d=Path(__file__).parent
prev=d.parent/'Bloodridge-Battle-Screen-20261013'
final=Image.open(d/'battle-screen.png').convert('RGBA')
original=Image.open(prev/'actual-game-before.png').convert('RGBA')
combat=Image.open(prev/'captured-combat.png').convert('RGBA')
hud=Image.open(prev/'captured-hud.png').convert('RGBA')
cp=list(combat.get_flattened_data());hp=list(hud.get_flattened_data())
fp=list(final.get_flattened_data());op=list(original.get_flattened_data())
result={'size':final.size,'combat_pixel_count':sum(c[3]>0 for c in cp),'combat_mismatches':sum(c[3]>0 and f!=o for c,f,o in zip(cp,fp,op)),'hud_mismatches':sum(h[3]>0 and f!=o for h,f,o in zip(hp,fp,op)),'scenery_palette_colors':len(Image.open(d/'cohesive-palette.png').convert('RGB').getcolors(2000000)),'native_source_size':Image.open(d/'cohesive-source.png').size,'native_crop_offset':[73,46],'resampling':False,'production_integration':False}
assert result['combat_mismatches']==0
assert result['hud_mismatches']==0
assert result['scenery_palette_colors']<=64
for name in ['battle-screen.png','bloodridge-battleground.aseprite','cohesive-source.png','ground-source.png']:
 result[name+'_sha256']=hashlib.sha256((d/name).read_bytes()).hexdigest()
(d/'verification.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))
