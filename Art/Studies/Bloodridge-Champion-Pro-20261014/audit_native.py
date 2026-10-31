from pathlib import Path
from PIL import Image
from collections import deque
import hashlib,json
root=Path(__file__).resolve().parent
project=root.parents[2]
def read(p):return Image.open(p).convert("RGBA")
def pixels(i):return list(i.get_flattened_data())
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
a=read(root/"champion-review-1x.png")
up=read(root/"champion-review-4x.png")
reopen=read(root/"champion-reopened-check.png")
source=project/"Assets/Resources/ClarthStudy/blood_rot_skirmisher_sw_idle.png"
atlas=source.with_suffix(".json")
ref=read(source).crop((0,0,160,168))
allowed={c for c in pixels(ref) if c[3]}
actual={c for c in pixels(a) if c[3]}
board=read(root/"native-lineup.png")
protected=all(board.getpixel((x+20,y+40))==ref.getpixel((x,y)) for y in range(168) for x in range(160) if ref.getpixel((x,y))[3])
exact=all(board.getpixel((x+210,y+40))==a.getpixel((x,y)) for y in range(168) for x in range(160) if a.getpixel((x,y))[3])
points={(x,y) for y in range(a.height) for x in range(a.width) if a.getpixel((x,y))[3]}
components=[]
while points:
 p=points.pop();q=deque([p]);n=0
 while q:
  x,y=q.popleft();n+=1
  for dx in [-1,0,1]:
   for dy in [-1,0,1]:
    nxt=(x+dx,y+dy)
    if nxt in points:points.remove(nxt);q.append(nxt)
 components.append(n)
floor=read(root/"floor-placement-1x.png")
floor_exact=all(floor.getpixel((x+38,y+20))==a.getpixel((x,y)) for y in range(152) for x in range(160) if a.getpixel((x,y))[3])
result={
 "status":"Static candidate awaiting owner review",
 "route":"PixelLab Pro candidate 2 plus native Aseprite southwest flip, leg posing and four displaced-pixel cleanup",
 "pixelLab_generations_charged":15,
 "native_canvas":list(a.size),"opaque_bounds_exclusive":list(a.getbbox()),
 "opaque_colors":len(actual),"alpha_values":sorted({c[3] for c in pixels(a)}),
 "off_approved_frame_colors":[list(c) for c in sorted(actual-allowed)],
 "components_8_neighbor":sorted(components,reverse=True),
 "source_png_sha256":sha(source),"source_atlas_sha256":sha(atlas),
 "source_hashes_unchanged":sha(source)=="4a55489a84ae18391264ad345b97ec96a70022eac38a0b0f7408dfee21bd0210" and sha(atlas)=="ad176a6a94541b6c7ec8b670fc05d5ffb83349cbc8ebdd87af0876ac799e7a25",
 "native_lineup_source_pixels_exact":protected,"native_lineup_candidate_pixels_exact":exact,"floor_candidate_pixels_exact":floor_exact,
 "exact_4x_nearest":up.size==(640,672) and all(up.getpixel((x,y))==a.getpixel((x//4,y//4)) for y in range(672) for x in range(640)),
 "reopened_master_pixel_identical":a.size==reopen.size and pixels(a)==pixels(reopen),
 "visual_audit":{
  "cold_read":"A hooded grave-duelist holding one substantial khopesh overhead and another low, coiled over a bent front leg with a torn mantle trailing behind.",
  "first_reads":["High blade and raised hand","Ember-scarred face","Low crescent blade and separate feet"],
  "observed_improvements":["Two substantial equipped blades; former tiny raised weapon removed by new generation","Distinct high/low silhouette and more compressed active stance","Same native body category and matching earthy rendering beside approved Skirmisher"],
  "remaining_limits":["High striking guard is more expressive than a relaxed idle; animation production needs its own anticipation and recovery poses","Chest rendered as close-fitting layered battle gear rather than the brief's bare chest","Owner approval pending; movement, combat crowding and runtime binding untested"]
 },
 "floor_study":"Static native composition on unchanged captured KS floor crop; separate review contact shadows. Not a running fight capture.",
 "validation_note":"No callable MCP validate tool is exposed; Aseprite palette analyze and independent PNG checks used."
}
checks=[a.size==(160,168),result["alpha_values"]==[0,255],not result["off_approved_frame_colors"],result["source_hashes_unchanged"],protected,exact,floor_exact,result["exact_4x_nearest"],result["reopened_master_pixel_identical"],len(components)==1]
result["mechanical_checks_passed"]=all(checks)
(root/"audit.json").write_text(json.dumps(result,indent=2),encoding="utf-8")
print(json.dumps(result,indent=2))
assert all(checks)

