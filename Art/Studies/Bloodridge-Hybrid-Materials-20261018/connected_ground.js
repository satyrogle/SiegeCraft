// Continuous valley, including the terrain outside the two playable approaches.
// Road and camera clearance use the existing preview curves, without changing them.
const routeFootprints=[],cameraFootprints=[]
for(const route of Object.values(routes))for(let i=0;i<=150;i++){
 const p=route.getPointAt(i/150),q=route.getPointAt(Math.min(1,i/150+.02)),d=q.clone().sub(p).setY(0).normalize()
 routeFootprints.push(p);cameraFootprints.push(p.clone().addScaledVector(d,-5.7))
}
function clearance(x,z,points){let d=1e6;for(const p of points)d=Math.min(d,(x-p.x)**2+(z-p.z)**2);return Math.sqrt(d)}
function regionHeight(x,z){
 const base=height(x,z),left=5.6*Math.exp(-((x+17+Math.sin(z*.19))**2/30)),right=6.8*Math.exp(-((x-18-Math.sin(z*.14)*2)**2/35))
 const back=(9+2*Math.sin(x*.27)+Math.cos(x*.81))*Math.exp(-((z+28+Math.sin(x*.17)*2)**2/33))
 const far=(7+2*Math.sin(z*.15+x*.13))*Math.exp(-((Math.abs(x)-34)**2/70))
 const margin=ease(clamp((clearance(x,z,routeFootprints)-3.1)/3))*ease(clamp((clearance(x,z,cameraFootprints)-2)/2))
 return base+((left+right)*ease(clamp((10-z)/17))+back+far)*margin
}
soil.alphaMap=null;soil.alphaTest=0;soil.map.repeat.set(13,15)
const groundGeo=new THREE.PlaneGeometry(108,120,180,200);groundGeo.rotateX(-Math.PI/2);groundGeo.translate(0,0,-20)
const pos=groundGeo.attributes.position,groundColors=[],rockBlend=[]
for(let i=0;i<pos.count;i++){
 const x=pos.getX(i),z=pos.getZ(i),y=regionHeight(x,z);pos.setY(i,y)
 const dx=regionHeight(x-.3,z)-regionHeight(x+.3,z),dz=regionHeight(x,z-.3)-regionHeight(x,z+.3),rise=y-height(x,z)
 const shade=Math.round(clamp(.89+dx*.13-dz*.07,.58,1.04)*12)/12
 const c=new THREE.Color('#ffffff').multiplyScalar(shade);groundColors.push(c.r,c.g,c.b)
 rockBlend.push(clamp(rise*.34+Math.abs(dx)*.45+Math.abs(dz)*.2-.14))
}
groundGeo.setAttribute('color',new THREE.Float32BufferAttribute(groundColors,3));groundGeo.setAttribute('rockBlend',new THREE.Float32BufferAttribute(rockBlend,1));groundGeo.computeVertexNormals()
// Use actual stone/root pixels as the continuous bank material, beneath the detail.
const bankCanvas=document.createElement('canvas');bankCanvas.width=bankCanvas.height=128
const bankCtx=bankCanvas.getContext('2d');bankCtx.imageSmoothingEnabled=false;bankCtx.drawImage(slabTex.image,0,0,512,256,0,0,128,128)
const bankTex=new THREE.CanvasTexture(bankCanvas);bankTex.colorSpace=THREE.SRGBColorSpace;bankTex.magFilter=bankTex.minFilter=THREE.NearestFilter;bankTex.wrapS=bankTex.wrapT=THREE.MirroredRepeatWrapping;layerTex.bank=bankTex
soil.onBeforeCompile=shader=>{
 shader.uniforms.bankMap={value:bankTex}
 shader.vertexShader='attribute float rockBlend; varying float vRockBlend; varying vec3 vLand; varying vec3 vLandN;\n'+shader.vertexShader
 shader.vertexShader=shader.vertexShader.replace('#include <begin_vertex>','#include <begin_vertex>\nvRockBlend=rockBlend;vLand=position;vLandN=normal;')
 shader.fragmentShader='uniform sampler2D bankMap; varying float vRockBlend; varying vec3 vLand; varying vec3 vLandN;\n'+shader.fragmentShader
 shader.fragmentShader=shader.fragmentShader.replace('#include <map_fragment>','#include <map_fragment>\nvec3 w=pow(abs(normalize(vLandN)),vec3(4.0));w/=w.x+w.y+w.z;vec3 bank=(texture2D(bankMap,vLand.zy*.32).rgb*w.x+texture2D(bankMap,vLand.xz*.32).rgb*w.y+texture2D(bankMap,vLand.xy*.32).rgb*w.z)*vec3(.72,.72,.78);diffuseColor.rgb=mix(diffuseColor.rgb,bank,clamp(vRockBlend,0.0,1.0));')
}
const ground=new THREE.Mesh(groundGeo,soil);scene.add(ground)
