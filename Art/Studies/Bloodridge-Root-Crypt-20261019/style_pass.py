"""One bounded native Ideogram material pass; original floor and court stay protected."""
import json,shutil,time
import numpy as np
from PIL import Image,ImageFilter
from edit_region import ROOT,INPUT,OUTPUT,REPO,api

FOLDER=ROOT/'crypt-material'

def run():
    FOLDER.mkdir(exist_ok=True)
    image=Image.open(ROOT/'crypt/cleaned.png').convert('RGB')
    mask=Image.open(ROOT/'crypt/edit-mask.png').convert('L')
    masked=image.convert('RGBA');masked.putalpha(Image.fromarray(255-np.array(mask)));masked.save(INPUT/'crypt-material.png')
    receipt=FOLDER/'submission.json'
    if receipt.exists():pid=json.loads(receipt.read_text())['prompt_id']
    else:
        g=json.loads((REPO/'Art/Studies/Stage-Pixel-Match-20261018/skullpeak/workflow-api.json').read_text())
        g['4']['inputs']['text']=json.dumps({
          'high_level_description':'A timeworn Bloodridge burial crypt built into a tangled root bank, preserving the exact existing composition and oblique 2.5D battle camera. Fine controlled dusty hand-painted pixel clusters, just like the surrounding ruined court. Worn naturally irregular stone shapes and slender fibrous roots. Keep the existing solid fighting floor unchanged.',
          'compositional_deconstruction':{
           'background':'Retain the continuous ancient burial complex behind the crypt. Worn narrow burial recesses, eroded sepia limestone, rubble terraces and knotted dry roots physically meet the surrounding original scenery. Ash brown midtones, muted grey-mauve shadows, restrained tarnished bronze. Soft stepped colour clusters, chipped corners and broken edges; contours are described by materials and shadows, never heavy ink.',
           'elements':[{'type':'obj','bbox':[0,120,570,790],'desc':'The existing asymmetrical sunken crypt in the centre has a collapsed broad lintel, a deep shaded opening with shallow worn descending steps, and thick branching roots gripping its weathered masonry. Preserve its silhouette and location but refine its material to match the surrounding original art. Break up the clean rectangular toy-like blocks into irregular eroded stone courses and crumbly joins. Root strands vary in thickness and disappear into the adjoining ruined bank. Thin branches, small stone chips, dusty crevices, tiny pale reeds and rust seedpods. The crypt is embedded in the same ruined cliff, not a detached asset in front of sky.'}]}},ensure_ascii=False)
        g['5']['inputs']['text']='thick black outline, black ink, comic book, Minecraft, voxel, cubic blocks, clean rectangular bricks, toy model, smooth plastic, giant pixels, cartoon, low-detail game icon, noisy dithering, photograph, realistic 3D render, polished new masonry, flat empty background, detached cutout, floating building, separate panels, text, watermark, new characters'
        g['15']={'class_type':'LoadImage','inputs':{'image':'ks-bloodridge-root-crypt-20261019/crypt-material.png'}}
        g['16']={'class_type':'VAEEncode','inputs':{'pixels':['15',0],'vae':['3',0]}}
        g['17']={'class_type':'SetLatentNoiseMask','inputs':{'samples':['16',0],'mask':['15',1]}}
        g['18']={'class_type':'SplitSigmas','inputs':{'sigmas':['10',0],'step':5}}
        g['12']['inputs']['latent_image']=['17',0];g['12']['inputs']['sigmas']=['18',1]
        g['10']['inputs'].update(width=image.width,height=image.height,steps=20)
        g['11']['inputs']['noise_seed']=20261019236
        g['14']['inputs']['filename_prefix']='KS-Bloodridge-Root-Crypt-20261019/material'
        (FOLDER/'workflow-api.json').write_text(json.dumps(g,indent=2))
        # A bounded slot after the currently running job; never interrupt/cancel a job.
        r=api('/prompt',{'prompt':g,'client_id':'codex-bloodridge-material','front':True})
        receipt.write_text(json.dumps(r,indent=2));pid=r['prompt_id']
    print(pid,flush=True)
    for i in range(720):
        h=api('/history/'+pid).get(pid)
        if h:
            (FOLDER/'history.json').write_text(json.dumps(h,indent=2))
            if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h['status'])
            images=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
            if images:
                im=images[0];shutil.copyfile(OUTPUT/im.get('subfolder','')/im['filename'],FOLDER/'raw.png')
                rendered=Image.open(FOLDER/'raw.png').convert('RGB');assert rendered.size==image.size
                base=Image.open(ROOT/'crypt/source.png').convert('RGB')
                blend=mask.filter(ImageFilter.MinFilter(7)).filter(ImageFilter.GaussianBlur(5))
                base.paste(rendered,(0,0),blend);base.save(ROOT/'crypt/refined.png')
                print('Native material pass saved',flush=True);return
        if i%12==0:print('Local material pass queued or rendering',flush=True)
        time.sleep(5)
    raise TimeoutError('Receipt retained')

if __name__=='__main__':run()
