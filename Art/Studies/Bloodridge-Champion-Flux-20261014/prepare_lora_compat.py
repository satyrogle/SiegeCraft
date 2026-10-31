import json,struct,pathlib,hashlib
ROOT=pathlib.Path(__file__).resolve().parent
source=pathlib.Path('C:/Users/jacob/ComfyUI-Shared/models/loras/flux2klein_pixel_art_sprite_limbicnation.safetensors')
base=pathlib.Path('C:/Users/jacob/ComfyUI-Shared/models/diffusion_models/flux-2-klein-base-4b.safetensors')
target=source.parent/'ks-champion-flux-20261014/modulation-compat.safetensors'
def header(path):
 with path.open('rb') as f:
  n=struct.unpack('<Q',f.read(8))[0];return json.loads(f.read(n)),n+8
h,start=header(source);b,_=header(base)
out={};payload=bytearray();records=[]
with source.open('rb') as f:
 for stem in ['double_stream_modulation_img','double_stream_modulation_txt','single_stream_modulation']:
  a=h[stem+'.linear.lora_A.weight'];z=h[stem+'.linear.lora_B.weight']
  assert [z['shape'][0],a['shape'][1]]==b[stem+'.lin.weight']['shape']
  assert a['shape'][0]==z['shape'][1]
  for part in ['A','B']:
   old=stem+'.linear.lora_'+part+'.weight';new='diffusion_model.'+stem+'.lin.lora_'+part+'.weight'
   entry=h[old];lo,hi=entry['data_offsets'];f.seek(start+lo);raw=f.read(hi-lo)
   assert len(raw)==hi-lo
   out[new]={'dtype':entry['dtype'],'shape':entry['shape'],'data_offsets':[len(payload),len(payload)+len(raw)]}
   payload.extend(raw)
   records.append({'source_key':old,'compatible_key':new,'shape':entry['shape'],'tensor_bytes_sha256':hashlib.sha256(raw).hexdigest()})
out['__metadata__']={'source':source.name,'purpose':'KS review-only naming compatibility; tensor bytes unchanged'}
encoded=json.dumps(out,separators=(',',':')).encode();encoded+=b' '*((-len(encoded))%8)
target.parent.mkdir(parents=True,exist_ok=True)
with target.open('wb') as f:f.write(struct.pack('<Q',len(encoded)));f.write(encoded);f.write(payload)
check,offset=header(target)
with target.open('rb') as f:
 for rec in records:
  lo,hi=check[rec['compatible_key']]['data_offsets'];f.seek(offset+lo)
  assert hashlib.sha256(f.read(hi-lo)).hexdigest()==rec['tensor_bytes_sha256']
evidence={'source':str(source),'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'target':str(target),'tensor_count':len(records),'tensor_bytes_identical':True,'records':records}
(ROOT/'lora-compatibility.json').write_text(json.dumps(evidence,indent=2))
print(json.dumps({'target':str(target),'tensors':len(records),'bytes':target.stat().st_size}))

