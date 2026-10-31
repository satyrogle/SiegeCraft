"""One local reference-conditioned concept. No cloud nodes or production writes."""
import json, pathlib, shutil, time, urllib.request, sys

ROOT = pathlib.Path(__file__).resolve().parent
PROJECT = ROOT.parents[2]
SERVER = 'http://127.0.0.1:8191'
INPUT = pathlib.Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-cult-focus-20261014')
OUTPUT = pathlib.Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')

def api(path, body=None):
    req = urllib.request.Request(SERVER+path, data=None if body is None else json.dumps(body).encode(), headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req, timeout=60) as r:
        return json.load(r)

def node(name, **inputs):
    return {'class_type':name, 'inputs':inputs}

def main():
    queue = api('/queue')
    if queue['queue_running'] or queue['queue_pending']:
        raise RuntimeError('Existing ComfyUI work found; no job submitted.')
    INPUT.mkdir(parents=True, exist_ok=True)
    sources = {
        'structure.png': PROJECT/'Art/Studies/Cult-Focus-Rune-20261014/focus-rune.png',
        'leader.png': PROJECT/'Art/Concepts/InnerCircle/2026-10-10/leader_neural_relic_v17.png',
    }
    for name, path in sources.items():
        shutil.copyfile(path, INPUT/name)
    prompt = '''Create a single extraordinary pixel-art command-button concept for an ancient interstellar cult of flesh-and-machine conquerors. Picture 1 is only a rough layout guide for a Focus Fire command: three hooked angular rune strokes converge inward on one central target mark, within a dark physical button housing. Redesign it with strong art direction and sophisticated craftsmanship. Picture 2 supplies this cult's established materials and design language: grown volcanic-black armour, broken angular sacred geometry, deep violet recesses, restrained worn ancient-gold inlays, small opalescent accents, ominous alien authority. Translate those materials into the button; the output contains only one button, not the character. Garments are now black in the approved direction; use black as the dominant mass.

One large, bold, coherent original alien rune is the focal point. It feels like a command sigil on the conqueror's neural relic. Three heavy hooked strokes converge on a small central rhombus, with strong intentional negative spaces. Sharp split obsidian edges, severe imposing proportions, a few violet reflective facets, thick restrained gold engraving. The dark housing is subordinate to the rune. Original design, flat front view, no perspective tilt. The rune is readable at small size. The object occupies roughly three quarters of the square image with clear margins. High-quality handmade game pixel-art appearance with deliberate square pixel clusters, crisp stepped diagonals, limited palette and broad flat shaded facets, styled as a 96px icon enlarged with nearest-neighbour. A single consistent upper-left light. Keep material detail concentrated at edges. Isolated on a uniform charcoal background. All visual attention belongs to the rune, not a portrait. No writing or alphabetic letters. No UI sheet, captions, extra icons, weapons, skull, eyes, character anatomy, sparkling particles, neon, lens effects, gradient glow or blur. This is the ready state only.'''
    graph = {
        '1': node('UNETLoader', unet_name='qwen_image_edit_2511_int8_convrot.safetensors', weight_dtype='default'),
        '2': node('LoraLoaderModelOnly', model=['1',0], lora_name='Qwen-Image-Edit-2511-Lightning-4steps-V1.0-bf16.safetensors', strength_model=1.0),
        '3': node('ModelSamplingAuraFlow', model=['2',0], shift=3.0),
        '4': node('CLIPLoader', clip_name='qwen_2.5_vl_7b_fp8_scaled.safetensors', type='qwen_image', device='default'),
        '5': node('VAELoader', vae_name='qwen_image_vae.safetensors'),
        '6': node('LoadImage', image='ks-cult-focus-20261014/structure.png'),
        '7': node('ImageCrop', image=['6',0], width=112, height=84, x=0, y=0),
        '8': node('LoadImage', image='ks-cult-focus-20261014/leader.png'),
        '9': node('TextEncodeQwenImageEditPlus', clip=['4',0], prompt=prompt, vae=['5',0], image1=['7',0], image2=['8',0]),
        '10': node('TextEncodeQwenImageEditPlus', clip=['4',0], prompt='', vae=['5',0], image1=['7',0], image2=['8',0]),
        '11': node('EmptySD3LatentImage', width=768, height=768, batch_size=1),
        '12': node('KSampler', model=['3',0], positive=['9',0], negative=['10',0], latent_image=['11',0], seed=2026101401, steps=4, cfg=1.0, sampler_name='euler', scheduler='simple', denoise=1.0),
        '13': node('VAEDecode', samples=['12',0], vae=['5',0]),
        '14': node('SaveImage', images=['13',0], filename_prefix='KS-Cult-Focus-20261014/qwen-local-reference-trial'),
    }
    suffix = ''
    if '--material-only' in sys.argv:
        suffix = '-material-only'
        prompt = '''Using the supplied alien leader as a MATERIAL AND CULTURE reference only, design one beautiful original dark alien cult command-button icon for a pixel-art roguelike. The output is a UI artefact, NOT a character, face, portrait, medallion or piece of jewellery. Interpret the leader's carved black volcanic-glass regalia, ancient gold, bruised violet and opalescent material in a fresh graphic design.

The icon is an imposing vertical broken obsidian tablet, flat front view, with a square overall footprint, bevelled irregular corners and a small split at the top and bottom. It is a neural command interface built by a cunning flesh-and-machine cult who worship old gods. Its central FOCUS FIRE glyph consists of THREE STRONG ANGULAR RUNIC STROKES aiming inward at ONE SMALL CENTRAL DIAMOND. Each stroke is a thick angular carved lightning-hook, with sharp right-angle bends and deliberate gaps. The three strokes form one ominous coherent symbol, like an alien command written in a sacred alphabet. The symbol fills most of the tablet. Show precision, menace and intelligence through its geometry. Strong graphic silhouette and disciplined negative spaces. Rune in muted pale ancient gold with deep purple interior incisions; a tiny opalescent central diamond. Black obsidian plate with a few broad violet facets and thin chipped aged gold bindings. Black is dominant. The surrounding frame is much less bright than the rune.

Draw as masterful crisp pixel art: a small 96 by 96 sprite enlarged with nearest-neighbour pixels to fill the canvas. Broad deliberate square pixel clusters, clean stair-step diagonals, limited colours, hard shadow shapes, selective one-pixel highlights and very little texture. One upper-left light. Centred single isolated object on a flat dark charcoal background with wide clear margins. The object is flat and upright. No face or eyes, no crown or horns, no circle or shield, no letters or words, no intricate decorative filigree, no photorealism, no blurry edges, no sparkling stars, no magic smoke, no neon, no radiant aura. One ready-state icon only.'''
        del graph['6'], graph['7']
        graph['9']['inputs'] = dict(clip=['4',0], prompt=prompt, vae=['5',0], image1=['8',0])
        graph['10']['inputs'] = dict(clip=['4',0], prompt='', vae=['5',0], image1=['8',0])
        graph['12']['inputs']['seed'] = 2026101402
        graph['14']['inputs']['filename_prefix'] = 'KS-Cult-Focus-20261014/qwen-material-only'
    (ROOT/('prompt'+suffix+'.txt')).write_text(prompt, encoding='utf-8')
    (ROOT/('workflow-api'+suffix+'.json')).write_text(json.dumps(graph, indent=2), encoding='utf-8')
    result = api('/prompt', {'prompt':graph, 'client_id':'ks-cult-focus-local-20261014'})
    (ROOT/('submission'+suffix+'.json')).write_text(json.dumps(result, indent=2))
    pid = result['prompt_id']
    print('Submitted local Qwen trial:', pid, flush=True)
    start = time.monotonic()
    while time.monotonic()-start < 1800:
        entry = api('/history/'+pid).get(pid)
        if entry:
            (ROOT/('history'+suffix+'.json')).write_text(json.dumps(entry, indent=2))
            if entry.get('status',{}).get('status_str') == 'error':
                raise RuntimeError(str(entry['status']['messages'][-1]))
            files = []
            for out in entry.get('outputs',{}).values():
                for item in out.get('images',[]):
                    src = OUTPUT/item.get('subfolder','')/item['filename']
                    dst = ROOT/item['filename']
                    shutil.copyfile(src,dst)
                    files.append(str(dst))
            if files:
                print(json.dumps({'elapsed_seconds':round(time.monotonic()-start), 'images':files}),flush=True)
                return
        time.sleep(5)
    raise TimeoutError('Job still pending; consult saved submission ID. Do not resubmit automatically.')

if __name__ == '__main__':
    main()
