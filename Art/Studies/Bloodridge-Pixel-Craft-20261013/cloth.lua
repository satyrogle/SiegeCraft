local prev='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Refinement-20261013/'
local out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Pixel-Craft-20261013/'
local pc=app.pixelColor
local cloth=Image{fromFile=prev..'burial-linen.png'}
local sprite=app.open(prev..'bloodridge-court-refinement.aseprite')
local layer=sprite.layers[8]
local function C(r,g,b) return pc.rgba(r,g,b,255) end
-- Existing linen colours only: warm face, cool shaded fold and rotten crevice.
local face=C(133,92,75);local lit=C(134,102,90);local turn=C(105,69,59);local crease=C(84,51,46);local edge=C(98,61,54)
local sheets=Image(226*8,562*4,ColorMode.RGB)
local film=Image(160*4,330*2,ColorMode.RGB)
local bareFilm=Image(75*8,210,ColorMode.RGB)
local samples={[0]=true,[4]=true,[8]=true,[12]=true,[16]=true,[20]=true,[24]=true,[31]=true};local sample=0
local ranges={{},{}}
for y=141,465 do
 for part=1,2 do
  local a,b=part==1 and 170 or 195,part==1 and 196 or 222
  local lo,hi=999,-1
  -- Original traced strips are non-overlapping in these spans. First ends at row 304.
  if part==2 or y<=304 then
   for x=a,b do if pc.rgbaA(cloth:getPixel(x,y))>0 then lo=math.min(lo,x);hi=math.max(hi,x) end end
  end
  if hi>=lo then ranges[part][y]={lo,hi} end
 end
end
for frame=0,31 do
 local im=Image(226,562,ColorMode.RGB)
 -- Both tie points are immutable, including the first folds under their bands.
 for y=0,157 do for x=0,225 do im:drawPixel(x,y,cloth:getPixel(x,y)) end end
 local theta=frame*math.pi*2/32
 for part=1,2 do
  local length=part==1 and 148 or 308
  local phase=theta+(part==1 and 0 or .95)
  local lastLeft,lastRight,lastY=nil,nil,nil
  for y=158,465 do
   local range=ranges[part][y]
   if range then
    local t=math.min(1,(y-157)/length)
    local free=t*t*(3-2*t)
    -- Travelling bend; the second strip responds later and has a longer free end.
    local wave=phase-2.15*t
    local bend=free*((part==1 and 6.0 or 8.0)*math.sin(wave)+1.6*math.sin(2*phase-4*t))
    local twist=math.sin(wave+.65)
    local widthFactor=1-free*(.19+.19*twist)
    local originalWidth=range[2]-range[1]+1
    local width=math.max(t>.985 and 1 or 2,math.floor(originalWidth*widthFactor+.5))
    local centre=(range[1]+range[2])*.5+bend
    local left=math.floor(centre-(width-1)/2+.5)
    local yy=y-math.floor(free*free*2.0*(1-math.cos(phase-1.1*t))+.5)
    -- A one-pixel tip cannot jump clear of its preceding row when the cloth turns.
    -- Keep the same continuous fabric silhouette through the narrowest poses.
    if lastLeft and yy>lastY then
     left=math.max(lastLeft-width+1,math.min(lastRight,left))
     if yy-lastY>1 then
      local join=math.max(lastLeft,math.min(lastRight,math.floor(left+width/2)))
      for by=lastY+1,yy-1 do im:drawPixel(join,by,edge) end
     end
    end
    local fold=.35+.18*twist
    local foldShape=math.max(0,1-math.abs(t-(.30+.035*math.sin(wave)))/.23,1-math.abs(t-.76)/.14)
    local lightShape=math.max(0,1-math.abs(t-.22)/.20,1-math.abs(t-(.66+.025*math.sin(wave)))/.18)
    -- Shade moves with the twisting plane, never an unrelated brightness pulse.
    for dx=0,width-1 do
     local u=(dx+.5)/width
     local colour=face
     if u<.17+.10*twist then colour=turn end
     if u>fold and u<fold+.16*foldShape then colour=crease end
     if u>.96-.30*lightShape+.06*twist and u<.94 then colour=lit end
     if dx==0 and free>.25 then colour=edge end
     local xx=left+dx
     -- Two small persistent edge losses are transported with the strip's material.
     local nickDepth=math.min(2,math.max(0,width-3))
     local nick=(part==1 and y>=264 and y<=270 and dx<nickDepth) or (part==2 and y>=294 and y<=300 and dx>=width-nickDepth)
     if not nick and xx>=0 and xx<226 and yy>157 then im:drawPixel(xx,yy,colour) end
    end
    lastLeft,lastRight,lastY=left,left+width-1,yy
   end
  end
 end
 layer:cel(frame+1).image=im
 layer:cel(frame+1).position=Point(1186,515)
 sheets:drawImage(im,Point((frame%8)*226,math.floor(frame/8)*562))
 if samples[frame] then
  local flat=Image(1440,900,ColorMode.RGB);flat:drawSprite(sprite,frame+1)
  local crop=Image(flat,Rectangle(1250,510,158,322));film:drawImage(crop,Point((sample%4)*160,math.floor(sample/4)*330))
  local bare=Image(im,Rectangle(160,130,66,210));bareFilm:drawImage(bare,Point(sample*75,0));sample=sample+1
 end
end
sprite:saveAs(out..'cloth-pass.aseprite')
sheets:saveAs(out..'linen-sheet.png')
film:resize(1280,1320);film:saveAs(out..'cloth-context-filmstrip-2x.png')
bareFilm:resize(1200,420);bareFilm:saveAs(out..'cloth-filmstrip-2x.png')
local flat=Image(1440,900,ColorMode.RGB);flat:drawSprite(sprite,1);flat:saveAs(out..'cloth-pass.png')
