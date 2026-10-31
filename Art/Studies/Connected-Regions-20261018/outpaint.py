"""Local masked extensions from native court pixels. Receipts make every job resumable."""
import json,pathlib,time,urllib.request,shutil,sys
from PIL import Image
ROOT=pathlib.Path(__file__).resolve().parent
REPO=ROOT.parents[2]
BASE=REPO/'Art/Pipeline/runs/hunters-sheet-20261015e/raw/option-1-workflow.json'
INPUT=pathlib.Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input')
OUTPUT=pathlib.Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')
SERVER='http://127.0.0.1:8191'
W,H,CONTEXT=1376,768,512
def api(path,body=None):
    req=urllib.request.Request(SERVER+path,data=None if body is None else json.dumps(body).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=30) as r:return json.load(r)
SOURCES={
 'bloodridge':pathlib.Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/comparison/bloodridge-reference.png'),
 'skullpeak':REPO/'Art/Studies/Stage-Pixel-Match-20261018/skullpeak/skullpeak-review.png',
 'hunters':REPO/'Art/Studies/Stage-Pixel-Match-20261018/hunters/hunters-review.png'}
SUBJECTS={
 'bloodridge':('Muted ash-limestone, dusty sepia, violet-brown shadow, dull ochre. A dry ancient burial region with soft uneven stone forms, eroded roots and funerary architecture.',
 ['The middle approach to a burial court. A low ruined burial terrace follows the upper quarter, broken small sealed tomb niches and a few gnarled dry roots binding the masonry. Its height gradually increases toward the RIGHT where it connects seamlessly to the existing burial court. The far LEFT is a simpler ash bank with a few sunken stones. No giant door, no second boss monument.',
  'The bare outskirts of the same burial region. Flat worn ash ground with a few small half-buried rounded limestone slabs. The rear edge is a low eroded bank with one broken burial marker at the RIGHT, joining the existing ruins there. The far LEFT is sparse open ash with a low pale distant ridge. No monumental architecture.']),
 'skullpeak':('Muted blue-grey slate, violet-grey shadows, old dark iron, isolated ivory snow patches, tiny wine-red cloth accents. A cold mountain clan region.',
 ['The middle approach to an iron mountain oath court. A low weathered slate retaining wall and scattered blunt iron braces follow the upper quarter. On the RIGHT they join the stronger existing court walls. On the LEFT the retaining wall breaks down into a low natural slate shelf with sparse snow in its recesses. One small worn wine-red pennant near the right. No second gate or giant skull.',
  'The bare outer mountain shelf before the clan fortifications. A broad level stone and packed-grey-dust fighting ground. Only a low rough slate ridge at the upper edge with small snow patches in sheltered cracks. A single broken iron brace far RIGHT links it to the existing approach. Far LEFT opens toward pale distant mountain silhouettes. No fortress, no gate, no busy decoration.']),
 'hunters':('Muted ash-grey basalt, dusty mauve, dark obsidian alloy, restrained violet seams. An alien hunter region in a war-torn lawless space wasteland.',
 ['The middle approach through an old wreck field. Low broken starship ribs and a few half-buried hull plates follow the upper quarter, increasingly substantial toward the RIGHT where they join the existing engine wreck court. At the far LEFT, sparse low wreck fragments sink into the ash. The fighting plane stays broad and empty. No second giant engine arch.',
  'The bare outer ash flats of the same wreck region. Broad level ash-grey basalt ground, quiet worn slabs partially buried in smooth dust. A low charcoal escarpment on the rear horizon. Only two small half-buried dark metal fragments at the RIGHT connect to the existing wreck field. Far LEFT is almost empty. No engines, huge wrecks or neon effects.'])}

