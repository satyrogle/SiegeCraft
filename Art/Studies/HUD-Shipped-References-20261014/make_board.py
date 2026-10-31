from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import hashlib, json

ROOT = Path(__file__).resolve().parent
BG, PANEL, LINE = '#14151b', '#20222b', '#3b3e4b'
INK, MUTED, ACCENT = '#f0ede5', '#b0b4c2', '#d6be88'
FONT = Path('C:/Windows/Fonts')
def font(size, bold=False):
    return ImageFont.truetype(str(FONT / ('segoeuib.ttf' if bold else 'segoeui.ttf')), size)

refs = [
    dict(number='01', title='FIRE EMBLEM', subtitle='The Blazing Blade / GBA', file='fire-emblem-forecast.png',
         page='https://fireemblemwiki.org/wiki/File:Ss_fe07_strategic_combat_forecast.png',
         source='https://fireemblemwiki.org/w/images/f/f5/Ss_fe07_strategic_combat_forecast.png',
         label='STRUCTURED INFORMATION',
         crops=[((4, 4, 79, 56), 5)],
         observations=['Small icons sit beside clear words.', 'Thin borders separate values and ownership.'],
         transfer='Study the spacing and hierarchy.',
         avoid='KS keeps damage / MISS, not hit percentages.'),
    dict(number='02', title='ONE PIECE', subtitle="Shonen Jump's One Piece / GBA", file='one-3.png',
         page='https://lospec.com/screenshots/Game%20Boy%20Advance/One%20Piece',
         source='https://cdn.lospec.com/screenshots/Game Boy Advance/One Piece/One Piece (Eng) 003.png',
         label='FACTION IDENTITY',
         crops=[((0, 0, 110, 31), 4), ((0, 136, 172, 160), 3)],
         observations=['A single emblem gives the HUD its identity.', 'Long, simple bars leave room for the action.'],
         transfer='Study the emblem-to-bar relationship.',
         avoid='KS keeps its cult identity and dark palette.'),
    dict(number='03', title='CASTLEVANIA', subtitle='Aria of Sorrow / GBA', file='aria-2.png',
         page='https://lospec.com/screenshots/Game%20Boy%20Advance/Castlevania%20-%20Aria%20of%20Sorrow',
         source='https://cdn.lospec.com/screenshots/Game Boy Advance/Castlevania - Aria of Sorrow/Castlevania - Aria of Sorrow (2018) (Eng) 002.png',
         label='RESTRAINED GOTHIC DETAIL',
         crops=[((0, 0, 110, 27), 4), ((168, 138, 240, 160), 5)],
         observations=['Small gold corners frame dark information.', 'Bright type and distinct bars carry the signal.'],
         transfer='Study the small trim and dark/light contrast.',
         avoid='KS symbols and controls will be original.'),
]

def make_card(ref):
    card=Image.new('RGB',(768,1120),PANEL); d=ImageDraw.Draw(card)
    d.text((24,18),ref['number'],font=font(30,True),fill=ACCENT)
    d.text((88,20),ref['title'],font=font(28,True),fill=INK)
    d.text((88,59),ref['subtitle'],font=font(22),fill=MUTED)
    im=Image.open(ROOT/'sources'/ref['file']).convert('RGB')
    assert im.size==(240,160)
    card.paste(im.resize((720,480),Image.Resampling.NEAREST),(24,106))
    d.text((24,601),'Full source screen / 3x nearest-neighbour',font=font(19),fill=MUTED)
    d.line((24,642,744,642),fill=LINE,width=1)
    d.text((24,661),ref['label'],font=font(22,True),fill=ACCENT)
    y=707
    for box, scale in ref['crops']:
        crop=im.crop(box); crop=crop.resize((crop.width*scale,crop.height*scale),Image.Resampling.NEAREST)
        card.paste(crop,(24,y));y+=crop.height+12
    for i,line in enumerate(ref['observations']):d.text((24,997+i*29),line,font=font(21),fill=INK)
    d.text((24,1063),ref['transfer'],font=font(19),fill=MUTED)
    return card

cards=[make_card(r) for r in refs]
board=Image.new('RGB',(2400,1320),BG);d=ImageDraw.Draw(board)
d.text((24,20),'KINDRED SIEGE / CHOOSE THE HUD CRAFT',font=font(35,True),fill=INK)
d.text((24,72),'Three shipped-game references. Pick the treatment of borders, lettering and bars.',font=font(24),fill=MUTED)
for i,c in enumerate(cards):board.paste(c,(24+i*792,124))
d.text((24,1260),'Reference screenshots only. KS keeps its cult identity, native fighters and world 1x / UI 2x.',font=font(23),fill=INK)
board.save(ROOT/'hud-reference-board.png')

phone=Image.new('RGB',(816,3620),BG);d=ImageDraw.Draw(phone)
d.text((24,20),'KS / HUD REFERENCES',font=font(32,True),fill=INK)
d.text((24,69),'Choose 1, 2 or 3. Original screenshots; exact pixel enlargements.',font=font(20),fill=MUTED)
for i,c in enumerate(cards):phone.paste(c,(24,118+i*1140))
d.text((24,3555),'KS retains the cult identity and its world 1x / UI 2x scale.',font=font(21),fill=INK)
phone.save(ROOT/'hud-reference-phone.png')

for ref in refs:
    data=(ROOT/'sources'/ref['file']).read_bytes()
    ref['sha256']=hashlib.sha256(data).hexdigest()
    ref['source_dimensions']=[240,160]
    ref['status']='reference only; no UI design selected or game assets copied'
(ROOT/'selected-sources.json').write_text(json.dumps(refs,indent=2),encoding='utf-8')
print('Created side-by-side and phone boards from three 240x160 source captures.')
