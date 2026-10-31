-- Native static fighter-scale references. These are not gameplay screenshots.
local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Depth-20261020/'
local common='C:/Users/jacob/.codex/scratch/ks-jobs2-5-20261018/bloodridge-root-crypt/'
local pc=app.pixelColor
local art=Image{fromFile=root..'panorama.png'}
local foreground=Image{fromFile=root..'court-foreground.png'}
foreground:drawImage(Image{fromFile=root..'near-left.png'})
foreground:drawImage(Image{fromFile=root..'near-scrub.png'})
local actors={fighter=Image{fromFile=common..'fighter.png'},champion=Image{fromFile=common..'champion.png'}}
local figures={{388,494,'fighter',true},{568,354,'fighter',true},{568,634,'fighter',true},{808,354,'champion',false},{808,634,'fighter',false},{988,494,'fighter',false}}
for _,worldX in ipairs({0,1400,2752}) do
 local shot=Image(art,Rectangle(worldX,0,1376,768))
 for _,f in ipairs(figures) do
  local original=actors[f[3]];local im=Image(original)
  if f[4] then for y=0,im.height-1 do for x=0,im.width-1 do im:drawPixel(x,y,original:getPixel(im.width-1-x,y)) end end end
  for sy=-7,7 do for sx=-32,32 do if sx*sx/(32*32)+sy*sy/(7*7)<=1 then
    local x,y=f[1]+sx,f[2]-4+sy;local c=shot:getPixel(x,y)
    local r,g,b=pc.rgbaR(c),pc.rgbaG(c),pc.rgbaB(c)
    shot:drawPixel(x,y,pc.rgba(math.floor(r*.67+24*.33),math.floor(g*.67+20*.33),math.floor(b*.67+33*.33),255))
  end end end
  shot:drawImage(im,Point(f[1]-math.floor(im.width/2),f[2]-152))
 end
 shot:drawImage(Image(foreground,Rectangle(worldX,0,1376,768)))
 shot:saveAs(root..'native-reference-'..worldX..'.png')
end
