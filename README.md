# SWE402 — Improving Game Feel

Jehad Alharbi — 202248820

## Run

1. Open this repository's root folder in Unity Hub using **6000.3.2f1**.
2. Allow the existing packages to import. No extra tweening or audio packages are required.
3. Open `Assets/_Project/Scenes/Arcade.unity` for the playable game with its main menu. This is also the first scene in Build Settings.
4. Select 16:9 in Game view, press Play, and choose Easy, Medium, or Hard. The ball starts only after a difficulty is selected.
5. Move with the mouse, A/D, or the left/right arrows. Menus support mouse selection or arrow keys and Enter.

For the assignment comparison, open `Assets/_Project/Scenes/Before.unity` or `Assets/_Project/Scenes/After.unity`. Their gameplay rules remain unchanged: the ball launches automatically, the final lost life resets score/lives, and stopping/restarting Play restores the board. The extra gameplay systems below are isolated in Arcade.

## Arcade mode

| Difficulty | Ball speed (world units/second) | Starting lives |
|---|---:|---:|
| Easy | 9 | 3 |
| Medium | 12 | 3 |
| Hard | 15 | 3 |

Each run has the same 28-brick board and awards 10 points per brick. The chosen speed applies to the initial launch, every bounce, and every relaunch after a miss.

- Losing the third life opens **Game Over** with the final score, **Try Again**, and **Return to Main Menu**.
- Try Again keeps the difficulty and rebuilds the full board, resets the score, and restores three lives.
- Breaking the last brick opens **Victory** with the final score and **Return to Main Menu**.
- The ball and paddle stop on both result screens. Returning to the menu clears the previous board and feedback before another difficulty is chosen.

`Assets/_Project/Arcade` contains the independent round rules, scene controller, gameplay adapters, and gold/ivory menu UI. The mode reuses the existing paddle controls, physics assets, and five feedback components. Its brick prefab is duplicated so the comparison scenes retain their original rules and assets.

## Intended feel

Playful, bouncy, and easy to read. Small, brief reactions make contact feel responsive without hiding the ball or overwhelming the player.

## Improvements

The five feedback scripts and rendering assets are in `Assets/_Project/Feedback`. They are already assigned on `GameFeel - After only` in After and `GameFeel - Arcade only` in Arcade.

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

The independent round-rule tests run with `dotnet run --project tests/ArcadeRoundChecks.csproj` using the .NET 8 SDK. They compile the actual `ArcadeRound.cs` and pass 7,868 assertions across fixed scenarios and 300 randomized runs. The nine Python source checks also pass. The .NET SDK is only needed for these optional checks, not for opening or playing the Unity project.

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

OpenAI ChatGPT/Codex assisted with interpreting the brief, inspecting the starter, writing the five feedback scripts, original procedural audio, the simple feedback shader/material, After scene wiring, the optional Arcade mode and menus, executable round-rule tests, source-level checks, and documentation. No completed JuiceLab feedback modules were imported.

Student review, Unity tests, and revisions: **[Describe the tests and changes you actually performed. Do not submit this placeholder.]**

## Credits

- Course starter: supplied SWE402 JuiceLab Dead Starter, adapted from [JuiceLab](https://github.com/serkoceyhan/JuiceLab) by Serkan Ceyhan, MIT. Original [LICENSE](LICENSE) and [SOURCE.md](SOURCE.md) are retained.
- Existing Unity/TextMesh Pro assets and their included licenses are retained.
- New feedback scripts, shader, and synthesized sound cues were created for this assignment with AI assistance. No downloaded sound samples or paid assets are used.
