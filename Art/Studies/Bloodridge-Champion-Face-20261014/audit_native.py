from pathlib import Path
from PIL import Image
from collections import deque
import json,hashlib
root=Path(__file__).resolve().parent
project=root.parents[2]
def im(p):return Image.open(p).convert("RGBA")
def px(a):return list(a.get_flattened_data())
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
a=im(root/"champion-review-1x.png")
base=im(root.parent/"Bloodridge-Champion-Identity-20261014/champion-review-1x.png")
up=im(root/"champion-review-4x.png");re=im(root/"champion-reopened-check.png")
source=project/"Assets/Resources/ClarthStudy/blood_rot_skirmisher_sw_idle.png";atlas=source.with_suffix(".json")
ref=im(source).crop((0,0,160,168));allowed={c for c in px(ref) if c[3]}
changed=[(x,y) for y in range(168) for x in range(160) if a.getpixel((x,y))!=base.getpixel((x,y))]
outside=[p for p in changed if not(64<=p[0]<95 and 29<=p[1]<65)]
board=im(root/"native-lineup.png");floor=im(root/"floor-placement-1x.png")
refexact=all(board.getpixel((x+20,y+40))==ref.getpixel((x,y)) for y in range(168) for x in range(160) if ref.getpixel((x,y))[3])
exact=all(board.getpixel((x+210,y+40))==a.getpixel((x,y)) for y in range(168) for x in range(160) if a.getpixel((x,y))[3])
floorexact=all(floor.getpixel((x+38,y+20))==a.getpixel((x,y)) for y in range(152) for x in range(160) if a.getpixel((x,y))[3])
points={(x,y) for y in range(168) for x in range(160) if a.getpixel((x,y))[3]};components=[]
while points:
 p=points.pop();q=deque([p]);n=0
 while q:
  x,y=q.popleft();n+=1
  for dx in [-1,0,1]:
   for dy in [-1,0,1]:
    v=x+dx,y+dy
    if v in points:points.remove(v);q.append(v)
 components.append(n)
actual={c for c in px(a) if c[3]}
result={"status":"Static face correction for owner review","native_canvas":list(a.size),"opaque_bounds_exclusive":list(a.getbbox()),"opaque_colours":len(actual),"alpha_values":sorted({c[3] for c in px(a)}),"off_approved_colours":[list(c) for c in actual-allowed],"changed_pixels":len(changed),"changes_outside_head":outside,"components_8_neighbour":sorted(components,reverse=True),"source_png_sha256":sha(source),"source_atlas_sha256":sha(atlas),"approved_sources_unchanged":sha(source)=="4a55489a84ae18391264ad345b97ec96a70022eac38a0b0f7408dfee21bd0210" and sha(atlas)=="ad176a6a94541b6c7ec8b670fc05d5ffb83349cbc8ebdd87af0876ac799e7a25","approved_reference_pixels_exact":refexact,"candidate_pixels_exact_in_native_board":exact,"candidate_pixels_exact_on_floor":floorexact,"exact_nearest_4x":up.size==(640,672) and all(up.getpixel((x,y))==a.getpixel((x//4,y//4)) for y in range(672) for x in range(640)),"reopened_master_identical":px(a)==px(re),"visual_audit":{"cold_read":"A stern exposed male face in an open linen headwrap; brow, nose, mouth, cheek, jaw and neck have readable flesh planes.","native_read":"Personally inspected 1x sprite, comparison and captured-floor composition, plus enlarged head and 4x sprite.","limits":["Static only","Owner approval pending","Animation and live combat crowding untested"]},"subscription_generations":1,"validation_note":"No callable MCP validate is exposed; independent PNG checks used."}
checks=[a.size==(160,168),not outside,not result["off_approved_colours"],result["alpha_values"]==[0,255],len(components)==1,result["approved_sources_unchanged"],refexact,exact,floorexact,result["exact_nearest_4x"],result["reopened_master_identical"]]
result["mechanical_checks_passed"]=all(checks)
(root/"audit.json").write_text(json.dumps(result,indent=2),encoding="utf-8")
print(json.dumps(result,indent=2))
assert all(checks)

