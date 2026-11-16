# Unity verification and submission checklist

## Masonry particles checkpoint

- Open `Assets/_Project/Scenes/Medieval05_MasonryParticles.unity` in Unity 6000.3.2f1. Let the new shader/materials/script import and check Console. No particle should render pink or as an opaque full billboard square.
- Break isolated bricks with the ball and Fireball, including side hits. Chips should spray from the contact face, rotate/fall and fade; powder should expand faintly; brass sparks should align with velocity and disappear quickly.
- At 1920x1080 and 1280x720, verify the ball remains readable through the burst, dust stays behind it, particle colors fit the brick, and top-row/edge hits stay visually clear.
- Compare with BrickBreak: retain the flash, carved four-piece break, impact +points and existing sound. Only the old uniform particle burst should be replaced.
- Produce rapid hits and a full Fireball column. The hierarchy should retain eight burst slots with three systems each. No per-hit GameObjects/material instances or unbounded particles should appear; score/lives and collision behavior must stay exact.
- Check default caps (120 simultaneously emitted particles across this pool), and optionally the Inspector maxima (264). Pool capacity changes require restarting Play. Higher intensity should change size/speed rather than emission count.
- Lose/retry, win/Next Level and return/menu while particles are alive. All resets must clear every system; idle menus must never emit particles continuously.
- Set each layer count to zero and restore it to confirm layers can be isolated. Disable/re-enable MasonryParticleFeedback: particles clear and resume on the next break; the separate break/score/audio effects continue. Test with audio muted and check frame rate during clustered breaks.
- Stop/restart Play twice and inspect for stale listeners or duplicate systems. Reopen BrickBreak and earlier scenes to verify their original particle implementation remains intact.

C# syntax, particle-mask bounds, event lifecycle, caps and scene/asset reference/preservation checks pass. Unity engine/shader compilation, actual particle rendering, layering/motion, frame rate and play-mode tests above remain pending.

## Brick break / impact scores checkpoint

- Open `Assets/_Project/Scenes/Medieval04_BrickBreak.unity` in Unity 6000.3.2f1; let the new script import and check Console for errors. The feedback component should already have both materials, camera and score text assigned.
- Break one brick with the ball. It must award the same ten points immediately and bounce normally, then show a warm flash, four matching irregular pieces with brief recoil/spin/fall/fade, and a shadowed gold +10 at that position.
- Hit different faces and use Fireball. Check direction, original stone/brass alignment, no oversized/missing pieces, and no duplicate score. Fragments should stay behind the live ball and intact bricks.
- Break several bricks rapidly and repeatedly cast Fireball. The fixed pool should not add hierarchy objects or material instances per hit; each reused slot should reset color, rotation, scale, alpha and label. Pool overflow may replace older visual effects but must never affect earned points.
- At 1920x1080 and 1280x720, break the top row and side columns. Popups should remain readable and within bounds clear of the top HUD/spell bar. Moving the pointer over labels must not stop paddle following.
- Lose/retry, win/Next Level, and return/menu while fragments are visible. Resets should clear all copies/popups. The existing immediate result screen covers effects rather than delaying game flow.
- Disable StoneBreakFeedback during an effect and score with it disabled. Normal score, collision, particles and audio should continue. Re-enable it mid-run: old points must not produce a bonus popup, and the next hit should show the correct new increase.
- Stop/restart Play twice and inspect for stale subscriptions, runtime sprite leaks or doubled effects. Check animation clarity/frame rate with rapid hits, and reduce speed/lifetime if fragments distract from the ball.
- Reopen StoneCourt and earlier scenes; their break behavior should remain unchanged.

Source C# syntax, scene/asset references and preservation, fragment polygon/UV geometry and popup-bound checks pass. Unity compilation, actual runtime sprite/text rendering, animation and play-mode tests above remain pending.

## Stone court checkpoint

