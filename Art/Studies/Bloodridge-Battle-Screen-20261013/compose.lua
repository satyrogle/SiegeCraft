local root='C:/Users/jacob/KindredSiege-Clean/'
local out=root..'Art/Studies/Bloodridge-Battle-Screen-20261013/'
local previous=root..'Art/Studies/Bloodridge-Court-Wind-20261013/'
local capture='C:/Users/jacob/.codex/scratch/ks-sprint-20261012/r4-full/coverage-raiders-Chief.png'
local shot=Image{fromFile=capture}
local old=Image{fromFile=root..'Assets/Resources/CampaignArt/Stages/bloodridge.png'}
local source=Image{fromFile=previous..'scenery.png'}
local function rgba(r,g,b,a) return app.pixelColor.rgba(r,g,b,a or 255) end
local function image() return Image(1440,900,ColorMode.RGB) end
local function rectangle(im,x,y,w,h,c)
 for yy=math.max(0,y),math.min(899,y+h-1) do for xx=math.max(0,x),math.min(1439,x+w-1) do im:drawPixel(xx,yy,c) end end
end
local function line(im,x1,y1,x2,y2,c)
 local n=math.max(math.abs(x2-x1),math.abs(y2-y1))
 for i=0,n do local x=math.floor(x1+(x2-x1)*i/math.max(1,n));local y=math.floor(y1+(y2-y1)*i/math.max(1,n));if x>=32 and x<1408 and y>=80 and y<834 then im:drawPixel(x,y,c) end end
end
local floor=image(); floor:clear(rgba(29,24,30))
rectangle(floor,32,80,1376,754,rgba(158,145,132))
-- Wide ash-stone terrace. Native-pixel seams are drawn beneath the recorded formations.
local rows={310,369,449,559,704,834}
for i=1,#rows-1 do
 local y=rows[i]; local yy=rows[i+1]
 local shade=({{159,147,135},{157,145,132},{154,142,130},{150,139,127},{145,134,123}})[i]
 rectangle(floor,32,y,1376,yy-y,rgba(table.unpack(shade)))
 line(floor,32,y,1407,y,rgba(116,104,95))
 line(floor,32,y+1,1407,y+1,rgba(172,157,143))
 for j=0,6 do
  local x=80+j*235+(i%2)*116
  line(floor,x,y,x+(x-720)*.12,yy-1,rgba(129,116,105))
 end
end
-- Reuse the accepted floor's chipped joins, in sparse patches between fighting lanes.
local patches={{345,280,120,65,680,370},{295,320,160,50,500,748},{400,240,130,75,100,515},{390,330,135,40,1080,740},{335,285,155,60,680,590}}
for _,p in ipairs(patches) do
 for yy=0,p[4]-1 do for xx=0,p[3]-1 do
  local c=source:getPixel(p[1]+xx,p[2]+yy)
  local r=app.pixelColor.rgbaR(c);local g=app.pixelColor.rgbaG(c);local b=app.pixelColor.rgbaB(c)
  if r<145 and r>90 then floor:drawPixel(p[5]+xx,p[6]+yy,rgba(math.floor((r+155)/2),math.floor((g+140)/2),math.floor((b+128)/2))) end
 end end
end
-- Contact only, beneath the captured feet and bars; no filter over fighter colours.
for _,pt in ipairs({{250,640,33},{420,420,24},{420,560,27},{420,700,30},{600,420,43},{600,700,35},{840,420,38},{840,560,38},{840,700,39},{1020,420,28},{1020,530,27},{1160,620,45}}) do
 for y=-5,4 do for x=-pt[3],pt[3] do
  if (x/pt[3])^2+(y/5)^2<1 then
   local bx,by=pt[1]+x,pt[2]+y;local c=floor:getPixel(bx,by)
   floor:drawPixel(bx,by,rgba(math.floor(app.pixelColor.rgbaR(c)*.77),math.floor(app.pixelColor.rgbaG(c)*.77),math.floor(app.pixelColor.rgbaB(c)*.78)))
  end
 end end
end
-- Native-scale court sections. Upper architecture encloses the existing battle footprint.
local arch=image()
for y=0,423 do for x=0,631 do
 local c=source:getPixel(x,y)
 -- Keep the entire upper structure; below it, retain the irregular portal silhouette.
 local maxX= y<190 and 631 or math.max(0,310-(y-190)*.72)
 if x<=maxX and y<400 then
  arch:drawPixel(32+x,80+y,c)
  arch:drawPixel(1407-x,80+y,c)
 end
end end
-- Recessed centre connecting the two sides, from the existing distant court pixels.
for y=0,189 do for x=0,111 do
 arch:drawPixel(664+x,80+y,source:getPixel(350+x,y))
end end
-- Connect rear stair line to the open battle terrace without changing sprite scale.
for y=270,309 do
 local c=source:getPixel(390,200+(y-270)%10)
 rectangle(floor,300,y,840,1,c)
end
local framing=image()
local fg=Image{fromFile=previous..'foreground.png'}
for y=340,423 do for x=0,631 do
 local c=fg:getPixel(x,y)
 if app.pixelColor.rgbaA(c)>0 then
  if x<155 then framing:drawPixel(32+x,410+y,c) end
  if x>505 then framing:drawPixel(776+x,410+y,c) end
 end
end end
-- EXACT captured combat and HUD pixels, separated by comparison with the original stage.
local combat=image();local hud=image();local retained=0
for y=0,899 do for x=0,1439 do
 local c=shot:getPixel(x,y)
 if y<80 or y>=834 or x<32 or x>=1408 then hud:drawPixel(x,y,c)
 elseif c~=old:getPixel(x-32,y-66) then combat:drawPixel(x,y,c);retained=retained+1 end
end end
local s=Sprite(1440,900,ColorMode.RGB)
s.layers[1].name='01 ash stone fighting terrace';s:newCel(s.layers[1],1,floor)
local function layer(name,im) local l=s:newLayer();l.name=name;s:newCel(l,1,im);return l end
layer('02 retained burial architecture at 1x',arch)
layer('03 near edge framing at 1x',framing)
layer('04 actual captured combat unchanged',combat)
layer('05 actual captured HUD unchanged at 2x',hud)
s:saveAs(out..'bloodridge-battle-screen.aseprite')
local merged=image();merged:drawSprite(s,1)
merged:saveAs(out..'battle-screen.png')
combat:saveAs(out..'captured-combat.png');hud:saveAs(out..'captured-hud.png')
local stage=image();stage:drawImage(floor);stage:drawImage(arch);stage:drawImage(framing);stage:saveAs(out..'scenery.png')
shot:saveAs(out..'actual-game-before.png')
local f=io.open(out..'composition-check.txt','w');f:write('1440x900 screenshot composite\nSource: '..capture..'\nRetained non-background combat pixels: '..retained..'\nWorld sections native 1x. UI from actual game capture, unchanged.\n');f:close()
print('Saved '..out..'battle-screen.png; combat pixels '..retained)
