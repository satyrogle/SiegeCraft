"""One free local Qwen edit: reveal the floor behind an extracted foreground piece.
Image composition/extraction is authored by the native Aseprite Lua scripts.
"""
from pathlib import Path
import json, shutil, time, urllib.request

ROOT=Path(__file__).resolve().parent
REPO=ROOT.parents[2]
SERVER='http://127.0.0.1:8191'
INPUT=Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-bloodridge-depth-20261020')
OUTPUT=Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')

def api(path,body=None):
    req=urllib.request.Request(SERVER+path,data=None if body is None else json.dumps(body).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=30) as response:return json.load(response)

def main():
    receipt=ROOT/'underpaint-submission-2.json'
    if receipt.exists():
        pid=json.loads(receipt.read_text())['prompt_id']
    else:
        queue=api('/queue')
        if queue['queue_running'] or queue['queue_pending']:
            raise RuntimeError('Shared queue became busy. No job submitted; do not pre-empt it.')
        INPUT.mkdir(parents=True,exist_ok=True)
        shutil.copyfile(ROOT/'underpaint-input-2.png',INPUT/'underpaint-2.png')
        graph=json.loads((REPO/'Art/Studies/Bloodridge-Root-Crypt-20261019/crypt/workflow-api.json').read_text())
        graph['6']['inputs']['image']='ks-bloodridge-depth-20261020/underpaint-2.png'
        prompt=('Fill only the transparent holes with the same continuous worn limestone and ash battle floor. '
            'This is a pixel-art game background at native size. The missing areas were a near foreground broken '
            'wall and pale shrubs; they have been removed to reveal the ground BEHIND them. Complete the floor '
            'beneath those missing shapes. Match the surrounding dusty sepia, muted violet-brown shadows, irregular '
            'weathered stone slabs, scattered small chips and sparse ash. Let existing slab edges continue naturally '
            'through the holes, with varied spacing. The surface is flat and solid, continuous with the visible floor. '
            'Preserve perspective, pixel density and worn cluster detail. Keep all visible surroundings unchanged. '
            'Do not restore the wall or shrubs. No new object, repeating tiles, decoration, grass, cliff, hole, '
            'abyss, character, text, clean blocks, black outlines or glossy 3D lighting.')
        graph['9']['inputs']['prompt']=prompt
        graph['12']['inputs']['seed']=20261020422
        graph['14']['inputs']['filename_prefix']='KS-Bloodridge-Depth-20261020/underpaint-2'
        (ROOT/'underpaint-prompt-2.txt').write_text(prompt)
        (ROOT/'underpaint-workflow-2.json').write_text(json.dumps(graph,indent=2))
        result=api('/prompt',{'prompt':graph,'client_id':'codex-bloodridge-depth'})
        receipt.write_text(json.dumps(result,indent=2));pid=result['prompt_id']
    print('Local underpaint job:',pid,flush=True)
    for _ in range(600):
        history=api('/history/'+pid).get(pid)
        if history:
            (ROOT/'underpaint-history-2.json').write_text(json.dumps(history,indent=2))
            if history.get('status',{}).get('status_str')=='error':raise RuntimeError(history['status'])
            images=[im for output in history.get('outputs',{}).values() for im in output.get('images',[])]
            if images:
                entry=images[0]
                shutil.copyfile(OUTPUT/entry.get('subfolder','')/entry['filename'],ROOT/'underpaint-raw-2.png')
                print('Native underpaint candidate saved.',flush=True);return
        time.sleep(3)
    raise TimeoutError('Local generation not complete; receipt retained.')

if __name__=='__main__':main()
