from pathlib import Path
import base64, json, re
root=Path(__file__).resolve().parent
dest=Path('C:/Users/jacob/.codex/visualizations/2026/10/17/01a14b30-98f3-7b50-ac0b-86e613e812fb/bloodridge-pixel-terrain.html')
before=(root/'bloodridge-hybrid-before.html').read_text(encoding='utf-8')
s=before
assets={name:'data:image/png;base64,'+base64.b64encode((root/file).read_bytes()).decode() for name,file in {'earth':'ash-earth.png','slabs':'cracked-slabs.png','stone':'masonry.png','bark':'bark.png','niche':'niche.png'}.items()}
a=s.index('function texture()')
b=s.index('const groundGeo=',a)
new="const textureAssets="+json.dumps(assets)+"\n"+r'''
async function pixelTexture(key,rx=1,ry=1){const img=new Image();img.src=textureAssets[key];await img.decode();const tex=new THREE.Texture(img);tex.needsUpdate=true;tex.colorSpace=THREE.SRGBColorSpace;tex.magFilter=THREE.NearestFilter;tex.minFilter=THREE.NearestMipmapNearestFilter;tex.wrapS=tex.wrapT=THREE.MirroredRepeatWrapping;tex.repeat.set(rx,ry);return tex}
const [soilTex,slabTex,stoneTex,barkTex,nicheTex]=await Promise.all([pixelTexture('earth',8,10),pixelTexture('slabs'),pixelTexture('stone'),pixelTexture('bark',2,3),pixelTexture('niche')])
const soil=new THREE.MeshStandardMaterial({map:soilTex,roughness:1,color:'#ffffff',flatShading:true}),stone=new THREE.MeshStandardMaterial({map:stoneTex,color:'#f2eaf1',roughness:1,flatShading:true}),stoneLight=new THREE.MeshStandardMaterial({map:stoneTex,color:'#ffffff',roughness:1,flatShading:true}),dark=new THREE.MeshStandardMaterial({color:'#211823',roughness:1}),bronze=new THREE.MeshStandardMaterial({color:'#78634e',roughness:1,metalness:.05}),rootMat=new THREE.MeshStandardMaterial({map:barkTex,color:'#e8dce4',roughness:1,flatShading:true})
'''
s=s[:a]+new+s[b:]
s=s.replace('antialias:true','antialias:false').replace('renderer.setPixelRatio(Math.min(devicePixelRatio,1.5))','renderer.setPixelRatio(1)')
s=s.replace('renderer.toneMappingExposure=1.25','renderer.toneMappingExposure=1.35')
s=s.replace("new THREE.HemisphereLight('#e2c9a5','#54455d',2.1)","new THREE.HemisphereLight('#d4c8ce','#766373',2.6)")
s=s.replace("new THREE.DirectionalLight('#ffe0ac',3)","new THREE.DirectionalLight('#e5d6c8',2.0)")
s=s.replace('const vertices=[],indices=[],across=10','const vertices=[],indices=[],uvs=[],across=10')
s=s.replace('vertices.push(x,height(x,z)+.1,z);if(i<240','vertices.push(x,height(x,z)+.1,z);uvs.push(j/across,i/240*18);if(i<240')
s=s.replace("geo.setIndex(indices);geo.computeVertexNormals();const m=", "geo.setIndex(indices);geo.setAttribute('uv',new THREE.Float32BufferAttribute(uvs,2));geo.computeVertexNormals();const m=")
s=s.replace("{color:'#a58e72',roughness:1,side:THREE.DoubleSide}","{map:slabTex,color:'#ffffff',roughness:1,side:THREE.DoubleSide}")
s=s.replace("color:'#f3ce8a',transparent:true,opacity:.75","color:'#d5b77c',transparent:true,opacity:.6")
s=s.replace("color:'#ad9376',transparent:true,opacity:.3","color:'#968475',transparent:true,opacity:.13")
s=s.replace('const boxGeo=new THREE.BoxGeometry(1,1,1)',r'''
const boxGeo=new THREE.BoxGeometry(1,1,1,3,2,2)
// Shared chipped block silhouette. Deterministic coordinate noise keeps face seams together.
const blockPos=boxGeo.attributes.position
for(let i=0;i<blockPos.count;i++){let x=blockPos.getX(i),y=blockPos.getY(i),z=blockPos.getZ(i),n=Math.sin(x*31.3+y*17.1+z*47.7)*.04;blockPos.setXYZ(i,x*(.96+n),y*(.97+n),z*(.96+n))}
boxGeo.computeVertexNormals()
''')
s=s.replace('const pebGeo=new THREE.DodecahedronGeometry(1,0)','const pebGeo=new THREE.DodecahedronGeometry(1,1)')
s=s.replace('pebbles=new THREE.InstancedMesh(pebGeo,stone,160)',"pebbles=new THREE.InstancedMesh(pebGeo,new THREE.MeshStandardMaterial({map:slabTex,color:'#b7abb6',roughness:1,flatShading:true}),160)")
s=s.replace("block(s*2.25,gateY+1.8,-13.99,.65,2.7,.12,bronze);",'')
s=s.replace('new THREE.Vector3(x+s*1.4,gateY+6.5,-17)','new THREE.Vector3(s*(2.1+i*.13),gateY+5.65,-15.15)')
s=s.replace('new THREE.Vector3(x-s*.3,gateY+4,-15)','new THREE.Vector3(s*(3+i*.16),gateY+3.7,-14.7)')
s=s.replace("m.scale.set(w,h,d);m.rotation.y=rot", "m.scale.set(w,h,d);m.rotation.y=rot")
needle='// A pawn is only a position/scale marker.'
detail=r'''
// The original court's funerary reliefs sit in recessed masonry beside the opening.
const nicheMat=new THREE.MeshBasicMaterial({map:nicheTex,color:'#ffffff',side:THREE.DoubleSide})
for(const side of [-1,1]){const niche=new THREE.Mesh(new THREE.PlaneGeometry(1.08,2.7),nicheMat);niche.position.set(side*2.25,gateY+2.75,-13.8);scene.add(niche)}
// Fallen fragments and low burial walls grow denser only near the destination.
for(let i=0;i<16;i++){const x=(i%2?-1:1)*(5.8+(i%4)*.55),z=-12-(i%5)*.8;block(x,height(x,z)+.2,z,.45+(i%3)*.23,.35,.5,stone,(i*.77)%3)}
for(const side of [-1,1]){const x=side*10.8,z=-8.8,y=height(x,z);block(x,y+1.5,z,2.4,3,1.1,stone);const inset=new THREE.Mesh(new THREE.PlaneGeometry(1.15,2),nicheMat);inset.position.set(x,y+1.55,z+.57);scene.add(inset);block(x,y+3.05,z,2.75,.3,1.35,stoneLight)}
// Low, irregular rubble is embedded beside the roads; travel clearances are retained.
const fragments=new THREE.InstancedMesh(boxGeo,stoneLight,150)
let debris=0
for(let i=0;i<450&&debris<150;i++){const x=(rnd()-.5)*27,z=(rnd()-.5)*35,d=roadDistance(x,z);if(d<1.1||d>3.7||z< -12&&Math.abs(x)<4)continue;const scale=.12+rnd()*.24;dummy.position.set(x,height(x,z)+.02,z);dummy.rotation.set(.05,rnd()*6,.1);dummy.scale.set(scale*2,.07+scale*.3,scale);dummy.updateMatrix();fragments.setMatrixAt(debris++,dummy.matrix)}fragments.count=debris;fragments.receiveShadow=true;scene.add(fragments)
'''
s=s.replace(needle,detail+'\n'+needle)
s=s.replace('soilTex.dispose();renderer.dispose()','[soilTex,slabTex,stoneTex,barkTex,nicheTex].forEach(t=>t.dispose());renderer.dispose()')
s=s.replace('environment blockout','court-material study')
# Guard approved behavior: no changes to camera, route control points, travel timing or transitions.
for start,end in [('const overview=','let seed='),('const rawRoutes=','function road('),('const p=new THREE.Vector3()','// perf:')]:
    old=before[before.index(start):before.index(end,before.index(start))]
    cur=s[s.index(start):s.index(end,s.index(start))]
    if start=='const overview=':
        # Lighting belongs to the visual treatment and lies in this source range.
        old=old[:old.index('scene.add(new THREE.HemisphereLight')]
        cur=cur[:cur.index('scene.add(new THREE.HemisphereLight')]
    if start=='const p=new THREE.Vector3()':
        old=old.replace('soilTex.dispose();renderer.dispose()','[soilTex,slabTex,stoneTex,barkTex,nicheTex].forEach(t=>t.dispose());renderer.dispose()')
    assert old==cur,start
assert len(s.encode())<1000000
dest.write_text(s,encoding='utf-8')
(root/'pixel-terrain-check.mjs').write_text(re.search(r'<script type="module">([\s\S]*?)</script>',s).group(1),encoding='utf-8')
print('Written',len(s.encode()),'bytes. Camera, paths, timing, entry/return state logic unchanged.')
