import hashlib, json
from pathlib import Path
from PIL import Image

root = Path(__file__).parent
project = root.parents[2]
source = project / 'Assets/Resources/ClarthStudy/blood_rot_skirmisher_sw_idle.png'
meta = source.with_suffix('.json')
expected = {
    str(source): '4a55489a84ae18391264ad345b97ec96a70022eac38a0b0f7408dfee21bd0210',
    str(meta): 'ad176a6a94541b6c7ec8b670fc05d5ffb83349cbc8ebdd87af0876ac799e7a25',
}
original = Image.open(source).convert('RGBA').crop((0, 0, 160, 168))
reference = Image.open(root / 'approved-skirmisher-native.png').convert('RGBA')
candidate = Image.open(root / 'champion-study-native.png').convert('RGBA')
enlarged = Image.open(root / 'champion-study-4x.png').convert('RGBA')
changed = [(i % 160, i // 160) for i, (a, b) in enumerate(zip(original.getdata(), candidate.getdata())) if a != b]
source_colors = set(Image.open(source).convert('RGBA').getdata())
candidate_colors = set(candidate.getdata())
checks = {
    'source_files_unchanged': all(hashlib.sha256(Path(p).read_bytes()).hexdigest() == h for p, h in expected.items()),
    'reference_exact_first_frame': reference.size == original.size and reference.tobytes() == original.tobytes(),
    'candidate_native_160x168': candidate.size == (160, 168),
    'binary_alpha': {c[3] for c in candidate_colors} <= {0, 255},
    'no_new_opaque_colors': {c for c in candidate_colors if c[3]} <= {c for c in source_colors if c[3]},
    'exact_4x_export': enlarged.size == (640, 672) and enlarged.tobytes() == candidate.resize((640, 672), Image.Resampling.NEAREST).tobytes(),
    'changes_scoped_to_cloth': all(61 <= x <= 106 and 28 <= y <= 115 for x, y in changed),
}
result = {'mechanical_checks': checks, 'mechanical_pass': all(checks.values()), 'changed_pixels': len(changed), 'changed_bounds': [min(x for x,y in changed), min(y for x,y in changed), max(x for x,y in changed), max(y for x,y in changed)], 'visual_verdict': 'FAIL: costume variation does not establish a distinct champion.', 'source_hashes': expected}
(root / 'audit.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
print(json.dumps(result))
