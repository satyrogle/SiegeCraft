from pathlib import Path
import json
from PIL import Image,ImageDraw,ImageFont
from generate_batch import COMMANDS

ROOT=Path(__file__).resolve().parent
FONT='C:/Windows/Fonts/segoeui.ttf';BOLD='C:/Windows/Fonts/seguisb.ttf'
def font(size,bold=False):return ImageFont.truetype(BOLD if bold else FONT,size)
def text(d,xy,s,size=24,fill='#ddd1df',bold=False):d.text(xy,s,font=font(size,bold),fill=fill)
manifest=json.loads((ROOT/'manifest.json').read_text()) if (ROOT/'manifest.json').exists() else []

key=Image.new('RGB',(1152,1550),'#17131d');d=ImageDraw.Draw(key)
text(d,(32,26),'KINDRED SIEGE / CULT COMMAND RUNES',22,'#be965c')
text(d,(32,67),'One visual language. Five meanings.',41,bold=True)
text(d,(32,123),'Diamond = the unit or target. Boundary = containment. Tips = direction.',24)
sheet=Image.open(ROOT/'command-runes.png').convert('RGBA')
for i,(_,name,meaning) in enumerate(COMMANDS):
    y=196+i*260
    tile=sheet.crop((i*64,0,(i+1)*64,64)).resize((192,192),Image.Resampling.NEAREST)
    key.paste(tile,(40,y),tile)
    text(d,(282,y+36),name,32,'#dfc88b',True)
    text(d,(282,y+94),meaning,23)
text(d,(32,1510),'Models generate the housings. Exact authored glyphs preserve each command.',22)
key.save(ROOT/'00-rune-key.png')

for model,label in [('flux','FLUX.2 KLEIN'),('qwen','QWEN IMAGE EDIT')]:
    items={v['id']:v for v in manifest if v['model']==model and v.get('status')=='complete'}
    if not items:continue
    page=Image.new('RGB',(1152,3300),'#17131d');d=ImageDraw.Draw(page)
    text(d,(32,24),'KINDRED SIEGE / '+label,24,'#be965c')
    text(d,(32,66),'Ten command-button candidates',40,bold=True)
    text(d,(32,122),'A / CUT OBSIDIAN',25);text(d,(608,122),'B / NEURAL RELIQUARY',25)
    for cmd in range(5):
        y=174+cmd*610
        for col,idx in [(0,cmd+1),(1,cmd+6)]:
            ident=('F' if model=='flux' else 'Q')+f'{idx:02d}';x=32+col*576
            if ident not in items:
                text(d,(x,y+220),ident+' / generating',27);continue
            item=items[ident];im=Image.open(item['native']).convert('RGB').resize((512,512),Image.Resampling.NEAREST)
            page.paste(im,(x,y))
            text(d,(x,y+523),ident+' / '+item['command'],28,'#dfc88b',True)
        text(d,(32,y+568),COMMANDS[cmd][2],22)
    text(d,(32,3250),'128px native studies shown at 4x. Same rune shapes across both models.',22)
    page.save(ROOT/f'01-{model}-ten-candidates.png')
print('Review sheets updated:',len([v for v in manifest if v.get('status')=='complete']),'complete')
