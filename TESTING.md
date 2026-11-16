# Unity verification and submission checklist

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

Automated checks: `python3 tests/verify_project.py` (nine source/asset checks) and `dotnet run --project tests/ArcadeRoundChecks.csproj` (.NET 8, 7,868 rule assertions). These pass in the implementation environment. Unity import, C# engine compilation, physics, UI interaction and visual/audio checks above still require the editor.

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
