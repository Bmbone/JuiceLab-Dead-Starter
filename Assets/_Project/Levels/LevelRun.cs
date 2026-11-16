using JuiceStudy.Arcade;

namespace JuiceStudy.Levels
{
    /// <summary>Selection, retry and next-level rules shared by the Unity scene and tests.</summary>
    public sealed class LevelRun
    {
        public ArcadeRound Round { get; } = new ArcadeRound();
        public LevelId SelectedLevel { get; private set; } = LevelId.Courtyard;
        public LevelDefinition Layout => LevelCatalog.Get(SelectedLevel);
        public bool ChoosingDifficulty { get; private set; }
        public bool HasNextLevel => Round.Phase == RoundPhase.Won && SelectedLevel != LevelId.Crown;

        public bool SelectLevel(LevelId level)
        {
            if (Round.Phase == RoundPhase.Playing) return false;
            LevelCatalog.Get(level);
            SelectedLevel = level;
            Round.ReturnToMenu();
            ChoosingDifficulty = true;
            return true;
        }

        public bool TryStart(Difficulty difficulty)
        {
            if (!ChoosingDifficulty || Round.Phase == RoundPhase.Playing) return false;
            Round.Start(difficulty, Layout.BrickCount);
            ChoosingDifficulty = false;
            return true;
        }

        public bool TryRetry()
        {
            if (Round.Phase != RoundPhase.GameOver) return false;
            Round.Start(Round.Difficulty, Layout.BrickCount);
            ChoosingDifficulty = false;
            return true;
        }

        public bool TrySelectNext()
        {
            if (!HasNextLevel) return false;
            return SelectLevel((LevelId)((int)SelectedLevel + 1));
        }

        public void ReturnToMenu()
        {
            Round.ReturnToMenu();
            ChoosingDifficulty = false;
        }
    }
}
