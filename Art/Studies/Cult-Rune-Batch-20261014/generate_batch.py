"""Exactly ten FLUX and ten Qwen housings, with deterministic command glyph assembly."""
from pathlib import Path
import json,time,urllib.request,shutil,sys
from PIL import Image,ImageDraw,ImageFilter

ROOT=Path(__file__).resolve().parent
HOST='http://127.0.0.1:8191'; OUT=Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')
COMMANDS=[('hold','HOLD','The unit mark is enclosed by an unbroken barrier.'),('fall-back','FALL BACK','The unit mark returns left behind the boundary.'),('focus-fire','FOCUS FIRE','Three strokes point inward to one target mark.'),('unleash','UNLEASH','The barrier is broken; force points outward from the unit mark.'),('eclipse','ECLIPSE','An occluded disc marks Bloodridge\'s acquired chief order.')]
PALETTE=['#0a0910','#111019','#191523','#221c2e','#30243e','#42314f','#574263','#725b7d','#92849f','#c4b5c8','#eadfce','#34252a','#513631','#775036','#a47747','#c4a068','#dfc88b','#a39ca7','#34313b','#4d4654','#201a23','#18171b','#27242c','#675a69']
PALETTE.append('#000000')  # Retained generated silhouette outline; 25-colour study palette.
def api(path,body=None):
    req=urllib.request.Request(HOST+path,data=None if body is None else json.dumps(body).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=60) as r:return json.load(r)
