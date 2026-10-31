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
base=read(root.parent/"Bloodridge-Champion-Pro-20261014/champion-review-1x.png")
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
regions=json.loads((root/"native-cleanup.json").read_text())["changed_regions"]
def authorized(x,y):return any(r["x"]<=x<r["x"]+r["width"] and r["y"]<=y<r["y"]+r["height"] for r in regions)
changed=[(x,y) for y in range(168) for x in range(160) if a.getpixel((x,y))!=base.getpixel((x,y))]
outside=[p for p in changed if not authorized(*p)]
def area_same(x,y,w,h):return all(a.getpixel((xx,yy))==base.getpixel((xx,yy)) for yy in range(y,y+h) for xx in range(x,x+w))
points={(x,y) for y in range(168) for x in range(160) if a.getpixel((x,y))[3]}
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
 "status":"Static identity revision awaiting owner approval",
 "route":"One PixelLab Pixen edit used only for native headwrap guide; Aseprite authored face, shoulder, cloth and weapon edits",
 "subscription_generations":1,"native_canvas":list(a.size),"opaque_bounds_exclusive":list(a.getbbox()),
 "opaque_colors":len(actual),"alpha_values":sorted({c[3] for c in pixels(a)}),
 "off_approved_frame_colors":[list(c) for c in sorted(actual-allowed)],
 "components_8_neighbor":sorted(components,reverse=True),
 "changed_pixels_from_previous_candidate":len(changed),"changes_outside_named_feature_regions":outside,
 "both_grips_unchanged":area_same(103,32,18,15) and area_same(24,87,23,17),
 "front_foot_unchanged":area_same(24,128,25,13),"rear_foot_unchanged":area_same(99,119,23,12),
 "source_png_sha256":sha(source),"source_atlas_sha256":sha(atlas),
 "source_hashes_unchanged":sha(source)=="4a55489a84ae18391264ad345b97ec96a70022eac38a0b0f7408dfee21bd0210" and sha(atlas)=="ad176a6a94541b6c7ec8b670fc05d5ffb83349cbc8ebdd87af0876ac799e7a25",
 "native_lineup_source_pixels_exact":protected,"native_lineup_candidate_pixels_exact":exact,"floor_candidate_pixels_exact":floor_exact,
 "exact_4x_nearest":up.size==(640,672) and all(up.getpixel((x,y))==a.getpixel((x//4,y//4)) for y in range(672) for x in range(640)),
 "reopened_master_pixel_identical":a.size==reopen.size and pixels(a)==pixels(reopen),
 "visual_audit":{
  "cold_read":"A scarred burial-wrapped duel fighter in a high and low khopesh guard, with hanging bronze grave plates and split linen strips.",
  "identity_changes":["Pointed assassin-like hood replaced by open burial headwrap","Ash face with three deliberate diagonal cheek fissures","Three tethered bronze plates replace generated shoulder spikes","Pointed tabard end changed into three unequal folded burial strips","Upper sickle spine thickened in bronze, lower inner edge strengthened"],
  "observed":"Headwrap changes the face and head silhouette at 1x; high/low blades remain readable beside approved tribe art and on the captured floor.",
  "reservation":"The pale headwrap is now the strongest head marker; owner must judge whether that contrast fits the champion.",
  "not_verified":["Owner approval","Animation","Crowded fight readability","Runtime binding"],
  "legal_scope":"Visual differentiation only; no legal clearance conclusion"
 },
 "floor_study":"Static native composition on existing captured KS floor with separate contact shadows. Not a new running fight capture.",
 "validation_note":"Callable MCP validate remains unavailable; Aseprite palette analyze and independent PNG checks used."
}
checks=[a.size==(160,168),result["alpha_values"]==[0,255],not result["off_approved_frame_colors"],len(components)==1,not outside,result["both_grips_unchanged"],result["front_foot_unchanged"],result["rear_foot_unchanged"],result["source_hashes_unchanged"],protected,exact,floor_exact,result["exact_4x_nearest"],result["reopened_master_pixel_identical"]]
result["mechanical_checks_passed"]=all(checks)
(root/"audit.json").write_text(json.dumps(result,indent=2),encoding="utf-8")
print(json.dumps(result,indent=2))
assert all(checks)

