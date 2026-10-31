from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,urllib.request,concurrent.futures,subprocess,sys,hashlib,struct,time
ROOT=Path(__file__).parent
ASE=Path(r"C:/Program Files (x86)/Steam/steamapps/common/Aseprite/Aseprite.exe")
SIZE=256
FONT=ImageFont.truetype("C:/Windows/Fonts/arial.ttf",22)
def pixels(im):return list(im.get_flattened_data())
def download(task):
 path,url=task
 if path.exists():return
 for attempt in range(3):
  try:
   req=urllib.request.Request(url,headers={"User-Agent":"KS-ArtReview/1"})
   with urllib.request.urlopen(req,timeout=60) as r:data=r.read()
   with Image.open(__import__('io').BytesIO(data)) as im:assert im.size==(SIZE,SIZE)
   path.write_bytes(data);return
  except Exception:
   if attempt==2:raise
   time.sleep(1)
def timing(state,count):
 if state=="idle":dur=[100]*count
 elif state=="move":dur=[50]*count
 elif state=="attack":dur=[60 if i<12 else 40 if i<20 else 60 for i in range(count)]
 elif state=="hit":dur=[40 if i<5 else 50 for i in range(count)];dur[-1]=130
 elif state=="block":dur=[60]*count;dur[4 if count==15 else 8]=260;dur[7 if count==15 else 15]=130;dur[-1]=180
 elif state=="parry":dur=[60 if i<3 else 30 if i<8 else 50 for i in range(count)];dur[-1]=100
 elif state=="death":dur=([160,120,140,180,160,110,70,50,50,70,100,160,250,250,1200] if count==15 else [110 if i<5 else 70 if i<13 else 50 if i<29 else 90 for i in range(count)]);dur[-1]=1200 if count==15 else 1000
 elif state=="victory":dur=[80]*count;dur[-1]=500
 else:raise ValueError(state)
 return dur
