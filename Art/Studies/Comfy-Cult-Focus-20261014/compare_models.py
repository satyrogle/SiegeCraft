from PIL import Image, ImageDraw, ImageFont
from pathlib import Path
import json

p=Path(__file__).resolve().parent
board=Image.new('RGB',(1632,1032),'#15121b')
d=ImageDraw.Draw(board)
font='C:/Windows/Fonts/segoeui.ttf'
f=lambda size:ImageFont.truetype(font,size)
d.text((32,20),'KINDRED SIEGE / CULT COMMAND STUDY',font=f(21),fill='#be965c')
d.text((32,52),'Qwen and FLUX — one concept each',font=f(37),fill='#e4d9cc')
d.text((32,111),'QWEN IMAGE EDIT 2511',font=f(25),fill='#e4d9cc')
d.text((832,111),'FLUX.2 KLEIN BASE 4B + PIXEL LoRA',font=f(25),fill='#e4d9cc')
for x,name in [(32,'qwen-material-only_00001_.png'),(832,'flux-klein-pixel-lora_00001_.png')]:
    im=Image.open(p/name).convert('RGB')
    assert im.size==(768,768)
    board.paste(im,(x,160))
d.text((32,943),'Same leader reference and material brief. Original outputs, unretouched.',font=f(23),fill='#b3a9bd')
d.text((32,982),'Concept comparison only. FLUX LoRA loaded partially; details saved in the audit.',font=f(22),fill='#b3a9bd')
board.save(p/'qwen-vs-flux.png')
audit={
 'owner_feedback':'Jacob said the Qwen images are great and requested one FLUX comparison before more variants. This supersedes treating Qwen visual direction as owner-rejected; the prior semantic observations remain technical review notes.',
 'model':'flux-2-klein-base-4b.safetensors',
 'lora':'flux2klein_pixel_art_sprite_limbicnation.safetensors',
 'lora_strength':1.0,
 'lora_loading':'63 patches attached; 6 weight entries skipped: lora_A and lora_B on double_stream_modulation_img.linear, double_stream_modulation_txt.linear, single_stream_modulation.linear. Not full LoRA compatibility proof.',
 'settings':{'steps':20,'cfg':5.0,'sampler':'euler','scheduler':'Flux2Scheduler','width':768,'height':768,'seed':2026101402},
 'prompt_id':'44794f2c-d870-4a8d-9ac1-cdff0c281811',
 'generation_count':1,
 'elapsed_seconds':100,
 'reference':'leader_neural_relic_v17.png, local, scaled in workflow to768x512',
 'visual_audit':['Visually inspected full768px output','Sharper separated gold glyphs than Qwen, but split into two groups rather than three converging hooks','Very bright violet/cyan centre and purple frame drift from restrained non-neon brief','Tall card-like silhouette; not a square production command button','Pixel sizes remain inconsistent; still raw concept art','No new variants or automatic state expansion'],
 'comparison':'qwen-vs-flux.png contains both original768px outputs without rescaling or retouching',
 'paid_api_calls':0,'new_downloads':0,'production_changes':False,
 'sources':['https://huggingface.co/Limbicnation/pixel-art-lora','https://huggingface.co/black-forest-labs/FLUX.2-klein-base-4B']
}
(p/'audit-flux.json').write_text(json.dumps(audit,indent=2),encoding='utf-8')
print(p/'qwen-vs-flux.png')
