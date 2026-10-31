"""One local ComfyUI concept sheet, queued normally without changing other work."""
import json, pathlib, urllib.request, urllib.parse, shutil, sys

ROOT = pathlib.Path(__file__).resolve().parent
REPO = ROOT.parents[2]
BASE = REPO / 'Art/Pipeline/runs/hunters-sheet-20261015e/raw/option-1-workflow.json'
SERVER = 'http://127.0.0.1:8191'

def api(path, data=None):
    req = urllib.request.Request(SERVER+path, data=None if data is None else json.dumps(data).encode(), headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req, timeout=45) as response:
        return json.load(response)

def submit():
    receipt = ROOT/'submission.json'
    if receipt.exists():
        print(json.dumps({'existing_submission':json.loads(receipt.read_text())}), flush=True)
        return
    graph = json.loads(BASE.read_text(encoding='utf-8'))
    prompt = '''A single meticulously aligned sprite atlas of EIGHT square magical military command ability illustrations, exactly FOUR equal columns and TWO equal rows, evenly spaced on a uniform near-black violet background. Each tile is a self-contained crisp pixel-art emblem without a frame. All eight share a coherent alien cult aesthetic: polished black obsidian, aged pale gold, opalescent lavender light, carved angular ceremonial shapes, restrained muted violet. Sophisticated sinister alien science fantasy. Readable broad pixel clusters, precise stair stepped edges, high quality 2D pixel game art. NO writing, NO letters, NO numbers, NO humans. Symbols centered in each equal tile with generous empty margin. No icon crosses its tile. Palette black violet #14101b, shadow purple #33243c, grey lavender #a99ab7, old gold #bea174, parchment #eadfcb, dull blood red #ad6665. Soft controlled highlights, no bright neon.
TOP ROW FROM LEFT TO RIGHT: 1. A solid obsidian kite shield protected by a translucent pale violet hemispherical barrier, defensive ward. 2. A pair of pale sweeping retreat chevrons with small red embers being erased into dust, cleansing escape. 3. A stark golden targeting reticle enclosing a cracked crimson enemy chest plate, dangerous focused vulnerability. 4. Three ivory claw-shaped lightning slashes bursting forward through a muted red sun, violent speed burst.
BOTTOM ROW FROM LEFT TO RIGHT: 1. A black forearm guard deflecting a falling weapon in a golden angular impact star, forceful counter and stagger. 2. A long silver-violet swept arrow with three ghosted afterimages, swift evasive dash. 3. A single long golden needle piercing a cracked black shield, armour penetration. 4. An obsidian circular sun surrounded by four evenly spaced slow golden flame shapes, sustained controlled power.
The design fills a 1536 by 1024 landscape atlas: each of the eight cells is 384 by 512. Emblems occupy the central 250 by 300 of their cells, keeping the remaining margins uniformly empty. Balanced equal visual weight, no thick decorative borders. This is a UI concept sprite sheet for Kindred Siege, not a painting of a world or a background. Eight distinct illustrations, no extra objects.'''
    graph['4']['inputs']['text'] = prompt
    graph['5']['inputs']['text'] = 'text, words, letters, numbers, labels, watermark, logo, characters, people, faces, landscapes, medieval castle, bright neon, photorealism, glossy mobile game, bevelled plastic, gradient rainbow, ornate frame, illustration crossing tile boundaries, blurry, messy tiny symbols, extra rows, extra columns'
    graph['11']['inputs']['noise_seed'] = 2026102752
    graph['14']['inputs']['filename_prefix'] = 'KS-Shade-Commands-20261027/command-emblems'
    types = api('/object_info')
    missing = [v['class_type'] for v in graph.values() if v['class_type'] not in types]
    if missing:
        raise RuntimeError('Missing node types: '+repr(missing))
    (ROOT/'prompt.txt').write_text(prompt, encoding='utf-8')
    (ROOT/'workflow-api.json').write_text(json.dumps(graph,indent=2),encoding='utf-8')
    queue = api('/queue')
    result = api('/prompt', {'prompt':graph,'client_id':'codex-shade-commands-20261027'})
    receipt.write_text(json.dumps(result,indent=2),encoding='utf-8')
    print(json.dumps({'submission':result,'ahead_running':len(queue['queue_running']),'ahead_pending':len(queue['queue_pending']),'model':graph['1']['inputs']['unet_name']}), flush=True)

def status():
    receipt = json.loads((ROOT/'submission.json').read_text())
    pid = receipt['prompt_id']
    h = api('/history/'+pid).get(pid)
    if not h:
        q=api('/queue')
        pending=[x[1] for x in q['queue_pending']]
        ahead=pending.index(pid)+len(q['queue_running']) if pid in pending else 0
        print(json.dumps({'status':'queued_or_running','running':[x[1] for x in q['queue_running']],'pending_count':len(pending),'jobs_ahead':ahead,'our_prompt':pid}), flush=True)
        return
    (ROOT/'history.json').write_text(json.dumps(h,indent=2),encoding='utf-8')
    if h.get('status',{}).get('status_str') == 'error':
        print(json.dumps({'status':'error','messages':h['status'].get('messages',[])[-2:]}),flush=True)
        return
    files=[]
    for node in h.get('outputs',{}).values():
        for im in node.get('images',[]):
            query=urllib.parse.urlencode({'filename':im['filename'],'subfolder':im.get('subfolder',''),'type':im.get('type','output')})
            with urllib.request.urlopen(SERVER+'/view?'+query,timeout=30) as response:
                data=response.read()
            out=ROOT/'command-emblems-raw.png'
            out.write_bytes(data)
            files.append(str(out))
    print(json.dumps({'status':h.get('status',{}),'files':files}),flush=True)

if __name__=='__main__':
    status() if len(sys.argv)>1 and sys.argv[1]=='status' else submit()
