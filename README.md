# After — SWE402: Improving Game Feel

**Jehad Alharbi — 202248820**  
**Unity:** 6000.3.2f1 (Unity 6.3)  
**Final project / submission name:** After  
**In-game theme/title:** Medieval brick-breaker / Brickkeep

After improves the supplied brick-breaker with clearer impacts, reactive animation, distinct audio, animated score feedback and a short ball trail. The final polished version adds castle masonry, a larger jade/gold interface, richer destruction effects, restrained shake, music and ability presentation.

## Open and play

1. Pull `main` and add the folder containing `Assets`, `Packages` and `ProjectSettings` to Unity Hub. Your final project is named **After**.
2. Open it using Unity **6000.3.2f1** and allow packages/assets to import.
3. For the latest polished version committed here, open [Medieval14_GiantGripVFX.unity](Assets/_Project/Scenes/Medieval14_GiantGripVFX.unity). It is first in Build Settings.
4. Select **16:9** in Game view, press Play, choose a level, then choose a difficulty. Focus Game view for keyboard input.
5. Stop Play before switching scenes. Stop/restart Play for a fresh baseline board.

If you renamed the final polished scene to **After** locally, open that renamed final copy. The scene paths below identify the files currently committed to this repository; the older `After.unity` is a separate checkpoint.

| Version | Scene in this repository | Purpose |
|---|---|---|
| Before | [Before.unity](Assets/_Project/Scenes/Before.unity) | Original unpolished starter. |
| Original After comparison | [After.unity](Assets/_Project/Scenes/After.unity) | Five feedback improvements with the original gameplay rules. |
| Final polished After | [Medieval14_GiantGripVFX.unity](Assets/_Project/Scenes/Medieval14_GiantGripVFX.unity) | Latest medieval presentation, menus, levels/difficulties and abilities. |

The original Before/After pair preserves layout, speed, controls, collision shapes, scoring and lives behavior. The final extended version includes additional gameplay systems; its level/difficulty selection and abilities are optional extensions beyond that controlled comparison. All earlier scene checkpoints remain available.

## Controls

| Action | Input |
|---|---|
| Move paddle | Mouse, A/D, or left/right arrows |
| Select menus | Mouse, or arrow keys and Enter |
| Fireball | 1, keypad 1, Space, or first hotbar button |
| Giant's Grip | 2, keypad 2, or second hotbar button |
| Rune Shield | 3, keypad 3, or third hotbar button |
| Pause / settings in final version | Esc or the Settings button |

Spells, menus and pause settings belong to the extended version. In the original comparison scenes the ball launches automatically; losing the final life resets lives/score without rebuilding the board. In the extended version, Game Over offers retry/menu, and victory offers next-level/menu options as appropriate.

## Intended feel

**Responsive, playful and satisfying, with a coherent medieval identity.** Reactions should be brief enough to confirm contact and reward progress while keeping the ball, paddle and score readable.

## Five feedback improvements

Paths in this table are relative to `Assets/_Project`. Original comparison implementations remain available; the final version layers the later presentation on top.

| Improvement | Trigger | Implementation file / object | Purpose |
|---|---|---|---|
| Brick impact effect | `GameEvents.BrickDestroyed` | `Feedback/BrickBurstFeedback.cs`; final masonry effects in `MasonryParticles/MasonryParticleFeedback.cs` | Confirms destruction with a short burst of brick-colored particles/fragments. |
| Reactive animation | `GameEvents.BallHitPaddle` | `Feedback/PaddleSquashFeedback.cs`; paddle visual child | Makes contact feel bouncy while keeping the original comparison collider unchanged. |
| Event-based audio | Paddle contact, brick destruction, ball loss | `Feedback/EventAudioFeedback.cs`; final `CastleAudio/CastleAudioFeedback.cs` | Distinguishes successful contact, reward and failure through sound. The three required sounds count together as one improvement. |
| Interface feedback | `ScoreChanged`, when points increase | Original `Feedback/ScorePopFeedback.cs`; final `Presentation/MedievalScoreFeedback.cs` and ScoreText | Emphasizes earned points through a brief scale/color/movement pulse and floating gain label. |
| Additional technique: ball trail | Ball movement, cleared on life loss/reset | `Feedback/BallTrailFeedback.cs`; reused ball trail | Makes the ball's direction easier to follow without leaving a long distracting tail. |

Brick destruction combines particles and sound; paddle contact combines animation and sound. The extended presentation also includes castle pillars, stone bricks, larger menu/HUD text, brick reactions, life-loss screen shake, music, result screens and visual feedback for all three spells.

## Script to explain in the video

Show [MedievalScoreFeedback.cs](Assets/_Project/Presentation/MedievalScoreFeedback.cs), focusing on `OnScore` and `ApplyPulse`, then show its component in the final scene's Inspector.

- **Trigger:** `GameEvents.ScoreChanged`. A positive score increase starts the feedback; score-zero resets clear it.
- **Animation:** briefly enlarge, lift and brighten the text; show the earned points; return to the captured resting pose.
- **Current final-scene values:** Peak Scale **1.10**, Upward Push **8**, Pulse Duration **0.28 s**, Gain Duration **0.55 s**, Gain Rise **14**. The serialized Inspector value overrides the script's default Peak Scale of 1.12.
- **Why these controls matter:** a smaller peak/shorter reaction feels restrained; more scale or longer duration gives stronger emphasis. Record any actual old/new values you change after the playtest.
- **Repeated hits:** the pulse uses the saved rest pose, and the gain popup combines rapid increases. The script animates the display while LevelSession owns the real score.

