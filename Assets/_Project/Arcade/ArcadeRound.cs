using System;

namespace JuiceStudy.Arcade
{
    public enum Difficulty { Easy, Medium, Hard }
    public enum RoundPhase { Menu, Playing, GameOver, Won }

    /// <summary>Round rules without Unity dependencies, shared by the game and executable tests.</summary>
    public sealed class ArcadeRound
    {
        private readonly int startingLives;
        public RoundPhase Phase { get; private set; } = RoundPhase.Menu;
        public Difficulty Difficulty { get; private set; } = Difficulty.Medium;
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int RemainingBricks { get; private set; }
        public int Combo { get; private set; }
        public float BallSpeed => SpeedFor(Difficulty);

        public ArcadeRound(int startingLives = 3)
        {
            if (startingLives < 1) throw new ArgumentOutOfRangeException(nameof(startingLives));
            this.startingLives = startingLives;
        }

        public static float SpeedFor(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy: return 9f;
                case Difficulty.Medium: return 12f;
                case Difficulty.Hard: return 15f;
                default: throw new ArgumentOutOfRangeException(nameof(difficulty));
            }
        }

        public void Start(Difficulty difficulty, int brickCount)
        {
            SpeedFor(difficulty);
            if (brickCount < 1) throw new ArgumentOutOfRangeException(nameof(brickCount));
            Difficulty = difficulty;
            Score = Combo = 0;
            Lives = startingLives;
            RemainingBricks = brickCount;
            Phase = RoundPhase.Playing;
        }

        public bool TryBreakBrick(int points)
        {
            if (Phase != RoundPhase.Playing) return false;
            if (points < 1) throw new ArgumentOutOfRangeException(nameof(points));
            Score += points;
            Combo++;
            RemainingBricks--;
            if (RemainingBricks == 0) Phase = RoundPhase.Won;
            return true;
        }

        public bool TryLoseLife()
        {
            if (Phase != RoundPhase.Playing) return false;
            Lives--;
            Combo = 0;
            if (Lives == 0) Phase = RoundPhase.GameOver;
            return true;
        }

        public void HitPaddle()
        {
            if (Phase == RoundPhase.Playing) Combo = 0;
        }

        public void ReturnToMenu()
        {
            Phase = RoundPhase.Menu;
            Score = Lives = Combo = RemainingBricks = 0;
        }
    }
}
