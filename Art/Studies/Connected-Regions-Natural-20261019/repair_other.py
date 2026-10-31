"""Whole-width local image edits. No tiled flooring or mirrored joins."""
import pathlib,json,shutil,time,urllib.request,sys
from PIL import Image,ImageDraw
import numpy as np
ROOT=pathlib.Path(__file__).resolve().parent
REPO=ROOT.parents[2]
OLD=REPO/'Art/Studies/Connected-Regions-20261018'
INPUT=pathlib.Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-natural-regions-20261019')
OUTPUT=pathlib.Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')
INPUT.mkdir(exist_ok=True)
sys.path.insert(0,str(OLD))
from outpaint import SOURCES,api
SUBJECTS={
 'bloodridge':'Bloodridge burial lands: muted ash-brown limestone, violet-brown shadows, worn pale stone and dry ancient roots. The far left has only low eroded banks and scattered sunken stones. Small burial niches emerge gradually along the back edge toward the middle, becoming taller ruined masonry toward the original court on the right. No duplicate monumental entrance.',
 'skullpeak':'Skullpeak mountain clan lands: quiet grey slate, old dark iron, sparse ivory snow held on rear ledges. The far left is a bare mountain shelf with a low natural slate bank. A few broken iron braces and low irregular retaining stones develop into fortifications toward the original oath court on the right. No repeated skull gate. No tidy rectangular brickwork.',
 'hunters':'Alien hunter wastes: ash-grey basalt and dusty mauve, weathered dark starship metal, very sparse dull violet seams. The far left has almost empty ash flats below a low rock bank. Small buried wreck fragments gradually become larger broken ship ribs toward the original engine court on the right. No intact factory wall, city, repeated engines or rows of identical wrecks.'}
def run(region):
    folder=ROOT/region/'single-edit';folder.mkdir(parents=True,exist_ok=True)
    receipt=folder/'submission.json'
    if receipt.exists():pid=json.loads(receipt.read_text())['prompt_id']
    else:
        q=api('/queue')
        if q['queue_running'] or q['queue_pending']:raise RuntimeError('Local queue occupied; left untouched.')
        # Remove the failed repeated floor from the editable reference. Its solid fill
        # is a diffusion guide only. The entire new 2752-pixel area is regenerated.
        original=Image.open(SOURCES[region]).convert('RGB')
        guide=Image.open(OLD/region/'panorama.png').convert('RGBA')
        color=tuple(np.median(np.array(original)[420:620,400:1000],axis=(0,1)).astype(int))
        # Keep existing visual context; the model redraws repetitive ground, not a blank guide.
        guide.paste(original,(2752,0))
        alpha=Image.new('L',guide.size,0);ImageDraw.Draw(alpha).rectangle((2752,0,4127,767),fill=255)
        guide.putalpha(alpha);guide.save(folder/'guide.png');guide.save(INPUT/(region+'-guide.png'))
        original.save(INPUT/(region+'-court.png'))
        prompt='Replace the repeated paving and mirrored rocks in the left two thirds with unique natural ground. Erase the visible vertical joins between sections. Redraw the extension as one connected '+SUBJECTS[region]+' landscape in the same pixel art style and colours. A bare start on the far left develops gradually toward the existing right-hand court. The ground stays flat across the entire lower two thirds. Use quiet irregular stone and earth with shallow unique cracks, fewer pavers toward the left. Rear scenery continues naturally with varied organic shapes. No duplicated pillars, reflected patches, repeating fork cracks, pits or foreground structures. Preserve the entire right third, the original court, exactly. Keep the same camera, native small pixel clusters, muted palette and lighting. One continuous battle environment, no panels.'
        g=json.loads((REPO/'Art/Studies/Bloodridge-Region-Local-20261018/workflow-api.json').read_text())
        g['6']['inputs']['image']='ks-natural-regions-20261019/'+region+'-guide.png'
        g['8']['inputs']['image']='ks-natural-regions-20261019/'+region+'-court.png'
        g['9']['inputs']['prompt']=prompt
        g['10']['inputs']['prompt']=''
        for n in ['9','10']:g[n]['inputs'].pop('image2',None)
        g.pop('8')
        g.pop('11')
        g['15']={'class_type':'VAEEncode','inputs':{'pixels':['6',0],'vae':['5',0]}}
        g['16']={'class_type':'SetLatentNoiseMask','inputs':{'samples':['15',0],'mask':['6',1]}}
        g['12']['inputs'].update(latent_image=['16',0],seed=2026101960+list(SUBJECTS).index(region))
        g['14']['inputs']['filename_prefix']='KS-Natural-Regions-20261019/'+region
        (folder/'workflow-api.json').write_text(json.dumps(g,indent=2))
        (folder/'prompt.txt').write_text(prompt)
        receipt.write_text(json.dumps(api('/prompt',{'prompt':g,'client_id':'codex-natural-regions'}),indent=2))
        pid=json.loads(receipt.read_text())['prompt_id']
    print(region,pid,flush=True)
    for attempt in range(720):
        h=api('/history/'+pid).get(pid)
        if h:
            (folder/'history.json').write_text(json.dumps(h,indent=2))
            if h.get('status',{}).get('status_str')=='error':raise RuntimeError(str(h['status']))
            images=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
            if images:
                im=images[0];shutil.copyfile(OUTPUT/im.get('subfolder','')/im['filename'],folder/'raw.png')
                print('Saved',region,flush=True);return
        if attempt%12==0:print(region,'local edit running',flush=True)
        time.sleep(5)
    raise TimeoutError('Receipt retained; resume, do not resubmit.')
if __name__=='__main__':
    for region in sys.argv[1:]:run(region)