This is a C# animation; an Animator clip is not required. The complete timed narration and footage checklist are in [VIDEO-SCRIPT.md](VIDEO-SCRIPT.md).

## Playtest and selected post-playtest feature

**Selected feature: player-selectable levels and difficulty.** This is the chosen optional extension to evaluate after the friend's Before/After playtest. Its implementation is already available in the repository; the actual playtest observations and any feedback-driven tuning are still to be recorded.

The assignment keeps the original layout and ball speed unchanged, so this extension is documented separately from the five required feedback improvements. Complete at least one small feedback revision based on what the tester actually notices as well.

| Stage | What to do / record |
|---|---|
| First playtest | Ask one other person to play Before and After. Ask which feels more satisfying, which reactions clarify events, and what distracts them or is hard to notice. |
| Actual observation | **[Record the tester's real comment. Leave their name out.]** |
| Required feedback revision | **[Component/setting, old value → new value, and how it addresses that observation.]** |
| Selected optional feature | Level selection plus Easy/Medium/Hard. Evaluate whether layout variety and player-selected challenge help; record the actual response. |
| Retest result | **[Record what improved, stayed unclear, or needs more work after the revision.]** |

No completed friend playtest or invented feedback is claimed here. Replace the bracketed fields after testing, then use those same facts in the video.

## Levels and difficulty — selected optional extension

| Level | Formation | Bricks | Maximum score |
|---|---|---:|---:|
| 1 — Courtyard | Four solid rows | 28 | 280 |
| 2 — Twin Towers | Two side towers with a central opening | 26 | 260 |
| 3 — Crown | Crown-shaped tips, broad center and tapered base | 24 | 240 |

| Difficulty | Ball speed (world units/s) | Starting lives |
|---|---:|---:|
| Easy | 9 | 3 |
| Medium | 12 | 3 |
| Hard | 15 | 3 |

Each level supports every difficulty. Retry keeps the selected combination and restores the board, three lives, zero score and ready spells. Winning levels 1/2 offers Next Level, followed by that level's difficulty selection; level 3 offers return to the menu. Scores are per level.

## Demonstration video

**Accessible video link:** [Add the real 2–3-minute narrated video link before submission.]

Use [VIDEO-SCRIPT.md](VIDEO-SCRIPT.md) to record:

1. **0:00–0:25:** Before gameplay, clearly labeled.
2. **0:25–1:20:** After gameplay: show all five feedback techniques and deliberately lose a life. Keep game audio audible under narration.
3. **1:20–1:50:** Score script/Inspector, its event trigger and one tuning parameter.
4. **1:50–2:20:** Actual friend feedback, the revision and a retest demonstration.
5. **2:20–2:45:** Brief optional level/difficulty feature demonstration.

## Verification

At the latest implementation checkpoint, **12 existing source checks and 7 additional asset/scene checks passed**. C# syntax, serialized data, asset references, shader-mask samples and layout calculations were checked. Older Assets were preserved.

Unity/.NET were unavailable for the latest implementation pass: engine C#/shader compilation, executable rule harnesses, listening/visual review, input/physics, builds and all nine level/difficulty playthroughs still need to be run. Source checks do not establish runtime correctness. The real-person playtest is also pending.

```sh
python tests/verify_project.py
python tests/verify_polish.py
dotnet run --project tests/ArcadeRoundChecks.csproj
dotnet run --project tests/AbilityChecks.csproj
dotnet run --project tests/LevelChecks.csproj
```

See [TESTING.md](TESTING.md) for engine checks and the play matrix. Before submission, verify original Before behavior, repeated hits, correct score, clean effect resets, readable ball/paddle and visible outcomes with audio muted. The final settings menu can disable Screen Shake and Brick Motion.

## AI use

OpenAI ChatGPT/Codex assisted with planning, code/debugging, procedural shaders and audio, feedback components, scene wiring, medieval UI/assets, optional levels/difficulties/abilities, effect polish, source checks, documentation and the video script. The student remains responsible for reviewing, testing and tuning the submission. No completed upstream JuiceLab feedback modules were imported.

**Student review, Unity testing and revisions:** [Record the tests and changes you actually performed.]

## Credits and development history

- Course starter: SWE402 JuiceLab Dead Starter, adapted from [JuiceLab](https://github.com/serkoceyhan/JuiceLab) by Serkan Ceyhan under MIT. [LICENSE](LICENSE) and [SOURCE.md](SOURCE.md) are retained.
- Existing Unity/TextMesh Pro assets and their licenses are retained. The imported display typeface is a renamed subset of Liberation Serif Bold, with its SIL OFL license and modification note included.
- Castle background artwork was supplied by the project owner. New procedural visual/audio assets and feedback code were developed for this project with AI assistance; no paid asset packs or downloaded sound samples were used.
- [DEVELOPMENT-HISTORY.md](DEVELOPMENT-HISTORY.md) preserves the detailed scene-by-scene implementation notes. [REVIEW-STEPS.md](REVIEW-STEPS.md) retains the review steps.
