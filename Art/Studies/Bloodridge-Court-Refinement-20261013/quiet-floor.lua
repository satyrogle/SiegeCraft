local out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Refinement-20261013/'
local old='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Battle-Screen-20261013/'
local pc=app.pixelColor;local src=Image{fromFile=out..'source-clean.png'}
local pal={};for _,h in ipairs({'6d5350','7c625d','86665a','91766d','9c8175','a48879','a27d69'}) do pal[#pal+1]=pc.rgba(tonumber(h:sub(1,2),16),tonumber(h:sub(3,4),16),tonumber(h:sub(5,6),16),255) end
local function inFloor(x,y)
 if y<285+math.max(0,390-x)*.56 then return false end
 if x>1450 and y>480 then return false end
 if y>640 and x<(y-640)*.95 then return false end
 if y>855 and x>1370-(y-855)*.8 then return false end
 return true
end
local cache={};local function snap(c)
 if cache[c] then return cache[c] end
 local best,bestD=pal[1],999999
 for _,p in ipairs(pal) do local d=(pc.rgbaR(c)-pc.rgbaR(p))^2+(pc.rgbaG(c)-pc.rgbaG(p))^2+(pc.rgbaB(c)-pc.rgbaB(p))^2;if d<bestD then best,bestD=p,d end end
 cache[c]=best;return best
end
local flat=Image(src)
for y=0,src.height-1 do for x=0,src.width-1 do if inFloor(x,y) then flat:drawPixel(x,y,snap(src:getPixel(x,y))) end end end
-- Discrete majority cleanup merges minor mottling into stable same-colour clusters.
-- No interpolation, blur, scaling or edits to protected fighter/HUD layers.
for pass=1,4 do
 local nextImage=Image(flat)
 for y=2,src.height-3 do for x=2,src.width-3 do if inFloor(x,y) then
  local counts={};local c=flat:getPixel(x,y);local best,bestN=c,0
  for yy=y-2,y+2 do for xx=x-2,x+2 do local p=flat:getPixel(xx,yy);counts[p]=(counts[p] or 0)+1 end end
  for p,n in pairs(counts) do if n>bestN or (n==bestN and p<best) then best,bestN=p,n end end
  if (counts[c] or 0)<=7 and bestN>=10 then nextImage:drawPixel(x,y,best) end
 end end end
 flat=nextImage
end
flat:saveAs(out..'source-quiet-floor.png')
local bg=Image(1440,900,ColorMode.RGB)
for y=80,833 do for x=32,1407 do bg:drawPixel(x,y,flat:getPixel(x+73,y-80)) end end
local s=Sprite(1440,900,ColorMode.RGB);s.layers[1].name='quiet fighting floor with preserved architecture';s:newCel(s.layers[1],1,bg)
for _,v in ipairs({{'protected captured combat','captured-combat.png'},{'protected captured HUD','captured-hud.png'}}) do local l=s:newLayer();l.name=v[1];l.isEditable=false;s:newCel(l,1,Image{fromFile=old..v[2]}) end
s:saveAs(out..'pass-01-corrected.aseprite');local render=Image(1440,900,ColorMode.RGB);render:drawSprite(s,1);render:saveAs(out..'pass-01-corrected.png')
local crop=Image(render,Rectangle(340,330,330,260));crop:resize(660,520);crop:saveAs(out..'pass-01-corrected-closeup.png')
