"""One composition-led local background study; preserves the original source court."""
from pathlib import Path
import json, shutil, sys, time, urllib.request
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[2]
COURT = Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/comparison/bloodridge-reference.png')
INPUT = Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-bloodridge-composition-20261019')
OUTPUT = Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')
SERVER = 'http://127.0.0.1:8191'

def api(path, body=None):
    req = urllib.request.Request(SERVER+path, data=None if body is None else json.dumps(body).encode(), headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req, timeout=30) as response:
        return json.load(response)

def guide():
    # This is an input layout drawing, never presented as a finished background.
    im = Image.new('RGB', (4128,768), '#b09a87')
    d = ImageDraw.Draw(im)
    d.polygon([(0,150),(220,105),(540,148),(700,96),(940,145),(1220,70),(1510,112),(1740,42),(2000,88),(2240,38),(2510,110),(2840,35),(2840,420),(0,420)], fill='#8c7b71')
    # A single connected eroded bank changes enclosure around the clearings.
    d.polygon([(0,253),(180,222),(360,247),(570,192),(780,245),(1000,210),(1190,290),(1370,310),(1510,260),(1650,285),(1870,210),(2020,178),(2200,210),(2370,155),(2550,180),(2810,240),(2810,630),(0,630)], fill='#695a55')
    d.polygon([(0,344),(250,317),(510,355),(790,333),(1050,410),(1300,465),(1540,472),(1800,420),(2060,370),(2310,345),(2550,325),(2810,310),(2810,768),(0,768)], fill='#92796a')
    # Shallow silt channel winds through the ground instead of a repeated paving grid.
    d.line([(0,625),(350,548),(650,590),(940,530),(1180,558),(1410,485),(1650,472),(1870,410),(2200,426),(2490,345),(2810,355)], fill='#78655e', width=47)
    d.line([(0,603),(350,529),(650,571),(940,512),(1180,540),(1410,466),(1650,454),(1870,391),(2200,407),(2490,326),(2810,336)], fill='#ae9380', width=12)
    # The middle landmark: one hollow mineral-root tree gripping the ravine bank.
    d.polygon([(1050,360),(1140,310),(1190,208),(1160,100),(1190,36),(1240,0),(1310,0),(1265,100),(1280,188),(1390,230),(1480,190),(1580,85),(1615,46),(1640,55),(1610,134),(1510,270),(1450,310),(1540,345),(1660,364),(1660,388),(1470,374),(1370,331),(1320,270),(1280,251),(1235,330),(1140,371)], fill='#463c3a')
    d.line([(1225,296),(1302,219),(1395,260),(1475,324),(1600,364),(1830,330),(2040,282),(2260,290),(2490,253),(2780,306)], fill='#6c5749', width=22)
    d.line([(1293,210),(1238,134),(1280,32)], fill='#98806a', width=10)
    d.line([(1395,259),(1510,220),(1598,106)], fill='#92745c', width=9)
    # A sheltered dark pool behind the middle battle clearing, not an obstacle in it.
    d.polygon([(1330,345),(1415,324),(1560,327),(1660,355),(1600,379),(1460,389),(1340,374)], fill='#534c49')
    d.line([(1390,356),(1490,346),(1590,357)], fill='#9c836a', width=6)
    # Deep rear cut of the ravine and a curving root shelf break the flat rear strip.
    d.polygon([(1030,379),(1190,405),(1340,386),(1500,401),(1640,370),(1750,392),(1880,364),(1940,393),(1840,430),(1650,453),(1480,440),(1330,455),(1180,434)], fill='#554945')
    d.polygon([(1120,360),(1240,366),(1380,349),(1510,368),(1650,345),(1780,368),(1890,345),(1940,366),(1780,392),(1610,382),(1460,401),(1310,386),(1170,403),(1080,387)], fill='#887361')
    d.line([(1180,372),(1320,368),(1460,382),(1610,361),(1740,382),(1900,355),(2150,328),(2400,333),(2780,304)], fill='#a38a6c', width=15)
    # The pool sits inside the root shelf, visible behind the open clearing.
    d.polygon([(1320,398),(1430,390),(1540,401),(1630,394),(1660,417),(1550,437),(1420,431),(1320,420)], fill='#534c49')
    d.line([(1390,413),(1490,407),(1590,416)], fill='#9c836a', width=6)
    # Tomb terraces are embedded in the same bank and root system toward the court.
    for x,y,w,h in [(1940,180,210,142),(2200,145,175,168),(2440,175,210,139)]:
        d.polygon([(x,y+24),(x+w-24,y),(x+w,y+h),(x,y+h)], fill='#746252')
        d.rectangle((x+28,y+48,x+w-40,y+h), fill='#4f4441')
        d.line([(x,y+h),(x+w,y+h)], fill='#b0997e', width=7)
    # Proposed regional flora: small pale thorn leaves and rusty seedpod clusters.
    for x,y,size in [(110,350,32),(390,323,28),(760,341,46),(1150,338,51),(1335,345,42),(1600,341,55),(1760,362,33),(2070,337,37),(2400,329,44)]:
        d.line([(x,y+15),(x+2,y-size)], fill='#50453e', width=5)
        for sign in [-1,1]:
            d.polygon([(x,y),(x+sign*size,y-size//2),(x+sign*size//3,y-size//3)], fill='#a08a71')
        d.ellipse((x-6,y-size-9,x+6,y-size+3), fill='#9c5940')
    # Foreground roots and scrub frame the region; central fighter silhouettes stay clear.
    d.polygon([(0,768),(0,530),(80,565),(150,560),(260,660),(345,647),(530,740),(730,768)], fill='#51433f')
    d.line([(0,727),(230,743),(355,720),(520,767)], fill='#78604f', width=24)
    d.polygon([(980,768),(1080,685),(1180,671),(1290,695),(1400,634),(1480,646),(1580,685),(1740,768)], fill='#5d4b42')
    d.line([(1110,768),(1280,751),(1445,709),(1600,741),(1740,768)], fill='#8b7058', width=17)
    d.polygon([(2130,768),(2280,735),(2450,752),(2600,705),(2800,748),(2800,768)], fill='#59483e')
    for x,y,size in [(90,640,98),(230,705,76),(1050,752,60),(1430,735,88),(1550,765,65),(2320,752,55),(2620,746,61)]:
        for sign,dx,dy in [(-1,1,1),(1,1,1),(-1,.7,1.5),(1,.6,1.35)]:
            d.polygon([(x,y),(int(x+sign*size*dx),int(y-size*dy)),(int(x+sign*size*.25),int(y-size*.3))], fill='#7b6a54')
        d.line([(x,y),(x+7,y-size)],fill='#514236',width=7)
        d.ellipse((x-11,y-size-17,x+11,y-size+5),fill='#a36243')
    court = Image.open(COURT).convert('RGB')
    im.paste(court,(2752,0))
    im.save(ROOT/'layout-guide.png')
    im.save(INPUT/'layout-guide.png')
    court.save(INPUT/'original-court.png')
    alpha = Image.new('L',im.size,0)
    ImageDraw.Draw(alpha).rectangle((3008,0,4127,767),fill=255)
    latent_guide = im.convert('RGBA'); latent_guide.putalpha(alpha)
    latent_guide.save(INPUT/'masked-guide.png')
    return im

def submit():
    INPUT.mkdir(exist_ok=True)
    receipt = ROOT/'submission.json'
    if receipt.exists():
        return json.loads(receipt.read_text())['prompt_id']
    layout = guide()
    g = json.loads((REPO/'Art/Studies/Connected-Regions-Natural-20261019/bloodridge/flux-edit/workflow-api.json').read_text())
    g['7']['inputs']['image'] = 'ks-bloodridge-composition-20261019/original-court.png'
    g['20']['inputs']['image'] = 'ks-bloodridge-composition-20261019/masked-guide.png'
    # Image 1 supplies the rendering; image 2 supplies the connected composition.
    g['25']={'class_type':'ImageScale','inputs':{'image':['20',0],'upscale_method':'area','width':1376,'height':256,'crop':'disabled'}}
    g['26']={'class_type':'VAEEncode','inputs':{'pixels':['25',0],'vae':['4',0]}}
    g['23']={'class_type':'ReferenceLatent','inputs':{'conditioning':['10',0],'latent':['26',0]}}
    g['24']={'class_type':'ReferenceLatent','inputs':{'conditioning':['11',0],'latent':['26',0]}}
    g['13']['inputs'].update(positive=['23',0],negative=['24',0],cfg=4.0)
    prompt = '''pixel art environment, fine native pixel clusters. Image 1 is the original Bloodridge court and defines the rendering, weathered materials, muted sepia and violet-brown shadows, light and camera. Image 2 is only a rough composition plan: replace its crude painted shapes with finished detailed scenery. Make ONE continuous 2.5D landscape from the ash outskirts on the LEFT to the original burial court on the RIGHT, at the same native scale.

The left begins with wind-carved ash banks, embedded old grave stones, sparse pale thorn plants, rusty red seedpods and a shallow dry silt channel. It is bare but has character and nature. Farther right the SAME bank folds into a root-choked ravine. One enormous hollow calcified root tree leans over a sheltered dark pool behind the middle clearing: twisted split bark, ivory dead branches, warm rust-red pods hanging underneath, reed clumps and climbing roots gripping the rock. A few tiny dark scavenger silhouettes perch far back; small insects gather around seedpods. These are quiet background details.

The SAME roots continue visibly rightward through eroded burial terraces, wrapping niches and old corroded bronze fittings, and become the roots already growing into the original court. The SAME silt channel curves along the whole region and fades into the worn court paving. Rear silhouettes change from open vistas to the intimate root hollow to tall burial architecture. Foreground root tangles, bone-white scrub and broken near-edge stone frame the lower corners. All shapes belong to one connected physical place; no rows of pasted props or repeated scenery.

Keep readable level fighting clearings around the middle-lower part, with varied ground materials and organic edges. Rich natural scenery surrounds those clearings instead of being squeezed into a thin strip. Same oblique battle camera everywhere. Match the original court's worn rounded forms, layered native pixel detail and atmospheric depth. Keep its actual main gate and court design as the destination. No humans, combat creatures, text, panels, tiling, clean brick walls, graphic black outlines or shiny 3D surfaces.'''
    g['5']['inputs']['text'] = prompt
    g['16']['inputs']['noise_seed'] = 2026101991
    g['15']['inputs']['steps'] = 20
    g['19']['inputs']['filename_prefix'] = 'KS-Bloodridge-Composition-20261019/region'
    (ROOT/'workflow-api.json').write_text(json.dumps(g,indent=2))
    (ROOT/'prompt.txt').write_text(prompt)
    r = api('/prompt',{'prompt':g,'client_id':'codex-bloodridge-composition'})
    receipt.write_text(json.dumps(r,indent=2))
    return r['prompt_id']

def collect(pid):
    for n in range(720):
        h = api('/history/'+pid).get(pid)
        if h:
            (ROOT/'history.json').write_text(json.dumps(h,indent=2))
            if h.get('status',{}).get('status_str') == 'error':
                raise RuntimeError(h['status'])
            images = [im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
            if images:
                im=images[0]
                shutil.copyfile(OUTPUT/im.get('subfolder','')/im['filename'],ROOT/'raw.png')
                print('Saved raw region',flush=True)
                return
        if n%12 == 0: print('Local composition study queued or rendering',flush=True)
        time.sleep(5)
    raise TimeoutError('Receipt retained; do not resubmit')

if __name__ == '__main__':
    if '--guide-only' in sys.argv:
        INPUT.mkdir(exist_ok=True); guide()
    else:
        pid=submit(); print(pid,flush=True)
        collect(pid)
