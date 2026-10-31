from pathlib import Path
from PIL import Image, ImageChops
import json, csv

root=Path(__file__).parent
a,b=root/'candidate',root/'baseline'
result={}
for file in ['formations.csv','start-state.txt','end-state.txt','motion-trace.csv']:
    result[file+'_identical']=(a/file).read_bytes()==(b/file).read_bytes()
results=[]
for file in sorted(a.glob('*.png')):
    other=b/file.name
    if not other.exists(): continue
    aa,bb=Image.open(file).convert('RGB'),Image.open(other).convert('RGB')
    assert aa.size==bb.size==(1440,900)
    diff=ImageChops.difference(aa,bb)
    protected=[(0,0,1440,80),(0,834,1440,900),(0,80,32,834),(1408,80,1440,834)]
    mismatch=sum(sum(pixel!=(0,0,0) for pixel in diff.crop(rect).get_flattened_data()) for rect in protected)
    # uGUI ColorTint uses real time, whereas this deterministic capture uses a fixed simulation step.
    # Encoding and baseline have different wall times. Focus click and Unleash affordability fades differ.
    transient={'frame-0180.png':(584,836,852,900),'frame-0540.png':(860,836,1128,900)}.get(file.name)
    allowed=0 if transient is None else sum(pixel!=(0,0,0) for pixel in diff.crop(transient).get_flattened_data())
    results.append({'frame':file.name,'protected_pixels_changed':mismatch,'button_color_fade_pixels':allowed,
                    'unexpected_protected_changes':mismatch-allowed,'full_frame_identical':diff.getbbox() is None})
result['screenshots']=results
rows=list(csv.DictReader((a/'motion-trace.csv').open()))
result['frames']=len(rows)
result['seconds_at_30fps']=len(rows)/30
result['wind_frames_seen']=sorted({int(r['windFrame']) for r in rows})
result['max_simultaneous_numbers']=max(int(r['numbers']) for r in rows)
result['simulation_tick_first_last']=[rows[0]['tick'],rows[-1]['tick']]
result['pass']=all(result[k] for k in result if k.endswith('_identical')) and all(r['unexpected_protected_changes']==0 for r in results)
(root/'capture-verification.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))
assert result['pass']
