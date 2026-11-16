# Review checkpoints

Each stage is pushed separately to main. Stop Play before pulling and open the scene named in its checkpoint below. From the Abilities checkpoint onward, each new feature checkpoint gets a new scene; earlier scenes and their gameplay adapters stay frozen.

## 1. Starter repairs

- Restored the actual EmojiOne PNG; the previous commit stored a Git LFS pointer.
- Store small game media as regular Git blobs so a fresh clone contains playable assets.
- Register After first in Build Settings, with Before still available.
- Center only After's camera and restore its Canvas scale. Physics objects and Before remain unchanged.
- Validation: seven source checks; Unity import/play mode still requires the editor.

## 2. HUD pointer fix

- Disabled Raycast Target only on After's score and lives labels. The paddle's existing UI-hover guard otherwise stops mouse following over these display-only labels.
- Review: stop Play, pull main, open `Assets/_Project/Scenes/After.unity`, and play in 16:9. Move the mouse horizontally over the score and lives labels; the paddle should keep following. Confirm both labels still update during scoring and ball loss.
- Validation: all seven source checks pass; Before and shared gameplay remain unchanged. Unity runtime verification is still pending.
- Roadmap: simple UI and VFX first, followed by textures, background, sound effects, and music. Gameplay abilities, including the brick-piercing fireball, belong in a separate optional mode to preserve the assignment comparison.

## 3. Simple medieval HUD styling

- Moved score and lives to matching top corners with equal 64-unit side margins and 40-unit top margins at the Canvas's 1920x1080 reference resolution.
- Styled score in warm gold and lives in parchment ivory, using matching 36-point bold text with light letter spacing. Existing score-pop feedback and non-blocking pointer behavior are preserved.
- Review: pull main, open `Assets/_Project/Scenes/After.unity`, and compare the HUD with Before. Check at 1920x1080 and 1280x720; score and lives should stay readable in opposite corners. Break bricks and lose a ball to confirm values and score animation still update.
- Validation: all seven source checks pass; Before, gameplay, and feedback scripts remain unchanged. Unity visual/play-mode checks are pending.
- Next small checkpoint: improve brick-impact VFX.

## 4. Difficulty menu, game over and victory

- Added `Assets/_Project/Scenes/Arcade.unity` as the first build scene, with Easy (9), Medium (12), and Hard (15) ball speeds and three lives per run.
- Added a gold/ivory main menu, in-game difficulty label, Game Over with Try Again/Return to Main Menu, and Victory with Return to Main Menu. Result screens retain the final score and stop gameplay.
- Try Again preserves difficulty and resets the complete board, lives, score, paddle, and transient feedback. Returning to the main menu allows choosing a new difficulty without reloading scenes or changing global time scale.
- Before/After, their gameplay scripts, their brick prefab, and the five feedback implementations are untouched. Arcade has its own round rules and gameplay adapters, reusing shared physics assets and feedback events.
- Validation: nine source checks and 7,868 compiled C# rule assertions pass, including 300 randomized complete runs. Unity import/play-mode and menu interaction checks remain pending; see TESTING.md.
- Review: stop Play, pull main, open Arcade (not After), and exercise all three difficulties, a complete loss/retry, and a complete win/return. This checkpoint contains only the requested run flow; further VFX work waits for review.

## 5. Three medieval abilities

- New scene: `Assets/_Project/Scenes/Abilities.unity`, registered first in Build Settings. Arcade, After and Before and all their existing Assets are unchanged byte for byte.
- Fireball (1/Space, 5-second cooldown) pierces multiple bricks. Giant's Grip (2, 12-second cooldown) widens the paddle by 60% for 6 seconds. Rune Shield (3, 14-second cooldown) blocks one miss and expires after 8 seconds.
- Added clickable hotbar buttons, ready/active/cooldown text and recharge bars, a flame core/trail, paddle tint, a shield line/deflection flash, and synthesized cast/deflection cues with bounded audio voices.
- Ball and fireball share a guarded damage path; simultaneous hits cannot double-score. Shield reflection keeps the difficulty speed. A life loss cancels effects without refilling cooldowns; retry/menu/result transitions reset all spell state and visuals.
- Validation: eleven source/asset checks, compiled ability-rule checks (cooldown stress, shield crossings, duplicate hits, reset behavior), the existing 7,868 round assertions, and C# syntax parsing. Unity import, engine compilation, physics, UI and audio testing still require the editor.
- Review: stop Play, pull main, open Abilities and select a difficulty. Cast all three abilities with keys and clicks; verify each cooldown, shield consumption, expanded-paddle edges, fireball scoring, retry, win and menu resets. Stop here for review before another scene is created.

