using JuiceStudy.Abilities;
using JuiceStudy.Arcade;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JuiceStudy.Levels
{
    /// <summary>One persistent UI hierarchy; switching screens never adds more buttons/listeners.</summary>
    public sealed class LevelMenuView
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
        private readonly Button[] levelButtons = new Button[3];
        private readonly Button easy;
        private readonly Button medium;
        private readonly Button hard;
        private readonly Button retry;
        private readonly Button returnToMenu;
        private readonly Button back;
        private readonly Button next;

        public LevelMenuView(LevelSession owner, TMP_FontAsset font)
        {
            this.font = font;
            var root = new GameObject("Level selection UI", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(owner.transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            modeLabel = Text("Level and difficulty", root.transform, 0, 0, 560, 64, 26, Gold);
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

            var panel = Rect("Menu panel", backdrop, 0, 0, 840, 960);
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(0.07f, 0.075f, 0.10f);
            panelImage.raycastTarget = false;
            eyebrow = Text("Screen label", panel, 0, 380, 700, 40, 22, Gold);
            eyebrow.characterSpacing = 5;
            title = Text("Title", panel, 0, 300, 760, 110, 76, Gold);
            var rule = Rect("Gold divider", panel, 0, 220, 560, 2).gameObject.AddComponent<Image>();
            rule.color = Gold;
            rule.raycastTarget = false;
            subtitle = Text("Subtitle", panel, 0, 164, 740, 70, 26, Ivory);
            detail = Text("Result score", panel, 0, 108, 760, 45, 26, Gold);

            for (int i = 0; i < levelButtons.Length; i++)
            {
                LevelId id = (LevelId)(i + 1);
                LevelDefinition layout = LevelCatalog.Get(id);
                levelButtons[i] = MakeButton($"LEVEL {layout.Number}  -  {layout.Name.ToUpperInvariant()}",
                    panel, -i * 106, () => owner.ChooseLevel(id));
            }
            easy = MakeButton("EASY  -  Slower ball", panel, 0, () => owner.StartRound(Difficulty.Easy));
            medium = MakeButton("MEDIUM  -  Balanced", panel, -106, () => owner.StartRound(Difficulty.Medium));
            hard = MakeButton("HARD  -  Fastest ball", panel, -212, () => owner.StartRound(Difficulty.Hard));
            retry = MakeButton("TRY AGAIN", panel, -36, owner.TryAgain);
            returnToMenu = MakeButton("RETURN TO MAIN MENU", panel, -142, owner.ReturnToMainMenu);
            back = MakeButton("BACK TO LEVELS", panel, -306, owner.ReturnToMainMenu, 56);
            next = MakeButton("NEXT LEVEL", panel, -36, owner.ChooseNextLevel);
            var footer = Text("Controls", panel, 0, -402, 800, 90, 22, Ivory);
            footer.text = "Move: mouse, A/D, or arrow keys\nSpells: 1 / Space = Fireball, 2 = Wide Paddle, 3 = Shield\nMenus: mouse, or arrows and Enter";
        }

        public void ShowMainMenu()
        {
            SetButtons(true, false, false, false);
            overlay.SetActive(true);
            modeLabel.gameObject.SetActive(false);
            eyebrow.text = "MAIN MENU";
            title.text = "BRICKKEEP";
            subtitle.text = "Choose a level, then choose your difficulty.";
            detail.text = "3 LEVELS  |  3 DIFFICULTIES EACH";
            Select(levelButtons[0]);
        }

        public void ShowDifficulties(LevelDefinition layout)
        {
            SetButtons(false, true, false, false);
            overlay.SetActive(true);
            modeLabel.gameObject.SetActive(false);
            eyebrow.text = $"LEVEL {layout.Number} - SELECT DIFFICULTY";
            title.text = layout.Name.ToUpperInvariant();
            subtitle.text = layout.Description;
            detail.text = $"{layout.BrickCount} BRICKS  |  3 LIVES  |  3 SPELLS";
            Select(easy);
        }

        public void ShowPlaying(LevelDefinition layout, Difficulty difficulty)
        {
            overlay.SetActive(false);
            modeLabel.text = $"L{layout.Number} {layout.Name.ToUpperInvariant()} | {difficulty.ToString().ToUpperInvariant()}";
            modeLabel.gameObject.SetActive(true);
            Select(null);
        }

        public void ShowResult(LevelRun run)
        {
            ArcadeRound round = run.Round;
            bool won = round.Phase == RoundPhase.Won;
            SetButtons(false, false, !won, true, run.HasNextLevel);
            overlay.SetActive(true);
            modeLabel.gameObject.SetActive(false);
            eyebrow.text = $"LEVEL {run.Layout.Number} - {run.Layout.Name.ToUpperInvariant()} | {round.Difficulty.ToString().ToUpperInvariant()}";
            title.text = won ? "VICTORY!" : "GAME OVER";
            subtitle.text = won ? $"{run.Layout.Name} cleared. The keep is yours!" : "Your lives ran out. Ready for another attempt?";
            detail.text = $"LEVEL SCORE  {round.Score}";
            returnToMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, won && !run.HasNextLevel ? -92 : -142);
            Select(won ? (run.HasNextLevel ? next : returnToMenu) : retry);
        }

        private void SetButtons(bool pickLevel, bool difficulties, bool tryAgain, bool showReturn, bool showNext = false)
        {
            foreach (Button button in levelButtons) button.gameObject.SetActive(pickLevel);
            easy.gameObject.SetActive(difficulties);
            medium.gameObject.SetActive(difficulties);
            hard.gameObject.SetActive(difficulties);
            retry.gameObject.SetActive(tryAgain);
            returnToMenu.gameObject.SetActive(showReturn);
            back.gameObject.SetActive(difficulties);
            next.gameObject.SetActive(showNext);
        }

        private static void Select(Button button)
        {
            if (EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(null);
            if (button != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        private Button MakeButton(string label, Transform parent, float y, UnityAction onClick, float height = 80)
        {
            var rect = Rect(label, parent, 0, y, 600, height);
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
            Text("Label", rect, 0, 0, 560, height - 10, 30, Ivory).text = label;
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
