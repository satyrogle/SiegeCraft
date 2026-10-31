from PIL import Image, ImageChops
from pathlib import Path
import json, hashlib
d=Path(__file__).parent
root=Path('C:/Users/jacob/KindredSiege-Clean')
specs=[('clarth','Assets/Resources/ClarthStudy/clarth_warden_se_idle.png',(164,188),(230,209)),('bloodridge','Assets/Resources/ClarthStudy/blood_rot_warden_sw_idle.png',(172,184),(410,201))]
atlas=json.loads((d/'review-sheet.json').read_text())
sheet=Image.open(d/'review-sheet.png').convert('RGBA')
frames=[sheet.crop((f['frame']['x'],f['frame']['y'],f['frame']['x']+632,f['frame']['y']+424)) for f in atlas['frames']]
results={}
for name,rel,(w,h),(x,y) in specs:
    path=root/rel
    src=Image.open(path).convert('RGBA').crop((0,0,w,h))
    layer=Image.open(d/(name+'.png')).convert('RGBA').crop((x,y,x+w,y+h))
    expected=list(src.getdata())
    got=list(layer.getdata())
    mismatch=sum(a!=b for a,b in zip(expected,got) if a[3] or b[3])
    visible=[]
    for fr in frames:
        region=list(fr.crop((x,y,x+w,y+h)).getdata())
        visible.append(sum(a!=b for a,b in zip(expected,region) if a[3]==255))
    results[name]={'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'layer_pixel_mismatches':mismatch,'visible_pixel_mismatches_each_frame':visible}
    assert mismatch==0 and max(visible)==0
assert len(frames)==32 and all(f['duration']==125 for f in atlas['frames'])
assert all(fr.getchannel('A').getextrema()==(255,255) for fr in frames)
diffs=[]
for i in range(32):
    diff=ImageChops.difference(frames[i],frames[(i+1)%32]).convert('RGB')
    diffs.append(sum(p!=(0,0,0) for p in diff.getdata()))
assert 0<diffs[-1]<=max(diffs[:-1])*1.5
src=Image.open(d/'pixellab-source.png').convert('RGBA')
partition=Image.open(d/'environment.png').convert('RGBA')
assert ImageChops.difference(src,partition).getbbox() is None
result={'frames':32,'duration_ms':4000,'canvas':[632,424],'all_frames_opaque':True,'fighter_checks':results,'adjacent_changed_pixels':diffs,'seam_changed_pixels':diffs[-1],'source_partition_exact':True,'production_integration':False}
(d/'verification.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result))

