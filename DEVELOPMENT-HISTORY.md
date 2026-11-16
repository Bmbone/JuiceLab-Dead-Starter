# Development history

These are archived implementation checkpoints. Their scene names, verification notes and “next checkpoint” statements describe the project at that time. For the final project **After**, current run instructions and submission notes, use [README.md](README.md).

## Giant's Grip and regression checkpoint — change 14

Open `Assets/_Project/Scenes/Medieval14_GiantGripVFX.unity`. Giant's Grip gains an emerald outline/glow around the enlarged paddle, gold end runes, a short rising activation burst and a softer gold expiry burst. A large ENLARGED / RESTORED plaque sits above Settings beside the left pillar. The earlier Fireball and Rune Shield presentation remains present.

`GiantGripVfx/GiantGripVisualFeedback.cs` observes the existing `AbilityPaddle.Width` after gameplay updates and positions its decorations from the real BoxCollider2D shape transformed by the current paddle pose. Automatic physics syncing is off in this project; calculating these bounds from the current transform avoids depending on a cached physics pose. It never changes paddle position/scale, collider, controls, ability state, sound, duration or cooldown. The original 3-unit width, 1.6x expansion to 4.8 units, six-second effect and twelve-second cooldown remain unchanged. End runes sit inside the actual reach; the faint outer glow is decorative. No substitute countdown is displayed.

The retained noninteractive plaque uses the HUD's borrowed font at 22/32 points. It stays above the existing Settings button, hides when less than 272 reference units of left gutter are available, and remains below menus/settings. On normal expiry the label briefly changes to RESTORED; life loss, score-zero resets and menu/results clear everything without reporting an expiry. Re-enabling during an active spell resumes its appearance without repeating an activation burst. Animation uses scaled time, so it pauses with the round.

| Setting / resource | Default |
|---|---:|
| Aura Opacity | 0.65 |
| Enable Motes / Pool | On / 8 sprites |
| Activation mote lifetime | 0.32–0.44 s |
| Expiry mote lifetime | 0.22–0.30 s |
| RESTORED confirmation | Up to 0.40 s, including fade |
| World renderers | 11 reused sprites; no new physics or audio voice |

Select `Giant Grip VFX - Emerald Reach 14` to tune opacity or disable motes. One owned white-texture quad, three shared procedural URP materials, property blocks and the fixed pool are allocated once. No per-use GameObjects, sprite/material instances or audio voices are created. A private System.Random stream affects cosmetics only. Disable removes only the new overlays, leaving the original paddle presentation intact; destruction releases owned UI/quad resources and retains the borrowed font and material assets.

The combined source regression pass is complete: **12 existing tests + 7 added asset/scene checks pass**. `tests/verify_polish.py` checks every scene/prefab's local object graph, metadata uniqueness/targets, build-scene GUIDs, latest project-script fields, scene preservation, new materials and eight PCM WAV payloads. Additional checks parse all 63 project C# scripts, 20 scenes, 6 prefabs, 22 materials, the mixer and 9 serialized assets, and sample shader bounds, glyph advances and 16:9/4:3 badge layout. No source/schema failures remain in these checks.

All 372 prior Assets blobs remain unchanged. This scene adds only the independent Giant's Grip root/transform/controller plus its SceneRoots entry. **Unity and .NET are unavailable here**, so C# semantic compilation, URP shader compilation, the three executable rule harnesses, listening/visual review, actual pause/reset/input behavior, all nine level/difficulty playthroughs and build/performance checks remain pending. Source checks do not establish engine correctness. The full current matrix and commands are in TESTING.md.

The planned visual polish pass is implemented. The remaining stage is Unity verification and any fixes its results identify.

## Rune Shield visual checkpoint — change 13

Open `Assets/_Project/Scenes/Medieval13_RuneShieldVFX.unity`. Rune Shield now appears as a jade/gold ward with geometric runes, two crystal-like end anchors, a gold deflection pulse and a fixed burst of rising shards. An ARMED / DEFLECTED plaque appears beside the right pillar. Its one-use protection, reflection position/velocity, duration, cooldown, cast keys and existing sound cues remain unchanged.

The original shield line sits at y=-5.15, mostly behind the enlarged spell cards. The new ward keeps its lower edge on that same plane and places its rune row 0.30 world units above it, in the visible gap above the hotbar. All world elements sort behind the normal ball and paddle. The noninteractive status plaque stays outside the court and hides when the camera/Canvas leave less than 272 reference units of right gutter; menus and settings render above it.

`RuneShieldVfx/RuneShieldVisualFeedback.cs` observes the existing `Rune shield` LineRenderer after the ability controller updates it. Visibility indicates the native shield/flash presentation; its existing `0.065 + flash * 0.15` width marker identifies a successful deflection. The new component does not cast, consume protection, predict a collision or change reflection. It hides only original rendering with forceRenderingOff, preserving enabled state, geometry, widths, colors and material. Disable restores that rendering; a mid-ability re-enable resumes the current appearance without emitting another activation/deflection burst.

The new native URP shader and three shared materials draw ward, anchor and shard masks from one owned white-texture quad. The ARMED badge follows active shield presentation; after use it switches to DEFLECTED and briefly lingers as confirmation, rather than implying renewed protection. Expiry hides the ward/armed badge without a deflection burst. A rising source flash emits twelve shared-pool shards across the rune row; it does not claim an exact impact point.

| Setting / resource | Default |
|---|---:|
| Ward Opacity | 0.75 |
| Enable Shards / Pool | On / 12 sprites |
| Shard lifetime | 0.22–0.32 s |
| Deflection badge confirmation | Up to 0.45 s, including fade |
| World renderers | 15 reused sprites; no new physics or audio voice |

