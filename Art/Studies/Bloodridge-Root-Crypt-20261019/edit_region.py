"""Local native Qwen edits of the accepted grove direction; preserve the battle floor."""
from pathlib import Path
import json, shutil, sys, time, urllib.request
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

ROOT=Path(__file__).resolve().parent
REPO=ROOT.parents[2]
PRIOR=REPO/'Art/Studies/Bloodridge-Region-Composition-20261019'
INPUT=Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-bloodridge-root-crypt-20261019')
OUTPUT=Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')
COURT=Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/comparison/bloodridge-reference.png')
SERVER='http://127.0.0.1:8191'

EDITS={
 'crypt':{'box':(1248,0,3136,768),'polygon':[(148,0),(1503,0),(1503,275),(1410,352),(1260,386),(1090,386),(915,354),(730,386),(530,414),(340,427),(148,388)],
 'prompt':'''Edit only the selected upper scenery. Keep this exact pixel-art drawing style, fine stone chips and clusters, sepia limestone, muted violet-brown shadows and old corroded bronze. Keep the entire existing battle floor, the root grove at the left edge and the original court on the right unchanged. This is ONE physically connected Bloodridge burial complex, not separate pictures.
Replace the repetitive row of tomb doors and staircases in the middle with ONE dramatic SUNKEN ROOT CRYPT. A huge ancient cracked lintel has partly collapsed into the eroded burial cliff. Thick calcified roots curve over it into a low asymmetrical arch, like the exposed ribs of a dead tree gripping a ruined roof. The arch frames one deep shadowed recess, with shallow broken steps descending into it BEHIND the clear fighting ground. At one side the broken stone roof opens to dusty light. The other side is close and root-choked. Weathered burial carvings are almost rubbed away, stone faces are irregular and chipped, small old bronze braces are dull, half buried and corroded. No new tall boss gate: the original court at far right remains the destination.
Make the same roots from the preserved grove continue visibly along this eroded bank, over the collapsed arch and into the existing court's masonry. A shallow dark seep at the wall foot connects to the grove pool. Brittle pale reeds and small rust-red seedpods grow in a few protected crevices. Quiet tiny moth silhouettes gather near the pods. Rich rounded crumbling stone and tangled woody forms; strongly varied heights, overhang and enclosure. No evenly spaced doors or props. Preserve the worn material detail and hand-shaped pixel clusters of the existing image. No open desert panorama, mountains, big empty sky, smooth 3D cliffs, brick parapet, foreground holes, people, text or UI. The lower half remains the same broad solid battle floor.'''} ,
 'outskirts':{'box':(0,0,1376,768),'polygon':[(0,0),(668,0),(668,155),(640,242),(594,317),(550,357),(474,384),(320,420),(0,443)],
 'prompt':'''Edit only the selected left scenery while keeping the existing hollow root grove, pool, the lower battle floor and the exact pixel-art drawing style unchanged. Make the bare beginning belong physically to the SAME Bloodridge burial complex. This is an intimate wind-scoured BURIAL WASH at the foot of eroded ash-limestone banks, not an empty generic desert with distant mountains. Large irregular low bank shapes fold gradually down into the existing root pool on the right. The bank exposes thin old grave slabs, a few corroded bronze fragments and tangled dry roots protruding from the earth. One small leaning burial marker is partly swallowed by the bank; no large gate or monumental buildings.
The start is sparse, worn and quiet. The material is as detailed and deliberate as the preserved court: chipped edges, irregular native pixel clusters, dusty sepia stone, deep violet-brown crevices and ash caught in sheltered folds. Sparse small pale thorn fans, dull rust-red seedpods and brittle reeds become denser near the existing grove pool. Two tiny long-legged ash birds can stand on the far rear bank near the reeds; they stay tiny background wildlife, not foreground characters. Show a continuous shallow silt channel entering the existing pool, with its bank and roots physically joining the grove. Replace the abrupt vertical skyline join with the silhouette of that same eroded bank. No copied wall, no repeated shrub stamps, no rows of tombs, no smooth painted hills, no blue sky, no cartoon outlines, no foreground cliffs, no people, no text. Keep the original 2.5D battle camera and original lower fighting ground.'''}
}

def api(path,body=None):
    req=urllib.request.Request(SERVER+path,data=None if body is None else json.dumps(body).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=30) as r:return json.load(r)

def base():
    im=Image.open(PRIOR/'panorama.png').convert('RGB')
    im.paste(Image.open(COURT).convert('RGB'),(2752,0))
    im.save(ROOT/'base.png')
    return im

def run(name):
    spec=EDITS[name];folder=ROOT/name;folder.mkdir(exist_ok=True);INPUT.mkdir(exist_ok=True)
    im=base().crop(spec['box']);edit=Image.new('L',im.size)
    ImageDraw.Draw(edit).polygon(spec['polygon'],fill=255)
    edit.save(folder/'edit-mask.png');im.save(folder/'source.png')
    masked=im.convert('RGBA');masked.putalpha(Image.fromarray(255-np.array(edit)))
    masked.save(INPUT/f'{name}.png')
    receipt=folder/'submission.json'
    if receipt.exists():pid=json.loads(receipt.read_text())['prompt_id']
    else:
        graph=json.loads((PRIOR/'outskirts/workflow-api.json').read_text())
        graph['6']['inputs']['image']=f'ks-bloodridge-root-crypt-20261019/{name}.png'
        graph['9']['inputs']['prompt']=spec['prompt']
        graph['12']['inputs']['seed']=20261019231+(name=='outskirts')
        graph['14']['inputs']['filename_prefix']=f'KS-Bloodridge-Root-Crypt-20261019/{name}'
        (folder/'workflow-api.json').write_text(json.dumps(graph,indent=2))
        (folder/'prompt.txt').write_text(spec['prompt'])
        result=api('/prompt',{'prompt':graph,'client_id':'codex-bloodridge-root-crypt'})
        receipt.write_text(json.dumps(result,indent=2));pid=result['prompt_id']
    print(name,pid,flush=True)
    for i in range(720):
        h=api('/history/'+pid).get(pid)
        if h:
            (folder/'history.json').write_text(json.dumps(h,indent=2))
            if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h['status'])
            images=[im for output in h.get('outputs',{}).values() for im in output.get('images',[])]
            if images:
                entry=images[0];shutil.copyfile(OUTPUT/entry.get('subfolder','')/entry['filename'],folder/'raw.png')
                rendered=Image.open(folder/'raw.png').convert('RGB');assert rendered.size==im.size
                blend=edit.filter(ImageFilter.MinFilter(7)).filter(ImageFilter.GaussianBlur(5))
                im.paste(rendered,(0,0),blend);im.save(folder/'edited.png')
                print('Saved native edit',name,rendered.size,flush=True);return
        if i%12==0:print('Local pixel-art edit queued or rendering',name,flush=True)
        time.sleep(5)
    raise TimeoutError('Receipt retained; resume without duplicating')

if __name__=='__main__':run(sys.argv[1])