## 6. Three layouts with independent difficulty selection

- New scene: `Assets/_Project/Scenes/Levels.unity`, first in Build Settings. Abilities, Arcade, After, Before and all their existing Assets are unchanged byte for byte.
- Level selection offers Courtyard (28 bricks), Twin Towers (26), and Crown (24), followed by Easy/Medium/Hard for the selected layout. All nine combinations preserve the three abilities, three starting lives and 9/12/15 ball speeds.
- The HUD/result screen shows level and difficulty. Retry preserves both choices and restores that board. Victory uses its actual brick count and score: 280, 260 or 240.
- Next Level after winning levels 1/2 opens the next difficulty selector without launching. Back to Levels and Return to Main Menu allow direct replay of any layout. Each level starts a fresh score, lives and ability state.
- New LevelCatalog/LevelRun rules and independent scene adapters/prefab preserve the earlier checkpoints while reusing immutable round and ability helpers.
- Validation: twelve source/asset checks, 5,042 compiled level assertions covering all nine combinations and 300 mixed attempts, the existing round/ability checks, and C# syntax parsing pass. Unity engine compilation and play-mode/visual checks remain pending.
- Review: stop Play, pull main, open Levels, compare all three formations, and exercise level/difficulty selection, loss/retry, victory/next-level difficulty selection, and return to menu. Stop here for review before further changes.


## 7. Score plaque, color flash and upward pulse

- New scene: `Assets/_Project/Scenes/MedievalUI01_Score.unity`, first in Build Settings and based on the existing Medieval checkpoint. All earlier scene files and shared feedback/gameplay assets are unchanged.
- One UI improvement: a dark score plaque with bronze borders and a gold accent, 40-point gold score, a restrained ivory/scale/upward pulse, and one reused floating gain label. Rapid hits combine their gain label; resets do not produce reward feedback.
- New `Presentation/MedievalScoreFeedback.cs` replaces ScorePopFeedback only in this scene. The actual score still comes directly from LevelSession. The component returns to its captured position/scale/color, clears on disable/reset, and creates no objects on scoring. Decoration and gain text cannot intercept pointer input.
- Review: stop Play, pull main, open **MedievalUI01_Score**, then choose a level and difficulty. Compare the score with Medieval at 1920x1080 and 1280x720. Break individual bricks, fire through a row for rapid hits, move the pointer over the plaque, lose/retry a run, and return to the menu. Check correct totals, a short +points label and complete animation recovery. Toggle the new component off/on during a run and confirm that values still update.
- Validation: source/asset comparison checks pass; only four presentation blocks differ from Medieval and all physics/gameplay blocks match. Unity editor compilation and visual/runtime review remain pending.
- Stop here for review. Next requested checkpoints: sounds, paddle/ball presentation, life-loss shake and brick-break polish, each in a separate scene.

## 8. Larger Royal Interface menus and HUD

