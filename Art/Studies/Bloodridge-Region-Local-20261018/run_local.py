"""One local Qwen region concept, reusing the recorded dual-reference graph."""
import json, pathlib, shutil, time, urllib.request
from PIL import Image, ImageDraw

ROOT = pathlib.Path(__file__).resolve().parent
REPO = ROOT.parents[2]
SERVER = 'http://127.0.0.1:8191'
INPUT = pathlib.Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-region-local-20261018')
OUTPUT = pathlib.Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')

def api(path, body=None):
    request = urllib.request.Request(SERVER+path, data=None if body is None else json.dumps(body).encode(), headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(request, timeout=20) as response:
        return json.load(response)

prompt = '''Picture 1 is only a rough spatial plan. Redraw its whole landscape as a coherent original Bloodridge region concept in the native pixel-art style and material palette of picture 2. Preserve the spatial journey and connected paths from picture 1, but replace its flat geometric marks with believable terrain and buildings. Picture 2 supplies ash-brown limestone, violet-brown shadow, petrified roots, burial niches, tarnished bronze, rotting linen, and the dry oppressive atmosphere of an Egyptian-inspired burial clan. Do not copy picture 2 as one giant courtyard.

Show the ENTIRE region in one wide, elevated oblique view. From left to right there are three groups of three open encounter spaces, followed by ONE final boss court at the right. They lie on ONE continuous rocky shelf, connected by branching worn paths. No separate islands, no floating arenas, no UI nodes. The whole left third is nearly bare earth and ash: low natural rock edges, a few roots and only one tiny burial marker, no buildings or paving grid. The middle begins adding modest tomb niches, broken low walls and short sections of old paving. The next area becomes connected burial courts, larger niches and more enclosing architecture. Finally all paths converge on a tall paired-pylon funerary gate and a broad flat boss court at the far right. The gate is the biggest structure; nothing larger beyond it. Increase construction gradually from minimum at the start to maximum at the boss, with the same geology, architecture and daylight throughout.

Make the three route choices physically visible in each group, with open flat fight areas and traversable connections. Avoid a cliff gap cutting the route. Keep terrain silhouettes and quiet clearings readable at small scale. Restrained connected pixel clusters, crisp edges, subdued ash/sepia palette, sparse detail in the early area, deliberate stone and cloth shapes later. This is a cohesive game region concept, not a cinematic painting. No people, enemies, text, labels, icons, borders, coloured diagram lines, grid or numbered markers. No green forest, lava, snow, neon, skull carpet or unrelated biome.'''

def prepare():
    # A disposable spatial guide, not artwork or a claimed finished environment.
    canvas = Image.new('RGB', (1024,576), '#766257')
    draw = ImageDraw.Draw(canvas)
    draw.polygon([(0,95),(190,75),(390,90),(620,60),(840,45),(1024,80),(1024,480),(860,525),(650,500),(390,535),(170,505),(0,530)], fill='#a18a72')
    centers = [[(110,155),(105,295),(115,435)],[(355,145),(350,290),(360,430)],[(615,150),(625,290),(620,435)]]
    for left,right in zip(centers,centers[1:]):
        for i,(x,y) in enumerate(left):
            draw.line([(x,y),right[i]], fill='#baa086', width=22)
            draw.line([(x,y),right[min(i+1,2)]], fill='#b59a7d', width=15)
    for x,y in centers[-1]:
        draw.line([(x,y),(875,290)],fill='#baa086',width=22)
    for group in centers:
        for x,y in group:
            draw.ellipse((x-76,y-46,x+76,y+46), fill='#b39a7e')
    for x,y in centers[1]:
        draw.rectangle((x-67,y-66,x-27,y-51),fill='#63544e')
    for x,y in centers[2]:
        draw.rectangle((x-80,y-78,x+65,y-55),fill='#5e4d47')
        draw.rectangle((x-85,y-65,x-66,y+35),fill='#69594e')
    draw.ellipse((783,226,1005,363),fill='#b39a7e')
    draw.rectangle((812,102,975,212),fill='#493e3b')
    draw.rectangle((804,86,839,217),fill='#5e4d47')
    draw.rectangle((950,86,986,217),fill='#5e4d47')
    draw.rectangle((864,147,929,212),fill='#302c2e')
    canvas.save(ROOT/'layout-guide.png')
    INPUT.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(ROOT/'layout-guide.png',INPUT/'layout-guide.png')
    shutil.copyfile(REPO/'Art/Studies/Bloodridge-Court-Refinement-20261013/source-clean.png',INPUT/'court-style.png')
    graph = json.loads((REPO/'Art/Studies/Comfy-Cult-Focus-20261014/workflow-api.json').read_text(encoding='utf-8'))
    graph['6']['inputs']['image']='ks-region-local-20261018/layout-guide.png'
    del graph['7']
    graph['8']['inputs']['image']='ks-region-local-20261018/court-style.png'
    for key,text in [('9',prompt),('10','')]:
        graph[key]['inputs'].update(prompt=text,image1=['6',0],image2=['8',0])
    graph['11']['inputs'].update(width=1024,height=576,batch_size=1)
    graph['12']['inputs']['seed']=2026101801
    graph['14']['inputs']['filename_prefix']='KS-Region-Local-20261018/bloodridge-whole-region'
    (ROOT/'workflow-api.json').write_text(json.dumps(graph,indent=2),encoding='utf-8')
    (ROOT/'prompt.txt').write_text(prompt,encoding='utf-8')
    return graph

def main():
    submission=ROOT/'submission.json'
    if submission.exists():
        pid=json.loads(submission.read_text())['prompt_id']
        print('Resuming existing job '+pid,flush=True)
    else:
        graph=prepare()
        queue=api('/queue')
        if queue['queue_running'] or queue['queue_pending']:
            raise RuntimeError('Another job is present. No submission; do not interrupt it.')
        allowed={'UNETLoader','LoraLoaderModelOnly','ModelSamplingAuraFlow','CLIPLoader','VAELoader','LoadImage','TextEncodeQwenImageEditPlus','EmptySD3LatentImage','KSampler','VAEDecode','SaveImage'}
        if any(n['class_type'] not in allowed for n in graph.values()):
            raise RuntimeError('Unexpected node in local-only graph')
        result=api('/prompt',{'prompt':graph,'client_id':'codex-bloodridge-region-local-20261018'})
        submission.write_text(json.dumps(result,indent=2),encoding='utf-8')
        pid=result['prompt_id']
        print('Submitted '+pid,flush=True)
    start=time.monotonic()
    last_report=0
    while time.monotonic()-start < 1800:
        entry=api('/history/'+pid).get(pid)
        if entry:
            (ROOT/'history.json').write_text(json.dumps(entry,indent=2),encoding='utf-8')
            if entry.get('status',{}).get('status_str')=='error':
                raise RuntimeError(str(entry['status']['messages'][-1]))
            for output in entry.get('outputs',{}).values():
                for im in output.get('images',[]):
                    src=OUTPUT/im.get('subfolder','')/im['filename']
                    shutil.copyfile(src,ROOT/'bloodridge-region-local.png')
                    print('Saved '+str(ROOT/'bloodridge-region-local.png'),flush=True)
                    return
        elapsed=int(time.monotonic()-start)
        if elapsed-last_report>=30:
            print('Waiting for local job, '+str(elapsed)+' seconds',flush=True)
            last_report=elapsed
        time.sleep(5)
    raise TimeoutError('Local job still pending; resume using saved ID, never blindly resubmit.')

if __name__=='__main__':
    main()
