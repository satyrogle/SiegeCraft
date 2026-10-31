local out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Refinement-20261013/'
local prior='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Battleground-20261013/'
local cap='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Battle-Screen-20261013/'
local pc=app.pixelColor;local source=Image{fromFile=out..'source-clean.png'}
local material=Image{fromFile=prior..'ground-source.png'}
local function C(h) return pc.rgba(tonumber(h:sub(1,2),16),tonumber(h:sub(3,4),16),tonumber(h:sub(5,6),16),255) end
local P={C('6d5350'),C('7c625d'),C('86665a'),C('91766d'),C('9c8175'),C('a48879'),C('b29686')}
local function inside(x,y,points)
 local hit=false;local j=#points
 for i=1,#points do local a,b=points[i],points[j];if (a[2]>y)~=(b[2]>y) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then hit=not hit end;j=i end
 return hit
end
local function poly(img,pts,c)
 local minx,miny,maxx,maxy=img.width,img.height,0,0
 for _,p in ipairs(pts) do minx=math.min(minx,p[1]);miny=math.min(miny,p[2]);maxx=math.max(maxx,p[1]);maxy=math.max(maxy,p[2]) end
 for y=math.max(0,miny),math.min(img.height-1,maxy) do for x=math.max(0,minx),math.min(img.width-1,maxx) do if inside(x+.5,y+.5,pts) then img:drawPixel(x,y,c) end end end
end
local function line(img,x1,y1,x2,y2,c)
 local dx,dy=math.abs(x2-x1),math.abs(y2-y1);local sx=x1<x2 and 1 or -1;local sy=y1<y2 and 1 or -1;local err=dx-dy
 while true do if x1>=0 and y1>=0 and x1<img.width and y1<img.height then img:drawPixel(x1,y1,c) end;if x1==x2 and y1==y2 then break end;local e=2*err;if e>-dy then err=err-dy;x1=x1+sx end;if e<dx then err=err+dx;y1=y1+sy end end
