# Steam trailer craft for a pixel art indie game (researcher 4, 16 Oct 2026)

Sources marked "opened" were read; "search summary" were not opened. The researcher also checked Steam's live store player directly in a browser on 16 Oct 2026.

Short answer: the current 1440x900 recorder output is the wrong master. Use a 480x270 crop at 4x (exactly 1920x1080) for Steam and 8x (3840x2160) for YouTube. Gameplay in the first second, hook within 10 to 15 s, about 60 s, and make it work with no sound.

## Steam requirements
- Upload: highest resolution up to 1920x1080, 30/29.97 or 60/59.94 fps, 5,000+ kbps, .mov/.wmv/.mp4, H.264 and AAC preferred, 16:9 preferred, 44 or 48 kHz audio. Unusual resolutions are a common cause of failed processing; use 1920x1080 or 1280x720. [Steamworks: Trailers](https://partner.steamgames.com/doc/store/trailer)
- The first two trailers show before the screenshots; Valve strongly recommends the first be mostly gameplay with the HUD visible. Viewers in the Discovery Queue have "less than 10 seconds" and may watch without audio. Custom thumbnails must be a frame from the video, 1920x1080, avoid text. Microtrailers are 6 second loops Steam builds from the first visible video (six 1 second clips), not customisable. (same page; [Game Developer, May 2023](https://www.gamedeveloper.com/business/valve-imposing-new-trailer-restrictions-on-steam-developers))
- July 2025 player upgrade re-encoded about 400,000 trailers. [GamingOnLinux, Aug 2025](https://www.gamingonlinux.com/2025/08/valve-finally-upgraded-the-steam-trailer-video-player-re-encoding-around-400000-videos/)
- Next Fest needs at least one public trailer; Valve uses only that trailer. October 2026 registration closed 31 Aug; the next edition is planned for February 2027. [Steamworks: Next Fest](https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest)
- Direct check, 16 Oct 2026: the first trailer autoplayed with muted true. The player serves AV1 and H.264 at 1920x1080 about 5,800 kbps, 1280x720 2,600, 854x480 1,400, 640x360 1,000; AAC 48 kHz 192 kbps. Frame rate kept as uploaded (Celeste 30 fps, Balatro 60, Vampire Survivors 60, Hades 59.94).
- OPINION: uploading above 1080p gains nothing on Steam. At about 5.8 Mbps, 30 fps gets about twice the bits per frame of 60 fps, so a mostly static pixel camera may survive the encode better at 30. Keep the first trailer free of long black frames, logos and cards, since any second could be picked for the microtrailer. Build one strong text free hero frame for the thumbnail. A 1440x900 (16:10) upload is a risk on two counts.

## Capture without blur or shimmer
- ffmpeg `neighbor` is nearest neighbour; other flags blend. [ffmpeg scaler docs](https://ffmpeg.org/ffmpeg-scaler.html)
- 4:2:0 colour bleeds on pixel art unless scaled by an even factor first; scaling by 3 still bleeds (search summary). [TASVideos forum](https://tasvideos.org/Forum/Posts/443481)
- YouTube: H.264 High, 4:2:0, 1080p 8 Mbps at 30 fps or 12 at 60; 2160p 35 to 45 or 53 to 68 Mbps. [YouTube help](https://support.google.com/youtube/answer/1722171?hl=en). Practitioners upscale pixel footage to 4K so YouTube gives it more bitrate (search summary).
- 480x270 is exact 16:9 and maps to 1080p at 4x.
- Unity: Pixel Perfect Camera Upscale Render Texture and Pixel Snapping [URP 14](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/2d-pixelperfect.html); `Time.captureDeltaTime` advances game time by a fixed step for constant rate capture, but does not affect `Time.unscaledTime` [Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Time-captureDeltaTime.html); Unity Recorder works in the Editor only, not in builds [Recorder 4.0](https://docs.unity3d.com/Packages/com.unity.recorder@4.0/manual/index.html).
- Lieu faulted a 2026 trailer for stuttery frame rate and motion blur. [Derek Lieu, 23 Aug 2026](https://www.derek-lieu.com/blog/2026/8/23/dear-passengers-vs-banana-airways)

Framing a 480x300 game in 16:9 (researcher's arithmetic):
| Option | Result | Verdict |
|---|---|---|
| Crop 480x270 then 4x or 8x | 1920x1080 or 3840x2160 exact, full bleed, even factor | Best if no key HUD sits in the 30 cropped rows |
| 3x 1440x900 centred in 1920x1080 | bars, 62.5% of frame, odd factor bleeds | Weak |
| 3.6x to 1728x1080 | uneven pixels, shimmer | Avoid |
| A 480x270 trailer camera in engine | composed for 16:9 | Best if cheap |
- OPINION pipeline: pipe the 1x 480x300 render as RGB into ffmpeg losslessly (FFV1 or PNG) with a fixed capture step; crop, scale and encode later. Whole number zooms only; shake and pans in whole source pixels. 30 fps is fine for a mostly static auto battler camera. The game's own deterministic recorder beats OBS.

## Structure, length, mistakes
- Genre in 1 to 2 shots, then hook, then content; the hook ideally in the first 10 to 15 s and no later than halfway. [Derek Lieu, Oct 2022](https://www.derek-lieu.com/blog/2022/10/24/how-to-hook-the-audience-and-how-quickly-to-do-it)
- Length bands: 0 to 30 s snack, 30 to 90 appetizer, 90 to 180 entree; aim 90 s or fewer. [Lieu, Dec 2018](https://www.derek-lieu.com/blog/2018/12/20/ideal-game-trailer-length-and-labeling)
- Social structure: thumbstopper 1 to 10 s, intro 10 to 15, escalation about 40, climax 20 to 30; make a 30 s cutdown. [Lieu, Feb 2023](https://www.derek-lieu.com/blog/2023/2/12/how-to-optimize-a-game-trailer-for-social-media)
- Zukowski (Sept 2025): one minute is fine; The Alters' 3:23 cut lost viewers steadily while its 1:30 did not; slow panning lore dumps underperform. [HTMAG, 25 Sep 2025](https://howtomarketagame.com/2025/09/25/a-deep-dive-in-the-games-of-the-spring-showcases/)
- Failbetter: about 35% of viewers lost in the first two seconds; sweet spot 50 to 60 s; 60 to 65% retention is good for a launch trailer. [PCGamesInsider, 2019](https://www.pcgamesinsider.biz/interviews-and-opinion/68588/insight-how-effective-were-failbetters-trailers-for-sunless-skies/)
- Lieu's 10 mistakes: unknown logos up front; slow intro; repeated content; no game audio; bad eye trace; looping game music with no build; unfocused capture (plan, rehearse, capture); information overload; generic title cards; scattered story; plus not differentiated. [Lieu, Sep 2020](https://www.derek-lieu.com/blog/2020/9/14/10-common-indie-game-trailer-mistakes-and-how-to-fix-them)
- Capture for the backseats: big, dramatic, full screen actions; a 20% stat boost is invisible. [Lieu, Nov 2019](https://www.derek-lieu.com/blog/2019/11/25/capture-for-the-backseats)
- OPINION: for an auto battler, the hook must show the player's decision then the fight that pays it off. The deterministic sim lets the same seed be replayed for many takes, HUD on or off. HUD on in the Steam gameplay trailer; reduce it in hype cuts.

## Muted autoplay, text, music
- Build it to work silent like a GIF; text on its own cards, not over gameplay; keep game SFX for people who unmute. [Lieu, 2021](https://www.derek-lieu.com/blog/2021/4/18/the-simplest-trailer-to-make-for-your-steam-page); [Lieu, Jan 2020](https://www.derek-lieu.com/blog/2020/1/10/why-your-game-trailer-needs-sound-effects-on)
- Trailer music shape: slow intro, builds, climax, a fake out stopdown of silence, a final button on the title. [Lieu, Apr 2018](https://www.derek-lieu.com/blog/2018/4/17/how-production-music-teaches-trailer-structure)
- Title cards: start from three words, use verbs, lead with the message; test the cut with all cards removed. [Lieu, Apr 2020](https://www.derek-lieu.com/blog/2020/4/19/how-to-write-trailer-title-cards-with-strong-messaging). Tell, Show, Repeat. [Lieu, Apr 2023](https://www.derek-lieu.com/blog/2023/4/9/tell-show-repeat-the-2nd-easiest-game-trailer-to-make)
- OPINION: ask the audio collaborator for a cue built to the trailer shape at a fixed BPM that gives whole frames per beat (at 30 fps: 90 BPM = 20 frames, 120 = 15, 150 = 12), plus stems and hit points in frames. Have the sim log the time of every hit and kill so cuts can line up by code.

## Tools, and building from code
- Premiere has no native nearest neighbour scaling; DaVinci Resolve free exports up to UHD 60 fps; Shotcut has a nearest neighbour export setting (search summaries).
- ffmpeg xfade includes fadewhite and pixelize. A code pipeline (crop 480x270, scale neighbor to 1920x1080, white flashes, whole pixel shakes, pixel font cards rendered as 480x270 PNGs, one music file, an edit list) is reliable for hard cuts and flashes; complex motion graphics are easier in an editor. Commands were not run by the researcher.

## Trailers and wishlists
- Hard data is thin and mostly anecdotal. Showcase medians and a good trailer editor costing about $10K for a 30 s to 1 min trailer. [HTMAG, Sep 2025](https://howtomarketagame.com/2025/09/23/what-are-game-showcases/)
- We Harvest Shadows: most wishlists came from content creators; graphics attract, the demo sells. [HTMAG, Nov 2024](https://howtomarketagame.com/2024/11/18/how-we-harvest-shadows-220000-wishlists-in-3-months/)
- Channel matters: the same Spelunky trailer got about 100,000 views on Nintendo's channel and 7,971 on the developer's. [HTMAG, 2022](https://howtomarketagame.com/2022/03/21/if-you-build-it-they-wont-come-marketing-fallacies-and-missed-opportunities/)
- How many trailers: two in front, gameplay first; hide old ones; keep a 30 s announcement cut for an exclusive, plus 30 s Reddit and 15 s TikTok cuts. [HTMAG, Dec 2023](https://howtomarketagame.com/2023/12/05/why-am-i-not-getting-wishlists-a-step-by-step-solution/)
- OPINION plan: a 45 to 75 s Steam gameplay trailer first, HUD on, gameplay from frame one; a 30 s announcement cut held back; 6 s and 15 s social cuts; a launch trailer later. The store trailer must be live before Next Fest registration closes.

## Gaps
Steam gives no length guidance; how Steam displays a 16:10 upload not checked; Kindred Siege's HUD position against a 30 row crop not checked; no reliable views to wishlists dataset.
