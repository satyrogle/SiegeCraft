"""Correct the observed missing fight shelf with one bounded local masked pass."""
from pathlib import Path
import json, shutil, time, sys
import numpy as np
from PIL import Image, ImageDraw
from revise import ROOT, INPUT, OUTPUT, api, GROVE

NATURAL='--natural' in sys.argv
OUT=ROOT/('natural-ground' if NATURAL else 'shelf-correction')

def prepare():
    OUT.mkdir(exist_ok=True)
    im=Image.open(ROOT/'raw.png').convert('RGB')
    a=np.array(im);r,g,b=[a[:,:,i].astype(float) for i in range(3)]
    # Keep the sky's native clusters while replacing the unwanted cyan palette.
    sky=(b>r+18)&(g>r+15)&(np.indices(r.shape)[0]<270)
    lum=.2126*r+.7152*g+.0722*b
    for channel,offset in enumerate([12,-2,-13]):a[:,:,channel][sky]=np.clip(lum[sky]+offset,0,255)
    im=Image.fromarray(a);d=ImageDraw.Draw(im)
    # The ground, not a canyon, occupies the lower half at the unchanged battle camera.
    d.polygon([(0,370),(260,382),(510,426),(710,485),(1060,518),(1310,468),(1530,391),(1810,370),(2080,365),(2290,355),(2530,347),(2750,329),(3008,305),(3008,768),(0,768)],fill='#9a7f6f')
    # Sparse irregular exposed bedding gives the generator material/flow cues.
    d.polygon([(0,529),(340,472),(600,510),(790,579),(1020,599),(1200,571),(1470,501),(1790,453),(2080,443),(2370,419),(2650,423),(3008,399),(3008,441),(2670,467),(2380,459),(2080,491),(1800,491),(1510,546),(1230,615),(1040,638),(770,625),(580,561),(330,518),(0,579)],fill='#8b7266')
    d.line([(0,528),(340,471),(600,509),(790,578),(1020,598),(1200,570),(1470,500),(1790,452),(2080,442),(2370,418),(2650,422),(3008,398)],fill='#b89b83',width=9)
    d.polygon([(1460,737),(1620,700),(1770,715),(1980,699),(2200,732),(2400,702),(2590,738),(2750,726),(2870,768),(1460,768)],fill='#665048')
    d.line([(1460,735),(1620,698),(1770,713),(1980,697),(2200,730),(2400,700),(2590,736),(2750,724)],fill='#b2967d',width=12)
    old=Image.open(ROOT.parent/'panorama.png').convert('RGB')
    grove=Image.new('L',im.size);ImageDraw.Draw(grove).polygon(GROVE,fill=255)
    im.paste(old,(0,0),grove)
    im.paste(old.crop((3008,0,4128,768)),(3008,0))
    # Retain the new ravine backdrop; redraw the intervening ground as one surface.
    protection=Image.new('L',im.size)
    p=ImageDraw.Draw(protection)
    p.polygon([(0,0),(4127,0),(4127,300),(2780,300),(2530,301),(2310,309),(2080,310),(1810,313),(1550,328),(1390,359),(1250,360),(1130,396),(710,390),(510,329),(260,300),(0,300)],fill=255)
    protection=Image.fromarray(np.maximum(np.array(protection),np.array(grove)))
    ImageDraw.Draw(protection).rectangle((3008,0,4127,767),fill=255)
    im.save(OUT/'guide.png');im.save(INPUT/'shelf-guide.png')
    masked=im.convert('RGBA');masked.putalpha(protection);masked.save(INPUT/'shelf-masked.png')
    protection.save(OUT/'protected.png')
    Image.open(ROOT.parent/'original-court.png').save(INPUT/'court-style.png')

