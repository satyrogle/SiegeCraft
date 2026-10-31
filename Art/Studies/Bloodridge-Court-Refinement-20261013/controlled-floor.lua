local out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Refinement-20261013/'
local cap='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Battle-Screen-20261013/'
local pc=app.pixelColor;local source=Image{fromFile=out..'source-clean.png'}
local W,H=source.width,source.height
local pix,seen={},{}
for y=0,H-1 do for x=0,W-1 do pix[y*W+x+1]=source:getPixel(x,y) end end
local function floor(x,y) return y>=285+math.max(0,390-x)*.56 and not(x>1450 and y>480) and not(y>640 and x<(y-640)*.95) and not(y>855 and x>1370-(y-855)*.8) end
local result=Image(source);local changed=0
for y=0,H-1 do for x=0,W-1 do
 local start=y*W+x+1
 if not seen[start] and floor(x,y) then
  local c=pix[start];local q={start};local head=1;local small={};local border={};seen[start]=true
  while head<=#q do
   local id=q[head];head=head+1;if #small<=22 then small[#small+1]=id end
   local yy=math.floor((id-1)/W);local xx=(id-1)%W
   for _,n in ipairs({id-1,id+1,id-W,id+W}) do
    if n>=1 and n<=W*H then local ny=math.floor((n-1)/W);local nx=(n-1)%W
     if math.abs(nx-xx)+math.abs(ny-yy)==1 and floor(nx,ny) then
      if pix[n]==c then if not seen[n] then seen[n]=true;q[#q+1]=n end else border[pix[n]]=(border[pix[n]] or 0)+1 end
     end
    end
   end
  end
  if #q<=20 then
   local best,score=nil,-1
   for b,n in pairs(border) do local d=(pc.rgbaR(c)-pc.rgbaR(b))^2+(pc.rgbaG(c)-pc.rgbaG(b))^2+(pc.rgbaB(c)-pc.rgbaB(b))^2
    if d<1800 and n/(1+d/120)>score then best,score=b,n/(1+d/120) end
   end
   if best then for _,id in ipairs(small) do result:drawPixel((id-1)%W,math.floor((id-1)/W),best);changed=changed+1 end end
  end
 end
end end
-- Continuous material planes retain contours. Contrast compression is scenery-only.
local palette={};local map={}
for y=0,H-1 do for x=0,W-1 do if floor(x,y) then
 local c=result:getPixel(x,y)
 if not map[c] then
  local r,g,b=pc.rgbaR(c),pc.rgbaG(c),pc.rgbaB(c)
  map[c]=pc.rgba(math.floor(r*.67+156*.33+.5),math.floor(g*.67+129*.33+.5),math.floor(b*.67+117*.33+.5),255)
 end
 result:drawPixel(x,y,map[c])
end end end
result:saveAs(out..'source-controlled-floor.png')
local bg=Image(1440,900,ColorMode.RGB);for y=0,899 do for x=0,1439 do bg:drawPixel(x,y,result:getPixel(math.min(1585,x+73),math.max(0,math.min(991,y-80)))) end end
bg:saveAs(out..'controlled-environment.png')
local s=Sprite(1440,900,ColorMode.RGB);s.layers[1].name='controlled stone and ash floor';s:newCel(s.layers[1],1,bg)
for _,v in ipairs({{'protected combat','captured-combat.png'},{'protected HUD','captured-hud.png'}}) do local l=s:newLayer();l.name=v[1];s:newCel(l,1,Image{fromFile=cap..v[2]});l.isEditable=false end
s:saveAs(out..'pass-01-controlled.aseprite');local flat=Image(1440,900,ColorMode.RGB);flat:drawSprite(s,1);flat:saveAs(out..'pass-01-controlled.png')
local crop=Image(flat,Rectangle(340,330,330,260));crop:resize(660,520);crop:saveAs(out..'pass-01-controlled-closeup.png')
local f=io.open(out..'cluster-cleanup.txt','w');f:write('Merged '..changed..' pixels in small low-contrast connected components. Preserved geometry at native scale; scenery-only floor contrast x0.67.\n');f:close()
