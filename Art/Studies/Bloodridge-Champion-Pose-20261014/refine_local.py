"""One local pose-constrained material refinement; no cloud or production writes."""
import json, pathlib, shutil, time, urllib.request, sys

ROOT = pathlib.Path(__file__).resolve().parent
SERVER = 'http://127.0.0.1:8191'
INPUT = pathlib.Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-champion-pose-20261014')
OUTPUT = pathlib.Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')

def api(path, body=None):
    req = urllib.request.Request(SERVER + path, data=None if body is None else json.dumps(body).encode(), headers={'Content-Type': 'application/json'})
    with urllib.request.urlopen(req, timeout=45) as response:
        return json.load(response)

def node(kind, **inputs):
    return {'class_type': kind, 'inputs': inputs}

queue = api('/queue')
if queue['queue_running'] or queue['queue_pending']:
    raise RuntimeError('Other work is present; no job submitted.')
INPUT.mkdir(parents=True, exist_ok=True)
for filename in ['pose-input.png', 'style-input.png']:
    shutil.copyfile(ROOT / filename, INPUT / filename)

prompt = '''Picture 1 is the exact NEW pose and silhouette of one Bloodridge champion. Picture 2 is the APPROVED pixel-art rendering of his tribe. Finish picture 1 using the craftsmanship of picture 2. Preserve picture 1's upright asymmetrical duel stance: his face looks down-left in three-quarter battle view, lead hand extends a short hooked khopesh forward and downward, rear elbow is raised with the second short khopesh held above and behind his shoulder, two separate planted feet, lean torso, clear negative spaces. Keep this exact pose, weapon arrangement, size, location and 768-square framing. Do not copy the crouching stance from picture 2.

Repaint the flat simple modelling of picture 1 with picture 2's deliberate square pixel clusters, selective dark outlines, small worn-material shapes, anatomically convincing lean limbs and coherent upper-left lighting. This is a native small game sprite enlarged four times, NOT a high resolution painting. Keep approximately the same pixel detail density and tightly limited palette as picture 2. Rotted sepia and ash brown burial rags, irregular old linen bandages at wrists and calves, corroded dark bronze collar and armbands, an ashen human face with narrow ember-orange lava cracks beneath the headwrap. He is an eerie Egyptian desert guardian, an individual dangerous duelist. The head remains small and human-proportioned. Threadbare short split cloth at the waist leaves both legs readable; no big cape. Both blades have aged bronze bodies and a very narrow ember cutting edge, like picture 2. Keep the clothing dark and decayed, not shiny new armour.

One full body fighter only. Same uniform flat blue-grey background as picture 1, no floor, no cast shadow, no text, no new objects. No crown, antlers, horns, skull ornament, shield, staff, giant scythe, bulky muscles, plate armour, chibi, aura, smoke, sparks, bloom, blur, smooth gradients or photorealistic rendering. Render all existing body parts, both hands gripping their separate blades, and both feet completely inside the canvas.'''
graph = {
 '1': node('UNETLoader', unet_name='qwen_image_edit_2511_int8_convrot.safetensors', weight_dtype='default'),
 '2': node('LoraLoaderModelOnly', model=['1',0], lora_name='Qwen-Image-Edit-2511-Lightning-4steps-V1.0-bf16.safetensors', strength_model=1.0),
 '3': node('ModelSamplingAuraFlow', model=['2',0], shift=3.0),
 '4': node('CLIPLoader', clip_name='qwen_2.5_vl_7b_fp8_scaled.safetensors', type='qwen_image', device='default'),
 '5': node('VAELoader', vae_name='qwen_image_vae.safetensors'),
 '6': node('LoadImage', image='ks-champion-pose-20261014/pose-input.png'),
 '7': node('LoadImage', image='ks-champion-pose-20261014/style-input.png'),
 '8': node('TextEncodeQwenImageEditPlus', clip=['4',0], prompt=prompt, vae=['5',0], image1=['6',0], image2=['7',0]),
 '9': node('TextEncodeQwenImageEditPlus', clip=['4',0], prompt='', vae=['5',0], image1=['6',0], image2=['7',0]),
 '10': node('EmptySD3LatentImage', width=768, height=768, batch_size=1),
 '11': node('KSampler', model=['3',0], positive=['8',0], negative=['9',0], latent_image=['10',0], seed=2026101441, steps=4, cfg=1.0, sampler_name='euler', scheduler='simple', denoise=0.85),
 '12': node('VAEDecode', samples=['11',0], vae=['5',0]),
 '13': node('SaveImage', images=['12',0], filename_prefix='KS-Champion-Pose-20261014/material-refinement'),
}
suffix = '-pose-only' if '--pose-only' in sys.argv else ''
if suffix:
    prompt = '''Refine this exact pixel-art character drawing into a finished detailed small game sprite. This picture is the mandatory pose and silhouette. Keep his body, hands, blades and feet in exactly their current positions. An upright lean duelist, facing lower left in three quarter battle view, with his leading arm extended low to the left and rear arm bent up high to the right. Each hand grips its own short Egyptian khopesh. Keep both separated feet planted exactly as drawn, all the empty spaces between his limbs, and the short torn waist cloth. Preserve the upright stance and the high/low blade arrangement. DO NOT CROUCH or put both hands at the waist.

Improve only the painting and anatomical modelling inside this outline. The character is a frightening, lean Egyptian burial guardian with rotted sepia and ash-brown headwrap and rags, ashen human skin with tiny orange lava scars, weathered corroded bronze collar and arm bands, old irregular burial bandages on wrists and calves. The face is visible and human, recessed inside the headwrap. Two aged bronze khopesh with extremely narrow ember-orange cutting edges. Thin limbs with convincing elbows, wrists, fingers, knees and ankles. Dark brown shadows, warm dusty taupe highlights, muted ochre bronze; orange is restricted to the blade edges and a few facial cracks. Increase depth with one upper-left light, selective deep dark outlines, broken cloth folds, frayed hems, worn linen and tiny bronze edge chips. No shiny armour and no bright new gold.

CRISP NATIVE PIXEL ART enlarged four times, with deliberately chunky 4x4 pixel blocks, controlled square clusters, hard colour boundaries and clean stair-step diagonals. Limited 27-colour earthy palette. Match the detail density of an excellent 112px-tall game fighter. No small subpixel detail, no blur, no soft lighting, no oil painting, no high-resolution illustration, no cartoon or chibi proportions, no aura, sparks or smoke. Keep the same flat blue-grey background and the same margins. One character, same exact composition and silhouette, no other object or text.'''
    del graph['7']
    for key in ['8', '9']:
        graph[key]['inputs'].pop('image2', None)
    graph['8']['inputs']['prompt'] = prompt
    graph['11']['inputs']['seed'] = 2026101442
    graph['11']['inputs']['denoise'] = 1.0
    graph['13']['inputs']['filename_prefix'] = 'KS-Champion-Pose-20261014/pose-only-refinement'
