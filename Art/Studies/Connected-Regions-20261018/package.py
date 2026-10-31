"""Native-size background review exports. Original courts are never quantized again."""
from PIL import Image,ImageDraw,ImageChops
import numpy as np
import json,pathlib,sys,hashlib,shutil
ROOT=pathlib.Path(__file__).resolve().parent
REPO=ROOT.parents[2]
sys.path.insert(0,str(ROOT))
from outpaint import SOURCES
def seam(a,b):
    """Minimum-error native-pixel boundary, top to bottom; no alpha blur."""
    cost=((a.astype(np.int32)-b.astype(np.int32))**2).sum(axis=2).astype(np.float64)
    h,w=cost.shape;parent=np.zeros((h,w),dtype=np.int16)
    for y in range(1,h):
        previous=np.pad(cost[y-1],1,constant_values=1e15)
        options=np.stack([previous[0:w],previous[1:w+1],previous[2:w+2]])
        direction=np.argmin(options,axis=0)-1
        parent[y]=np.arange(w)+direction
        cost[y]+=options[direction+1,np.arange(w)]
    path=np.empty(h,dtype=np.int32);path[-1]=int(np.argmin(cost[-1]))
    for y in range(h-2,-1,-1):path[y]=parent[y+1,path[y+1]]
    return path
def stitch_floor(extension,court):
    """Quilt the court's native central floor, removing the guide's rectangular cuts.
    Generated rear scenery stays; the last court is untouched. All patches are 1x.
    """
    a=np.array(extension.convert('RGB'));ref=np.array(court.convert('RGB'))
    start=280;h=768-start;w=a.shape[1];overlap=128;patch_width=512
    floor=np.zeros((h,w,3),dtype=np.uint8)
    offsets=[320,493,377,541,425,302,510,351]
    x=0;index=0
    while x<w:
        source_x=offsets[index%len(offsets)]
        patch=ref[start:,source_x:source_x+patch_width]
        n=min(patch_width,w-x);patch=patch[:,:n]
        if x==0:floor[:,:n]=patch
        else:
            shared=min(overlap,n);path=seam(floor[:,x:x+shared],patch[:,:shared])
            for y in range(h):floor[y,x+path[y]:x+n]=patch[y,path[y]:]
        x+=patch_width-overlap;index+=1
    # Continue the final court's edge into the extension without changing that court.
    tail=ref[start:,:64][:,::-1].copy();x=w-64
    path=seam(floor[:,x:x+48],tail[:,:48])
    for y in range(h):floor[y,x+path[y]:]=tail[y,path[y]:]
    # An irregular horizontal pixel seam joins the new floor to generated rear scenery.
    band=40;path=seam(a[start:start+band].transpose(1,0,2),floor[:band].transpose(1,0,2))
    for x,y in enumerate(path):a[start+y:,x]=floor[y:,x]
    return Image.fromarray(a)
def connect_edges(panorama,extension_width):
    # Match each join to the next section's native edge. The irregular minimum-error
    # join stays on the extension side; the final reference court is never changed.
    a=np.array(panorama.convert('RGB'))
    for boundary in range(extension_width,0,-1376):
        patch=a[:,boundary:boundary+64][:,::-1].copy()
        x=boundary-64;path=seam(a[:,x:x+48],patch[:,:48])
        for y in range(a.shape[0]):a[y,x+path[y]:boundary]=patch[y,path[y]:]
    return Image.fromarray(a).convert('RGBA')
