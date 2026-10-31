local out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Hybrid-Materials-20261018/'
local src=Image{fromFile='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Refinement-20261013/source-clean.png'}
local function inside(x,y,p)
 local hit=false;local j=#p
 for i=1,#p do local a,b=p[i],p[j];if (a[2]>y)~=(b[2]>y) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then hit=not hit end;j=i end
 return hit
end
local function cut(name,ox,oy,w,h,poly)
 local im=Image(w,h,ColorMode.RGB)
 for y=0,h-1 do for x=0,w-1 do if inside(x,y,poly) then im:drawPixel(x,y,src:getPixel(ox+x,oy+y)) end end end
 im:saveAs(out..name..'.png')
end
-- Trace real stone/root contours; all retained pixels are untouched source artwork.
cut('rooted-wing',0,0,440,464,{{0,10},{45,10},{45,2},{201,2},{205,9},{237,6},{252,48},{272,82},{294,153},{323,168},{342,187},{369,179},{391,189},{404,220},{395,251},{413,283},{440,299},{427,320},{392,337},{361,354},{345,388},{296,402},{260,425},{203,448},{176,464},{0,464}})
cut('broken-wall',1120,138,410,181,{{0,113},{37,83},{64,70},{81,35},{118,28},{135,9},{194,0},{222,13},{229,23},{346,20},{410,36},{410,164},{356,173},{311,166},{243,181},{189,174},{138,178},{96,169},{31,174},{0,165}})
cut('loose-stones',404,202,233,91,{{0,45},{27,34},{35,19},{64,15},{79,23},{96,7},{124,0},{151,12},{163,31},{185,29},{210,42},{218,59},{233,66},{228,82},{194,91},{158,86},{117,89},{69,83},{22,88},{0,74}})
cut('root-outcrop',0,630,380,362,{{0,221},{12,190},{18,162},{31,154},{32,122},{48,110},{57,87},{75,92},{89,104},{88,127},{116,131},{143,131},{153,158},{175,178},{195,178},{216,212},{223,240},{239,251},{268,247},{286,264},{314,263},{336,275},{368,280},{380,296},{379,338},{341,349},{310,355},{272,351},{241,361},{205,355},{166,360},{123,351},{84,355},{55,341},{20,344},{0,326}})
print('Extracted three native pixel silhouettes; source untouched.')
