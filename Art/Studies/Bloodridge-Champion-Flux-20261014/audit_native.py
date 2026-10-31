from pathlib import Path
from PIL import Image
from collections import deque
import hashlib,json
root=Path(__file__).resolve().parent
project=root.parents[2]
def im(n):return Image.open(root/n).convert("RGBA")
def pix(i):return list(i.get_flattened_data())
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
a=im("champion-review-1x.png");up=im("champion-review-4x.png");reopen=im("champion-reopened-check.png")
source=project/"Assets/Resources/ClarthStudy/blood_rot_skirmisher_sw_idle.png"
atlas=source.with_suffix(".json")
ref=Image.open(source).convert("RGBA").crop((0,0,160,168))
allowed={c for c in pix(ref) if c[3]}
actual={c for c in pix(a) if c[3]}
board=im("native-lineup.png")
protected=all(board.getpixel((x+20,y+40))==ref.getpixel((x,y)) for y in range(168) for x in range(160) if ref.getpixel((x,y))[3])
candidate_exact=all(board.getpixel((x+210,y+40))==a.getpixel((x,y)) for y in range(168) for x in range(160) if a.getpixel((x,y))[3])
points={(x,y) for y in range(168) for x in range(160) if a.getpixel((x,y))[3]}
components=[]
while points:
 p=points.pop();q=deque([p]);component=[p]
 while q:
  x,y=q.popleft()
  for dx in [-1,0,1]:
   for dy in [-1,0,1]:
    n=(x+dx,y+dy)
    if n in points:points.remove(n);q.append(n);component.append(n)
 components.append(len(component))
result={
"status":"Static owner-review candidate; no production integration",
"chosen_generator":"PixelLab Pixflux, followed by native Aseprite guard and material edits; NOT a local FLUX output",
"local_FLUX_candidates":"All rejected: anatomy/material drift, malformed weapons or compatibility corruption",
"pixelLab_generations_charged":2,
"native_canvas":list(a.size),"opaque_bounds_exclusive":list(a.getbbox()),"opaque_colors":len(actual),
"alpha_values":sorted({c[3] for c in pix(a)}),"off_approved_frame_colors":[list(c) for c in actual-allowed],
"connected_components_8_neighbor":sorted(components,reverse=True),
"source_png_sha256":sha(source),"source_atlas_sha256":sha(atlas),
"source_hashes_unchanged":sha(source)=="4a55489a84ae18391264ad345b97ec96a70022eac38a0b0f7408dfee21bd0210" and sha(atlas)=="ad176a6a94541b6c7ec8b670fc05d5ffb83349cbc8ebdd87af0876ac799e7a25",
"native_lineup_source_pixels_exact":protected,"native_lineup_candidate_pixels_exact":candidate_exact,
"exact_4x_nearest":up.size==(640,672) and all(up.getpixel((x,y))==a.getpixel((x//4,y//4)) for y in range(672) for x in range(640)),
"reopened_master_pixel_identical":a.size==reopen.size and pix(a)==pix(reopen),
"visual_audit":{"silhouette":"Raised forward parrying blade, low reverse blade, broader chest and asymmetric cowl; clear difference from approved crouching Skirmisher","tribe":"Native earth palette, corroded bronze, pale wraps and narrow ember weapon accents retained","anatomy":"Two arms, two gripping hands, two separate feet; no new body resampling","limitations":["Forward weapon is shorter in silhouette than rear weapon","Static guard only; animation and live fight not checked","Owner judgement pending"]},
"floor_study":"Composited at native pixels onto an unchanged crop of captured KS fight; review contact shadows added. Not runtime integration.",
"validation_note":"MCP validate is unavailable. Palette analyze plus independent native PNG checks used."
}
checks=[a.size==(160,168),result["alpha_values"]==[0,255],not result["off_approved_frame_colors"],result["source_hashes_unchanged"],protected,candidate_exact,result["exact_4x_nearest"],result["reopened_master_pixel_identical"]]
result["mechanical_checks_passed"]=all(checks)
(root/"audit.json").write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))
assert all(checks)

