from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import json, urllib.request, concurrent.futures, hashlib, subprocess, sys
ROOT=Path(__file__).parent
PY=sys.executable
ASE=Path(r"C:/Program Files (x86)/Steam/steamapps/common/Aseprite/Aseprite.exe")
DURATIONS={"leader":120,"butcher":100,"glutton":160,"shade":130,"whisperer":110}
LABELS={"leader":"LEADER - Palm relic","butcher":"BUTCHER - Anvil work","glutton":"GLUTTON - Sleeping","shade":"SHADE - Reading orders","whisperer":"WHISPERER - Veil weaving"}
FONT=ImageFont.truetype("C:/Windows/Fonts/arial.ttf",22)
def download(task):
    path,url=task
    if not path.exists():
        req=urllib.request.Request(url,headers={"User-Agent":"KS-ArtReview/1"})
        with urllib.request.urlopen(req,timeout=60) as r: data=r.read()
        path.write_bytes(data)
    with Image.open(path) as im: assert im.size==(192,192),(path,im.size)
def process(name,meta):
    folder=ROOT/name
    raw=folder/"raw"; prepared=folder/"prepared"; frames=folder/"frames"
    for d in (raw,prepared,frames):d.mkdir(parents=True,exist_ok=True)
    urls=meta["download_urls"]
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        list(pool.map(download,[(raw/f"frame_{i:03d}.png",u) for i,u in enumerate(urls)]))
    sources=[Image.open(raw/f"frame_{i:03d}.png").convert("RGBA") for i in range(len(urls))]
    raw_count=len(sources)
    # PixelLab echoes the untouched input before its 40 generated motion frames.
    # Keep that source in raw/, but do not insert its different rasterisation into each repeat.
    assert raw_count==41,(name,raw_count)
    sources=sources[1:]
    playback_source_indices=list(range(1,41))
    if name=="glutton":
        # A breath can naturally retrace its expansion; avoid the generated closing colour jump.
        segment=sources[3:23]
        sources=segment+list(reversed(segment))
        playback_source_indices=list(range(4,24))+list(range(23,3,-1))
    # Preserve the fixed native canvas and global scale; do not recenter individual poses.
    for i,im in enumerate(sources):
        a=im.getchannel("A").point(lambda v:255 if v>=128 else 0)
        im.putalpha(a); im.save(prepared/f"frame_{i:03d}.png")
    cmd=[str(ASE),"-b","--script-param",f"dir={folder.as_posix()}","--script-param",f"count={len(sources)}","--script-param",f"ms={DURATIONS[name]}","--script",str(ROOT/"export_loop.lua")]
    result=subprocess.run(cmd,capture_output=True,text=True)
    (folder/"aseprite_export.log").write_text(result.stdout+"\n"+result.stderr,encoding="utf-8")
    if result.returncode!=0: raise RuntimeError(f"{name} export failed: {result.stderr}")
    imgs=[Image.open(frames/f"frame_{i:03d}.png").convert("RGBA") for i in range(len(sources))]
    assert len(imgs)>=40
    colour_set=set()
    for im in imgs: colour_set.update(im.getdata())
    assert len(colour_set)<=32,(name,len(colour_set))
    bounds=[im.getbbox() for im in imgs]
    edge_frames=[i for i,im in enumerate(imgs) if im.getchannel("A").crop((0,0,192,1)).getbbox() or im.getchannel("A").crop((0,191,192,192)).getbbox() or im.getchannel("A").crop((0,0,1,192)).getbbox() or im.getchannel("A").crop((191,0,192,192)).getbbox()]
    def delta(a,b):
        return sum(x!=y for x,y in zip(a.getdata(),b.getdata()))
    differences=[delta(imgs[i],imgs[i+1]) for i in range(len(imgs)-1)]
    duplicate_end=name!="glutton" and delta(imgs[-1],imgs[0])==0
    # Remove only an exact repeated closing frame; no invented interpolation.
    if duplicate_end:
        imgs=imgs[:-1]
        rerun=cmd.copy()
        rerun[rerun.index(f"count={len(sources)}")]=f"count={len(imgs)}"
        result=subprocess.run(rerun,capture_output=True,text=True)
        if result.returncode!=0: raise RuntimeError(f"{name} trimmed export failed")
    count=len(imgs); columns=8; rows=(count+columns-1)//columns
    sheet=Image.new("RGBA",(192*columns,192*rows))
    for i,im in enumerate(imgs):sheet.paste(im,((i%columns)*192,(i//columns)*192))
    sheet.save(folder/"sheet.png")
    samples=[round(i*(count-1)/7) for i in range(8)]
    contact=Image.new("RGB",(4*384,2*432),(43,45,52));draw=ImageDraw.Draw(contact)
    for j,i in enumerate(samples):
        tile=imgs[i].resize((384,384),Image.Resampling.NEAREST)
        xy=((j%4)*384,(j//4)*432+38);contact.paste(tile,xy,tile)
        draw.text(((j%4)*384+12,(j//4)*432+8),f"{name} / {i+1:02d}",font=FONT,fill=(233,225,215))
    contact.save(folder/"contact.png")
    report={"name":name,"raw_frames":raw_count,"echoed_reference_excluded_from_playback":True,"frames":count,"frame_ms":DURATIONS[name],"duration_ms":count*DURATIONS[name],"native_size":[192,192],"colours_total":len(colour_set),"bounds":bounds,"edge_frames":edge_frames,"exact_duplicate_closing_frame_removed":duplicate_end,"seam_changed_pixels":delta(imgs[-1],imgs[0]),"adjacent_changed_pixels":differences,"unique_frames":len({im.tobytes() for im in imgs}),"aseprite":str(folder/"camp_work.aseprite"),"sha256_sheet":hashlib.sha256((folder/"sheet.png").read_bytes()).hexdigest()}
    (folder/"validation.json").write_text(json.dumps(report,indent=2))
    report["playback_source_indices"]=playback_source_indices
    report["playback_mode"]="forward_return_breath" if name=="glutton" else "forward"
    ase_bytes=(folder/"camp_work.aseprite").read_bytes()
    import struct
    report["aseprite_frame_count"]=struct.unpack_from("<H",ase_bytes,6)[0]
    assert report["aseprite_frame_count"]==count,(name,report["aseprite_frame_count"],count)
    (folder/"validation.json").write_text(json.dumps(report,indent=2))
    clip={"name":name,"label":LABELS[name],"count":count,"ms":DURATIONS[name],"columns":columns,"width":192,"height":192,"sheet":f"{name}/sheet.png","gif":f"{name}/camp_work.gif","master":f"{name}/camp_work.aseprite","contact":f"{name}/contact.png"}
    (folder/"clip.json").write_text(json.dumps(clip,indent=2))
    print(json.dumps({k:report[k] for k in ("name","raw_frames","frames","colours_total","edge_frames","seam_changed_pixels","unique_frames")}),flush=True)
    return clip
if __name__=="__main__":
    data=json.loads((ROOT/"completed_jobs.json").read_text())
    names=sys.argv[1:] or list(data)
    clips=[]
    for name in names:clips.append(process(name,data[name]))
    (ROOT/"clips.json").write_text(json.dumps([json.loads((ROOT/name/"clip.json").read_text()) for name in DURATIONS if (ROOT/name/"clip.json").exists()],indent=2))
