local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/'
local out=root..'Bloodridge-Battleground-20261013/'
local before=root..'Bloodridge-Battle-Screen-20261013/'
local pc=app.pixelColor
local function im() return Image(1440,900,ColorMode.RGB) end
local source=Image{fromFile=out..'cohesive-palette.png'}
local back,ground,left,near=im(),im(),im(),im()
local ox=math.floor((source.width-1440)/2);local oy=math.floor((source.height-900)/2)
-- Native crop only: zero resizing or pixel enlargement. A lossless static partition,
-- not a claim that hidden scenery exists behind each plane for camera movement.
for y=80,833 do for x=32,1407 do
 local c=source:getPixel(x+ox,y+oy)
 local plane=ground
 if y<280 then plane=back end
 if x<290 and y<520 and x<(330-(y-240)*.55) then plane=left end
 if y>600 and (x<(y-570)*.64 or x>1440-(y-660)*1.3) then plane=near end
 plane:drawPixel(x,y,c)
end end
local shadow=im()
for _,p in ipairs({{250,640,32},{420,420,23},{420,560,26},{420,700,28},{600,420,41},{600,700,33},{840,420,36},{840,560,36},{840,700,37},{1020,420,26},{1020,530,25},{1160,620,43}}) do
 for y=-4,3 do for x=-p[3],p[3] do
  if (x/p[3])^2+(y/4)^2<1 then shadow:drawPixel(p[1]+x,p[2]+y,pc.rgba(38,28,32,58)) end
 end end
end
local s=Sprite(1440,900,ColorMode.RGB);s.layers[1].name='01 distant connected terraces';s:newCel(s.layers[1],1,back)
local function layer(n,i,locked) local l=s:newLayer();l.name=n;s:newCel(l,1,i);if locked then l.isEditable=false end;return l end
layer('02 continuous worn battleground',ground)
layer('03 single rooted burial entrance',left)
layer('04 stone lip roots and burial cloth foreground',near)
layer('05 fighter contact shadows',shadow)
local stage=im();stage:drawSprite(s,1);stage:saveAs(out..'scenery.png')
local combat=Image{fromFile=before..'captured-combat.png'}
local hud=Image{fromFile=before..'captured-hud.png'}
layer('06 captured combat - unchanged',combat,true)
layer('07 captured HUD - unchanged',hud,true)
s:saveAs(out..'bloodridge-battleground.aseprite')
local merged=im();merged:drawSprite(s,1);merged:saveAs(out..'battle-screen.png')
ground:saveAs(out..'ground-plane.png');back:saveAs(out..'distance.png');near:saveAs(out..'foreground.png');left:saveAs(out..'entrance.png')
local f=io.open(out..'native-crop.txt','w');f:write('Source '..source.width..'x'..source.height..'; native centre crop offset '..ox..','..oy..'. No resampling. Stage window x32..1407,y80..833.\n');f:close()
