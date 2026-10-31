from pathlib import Path
from PIL import Image
import base64, io, re

root=Path(__file__).resolve().parent
repo=root.parents[2]
target=Path('C:/Users/jacob/.codex/visualizations/2026/10/17/01a14b30-98f3-7b50-ac0b-86e613e812fb/bloodridge-ascent.html')
html=target.read_text(encoding='utf-8')
for token,path in [('MAP_ASSET',root.parent/'Bloodridge-Journey-Ascent-20261018b/region-extracted.png'),('COURT_ASSET',repo/'Art/Studies/Bloodridge-Court-Refinement-20261013/source-clean.png')]:
    image=Image.open(path)
    buffer=io.BytesIO()
    image.save(buffer,format='WEBP',quality=85,method=6)
    data='data:image/webp;base64,'+base64.b64encode(buffer.getvalue()).decode()
    if token=='MAP_ASSET':
        html=re.sub(r'(<img class="br-terrain" src=")[^"]+',lambda m:m.group(1)+data,html)
    else:
        html=html.replace(token,data)
target.write_text(html,encoding='utf-8')
assert len(html.encode())<1000000
assert 'MAP_ASSET' not in html and 'COURT_ASSET' not in html
(root/'preview-script.js').write_text(re.search(r'<script>([\s\S]*?)</script>',html).group(1),encoding='utf-8')
print('Preview embedded:',len(html.encode()),'bytes. Original image dimensions retained.')