end
local bg=Image(1440,900,ColorMode.RGB)
for y=0,899 do for x=0,1439 do bg:drawPixel(x,y,source:getPixel(math.min(1585,x+73),math.max(0,math.min(991,y-80)))) end end
local floor=Image(1440,900,ColorMode.RGB)
floor:clear(P[4])
-- Broad packed-ash masses describe a worn route; there is no per-pixel random noise.
local patches={
 {{0,410},{340,350},{595,355},{710,404},{935,443},{1150,476},{1440,460},{1440,515},{1180,532},{889,493},{665,450},{466,405},{0,480}},
 {{0,614},{192,535},{412,489},{545,502},{697,565},{906,587},{1097,602},{1311,643},{1440,711},{1440,770},{1200,700},{1035,665},{833,635},{620,611},{466,561},{330,569},{169,620},{0,689}},
 {{110,817},{349,728},{541,704},{683,737},{888,745},{1059,785},{1228,847},{1440,864},{1440,900},{60,900}}
}
for _,p in ipairs(patches) do poly(floor,p,P[5]) end
poly(floor,{{95,565},{196,541},{255,544},{191,558},{147,566}},P[6])
poly(floor,{{622,565},{740,574},{779,585},{677,582},{634,576}},P[6])
poly(floor,{{964,693},{1054,711},{1091,726},{1010,713}},P[6])
-- Native stone pieces. Source material is the earlier PixelLab material; no upscaling.
local shapes={
 {{3,10},{17,3},{76,2},{94,7},{92,29},{79,36},{16,36},{2,28}},
 {{7,6},{44,1},{107,3},{124,10},{117,37},{84,44},{11,41},{1,30}},
 {{2,9},{21,1},{61,3},{71,11},{64,27},{13,29},{0,22}},
 {{11,4},{59,0},{101,5},{110,18},{99,34},{68,40},{8,37},{0,24}},
 {{0,6},{24,2},{54,7},{59,19},{41,26},{7,23}},
 {{5,8},{37,1},{83,4},{89,18},{72,29},{28,33},{2,26}}
}
local tiles={};local tilesheet=Image(384,128,ColorMode.RGB)
for k,shape in ipairs(shapes) do
 local im=Image(128,64,ColorMode.RGB)
 local dark={};for _,p in ipairs(shape) do dark[#dark+1]={p[1],p[2]+3} end
 poly(im,dark,P[2]);poly(im,shape,P[5])
 -- Quiet source surface is sampled at 1:1 inside each authored contour.
 for y=0,63 do for x=0,127 do if inside(x+.5,y+.5,shape) then
  local c=material:getPixel(105+x,190+y);local r,g,b=pc.rgbaR(c),pc.rgbaG(c),pc.rgbaB(c)
  local lum=(r+g+b)/3
  if lum>156 then im:drawPixel(x,y,P[6]) elseif lum<119 then im:drawPixel(x,y,P[4]) end
 end end end
 for j=1,3 do local a,b=shape[j],shape[j+1];line(im,a[1],a[2],b[1],b[2],P[7]) end
 local a=shape[6];line(im,a[1]-9,a[2]-6,a[1]-4,a[2]-7,P[4]);line(im,a[1]-4,a[2]-7,a[1]+2,a[2]-7,P[4])
 tiles[k]=im;tilesheet:drawImage(im,Point(((k-1)%3)*128,math.floor((k-1)/3)*64))
 im:saveAs(out..'stone-'..k..'.png')
end
tilesheet:saveAs(out..'terrain-pieces.png')
-- Hand-laid clusters flank the processional wear path, with distinct non-grid offsets.
local placements={
 {337,343,3},{414,348,1},{519,353,2},{652,356,3},{729,359,4},{849,360,1},{958,354,5},{1029,357,2},{1160,351,4},{1281,362,1},
 {359,391,1},{468,400,3},{551,399,4},{675,407,5},{751,411,2},{892,405,1},{1000,403,4},{1131,400,3},{1212,408,2},
 {260,444,3},{345,449,2},{489,456,5},{559,463,1},{934,455,2},{1073,455,1},{1188,461,4},{1318,471,3},
 {146,487,2},{285,493,3},{1189,514,2},{1317,521,1},
 {58,542,4},{183,548,1},{288,561,3},{1220,568,4},{1340,580,3},
 {63,601,1},{184,612,2},{326,620,3},{1180,618,3},{1261,634,2},
 {53,661,2},{191,674,3},{284,668,4},{409,671,5},{1168,686,2},{1306,703,1},
 {49,723,3},{137,729,1},{249,722,2},{390,732,4},{515,725,3},{604,743,1},{719,745,4},{842,762,2},{988,776,3},{1079,785,1},{1192,803,4},
 {70,786,2},{211,792,4},{330,782,3},{417,797,2},{559,807,4},{683,809,3},{1280,760,3}
}
for _,v in ipairs(placements) do floor:drawImage(tiles[v[3]],Point(v[1],v[2])) end
-- A low-contrast half-buried processional inlay, kept below fighting feet.
poly(floor,{{100,765},{272,782},{423,794},{632,808},{829,809},{1044,806},{1215,801},{1357,797},{1361,805},{1183,811},{1031,817},{830,820},{628,819},{416,806},{266,794},{102,775}},P[2])
for _,p in ipairs({{176,773,185,785},{356,788,362,800},{581,804,584,816},{797,809,798,820},{1011,807,1010,818},{1220,801,1218,810}}) do line(floor,p[1],p[2],p[3],p[4],P[4]) end
-- Irregular silhouette meets the existing buried architecture, never a rectangular patch.
local border={{0,572},{63,546},{91,534},{116,526},{137,515},{167,511},{186,497},{211,486},{239,479},{260,460},{284,448},{300,435},{321,420},{332,396},{344,384},{366,376},{384,362},{411,360},{437,349},{473,345},{504,352},{542,344},{587,349},{629,346},{671,355},{704,348},{745,354},{786,349},{831,357},{875,347},{919,352},{958,343},{1004,349},{1041,344},{1083,352},{1130,346},{1174,354},{1210,349},{1255,361},{1298,363},{1341,371},{1391,374},{1440,385},{1440,900},{0,900}}
local composed=Image(bg)
local groundLayer=Image(1440,900,ColorMode.RGB)
for y=0,899 do for x=0,1439 do if inside(x+.5,y+.5,border) then local c=floor:getPixel(x,y);composed:drawPixel(x,y,c);groundLayer:drawPixel(x,y,c) end end end
floor:saveAs(out..'ground-complete.png');groundLayer:saveAs(out..'ground-visible.png');composed:saveAs(out..'crafted-environment.png')
local s=Sprite(1440,900,ColorMode.RGB);s.layers[1].name='background';s:newCel(s.layers[1],1,composed)
for _,v in ipairs({{'protected combat','captured-combat.png'},{'protected HUD','captured-hud.png'}}) do local l=s:newLayer();l.name=v[1];s:newCel(l,1,Image{fromFile=cap..v[2]});l.isEditable=false end
s:saveAs(out..'pass-01-crafted.aseprite');local flat=Image(1440,900,ColorMode.RGB);flat:drawSprite(s,1);flat:saveAs(out..'pass-01-crafted.png')
local crop=Image(flat,Rectangle(340,330,330,260));crop:resize(660,520);crop:saveAs(out..'pass-01-crafted-closeup.png')
local f=io.open(out..'terrain-layout.json','w');f:write('{"source":"PixelLab ground-source.png + authored silhouettes","scale":1,"pieceWidth":128,"pieceHeight":64,"pieces":6,"placements":[')
for i,v in ipairs(placements) do if i>1 then f:write(',') end;f:write('{"x":'..v[1]..',"y":'..v[2]..',"piece":'..v[3]..'}') end;f:write(']}');f:close()
