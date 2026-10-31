"""Copy only local review files into the existing localhost preview server."""
from pathlib import Path
import shutil
ROOT=Path(__file__).resolve().parent
DEST=Path('C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/connected-regions')
DEST.mkdir(exist_ok=True)
for name in ['index.html','overview.html','fighter.png','champion.png']:
    shutil.copyfile(ROOT/name,DEST/name)
for i in [1,2]:
    source=ROOT.parents[2]/'Assets/StreamingAssets/CampaignArt/Bloodridge'/f'ks-bloodridge-court-restrained-ash-review-{i}.png'
    shutil.copyfile(source,DEST/f'ash-{i}.png')
for region in ['bloodridge','skullpeak','hunters']:
    target=DEST/region;target.mkdir(exist_ok=True)
    for name in ['panorama.png','ground.png','rear.png','foreground.png','layers.json']:
        source=ROOT/region/name
        if source.exists():shutil.copyfile(source,target/name)
print('http://127.0.0.1:8782/connected-regions/')