- New scene: `Assets/_Project/Scenes/MedievalUI02_RoyalInterface.unity`, first in Build Settings. It carries forward the latest main-branch castle art and UI01 score feedback. UI01, Medieval and every older scene remain untouched.
- Full UI presentation pass: large serif headings, jade/gold/ivory palette, framed level cards with previews read from LevelCatalog, distinct difficulty colors, larger result text/buttons, button hover/press motion, matching score/level/lives panels, and a larger spell bar. Score and lives remain owned by LevelSession; existing buttons retain their callbacks.
- Added only presentation scripts plus a licensed, renamed serif-font subset. The new scene restores its zero-scale canvas and fits the corner-pivot castle artwork behind gameplay; no gameplay/physics component or shared prefab changed.
- Review: stop Play, pull main and open **MedievalUI02_RoyalInterface**. At 1920x1080 and 1280x720, visit all three level cards, all difficulty choices, Back to Levels, a complete loss/retry, win/Next Level and Return to Main Menu. Use both mouse and arrows/Enter. Verify readable labels, visible button selection, no overlaps and no duplicate panels after repeated transitions.
- During play check the three HUD panels, correct score/lives, score pulse/+points label, larger cooldown text, clickable spells and uninterrupted paddle movement under decorative UI. Resize the Game view and ensure the background remains centered behind the ball/bricks.
- Validation: source-level C# syntax, scene/reference/isolation, font metadata/coverage and text-width checks pass. Unity engine import/compilation, dynamic font atlas generation and visual/play-mode review are pending.
- Stop here for review. Brick visual design, break/recoil animation, score popups at impacts, richer particles and screen/brick shake belong in subsequent scene checkpoints.

## 9. Stone bricks and castle walls / pillars

- New scene: `Assets/_Project/Scenes/Medieval03_StoneCourt.unity`, first in Build Settings. Royal Interface and all older scenes remain frozen.
- Added a new native stone shader, separate brick/wall materials, an independent RunestoneBrick prefab and per-renderer surface styling. Bricks have grain, bevel highlights/shadows, brass braces/rivets and carvings, with six readable row colors.
- Side walls are coursed castle pillars with outward-projecting mouldings, feet, arrow slits, banners and crests. The upper wall has battlement notches above the reachable inner face. All decorative geometry stays at/outside the side-wall faces, has no colliders, and uses explicit render order.
- All physics transforms/colliders, brick health/points, gameplay scripts, abilities, menus and score feedback remain intact. The shader uses deterministic variation; it does not advance gameplay Random or instantiate materials.
- Review: stop Play, pull main, open **Medieval03_StoneCourt**, and test all three formations at 1920x1080 and 1280x720. Check brick/ball contrast against the background, stone/brass details, pillar banners/caps and upper battlements.
- Bounce against both inner wall faces and the ceiling, including near pillar bands. Use fireball through a row and complete loss/retry, win/next-level and return/menu flows. Verify the same hit boundaries, totals and lives as UI02; all bricks should still be destructible with the same rules.
- Validation: C# syntax, scene/prefab YAML and reference comparisons, preserved physics/gameplay data, decorative bounds and shader/material wiring pass. Unity engine/shader compilation, visual/performance review and playtesting remain pending.
- Stop here for review. Destruction animation, impact score popups, particle improvements and screen/brick shake follow in separate scene checkpoints.

## 10. Brick break animation and impact score popups

- New scene: `Assets/_Project/Scenes/Medieval04_BrickBreak.unity`, first in Build Settings. StoneCourt and every earlier scene, prefab, shader, material and gameplay script are unchanged.
- Added StoneBreakFeedback to the existing feedback object: a short warm flash/recoil, four irregular sprite fragments with matching stone/brass texture, spin/gravity/fade, and gold shadowed +points labels. Live objects retain their normal collision/scoring path.
- Labels derive their value from the actual ScoreChanged increase immediately preceding each BrickDestroyed event. Normal score text remains owned by LevelSession. Popups are bounded below the top HUD and above the spell bar, do not capture input, and stay beneath result/menu overlays.
- A fixed twelve-slot pool reuses its sprites, renderers and labels. Rapid hits recycle the oldest visual, reset clears all slots, re-enabling reads the current score, and runtime sprite shapes are freed on destruction. No per-hit scene objects, colliders or material instances are created.
- Review: stop Play, pull main, open **Medieval04_BrickBreak**, choose a level/mode, and break isolated bricks from different directions. Check the immediate score/bounce, a visible flash, matching fragments separating cleanly, and a readable +10 label that fades.
- Fire through several bricks rapidly. Check exact totals, one label per awarded brick within pool capacity, bounded effect count, and no stale tint/rotation/score on reused slots. Check top-row and side hits at 1920x1080 and 1280x720.
- Test disable/re-enable, loss/retry, win/next level and return/menu while effects are active. Overlays retain the original immediate result timing and cover effects. New runs must have no leftover pieces or popups.
- Validation: C# syntax, scene/reference/frozen-asset checks, fragment area/coverage/pixel-conversion/UV alignment, popup bounds and event ownership checks pass. Unity import/compilation, actual sprite rendering/animation and play-mode/performance review remain pending.
- Stop here for review. Particle refinement and restrained screen/brick shake remain separate checkpoints.