- Open `Assets/_Project/Scenes/Medieval03_StoneCourt.unity` in Unity 6000.3.2f1 and allow shader/material/script import. Check Console for C# or shader errors; no object should render pink.
- Compare all three formations at 1920x1080 and 1280x720. Confirm colored beveled bricks, brass straps/rivets, carvings and restrained grain remain readable behind a fast ball.
- Inspect both side pillars: coursed stone, mouldings/feet, arrow slits, banners and crests should layer correctly, with no decoration extending into the reachable court. Check the top-wall battlement silhouette.
- Hit both wall faces and the ceiling repeatedly, including beside caps and banners. Collision positions and bounce behavior must match Royal Interface. No new collision component should be present on the decorative hierarchy.
- Break bricks normally and with Fireball. Each still awards ten points once; their full sprite bounds and collider should agree, with the existing burst color matching the new row tint.
- Check all nine level/difficulty choices, ability collisions, complete losses/retries, victory/Next Level and return/menu. Surface details and pillar decoration counts must not grow across round transitions.
- Stop/restart Play twice and check for material/property-block warnings. Inspect frame rate during rapid breaks; all surfaces should share the two material assets rather than creating per-brick material instances.
- Reopen Royal Interface and earlier scenes. Their brick materials, colors and plain walls must retain their earlier versions.

Source/asset checks pass. Unity C# and shader compilation, graphics, frame rate, physics/play-mode checks above and actual playtest evidence are pending. Break/recoil animation, impact score popups, new particles and shake are queued for later scene checkpoints.

## Royal Interface checkpoint

- Open `Assets/_Project/Scenes/MedievalUI02_RoyalInterface.unity` in Unity 6000.3.2f1. Let the new font and scripts import. Check Console for engine/shader/font errors.
- Test at 1920x1080 and 1280x720 in 16:9. Menu headings should use the new serif typeface; body text should remain clear. No clipped glyphs, placeholder squares or overlapping labels should appear.
- Check all three level-card previews match their formations, then exercise all nine level/difficulty combinations. The same original callbacks should run.
- Navigate cards with Left/Right and Up/Down, then Enter. Back, Next Level, Try Again and Return to Main Menu should all remain reachable by keyboard and mouse. Hover/press scale must settle and reset when a button is hidden.
- Lose all lives and retry the same board/mode. Win levels 1/2 and continue to the next difficulty selector; win Crown and return to the menu. Repeat transitions and verify panels, decorations and button listeners do not duplicate.
- During play, score, level/difficulty and lives should be readable in separate top panels. Check exact totals and the retained score flash/pulse/combined +points label during rapid hits.
- Check the wider spell bar, active/cooldown status, mouse clicks and original key bindings. Move the pointer over decorative frames and labels: they must not interrupt paddle tracking.
- Resize the Game view: the corner-pivot castle artwork should cover the viewport, stay centered and remain behind gameplay. Verify ball/bricks are never hidden by it.
- Stop/restart Play twice and inspect for stale runtime font/material objects or duplicate motion. Reopen UI01 and older scenes to verify their versions remain intact.

C# syntax and source/asset/text-metric checks pass. Unity engine compilation, font-atlas creation, actual layout and input/play-mode verification are pending. Brick styling, break animations, world-space score popups and shake are intentionally queued for later scene checkpoints.

## Medieval score UI checkpoint

- Open `Assets/_Project/Scenes/MedievalUI01_Score.unity`, select a level/difficulty, and test at 1920x1080 and 1280x720 in 16:9. The framed score must sit above the bricks without overlapping the level label or lives.
- Break a brick: the displayed score must remain exact, briefly turn ivory, grow and move upward, then return to its original gold/rest pose. A +10 label should float and fade.
- Break several bricks rapidly with the ball/fireball: the score must never lose points and the one gain label should combine recent gains without accumulating extra UI objects.
- Keep the pointer over the plaque while moving: it must not block paddle following.
- Disable/re-enable MedievalScoreFeedback during a pulse: scale, position and color must restore; scoring must continue with the component disabled, and re-enabling must not reward existing points.
- Lose all lives and retry, return to menu, change level/difficulty, and repeat. Score zero must not produce a reward popup, and each fresh run should show exactly one plaque.
- Reopen Medieval and all earlier scenes: their score/UI behavior must remain unchanged.

Source/asset comparisons passed. Unity import, engine compilation, layout and play-mode checks above are pending; no new playtest or audio result is claimed for this checkpoint.

## Levels checkpoint

