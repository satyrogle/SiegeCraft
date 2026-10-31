from pathlib import Path
from PIL import Image, ImageChops
import json

root=Path(__file__).parent
reference=Image.open(root/'assets/fight.png').convert('RGB')
bounds=(180,280,1268,750)
checks={}
for name in ['A','B','C']:
    image=Image.open(root/f'layout-{name}.png').convert('RGB')
    unchanged=ImageChops.difference(reference.crop(bounds),image.crop(bounds)).getbbox() is None
    checks[name]={'size':image.size,'fighterAreaUnchanged':unchanged}
    assert unchanged and image.size==(1440,900)
data={'source':'Bloodridge-Moving-Fight-20261013/candidate/frame-0180.png',
      'protectedRectangle':bounds,'protectedPixelsPerLayout':1088*470,'checks':checks,
      'review':'Native exports and comparison board visually inspected. Browser compare, enlargement and native-size controls checked. No console warnings or errors.',
      'limits':'Static layout sketches. Existing combat overlays retained. Not implemented or approved UI art.'}
(root/'audit.json').write_text(json.dumps(data,indent=2))
print(json.dumps(checks))
