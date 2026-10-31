local root='C:/Users/jacob/KindredSiege-Clean/'
local out=root..'Art/Studies/Bloodridge-Battleground-20261013/'
local prev=root..'Art/Studies/Bloodridge-Battle-Screen-20261013/'
local art=root..'Art/Studies/Bloodridge-Court-Wind-20261013/'
local pc=app.pixelColor
local function rgba(r,g,b,a) return pc.rgba(r,g,b,a or 255) end
local function img() return Image(1440,900,ColorMode.RGB) end
local function rect(im,x,y,w,h,c)
 for yy=math.max(80,y),math.min(833,y+h-1) do for xx=math.max(32,x),math.min(1407,x+w-1) do im:drawPixel(xx,yy,c) end end
end
local function poly(im,pts,c)
 local low,high=900,0;for _,p in ipairs(pts) do low=math.min(low,p[2]);high=math.max(high,p[2]) end
 for y=math.max(80,low),math.min(833,high) do
  local cuts={};for k,p in ipairs(pts) do local q=pts[k%#pts+1];if (p[2]<=y and q[2]>y) or(q[2]<=y and p[2]>y) then cuts[#cuts+1]=p[1]+(y-p[2])*(q[1]-p[1])/(q[2]-p[2]) end end
  table.sort(cuts);for k=1,#cuts-1,2 do rect(im,math.ceil(cuts[k]),y,math.floor(cuts[k+1])-math.ceil(cuts[k])+1,1,c) end
 end
end
local function blit(dst,src,dx,dy,sx,sy,w,h)
 sx=sx or 0;sy=sy or 0;w=w or src.width;h=h or src.height
 for y=0,h-1 do for x=0,w-1 do local xx,yy=dx+x,dy+y
  if xx>=32 and xx<1408 and yy>=80 and yy<834 then local c=src:getPixel(sx+x,sy+y);if pc.rgbaA(c)>0 then dst:drawPixel(xx,yy,c) end end
 end end
end
local function tone(c,mix,base)
 return rgba(math.floor(pc.rgbaR(c)*(1-mix)+base[1]*mix),math.floor(pc.rgbaG(c)*(1-mix)+base[2]*mix),math.floor(pc.rgbaB(c)*(1-mix)+base[3]*mix),pc.rgbaA(c))
end
local tex=Image{fromFile=out..'ground-source.png'}
local source=Image{fromFile=art..'scenery.png'}
local distantSource=Image{fromFile=art..'distance.png'}
local portal=Image{fromFile=art..'architecture.png'}
local foregroundSource=Image{fromFile=art..'foreground.png'}
local base=img();base:clear(rgba(29,24,30));rect(base,32,80,1376,754,rgba(108,94,91))
local ground=img()
-- Pixel-preserving image quilting: choose an irregular minimum-error cut through
-- overlapping source swatches rather than repeating hard rectangular tile seams.
local function dist(a,b)
 return (pc.rgbaR(a)-pc.rgbaR(b))^2+(pc.rgbaG(a)-pc.rgbaG(b))^2+(pc.rgbaB(a)-pc.rgbaB(b))^2
end
local function quilt(dx,dy,left,top)
 local size=512;local overlap=80;local vc,hc={},{}
 if left then
  local costs,parents={},{}
  for y=0,511 do costs[y]={};parents[y]={};for x=0,overlap-1 do
   local xx,yy=dx+x,dy+y;local e=0
   if xx>=32 and xx<1408 and yy<834 then e=dist(ground:getPixel(xx,yy),tex:getPixel(x,y)) end
   local best,bx=0,x
   if y>0 then best=math.huge;for k=math.max(0,x-1),math.min(overlap-1,x+1) do if costs[y-1][k]<best then best=costs[y-1][k];bx=k end end end
   costs[y][x]=e+best;parents[y][x]=bx
  end end
  local bx=0;for x=1,overlap-1 do if costs[511][x]<costs[511][bx] then bx=x end end
  for y=511,0,-1 do vc[y]=bx;bx=parents[y][bx] end
 end
 if top then
  local costs,parents={},{}
  for x=0,511 do costs[x]={};parents[x]={};for y=0,overlap-1 do
   local xx,yy=dx+x,dy+y;local e=0
   if xx>=32 and xx<1408 and yy<834 then e=dist(ground:getPixel(xx,yy),tex:getPixel(x,y)) end
   local best,by=0,y
   if x>0 then best=math.huge;for k=math.max(0,y-1),math.min(overlap-1,y+1) do if costs[x-1][k]<best then best=costs[x-1][k];by=k end end end
   costs[x][y]=e+best;parents[x][y]=by
  end end
  local by=0;for y=1,overlap-1 do if costs[511][y]<costs[511][by] then by=y end end
  for x=511,0,-1 do hc[x]=by;by=parents[x][by] end
 end
 for y=0,511 do for x=0,511 do local xx,yy=dx+x,dy+y
  if xx>=32 and xx<1408 and yy>=230 and yy<834 and(not left or x>=overlap or x>=vc[y]) and(not top or y>=overlap or y>=hc[x]) then
   ground:drawPixel(xx,yy,tex:getPixel(x,y))
  end
 end end
end
for row=0,1 do for col=0,3 do quilt(32+col*432,230+row*432,col>0,row>0) end end
-- One illumination field binds the material to the court. All edits stay in scenery.
for y=230,833 do for x=32,1407 do
 local c=tone(ground:getPixel(x,y),.38,{156,139,127});local side=math.max(0,(190-x)/190,(x-1210)/190)
 local near=math.max(0,(y-700)/500);local m=math.min(.40,side*.32+near)
 ground:drawPixel(x,y,tone(c,m,{73,61,60}))
end end
local far=img()
-- Staggered recesses, no reflected duplicate main portal. Their feet share the court.
blit(far,distantSource,32,80)
blit(far,distantSource,420,80)
blit(far,distantSource,808,80)
-- Infill the far-left wall only where the retained main portal will cover it.
rect(far,32,80,180,200,rgba(84,67,65))
local architecture=img()
blit(architecture,portal,32,80)
-- Eroded ash shoulders connect the walls to the paving; irregular edges stay outside formations.
local erosion=img()
poly(erosion,{{32,441},{168,440},{158,462},{185,469},{147,490},{138,522},{96,538},{118,570},{73,594},{32,606}},rgba(103,85,76))
poly(erosion,{{1407,325},{1301,325},{1280,369},{1313,390},{1285,425},{1300,451},{1278,481},{1308,518},{1286,546},{1327,577},{1314,624},{1346,657},{1318,693},{1339,739},{1288,781},{1292,834},{1407,834}},rgba(102,85,78))
poly(erosion,{{32,721},{86,705},{140,744},{191,751},{225,792},{297,812},{311,834},{32,834}},rgba(96,78,72))
-- Rock-and-ash cluster texture from the retained court, constrained to the broken shoulders.
for y=80,833 do for x=32,1407 do
 if pc.rgbaA(erosion:getPixel(x,y))>0 then
  local c=source:getPixel(241+(x%91),118+(y%73))
  erosion:drawPixel(x,y,tone(c,.48,{106,87,77}))
 end
end end
-- Broken slab caps on the shoulder, using intact surface pixels from the new material.
local chips={{109,493,38,15},{70,564,27,13},{148,456,31,10},{1297,432,49,19},{1338,557,33,15},{1325,679,51,19},{1280,803,67,20},{115,769,62,23},{185,809,55,19}}
for i,p in ipairs(chips) do
 poly(erosion,{{p[1],p[2]+p[4]},{p[1]+p[3],p[2]+p[4]-2},{p[1]+p[3]-4,p[2]+p[4]+7},{p[1]+4,p[2]+p[4]+9}},rgba(63,50,49))
 for y=0,p[4]-1 do for x=0,p[3]-1 do
  if x>math.max(0,4-y) and x<p[3]-math.max(0,y-p[4]+5) then erosion:drawPixel(p[1]+x,p[2]+y,tex:getPixel(40+(i*47)%350+x,35+(i*31)%300+y)) end
 end end
end
-- Shallow burial joins set into paving, between fighters: no raised centre obstruction.
local seams=img()
local function seam(pts)
 poly(seams,pts,rgba(91,75,69,125))
end
-- Burial lids already occur in the authored paving material; no extra geometric overlay.
-- Restrained wall/contact shadows. They never tint the combat overlay.
local shadows=img()
poly(shadows,{{32,383},{170,423},{343,613},{301,627},{178,519},{32,494}},rgba(34,27,31,35))
poly(shadows,{{1330,314},{1407,328},{1407,739},{1351,710},{1288,538}},rgba(34,27,31,45))
for _,p in ipairs({{250,640,33},{420,420,24},{420,560,27},{420,700,30},{600,420,43},{600,700,35},{840,420,38},{840,560,38},{840,700,39},{1020,420,28},{1020,530,27},{1160,620,45}}) do
 for y=-5,4 do for x=-p[3],p[3] do if(x/p[3])^2+(y/5)^2<1 then shadows:drawPixel(p[1]+x,p[2]+y,rgba(34,26,29,70)) end end end
end
local near=img()
blit(near,foregroundSource,32,750,0,340,155,84)
blit(near,foregroundSource,1282,750,506,340,126,84)
-- A close cropped root-bearing pier, using the same stone and roots as the portal.
blit(near,portal,1324,477,0,30,84,354)
local combat=Image{fromFile=prev..'captured-combat.png'}
local hud=Image{fromFile=prev..'captured-hud.png'}
local s=Sprite(1440,900,ColorMode.RGB);s.layers[1].name='00 underlying ash';s:newCel(s.layers[1],1,base)
local function layer(n,im) local l=s:newLayer();l.name=n;s:newCel(l,1,im);return l end
layer('01 worn native paving',ground);layer('02 receding burial terraces',far);layer('03 single rooted burial portal',architecture)
layer('04 broken court shoulders',erosion);layer('05 inset burial joints',seams);layer('06 grounded shadows',shadows);layer('07 close stone and root framing',near)
local scenery=img();scenery:drawSprite(s,1);scenery:saveAs(out..'scenery.png')
layer('08 actual combat - protected pixels',combat).isEditable=false
layer('09 actual HUD - protected pixels',hud).isEditable=false
s:saveAs(out..'bloodridge-battleground.aseprite')
local final=img();final:drawSprite(s,1);final:saveAs(out..'battle-screen.png')
ground:saveAs(out..'ground-plane.png');erosion:saveAs(out..'court-shoulders.png');far:saveAs(out..'distance.png');near:saveAs(out..'foreground.png')
print('Saved battleground review and 10-layer master')
