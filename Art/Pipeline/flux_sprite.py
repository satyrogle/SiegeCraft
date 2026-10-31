"""Kindred Siege sprite pipeline: a description in, game ready pixel sprites out, on the local ComfyUI.

Free and local: FLUX.2 Klein 4B with the Limbicnation pixel art LoRA on ComfyUI ArtLab at 127.0.0.1:8191, the graph
Codex proved for the cult rune and champion studies (Art/Studies/Comfy-Cult-Focus-20261014/workflow-api-flux.json).
Made 14 Oct 2026 for the hunters (Jacob: "just use the local comfy using flux ... create a [pipeline] then we can go
and create whatever we want").

Each step is its own command, so a look can sit between them:
  python -I flux_sprite.py generate <job.json>   queue one image per option, copy the raw outputs into the job folder
  python -I flux_sprite.py sprite <job.json>     key out the matte, cut to the figure, shrink to native size, cut colours
  python -I flux_sprite.py sheet <job.json>      one numbered picture of the options at 3x beside an approved fighter

A job file (JSON) holds:
  name, out (folder, relative to the repo), reference (an approved sheet PNG whose frame 0 sets the style, or null),
  matte (hex), native_height (figure height in game pixels), colours (most colours kept), negative (text),
  beside (an approved sheet PNG shown next to the options for scale), options: [{id, label, prompt, seed}].
"""
import json
import pathlib
import shutil
import sys
import time
import urllib.request

import numpy as np
from PIL import Image, ImageDraw, ImageFont

REPO = pathlib.Path(__file__).resolve().parents[2]
SERVER = "http://127.0.0.1:8191"
COMFY_INPUT = pathlib.Path("C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab/input")
COMFY_OUTPUT = pathlib.Path("C:/Users/jacob/ComfyUI-Shared/output/ArtLab")
WORKFLOW = REPO / "Art/Studies/Comfy-Cult-Focus-20261014/workflow-api-flux.json"
CANVAS = 768


def api(path, body=None):
    req = urllib.request.Request(SERVER + path, data=None if body is None else json.dumps(body).encode(),
                                 headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=60) as r:
        return json.load(r)


def hex_rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def frame0(sheet_png):
    """Frame 0 of an approved sprite sheet, using the sheet's own JSON."""
    sheet = pathlib.Path(sheet_png)
    meta = json.loads(sheet.with_suffix(".json").read_text(encoding="utf-8"))
    f = meta["frames"][0]["frame"]
    return Image.open(sheet).convert("RGBA").crop((f["x"], f["y"], f["x"] + f["w"], f["y"] + f["h"]))


def load_job(path):
    job = json.loads(pathlib.Path(path).read_text(encoding="utf-8"))
    job["_out"] = REPO / job["out"]
    job["_out"].mkdir(parents=True, exist_ok=True)
    return job


