local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Region-Composition-20261019/terrain-revision/'
local s=Sprite(4128,768,ColorMode.RGB)
s.layers[1].name='Revised terrain and preserved grove'
s:newCel(s.layers[1],1,Image{fromFile=root..'new-region.png'})
local court=s:newLayer();court.name='Original court core - unchanged'
s:newCel(court,1,Image{fromFile=root..'court-core.png'},Point(3008,0))
local plan=s:newLayer();plan.name='Geography guide - hidden'
s:newCel(plan,1,Image{fromFile=root..'layout-guide.png'});plan.isVisible=false
s:saveAs(root..'bloodridge-terrain.aseprite');s:close()
local reopened=app.open(root..'bloodridge-terrain.aseprite')
reopened:saveAs(root..'master-export.png');reopened:close()
