"""Bounded native terrain revision with local Flux; previous art remains intact."""
from pathlib import Path
import json, shutil, sys, time, urllib.request
from PIL import Image, ImageDraw, ImageFilter

ROOT=Path(__file__).resolve().parent
PRIOR=ROOT.parent
INPUT=Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-bloodridge-terrain-20261019')
OUTPUT=Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')
SERVER='http://127.0.0.1:8191'
GROVE=[(688,0),(881,0),(889,118),(1000,170),(1120,194),(1220,227),(1280,295),(1290,405),(1210,460),(1100,484),(955,520),(800,507),(635,470),(565,410),(574,355),(645,304),(667,211)]

def api(path,body=None):
    request=urllib.request.Request(SERVER+path,data=None if body is None else json.dumps(body).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(request,timeout=30) as response:return json.load(response)

def prepare():
    INPUT.mkdir(exist_ok=True)
    old=Image.open(PRIOR/'panorama.png').convert('RGB')
    # Large spatial masses, not a row of props. This guide is never a finished deliverable.
    im=Image.new('RGB',(4128,768),'#ad9686');d=ImageDraw.Draw(im)
    d.polygon([(0,162),(180,90),(320,117),(455,51),(650,100),(840,135),(1020,99),(1240,150),(1450,125),(1640,195),(1800,171),(2020,230),(2350,209),(2660,150),(3100,184),(3100,510),(0,510)],fill='#918075')
    # Low, distant mesas leave open sky at the start and through the central gorge.
    d.polygon([(0,245),(170,228),(260,260),(410,205),(545,250),(640,219),(780,315),(950,334),(1110,303),(1300,373),(1500,364),(1670,340),(1810,385),(2030,366),(2210,329),(2440,340),(2620,272),(2920,309),(3100,330),(3100,600),(0,600)],fill='#79665e')
    # Front bank dips into the grove, then climbs along the lip of a deep ravine.
    d.polygon([(0,328),(180,294),(330,352),(470,347),(610,409),(760,490),(1010,518),(1240,533),(1450,456),(1690,433),(1920,406),(2180,382),(2450,366),(2710,340),(3008,305),(3008,768),(0,768)],fill='#9d8170')
    # The ravine is a deep diagonal opening behind the walkable shelf, not a tiled wall.
    d.polygon([(1320,95),(1420,199),(1510,238),(1635,268),(1795,241),(1930,192),(2050,97),(2180,70),(2290,125),(2200,266),(2100,309),(1940,341),(1780,365),(1630,373),(1500,407),(1370,407),(1300,345)],fill='#554a48')
    d.polygon([(1450,134),(1515,229),(1600,267),(1730,289),(1890,253),(2040,177),(2140,113),(2180,169),(2080,270),(1925,311),(1740,333),(1570,340),(1465,370),(1400,327)],fill='#77675f')
    d.line([(1325,436),(1470,434),(1670,405),(1880,379),(2060,354),(2290,337),(2520,328),(2780,318),(3008,305)],fill='#c0a18a',width=18)
    # An eroded tilted slab on the far lip makes the change in height readable.
    d.polygon([(2170,270),(2270,216),(2325,109),(2300,34),(2380,21),(2460,131),(2410,232),(2530,312),(2420,337),(2280,317)],fill='#6d5750')
    d.line([(2330,69),(2395,164),(2340,264),(2450,313)],fill='#a88a70',width=13)
    # A single root follows the rock lip from the grove towards the court roots.
    d.line([(1150,357),(1290,404),(1480,420),(1630,393),(1830,369),(2050,342),(2240,354),(2470,328),(2690,350),(2850,336)],fill='#6c5344',width=23)
    d.line([(1150,347),(1290,395),(1480,411),(1630,384),(1830,360),(2050,333),(2240,345),(2470,319),(2690,341),(2850,327)],fill='#ad9174',width=7)
    # Only the final approach acquires burial masonry, embedded in the cliff.
    d.polygon([(2580,106),(2740,127),(2785,322),(2540,342)],fill='#7e6758')
    d.polygon([(2620,170),(2695,178),(2710,301),(2600,309)],fill='#51433f')
    d.line([(2520,377),(2630,362),(2750,369),(2860,333),(3008,315)],fill='#b59a81',width=11)
    # Continuous worn silt and bedrock, never stamped source-floor tiles.
    d.line([(0,470),(250,424),(450,480),(660,540),(890,592),(1150,589),(1450,566),(1680,531),(1910,507),(2220,491),(2470,461),(2700,466),(3008,438)],fill='#8a7064',width=54)
    d.line([(0,451),(250,405),(450,461),(660,521),(890,573),(1150,570),(1450,547),(1680,512),(1910,488),(2220,472),(2470,442),(2700,447),(3008,419)],fill='#ad8e79',width=10)
    # Uneven near edge exposes the cliff, changing the shape of the landmass.
    d.polygon([(0,655),(180,626),(350,681),(510,673),(670,733),(860,768),(0,768)],fill='#645046')
    d.polygon([(1290,768),(1430,695),(1640,681),(1790,713),(1930,668),(2120,697),(2270,653),(2430,681),(2540,727),(2730,717),(2880,768)],fill='#57463f')
    d.line([(1440,695),(1640,681),(1790,713),(1930,668),(2120,697),(2270,653),(2430,681),(2540,727),(2730,717)],fill='#a58870',width=18)
    # Grouped flora at physical banks, not evenly repeated wallpaper stamps.
    for x,y,s in [(85,355,34),(430,374,20),(570,449,37),(1510,411,52),(1550,419,35),(2210,356,41),(2280,354,26),(2490,357,24)]:
        d.line([(x,y),(x-8,y-s)],fill='#4f453b',width=5)
        for sign in [-1,1]:d.polygon([(x,y),(x+sign*s,y-s),(x+sign*s//5,y-s//3)],fill='#a68c6d')
        d.ellipse((x-12,y-s-9,x+2,y-s+8),fill='#95573f')
    # Retain the actual grove and the original court destination.
    grove=Image.new('L',im.size,0);ImageDraw.Draw(grove).polygon(GROVE,fill=255)
    im.paste(old,(0,0),grove)
    im.paste(old.crop((2752,0,4128,768)),(2752,0))
    im.save(ROOT/'layout-guide.png');im.save(INPUT/'layout-guide.png')
    old.crop((360,0,1736,768)).save(INPUT/'grove-style.png')
    protection=grove.copy();ImageDraw.Draw(protection).rectangle((3008,0,4127,767),fill=255)
    protection.save(ROOT/'protected.png')
    masked=im.convert('RGBA');masked.putalpha(protection);masked.save(INPUT/'masked-guide.png')
    return protection

PROMPT='''Detailed pixel art panoramic 2.5D battle environment, native fine pixel clusters, muted dusty sepia, corroded warm bronze, violet-brown crevice shadows, calcified pale roots. Same art, light and oblique camera as the supplied root grove. The second reference is an exact GEOGRAPHY PLAN: follow its different silhouettes and land masses. Render one physically continuous long Bloodridge region. Important: only the FAR RIGHT is an Egyptian burial court. Most of the new region is wild wind-eroded natural terrain, with NO courtyard paving or walls.

LEFT QUARTER: spacious bare ash BADLANDS beneath open sky, low weathered sandstone mesas with rounded eroded profiles, a shallow dry wash curving down into the preserved hollow root grove. An exposed tilted rock shelf and a few dry thorn clumps with rust-red pods. Sparse and natural. No buildings, pillars, gates, stairs or brickwork in the left half. The hollow tree and its dark pool stay as the sheltered landmark.

MIDDLE: after the grove the land opens onto a DEEP WIND-CARVED RAVINE with layered cliff faces dropping into violet dust, distant broken mesas visible across the gap. A broad solid fighting shelf curves along the near lip and visibly RISES towards the right. An imposing leaning split sandstone blade on the far lip, roots hanging over the dark cut. The rear cliff has large irregular heights and gaps, no line of buildings. Foreground bottom-edge broken bedrock shows the thickness of the shelf and a steep drop. Alternating enclosure: open desert, sheltered grove, open high ravine, then enclosed court.

RIGHT APPROACH: the ravine shelf becomes an uneven burial terrace. ONE half-buried small tomb cut into the rock, a collapsed stair following the incline, roots crawling through the bank and into the EXISTING ORIGINAL COURT at the far right. Only there should carved Egyptian stone and paving take over. The continuous shallow silt wash becomes worn court paving gradually. Keep the actual final court gate and paving intact.

Physical connections must be visible: ash bank descending into the grove, the same roots gripping the near ravine lip, the same broad bedrock shelf climbing into the court. Ground material changes from loose ash to exposed layered rock to irregular old stone. No evenly repeating floor slabs, rows of tombs, same-height walls, tiled backdrop, panels, collage joins or mirrored rocks. Detailed weathered forms with calm fighter clearings, not noisy photorealism, not smooth painted concept art, not 3D models. No new people, no text or UI. Regional flora is grouped into small pale thorn fans, dull rust seedpods and reeds near the existing dark pool, not evenly scattered. Keep the real root grove and original right court unchanged; replace the repetitive architecture between them with this natural ravine.'''

def submit():
    receipt=ROOT/'submission.json'
    if receipt.exists():return json.loads(receipt.read_text())['prompt_id']
    prepare()
    graph=json.loads((PRIOR/'workflow-api.json').read_text())
    graph['7']['inputs']['image']='ks-bloodridge-terrain-20261019/grove-style.png'
    graph['20']['inputs']['image']='ks-bloodridge-terrain-20261019/masked-guide.png'
    graph['5']['inputs']['text']=PROMPT
    graph['6']['inputs']['text']='repeating courtyard walls, rows of tombs, endless paving, seamless wallpaper tile, equal-height architecture, generic sand, realistic photograph, shiny 3D scene, cutout panels'
    graph['16']['inputs']['noise_seed']=20261019143
    graph['19']['inputs']['filename_prefix']='KS-Bloodridge-Terrain-20261019/region'
    (ROOT/'workflow-api.json').write_text(json.dumps(graph,indent=2))
    (ROOT/'prompt.txt').write_text(PROMPT)
    result=api('/prompt',{'prompt':graph,'client_id':'codex-bloodridge-terrain'})
    receipt.write_text(json.dumps(result,indent=2));return result['prompt_id']

def collect(pid):
    for i in range(720):
        history=api('/history/'+pid).get(pid)
        if history:
            (ROOT/'history.json').write_text(json.dumps(history,indent=2))
            if history.get('status',{}).get('status_str')=='error':raise RuntimeError(history['status'])
            images=[im for out in history.get('outputs',{}).values() for im in out.get('images',[])]
            if images:
                im=images[0];shutil.copyfile(OUTPUT/im.get('subfolder','')/im['filename'],ROOT/'raw.png')
                print('Native terrain revision saved',flush=True);return
        if i%12==0:
            q=api('/queue');jobs=q.get('queue_running',[])+q.get('queue_pending',[])
            own=[j for j in jobs if j[1]==pid]
            if own:
                ahead=sum(j[0]<own[0][0] for j in jobs)
                print(json.dumps({'prompt_id':pid,'ahead':ahead,'running':any(j[1]==pid for j in q.get('queue_running',[]))}),flush=True)
        time.sleep(5)
    raise TimeoutError('Receipt retained; resume instead of resubmitting')

if __name__=='__main__':
    if '--guide-only' in sys.argv:prepare()
    else:
        prompt_id=submit();print(prompt_id,flush=True);collect(prompt_id)
