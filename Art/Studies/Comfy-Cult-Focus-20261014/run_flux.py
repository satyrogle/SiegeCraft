"""Single local FLUX comparison, using the installed base model and pixel LoRA."""
import json, pathlib, shutil, time
from run_qwen import api, node, ROOT, OUTPUT

def main():
    q=api('/queue')
    if q['queue_running'] or q['queue_pending']:
        raise RuntimeError('Existing ComfyUI work found; nothing submitted.')
    prompt='pixel art sprite, game asset, 16-bit pixel art. '+(ROOT/'prompt-material-only.txt').read_text(encoding='utf-8')
    graph={
      '1':node('UNETLoader',unet_name='flux-2-klein-base-4b.safetensors',weight_dtype='default'),
      '2':node('LoraLoaderModelOnly',model=['1',0],lora_name='flux2klein_pixel_art_sprite_limbicnation.safetensors',strength_model=1.0),
      '3':node('CLIPLoader',clip_name='qwen_3_4b.safetensors',type='flux2',device='default'),
      '4':node('VAELoader',vae_name='flux2-vae.safetensors'),
      '5':node('CLIPTextEncode',clip=['3',0],text=prompt),
      '6':node('CLIPTextEncode',clip=['3',0],text=''),
      '7':node('LoadImage',image='ks-cult-focus-20261014/leader.png'),
      '8':node('ImageScale',image=['7',0],upscale_method='lanczos',width=768,height=512,crop='disabled'),
      '9':node('VAEEncode',pixels=['8',0],vae=['4',0]),
      '10':node('ReferenceLatent',conditioning=['5',0],latent=['9',0]),
      '11':node('ReferenceLatent',conditioning=['6',0],latent=['9',0]),
      '12':node('EmptyFlux2LatentImage',width=768,height=768,batch_size=1),
      '13':node('CFGGuider',model=['2',0],positive=['10',0],negative=['11',0],cfg=5.0),
      '14':node('KSamplerSelect',sampler_name='euler'),
      '15':node('Flux2Scheduler',steps=20,width=768,height=768),
      '16':node('RandomNoise',noise_seed=2026101402),
      '17':node('SamplerCustomAdvanced',noise=['16',0],guider=['13',0],sampler=['14',0],sigmas=['15',0],latent_image=['12',0]),
      '18':node('VAEDecode',samples=['17',0],vae=['4',0]),
      '19':node('SaveImage',images=['18',0],filename_prefix='KS-Cult-Focus-20261014/flux-klein-pixel-lora'),
    }
    (ROOT/'prompt-flux.txt').write_text(prompt,encoding='utf-8')
    (ROOT/'workflow-api-flux.json').write_text(json.dumps(graph,indent=2))
    response=api('/prompt',{'prompt':graph,'client_id':'ks-cult-focus-flux-20261014'})
    (ROOT/'submission-flux.json').write_text(json.dumps(response,indent=2))
    pid=response['prompt_id']; print('Submitted FLUX:',pid,flush=True); start=time.monotonic()
    while time.monotonic()-start<1800:
        h=api('/history/'+pid).get(pid)
        if h:
            (ROOT/'history-flux.json').write_text(json.dumps(h,indent=2))
            if h.get('status',{}).get('status_str')=='error':
                raise RuntimeError(str(h['status']['messages'][-1]))
            files=[]
            for out in h.get('outputs',{}).values():
                for im in out.get('images',[]):
                    src=OUTPUT/im.get('subfolder','')/im['filename']; dst=ROOT/im['filename']; shutil.copyfile(src,dst); files.append(str(dst))
            if files:
                print(json.dumps({'elapsed_seconds':round(time.monotonic()-start),'images':files}),flush=True); return
        time.sleep(5)
    raise TimeoutError('Consult saved job ID; do not automatically resubmit.')

if __name__=='__main__':main()