Select `Rune Shield VFX - Ward and Deflection 13` to tune opacity or disable shards. The retained badge borrows the HUD font. No per-cast GameObjects, sprites or material instances are created; a separate fixed-seed System.Random stream controls only decorative shards. Scaled ages/animation pause with the round. Life loss, score-zero reset, menus/results and disable clear the new presentation, while renderer visibility and owned resources are restored/released on disable/destruction.

All prior Assets files remain frozen. The new scene adds only the independent Rune Shield root/transform/controller and its SceneRoots entry, with new script/shader/material assets. Fireball VFX, chapter/result UI, pause/preferences, audio/music, physics and other abilities keep their earlier configuration. Source checks pass for C# syntax, scene/material/GUID wiring, prior-asset preservation, source flash/visibility contracts, physical-plane/rune/badge layout, mask/shard bounds and lifecycle ownership. Unity is unavailable here: engine/shader compilation, actual glyph/alpha rendering, hotbar visibility, deflection detection at real frame rates, pause/cleanup and play-mode checks remain pending in TESTING.md.

Next proposed checkpoint: Giant's Grip visual feedback, in another new scene.

## Fireball visual checkpoint — change 12

Open `Assets/_Project/Scenes/Medieval12_FireballVFX.unity`. Fireball now has an ivory-hot core, an animated orange/gold flame flowing behind its upward motion, a faint warm halo, a softer short ribbon, a small expanding cast ring and fading embers. The new scene changes Fireball presentation only. Its existing piercing sweep, radius, speed, lifetime, cooldown, score path and sound remain unchanged; Giant's Grip and Rune Shield retain their current presentation.

`FireballVfx/FireballVisualFeedback.cs` observes AbilityEffects' existing `Fireball - reused` transform after gameplay updates. It never starts, moves, stops or damages with the logical projectile. Its original renderers are hidden with forceRenderingOff while keeping their materials, colors, enabled states and trail configuration available. Disabling the new component restores the original rendering. Re-enabling during flight resumes the new skin without replaying a cast ring in midair.

The new URP 2D shader and four shared materials supply procedural flame, soft glow, ribbon and ring masks. One owned full-rectangle sprite uses Unity's built-in white texture; no downloaded image or new audio is needed. The nominal 0.22-unit head is centered on the existing gameplay position, with the flame tail trailing below it. Halo, ribbon and embers sort behind the regular ball, while the Fireball head retains its high visibility.

| Setting / resource | Default |
|---|---:|
| Enable Embers | On |
| Ember Opacity / Halo Opacity | 0.70 / 0.20 |
| Ember pool | 16 reusable sprites |
| Ember emission / Lifetime | 32 per second, at most 4 per frame / 0.16–0.30 s |
| Ribbon duration / Initial width | 0.13 s / 0.24 world units |
| Cast ring lifetime | 0.16 s |

Select `Fireball VFX - Flame Trail and Cast 12` to adjust the two opacity sliders or disable embers. The effect creates its 19 SpriteRenderers, one TrailRenderer, property blocks and quad once. No per-cast GameObjects, sprite/material instances, colliders or AudioSources are created. A separate fixed-seed System.Random stream affects only cosmetic embers, leaving Unity's gameplay random stream unchanged.

Normal ceiling expiry stops the head/emission and lets the short ribbon/embers finish. Life loss, score-zero reset, menu/results and disable clear the new effects; recast repositions with emission off and clears old history to avoid a bridge back to the paddle. Flame age, ember movement and cast-ring timing use scaled time so they pause with the round. This adapter depends on the current reused-fireball hierarchy and lifecycle; TESTING.md includes the required engine checks.

All earlier Assets files remain frozen. The new scene adds only an independent VFX root/transform/controller and its SceneRoots entry, plus new shader/material assets. Existing chapter banners, result layout, pause/settings/preferences, music/SFX, ball/paddle, brick effects, physics and game rules retain ChapterPresentation's configuration. Source checks pass for C# syntax, scene/material/GUID wiring, preserved assets/blocks, centered head geometry, bounded shader masks/emission/pool and lifecycle/source-visibility ownership. Unity is unavailable here: engine/shader compilation, actual appearance, trail/vertex-color rendering, pause/cleanup and play-mode verification remain pending.

Next proposed checkpoint: Rune Shield visual feedback, followed by Giant's Grip in a separate scene.

## Chapter presentation checkpoint — change 11

Open `Assets/_Project/Scenes/Medieval11_ChapterPresentation.unity`. Starting or retrying a round now briefly displays a heraldic banner with the level number, name and difficulty beside the left castle pillar. Victory and game-over screens gain castle shield seals, a framed final score, gold/coral outcome accents and a clearer next-step line. The exact score and original result buttons remain owned by LevelMenuView.

| Element | Presentation |
|---|---|
| Level introduction | 232x300 banner in the left gutter; 32-point serif level name |
| Banner timing | 2.0 s default, with a short fade in/out and no launch delay |
| Final score | Original exact score text, enlarged to 60 pt inside a framed plaque |
| Victory / defeat | Gold CLEAR seal / coral RETRY seal |
| Next-step line | Next chapter name, final-clear encouragement or retry prompt |

`ChapterPresentation.cs` runs after MedievalInterfacePolish and waits for the existing generated UI. It observes score-zero round resets, then reads only the original mode label's `L<number> NAME | DIFFICULTY` context to identify the level/difficulty. It never parses or replaces the score, adds points, changes game state, adds button listeners or changes selection/navigation. Result visibility comes from the existing Return/Try Again/Next button states. The three catalog formations, nine mode combinations and immediate start/result timing are preserved.

The banner sits outside the original left-wall collision face, with a 16-unit outer margin and 24-unit safety allowance. It hides if the camera projection and Canvas scale leave less than 272 reference units of gutter. It never covers the court to perform a countdown. Its age uses scaled time, so the existing pause freezes the remaining display time; a non-final lost life does not replay it. All banner/decor images and text ignore raycasts. Its Canvas sorts below menus and the settings modal.

