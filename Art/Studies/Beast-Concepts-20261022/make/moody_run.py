"""The six beast ideas again, moody, on Krea 2 Turbo and on Qwen Image 2.1 Turbo with the same prompt and seed, so the
two models can be judged side by side. Every Krea 2 job is queued before every Qwen job, so ArtLab swaps models once.

Krea 2's graph is ComfyUI's template image_krea2_turbo_t2i_int8 without its LoRA and prompt enhancer: 8 steps, cfg 1,
euler, simple, the negative zeroed. Qwen's is qwen_run.graph.

python -I moody_run.py moody-jobs.json
"""
import json
import pathlib
import shutil
import sys
import time

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
import qwen_run as q  # noqa: E402

PREFIX = "KS-Beast-Concepts-20261022/moody/"
NAMES = ["shark-1", "shark-2", "shark-3", "bear-1", "bear-2", "bear-3"]


def prompt_for(spec, name):
    kind = name.split("-")[0]
    return " ".join([spec[kind], spec["ideas"][name], spec[kind + "_scene"], spec["style"]])


def krea_graph(prompt, seed, width, height, prefix):
    return {
        "1": {"class_type": "UNETLoader", "inputs": {"unet_name": "krea2_turbo_int8_convrot.safetensors", "weight_dtype": "default"}},
        "2": {"class_type": "CLIPLoader", "inputs": {"clip_name": "qwen3vl_4b_fp8_scaled.safetensors", "type": "krea2", "device": "default"}},
        "3": {"class_type": "VAELoader", "inputs": {"vae_name": "qwen_image_vae.safetensors"}},
        "4": {"class_type": "CLIPTextEncode", "inputs": {"clip": ["2", 0], "text": prompt}},
        "5": {"class_type": "ConditioningZeroOut", "inputs": {"conditioning": ["4", 0]}},
        "6": {"class_type": "EmptyLatentImage", "inputs": {"width": width, "height": height, "batch_size": 1}},
        "7": {"class_type": "KSampler", "inputs": {"model": ["1", 0], "positive": ["4", 0], "negative": ["5", 0], "latent_image": ["6", 0],
                                                   "seed": seed, "steps": 8, "cfg": 1.0, "sampler_name": "euler", "scheduler": "simple",
                                                   "denoise": 1.0}},
        "8": {"class_type": "VAEDecode", "inputs": {"samples": ["7", 0], "vae": ["3", 0]}},
        "9": {"class_type": "SaveImage", "inputs": {"images": ["8", 0], "filename_prefix": prefix}},
    }


def main():
    spec = json.loads(pathlib.Path(sys.argv[1]).read_text(encoding="utf-8"))
    out = pathlib.Path(spec["out"])
    out.mkdir(parents=True, exist_ok=True)
    w, h = spec["width"], spec["height"]
    left = {}
    for model in ("krea", "qwen"):
        for name in NAMES:
            label = model + "-" + name
            target = out / (label + ".png")
            if target.exists():
                print("have", label, flush=True)
                continue
            seed = spec[name.split("-")[0] + "_seed"]
            prompt = prompt_for(spec, name)
            if model == "krea":
                g = krea_graph(prompt, seed, w, h, PREFIX + label)
            else:
                g = q.graph({"text_encoder": "qwen3vl_8b_fp8_scaled.safetensors", "style": ""},
                            {"prompt": prompt, "seed": seed, "width": w, "height": h}, PREFIX + label)
            (out / (label + "-workflow.json")).write_text(json.dumps(g, indent=2), encoding="utf-8")
            left[label] = (q.api("/prompt", {"prompt": g, "client_id": "ks-beast-concepts"})["prompt_id"], target)
    print("queued", list(left), flush=True)
    start = time.monotonic()
    while left and time.monotonic() - start < 5400:
        for label, (pid, target) in list(left.items()):
            entry = q.api("/history/" + pid).get(pid)
            if entry and entry.get("status", {}).get("status_str") == "error":
                print("ERROR", label, json.dumps(entry["status"].get("messages", [])[-1:])[:400], flush=True)
                left.pop(label)
            elif entry and entry.get("status", {}).get("completed"):
                for o in entry.get("outputs", {}).values():
                    for im in o.get("images", []):
                        shutil.copyfile(q.COMFY_OUTPUT / im.get("subfolder", "") / im["filename"], target)
                print("made %s at %d s" % (label, time.monotonic() - start), flush=True)
                left.pop(label)
        time.sleep(5)
    print("still waiting on", list(left) if left else "nothing", flush=True)


if __name__ == "__main__":
    main()
