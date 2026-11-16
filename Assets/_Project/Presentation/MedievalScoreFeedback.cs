using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JuiceStudy.Presentation
{
    /// <summary>Presentation only. LevelSession remains the sole writer of the actual score.</summary>
    [DisallowMultipleComponent]
    public sealed class MedievalScoreFeedback : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField, Range(1f, 1.3f)] private float peakScale = 1.12f;
        [SerializeField, Range(0f, 16f)] private float upwardPush = 8f;
        [SerializeField, Range(0.1f, 0.6f)] private float pulseDuration = 0.28f;
        [SerializeField, Range(0.2f, 1f)] private float gainDuration = 0.55f;
        [SerializeField, Range(0f, 32f)] private float gainRise = 14f;
        [SerializeField] private Color rewardColor = new Color(1f, 0.95f, 0.76f, 1f);

        private RectTransform scoreRect;
        private RectTransform plaque;
        private TMP_Text gainText;
        private Vector3 restScale;
        private Vector2 restPosition;
        private Color restColor;
        private Vector2 gainPosition;
        private int previousScore;
        private int pendingGain;
        private float pulseElapsed;
        private float gainElapsed;
        private bool pulsing;

        private void Awake()
        {
            if (scoreText == null)
            {
                Debug.LogError("Assign ScoreText to MedievalScoreFeedback.", this);
                enabled = false;
                return;
            }

            scoreRect = scoreText.rectTransform;
            restScale = scoreRect.localScale;
            restPosition = scoreRect.anchoredPosition;
            restColor = scoreText.color;
            BuildPlaque();
        }

        private void OnEnable()
        {
            if (scoreRect == null) return;
            // Re-enabling mid-run must not mistake existing points for a new reward.
            string displayed = scoreText.text;
            int.TryParse(displayed.Substring(displayed.LastIndexOf(' ') + 1), out previousScore);
            ClearReaction();
            plaque.gameObject.SetActive(true);
            GameEvents.ScoreChanged += OnScore;
        }

        private void OnDisable()
        {
            GameEvents.ScoreChanged -= OnScore;
            ClearReaction();
            if (plaque != null) plaque.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // The plaque belongs to the HUD, so explicitly clean up this sibling hierarchy.
            if (plaque != null) Destroy(plaque.gameObject);
        }

        private void OnScore(int score)
        {
            int increase = score - previousScore;
            previousScore = score;
            if (increase < 0 || score == 0)
            {
                ClearReaction();
                return;
            }
            if (increase == 0) return;

            pendingGain = gainText.gameObject.activeSelf ? pendingGain + increase : increase;
            gainText.SetText("+{0}", pendingGain);
            gainText.color = rewardColor;
            gainText.rectTransform.anchoredPosition = gainPosition;
            gainText.gameObject.SetActive(true);
            pulseElapsed = gainElapsed = 0f;
            pulsing = true;
            ApplyPulse(1f);
        }

        private void LateUpdate()
        {
            float delta = Time.unscaledDeltaTime;
            if (pulsing)
            {
                pulseElapsed += delta;
                float t = Mathf.Clamp01(pulseElapsed / Mathf.Max(0.01f, pulseDuration));
                ApplyPulse((1f - t) * (1f - t));
                if (t >= 1f) pulsing = false;
            }
            if (!gainText.gameObject.activeSelf) return;

            gainElapsed += delta;
            float progress = Mathf.Clamp01(gainElapsed / Mathf.Max(0.01f, gainDuration));
            gainText.rectTransform.anchoredPosition = gainPosition + Vector2.up * (gainRise * progress);
            Color color = rewardColor;
            color.a *= 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.35f, 1f, progress));
            gainText.color = color;
            if (progress >= 1f)
            {
                pendingGain = 0;
                gainText.gameObject.SetActive(false);
            }
        }

        private void ApplyPulse(float emphasis)
        {
            // Always start from the captured rest pose; rapid hits cannot accumulate transforms.
            scoreRect.localScale = restScale * (1f + (peakScale - 1f) * emphasis);
            scoreRect.anchoredPosition = restPosition + Vector2.up * (upwardPush * emphasis);
            scoreText.color = Color.Lerp(restColor, rewardColor, emphasis);
        }

        private void ClearReaction()
        {
            pulsing = false;
            pendingGain = 0;
            pulseElapsed = gainElapsed = 0f;
            if (scoreRect != null) ApplyPulse(0f);
            if (gainText != null)
            {
                gainText.gameObject.SetActive(false);
                gainText.rectTransform.anchoredPosition = gainPosition;
            }
        }

        private void BuildPlaque()
        {
            // Construct one reusable decoration. No hierarchy objects are created by score events.
            plaque = Rect("Score plaque - UI01", scoreRect.parent, 48, -30, 470, 104);
            plaque.SetSiblingIndex(scoreRect.GetSiblingIndex());
            Paint(plaque, new Color(0.035f, 0.045f, 0.065f, 0.96f));
            Color bronze = new Color(0.40f, 0.31f, 0.16f, 1f);
            Color gold = new Color(0.91f, 0.77f, 0.49f, 1f);
            Paint(Rect("Top border", plaque, 0, 0, 470, 1), bronze);
            Paint(Rect("Bottom border", plaque, 0, -103, 470, 1), bronze);
            Paint(Rect("Left border", plaque, 0, 0, 1, 104), bronze);
            Paint(Rect("Right border", plaque, 469, 0, 1, 104), bronze);
            Paint(Rect("Gold accent", plaque, 0, 0, 4, 104), gold);
            Paint(Rect("Inset rule", plaque, 24, -84, 422, 1), new Color(0.4f, 0.31f, 0.16f, 0.6f));
            var gem = Rect("Gold diamond", plaque, 433, -73, 7, 7);
            gem.pivot = new Vector2(0.5f, 0.5f);
            gem.localRotation = Quaternion.Euler(0, 0, 45);
            Paint(gem, gold);

            var gainRect = Rect("Score gain - reused", plaque, 370, -22, 76, 42);
            gainText = gainRect.gameObject.AddComponent<TextMeshProUGUI>();
            gainText.font = scoreText.font;
            gainText.fontSize = 28;
            gainText.fontStyle = FontStyles.Bold;
            gainText.alignment = TextAlignmentOptions.Right;
            gainText.raycastTarget = false;
            gainPosition = gainRect.anchoredPosition;
            gainText.gameObject.SetActive(false);
        }

        private static void Paint(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.gameObject.layer = parent.gameObject.layer;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
    }
}