def run():
    receipt=OUT/'submission.json'
    if receipt.exists():pid=json.loads(receipt.read_text())['prompt_id']
    else:
        prepare();g=json.loads((ROOT/'workflow-api.json').read_text())
        g['7']['inputs']['image']='ks-bloodridge-terrain-20261019/court-style.png'
        g['20']['inputs']['image']='ks-bloodridge-terrain-20261019/shelf-masked.png'
        g['5']['inputs']['text']='''Fine native pixel art, match the original Bloodridge court material rendering, dusty warm sepia and violet-brown shadows. Follow the composition guide exactly. Complete the BROAD CONTINUOUS BATTLE GROUND across the entire LOWER HALF of this panoramic image. The deep ravine stays in the BACKGROUND above the fighting floor, NOT through the foreground. Keep the protected grove and far-right original court unchanged.
One enormous connected ash-and-bedrock SHELF in front of the distant ravine, wide enough for two armies. NO holes, cliffs or chasms interrupt the lower middle fighting space. The shelf has natural worn irregular edges and geological bedding, quiet sandy ash patches interlocking with exposed layered stone, a single shallow silt wash curving along the surface and scattered small stones. At the left it descends gently into the poolside root clearing, then rises subtly along the ravine bank towards the existing court on the right. Surface material gradually turns into old court paving only at the far right. DO NOT tile or repeat the court slabs across the rest of the environment. Real varied rock and sand shapes, crisp authored pixel clusters and warm material depth, not uniform blank sand, not noisy stippling. A few small grouped pale thorn plants and rust seedpods sit along the REAR BANK and extreme bottom corners. Rich irregular root and rock detail at the bank, quiet coherent ground in the middle-lower fight space. Only a thin irregular foreground ledge along the VERY BOTTOM 40 pixels suggests a drop beyond the screen. No large obelisk through the battlefield. Keep the already rendered distant ravine and mesas. Dusty grey-brown sky, no blue sky. No humans, no UI, no text, no new buildings.'''
        g['6']['inputs']['text']='holes across battlefield, canyon in foreground, narrow bridge, floating islands, generic flat sand, repeated floor tiles, blue sky, smooth 3D render'
        if NATURAL:
            # The preceding court reference caused an unwanted straight masonry wall.
            # Supply only unbuilt earth/root material, and do not repeat that reference.
            Image.open(ROOT.parent/'panorama.png').crop((570,310,1290,768)).save(INPUT/'natural-material.png')
            g['7']['inputs']['image']='ks-bloodridge-terrain-20261019/natural-material.png'
            g['5']['inputs']['text']='''pixel art environment with fine crisp native clusters, dusty ash brown, muted sepia sandstone and violet-brown shadows. Image 1 supplies NATURAL EARTH, ROOT and SILT MATERIALS, not buildings. Image 2 supplies the long landform composition. Finish the NATURAL GROUND in the lower half. Preserve the existing root grove, distant ravine skyline, and far-right court.
The ENTIRE LOWER HALF is a wide solid NATURAL BEDROCK SHELF, uninterrupted fighting space. It is part of the ash badlands, eroded into irregular broad slabs and shallow mud-cracked silt patches. Not a built walkway, not a bridge or balcony. The back of this shelf has an uneven rounded earthen bank with roots, worn ledges, scattered loose rocks, reeds and grouped pale thorn fans with rusty seedpods. NO BRICK WALL, no straight masonry parapet, no long straight horizontal line, no columns, no new structures. The deep ravine is visible BEHIND the earthen bank, high in the background. Above the ground preserve the dusty sky, open mesas, hollow tree and leaning sandstone blade.
Ground varies naturally along the continuous stretch: bare wind-sculpted ash at left, darker damp silt in front of the grove, exposed irregular worn sandstone bedding at middle, then broken stone gradually merging into existing court paving at far right. Large irregular forms, varied cracks and little piled gravel sheltered behind rock lips, native detailed material edges and quiet clearings. No uniform paving pattern or tile repetition. A shallow dry wash curves across the entire shelf, not a trench. At the very bottom near edge, a little irregular exposed stone and thorn scrub frames the view. No foreground chasm, no holes in the battle floor. Keep the protected tree and far-right court exactly. Same oblique 2.5D battle camera, no 3D render, no people, no UI, no text.'''
            g['6']['inputs']['text']='brick wall, straight parapet, long straight edge, masonry, bridge, balcony, repeated paving, foreground canyon, uniform sand floor, blue sky, 3D render'
        g['15']['inputs']['steps']=14;g['16']['inputs']['noise_seed']=20261019144
        if NATURAL:g['16']['inputs']['noise_seed']=20261019145
        g['19']['inputs']['filename_prefix']='KS-Bloodridge-Terrain-20261019/shelf'
        (OUT/'workflow-api.json').write_text(json.dumps(g,indent=2))
        result=api('/prompt',{'prompt':g,'client_id':'codex-bloodridge-shelf-correction'});receipt.write_text(json.dumps(result,indent=2));pid=result['prompt_id']
    print(pid,flush=True)
    for i in range(720):
        h=api('/history/'+pid).get(pid)
        if h:
            (OUT/'history.json').write_text(json.dumps(h,indent=2))
            if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h['status'])
            images=[im for out in h.get('outputs',{}).values() for im in out.get('images',[])]
            if images:
                im=images[0];shutil.copyfile(OUTPUT/im.get('subfolder','')/im['filename'],OUT/'raw.png')
                print('Shelf correction saved',flush=True);return
        if i%12==0:print('Bounded shelf correction queued or rendering',flush=True)
        time.sleep(5)
    raise TimeoutError('Resume receipt; do not duplicate')

if __name__=='__main__':run()
