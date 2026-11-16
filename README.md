# SWE402 — Improving Game Feel

Jehad Alharbi — 202248820

## Run

1. Open this repository's root folder in Unity Hub using **6000.3.2f1**.
2. Allow the existing packages to import. No extra tweening or audio packages are required.
3. Open `Assets/_Project/Scenes/MedievalUI02_RoyalInterface.unity` for the latest playable checkpoint. It is first in Build Settings; UI01, Medieval, Levels, Abilities, Arcade, After and Before retain their earlier versions.
4. Select 16:9 in Game view, press Play, choose a level, then choose Easy, Medium, or Hard. The ball starts only after both selections are made.
5. Move with the mouse, A/D, or the left/right arrows. Cast with **1 or Space**, **2**, and **3**, or click the hotbar buttons. Menus support mouse selection or arrow keys and Enter.

For the assignment comparison, open `Assets/_Project/Scenes/Before.unity` or `Assets/_Project/Scenes/After.unity`. Their gameplay rules remain unchanged: the ball launches automatically, the final lost life resets score/lives, and stopping/restarting Play restores the board. Arcade adds the menu/difficulty rules, Abilities adds spells, and Levels adds layout selection. Earlier scene assets and scripts remain unchanged.

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

These are initial implementation choices, not claimed playtest findings. Change one value at a time, then record actual observations below. No camera shake or hit-stop is used.

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

OpenAI ChatGPT/Codex assisted with interpreting the brief, inspecting the starter, writing the five feedback scripts, original procedural audio, the simple feedback shader/material, After scene wiring, the optional Arcade mode and menus, the Abilities checkpoint (gameplay, hotbar, procedural visuals and audio), the Levels checkpoint (three layouts, selection, per-level difficulty and progression), executable rule tests, source-level checks, documentation, the isolated medieval score UI/feedback checkpoint, and the larger Royal Interface menus/HUD including the renamed font subset. No completed JuiceLab feedback modules were imported.

Student review, Unity tests, and revisions: **[Describe the tests and changes you actually performed. Do not submit this placeholder.]**

## Credits

- Course starter: supplied SWE402 JuiceLab Dead Starter, adapted from [JuiceLab](https://github.com/serkoceyhan/JuiceLab) by Serkan Ceyhan, MIT. Original [LICENSE](LICENSE) and [SOURCE.md](SOURCE.md) are retained.
- Existing Unity/TextMesh Pro assets and their included licenses are retained.
- New feedback scripts, shader, and synthesized sound cues were created for this assignment with AI assistance. No downloaded sound samples or paid assets are used.

