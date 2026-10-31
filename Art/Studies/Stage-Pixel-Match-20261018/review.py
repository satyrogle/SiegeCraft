from PIL import Image, ImageDraw, ImageFont
import pathlib, shutil
ROOT=pathlib.Path(__file__).resolve().parent
DEST=pathlib.Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/comparison')
for name in ('skullpeak','hunters'):
    shutil.copyfile(ROOT/name/(name+'-review.png'),DEST/(name+'-pixel-v2.png'))
sources=[DEST/'bloodridge-reference.png',DEST/'skullpeak-pixel-v2.png',DEST/'hunters-pixel-v2.png']
for name,src in zip(('bloodridge','skullpeak','hunters'),sources):
    Image.open(src).crop((540,420,780,580)).save(DEST/(name+'-detail-v2.png'))
html=(DEST/'index.html').read_text(encoding='utf-8')
backup=DEST/'index-before-pixel-match.html'
if not backup.exists():backup.write_text(html,encoding='utf-8')
html=html.replace('src="skullpeak.png"','src="skullpeak-pixel-v2.png"').replace('src="hunters.png"','src="hunters-pixel-v2.png"')
html=html.replace('NEW CANDIDATE / NOT APPROVED','PIXEL REVISION / FOR YOUR REVIEW')
html=html.replace('Same stage area and proportions. No fighters or HUD. Click Inspect at 1x for native pixels.','Bloodridge unchanged. Revised Skullpeak and hunter scenes: quieter surfaces, limited palettes and native pixel cleanup.')
html=html.replace('The new candidates are shown unchanged. Neither candidate has replaced the shipped background.','The revised candidates use local generation plus palette and cluster cleanup, with no image rescaling. They remain review assets; neither has replaced the shipped background.')
html=html.replace('</style>','.detail{width:100%;height:160px;object-fit:none;image-rendering:pixelated;border:1px solid #5c5149;margin-top:18px}.detail-label{color:#b9b1a7;font-size:12px;margin:4px 0 0} </style>')
for name,title in [('bloodridge','Bloodridge court'),('skullpeak','Skullpeak'),('hunters','Hunter wastes')]:
    needle=f"<button onclick=\"inspect('{name}','{title}')\">Inspect at 1x</button>"
    html=html.replace(needle,needle+f'<img class="detail" src="{name}-detail-v2.png" alt="{title} floor detail at native pixel scale"><p class="detail-label">Floor detail · native 1x</p>')
(DEST/'index.html').write_text(html,encoding='utf-8')
# A review board only: full-resolution originals remain available in the 1x viewer.
board=Image.new('RGB',(2112,520),'#19171c');draw=ImageDraw.Draw(board)
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',28)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
for i,(title,src) in enumerate(zip(('BLOODRIDGE — ORIGINAL','SKULLPEAK — REVISED','HUNTERS — REVISED'),sources)):
    x=16+i*704
    draw.text((x,18),title,font=font,fill='#ece1cb')
    board.paste(Image.open(src).convert('RGB').resize((688,384),Image.Resampling.NEAREST),(x,62))
draw.text((16,470),'Review candidates. Native 1376 x 768 files are available in the comparison viewer. No production background replaced.',font=small,fill='#b9b1a7')
board.save(DEST/'pixel-match-board.png')
print(DEST/'index.html')