`ChapterPresentationView.cs` creates one reusable banner and result-decoration hierarchy from native UI shapes, including castle shield motifs. Decorative result elements fade in over 0.18 seconds while the existing score and actions are available immediately. The score's font size/rectangle are restored when leaving results or disabling the new component, preserving any new menu style already applied by its original owner. Fonts are borrowed from the existing HUD/interface; no new font/material/audio asset or per-round UI hierarchy is created.

All existing Assets files stay frozen. The cloned scene adds only an independent chapter-presentation root/transform/controller and its SceneRoots entry. Pause/settings, saved preferences, audio/music, physics, controls, ability rules, score/lives and old UI callbacks retain CourtSettings' configuration. Source checks pass for C# syntax, scene/GUID wiring, preserved assets/blocks, context formatting and catalog coverage, banner/crest/score layout bounds, fade bounds and event/font/score ownership. Unity is unavailable here: engine import/compilation, actual font layout/layering, gutter visibility and pause/menu/result play-mode review remain pending in TESTING.md.

Next proposed checkpoint: clearer spell visuals and ability feedback, in another new scene.

## Court settings checkpoint — change 10

Open `Assets/_Project/Scenes/Medieval10_CourtSettings.unity`. Press **Esc** or click **Pause / Esc** at the lower left during a round to open a large jade-and-gold pause panel. The same entry reads **Settings** on menus/results. The panel uses the existing serif heading typeface, a 76-point title, 33-point setting labels, percentage readouts and large controls on a 1920x1080 Canvas. It adds Music and Sound Effects sliders, independent Screen Shake and Brick Motion switches, Resume/Close, Main Menu during play, and Restore Defaults.

| Control | Behavior |
|---|---|
| Esc / Resume | Open or close the modal; resume the saved time scale |
| Music | Adjust the two arrangements together, independently of effects |
| Sound Effects | Adjust impacts, life/result cues and all three spell voices |
| Screen Shake / Brick Motion | Independently suppress their world-camera presentation |
| Restore Defaults | Music/SFX 100%, both motion options on |
| Up/Down / Left/Right / Enter | Navigate settings / adjust sliders / activate a control |

`CourtSettingsPanel.cs` owns the pause only when a round is playing. It saves Time.timeScale, sets it to zero and restores its saved value on close/disable/destruction. Gameplay/ability modules remain enabled, preserving active spell state, cooldowns and projectile state. Its early Update clears legacy input for modal and closing frames, while the existing Input System UI module continues processing panel input. A physical-key release latch keeps held spell shortcuts suppressed until released after closing. A full-screen raycast blocker keeps clicks off the spell bar and underlying menus. Resuming clears gameplay selection to avoid accidentally submitting a background button.

The new `CourtSettingsMixer.mixer` has Master, Music and SFX groups. After all Awake-created voices exist, the controller routes this scene's two music sources to Music and its impact/result/spell sources to SFX. Exposed mixer gains apply independently of source volumes, so existing ducking/variation stays intact. Sliders show relative gain: 100% preserves the preceding authored balance; zero maps to the mixer's -80 dB floor. No additional audio voices, clips, packages or AudioListeners are added. Mixer values are applied from Start/Update/UI callbacks, outside Awake/OnEnable.

The motion switches leave existing components enabled. A world-camera render callback runs after the existing shake callback and can restore its default view; it can also reset only the cached brick reaction children to their resting visual pose. Root transforms, colliders and sprite data remain untouched. Cache refreshes follow board rebuild/score resets; render-callback order refreshes follow the existing motion controller's ResetVersion. These adapters depend on the current child name `Brick reaction visual - collider free` and the current legacy-gameplay/new-UI input split, both checked in source and requiring Unity verification.

Preferences use four `Brickkeep.Scene10.*` PlayerPrefs keys and are saved when the panel closes. They persist between runs of this checkpoint; earlier scenes do not read them. The retained UI is built once, with an explicit keyboard-navigation loop and non-raycasting decorative trim/text. Disabling the settings component closes the panel, restores any owned time scale and original audio routing, and removes its render/input observers. Existing music/SFX components continue to own their original Inspector settings.

All previous Assets files are frozen. The new scene adds only an independent settings root/transform/controller and a SceneRoots entry; original gameplay, graphics, audio and UI component blocks retain CastleMusic's serialized values. Source checks pass for C# syntax, scene/mixer/GUID references, asset/block preservation, layout bounds, gain mapping, pause ownership, event/render cleanup and input-order contracts. Unity is unavailable here: engine/mixer/font import, actual paused input/spell behavior, UI focus/layout, rendered motion suppression, persistence and playback require the TESTING.md play-mode review.

Implementation references: Unity's [ResetInputAxes](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Input.ResetInputAxes.html), [Input System UI support](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/UISupport.html) and [AudioMixer.SetFloat](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Audio.AudioMixer.SetFloat.html). Native mixer field structure was checked against the [Unity Open Project mixer](https://github.com/UnityTechnologies/open-project-1/blob/main/UOP1_Project/Assets/Settings/Audio/DefaultAudioMixer.mixer); this checkpoint uses its own mixer configuration and no sample audio/code module from that project.

Next proposed checkpoint: improve level introductions and result-screen presentation, while retaining these settings.

## Castle music checkpoint — change 9

Open `Assets/_Project/Scenes/Medieval09_CastleMusic.unity`. The courtyard now has an original plucked-string and bell theme over a quiet open-fifth drone. Its calm menu arrangement blends into a fuller gameplay arrangement with bass, extra plucks and restrained frame-drum percussion. Both arrangements share one eight-bar, 96 BPM, 3/4 phrase: exactly 15 seconds / 661,500 samples at 44.1 kHz.

