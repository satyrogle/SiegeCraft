local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Depth-20261020/'
local raw=Image{fromFile=root..'underpaint-raw-2.png'}
local mask=Image{fromFile=root..'near-left-removal-mask-2.png'}
local pc=app.pixelColor
for y=0,raw.height-1 do for x=0,raw.width-1 do
 local c=raw:getPixel(x,y)
 raw:drawPixel(x,y,pc.rgba(pc.rgbaR(c),pc.rgbaG(c),pc.rgbaB(c),255-pc.rgbaA(mask:getPixel(x,y))))
end end
raw:saveAs(root..'underpaint-material-input.png')
