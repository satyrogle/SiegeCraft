"""The beasts the cut ins way: FLUX.2 Klein with the pixel art sprite LoRA, the graph the Dust Shark used
(Comfy-Cult-Focus-20261014/workflow-api-flux.json without its reference image), queued all at once on ArtLab.

python -I pixel_run.py pixel-jobs.json
"""
import json
import pathlib
import shutil
import sys
import time
import urllib.request

SERVER = "http://127.0.0.1:8191"
COMFY_OUTPUT = pathlib.Path("C:/Users/jacob/ComfyUI-Shared/output/ArtLab")
WORKFLOW = pathlib.Path("C:/Users/jacob/KindredSiege-Clean/Art/Studies/Comfy-Cult-Focus-20261014/workflow-api-flux.json")
PREFIX = "KS-Beast-Concepts-20261022/pixel/"


def api(path, body=None):
    req = urllib.request.Request(SERVER + path, data=None if body is None else json.dumps(body).encode(),
                                 headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=60) as r:
        return json.load(r)


def prompt_for(spec, job):
    kind = job["name"].split("-")[0]
    parts = [spec[kind + "_head"], job["idea"]]
    if kind == "shark":
        parts.append(spec["shark_hide"])
    parts.append(spec["tail"])
    return " ".join(parts)


def graph(spec, job):
    g = json.loads(WORKFLOW.read_text(encoding="utf-8"))
    for k in ("7", "8", "9", "10", "11"):
        g.pop(k)
    kind = job["name"].split("-")[0]
    g["5"]["inputs"]["text"] = prompt_for(spec, job)
    g["6"]["inputs"]["text"] = spec["negative_" + kind]
    g["13"]["inputs"]["positive"] = ["5", 0]
    g["13"]["inputs"]["negative"] = ["6", 0]
    g["13"]["inputs"]["cfg"] = spec["cfg"]
    g["12"]["inputs"].update(width=job["width"], height=job["height"])
    g["15"]["inputs"].update(width=job["width"], height=job["height"], steps=spec["steps"])
    g["16"]["inputs"]["noise_seed"] = job["seed"]
    g["19"]["inputs"]["filename_prefix"] = PREFIX + job["name"]
    return g


def main():
    spec = json.loads(pathlib.Path(sys.argv[1]).read_text(encoding="utf-8"))
    out = pathlib.Path(spec["out"])
    out.mkdir(parents=True, exist_ok=True)
    left = {}
    for job in spec["jobs"]:
        target = out / (job["name"] + ".png")
        if target.exists():
            print("have", job["name"], flush=True)
            continue
        g = graph(spec, job)
        (out / (job["name"] + "-workflow.json")).write_text(json.dumps(g, indent=2), encoding="utf-8")
        left[job["name"]] = (api("/prompt", {"prompt": g, "client_id": "ks-beast-concepts"})["prompt_id"], target)
    print("queued", list(left), flush=True)
    start = time.monotonic()
    while left and time.monotonic() - start < 3600:
        for name, (pid, target) in list(left.items()):
            entry = api("/history/" + pid).get(pid)
            if entry and entry.get("status", {}).get("status_str") == "error":
                print("ERROR", name, json.dumps(entry["status"].get("messages", [])[-1:])[:400], flush=True)
                left.pop(name)
            elif entry and entry.get("status", {}).get("completed"):
                for o in entry.get("outputs", {}).values():
                    for im in o.get("images", []):
                        shutil.copyfile(COMFY_OUTPUT / im.get("subfolder", "") / im["filename"], target)
                print("made %s at %d s" % (name, time.monotonic() - start), flush=True)
                left.pop(name)
        time.sleep(4)
    print("still waiting on", list(left) if left else "nothing", flush=True)


if __name__ == "__main__":
    main()
