# After

### SWE402 — Improving Game Feel

**Jehad Alharbi · 202248820**  
**Unity 6000.3.2f1 · C# · Universal Render Pipeline 2D**

[Watch the demonstration video](https://youtu.be/BF2wYM0IkJU)

After is a medieval brick-breaker developed from the SWE402 JuiceLab Dead Starter. My aim was to make each contact, destroyed brick and earned point feel responsive and rewarding while keeping the ball and paddle easy to follow.

The project combines particle effects, reactive animation, event-based sound and animated interface feedback. The final version extends the presentation with castle masonry, an enchanted ball, a wood-and-brass paddle, original audio, spell effects, and a larger jade-and-gold interface.

## Running the project

1. Add the repository folder containing `Assets`, `Packages` and `ProjectSettings` to Unity Hub.
2. Open it with **Unity 6000.3.2f1** and allow package import to complete.
3. Open [Assets/_Project/Scenes/After.unity](Assets/_Project/Scenes/After.unity), the final polished scene and sole entry in Build Settings.
4. Set Game view to **16:9**, press Play, then select a level and difficulty.

**After** is the final project and scene name. The repository contains one scene: `Assets/_Project/Scenes/After.unity`. It includes the complete medieval presentation, level/difficulty selection and abilities. Earlier development scenes have been removed from the current branch; their revisions remain in Git history.

The display font is stored with Git LFS. A Git checkout with LFS support retrieves the font asset; `git lfs pull` downloads it when needed.

## Controls

| Action | Input |
|---|---|
| Move paddle | Mouse, A/D, or left/right arrows |
| Menu selection | Mouse, or arrow keys and Enter |
| Fireball | 1, keypad 1, Space, or first hotbar button |
| Giant's Grip | 2, keypad 2, or second hotbar button |
| Rune Shield | 3, keypad 3, or third hotbar button |
| Pause / settings | Esc or the Settings button |

## Game feel

The intended feel is **responsive, playful and satisfying**, supported by a consistent medieval style. Reactions are brief and localized so that impacts remain clear during fast play.

Paths in this table are relative to `Assets/_Project`.

| Improvement | Trigger | Implementation | Purpose |
|---|---|---|---|
| Brick impact | `GameEvents.BrickDestroyed` | `Feedback/BrickBurstFeedback.cs`; final `MasonryParticles/MasonryParticleFeedback.cs` | Colored particles and masonry fragments confirm destruction. |
| Reactive animation | `GameEvents.BallHitPaddle` | `Feedback/PaddleSquashFeedback.cs`; paddle visual child | A short squash and rebound make contact feel bouncy without changing its collider. |
| Event-based audio | Paddle contact, brick destruction and ball loss | `Feedback/EventAudioFeedback.cs`; final `CastleAudio/CastleAudioFeedback.cs` | Distinct sounds communicate contact, reward and failure. The three required sounds form one improvement. |
| Interface feedback | Positive `ScoreChanged` event | `Feedback/ScorePopFeedback.cs`; final `Presentation/MedievalScoreFeedback.cs` | A brief scale/color/lift pulse and floating gain label emphasize earned points. |
| Ball trail | Ball movement; cleared on life loss/reset | `Feedback/BallTrailFeedback.cs` | A short, tapered trail improves motion readability. |

Brick destruction combines particles and sound, while paddle contact combines animation and sound. Reactions use saved resting poses and reusable effects to avoid permanent deformation or an increasing number of objects.

## Visual and audio design

- **Castle setting:** stone pillars, masonry walls, runestone bricks and courtyard artwork.
- **Brick destruction:** break animation, layered debris, floating impact scores and nearby brick reactions.
- **Ball and paddle:** an enchanted orb, wood-and-brass paddle surfaces, contact rings and restrained visual squash.
- **Interface:** larger typography, framed score/lives panels, level previews, difficulty cards, result screens and chapter banners.
- **Sound:** separate contact, brick, life-loss, defeat and victory cues, plus menu/play music transitions.
- **Motion settings:** Screen Shake and Brick Motion switches, with separate Music and Sound Effects volume controls.

## Levels and difficulty

Level and difficulty selection is the selected gameplay extension. Different formations add variety, while independent speed choices let the player control the challenge.

| Level | Formation | Bricks | Maximum score |
|---|---|---:|---:|
| Courtyard | Four solid rows | 28 | 280 |
| Twin Towers | Two side towers with an open center | 26 | 260 |
| Crown | Crown-shaped tips, a broad center and a tapered base | 24 | 240 |

| Difficulty | Ball speed (world units/s) | Starting lives |
|---|---:|---:|
| Easy | 9 | 3 |
| Medium | 12 | 3 |
| Hard | 15 | 3 |

All nine combinations are available. Each brick awards ten points. Retry restores the selected layout and difficulty, three lives, zero score and ready abilities. Winning Courtyard or Twin Towers offers the next level's difficulty selection; Crown victory returns to the menu. Scores are tracked per level.

## Abilities

| Ability | Effect | Duration | Cooldown |
|---|---|---|---:|
| Fireball | Fires upward through multiple bricks, with a flame core, ribbon and embers. | Until its ceiling/lifetime limit | 5 s |
| Giant's Grip | Enlarges the paddle by 60%, with emerald aura, gold runes and activation/expiry feedback. | 6 s | 12 s |
| Rune Shield | Reflects one descending ball from below the paddle, with a runic ward and deflection feedback. | One use; expires after 8 s | 14 s |

The hotbar displays readiness, active duration and cooldown. A lost life cancels active effects while preserving remaining cooldowns. Victory, game over and returning to the menu clear the ability state.

## Design and implementation

Feedback is separated from the round rules through `GameEvents`. Score presentation observes changes while the session owns the numeric score. Collision reactions animate visual children rather than physics objects; Giant's Grip deliberately changes paddle width as a separate ability.

The final score pulse uses a **1.10 peak scale**, **8-unit upward lift** and **0.28-second duration**. Its gain popup lasts **0.55 seconds**. Later ability effects read existing projectile, shield and paddle state, and reuse fixed visual pools. Giant's Grip decorations calculate the collider shape from the current transform to avoid depending on cached physics bounds.

Development progressed through individual scenes before the completed version was consolidated into `After.unity`. Earlier versions remain available in Git history. Level/difficulty selection and abilities are gameplay extensions beyond the original feedback-only comparison.

## Validation

The current single-scene configuration passes **13 source/asset tests**: 6 project checks and 7 scene, metadata, material and audio-payload checks. The checks target `After.unity` and its single Build Settings entry. C# syntax parsing covers 63 scripts. Earlier implementation passes also included shader-mask samples, font metrics and layout calculations.

Unity C#/shader compilation, runtime input/physics, visual/audio review and target-build performance remain unverified in the implementation environment. Recorded peer-playtest findings are not included in the current validation results.

[Validation details](TESTING.md) · [Project structure](START-HERE.md) · [Development history](DEVELOPMENT-HISTORY.md)

## AI assistance

I used **OpenAI ChatGPT/Codex** to assist with planning, code and debugging, procedural visual/audio assets, UI design, scene integration, source validation and documentation. The work was integrated into the course starter rather than imported from a completed game-feel solution. The validation results above distinguish automated source checks from engine testing.

## Credits

- **Course starter:** SWE402 JuiceLab Dead Starter, adapted from [JuiceLab](https://github.com/serkoceyhan/JuiceLab) by Serkan Ceyhan under the MIT license. [LICENSE](LICENSE) and [SOURCE.md](SOURCE.md) are retained.
- **Unity and TextMesh Pro:** existing assets and their included licenses.
- **Typography:** Liberation Sans and a renamed subset of Liberation Serif Bold; [font license](Assets/_Project/Presentation/Fonts/FONT-LICENSE.txt).
- **Castle background:** artwork supplied by the project owner.
- **Additional work:** feedback code, procedural shaders, generated sound cues and music developed for this project with AI assistance. No paid asset packs or downloaded sound samples were used.
