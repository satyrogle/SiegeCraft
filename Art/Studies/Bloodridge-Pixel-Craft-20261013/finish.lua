local prev='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Refinement-20261013/'
local out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Pixel-Craft-20261013/'
local pc=app.pixelColor
local old=Image{fromFile=prev..'ground-underpaint.png'}
local source=Image{fromFile=out..'floor-source.png'}
local palette,seen={},{}
for y=350,833 do for x=32,1407 do
 local c=old:getPixel(x,y);if not seen[c] then seen[c]=true;palette[#palette+1]=c end
end end
table.sort(palette)
local cache={}
local function distance(a,b) return (pc.rgbaR(a)-pc.rgbaR(b))^2+(pc.rgbaG(a)-pc.rgbaG(b))^2+(pc.rgbaB(a)-pc.rgbaB(b))^2 end
local function snap(c)
 if cache[c] then return cache[c] end
 local best,d=palette[1],999999
 for _,p in ipairs(palette) do local v=distance(c,p);if v<d then best,d=p,v end end
 cache[c]=best;return best
end
assert(source.width>=1440 and source.height>=900)
local ox=math.floor((source.width-1440)/2);local oy=math.floor((source.height-900)/2)
local ground=Image(1440,900,ColorMode.RGB)
for y=0,899 do for x=0,1439 do ground:drawPixel(x,y,snap(source:getPixel(x+ox,y+oy))) end end
-- Remove only isolated low-contrast 1px residue from palette snapping.
local cleaned=Image(ground)
for y=1,898 do for x=1,1438 do
 local c=ground:getPixel(x,y);local a=ground:getPixel(x-1,y);local b=ground:getPixel(x+1,y);local d=ground:getPixel(x,y-1);local e=ground:getPixel(x,y+1)
 if a==b and b==d and d==e and c~=a and distance(c,a)<800 then cleaned:drawPixel(x,y,a) end
end end
ground=cleaned
-- Merge low-contrast grain into the connected plane it borders. Strong stone
-- joints and chipped contours exceed this threshold and retain their pixels.
for pass=1,3 do
 local result=Image(ground)
 for y=1,898 do for x=1,1438 do
  local c=ground:getPixel(x,y);local counts={};local same=0;local best,n=c,0
  for yy=y-1,y+1 do for xx=x-1,x+1 do local p=ground:getPixel(xx,yy);counts[p]=(counts[p] or 0)+1;if p==c then same=same+1 end end end
  for p,k in pairs(counts) do if k>n or (k==n and p<best) then best,n=p,k end end
  if same<=3 and n>=5 and distance(c,best)<750 then result:drawPixel(x,y,best) end
 end end
 ground=result
end
ground:saveAs(out..'ground-underpaint.png')
local s=app.open(out..'cloth-pass.aseprite')
for _,cel in ipairs(s.layers[1].cels) do cel.image=ground end
s:saveAs(out..'bloodridge-pixel-craft.aseprite')
for frame=1,32 do
 local flat=Image(1440,900,ColorMode.RGB);flat:drawSprite(s,frame)
 flat:saveAs(out..'frames/frame-'..string.format('%02d',frame)..'.png')
 if frame==1 then
  flat:saveAs(out..'battle-screen.png')
  local detail=Image(flat,Rectangle(320,460,510,340));detail:resize(1020,680);detail:saveAs(out..'floor-detail-final-2x.png')
 end
end
local floorSprite=Sprite(1440,900,ColorMode.RGB);floorSprite.layers[1].name='refined floor only';floorSprite:newCel(floorSprite.layers[1],1,ground)
local pal=Palette(#palette);for i,c in ipairs(palette) do pal:setColor(i-1,Color(c)) end;floorSprite:setPalette(pal);floorSprite:saveAs(out..'floor-asset.aseprite')
local f=io.open(out..'floor-import.json','w');f:write('{"sourceWidth":'..source.width..',"sourceHeight":'..source.height..',"nativeCropOffset":['..ox..','..oy..'],"resampled":false,"paletteSource":"previous floor visible pixels","availableColours":'..#palette..'}');f:close()
