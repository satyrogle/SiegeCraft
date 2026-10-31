-- Bounded garment recolour of the original PixelLab sprite. No anatomy or skin redraw.
local src=app.open(app.params['in'])
app.command.ChangePixelFormat{format='rgb'}
local cel=src.cels[1]
local img=cel.image:clone()
local masks={
 {{90,42},{98,48},{107,43},{111,68},{99,79},{88,64}},
 {{87,67},{99,79},{111,67},{110,96},{108,111},{105,130},{96,144},{82,122},{82,93}},
 {{77,65},{83,75},{82,89},{69,103},{58,121},{54,142},{47,139},{43,122},{50,105},{56,92},{64,72}},
 {{80,116},{85,126},{94,147},{88,156},{90,162},{82,169},{73,159},{74,143}},
 {{112,75},{117,78},{125,92},{126,106},{134,118},{135,134},{130,142},{123,128},{112,107},{112,93}},
 {{109,122},{113,137},{123,156},{117,167},{107,158},{101,147}},
 {{82,83},{83,93},{76,100},{70,108},{66,118},{64,130},{60,144},{56,144},{52,139},{58,121},{59,106},{69,93}},
 {{100,142},{104,134},{109,144},{119,155},{112,164},{109,162}}
}
local function inside(x,y,p)
 local hit=false;local j=#p
 for i=1,#p do
  local a,b=p[i],p[j]
  if ((a[2]>y)~=(b[2]>y)) and (x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1]) then hit=not hit end
  j=i
 end
 return hit
end
local count=0
for y=0,img.height-1 do for x=0,img.width-1 do
 local on=false
 for _,p in ipairs(masks) do if inside(x+cel.position.x,y+cel.position.y,p) then on=true;break end end
 if on then
  local c=img:getPixel(x,y)
  local r,g,b,a=app.pixelColor.rgbaR(c),app.pixelColor.rgbaG(c),app.pixelColor.rgbaB(c),app.pixelColor.rgbaA(c)
  local gold=(r>g*1.32 and g>b*1.06)
  if a>0 and not gold then
   local v=math.floor((r*.2126+g*.7152+b*.0722)*.48+2)
   img:drawPixel(x,y,app.pixelColor.rgba(math.max(0,v-1),v,math.min(255,v+3),a));count=count+1
  end
 end
end end
cel.image=img
src:saveCopyAs(app.params['out'])
print('Garment pixels recoloured: '..count)
src:close()
