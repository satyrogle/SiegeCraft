from pathlib import Path
from PIL import Image, ImageChops
import shutil

root=Path(__file__).resolve().parent
repo=root.parents[2]
assets=root/'assets'
assets.mkdir(exist_ok=True)
shutil.copy2(repo/'Assets/Resources/CampaignArt/InnerCircle/shade_front.png',assets/'shade.png')
for name in ['hold','fallback','focus','unleash']:
    shutil.copy2(repo/f'Assets/Resources/CampaignArt/UI/icon_{name}.bytes',assets/f'{name}.png')
shutil.copy2(Path(r'C:\Users\jacob\codex-design-lab\runtimes\ks-pixel-kit-20261013\node_modules\@pebbly\aseprite-ai-artist\knowledge\fonts\pixel5x7.json'),assets/'pixel5x7.json')
atlas=root/'command-emblems-raw.png'
if atlas.exists():
    im=Image.open(atlas).convert('RGB')
    w,h=im.size
    for row in range(2):
        for col in range(4):
            cell=im.crop((col*w//4,row*h//2,(col+1)*w//4,(row+1)*h//2))
            background=Image.new('RGB',cell.size,cell.getpixel((0,0)))
            mask=ImageChops.difference(cell,background).convert('L').point(lambda v:255 if v>20 else 0)
            box=mask.getbbox()
            if box:
                cx,cy=(box[0]+box[2])//2,(box[1]+box[3])//2
                side=int(max(box[2]-box[0],box[3]-box[1])*1.3)
                cell=cell.crop((cx-side//2,cy-side//2,cx+side//2,cy+side//2))
            cell=ImageChops.subtract(cell,Image.new('RGB',cell.size,im.getpixel((0,0))))
            cell.resize((64,64),Image.Resampling.LANCZOS).quantize(colors=32,dither=Image.Dither.NONE).convert('RGB').save(assets/f'emblem-{row}-{col}.png')
    print('Atlas split into 8 cells:',im.size)
print('Reference copies ready; originals unchanged.')
