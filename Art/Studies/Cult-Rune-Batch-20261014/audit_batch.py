from pathlib import Path
import hashlib,json
from PIL import Image,ImageChops
from generate_batch import PALETTE,ROOT

manifest=json.loads((ROOT/'manifest.json').read_text())
allowed={tuple(bytes.fromhex(c[1:])) for c in PALETTE}
result={'scope':'Twenty isolated ready-state command studies; model housings plus exact authored glyphs.',
        'glyph_source_sha256':hashlib.sha256((ROOT/'command-runes.png').read_bytes()).hexdigest(),
        'files':[]}
glyphs=Image.open(ROOT/'command-runes.png').convert('RGBA')
for item in manifest:
    if item.get('status')!='complete':continue
    path=Path(item['native']);im=Image.open(path).convert('RGBA')
    big=Image.open(path.with_name(path.stem+'-4x.png')).convert('RGBA')
    i=item['command_index'];mask= glyphs.crop((i*64,0,i*64+64,64)).getchannel('A')
    colours=set(im.convert('RGB').get_flattened_data())
    glyph_pixels=[im.getpixel((32+x,32+y)) for y in range(64) for x in range(64) if mask.getpixel((x,y))]
    # Every foreground glyph pixel must remain substantially brighter than its dark well.
    legible=all(max(px[:3])>=119 for px in glyph_pixels)
    check={'id':item['id'],'size':list(im.size),'colours':len(colours),
       'palette_clean':colours<=allowed,'upscale_exact':ImageChops.difference(big.convert('RGB'),im.resize((512,512),Image.Resampling.NEAREST).convert('RGB')).getbbox() is None,
       'glyph_foreground_preserved':legible,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
       'raw_sha256':hashlib.sha256(Path(item['raw']).read_bytes()).hexdigest()}
    result['files'].append(check)
result['count']=len(result['files'])
result['unique_raws']=len({x['raw_sha256'] for x in result['files']})
result['unique_candidates']=len({x['sha256'] for x in result['files']})
result['mechanical_pass']=result['count']==20 and result['unique_raws']==20 and all(x['size']==[128,128] and x['palette_clean'] and x['upscale_exact'] and x['glyph_foreground_preserved'] for x in result['files'])
result['limits']='No Unity integration or in-battle/state readability test. Visual audit is recorded separately. Review backgrounds remain opaque.'
(ROOT/'audit-mechanical.json').write_text(json.dumps(result,indent=2))
print(json.dumps({k:v for k,v in result.items() if k!='files'},indent=2))
