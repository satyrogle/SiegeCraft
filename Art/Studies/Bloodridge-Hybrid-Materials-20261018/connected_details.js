// Continuous architecture sunk into the same rock banks; no camera-facing scenery.
function terrace(points,h){
 const curve=new THREE.CatmullRomCurve3(points.map(([x,z])=>new THREE.Vector3(x,0,z))),verts=[],uv=[],indices=[],steps=120,rows=8,length=curve.getLength()
 for(let i=0;i<=steps;i++){
  const p=curve.getPointAt(i/steps),base=regionHeight(p.x,p.z)-.35,fade=ease(clamp(i/12))*ease(clamp((steps-i)/12)),rise=h*fade*(.91+.09*Math.sin(i*.41))
  for(let j=0;j<=rows;j++){verts.push(p.x,base+rise*j/rows,p.z);uv.push(i/steps*length/6,j/rows);if(i<steps&&j<rows){const a=i*(rows+1)+j,b=a+rows+1;indices.push(a,b,a+1,a+1,b,b+1)}}
 }
 const geo=new THREE.BufferGeometry();geo.setAttribute('position',new THREE.Float32BufferAttribute(verts,3));geo.setAttribute('uv',new THREE.Float32BufferAttribute(uv,2));geo.setIndex(indices);geo.computeVertexNormals()
 const mat=new THREE.MeshBasicMaterial({map:layerTex.wall,alphaTest:.45,side:THREE.DoubleSide,toneMapped:false,color:'#dfd0c8'});scene.add(new THREE.Mesh(geo,mat))
}
terrace([[-14,4],[-13,-2],[-11,-7],[-8,-12],[-7,-17],[-10,-22],[-18,-24]],2.5)
terrace([[15,3],[15,-4],[12,-9],[8,-14],[8,-19],[13,-23],[20,-24]],2.8)
terrace([[-23,-20],[-17,-26],[-8,-27],[1,-28],[11,-26],[23,-23]],2.2)
// Broken paving and rubble laid into the land, not upright scenery cards.
function groundPatch(tex,x,z,w,d,angle=0){
 const geo=new THREE.PlaneGeometry(w,d,12,12);geo.rotateX(-Math.PI/2);geo.rotateY(angle)
 const a=geo.attributes.position;for(let i=0;i<a.count;i++){const px=x+a.getX(i),pz=z+a.getZ(i);a.setXYZ(i,px,regionHeight(px,pz)+.04,pz)}geo.computeVertexNormals()
 const mat=new THREE.MeshBasicMaterial({map:tex,alphaTest:.4,side:THREE.DoubleSide,toneMapped:false,color:'#e9dcd0'});scene.add(new THREE.Mesh(geo,mat))
}
for(let i=0;i<95;i++){
 const x=(rnd()-.5)*39,z=13-rnd()*43,d=roadDistance(x,z)
 if(d<1.5||z>7&&i%4!==0)continue
 groundPatch(i%3===0?layerTex.rim:layerTex.rubble,x,z,1.8+rnd()*3.2,1.1+rnd()*2.4,rnd()*6.28)
}
// Embed root outcrops in the foot of the banks. Their support is continuous terrain.
for(const [x,z,w,h,flip] of [[-12,3,5,2.4,false],[-12,-3,5.8,2.8,false],[-9,-9,5,2.5,false],[12,-3,5.7,2.5,true],[11,-9,5.2,2.5,true],[-7,-13,5.3,2.6,false],[7,-13,5.2,2.6,true]]){
 cutout('rim',x,z,w,h,{flip,y:regionHeight(x,z)-.5,yaw:flip?-.55:.55})
}
