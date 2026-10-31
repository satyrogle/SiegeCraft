const names=['Command bar','Orders at the side','Corner groups'];
const files=['fight','font','hold','fallback','focus','unleash','eclipse'];
const images={};
const C={ink:'#19161b',panel:'#221e23',edge:'#75614e',bone:'#efdfbe',muted:'#ad9a86',gold:'#d6aa5c',ally:'#99beac',enemy:'#da6b51',active:'#492b28'};
const glyphs={};let capTop=100,capBottom=0,space=3,selected=0;
const alphabet="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,!?:;'\"-+()0123456789/%*=_$";
const canvases=['a','b','c'].map(id=>document.getElementById(id));
function box(g,x,y,w,h,colour=C.panel,edge=C.edge){g.fillStyle=colour;g.fillRect(x,y,w,h);if(edge){g.fillStyle=edge;g.fillRect(x,y,w,2);g.fillRect(x,y+h-2,w,2);g.fillRect(x,y,2,h);g.fillRect(x+w-2,y,2,h)}}
function text(g,value,x,y,colour=C.bone,align='left'){
 value=value.toUpperCase();const w=measure(value);if(align==='center')x-=Math.floor(w/4)*2;if(align==='right')x-=w;g.fillStyle=colour;
 for(const ch of value){if(ch===' '){x+=space*2;continue}const b=glyphs[ch];if(!b)continue;for(const p of b.ink)if(p[1]>=capTop&&p[1]<=capBottom)g.fillRect(x+p[0]*2,y+(p[1]-capTop)*2,2,2);x+=(b.width+1)*2}
}
function measure(s){return [...s.toUpperCase()].reduce((w,ch)=>w+(ch===' '?space:(glyphs[ch]?.width||0)+1)*2,0)-2}
function line(g,x,y,w,colour=C.edge){g.fillStyle=colour;g.fillRect(x,y,w,2)}
function meter(g,x,y,w,label,value,col,align='left'){
 text(g,label,x,y,C.bone);text(g,value+'%',x+w,y,col,'right');box(g,x,y+24,w,8,'#100e12',null);g.fillStyle=col;g.fillRect(x,y+24,Math.floor(w*value/100/2)*2,8);
}
function smallControl(g,label,x,y,w){box(g,x,y,w,30);text(g,label,x+w/2,y+8,C.muted,'center')}
const orders=[['HOLD','1','hold','-30% HIT'],['FALL BACK','1','fallback','NO MELEE'],['FOCUS FIRE','1','focus','ACTIVE 5.0S'],['UNLEASH','2','unleash','+50% DMG'],['CHIEF ORDER','-','eclipse','NOT RECRUITED']];
function icon(g,name,x,y){g.drawImage(images[name],x,y,48,48)}
function command(g,n,x,y,w,h,stack=false){
 const [label,cost,id,detail]=orders[n],active=n===2,locked=n===4;
 box(g,x,y,w,h,active?C.active:C.panel,active?C.enemy:C.edge);
 if(active){g.fillStyle=C.enemy;g.fillRect(x,y,4,h)}
 g.globalAlpha=locked?.45:1;
 if(stack){icon(g,id,x+10,y+10);text(g,String(n+1),x+w-12,y+12,C.muted,'right');text(g,label,x+12,y+66,active?C.enemy:C.bone);text(g,cost==='-'?'LOCKED':cost+' PT',x+w-12,y+38,C.gold,'right')}
 else{icon(g,id,x+8,y+8);text(g,(n+1)+' '+label,x+66,y+10,active?C.enemy:C.bone);text(g,detail,x+66,y+36,C.muted);text(g,cost==='-'?'--':cost,x+w-12,y+10,C.gold,'right')}
 g.globalAlpha=1;
}
function base(g){g.imageSmoothingEnabled=false;g.drawImage(images.fight,0,0);box(g,0,0,1440,80,C.ink,null);box(g,0,832,1440,68,C.ink,null)}
function controls(g,x=1152,y=44){smallControl(g,'PAUSE',x,y,96);smallControl(g,'1X',x+104,y,50);smallControl(g,'TACTICS',x+162,y,108)}
function A(g){
 base(g);meter(g,24,14,388,'YOUR NERVE',90,C.ally);meter(g,1028,14,388,'THEIR NERVE',94,C.enemy);
 text(g,'BLOODRIDGE / CHIEF',720,10,C.bone,'center');text(g,'BREAK THEIR NERVE',720,36,C.muted,'center');text(g,'6.0 / 120.0S',720,60,C.muted,'center');
 text(g,'COMMAND POINTS 3',24,60,C.gold);text(g,'THEIR ORDERS 5',1008,60,C.muted,'right');controls(g,1132,46);
 for(let i=0;i<5;i++)command(g,i,24+i*280,836,272,60);
}
function B(g){
 base(g);text(g,'BLOODRIDGE / CHIEF',24,16);text(g,'BREAK THEIR NERVE',24,48,C.muted);
 meter(g,480,14,240,'YOUR NERVE',90,C.ally);meter(g,772,14,240,'THEIR NERVE',94,C.enemy);text(g,'6.0 / 120.0S',1070,18,C.muted);controls(g,1132,46);
 box(g,20,158,160,42,C.ink);text(g,'ORDERS / 3 PT',32,172,C.gold);
 for(let i=0;i<5;i++)command(g,i,20,208+i*104,160,94,true);
 text(g,'FOCUS FIRE',30,848,C.enemy);text(g,'YOUR FIGHTERS ATTACK ONE TARGET',30,874,C.muted);
 text(g,'ACTIVE 5.0S',714,860,C.bone,'center');text(g,'THEIR ORDERS 5',1410,860,C.muted,'right');
}
function CLayout(g){
 base(g);box(g,20,8,370,64);meter(g,34,18,342,'YOUR NERVE',90,C.ally);
 box(g,1050,8,370,64);meter(g,1064,18,342,'THEIR NERVE',94,C.enemy);
 text(g,'BLOODRIDGE / CHIEF',720,10,C.bone,'center');text(g,'BREAK THEIR NERVE',720,34,C.muted,'center');text(g,'6.0 / 120.0S',720,58,C.muted,'center');
 smallControl(g,'PAUSE',404,20,94);smallControl(g,'1X',506,20,44);smallControl(g,'TACTICS',924,20,110);
 // Two compact defensive commands, three offensive/support commands. Same five inputs as A and B.
 command(g,0,20,836,252,60);command(g,1,280,836,252,60);
 box(g,584,834,200,62,C.ink,C.gold);text(g,'3 POINTS',684,844,C.gold,'center');text(g,'THEIR ORDERS 5',684,872,C.muted,'center');
 const short=['FOCUS','UNLEASH','CHIEF'];
 for(let n=2;n<5;n++){
  const x=824+(n-2)*200;box(g,x,836,192,60,n===2?C.active:C.panel,n===2?C.enemy:C.edge);g.globalAlpha=n===4?.45:1;icon(g,orders[n][2],x+8,842);text(g,(n+1)+' '+short[n-2],x+66,846,n===2?C.enemy:C.bone);text(g,n===4?'LOCKED':orders[n][1]+' PT',x+66,872,C.muted);g.globalAlpha=1;
 }
}
function choose(n){selected=n;document.body.dataset.mode='detail';const g=document.getElementById('large').getContext('2d');g.imageSmoothingEnabled=false;g.drawImage(canvases[n],0,0);document.getElementById('detailLabel').textContent=String.fromCharCode(65+n)+' — '+names[n];document.getElementById('compare').setAttribute('aria-pressed','false');document.querySelectorAll('[data-pick]').forEach(b=>b.setAttribute('aria-pressed',+b.dataset.pick===n?'true':'false'))}
Promise.all(files.map(name=>new Promise((resolve,reject)=>{const im=new Image;im.onload=()=>{images[name]=im;resolve()};im.onerror=reject;im.src='assets/'+name+'.png'}))).then(()=>{
 const f=images.font,fc=document.createElement('canvas');fc.width=f.width;fc.height=f.height;const fg=fc.getContext('2d');fg.drawImage(f,0,0);const data=fg.getImageData(0,0,f.width,f.height).data,cell=f.width/8;space=Math.max(3,Math.floor(cell/3));
 for(let i=0;i<alphabet.length;i++){const cx=i%8*cell,cy=Math.floor(i/8)*cell;let left=cell,right=-1,pts=[];for(let y=0;y<cell;y++)for(let x=0;x<cell;x++)if(data[((cy+y)*f.width+cx+x)*4+3]>127){left=Math.min(left,x);right=Math.max(right,x);pts.push([x,y]);if(i<26){capTop=Math.min(capTop,y);capBottom=Math.max(capBottom,y)}}if(right>=left)glyphs[alphabet[i]]={width:right-left+1,ink:pts.map(([x,y])=>[x-left,y])}}
 [A,B,CLayout].forEach((fn,i)=>fn(canvases[i].getContext('2d')));
 canvases.forEach((c,i)=>{const a=document.createElement('a');a.id='export-'+i;a.href=c.toDataURL('image/png');a.download='layout-'+String.fromCharCode(65+i)+'.png';a.textContent=' PNG '+String.fromCharCode(65+i)+' ';a.style.color=C.gold;document.querySelector('footer').append(a)});
 const board=document.createElement('canvas');board.width=4416;board.height=1050;const bg=board.getContext('2d');bg.fillStyle='#141215';bg.fillRect(0,0,board.width,board.height);bg.imageSmoothingEnabled=false;
 const descriptions=['One horizontal glance across all five orders.','Larger controls; more of the left architecture covered.','Defence left, offence right; points between the groups.'];
 canvases.forEach((c,i)=>{const x=24+i*1464;bg.fillStyle='#d6aa5c';bg.font='bold 30px sans-serif';bg.fillText(String.fromCharCode(65+i)+' — '+names[i],x,44);bg.drawImage(c,x,72);bg.fillStyle='#b8aa98';bg.font='24px sans-serif';bg.fillText(descriptions[i],x,1012)});
 const boardLink=document.createElement('a');boardLink.id='export-board';boardLink.href=board.toDataURL('image/png');boardLink.download='three-layouts.png';boardLink.textContent=' Comparison PNG ';boardLink.style.color=C.gold;document.querySelector('footer').append(boardLink);
 const reference=document.createElement('canvas');reference.width=1440;reference.height=900;reference.getContext('2d').drawImage(images.fight,0,0);const expected=reference.getContext('2d').getImageData(180,280,1088,470).data;
 const checks=canvases.map(c=>{const actual=c.getContext('2d').getImageData(180,280,1088,470).data;let changed=0;for(let i=0;i<actual.length;i+=4)if(actual[i]!==expected[i]||actual[i+1]!==expected[i+1]||actual[i+2]!==expected[i+2]||actual[i+3]!==expected[i+3])changed++;return changed});
 document.getElementById('audit').textContent=checks.every(n=>n===0)?'Fighter area verified identical in all three.':'PROTECTED AREA CHECK FAILED: '+checks;
 document.getElementById('audit').dataset.changes=JSON.stringify(checks);document.getElementById('loading').remove();
 canvases.forEach((c,i)=>c.onclick=()=>choose(i));document.querySelectorAll('[data-pick]').forEach(b=>b.onclick=()=>choose(+b.dataset.pick));
 document.getElementById('compare').onclick=()=>{delete document.body.dataset.mode;document.body.classList.remove('native');document.getElementById('compare').setAttribute('aria-pressed','true');document.querySelectorAll('[data-pick]').forEach(b=>b.setAttribute('aria-pressed','false'))};
 document.getElementById('native').onclick=()=>{choose(selected);document.body.classList.toggle('native')};
}).catch(e=>document.getElementById('loading').textContent='Could not load the local fight assets: '+e);
