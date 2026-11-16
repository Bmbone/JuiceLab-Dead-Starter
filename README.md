# SWE402 — Improving Game Feel

Jehad Alharbi — 202248820

## Run

1. Open this repository's root folder in Unity Hub using **6000.3.2f1**.
2. Allow the existing packages to import. No extra tweening or audio packages are required.
3. Open `Assets/_Project/Scenes/Medieval06_ImpactMotion.unity` for the latest playable checkpoint. It is first in Build Settings; MasonryParticles, BrickBreak, StoneCourt, UI02, UI01, Medieval, Levels, Abilities, Arcade, After and Before retain their earlier versions.
4. Select 16:9 in Game view, press Play, choose a level, then choose Easy, Medium, or Hard. The ball starts only after both selections are made.
5. Move with the mouse, A/D, or the left/right arrows. Cast with **1 or Space**, **2**, and **3**, or click the hotbar buttons. Menus support mouse selection or arrow keys and Enter.

For the assignment comparison, open `Assets/_Project/Scenes/Before.unity` or `Assets/_Project/Scenes/After.unity`. Their gameplay rules remain unchanged: the ball launches automatically, the final lost life resets score/lives, and stopping/restarting Play restores the board. Arcade adds the menu/difficulty rules, Abilities adds spells, and Levels adds layout selection. Earlier scene assets and scripts remain unchanged.

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

## Intended feel

Playful, bouncy, and easy to read. Small, brief reactions make contact feel responsive without hiding the ball or overwhelming the player.

## Improvements

The five feedback scripts and rendering assets are in `Assets/_Project/Feedback`. They are already assigned on the `GameFeel` object in After, Arcade, Abilities and Levels.

| Improvement | Trigger | Implementation file/object | Purpose |
|---|---|---|---|
| Brick impact | `GameEvents.BrickDestroyed` | `BrickBurstFeedback.cs`; Feedback material/shader | A short, brick-colored burst at the contact point confirms the break. Eight reusable systems keep the effect count bounded. |
| Reactive animation | `GameEvents.BallHitPaddle` | `PaddleSquashFeedback.cs`; `Paddle/Visual` | Immediate visual compression and a small rebound make contact feel bouncy. The physics parent and collider are never scaled. |
| Event-based audio | `BallHitPaddle`, `BrickDestroyed`, `BallLost` | `EventAudioFeedback.cs` | A soft pluck, bright chime, and falling loss cue distinguish the three events. Four collision voices and a dedicated loss voice bound overlapping audio. |
| Interface feedback | `ScoreChanged`, only on increases | `ScorePopFeedback.cs`; `Canvas/ScoreText` | A short scale pop emphasizes gained points. HUD remains the only writer of the numeric text; resets do not trigger a reward pop. |
| Additional: ball trail | Ball movement; clear after `BallLost` | `BallTrailFeedback.cs`; runtime `Ball motion trail` | A short taper improves movement readability without a long distracting tail or a line across respawns. |

Brick destruction combines colored particles and sound. Paddle contact combines squash and sound. The three sounds count together as one improvement.

## Design and tuning

Select `GameFeel - After only` in After to tune the Inspector values:

- Brick burst: particle count, lifetime, speed, and size. Initial settings: 16 particles, at most 0.42 seconds.
- Paddle squash: amount and duration. Initial settings: 0.18 and 0.22 seconds. Every reaction is based on the stored rest scale, not the previous deformed scale.
- Audio: master volume and separate volumes for the three cues. Clips are synthesized once at runtime, not regenerated each collision. Closely spaced duplicate collision cues are rate-limited.
- Score pop: peak scale and duration. Initial settings: 1.16 and 0.18 seconds.
- Trail: duration and width. Initial settings: 0.12 seconds and 0.16 world units. Ball loss is announced before relaunch, so the trail resets in LateUpdate after the move.

These are initial implementation choices, not claimed playtest findings. Change one value at a time, then record actual observations below. After uses no camera shake or hit-stop. The optional Impact Motion checkpoint adds the separately configurable shake described above.

Each feedback component subscribes in OnEnable and unsubscribes in OnDisable. Temporary effects finish or reuse fixed pools. Feedback does not consume UnityEngine.Random, preserving the gameplay's random launch generator.

## Playtest and revision — REQUIRED BEFORE SUBMISSION

Have one other person play both scenes. Do not include their name.

- Which version felt more satisfying, and why? **[Record actual feedback.]**
- Which feedback made events clearer? **[Record actual feedback.]**
- Was anything distracting or difficult to notice? **[Record actual feedback.]**
- Revision: **[Identify the actual component/parameter and old/new values.]**
- Result after the change: **[Record what you observed when retesting.]**

## Verification status

Source-level checks can be run with `python3 tests/verify_project.py`. They check baseline preservation, scene wiring, feedback event lifecycle, and new asset GUIDs. They do not compile C#, compile shaders, run Unity, or verify sound/appearance.

The independent round-rule tests run with `dotnet run --project tests/ArcadeRoundChecks.csproj` using the .NET 8 SDK. They compile the actual `ArcadeRound.cs` and pass 7,868 assertions across fixed scenarios and 300 randomized runs. `dotnet run --project tests/AbilityChecks.csproj` compiles the actual ability timers and brick-health rules and passes cooldown, shield-crossing, reset and duplicate-hit scoring tests. `dotnet run --project tests/LevelChecks.csproj` passes 5,042 assertions covering the three layouts, all nine level/difficulty combinations, retries, next-level selection, scoring and 300 mixed attempts. All twelve Python source/asset checks pass, including byte comparisons that freeze the earlier checkpoints. The .NET SDK is only needed for these optional checks, not for opening or playing the Unity project.

Unity is not installed in the implementation environment. Editor import, compilation, runtime behavior, sound balance, and the real-person playtest still require testing in Unity. Use [TESTING.md](TESTING.md) and replace this status with the tests you actually perform before submission.

## Demonstration video — REQUIRED BEFORE SUBMISSION

Accessible link: **[Add your real 2–3-minute narrated video link.]**

Suggested sequence:

1. 0:00–0:25: label and show Before gameplay.
2. 0:25–1:25: label and show After. Identify all five improvements; include paddle contact, brick destruction, score updates, and a deliberate ball loss.
3. 1:25–1:55: show one feedback script or Inspector. Explain its event trigger and one value you tuned.
4. 1:55–2:30: explain the real playtest feedback and demonstrate the revision.

Keep game audio audible alongside narration. Different ball trajectories between scenes are expected.

## AI use

OpenAI ChatGPT/Codex assisted with planning and debugging, the five core feedback components, procedural audio and shaders, scene wiring, optional Arcade/Abilities/Levels gameplay, medieval menus/HUD and font subset, stone bricks/castle walls, pooled break animation and impact scores, layered masonry particles, render-only camera shake and child-renderer brick reactions, source/rule checks, and documentation. No completed JuiceLab feedback modules were imported.

Student review, Unity tests, and revisions: **[Describe the tests and changes you actually performed. Do not submit this placeholder.]**

## Credits

- Course starter: supplied SWE402 JuiceLab Dead Starter, adapted from [JuiceLab](https://github.com/serkoceyhan/JuiceLab) by Serkan Ceyhan, MIT. Original [LICENSE](LICENSE) and [SOURCE.md](SOURCE.md) are retained.
- Existing Unity/TextMesh Pro assets and their included licenses are retained.
- New feedback scripts, shader, and synthesized sound cues were created for this assignment with AI assistance. No downloaded sound samples or paid assets are used.

