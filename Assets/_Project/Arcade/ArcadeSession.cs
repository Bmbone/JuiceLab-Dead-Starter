using TMPro;
using UnityEngine;

namespace JuiceStudy.Arcade
{
    /// <summary>Coordinates only Arcade.unity; the course Before/After rules stay untouched.</summary>
    public sealed class ArcadeSession : MonoBehaviour
    {
        [SerializeField] private ArcadeBall ball;
        [SerializeField] private Paddle paddle;
        [SerializeField] private ArcadeBoard board;
        [SerializeField] private GameObject hud;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private GameObject feedback;
        private readonly ArcadeRound round = new ArcadeRound();
        private ArcadeMenuView menu;
        private Vector3 paddleStart;
        private int lastLossFrame = -1;
        public bool IsPlaying => round.Phase == RoundPhase.Playing;

        private void Awake()
        {
            paddleStart = paddle.transform.position;
            paddle.enabled = false;
        }

        private void Start()
        {
            menu = new ArcadeMenuView(this, scoreText.font);
            ReturnToMainMenu();
        }

        public void StartRound(Difficulty difficulty)
        {
            if (IsPlaying) return;
            ArcadeRound.SpeedFor(difficulty);
            ball.Hold();
            ResetFeedback();
            int count = board.Rebuild(this);
            round.Start(difficulty, count);
            lastLossFrame = -1;
            paddle.transform.position = paddleStart;
            Physics2D.SyncTransforms();
            scoreText.text = "SCORE 0";
            livesText.text = $"LIVES {round.Lives}";
            GameEvents.RaiseScoreChanged(0);
            GameEvents.RaiseLivesChanged(round.Lives);
            hud.SetActive(true);
            menu.ShowPlaying(difficulty);
            paddle.enabled = true;
            ball.Launch(round.BallSpeed);
        }

        public void TryAgain()
        {
            if (round.Phase == RoundPhase.GameOver) StartRound(round.Difficulty);
        }

        public void ReturnToMainMenu()
        {
            ball.Hold();
            paddle.enabled = false;
            board.Clear();
            round.ReturnToMenu();
            ResetFeedback();
            GameEvents.RaiseScoreChanged(0);
            hud.SetActive(false);
            menu.ShowMainMenu();
        }

        public void LoseLife()
        {
            // A second trigger notification in one physics frame must not consume another life.
            if (lastLossFrame == Time.frameCount || !round.TryLoseLife()) return;
            lastLossFrame = Time.frameCount;
            livesText.text = $"LIVES {round.Lives}";
            GameEvents.RaiseLivesChanged(round.Lives);
            GameEvents.RaiseBallLost();
            if (IsPlaying) ball.Launch(round.BallSpeed);
            else FinishRound();
        }

        public void HitPaddle() => round.HitPaddle();

        public void BreakBrick(int points, Vector2 point, Vector2 normal, Color color, Vector2 center, Vector2 size)
        {
            if (!round.TryBreakBrick(points)) return;
            scoreText.text = $"SCORE {round.Score}";
            GameEvents.RaiseScoreChanged(round.Score);
            GameEvents.RaiseBrickDestroyed(new ImpactInfo(point, normal, 1, color, center, size, round.Combo));
            if (!IsPlaying) FinishRound();
        }

        private void FinishRound()
        {
            ball.Hold();
            paddle.enabled = false;
            hud.SetActive(false);
            menu.ShowResult(round);
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
        }
    }
}
