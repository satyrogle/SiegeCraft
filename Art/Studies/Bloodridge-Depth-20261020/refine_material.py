"""Use the previous crypt's successful local material recipe on the hidden floor only."""
from pathlib import Path
import json, shutil, time
from render_underpaint import ROOT, REPO, INPUT, OUTPUT, api

def main():
    receipt=ROOT/'material-submission.json'
    if receipt.exists():pid=json.loads(receipt.read_text())['prompt_id']
    else:
        queue=api('/queue')
        if queue['queue_running'] or queue['queue_pending']:raise RuntimeError('Shared queue busy; not submitted or pre-empted')
        shutil.copyfile(ROOT/'underpaint-material-input.png',INPUT/'material.png')
        graph=json.loads((REPO/'Art/Studies/Bloodridge-Root-Crypt-20261019/crypt-material/workflow-api.json').read_text())
        graph['15']['inputs']['image']='ks-bloodridge-depth-20261020/material.png'
        graph['4']['inputs']['text']=json.dumps({
            'high_level_description':'Native detailed pixel-art fighting floor in the Bloodridge burial region. Match the unmasked original floor exactly in material, perspective, chipped stone detail and fine pixel density. Preserve everything outside the mask.',
            'compositional_deconstruction':{
                'background':'One continuous horizontal solid ground plane. Dusty ash brown and weathered sepia limestone with muted violet-grey shadows. Ground slopes away only in perspective, never steps or raised platforms. Fine hand-shaped pixel clusters and small chips. The floor is old, dull and mostly covered in ash, with partly buried flat irregular limestone patches flush with the surrounding ground.',
                'elements':[{'type':'obj','bbox':[0,370,950,1000],'desc':'Reconstruct the same flat worn battle floor in the missing lower-left region. Recessed cracks and slab joins are narrow, dusty and LOW CONTRAST. Break up the large clean geometric slab shapes into naturally eroded, fragmented old stone, varied widths and rounded chipped corners. Quiet specks of stone dust accumulate in sheltered crevices. Fine surface structure, restrained detail, perspective and grain matching the adjacent existing floor. No black or thick outlines. No raised tiles, heavy side faces or stairs. No flora, walls, pillars, figures, props or lights in this exposed fighting area.'}]}})
        graph['5']['inputs']['text']='black outline, ink, cartoon outlines, Minecraft, voxel, giant pixels, low resolution, big clean flat blocks, raised tiles, raised platforms, steps, floating islands, abyss, border around masked region, decorative floor, grid, repeating tiles, checkerboard, polished marble, glossy, 3D render, walls, plants, characters, text, watermark, noisy dithering'
        graph['10']['inputs'].update(width=1152,height=512,steps=20)
        graph['11']['inputs']['noise_seed']=20261020423
        graph['14']['inputs']['filename_prefix']='KS-Bloodridge-Depth-20261020/material'
        (ROOT/'material-workflow.json').write_text(json.dumps(graph,indent=2))
        result=api('/prompt',{'prompt':graph,'client_id':'codex-bloodridge-depth-material'})
        receipt.write_text(json.dumps(result,indent=2));pid=result['prompt_id']
    print('Local material pass:',pid,flush=True)
    for _ in range(720):
        history=api('/history/'+pid).get(pid)
        if history:
            (ROOT/'material-history.json').write_text(json.dumps(history,indent=2))
            if history.get('status',{}).get('status_str')=='error':raise RuntimeError(history['status'])
            images=[im for output in history.get('outputs',{}).values() for im in output.get('images',[])]
            if images:
                entry=images[0];shutil.copyfile(OUTPUT/entry.get('subfolder','')/entry['filename'],ROOT/'underpaint-material.png')
                print('Local material candidate saved.',flush=True);return
        time.sleep(3)
    raise TimeoutError('Receipt retained')

if __name__=='__main__':main()
