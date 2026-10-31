"""Reveal the original court paving in its original position beneath an irregular ash edge."""
from pathlib import Path
import numpy as np
from PIL import Image
from revise import ROOT

def run():
    im=Image.open(ROOT/'ground-clearance/composition.png').convert('RGB')
    court=Image.open('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/comparison/bloodridge-reference.png').convert('RGB')
    # This is the court's own left approach margin, at its existing world coordinates.
    # Never repeat, mirror, enlarge or shift a floor patch into the new terrain.
    source=np.array(court.crop((0,0,256,768))).astype(float)
    y,x=np.indices((768,256));world_x=x+2752
    edge=2860+29*np.sin(y*.027)+17*np.sin(y*.061)-(y-480)*.055
    tone=source.mean(axis=2)
    material=np.clip((tone-105)*.3,-11,11)
    alpha=np.clip((world_x-edge+material)/24,0,1)
    alpha*=np.clip((y-300)/65,0,1)
    # Preserve the existing backdrop. Complete the source floor before the hard core.
    mask=Image.fromarray(np.uint8(alpha*255))
    im.paste(Image.fromarray(np.uint8(source)),(2752,0),mask)
    im.save(ROOT/'ground-clearance/final-composition.png')
    im.crop((2640,320,3320,768)).save(ROOT/'ground-clearance/final-join-review.png')

if __name__=='__main__':run()