def run(region,step,revision=''):
    folder=ROOT/region/f'extend-{step}{revision}';folder.mkdir(parents=True,exist_ok=True)
    previous=SOURCES[region] if step==1 else ROOT/region/f'extend-{step-1}{revision}'/'joined.png'
    current=Image.open(previous).convert('RGB')
    # Only the left edge is the context for the next contiguous extension. Nothing is scaled.
    context=current.crop((0,0,CONTEXT,H));context_name=f'ks-connected-{region}-{step}{revision}-20261018.png'
    context.save(INPUT/context_name);context.save(folder/'context.png')
    if revision:
        # Preserve a native-size floor guide under a partial noise mask. The failed
        # unconditioned Skullpeak attempt mistook the floor for a vertical wall.
        guide=Image.new('RGBA',(W+CONTEXT,H))
        guide.paste(current.crop((0,0,W,H)).convert('RGBA'),(0,0))
        # Use central floor material rather than repeating the court's cliff corners.
        # This is only a native-pixel diffusion guide, never the delivered artwork.
        floor_source=Image.open(SOURCES[region]).convert('RGBA')
        floor_strip=floor_source.crop((320,320,1056,H))
        for x in range(-((step*197)%736),W,736):guide.paste(floor_strip,(x,320))
        guide.paste(context.convert('RGBA'),(W,0))
        alpha=Image.new('L',guide.size,255)
        for y in range(H):
            value=0 if y<260 else min(125,round((y-260)*125/100))
            alpha.paste(value,(0,y,W,y+1))
        guide.putalpha(alpha);guide.save(INPUT/context_name);guide.save(folder/'floor-guide.png')
    receipt=folder/'submission.json'
    if receipt.exists():pid=json.loads(receipt.read_text())['prompt_id']
    else:
        q=api('/queue')
        if q['queue_running'] or q['queue_pending']:raise RuntimeError('Local queue occupied; left untouched.')
        palette,subjects=SUBJECTS[region];subject=subjects[step-1];g=json.loads(BASE.read_text())
        text={'high_level_description':'One continuous side-view 2.5D pixel-art battle environment. The lower two thirds are a HORIZONTAL SLATE OR STONE FLOOR seen at an oblique angle, with broad irregular slabs foreshortened into wide shallow shapes. Never a vertical brick wall. All architecture stays at the top. Extend the existing scenery at the RIGHT naturally to the LEFT. Same ground plane, same material scale, same illumination and pixel clusters. '+subject,
         'style_description':{'medium':'pixel_art','art_style':'Crisp native pixel clusters, organic irregular shapes, subtly chipped edges, three or four flat tones per material. Broad quiet surfaces. Precise stepped pixel contours. Match the connected existing pixels on the right. No painted brush strokes, fine texture, dithering, blur or tiny gravel.','aesthetics':'quiet connected ancient game environment, restrained detail, subdued low contrast ground','lighting':'Even soft upper-left lighting in discrete pixel planes'},
         'compositional_deconstruction':{'background':palette+' Continuous flat walkable ground across the whole lower two thirds. Camera slightly elevated SIDE battle view, not top-down or an isometric map. Ground extends to left, right and bottom. Quiet paving shapes about 60-140 pixels wide and 20-55 pixels deep, partly buried in dust; not gigantic slabs.',
         'elements':[{'type':'obj','bbox':[0,0,350,1000],'desc':subject+' All obstacles confined to the rear upper third. No people or living statues.'},{'type':'obj','bbox':[350,0,1000,1000],'desc':'A wide clear fighting plane with the same height and perspective as the adjoining right-hand ground. Irregular low-contrast paving interrupted by broad smooth dust. No hills, stairs, cliffs, holes, props or fencing in the battle lanes. No repeated square tile grid.'}]}}
        g['4']['inputs']['text']=json.dumps(text)
        g['5']['inputs']['text']='collage, separate panels, dividing lines, map, roadmap, aerial view, isometric diamond, path toward vanishing point, horizon in middle, characters, people, creatures, text, HUD, symbols, photo, 3D render, brushwork, photoreal texture, grain, scattered gravel, dithering, bevelled tiles, large foreground obstacles, giant repeating slabs'
        g.pop('8')
        g['15']={'class_type':'LoadImage','inputs':{'image':context_name}}
        g['16']={'class_type':'ImagePadForOutpaint','inputs':{'image':['15',0],'left':W,'top':0,'right':0,'bottom':0,'feathering':0}}
        g['17']={'class_type':'VAEEncode','inputs':{'pixels':['16',0],'vae':['3',0]}}
        g['18']={'class_type':'SetLatentNoiseMask','inputs':{'samples':['17',0],'mask':['16',1]}}
        if revision:
            g.pop('16')
            g['17']['inputs']['pixels']=['15',0]
            g['18']['inputs']['mask']=['15',1]
        g['12']['inputs']['latent_image']=['18',0]
        g['10']['inputs'].update(width=W+CONTEXT,height=H)
        g['11']['inputs']['noise_seed']=2026101900+list(SOURCES).index(region)*10+step+(100 if revision else 0)
        g['14']['inputs']['filename_prefix']=f'KS-Connected-Regions-20261018/{region}-{step}{revision}'
        (folder/'workflow-api.json').write_text(json.dumps(g,indent=2),encoding='utf-8')
        (folder/'prompt.json').write_text(json.dumps(text,indent=2),encoding='utf-8')
        receipt.write_text(json.dumps(api('/prompt',{'prompt':g,'client_id':'codex-connected-regions'}),indent=2))
        pid=json.loads(receipt.read_text())['prompt_id']
    print(region,step,pid,flush=True)
    for attempt in range(720):
        h=api('/history/'+pid).get(pid)
        if h:
            (folder/'history.json').write_text(json.dumps(h,indent=2))
            if h.get('status',{}).get('status_str')=='error':raise RuntimeError(str(h['status']))
            images=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
            if images:
                im=images[0];shutil.copyfile(OUTPUT/im.get('subfolder','')/im['filename'],folder/'raw.png')
                generated=Image.open(folder/'raw.png').convert('RGB')
                joined=Image.new('RGB',(W+current.width,H));joined.paste(generated.crop((0,0,W,H)),(0,0));joined.paste(current,(W,0));joined.save(folder/'joined.png')
                print('Saved contiguous extension',region,step,joined.size,flush=True);return
        if attempt%12==0:print(region,step,'local outpaint active',flush=True)
        time.sleep(5)
    raise TimeoutError('Job receipt retained; do not resubmit.')

if __name__=='__main__':run(sys.argv[1],int(sys.argv[2]),sys.argv[3] if len(sys.argv)>3 else '')
