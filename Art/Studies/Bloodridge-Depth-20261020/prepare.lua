-- Native Aseprite pixel extraction. Existing art is never rescaled.
local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Depth-20261020/'
local source=Image{fromFile='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Root-Crypt-20261019/panorama.png'}
local pc=app.pixelColor
local wall={{522,768},{542,735},{554,670},{551,585},{554,549},{575,540},{583,518},{606,529},{625,526},{633,548},{653,550},{665,576},{692,597},{717,596},{734,617},{758,622},{764,644},{787,645},{802,664},{815,680},{830,708},{855,736},{884,768}}
local function inside(poly,x,y)
  local yes=false;local j=#poly
  for i=1,#poly do
    local a,b=poly[i],poly[j]
    if (a[2]>y)~=(b[2]>y) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then yes=not yes end
    j=i
  end
  return yes
end
local mask=Image(960,416,ColorMode.RGB);mask:clear()
local input=Image(960,416,ColorMode.RGB);input:clear()
local cutout=Image(960,416,ColorMode.RGB);cutout:clear()
local white=pc.rgba(255,255,255,255)
for y=0,415 do for x=0,959 do
  local wy=y+352;local c=source:getPixel(x,wy)
  -- Pale, near-edge scrub is selected by its actual palette, inside its footprint.
  local scrub=x<510 and wy>=478 and pc.rgbaR(c)>=180 and pc.rgbaG(c)>=170 and pc.rgbaB(c)>=157
  local selected=inside(wall,x,wy) or scrub
  if selected then mask:drawPixel(x,y,white);cutout:drawPixel(x,y,c)
  else input:drawPixel(x,y,c) end
end end
mask:saveAs(root..'near-left-removal-mask.png')
input:saveAs(root..'underpaint-input.png')
cutout:saveAs(root..'near-left-source.png')
local view=Image(960,416,ColorMode.RGB)
for y=0,415 do for x=0,959 do view:drawPixel(x,y,source:getPixel(x,y+352)) end end
view:saveAs(root..'underpaint-reference.png')
app.command.CloseAllFiles{ui=false}