def n(name,**inputs):return {'class_type':name,'inputs':inputs}
def prompt(style,command):
    command='authored command'  # Housing-only brief: share conditioning across seeds, apply command glyph afterward.
    finish={'A':'Hewn volcanic-glass tablet. Squat monumental silhouette, broad chipped bevels, warm worn-gold bindings, dark muted violet facets. A severe ancient military cult artefact.', 'B':'Layered neural reliquary. Two interlocking black obsidian plates, fine opalescent edge slivers, restrained old-gold clasps, black membrane seams and deep bruised-violet reflections. Alien biomechanical authority, graphic and imposing.'}[style]
    return f'''pixel art sprite, game asset, 16-bit pixel art. Using the supplied alien leader as a MATERIAL AND CULTURE reference only, design one beautiful original dark alien cult command-button HOUSING for a pixel-art roguelike. The output is a UI artefact, not a character, portrait, medallion or jewellery. Interpret the leader's carved black volcanic-glass regalia, ancient gold, bruised violet and opalescent material in a fresh graphic design. {finish}

One imposing square broken obsidian tablet, flat front view, bevelled irregular corners and a small split at the top and bottom. It is a neural command interface built by a cunning flesh-and-machine cult who worship old gods. A large EMPTY dark central recess occupies the middle sixty percent of the image. It is blank near-black, smooth and quiet, ready to receive the {command} rune later. Keep material detail at the surrounding rim and corners. The housing occupies eighty percent of the canvas, centred with wide clear margins. Square proportions. Black is dominant; old gold is sparse. Deep muted violet facets. Draw crisp pixel art: a small 128 by 128 sprite enlarged with nearest-neighbour pixels. Broad deliberate square clusters, clean stair-step diagonals, limited colours, hard shadow shapes, selective one-pixel highlights and very little texture. One upper-left light. Centred single isolated object on flat dark charcoal. No face, eyes, crown, horns, letters, words, filigree, photorealism, blurry edges, sparkling stars, smoke, neon or aura. One ready-state housing only.'''
def graph(model,style,cmd,seed,prefix):
    shared={'7':n('LoadImage',image=f'ks-rune-batch-20261014/blank-{style}.png'),'8':n('LoadImage',image='ks-rune-batch-20261014/leader.png')}
    if model=='flux':
        return {**shared,
        '1':n('UNETLoader',unet_name='flux-2-klein-base-4b.safetensors',weight_dtype='default'),
        '2':n('LoraLoaderModelOnly',model=['1',0],lora_name='flux2klein_pixel_art_sprite_limbicnation.safetensors',strength_model=1.0),
        '3':n('CLIPLoader',clip_name='qwen_3_4b.safetensors',type='flux2',device='default'),
        '4':n('VAELoader',vae_name='flux2-vae.safetensors'),
        '5':n('CLIPTextEncode',clip=['3',0],text=prompt(style,cmd)), '6':n('CLIPTextEncode',clip=['3',0],text=''),
        '9':n('ImageScale',image=['8',0],upscale_method='lanczos',width=768,height=512,crop='disabled'),
        '10':n('VAEEncode',pixels=['7',0],vae=['4',0]),'11':n('VAEEncode',pixels=['9',0],vae=['4',0]),
        '12':n('ReferenceLatent',conditioning=['5',0],latent=['10',0]),'13':n('ReferenceLatent',conditioning=['6',0],latent=['10',0]),
        '14':n('ReferenceLatent',conditioning=['5',0],latent=['11',0]),'15':n('ReferenceLatent',conditioning=['6',0],latent=['11',0]),
        '16':n('EmptyFlux2LatentImage',width=768,height=768,batch_size=1),
        '17':n('CFGGuider',model=['2',0],positive=['14',0],negative=['15',0],cfg=5.0),
        '18':n('KSamplerSelect',sampler_name='euler'),'19':n('Flux2Scheduler',steps=20,width=768,height=768),
        '20':n('RandomNoise',noise_seed=seed),'21':n('SamplerCustomAdvanced',noise=['20',0],guider=['17',0],sampler=['18',0],sigmas=['19',0],latent_image=['16',0]),
        '22':n('VAEDecode',samples=['21',0],vae=['4',0]),'23':n('SaveImage',images=['22',0],filename_prefix=prefix)}
    return {**shared,
        '1':n('UNETLoader',unet_name='qwen_image_edit_2511_int8_convrot.safetensors',weight_dtype='default'),
        '2':n('LoraLoaderModelOnly',model=['1',0],lora_name='Qwen-Image-Edit-2511-Lightning-4steps-V1.0-bf16.safetensors',strength_model=1.0),
        '3':n('ModelSamplingAuraFlow',model=['2',0],shift=3.0),'4':n('CLIPLoader',clip_name='qwen_2.5_vl_7b_fp8_scaled.safetensors',type='qwen_image',device='default'),
        '5':n('VAELoader',vae_name='qwen_image_vae.safetensors'),
        '9':n('TextEncodeQwenImageEditPlus',clip=['4',0],prompt=prompt(style,cmd),vae=['5',0],image1=['8',0]),
        '10':n('TextEncodeQwenImageEditPlus',clip=['4',0],prompt='',vae=['5',0],image1=['8',0]),
        '11':n('EmptySD3LatentImage',width=768,height=768,batch_size=1),
        '12':n('KSampler',model=['3',0],positive=['9',0],negative=['10',0],latent_image=['11',0],seed=seed,steps=4,cfg=1.0,sampler_name='euler',scheduler='simple',denoise=1.0),
        '13':n('VAEDecode',samples=['12',0],vae=['5',0]),'14':n('SaveImage',images=['13',0],filename_prefix=prefix)}

def assemble(raw,dst,command_index,style):
    # Model paints the housing; this exact silhouette layer guarantees symbol meaning.
    source=Image.open(raw).convert('RGB').resize((128,128),Image.Resampling.NEAREST)
    # Remove only corner-connected flat review backgrounds, preserving the object.
    for corner in [(0,0),(127,0),(0,127),(127,127)]:
        ImageDraw.floodfill(source,corner,(17,16,25),thresh=32)
    pal=Image.new('P',(1,1)); colours=[]
    for hexval in PALETTE:colours+=list(bytes.fromhex(hexval[1:]))
    pal.putpalette(colours+[0]*(768-len(colours)))
    panel=source.quantize(palette=pal,dither=Image.Dither.NONE).convert('RGBA')
    ImageDraw.floodfill(panel,(64,64),(10,9,16,255),thresh=28)
    d=ImageDraw.Draw(panel)
    d.polygon([(30,28),(98,28),(100,30),(100,98),(98,100),(30,100),(28,98),(28,30)],fill='#0a0910')
    mask=Image.open(ROOT/'command-runes.png').convert('RGBA').crop((command_index*64,0,command_index*64+64,64)).getchannel('A')
    rune=Image.new('RGBA',(64,64))
    m=mask.load();r=rune.load()
    for y in range(64):
        for x in range(64):
            if not m[x,y]:continue
            top=y==0 or not m[x,y-1];left=x==0 or not m[x-1,y];bottom=y==63 or not m[x,y+1];right=x==63 or not m[x+1,y]
            if top or left:col='#dfc88b' if style=='A' else '#eadfce'
            elif bottom or right:col='#775036' if style=='A' else '#725b7d'
            else:col='#c4a068' if style=='A' else '#c4b5c8'
            r[x,y]=(*bytes.fromhex(col[1:]),255)
    # A single-pixel recessed shadow; shape itself never changes with model or treatment.
    shadow=Image.new('RGBA',(64,64),'#34252a');shadow.putalpha(mask)
    panel.alpha_composite(shadow,(33,33));panel.alpha_composite(rune,(32,32))
    panel.save(dst);panel.resize((512,512),Image.Resampling.NEAREST).save(dst.with_name(dst.stem+'-4x.png'))
    housing=panel.copy();ImageDraw.Draw(housing).rectangle((32,32,95,95),fill='#0a0910')
    return {'native_size':[128,128],'glyph_source':'command-runes.png','glyph_index':command_index,'fixed_glyph':True,'palette_size':len(PALETTE)}