(ROOT / ('local-prompt' + suffix + '.txt')).write_text(prompt, encoding='utf-8')
(ROOT / ('local-workflow' + suffix + '.json')).write_text(json.dumps(graph, indent=2), encoding='utf-8')
submitted = api('/prompt', {'prompt': graph, 'client_id': 'ks-champion-pose-20261014'})
(ROOT / ('local-submission' + suffix + '.json')).write_text(json.dumps(submitted, indent=2))
pid = submitted['prompt_id']
print('Local refinement submitted: ' + pid, flush=True)
started = time.monotonic()
while time.monotonic() - started < 900:
    result = api('/history/' + pid).get(pid)
    if result:
        (ROOT / ('local-history' + suffix + '.json')).write_text(json.dumps(result, indent=2))
        if result.get('status', {}).get('status_str') == 'error':
            raise RuntimeError(str(result['status']['messages'][-1]))
        for output in result.get('outputs', {}).values():
            for item in output.get('images', []):
                source = OUTPUT / item.get('subfolder', '') / item['filename']
                destination = ROOT / ('local-raw' + suffix + '.png')
                shutil.copyfile(source, destination)
                print(json.dumps({'seconds': round(time.monotonic() - started), 'path': str(destination)}), flush=True)
                raise SystemExit(0)
    time.sleep(5)
raise TimeoutError('Job not finished; inspect the recorded job, do not resubmit.')
