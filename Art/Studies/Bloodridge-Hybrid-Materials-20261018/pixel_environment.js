// Environment artwork is flat-lit source pixels on depth-separated cutouts.
// The existing surface, paths and follow camera still provide the topology.
const gateY=height(0,-15)
const roadSamples=[...routes.hill.getPoints(100),...routes.low.getPoints(100)]
function roadDistance(x,z){let best=1e6;for(const p of roadSamples)best=Math.min(best,(p.x-x)**2+(p.z-z)**2);return Math.sqrt(best)}
const scenery=[]
function cutout(key,x,z,w,h,{flip=false,y=height(x,z)-.12,yaw=0,face=false,tint='#ffffff'}={}){
 const tex=layerTex[key],geo=new THREE.PlaneGeometry(w,h,key==='wing'?48:1,key==='wing'?48:1);geo.translate(0,h/2,0)
 if(key==='wing'){
  // The source includes an oblique stair footprint. Set its opaque bottom edge
  // onto the terrain instead of suspending that painted floor in a vertical card.
  const c=document.createElement('canvas');c.width=tex.image.width;c.height=tex.image.height
  const ctx=c.getContext('2d');ctx.drawImage(tex.image,0,0);const rgba=ctx.getImageData(0,0,c.width,c.height).data,verts=geo.attributes.position
  for(let i=0;i<verts.count;i++){const lx=verts.getX(i),oldY=verts.getY(i),ix=Math.max(0,Math.min(c.width-1,Math.round((lx/w+.5)*(c.width-1))));let bottom=c.height-1;while(bottom>0&&rgba[(bottom*c.width+ix)*4+3]<128)bottom--;const lift=(1-bottom/(c.height-1))*h,blend=1-Math.max(0,Math.min(1,(oldY-lift)/(h*.55)));verts.setY(i,oldY-lift*blend+(height(x+Math.cos(yaw)*lx,z-Math.sin(yaw)*lx)-height(x,z))*blend)}
  geo.computeVertexNormals()
 }
 const mat=new THREE.MeshBasicMaterial({map:tex,alphaTest:.45,side:THREE.DoubleSide,color:tint,toneMapped:false})
 const m=new THREE.Mesh(geo,mat);m.position.set(x,y,z);m.rotation.y=yaw
 if(flip)m.scale.x=-1
 scene.add(m);if(face)scenery.push(m);return m
}
// Open threshold: two unequal original-court wings, with a clear central passage.
cutout('wing',-4.3,-16.1,6.8,7.2,{yaw:.12})
cutout('marker',3.35,-15.7,2.9,7.2,{flip:true,yaw:-.14})
cutout('wall',-9,-17.3,8,3.55,{yaw:.18})
cutout('wall',8.1,-17.1,8.6,3.8,{flip:true,yaw:-.2})
// See the same destination through the passage before the unchanged full-screen reveal.
const arrivalImage=court.querySelector('img');await arrivalImage.decode()
const arrivalTex=new THREE.Texture(arrivalImage);arrivalTex.needsUpdate=true;arrivalTex.colorSpace=THREE.SRGBColorSpace;arrivalTex.magFilter=arrivalTex.minFilter=THREE.NearestFilter;layerTex.arrival=arrivalTex
const arrival=new THREE.Mesh(new THREE.PlaneGeometry(6.8,4.3),new THREE.MeshBasicMaterial({map:arrivalTex,toneMapped:false}))
arrival.position.set(0,height(0,-19)+2.05,-19);scene.add(arrival)
cutout('rim',-5.1,-12.6,5.6,2.9,{yaw:.35})
cutout('rim',5.8,-12.9,5.8,3,{flip:true,yaw:-.3})
// Broken burial walls appear only as the road approaches the court.
cutout('wall',-9.2,-7.5,6.2,2.7,{yaw:.55})
cutout('wall',10.5,-8.8,5.7,2.5,{yaw:-.4,flip:true})
cutout('marker',-10.6,-10.6,1.45,3.6,{yaw:.2})
// Root-bound outcrops follow the hill's flank, not a repeated block retaining wall.
for(const [x,z,w,h,flip] of [[-9.8,3.5,5.5,2.5,false],[-10.5,-1.5,6.5,3,false],[-10.2,-5,5.2,2.65,false],[2.5,-5.8,4.5,2.3,true],[11.9,-1.5,5.1,2.3,true],[12.4,4.6,3.7,1.65,true]]){
 cutout('rim',x,z,w,h,{flip,face:true})
}
// Low, irregular source rubble. Sparse at the start, denser toward the burial ground.
for(let i=0;i<100;i++){
 const x=(rnd()-.5)*27,z=(rnd()-.5)*35,d=roadDistance(x,z)
 if(d<1.8||Math.abs(x)>14*Math.sqrt(Math.max(.15,1-(z/24)**2))||z< -12&&Math.abs(x)<5)continue
 if(z>5&&i%4!==0)continue
 const w=.7+rnd()*1.7
 cutout('rubble',x,z,w,w*.39,{flip:rnd()>.5,face:true,y:height(x,z)-.035})
}
// Uneven silhouette blends the terrain into scattered root and stone edges.
for(const [x,z,w,h,flip] of [[-11,10,4.8,1.8,false],[-14,5,5.2,2.2,false],[-14.5,-4,5.2,2.3,false],[14.6,-5,5.2,2.2,true],[11.7,11,4.2,1.65,true]])cutout('rim',x,z,w,h,{flip,face:true})