`CastleMusic/CastleMusicController.cs` creates two reusable 2D AudioSources and schedules both at the same DSP time, after loading and checking their sample counts/rates. Linear crossfade weights keep their common backing from doubling. Both loops keep running through normal menu/round transitions, including life loss and retry. A separate scene root keeps music outside GameFeel's round-reset toggles.

| Situation | Music behavior |
|---|---|
| Main menu / difficulty selection | Calm arrangement |
| Start / retry a round | Blend into the gameplay arrangement without restarting the phrase |
| Lose a non-final life | Briefly lower music, then recover while play continues |
| Victory / game over | Fade to silence so the existing result cue stays clear |
| Return to menu / Next Level selector | Fade back to the calm arrangement |

Score-zero resets identify menu/play transitions. Existing final-brick/loss events identify results after the session commits its state. Next Level has no score-reset event, so the controller adds one state-only listener to that existing button after LevelMenuView builds its hierarchy; original callbacks are retained. No UI text is parsed. State observers remain attached while the component is disabled, allowing it to remember transitions made during silence; they are detached on destruction. Disabling stops both music sources immediately. Re-enabling schedules a fresh synchronized phrase in the remembered context, with a fade-in. Output-device changes also reschedule the pair together.

| Setting on `Castle Music - Round Transitions 09 / CastleMusicController` | Default |
|---|---:|
| Muted / Music Volume | Off / 0.22 |
| Crossfade / Fade In | 0.80 / 0.70 s |
| Result Fade | 0.18 s |
| Life Duck Gain / Hold | 0.35 / 0.70 s |

Music mute and volume are independent of CastleAudioFeedback and spell sounds. Muting keeps the phrase clock running. Life ducking attacks quickly and recovers within approximately 0.35 seconds after its hold. No AudioListener, per-transition objects, runtime clip synthesis, physics changes or global audio-setting changes are added. The two sources use a lower priority than the existing impact/result voices.

The two original OGG files in `CastleMusic/Clips` use wrapped note tails and matching lengths. Their import metadata requests preloaded, decompressed PCM with normalization off, preserving sample alignment. Regenerate with `python3 tools/generate_castle_music.py` (Python standard library plus FFmpeg with libvorbis). The generator composes both arrangements from the same backing, uses fixed offline drum-noise seeds, applies a shared peak gain, and writes deterministic OGG output with the same encoder version. Unity playback needs no extra package or FFmpeg installation. No external recordings, MIDI files or sound fonts are used.

All previous Assets files stay frozen. The new scene adds only the independent music root/controller and its SceneRoots entry; existing UI, visuals, SFX, abilities, controls, physics and game rules retain CastleAudio's configuration. Source validation passes for C# syntax, scene/GUID/importer references, preserved blocks/assets, decoded sample counts/levels/seam steps, deterministic regeneration, crossfade bounds and event/lifecycle ownership. Unity is unavailable here: engine import/compilation, actual loop seams, scheduling, mix and device/play-mode listening checks remain pending in TESTING.md.

Implementation references: Unity's [PlayScheduled](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioSource.PlayScheduled.html), [AudioClip.samples](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioClip-samples.html) and [audio-configuration event](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioSettings.OnAudioConfigurationChanged.html). These describe the APIs, not an executed Unity playtest.

Next checkpoint: a compact pause/settings panel with separate music/SFX controls and motion options, in another new scene.

## Castle audio checkpoint — change 8

Open `Assets/_Project/Scenes/Medieval08_CastleAudio.unity`. Paddle contacts now have a wooden knock/pluck and brass accent; walls give a quieter stone tap; brick breaks cycle through three layered stone-crunch/chip sounds. A descending plucked cue marks a lost life, a rising major bell/pluck fanfare marks victory, and a slower descending toll marks game over. These are original synthesized cues, supplied as eight mono 44.1 kHz PCM16 WAV assets in `CastleAudio/Clips`.

| Cue | Length | Character |
|---|---:|---|
| PaddleWood | 0.18 s | Wooden body, string pluck, small brass transient |
| WallStone | 0.14 s | Restrained low stone knock |
| BrickBreak1 / 2 / 3 | 0.24 s each | Three stone/noise/chip variations |
| LifeLost | 0.68 s | Brief descending three-note cue |
| VictoryFanfare | 1.85 s | Ascending major phrase and bell chord |
| GameOverToll | 1.78 s | Slower descending bell phrase |

The new scene replaces EventAudioFeedback with `CastleAudioFeedback.cs`; the old component does not also play. Four impact voices are reused, with separate paddle/wall/brick rate limits, deterministic brick-clip cycling, a capped combo pitch rise and subtle left/right panning. One dedicated life/result voice cannot be stolen by impacts. Life loss stops the old impact tails and briefly lowers new impacts while its cue plays; the ball still relaunches immediately.

The controller reads existing events and LevelSession.IsPlaying. The session commits its winning state before BrickDestroyed and its final-loss state before BallLost, so the last brick produces victory even when hit by Fireball, and the final life produces only the game-over cue. Outcome handling runs before the brick rate limit and latches once per round. No gameplay scripts, result timing or UI callbacks are changed.

| Setting on `GameFeel - Castle Audio 08 / CastleAudioFeedback` | Default |
|---|---:|
| Muted / Master Volume | Off / 0.65 |
| Impact Volume | 0.50 |
| Life Loss Volume / Result Volume | 0.65 / 0.75 |
| Collision Voices | 4, plus 1 dedicated life/result voice |
| Stereo Width | 0.22 (0 centers impacts) |

Mute and volume sliders affect cues already playing. These settings control the new checkpoint cues; existing spell cast/shield sounds retain their own settings. Retry, return to menu, score-zero reset and disabling stop active cues. Re-enabling during a result does not replay its fanfare/toll. There is no automatic menu playback or new AudioListener; clips and voices are not created per hit. Stop Play before changing pool size.

