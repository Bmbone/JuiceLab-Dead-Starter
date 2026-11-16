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
