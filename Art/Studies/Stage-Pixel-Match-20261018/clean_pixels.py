"""Native-resolution palette and low-contrast speckle cleanup, derived from Bloodridge's method.
No resampling, blur or dithering. Raw model output is retained beside the review asset.
"""
from PIL import Image
import numpy as np
import pathlib, json, sys
ROOT=pathlib.Path(__file__).resolve().parent
for name in sys.argv[1:]:
    folder=ROOT/name
    raw=Image.open(folder/(name+'.png')).convert('RGB')
    indexed=raw.quantize(colors=32,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE)
    indexed.save(folder/(name+'-palette.png'))
    palette=np.array(indexed.getpalette()[:96],dtype=np.int32).reshape(32,3)
    arr=np.array(indexed)
    distances=((palette[:,None,:]-palette[None,:,:])**2).sum(axis=2)
    height,width=arr.shape
    majority_changes=0
    for _ in range(3):
        padded=np.pad(arr,1,mode='edge')
        neighbors=[padded[y:y+height,x:x+width] for y in range(3) for x in range(3)]
        same=sum((v==arr).astype(np.uint8) for v in neighbors)
        best_count=np.zeros_like(arr);best=arr.copy()
        for color in range(32):
            counts=sum((v==color).astype(np.uint8) for v in neighbors)
            mask=counts>best_count;best[mask]=color;best_count[mask]=counts[mask]
        replace=(same<=3)&(best_count>=4)&(distances[arr,best]<2100)
        majority_changes+=int(replace.sum());arr[replace]=best[replace]
    flat=arr.ravel();out=flat.copy();seen=np.zeros(flat.size,dtype=bool)
    changed=0
    for start in range(flat.size):
        if seen[start]:continue
        c=int(flat[start]);seen[start]=True;todo=[start];boundary={};head=0
        while head<len(todo):
            p=todo[head];head+=1;y,x=divmod(p,width)
            neighbors=[]
            if x:neighbors.append(p-1)
            if x+1<width:neighbors.append(p+1)
            if y:neighbors.append(p-width)
            if y+1<height:neighbors.append(p+width)
            for q in neighbors:
                nc=int(flat[q])
                if nc==c:
                    if not seen[q]:seen[q]=True;todo.append(q)
                else:boundary[nc]=boundary.get(nc,0)+1
        # Remove only tiny low-contrast islands; protect high-contrast seams and highlights.
        max_area=12 if start//width>height*.36 else 4
        if len(todo)<=max_area:
            choices=[(count/(1+float(distances[c,nc])/120),nc) for nc,count in boundary.items() if distances[c,nc]<1600]
            if choices:
                _,replacement=max(choices)
                out[todo]=replacement;changed+=len(todo)
    cleaned=Image.fromarray(out.reshape(height,width),mode='P')
    cleaned.putpalette(indexed.getpalette())
    cleaned.save(folder/(name+'-review.png'))
    (folder/'cleanup.json').write_text(json.dumps({'source':name+'.png','output':name+'-review.png','dimensions':raw.size,'palette_colors':32,'low_contrast_island_pixels_merged':changed,'majority_replacements_across_three_passes':majority_changes,'resampled':False,'dither':False,'method':'Native palette reduction, three local majority passes and connected-component cleanup, based on the original Bloodridge prepare/controlled-floor pass; generated art, not claimed hand-drawn.'},indent=2))
    print(name,raw.size,'merged',changed,flush=True)
