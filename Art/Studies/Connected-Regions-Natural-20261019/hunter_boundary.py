from pathlib import Path
import numpy as np
from PIL import Image
import sys
root=Path('Art/Studies/Connected-Regions-Natural-20261019')
sys.path.insert(0,'Art/Studies/Connected-Regions-20261018');from outpaint import SOURCES
p=root/'hunters';im=Image.open(p/'candidate-final.png').convert('RGB');a=np.array(im)
ref=np.array(Image.open(SOURCES['hunters']).convert('RGB'))
x0=2752;n=128;offset=64
cost=((a[:,x0+offset:x0+offset+n].astype(float)-ref[:,offset:offset+n])**2).sum(axis=2)
# One material-following boundary between newly drawn terrain and its original
# court at the SAME world coordinates. No court patches copied elsewhere.
h,w=cost.shape;parent=np.zeros((h,w),np.int16)
for y in range(1,h):
 prev=np.pad(cost[y-1],1,constant_values=1e20)
 choices=np.stack([prev[:w],prev[1:w+1],prev[2:w+2]])
 d=choices.argmin(axis=0)-1;parent[y]=np.arange(w)+d
 cost[y]+=choices[d+1,np.arange(w)]
path=np.zeros(h,np.int32);path[-1]=cost[-1].argmin()
for y in range(h-2,-1,-1):path[y]=parent[y+1,path[y+1]]
path+=offset
for y,x in enumerate(path):
 t=np.clip((np.arange(1376)-(x-64))/128,0,1);t=t*t*(3-2*t)
 a[y,x0:]=np.round(a[y,x0:].astype(float)*(1-t[:,None])+ref[y].astype(float)*t[:,None]).astype(np.uint8)
Image.fromarray(a).save(p/'candidate-final.png')
print('Court boundary extent',int(path.min()),int(path.max()),'unique x',len(np.unique(path)))