def main():
    for folder in ['raw','native','workflows','histories']: (ROOT/folder).mkdir(exist_ok=True)
    manifest_path=ROOT/'manifest.json';manifest=json.loads(manifest_path.read_text()) if manifest_path.exists() else []
    completed_this_run=0
    for model in ['flux','qwen']:
        if '--model' in sys.argv and model!=sys.argv[sys.argv.index('--model')+1]:continue
        for i in range(10):
            short,name,meaning=COMMANDS[i%5];style='A' if i<5 else 'B';ident=f'{"F" if model=="flux" else "Q"}{i+1:02d}'
            if any(v['id']==ident and v.get('status')=='complete' for v in manifest):continue
            q=api('/queue')
            if q['queue_running'] or q['queue_pending']:raise RuntimeError('Another ComfyUI job present. Stop without interfering.')
            seed=2026101400+100+(0 if model=='flux' else 100)+i
            g=graph(model,style,name,seed,f'KS-Rune-Batch-20261014/{ident}-{short}')
            (ROOT/'workflows'/f'{ident}.json').write_text(json.dumps(g,indent=2))
            response=api('/prompt',{'prompt':g,'client_id':'ks-coherent-runes-20261014'})
            pid=response['prompt_id'];entry={'id':ident,'model':model,'command':name,'command_index':i%5,'style':style,'meaning':meaning,'seed':seed,'prompt_id':pid,'status':'running'}
            manifest.append(entry);manifest_path.write_text(json.dumps(manifest,indent=2))
            print(f'{ident} {name} {style} submitted {pid}',flush=True);start=time.monotonic()
            while time.monotonic()-start<1800:
                h=api('/history/'+pid).get(pid)
                if h:
                    (ROOT/'histories'/f'{ident}.json').write_text(json.dumps(h,indent=2))
                    if h.get('status',{}).get('status_str')=='error':raise RuntimeError(str(h['status']['messages'][-1]))
                    images=[im for v in h.get('outputs',{}).values() for im in v.get('images',[])]
                    if images:
                        im=images[0];raw=ROOT/'raw'/f'{ident}-{short}.png';shutil.copyfile(OUT/im.get('subfolder','')/im['filename'],raw)
                        native=ROOT/'native'/f'{ident}-{short}.png';entry.update(assemble(raw,native,i%5,style));entry.update(status='complete',raw=str(raw),native=str(native),elapsed_seconds=round(time.monotonic()-start))
                        manifest_path.write_text(json.dumps(manifest,indent=2));print(f'{ident} complete ({entry["elapsed_seconds"]}s)',flush=True);completed_this_run+=1;break
                time.sleep(5)
            else:raise TimeoutError(f'{ident} still pending; inspect saved prompt_id, do not resubmit.')
            if '--one' in sys.argv or (ROOT/'STOP_AFTER_CURRENT').exists():return
    print(str(sum(v.get('status')=='complete' for v in manifest))+' / 20 COMPLETE',flush=True)
if __name__=='__main__':main()
