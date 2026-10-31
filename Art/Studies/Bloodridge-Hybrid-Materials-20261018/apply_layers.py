from pathlib import Path
import base64,json,re
root=Path(__file__).resolve().parent
vis=Path('C:/Users/jacob/.codex/visualizations/2026/10/17/01a14b30-98f3-7b50-ac0b-86e613e812fb')
before=(root/'bloodridge-hybrid-before.html').read_text(encoding='utf-8')
s=(vis/'bloodridge-pixel-terrain.html').read_text(encoding='utf-8')
study=root.parent/'Bloodridge-Court-Refinement-20261013'
files={'earth':root/'ash-earth.png','slabs':root/'cracked-slabs.png','wing':root/'rooted-wing.png','wall':root/'broken-wall.png','rubble':root/'loose-stones.png','rim':root/'root-outcrop.png','marker':root/'marker-crop.png'}
assets={k:'data:image/png;base64,'+base64.b64encode(v.read_bytes()).decode() for k,v in files.items()}
a=s.index('const textureAssets=');b=s.index('const groundGeo=',a)
material='const textureAssets='+json.dumps(assets)+'\n'+r'''
async function pixelTexture(key,rx=1,ry=1){const img=new Image();img.src=textureAssets[key];await img.decode();const tex=new THREE.Texture(img);tex.needsUpdate=true;tex.colorSpace=THREE.SRGBColorSpace;tex.magFilter=THREE.NearestFilter;tex.minFilter=THREE.NearestFilter;tex.generateMipmaps=false;tex.wrapS=tex.wrapT=THREE.MirroredRepeatWrapping;tex.repeat.set(rx,ry);return tex}
const soilTex=await pixelTexture('earth',4,5),slabTex=await pixelTexture('slabs')
const layerTex=Object.fromEntries(await Promise.all(['wing','wall','rubble','rim','marker'].map(async k=>[k,await pixelTexture(k)])))
const soil=new THREE.MeshBasicMaterial({map:soilTex,color:'#ffffff',vertexColors:true,toneMapped:false}),bronze=new THREE.MeshStandardMaterial({color:'#78634e',roughness:1,metalness:.05})
// Pixel-dissolved path margins, with no smooth transparent fringe.
const edgeCanvas=document.createElement('canvas');edgeCanvas.width=64;edgeCanvas.height=128
const edgeContext=edgeCanvas.getContext('2d');edgeContext.fillStyle='#000';edgeContext.fillRect(0,0,64,128)
for(let y=0;y<128;y++)for(let x=0;x<64;x++){const edge=Math.min(x,63-x)/16,noise=((x*73856093^y*19349663)>>>0)%101/100;edgeContext.fillStyle=edge>noise?'#fff':'#000';edgeContext.fillRect(x,y,1,1)}
const edgeTex=new THREE.CanvasTexture(edgeCanvas);edgeTex.magFilter=edgeTex.minFilter=THREE.NearestFilter;edgeTex.wrapT=THREE.RepeatWrapping
// Eroded pixel edge replaces the clean oval tabletop silhouette.
const borderCanvas=document.createElement('canvas');borderCanvas.width=192;borderCanvas.height=224
const borderCtx=borderCanvas.getContext('2d');borderCtx.fillStyle='#000';borderCtx.fillRect(0,0,192,224)
for(let y=0;y<224;y++)for(let x=0;x<192;x++){const d=Math.min(x,191-x,y,223-y),wave=3*Math.sin(x*.31)+3*Math.cos(y*.23)+2*Math.sin((x+y)*.53),noise=((x*73856093^y*19349663)>>>0)%101/100;const coverage=Math.max(0,Math.min(1,(d-4-wave)/9));if(coverage>=noise&&coverage>0){borderCtx.fillStyle='#fff';borderCtx.fillRect(x,y,1,1)}}
const borderTex=new THREE.CanvasTexture(borderCanvas);borderTex.magFilter=borderTex.minFilter=THREE.NearestFilter;soil.alphaMap=borderTex;soil.alphaTest=.5
'''
s=s[:a]+material+s[b:]
s=s.replace('const ground=new THREE.Mesh',r'''
const groundColors=[]
for(let i=0;i<pos.count;i++){const x=pos.getX(i),z=pos.getZ(i),slope=height(x-.2,z)-height(x+.2,z),step=Math.round(slope*8)/8;const c=new THREE.Color('#ffffff').multiplyScalar(.92+Math.max(-.14,Math.min(.06,step)));groundColors.push(c.r,c.g,c.b)}
groundGeo.setAttribute('color',new THREE.Float32BufferAttribute(groundColors,3))
const ground=new THREE.Mesh''')
s=re.sub(r'const sideGeo=.*?scene.add\(base\)\n','',s)
s=s.replace('w=.87','w=1.05+.8*clamp((-p.z+2)/15)').replace('uvs.push(j/across,i/240*18)','uvs.push(j/across,i/240*6)')
s=s.replace("new THREE.MeshStandardMaterial({map:slabTex,color:'#ffffff',roughness:1,side:THREE.DoubleSide})","new THREE.MeshBasicMaterial({map:slabTex,alphaMap:edgeTex,alphaTest:.5,color:'#ffffff',toneMapped:false,side:THREE.DoubleSide})")
a=s.index('const boxGeo=');b=s.index('// A pawn is only',a)
s=s[:a]+(root/'pixel_environment.js').read_text(encoding='utf-8')+'\n'+s[b:]
# Scenery yaw is a render-only adjustment; camera and transition functions are untouched.
s=s.replace('renderer.render(scene,camera)','for(const m of scenery)m.rotation.y=Math.atan2(camera.position.x-m.position.x,camera.position.z-m.position.z);\nrenderer.render(scene,camera)')
s=s.replace('[soilTex,slabTex,stoneTex,barkTex,nicheTex].forEach(t=>t.dispose())','[soilTex,slabTex,edgeTex,borderTex,...Object.values(layerTex)].forEach(t=>t.dispose())')
s=s.replace('court-material study','layered court artwork study')
for start,end in [('const overview=','scene.add(new THREE.HemisphereLight'),('const rawRoutes=','function road('),('const p=new THREE.Vector3()','// perf:')]:
 old=before[before.index(start):before.index(end,before.index(start))]
 cur=s[s.index(start):s.index(end,s.index(start))]
 if start=='const p=new THREE.Vector3()':
  cur=cur.replace('for(const m of scenery)m.rotation.y=Math.atan2(camera.position.x-m.position.x,camera.position.z-m.position.z);\n','')
  cur=cur.replace('[soilTex,slabTex,edgeTex,borderTex,...Object.values(layerTex)].forEach(t=>t.dispose())','soilTex.dispose()')
 assert old==cur,start
assert len(s.encode())<1000000,len(s.encode())
(vis/'bloodridge-layered-court.html').write_text(s,encoding='utf-8')
(root/'layered-court-check.mjs').write_text(re.search(r'<script type="module">([\s\S]*?)</script>',s).group(1),encoding='utf-8')
print('Written',len(s.encode()),'bytes; camera, route curves, timing and transition logic preserved.')
