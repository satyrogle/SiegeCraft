from pathlib import Path
from PIL import Image
from collections import Counter, deque
import hashlib, json
root=Path(__file__).resolve().parent
project=root.parents[2]
def image(name): return Image.open(root/name).convert("RGBA")
def pixels(im): return list(im.getdata())
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
native=image("champion-review-1x.png")
large=image("champion-review-4x.png")
reopened=image("champion-reopened-check.png")
source=project/"Assets/Resources/ClarthStudy/blood_rot_skirmisher_sw_idle.png"
atlas=source.with_suffix(".json")
reference=Image.open(source).convert("RGBA").crop((0,0,160,168))
chiefsrc=project/"Assets/Resources/CampaignArt/bloodridge_chief_sw_idle.png"
chief=Image.open(chiefsrc).convert("RGBA").crop((0,0,192,256))
allowed={c for c in pixels(reference) if c[3]}
actual={c for c in pixels(native) if c[3]}
lineup=image("native-lineup.png")
refs=[("approved_skirmisher",reference,(28,102)),("champion",native,(203,102)),("approved_chief",chief,(405,18))]
lineup_exact={}
for name,im,(ox,oy) in refs:
    lineup_exact[name]=all(lineup.getpixel((x+ox,y+oy))==im.getpixel((x,y)) for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))[3])
transparent={(x,y) for y in range(native.height) for x in range(native.width) if native.getpixel((x,y))[3]==0}
edge={p for p in transparent if p[0] in (0,native.width-1) or p[1] in (0,native.height-1)}
queue=deque(edge)
while queue:
    x,y=queue.popleft()
    for p in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
        if p in transparent and p not in edge:
            edge.add(p);queue.append(p)
audit={
"native_canvas":list(native.size),
"opaque_bounds_exclusive":list(native.getbbox()),
"intended_body_height_px":112,
"body_head_y":29,
"ground_y":140,
"opaque_colors":len(actual),
"alpha_values":sorted({p[3] for p in pixels(native)}),
"off_approved_palette":[list(x) for x in actual-allowed],
"source_png_sha256":sha(source),
"source_atlas_sha256":sha(atlas),
"source_hash_unchanged":sha(source)=="4a55489a84ae18391264ad345b97ec96a70022eac38a0b0f7408dfee21bd0210" and sha(atlas)=="ad176a6a94541b6c7ec8b670fc05d5ffb83349cbc8ebdd87af0876ac799e7a25",
"approved_reference_equals_runtime_frame":pixels(reference)==pixels(Image.open(root.parent/"Bloodridge-Champion-Specimen-20261014/approved-skirmisher-native.png").convert("RGBA")),
"chief_reference_equals_runtime_frame":pixels(chief)==pixels(image("approved-chief-native.png")),
"four_x_exact_nearest":large.size==(640,672) and all(large.getpixel((x,y))==native.getpixel((x//4,y//4)) for y in range(large.height) for x in range(large.width)),
"reopened_master_pixel_identical":native.size==reopened.size and pixels(native)==pixels(reopened),
"comparison_board_opaque_pixels_exact":lineup_exact,
"enclosed_transparent_pixel_count":len(transparent-edge),
"floor_review":"Native static composition on a crop of a captured KS fight. Added review contact shadow. Not a new game capture.",
"mechanical_validator":"Aseprite MCP validate unavailable. Palette analysis and this independent read-only PNG audit.",
"visual_verdict":"Distinct pose and clear native silhouette; wraps, collar, face and twin khopesh retain tribe cues. Leaner and less ornate than approved Skirmisher. Owner review pending; not animated or integrated."
}
checks=[
native.size==(160,168),audit["alpha_values"]==[0,255],not audit["off_approved_palette"],
audit["source_hash_unchanged"],audit["approved_reference_equals_runtime_frame"],audit["chief_reference_equals_runtime_frame"],
audit["four_x_exact_nearest"],audit["reopened_master_pixel_identical"],all(lineup_exact.values()),
audit["enclosed_transparent_pixel_count"]==0]
audit["mechanical_checks_passed"]=all(checks)
(root/"audit.json").write_text(json.dumps(audit,indent=2),encoding="utf-8")
print(json.dumps(audit,indent=2))
assert all(checks), "An audit check failed"