Rebuild the eight WAVs with `python3 tools/generate_castle_audio.py` (Python standard library only). The generator uses fixed local noise seeds, modal partials and smooth attack/release envelopes, and peak-normalizes each clip to 0.72 before writing PCM16. It leaves Unity metadata intact and does not affect gameplay randomness. The AudioImporter metadata requests preloaded, decompressed PCM. No downloaded recordings or audio packages are used.

All existing Assets files and earlier scenes remain frozen. Only the cloned scene's audio component and feedback-object name differ from BallPaddle; its graphics, camera, physics, controls, formations, abilities, scoring and lives are preserved. Validation: C# syntax, YAML/clip/GUID wiring, scene/asset preservation, deterministic WAV generation, sample format/levels/boundaries, event ordering and audio lifecycle source checks pass. Unity is unavailable here: engine import/compilation, actual playback/mix/device listening and play-mode verification remain pending; see TESTING.md.

Next checkpoint: a restrained medieval music loop with transitions between menu, play and results, in another new scene.

## Ball and paddle checkpoint — change 7

Open `Assets/_Project/Scenes/Medieval07_BallPaddle.unity`. The paddle now has carved wood grain, beveled brass endcaps, rivets and jade rune inlays. The ball is a shaded ivory-and-gold orb with small jade markings and a faint warm halo. A thinner, shorter gold trail matches the castle palette. Ball contacts add a brief directional visual compression and surface glint; paddle contacts also emit a small expanding gold ring and a sharper, reduced paddle rebound.

The new `RelicGear` shader and four shared materials draw these details without downloaded textures. `RelicGearFeedback.cs` builds a collider-free rendering child under each existing visual, plus a halo and a four-slot contact-ring pool. It hides original rendering with forceRenderingOff while preserving each original renderer's sprite, material, tint and bounds for the existing ability systems. In particular, Fireball continues to use the original ball material. Giant's Grip retains its existing width and tint behavior, which brightens the new jade inlays; brass caps keep their world-space width.

`BallContactRelay.cs` observes actual ball/brick collision contacts and only forwards their normal to the visual controller. Paddle/wall contacts use the existing events. Remote Fireball destruction does not masquerade as a collision with the ball. No physics response, damage or scoring is added by the observer. Contact pulses restart within a fixed bound rather than accumulating scale; resets clear rings and restore the orb child to rest.

| Setting on `GameFeel - Relic Gear 07` | Default |
|---|---:|
| RelicGearFeedback / Enable Contact Feedback | On |
| Contact Strength | 1 (0 disables new flashes, ball compression and rings) |
| Ball Squash / Contact Duration | 0.10 / 0.14 s |
| Halo Opacity | 0.24 |
| PaddleSquashFeedback / Squash Amount / Duration | 0.10 / 0.18 s |
| BallTrailFeedback / Trail Duration / Width | 0.10 s / 0.13 world units |
| Contact ring pool / Lifetime | 4 slots / 0.16 s |

The contact toggle leaves the skins and halo visible. PaddleSquashFeedback is an independent retained component; disable it separately to remove the paddle rebound. Disable RelicGearFeedback to restore the original ball/paddle rendering. Changing Halo Opacity to zero hides the halo. Loss, retry/menu resets and disabling clear the new contact effects; resource counts stay fixed during play.

All existing scene and asset files remain frozen. The new scene adds two presentation components and adjusts only its own serialized paddle-squash/trail settings. Ball/paddle/camera transforms, source sprites/materials, colliders, speeds, controls, lives, score, formations, abilities, result timing, earlier destruction effects and audio retain the preceding checkpoint's configuration.

Validation: new C# syntax, scene/material/GUID references, preserved physics/source-renderer/gameplay blocks, pulse/scale/ring bounds and source/event lifecycle checks pass. Unity is unavailable here: engine/shader compilation, actual graphics, ball readability, real collision delivery and play-mode verification remain pending. Follow TESTING.md before treating this checkpoint as runtime verified.

Next checkpoint: richer impact sounds and distinct life-loss, victory and game-over cues in another new scene.

## Impact motion checkpoint — change 6

Open `Assets/_Project/Scenes/Medieval06_ImpactMotion.unity`. Brick destruction now produces a restrained camera pulse and a brief damped reaction in nearby surviving bricks. Losing a life while play continues produces a stronger camera jolt. Motion decays to rest; repeated hits refresh a bounded pulse. The HUD, impact labels and existing immediate menu/result screens stay stable.

`ImpactMotion/ImpactMotionFeedback.cs` applies a temporary view matrix only inside URP's camera-render callbacks, then resets it after rendering, before Update/FixedUpdate and on disable/reset. It never changes the camera transform or projection. This keeps the existing mouse paddle's ScreenToWorldPoint mapping outside rendering unchanged. The castle background gets a small constant overscan margin based on configured maximum displacement to cover viewport edges; the margin does not pulse on individual hits and is removed when camera motion is disabled.

`Prefabs/ReactiveRunestoneBrick.prefab` duplicates the previous brick without changing its collider, root transform, health, points, material or surface component. `BrickVisualReaction.cs` creates one collider-free child renderer, copies the root appearance/property block after surface styling, and animates only that child. Root rendering is suppressed with forceRenderingOff while its enabled state, tint and bounds remain available to gameplay. Disabling the reaction component restores root rendering; resets return the child pose to rest. Live bricks react only within the configured radius; destroyed bricks retain the earlier four-piece break animation.

Select `GameFeel - Impact Motion 06` and tune ImpactMotionFeedback:

| Inspector setting | Default |
|---|---:|
| Enable Camera Shake / Enable Brick Reactions | On / On |
| Motion Strength | 1 (set **0** to disable all new motion) |
| Brick Shake Amplitude / Duration | 0.018 world units / 0.11 s |
| Life Loss Amplitude / Duration | 0.075 world units / 0.26 s |
| Neighbor Radius | 2.6 world units |
| Neighbor Amplitude / Duration | 0.035 world units / 0.20 s |
| Neighbor Compression | 0.016 |

