local out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Refinement-20261013/'
local cap='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Battle-Screen-20261013/'
local pc=app.pixelColor
local function canvas() return Image(1440,900,ColorMode.RGB) end
local function C(r,g,b,a) return pc.rgba(r,g,b,a or 255) end
local bg=Image{fromFile=out..'controlled-environment.png'}
local source=Image{fromFile=out..'source-clean.png'}
local combat=Image{fromFile=cap..'captured-combat.png'};local hud=Image{fromFile=cap..'captured-hud.png'}
local function inside(x,y,p)
 local h=false;local j=#p
 for i=1,#p do local a,b=p[i],p[j];if (a[2]>y)~=(b[2]>y) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then h=not h end;j=i end
 return h
end
local function ellipse(im,cx,cy,rx,ry,col)
 for y=math.floor(cy-ry),math.ceil(cy+ry) do for x=math.floor(cx-rx),math.ceil(cx+rx) do if x>=0 and y>=0 and x<im.width and y<im.height and ((x-cx)/rx)^2+((y-cy)/ry)^2<=1 then im:drawPixel(x,y,col) end end end
end
local floorBorder={{0,572},{63,546},{91,534},{116,526},{137,515},{167,511},{186,497},{211,486},{239,479},{260,460},{284,448},{300,435},{321,420},{332,396},{344,384},{366,376},{384,362},{411,360},{437,349},{473,345},{504,352},{542,344},{587,349},{629,346},{671,355},{704,348},{745,354},{786,349},{831,357},{875,347},{919,352},{958,343},{1004,349},{1041,344},{1083,352},{1130,346},{1174,354},{1210,349},{1255,361},{1298,363},{1341,371},{1391,374},{1440,385},{1440,900},{0,900}}
-- The fighting floor is opaque all the way under architecture and the near props.
-- Hidden areas are underpainted in the same material; camera motion is not claimed.
local ground=canvas();local back=canvas();local portal=canvas()
local portalPoly={{0,0},{235,0},{231,176},{316,245},{380,293},{385,330},{325,404},{247,466},{163,511},{0,576}}
for y=0,899 do for x=0,1439 do
 local g=bg:getPixel(x,y)
 if not inside(x,y,floorBorder) then g=bg:getPixel(510+(x%128),530+(y%96)) end
 -- Remove the source's cropped old near pieces and paint floor behind the new ones.
 if (x<240 and y>730 and x<240-(834-y)*1.5) or (x>1360 and y>550) then g=bg:getPixel(960+(x%128),y) end
 ground:drawPixel(x,y,g)
 if not inside(x,y,floorBorder) then
  if inside(x,y,portalPoly) then portal:drawPixel(x,y,bg:getPixel(x,y)) else back:drawPixel(x,y,bg:getPixel(x,y)) end
 end
end end
-- Two native-scale foreground assets, traced around the actual stone/root silhouette.
local nearLeft=Image(380,362,ColorMode.RGB);local leftSource=Image(source,Rectangle(0,630,380,362))
local leftPoly={{0,4},{28,19},{49,25},{54,55},{78,57},{89,104},{88,127},{116,131},{143,131},{153,158},{175,178},{195,178},{216,212},{223,240},{239,251},{268,247},{286,264},{314,263},{336,275},{380,280},{380,362},{0,362}}
for y=0,361 do for x=0,379 do if inside(x,y,leftPoly) then nearLeft:drawPixel(x,y,leftSource:getPixel(x,y)) end end end
local nearRight=Image(226,562,ColorMode.RGB);local rightSource=Image(source,Rectangle(1360,430,226,562))
local rightPoly={{226,31},{186,37},{171,48},{163,62},{157,93},{145,105},{142,137},{142,170},{148,185},{138,201},{135,235},{133,274},{126,300},{122,333},{111,368},{102,396},{91,409},{68,418},{41,433},{28,448},{7,470},{0,489},{0,562},{226,562}}
local clothPoly={{181,141},{196,150},{189,191},{194,226},{190,262},{180,305},{176,300},{181,247},{178,218},{183,190},{177,174}}
local clothPoly2={{204,151},{216,153},{205,247},{209,309},{205,354},{207,420},{199,465},{195,447},{202,384},{199,328},{202,272},{197,233}}
local cloth=Image(226,562,ColorMode.RGB)
for y=0,561 do for x=0,225 do if inside(x,y,rightPoly) then
 local c=rightSource:getPixel(x,y)
 if inside(x,y,clothPoly) or inside(x,y,clothPoly2) then
  cloth:drawPixel(x,y,c);nearRight:drawPixel(x,y,rightSource:getPixel(math.max(143,x-30),y))
 else nearRight:drawPixel(x,y,c) end
