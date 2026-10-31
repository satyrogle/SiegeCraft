from pathlib import Path
from PIL import Image
from collections import deque
import hashlib, json
d=Path(__file__).parent
prev=d.parent/'Bloodridge-Court-Refinement-20261013'
sheet=Image.open(d/'linen-sheet.png').convert('RGBA')
counts=[]
for frame in range(32):
    im=sheet.crop(((frame%8)*226,(frame//8)*562,(frame%8+1)*226,(frame//8+1)*562))
    p=im.load()
    remaining={(x,y) for y in range(140,470) for x in range(160,226) if p[x,y][3]}
    components=[]
    while remaining:
        seed=remaining.pop();q=deque([seed]);size=0;max_y=0
        while q:
            x,y=q.popleft();size+=1;max_y=max(max_y,y)
            for dx,dy in [(1,0),(-1,0),(0,1),(0,-1),(1,1),(-1,-1),(1,-1),(-1,1)]:
                point=(x+dx,y+dy)
                if point in remaining:remaining.remove(point);q.append(point)
        if size>12:components.append(size)
        else:assert max_y<=157,('detached edited cloth pixels',frame+1,size,max_y)
    counts.append(len(components))
    assert len(components)==2,(frame+1,components)
original_hashes=json.loads((d/'protected-input-hashes.json').read_text(encoding='utf-8-sig'))
for row in original_hashes:
    assert hashlib.sha256(Path(row['Path']).read_bytes()).hexdigest().upper()==row['Hash']
fixed=['rear-burial-architecture.png','rooted-entrance.png','contact-shadows.png','foreground.png','foreground-bronze-marker.png','foreground-root-rim.png','ks-bloodridge-court-restrained-ash-review-1.png','ks-bloodridge-court-restrained-ash-review-2.png']
for file in fixed:assert (d/file).read_bytes()==(prev/file).read_bytes(),file
floor=Image.open(d/'ground-underpaint.png').convert('RGBA')
palette=set(Image.open(prev/'ground-underpaint.png').convert('RGBA').get_flattened_data())
used=set(floor.get_flattened_data())
assert used.issubset(palette)
result={'status':'PASS','scope':'All-frame linen connectivity and preserved source files; not subjective art approval',
        'significant_cloth_components_per_frame':counts,'diagonal_connectivity_allowed':True,
        'no_detached_pixels_below_original_ties':True,
        'all_protected_input_hashes_unchanged':True,'unchanged_scenery_files':fixed,
        'floor_colours_all_from_prior_floor':True,'floor_colour_count':len(used)}
(d/'scope-verification.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))
