using System;
using System.Collections.Generic;
using JuiceStudy.Arcade;
using JuiceStudy.Abilities;
using JuiceStudy.Levels;

internal static class LevelChecks
{
    private static int checks;
    private static void Check(bool condition, string message)
    {
        checks++;
        if (!condition) throw new Exception(message);
    }
    private static void Reject(Action action)
    {
        bool rejected = false;
        try { action(); }
        catch (ArgumentException) { rejected = true; }
        Check(rejected, "Invalid selection or layout must be rejected.");
    }

    public static int Main()
    {
        int[] counts = { 28, 26, 24 };
        var fingerprints = new HashSet<string>();
        foreach (LevelId id in Enum.GetValues(typeof(LevelId)))
        {
            LevelDefinition layout = LevelCatalog.Get(id);
            int actual = 0;
            string fingerprint = "";
            for (int row = 0; row < layout.Rows; row++)
            for (int column = 0; column < layout.Columns; column++)
            {
                bool occupied = layout.HasBrick(row, column);
                if (occupied) actual++;
                fingerprint += occupied ? "#" : ".";
                Check(occupied == layout.HasBrick(row, layout.Columns - 1 - column), "Layouts must be symmetric.");
            }
            Check(actual == counts[(int)id - 1] && actual == layout.BrickCount, "Catalog must count actual occupied cells.");
            Check(layout.MaximumScore == actual * 10, "Maximum score follows the selected layout.");
            Check(layout.Columns == 7 && layout.Rows >= 4 && layout.Rows <= 6, "Layouts must fit the existing court grid.");
            Check(fingerprints.Add(fingerprint), "Every level must have a distinct brick distribution.");
            Reject(() => layout.HasBrick(-1, 0));
            Reject(() => layout.HasBrick(layout.Rows, 0));
        }
        var towers = LevelCatalog.Get(LevelId.TwinTowers);
        for (int row = 0; row < towers.Rows; row++)
            Check(!towers.HasBrick(row, 3) && towers.HasBrick(row, 0) && towers.HasBrick(row, 6),
                "Twin Towers needs an open center and occupied flanks.");
        var crown = LevelCatalog.Get(LevelId.Crown);
        Check(crown.HasBrick(0, 0) && !crown.HasBrick(0, 1) && crown.HasBrick(0, 3) && crown.HasBrick(0, 6),
            "Crown must have three top tips.");
        Check(crown.HasBrick(crown.Rows - 1, 3) && !crown.HasBrick(crown.Rows - 1, 2), "Crown must taper at the bottom.");
        Reject(() => LevelCatalog.Get((LevelId)0));
        Reject(() => LevelCatalog.Get((LevelId)4));
        Reject(() => new LevelDefinition(LevelId.Courtyard, "bad", "", "..."));
        Reject(() => new LevelDefinition(LevelId.Courtyard, "bad", "", "##", "#"));
        Reject(() => new LevelDefinition(LevelId.Courtyard, "bad", "", "#?"));
        string[] source = { "##" };
        var immutable = new LevelDefinition(LevelId.Courtyard, "copy", "", source);
        source[0] = "..";
        Check(immutable.BrickCount == 2 && immutable.HasBrick(0, 0), "Outside data cannot mutate an active layout.");

        foreach (LevelId id in Enum.GetValues(typeof(LevelId)))
        foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
        {
            var run = new LevelRun();
            Check(!run.TryStart(difficulty) && !run.TryRetry() && !run.TrySelectNext(), "Menu cannot launch a stale selection.");
            Check(run.SelectLevel(id) && run.ChoosingDifficulty && run.Round.Phase == RoundPhase.Menu,
                "Selecting a level opens difficulty selection without starting gameplay.");
            Reject(() => run.TryStart((Difficulty)99));
            Check(run.ChoosingDifficulty && run.Round.Phase == RoundPhase.Menu, "Invalid difficulty cannot start play.");
            Check(run.TryStart(difficulty), "Every level must support every difficulty.");
            Check(run.SelectedLevel == id && run.Round.Difficulty == difficulty && run.Round.BallSpeed == ArcadeRound.SpeedFor(difficulty),
                "Level and difficulty selections must both be applied.");
            Check(run.Round.Lives == 3 && run.Round.Score == 0 && run.Round.RemainingBricks == run.Layout.BrickCount,
                "A level starts with three lives, zero score and its actual brick count.");
            run.Round.TryBreakBrick(10);
            run.Round.TryBreakBrick(10);
            Check(!run.TryStart(Difficulty.Easy) && !run.SelectLevel(LevelId.Crown) && !run.TryRetry(),
                "Repeated clicks cannot change or reset an active run.");
            run.Round.TryLoseLife(); run.Round.TryLoseLife(); run.Round.TryLoseLife();
            Check(run.Round.Phase == RoundPhase.GameOver && run.Round.Score == 20, "Game over preserves the earned score.");
            Check(!run.TrySelectNext(), "Losing cannot skip to another level.");
            Check(run.TryRetry(), "Try Again must be available on game over.");
            Check(run.SelectedLevel == id && run.Round.Difficulty == difficulty && run.Round.Score == 0 &&
                  run.Round.Lives == 3 && run.Round.RemainingBricks == run.Layout.BrickCount,
                "Retry must keep both choices and restore exactly the selected board.");
            var health = new BrickHealth[run.Layout.BrickCount];
            for (int i = 0; i < health.Length; i++) health[i] = new BrickHealth(1);
            // Fireball column hits followed by queued normal-ball hits cannot double-score on any layout.
            for (int i = 0; i < 4; i++) if (health[i].TryDestroyWithHit()) run.Round.TryBreakBrick(10);
            for (int i = 0; i < 4; i++) if (health[i].TryDestroyWithHit()) run.Round.TryBreakBrick(10);
            Check(run.Round.Score == 40, "Duplicate projectile/ball hits must not corrupt the level score.");
            for (int i = 4; i < health.Length - 1; i++) if (health[i].TryDestroyWithHit()) run.Round.TryBreakBrick(10);
            Check(run.Round.Phase == RoundPhase.Playing && run.Round.RemainingBricks == 1, "Win must wait for the final brick.");
            if (health[health.Length - 1].TryDestroyWithHit()) run.Round.TryBreakBrick(10);
            Check(run.Round.Phase == RoundPhase.Won && run.Round.Score == run.Layout.MaximumScore,
                "Winning score and brick total must come from this level.");
            Check(!run.TryRetry() && !run.Round.TryBreakBrick(10) && !run.Round.TryLoseLife(), "Won level must remain stopped.");
            if (id != LevelId.Crown)
            {
                Check(run.HasNextLevel && run.TrySelectNext(), "Next Level is available after levels one and two.");
                Check((int)run.SelectedLevel == (int)id + 1 && run.ChoosingDifficulty && run.Round.Phase == RoundPhase.Menu,
                    "Next Level must ask for a new difficulty before play starts.");
                Difficulty changed = (Difficulty)(((int)difficulty + 1) % 3);
                Check(run.TryStart(changed) && run.Round.Difficulty == changed && run.Round.Lives == 3 && run.Round.Score == 0,
                    "Next level may use a different difficulty and starts a fresh score/life count.");
            }
            else Check(!run.HasNextLevel && !run.TrySelectNext() && run.SelectedLevel == LevelId.Crown,
                "Level three cannot create a nonexistent fourth level.");
            run.ReturnToMenu();
            Check(!run.ChoosingDifficulty && run.Round.Phase == RoundPhase.Menu && !run.TryStart(difficulty),
                "Returning to menu requires selecting a level again.");
        }

        var random = new Random(402);
        var repeated = new LevelRun();
        for (int attempt = 0; attempt < 300; attempt++)
        {
            var id = (LevelId)random.Next(1, 4);
            var difficulty = (Difficulty)random.Next(3);
            repeated.ReturnToMenu();
            repeated.SelectLevel(id);
            Check(repeated.TryStart(difficulty), "Repeated selection should not leave stale menu state.");
            int broken = 0, missed = 0;
            while (repeated.Round.Phase == RoundPhase.Playing)
            {
                if (random.Next(5) == 0) { repeated.Round.TryLoseLife(); missed++; }
                else { repeated.Round.TryBreakBrick(10); broken++; }
                Check(repeated.Round.RemainingBricks == repeated.Layout.BrickCount - broken && repeated.Round.Score == broken * 10 &&
                      repeated.Round.Lives == 3 - missed && repeated.Round.BallSpeed == ArcadeRound.SpeedFor(difficulty),
                    "Switching layouts and difficulties cannot leak totals or speed between attempts.");
            }
        }
        Console.WriteLine($"PASS: {checks} level assertions; all 9 level/difficulty combinations and 300 mixed attempts.");
        return 0;
    }
}
