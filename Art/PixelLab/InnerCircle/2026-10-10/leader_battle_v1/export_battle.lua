local p=app.params
local root=p["dir"];local count=tonumber(p["count"]);local size=256
local timings={};for ms in string.gmatch(p["timings"],"%d+") do timings[#timings+1]=tonumber(ms) end
assert(#timings==count,"timing count mismatch")
local function file(i) return root.."/prepared/frame_"..string.format("%03d",i)..".png" end
local spr=Sprite(size,size,ColorMode.RGB)
spr:newCel(spr.layers[1],spr.frames[1],Image{fromFile=file(0)},Point(0,0))
for i=1,count-1 do
 local fr=spr:newEmptyFrame()
 spr:newCel(spr.layers[1],fr,Image{fromFile=file(i)},Point(0,0))
end
app.activeSprite=spr
local dust=0
for _,cel in ipairs(spr.cels) do
 local im=cel.image;local seen={}
 local function key(x,y) return y*im.width+x end
 local function opaque(x,y) return x>=0 and y>=0 and x<im.width and y<im.height and app.pixelColor.rgbaA(im:getPixel(x,y))>0 end
 for y=0,im.height-1 do for x=0,im.width-1 do
  if (x==0 or y==0 or x==size-1 or y==size-1) and opaque(x,y) and not seen[key(x,y)] then
   local q={{x,y}};seen[key(x,y)]=true;local n=1
   while n<=#q do
    local z=q[n];n=n+1
    for dy=-1,1 do for dx=-1,1 do
     local nx=z[1]+dx;local ny=z[2]+dy
     if opaque(nx,ny) and not seen[key(nx,ny)] then seen[key(nx,ny)]=true;q[#q+1]={nx,ny} end
    end end
   end
   if #q<=4 then for _,z in ipairs(q) do im:drawPixel(z[1],z[2],app.pixelColor.rgba(0,0,0,0));dust=dust+1 end end
  end
 end end
end
for i,fr in ipairs(spr.frames) do fr.duration=timings[i]/1000 end
local tag=spr:newTag(1,#spr.frames);tag.name=p["state"];tag.aniDir=AniDir.FORWARD
local palette=Palette{fromFile=p["palette"]}
spr:setPalette(palette)
app.command.ChangePixelFormat{format="indexed",dithering="none"}
spr:saveCopyAs(root.."/"..p["state"]..".aseprite")
for i,fr in ipairs(spr.frames) do
 local im=Image(size,size,ColorMode.RGB);im:drawSprite(spr,fr)
 im:saveAs(root.."/frames/frame_"..string.format("%03d",i-1)..".png")
end
spr:saveCopyAs(root.."/preview.gif")
print("frames "..#spr.frames.." palette "..#spr.palettes[1].." border dust "..dust)
spr:close()