## 11. Layered masonry destruction particles

- New scene: `Assets/_Project/Scenes/Medieval05_MasonryParticles.unity`, first in Build Settings. It replaces the old burst component only in this new scene; BrickBreak and all earlier assets stay frozen.
- Added faceted, spinning/falling stone chips, soft low-opacity expanding dust and short brass sparks aligned with their velocity. Colors follow the brick stone and metal palette; dust/chips sort behind the ball, while sparks remain narrow and brief.
- Eight reusable slots each contain three capped ParticleSystems. Default count is 8 chips + 3 puffs + 4 sparks per break, at most 120 emitted particles in the default pool and 264 at the maximum Inspector limits. Manual emission, fixed system seeds and a separate cosmetic random stream preserve game rules/randomness.
- Score resets, retry/menu feedback resets and disabling the component clear all systems. No per-hit GameObjects/material instances, collisions or continuous emission are added. Earlier break animation, impact labels, audio and HUD are unchanged.
- Review: stop Play, pull main and open **Medieval05_MasonryParticles**. At 1920x1080 and 1280x720, break isolated bricks from several directions and compare with BrickBreak. Check chip silhouettes/arc/spin, faint soft dust and small brass glints.
- Use Fireball through multiple bricks and create rapid hits. Verify exact score/lives, a readable ball, bounded particle count, clear resets and no uniform old burst in addition to the new layers. Test loss/retry, win/next-level and return/menu flows while particles are active.
- Toggle MasonryParticleFeedback off/on: particle layers should clear/resume, with the separate break animation, score labels and audio still functioning. Mute audio and check that impact feedback is readable without sound.
- Validation: C# syntax, scene/material/shader references, particle bounds, event/ownership and source preservation checks pass. Unity C#/shader compilation, actual appearance, rendering/performance and play-mode review remain pending.
- Stop here for review. Camera and brick shake follow in the next visual feedback checkpoint.

## 12. Screen shake and neighboring brick reactions

- New scene: `Assets/_Project/Scenes/Medieval06_ImpactMotion.unity`, first in Build Settings. Earlier scenes, prefabs, materials/shaders and gameplay code remain unchanged.
- Added capped, damped brick-impact camera pulses, stronger life-loss jolts while playing, and small nearby-brick reactions. Menus/results/HUD stay stable; original immediate end-of-round timing is preserved.
- Camera motion is applied only as a URP render-time view override, restored after rendering and before input/physics callbacks. The camera transform/projection stay fixed. Adaptive constant background overscan covers the maximum displacement without zooming on each impact.
- A new ReactiveRunestoneBrick prefab retains the original root SpriteRenderer, collider and LevelBrick settings. One child renderer per brick copies its appearance and alone receives the motion/compression. Root rendering is suppressed without disabling its data; collider/health/points and hit bounds remain fixed.
- Controls on `GameFeel - Impact Motion 06 / ImpactMotionFeedback`: separate Enable Camera Shake and Enable Brick Reactions toggles, a zero-to-one Motion Strength slider, amplitudes, durations and neighbor radius. Motion Strength 0 disables the whole new pass. Reset, disable/re-enable, menu and retry return the view and children to rest.
- Review: stop Play, pull main and open **Medieval06_ImpactMotion**. Break isolated/clustered bricks and use Fireball. Check a small capped camera pulse and only nearby survivors reacting, with no overlapping visual bricks or increased displacement during rapid hits.
- Deliberately lose non-final lives: compare the stronger brief jolt with brick impacts. Test mouse following over the same screen position during motion, keyboard movement, edge bounces and source/collider bounds. The HUD should remain still, the background should cover edges, and normal controls/speeds must match MasonryParticles.
- Toggle each motion setting and disable the controller/reaction component mid-pulse. Check exact rest recovery, visibility, current brick tint and cleanup. Test loss/retry, win/Next Level, menu transitions and stop/restart Play twice.
- Validation: C# syntax, scene/prefab/reference/isolation, pulse bounds, clearance/overscan and callback lifecycle/source checks pass. Actual Unity compilation, render callbacks, renderer bounds, input behavior and visual/play-mode verification are pending.
- Stop here for review. Ball/paddle polish and event/result audio remain the next checkpoints.

