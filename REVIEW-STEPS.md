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
