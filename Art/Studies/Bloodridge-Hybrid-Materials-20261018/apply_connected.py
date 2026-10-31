from pathlib import Path
import re
root=Path(__file__).resolve().parent
vis=Path('C:/Users/jacob/.codex/visualizations/2026/10/17/01a14b30-98f3-7b50-ac0b-86e613e812fb')
before=(vis/'bloodridge-layered-court.html').read_text(encoding='utf-8')
s=before
a=s.index('const groundGeo=');b=s.index('const rawRoutes=',a)
s=s[:a]+s[b:]
at=s.index('function road(');s=s[:at]+(root/'connected_ground.js').read_text(encoding='utf-8')+'\n'+s[at:]
a=s.index('// Broken burial walls appear');b=s.index('// A pawn is only',a)
s=s[:a]+(root/'connected_details.js').read_text(encoding='utf-8')+'\n'+s[b:]
# The entrance's original artwork stays, but the surrounding wall fragments are replaced by terraces.
s=re.sub(r"cutout\('wall',(?:-9,-17\.3|8\.1,-17\.1).*?\n",'',s)
s=s.replace("scene.background=new THREE.Color('#29222e')","scene.background=new THREE.Color('#776966')")
s=s.replace("scene.fog=new THREE.Fog('#29222e',38,95)","scene.fog=new THREE.Fog('#776966',38,100)")
s=s.replace('layered court artwork study','connected Bloodridge region study')
for start,end in [('const overview=','scene.add(new THREE.HemisphereLight'),('const p=new THREE.Vector3()','// perf:')]:
 assert before[before.index(start):before.index(end,before.index(start))]==s[s.index(start):s.index(end,s.index(start))],start
routes=re.search(r'const rawRoutes=[\s\S]*?const routes=.*?\n',before).group(0)
assert routes in s
assert re.search(r'function height\(.*?\n',before).group(0) in s
assert len(s.encode())<1000000,len(s.encode())
(vis/'bloodridge-connected-region.html').write_text(s,encoding='utf-8')
(root/'connected-check.mjs').write_text(re.search(r'<script type="module">([\s\S]*?)</script>',s).group(1),encoding='utf-8')
print('Written',len(s.encode()),'bytes. Original camera, route curves, timings, transitions and state logic unchanged.')
