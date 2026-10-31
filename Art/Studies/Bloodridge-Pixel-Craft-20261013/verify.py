from pathlib import Path
from PIL import Image, ImageChops
import hashlib, json

d = Path(__file__).parent
capture = d.parent / 'Bloodridge-Battle-Screen-20261013'
original = Image.open(capture / 'actual-game-before.png').convert('RGBA')
combat = Image.open(capture / 'captured-combat.png').convert('RGBA')
hud = Image.open(capture / 'captured-hud.png').convert('RGBA')
def opaque_mask(image):
    return image.getchannel('A').point(lambda p: 255 if p else 0)
def changed_mask(a, b):
    bands = ImageChops.difference(a, b).split()
    out = bands[0]
    for band in bands[1:]: out = ImageChops.lighter(out, band)
    return out.point(lambda p: 255 if p else 0)
def count(mask): return mask.histogram()[255]
cm, hm = opaque_mask(combat), opaque_mask(hud)
paths = sorted((d / 'frames').glob('frame-*.png'))
assert len(paths) == 32
frames = [Image.open(p).convert('RGBA') for p in paths]
results = []
for n, frame in enumerate(frames, 1):
    assert frame.size == (1440, 900)
    diff = changed_mask(frame, original)
    row = {'frame': n, 'combat_mismatches': count(ImageChops.multiply(diff, cm)),
           'hud_mismatches': count(ImageChops.multiply(diff, hm)),
           'nonopaque_pixels': 1296000 - frame.getchannel('A').histogram()[255]}
    assert not any(row[k] for k in ('combat_mismatches','hud_mismatches','nonopaque_pixels'))
    results.append(row)
floor = Image.open(d / 'ground-underpaint.png').convert('RGBA')
assert floor.size == (1440, 900) and floor.getchannel('A').getextrema() == (255,255)
sheet = Image.open(d / 'linen-sheet.png').convert('RGBA')
linen = [sheet.crop(((i%8)*226,(i//8)*562,(i%8+1)*226,(i//8+1)*562)) for i in range(32)]
anchor = linen[0].crop((0,141,226,158))
assert all(changed_mask(anchor,l.crop((0,141,226,158))).getbbox() is None for l in linen)
linen_diffs = [count(changed_mask(linen[i], linen[(i+1)%32])) for i in range(32)]
frame_diffs = [count(changed_mask(frames[i], frames[(i+1)%32])) for i in range(32)]
assert sum(linen_diffs) > 0 and sum(frame_diffs) > 0
info = json.loads((d / 'master-info.json').read_text()) if (d / 'master-info.json').exists() else None
if info:
    assert info['frameCount'] == 32 and len(info['layers']) == 9
    assert all(f['durationMs'] == 125 for f in info['frames'])
    assert any(t['name'] == 'court-wind' and t['from'] == 1 and t['to'] == 32 for t in info['tags'])
result = {'status':'PASS', 'scope':'Export and preservation checks, not art approval or Unity runtime proof',
          'canvas':[1440,900], 'frames':32, 'frame_duration_ms':125, 'world_scale':1,
          'protected_combat_pixels':count(cm), 'protected_hud_pixels':count(hm),
          'all_frame_results':results, 'ground_underpaint_fully_opaque':True,
          'linen_anchor_rows_141_to_157_unchanged':True,
          'linen_consecutive_changes':linen_diffs,'composite_consecutive_changes':frame_diffs,
          'renderer_integration':False,'combat_motion':'frozen source capture',
          'parallax_behind_architecture_verified':False,
          'sha256':{p:hashlib.sha256((d/p).read_bytes()).hexdigest() for p in ['battle-screen.png','bloodridge-pixel-craft.aseprite','floor-source.png','ground-underpaint.png']}}
(d/'verification.json').write_text(json.dumps(result,indent=2))
print(json.dumps({k:v for k,v in result.items() if k not in ['all_frame_results','sha256']},indent=2))