**Camera-shake off setting:** uncheck **Enable Camera Shake** on ImpactMotionFeedback. **Brick-motion off setting:** uncheck **Enable Brick Reactions**. Either can be changed independently while playing. Setting Motion Strength to zero disables both. Disabling the component restores the view and stops brick motion; other animation, particle, scoring and audio feedback remain available.

All old scenes, prefabs, materials/shaders, gameplay scripts, ball/paddle/camera transforms and collision settings are preserved. The new scene only selects the independent reactive prefab and adds the controller. No new physics objects, per-hit GameObjects/material instances, gameplay randomness or global time changes are introduced.

Validation: C# syntax, scene/reference/metadata and duplicated-prefab checks, preserved physics/health/score, bounded concurrent-pulse samples, background coverage, inter-brick clearance, render/event lifecycle and reset checks pass. Unity is unavailable here: engine compilation, actual URP callback rendering/restoration, source renderer bounds, pointer control during jolts, visual tuning and playtesting remain pending. See TESTING.md before treating the pass as runtime verified.

Implementation references: Unity's [worldToCameraMatrix documentation](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Camera-worldToCameraMatrix.html) and [beginCameraRendering](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rendering.RenderPipelineManager-beginCameraRendering.html). These API references informed the render-only approach; they are not evidence of an executed Unity playtest.

Next checkpoint: ball/paddle appearance and contact feedback, followed by the remaining audio polish.

## Masonry particles checkpoint — change 5

Open `Assets/_Project/Scenes/Medieval05_MasonryParticles.unity`. This checkpoint replaces the earlier uniform brick burst with three complementary layers: faceted stone chips that spray from the struck face and tumble/fall, faint expanding powder puffs, and a few brief velocity-aligned brass glints. The existing flash, four carved break pieces, impact score labels, UI, audio and castle visuals remain in place.

`MasonryParticles/MasonryParticleFeedback.cs` listens only to `BrickDestroyed` and score resets. A new native URP 2D shader supplies soft dust, clipped/beveled chip silhouettes and narrow spark masks, with three separate material assets. No image texture or downloaded media is needed. Dust and chips draw behind the ball; short, narrow sparks add a metal accent without a full-screen flash. Sparks have zero camera-velocity stretching, ready for the later camera-shake pass.

| Setting on `GameFeel - Masonry Particles 05` | Default |
|---|---:|
| Pool Size | 8 burst slots / 24 ParticleSystems |
| Chip / Dust / Spark Count | 8 / 3 / 4 per break |
| Intensity | 1.0 |
| Dust Opacity | 0.16 |
| Chip lifetime | 0.24–0.40 s |
| Dust lifetime | 0.28–0.42 s |
| Spark lifetime | 0.13–0.23 s |

At default counts, this pool can hold at most 120 emitted particles at once; the full Inspector limits cap it at 264. Reusing a slot clears its older particles. Emission is manual and event-based, with play-on-awake, looping and automatic emission disabled. The three systems use fixed internal seeds plus a separate cosmetic System.Random stream, preserving gameplay randomness. No scene objects or materials are created per hit. Disabling/resetting feedback clears every system.

The new scene replaces the old BrickBurstFeedback component, so the old burst does not stack on top. All older scenes, feedback scripts, shaders/materials, brick prefabs and gameplay/physics components are unchanged. Stop Play before changing pool size. Set a layer count to zero to disable/isolate it. Count, intensity and opacity settings apply to subsequent hits; disabling MasonryParticleFeedback leaves the separate break animation, score feedback and audio active.

Validation: C# syntax, scene/reference/material/metadata checks, frozen gameplay/physics/break-animation comparisons, particle caps, event lifecycle and procedural-mask bounds checks pass. Unity is not available here: engine/shader compilation, actual particle appearance, motion, layering, frame rate and playtesting still require TESTING.md. Source checks do not establish a Unity render result.

Next checkpoint: restrained life-loss camera shake and brick reactions, each applied only to presentation in a new scene.

## Brick break checkpoint — change 4

Open `Assets/_Project/Scenes/Medieval04_BrickBreak.unity`. Destroying a brick now adds a brief warm flash, a small visual recoil, four irregular stone pieces that separate/spin/fall and fade, and a large gold impact score with a dark shadow. The fragments retain the original brick's grain, brass and carving. They draw behind the live ball and intact bricks; the flash is brief. Existing sound and particles still trigger once through the same events.

`BreakEffects/StoneBreakFeedback.cs` listens to `ScoreChanged` and `BrickDestroyed`. LevelSession already raises the actual awarded score just before the break event, so the impact label uses that increase (currently +10 per brick). It never writes the normal score, changes damage, delays a bounce or moves an original brick/collider. Only the pooled visual copies are animated. The existing HUD pulse remains in place.

Twelve reusable slots each hold one flash, four sprite fragments and one shadowed popup. Five shared runtime sprite shapes are created once, rather than new objects or materials for each hit. When full, the pool replaces its oldest visual slot; earned points are unaffected. Score resets, menu/retry transitions and disabling the component clear the effects; re-enabling reads the current score without rewarding old points. Runtime sprites are released on destruction.

| Inspector setting on `GameFeel - Brick Break 04` | Initial value |
|---|---:|
| Pool Size | 12 |
| Fragment Duration / Flash Duration | 0.48 s / 0.065 s |
| Fragment Speed / Gravity | 1.6 / 4.4 |
| Popup Duration / Font Size | 0.66 s / 42 pt |
| Popup Rise / Peak Scale | 56 Canvas units / 1.18 |

