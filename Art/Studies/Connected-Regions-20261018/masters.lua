local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Connected-Regions-20261018/'
for _,region in ipairs({'bloodridge','skullpeak','hunters'}) do
 local s=Sprite(4128,768,ColorMode.RGB)
 local files={'ground.png','rear.png','foreground.png'}
 local names={'01 - connected fighting ground','02 - static rear scenery','04 - foreground above fighters'}
 for i,file in ipairs(files) do
  local l=i==1 and s.layers[1] or s:newLayer();l.name=names[i]
  s:newCel(l,1,Image{fromFile=root..region..'/'..file})
 end
 s:saveAs(root..region..'/'..region..'-connected.aseprite')
 s:close()
 local reopened=app.open(root..region..'/'..region..'-connected.aseprite')
 reopened:saveAs(root..region..'/master-export.png')
 reopened:close()
end
