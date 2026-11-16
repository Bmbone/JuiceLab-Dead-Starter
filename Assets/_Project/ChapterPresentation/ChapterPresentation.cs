using JuiceStudy.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JuiceStudy.Chapters
{
    /// <summary>Nonblocking chapter banners and result decoration, with existing actions and score ownership.</summary>
    [DefaultExecutionOrder(300), DisallowMultipleComponent]
    public sealed class ChapterPresentation : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Collider2D leftWall;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField, Range(0.8f, 3f)] private float introSeconds = 2f;
        private ChapterPresentationView view;
        private Transform overlay;
        private TMP_Text modeLabel;
        private TMP_Text detail;
        private Button retry;
        private Button returnToMenu;
        private Button next;
        private bool wired;
        private bool pendingIntro;
        private float introAge = 999;
        private bool resultVisible;
        private bool lastWon;
        private bool lastNext;
        private string lastContext;
        private bool detailOverridden;
        private float detailFontSize;
        private Vector2 detailSize;

        private void Awake()
        {
            if (session != null && worldCamera != null && leftWall != null && scoreText != null) return;
            Debug.LogError("Assign session, camera, left wall and score text to ChapterPresentation.", this);
            enabled = false;
        }

        private void OnEnable() => GameEvents.ScoreChanged += OnScore;

        private void OnScore(int score)
        {
            if (score != 0) return;
            pendingIntro = session != null && session.IsPlaying;
            introAge = 999;
            view?.HideIntro();
        }

        private void LateUpdate()
        {
            // MedievalInterfacePolish (default order) styles the generated menu before this adapter runs.
            if (!wired && !TryWire()) return;
            if (pendingIntro && session.IsPlaying)
            {
                pendingIntro = false;
                if (TryReadContext(out LevelDefinition layout, out string difficulty))
                {
                    view.SetIntro(layout, difficulty);
                    introAge = 0;
                }
            }
            if (!session.IsPlaying) introAge = 999;
            if (introAge < Mathf.Max(0.8f, introSeconds))
            {
                float duration = Mathf.Max(0.8f, introSeconds);
                float alpha = Mathf.SmoothStep(0, 1, introAge / 0.12f) *
                    (1 - Mathf.SmoothStep(0, 1, (introAge - (duration - 0.35f)) / 0.35f));
                // The banner is hidden if a narrow viewport leaves too little space beside the court.
                view.UpdateIntro(alpha, worldCamera.WorldToScreenPoint(new Vector3(leftWall.bounds.max.x, 0, 0)).x);
                introAge += Time.deltaTime; // Pausing preserves the remaining banner time.
            }
            else view.HideIntro();
            UpdateResult();
        }

        private bool TryWire()
        {
            Transform root = session.transform.Find("Level selection UI");
            if (root == null) return false;
            overlay = root.Find("Menu overlay");
            Transform panel = overlay.Find("Menu panel");
            modeLabel = root.Find("Level and difficulty").GetComponent<TMP_Text>();
            detail = panel.Find("Result score").GetComponent<TMP_Text>();
            retry = panel.Find("TRY AGAIN").GetComponent<Button>();
            returnToMenu = panel.Find("RETURN TO MAIN MENU").GetComponent<Button>();
            next = panel.Find("NEXT LEVEL").GetComponent<Button>();
            TMP_Text title = panel.Find("Title").GetComponent<TMP_Text>();
            view = new ChapterPresentationView(transform, (RectTransform)panel, detail.transform,
                scoreText.font, title.font);
            wired = true;
            return true;
        }

        private bool TryReadContext(out LevelDefinition layout, out string difficulty)
        {
            layout = null;
            difficulty = string.Empty;
            // Read only the documented presentation context "L<number> NAME | DIFFICULTY".
            // Score/lives/outcomes are not inferred from text, and no game state is written.
            string text = modeLabel.text;
            int space = string.IsNullOrEmpty(text) ? -1 : text.IndexOf(' ');
            if (space <= 1 || text[0] != 'L' || !int.TryParse(text.Substring(1, space - 1), out int number) ||
                number < 1 || number > 3) return false;
            layout = LevelCatalog.Get((LevelId)number);
            int divider = text.IndexOf('|');
            if (divider >= 0) difficulty = text.Substring(divider + 1).Trim();
            return true;
        }

        private void UpdateResult()
        {
            bool showing = overlay.gameObject.activeInHierarchy && returnToMenu.gameObject.activeSelf;
            if (!showing)
            {
                if (resultVisible)
                {
                    RestoreDetail();
                    view.HideResult();
                }
                resultVisible = false;
                return;
            }
            bool won = !retry.gameObject.activeSelf;
            bool hasNext = next.gameObject.activeSelf;
            if (!resultVisible || won != lastWon || hasNext != lastNext || lastContext != modeLabel.text)
            {
                if (!detailOverridden)
                {
                    detailSize = detail.rectTransform.sizeDelta;
                    detailFontSize = detail.fontSize;
                    detailOverridden = true;
                }
                TryReadContext(out LevelDefinition layout, out _);
                string cue = hasNext && layout != null && layout.Number < 3
                    ? "NEXT CHAPTER  /  " + LevelCatalog.Get((LevelId)(layout.Number + 1)).Name.ToUpperInvariant()
                    : won ? "THE KEEP IS YOURS. CHOOSE YOUR NEXT CHALLENGE."
                    : "REGROUP. YOUR NEXT ATTEMPT STARTS FRESH.";
                view.ShowResult(won, cue);
                resultVisible = true;
                lastWon = won;
                lastNext = hasNext;
                lastContext = modeLabel.text;
            }
            // The original TMP_Text still displays the exact string written by LevelMenuView.
            detail.fontSize = 60;
            detail.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 74);
            view.TickResult(Time.unscaledDeltaTime);
        }

        private void RestoreDetail()
        {
            if (!detailOverridden || detail == null) return;
            detail.rectTransform.sizeDelta = detailSize;
            // A menu transition may already have selected its own 28-point style; keep that new style.
            if (Mathf.Approximately(detail.fontSize, 60)) detail.fontSize = detailFontSize;
            detailOverridden = false;
        }

        private void OnDisable()
        {
            GameEvents.ScoreChanged -= OnScore;
            pendingIntro = false;
            introAge = 999;
            resultVisible = false;
            RestoreDetail();
            view?.HideIntro();
            view?.HideResult();
        }

        private void OnDestroy()
        {
            GameEvents.ScoreChanged -= OnScore;
            RestoreDetail();
            view?.Dispose();
        }
    }
}
