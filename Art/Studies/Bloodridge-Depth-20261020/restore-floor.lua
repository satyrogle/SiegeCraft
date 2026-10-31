-- Recover the existing continuous underfloor at its original world coordinates.
-- No floor patch is copied to a second place, mirrored, tiled or enlarged.
local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Depth-20261020/'
local src=Image{fromFile='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Root-Crypt-20261019/panorama.png'}
local oldFloor=Image{fromFile='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Region-Composition-20261019/terrain-revision/ground-clearance/final-composition.png'}
local pc=app.pixelColor
local function inside(p,x,y)
 local yes=false;local j=#p
 for i=1,#p do local a,b=p[i],p[j];if (a[2]>y)~=(b[2]>y) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then yes=not yes end;j=i end
 return yes
end
local clear={{455,594},{474,575},{526,558},{533,501},{576,491},{636,497},{663,523},{683,557},{729,572},{790,614},{824,649},{868,705},{930,768},{455,768}}
local wall={{465,604},{478,600},{484,593},{550,584},{553,548},{574,540},{586,518},{612,527},{626,530},{634,550},{658,551},{663,575},{691,588},{715,594},{723,617},{756,622},{766,641},{787,643},{797,660},{814,679},{826,695},{845,728},{856,749},{884,768},{640,768},{608,743},{580,720},{548,702},{520,697},{503,682},{491,669},{480,649},{476,630},{467,620}}
local mask=Image(1152,512,ColorMode.RGB);mask:clear()
local near=Image(1152,512,ColorMode.RGB);near:clear()
local scrub=Image(4128,768,ColorMode.RGB);scrub:clear()
local floor=Image(src,Rectangle(0,256,1152,512))
for y=0,511 do for x=0,1151 do local wy=y+256;local c=src:getPixel(x,wy)
 if x<510 and wy>=478 and pc.rgbaR(c)>=180 and pc.rgbaG(c)>=170 and pc.rgbaB(c)>=157 then scrub:drawPixel(x,wy,c) end
 if inside(wall,x,wy) then near:drawPixel(x,y,c) end
 if inside(clear,x,wy) then
  mask:drawPixel(x,y,pc.rgba(255,255,255,255))
  local d=20
  for i=1,19 do if not inside(clear,x-i,wy) or not inside(clear,x+i,wy) or not inside(clear,x,wy-i) then d=i;break end end
  local a=math.min(1,d/19);local old=oldFloor:getPixel(x,wy)
  local r,g,b=pc.rgbaR(old),pc.rgbaG(old),pc.rgbaB(old)
  -- Reject pale old vegetation inside this underfloor-only mask, using an ash
  -- material color rather than bringing an old plant into the fighting lane.
  if x<565 and r>175 and g>160 and b>145 then r,g,b=134,108,99 end
  floor:drawPixel(x,y,pc.rgba(math.floor(pc.rgbaR(c)*(1-a)+r*a+.5),math.floor(pc.rgbaG(c)*(1-a)+g*a+.5),math.floor(pc.rgbaB(c)*(1-a)+b*a+.5),255))
 end
end end
mask:saveAs(root..'wall-removal-mask.png');near:saveAs(root..'near-wall-source.png');scrub:saveAs(root..'near-scrub.png');floor:saveAs(root..'underpaint-restored.png')
