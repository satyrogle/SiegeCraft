import json, pathlib, shutil, time, urllib.request, sys
ROOT=pathlib.Path(__file__).resolve().parent
PROJECT=ROOT.parents[2]
SERVER='http://127.0.0.1:8191'
INPUT=pathlib.Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-champion-flux-20261014')
OUTPUT=pathlib.Path('C:/Users/jacob/ComfyUI-Shared/output/ArtLab')
def api(path,body=None):
    req=urllib.request.Request(SERVER+path,data=None if body is None else json.dumps(body).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=45) as response:return json.load(response)
def main():
    q=api('/queue')
    if q['queue_running'] or q['queue_pending']:raise RuntimeError('Existing queue occupied; nothing submitted.')
    INPUT.mkdir(parents=True,exist_ok=True)
    shutil.copyfile(ROOT.parent/'Bloodridge-Champion-Specimen-20261014/approved-skirmisher-native.png',INPUT/'approved-skirmisher.png')
    graph=json.loads((ROOT.parent/'Comfy-Cult-Focus-20261014/workflow-api-flux.json').read_text())
    prompt="""pixel art sprite, game asset, 32-bit pixel art, transparent background. Design ONE Bloodridge duel champion from the same tribe and same game as the supplied approved sprite. Match its sophisticated native pixel craftsmanship, strong anatomical modelling, selective dark outlines, warm earth palette, light direction, hand and head proportions, and slightly elevated three-quarter camera facing southwest (left and toward the viewer). This is a new individual with a NEW POSE, not the identical man in the reference.

A terrifying lean athletic Egyptian burial guardian, broad through the shoulders and tapering at the waist, muscular forearms and convincing thighs, mature human proportions. His own aggressive COILED DUELLING GUARD: torso twists toward the left, forward knee bent over a planted foot, rear leg braced diagonally to the right. His leading hand aims a long Egyptian khopesh horizontally toward the left at chest height. His rear hand holds a second khopesh in a reverse low guard beside his hip, its curved blade clearly outside his body. Exactly two hands, each visibly gripping one blade. Khopesh are broad curved sword blades with a straight lower section and heavy crescent tip, not small farming sickles.

Distinct identity: a close-fitting burial headwrap with a visible grim ashen face and a few narrow ember scars; a stepped corroded bronze gorget and ONE ragged diagonal funerary stole across one shoulder; bare sinewy upper arms with bronze bands, old ash-linen wraps on forearms and calves, dark fitted battle cloth and split rotted waist panels. Three short frayed cloth tails trail behind him. Restrained orange heat inside blade edges. Menacing, grounded and poised to strike. No antlers, crown, staff, shield, heavy plate armour or huge cape.

Precise dense pixel clusters like the reference, crisp angular cloth folds, sharply readable bronze and linen, high value contrast around the face, hands and blade edges. Strong shadow planes and narrow lit rims. A 112px-tall game fighter enlarged four times with nearest-neighbour pixel edges, detailed but never smooth or blurred. Full body and both sword tips inside the image with clear margins. One character only, centred. Isolated sprite on a perfectly uniform desaturated blue-grey (#56616c) matte for extraction; no floor, cast shadow, text, border, particles or scenery. The reference supplies the STYLE and ANATOMY; use the new pose and individual costume described here."""
    graph['2']['inputs']['strength_model']=1.0
    if '--compat' in sys.argv:
        graph['20']={'class_type':'LoraLoaderModelOnly','inputs':{'model':['2',0],'lora_name':'ks-champion-flux-20261014\\modulation-compat.safetensors','strength_model':1.0}}
        graph['13']['inputs']['model']=['20',0]
    prompt=prompt.replace('broad through the shoulders and tapering at the waist, muscular forearms and convincing thighs','sinewy angular shoulders and narrow waist, lean defined forearms and convincing thighs; ash-brown skin, never a bulky bodybuilder')
    prompt=prompt.replace('His leading hand aims a long Egyptian khopesh horizontally toward the left at chest height. His rear hand holds a second khopesh in a reverse low guard beside his hip, its curved blade clearly outside his body. Exactly two hands, each visibly gripping one blade. Khopesh are broad curved sword blades with a straight lower section and heavy crescent tip, not small farming sickles.','His LEFT arm reaches forward toward the left, LEFT fist around the handle of a large deeply curved Egyptian khopesh. His RIGHT arm bends down beside his RIGHT hip, RIGHT fist around the handle of the SECOND khopesh, whose broad crescent blade curls outward to the right. The two hands are far apart: one at chest height on the left, one at waist height on the right. Each sword connects directly to its own closed hand. Exactly two arms and two swords. Both are unmistakable Egyptian KHOPESH: a straight lower blade flaring into a broad heavy crescent upper half with a deeply concave inside edge. No straight katana, no detached sword, no sword hanging from belt, no crossed arms.')
    prompt=prompt.replace('a stepped corroded bronze gorget','a small stepped DARK CORRODED BRONZE gorget with dull worn ochre edges, blackened recesses and no shiny gold')
    prompt=prompt.replace('a visible grim ashen face','a visible grim human face of dark ash-brown skin')
    prompt=prompt.replace('old ash-linen wraps on forearms and calves','visible pale weathered burial bandages on forearms and calves')
    prompt += '\nThe sprite must retain the deeply decayed brown-on-brown colour relationships of the reference. Dark rotted sepia trousers, no blue fabric, no bright gold armour, no white face paint, no skull mask. Crisp native-scale pixel shapes and intentional highlights; no noisy photographic surface texture.'
    if '--short' in sys.argv:
        prompt='pixel art sprite, game asset, 32-bit pixel art. Redraw the supplied approved Bloodridge fighter as his tribe\'s elite champion. Preserve this exact rendering style, limited sepia and ash-brown colours, glowing ember cracks, burial wraps, deep corroded bronze and lean anatomy. Keep the hood and visible human face under it, clean shaven, the same three-quarter view facing down-left and the same native pixel cluster size. Change his pose: a deep forward duelling stance with separated planted feet, lead knee bent, rear leg braced to the right. One twin khopesh is held forward low and the other blade curls upward beside the rear shoulder in a protective guard. Two separate hands gripping two thick crescent Egyptian khopesh, each blade curved like those in the reference. Add an asymmetric ragged shoulder stole and a segmented tarnished bronze collar. Keep the threatening dark decayed Medjay identity. Native 112px fighter enlarged with nearest pixels. Single isolated whole-body sprite, all blades and feet inside the canvas, plain uniform blue-grey background. No extra weapons, shiny gold, beard, white face paint, blue trousers, muscular bodybuilder, realistic skin texture, bright backdrop or text.'
    graph['5']['inputs']['text']=prompt
    graph['6']['inputs']['text']='chibi, child, cartoon, stick limbs, stiff upright pose, tiny sickles, oversized head, front view, blurry painting, smooth gradients, photograph, extra arms, extra weapons, fire aura, background scenery, text'
    if '--short' in sys.argv:graph['6']['inputs']['text']=''
    graph['7']['inputs']['image']='ks-champion-flux-20261014/approved-skirmisher.png'
    graph['8']['inputs'].update(upscale_method='nearest-exact',width=640,height=672)
    graph['13']['inputs']['cfg']=4.0
    graph['15']['inputs']['steps']=28
    seed=int(sys.argv[1]) if len(sys.argv)>1 else 2026101451
    graph['16']['inputs']['noise_seed']=seed
    graph['19']['inputs']['filename_prefix']='KS-Champion-Flux-20261014/champion-'+str(seed)
    (ROOT/'prompt.txt').write_text(prompt,encoding='utf-8')
    (ROOT/('workflow-'+str(seed)+'.json')).write_text(json.dumps(graph,indent=2))
    submission=api('/prompt',{'prompt':graph,'client_id':'ks-champion-flux-20261014'})
    (ROOT/('submission-'+str(seed)+'.json')).write_text(json.dumps(submission,indent=2))
    pid=submission['prompt_id'];print('Submitted '+pid,flush=True);start=time.monotonic()
    while time.monotonic()-start<1800:
        result=api('/history/'+pid).get(pid)
        if result:
            (ROOT/('history-'+str(seed)+'.json')).write_text(json.dumps(result,indent=2))
            if result.get('status',{}).get('status_str')=='error':raise RuntimeError(str(result['status']['messages'][-1]))
            for out in result.get('outputs',{}).values():
                for im in out.get('images',[]):
                    target=ROOT/('flux-'+str(seed)+'.png');shutil.copyfile(OUTPUT/im.get('subfolder','')/im['filename'],target)
                    print(json.dumps({'elapsed_seconds':round(time.monotonic()-start),'image':str(target)}),flush=True);return
        time.sleep(5)
    raise TimeoutError('Inspect saved job ID; do not resubmit automatically.')
if __name__=='__main__':main()

