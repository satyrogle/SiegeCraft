-- Author the native depth master and animator source references in Aseprite.
local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Depth-20261020/'
local prior='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Root-Crypt-20261019/'
local court='C:/Users/jacob/KindredSiege-Clean/Assets/StreamingAssets/CampaignArt/Bloodridge/'
local W,H=4128,768
local pc=app.pixelColor
local function empty() local im=Image(W,H,ColorMode.RGB);im:clear();return im end
local original=Image{fromFile=prior..'panorama.png'}
local scene=Image(original)
local raw=Image{fromFile=root..'underpaint-restored.png'}
assert(raw.width==1152 and raw.height==512,'Underpaint must stay at native size')
local mask=Image{fromFile=root..'wall-removal-mask.png'}
local nearSource=Image{fromFile=root..'near-wall-source.png'}
local scrub=Image{fromFile=root..'near-scrub.png'}
-- Retain the exact original region palette. Only the masked hidden floor is new.
local colors,seen,cache={},{},{}
for y=0,H-1 do for x=0,W-1 do local c=original:getPixel(x,y)
  if not seen[c] then seen[c]=true;colors[#colors+1]={pc.rgbaR(c),pc.rgbaG(c),pc.rgbaB(c),c} end
end end
local function nearest(c)
  if cache[c] then return cache[c] end
  local r,g,b=pc.rgbaR(c),pc.rgbaG(c),pc.rgbaB(c)
  local best,dist=colors[1][4],1e10
  for _,p in ipairs(colors) do local d=(r-p[1])^2*2+(g-p[2])^2*3+(b-p[3])^2
    if d<dist then best,dist=p[4],d end end
  cache[c]=best;return best
end
local replacement=Image(1152,512,ColorMode.RGB);replacement:clear()
for y=0,511 do for x=0,1151 do if pc.rgbaA(mask:getPixel(x,y))>0 then
  local c=nearest(raw:getPixel(x,y));scene:drawPixel(x,y+256,c);replacement:drawPixel(x,y,c)
end end end
replacement:saveAs(root..'hidden-floor-reconstruction.png')
-- The existing court's authored background already continues behind its near rim.
local courtUnder=Image{fromFile=court..'ground-underpaint.png'}
courtUnder:drawImage(Image{fromFile=court..'rear-burial-architecture.png'})
courtUnder:drawImage(Image{fromFile=court..'rooted-entrance.png'})
local courtMask=Image{fromFile=court..'foreground.png'}
local courtFront=empty()
for y=0,H-1 do for x=0,1375 do
  if pc.rgbaA(courtMask:getPixel(x+32,y+66))>0 then
    courtFront:drawPixel(x+2752,y,original:getPixel(x+2752,y))
    scene:drawPixel(x+2752,y,nearest(courtUnder:getPixel(x+32,y+66)))
  end
end end
-- Far skyline has a complete overlap under the rear plane. Only its exposed
-- window is visible; no detached rooted object is assigned a parallax transform.
local far=Image(original)
local rear,floor=empty(),empty()
local border={{0,312},{180,333},{350,333},{480,315},{630,363},{730,443},{870,492},{1060,505},{1210,465},{1360,410},{1500,426},{1690,422},{1830,390},{1980,389},{2150,413},{2320,430},{2480,456},{2660,442},{2752,370},{2900,298},{3100,285},{3300,288},{3500,290},{3710,294},{3900,300},{4127,302}}
local function floorY(x)
  for i=2,#border do if x<=border[i][1] then
    local a,b=border[i-1],border[i];return a[2]+(b[2]-a[2])*(x-a[1])/(b[1]-a[1])
  end end
  return border[#border][2]
end
for x=0,W-1 do local line=floorY(x)
  for y=0,H-1 do
    -- A protected skyline opening, safely contained in the existing sky.
    local open=x<625 and y<148
    if not open then
      if y<line then rear:drawPixel(x,y,scene:getPixel(x,y))
      else floor:drawPixel(x,y,scene:getPixel(x,y)) end
    end
  end
end
-- Move the former central foreground wall to the bottom edge, below the real
-- formation's lowest 634px ground anchors. Preserve its original source pixels.
local near=empty();near:drawImage(nearSource,Point(0,400)) -- 256 + 144 px
far:saveAs(root..'far.png');rear:saveAs(root..'rear.png');floor:saveAs(root..'ground.png')
courtFront:saveAs(root..'court-foreground.png');near:saveAs(root..'near-left.png')
local debugMask=empty()
for y=0,H-1 do for x=0,W-1 do
  if pc.rgbaA(courtFront:getPixel(x,y))>0 or pc.rgbaA(near:getPixel(x,y))>0 or pc.rgbaA(scrub:getPixel(x,y))>0 then
    debugMask:drawPixel(x,y,pc.rgba(223,144,94,255))
  end
end end
debugMask:saveAs(root..'near-mask.png')
local composed=empty();composed:drawImage(far);composed:drawImage(floor);composed:drawImage(rear)
composed:drawImage(courtFront);composed:drawImage(near);composed:drawImage(scrub)
composed:saveAs(root..'panorama.png')
-- Complete rest frame of every plane is editable and aligned at world (0,0).
local s=Sprite(W,H,ColorMode.RGB)
s.layers[1].name='01 Distant skyline - exposed sky only';s:newCel(s.layers[1],1,far)
for _,item in ipairs({{'02 Stable fighting floor',floor},{'03 Rooted rear architecture',rear},
  {'04 Original court near rim',courtFront},{'05 Near wash framing - formation clearance',near},{'06 Rooted near scrub - static',scrub}}) do
  local layer=s:newLayer();layer.name=item[1];s:newCel(layer,1,item[2])
end
s:saveAs(root..'bloodridge-depth.aseprite');s:close()
local reopened=app.open(root..'bloodridge-depth.aseprite')
reopened:saveAs(root..'master-export.png');reopened:close()
-- Context crops are source references, not newly authored animation clips.
local pieces={
  {'grove-reeds-a',Rectangle(604,300,154,170)},
  {'grove-reeds-b',Rectangle(1178,250,132,132)},
  {'grove-pool',Rectangle(780,337,420,149)},
  {'crypt-shelter',Rectangle(1732,92,470,280)}
}
for _,piece in ipairs(pieces) do Image(original,piece[2]):saveAs(root..piece[1]..'-reference.png') end
local pool=Image(420,149,ColorMode.RGB);pool:clear()
local function inside(poly,x,y)
  local yes=false;local j=#poly
  for i=1,#poly do local a,b=poly[i],poly[j]
    if (a[2]>y)~=(b[2]>y) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then yes=not yes end;j=i end
  return yes
end
local poolPoly={{12,57},{100,34},{203,16},{289,29},{388,44},{402,77},{334,102},{263,115},{210,141},{156,118},{101,119},{49,91}}
for y=0,148 do for x=0,419 do if inside(poolPoly,x,y) then
  local c=original:getPixel(x+780,y+337)
  -- Constrain the proposed ripple domain to the existing cool dark seep.
  if pc.rgbaR(c)<112 and pc.rgbaB(c)>=pc.rgbaG(c)-6 then pool:drawPixel(x,y,pc.rgba(255,255,255,255)) end
end end end
pool:saveAs(root..'grove-pool-motion-mask.png')
local f=io.open(root..'native-authoring.txt','w')
f:write('Native Aseprite Lua. Source colors: '..#colors..'.\nNo frame resizing. Original court core retained.\nNew local hidden floor appears only through the extraction mask.\nNear wash framing moved down 144 px to clear formation feet.\nAnimation source references are not finished clips.\n');f:close()
