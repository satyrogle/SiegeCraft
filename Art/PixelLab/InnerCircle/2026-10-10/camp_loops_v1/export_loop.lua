local p=app.params
local root=p["dir"]
local count=tonumber(p["count"])
local ms=tonumber(p["ms"] or "120")
assert(root and count,"need dir and count")
local function file(i) return root.."/prepared/frame_"..string.format("%03d",i)..".png" end
local spr=Sprite(192,192,ColorMode.RGB)
spr:newCel(spr.layers[1],spr.frames[1],Image{fromFile=file(0)},Point(0,0))
for i=1,count-1 do
  local im=Image{fromFile=file(i)}
  app.activeSprite=spr
  local fr=spr:newEmptyFrame()
  spr:newCel(spr.layers[1],fr,im,Point(0,0))
end
app.activeSprite=spr
-- Remove only isolated dust clusters of at most four pixels touching the canvas edge.
-- Keep character geometry and legitimate floating fragments intact.
local dust=0
for _,cel in ipairs(spr.cels) do
  local im=cel.image
  local seen={}
  local function key(x,y) return y*im.width+x end
  local function opaque(x,y)
    return x>=0 and y>=0 and x<im.width and y<im.height and app.pixelColor.rgbaA(im:getPixel(x,y))>0
  end
  for y=0,im.height-1 do for x=0,im.width-1 do
    local gx=x+cel.position.x
    local gy=y+cel.position.y
    if (gx==0 or gy==0 or gx==spr.width-1 or gy==spr.height-1) and opaque(x,y) and not seen[key(x,y)] then
      local queue={{x,y}};seen[key(x,y)]=true;local n=1
      while n<=#queue do
        local q=queue[n];n=n+1
        for dy=-1,1 do for dx=-1,1 do
          local nx=q[1]+dx;local ny=q[2]+dy
          if opaque(nx,ny) and not seen[key(nx,ny)] then
            seen[key(nx,ny)]=true;queue[#queue+1]={nx,ny}
          end
        end end
      end
      if #queue<=4 then
        for _,q in ipairs(queue) do im:drawPixel(q[1],q[2],app.pixelColor.rgba(0,0,0,0));dust=dust+1 end
      end
    end
  end end
end
print("removed border dust pixels "..dust)
for _,fr in ipairs(spr.frames) do fr.duration=ms/1000 end
local tag=spr:newTag(1,#spr.frames)
tag.name="CampWork"
tag.aniDir=AniDir.FORWARD
app.command.ColorQuantization{ui=false,withAlpha=true,maxColors=32}
app.command.ChangePixelFormat{format="indexed",dithering="none"}
spr:saveCopyAs(root.."/camp_work.aseprite")
for i,fr in ipairs(spr.frames) do
  local im=Image(spr.width,spr.height,ColorMode.RGB)
  im:drawSprite(spr,fr)
  im:saveAs(root.."/frames/frame_"..string.format("%03d",i-1)..".png")
end
spr:saveCopyAs(root.."/camp_work.gif")
print("exported "..#spr.frames.." frames, palette "..#spr.palettes[1])
spr:close()