## 13. Castle paddle, enchanted orb and contact feedback

- New scene: `Assets/_Project/Scenes/Medieval07_BallPaddle.unity`, first in Build Settings. All earlier scenes and Assets files remain unchanged.
- Added a wood-grain paddle with rounded brass caps, rivets and jade inlays, plus a shaded ivory/gold orb, faint halo and a shorter/thinner gold trail. Paddle rebound is smaller and faster in this scene only.
- Paddle/wall/actual brick contacts produce a short bounded orb compression/glint. Paddle contacts also use a four-slot expanding-ring pool. Only visual children move; original colliders, root transforms, speeds, score, lives and input remain unchanged.
- Original source renderers keep their sprite/material/tint/bounds while their rendering is hidden. This preserves the material used by Fireball. Giant's Grip keeps its existing widened collider and brightens the jade inlay. The brick observer cannot award points or change velocity; remote spell hits do not trigger orb feedback.
- Review: stop Play, pull main and open **Medieval07_BallPaddle**. Compare graphics with ImpactMotion at 1920x1080 and 1280x720. Check wood/caps/rivets, orb shading, restrained halo and a clear ball silhouette over the castle background and debris.
- Test central/edge paddle hits, walls and bricks in Easy, Medium and Hard. Contact animation must settle, paddle/ball controls and speed must match, and rapid hits must never stack the squash or grow the four-ring pool.
- Cast Fireball and Giant's Grip. Fireball must retain its original flame/core look and score normally; remote brick hits should not pulse the orb. Verify widened paddle edge bounces and fixed-width brass caps/inlays.
- Test life loss, retry, Next Level and return/menu mid-effect. Toggle Contact Feedback/Strength, set Halo Opacity to zero, and disable/re-enable the skin controller. The original rendering must return on disable, new effects must reset, and no duplicate bodies/rings should appear.
- Validation: C# syntax, scene/material/metadata and source preservation, fixed physics/gameplay/source-renderer blocks, pulse/scale/pool bounds and lifecycle checks pass. Unity engine/shader compilation, actual rendering/collisions/input and play-mode review remain pending.
- Stop here for review. Richer impact and separate life-loss/victory/game-over audio remain the next checkpoint.

## 14. Castle impacts and separate life/result sounds

- New scene: `Assets/_Project/Scenes/Medieval08_CastleAudio.unity`, first in Build Settings. It replaces only the new scene's old audio component and feedback-object name; all earlier Assets files stay unchanged.
- Added eight original PCM cues: wooden paddle pluck, quiet stone wall tap, three stone-break variations, a short descending life-loss cue, an ascending victory fanfare and a slower game-over toll. The reproducible standard-library generator is `tools/generate_castle_audio.py`.
- Four fixed impact voices handle rate-limited contacts with subtle panning/variation; one reserved voice handles life/result cues. Final-break victory is checked before throttling. The final life plays game-over alone, and a once-per-round gate prevents repeated result playback.
- Existing events and IsPlaying identify outcomes after the session commits its state. No gameplay timing, physics, ability or UI code changes are introduced. Life loss still relaunches immediately; new impacts lower briefly beneath the life cue.
- Review: stop Play, pull main and open **Medieval08_CastleAudio**. Compare paddle, wall and brick sounds; trigger rapid Fireball breaks, repeated collisions and a final Fireball hit. Check exact scoring and a clear victory cue even after a rapid preceding break.
- Lose a non-final life, then lose all lives: the short cue should accompany remaining lives, and only the longer toll should accompany game over. Repeat all nine level/difficulty choices and win/Next Level, retry and return/menu flows.
- Tune Master/Impact/Life Loss/Result volume, Muted and Stereo Width on CastleAudioFeedback. Changes should affect already-playing new cues. Spell audio retains its existing settings. Disable/re-enable the controller and stop/restart Play twice: no stale cues, repeated fanfares, duplicate listeners or extra voices should remain.
- Validation: source syntax, scene/clip/metadata references, frozen assets/gameplay/graphics, deterministic WAV hashes, PCM peaks/RMS/DC/boundaries, event-order and lifecycle source checks pass. Unity import, audio playback/mixing, actual device listening and play-mode review are pending.
- Stop here for review. Medieval background music and its transitions remain a separate checkpoint.

