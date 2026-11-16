using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JuiceStudy.Arcade
{
    /// <summary>One persistent UI hierarchy; switching screens never adds more buttons/listeners.</summary>
    public sealed class ArcadeMenuView
    {
        private static readonly Color Gold = new Color(0.906f, 0.773f, 0.494f);
        private static readonly Color Ivory = new Color(0.918f, 0.859f, 0.761f);
        private readonly TMP_FontAsset font;
        private readonly GameObject overlay;
        private readonly TMP_Text eyebrow;
        private readonly TMP_Text title;
        private readonly TMP_Text subtitle;
        private readonly TMP_Text detail;
        private readonly TMP_Text modeLabel;
        private readonly Button easy;
        private readonly Button medium;
        private readonly Button hard;
        private readonly Button retry;
        private readonly Button returnToMenu;

        public ArcadeMenuView(ArcadeSession owner, TMP_FontAsset font)
        {
            this.font = font;
            var root = new GameObject("Arcade menu UI", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(owner.transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            modeLabel = Text("Difficulty", root.transform, 0, 0, 420, 64, 28, Gold);
            var modeRect = modeLabel.rectTransform;
            modeRect.anchorMin = modeRect.anchorMax = new Vector2(0.5f, 1);
            modeRect.pivot = new Vector2(0.5f, 1);
            modeRect.anchoredPosition = new Vector2(0, -40);

            var backdrop = Rect("Menu overlay", root.transform, 0, 0, 0, 0);
            backdrop.anchorMin = Vector2.zero;
            backdrop.anchorMax = Vector2.one;
            var shade = backdrop.gameObject.AddComponent<Image>();
            shade.color = new Color(0.035f, 0.04f, 0.065f, 0.98f);
            shade.raycastTarget = true;
            overlay = backdrop.gameObject;

            var panel = Rect("Menu panel", backdrop, 0, 0, 800, 860);
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(0.07f, 0.075f, 0.10f);
            panelImage.raycastTarget = false;
            eyebrow = Text("Screen label", panel, 0, 330, 700, 40, 22, Gold);
            eyebrow.characterSpacing = 5;
            title = Text("Title", panel, 0, 250, 720, 110, 76, Gold);
            var rule = Rect("Gold divider", panel, 0, 172, 560, 2).gameObject.AddComponent<Image>();
            rule.color = Gold;
            rule.raycastTarget = false;
            subtitle = Text("Subtitle", panel, 0, 116, 700, 70, 26, Ivory);
            detail = Text("Result score", panel, 0, 56, 700, 45, 28, Gold);

            easy = MakeButton("EASY  -  Slower ball", panel, -36, () => owner.StartRound(Difficulty.Easy));
            medium = MakeButton("MEDIUM  -  Balanced", panel, -140, () => owner.StartRound(Difficulty.Medium));
            hard = MakeButton("HARD  -  Fastest ball", panel, -244, () => owner.StartRound(Difficulty.Hard));
            retry = MakeButton("TRY AGAIN", panel, -60, owner.TryAgain);
            returnToMenu = MakeButton("RETURN TO MAIN MENU", panel, -164, owner.ReturnToMainMenu);
            var footer = Text("Controls", panel, 0, -365, 740, 60, 22, Ivory);
            footer.text = "Move: mouse, A/D, or arrow keys\nMenus: mouse, or arrows and Enter";
        }

        public void ShowMainMenu()
        {
            SetButtons(true, false, false);
            overlay.SetActive(true);
            modeLabel.gameObject.SetActive(false);
            eyebrow.text = "MAIN MENU";
            title.text = "BRICKKEEP";
            subtitle.text = "Choose your challenge. Break every brick.";
            detail.text = "3 LIVES  |  3 DIFFICULTIES";
            Select(easy);
        }

        public void ShowPlaying(Difficulty difficulty)
        {
            overlay.SetActive(false);
            modeLabel.text = difficulty.ToString().ToUpperInvariant() + " MODE";
            modeLabel.gameObject.SetActive(true);
            Select(null);
        }

        public void ShowResult(ArcadeRound round)
        {
            bool won = round.Phase == RoundPhase.Won;
            SetButtons(false, !won, true);
            overlay.SetActive(true);
            modeLabel.gameObject.SetActive(false);
            eyebrow.text = round.Difficulty.ToString().ToUpperInvariant() + " MODE";
            title.text = won ? "VICTORY!" : "GAME OVER";
            subtitle.text = won ? "All bricks cleared. The keep is yours!" : "Your lives ran out. Ready for another attempt?";
            detail.text = $"FINAL SCORE  {round.Score}";
            returnToMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, won ? -100 : -164);
            Select(won ? returnToMenu : retry);
        }

        private void SetButtons(bool difficulties, bool tryAgain, bool showReturn)
        {
            easy.gameObject.SetActive(difficulties);
            medium.gameObject.SetActive(difficulties);
            hard.gameObject.SetActive(difficulties);
            retry.gameObject.SetActive(tryAgain);
            returnToMenu.gameObject.SetActive(showReturn);
        }

        private static void Select(Button button)
        {
            if (EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(null);
            if (button != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        private Button MakeButton(string label, Transform parent, float y, UnityAction onClick)
        {
            var rect = Rect(label, parent, 0, y, 560, 80);
            var image = rect.gameObject.AddComponent<Image>();
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = new Color(0.16f, 0.17f, 0.21f);
            colors.highlightedColor = new Color(0.32f, 0.27f, 0.17f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.45f, 0.33f, 0.12f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.onClick.AddListener(onClick);
            Text("Label", rect, 0, 0, 520, 66, 30, Ivory).text = label;
            return button;
        }

        private TMP_Text Text(string name, Transform parent, float x, float y, float width, float height, float size, Color color)
        {
            var rect = Rect(name, parent, x, y, width, height);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
    }
}