end end end
nearLeft:saveAs(out..'foreground-root-rim.png');nearRight:saveAs(out..'foreground-bronze-marker.png');cloth:saveAs(out..'burial-linen.png')
local fg=canvas();fg:drawImage(nearLeft,Point(-40,620));fg:drawImage(nearRight,Point(1186,515))
-- Asymmetric footprints follow the captured boots/hem, not formation-slot centres.
local contacts={{223,636,8},{269,624,9},{398,418,5},{435,418,5},{547,416,12},{620,415,13},{391,558,6},{423,555,7},{385,693,7},{449,695,10},{565,697,9},{624,693,10},{815,410,8},{859,418,10},{820,556,10},{860,556,10},{798,699,10},{873,691,11},{998,418,6},{1033,416,6},{1004,526,6},{1031,525,6},{1122,617,10},{1150,618,17},{1192,618,11}}
local shadows=canvas()
for _,f in ipairs(contacts) do
 -- Upper-right light: each footprint grows left/down. Short penumbra and firm contact.
 for k=12,1,-1 do ellipse(shadows,f[1]-k*1.65,f[2]+k*.32,f[3]+k*.35,2.1+k*.09,C(57,41,45,24+math.floor((13-k)*2.5))) end
 ellipse(shadows,f[1],f[2]+1,f[3]+2,2.3,C(50,36,42,91))
 ellipse(shadows,f[1],f[2]+.3,f[3],1.2,C(40,28,36,145))
end
shadows:saveAs(out..'contact-shadows.png')
ground:saveAs(out..'ground-underpaint.png');back:saveAs(out..'rear-burial-architecture.png');portal:saveAs(out..'rooted-entrance.png');fg:saveAs(out..'foreground.png')
local s=Sprite(1440,900,ColorMode.RGB);s.layers[1].name='01 complete fighting floor';s:newCel(s.layers[1],1,ground)
local function layer(name,img,locked) local l=s:newLayer();l.name=name;s:newCel(l,1,img);if locked then l.isEditable=false end;return l end
layer('02 rear burial terraces and sealed figure',back)
layer('03 rooted entrance',portal)
local shadowLayer=layer('04 pose-specific contact shadows',shadows)
layer('05 captured combat - protected',combat,true)
layer('06 near stone and roots',fg)
local linen=canvas();linen:drawImage(cloth,Point(1186,515));layer('07 burial linen',linen)
layer('08 captured HUD - protected',hud,true)
s:saveAs(out..'pass-02-grounding.aseprite')
local flat=canvas();flat:drawSprite(s,1);flat:saveAs(out..'pass-02-grounding.png')
local c=Image(flat,Rectangle(178,451,297,272));c:resize(594,544);c:saveAs(out..'pass-02-contact-detail.png')
local data=io.open(out..'asset-layout.json','w');data:write('{"canvas":[1440,900],"worldPixelScale":1,"layers":[{"file":"ground-underpaint.png","x":0,"y":0,"opaque":true},{"file":"rear-burial-architecture.png","x":0,"y":0},{"file":"rooted-entrance.png","x":0,"y":0},{"file":"foreground-root-rim.png","x":-40,"y":620},{"file":"foreground-bronze-marker.png","x":1186,"y":515},{"file":"burial-linen.png","x":1186,"y":515}],"camera":"fixed","integration":false,"parallaxBehindArchitectureVerified":false}');data:close()
