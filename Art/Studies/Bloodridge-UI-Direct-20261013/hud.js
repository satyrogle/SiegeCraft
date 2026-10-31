const canvas=document.querySelector('#screen'),g=canvas.getContext('2d'),images={},glyphs={};
const C={ink:'#17151b',black:'#0e0d12',edge:'#635344',bone:'#e3d4b4',dim:'#a3907a',gold:'#c5a369',ally:'#9db3a0',enemy:'#c46b55',panel:'#272128'};
let capTop=100,capBottom=0,space=3,state='normal',selected=-1,target=false;
const alphabet="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,!?:;'\"-+()0123456789/%*=_$";
function rect(x,y,w,h,c){g.fillStyle=c;g.fillRect(x,y,w,h)}
function measure(s){return [...s.toUpperCase()].reduce((w,ch)=>w+(ch===' '?space:(glyphs[ch]?.width||0)+1)*2,0)-2}
function text(s,x,y,c=C.bone,align='left'){s=s.toUpperCase();if(align==='center')x-=Math.floor(measure(s)/4)*2;if(align==='right')x-=measure(s);g.fillStyle=c;for(const ch of s){if(ch===' '){x+=space*2;continue}const b=glyphs[ch];if(!b)continue;for(const p of b.ink)if(p[1]>=capTop&&p[1]<=capBottom)g.fillRect(x+p[0]*2,y+(p[1]-capTop)*2,2,2);x+=(b.width+1)*2}}
function stroke(points,c,w=2){g.strokeStyle=c;g.lineWidth=w;g.beginPath();points.forEach(([x,y],i)=>i?g.lineTo(x,y):g.moveTo(x,y));g.stroke()}
function frame(x,y,w,h,c=C.edge){rect(x+6,y,w-12,2,c);rect(x,y+6,2,h-12,c);rect(x+w-2,y+6,2,h-12,c);rect(x+6,y+h-2,w-12,2,c);rect(x+2,y+2,4,4,c);rect(x+w-6,y+2,4,4,c);rect(x+2,y+h-6,4,4,c);rect(x+w-6,y+h-6,4,4,c)}
// Authored 16x16 pixel masks. 1=ivory edge, 2=material shadow, 3=semantic accent.
const masks=[
['0000000000000000','0011111111111100','0012222222222100','0012111111122100','0012133333122100','0012133333122100','0012133133122100','0012133133122100','0012133133122100','0001213131221000','0001213131221000','0000123132210000','0000012312100000','0000001211000000','0000000110000000','0000000000000000'],
['0000000000000000','0000000000110000','0000011001221000','0000122101221000','0000122101221000','0000122100110000','0000011000000000','0000000000110000','0000011000000000','0000000000000000','0000000000000000','0000333000333000','0003300003300000','0033333303333330','0003300003300000','0000333000333000'],
['0000000000000000','0011110000111100','0010000000000100','0010000300000100','0000000300000000','0000001110000000','0000012221000000','0003122322130000','0000012221000000','0000001110000000','0000000300000000','0010000300000100','0010000000000100','0011110000111100','0000000000000000','0000000000000000'],
['0000000000000000','0000003000000000','0011003300001100','0122100330012210','0122210030122210','0012221001222100','0001221001221000','0000110000110000','0000000330000000','0000003303000000','0001130003110000','0012210001221000','0012210001221000','0001100000110000','0000000000000000','0000000000000000'],
['0000000000000000','0010000000000100','0011000000001100','0001100000011000','0000110110110000','0000011111100000','0000001221000000','0000001221000000','0000001111000000','0000001221000000','0000001221000000','0000001221000000','0000011111100000','0000012222100000','0000011111100000','0000000000000000']];
function icon(n,x,y,accent,disabled=false){const colors={'1':disabled?C.edge:C.bone,'2':disabled?'#332d30':'#73614f','3':disabled?C.edge:accent};masks[n].forEach((row,j)=>[...row].forEach((v,i)=>{if(v!=='0')rect(x+8+i*2,y+8+j*2,2,2,colors[v])}))}
function pip(x,y,filled,c=C.gold){rect(x+2,y,8,2,c);rect(x,y+2,12,8,c);rect(x+2,y+10,8,2,c);rect(x+2,y+2,8,8,filled?c:C.black);if(filled)rect(x+2,y+2,6,2,C.bone)}
function meter(x,y,w,label,c){text(label,x,y);text('100%',x+w,y,c,'right');rect(x,y+24,w,10,C.black);for(let n=0;n<20;n++){const z=Math.floor((w/20)/2)*2;rect(x+n*w/20,y+24,z-2,8,c)}rect(x,y+34,w,2,C.edge)}
function key(s,x,y){frame(x,y,24,24);text(s,x+12,y+6,C.dim,'center')}
function small(s,x,y,w){frame(x,y,w,28);text(s,x+w/2,y+8,C.dim,'center')}
function header(){rect(0,0,1440,80,C.ink);rect(0,78,1440,2,C.edge);meter(30,14,370,'YOUR NERVE',C.ally);meter(1040,14,370,'BLOODRIDGE',C.enemy);text('BLOODRIDGE',720,10,C.bone,'center');text('THE BURIAL COURT',720,34,C.dim,'center');text('0.0 / 120.0S',720,60,C.dim,'center');text('GOAL / NO LOSSES',30,60,C.dim);text('THEIR ORDERS 5',1410,60,C.dim,'right');small('PAUSE',436,12,84);small('1X',530,12,42);small('TACTICS',880,12,112)}
const labels=['HOLD','FALL BACK','FOCUS FIRE','UNLEASH','CHIEF ORDER'],costs=[1,1,1,2,0],effects=['Front takes less damage. Drains your nerve.','Avoid melee and casts for a short time.','Your fighters attack one chosen enemy.','More damage. Drains your nerve.','Recruit a chief to gain their order.'];
function footer(){
 rect(0,768,1440,132,C.ink);rect(0,768,1440,2,C.edge);rect(0,802,1440,2,C.edge);
 let hint='FIGHTERS ACT ON THEIR OWN. CHOOSE WHEN TO INTERVENE.',col=C.dim;
 if(state==='focus'){hint=target?'TARGET CHOSEN / ILLUSTRATIVE PREVIEW':'CHOOSE AN ENEMY / ESC TO CANCEL';col=C.gold}
 else if(state==='threat'){hint='HOLD ACTIVE / FRONT PROTECTED / NERVE DRAINING';col=C.ally}
 else if(state==='blocked'){hint='CHIEF ORDER UNAVAILABLE / NO CHIEF RECRUITED';col=C.dim}
 else if(selected>=0)hint=effects[selected];
 text(hint,720,780,col,'center');
 text('COMMAND',34,824,C.dim);text('POINTS',34,844,C.dim);for(let i=0;i<5;i++)pip(34+i*24,870,true);rect(192,818,2,64,C.edge);
 for(let i=0;i<5;i++){
 const x=220+i*240,active=(state==='focus'&&i===2)||(state==='threat'&&i===0),hover=selected===i,disabled=i===4;
 if(active||hover){rect(x-10,812,224,80,C.panel);rect(x-10,812,224,2,active?(i===2?C.gold:C.ally):C.edge)}
 icon(i,x,826,i===0?C.ally:i===2?C.enemy:C.gold,disabled);key(String(i+1),x+180,804);
 text(labels[i],x+60,830,disabled?C.dim:C.bone);
 if(disabled)text('NONE YET',x+60,858,C.edge);
 else if(active&&state==='threat'){text('ACTIVE 2.4S',x+60,858,C.ally);rect(x+60,882,128,4,C.black);rect(x+60,882,92,4,C.ally)}
 else if(active&&state==='focus')text(target?'TARGET SET':'SELECT TARGET',x+60,858,C.gold);
 else{for(let p=0;p<costs[i];p++)pip(x+60+p*18,858,true);text(costs[i]+' PT',x+106,858,C.dim)}
 if(i<4)rect(x+222,830,2,42,'#302930');
 }
}
function brackets(x,y,w,h){const a=16;for(const [px,py,sx,sy] of [[x,y,1,1],[x+w,y,-1,1],[x,y+h,1,-1],[x+w,y+h,-1,-1]]){rect(px+(sx<0?-a:0),py,a,4,C.gold);rect(px,py+(sy<0?-a:0),4,a,C.gold)}}
function overlays(){if(state==='focus'){brackets(774,298,128,142);rect(756,262,166,24,C.ink);text(target?'TARGET CHOSEN':'SELECT ENEMY',838,268,C.gold,'center')}
 if(state==='threat'){rect(1044,106,360,60,C.ink);rect(1044,106,4,60,C.enemy);text('CHIEF CASTING',1060,118,C.enemy);text('1.8S',1388,118,C.bone,'right');rect(1060,148,328,4,C.edge);rect(1060,148,210,4,C.enemy)}
}
function draw(){g.imageSmoothingEnabled=false;g.drawImage(images.fight,0,0);header();footer();overlays();document.querySelectorAll('[data-state]').forEach(b=>b.setAttribute('aria-pressed',b.dataset.state===state));document.querySelector('#download').href=canvas.toDataURL('image/png');document.querySelector('#download').download='ks-direct-'+state+'.png';document.querySelector('#status').textContent=state==='normal'?'Battlefield pixels preserved. Click an order or use the state tabs.':'Illustrative state. Esc returns to normal. No simulation is running.'}
function setState(s){state=s;selected=-1;target=false;draw()}
canvas.addEventListener('click',e=>{const b=canvas.getBoundingClientRect(),x=(e.clientX-b.left)*1440/b.width,y=(e.clientY-b.top)*900/b.height;if(y>806&&x>210){selected=Math.min(4,Math.floor((x-210)/240));state=selected===2?'focus':selected===4?'blocked':'normal';target=false;draw()}else if(state==='focus'&&x>774&&x<906&&y>298&&y<444){target=true;draw()}});
document.addEventListener('keydown',e=>{if(e.key==='Escape')setState('normal');if(e.key==='3')setState('focus')});
document.querySelectorAll('[data-state]').forEach(b=>b.onclick=()=>setState(b.dataset.state));document.querySelector('#native').onclick=()=>document.body.classList.toggle('native');
Promise.all(['fight','font'].map(name=>new Promise((res,rej)=>{const im=new Image;im.onload=()=>{images[name]=im;res()};im.onerror=rej;im.src='assets/'+name+'.png'}))).then(()=>{
 const f=images.font,fc=document.createElement('canvas');fc.width=f.width;fc.height=f.height;const fg=fc.getContext('2d');fg.drawImage(f,0,0);const d=fg.getImageData(0,0,f.width,f.height).data,cell=f.width/8;space=Math.max(3,Math.floor(cell/3));
 for(let i=0;i<alphabet.length;i++){const cx=i%8*cell,cy=Math.floor(i/8)*cell;let left=cell,right=-1,pts=[];for(let y=0;y<cell;y++)for(let x=0;x<cell;x++)if(d[((cy+y)*f.width+cx+x)*4+3]>127){left=Math.min(left,x);right=Math.max(right,x);pts.push([x,y]);if(i<26){capTop=Math.min(capTop,y);capBottom=Math.max(capBottom,y)}}if(right>=left)glyphs[alphabet[i]]={width:right-left+1,ink:pts.map(([x,y])=>[x-left,y])}}
 draw();const ref=document.createElement('canvas');ref.width=1440;ref.height=900;const rg=ref.getContext('2d');rg.drawImage(images.fight,0,0);const a=g.getImageData(0,80,1440,688).data,b=rg.getImageData(0,80,1440,688).data;let changes=0;for(let n=0;n<a.length;n++)if(a[n]!==b[n])changes++;canvas.dataset.protectedChannelChanges=String(changes);
}).catch(e=>document.querySelector('#status').textContent='Load error: '+e);
