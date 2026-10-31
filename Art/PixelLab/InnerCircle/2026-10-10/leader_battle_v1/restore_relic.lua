local p=app.params
local function rgba(path)
 local src=app.open(path)
 local image=Image(src.width,src.height,ColorMode.RGB)
 image:drawSprite(src,1)
 src:close()
 return image
end
local im=rgba(p["in"])
local original=rgba(p["reference"])
local sprite=Sprite(192,192,ColorMode.RGB)
for y=31,54 do for x=42,52 do
 local c=original:getPixel(x,y)
 if app.pixelColor.rgbaA(c)>0 then im:drawPixel(109+x-42,34+y-31,c) end
end end
sprite:newCel(sprite.layers[1],sprite.frames[1],im,Point(0,0))
sprite:saveCopyAs(p["out"])
sprite:saveCopyAs(p["ase"])
sprite:close()
