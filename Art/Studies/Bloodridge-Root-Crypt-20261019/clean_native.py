"""Native cleanup inputs; neither background nor fighter pixels are enlarged."""
from pathlib import Path
import json
import cv2,numpy as np
from PIL import Image,ImageFilter

ROOT=Path(__file__).resolve().parent

def run():
    p=ROOT/'crypt'
    raw=np.array(Image.open(p/'raw.png').convert('RGB'));base=np.array(Image.open(p/'source.png').convert('RGB'))
    active=np.array(Image.open(p/'edit-mask.png'))>0
    light=(raw[:,:,0]>165)&(raw[:,:,1]>140)&(raw[:,:,2]>115)&active
    _,labels=cv2.connectedComponents(light.astype('uint8'),8)
    sky=np.isin(labels,list(set(labels[0,:])-set([0])))
    work=raw.copy();work[sky]=base[sky]
    dark=(work.mean(axis=2)<40)&active
    inside=cv2.distanceTransform(dark.astype('uint8'),cv2.DIST_L2,3)
    ink=dark&(inside<3.9)
    work=cv2.inpaint(work,ink.astype('uint8')*255,3,cv2.INPAINT_TELEA)
    detail=base.astype(float)-np.array(Image.fromarray(base).filter(ImageFilter.GaussianBlur(2))).astype(float)
    stone=active&(~sky)&(work.mean(axis=2)>56)
    work=np.clip(work.astype(float)+np.clip(detail,-5,5)*stone[:,:,None]*.6,0,255).astype('uint8')
    blend=Image.open(p/'edit-mask.png').filter(ImageFilter.MinFilter(7)).filter(ImageFilter.GaussianBlur(5))
    cleaned=Image.fromarray(base);cleaned.paste(Image.fromarray(work),(0,0),blend);cleaned.save(p/'cleaned.png')
    # Qwen duplicated its ash bird three times. Retain the first; restore original
    # pixels at the other two, rather than generating or cloning replacement fauna.
    p=ROOT/'outskirts';image=np.array(Image.open(p/'edited.png').convert('RGB'))
    old=np.array(Image.open(p/'source.png').convert('RGB'))
    birds=Image.new('L',(image.shape[1],image.shape[0]))
    from PIL import ImageDraw
    ImageDraw.Draw(birds).rectangle((103,235,189,290),fill=255)
    birds=birds.filter(ImageFilter.GaussianBlur(2))
    outskirts=Image.fromarray(image);outskirts.paste(Image.fromarray(old),(0,0),birds)
    outskirts.save(p/'cleaned.png')
    (ROOT/'cleanup.json').write_text(json.dumps({'removed_heavy_ink_pixels':int(ink.sum()),'restored_connected_rear_pixels':int(sky.sum()),'duplicate_bird_pixels_restored':int((np.array(birds)>0).sum()),'native_scale':1},indent=2))

if __name__=='__main__':run()
