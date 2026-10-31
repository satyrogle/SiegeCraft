# Local open weight AI models for game SFX and music (RTX 3060 12 GB, Windows, ComfyUI), as of October 2026

Research date: 13 Oct 2026. Reddit could not be fetched by the research tool (blocked), so community opinion is thin. No source benchmarked any model on an RTX 3060; VRAM and speed figures below come from other cards and are labelled.

## Which models are current and good as of late 2026, and what are their specs?

### Takeaway
The current local stack is **Stable Audio 3** (released 20 May 2026; Small SFX, Small Music and Medium are open weight, native in ComfyUI) for SFX, ambience and instrumental loops, and **ACE-Step 1.5** (Jan 2026, MIT, plus a 4B XL in April 2026, native in ComfyUI) for full music and songs with vocals. Most 2023 to 2025 models (MusicGen, MAGNeT, AudioLDM 2, Tango 2, Stable Audio Open 1.0 and Small, ACE-Step v1) are now superseded, and several of the remaining video to audio and song models carry non commercial or territory restricted licences.

### Cited Findings

**Stable Audio 3 family (Stability AI, released 20 May 2026). CURRENT.**
- Four models: Small SFX, Small (music), Medium, Large. Small SFX, Small and Medium are open weights on Hugging Face; Large is API or enterprise self hosting only. [Stability AI announcement](https://stability.ai/news-updates/meet-stable-audio-3-the-model-family-built-for-artistic-experimentation-with-open-weight-models)
- Official repo specs: Small-Music and Small-SFX 433M params, max 120 s; Medium 1.4B params, max 380 s (6:20), needs CUDA GPU; Large 2.7B, API only. 44.1 kHz stereo. [Stability-AI/stable-audio-3 GitHub](https://github.com/Stability-AI/stable-audio-3). Param count conflicts slightly: Stability/fal and press say 459M for the Small models ([the-decoder](https://the-decoder.com/stability-ai-launches-stable-audio-3-0-with-up-to-six-minute-tracks-and-open-weights/); [fal.ai](https://fal.ai/models/fal-ai/stable-audio-3/small/sfx/text-to-audio)), and the HF card shows about 0.6B including the text encoder ([HF small-sfx card](https://huggingface.co/stabilityai/stable-audio-3-small-sfx)).
- Peak VRAM measured on an H200: Medium 5.07 GB (5 s clip) to 6.52 GB (380 s clip); Small 1.69 GB to 2.40 GB. Chunked decoding lowers Medium at 120 s to about 5.14 GB. Speed on H200: Medium 380 s clip in 1.31 s; Small 120 s clip in 0.45 s. No consumer GPU figures in the repo. [Stability-AI/stable-audio-3 GitHub](https://github.com/Stability-AI/stable-audio-3)
- Consumer card report (Windows tool "local-sounds", bake-off Oct 2026 on RTX 5080): Small SFX about 0.5 s per take at 8 steps, about 2.5 GB VRAM (about 4 GB with the checker models loaded), 3.3 GB download; Medium about 0.43 s per take, needs about 6 GB on top of Small and about 10 GB free while loading. [vl3c/local-sounds](https://github.com/vl3c/local-sounds)
- Official repo says the Small models run "CPU only" in its own runtime (TFLite); ComfyUI and local-sounds run them on GPU, so this is a runtime choice, not a model limit. [Stability-AI/stable-audio-3 GitHub](https://github.com/Stability-AI/stable-audio-3); [Comfy blog](https://blog.comfy.org/p/stable-audio-3-day-0-support)
- Features: per second variable length, single and multi segment inpainting, continuation past the end of a clip, audio to audio editing, LoRA training docs for Small and Medium. [Stability AI announcement](https://stability.ai/news-updates/meet-stable-audio-3-the-model-family-built-for-artistic-experimentation-with-open-weight-models); [GitHub](https://github.com/Stability-AI/stable-audio-3)
- Training data: Stability says all SA3 models are trained on fully licensed data; the arXiv paper says Small SFX was trained on a higher quality subset of Freesound, and that separating SFX from music improved perceptual quality in both. [Stability AI announcement](https://stability.ai/news-updates/meet-stable-audio-3-the-model-family-built-for-artistic-experimentation-with-open-weight-models); [arXiv 2605.17991](https://arxiv.org/html/2605.17991v1)
- Licence: Stability AI Community License, gated on HF (accept terms, share contact info); the gate also references Gemma Terms of Use for the bundled T5Gemma text encoder, including use restrictions in Section 3.2. [HF small-sfx card](https://huggingface.co/stabilityai/stable-audio-3-small-sfx)

**Stable Audio Open 1.0 (July 2024). SUPERSEDED by SA3 for most uses.**
- 1B params, up to 47 s stereo, 44.1 kHz. Cannot generate realistic vocals; better at SFX and field recordings than music; English prompts. Trained on 486,492 recordings (472,618 Freesound, 13,874 Free Music Archive) under CC0, CC BY or CC Sampling+. Licence: Stability AI Community License. [HF stable-audio-open-1.0](https://huggingface.co/stabilityai/stable-audio-open-1.0)
- One hands on ComfyUI user judged SAO 1.0 output quality and "mix" prompt following better than SA3 Small SFX, but SA3 Small SFX about 50x faster. [jurn.link guide, 22 May 2026](https://jurn.link/dazposer/index.php/2026/05/22/how-to-install-stable-audio-3-0-small-sfx-safetensors-and-run-it-in-comfyui/)

**Stable Audio Open Small (May 2025). SUPERSEDED by SA3 Small SFX.**
- 0.5B params, max 11 s stereo, 44.1 kHz, optimised for Arm CPUs. Same limitations as SAO 1.0 (no vocals, better at SFX than music). Stability AI Community License. [HF stable-audio-open-small](https://huggingface.co/stabilityai/stable-audio-open-small)

**ACE-Step 1.5 (ACE Studio and StepFun, Jan 2026) and 1.5 XL (2 Apr 2026). CURRENT, best local music option.**
- Variants: 2B DiT (base, sft, turbo; turbo uses 8 steps, base/sft 50); XL 4B DiT (xl-base, xl-sft, xl-turbo, released 2026-04-02); planner LMs 0.6B, 1.7B, 4B (Qwen3 based). No v2 announced. [ace-step/ACE-Step-1.5 GitHub](https://github.com/ace-step/ACE-Step-1.5)
- VRAM tiers from the repo: 6 GB or less: 2B turbo, no LM, INT8, full CPU offload. 8 to 16 GB: 2B turbo/sft with 0.6B LM (8 to 12 GB) or 1.7B LM (12 to 16 GB). At 12 GB specifically: 2B models work with the 1.7B LM; XL needs offload plus quantization (XL weights about 9 GB vs about 4.7 GB for 2B). 16 to 20 GB: XL turbo needs CPU offload below 20 GB. [ACE-Step-1.5 GitHub](https://github.com/ace-step/ACE-Step-1.5). Third party guide: XL 4B needs at least 12 GB with offload, 20 GB recommended. [hysenlabs summary via search](https://hysenlabs.com/projects/ace-step-ace-step-1-5)
- Speed (developer figures): under 2 s per full song on A100, under 10 s on RTX 3090. Duration 10 s to 10 min; batch up to 8. Lyrics in 50+ languages. Features: cover, repaint/edit, reference audio, track separation, multi track layering ("lego"), Vocal2BGM, metadata control of duration, BPM, key, time signature. [ACE-Step-1.5 GitHub](https://github.com/ace-step/ACE-Step-1.5); [ComfyUI docs](https://docs.comfy.org/tutorials/audio/ace-step/ace-step-v1-5.md)
- XL in ComfyUI: xl-base for versatility, xl-sft for peak quality, xl-turbo (8 steps, no CFG) about 6x faster than base/sft; sample tracks include vocal and instrumental. [Comfy blog, 17 Apr 2026](https://blog.comfy.org/p/ace-step-15-xl-commercial-grade-music)
- LoRA: repo cites 8 songs in about 1 hour on "RTX 3090 (12GB)" (note: a 3090 has 24 GB, so this figure is internally inconsistent); a third party says LoRA training needs 24 GB+. [ACE-Step-1.5 GitHub](https://github.com/ace-step/ACE-Step-1.5); [therundown search summary](https://www.therundown.ai/tools/ace-step-1-5)
- Licence: weights MIT ("License: mit"); model card says "You can strictly use the generated music for commercial purposes." Training data claimed as professionally licensed tracks, public domain and royalty free music, and synthetic MIDI to audio; not independently verified. [HF ACE-Step/Ace-Step1.5](https://huggingface.co/ACE-Step/Ace-Step1.5). Code MIT. [GitHub](https://github.com/ace-step/ACE-Step-1.5)
- Windows: .bat launchers and a portable package. [GitHub](https://github.com/ace-step/ACE-Step-1.5)
- Quality claim (self reported): between Suno v4.5 and Suno v5. [arXiv 2602.00744](https://arxiv.org/html/2602.00744v1)

**ACE-Step v1 (3.5B, 2025). SUPERSEDED by 1.5.**
- ComfyUI native, file `ace_step_v1_3.5b.safetensors` in `models/checkpoints`; Apache-2.0, free for commercial use per ComfyUI docs. [ComfyUI docs ACE-Step v1](https://docs.comfy.org/tutorials/audio/ace-step/ace-step-v1)

**HeartMuLa (2026). CURRENT, songs with lyrics.**
- HeartMuLa-oss-3B releases 14 Jan, 23 Jan (RL version) and 13 Feb 2026 ("happy-new-year", recommended); 7B is internal only, not released. Licence for the repo "and all related model weights" changed to Apache 2.0 on 20 Jan 2026. Default max length 240 s (4 min); lyrics with [Verse]/[Chorus] tags, "almost all languages". MuLaCover (cover/remix) released 16 Sep 2026. Community ComfyUI node "HeartMuLa_ComfyUI" by Benji. No official VRAM figure; suggests `--lazy_load true` on one GPU. [HeartMuLa/heartlib GitHub](https://github.com/HeartMuLa/heartlib)
- Third party docs: 8 GB minimum with lazy load, 3B peak about 6.2 GB, 16 GB+ recommended; pinned dependency conflicts reported. [Hermes agent HeartMuLa skill](https://hermes-agent.nousresearch.com/docs/user-guide/skills/bundled/media/media-heartmula)

**YuE (M-A-P/HKUST, 2025) and YuE2-3B (Sept 2026).**
- Original YuE weights relicensed to Apache 2.0; maintainers encourage commercial use of outputs and ask for attribution "YuE by HKUST/M-A-P". [YuE HF README via search](https://huggingface.co/m-a-p/YuE-s1-7B-anneal-en-cot/blob/main/README.md)
- YuE2-3B: weights **CC BY-NC 4.0** (non commercial). Linux, "24GB NVIDIA GPU with BF16 support" listed, 24 GB host RAM; measured peak about 11 GiB on RTX 4090 (14.08 GiB at max context). 3.6 min song in 71 s on RTX 4090. English and Mandarin. Full songs with vocals. [HF m-a-p/YuE2-3B](https://huggingface.co/m-a-p/YuE2-3B)

**DiffRhythm (ASLP lab, 2025).**
- Code and DiT weights Apache 2.0 (since 2025-03-07). Base 1m35s, full 4m45s (285 s); v1.2 on 2025-05-09. Minimum 8 GB VRAM for base with `--chunked`. Windows .bat scripts, needs espeak-ng env vars. Lyrics via LRC files. DiffRhythm 2 exists (no specs found). [ASLP-lab/DiffRhythm GitHub](https://github.com/ASLP-lab/DiffRhythm)

**SongBloom (Tencent AI Lab, NeurIPS 2025).**
- In its own paper SongBloom-full beats DiffRhythm-full and YuE-7B on several objective metrics (self reported; DiffRhythm-full used a 10 s audio prompt, YuE a 30 s prompt). [SongBloom arXiv 2506.07634](https://arxiv.org/pdf/2506.07634)

**SongGeneration / LeVo / LeVo 2 (Tencent).**
- Licence conflicting: a HF Space copy of the licence says "only for academic, research and education purposes ... refrain from using it for any commercial or production purposes"; a LeVo2 GGUF repackager repeats research only terms; a HF model card PR claims Apache-2.0; a blog says LeVo 2 (1 Mar 2026) is commercial grade and free. [HF Space commit](https://huggingface.co/spaces/smartdigitalnetworks/SongGeneration/commit/93f7efbca01ea0302a1dcfa4b1bff2e81bd84a9f); [LeVo2-GGUF](https://huggingface.co/ckadirt/LeVo2-GGUF); [lglg666 PR](https://huggingface.co/lglg666/SongGeneration-base/commit/a4babc256c5e50ff59291aff222602640bd5606e); [gaga.art blog](https://gaga.art/blog/songgeneration-2/)

**MusicGen, AudioGen, MAGNeT, JASCO, MusicGen Style (Meta AudioCraft, 2023 to 2024). SUPERSEDED and non commercial.**
- "The models weights in this repository are released under the CC-BY-NC 4.0 license"; code MIT. Covers MusicGen, AudioGen, MAGNeT, JASCO, MusicGen Style. [facebookresearch/audiocraft](https://github.com/facebookresearch/audiocraft)
- MusicGen large 3.3B (also 300M, 1.5B), mono 32 kHz, no realistic vocals, sometimes collapses to silence at the end; research use. Trained on licensed Meta, Shutterstock, Pond5 music. [HF musicgen-large](https://huggingface.co/facebook/musicgen-large)

**AudioLDM 2 (2023) and Tango 2 / TangoFlux (2024). SUPERSEDED and non commercial.**
- AudioLDM 2 weights cc-by-nc-sa-4.0; variants audioldm2 (1.1B), -large (1.5B), -music; examples output 16 kHz. [HF cvssp/audioldm2](https://huggingface.co/cvssp/audioldm2)
- Tango 2 weights cc-by-nc-sa-4.0, 16 kHz. [HF declare-lab/tango2](https://huggingface.co/declare-lab/tango2)
- TangoFlux: non commercial research only; subject to Stability AI Community License plus WavCaps academic only data terms. [starlog summary](https://starlog.is/articles/developer-tools/declare-lab-tangoflux); [PromptLayer](https://www.promptlayer.com/models/tangoflux)

**MMAudio (2024). Video/text to audio, non commercial weights.**
- Code MIT; weights CC-BY-NC 4.0, "not guaranteed for commercial use". About 6 GB VRAM in 16 bit. Default 8 s clip; other lengths may lose quality. Text to audio works without video. Tested on Ubuntu only. Default model `large_44k_v2`. [hkchengrex/MMAudio](https://github.com/hkchengrex/MMAudio)

**HunyuanVideo-Foley (Tencent, 28 Aug 2025; XL 29 Sep 2025). Video to audio, territory restricted.**
- 48 kHz output; XL has offload inference to cut VRAM; usage needs a video file. No VRAM figure published. [HF tencent/HunyuanVideo-Foley](https://huggingface.co/tencent/HunyuanVideo-Foley)
- Licence: TENCENT HUNYUAN COMMUNITY LICENSE AGREEMENT. Territory = "the worldwide territory, excluding the territory of the European Union, United Kingdom and South Korea." Licence needed above 100 million MAU. "Tencent claims no rights in Outputs You generate." Outputs must not be used "to improve any other AI model". [LICENSE](https://github.com/Tencent-Hunyuan/HunyuanVideo-Foley/blob/main/LICENSE)

**ThinkSound and PrismAudio (Alibaba FunAudioLLM, 2025 to 2026). Research only.**
- Code and most models Apache 2.0, but the project states code, models and dataset are for research and educational purposes only and commercial use is not permitted; bundled Stable Audio Open VAE is under Stability Community License. Any2Audio (video, text, audio). Windows `setup_windows.bat`. PrismAudio (video to audio, ICLR 2026) released on the `prismaudio` branch 2026-03-24. [FunAudioLLM/ThinkSound](https://github.com/FunAudioLLM/ThinkSound)

**Woosh (Sony AI, April 2026). SFX foundation model, non commercial.**
- Models: Woosh-Flow (text to audio), Woosh-DFlow (distilled T2A), Woosh-VFlow and DVFlow (video to audio). "The open weights in the releases page are released under the CC-BY-NC license"; code mostly MIT. [SonyResearch/Woosh](https://github.com/SonyResearch/Woosh)
- Authors claim competitive or better results than Stable Audio Open and TangoFlux (self reported). Sony also has a private model trained on Pro Sound Effects and BOOM libraries, available to license. [arXiv 2604.01929](https://arxiv.org/abs/2604.01929); [Sony AI blog](https://ai.sony/blog/introducing-woosh-sony-ais-sound-effect-foundation-model)

**MOSS-SoundEffect v2.0 (OpenMOSS, 26 May 2026).**
- 1.3B DiT, flow matching, 48 kHz, up to 30 s, bilingual prompts (v1 was an 8B MossTTSDelay model). Family wide statement: "Models in MOSS-TTS Family are licensed under the Apache License 2.0". [OpenMOSS/MOSS-TTS](https://github.com/OpenMOSS/MOSS-TTS)
- In the local-sounds bake-off on RTX 5080 it took 13.3 s per take (vs about 0.5 s for SA3 Small SFX) and was removed as an engine in Oct 2026. [vl3c/local-sounds](https://github.com/vl3c/local-sounds)

**Others seen but not verified.**
- AudioX (2025, inpainting, video regions) and AudioX-Turbo paper (arXiv 2606.12555). [AIWG wiki](https://aiwg.miraheze.org/wiki/Open_Source_-_Running_Models_Locally); [arXiv 2606.12555](https://arxiv.org/pdf/2606.12555)
- Stable Audio 2.5 is listed on the AIWG "open source" page, but that page does not confirm open weights. [AIWG wiki, updated 8 Apr 2026](https://aiwg.miraheze.org/wiki/Open_Source_-_Running_Models_Locally)

### Inferences
- On a 12 GB RTX 3060: SA3 Small SFX (about 2.5 GB) and SA3 Medium (about 5 to 6.5 GB peak on H200) should both fit; ACE-Step 1.5 2B turbo with 1.7B LM is the repo's own recommended 12 GB config; ACE-Step XL and YuE2 are at the edge (offload/quantization needed). LIKELY.
- Speed on a 3060 is not measured anywhere I found. A 3060 has roughly a third of a 3090's compute, so ACE-Step full songs probably take tens of seconds rather than under 10 s, and SA3 Small takes a few seconds per clip. GUESS.
- Superseded: MusicGen, MAGNeT, AudioGen, AudioLDM 2, Tango 2, TangoFlux, SAO Small, ACE-Step v1. SAO 1.0 is still worth keeping as a slower, possibly higher quality SFX fallback.

### Gaps
- No RTX 3060 or 12 GB benchmark for any model.
- SongBloom licence: GitHub and HF pages returned 404/401; not verified.
- SongGeneration/LeVo 2 licence: official LICENSE could not be fetched; sources conflict.
- Kling-Foley: not checked; unknown whether weights are open.
- AudioX/AudioX-Turbo licence and VRAM: not checked.
- DiffRhythm 2 specs and licence: not checked.
- HunyuanVideo-Foley VRAM on 12 GB: no figure found.

## Which are best for short punchy SFX vs loopable background music vs ambience?

### Takeaway
SFX and ambience: SA3 Small SFX first (fast, 120 s max, GPU light, commercial under $1M), SAO 1.0 as a slower fallback, SA3 Medium "SFX"/"One-shot" category for higher quality. Music loops and BGM: SA3 Small Music or Medium for instrumental loops with BPM in the prompt, ACE-Step 1.5 for structured tracks, BPM/key control and vocals. Nothing found offers a guaranteed seamless loop; looping needs editing or inpainting.

### Cited Findings
- SA3 ComfyUI categories: Music, Instrument, SFX, One-shot. Small-SFX is for "sound effects and short ambiance", Small-Music for "short music loops". [ComfyUI SA3 tutorial](https://docs.comfy.org/tutorials/audio/stable-audio/stable-audio-3)
- Comfy's examples: SFX category "Impacts, textures, environments, and motion sounds for pictures and games" (gravel footsteps 8 s, car whoosh with Doppler 3 s); One-shots for drums, UI, transitions (3 s examples); instrument loops with BPM (muted guitar strum loop 8 s at 100 BPM, pluck loop 10 s at 128 BPM); full tracks (lo-fi 150 s at 75 BPM, synthwave 180 s at 110 BPM). [Comfy blog SA3 day 0](https://blog.comfy.org/p/stable-audio-3-day-0-support)
- SA3 supports continuation and multi segment inpainting, set by mask start and end in seconds. [Stability-AI/stable-audio-3](https://github.com/Stability-AI/stable-audio-3)
- ACE-Step 1.5 runs from 10 s loops to 10 min tracks, with BPM, key and time signature control, repaint and multi track layering. [Comfy blog XL](https://blog.comfy.org/p/ace-step-15-xl-commercial-grade-music); [ACE-Step-1.5 GitHub](https://github.com/ace-step/ACE-Step-1.5)
- ACE-Step is a music model and not built for SFX. [search summary of ACE-Step-1.5 repo](https://github.com/ace-step/ACE-Step-1.5)
- SAO 1.0 and SAO Small are better at SFX and field recordings than music and cannot do vocals. [HF SAO 1.0](https://huggingface.co/stabilityai/stable-audio-open-1.0)
- Video to audio models (MMAudio, HunyuanVideo-Foley, ThinkSound/PrismAudio, Woosh-VFlow) suit Foley synced to footage; AIWG lists MMAudio for "footsteps and impacts for silent video". [AIWG wiki](https://aiwg.miraheze.org/wiki/Open_Source_-_Running_Models_Locally)
- Practical SFX problems from a game engine vendor: generated clips often have a few ms of silence or a slow attack at the start (audible on jumps and hits, fix by trimming); use WAV not MP3 for instant playback; gain must be balanced in engine; first results are rarely the best. [Summer Engine blog, 6 Jun 2026](https://www.summerengine.com/blog/ai-sound-effect-generator-for-games)
- AudioCraft-era test: ambient scenes easier than sharp, frame specific impacts. [Pixverse listicle, via search](https://pixverse.ai/en/blog/best-ai-sound-effect-generator)

### Inferences
- For a pixel art roguelike: hits, spells, UI clicks, footsteps → SA3 Small SFX (fast enough to batch dozens of takes and pick), then trim the attack. Ambience beds → SA3 Small SFX or Medium at 30 to 120 s. Combat/menu loops → SA3 Small Music or ACE-Step 1.5 turbo with explicit BPM, then crossfade or inpaint the seam. LIKELY.
- Video to audio models are a poor fit for a 2.5D game with no rendered footage per sound, and every one found is non commercial or territory restricted. LIKELY.

### Gaps
- No source tested seamless loop output from any model.
- No independent listening test of SA3 Small SFX vs SAO 1.0 vs Woosh vs MOSS for short game SFX.

## Which have native ComfyUI support, and what model files go where?

### Takeaway
Native in ComfyUI core: Stable Audio 3 (needs ComfyUI v0.22.0+), ACE-Step 1.5 and 1.5 XL, ACE-Step v1, Stable Audio Open 1.0. Custom nodes only: MMAudio (kijai), HunyuanVideo-Foley (several wrappers), HeartMuLa (community). No ComfyUI nodes found for YuE2, Woosh, MOSS-SoundEffect, ThinkSound.

### Cited Findings
- **SA3 (native, v0.22.0+):** templates under Template → Audio → Stable Audio 3.0 (`audio_stable_audio_3_medium.json`, `audio_stable_audio_3_medium_base.json`). [Comfy blog](https://blog.comfy.org/p/stable-audio-3-day-0-support)
  - `models/checkpoints/`: `stable_audio_3_medium.safetensors`, `stable_audio_3_medium_base.safetensors` from [Comfy-Org/stable-audio-3](https://huggingface.co/Comfy-Org/stable-audio-3). `models/text_encoders/`: `t5gemma_b_b_ul2.safetensors` (required for all SA3), and `qwen3.5_2b_bf16.safetensors` (only for the Medium "reprompt" workflow) from Comfy-Org/Qwen3.5. [ComfyUI SA3 tutorial](https://docs.comfy.org/tutorials/audio/stable-audio/stable-audio-3)
  - Small SFX: `stable_audio_3_small_sfx.safetensors` (2.3 GB) in `models/checkpoints`, `t5gemma_b_b_ul2.safetensors` (1.2 GB) in `models/text_encoders`; settings 8 steps, CFG 1.0, PingPong sampler (community node by Blepping, single file in custom_nodes). The same user says SA3 Medium needed Flash Attention 2 in their setup. [jurn.link guide](https://jurn.link/dazposer/index.php/2026/05/22/how-to-install-stable-audio-3-0-small-sfx-safetensors-and-run-it-in-comfyui/)
  - Comment reports on the Comfy blog: audio was "awful" with Sage Attention enabled; starting ComfyUI without Sage Attention fixed it (anecdotal). [Comfy blog comments](https://blog.comfy.org/p/stable-audio-3-day-0-support)
- **ACE-Step 1.5 (native):** AIO `ace_step_1.5_turbo_aio.safetensors` → `models/checkpoints/` (recommended); or split: `acestep_v1.5_turbo.safetensors` → `models/diffusion_models/`, `qwen_0.6b_ace15.safetensors` and `qwen_1.7b_ace15.safetensors` → `models/text_encoders/`, `ace_1.5_vae.safetensors` → `models/vae/`, all from [Comfy-Org/ace_step_1.5_ComfyUI_files](https://huggingface.co/Comfy-Org/ace_step_1.5_ComfyUI_files). Templates `audio_ace_step_1_5_checkpoint` and `audio_ace_step_1_5_split`. Cover and repaint listed as "Coming Soon" in ComfyUI; LoRA training supported. [ComfyUI ACE-Step 1.5 docs](https://docs.comfy.org/tutorials/audio/ace-step/ace-step-v1-5.md)
- **ACE-Step 1.5 XL (native, 17 Apr 2026):** templates `audio_ace_step1_5_xl_base.json`, `_xl_sft.json`, `_xl_turbo.json`. [Comfy blog XL](https://blog.comfy.org/p/ace-step-15-xl-commercial-grade-music). Weight file e.g. `split_files/diffusion_models/acestep_v1.5_xl_base_bf16.safetensors` in the Comfy-Org repo. [comfy.org model page](https://comfy.org/p/supported-models/acestep-v1-5-xl-base-bf16.md)
- **Community ACE-Step packs:** ComfyUI-AceStep_SFT, ACE-Step-ComfyUI (APG/ADG guidance). [RunComfy listing](https://www.runcomfy.com/comfyui-nodes/ComfyUI-AceStep_SFT)
- **ACE-Step v1 (native):** `ace_step_v1_3.5b.safetensors` → `models/checkpoints`. [ComfyUI docs v1](https://docs.comfy.org/tutorials/audio/ace-step/ace-step-v1)
- **MMAudio (custom):** kijai/ComfyUI-MMAudio (MIT code, marked WIP); models in `ComfyUI/models/mmaudio` from Kijai/MMAudio_safetensors; BigVGAN v2 auto downloads to `models/mmaudio/nvidia/bigvgan_v2_44khz_128band_512x`. [kijai/ComfyUI-MMAudio](https://github.com/kijai/ComfyUI-MMAudio). RunComfy lists last update 2026-03-26. [RunComfy](https://www.runcomfy.com/comfyui-nodes/ComfyUI-MMAudio/mm-audio-sampler)
- **HunyuanVideo-Foley (custom):** if-ai ComfyUI-HunyuanVideo-Foley (auto downloads models); Vantage-HunyuanFoley (FP8, low VRAM offload); Comfyui-HunyuanFoley (last updated 2025-08-29). [RunComfy](https://www.runcomfy.com/comfyui-nodes/ComfyUI-HunyuanVideo-Foley); [Vantage-HunyuanFoley](https://github.com/vantagewithai/Vantage-HunyuanFoley)
- **HeartMuLa (custom):** HeartMuLa_ComfyUI by Benji. [heartlib GitHub](https://github.com/HeartMuLa/heartlib)
- After placing files press R to refresh; missing nodes usually mean ComfyUI needs updating (nightly). [ComfyUI SA3 tutorial](https://docs.comfy.org/tutorials/audio/stable-audio/stable-audio-3)

### Inferences
- Jacob's existing ComfyUI must be at v0.22.0 or later for SA3; earlier installs will show missing nodes. LIKELY.
- If Sage Attention is enabled globally in his ComfyUI launch flags, that may break SA3 output. GUESS (two anecdotal comments).

### Gaps
- SAO 1.0 ComfyUI file names (believed `stable_audio_open_1.0.safetensors` + `t5_base.safetensors`) not verified this session.
- Official ComfyUI file link for SA3 Small Music not seen on the docs page.
- No confirmed 12 GB ComfyUI run of ACE-Step XL found.

## Licence traps

### Takeaway
Clean for commercial output: ACE-Step 1.5/XL (MIT), ACE-Step v1 (Apache 2.0), HeartMuLa (Apache 2.0), YuE v1 (Apache 2.0), DiffRhythm (Apache 2.0), MOSS-SoundEffect (Apache 2.0, family statement). Commercial with conditions: Stable Audio 3 / SAO (Stability Community Licence: under US$1M annual revenue, must register). Non commercial: MusicGen/AudioGen/MAGNeT/JASCO, AudioLDM 2, Tango 2, TangoFlux, MMAudio, Woosh, YuE2-3B. Research only: ThinkSound/PrismAudio, probably SongGeneration. Territory trap: HunyuanVideo-Foley licence excludes the UK, EU and South Korea.

### Cited Findings
- Stability AI Community License (last updated 5 July 2024): commercial users "must register with Stability AI"; if you or affiliates "generate more than USD $1,000,000 in annual revenue", licences "shall terminate" and you must request an Enterprise licence; "You own any outputs generated from the Models or Derivative Works to the extent permitted by applicable law"; no using the materials or outputs to "create or improve any foundational generative AI model"; Section IV(a) requires displaying "Powered by Stability AI" when distributing the materials or products using them. [Stability Community License](https://stability.ai/community-license-agreement)
- SA3 announcement: under the Community License users own and can commercialise outputs; Enterprise License for organisations over $1M annual revenue. [Stability AI](https://stability.ai/news-updates/meet-stable-audio-3-the-model-family-built-for-artistic-experimentation-with-open-weight-models)
- SA3 HF gate also references the Gemma Terms of Use (T5Gemma encoder) with Section 3.2 use restrictions. [HF small-sfx](https://huggingface.co/stabilityai/stable-audio-3-small-sfx)
- ACE-Step 1.5: MIT; commercial use of generated music stated. [HF ACE-Step1.5](https://huggingface.co/ACE-Step/Ace-Step1.5). The project also says users should still check output compliance with copyright law in their jurisdiction. [communeify summary via search](https://www.communeify.com/en/blog/ace-step-1-5-opensource-ai-music-generator-4gb-vram/)
- AudioCraft: weights "CC-BY-NC 4.0" (MusicGen, AudioGen, MAGNeT, JASCO). [audiocraft](https://github.com/facebookresearch/audiocraft)
- AudioLDM 2 and Tango 2: cc-by-nc-sa-4.0. [audioldm2](https://huggingface.co/cvssp/audioldm2); [tango2](https://huggingface.co/declare-lab/tango2)
- MMAudio weights CC-BY-NC 4.0. [MMAudio](https://github.com/hkchengrex/MMAudio)
- Woosh weights CC-BY-NC. [Woosh](https://github.com/SonyResearch/Woosh)
- YuE2-3B weights CC BY-NC 4.0, while original YuE is Apache 2.0. [YuE2-3B](https://huggingface.co/m-a-p/YuE2-3B); [YuE README](https://huggingface.co/m-a-p/YuE-s1-7B-anneal-en-cot/blob/main/README.md)
- ThinkSound/PrismAudio: research and educational only, commercial use not permitted, despite Apache 2.0 code. [ThinkSound](https://github.com/FunAudioLLM/ThinkSound)
- HunyuanVideo-Foley territory excludes "the European Union, United Kingdom and South Korea"; outputs cannot be used to improve other AI models. [Tencent licence](https://github.com/Tencent-Hunyuan/HunyuanVideo-Foley/blob/main/LICENSE)
- local-sounds summarises the Stability terms as "free for personal, research and non-commercial use", commercial allowed for orgs "under US $1M a year, after registering with Stability AI". [local-sounds](https://github.com/vl3c/local-sounds)

### Inferences
- Jacob is a Brunel MSc student and visa applicant, so likely UK based: the Tencent Hunyuan licence grants him no rights in the UK, so HunyuanVideo-Foley is out regardless of commercial terms. LIKELY.
- For a solo indie under $1M revenue, SA3 output is shippable after registering with Stability; the "Powered by Stability AI" line most likely only bites if he distributes the model or a product that runs it, not baked WAVs, but this reading is not confirmed. GUESS; worth a direct check of the licence text before launch.
- Safest zero condition music path: ACE-Step 1.5 (MIT). Safest zero condition SFX path: MOSS-SoundEffect v2.0 (Apache 2.0 family statement), but it is slow and was dropped in one bake-off. LIKELY.

### Gaps
- Whether the Stability attribution clause applies to games shipping only generated audio: not resolved.
- Exact per model licence file for MOSS-SoundEffect v2.0 not read (only the family statement).
- SongGeneration, SongBloom licences unverified.

## Real world quality reports from game devs

### Takeaway
Very little independent game dev testing exists online that the tool could reach (Reddit is blocked to the fetcher). What exists: one Windows SFX tool chose SA3 Small SFX over MOSS after an Oct 2026 bake-off, one ComfyUI user rated SAO 1.0 higher quality but 50x slower than SA3 Small SFX, and ACE-Step quality claims are mostly self reported.

### Cited Findings
- local-sounds (Windows SFX generator for animation, Oct 2026): SA3 Small SFX is the default engine; MOSS-SoundEffect v2.0 was removed after a bake-off (13.3 s per take vs 0.5 s). Tool adds CLAP prompt match scoring and Audiobox Aesthetics quality scoring. [vl3c/local-sounds](https://github.com/vl3c/local-sounds)
- ComfyUI user: SA3 Small SFX gives "reasonable" results with "blistering speed"; SAO 1.0 gives better quality and follows mix instructions better. [jurn.link](https://jurn.link/dazposer/index.php/2026/05/22/how-to-install-stable-audio-3-0-small-sfx-safetensors-and-run-it-in-comfyui/)
- A game dev GitHub issue proposes SA3 Small SFX only as a reference for procedural sounds, not a replacement; no listening results. [BS3D issue #442](https://github.com/AntoninPrazsky/BS3D/issues/442)
- ACE-Step 1.5: developers place it between Suno v4.5 and v5; a promotional blog says Suno v5 still leads on raw quality. [arXiv 2602.00744](https://arxiv.org/html/2602.00744v1); [promptspace blog](https://www.promptspace.in/blog/ace-step-1-5-the-free-ai-music-generator-that-actually-beats-suno)
- Game SFX practical issues: slow attacks, level mismatch, timing feel. [Summer Engine](https://www.summerengine.com/blog/ai-sound-effect-generator-for-games)

### Inferences
- Expect to generate many takes per SFX and curate; SA3 Small SFX's speed makes that cheap on a 3060. LIKELY.

### Gaps
- No Reddit (r/gamedev, r/StableDiffusion, r/comfyui) threads could be fetched; community sentiment is unverified.
- No independent blind test of ACE-Step 1.5 vs HeartMuLa vs SA3 Medium for game BGM.
