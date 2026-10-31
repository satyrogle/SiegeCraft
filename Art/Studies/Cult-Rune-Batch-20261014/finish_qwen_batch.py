"""Keep the remaining Qwen seeds in one graph so model weights are shared across images."""
import json,time,shutil
from generate_batch import ROOT,OUT,api,n,prompt,graph,assemble,COMMANDS

mp=ROOT/'manifest.json';manifest=json.loads(mp.read_text())
todo=[i for i in range(10) if not any(e['id']==f'Q{i+1:02d}' and e.get('status')=='complete' for e in manifest)]
assert todo and all(not(e.get('status')=='running') for e in manifest),'Unfinished previous submission: reconcile first.'
queue=api('/queue');assert not queue['queue_running'] and not queue['queue_pending'],'Queue occupied; nothing submitted.'
g=graph('qwen','A','authored command',2026101600,'unused')
for key in ['12','13','14']:g.pop(key)
g['15']=n('TextEncodeQwenImageEditPlus',clip=['4',0],prompt=prompt('B','authored command'),vae=['5',0],image1=['8',0])
pending=[]
for i in todo:
    short,name,meaning=COMMANDS[i%5];style='A' if i<5 else 'B';ident=f'Q{i+1:02d}';seed=2026101600+i
    sampler,decoder,saver=map(str,[100+i*3,101+i*3,102+i*3])
    g[sampler]=n('KSampler',model=['3',0],positive=['9' if style=='A' else '15',0],negative=['10',0],latent_image=['11',0],seed=seed,steps=4,cfg=1.0,sampler_name='euler',scheduler='simple',denoise=1.0)
    g[decoder]=n('VAEDecode',samples=[sampler,0],vae=['5',0])
    g[saver]=n('SaveImage',images=[decoder,0],filename_prefix=f'KS-Rune-Batch-20261014/shared-{ident}-{short}')
    pending.append({'id':ident,'model':'qwen','command':name,'command_index':i%5,'style':style,'meaning':meaning,'seed':seed,'status':'running','save_node':saver,'output_prefix':f'shared-{ident}-{short}'})
(ROOT/'workflows'/'qwen-shared-model-batch.json').write_text(json.dumps(g,indent=2))
response=api('/prompt',{'prompt':g,'client_id':'ks-coherent-runes-shared-qwen-20261014'});pid=response['prompt_id']
for e in pending:e['prompt_id']=pid;manifest.append(e)
mp.write_text(json.dumps(manifest,indent=2));print(f'Submitted {len(pending)} Qwen seeds in one shared-model graph: {pid}',flush=True)
start=time.monotonic()
while time.monotonic()-start<3600:
    for e in pending:
        if e['status']=='complete':continue
        files=list((OUT/'KS-Rune-Batch-20261014').glob(e['output_prefix']+'_*.png'))
        if not files:continue
        src=max(files,key=lambda p:p.stat().st_mtime);short=COMMANDS[e['command_index']][0]
        raw=ROOT/'raw'/f'{e["id"]}-{short}.png';dst=ROOT/'native'/f'{e["id"]}-{short}.png'
        try:
            shutil.copyfile(src,raw);e.update(assemble(raw,dst,e['command_index'],e['style']))
        except (OSError,ValueError):continue
        e.update(status='complete',raw=str(raw),native=str(dst),elapsed_seconds=round(time.monotonic()-start));mp.write_text(json.dumps(manifest,indent=2))
        print(e['id']+' rendered and assembled',flush=True)
    history=api('/history/'+pid).get(pid)
    if history:
        (ROOT/'histories'/'qwen-shared-model-batch.json').write_text(json.dumps(history,indent=2))
        if history.get('status',{}).get('status_str')=='error':raise RuntimeError(str(history['status']['messages'][-1]))
        if all(e['status']=='complete' for e in pending):
            print('20 / 20 COMPLETE; shared Qwen job succeeded.',flush=True);break
    time.sleep(5)
else:raise TimeoutError('Saved prompt_id remains authoritative; do not resubmit.')
