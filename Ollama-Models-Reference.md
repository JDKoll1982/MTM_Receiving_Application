# [Ollama](#ollama) Model Reference Guide
> **[Ollama](#ollama)** is a free program for running AI directly on your own computer — no internet or subscription needed. [See what Ollama is ↓](#ollama)
>
> **Your Hardware:** RTX 4070 Super (12 GB [VRAM](#vram)) + ~28 GB usable [RAM](#ram) = **~40 GB total budget**
>
> Ollama auto-splits models across [VRAM](#vram) and [RAM](#ram) — no manual config needed.
> Models ≤ 12 GB run fully on [GPU](#gpu) (fastest). Models 12–40 GB split [GPU](#gpu)+[RAM](#ram) (still fast).

---

## 🔤 Tech Terms Quick Reference

*Click any term to jump to its full plain-English explanation in the glossary below.*

| Term | Plain English |
|------|---------------|
| [Ollama](#ollama) | The free program used to run AI on your own computer |
| [Local AI](#local-ai) | AI running on your own computer — no internet needed once downloaded |
| [Open-Source](#open-source) | Free software anyone can download, use, and share |
| [Parameters (7B, 32B...)](#parameters) | How many "brain cells" a model has — bigger = smarter but needs more memory |
| [Quantized / Q4](#quantized) | Compressed model that uses less memory with minimal quality loss |
| [VRAM](#vram) | Your graphics card's dedicated memory |
| [RAM](#ram) | Your computer's main memory |
| [GPU](#gpu) | Your graphics card processor |
| [CPU](#cpu) | Your computer's main processor chip |
| [GPU+RAM Split](#gpu-ram-split) | Model layers shared between card and system memory |
| [Context Window](#context-window) | How much text the AI can read and remember at once (128K = a lot) |
| [Vision AI](#vision-ai) | AI that can look at and understand images or photos |
| [RAG](#rag) | AI that searches your own documents before answering |
| [Embeddings](#embeddings) | Text converted to numbers for similarity matching |
| [Vector](#vector) | A list of numbers representing meaning |
| [Function Calling](#function-calling) | AI that can trigger real actions and use external tools |
| [Mixture-of-Experts](#mixture-of-experts) | Model architecture that activates only parts of itself per query |
| [Chain-of-Thought](#chain-of-thought) | AI that shows its step-by-step reasoning |
| [Fill-in-the-Middle](#fill-in-the-middle) | Code completion that fills gaps in existing code |
| [OCR](#ocr) | Turning image text into readable computer text |
| [Speech-to-Text](#stt) | Converting spoken audio into written text |
| [TTS](#tts) | Converting written text into spoken audio |
| [Diarization](#diarization) | Identifying who said what in an audio recording |
| [SQL](#sql) | A language for asking questions from databases |
| [API](#api) | A way for programs to talk to each other or share data |
| [CUDA](#cuda) | NVIDIA's framework that lets software use your GPU |
| [NVMe](#nvme) | A very fast kind of SSD storage often used for model libraries |
| [SSD](#ssd) | Fast storage with no moving parts |
| [PCIe](#pcie) | The high-speed connection used by GPUs and some SSDs |
| [XMP](#xmp) | A one-click memory setting that lets RAM run at its rated speed |
| [Tokens](#tokens) | The small chunks of text AI models count instead of whole words |
| [Multimodal](#multimodal) | AI that handles more than one input type, like text plus images |
| [Terminal](#terminal) | The text-based window where you type commands like `ollama pull` |
| [Python](#python) | A popular programming language many AI tools depend on |
| [pip](#pip) | Python's package installer command |
| [Stable Diffusion](#stable-diffusion) | The popular family of image models behind SDXL |
| [Checkpoint](#checkpoint) | A downloadable image-model file used by image tools |
| [FLUX.1](#flux1) | A state-of-the-art image generation model *(requires separate tool)* |
| [SDXL](#sdxl) | Stable Diffusion XL — a popular image generation model *(requires separate tool)* |
| [ComfyUI](#comfyui) | A visual workflow tool for running image/video AI locally |
| [IDE](#ide) | Code editor software like VS Code |
| [Whisper.cpp](#whisper-cpp) | Free program for turning audio recordings into text |
| [WhisperX](#whisperx) | Whisper.cpp with automatic speaker identification added |
| [Kokoro TTS](#kokoro) | Free, natural-sounding text-to-speech program |
| [Piper TTS](#piper) | Lightweight offline text-to-speech program |
| [InvokeAI](#invokeai) | User-friendly free image generation program |
| [Automatic1111](#automatic1111) | Popular free image generation program with many options |
| [Fooocus](#fooocus) | Simplified beginner-friendly image generation |
| [Wan2.1](#wan21) | Free open-source AI video generation tool |
| [AnimateDiff](#animatediff) | Tool that turns still images into short animated clips |
| [CogVideoX](#cogvideox) | A separate AI video model that does not run through Ollama |

---

## 🖥️ Your Current Setup & Upgrade Recommendations

> *Auto-detected on May 29, 2026 — based on your actual hardware.*

### 📊 Current Hardware at a Glance

| Component | What You Have | AI Rating |
|-----------|--------------|-----------|
| **[CPU](#cpu)** | Intel Core i9-14900K — 24 cores / 32 threads | 🟢 Excellent — no upgrade needed |
| **[GPU](#gpu)** | NVIDIA RTX 4070 Super — 12 GB [VRAM](#vram) | 🟡 Good — VRAM is the main limit |
| **[RAM](#ram)** | 32 GB DDR5-6000 — 2 of 4 slots used | 🟡 Good — easy and cheap to expand |
| **Storage** | 990 PRO 2TB [NVMe](#nvme) (C:) + 980 PRO 1TB [NVMe](#nvme) (D:) | 🟢 Fast — ample for model storage |
| **Motherboard** | MSI Z790-P WIFI | 🟢 Excellent — supports up to 128 GB RAM |

**Your current AI budget:** ~40 GB (12 GB [VRAM](#vram) + ~28 GB usable [RAM](#ram))

> Models at or under 40 GB work on your machine today. The upgrades below push that ceiling higher and make everything faster.

---

### 🥇 Upgrade #1 — Add More RAM (Biggest Bang for Your Buck)

**Cost: ~$60–$250 | Impact: 🔴 Very High**

Your board (MSI Z790-P WIFI) supports up to **128 GB** of DDR5 RAM and you have **2 empty slots** right now. Adding RAM is the cheapest way to run bigger AI models.

| Option | What to Buy | New AI Budget | Models Unlocked |
|--------|------------|---------------|-----------------|
| **Quick Win** | 2× Corsair CMH32GX5M2B6000Z30 16GB sticks (~$60) | ~72 GB | Comfortable 70B models |
| **Best Value** | 2× 32GB DDR5-6000 sticks (~$130) | ~96 GB | 70B models fully, early 100B+ models |
| **Maximum** | Replace all 4 slots with 4× 32GB (~$250) | ~120 GB | Near any model that exists today |

> **Why this matters:** Right now `llama3.3:70b` (~40 GB) is at the edge of your budget and may stutter. With 64 GB RAM your AI budget becomes ~72 GB — `llama3.3:70b` runs smoothly with headroom to spare, and you can start testing 100B-class models as they arrive.
>
> **Tip:** Match your existing Corsair part number (`CMH32GX5M2B6000Z30`) to guarantee DDR5-6000 [XMP](#xmp) compatibility on your Z790 board with no extra setup.

---

### 🥈 Upgrade #2 — GPU with More VRAM

**Cost: $800–$2,500 | Impact: 🔴 Very High for speed**

[VRAM](#vram) is the single biggest speed lever for AI. When a model fits entirely in [VRAM](#vram) it runs at maximum speed with no slowdown. Right now your 12 GB means most 20–32 GB models use the slower [GPU+RAM split](#gpu-ram-split) path.

| Option | [VRAM](#vram) | Cost (approx.) | What Changes |
|--------|------|----------------|-------------|
| **RTX 4090** (used/refurb) | 24 GB | ~$1,500–$1,800 | All 20 GB models run fully on [GPU](#gpu) — `qwen2.5-coder:32b` at full GPU speed |
| **RTX 5080** (new) | 16 GB | ~$1,000 | Moderate gain; 16 GB models fully on [GPU](#gpu); much better efficiency |
| **RTX 5090** (new) | 32 GB | ~$2,000–$2,500 | Near everything under 32 GB runs at full GPU speed — the ultimate local AI card |

> **Why this matters:** Your most-used code AI (`qwen2.5-coder:32b`, ~20 GB) currently runs split across [GPU](#gpu) + [RAM](#ram). An RTX 4090 puts the whole thing on the GPU — you'd notice responses that feel near-instant instead of a 1–3 second wait per chunk.
>
> **Best value pick:** A used RTX 4090 offers the biggest practical jump for local AI at a reasonable price. The 5090 is future-proof but harder to justify unless you're doing heavy image/video generation alongside chat models.
>
> ⚠️ Your MSI Z790-P WIFI has a [PCIe](#pcie) 5.0 x16 slot — all cards above are compatible.

---

### 🥉 Upgrade #3 — Dedicated Model Storage Drive

**Cost: ~$200–$350 | Impact: 🟡 Medium (faster model loading)**

Your C: drive (990 PRO 2TB) has **232 GB free** — and AI models are large (4–40 GB each). As you collect more models, C: will fill up. Your D: drive (980 PRO 1TB) has ~1 TB free and is a fine place to store models *right now*, but a dedicated large [NVMe](#nvme) drive makes things tidier and faster.

| Option | Cost | Benefit |
|--------|------|---------|
| **Samsung 990 Pro 4TB** [NVMe](#nvme) | ~$280 | Fastest [PCIe](#pcie) 4.0 option — store 50+ large models with room to spare |
| **Samsung 870 EVO 4TB** SATA [SSD](#ssd) | ~$180 | Slower than [NVMe](#nvme) but fine for model storage since loading is not the bottleneck |

> **Why this matters:** Each model you pull is 4–40 GB. With your current free space you have room for maybe 5–8 large models before you're juggling. A 4TB drive gives you a permanent, organized model library.
>
> **How to redirect Ollama model storage:** After adding a new drive, set the `OLLAMA_MODELS` environment variable to point to it — Ollama will automatically use that location for all new downloads.

---

### 📋 Upgrade Priority Summary

| Priority | Upgrade | Cost | AI Impact |
|----------|---------|------|-----------|
| 1st | Add 2× 16GB DDR5-6000 RAM (→ 64 GB total) | ~$60 | 70B models run comfortably |
| 2nd | RTX 4090 or RTX 5090 (more [VRAM](#vram)) | $1,500–$2,500 | All 20–32 GB models at full GPU speed |
| 3rd | 4TB [NVMe](#nvme) for model storage | ~$250–$350 | Larger model library, faster loads |

> **Bottom line:** Your i9-14900K and Z790 board are not holding you back at all — they're excellent. The only limits on your local AI experience are [VRAM](#vram) (determines what runs at full speed) and [RAM](#ram) (determines how large a model you can run at all). The $60 RAM upgrade is the highest-value move you can make today.

---

## 🟢 Fits Fully in [VRAM](#vram) (≤ 12 GB) — Fastest Response

> **Restriction guide:** `Restricted` = strongly safety-aligned by default, `Less restricted` = usually more permissive, `Unrestricted` = commonly treated as uncensored, `N/A` = non-chat model. This is a best-effort guide for default Ollama variants; fine-tunes and system prompts can change behavior.

| Model | Parameters | Pull Command | Size | Restricted / Censored? | Best For |
|-------|------------|-------------|------|-------------------------|----------|
| `qwen2.5-coder:14b` | 14B | `ollama pull qwen2.5-coder:14b` | ~8.9 GB | Restricted | Code generation, [IDE](#ide) assistant, code review |
| `deepseek-coder-v2:16b` | 16B | `ollama pull deepseek-coder-v2:16b` | ~9 GB | Restricted | Code generation, debugging, refactoring |
| `codestral:22b` | 22B | `ollama pull codestral:22b` | ~12 GB | Restricted | Code generation, [fill-in-the-middle](#fill-in-the-middle) completion |
| `gemma3:12b` | 12B | `ollama pull gemma3:12b` | ~8 GB | Restricted | General chat, summarization, [Vision AI](#vision-ai) (image understanding) |
| `mistral-nemo:12b` | 12B | `ollama pull mistral-nemo:12b` | ~7 GB | Restricted | General chat, creative writing, roleplay |
| `phi4:14b` | 14B | `ollama pull phi4:14b` | ~8 GB | Restricted | Math, STEM reasoning, logic puzzles |
| `starcoder2:15b` | 15B | `ollama pull starcoder2:15b` | ~9 GB | Restricted | Code completion, [fill-in-the-middle](#fill-in-the-middle), multi-language programming |
| `sqlcoder:7b` | 7B | `ollama pull sqlcoder:7b` | ~4.1 GB | Restricted | [SQL](#sql) query generation, database schema analysis |
| `meditron:7b` | 7B | `ollama pull meditron:7b` | ~4.1 GB | Restricted | Medical Q&A, clinical notes, healthcare research |
| `llava-llama3:8b` | 8B | `ollama pull llava-llama3:8b` | ~5 GB | Restricted | Image understanding, visual Q&A, screenshot analysis |
| `moondream2` | Not stated | `ollama pull moondream2` | ~1.7 GB | Less restricted | Lightweight image captioning, quick visual tasks |
| `hermes3:8b` | 8B | `ollama pull hermes3:8b` | ~4.7 GB | Less restricted | [Function calling](#function-calling), AI agents, tool use, structured output |
| `llama3.1:8b` | 8B | `ollama pull llama3.1:8b` | ~4.7 GB | Restricted | General chat, [function calling](#function-calling), quick tasks |

---

## 🔵 [GPU](#gpu) + [RAM](#ram) Split (12–40 GB) — Balanced Performance

| Model | Parameters | Pull Command | Size | Restricted / Censored? | Best For |
|-------|------------|-------------|------|-------------------------|----------|
| `qwen2.5-coder:32b` | 32B | `ollama pull qwen2.5-coder:32b` | ~20 GB | Restricted | ⭐ Code generation, [IDE](#ide) assistant, code review, [SQL](#sql), debugging |
| `qwen2.5:32b` | 32B | `ollama pull qwen2.5:32b` | ~20 GB | Restricted | General chat, summarization, long [context window](#context-window) (128K), multilingual, [function calling](#function-calling) |
| `deepseek-r1:32b` | 32B | `ollama pull deepseek-r1:32b` | ~20 GB | Restricted | [Chain-of-thought](#chain-of-thought) reasoning, math, logic, research analysis |
| `qwq:32b` | 32B | `ollama pull qwq:32b` | ~20 GB | Restricted | Deep reasoning, math competitions, STEM problem solving |
| `gemma3:27b` | 27B | `ollama pull gemma3:27b` | ~16 GB | Restricted | General chat, long [context window](#context-window) (128K), [Vision AI](#vision-ai), multilingual |
| `aya-expanse:32b` | 32B | `ollama pull aya-expanse:32b` | ~20 GB | Restricted | Multilingual translation, cross-language tasks (100+ languages) |
| `llava:34b` | 34B | `ollama pull llava:34b` | ~20 GB | Restricted | Advanced image understanding, document [OCR](#ocr), visual reasoning |
| `mixtral:8x7b` | 8x7B (MoE) | `ollama pull mixtral:8x7b` | ~26 GB | Less restricted | General chat, creative writing, fast [mixture-of-experts](#mixture-of-experts) architecture |
| `llama3.3:70b` | 70B | `ollama pull llama3.3:70b` | ~40 GB | Restricted | ⭐ Best overall quality — creative writing, roleplay, complex reasoning (pushes budget) |

---

## ⚡ Tiny / Ultrafast (< 3 GB) — Edge, Offline, or Low-Latency Needs

| Model | Parameters | Pull Command | Size | Restricted / Censored? | Best For |
|-------|------------|-------------|------|-------------------------|----------|
| `phi3.5:3.8b` | 3.8B | `ollama pull phi3.5:3.8b` | ~2.2 GB | Restricted | Quick code help, on-device, offline assistant |
| `llama3.2:3b` | 3B | `ollama pull llama3.2:3b` | ~2 GB | Restricted | Fast general chat, summarization, lightweight agent |
| `gemma2:2b` | 2B | `ollama pull gemma2:2b` | ~1.6 GB | Restricted | Ultra-fast responses, minimal resource tasks |
| `tinyllama:1.1b` | 1.1B | `ollama pull tinyllama:1.1b` | ~638 MB | Less restricted | Embedded/edge AI, extreme speed testing |

---

## 🔍 [Embeddings](#embeddings) (Semantic Search / [RAG](#rag) Pipelines)

> These models don't chat — they convert text to [vectors](#vector) for similarity search and [RAG](#rag).

| Model | Parameters | Pull Command | Size | Restricted / Censored? | Best For |
|-------|------------|-------------|------|-------------------------|----------|
| `nomic-embed-text` | Not stated | `ollama pull nomic-embed-text` | ~274 MB | N/A | ⭐ Semantic search, [RAG](#rag), document similarity |
| `mxbai-embed-large` | Not stated | `ollama pull mxbai-embed-large` | ~670 MB | N/A | High-accuracy [embeddings](#embeddings), knowledge base indexing |
| `all-minilm` | Not stated | `ollama pull all-minilm` | ~46 MB | N/A | Ultra-lightweight [embeddings](#embeddings), local search |
| `nomic-embed-text:v1.5` | Not stated | `ollama pull nomic-embed-text:v1.5` | ~274 MB | N/A | Code + text [embeddings](#embeddings), hybrid search |

---

## 🎨 Image Generation — Requires Separate Tools (Not Ollama)

> Ollama handles **text and vision understanding only**, not image *generation*. Use these instead:

| Tool | Install | Best For |
|------|---------|----------|
| **[ComfyUI](#comfyui) + [FLUX.1](#flux1)** | [github.com/comfyanonymous/ComfyUI](https://github.com/comfyanonymous/ComfyUI) | Photorealistic image generation, workflows |
| **[ComfyUI](#comfyui) + [SDXL](#sdxl)** | Same as above, load [SDXL](#sdxl) checkpoint | Artistic generation, style transfer |
| **[InvokeAI](#invokeai)** | [invoke.ai](https://invoke.ai/) or [github.com/invoke-ai/InvokeAI](https://github.com/invoke-ai/InvokeAI) | User-friendly image generation UI |
| **[Automatic1111](#automatic1111)** | [github.com/AUTOMATIC1111/stable-diffusion-webui](https://github.com/AUTOMATIC1111/stable-diffusion-webui) | Classic SD interface, huge plugin ecosystem |
| **[Fooocus](#fooocus)** | [github.com/lllyasviel/Fooocus](https://github.com/lllyasviel/Fooocus) | Simplified beginner-friendly image generation |

> Your RTX 4070 Super is **excellent** for image generation — [FLUX.1](#flux1) at full quality runs well with 12 GB [VRAM](#vram).

---

## 🎬 Video Generation — Requires Separate Tools (Not Ollama)

| Tool | Install | Best For |
|------|---------|----------|
| **[Wan2.1](#wan21)** (via [ComfyUI](#comfyui)) | [github.com/Wan-Video/Wan2.1](https://github.com/Wan-Video/Wan2.1) | Open-source video generation, text-to-video |
| **[AnimateDiff](#animatediff)** (via [ComfyUI](#comfyui)) | [github.com/guoyww/AnimateDiff](https://github.com/guoyww/AnimateDiff) | Animate existing images, short clips |
| **[CogVideoX](#cogvideox)** | [github.com/THUDM/CogVideo](https://github.com/THUDM/CogVideo) | Longer video generation, higher consistency |

> Note: Video generation is very [VRAM](#vram)-hungry. 12 GB is workable for short clips at lower resolution.

---

## 🎙️ Audio / Speech — Requires Separate Tools (Not Ollama)

| Tool | Pull / Install | Best For |
|------|---------------|----------|
| **[Whisper.cpp](#whisper-cpp)** | [github.com/ggerganov/whisper.cpp/releases](https://github.com/ggerganov/whisper.cpp/releases) | [Speech-to-Text](#stt) transcription, meeting notes |
| **[Kokoro TTS](#kokoro)** | [github.com/remsky/Kokoro-FastAPI](https://github.com/remsky/Kokoro-FastAPI) | [TTS](#tts), natural voice synthesis |
| **[Piper TTS](#piper)** | [github.com/rhasspy/piper/releases](https://github.com/rhasspy/piper/releases) | Offline [TTS](#tts), home automation voice |
| **[WhisperX](#whisperx)** | Install with [pip](#pip): `pip install whisperx` (requires [Python](#python)) | [Diarized](#diarization) transcription (who said what) |

---

## 🧠 Specialty / Niche Models on Ollama

| Model | Parameters | Pull Command | Size | Restricted / Censored? | Best For |
|-------|------------|-------------|------|-------------------------|----------|
| `orca-mini:3b` | 3B | `ollama pull orca-mini:3b` | ~2 GB | Less restricted | Explaining reasoning step-by-step (teaching AI) |
| `wizard-math:7b` | 7B | `ollama pull wizard-math:7b` | ~4.1 GB | Less restricted | Math tutoring, step-by-step equation solving |
| `nous-hermes2:10.7b` | 10.7B | `ollama pull nous-hermes2:10.7b` | ~6.1 GB | Unrestricted | Uncensored general purpose, instruction following |
| `openhermes:7b` | 7B | `ollama pull openhermes:7b` | ~4.1 GB | Unrestricted | Roleplay, creative fiction, character AI |
| `stablelm2:12b` | 12B | `ollama pull stablelm2:12b` | ~6.9 GB | Less restricted | Creative writing, fiction, storytelling |
| `solar:10.7b` | 10.7B | `ollama pull solar:10.7b` | ~6.1 GB | Restricted | Long document summarization, report generation |
| `dolphin-mistral:7b` | 7B | `ollama pull dolphin-mistral:7b` | ~4.1 GB | Unrestricted | Roleplay, unrestricted creative tasks |
| `nexusraven:13b` | 13B | `ollama pull nexusraven:13b` | ~7.4 GB | Less restricted | [Function calling](#function-calling), [API](#api) integration, tool orchestration |

---

## 📊 Quick Pick Guide

| I want to... | Use this |
|-------------|----------|
| Replace GitHub Copilot | `qwen2.5-coder:32b` |
| Think through hard problems | `deepseek-r1:32b` or `qwq:32b` |
| Chat naturally in any language | `qwen2.5:32b` or `aya-expanse:32b` |
| Analyze / describe an image | `llava:34b` or `gemma3:27b` |
| Generate images | [ComfyUI](#comfyui) + [FLUX.1](#flux1) (separate tool) |
| Generate video | [ComfyUI](#comfyui) + [Wan2.1](#wan21) (separate tool) |
| Transcribe audio | [Whisper.cpp](#whisper-cpp) (separate tool) |
| Write creative fiction | `llama3.3:70b` or `mixtral:8x7b` |
| Write SQL queries | `sqlcoder:7b` or `qwen2.5-coder:32b` |
| Answer medical questions | `meditron:7b` |
| Build an AI agent | `hermes3:8b` or `qwen2.5:32b` |
| Search my documents ([RAG](#rag)) | `nomic-embed-text` + any chat model |
| Get an instant fast response | `phi3.5:3.8b` or `llama3.2:3b` |

---

## 📖 Tech Terms Glossary

### Core AI Basics

<a id="vram"></a>
### VRAM — Video RAM
The dedicated memory built into your graphics card. It's much faster than regular [RAM](#ram) for AI tasks because the [GPU](#gpu) can access it directly without going through the system bus. Your RTX 4070 Super has **12,282 MB (~12 GB) of VRAM**. When a model fits entirely in VRAM it runs at maximum speed with zero slowdown.

---

<a id="ollama"></a>
### Ollama
A free program that lets you download and run AI models directly on your own computer — no internet connection required after the initial download, no subscription fees, and no usage limits. Think of it like a "player" for AI models, similar to how VLC is a player for video files. Once installed, you use the `ollama pull <model-name>` command in a [terminal](#terminal) to download any model listed in this guide, then interact with it. This is [Local AI](#local-ai) — your data never leaves your machine.

**⬇️ Download for free:** [ollama.com](https://ollama.com)

---

<a id="local-ai"></a>
### Local AI (On-Device AI)
AI that runs entirely on your own computer, with no data sent to the internet. Your questions, documents, and the AI's answers all stay on your machine. **Benefits:** complete privacy, no subscription fees, works offline, no usage limits. **Drawback:** requires capable hardware (like your RTX 4070 Super). [Ollama](#ollama) is the tool that makes local AI easy to set up — no technical expertise required.

---

<a id="open-source"></a>
### Open-Source
Software or AI models where the underlying code (or model weights) are made freely available for anyone to download, use, modify, and share — at no cost. Most models in this guide are open-source, meaning no per-message fees and no vendor lock-in. It also means the community can inspect, improve, and build on them. The opposite is closed-source or proprietary software that you cannot freely inspect or share.

---

<a id="parameters"></a>
### Parameters (7B, 14B, 32B...)
The "B" stands for **billion**, and refers to how many individual numerical values make up an AI model — think of them like "brain connections." More parameters generally means smarter and more capable responses, but also a larger file size and more memory required. A 7B model is quick and fits easily in [VRAM](#vram); a 32B model is much more capable but needs [GPU+RAM split](#gpu-ram-split). You'll see this in every model name: `qwen2.5:32b` means 32 billion parameters.

---

<a id="tokens"></a>
### Tokens — The Units AI Counts Text In
AI models do not count whole words the same way humans do. Instead, they count small text pieces called tokens. A short word might be one token, while a longer word or punctuation-heavy text may use more. This is why model limits are described in tokens instead of words.

---

<a id="context-window"></a>
### Context Window (128K, 8K...)
The maximum amount of text an AI model can "hold in mind" at one time — like short-term memory. The number refers to chunks of text called [tokens](#tokens) (roughly 3/4 of a word each). "128K context" means the model can read roughly **96,000 words at once** — about the length of a full novel. A larger context window lets you send longer documents, longer conversations, or larger chunks of code without the AI "forgetting" what was at the beginning.

---

<a id="multimodal"></a>
### Multimodal — More Than One Kind of Input
Multimodal AI can work with more than one type of input or output. The most common example is text plus images, but it can also include audio or video. If a model is multimodal, it is not limited to plain text chat.

---

<a id="vision-ai"></a>
### Vision AI ([Multimodal](#multimodal))
An AI model that can look at and understand images, not just read text. You can send it a photo, screenshot, diagram, or document scan and ask questions about what it sees — for example: "What does this chart show?" or "Read the text in this screenshot." Models in this guide with vision capability include `gemma3:12b`, `gemma3:27b`, `llava-llama3:8b`, and `llava:34b`. *[Multimodal](#multimodal)* simply means the AI works with multiple types of input — text and images.

---

### Hardware And Performance

<a id="ram"></a>
### RAM — System Memory
Your computer's main memory used for running programs, your OS, and open applications. Slower than [VRAM](#vram) for AI tasks but much larger. When an AI model is too big to fit in [VRAM](#vram), Ollama spills the remaining layers here — this still works, just slower for those layers.

---

<a id="nvme"></a>
### NVMe — Very Fast SSD Storage
NVMe is a high-speed type of [SSD](#ssd) storage that plugs into a fast motherboard connection. In plain English: it is the kind of drive that makes large AI model files load much faster than older storage types. It is excellent for keeping a local AI model library because models are often several gigabytes each.

---

<a id="ssd"></a>
### SSD — Solid-State Drive
An SSD is a storage drive with no spinning disks inside. That makes it much faster, quieter, and more durable than old mechanical hard drives. AI tools benefit because models load faster and large downloads finish sooner.

---

<a id="pcie"></a>
### PCIe — High-Speed Expansion Connection
PCIe is the high-speed connection standard used by modern [GPU](#gpu) cards and some fast [NVMe](#nvme) drives. When you see terms like "PCIe 4.0" or "PCIe 5.0," think of them as different generations of speed.

---

<a id="xmp"></a>
### XMP — One-Click RAM Speed Profile
XMP is a BIOS setting that lets your memory sticks run at the faster speed they were sold for. Without XMP, RAM often runs at a slower default speed. For a non-technical user, it is basically a safe "use the speed I paid for" switch for compatible RAM kits.

---

<a id="gpu"></a>
### GPU — Graphics Processing Unit
Your graphics card processor (in your case, the NVIDIA RTX 4070 Super). Originally designed for rendering game graphics, GPUs are exceptionally fast at the parallel math AI models require. Running AI on a GPU is typically 10–50× faster than running it on a CPU.

---

<a id="gpu-ram-split"></a>
### GPU + RAM Split (GPU Offloading)
When a model is too large to fit entirely in [VRAM](#vram), Ollama automatically loads as many layers as possible onto the [GPU](#gpu) and puts the rest into system [RAM](#ram). The GPU handles the bulk of computation (fast), while occasional RAM access adds slight latency. This is completely automatic — no configuration needed.

---

### Search And Knowledge Tools

<a id="rag"></a>
### RAG — Retrieval-Augmented Generation
A technique where the AI searches through your own documents (PDFs, notes, code files, wikis) before answering a question, so it gives answers grounded in *your* data rather than just its training data. Example: "Search my company manual and answer this question." Requires an [embeddings](#embeddings) model to index the documents first.

---

<a id="embeddings"></a>
### Embeddings
The process of converting text into a list of numbers ([vectors](#vector)) that capture meaning. Similar sentences produce similar number patterns, which lets computers find related content even when exact words don't match. [Embeddings](#embeddings) models (like `nomic-embed-text`) are tiny, fast, and don't chat — they just transform text into searchable vectors for [RAG](#rag) pipelines.

---

<a id="vector"></a>
### Vector
A list of numbers that represents the "meaning" of a piece of text in mathematical form. For example, "dog" and "puppy" produce very similar vectors because they mean similar things. Vectors are what [embeddings](#embeddings) models produce, and vector databases store them for fast similarity search.

---

### Model Behaviors And Capabilities

<a id="quantized"></a>
### Quantized / Q4
A compression technique that shrinks AI model file sizes by storing numbers with less precision — like rounding to fewer decimal places. A "Q4" model uses 4 bits per value instead of 16 or 32 bits. This typically cuts model size by ~75% with only a small drop in quality. Almost all models you download from Ollama are quantized by default.

---

<a id="function-calling"></a>
### Function Calling (Tool Use)
The ability of an AI model to trigger real actions — like searching the web, running code, calling an API, reading a file, or sending a message — rather than just generating text. A model with strong function calling can act as an autonomous agent that uses your computer as a set of tools.

---

<a id="mixture-of-experts"></a>
### Mixture-of-Experts (MoE)
A model architecture where instead of one large neural network handling everything, the model is made of many smaller "expert" sub-networks. Each query only activates a few relevant experts. This makes the model faster and cheaper to run — a 26 GB MoE model can match the quality of a 65 GB standard model because most of its parameters sit idle per query.

---

<a id="chain-of-thought"></a>
### Chain-of-Thought (CoT)
A reasoning technique where the AI works through a problem step-by-step before giving a final answer — similar to "showing your work" in math class. Models trained for chain-of-thought (like DeepSeek-R1 and QwQ) are dramatically better at logic, math, and multi-step problems because they reason out loud rather than jumping to conclusions.

---

<a id="fill-in-the-middle"></a>
### Fill-in-the-Middle (FIM)
A code completion technique where the AI inserts code into the *middle* of existing code, not just appending to the end. Given code above and below the cursor, the model fills the gap intelligently. This is what makes [IDE](#ide) tab-completion feel smart. Codestral and StarCoder2 are trained specifically for this pattern.

---

<a id="ocr"></a>
### OCR — Optical Character Recognition
The ability to read text out of images. For example, photographing a receipt and extracting the itemized prices as editable text. Vision AI models like LLaVA can perform OCR as part of general image understanding, though dedicated OCR tools are often more accurate for structured documents.

---

### Developer And Workflow Terms

<a id="terminal"></a>
### Terminal — The Text Window for Commands
A terminal is the text-based window where you type commands instead of clicking buttons in a normal app. Examples in this guide include `ollama pull qwen2.5:32b` or `pip install whisperx`. It may look technical, but for many AI tools it is just the simplest way to install or run something.

---

<a id="python"></a>
### Python — A Common AI Tool Language
Python is one of the most widely used programming languages in AI. Many tools, helpers, and install commands in the AI world depend on Python being present on your computer. You do not need to know how to program in Python to use most of these tools, but you may need it installed.

---

<a id="pip"></a>
### pip — Python's Package Installer
`pip` is the standard command used to install extra Python-based tools. When you see a command like `pip install whisperx`, it means "download and install this tool through Python." If Python is not installed first, `pip` will not work.

---

<a id="ide"></a>
### IDE — Integrated Development Environment
A software application that provides a complete environment for writing code — including a code editor, file explorer, debugger, and terminal all in one place. Examples include VS Code, Visual Studio, and JetBrains Rider. An AI coding assistant (like GitHub Copilot, or a local model via the OAI-Compatible extension) integrates directly into your IDE to suggest code as you type.

---

<a id="sql"></a>
### SQL — Structured Query Language
A simple language used to ask questions from databases — like asking "Give me all orders from last month" in a structured way the database understands. Non-programmers often encounter SQL when working with business data systems. The `sqlcoder:7b` model is trained specifically to write SQL for you: you describe in plain English what data you want, and it writes the query automatically.

---

<a id="api"></a>
### API — Application Programming Interface
A standardized way for programs to talk to each other. Think of it like a restaurant drive-through: you place your order in a specific format, the kitchen responds in a predictable format — you don't need to know how the kitchen works internally. When an AI model supports an "API," it means other apps (like VS Code extensions, chat tools, or automations) can send questions to the AI and receive answers automatically, without a human typing in a chat window.

---

### Audio And Speech

<a id="stt"></a>
### Speech-to-Text (STT / Transcription)
The ability to convert spoken audio — from a microphone, voice recording, or audio file — into written text. This is the technology behind automatic captions, meeting notes, and voice dictation. [Whisper.cpp](#whisper-cpp) and [WhisperX](#whisperx) are the most popular free tools for doing this on your own computer. *Not to be confused with [TTS](#tts) (Text-to-Speech), which does the reverse.*

---

<a id="tts"></a>
### TTS — Text-to-Speech
Converting written text into spoken audio. Modern TTS systems like Kokoro and Piper can produce natural-sounding voices entirely on your local machine with no data sent to the cloud. Useful for accessibility tools, voice assistants, audiobook generation, or adding narration to videos.

---

<a id="diarization"></a>
### Diarization (Speaker Diarization)
Automatically identifying *who* is speaking in an audio recording and labeling each segment. For example, turning a recorded meeting into a transcript that says "John: ..." and "Sarah: ..." instead of one undifferentiated block of text. WhisperX adds diarization on top of standard speech-to-text transcription.

---

<a id="whisper-cpp"></a>
### Whisper.cpp — Free Speech-to-Text Tool
A free, fast, [open-source](#open-source) program that converts audio recordings into text ([Speech-to-Text](#stt)). Based on OpenAI's Whisper model but optimized to run efficiently on your own computer — no internet or cloud account needed. Great for transcribing meetings, interviews, lectures, podcasts, or creating subtitles for videos.

**⬇️ Download (Windows/Mac/Linux):** [github.com/ggerganov/whisper.cpp/releases](https://github.com/ggerganov/whisper.cpp/releases)
**Full GitHub page:** [github.com/ggerganov/whisper.cpp](https://github.com/ggerganov/whisper.cpp)

---

<a id="whisperx"></a>
### WhisperX — Whisper with Speaker Labels
An enhanced version of [Whisper.cpp](#whisper-cpp) that adds [diarization](#diarization) — it not only transcribes what was said, but also labels *who* said it. Instead of one big block of text, you get a transcript formatted like a script: "**Speaker 1:** ... / **Speaker 2:** ..." Ideal for multi-person meeting recordings or interview transcripts.

**⬇️ Install:** Use [pip](#pip): `pip install whisperx` *(requires [Python](#python) — see the GitHub page for setup help)*
**Full GitHub page:** [github.com/m-bain/whisperX](https://github.com/m-bain/whisperX)

---

<a id="kokoro"></a>
### Kokoro TTS — Natural-Sounding Text-to-Speech
A free, high-quality [TTS](#tts) program that converts written text into very natural-sounding speech — far better than old robotic computer voices. Runs entirely on your computer with no internet connection needed. Great for creating voiceovers, listening to documents read aloud, or building accessibility tools.

**⬇️ Download / Install:** [github.com/remsky/Kokoro-FastAPI](https://github.com/remsky/Kokoro-FastAPI)

---

<a id="piper"></a>
### Piper TTS — Lightweight Offline Voice Synthesis
A small, fast [TTS](#tts) program designed to run on low-power devices (like a Raspberry Pi) as well as full PCs. Produces clear, natural speech with many downloadable voice styles. Popular for home automation ("Hey, your laundry is done"), accessibility tools, and any project that needs offline speech without heavy resource use.

**⬇️ Download (Windows/Mac/Linux):** [github.com/rhasspy/piper/releases](https://github.com/rhasspy/piper/releases)
**Full GitHub page:** [github.com/rhasspy/piper](https://github.com/rhasspy/piper)

---

### Image And Video Tools

<a id="stable-diffusion"></a>
### Stable Diffusion — The Big Family of Local Image Models
Stable Diffusion is the well-known family of AI image generators that power many local art tools. [SDXL](#sdxl) is one of its newer, higher-quality versions. If you see a tool advertising support for Stable Diffusion, it usually means it can run a large library of downloadable art models made by the community.

---

<a id="checkpoint"></a>
### Checkpoint — A Downloadable Image Model File
In image-generation tools, a checkpoint is the main model file you download and load to get a certain visual style or capability. Think of it like choosing a different "brain" for the image tool. One checkpoint may be photorealistic, another may specialize in anime, and another may be tuned for logos or product shots.

---

<a id="flux1"></a>
### FLUX.1
A state-of-the-art [open-source](#open-source) image generation model by Black Forest Labs (the original creators of Stable Diffusion). FLUX.1 produces highly realistic, detail-accurate images and handles text inside images far better than older models. The full-quality version fits comfortably in 12 GB [VRAM](#vram), making it an excellent match for your RTX 4070 Super. **Does not run through Ollama** — use [ComfyUI](#comfyui) or [InvokeAI](#invokeai).

**⬇️ Download models:** [huggingface.co/black-forest-labs](https://huggingface.co/black-forest-labs)
**Run with:** [ComfyUI](#comfyui) or [InvokeAI](#invokeai)

---

<a id="sdxl"></a>
### SDXL — Stable Diffusion XL
An upgraded version of the original [Stable Diffusion](#stable-diffusion) image generation model. Produces higher-resolution, more detailed images than the base SD 1.5. Widely supported by [ComfyUI](#comfyui), [Automatic1111](#automatic1111), and [InvokeAI](#invokeai) with a massive community library of custom art-style model files (called [checkpoints](#checkpoint)). Slightly older than [FLUX.1](#flux1) but has a far larger ecosystem of fine-tuned models. **Does not run through Ollama** — use one of the image generation tools below.

**⬇️ Browse free models:** [civitai.com](https://civitai.com) (thousands of free SDXL styles)
**Run with:** [ComfyUI](#comfyui), [Automatic1111](#automatic1111), or [InvokeAI](#invokeai)

---

<a id="comfyui"></a>
### ComfyUI
A powerful, node-based visual workflow editor for running image and video AI models locally on your own hardware. You connect building blocks (nodes) together to create custom pipelines — for example: text prompt -> [FLUX.1](#flux1) model -> upscaler -> save as PNG. More complex than simpler tools like [Automatic1111](#automatic1111) or [InvokeAI](#invokeai), but far more flexible and capable. **Required for running video generation tools** like [Wan2.1](#wan21) and [AnimateDiff](#animatediff). Runs entirely on your machine with no cloud service required.

**⬇️ Download:** [github.com/comfyanonymous/ComfyUI/releases](https://github.com/comfyanonymous/ComfyUI/releases)
**Full GitHub page:** [github.com/comfyanonymous/ComfyUI](https://github.com/comfyanonymous/ComfyUI)

---

<a id="invokeai"></a>
### InvokeAI — User-Friendly Image Generation
A polished, easy-to-use web interface for generating images with AI locally on your own computer. Supports [FLUX.1](#flux1), [SDXL](#sdxl), and many other image models. One of the easiest starting points for non-technical users who want to generate AI images without the complexity of [ComfyUI](#comfyui) or [Automatic1111](#automatic1111). Just install, open your browser, and start creating.

**⬇️ Download:** [invoke.ai](https://invoke.ai/) or [github.com/invoke-ai/InvokeAI](https://github.com/invoke-ai/InvokeAI)

---

<a id="automatic1111"></a>
### Automatic1111 (Stable Diffusion Web UI)
The most widely used free image generation program, with a massive community, thousands of add-ons, and support for nearly every [SDXL](#sdxl) and [Stable Diffusion](#stable-diffusion) model available. More settings and options than [InvokeAI](#invokeai), but also more complex to set up. Excellent if you want deep control over image generation or want access to the largest plugin ecosystem.

**⬇️ Download:** [github.com/AUTOMATIC1111/stable-diffusion-webui](https://github.com/AUTOMATIC1111/stable-diffusion-webui)

---

<a id="fooocus"></a>
### Fooocus — Simple Image Generation
A streamlined image generation program designed to keep settings simple while still producing beautiful results with little technical knowledge required. Built on top of [SDXL](#sdxl). Great for beginners who just want to type a description and get a great-looking image without configuring anything.

**⬇️ Download:** [github.com/lllyasviel/Fooocus](https://github.com/lllyasviel/Fooocus)

---

<a id="wan21"></a>
### Wan2.1 — Open-Source Video Generation
A free, [open-source](#open-source) AI model for generating short video clips from text descriptions or still images. Runs through [ComfyUI](#comfyui). Produces animated clips of a few seconds — think of it as image generation, but with motion added. Video generation requires significantly more [VRAM](#vram) and time than image generation.

**⬇️ GitHub:** [github.com/Wan-Video/Wan2.1](https://github.com/Wan-Video/Wan2.1)
**Runs via:** [ComfyUI](#comfyui) — [github.com/comfyanonymous/ComfyUI/releases](https://github.com/comfyanonymous/ComfyUI/releases)

---

<a id="animatediff"></a>
### AnimateDiff — Animate Still Images
A tool that takes a still [SDXL](#sdxl) or [Stable Diffusion](#stable-diffusion) image and adds motion to it, creating a short looping animation. Run through [ComfyUI](#comfyui). Useful for creating animated avatars, short social media clips, or animated wallpapers from AI-generated images — all on your own machine.

**⬇️ GitHub:** [github.com/guoyww/AnimateDiff](https://github.com/guoyww/AnimateDiff)
**Runs via:** [ComfyUI](#comfyui) — [github.com/comfyanonymous/ComfyUI/releases](https://github.com/comfyanonymous/ComfyUI/releases)

---

<a id="cogvideox"></a>
### CogVideoX — Local Video Generation Model
CogVideoX is a video-generation model family for creating longer and more consistent clips than many lightweight local options. It does not run through [Ollama](#ollama). Instead, people usually run it through [ComfyUI](#comfyui) workflows or directly from its own project files.

**⬇️ GitHub:** [github.com/THUDM/CogVideo](https://github.com/THUDM/CogVideo)
**Common app to run it with:** [ComfyUI](#comfyui) — [github.com/comfyanonymous/ComfyUI/releases](https://github.com/comfyanonymous/ComfyUI/releases)

---
