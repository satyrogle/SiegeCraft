"""Local review candidates. Original stages and shipped assets remain untouched."""
import json, pathlib, time, urllib.request, shutil
ROOT=pathlib.Path(__file__).resolve().parent
REPO=ROOT.parents[2]
BASE=REPO/'Art/Pipeline/runs/hunters-sheet-20261015e/raw/option-1-workflow.json'
SERVER='http://127.0.0.1:8191'
OUTPUT=pathlib.Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')
def api(path,body=None):
    req=urllib.request.Request(SERVER+path,data=None if body is None else json.dumps(body).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=30) as r:return json.load(r)

style={
 'medium':'pixel_art',
 'art_style':'Authentic hand-pixelled 2D game background. Visible square pixel clusters and stair-stepped contours everywhere. Muted limited palette, flat colour clusters, two or three discrete shades per stone. Organic uneven shapes with carefully chipped corners. Broad quiet surfaces with occasional short angular cracks. Crisp pixel edges without antialiasing. The controlled pixel craft of an old burial court: irregular broad paving stones, shallow angular shadows, weathering only on selected edges. No paint brushwork, no gritty texture layer, no dithering, no gradients.',
 'aesthetics':'quiet, restrained, ancient, tactile, subdued, coherent pixel clusters, readable 2.5D battle stage',
 'lighting':'Soft upper-left light expressed as discrete flat pixel colour planes. Even readable midtone fighting ground. Subtle contact shadows, no dramatic vignette.'
}
floor='The lower two thirds form one wide open flat fighting court. Large irregular worn slabs spanning about a tenth of the screen width, with broad nearly solid colour faces. Only occasional short broken seams and a few small chips, much empty midtone area for fighter silhouettes. Oblique side view looking slightly down at the ground, like a 2D RPG battle screen. The floor reaches all four side and bottom edges; no isolated platform, no border, no foreground fence.'
subjects={
 'skullpeak':{
  'description':'Empty Skullpeak mountain clan oath court, rendered entirely in crisp, deliberately clustered pixel art. Blue-grey slate and iron, sparse old wine-red cloth, bone-white snow confined to edge ledges.',
  'rear':'Across the upper third, an uneven old slate retaining wall and broad low steps. On the right, a squat heavy dark iron oath gate with a blunt skull-shaped keystone carved into the stone, framed by two battered iron braces and one hanging rag of wine-red cloth. On the left, a lower crumbled wall reveals a pale flat silhouette of a distant mountain. Snow rests in a few broad solid ivory patches on the outer ledges, never scattered across the fighting court. All rock shapes are simplified broad angular pixel clusters, not finely faceted.',
  'palette':['#292933','#42404b','#5b5965','#777983','#96979b','#b8b7b0','#d2cbbb','#61464e'],
  'ground':'The paving is dusty desaturated slate grey with a slight lavender undertone. Flat stone surfaces are a quiet middle grey; only a few dark broken seams. No cobbles, gravel or granular frost.'},
 'hunters':{
  'description':'Empty alien hunters landing court in a lawless space wasteland, rendered entirely in crisp, deliberately clustered pixel art. Muted ash-grey and mauve ground, broken obsidian starship ribs and sparse dim violet seams.',
  'rear':'Across the upper third, a broken starship hull embedded into a low basalt ridge. On the left, one great incomplete ribbed engine arch leans into the old wreck with a heavy torn black cable hanging beside it. Three broad irregular hull plates recede to the right, with just a few dim muted violet cuts in their seams. The distant horizon is a low flat charcoal escarpment against a dusty pale mauve sky. Salvaged alien technology, not medieval fortifications. Wreckage is rendered in large flat pixel planes and stepped silhouettes, with a few selected chips, no intricate greebles.',
  'palette':['#29272f','#413e49','#5a535f','#78717c','#969098','#b1a9a8','#cbc0b5','#7f628d'],
  'ground':'The court is a broad old landing apron made of worn irregular basalt plates, buried shallowly in pale ash. Dusty pale grey with mauve shadows, large nearly solid middle-value stone faces. No scattered gravel, no glittering sand, no tiny stones.'}
}
for n,(name,subject) in enumerate(subjects.items()):
    folder=ROOT/name;folder.mkdir(exist_ok=True)
    receipt=folder/'submission.json'
    if receipt.exists():pid=json.loads(receipt.read_text())['prompt_id']
    else:
        q=api('/queue')
        if q['queue_running'] or q['queue_pending']:raise RuntimeError('Queue occupied; left untouched.')
        g=json.loads(BASE.read_text())
        caption={'high_level_description':subject['description']+' One single continuous 2.5D battle scene, no figures, no text.',
                 'style_description':dict(style,color_palette=subject['palette']),
                 'compositional_deconstruction':{'background':floor+' '+subject['ground'],
                 'elements':[{'type':'obj','bbox':[0,0,360,1000],'desc':subject['rear']},
                             {'type':'obj','bbox':[360,0,1000,1000],'desc':floor+' '+subject['ground']}]}}
        g['4']['inputs']['text']=json.dumps(caption)
        g['5']['inputs']['text']='photograph, painterly, digital painting, concept painting, brush strokes, realistic render, 3D render, noisy texture, fine gravel, pebbles, tiny cobblestones, scattered debris, dithering, speckles, smooth gradients, blur, antialiasing, glossy, high contrast floor, repeating tile grid, voxel cubes, people, guards, human figures, creatures, letters, UI, border, frame'
        for node in ('8','10'):g[node]['inputs'].update(width=1376,height=768)
        g['11']['inputs']['noise_seed']=2026101870+n
        g['14']['inputs']['filename_prefix']='KS-Stage-Pixel-Match-20261018/'+name
        (folder/'workflow-api.json').write_text(json.dumps(g,indent=2))
        (folder/'prompt.json').write_text(json.dumps(caption,indent=2))
        receipt.write_text(json.dumps(api('/prompt',{'prompt':g,'client_id':'codex-stage-pixel-match'}),indent=2))
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
        if attempt%12==0:print(name+' local generation active',flush=True)
        time.sleep(5)
    else:raise TimeoutError('Retained job ID; do not submit another job.')
