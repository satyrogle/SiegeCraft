-- Native pixel reconstruction of the small revealed floor. Model drafts retained,
-- but rejected for clean raised blocks. No texture copying, tiling or mirroring.
local root='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Depth-20261020/'
local src=Image{fromFile='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Root-Crypt-20261019/panorama.png'}
local mask=Image{fromFile=root..'near-left-removal-mask-2.png'}
local out=Image(src,Rectangle(0,256,1152,512));local pc=app.pixelColor
local colors,seen,cache={},{},{}
for y=0,767 do for x=0,4127 do local c=src:getPixel(x,y);if not seen[c] then
 seen[c]=true;colors[#colors+1]={pc.rgbaR(c),pc.rgbaG(c),pc.rgbaB(c),c}
end end end
local function color(r,g,b)
 r,g,b=math.floor(r+.5),math.floor(g+.5),math.floor(b+.5);local key=r*65536+g*256+b
 if cache[key] then return cache[key] end
 local best,dist=colors[1][4],1e10
 for _,p in ipairs(colors) do local d=(r-p[1])^2*2+(g-p[2])^2*3+(b-p[3])^2;if d<dist then best,dist=p[4],d end end
 cache[key]=best;return best
end
local function inside(p,x,y)
 local yes=false;local j=#p
 for i=1,#p do local a,b=p[i],p[j];if (a[2]>y)~=(b[2]>y) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then yes=not yes end;j=i end
 return yes
end
-- Individually shaped flush stone remnants, irregularly separated by ash.
local stones={
 {{0,455},{75,451},{133,458},{161,474},{146,487},{77,495},{0,488}},
 {{211,465},{308,455},{382,463},{402,479},{386,494},{315,503},{233,493},{205,482}},
 {{432,494},{515,478},{580,493},{606,509},{581,523},{493,532},{437,518}},
 {{73,527},{161,513},{217,527},{243,549},{222,565},{141,581},{73,567},{49,544}},
 {{306,536},{390,519},{447,535},{465,551},{434,572},{349,581},{299,563}},
 {{561,559},{663,544},{721,564},{735,585},{711,599},{622,615},{562,597},{541,579}},
 {{0,608},{43,595},{92,606},{111,627},{90,653},{29,661},{0,654}},
 {{183,611},{264,593},{331,604},{354,628},{333,649},{238,665},{175,648},{157,628}},
 {{409,642},{493,617},{556,630},{591,652},{576,676},{504,689},{431,681},{399,661}},
 {{696,659},{783,636},{856,650},{886,674},{865,698},{765,715},{702,693},{680,675}},
 {{42,713},{139,685},{213,700},{231,728},{199,753},{115,766},{47,748},{25,730}},
 {{302,731},{381,704},{459,718},{486,745},{468,767},{314,768},{286,751}},
 {{549,738},{632,713},{713,724},{750,749},{735,768},{559,768},{531,754}},
 {{852,736},{926,714},{998,732},{1025,757},{1011,768},{861,768},{833,754}}
}
local edge={{0,444},{492,444},{542,478},{650,484},{721,551},{849,633},{969,704},{1092,768}}
local function border(x)
 for i=2,#edge do if x<=edge[i][1] then local a,b=edge[i-1],edge[i];local m=(b[2]-a[2])/(b[1]-a[1]);return a[2]+(x-a[1])*m,m end end
 return 768,0
end
for x=0,1151 do local top,slope=border(x)
 for y=0,511 do if pc.rgbaA(mask:getPixel(x,y))>0 then
  local wy=y+256;local drift=2*math.sin(x*.011+wy*.019)+1.5*math.sin(x*.027-wy*.008)
  local r,g,b=145-(wy-444)*.024+drift,122-(wy-444)*.030+drift,109-(wy-444)*.023+drift
  for i,p in ipairs(stones) do if inside(p,x,wy) then
    local wear=math.sin(x*.049+wy*.083)*1.6+math.sin(x*.017-wy*.041)*1.5
    r,g,b=155+wear,135+wear,120+wear
    -- Broken, quiet edging, never a thick dark extrusion.
    if not inside(p,x,wy+2) then r,g,b=r-12,g-11,b-8 end
    if not inside(p,x,wy-1) then r,g,b=r+4,g+4,b+3 end
    break
  end end
  -- Sparse, tiny linked clusters, concentrated in the ash rather than blanket grain.
  local cellX,cellY=math.floor(x/3),math.floor(wy/2)
  local h=(cellX*127+cellY*233+cellX*cellY*3)%997
  if h<13 then r,g,b=r-6,g-5,b-4 elseif h>989 then r,g,b=r+5,g+5,b+4 end
  local a=math.min(1,math.max(0,(wy-top)/math.sqrt(1+slope*slope)/28))
  local old=src:getPixel(x,wy)
  out:drawPixel(x,y,color(pc.rgbaR(old)*(1-a)+r*a,pc.rgbaG(old)*(1-a)+g*a,pc.rgbaB(old)*(1-a)+b*a))
 end end
end
-- A few asymmetric recessed hairline cracks cross the eroded patches.
local cracks={{{82,534},{103,545},{98,555},{114,563}},{{325,546},{343,553},{339,565}},{{465,640},{478,649},{473,658},{487,668}},{{740,670},{754,678},{750,686}},{{128,716},{143,724},{139,738}}}
for _,path in ipairs(cracks) do for i=2,#path do local a,b=path[i-1],path[i];local count=math.max(math.abs(a[1]-b[1]),math.abs(a[2]-b[2]))
 for n=0,count do local x=math.floor(a[1]+(b[1]-a[1])*n/count+.5);local wy=math.floor(a[2]+(b[2]-a[2])*n/count+.5);local y=wy-256
  if pc.rgbaA(mask:getPixel(x,y))>0 then out:drawPixel(x,y,color(132,109,100)) end
 end
end end
out:saveAs(root..'underpaint-native.png')
