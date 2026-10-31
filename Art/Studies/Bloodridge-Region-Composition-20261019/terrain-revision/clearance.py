"""Native lower-ground correction: clear the observed front-row overlap, no resampling."""
import json, shutil, time
import numpy as np
from PIL import Image, ImageDraw, ImageFilter
from revise import ROOT, INPUT, OUTPUT, api, GROVE

OUT=ROOT/'ground-clearance'
OUT.mkdir(exist_ok=True)

def run():
    receipt=OUT/'submission.json'
    source=Image.open(ROOT/'natural-ground/raw.png').convert('RGB')
    mask=Image.new('L',(3008,288),0);d=ImageDraw.Draw(mask)
    # White is editable. Preserve the left-edge flora and final court approach.
    d.polygon([(530,45),(660,30),(930,25),(1260,20),(1560,18),(1900,15),(2240,15),(2610,20),(2850,30),(2920,80),(2920,287),(560,287)],fill=255)
    grove=Image.new('L',source.size);ImageDraw.Draw(grove).polygon(GROVE,fill=255)
    mask=Image.fromarray(np.minimum(np.array(mask),255-np.array(grove.crop((0,480,3008,768)))))
    mask.save(OUT/'edit-mask.png')
    guide=source.crop((0,480,3008,768));flat=Image.new('RGB',guide.size,'#8e7468');p=ImageDraw.Draw(flat)
    p.polygon([(0,60),(420,86),(800,71),(1140,30),(1490,53),(1760,36),(2150,67),(2540,39),(3008,69),(3008,150),(2600,123),(2180,139),(1790,103),(1500,127),(1130,109),(800,144),(400,158),(0,131)],fill='#94796b')
    p.line([(550,145),(850,179),(1190,154),(1580,167),(1920,139),(2340,174),(2730,156),(2950,171)],fill='#a78a77',width=6)
    guide.paste(flat,(0,0),mask);guide.save(OUT/'guide.png')
    masked=guide.convert('RGBA');masked.putalpha(Image.fromarray(255-np.array(mask)));masked.save(INPUT/'clearance-masked.png')
    source.crop((1400,367,2776,505)).save(INPUT/'clearance-material.png')
    if receipt.exists():pid=json.loads(receipt.read_text())['prompt_id']
    else:
        g=json.loads((ROOT/'workflow-api.json').read_text())
        g['7']['inputs']['image']='ks-bloodridge-terrain-20261019/clearance-material.png'
        g['20']['inputs']['image']='ks-bloodridge-terrain-20261019/clearance-masked.png'
        g['13']['inputs'].update(positive=['10',0],negative=['11',0])
        g['15']['inputs'].update(width=3008,height=288,steps=12)
        g['16']['inputs']['noise_seed']=20261019146
        g['5']['inputs']['text']='''pixel art natural GROUND SURFACE only, oblique battle floor continuing from the reference material. Extend the existing ash brown and dusty sepia worn bedrock ground down through the ENTIRE image. All of this narrow image is the TOP SURFACE of the SAME solid ground. Irregular broad worn flat rock strata and shallow silt cracks, muted warm violet-brown shadows and quiet varied ash deposits. The lighting and tiny native pixel clusters match the reference floor. No visible side face, no cliff, no dark void, no terrace edge or drop. This is continuous level fighting ground with subtle material variation and native pixel detail, not a vertical wall. Cover the previous dark foreground cliff with the same solid naturally weathered floor. Continue unbroken ground through the bottom edge and right side. No new architecture, props or creatures, no tile pattern, no straight horizontal boundaries. The already preserved flora at the left edge stays as it is.'''
        g['6']['inputs']['text']='cliff, ledge, rock side face, hole, foreground gap, chasm, wall, deep black shadow, vertical rock, terrace, brick, tile grid, objects, buildings, sky'
        g['19']['inputs']['filename_prefix']='KS-Bloodridge-Terrain-20261019/ground-clearance'
        (OUT/'workflow-api.json').write_text(json.dumps(g,indent=2))
        result=api('/prompt',{'prompt':g,'client_id':'codex-bloodridge-ground-clearance'});receipt.write_text(json.dumps(result,indent=2));pid=result['prompt_id']
    print(pid,flush=True)
    for i in range(720):
        h=api('/history/'+pid).get(pid)
        if h:
            (OUT/'history.json').write_text(json.dumps(h,indent=2))
            if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h['status'])
            images=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
            if images:
                im=images[0];shutil.copyfile(OUTPUT/im.get('subfolder','')/im['filename'],OUT/'raw.png')
                rendered=Image.open(OUT/'raw.png').convert('RGB')
                assert rendered.size==(3008,288)
                # Feather only inside the correction mask; no floor copies or scaling.
                blend=mask.filter(ImageFilter.MinFilter(9)).filter(ImageFilter.GaussianBlur(5))
                source.paste(rendered,(0,480),blend);source.save(OUT/'composition.png')
                print('Native ground correction saved',flush=True);return
        if i%12==0:print('Ground-only correction rendering or queued',flush=True)
        time.sleep(5)
    raise TimeoutError('Receipt retained')

if __name__=='__main__':run()
