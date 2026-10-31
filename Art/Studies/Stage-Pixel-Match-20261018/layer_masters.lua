local repo='C:/Users/jacob/KindredSiege-Clean/'
for _,region in ipairs({'skullpeak','hunters'}) do
 local s=Sprite(1376,768,ColorMode.RGB)
 local files={'ground.png','rear.png','foreground.png'}
 local names={'01 - opaque fighting ground','02 - static rear scenery','04 - near edge above fighters'}
 for i,file in ipairs(files) do
  local l=i==1 and s.layers[1] or s:newLayer();l.name=names[i]
  s:newCel(l,1,Image{fromFile=repo..'Assets/StreamingAssets/CampaignArt/RegionalReview/'..region..'/'..file})
 end
 s:saveAs(repo..'Art/Studies/Stage-Pixel-Match-20261018/'..region..'/'..region..'-stage.aseprite')
 s:close()
end
