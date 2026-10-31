"""Bounded cleanup of visually identified generation artefacts; raw outputs stay intact."""
from PIL import Image,ImageDraw
from generate_batch import ROOT
corrections={'Q01-hold':[(0,0,17,17)],
             'Q02-fall-back':[(0,0,127,13),(0,0,13,127),(114,0,127,127),(0,114,127,127)],
             'Q07-fall-back':[(0,0,127,13),(0,0,13,127),(114,0,127,127),(0,114,127,127)],
             'Q06-hold':[(0,0,127,16),(0,0,16,127),(112,0,127,127),(0,112,127,127)],
             'Q08-focus-fire':[(0,0,127,13),(0,0,13,127),(114,0,127,127),(0,114,127,127)],
             'Q09-unleash':[(0,0,127,13),(0,0,13,127),(114,0,127,127),(0,114,127,127)],
             'Q04-unleash':[(0,0,127,11),(0,0,11,127),(116,0,127,127),(0,116,127,127)],
             'Q05-eclipse':[(0,0,127,10),(0,0,11,127),(116,0,127,127),(0,116,127,127)],
             'Q03-focus-fire':[(0,0,127,14),(0,0,16,127),(112,0,127,127),(0,112,127,127)]}
for stem,boxes in corrections.items():
    path=ROOT/'native'/f'{stem}.png'
    if not path.exists():continue
    im=Image.open(path).convert('RGBA');d=ImageDraw.Draw(im)
    for box in boxes:d.rectangle(box,fill='#111019')
    im.save(path);im.resize((512,512),Image.Resampling.NEAREST).save(path.with_name(stem+'-4x.png'))
print('Applied recorded outside-housing cleanup:',list(corrections))
