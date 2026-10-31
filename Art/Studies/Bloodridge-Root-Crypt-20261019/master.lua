local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Root-Crypt-20261019/'
local s=Sprite(4128,768,ColorMode.RGB)
s.layers[1].name='Preserved grove and native fighting floor'
s:newCel(s.layers[1],1,Image{fromFile=root..'base.png'})
local out=s:newLayer();out.name='Ash wash - native edit'
s:newCel(out,1,Image{fromFile=root..'outskirts/layer.png'})
local crypt=s:newLayer();crypt.name='Sunken root crypt - native edit'
s:newCel(crypt,1,Image{fromFile=root..'crypt/layer.png'},Point(1248,0))
local court=s:newLayer();court.name='Original court - approach floor edge blended'
s:newCel(court,1,Image{fromFile=root..'court-display.png'},Point(2752,0))
s:saveAs(root..'bloodridge-root-crypt.aseprite');s:close()
local again=app.open(root..'bloodridge-root-crypt.aseprite')
again:saveAs(root..'master-export.png');again:close()
