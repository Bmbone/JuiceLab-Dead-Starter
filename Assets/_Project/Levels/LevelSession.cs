using JuiceStudy.Abilities;
using JuiceStudy.Arcade;
using TMPro;
using UnityEngine;

namespace JuiceStudy.Levels
{
    /// <summary>Owns Levels.unity; previous checkpoints and their adapters remain untouched.</summary>
    public sealed class LevelSession : MonoBehaviour
    {
        [SerializeField] private LevelBall ball;
        [SerializeField] private AbilityPaddle paddle;
        [SerializeField] private LevelBoard board;
        [SerializeField] private GameObject hud;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private GameObject feedback;
        [SerializeField] private LevelAbilityController abilities;
        private readonly LevelRun run = new LevelRun();
        private LevelMenuView menu;
        private Vector3 paddleStart;
        private int lastLossFrame = -1;
        public bool IsPlaying => run.Round.Phase == RoundPhase.Playing;

        private void Awake()
        {
            paddleStart = paddle.transform.position;
            paddle.enabled = false;
        }

        private void Start()
        {
            menu = new LevelMenuView(this, scoreText.font);
            ReturnToMainMenu();
        }

        public void StartRound(Difficulty difficulty)
        {
            if (!run.TryStart(difficulty)) return;
            BuildAndLaunch();
        }

        public void ChooseLevel(LevelId level)
        {
            if (run.SelectLevel(level)) menu.ShowDifficulties(run.Layout);
        }

        public void ChooseNextLevel()
        {
            if (run.TrySelectNext()) menu.ShowDifficulties(run.Layout);
        }

        private void BuildAndLaunch()
        {
            ball.Hold();
            ResetFeedback();
            int count = board.Rebuild(this, run.Layout);
            Debug.Assert(count == run.Layout.BrickCount, "Built board must match the selected level.");
            lastLossFrame = -1;
            paddle.transform.position = paddleStart;
            Physics2D.SyncTransforms();
            scoreText.text = "SCORE 0";
            livesText.text = $"LIVES {run.Round.Lives}";
            GameEvents.RaiseScoreChanged(0);
            GameEvents.RaiseLivesChanged(run.Round.Lives);
            hud.SetActive(true);
            menu.ShowPlaying(run.Layout, run.Round.Difficulty);
            abilities.BeginRound();
            paddle.enabled = true;
            ball.Launch(run.Round.BallSpeed);
        }

        public void TryAgain()
        {
            if (run.TryRetry()) BuildAndLaunch();
        }

        public void ReturnToMainMenu()
        {
            ball.Hold();
            paddle.enabled = false;
            abilities.EndRound();
            board.Clear();
            run.ReturnToMenu();
            ResetFeedback();
            GameEvents.RaiseScoreChanged(0);
            hud.SetActive(false);
            menu.ShowMainMenu();
        }

        public void LoseLife()
        {
            // A second trigger notification in one physics frame must not consume another life.
            if (lastLossFrame == Time.frameCount || !run.Round.TryLoseLife()) return;
            lastLossFrame = Time.frameCount;
            livesText.text = $"LIVES {run.Round.Lives}";
            GameEvents.RaiseLivesChanged(run.Round.Lives);
            GameEvents.RaiseBallLost();
            abilities.OnLifeLost();
            if (IsPlaying) ball.Launch(run.Round.BallSpeed);
            else FinishRound();
        }

        public void HitPaddle() => run.Round.HitPaddle();

        public void BreakBrick(int points, Vector2 point, Vector2 normal, Color color, Vector2 center, Vector2 size)
        {
            if (!run.Round.TryBreakBrick(points)) return;
            scoreText.text = $"SCORE {run.Round.Score}";
            GameEvents.RaiseScoreChanged(run.Round.Score);
            GameEvents.RaiseBrickDestroyed(new ImpactInfo(point, normal, 1, color, center, size, run.Round.Combo));
            if (!IsPlaying) FinishRound();
        }

        private void FinishRound()
        {
            ball.Hold();
            paddle.enabled = false;
            abilities.EndRound();
            hud.SetActive(false);
            menu.ShowResult(run);
        }

        private void ResetFeedback()
        {
            // Existing feedback clears its audio, particles, scale and trail in OnDisable.
            feedback.SetActive(false);
            feedback.SetActive(true);
        }

        private void OnDisable()
        {
            if (ball != null) ball.Hold();
            if (paddle != null) paddle.enabled = false;
            if (abilities != null) abilities.EndRound();
        }
    }
}
