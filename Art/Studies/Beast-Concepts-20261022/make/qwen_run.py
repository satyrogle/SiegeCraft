"""Queue Qwen Image 2.1 Turbo text to image jobs on ArtLab and copy the outputs.

The graph is ComfyUI's own template image_qwen_image_2_1_turbo_t2i (templates 0.11.94) without its prompt enhancer:
8 steps on its manual sigmas, euler, cfg 1, the QwenImage21Cache patch on the model.

python -I qwen_run.py <jobs.json>
jobs.json: {"out": abs folder, "text_encoder": file, "style": str, "jobs": [{"name", "prompt", "seed", "width", "height"}]}
"""
import json
import pathlib
import shutil
import sys
import time
import urllib.request

SERVER = "http://127.0.0.1:8191"
COMFY_OUTPUT = pathlib.Path("C:/Users/jacob/ComfyUI-Shared/output/ArtLab")
SIGMAS = "1.0, 0.978453, 0.95418, 0.926626, 0.89508, 0.845148, 0.704534, 0.414568, 0"


def api(path, body=None):
    req = urllib.request.Request(SERVER + path, data=None if body is None else json.dumps(body).encode(),
                                 headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=60) as r:
        return json.load(r)


def graph(spec, job, prefix):
    prompt = job["prompt"] + " " + spec.get("style", "")
    return {
        "1": {"class_type": "UNETLoader", "inputs": {"unet_name": "qwen_image_2.1_turbo_int8_convrot.safetensors", "weight_dtype": "default"}},
        "2": {"class_type": "QwenImage21Cache", "inputs": {"model": ["1", 0], "device": "auto", "dtype": "default"}},
        "3": {"class_type": "CLIPLoader", "inputs": {"clip_name": spec["text_encoder"], "type": "qwen_image", "device": "default"}},
        "4": {"class_type": "TextEncodeQwenImage21", "inputs": {"clip": ["3", 0], "prompt": prompt, "negative_prompt": "", "resolution": 1024}},
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


def main():
    spec = json.loads(pathlib.Path(sys.argv[1]).read_text(encoding="utf-8"))
    only = sys.argv[2:]
    out = pathlib.Path(spec["out"])
    out.mkdir(parents=True, exist_ok=True)
    for job in spec["jobs"]:
        if only and job["name"] not in only:
            continue
        target = out / (job["name"] + ".png")
        if target.exists():
            print("have", target.name, flush=True)
            continue
        g = graph(spec, job, "KS-Beast-Concepts-20261022/" + job["name"])
        (out / (job["name"] + "-workflow.json")).write_text(json.dumps(g, indent=2), encoding="utf-8")
        pid = api("/prompt", {"prompt": g, "client_id": "ks-beast-concepts"})["prompt_id"]
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
