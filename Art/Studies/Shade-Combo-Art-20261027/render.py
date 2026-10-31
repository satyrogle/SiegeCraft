"""One isolated local ComfyUI illustrated screen; never alters existing jobs."""
from pathlib import Path
import json, urllib.request, urllib.parse, sys

ROOT=Path(__file__).resolve().parent
SERVER='http://127.0.0.1:8191'
def api(path,data=None):
    req=urllib.request.Request(SERVER+path,data=None if data is None else json.dumps(data).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=45) as r:return json.load(r)

def submit():
    if (ROOT/'submission.json').exists():
        print('Already submitted. Use status.');return
    graph=json.loads((ROOT.parent/'Shade-Commands-20261027/workflow-api.json').read_text())
    prompt='''Create a beautifully art-directed finished illustrated video game mechanic teaching screen, landscape 1536x1024. ONE continuous hand-painted science-fantasy artwork, dramatic graphic silhouettes, confident inked shapes and painterly edges, beautiful restrained material rendering. This is a premium game artist's painted interface concept, NOT a wireframe, NOT a webpage, NOT a grid of cards. No boxes or rectangular panels anywhere. The physical illustration itself carries the progression, like a painted branching tree carrying the choices in a beautiful strategy game. Original Kindred Siege alien cult identity: obsidian-black living membranes, restrained old-gold seams, opalescent lavender technology, ancient but impossibly advanced. Dark ink-violet negative space, warm ivory text, dull rust-red enemy contrast. No neon, no cyberpunk cables, no medieval castle, no cherry blossoms. The composition teaches ONE combination through a strong illustrated upper equation and a HUGE lower combat result.

COMPOSITION: Upper 12 percent is quiet near-black ink wash with small spaced text 'KINDRED SIEGE' and the readable main title 'PARRY + WARD'. Below, the upper third has TWO large free-floating illustrated ingredients connected by a single elegant ivory PLUS sign at the exact center, with generous space. The left ingredient at x330 y285 is a dynamic close-up of a black-and-gold plated forearm turning aside an incoming rusty curved enemy blade: one crisp ivory-gold deflection spark, readable physical contact. Under this close-up, clearly written 'LAND THE PARRY'. The right ingredient at x1080 y280 is an extraordinary dark obsidian seed-like ward relic opening its black membranes around a pale lavender protective field, sculptural and physical, elegant asymmetry. Under it, clearly written 'WARD ENHANCEMENT'. No icons in circles, no cards, these are large painted objects in the same artwork. A subtle sweeping root of black lacquer and old-gold light joins these two ingredients, curving downward into the outcome, directing the eye.

THE BIG PAYOFF: The entire lower half is a single enormous dynamic three-quarter-side-view battle demonstration. Four distinct small-war-party allied alien fighters in black draped cult regalia and old-gold details stand together facing RIGHT on an understated dark slab of ground. One front warrior, a shorter shield-bearer, a rear rifle bearer and a staff wielder are all clearly distinct, physically grounded, spaced with readable silhouettes, no overlapping bodies. A SINGLE beautifully illustrated translucent PALE VIOLET protective membrane spreads from the front fighter and wraps around ALL FOUR allied fighters together. Its front-right edge catches the next blade strike with an ivory-gold impact; allies inside remain unharmed and visible. Outside the membrane on the far right stands ONE aggressive tall rust-red wrapped desert tribal warrior with a curved weapon. The enemy remains upright, balanced and dangerous, NOT knocked down and NOT staggered. The allied protection is the largest, brightest focal point on the entire screen. Beautiful decisive action art, readable shield boundary, no excessive particles, no numbers, no health bars, no explosions, no gore. The enemy's orange edge contrasts with the lavender squad ward. A clear small visual trail links the deflection close-up above to this new protective squad membrane below.

At the lower left beside the battle, legible large ivory words 'THE SQUAD GETS A SHIELD'. At the very bottom on a quiet dark painted margin, one short line in muted warm red: 'TRADEOFF: NO STAGGER OPENING'. Tiny footer 'SHADE - ONE ENHANCEMENT SLOT'. NO other writing. Text large and sparse. No menus, no statistics, no repeated information, no square containers, no spreadsheet, no branching mechanics, no talent tree nodes. This is a single cohesive atmospheric illustrated game screen teaching cause and effect. The art, not framing, creates hierarchy. Make it gorgeous and instantly understandable: deflect the hit PLUS Ward enhancement EQUALS protected squad, enemy still standing. Painted art with deliberate broad shapes and precise controlled edges suitable for a sophisticated pixel-art game's illustrated menus.''' 
    graph['4']['inputs']['text']=prompt
    graph['5']['inputs']['text']='cards, boxes, rectangles, web dashboard, spreadsheet, mockup wireframe, ornate borders, tiny unreadable paragraphs, excessive particles, neon blue, generic medieval knight, anime schoolgirl, cherry blossom, tree icons, crowd, extra arms, dead enemy, knocked down enemy, blood, logo watermark'
    graph['11']['inputs']['noise_seed']=2026102761
    graph['14']['inputs']['filename_prefix']='KS-Shade-Combo-Art-20261027/parry-ward'
    (ROOT/'prompt.txt').write_text(prompt,encoding='utf-8')
    (ROOT/'workflow-api.json').write_text(json.dumps(graph,indent=2),encoding='utf-8')
    types=api('/object_info')
    assert all(n['class_type'] in types for n in graph.values())
    q=api('/queue')
    result=api('/prompt',{'prompt':graph,'client_id':'codex-shade-combo-art-20261027'})
    (ROOT/'submission.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
    print(json.dumps({'submission':result,'ahead':len(q['queue_running'])+len(q['queue_pending'])}),flush=True)

def status():
    pid=json.loads((ROOT/'submission.json').read_text())['prompt_id']
    h=api('/history/'+pid).get(pid)
    if not h:
        q=api('/queue');pending=[j[1] for j in q['queue_pending']]
        print(json.dumps({'running':any(j[1]==pid for j in q['queue_running']),'jobs_ahead':pending.index(pid)+len(q['queue_running']) if pid in pending else 0}));return
    (ROOT/'history.json').write_text(json.dumps(h,indent=2),encoding='utf-8')
    outputs=[]
    for n in h.get('outputs',{}).values():
        for im in n.get('images',[]):
            qs=urllib.parse.urlencode({'filename':im['filename'],'subfolder':im.get('subfolder',''),'type':im.get('type','output')})
            with urllib.request.urlopen(SERVER+'/view?'+qs,timeout=30) as r:(ROOT/'parry-ward-art.png').write_bytes(r.read())
            outputs.append(str(ROOT/'parry-ward-art.png'))
    print(json.dumps({'status':h.get('status',{}),'files':outputs}),flush=True)

if __name__=='__main__':
    status() if len(sys.argv)>1 and sys.argv[1]=='status' else submit()
