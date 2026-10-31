local out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Refinement-20261013/'
local old='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Battle-Screen-20261013/'
local pc=app.pixelColor
local src=Image{fromFile=out..'source.png'}
local colors={'1b0d18','23131d','2a1824','321e27','39242b','432d33','493438','523d41','5b4446','613f41','6d5350','745e60','7c625d','86665a','91766d','9c8175','a48879','b29686','bda290','c5a894','d1b7a2','69453b','855c4b','a47358','af805e','c19574','54332e','623d36','a27d69','c19e8a','3e302d','4d413d'}
local pal={};local colorCache={}
for _,h in ipairs(colors) do pal[#pal+1]=pc.rgba(tonumber(h:sub(1,2),16),tonumber(h:sub(3,4),16),tonumber(h:sub(5,6),16),255) end
local function dist(a,b) return (pc.rgbaR(a)-pc.rgbaR(b))^2+(pc.rgbaG(a)-pc.rgbaG(b))^2+(pc.rgbaB(a)-pc.rgbaB(b))^2 end
local function snap(c)
 if colorCache[c] then return colorCache[c] end
 local best,bestd=pal[1],999999
 for _,v in ipairs(pal) do local d=dist(c,v);if d<bestd then best,bestd=v,d end end
 colorCache[c]=best;return best
end
local mapped=Image(src.width,src.height,ColorMode.RGB)
for y=0,src.height-1 do for x=0,src.width-1 do mapped:drawPixel(x,y,snap(src:getPixel(x,y))) end end
mapped:saveAs(out..'source-palette-before-cleanup.png')
-- Remove low-contrast texture flecks without moving geometry or resampling.
-- Strong material boundaries are protected; this is a cleanup aid, not hand-drawn art proof.
for pass=1,3 do
 local nextImage=Image(mapped)
 for y=2,src.height-3 do for x=2,src.width-3 do
  local c=mapped:getPixel(x,y);local counts={};local same=0;local best,bestN=c,0
  for yy=y-1,y+1 do for xx=x-1,x+1 do
   local n=mapped:getPixel(xx,yy);counts[n]=(counts[n] or 0)+1
   if n==c then same=same+1 end
  end end
  for n,k in pairs(counts) do if k>bestN or (k==bestN and n<best) then best,bestN=n,k end end
  if same<=2 and bestN>=4 and dist(c,best)<2100 then nextImage:drawPixel(x,y,best) end
 end end
 mapped=nextImage
end
mapped:saveAs(out..'source-clean.png')
local bg=Image(1440,900,ColorMode.RGB)
-- Keep native pixels. Main source translates down to retain the rear landmark below the HUD.
for y=80,833 do for x=32,1407 do bg:drawPixel(x,y,mapped:getPixel(x+73,y-80)) end end
local s=Sprite(1440,900,ColorMode.RGB);s.layers[1].name='draft environment';s:newCel(s.layers[1],1,bg)
local p=Palette(#pal);for i,c in ipairs(pal) do p:setColor(i-1,Color(c)) end;s:setPalette(p)
local function layer(n,file) local l=s:newLayer();l.name=n;s:newCel(l,1,Image{fromFile=file});l.isEditable=false end
layer('protected captured combat',old..'captured-combat.png');layer('protected captured HUD',old..'captured-hud.png')
s:saveAs(out..'pass-01-layout.aseprite')
local flat=Image(1440,900,ColorMode.RGB);flat:drawSprite(s,1);flat:saveAs(out..'pass-01-layout.png')
local crop=Image(flat,Rectangle(340,330,330,260));crop:resize(660,520);crop:saveAs(out..'pass-01-closeup.png')
