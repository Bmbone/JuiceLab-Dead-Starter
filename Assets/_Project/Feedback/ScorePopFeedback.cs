using TMPro;
using UnityEngine;

namespace JuiceStudy
{
    /// <summary>HUD still owns the text value; this component owns only its visual scale.</summary>
    public sealed class ScorePopFeedback : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField, Range(1.01f, 1.4f)] private float peakScale = 1.16f;
        [SerializeField, Range(0.05f, 0.5f)] private float duration = 0.18f;
        private Vector3 restScale;
        private int previousScore;
        private float elapsed;
        private bool popping;

        private void Awake()
        {
            if (scoreText == null)
            {
                Debug.LogError("Assign ScoreText to ScorePopFeedback.", this);
                enabled = false;
                return;
            }
            restScale = scoreText.transform.localScale;
        }

        private void OnEnable()
        {
            previousScore = GameManager.Instance != null ? GameManager.Instance.Score : 0;
            GameEvents.ScoreChanged += OnScore;
        }

        private void OnDisable()
        {
            GameEvents.ScoreChanged -= OnScore;
            popping = false;
            if (scoreText != null && restScale != Vector3.zero) scoreText.transform.localScale = restScale;
        }

        private void OnScore(int score)
        {
            popping = score > previousScore;
            previousScore = score;
            elapsed = 0;
            scoreText.transform.localScale = restScale * (popping ? peakScale : 1);
        }

        private void Update()
        {
            if (!popping) return;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
            float emphasis = (1 - t) * (1 - t);
            scoreText.transform.localScale = restScale * (1 + (peakScale - 1) * emphasis);
            if (t >= 1) popping = false;
        }
    }
}
