"""The beasts the full Dust Shark way: design pictures on Qwen Image 2.1, pixel art on FLUX.2 Klein with the pixel
sprite LoRA (the Dust Shark's graph, with or without a reference picture), queued all at once on ArtLab, one model
after the other so ArtLab swaps models only once.

python -I dir_run.py <jobs.json> [names]
jobs.json: {"out": abs folder, "flux_negative": str, "cfg": float, "steps": int,
            "jobs": [{"name", "model": "qwen" or "flux", "prompt", "seed", "width", "height", "ref": optional picture}]}
"""
import json
import pathlib
import shutil
import sys
import time
import urllib.request
import uuid

SERVER = "http://127.0.0.1:8191"
COMFY_OUTPUT = pathlib.Path("C:/Users/jacob/ComfyUI-Shared/output/ArtLab")
FLUX = pathlib.Path("C:/Users/jacob/KindredSiege-Clean/Art/Studies/Comfy-Cult-Focus-20261014/workflow-api-flux.json")
SIGMAS = "1.0, 0.978453, 0.95418, 0.926626, 0.89508, 0.845148, 0.704534, 0.414568, 0"
PREFIX = "KS-Beast-Concepts-20261022/directions/"


def api(path, body=None):
    req = urllib.request.Request(SERVER + path, data=None if body is None else json.dumps(body).encode(),
                                 headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=60) as r:
        return json.load(r)


def upload(path):
    """Send a reference picture to ArtLab's input folder and return the name LoadImage wants."""
    p = pathlib.Path(path)
    b = uuid.uuid4().hex
    head = ("--%s\r\nContent-Disposition: form-data; name=\"subfolder\"\r\n\r\nks-beasts-20261023\r\n"
            "--%s\r\nContent-Disposition: form-data; name=\"overwrite\"\r\n\r\ntrue\r\n"
            "--%s\r\nContent-Disposition: form-data; name=\"image\"; filename=\"%s\"\r\n"
            "Content-Type: image/png\r\n\r\n" % (b, b, b, p.name)).encode()
    body = head + p.read_bytes() + ("\r\n--%s--\r\n" % b).encode()
    req = urllib.request.Request(SERVER + "/upload/image", data=body,
                                 headers={"Content-Type": "multipart/form-data; boundary=" + b})
    with urllib.request.urlopen(req, timeout=60) as r:
        o = json.load(r)
    return o["subfolder"] + "/" + o["name"] if o.get("subfolder") else o["name"]


def qwen(job, prefix):
    return {
        "1": {"class_type": "UNETLoader", "inputs": {"unet_name": "qwen_image_2.1_turbo_int8_convrot.safetensors", "weight_dtype": "default"}},
        "2": {"class_type": "QwenImage21Cache", "inputs": {"model": ["1", 0], "device": "auto", "dtype": "default"}},
        "3": {"class_type": "CLIPLoader", "inputs": {"clip_name": "qwen3vl_8b_fp8_scaled.safetensors", "type": "qwen_image", "device": "default"}},
        "4": {"class_type": "TextEncodeQwenImage21", "inputs": {"clip": ["3", 0], "prompt": job["prompt"], "negative_prompt": "", "resolution": 1024}},
        "5": {"class_type": "VAELoader", "inputs": {"vae_name": "qwen_image_2.1_vae_bf16.safetensors"}},
        "6": {"class_type": "EmptyLatentImage", "inputs": {"width": job["width"], "height": job["height"], "batch_size": 1}},
        "7": {"class_type": "RandomNoise", "inputs": {"noise_seed": job["seed"]}},
        "8": {"class_type": "CFGGuider", "inputs": {"model": ["2", 0], "positive": ["4", 0], "negative": ["4", 1], "cfg": 1.0}},
        "9": {"class_type": "KSamplerSelect", "inputs": {"sampler_name": "euler"}},
        "10": {"class_type": "ManualSigmas", "inputs": {"sigmas": SIGMAS}},
        "11": {"class_type": "SamplerCustomAdvanced", "inputs": {"noise": ["7", 0], "guider": ["8", 0], "sampler": ["9", 0],
                                                                 "sigmas": ["10", 0], "latent_image": ["6", 0]}},
        "12": {"class_type": "VAEDecode", "inputs": {"samples": ["11", 0], "vae": ["5", 0]}},
        "13": {"class_type": "SaveImage", "inputs": {"images": ["12", 0], "filename_prefix": prefix}},
    }


def flux(spec, job, prefix):
    g = json.loads(FLUX.read_text(encoding="utf-8"))
    g["5"]["inputs"]["text"] = job["prompt"]
    g["6"]["inputs"]["text"] = spec.get("flux_negative", "")
    if job.get("ref"):
        g["7"]["inputs"]["image"] = upload(job["ref"])
        g["8"]["inputs"].update(width=job["width"], height=job["height"])
    else:
        for k in ("7", "8", "9", "10", "11"):
            g.pop(k)
        g["13"]["inputs"]["positive"] = ["5", 0]
        g["13"]["inputs"]["negative"] = ["6", 0]
    g["13"]["inputs"]["cfg"] = spec.get("cfg", 5.0)
    g["12"]["inputs"].update(width=job["width"], height=job["height"])
    g["15"]["inputs"].update(width=job["width"], height=job["height"], steps=spec.get("steps", 24))
    g["16"]["inputs"]["noise_seed"] = job["seed"]
    g["19"]["inputs"]["filename_prefix"] = prefix
    return g


def main():
    spec = json.loads(pathlib.Path(sys.argv[1]).read_text(encoding="utf-8"))
    only = sys.argv[2:]
    out = pathlib.Path(spec["out"])
    out.mkdir(parents=True, exist_ok=True)
    left = {}
    for job in sorted(spec["jobs"], key=lambda j: j["model"] != "qwen"):
        target = out / (job["name"] + ".png")
        if (only and job["name"] not in only) or target.exists():
            continue
        prefix = PREFIX + job["name"]
        g = qwen(job, prefix) if job["model"] == "qwen" else flux(spec, job, prefix)
        (out / (job["name"] + "-workflow.json")).write_text(json.dumps(g, indent=2), encoding="utf-8")
        left[job["name"]] = (api("/prompt", {"prompt": g, "client_id": "ks-beast-directions"})["prompt_id"], target)
    print("queued", list(left), flush=True)
    start = time.monotonic()
    while left and time.monotonic() - start < 3000:
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
