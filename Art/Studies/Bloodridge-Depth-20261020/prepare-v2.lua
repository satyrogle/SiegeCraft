-- Repair the too-tight first removal mask; leave enough context to reconstruct a floor.
local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Depth-20261020/'
local src=Image{fromFile='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Root-Crypt-20261019/panorama.png'}
local pc=app.pixelColor
local function inside(p,x,y)
 local yes=false;local j=#p
 for i=1,#p do local a,b=p[i],p[j];if (a[2]>y)~=(b[2]>y) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then yes=not yes end;j=i end
 return yes
end
local clear={{0,444},{492,444},{542,478},{650,484},{721,551},{849,633},{969,704},{1092,768},{0,768}}
local wall={{465,604},{478,600},{484,593},{550,584},{553,548},{574,540},{586,518},{612,527},{626,530},{634,550},{658,551},{663,575},{691,588},{715,594},{723,617},{756,622},{766,641},{787,643},{797,660},{814,679},{826,695},{845,728},{856,749},{884,768},{640,768},{608,743},{580,720},{548,702},{520,697},{503,682},{491,669},{480,649},{476,630},{467,620}}
local mask=Image(1152,512,ColorMode.RGB);mask:clear()
local input=Image(1152,512,ColorMode.RGB);input:clear()
local near=Image(1152,512,ColorMode.RGB);near:clear()
for y=0,511 do for x=0,1151 do
 local wy=y+256;local c=src:getPixel(x,wy)
 if inside(clear,x,wy) then mask:drawPixel(x,y,pc.rgba(255,255,255,255)) else input:drawPixel(x,y,c) end
 local shrub=x<510 and wy>=478 and pc.rgbaR(c)>=180 and pc.rgbaG(c)>=170 and pc.rgbaB(c)>=157
 if shrub or inside(wall,x,wy) then near:drawPixel(x,y,c) end
end end
mask:saveAs(root..'near-left-removal-mask-2.png');input:saveAs(root..'underpaint-input-2.png');near:saveAs(root..'near-left-source-2.png')
