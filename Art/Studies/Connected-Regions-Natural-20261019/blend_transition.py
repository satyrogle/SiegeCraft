"""Match lighting at the boundary of a newly generated transition, at native size.
No original floor patches are tiled, reflected or quilted. Sources remain untouched.
"""
from pathlib import Path
import cv2,numpy as np,sys
from PIL import Image
root=Path(__file__).resolve().parent
sys.path.insert(0,str(root.parents[2]/'Art/Studies/Connected-Regions-20261018'))
from outpaint import SOURCES
for region in sys.argv[1:]:
 p=root/region
 base=Image.open(p/('clear-ground/raw.png' if region=='bloodridge' else 'flux-edit/raw.png')).convert('RGB')
 base.paste(Image.open(SOURCES[region]).convert('RGB'),(2752,0))
 source=Image.open(p/'qwen-transition/raw.png').convert('RGB')
 dst=cv2.cvtColor(np.array(base),cv2.COLOR_RGB2BGR);src=cv2.cvtColor(np.array(source),cv2.COLOR_RGB2BGR)
 mask=np.zeros((768,1024),np.uint8);mask[1:767,64:896]=255
 x,y,w,h=cv2.boundingRect(mask)
 result=cv2.seamlessClone(src,dst,mask,(2112+x+w//2,y+h//2),cv2.NORMAL_CLONE)
 if region=='hunters':
  # The model introduced a straight colour step at native x=2965 inside the edit.
  # Correct the low-frequency jump on the generated side only; no texture copying.
  edge=2965;start=2565
  work=result.astype(np.float32)
  jump=np.median(work[:,edge:edge+4],axis=1)-np.median(work[:,edge-4:edge],axis=1)
  jump=cv2.GaussianBlur(jump[:,None,:],(1,41),12)[:,0,:]
  t=np.linspace(0,1,edge-start,dtype=np.float32);weight=t*t*(3-2*t)
  work[:,start:edge]+=jump[:,None,:]*weight[None,:,None]
  result=np.clip(work,0,255).astype(np.uint8)
 assert np.array_equal(result[:,3008:],dst[:,3008:])
 Image.fromarray(cv2.cvtColor(result,cv2.COLOR_BGR2RGB)).save(p/'candidate-final.png')
 print(region,'native transition colour continuity; court core unchanged')