## 15. Original courtyard music and round transitions

- New scene: `Assets/_Project/Scenes/Medieval09_CastleMusic.unity`, first in Build Settings. All earlier Assets files and gameplay remain unchanged.
- Added two original 15-second, 96 BPM, 3/4 arrangements: a calm lute/bell/drone menu theme and a fuller bass/pluck/frame-drum play version. Matching sample counts and a shared DSP start keep them aligned for linear crossfades.
- A separate music root survives normal GameFeel reset toggles. Music continues through lost lives/retry; life loss briefly lowers it, results fade it out, and returning to menu or choosing Next Level fades the calm version back in. Existing sound effects stay in place.
- The controller observes existing score/break/loss events and adds one state-only Next Level button listener after UI creation. Disabling stops playback while state observers remember menu/round transitions until destruction; re-enabling starts a new synchronized phrase in the current remembered context. Device changes reschedule the pair together.
- Review: stop Play, pull main and open **Medieval09_CastleMusic**. Listen through at least five loop boundaries in the menu and during play; check no clicks, gaps, drift, doubled backing or volume spikes. Move between menu/difficulties/play quickly and verify smooth transitions without phrase restarts.
- Lose non-final lives, retry, win with ball/Fireball, lose all lives, use Next Level and return/menu. Existing life/result cues should remain clear, with no game-flow delay; result music should fade to silence and menu music should resume at selection screens.
- Tune Music Volume/Muted independently of SFX and spell settings. Check life ducking and live slider changes, disable/re-enable mid-transition/results, and change output device if available. Two music voices should remain fixed, with one existing AudioListener and no stale button/event subscriptions after scene unload.
- Regenerate via `python3 tools/generate_castle_music.py` using Python and FFmpeg/libvorbis. Source checks cover syntax/wiring/preservation, deterministic encoder output on the same version, decoded sample counts/levels/seams, crossfade bounds and lifecycle. Actual Unity compilation/import, playback, phase alignment and listening review remain pending.
- Stop here for review. A compact pause/settings panel is the next proposed checkpoint.

## 16. Pause, sound sliders and motion settings

- New scene: `Assets/_Project/Scenes/Medieval10_CourtSettings.unity`, first in Build Settings. Earlier Assets files remain unchanged.
- Added a retained jade/gold modal with serif headings, large labels, music/SFX percentage sliders, independent screen/brick motion toggles, Resume/Close, Main Menu and Restore Defaults. Open with Esc or the lower-left entry, placed clear of the spell bar.
- A playing round pauses through owned Time.timeScale restoration, with its gameplay and ability components left enabled. Early legacy-input reset and the existing new Input System UI module separate modal input from spell shortcuts; the overlay blocks underlying clicks. These input/engine assumptions need Unity play-mode review.
- A new native mixer routes this scene's two music voices separately from impact/result/spell voices. Relative sliders retain authored source balance at 100%; the original ducking and music transitions remain in place. Four namespaced PlayerPrefs values persist only this checkpoint's preferences.
- Motion toggles suppress only world-camera rendering and cached brick visual children, preserving physics/root transforms. The adapters refresh brick caches after rebuilds and render order after feedback resets. No earlier scripts are edited.
- Review at 1920x1080 and 1280x720: open from menus, results and all nine level/difficulty combinations; navigate by mouse and keyboard, adjust both buses, toggle motion, restore defaults and close/resume. Check readable focus/readouts and no overlap with the HUD or spell bar.
- Pause with active Fireball, Giant's Grip and Rune Shield. Try every shortcut and click outside the modal: no spell/cooldown/life/score state should change while paused. Resume with Esc/Enter/mouse and verify no cast or selection leaks through. Check audio continues independently.
- Test Main Menu, disable/re-enable the settings component, scene unload/reload and stop Play while paused. Time scale must restore; audio routing, UI, render/event subscriptions and persistent values must clean up correctly. Check the two motion options independently after retry and new-board rebuilds.
- C# syntax, scene/mixer/GUID/asset preservation, geometry bounds, dB mapping and lifecycle/input-order source checks pass. Unity compilation/mixer/font import, actual input blocking/pause preservation, layout/focus, motion/persistence and playback remain pending.
- Stop here for review. Level introductions and richer result presentation are the next proposed checkpoint.

