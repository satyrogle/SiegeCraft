# Paid and hosted AI tools for game SFX and music (as of 13 Oct 2026)

Scope: hosted tools a solo UK indie dev could pay for to make SFX and music for a paid Steam game. All pages below were seen on 13 Oct 2026 unless stated. Prices are as shown to a fetch from this machine (some show GBP or EUR, most USD, VAT usually extra). Terms change often in this market; every "Last updated" date found is recorded.

Format note: each finding is "fact. [Source](URL)". Short quotes are the exact wording on the page.

## 1. Sound effects tools: what they make, price, rights, API, format

### Takeaway
ElevenLabs Sound Effects is the best documented hosted text to SFX tool for games: paid plans from about $6/month give commercial use, you keep the output, it has an API, a seamless loop switch, and 48 kHz WAV for non looping sounds. Stable Audio (now 3.0, with an SFX model) and Adobe Firefly Generate Sound Effects (GA 20 Aug 2026) are the main alternatives; Krotos is library based rather than truly generative, Soundly has no text to SFX generator, and Optimizer.ai and smaller tools have thin, conflicting documentation.

### Cited Findings

**ElevenLabs Sound Effects**
- Max 30 seconds per generation; a loop option "Creates sound effects that can be played on repeat without perceptible start/end points". [ElevenLabs SFX docs](https://elevenlabs.io/docs/capabilities/sound-effects)
- Output: "MP3 for all effects; WAV at 48 kHz for non-looping effects". So looped SFX come as MP3 only per the docs. [ElevenLabs SFX docs](https://elevenlabs.io/docs/capabilities/sound-effects)
- Cost: docs say "40 credits per second when duration is specified"; the pricing page FAQ says Sound Effects are 200 credits per generation. These differ (likely 200 is the auto duration price), so check in app. [ElevenLabs SFX docs](https://elevenlabs.io/docs/capabilities/sound-effects); [ElevenLabs pricing](https://elevenlabs.io/pricing)
- Has a public API endpoint (text to sound effects / convert). [ElevenLabs SFX docs](https://elevenlabs.io/docs/capabilities/sound-effects)
- Plans seen 13 Oct 2026: Free $0 / 10k credits; Starter $6 / 30k credits ("Commercial License" listed, $1 first month offer until 18 Oct); Creator $22 / 121k; Pro $99 / 600k; Scale $299 / 1.8M (3 seats); Business $990 / 6M (10 seats). Audio listed as 44.1 kHz, 128 kbps (Free, Starter) or 128 and 192 kbps (Creator up). Pro lists "44.1kHz PCM audio output via API". [ElevenLabs pricing](https://elevenlabs.io/pricing)
- Rights (general Terms of Use, "Last Updated: 09 October 2026"): free plans are non commercial only (s.1(c)(i)); on paid plans "you may use the Services for commercial purposes" (s.1(c)(ii)); ownership: "as between you and ElevenLabs, you retain all rights in and to your Output" (s.4(c)(ii)). No SFX specific clause and no attribution rule for free users found. [ElevenLabs Terms of Use](https://elevenlabs.io/terms-of-use)
- Described by a 2026 game SFX guide as one of the most capable text to SFX generators (vendor blog, not independent testing). [Summer Engine blog](https://www.summerengine.com/blog/ai-sound-effect-generator-for-games)

**Stable Audio (Stability AI), hosted at stableaudio.com and via API**
- Stable Audio 3.0 launched 20 May 2026: four models, Small SFX (459M params), Small (459M), Medium (1.4B), Large (2.7B). Medium and Large make tracks up to 6 min 20 s. Three are open weights on Hugging Face; Large is only via Stability API, fal.ai, or enterprise self hosting. Stability says it is trained entirely on licensed data. [TechCrunch](https://techcrunch.com/2026/05/20/stability-ai-release-a-new-audio-model-that-can-create-six-minute-songs/); [The Decoder](https://the-decoder.com/stability-ai-launches-stable-audio-3-0-with-up-to-six-minute-tracks-and-open-weights/)
- Stable Audio 3.0 makes instrumental music and sound effects, no vocals or lyrics. [Stability AI explainer, updated 9 Sep 2026](https://stability.ai/explainers/stable-audio-vs-competitors-licensing-export-rights-and-self-hosting-compared)
- Hosted plans per Stability's own explainer (page "Last updated September 9, 2026"): Solo $12/mo, 660 credits; Session $30, 1,800 credits; Producer $90, 6,000 credits; Studio $199/mo, 14,000 credits. "no cap on downloading generated audio". [Stability AI explainer](https://stability.ai/explainers/stable-audio-vs-competitors-licensing-export-rights-and-self-hosting-compared)
- Rights: "Outputs are yours under the Stability AI Community License"; organisations under $1M annual revenue may use outputs commercially; above $1M needs an Enterprise licence (which includes indemnification). [Stability AI explainer](https://stability.ai/explainers/stable-audio-vs-competitors-licensing-export-rights-and-self-hosting-compared); [TechCrunch](https://techcrunch.com/2026/05/20/stability-ai-release-a-new-audio-model-that-can-create-six-minute-songs/)
- Official FAQ (via search snippet; page did not render for fetch): a paid subscription is required for commercial use of generated tracks, and audio generated on Pro and higher remains covered after cancellation. [stableaudio.com FAQ](https://stableaudio.com/faqs)
- Conflict: third party sites still list old plan names (Pro $11.99, Studio $29.99, Max $89.99) and one claims a 100,000 monthly active user cap for apps and games under the Creator licence. Not confirmed on any Stability page. [CostBench](https://costbench.com/software/ai-music-generators/stable-audio/); [Dynamoi](https://dynamoi.com/learn/ai-music-distribution/suno-vs-stable-audio-commercial-rights)
- Positioned by a game SFX guide for ambience (wind, rain, crowds, machinery). [Summer Engine blog](https://www.summerengine.com/blog/ai-sound-effect-generator-for-games)

**Adobe Firefly Generate Sound Effects**
- Generally available to all on Firefly web and mobile since 20 Aug 2026; powered by the Firefly Audio Model; makes "custom sounds that match the action, timing and energy of your content" (you can describe the sound or perform it as a voice reference). [Adobe blog, 20 Aug 2026](https://blog.adobe.com/en/publish/2026/08/20/adobe-firefly-expands-its-creative-ai-studio-generate-music-speech-and-sound-effects-in-one-place); [The Next Web](https://thenextweb.com/news/adobe-firefly-audio-music-speech-sound-effects)
- Generate Sound Effects is a "premium" feature that spends monthly generative credits (credits reset each cycle, no rollover). [Adobe Firefly plans FAQ](https://www.adobe.com/cc-shared/fragments/products/firefly/plans/faq)
- Plan prices (third party, checked Adobe US page 25 Aug 2026): Standard $9.99/mo 2,000 credits; Pro $19.99/mo 4,000; Pro Plus $49.99/mo 10,000; Premium $199.99/mo 50,000. [SaaS CRM Review](https://saascrmreview.com/?p=7019)
- Adobe markets the audio as commercially safe (trained on licensed and public domain content). This is a vendor claim. [iTWire](https://itwire.com/business-it-news/business-software/adobe-makes-firefly-a-full-audio-studio-and-the-licence-is-the-actual-product); [Adobe blog](https://blog.adobe.com/en/publish/2026/08/20/adobe-firefly-expands-its-creative-ai-studio-generate-music-speech-and-sound-effects-in-one-place)

**Krotos Studio**
- AI ambience generator from text (and image to ambience), built on a library of professionally recorded sounds rather than pure generation. [Newsshooter](https://www.newsshooter.com/2025/01/02/krotos-ai-image-to-ambience-feature/); [Post Magazine](https://www.postmagazine.com/Press-Center/Daily-News/2024/Krotos-taps-AI-to-create-soundtrack-ambiences.aspx)
- Krotos Studio from $9.99/mo; Krotos Studio Pro $34.99/mo or $279.92/yr (Sound On Sound); Post Magazine gave $23.49/mo earlier. [Sound On Sound](https://www.soundonsound.com/news/krotos-launch-krotos-studio-pro); [Post Magazine](https://www.postmagazine.com/Press-Center/Daily-News/2024/Krotos-taps-AI-to-create-soundtrack-ambiences.aspx)
- A competitor's comparison says games need Pro or higher. Not confirmed on a Krotos page. [Cinevva guide](https://app.cinevva.com/ko/guides/ai-sound-effect-generators)

**Soundly**
- Homepage shows no text to SFX generation. AI features are Natural Language Search (finding existing library sounds) and Voice Designer (synthetic announcement type voices). Free and Pro (250,000 sound Pro Library) tiers; prices not shown on the page; licence is in a separate EULA PDF. [Soundly](https://getsoundly.com/)

**Optimizer.ai and smaller SFX tools**
- Optimizer.ai: no official current pricing found. One directory lists Starter $10, Pro $25, Unlimited $65/mo, Enterprise/API custom; a 2024 article listed $4 for 2,000 credits. [Softwarefinder](https://softwarefinder.com/artificial-intelligence/optimizerai); [Creati.ai](https://creati.ai/ai-tools/optimizerai/)
- A 2026 review says premium tiers unlock high resolution exports and commercial licensing (implying free is not cleared). [AIIndigo](https://aiindigo.com/blog/sound-effects-a-practical-review-of-optimizerai-s-audio-engine-in-2026)
- SFX Engine and LoudMe are named for short punchy UI and combat SFX; Phaser's browser SFX Generator does 100 ms to 60 s clips in batches of 4. Licence terms not checked. [Summer Engine blog](https://www.summerengine.com/blog/ai-sound-effect-generator-for-games); [Phaser SFX Generator](https://phaser.io/tools/sfx-generator)

### Inferences
- For a solo dev, ElevenLabs Starter or Creator is the lowest friction paid SFX route with clear written ownership and commercial terms; Pro is needed for PCM via API.
- Looped ElevenLabs SFX come as MP3 only (per docs). MP3 encoder padding can add a gap at the loop point, so loops may need re-exporting or checking in an editor before use in Unity.
- Stable Audio's Community Licence threshold ($1M revenue) is well above a solo dev's likely revenue, so commercial use on a paid hosted plan looks fine; Stable Audio 3.0 Small SFX is also open weights, which overlaps with any local tools research.
- Adobe Firefly makes sense mainly if Jacob already pays for Creative Cloud or Firefly credits.

### Gaps
- ElevenLabs SFX model version name and whether 48 kHz WAV needs a particular plan were not stated.
- Stable Audio hosted export format and sample rate, loop feature and stems were not found on an official page (pricing page did not render).
- Adobe Firefly: per clip credit cost, max duration, output format, and whether individual (non enterprise) plans get IP indemnity were not found.
- Krotos and Optimizer.ai licence text for games not read first hand.
- No independent blind quality comparison of these SFX tools for games found.

## 2. Music tools: what they make, price, rights, API, format

### Takeaway
As of Oct 2026 the cleanest written licence for a small paid game is ElevenLabs Music (Starter and up explicitly allow "Indie Games" under $500k lifetime revenue) and Stable Audio (paid hosted plan, under $1M revenue). Suno Pro/Premier assigns ownership but now caps downloads (20 or 60 per month) and commercial use only covers songs downloaded through approved channels. Udio cannot be used for a shipped game right now because downloads have been off since Oct 2025. AIVA Pro gives copyright to you; Soundraw lists video games on all plans; Mubert and Beatoven keep copyright themselves.

### Cited Findings

**Suno (v6 family)**
- v6, v6-wild and v6-mini launched 9 Sep 2026, built with Warner Music Group, BMG and Believe; v6-mini is the free model, v6 and v6-wild need Pro or Premier. [The Rundown](https://www.therundown.ai/news/suno-v6-music-partners-paid-fan-remixes); [The Next Web](https://thenextweb.com/news/suno-v6-warner-bmg-believe-licensed-models); [DataNorth](https://datanorth.ai/news/suno-launches-v6-v6-wild-and-v6-mini)
- Pricing page seen 13 Oct 2026: Free $0, 50 credits/day, "No commercial rights", "No monthly song downloads"; Pro $8/mo shown with "billed yearly saves $24", 2,500 credits/mo, "Commercial use rights", "20 song downloads per month", 2 stem separation types; Premier $24/mo shown with yearly saving of $72, 10,000 credits/mo, "60 song downloads per month", 3 stem types, includes Suno Studio. Extra downloads and credits can be bought. [Suno pricing](https://suno.com/pricing)
- Terms ("Date of Last Revision: August 10, 2026", "Effective Date: September 3, 2026"): Pro/Premier, "Suno hereby assigns to you all of its right, title and interest in and to any Output owned by Suno"; free users may only use Outputs for "lawful, personal and non-commercial purposes". [Suno Terms](https://suno.com/terms)
- New download rule: "You may not commercially exploit Output that has not been downloaded by you through an approved channel"; download numbers per month "depends on your service tier". [Suno Terms](https://suno.com/terms)
- Other clauses: "Suno makes no representation or warranty to you that any copyright will vest in any Output"; "In no event will you use the Output or your Voice Model to compete with Suno"; "Nothing in this paragraph permits commercial use of any Remix." [Suno Terms](https://suno.com/terms)
- Stems: Auto Split up to 12 stems (Pro and Premier), Split from Mix (Pro and Premier), Advanced Split (Premier). Studio exports are WAV; Studio offers a multitrack export of all tracks as stems. [Suno Help: Exporting from Studio](https://help.suno.com/en/articles/13925249); [Dubspot](https://blog.dubspot.com/export-ai-stems-to-your-daw-2026)
- Studio 2.0 (13 Aug 2026) reportedly lets Premier export 32 bit / 48 kHz multitracks and stems with no cap, not counted against monthly downloads. Third party only, not confirmed in Suno notes. [Dubspot](https://blog.dubspot.com/suno-studio-2-0)

**Udio**
- UMG and Udio settled on 29 Oct 2025 and announced a new licensed platform for 2026; content would be kept in a "walled garden" during the transition. [UMG/Udio press release](https://tools.prnewswire.com/en-us/live/20813/release/20251029EN10848); [Digital Music News](https://www.digitalmusicnews.com/2025/10/29/umg-udio-deal-everything-we-know/)
- Udio turned off downloads on 30 Oct 2025 without warning, including for paying subscribers' older songs. [Digital Music News](https://www.digitalmusicnews.com/2025/10/31/udio-downloads-disabled-umg-deal/)
- After backlash it opened a one off 48 hour download window from 3 Nov 2025; songs downloaded then stay under the old terms (owned by the user). [Digital Music News](https://www.digitalmusicnews.com/2025/10/31/udio-umg-deal-downloads-resumed-48-hours/); [Billboard](https://billboard.com/pro/udio-deal-backlash-ai-users-download-ai-songs-48-hours)
- Warner later reached a similar settlement with Udio (Nov 2025). [RouteNote](https://routenote.com/blog/udio-stops-user-downloads-after-umg-deal-heres-why/)
- 2026 reviews say downloads are still paused pending the licensed relaunch and output is limited to personal, non commercial use. Secondary source. [Dupple](https://dupple.com/learn/suno-vs-udio)

**ElevenLabs Music (Eleven Music)**
- Music Terms "Last updated: 09 October 2026". Ownership: "you retain any and all rights in and to your Input and Output" (s.2.2). [ElevenLabs Music Terms](https://elevenlabs.io/music-terms)
- Media Rights, Free to Business: "All online and offline commercial use permitted, except film, TV, radio, and Studio Games". Enterprise Music has no exclusions. [ElevenLabs Music Terms](https://elevenlabs.io/music-terms)
- "Indie Game" (s.4.8(c)): game revenue under $500,000 since first commercial release, and developer/publisher game revenue under $1,000,000 in its last completed financial year. Indie Games are therefore allowed on Starter and up. If a game stops qualifying, "you have 60 days from that date to obtain a plan that permits use of Output in a Studio Game" (s.4.7); copies already distributed may keep the music. [ElevenLabs Music Terms](https://elevenlabs.io/music-terms)
- Exhibit A: Free, downloads "Not permitted", attribution required, streaming distribution prohibited. Starter, 30 min downloads/month, no attribution, streaming distribution prohibited. Creator, 250 min/month, streaming distribution permitted. Pro, 500 min/month, high quality downloads (lossless WAV or 192 kbps MP3) only from Pro up. [ElevenLabs Music Terms](https://elevenlabs.io/music-terms)
- Conflict: pricing page lists 192 kbps for Creator, while Exhibit A says high quality downloads start at Pro. [ElevenLabs pricing](https://elevenlabs.io/pricing); [ElevenLabs Music Terms](https://elevenlabs.io/music-terms)
- Cost: 900 credits per minute of music. [ElevenLabs pricing](https://elevenlabs.io/pricing)
- Music API: paid users only; composition plans (JSON, per section style, lyrics and length); streaming for interactive use; stems in self serve features; stem separation into 2, 4 or 6 stems (paid, also via API). Monthly API minute cap conflicts between localised pages (4,800 vs 6,600). [ElevenLabs Music API](https://elevenlabs.io/music-api); [ElevenLabs blog](https://elevenlabs.io/blog/eleven-music-new-tools-for-exploring-editing-and-producing-music-with-ai); [Music quickstart](https://elevenlabs.io/docs/cookbooks/music)

**Google Lyria (Gemini API)**
- Models: Lyria 3 Clip (`lyria-3-clip-preview`, always 30 s, "Short clips, loops, previews"); Lyria 3.5 (`lyria-3.5`, full songs, "A couple of minutes"). Output MP3 by default, WAV available for 3.5, "44.1 kHz high-fidelity stereo". All audio carries an inaudible SynthID watermark. No stems. Instrumental by prompting ("Instrumental only, no vocals"). [Gemini API music docs](https://ai.google.dev/gemini-api/docs/music-generation)
- Price: Lyria 3.5 "$0.08 per song"; Lyria 3 Clip Preview $0.04; Lyria 3 Pro Preview $0.08; no free tier. Lyria RealTime has no listed price. [Gemini API pricing](https://ai.google.dev/gemini-api/docs/pricing)
- Conflict: aggregators describe Lyria 3 as 48 kHz stereo; Google's current doc says 44.1 kHz. Lyria 2 (lyria-002) listed by an aggregator at $0.06 per 30 s. [OpenRouter](https://openrouter.ai/models/google/lyria-3-pro-preview); [CometAPI](https://www.cometapi.com/de/ai-music-api-pricing/)
- Terms ("Last updated 2026-04-28", effective 23 Mar 2026): "Google won't claim ownership over that content", but Google may generate similar content for others; Services are for "professional or business purposes"; UK, EEA and Swiss users get paid service data terms on all services. Unpaid service content can be used to improve Google's products. No music specific commercial clause found. [Gemini API Terms](https://ai.google.dev/gemini-api/terms)

**Stable Audio (music side)**: see section 1. 3.0 Medium/Large make instrumental tracks up to 6:20, outputs yours under the Community Licence below $1M revenue. [TechCrunch](https://techcrunch.com/2026/05/20/stability-ai-release-a-new-audio-model-that-can-create-six-minute-songs/); [Stability AI explainer](https://stability.ai/explainers/stable-audio-vs-competitors-licensing-export-rights-and-self-hosting-compared)

**AIVA**
- Free: 3 downloads/mo, "Copyright owned by AIVA", "No monetization", credit AIVA, MP3/MIDI, up to 3 min. Standard €11/mo + VAT (annual): 15 downloads, copyright owned by AIVA, monetisation limited to YouTube, Twitch, TikTok, Instagram (so not a game). Pro €33/mo + VAT (annual): 300 downloads, "Copyright owned by YOU", full monetisation, all formats including WAV and MIDI, up to 5:30. [AIVA pricing](https://www.aiva.ai/pricing)

**Soundraw**
- Prices shown in GBP: Creator £4.99/mo (unlimited, MP3); Artist Starter £9.08 (10, MP3); Artist Pro £10.75 (20, MP3 and WAV, stems); Artist Unlimited £14.92 (unlimited, MP3/WAV, stems); Enterprise custom with API. [Soundraw pricing](https://soundraw.io/pricing)
- Licence page (dated 30 Sep 2026): video games listed under "All Plans"; you get "a license (=permission) to use any track you download" (not ownership); Content ID registration not allowed; distributing to Spotify etc only after clearly modifying the beat. Meditation/lo-fi/relaxation content may only stay up while subscribed. [Soundraw licence](https://soundraw.io/license)

**Mubert**
- Ambassador free (25 tracks, MP3); Creator $14 ($11.69 annual); Pro $39 ($32.49 annual), labelled "Commercial usage", WAV and MP3; Business $199 ($149.29 annual) for apps and agencies. Separate Music API with custom pricing. [Mubert pricing](https://mubert.com/render/pricing)
- Licence: "Mubert owns all the rights to the tracks generated"; you may "Use our music in apps or games" under its Sync License; no Content ID; no resale or streaming distribution. Plan labels on the page are inconsistent (some "Non-Commercial" plans list "Commercial Usage"). [Mubert licence](https://mubert.com/render/license)

**Beatoven.ai**
- Terms ("Last update: 5 June 2024", may be stale): licence is "non-exclusive, limited, royalty-free, perpetual, worldwide", only as synchronised with your "video, storybook, game, podcast or other audio-visual content"; "Beatoven shall own and be the copyright owner of Your AI Music"; credit required "wherever practicable"; s.6.3(c) excludes the right to "reproduce, copy, download or otherwise sell copies of, or files embodying" the music. [Beatoven Terms](https://www.beatoven.ai/terms-of-use)
- Prices (third party): Free (no downloads, no commercial licence), Creator $10/mo, Visionary $20/mo, pay as you go $3/min. [CostBench](https://costbench.com/software/ai-music-apis/beatoven-ai/); [Toolradar](https://toolradar.com/tools/beatoven)

**Loudly**
- Third party: Free non commercial, 1 download/day; Personal (commercial) from about $10/mo, 900 creations/mo, up to 7 min; Pro, 3,000 creations/mo, sub licensing to freelancers, ad campaigns up to $100K/yr. [CostBench](https://costbench.com/software/ai-music-generators/loudly/)
- Perpetual, worldwide royalty free licence is stated on Loudly's API page (custom pricing). [Loudly Music API](https://www.loudly.com/?p=3134)

**Soundful**
- Third party: free plan personal use with attribution; Pro about $9.99/mo with stem packs and MIDI; Business licence covers "apps, games, film, TV" for content over 100K monthly users, for companies under $1M revenue; Business price quoted variously as $45.99, $49.99 or $120.83/mo. [Toolradar](https://toolradar.com/tools/soundful/pricing); [AI Tool Curator](https://www.aitoolcurator.com/ai-tools/content-creation/soundful/); [Vidmuse](https://vidmuse.ai/blog/soundful-ai-review)

**Adobe Firefly Generate Music**: GA 20 Aug 2026, Firefly Music Model, "universally licensed original tracks tuned to your video's length and mood". [Adobe blog](https://blog.adobe.com/en/publish/2026/08/20/adobe-firefly-expands-its-creative-ai-studio-generate-music-speech-and-sound-effects-in-one-place)

### Inferences
- Rough fit for a commercial Steam game by a UK solo dev (my reading, not legal advice):
  - Good on paper: ElevenLabs Music Starter+ (indie game clause), Stable Audio paid hosted, AIVA Pro, Suno Pro/Premier (with download caps), Soundraw (licence, not ownership), Lyria via paid Gemini API.
  - Risky or unclear: Mubert (they own it), Beatoven (they own it, and 6.3(c) arguably conflicts with shipping audio files inside a game build), Loudly, Soundful (third party info only), Adobe (per feature terms not read).
  - Not usable now: Udio (no downloads), AIVA Standard and any free tier.
- Suno's real monthly price is probably $10 (Pro) and $30 (Premier) on monthly billing, given the page shows $8 and $24 with yearly savings of $24 and $72. Not shown directly.
- Suno's 20 downloads/month on Pro could be tight for a game soundtrack with many variants; Premier Studio exports may avoid the cap if the Dubspot report is right.
- ElevenLabs Starter forbids streaming distribution, so releasing the soundtrack on Spotify or Bandcamp style services would need Creator or higher.

### Gaps
- Udio: no confirmed 2026 launch date or restored downloads found as of 13 Oct 2026; Udio's terms page did not render.
- Suno exact download caps and pricing of extra downloads came from secondary reporting plus the pricing page; Suno's own downloads policy page was not read.
- Stable Audio, Loudly, Soundful and Beatoven current official pricing pages did not render or 404'd; figures are third party.
- Lyria RealTime pricing and availability for commercial use not found.
- Whether Gemini API output can be used in a sold game is not addressed music specifically; only general ownership wording found.
- AIVA monthly (non annual) prices not shown.

## 3. Stems and seamless loops (for adaptive game music)

### Takeaway
Real stem output exists in Suno (up to 12 stems, WAV), ElevenLabs Music (2, 4 or 6 stem separation and API stems), Soundraw (Artist Pro and up), Soundful (stem packs) and Mubert (stem editing). Seamless loops are explicitly offered only by ElevenLabs Sound Effects (loop switch, MP3) and loosely by Lyria 3 Clip ("loops" use case). No hosted tool found that outputs ready made interactive layers for Unity, FMOD or Wwise.

### Cited Findings
- Suno: Auto Split up to 12 stems (Pro/Premier), Advanced Split (Premier), Studio multitrack export, WAV exports. [Suno pricing](https://suno.com/pricing); [Suno Help](https://help.suno.com/en/articles/13925249); [Dubspot](https://blog.dubspot.com/export-ai-stems-to-your-daw-2026)
- Some Suno projects split into 5 to 12 stems, not always 12. [Dubspot](https://blog.dubspot.com/export-ai-stems-to-your-daw-2026)
- ElevenLabs Music: stems listed in Music API self serve features; stem separation into 2, 4 or 6 stems, paid feature, API separate stems endpoint; composition plans control each section's length. [ElevenLabs Music API](https://elevenlabs.io/music-api); [ElevenLabs blog](https://elevenlabs.io/blog/eleven-music-new-tools-for-exploring-editing-and-producing-music-with-ai)
- ElevenLabs SFX loop option, max 30 s, MP3 only for loops. [ElevenLabs SFX docs](https://elevenlabs.io/docs/capabilities/sound-effects)
- Soundraw stems on Artist Pro (£10.75/mo) and up. [Soundraw pricing](https://soundraw.io/pricing)
- Mubert "Edit Stems" (free and Creator), "Full Track Editor" (Pro, Business). [Mubert pricing](https://mubert.com/render/pricing)
- Soundful stem packs on Pro, 35 stem packs/mo on Business (third party). [Toolradar](https://toolradar.com/tools/soundful/pricing)
- AIVA Pro exports MIDI and WAV (MIDI lets you rebuild layers yourself). [AIVA pricing](https://www.aiva.ai/pricing)
- Lyria 3 Clip is pitched for "Short clips, loops, previews"; no stems. [Gemini API music docs](https://ai.google.dev/gemini-api/docs/music-generation)
- Common 2026 workflow advice: generate layers or stems with AI, then assemble adaptive logic in the engine or FMOD/Wwise. Vendor blog. [Soundverse guide](https://www.soundverse.ai/blog/article/ai-music-for-game-developers-complete-guide-1309)

### Inferences
- Stem separation (splitting a finished mix) is not the same as composed layers; separated stems can have bleed. For vertical layering in Unity, Suno Studio multitrack or AIVA MIDI give cleaner control than AI separation.
- Loop points for music will mostly need manual editing in a DAW or Audacity, whatever the tool.

### Gaps
- No hosted AI tool found that exports game ready interactive layers or loop points (e.g. intensity tiers with matched bars) directly for Unity/FMOD/Wwise.
- Stable Audio stems and loop features not confirmed from official pages.
- ElevenLabs Music stem output format and sample rate not stated.

## 4. Licence rules: free tier, ownership, free to paid upgrade, 2025 label deal changes

### Takeaway
Every tool checked bars commercial use on the free tier. Paid tiers split into "you own it" (Suno Pro/Premier assignment, ElevenLabs, Stable Audio, AIVA Pro, Google "won't claim ownership") and "they own it, you get a licence" (Mubert, Beatoven, Soundraw). Free tier songs do not become commercial after upgrading on Suno. The 2025 label settlements cost Udio its downloads and pushed Suno to licensed v6 models, paid only downloads with monthly caps, and a rule that only properly downloaded songs can be used commercially.

### Cited Findings
- Suno free: personal, non commercial only; no downloads. [Suno Terms](https://suno.com/terms); [Suno pricing](https://suno.com/pricing)
- Suno upgrade: a Suno help article (edited 17 Dec 2025, reported second hand) says upgrading does not automatically give free plan songs commercial rights; Suno may grant retroactive rights case by case. [Jack Righteous](https://jackrighteous.com/en-gb/blogs/guides-using-suno-ai-music-creation/mailbag-if-i-pay-for-one-month-of-suno-ai-can-i-monetize-my-songs); [Dynamoi](https://dynamoi.com/learn/guides/suno-commercial-rights-explained)
- Same source says under the 3 Sep 2026 terms, rights from a qualifying commercial download are perpetual. Not confirmed in the clauses fetched. [Jack Righteous](https://jackrighteous.com/en-gb/blogs/guides-using-suno-ai-music-creation/mailbag-if-i-pay-for-one-month-of-suno-ai-can-i-monetize-my-songs)
- Suno and Warner settled in Nov 2025; Suno said downloads would need a paid account, free songs would be play and share only, paid users would get monthly download caps with paid extras, and old models would be deprecated when licensed models launched in 2026. [Billboard](https://billboard.com/pro/suno-warner-music-sign-ai-licensing-deal-settle-lawsuit/); [Music Business Worldwide](https://www.musicbusinessworldwide.com/warner-music-group-settles-with-suno/); [Digital Music News](https://www.digitalmusicnews.com/?p=334073)
- Those changes are now live: v6 (9 Sep 2026), Pro 20 / Premier 60 downloads per month, caps reported to apply to older songs too. UMG and Sony are reported still in litigation with Suno; a German court ruled against Suno in a GEMA case. [The Rundown](https://www.therundown.ai/news/suno-v6-music-partners-paid-fan-remixes); [The Next Web](https://thenextweb.com/news/suno-v6-warner-bmg-believe-licensed-models)
- Udio: downloads off since 30 Oct 2025 after the UMG deal; one 48 hour window in Nov 2025; walled garden. [Digital Music News](https://www.digitalmusicnews.com/2025/10/31/udio-downloads-disabled-umg-deal/); [Digital Music News](https://www.digitalmusicnews.com/2025/10/31/udio-umg-deal-downloads-resumed-48-hours/)
- ElevenLabs: free = non commercial (s.1(c)(i)); paid = commercial; you retain rights in Output. Music free tier: no downloads, attribution required. [ElevenLabs Terms](https://elevenlabs.io/terms-of-use); [ElevenLabs Music Terms](https://elevenlabs.io/music-terms)
- Stable Audio: free not cleared for commercial; Pro and higher outputs stay covered after cancelling; outputs yours under Community Licence. [stableaudio.com FAQ](https://stableaudio.com/faqs); [Stability AI explainer](https://stability.ai/explainers/stable-audio-vs-competitors-licensing-export-rights-and-self-hosting-compared)
- AIVA: only Pro gives "Copyright owned by YOU". [AIVA pricing](https://www.aiva.ai/pricing)
- Mubert: "Mubert owns all the rights". Beatoven: "Beatoven shall own and be the copyright owner". Soundraw: "a license (=permission)". [Mubert licence](https://mubert.com/render/license); [Beatoven Terms](https://www.beatoven.ai/terms-of-use); [Soundraw licence](https://soundraw.io/license)
- Google: "Google won't claim ownership over that content". [Gemini API Terms](https://ai.google.dev/gemini-api/terms)
- Steam (Jan 2026 rewrite): devs must disclose AI generated content shipped in the game or on the store page (audio included); AI dev tools used behind the scenes no longer need disclosure. [Gigazine](https://gigazine.net/gsc_news/en/20260120-steam-updates-ai-disclosure-guidelines); [Notebookcheck](https://www.notebookcheck.net/Steam-updates-AI-disclosure-form-requiring-developers-to-report-visible-and-in-game-AI-but-not-background-tools.1206103.0.html); [Slashdot](https://games.slashdot.org/story/26/01/19/1735231/valve-has-significantly-rewritten-steams-rules-for-how-developers-must-disclose-ai-use)

### Inferences
- "You own it" in these terms is contractual. Suno openly says copyright may not vest at all. For a UK dev this means the assets are usable but may be hard to protect against copying.
- Practical rule: only generate shippable audio while on a paid plan, download it through the official button during that subscription, and keep receipts, plan and date per file.
- Any AI audio shipped in the game must be ticked on Steam's AI disclosure form.

### Gaps
- UK copyright position (CDPA s.9(3) computer generated works and the 2025/26 government consultation) was not researched here.
- Adobe Firefly audio specific terms and IP indemnity for individual plans not read first hand.
- Suno's help article on retroactive rights and its downloads policy page were not opened directly.

## 5. Use in shipped games and dev recommendations

### Takeaway
No credible named Steam game was found that publicly shipped with Suno, Udio or ElevenLabs music or SFX and drew coverage. Evidence is small: itch.io devs crediting Suno, guides calling ElevenLabs the cleanest licence, and forum warnings about disclosure and backlash.

### Cited Findings
- An itch.io dev credited a game's intro song to Suno with no visible backlash in the thread. [itch.io post](https://itch.io/post/14745891)
- Another itch.io dev described generating about 15 Suno versions per track before choosing. [itch.io post](https://itch.io/post/11137557)
- A Reddit commenter (mirrored snapshot, July 2026) warned that Steam needs AI music disclosed and that ads with AI music may get "brigaded". Anecdote. [Reddit mirror](https://reddit.sentinel-team.org/posts/1uppsyv/snapshots/2026-07-10T23%3A40%3A07.6471Z)
- A 2026 game music guide calls ElevenLabs Music the cleanest licensing for a shipped game and flags litigation tail risk for Suno. SEO style source. [Cinevva guide](https://app.cinevva.com/hi/guides/ai-music-generators-games)

### Inferences
- Player backlash risk in 2026 coverage centres on AI voices and art more than SFX; AI music sits in between.

### Gaps
- No r/gamedev or r/IndieDev thread was opened directly; search tools returned mirrors and SEO pages.
- No named shipped Steam games using these tools for audio were confirmed.
- A search summary claimed about one in five games in Steam's content survey carried an AI disclosure by July 2026, but the original source could not be pinned down, so it is left out of the findings.
