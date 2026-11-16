# After — recording script (about 2 minutes 45 seconds)

Record in 16:9. Label Before/After, keep game audio audible beneath your voice, and replace the three bracketed playtest fields with real observations. This is a narration script and shot list, not a captured gameplay clip.

## Footage and narration

### 0:00–0:25 — Before

**Show:** the original `Before.unity`: paddle contact, a brick breaking, score changing, then ordinary play. Keep the BEFORE label visible.

> This is the original brick-breaker before my improvements. The paddle, ball, bricks, score, and lives work, but there is very little feedback. Collisions feel plain, and breaking a brick does not feel especially rewarding.

### 0:25–1:20 — After

**Show:** your final project named After, using the latest polished scene. Label it AFTER. Show several brick breaks, paddle contact, the score/trail, and a deliberate miss. Avoid casting a size-changing ability during the explanation of collision-only visual squash.

> My goal for After was a responsive, satisfying game with a consistent medieval style.
>
> First, destroyed bricks produce colored fragments and particles, making each break easier to notice.
>
> Second, the paddle reacts to contact with a brief squash and rebound. This animates its appearance without changing the collision shape.
>
> Third, paddle contact, brick destruction, and losing a life have different sounds. Brick destruction combines sound and particles to make the same event feel stronger.
>
> Fourth, gaining points makes the score flash, grow briefly, and display a floating points popup.
>
> Fifth, the ball has a short trail, helping the player follow its movement.

**Deliberately miss the ball.**

> Losing a life also produces a clear sound and screen shake. The larger interface, castle pillars, and matching colors help bring the presentation together.

### 1:20–1:50 — Your script / Inspector

**Show:** [MedievalScoreFeedback.cs](Assets/_Project/Presentation/MedievalScoreFeedback.cs), particularly `OnScore(int score)` and `ApplyPulse(float emphasis)`. Then show its component's Peak Scale and Pulse Duration in the final scene's Inspector. A C#-driven animation meets this part of the demonstration; no separate animation clip is needed.

> This is my MedievalScoreFeedback script. It listens for the ScoreChanged event and starts the animation only when the score increases.
>
> The Peak Scale setting is 1.10, so the text becomes ten percent larger. Pulse Duration is 0.28 seconds, keeping the reaction brief.
>
> The animation returns to a saved resting position and scale, so repeated hits do not permanently enlarge or move the score.

**Technical note:** 1.10 is the actual serialized value in the final scene. The field default in the C# file is 1.12; show the Inspector for the scene value. LevelSession owns the score number; this script supplies presentation.

### 1:50–2:20 — Actual playtest and revision

**Show:** the effect/setting that you really revised, followed by a brief retest. Use the same actual observation and old/new values in README.md.

> I asked another person to compare Before and After. They noticed **[actual observation]**.
>
> Based on that feedback, I changed **[actual setting, including its old and new values]**.
>
> When we tested again, **[actual result]**.

If the friend playtest has not happened, leave this segment for a later take. Its results cannot be filled in from the source checks. A possible test target is score emphasis, particle intensity or shake strength; choose the revision based on what the tester actually reports.

### 2:20–2:45 — Selected optional feature: levels and difficulty

**Show:** Courtyard / Twin Towers / Crown, then Easy / Medium / Hard. Briefly launch a selection if time allows.

> For the optional post-playtest feature, I selected level and difficulty selection.
>
> The player can choose Courtyard, Twin Towers, or Crown, then select Easy, Medium, or Hard. The different formations add variety, while the difficulty options change the ball speed.
>
> This extends the game beyond the main feedback comparison and gives players more control over the challenge.

The feature's implementation is already available; this wording identifies it as the selected extension without inventing a request from the tester. The assignment's controlled Before/After comparison keeps layout and ball speed unchanged, so show this separately from the five required feedback techniques. Keep the required feedback revision grounded in the real test as well.

## Quick recording setup

- Open `Before.unity` for the first shot; the committed latest polished source is `Medieval14_GiantGripVFX.unity`. If you renamed that final scene to After locally, use the renamed final copy. The repository's original `After.unity` is an earlier comparison checkpoint.
- Capture game audio and microphone; check a short recording for balance before the full take.
- Use paused footage or a short cut to the script/Inspector, then return to gameplay.
- Keep the finished video between two and three minutes. Read naturally; allow remaining time for the on-screen actions.
- Add the accessible video link and real playtest details to README.md before submission.
