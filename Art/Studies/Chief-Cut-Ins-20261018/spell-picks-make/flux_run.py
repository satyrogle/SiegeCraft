"""Queue FLUX.2 Klein + pixel LoRA jobs (the hunters' graph, no reference) and copy the outputs.

python -I flux_run.py <jobs.json>
jobs.json: {"out": abs folder, "negative": str, "steps": int, "cfg": float,
            "jobs": [{"name", "prompt", "seed", "width", "height"}]}
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


def api(path, body=None):
    req = urllib.request.Request(SERVER + path, data=None if body is None else json.dumps(body).encode(),
                                 headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=60) as r:
        return json.load(r)


def graph(spec, job, prefix):
    g = json.loads(WORKFLOW.read_text(encoding="utf-8"))
    for k in ("7", "8", "9", "10", "11"):
        g.pop(k)
    g["5"]["inputs"]["text"] = job["prompt"]
    g["6"]["inputs"]["text"] = spec.get("negative", "")
    g["13"]["inputs"]["positive"] = ["5", 0]
    g["13"]["inputs"]["negative"] = ["6", 0]
    g["13"]["inputs"]["cfg"] = spec.get("cfg", 4.5)
    g["12"]["inputs"].update(width=job["width"], height=job["height"])
    g["15"]["inputs"].update(width=job["width"], height=job["height"], steps=spec.get("steps", 24))
    g["16"]["inputs"]["noise_seed"] = job["seed"]
    g["19"]["inputs"]["filename_prefix"] = prefix
    return g


def main():
    spec = json.loads(pathlib.Path(sys.argv[1]).read_text(encoding="utf-8"))
    out = pathlib.Path(spec["out"])
    out.mkdir(parents=True, exist_ok=True)
    for job in spec["jobs"]:
        target = out / (job["name"] + ".png")
        if target.exists():
            print("have", target.name, flush=True)
            continue
        g = graph(spec, job, spec.get("prefix", "KS-Chief-CutIns-20261018/dust-shark/") + job["name"])
        (out / (job["name"] + "-workflow.json")).write_text(json.dumps(g, indent=2), encoding="utf-8")
        pid = api("/prompt", {"prompt": g, "client_id": "ks-dust-shark"})["prompt_id"]
        start = time.monotonic()
        while True:
            if time.monotonic() - start > 1800:
                raise TimeoutError("job " + pid + " still running")
            entry = api("/history/" + pid).get(pid)
            if entry and entry.get("status", {}).get("status_str") == "error":
                raise RuntimeError(str(entry["status"].get("messages", [])[-1:]))
            if entry and entry.get("status", {}).get("completed"):
                for o in entry.get("outputs", {}).values():
                    for im in o.get("images", []):
                        shutil.copyfile(COMFY_OUTPUT / im.get("subfolder", "") / im["filename"], target)
                break
            time.sleep(3)
        print("made %s in %d s" % (target.name, time.monotonic() - start), flush=True)


if __name__ == "__main__":
    main()
