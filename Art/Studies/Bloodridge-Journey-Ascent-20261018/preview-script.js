
(()=>{
const root=document.getElementById('br-ascent'),$=s=>root.querySelector(s),world=$('.br-world'),court=$('.br-court'),flag=$('.br-banner'),roads=$('.br-routes'),action=$('.br-action');
const points=[[15,55],[21,68],[20,85],[43,44],[44,59],[47,68],[68,31],[63,43],[73,50],[87,22]];
const names=['Ash marker','Dry wash','Outer stones','Burial niches','Root pass','Old shrine','Fallen colonnade','The Root Court','Bronze terrace','Chief’s gate'];
const links=[[0,3],[0,4],[1,4],[1,5],[2,4],[2,5],[3,6],[3,7],[4,6],[4,7],[5,7],[5,8],[6,9],[7,9],[8,9]];
let phase='map',busy=false;let camera={x:0,y:0,z:1};
const reduced=()=>matchMedia('(prefers-reduced-motion: reduce)').matches;
const smooth=t=>t*t*(3-2*t),mix=(a,b,t)=>a+(b-a)*t;
function threshold(t){const a=mix(48,-25,t),b=mix(52,125,t),top=mix(44,-25,t),bottom=mix(58,125,t);return `polygon(${a}% ${bottom}%,${a}% ${top+4}%,${a+2}% ${top+4}%,${a+2}% ${top}%,${b-2}% ${top}%,${b-2}% ${top+4}%,${b}% ${top+4}%,${b}% ${bottom}%)`;}
function animate(ms,fn){return new Promise(resolve=>{let start=performance.now();function frame(now){let t=reduced()?1:Math.min(1,(now-start)/ms);fn(smooth(t),t);if(t<1)requestAnimationFrame(frame);else resolve();}requestAnimationFrame(frame);});}
function cam(x,y,z){camera={x,y,z};world.style.transform=`translate(${x}%,${y}%) scale(${z})`;}
function focus(x,y,z){return {x:Math.min(0,Math.max(100-100*z,50-x*z)),y:Math.min(0,Math.max(100-100*z,50-y*z)),z};}
function marker(x,y){flag.style.left=x+'%';flag.style.top=y+'%';}
function path(a,b){const p=points[a],q=points[b];return `M${p[0]},${p[1]} C${p[0]+(q[0]-p[0])*.6},${p[1]} ${q[0]-(q[0]-p[0])*.35},${q[1]} ${q[0]},${q[1]}`;}
function save(){window.openai?.setWidgetState?.({modelContent:{proposal:'Bloodridge ascent',view:phase},privateContent:{version:2,phase}}).catch(()=>{});}
function draw(){roads.innerHTML='';for(const[a,b]of links){let p=document.createElementNS('http://www.w3.org/2000/svg','path');p.setAttribute('d',path(a,b));if(a===1&&b===4)p.classList.add('walked');if(a===4&&b===7)p.classList.add(phase==='returned'?'walked':'selected');if(phase==='returned'&&a===7&&b===9)p.classList.add('selected');roads.append(p);}
const box=$('.br-nodes');box.innerHTML='';points.forEach((p,i)=>{const b=document.createElement(i===7?'button':'div');b.className='br-dot'+([1,4].includes(i)||i===7&&phase==='returned'?' visited':'')+(i===7&&phase!=='returned'?' active':'')+(i===9?' boss':'');b.style.left=p[0]+'%';b.style.top=p[1]+'%';b.innerHTML='<span class="br-mark"><b>'+([1,4].includes(i)?'✓':i===7&&phase==='returned'?'⚑':i===9?'◇':'·')+'</b></span><span class="br-label">'+names[i]+'</span>';if(i===7){b.type='button';b.disabled=busy||phase!=='map';b.setAttribute('aria-label','Enter the Root Court');b.onclick=enter;}else b.setAttribute('aria-label',names[i]+([1,4].includes(i)?' completed':' future stage'));box.append(b);});
action.disabled=busy;action.textContent=busy?'TRAVELLING…':phase==='court'?'RETURN AFTER SURRENDER':phase==='returned'?'REPLAY THE JOURNEY':'ENTER THE COURT';$('.br-name').textContent=phase==='returned'?'The court has surrendered':'The Root Court';$('.br-description').textContent=phase==='court'?'Arrival preview · return to see the route change.':phase==='returned'?'Your standard remains. The chief’s gate lies ahead.':'Follow the old road beneath the roots.';$('[data-count]').textContent=phase==='returned'?'3 / 4 stages cleared':'2 / 4 stages cleared';}
async function enter(){if(busy||phase!=='map')return;busy=true;draw();root.classList.add('moving');const p=points[4],q=points[7];
let trail=document.createElementNS('http://www.w3.org/2000/svg','path');trail.setAttribute('d',path(4,7));trail.classList.add('trail');trail.setAttribute('pathLength','1');trail.style.strokeDasharray='1';trail.style.strokeDashoffset='1';roads.append(trail);
await animate(2600,(t)=>{const u=1-t,x=u*u*u*p[0]+3*u*u*t*(p[0]+(q[0]-p[0])*.6)+3*u*t*t*(q[0]-(q[0]-p[0])*.35)+t*t*t*q[0],y=u*u*u*p[1]+3*u*u*t*p[1]+3*u*t*t*q[1]+t*t*t*q[1];marker(x,y);let z=1+.65*Math.sin(t*Math.PI/2),c=focus(x,y,z);cam(c.x,c.y,c.z);trail.style.strokeDashoffset=1-t;});
const from={...camera},to=focus(q[0],q[1],3.1);await animate(1000,t=>cam(mix(from.x,to.x,t),mix(from.y,to.y,t),mix(from.z,to.z,t)));
court.style.visibility='visible';court.setAttribute('aria-hidden','false');await animate(1200,t=>{court.style.clipPath=threshold(t);court.querySelector('img').style.transform=`scale(${mix(1.35,1,t)})`;});
phase='court';busy=false;$('.br-court-title').classList.add('show');draw();save();}
async function leave(){busy=true;draw();$('.br-court-title').classList.remove('show');await animate(950,t=>{court.style.clipPath=threshold(1-t);court.querySelector('img').style.transform=`scale(${mix(1,1.35,t)})`;});court.style.visibility='hidden';court.setAttribute('aria-hidden','true');phase='returned';draw();const from={...camera};await animate(1700,t=>cam(mix(from.x,0,t),mix(from.y,0,t),mix(from.z,1,t)));root.classList.remove('moving');busy=false;draw();save();}
function reset(){phase='map';cam(0,0,1);marker(...points[4]);court.style.visibility='hidden';court.style.clipPath='circle(0% at 50% 50%)';court.setAttribute('aria-hidden','true');$('.br-court-title').classList.remove('show');root.classList.remove('moving');draw();}
action.onclick=()=>{if(busy)return;if(phase==='map')enter();else if(phase==='court')leave();else{reset();save();}};
function restore(v){if(v?.privateContent?.version!==2)return;reset();phase=v.privateContent.phase;if(!['map','court','returned'].includes(phase))phase='map';if(phase==='court'){const q=points[7],c=focus(...q,3.1);cam(c.x,c.y,c.z);marker(...q);court.style.visibility='visible';court.style.clipPath='circle(100% at 50% 50%)';court.querySelector('img').style.transform='none';court.setAttribute('aria-hidden','false');root.classList.add('moving');$('.br-court-title').classList.add('show');}else if(phase==='returned')marker(...points[7]);draw();}
reset();restore(window.openai?.widgetState);window.addEventListener('openai:set_globals',e=>{if(!busy)restore(e.detail?.globals?.widgetState);});
})();