## 17. Chapter banners and result presentation

- New scene: `Assets/_Project/Scenes/Medieval11_ChapterPresentation.unity`, first in Build Settings. All prior Assets files remain unchanged.
- Added a short heraldic level/difficulty banner beside the left pillar, with a gutter-safety check and no countdown or launch delay. Pause freezes its remaining time; non-final life loss does not restart it.
- Added castle shield seals, a framed 60-point final-score readout, gold CLEAR/coral RETRY accents and a context-specific next-step line. The original score text remains exact and the existing Try Again/Next Level/Main Menu actions and navigation remain immediately available.
- One retained UI hierarchy reuses the existing body/display fonts. New visuals ignore raycasts and sit below settings. Result score styling restores on menu transitions/disable; decorative fading introduces no game-flow delay or new audio.
- Review all nine level/difficulty choices at 1920x1080 and 1280x720. Check correct chapter/difficulty, a readable banner outside the collision court, normal immediate ball launch and unchanged HUD/spell-bar input. Narrow viewports should hide the banner if the gutter is insufficient.
- Pause/resume during the banner, retry, return/menu and continue through Next Level. The banner should appear once per fresh round, without stale names or a replay on non-final life loss.
- Win with ball/Fireball and lose all lives. Check exact final scores, score-plaque/seal spacing, correct next-chapter cue and all existing mouse/keyboard actions. Crown should retain only its valid return action. Settings must remain above all new decoration.
- Disable/re-enable the new controller during play/results and stop/restart Play twice. Old score size/rectangle should restore, earlier menus should not inherit result styling, resources/listeners should not grow, and result audio should not replay.
- Source checks cover syntax/wiring, scene/asset preservation, mode-context/catalog contract, font/layout/gutter/fade bounds and lifecycle/score ownership. Actual Unity compilation/import, font rendering, layering, input and play-mode review remain pending.
- Stop here for review. Spell visuals and ability feedback are the next proposed checkpoint.

## 18. Fireball flame, trail and casting visuals

- New scene: `Assets/_Project/Scenes/Medieval12_FireballVFX.unity`, first in Build Settings. Earlier Assets files remain unchanged.
- Added a centered hot flame head, warm halo, soft short ribbon, small cast ring and fading embers. A new native shader/four shared materials and one runtime quad provide the masks without downloaded media.
- The controller observes the existing reused Fireball transform and hides only its original rendering. It does not cast, move, collide, damage, score or change cooldowns. Native visuals restore when disabled; a mid-flight re-enable does not replay the cast ring.
- Sixteen reused ember sprites cap emission at 32/s and four spawns per frame, with a separate cosmetic random stream. Normal expiry lets the tail finish; life loss, reset, menus/results and disable clear it. Recast repositions with emission off and clears history. No objects/materials/audio voices are created per cast.
- Review all nine level/difficulty choices at 1920x1080 and 1280x720. Cast from center and paddle edges, including during expanded-paddle play. Check centered head, trailing flame direction, restrained halo, readable normal ball and no opaque quad/square artifacts.
- Pierce clustered bricks and the final brick. Confirm unchanged speed/radius/damage, one score award per brick, original destruction effects and immediate correct victory/audio/menu behavior. Cast attempts during cooldown/settings must not produce false rings or embers.
- Pause during flight, expire naturally, lose a life mid-flight, retry, Next Level and return/menu. Check frozen motion while paused, no trail bridge/leftover embers and no changed spell/score state. Disable/re-enable mid-flight and inspect original renderer/material/trail restoration.
- Inspect the fixed 19-sprite/one-trail hierarchy and runtime quad through repeated casts and stop/restart Play. Shader/C# import, actual rendering/vertex colors, layering, cleanup, input and play-mode/performance review remain pending; source/reference/preservation and numeric bounds checks pass.
- Stop here for review. Rune Shield presentation is the next proposed checkpoint, with Giant's Grip kept separate.