Labels use the same 1920x1080 Canvas scaling as the menus, with bounds that clear the 134-unit top HUD and 96-unit spell bar. Menus/results stay above impact labels and fragments; the existing immediate victory/game-over flow is preserved. Stop Play before changing pool size or popup font size; the other motion/duration settings are read while playing. Disable StoneBreakFeedback to remove this pass while scoring and the existing feedback continue.

Validation: C# syntax, scene/reference/metadata checks, frozen gameplay/physics/prefab comparisons, polygon coverage and pixel-space/stone-UV reconstruction, popup bounds at the full punch range, and event/score ownership checks pass. Unity is not installed here: engine compilation, runtime sprite generation, stone-fragment appearance, text layering, sound balance and real playtesting remain pending.

Implementation reference: Unity's [Sprite.OverrideGeometry API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Sprite.OverrideGeometry.html) specifies pixel coordinates in the sprite rectangle; the helper converts the centered fragment vertices accordingly. This pass uses project-native geometry and the existing materials/font, with no downloaded media.

Next checkpoint: refine the existing destruction particles with dust/chips/sparks, then add restrained screen/brick shake in a separate scene.

## Stone court checkpoint — change 3

Open `Assets/_Project/Scenes/Medieval03_StoneCourt.unity`. This visual checkpoint retains the Royal Interface and adds colored stone bricks with beveled lighting, fine grain, aged-brass braces/rivets and diamond carvings. Six brighter stone tints distinguish destructible rows from the darker, permanent castle masonry.

The side walls now read as coursed castle pillars, with projecting mouldings, feet, narrow arrow-slit recesses, hanging banners and gold crests. The upper wall has masonry courses and battlement notches. Caps project outward; every decorative piece stays at or outside the original inner wall faces. Upper-wall notches begin above its reachable collision face. These details add no colliders.

| Asset / component | Purpose / tuning |
|---|---|
| `StoneCourt/CastleStone.shader` | Native URP 2D stone grain, bevel shading, joints, brass details and optional top-wall notches. |
| `StoneCourt/RunestoneBricks.mat` | Brick look: Bevel 0.055, Joint 0.01, Grain 0.15. |
| `StoneCourt/CastleMasonry.mat` | Fixed-wall look: Bevel 0.055, Joint 0.022, Grain 0.18. |
| `StoneCourt/MedievalStoneSurface.cs` | Per-renderer property blocks supply sprite bounds, physical size, deterministic variation and tint. No material instances or gameplay random values. |
| `StoneCourt/CastleCourtDecor.cs` | Builds one hierarchy of decorative pillar mouldings, recesses and banners. |
| `Prefabs/RunestoneBrick.prefab` | Duplicates LevelBrick's unchanged health, points, sprite dimensions and collider; adds only the surface style. |

On each side wall, MedievalStoneSurface uses two columns and fourteen staggered courses. The top uses fourteen columns/two courses and Crenellations. Bricks follow their existing SpriteRenderer row tint; permanent masonry uses its separate Stone Tint. Material changes should be made to these new materials only.

All earlier scenes, shared gameplay scripts, earlier prefabs, UI assets, wall/ball/paddle transforms and collider settings are preserved. The same three layouts, difficulties, abilities, point values and lives apply. Existing particles inherit the updated brick colors; their behavior has not been revised in this checkpoint.

Validation: C# syntax, Unity YAML/reference checks, comparisons of all physics/gameplay components, duplicate-prefab collision/health/point checks, wall-decoration bounds and shader-property wiring pass. Unity is unavailable here: engine C# and shader compilation, actual stone appearance, frame rate and play-mode review remain pending. This does not claim a Unity-rendered preview or completed playtest.

Next checkpoint: brick break/recoil animation and impact score feedback, followed by particle and restrained shake passes in separate scenes.

## Royal interface checkpoint — change 2

Open `Assets/_Project/Scenes/MedievalUI02_RoyalInterface.unity`. This UI-only checkpoint uses the castle artwork from the latest Medieval scene, with a framed dark jade/gold menu, an imported serif display typeface, larger text, three horizontal level cards with accurate miniature board previews, colored difficulty cards, larger result screens, and subtle button hover/press motion. The original buttons and their callbacks still control level selection, difficulty, retry, next level and returning to the menu.

| Element | New reference size (1920x1080 Canvas) |
|---|---:|
| Main and result headings | 116 pt |
| Level card headings | 42 pt |
| Difficulty card headings | 48 pt |
| Menu descriptions / result buttons | 34 pt |
| Actual score / lives | 52 / 48 pt |
| In-game level heading / difficulty | 40 / 26 pt |
| Spell names / status | 24 / 23 pt |

The three top panels distinguish score, level/difficulty and lives. UI01's gold score flash, upward pulse and combined gain popup are retained, with a larger gain label. The spell bar is wider and taller. Decorative graphics do not intercept pointer input; explicit arrow-key navigation connects the horizontal cards and existing result buttons.

`Presentation/MedievalInterfacePolish.cs` waits for LevelMenuView to construct its hierarchy, then styles it once and refreshes the active screen/navigation as selections change. `MedievalButtonMotion.cs` animates only UI transforms. `Fonts/BrickkeepDisplay-Bold.ttf` is a renamed ASCII subset of Liberation Serif Bold under SIL OFL 1.1; its license, copyright and modification note are included. A dynamic TextMesh Pro display atlas is created once; body text uses the original Liberation Sans.

The latest Medieval scene had a zero-scale HUD and a corner-pivot background sprite. Only the new scene restores the HUD's unit scale and centers/covers the camera with the existing artwork behind gameplay. All gameplay adapters, brick prefabs, ball speed, scoring, lives, layouts and earlier scenes are unchanged from the current main branch.