def package(region,step=2,revision=''):
    folder=ROOT/region
    raw=Image.open(folder/f'extend-{step}{revision}'/'joined.png').convert('RGB')
    court=Image.open(SOURCES[region]).convert('RGB')
    # One reference-derived palette across all new pixels; no resampling or dithering.
    palette=court.quantize(colors=32,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE)
    extension=raw.crop((0,0,raw.width-1376,768)).quantize(palette=palette,dither=Image.Dither.NONE)
    arr=np.array(extension);colors=np.array(palette.getpalette()[:96],dtype=np.int32).reshape(32,3)
    distances=((colors[:,None,:]-colors[None,:,:])**2).sum(axis=2)
    height,width=arr.shape;changes=0
    for _ in range(3):
        padded=np.pad(arr,1,mode='edge')
        neighbors=[padded[y:y+height,x:x+width] for y in range(3) for x in range(3)]
        same=sum((v==arr).astype(np.uint8) for v in neighbors)
        best_count=np.zeros_like(arr);best=arr.copy()
        for color in range(32):
            counts=sum((v==color).astype(np.uint8) for v in neighbors)
            mask=counts>best_count;best[mask]=color;best_count[mask]=counts[mask]
        replace=(same<=3)&(best_count>=4)&(distances[arr,best]<2100)
        changes+=int(replace.sum());arr[replace]=best[replace]
    cleaned=Image.fromarray(arr);cleaned.putpalette(palette.getpalette())
    if revision:cleaned=stitch_floor(cleaned,court)
    panorama=Image.new('RGBA',raw.size);panorama.paste(cleaned.convert('RGBA'),(0,0));panorama.paste(court,(width,0))
    panorama=connect_edges(panorama,width)
    panorama.save(folder/'panorama.png')
    assert panorama.crop((width,0,width+1376,768)).convert('RGB').tobytes()==court.tobytes()
    # Static rear plane and fighting floor, with the existing court's exact foreground mask.
    rear_mask=Image.new('L',raw.size);ImageDraw.Draw(rear_mask).rectangle((0,0,raw.width,285),fill=255)
    front_mask=Image.new('L',raw.size)
    if region!='bloodridge':
        previous=REPO/'Assets/StreamingAssets/CampaignArt/RegionalReview'/region
        rear_mask.paste(Image.open(previous/'rear.png').getchannel('A'),(width,0))
        front_mask.paste(Image.open(previous/'foreground.png').getchannel('A'),(width,0))
    else:
        # Original foreground is a full-screen overlay; crop at the native stage origin.
        original=Image.open(REPO/'Assets/StreamingAssets/CampaignArt/Bloodridge/foreground.png').convert('RGBA').crop((32,66,1408,834))
        front_mask.paste(original.getchannel('A'),(width,0))
    rear=panorama.copy();rear.putalpha(rear_mask)
    front=panorama.copy();front.putalpha(front_mask)
    ground=panorama.copy();under=Image.new('RGBA',raw.size,panorama.getpixel((800,550)))
    ground.paste(under,(0,0),ImageChops.lighter(rear_mask,front_mask))
    # Soft alpha in the original overlay would double-blend; masks are hard pixel selections.
    front_mask=front_mask.point(lambda p:255 if p else 0);front.putalpha(front_mask)
    ground=panorama.copy();ground.paste(under,(0,0),ImageChops.lighter(rear_mask,front_mask))
    for name,im in [('ground',ground),('rear',rear),('foreground',front)]:im.save(folder/(name+'.png'))
    recomposed=Image.alpha_composite(Image.alpha_composite(ground,rear),front)
    assert recomposed.tobytes()==panorama.tobytes()
    stops=[round(width*i/3) for i in range(4)]
    for i,x in enumerate(stops):panorama.crop((x,0,x+1376,768)).save(folder/f'view-{i+1}.png')
    metadata={'region':region,'source':f'extend-{step}{revision}/joined.png','dimensions':raw.size,'viewport':[1376,768],'review_viewports':stops,'final_court_x':width,'final_court_pixels_preserved':True,'scale':1,'resampled':False,'new_scenery_palette_colors':32,'native_floor_quilting':bool(revision),'majority_replacements':changes,'static_recomposition_matches':True,'sha256':hashlib.sha256(panorama.tobytes()).hexdigest(),'status':'Art review. No runtime binding or new animation. Viewports are not new encounters.'}
    (folder/'layers.json').write_text(json.dumps(metadata,indent=2))
    print(region,raw.size,'court pixels exact, layer recomposition exact',flush=True)
def actors():
    src=REPO/'Assets/Resources/CampaignArt'
    for key,name in [('clarth_warden_sw','fighter'),('bloodridge_champion_sw','champion')]:
        f=json.loads((src/(key+'_idle.json')).read_text())['frames'][0]['frame']
        im=Image.open(src/(key+'_idle.png'))
        im.crop((f['x'],f['y'],f['x']+f['w'],f['y']+f['h'])).save(ROOT/(name+'.png'))
if __name__=='__main__':
    actors()
    for region in sys.argv[1:]:package(region,revision='' if region=='bloodridge' else 'b')
