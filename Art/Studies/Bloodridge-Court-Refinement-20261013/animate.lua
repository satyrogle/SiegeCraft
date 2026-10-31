local out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Refinement-20261013/'
local cap='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Battle-Screen-20261013/'
local pc=app.pixelColor;local function im() return Image(1440,900,ColorMode.RGB) end
local ground=Image{fromFile=out..'ground-underpaint.png'}
local back=Image{fromFile=out..'rear-burial-architecture.png'}
local portal=Image{fromFile=out..'rooted-entrance.png'}
local shadows=Image{fromFile=out..'contact-shadows.png'}
local combat=Image{fromFile=cap..'captured-combat.png'}
local fg=Image{fromFile=out..'foreground.png'}
local cloth=Image{fromFile=out..'burial-linen.png'}
local hud=Image{fromFile=cap..'captured-hud.png'}
local dusts={Image{fromFile=out..'ks-bloodridge-court-restrained-ash-review-1.png'},Image{fromFile=out..'ks-bloodridge-court-restrained-ash-review-2.png'}}
local s=Sprite(1440,900,ColorMode.RGB)
local layerNames={'01 complete fighting floor','02 rear burial terraces and sealed figure','03 rooted entrance','04 pose-specific contact shadows','05 PixelLab ash and dust','06 captured combat - protected','07 near stone and roots','08 anchored burial linen','09 captured HUD - protected'}
local layers={s.layers[1]};layers[1].name=layerNames[1]
for i=2,#layerNames do local l=s:newLayer();l.name=layerNames[i];layers[i]=l end
for i=2,32 do s:newEmptyFrame(i) end
local static={ground,back,portal,shadows,false,combat,fg,false,hud}
local staticBase=im();for _,img in ipairs({ground,back,portal,shadows}) do staticBase:drawImage(img) end;staticBase:saveAs(out..'rear-composite.png')
local clothSheet=Image(226*8,562*4,ColorMode.RGB)
local film=Image(150*4,290*2,ColorMode.RGB);local filmIndex=0
local samples={[0]=true,[4]=true,[8]=true,[12]=true,[16]=true,[20]=true,[24]=true,[31]=true}
for frame=0,31 do
 local fi=frame+1
 s.frames[fi].duration=.125
 for k,img in pairs(static) do if img then s:newCel(layers[k],fi,img) end end
 local localFrame=frame%24;local sheet=dusts[math.floor(frame/24)+1]
 local dust=Image(sheet,Rectangle((localFrame%6)*1024,math.floor(localFrame/6)*600,1024,600))
 s:newCel(layers[5],fi,dust,Point(208,140))
 local clothFrame=Image(226,562,ColorMode.RGB)
 for y=0,561 do
  local free=math.max(0,math.min(1,(y-157)/165))
  local offset=math.floor(free*(2.6*math.sin(frame/32*math.pi*2-(y-157)/90)+.8*math.sin(frame/32*math.pi*4-(y-157)/55))+.5)
  for x=0,225 do local c=cloth:getPixel(x,y);if pc.rgbaA(c)>0 and x+offset>=0 and x+offset<226 then clothFrame:drawPixel(x+offset,y,c) end end
 end
 s:newCel(layers[8],fi,clothFrame,Point(1186,515))
 clothSheet:drawImage(clothFrame,Point((frame%8)*226,math.floor(frame/8)*562))
 local flat=im();flat:drawSprite(s,fi);flat:saveAs(out..'frames/frame-'..string.format('%02d',fi)..'.png')
 if frame==0 then flat:saveAs(out..'battle-screen.png') end
 if samples[frame] then
  local crop=Image(flat,Rectangle(1275,545,133,280));film:drawImage(crop,Point((filmIndex%4)*150,math.floor(filmIndex/4)*290));filmIndex=filmIndex+1
 end
end
layers[6].isEditable=false;layers[9].isEditable=false
local tag=s:newTag(1,32);tag.name='court-wind';tag.aniDir=AniDir.FORWARD
s:saveAs(out..'bloodridge-court-refinement.aseprite')
clothSheet:saveAs(out..'linen-sheet.png');film:resize(1200,1160);film:saveAs(out..'linen-filmstrip-2x.png')
local f=io.open(out..'animation.json','w');f:write('{"frames":32,"fps":8,"durationMs":125,"loopMs":4000,"tag":"court-wind","dust":{"x":208,"y":140,"width":1024,"height":600,"firstSheetFrames":24,"columns":6},"linen":{"x":1186,"y":515,"width":226,"height":562,"columns":8,"anchorRows":[141,157]},"combat":"frozen captured pixels","rendererIntegration":false}');f:close()
