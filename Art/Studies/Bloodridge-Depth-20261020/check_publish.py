"""Read-only pixel verification, then copy reviewed native exports to the local server.
No image editing occurs in this helper. Aseprite authors all masters and exports.
"""
from pathlib import Path
import hashlib, json, shutil
from PIL import Image
import numpy as np

ROOT=Path(__file__).resolve().parent
PRIOR=ROOT.parent/'Bloodridge-Root-Crypt-20261019'
DEST=Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/bloodridge-depth')

def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def rgba(path):return np.array(Image.open(path).convert('RGBA'))

def main():
    config=json.loads((ROOT/'layers.json').read_text())
    original=rgba(PRIOR/'panorama.png');final=rgba(ROOT/'panorama.png')
    reexport=rgba(ROOT/'master-export.png')
    assert final.shape==(768,4128,4)
    assert np.array_equal(final,reexport),'Saved master did not reopen exactly'
    assert np.array_equal(final[:,3008:],original[:,3008:]),'Protected court core changed'
    assert np.array_equal(final[:,2752:],original[:,2752:]),'The existing full court view changed'
    assert np.array_equal(final[:,:][...,3],np.full((768,4128),255)),'Composite has holes'
    planes=[rgba(ROOT/p['file']) for p in config['planes']]
    assert all(p.shape==final.shape for p in planes)
    assert all(set(np.unique(p[...,3])).issubset({0,255}) for p in planes),'Plane alpha must stay on the native grid'
    comp=planes[0].copy()
    for plane in planes[1:]:
        opaque=plane[...,3]==255;comp[opaque]=plane[opaque]
    assert np.array_equal(comp,final),'Plane order differs from native master'
    palette=set(map(tuple,original[:,:,:3].reshape(-1,3)))
    actual=set(map(tuple,final[:,:,:3].reshape(-1,3)))
    assert actual.issubset(palette),'A new background color escaped the source palette'
    allowed=np.zeros((768,4128),dtype=bool)
    allowed[256:768,:1152]=rgba(ROOT/'wall-removal-mask.png')[...,3]>0
    allowed|=rgba(ROOT/'near-left.png')[...,3]>0
    assert np.array_equal(final[~allowed],original[~allowed]),'An unrelated original region pixel changed'
    near=rgba(ROOT/'near-left.png')
    # Solid wall pixels stay below the lowest normal formation feet in view zero.
    assert not np.any(near[340:635,530:650,3]),'Relocated wall still crosses front-lane feet'
    detail={
      'dimensions':[4128,768],'native_scale':1,'resampled':False,
      'background_palette_entries':len(actual),'source_palette_entries':len(palette),
      'reopened_aseprite_export_exact':True,'plane_composite_exact':True,
      'original_court_core_x3008_to4127_exact':True,
      'existing_full_court_view_x2752_to4127_exact':True,
      'original_court_source_sha256':sha(PRIOR/'original-court.png'),
      'prior_panorama_sha256':sha(PRIOR/'panorama.png'),
      'full_composite_opaque':True,'native_binary_plane_alpha':True,
      'near_wall_clears_first_view_front_lane_feet':True,
      'new_animation_authored':False,'existing_ash_reused_at_8fps':True,
      'unity_tested':False,'moving_fight_tested':False,'art_approved':False,
      'first_underpaint_candidate_rejected':True,
      'all_generated_and_polygon_floor_drafts_excluded':True,
      'selected_floor_source':'../Bloodridge-Region-Composition-20261019/terrain-revision/ground-clearance/final-composition.png at identical world coordinates',
      'pixels_changed':int(np.count_nonzero(np.any(final!=original,axis=2))),
      'all_pixels_outside_wall_reconstruction_and_relocation_unchanged':True,
      'plane_opaque_pixel_counts':{p['id']:int(np.count_nonzero(im[...,3])) for p,im in zip(config['planes'],planes)},
      'limits':['Study only; no encounter mapping or Unity binding','Six figures are static scale references, not a gameplay capture','Claude environmental clips pending; existing ash only','Other stage/formation clearances require moving-fight review']
    }
    (ROOT/'verification.json').write_text(json.dumps(detail,indent=2))
    DEST.mkdir(exist_ok=True)
    files=['index.html','review.js','layers.json','verification.json','panorama.png','far.png','rear.png','ground.png','court-foreground.png','near-left.png','near-scrub.png','near-mask.png']
    files += [p.name for p in ROOT.glob('*-reference.png') if p.name!='underpaint-reference.png']
    for file in files:shutil.copyfile(ROOT/file,DEST/file)
    shutil.copyfile(PRIOR/'panorama.png',DEST/'previous.png')
    for file in ['fighter.png','champion.png','ash-1.png','ash-2.png']:
        shutil.copyfile(DEST.parent/'bloodridge-root-crypt'/file,DEST/file)
    print(json.dumps(detail,indent=2))
    print('http://127.0.0.1:8782/bloodridge-depth/index.html')

if __name__=='__main__':main()
