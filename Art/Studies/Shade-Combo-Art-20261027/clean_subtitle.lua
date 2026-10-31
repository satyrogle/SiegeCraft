local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Shade-Combo-Art-20261027/'
local sprite=app.open(root..'parry-ward-art.png')
sprite.layers[1].name='ComfyUI concept - flat original artwork'
local layer=sprite:newLayer()
layer.name='Remove invented subtitle only'
local cover=Image(360,28,ColorMode.RGB)
for y=0,27 do
  for x=0,359 do cover:drawPixel(x,y,app.pixelColor.rgba(0,0,0,255)) end
end
sprite:newCel(layer,1,cover,Point(600,112))
sprite:saveAs(root..'parry-ward-study.aseprite')
sprite:saveCopyAs(root..'parry-ward-study.png')
sprite:close()
