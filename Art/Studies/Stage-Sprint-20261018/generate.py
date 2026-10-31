"""Two review-only local stages; preserve Claude's graph and never interrupt the queue."""
import json, pathlib, time, urllib.request, shutil
ROOT=pathlib.Path(__file__).resolve().parent
REPO=ROOT.parents[2]
BASE=REPO/'Art/Pipeline/runs/hunters-sheet-20261015e/raw/option-1-workflow.json'
SERVER='http://127.0.0.1:8191'
OUTPUT=pathlib.Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')
def api(path,body=None):
    req=urllib.request.Request(SERVER+path,data=None if body is None else json.dumps(body).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=30) as r:return json.load(r)
style='A finished 2.5D pixel art battle background, one continuous scene, 1376 by 768 pixels. Elevated three-quarter SIDE battle view, never overhead or isometric diamond. Hand-placed irregular pixel clusters, carefully chipped edges, mottled broad stone shapes, restrained desaturated palette, uneven ancient materials, the visual density of a weathered burial court in an ambitious pixel RPG. No clean repeating grid or plastic 3D blocks. Light from upper left. The entire lower two thirds is one continuous broad flat walkable fighting floor with subdued low-contrast texture; the middle and lower thirds stay clear for formations. A strong asymmetric regional landmark in the upper third, layered enclosing edges and modest foreground corner framing. No characters, UI or text.'
subjects={
 'skullpeak':'The Skullpeak mountain clan\'s iron oath court: a dark weathered iron gate set into rough slate crags along the upper edge, narrow steps on the far right rising to the gate, torn dark wine banners lashed to old iron braces, snow caught only in recesses and along the outer edges. The fighting ground is worn blue-grey stone with cracked irregular slabs and sparse frost, not a white tile floor. Muted icy slate, old iron, bone snow and small oxblood cloth accents; a few distant cold peaks above the gate. Quiet pale grey middle ground gives dark fighters clear silhouettes.',
 'hunters':'An alien mercenary landing ground on a war-torn lawless space wasteland, distinct from a red canyon: broad ash-grey ground of fractured old basalt and fine pale dust, surrounded at the upper and outer edges by the ribbed black wrecks of defeated starships, an asymmetrical collapsed engine arch on the far left, distant jagged obsidian escarpments and tiny dim violet seams inside broken machinery. Empty open duelling ground in the lower two thirds, no machinery in the fighting lanes. Muted ash, worn charcoal, cold mauve shadows and very restrained violet glints. Salvaged alien technology, no medieval castle, no red desert canyon.'}
for n,(name,subject) in enumerate(subjects.items()):
    folder=ROOT/name;folder.mkdir(exist_ok=True)
    receipt=folder/'submission.json'
    if receipt.exists():pid=json.loads(receipt.read_text())['prompt_id']
    else:
        q=api('/queue')
        if q['queue_running'] or q['queue_pending']:raise RuntimeError('Queue occupied; leave it untouched.')
        g=json.loads(BASE.read_text())
        g['4']['inputs']['text']=json.dumps({'high_level_description':style+' '+subject,'compositional_deconstruction':{'background':subject,'elements':[{'type':'obj','bbox':[0,0,1000,1000],'desc':style+' '+subject}]}})
        g['5']['inputs']['text']='photorealism, 3d render, plastic, voxel blocks, clean geometry, cutout islands, tiled checkerboard floor, character, people, creatures, text, HUD, labels, panels, spritesheet, busy central floor, blurred pixels, smooth gradients, cinematic depth of field, watermark'
        for node in ('8','10'):g[node]['inputs'].update(width=1376,height=768)
        g['11']['inputs']['noise_seed']=2026101825+n
        g['14']['inputs']['filename_prefix']='KS-Stage-Sprint-20261018/'+name
        (folder/'workflow-api.json').write_text(json.dumps(g,indent=2))
        receipt.write_text(json.dumps(api('/prompt',{'prompt':g,'client_id':'codex-stage-sprint'}),indent=2))
        pid=json.loads(receipt.read_text())['prompt_id']
    print(name+' '+pid,flush=True)
    for attempt in range(480):
        h=api('/history/'+pid).get(pid)
        if h:
            (folder/'history.json').write_text(json.dumps(h,indent=2))
            if h.get('status',{}).get('status_str')=='error':raise RuntimeError(str(h['status']))
            images=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
            if images:
                im=images[0];shutil.copyfile(OUTPUT/im.get('subfolder','')/im['filename'],folder/(name+'.png'))
                print('Saved '+name,flush=True);break
        if attempt%6==0:print(name+' local generation active',flush=True)
        time.sleep(5)
    else:raise TimeoutError('Retained job ID; do not submit another job.')
