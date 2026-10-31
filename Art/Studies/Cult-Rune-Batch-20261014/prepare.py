from pathlib import Path
from PIL import Image, ImageDraw
import json, struct, shutil, hashlib

ROOT=Path(__file__).resolve().parent
INPUT=Path('C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input/ks-rune-batch-20261014')
INPUT.mkdir(parents=True,exist_ok=True)
shutil.copyfile(ROOT.parents[2]/'Art/Concepts/InnerCircle/2026-10-10/leader_neural_relic_v17.png',INPUT/'leader.png')
# Blank composition guides reserve a dark, square rune field. These are structure guides, not final art.
for style in ['A','B']:
    im=Image.new('RGB',(128,128),'#242329'); d=ImageDraw.Draw(im)
    pts=[(23,10),(59,10),(63,15),(68,10),(105,10),(117,23),(117,105),(105,117),(69,117),(64,112),(59,117),(23,117),(10,104),(10,23)]
    if style=='B':pts=[(25,9),(55,9),(61,15),(70,9),(103,9),(119,26),(115,64),(119,103),(103,119),(70,115),(62,119),(24,119),(9,103),(13,64),(9,25)]
    d.polygon(pts,fill='#15121e',outline='#755875',width=2)
    d.line([(25,17),(54,17)],fill='#b8945f',width=2);d.line([(78,17),(102,17),(110,25)],fill='#755238',width=2)
    d.polygon([(30,28),(98,28),(101,31),(101,97),(98,100),(30,100),(27,97),(27,31)],fill='#100e18')
    im.resize((768,768),Image.Resampling.NEAREST).save(INPUT/f'blank-{style}.png')

# Adapt only the six previously ignored naming keys, keeping every tensor byte identical.
source=Path('C:/Users/jacob/ComfyUI-Shared/models/loras/flux2klein_pixel_art_sprite_limbicnation.safetensors')
target=source.parent/'ks-rune-batch-20261014'/'pixel-art-modulation-keymap.safetensors'
target.parent.mkdir(exist_ok=True)
with source.open('rb') as f:
    old_size=struct.unpack('<Q',f.read(8))[0]; header=json.loads(f.read(old_size)); maps={}
    for module in ['double_stream_modulation_img','double_stream_modulation_txt','single_stream_modulation']:
        for term in ['A','B']:
            old=f'{module}.linear.lora_{term}.weight'; new=f'diffusion_model.{module}.lin.lora_{term}.weight'
            if old in header: header[new]=header.pop(old); maps[old]=new
    assert len(maps)==6
    header['__metadata__']={**header.get('__metadata__',{}),'ks_source':str(source),'ks_key_remap':json.dumps(maps)}
    blob=json.dumps(header,separators=(',',':')).encode(); blob+=b' '*((-len(blob))%8)
    before=hashlib.sha256()
    with target.open('wb') as out:
        out.write(struct.pack('<Q',len(blob)));out.write(blob)
        while chunk:=f.read(1024*1024):before.update(chunk);out.write(chunk)
with target.open('rb') as f:
    n=struct.unpack('<Q',f.read(8))[0];f.seek(n,1);after=hashlib.sha256()
    while chunk:=f.read(1024*1024):after.update(chunk)
assert before.hexdigest()==after.hexdigest()
(ROOT/'lora-keymap.json').write_text(json.dumps({'source':str(source),'adapted_copy':str(target),'mapped_keys':maps,'tensor_payload_sha256':after.hexdigest(),'tensor_bytes_identical':True},indent=2))
print('Prepared two local guides, leader reference and a task-scoped LoRA naming adapter; tensor payload unchanged.')
