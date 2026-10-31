"""Reuse Claude's 15 Oct local Ideogram concept graph without modifying it."""
import json, pathlib, shutil, time, urllib.request

ROOT=pathlib.Path(__file__).resolve().parent
REPO=ROOT.parents[2]
SERVER='http://127.0.0.1:8191'
OUTPUT=pathlib.Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')
BASE=REPO/'Art/Pipeline/runs/hunters-sheet-20261015e/raw/option-1-workflow.json'

def api(path,body=None):
    req=urllib.request.Request(SERVER+path,data=None if body is None else json.dumps(body).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=30) as response:
        return json.load(response)

def main():
    receipt=ROOT/'submission.json'
    if receipt.exists():
        pid=json.loads(receipt.read_text())['prompt_id']
        print('Resuming saved job '+pid,flush=True)
    else:
        graph=json.loads(BASE.read_text(encoding='utf-8-sig'))
        description=json.loads((ROOT/'description.json').read_text(encoding='utf-8-sig'))
        graph['4']['inputs']['text']=json.dumps(description)
        graph['5']['inputs']['text']='checkerboard, transparency grid, transparent background, white background, isolated cutout, floating island, smooth 3D render, miniature model, golden architecture, single courtyard, single room, closeup, large building filling image, disconnected panels, collage, separate islands, four vertical panels, single linear corridor, missing paths, huge cliffs blocking routes, photographic aerial landscape, photorealism, smooth digital painting, voxel cubes, cartoon black outlines, perfect clean masonry, signature, watermark'
        graph['14']['inputs']['filename_prefix']='KS-Bloodridge-Journey-Ascent-20261018b/region'
        original=json.loads(BASE.read_text(encoding='utf-8-sig'))
        changes=[k for k in graph if graph[k]!=original[k]]
        assert set(changes)=={'4','5','14'},changes
        allowed={'UNETLoader','CLIPLoader','VAELoader','CLIPTextEncode','CFGOverride','DualModelGuider','EmptyFlux2LatentImage','KSamplerSelect','Ideogram4Scheduler','RandomNoise','SamplerCustomAdvanced','VAEDecode','SaveImage'}
        assert all(n['class_type'] in allowed for n in graph.values())
        (ROOT/'workflow-api.json').write_text(json.dumps(graph,indent=2),encoding='utf-8')
        queue=api('/queue')
        if queue['queue_running'] or queue['queue_pending']:
            raise RuntimeError('Another job is present; no submission and no interruption.')
        result=api('/prompt',{'prompt':graph,'client_id':'codex-bloodridge-ideogram-20261018'})
        receipt.write_text(json.dumps(result,indent=2),encoding='utf-8')
        pid=result['prompt_id']
        print('Submitted '+pid,flush=True)
    start=time.monotonic()
    last_report=0
    while time.monotonic()-start<2400:
        entry=api('/history/'+pid).get(pid)
        if entry:
            (ROOT/'history.json').write_text(json.dumps(entry,indent=2),encoding='utf-8')
            if entry.get('status',{}).get('status_str')=='error':
                raise RuntimeError(str(entry['status']['messages'][-1]))
            for output in entry.get('outputs',{}).values():
                for im in output.get('images',[]):
                    src=OUTPUT/im.get('subfolder','')/im['filename']
                    shutil.copyfile(src,ROOT/'bloodridge-whole-region.png')
                    print('Saved '+str(ROOT/'bloodridge-whole-region.png'),flush=True)
                    return
        elapsed=int(time.monotonic()-start)
        if elapsed-last_report>=30:
            print('Local job active, '+str(elapsed)+' seconds',flush=True)
            last_report=elapsed
        time.sleep(5)
    raise TimeoutError('Job remains pending. Inspect saved ID; never blindly resubmit.')

if __name__=='__main__':
    main()