Validation: C# syntax parsing, Unity scene YAML/reference comparisons, serialized-field/asset metadata checks and font text-width checks pass. Unity is not installed here, so editor import/compilation, display-atlas creation, visual layout, actual input and menu transitions still require the checks in TESTING.md. No new gameplay or real-person playtest result is claimed.

Next requested work remains separate: brick materials/design, break/recoil animation, world-space score popups, richer destruction particles, and restrained screen/brick shake. Stop at this UI checkpoint for review before the next scene.

## Score UI checkpoint — change 1

Open `Assets/_Project/Scenes/MedievalUI01_Score.unity`. This scene duplicates Medieval and adds one focused presentation change: a dark, bronze-framed score plaque with a gold accent, larger gold score text, and a brief ivory flash, upward push and scale pulse when points increase. A floating `+points` label combines rapid gains and fades away. The existing level selection, abilities, lives, controls and physics are preserved.

`Assets/_Project/Presentation/MedievalScoreFeedback.cs` replaces the older score-pop component only in this new scene. LevelSession remains the sole writer of the actual numeric score. The new feedback reuses one plaque/popup, restores the captured rest pose after each pulse, clears on resets, and never blocks mouse input. Earlier scenes and shared assets were not edited.

On `GameFeel - Medieval UI 01`, tune **Peak Scale** (1.12), **Upward Push** (8 Canvas units), **Pulse Duration** (0.28 s), **Gain Duration** (0.55 s), **Gain Rise** (14 units), and **Reward Color**. Set Peak Scale to 1 and both movement values to 0 for color-only feedback; disable the component to remove its decoration and animation while score updates continue.

Validation for this checkpoint: source/asset checks confirm that the new scene differs from Medieval only in four score/presentation blocks, all physics and gameplay wiring match, the replacement component is assigned, new metadata is unique, and earlier scene files are unchanged. Unity is unavailable in this environment; editor compilation, appearance, menu/retry transitions and actual repeated-hit playtesting remain pending. The older .NET gameplay checks were not rerun for this presentation-only change.

Further requested changes (audio, ball/paddle presentation, life-loss shake and brick destruction) are pending review of this first checkpoint. Each next change should get its own scene and commit.

## Three levels

| Level | Brick distribution | Bricks | Maximum score |
|---|---|---:|---:|
| 1. Courtyard | Four solid rows | 28 | 280 |
| 2. Twin Towers | Two tall side towers with an open middle lane | 26 | 260 |
| 3. Crown | Three top tips, a broad center, and a tapered base | 24 | 240 |

All three levels are available from the main menu. Select one to see its description and choose Easy (speed 9), Medium (12), or Hard (15). All nine level/difficulty combinations support the same three abilities and start with three lives, zero score, and ready abilities. The HUD and result screen identify both the level and difficulty.

- **Try Again** restores the same level and difficulty, its full brick layout, three lives, zero score and ready abilities.
- Winning levels 1 or 2 offers **Next Level**, which opens that level's difficulty selection. It waits for your choice before launching the ball.
- **Return to Main Menu** lets you select any level again. **Back to Levels** cancels difficulty selection without starting a run.
- Level 3 victory has a Return to Main Menu button. Scores are per level, with no cumulative campaign score.

`Assets/_Project/Levels` contains the immutable layout catalog, selection/progression rules, scene adapters and updated menus. `LevelBrick.prefab` is independent of the earlier prefabs. The existing ability timing, effects, hotbar and paddle components are reused without modifying their previous versions.

## Abilities checkpoint

| Ability | Key | Effect | Cooldown |
|---|---|---|---:|
| Fireball | 1 / Space | Shoots upward through multiple bricks with a flame core, trail, impact bursts and a whoosh. | 5 s |
| Giant's Grip | 2 | Makes the paddle 60% wider for 6 seconds, with a green tint and chime. Collision width, bounce angles and movement limits follow its new size. | 12 s |
| Rune Shield | 3 | Creates a cyan barrier below the paddle. It reflects one downward-crossing ball without losing a life and expires after 8 seconds if unused. | 14 s |

The clickable hotbar shows READY, effect duration, cooldown seconds and recharge bars. All abilities start ready on each new run/retry and have independent cooldowns. A lost life cancels active effects and the fireball while preserving cooldowns; game over, victory and returning to the menu clear the entire ability state. Abilities cannot be used in menus or result screens.

Fireball hits use the same scoring/destruction path as normal ball hits. Each brick awards points once, including simultaneous hits, and fireballs can trigger the normal victory screen. The fireball is reused and its movement is swept to avoid skipping bricks. Shield reflection preserves the chosen ball speed. The existing three difficulties, three lives, retry and menu flow remain available.

This checkpoint has its own scripts in `Assets/_Project/Abilities` and its own `AbilityBrick.prefab`; the previous Arcade/After/Before scenes and their assets stay frozen. Open the scene for the checkpoint you want to compare.

## Arcade mode

| Difficulty | Ball speed (world units/second) | Starting lives |
|---|---:|---:|
| Easy | 9 | 3 |
| Medium | 12 | 3 |
| Hard | 15 | 3 |

Arcade and Abilities use the original 28-brick board. Levels uses the selected distribution in the table above. All award 10 points per brick. The chosen speed applies to the initial launch, every bounce, and every relaunch after a miss.

- Losing the third life opens **Game Over** with the final score, **Try Again**, and **Return to Main Menu**.
- Try Again keeps the difficulty and rebuilds the full board, resets the score, and restores three lives.
- Breaking the last brick opens **Victory** with the final score and **Return to Main Menu**.
- The ball and paddle stop on both result screens. Returning to the menu clears the previous board and feedback before another difficulty is chosen.

`Assets/_Project/Arcade` contains the independent round rules, scene controller, gameplay adapters, and gold/ivory menu UI. The mode reuses the existing paddle controls, physics assets, and five feedback components. Its brick prefab is duplicated so the comparison scenes retain their original rules and assets.

