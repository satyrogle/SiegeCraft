local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Region-Composition-20261019/'
local s=Sprite(4128,768,ColorMode.RGB)
s.layers[1].name='New region composition'
s:newCel(s.layers[1],1,Image{fromFile=root..'new-region.png'})
local l=s:newLayer();l.name='Original court core - unchanged'
s:newCel(l,1,Image{fromFile=root..'court-core.png'},Point(3008,0))
local plan=s:newLayer();plan.name='Input composition guide - hidden'
s:newCel(plan,1,Image{fromFile=root..'layout-guide.png'});plan.isVisible=false
s:saveAs(root..'bloodridge-composition.aseprite');s:close()
local again=app.open(root..'bloodridge-composition.aseprite')
again:saveAs(root..'master-export.png');again:close()
