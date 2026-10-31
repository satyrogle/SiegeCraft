local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Stage-Pixel-Match-20261018/'
for _,name in ipairs({'skullpeak','hunters'}) do
 local s=Sprite(1376,768,ColorMode.RGB)
 s.layers[1].name='Raw local model output — preserved'
 s:newCel(s.layers[1],1,Image{fromFile=root..name..'/'..name..'.png'})
 s.layers[1].isVisible=false
 s.layers[1].isEditable=false
 local l=s:newLayer();l.name='Native palette and cluster cleanup — review'
 local cleaned=app.open(root..name..'/'..name..'-review.png')
 app.activeSprite=cleaned
 app.command.ChangePixelFormat{format='rgb'}
 s:newCel(l,1,cleaned.cels[1].image)
 cleaned:close()
 s:saveAs(root..name..'/'..name..'-review.aseprite')
 s:close()
end