## 19. Rune Shield ward, status and deflection

- New scene: `Assets/_Project/Scenes/Medieval13_RuneShieldVFX.unity`, first in Build Settings. All earlier Assets files stay unchanged.
- Added a jade/gold runic ward, two crystal anchors, a gold deflection pulse, twelve fading shards and an ARMED / DEFLECTED plaque beside the right pillar. The original protection plane stays fixed; the rune row rises into the gap above the spell bar, with world visuals behind ball/paddle and a gutter safety check for the badge.
- The controller reads the original line's visibility/width flash and hides only its rendering. It never changes cast eligibility, one-use consumption, cooldown, duration, collision or reflection. Disabling restores the old line; mid-ability re-enable does not replay bursts.
- One quad, three materials, fifteen SpriteRenderers and one retained font-borrowing badge are reused. Twelve shard slots cap the deflection burst; cosmetic randomness is independent. Pausing freezes effect ages, and life/reset/menu/disable clears them.
- Review all nine level/difficulty choices at 1920x1080 and 1280x720. Cast with 3/keypad 3/hotbar, verify ARMED, visible runes above the cards, correct endpoints and no blocked pointer or hidden ball. Try cooldown/settings-blocked casts: no false activation should appear.
- Let the shield expire unused, then deflect a miss in another run. Expiry should hide without shards; a real deflection should pulse gold, emit one bounded burst and show DEFLECTED while retaining the existing upward reflection and cue. It must not grant a second protection.
- Test pause, life loss, retry, Next Level, return/menu, rapid/low-frame-rate play and disable/re-enable. Check no stale badge or fake burst, restored native renderer flags and unchanged Fireball/other abilities. Narrow viewports should hide the plaque before it overlaps the court.
- Source checks cover syntax/wiring/frozen assets, native width-marker/lifecycle ownership, plane and HUD clearance, shader masks and shard bounds. Unity engine/shader import, actual readability/detection/rendering, cleanup and play-mode/performance checks remain pending.
- Stop here for review. Giant's Grip presentation is next.

## 20. Giant's Grip polish and combined regression review

- Stop Play, pull main and open `Assets/_Project/Scenes/Medieval14_GiantGripVFX.unity`, now first in Build Settings. Every earlier scene remains available.
- Choose any level/difficulty and cast 2, keypad 2 or the second spell card. The original paddle expands immediately; emerald aura and gold edge runes follow its actual collider geometry, calculated from the current pose to avoid a stale physics-bounds read with AutoSyncTransforms off. A brief rising burst and ENLARGED plaque confirm the change.
- Wait for the native six-second effect to expire. The physical paddle returns normally; gold motes and a brief RESTORED plaque confirm expiry. Casting during cooldown should produce no false effect.
- Check both movement limits, simultaneous Fireball/Rune Shield, pause/resume, losing a life, retry, Next Level and return to the menu. No new effect should change reach, speed, score, protection, timers or sound. A disable/re-enable during expansion should not repeat a cast burst.
- The badge sits above Settings in the left gutter; the shield badge remains on the right. Check 1080p/720p and a narrow viewport. Tune Aura Opacity and Enable Motes on the new root; inspect the fixed eight-mote pool.
- Available regression checks pass: 12 existing source tests and 7 added asset/scene/audio-payload tests, plus C# syntax, serialized-data and numeric shader/text/layout checks. All 372 older Assets blobs are preserved.
- Engine verification is still open: Unity/.NET are absent here. Import/compile in Unity 6000.3.2f1, run the three rule harnesses and complete the nine-choice play matrix in TESTING.md before treating this as a tested build. Record any actual failure for a focused follow-up fix.
