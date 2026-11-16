using System;
using JuiceStudy.Arcade;

internal static class ArcadeRoundChecks
{
    private static int checks;

    private static void Check(bool condition, string message)
    {
        checks++;
        if (!condition) throw new Exception(message);
    }

    private static void Reject(Action action, string message)
    {
        bool rejected = false;
        try { action(); }
        catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, message);
    }

    public static int Main()
    {
        var round = new ArcadeRound();
        Check(round.Phase == RoundPhase.Menu, "A fresh game starts in the menu.");
        Check(!round.TryLoseLife() && !round.TryBreakBrick(10), "Menu ignores gameplay callbacks.");
        Check(ArcadeRound.SpeedFor(Difficulty.Easy) == 9 &&
              ArcadeRound.SpeedFor(Difficulty.Medium) == 12 &&
              ArcadeRound.SpeedFor(Difficulty.Hard) == 15, "Difficulty speeds must be 9, 12 and 15.");
        Reject(() => new ArcadeRound(0), "Zero starting lives is invalid.");
        Reject(() => round.Start((Difficulty)99, 28), "Unknown difficulty must be rejected.");
        Reject(() => round.Start(Difficulty.Easy, 0), "An empty board must be rejected.");
        Check(round.Phase == RoundPhase.Menu, "Invalid starts must not leave the menu.");

        foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
        {
            round.Start(difficulty, 28);
            Check(round.Lives == 3 && round.Score == 0 && round.RemainingBricks == 28, "Fresh round resets everything.");
            round.TryBreakBrick(10);
            round.TryBreakBrick(10);
            Check(round.Score == 20 && round.Combo == 2, "Brick scoring and combo must advance.");
            round.HitPaddle();
            Check(round.Combo == 0, "Paddle contact ends a combo.");
            round.TryLoseLife();
            Check(round.Phase == RoundPhase.Playing && round.Lives == 2 && round.Score == 20,
                "First miss keeps the run and score alive.");
            round.TryLoseLife();
            Check(round.Phase == RoundPhase.Playing && round.Lives == 1, "Second miss leaves one life.");
            round.TryLoseLife();
            Check(round.Phase == RoundPhase.GameOver && round.Lives == 0 && round.Score == 20,
                "Final miss ends the run without silently resetting its score.");
            Check(!round.TryLoseLife() && !round.TryBreakBrick(10), "Game over must ignore late callbacks.");

            // This is the same reset used by the Try Again button: retain the selected difficulty.
            round.Start(round.Difficulty, 28);
            Check(round.Difficulty == difficulty && round.BallSpeed == ArcadeRound.SpeedFor(difficulty),
                "Retry must keep the difficulty and speed.");
            Check(round.Score == 0 && round.Lives == 3 && round.RemainingBricks == 28,
                "Retry restores every brick, three lives and zero score.");
            for (int i = 0; i < 27; i++) round.TryBreakBrick(10);
            Check(round.Phase == RoundPhase.Playing && round.RemainingBricks == 1, "Do not win early.");
            round.TryBreakBrick(10);
            Check(round.Phase == RoundPhase.Won && round.Score == 280 && round.RemainingBricks == 0,
                "Destroying the final brick wins with the final score included.");
            Check(!round.TryLoseLife() && !round.TryBreakBrick(10), "Win state rejects all late gameplay callbacks.");
            round.ReturnToMenu();
            Check(round.Phase == RoundPhase.Menu && round.Score == 0 && round.Lives == 0 && round.RemainingBricks == 0,
                "Return to menu clears the previous run.");
        }

        round.Start(Difficulty.Hard, 28);
        Reject(() => round.TryBreakBrick(0), "Zero-point brick must be rejected.");
        Check(round.RemainingBricks == 28 && round.Score == 0, "Invalid brick must not corrupt state.");

        // Exercise invariants across many interleaved runs, independently tracking expected totals.
        var random = new Random(402);
        for (int run = 0; run < 300; run++)
        {
            var difficulty = (Difficulty)(run % 3);
            round.Start(difficulty, 28);
            int destroyed = 0, lost = 0;
            while (round.Phase == RoundPhase.Playing)
            {
                if (random.Next(4) == 0) { round.TryLoseLife(); lost++; }
                else { round.TryBreakBrick(10); destroyed++; }
                Check(round.Score == destroyed * 10 && round.RemainingBricks == 28 - destroyed && round.Lives == 3 - lost,
                    "Interleaved brick/life events must preserve totals.");
                Check(round.BallSpeed == ArcadeRound.SpeedFor(difficulty), "Difficulty speed cannot drift during a run.");
            }
            Check((round.Phase == RoundPhase.Won && destroyed == 28) ||
                  (round.Phase == RoundPhase.GameOver && lost == 3), "Terminal reason must match board/lives.");
            int finalScore = round.Score, finalLives = round.Lives;
            for (int i = 0; i < 5; i++) { round.TryBreakBrick(10); round.TryLoseLife(); }
            Check(round.Score == finalScore && round.Lives == finalLives, "Late callbacks cannot change results.");
            round.ReturnToMenu();
        }
        Console.WriteLine($"PASS: {checks} assertions, all difficulties and 300 randomized complete runs.");
        return 0;
    }
}
