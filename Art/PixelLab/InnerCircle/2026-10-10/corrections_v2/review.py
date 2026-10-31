from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import hashlib,json
p=Path(__file__).parent
old=Path('C:/Users/jacob/KindredSiege-Clean/Assets/Resources/CampaignArt/InnerCircle')
names=['leader','butcher','glutton','shade','whisperer']
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',20)
def row(folder,title):
    sheet=Image.new('RGB',(2000,452),(48,49,54)); d=ImageDraw.Draw(sheet)
    d.text((16,10),title,font=font,fill=(236,226,211))
    for i,name in enumerate(names):
        im=Image.open(folder/f'{name}_front.png').convert('RGBA')
        large=im.resize((384,384),Image.Resampling.NEAREST)
        d.text((i*400+16,44),name.upper(),font=font,fill=(236,226,211))
        sheet.paste(large,(i*400+8,68),large)
    return sheet
before=row(old,'PREVIOUS')
after=row(p,'CORRECTIONS — FOR REVIEW')
after.save(p/'corrected-sprites.png')
comparison=Image.new('RGB',(2000,912),(20,20,24));comparison.paste(before,(0,0));comparison.paste(after,(0,460));comparison.save(p/'before-after.png')
checks=[]
for name in names:
    f=p/f'{name}_front.png';im=Image.open(f).convert('RGBA')
    assert im.size==(192,192)
    count=len(im.getcolors(192*192));assert count<=32
    assert im.getpixel((0,0))[3]==0
    checks.append(dict(name=name,size=im.size,colours=count,bounds=im.getbbox(),sha256=hashlib.sha256(f.read_bytes()).hexdigest(),original_unchanged=hashlib.sha256((old/f'{name}_front.png').read_bytes()).hexdigest()))
(p/'validation.json').write_text(json.dumps(checks,indent=2))
print(json.dumps(checks,indent=2))