- Open `Assets/_Project/Scenes/Levels.unity`. Play should show Courtyard, Twin Towers and Crown. Choose a level: its name/description and Easy/Medium/Hard should appear, while the ball remains stopped.
- Use Back to Levels and choose a different board. Changing selections or repeatedly clicking must not start/reset play until a difficulty is chosen.
- Exercise all nine combinations. Courtyard should have 28 bricks in four solid rows, Twin Towers 26 bricks in two tall flanks with an open center, and Crown 24 bricks in a crown formation. All must fit inside the existing court.
- Check the HUD identifies both selected level and difficulty. Verify Rigidbody2D ball speed is 9 on Easy, 12 on Medium, 15 on Hard, including after bounces and life loss.
- Score points, lose all lives, then Try Again. It must restore the same layout/difficulty with its correct brick count, three lives, zero score and all three abilities ready.
- Break all bricks, including the final brick with a fireball. Victory should occur exactly once with maximum score 280/260/240 for levels 1/2/3. Late hits must not change the result.
- Win level 1, select Next Level, and choose a different difficulty for Twin Towers. Repeat from Twin Towers to Crown. The next board must not auto-launch before choosing difficulty. Each new level starts with zero score, three lives and ready abilities.
- Crown's victory screen should offer Return to Main Menu without a nonexistent fourth level. Return from each result screen and replay any of the three levels directly.
- On every board, check fireball piercing/duplicate-hit scoring, widened-paddle edge bounces, one-use shield reflection, cooldown UI, life-loss cleanup, and retry/menu cleanup.
- Test mouse and keyboard menu navigation at 1920x1080 and 1280x720 in 16:9. Verify Back/Next/Return buttons do not overlap the controls text, and the mode label remains readable next to score/lives.
- Reopen Abilities, Arcade, After and Before; each should retain the behavior of its own checkpoint.

Automated level rules: `dotnet run --project tests/LevelChecks.csproj` (.NET 8, 5,042 assertions across all nine combinations and 300 mixed attempts). Source/asset checks: `python3 tests/verify_project.py` (twelve checks, including frozen older assets). The existing ability and round checks also pass. Unity engine compilation, visual layout, actual physics and UI interaction still require play-mode verification.

## Abilities checkpoint

- Open `Assets/_Project/Scenes/Abilities.unity` and start a difficulty. The three hotbar abilities should all be READY, and remain usable across Easy/Medium/Hard.
- Press 1 or Space, and also try clicking Fireball. A single flame should travel straight upward through several bricks, with a short orange trail and whoosh. Each brick still awards 10 points; rapid/simultaneous ball/fireball hits must not double-score.
- Press/release Fireball repeatedly during its 5-second cooldown. It should stay unavailable until ready, with a countdown and recharge bar. Hold a key: it must not cast repeatedly. Clicking another hotbar ability must not change what Space does.
- Press 2 or click Giant's Grip. The paddle should become 60% wider and green for 6 seconds, then return exactly to its normal size/color. Bounce near both expanded edges; aim should use the expanded collider width. Move to each wall: the expanded paddle must remain inside the court.
- Verify Giant's Grip cannot stack or recast during its 12-second cooldown.
- Press 3 or click Rune Shield. A cyan line should appear below the paddle. Deliberately miss: one descending ball should reflect upward, with a flash/cue and no lost life, keeping the selected speed. A second miss must lose a life. Upward movement must not consume the shield.
- Let another shield expire unused after 8 seconds. It should disappear and still require the remainder of the 14-second cooldown. Casting after a ball has already passed the line must not teleport it back.
- Cast multiple abilities together. Lose a life: all active buffs and the fireball should clear, while cooldowns keep counting. Retry/new difficulty must instead restore every ability to READY.
- Use the fireball to break the final brick. Victory must appear once with the correct final score; no spell remains active behind results. Repeat the loss/retry and return-to-menu flow with abilities active.
- Check keyboard and hotbar buttons at 1920x1080 and 1280x720 in 16:9. The hotbar should sit below the paddle without changing the score/lives UI. Mute audio and verify casts, duration, shield use and cooldowns remain clear.
- Reopen Arcade, After and Before: no abilities or changed rules should appear in those checkpoints.

Automated ability rules: `dotnet run --project tests/AbilityChecks.csproj` (.NET 8). Source/asset checks: `python3 tests/verify_project.py` (twelve checks, including frozen earlier assets). Existing round rules: `dotnet run --project tests/ArcadeRoundChecks.csproj`. These do not replace Unity engine compilation and the play-mode checks above.

## Arcade main menu and complete runs

