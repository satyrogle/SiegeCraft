// Frontend camera / existing-clip binding only. New environmental clips belong to Claude.
const $ = id => document.getElementById(id);
const canvas = $('scene'), ctx = canvas.getContext('2d', {alpha:false});
ctx.imageSmoothingEnabled = false;
const stops = [{x:0,name:'Ash wash'},{x:400,name:'Root hollow'},{x:1400,name:'Sunken crypt'},{x:2200,name:'Court approach'},{x:2752,name:'Original court'}];
const reduced = matchMedia('(prefers-reduced-motion: reduce)').matches;
if (reduced) { $('ash').checked=false; $('depth').checked=false; }
const load = file => new Promise((resolve,reject) => {
  const img=new Image(); img.onload=()=>resolve(img); img.onerror=()=>reject(new Error(`Could not load ${file}`)); img.src=file;
});
let config,images,clock=0,x=0,lag=0,tween=null,journey=false,dwell=0,last=0,lastFrame=-1;
let renders=0,lastReadout='';
function nearestStop() { return stops.reduce((a,b)=>Math.abs(a.x-x)<Math.abs(b.x-x)?a:b); }
function updateControls() {
  const px=Math.round(x), stop=nearestStop();
  $('position').value=String(px); $('position-readout').value=`${px} / 2752 px`;
  $('location').value=Math.abs(stop.x-px)<2?String(stop.x):'';
  $('view-window').style.left=`${px/4128*100}%`;
  $('view-window').style.width=`${1376/4128*100}%`;
  $('place-name').textContent=tween?`${tween.fromName} → ${tween.toName}`:stop.name;
  const text=$('paused').checked?'Paused — camera and ash held':tween?'Travelling — native pixel steps':
    $('original').checked?'Previous flattened composition':$('occlusion').checked?'Near scenery mask — inspect figure clearance':'Resting — fixed battle framing';
  if(text!==lastReadout){$('status').textContent=text;lastReadout=text;}
  $('journey').textContent=journey?'Stop travel':'Travel the region';
}
function moveTo(target,automated=false) {
  target=Math.max(0,Math.min(2752,target));
  if(!automated){journey=false;dwell=0;}
  if(reduced||$('paused').checked){x=target;lag=0;tween=null;lastFrame=-1;updateControls();return;}
  const to=stops.find(p=>p.x===target)||{name:'Across Bloodridge'};
  tween={from:x,to:target,at:clock,duration:Math.max(1.6,Math.min(4.3,Math.abs(target-x)/290)),fromName:nearestStop().name,toName:to.name};
  updateControls();
}
function nextStop(direction) {
  return direction>0?(stops.find(p=>p.x>x+2)||stops.at(-1)):
    ([...stops].reverse().find(p=>p.x<x-2)||stops[0]);
}
function travel() {
  if(journey){journey=false;tween=null;updateControls();return;}
  $('paused').checked=false;
  if(x>2){x=0;tween=null;lag=0;}
  journey=true;dwell=0;moveTo(nextStop(1).x,true);
}
function paintAsh(px) {
  if(!$('ash').checked)return;
  const frame=Math.floor(clock*config.existingAsh.fps)%32;
  for(const emitter of config.emitters){
    const [ex,ey,ew,eh]=emitter.rect,[cx,cy,cw,ch]=emitter.clip;
    if(cx-px>1376||cx+cw-px<0)continue;
    const f=(frame+emitter.phaseFrames)%32,local=f<24?f:f-24;
    const im=images[f<24?'ash-a':'ash-b'];
    ctx.save();ctx.beginPath();ctx.rect(cx-px,cy,cw,ch);ctx.clip();ctx.globalAlpha=emitter.opacity;
    ctx.drawImage(im,(local%6)*1024,Math.floor(local/6)*600,1024,600,ex-px,ey,ew,eh);
    ctx.restore();
  }
}
function paintFigures(){
  if(!$('figures').checked)return;
  // Three references per side, placed on the actual normal-fight lane X values
  // and top/middle/bottom ground anchors, minus stage (32,66). Not a sim fixture.
  const figures=[
    {x:388,y:494,file:'fighter',flip:true},
    {x:568,y:354,file:'fighter',flip:true},
    {x:568,y:634,file:'fighter',flip:true},
    {x:808,y:354,file:'champion',flip:false},
    {x:808,y:634,file:'fighter',flip:false},
    {x:988,y:494,file:'fighter',flip:false}
  ];
  for(const f of figures){
    const im=images[f.file],anchorX=Math.floor(im.width/2),anchorY=152;
    ctx.save();ctx.fillStyle='#18142155';
    for(let sy=-7;sy<=7;sy++)for(let sx=-32;sx<=32;sx++)if(sx*sx/(32*32)+sy*sy/(7*7)<=1)ctx.fillRect(f.x+sx,f.y-4+sy,1,1);
    ctx.translate(f.x,f.y);if(f.flip)ctx.scale(-1,1);
    ctx.drawImage(im,-anchorX,-anchorY);ctx.restore();
  }
}
function paint(){
  if(!images)return;
  const px=Math.round(x), far=Math.round(lag), front=Math.round(-lag/3);
  ctx.fillStyle='#b6a395';ctx.fillRect(0,0,1376,768);
  if($('original').checked){ctx.drawImage(images.original,-px,0);paintAsh(px);paintFigures();}
  else{
    ctx.drawImage(images.far,-px+far,0);
    ctx.drawImage(images.ground,-px,0);ctx.drawImage(images.rear,-px,0);
    paintAsh(px);paintFigures();
    ctx.drawImage(images['court-front'],-px+front,0);ctx.drawImage(images['near-left'],-px+front,0);ctx.drawImage(images['near-scrub'],-px,0);
    if($('occlusion').checked){
      ctx.save();ctx.globalAlpha=.58;ctx.drawImage(images.mask,-px+front,0);ctx.restore();
    }
  }
  renders++;canvas.dataset.frame=String(renders);canvas.dataset.worldX=String(px);
  canvas.dataset.ashFrame=String(Math.floor(clock*8)%32);canvas.dataset.cameraLag=String(far);
}
function frame(now){
  const elapsed=last?Math.min(.06,(now-last)/1000):0;last=now;
  const paused=$('paused').checked,dt=paused?0:elapsed,oldX=x;
  clock+=dt;
  if(tween){
    const t=Math.min(1,(clock-tween.at)/tween.duration),ease=t*t*(3-2*t);
    x=tween.from+(tween.to-tween.from)*ease;
    if(t===1){x=tween.to;tween=null;dwell=clock+2.2;}
  }
  if(journey&&!tween&&clock>=dwell){
    if(x>=2752){journey=false;}else moveTo(nextStop(1).x,true);
  }
  if(!paused){
    const desired=$('depth').checked&&tween?Math.max(-18,Math.min(18,(x-oldX)/Math.max(dt,.001)*.04)):0;
    lag+=(desired-lag)*Math.min(1,dt*7);if(Math.abs(lag)<.05)lag=0;
  }
  const key=[Math.round(x),Math.round(lag),Math.floor(clock*8)%32,$('ash').checked,$('figures').checked,$('original').checked,$('occlusion').checked].join('|');
  if(key!==lastFrame){paint();lastFrame=key;updateControls();}
  requestAnimationFrame(frame);
}
$('journey').onclick=travel;
$('previous').onclick=()=>moveTo(nextStop(-1).x);
$('next').onclick=()=>moveTo(nextStop(1).x);
$('location').onchange=e=>moveTo(Number(e.target.value));
$('position').oninput=e=>{journey=false;tween=null;x=Number(e.target.value);lag=0;lastFrame=-1;updateControls();};
for(const id of ['depth','ash','figures','occlusion','original','paused'])$(id).onchange=()=>{if(id==='depth'&&!$('depth').checked)lag=0;lastFrame=-1;updateControls();};
let dragging=false;
function scrub(e){const bounds=$('overview').getBoundingClientRect();journey=false;tween=null;lag=0;x=Math.max(0,Math.min(2752,(e.clientX-bounds.left)/bounds.width*4128-688));lastFrame=-1;updateControls();}
$('overview').onpointerdown=e=>{dragging=true;$('overview').setPointerCapture(e.pointerId);scrub(e);};
$('overview').onpointermove=e=>{if(dragging)scrub(e);};
$('overview').onpointerup=()=>dragging=false;
$('overview').onpointercancel=()=>dragging=false;
try{
  const response=await fetch('layers.json');if(!response.ok)throw new Error('Could not load layer manifest');config=await response.json();
  const files={far:'far.png',ground:'ground.png',rear:'rear.png','court-front':'court-foreground.png','near-left':'near-left.png','near-scrub':'near-scrub.png',mask:'near-mask.png',original:'previous.png',fighter:'fighter.png',champion:'champion.png','ash-a':'ash-1.png','ash-b':'ash-2.png'};
  images=Object.fromEntries(await Promise.all(Object.entries(files).map(async([id,file])=>[id,await load(file)])));
  const requested=Number(new URLSearchParams(location.search).get('x')||0);x=Math.max(0,Math.min(2752,requested));
  updateControls();requestAnimationFrame(frame);
}catch(error){$('error').textContent=error.message;$('status').textContent='Review assets could not be loaded.';console.error(error);}
