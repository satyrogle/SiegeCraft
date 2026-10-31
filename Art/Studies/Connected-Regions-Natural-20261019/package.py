"""Native-size background review exports. Original courts are never quantized again."""
from PIL import Image,ImageDraw,ImageChops
import numpy as np
import json,pathlib,sys,hashlib,shutil
ROOT=pathlib.Path(__file__).resolve().parent
REPO=ROOT.parents[2]
sys.path.insert(0,str(ROOT))
sys.path.insert(0,str(REPO/'Art/Studies/Connected-Regions-20261018'))
from outpaint import SOURCES
def package(region):
    folder=ROOT/region
    raw=Image.open(folder/'candidate-final.png').convert('RGB')
    court=Image.open(SOURCES[region]).convert('RGB')
    # One reference-derived palette across all new pixels; no resampling or dithering.
    palette=court.quantize(colors=32,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE)
    extension=raw.crop((0,0,3008,768)).quantize(palette=palette,dither=Image.Dither.NONE)
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
    panorama=Image.new('RGBA',raw.size);panorama.paste(cleaned.convert('RGBA'),(0,0));panorama.paste(court.crop((256,0,1376,768)),(3008,0))
    panorama.save(folder/'panorama.png')
    assert panorama.crop((3008,0,4128,768)).convert('RGB').tobytes()==court.crop((256,0,1376,768)).tobytes()
    # Static rear plane and fighting floor, with the existing court's exact foreground mask.
    rear_mask=Image.new('L',raw.size);ImageDraw.Draw(rear_mask).rectangle((0,0,raw.width,285),fill=255)
    front_mask=Image.new('L',raw.size)
    if region!='bloodridge':
        previous=REPO/'Assets/StreamingAssets/CampaignArt/RegionalReview'/region
        rear_mask.paste(Image.open(previous/'rear.png').getchannel('A'),(2752,0))
        front_mask.paste(Image.open(previous/'foreground.png').getchannel('A'),(2752,0))
    else:
        # Original foreground is a full-screen overlay; crop at the native stage origin.
        original=Image.open(REPO/'Assets/StreamingAssets/CampaignArt/Bloodridge/foreground.png').convert('RGBA').crop((32,66,1408,834))
        front_mask.paste(original.getchannel('A'),(2752,0))
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
    stops=[round(2752*i/3) for i in range(4)]
    for i,x in enumerate(stops):panorama.crop((x,0,x+1376,768)).save(folder/f'view-{i+1}.png')
    metadata={'region':region,'source':'candidate-final.png','dimensions':raw.size,'viewport':[1376,768],'review_viewports':stops,'final_court_x':2752,'protected_court_core_x':3008,'protected_court_core_pixels_preserved':True,'original_court_source_files_unchanged':True,'approach_edge_blended_pixels':256,'scale':1,'resampled':False,'new_scenery_palette_colors':32,'native_floor_quilting':False,'mirrored_joins':False,'method':'Whole-width local Flux terrain redraw, local Qwen transition edit, native gradient-domain boundary colour matching; original court source files and court core preserved','majority_replacements':changes,'static_recomposition_matches':True,'sha256':hashlib.sha256(panorama.tobytes()).hexdigest(),'status':'Art review. No runtime binding or new animation. Viewports are not new encounters.'}
    (folder/'layers.json').write_text(json.dumps(metadata,indent=2))
    print(region,raw.size,'court core pixels exact, layer recomposition exact',flush=True)

if __name__=='__main__':
    for region in sys.argv[1:]:package(region)