def process(job,meta):
 name=job["name"];state=job["state"];relative=job.get("raw_name",name);folder=ROOT/relative
 for d in ("prepared","frames"):(folder/d).mkdir(parents=True,exist_ok=True)
 raw=[Image.open(folder/"raw"/f"frame_{i:03d}.png").convert("RGBA") for i in range(len(meta["download_urls"]))]
 has_echo=job.get("engine")!="skeleton_v3"
 assert len(raw)==job["count"]+(1 if has_echo else 0),(name,len(raw))
 sources=raw[1:] if has_echo else raw
 frozen_end_from=None
 if state=="death":
  frozen_end_from=9 if len(sources)==15 else max(0,len(sources)-6)
  for i in range(frozen_end_from,len(sources)):sources[i]=sources[-1].copy()
 for i,im in enumerate(sources):
  im.putalpha(im.getchannel("A").point(lambda v:255 if v>=128 else 0))
  im.save(folder/"prepared"/f"frame_{i:03d}.png")
 durations=timing(state,len(sources))
 args=[str(ASE),"-b","--script-param",f"dir={folder.as_posix()}","--script-param",f"count={len(sources)}","--script-param",f"timings={','.join(map(str,durations))}","--script-param",f"state={state}","--script-param",f"palette={(ROOT/'leader_palette.png').as_posix()}","--script",str(ROOT/"export_battle.lua")]
 run=subprocess.run(args,capture_output=True,text=True)
 (folder/"aseprite.log").write_text(run.stdout+"\n"+run.stderr,encoding="utf-8")
 if run.returncode:raise RuntimeError(name+" Aseprite failed: "+run.stdout+run.stderr)
 files=[folder/"frames"/f"frame_{i:03d}.png" for i in range(len(sources))]
 imgs=[Image.open(f).convert("RGBA") for f in files]; colours=set()
 for im in imgs:colours.update(pixels(im))
 assert len(colours)<=32,(name,len(colours))
 base_colours=set(pixels(Image.open(ROOT/"leader_palette.png").convert("RGBA")))
 assert colours.issubset(base_colours|{(0,0,0,0)}),(name,"palette drift")
 ase=(folder/(state+".aseprite")).read_bytes();master_count=struct.unpack_from("<H",ase,6)[0];assert master_count==len(imgs),(name,master_count)
 edges=[i for i,im in enumerate(imgs) if im.getchannel("A").crop((0,0,SIZE,1)).getbbox() or im.getchannel("A").crop((0,SIZE-1,SIZE,SIZE)).getbbox() or im.getchannel("A").crop((0,0,1,SIZE)).getbbox() or im.getchannel("A").crop((SIZE-1,0,SIZE,SIZE)).getbbox()]
 bounds=[im.getbbox() for im in imgs]
 def diff(a,b):return sum(x!=y for x,y in zip(pixels(a),pixels(b)))
 cols=8;rows=(len(imgs)+cols-1)//cols;sheet=Image.new("RGBA",(cols*SIZE,rows*SIZE))
 frame_json=[]
 for i,im in enumerate(imgs):
  x=(i%cols)*SIZE;y=(i//cols)*SIZE;sheet.paste(im,(x,y))
  frame_json.append({"frame":{"x":x,"y":y,"w":SIZE,"h":SIZE},"duration":durations[i]})
 sheet.save(folder/"sheet.png")
 sheet_data={"frames":frame_json,"anchorX":128,"anchorY":219,"loop":job["loop"],"state":state,"direction":job["facing"],"sourcePixelScale":1}
 (folder/"sheet.json").write_text(json.dumps(sheet_data,indent=2))
 samples=[round(i*(len(imgs)-1)/7) for i in range(8)]
 contact=Image.new("RGB",(1024,600),(43,45,52));draw=ImageDraw.Draw(contact)
 for j,i in enumerate(samples):
  x=(j%4)*256;y=(j//4)*300;draw.text((x+10,y+8),f'{state} {job["direction"]} / {i+1:02d}',font=FONT,fill=(232,223,211))
  contact.paste(imgs[i],(x,y+38),imgs[i])
 contact.save(folder/"contact.png")
 gif=Image.open(folder/"preview.gif");gif_ms=0
 for i in range(gif.n_frames):gif.seek(i);gif_ms+=gif.info["duration"]
 assert gif_ms==sum(durations),(name,gif_ms,sum(durations))
 report={"name":name,"state":state,"direction":job["facing"],"canvas":[SIZE,SIZE],"original_sprite_size":[192,192],"padding":[32,32],"pixel_scale":1,"frames":len(imgs),"raw_frames":len(raw),"echoed_reference_excluded":has_echo,"unique_frames":len({im.tobytes() for im in imgs}),"colours":len(colours),"shared_palette_verified":True,"master_frames":master_count,"duration_ms":sum(durations),"durations":durations,"bounds":bounds,"edge_frames":edges,"seam_changed_pixels":diff(imgs[-1],imgs[0]),"adjacent_changed_pixels":[diff(imgs[i],imgs[i+1]) for i in range(len(imgs)-1)],"sha256_sheet":hashlib.sha256((folder/"sheet.png").read_bytes()).hexdigest(),"review_status":"pending_owner"}
 (folder/"validation.json").write_text(json.dumps(report,indent=2))
 report["frozen_end_from"]=frozen_end_from
 (folder/"validation.json").write_text(json.dumps(report,indent=2))
 clip={"name":name,"state":state,"direction":job["direction"],"label":job["label"],"loop":job["loop"],"count":len(imgs),"durations":durations,"ms":sum(durations),"columns":8,"width":SIZE,"height":SIZE,"sheet":relative+"/sheet.png","gif":relative+"/preview.gif","master":relative+"/"+state+".aseprite","contact":relative+"/contact.png"}
 (folder/"clip.json").write_text(json.dumps(clip,indent=2))
 print(json.dumps({k:report[k] for k in ("name","frames","unique_frames","colours","duration_ms","edge_frames","seam_changed_pixels")}),flush=True)
def main():
 jobs=json.loads((ROOT/"selected_jobs.json").read_text());done=json.loads((ROOT/"completed_jobs.json").read_text())
 lookup={j["name"]:j for j in jobs}
 names=sys.argv[1:] or [j["name"] for j in jobs if j.get("raw_name",j["name"]) in done]
 tasks=[]
 for name in names:
  key=lookup[name].get("raw_name",name);d=ROOT/key/"raw";d.mkdir(parents=True,exist_ok=True)
  tasks.extend((d/f"frame_{i:03d}.png",url) for i,url in enumerate(done[key]["download_urls"]))
 with concurrent.futures.ThreadPoolExecutor(max_workers=8) as pool:list(pool.map(download,tasks))
 for name in names:process(lookup[name],done[lookup[name].get("raw_name",name)])
 clips=[json.loads((ROOT/j.get("raw_name",j["name"])/"clip.json").read_text()) for j in jobs if (ROOT/j.get("raw_name",j["name"])/"clip.json").exists()]
 (ROOT/"clips.json").write_text(json.dumps(clips,indent=2))
 reports=[json.loads((ROOT/j.get("raw_name",j["name"])/"validation.json").read_text()) for j in jobs if (ROOT/j.get("raw_name",j["name"])/"validation.json").exists()]
 (ROOT/"asset_checks.json").write_text(json.dumps(reports,indent=2))
if __name__=="__main__":main()