def reference_input(job):
    """The style reference, 4x nearest on the matte, copied into ComfyUI's input folder. Returns its input name."""
    if not job.get("reference"):
        return None
    ref = frame0(REPO / job["reference"])
    ref = ref.crop(ref.getbbox())
    scale = max(1, min((CANVAS - 64) // ref.width, (CANVAS - 64) // ref.height, 4))
    big = ref.resize((ref.width * scale, ref.height * scale), Image.NEAREST)
    canvas = Image.new("RGBA", (CANVAS, CANVAS), hex_rgb(job["matte"]) + (255,))
    canvas.alpha_composite(big, ((CANVAS - big.width) // 2, (CANVAS - big.height) // 2))
    folder = COMFY_INPUT / ("ks-" + job["name"])
    folder.mkdir(parents=True, exist_ok=True)
    canvas.convert("RGB").save(folder / "reference.png")
    canvas.convert("RGB").save(job["_out"] / "reference-input.png")
    return "ks-" + job["name"] + "/reference.png"


def graph_for(job, option, ref_name):
    g = json.loads(WORKFLOW.read_text(encoding="utf-8"))
    g["5"]["inputs"]["text"] = option["prompt"]
    g["6"]["inputs"]["text"] = job.get("negative", "")
    g["12"]["inputs"].update(width=CANVAS, height=CANVAS)
    g["15"]["inputs"].update(width=CANVAS, height=CANVAS, steps=job.get("steps", 24))
    g["13"]["inputs"]["cfg"] = job.get("cfg", 4.0)
    g["16"]["inputs"]["noise_seed"] = option["seed"]
    g["19"]["inputs"]["filename_prefix"] = "KS-Pipeline/" + job["name"] + "/option-" + str(option["id"])
    if ref_name:
        g["7"]["inputs"]["image"] = ref_name
        g["8"]["inputs"].update(upscale_method="nearest-exact", width=CANVAS, height=CANVAS)
    else:
        # No reference: condition on the text alone.
        for k in ("7", "8", "9", "10", "11"):
            g.pop(k)
        g["13"]["inputs"]["positive"] = ["5", 0]
        g["13"]["inputs"]["negative"] = ["6", 0]
    return g


def generate(job):
    ref_name = reference_input(job)
    raw = job["_out"] / "raw"
    raw.mkdir(exist_ok=True)
    for option in job["options"]:
        target = raw / ("option-%d.png" % option["id"])
        if target.exists():
            print("have", target.name, flush=True)
            continue
        graph = graph_for(job, option, ref_name)
        (raw / ("option-%d-workflow.json" % option["id"])).write_text(json.dumps(graph, indent=2), encoding="utf-8")
        pid = api("/prompt", {"prompt": graph, "client_id": "ks-pipeline-" + job["name"]})["prompt_id"]
        start = time.monotonic()
        while True:
            if time.monotonic() - start > 1800:
                raise TimeoutError("job " + pid + " still running; look before resubmitting")
            entry = api("/history/" + pid).get(pid)
            if entry and entry.get("status", {}).get("status_str") == "error":
                raise RuntimeError(str(entry["status"].get("messages", [])[-1:]))
            if entry and entry.get("status", {}).get("completed"):
                for out in entry.get("outputs", {}).values():
                    for im in out.get("images", []):
                        shutil.copyfile(COMFY_OUTPUT / im.get("subfolder", "") / im["filename"], target)
                break
            time.sleep(4)
        print("made %s in %d s" % (target.name, time.monotonic() - start), flush=True)


def cut_out(raw_rgb, matte, tolerance):
    """Foreground mask: everything not reachable from the border through matte coloured pixels."""
    a = np.asarray(raw_rgb, dtype=np.int32)
    h, w, _ = a.shape
    border = np.concatenate([a[0], a[-1], a[:, 0], a[:, -1]])
    bg = np.median(border, axis=0) if matte is None else np.array(matte)
    near = np.abs(a - bg).sum(axis=2) <= tolerance
    seen = np.zeros((h, w), dtype=bool)
    stack = [(y, x) for x in range(w) for y in (0, h - 1) if near[y, x]] + \
            [(y, x) for y in range(h) for x in (0, w - 1) if near[y, x]]
    for y, x in stack:
        seen[y, x] = True
    while stack:
        y, x = stack.pop()
        for ny, nx in ((y - 1, x), (y + 1, x), (y, x - 1), (y, x + 1)):
            if 0 <= ny < h and 0 <= nx < w and near[ny, nx] and not seen[ny, nx]:
                seen[ny, nx] = True
                stack.append((ny, nx))
    return ~seen


def sprite(job):
    raw = job["_out"] / "raw"
    native = job["_out"] / "native"
    native.mkdir(exist_ok=True)
    for option in job["options"]:
        src = raw / ("option-%d.png" % option["id"])
        if not src.exists():
            print("missing", src.name)
            continue
        rgb = Image.open(src).convert("RGB")
        mask = cut_out(rgb, None, job.get("tolerance", 60))
        ys, xs = np.nonzero(mask)
        box = (int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1)
        rgba = rgb.convert("RGBA")
        rgba.putalpha(Image.fromarray((mask * 255).astype(np.uint8)))
        fig = rgba.crop(box)
        scale = job["native_height"] / fig.height
        size = (max(1, round(fig.width * scale)), job["native_height"])
        small = fig.resize(size, Image.BOX)
        alpha = small.getchannel("A").point(lambda v: 255 if v >= 128 else 0)
        opaque = small.convert("RGB")
        quant = opaque.quantize(colors=job.get("colours", 32), method=Image.MEDIANCUT, dither=Image.NONE).convert("RGB")
        out = quant.convert("RGBA")
        out.putalpha(alpha)
        out.save(native / ("option-%d.png" % option["id"]))
        out.resize((out.width * 4, out.height * 4), Image.NEAREST).save(native / ("option-%d-4x.png" % option["id"]))
        print("option %d: figure %dx%d native" % (option["id"], out.width, out.height))


def sheet(job):
    native = job["_out"] / "native"
    font = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 34)
    small = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 16)
    scale = 3
    beside = frame0(REPO / job["beside"]) if job.get("beside") else None
    if beside is not None:
        beside = beside.crop(beside.getbbox())
    sprites = [(o, Image.open(native / ("option-%d.png" % o["id"])).convert("RGBA")) for o in job["options"]
               if (native / ("option-%d.png" % o["id"])).exists()]
    tall = max([s.height for _, s in sprites] + ([beside.height] if beside else [])) * scale
    cell = max(max(s.width for _, s in sprites) * scale + 40, 300)
    left = beside.width * scale + 40 if beside else 0
    width = min(1400, left + cell * len(sprites))
    img = Image.new("RGB", (width, tall + 140), (40, 36, 34))
    d = ImageDraw.Draw(img)
    floor = (58, 52, 47)
    d.rectangle((0, 50, width, tall + 70), fill=floor)
    if beside:
        b = beside.resize((beside.width * scale, beside.height * scale), Image.NEAREST)
        img.paste(b, (20, 60 + tall - b.height), b)
        d.text((20, 14), "approved, for size", font=small, fill=(200, 190, 170))
    for i, (o, s) in enumerate(sprites):
        x = left + i * cell
        big = s.resize((s.width * scale, s.height * scale), Image.NEAREST)
        img.paste(big, (x + (cell - big.width) // 2, 60 + tall - big.height), big)
        d.text((x + 12, 4), str(o["id"]), font=font, fill=(250, 236, 205))
        d.text((x + 12, tall + 78), o.get("label", ""), font=small, fill=(225, 214, 192))
    path = job["_out"] / ("pick-" + job["name"] + ".png")
    img.save(path)
    print("sheet", path, img.size)


if __name__ == "__main__":
    step, job_file = sys.argv[1], sys.argv[2]
    job = load_job(job_file)
    {"generate": generate, "sprite": sprite, "sheet": sheet}[step](job)