- Open `Assets/_Project/Scenes/Arcade.unity` in Unity 6000.3.2f1. Press Play: the main menu should appear, with no moving ball or active paddle behind it.
- Start each difficulty. Inspect the ball's Rigidbody2D linear velocity magnitude during play: Easy 9, Medium 12, Hard 15. Check again after paddle/wall contact and after losing a life.
- Confirm every run begins with 28 bricks, score 0, and three lives. The top-center label identifies the chosen mode.
- Miss twice: play should continue with two lives, then one. Any earned score should remain.
- Miss a third time: Game Over should show the final score and exactly two buttons, Try Again and Return to Main Menu. The ball/paddle must stay stopped; score/lives must not reset behind the result screen.
- Click Try Again: the same difficulty should restart with all 28 bricks, three lives, score 0, and no old trail, particle burst or audio left from the previous run.
- Return to Main Menu after a loss, choose a different difficulty, and verify its speed is applied.
- Clear all 28 bricks: Victory should show score 280 and a Return to Main Menu button. It must not appear while any brick remains, and the stopped ball must not cause a later game over.
- Return to Main Menu after winning and start another run. Repeat menu/retry transitions several times; buttons, feedback and event listeners should not duplicate.
- Test button selection with mouse and with arrow keys/Enter. Check the main menu, Game Over and Victory at 1920x1080 and 1280x720 in 16:9.
- Reopen Before and After separately. Their original auto-launch and life-reset behavior must remain intact.

Automated checks: `python3 tests/verify_project.py` (twelve source/asset checks) and `dotnet run --project tests/ArcadeRoundChecks.csproj` (.NET 8, 7,868 rule assertions). These pass in the implementation environment. Unity import, C# engine compilation, physics, UI interaction and visual/audio checks above still require the editor.

## Import and first run

- Open with Unity 6000.3.2f1 and let packages import; check Console for C# and shader errors.
- Open Before, play in 16:9, and verify the silent original behavior. Stop Play mode.
- Open After. Select `GameFeel - After only`: all five script components and their references should be assigned.
- Press Play and confirm all five techniques. No additional Inspector setup should be necessary.
- If an error occurs, copy its full Console text, including the file name and line number, for debugging.

## Event and repetition checks

- Hit the paddle repeatedly: only its visual child compresses and always returns to its original scale. Its BoxCollider2D stays fixed.
- Break bricks rapidly: each burst uses the brick color, remains near the break, and disappears. The burst pool does not keep growing.
- Listen for distinct paddle, brick, and loss sounds. Repeated impacts must not become excessively loud. Adjust volume while listening on the target device.
- Verify every points change in the normal HUD text. Rapid gains should restart one score pop, not stack permanent enlargement.
- Deliberately lose a ball. The lives value changes; a distinct falling cue plays. The trail must not draw a line from the death zone to the launch point.
- Lose the final life. Confirm the starter resets score/lives; score zero should not get a reward pop. Bricks are not rebuilt (this is expected).
- Mute game audio. Brick disappearance/particles, score changes, and lives updates must still make events clear.
- Stop and restart Play mode twice. Effects/audio should not duplicate because of stale event subscriptions.
- Reopen Before and verify no feedback has leaked into it.

## Disabling feedback

While playing After, disable each feedback component in turn, then re-enable it. Reactions should stop, reset/clear, and resume without missing references or permanent scaling. The numeric score should continue updating when ScorePopFeedback is disabled because HUD always owns the text value. No camera-shake disable setting is needed because no shake is added.

## Real playtest

Ask one other person to play Before and After. Ask which feels more satisfying, what makes events easier to read, and what distracts them. Make at least one change based on what they say and retest. Record the observation, old/new value, and result in README; leave out their name. Do not describe an imagined playtest as evidence.

## Before submitting

- Replace every README placeholder, including the playtest, student testing statement, and real video link.
- Record 2–3 minutes with narration and game audio. Show Before for 20–30 seconds, all five effects in After, one implementation, and the actual revision.
- Test the video link from an account/viewer that has not been granted special access.
- Ensure GitHub contains Assets, Packages, ProjectSettings, README, LICENSE, and SOURCE, but not Library, Temp, or Logs.
- Submit the GitHub repository URL on Blackboard by November 16, 2026, 2:00 PM Asia/Riyadh.

