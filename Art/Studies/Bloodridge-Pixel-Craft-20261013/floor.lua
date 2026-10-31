local prev='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Refinement-20261013/'
local out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Pixel-Craft-20261013/'
local pc=app.pixelColor
local original=Image{fromFile=prev..'ground-underpaint.png'}
local ground=Image(original)
local function C(r,g,b) return pc.rgba(r,g,b,255) end
local ash=C(141,111,99);local stone=C(149,122,112);local light=C(161,134,120);local shade=C(135,108,101);local deep=C(125,98,92)
local function inside(x,y,p)
 local yes=false;local j=#p
 for i=1,#p do local a,b=p[i],p[j];if (a[2]>y)~=(b[2]>y) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then yes=not yes end;j=i end
 return yes
end
-- Material grouping: warm grain in the earth becomes connected ash deposits.
-- The stone's existing cooler chipped perimeter and dark cracks are preserved.
local earth={}
for _,rgb in ipairs({{160,126,109},{141,104,89},{161,120,98},{169,128,102},{181,142,116}}) do earth[C(table.unpack(rgb))]=true end
for y=350,899 do for x=0,1439 do
 local c=original:getPixel(x,y)
 if earth[c] then ground:drawPixel(x,y,ash) end
end end
-- Individually traced surface planes. These are the existing slabs' interiors,
-- not a repeated stamp or a newly imposed paving grid.
local stones={
 {{360,365},{421,359},{457,365},{444,378},{365,380},{346,376}},
 {{472,365},{544,365},{563,374},{545,383},{461,387},{449,381}},
 {{582,367},{651,364},{676,372},{664,381},{576,384}},
 {{697,369},{771,366},{799,375},{788,386},{691,388},{680,381}},
 {{882,358},{947,358},{974,369},{949,380},{882,377},{872,368}},
 {{988,358},{1060,361},{1094,374},{1074,381},{993,379},{977,370}},
 {{354,402},{399,395},{445,399},{468,412},{449,427},{385,430},{344,422}},
 {{465,397},{540,397},{574,410},{559,429},{492,433},{464,422}},
 {{595,397},{659,389},{697,402},{685,417},{620,422},{578,413}},
 {{737,394},{794,393},{823,407},{812,429},{750,430},{720,415}},
 {{856,399},{909,394},{938,404},{928,419},{861,421},{842,410}},
 {{1050,405},{1128,403},{1172,417},{1161,434},{1082,435},{1041,421}},
 {{1185,408},{1265,409},{1320,424},{1291,440},{1195,437},{1170,423}},
 {{928,440},{1000,437},{1034,450},{1015,469},{945,474},{911,458}},
 {{1098,450},{1163,445},{1203,457},{1201,475},{1120,479},{1082,466}},
 {{1205,455},{1287,451},{1337,464},{1324,483},{1235,485},{1199,473}},
 {{269,526},{311,520},{347,529},{336,540},{281,542},{262,536}},
 {{188,552},{247,545},{291,555},{279,570},{225,577},{172,568}},
 {{97,576},{151,559},{210,565},{224,580},{195,600},{136,602},{96,593}},
 {{241,580},{305,574},{347,587},{321,606},{256,611},{209,597}},
 {{337,573},{403,563},{450,580},{443,596},{379,606},{328,596}},
 {{811,511},{869,510},{899,522},{885,540},{826,540},{801,526}},
 {{919,493},{984,485},{1032,499},{1017,513},{952,522},{913,506}},
 {{1044,508},{1126,500},{1171,515},{1158,532},{1076,541},{1031,526}},
 {{1157,545},{1221,537},{1275,550},{1262,571},{1199,573},{1151,560}},
 {{876,565},{935,555},{994,566},{1003,586},{960,603},{884,601},{855,585}},
 {{1004,564},{1064,553},{1117,570},{1108,590},{1042,600},{1004,587}},
 {{1182,594},{1254,584},{1314,596},{1312,619},{1255,630},{1195,619}},
 {{134,626},{202,613},{270,617},{309,635},{282,655},{200,666},{127,655}},
 {{338,638},{410,622},{486,628},{534,646},{520,666},{461,682},{368,685},{315,667}},
 {{567,615},{609,606},{652,616},{648,634},{593,646},{547,632}},
 {{689,611},{754,602},{816,617},{804,638},{751,650},{685,638}},
 {{865,629},{929,618},{986,632},{970,651},{910,666},{849,650}},
 {{1015,638},{1070,625},{1128,638},{1120,659},{1057,671},{1005,657}},
 {{71,691},{114,677},{173,681},{194,697},{166,715},{109,726},{57,713}},
 {{186,697},{251,685},{305,692},{321,710},{293,729},{216,739},{174,722}},
 {{366,704},{430,695},{490,705},{491,721},{454,737},{378,740},{341,723}},
 {{517,704},{582,692},{651,696},{691,713},{677,734},{609,745},{536,737},{500,722}},
 {{934,700},{996,686},{1070,697},{1105,716},{1085,737},{1009,748},{945,738},{915,720}},
 {{1145,684},{1210,672},{1288,684},{1311,705},{1288,724},{1206,733},{1144,714}},
 {{40,753},{73,730},{125,729},{166,750},{151,780},{90,796},{39,785}},
 {{183,752},{240,739},{304,745},{330,766},{304,790},{225,803},{168,781}},
 {{380,759},{454,743},{533,751},{563,773},{541,795},{460,806},{381,791},{352,777}},
 {{585,759},{665,749},{742,760},{767,782},{742,806},{663,816},{587,798},{558,780}},
 {{760,805},{833,792},{914,801},{963,826},{942,853},{870,872},{794,864},{753,841}},
 {{981,768},{1058,751},{1134,761},{1165,782},{1141,806},{1069,817},{990,806},{960,789}},
 {{1163,766},{1229,747},{1304,759},{1332,778},{1323,801},{1254,816},{1184,803},{1150,783}},
 {{456,830},{529,816},{605,825},{637,848},{616,879},{537,892},{460,873},{431,851}}
}
local changed=0
for index,p in ipairs(stones) do
 local x0,y0,x1,y1=1440,900,0,0
 for _,v in ipairs(p) do x0=math.min(x0,v[1]);y0=math.min(y0,v[2]);x1=math.max(x1,v[1]);y1=math.max(y1,v[2]) end
 local w,h=x1-x0,y1-y0
 -- Surface wear follows a long fractured plane with a small connected chipped end.
 local wear={{x0+w*.06,y0+h*.63},{x0+w*.30,y0+h*.57},{x0+w*.43,y0+h*.65},{x0+w*.38,y0+h*.75},{x0+w*.20,y0+h*.78},{x0+w*.11,y0+h*.88}}
 local glint={{x0+w*.55,y0+h*.13},{x0+w*.81,y0+h*.19},{x0+w*.89,y0+h*.28},{x0+w*.75,y0+h*.25},{x0+w*.60,y0+h*.23}}
 for y=math.floor(y0),math.ceil(y1) do for x=math.floor(x0),math.ceil(x1) do if inside(x,y,p) then
  -- Keep the existing chipped edge at the selection boundary instead of tracing an outline.
  local edge=not(inside(x-2,y,p) and inside(x+2,y,p) and inside(x,y-2,p) and inside(x,y+2,p))
  if not edge then
   local c=stone
   if index%4==0 then c=light end
   if inside(x,y,wear) then c=shade end
   if index%3~=0 and inside(x,y,glint) then c=light end
   ground:drawPixel(x,y,c);changed=changed+1
  end
 end end end
end
ground:saveAs(out..'ground-underpaint.png')
local sprite=app.open(prev..'bloodridge-court-refinement.aseprite')
local layer=sprite.layers[1]
for _,cel in ipairs(layer.cels) do cel.image=ground end
sprite:saveAs(out..'floor-pass.aseprite')
local flat=Image(1440,900,ColorMode.RGB);flat:drawSprite(sprite,1);flat:saveAs(out..'floor-pass.png')
local crop=Image(flat,Rectangle(320,460,510,340));crop:resize(1020,680);crop:saveAs(out..'floor-detail-2x.png')
local f=io.open(out..'floor-work.txt','w');f:write('Retouched '..#stones..' individually traced stone surfaces; '..changed..' surface pixels. Grouped earth material colours; no resampling.\n');f:close()
