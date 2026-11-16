using JuiceStudy.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JuiceStudy.Presentation
{
    /// <summary>
    /// Restyles LevelMenuView's existing hierarchy and actions once it has been constructed.
    /// No game rules, button listeners, score ownership or collision objects are replaced.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MedievalInterfacePolish : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private RectTransform hudRoot;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private Font headingTypeface;
        [SerializeField] private SpriteRenderer background;

        private static readonly Color Gold = new Color(0.95f, 0.81f, 0.54f);
        private static readonly Color Ivory = new Color(0.98f, 0.95f, 0.85f);
        private static readonly Color Muted = new Color(0.72f, 0.80f, 0.77f);
        private static readonly Color Jade = new Color(0.47f, 0.85f, 0.72f);
        private static readonly Color Coral = new Color(1f, 0.57f, 0.48f);
        private TMP_FontAsset displayFont;
        private TMP_FontAsset bodyFont;
        private Transform menuRoot;
        private RectTransform panel;
        private GameObject overlay;
        private TMP_Text eyebrow;
        private TMP_Text title;
        private TMP_Text subtitle;
        private TMP_Text detail;
        private TMP_Text oldMode;
        private TMP_Text levelTitle;
        private TMP_Text difficultyTitle;
        private GameObject levelReadout;
        private Button[] levels;
        private Button[] difficulties;
        private Button retry;
        private Button returnToMenu;
        private Button back;
        private Button next;
        private string lastScreen;
        private string lastMode;
        private int lastWidth;
        private int lastHeight;
        private bool wired;

        private void Awake()
        {
            if (session == null || hudRoot == null || scoreText == null || livesText == null)
            {
                Debug.LogError("Assign the session and HUD references to MedievalInterfacePolish.", this);
                enabled = false;
                return;
            }
            bodyFont = scoreText.font;
            if (headingTypeface != null) displayFont = TMP_FontAsset.CreateFontAsset(headingTypeface);
            if (displayFont == null) displayFont = bodyFont;
        }

        private void LateUpdate()
        {
            // LevelSession creates the menu in Start; waiting here avoids dependence on Start order.
            if (!wired)
            {
                Transform candidate = session.transform.Find("Level selection UI");
                if (candidate == null) return;
                Wire(candidate);
                wired = true;
            }

            bool visible = oldMode.gameObject.activeSelf;
            levelReadout.SetActive(visible);
            if (visible && oldMode.text != lastMode) RefreshMode();
            string screen = overlay.activeSelf ? eyebrow.text + "/" + title.text : "PLAYING";
            if (screen != lastScreen)
            {
                lastScreen = screen;
                RefreshScreen();
            }
            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                lastWidth = Screen.width;
                lastHeight = Screen.height;
                FitBackground();
            }
        }

        private void Wire(Transform root)
        {
            menuRoot = root;
            overlay = root.Find("Menu overlay").gameObject;
            panel = (RectTransform)overlay.transform.Find("Menu panel");
            eyebrow = panel.Find("Screen label").GetComponent<TMP_Text>();
            title = panel.Find("Title").GetComponent<TMP_Text>();
            subtitle = panel.Find("Subtitle").GetComponent<TMP_Text>();
            detail = panel.Find("Result score").GetComponent<TMP_Text>();
            oldMode = root.Find("Level and difficulty").GetComponent<TMP_Text>();
            oldMode.enabled = false;
            levels = new Button[3];
            difficulties = new Button[3];
            string[] modes = { "EASY  -  Slower ball", "MEDIUM  -  Balanced", "HARD  -  Fastest ball" };
            for (int i = 0; i < 3; i++)
            {
                LevelDefinition layout = LevelCatalog.Get((LevelId)(i + 1));
                levels[i] = panel.Find($"LEVEL {layout.Number}  -  {layout.Name.ToUpperInvariant()}").GetComponent<Button>();
                difficulties[i] = panel.Find(modes[i]).GetComponent<Button>();
                BuildLevelCard(levels[i], layout, (i - 1) * 456);
                BuildDifficultyCard(difficulties[i], i, (i - 1) * 456);
            }
            retry = panel.Find("TRY AGAIN").GetComponent<Button>();
            returnToMenu = panel.Find("RETURN TO MAIN MENU").GetComponent<Button>();
            back = panel.Find("BACK TO LEVELS").GetComponent<Button>();
            next = panel.Find("NEXT LEVEL").GetComponent<Button>();
            StyleAction(retry, "TRY AGAIN");
            StyleAction(returnToMenu, "RETURN TO MAIN MENU");
            StyleAction(next, "NEXT LEVEL");
            StyleAction(back, "BACK TO LEVELS", 380, 68, 28);

            BuildMenuBackdrop();
            Position(panel, 0, 0, 1488, 900);
            panel.GetComponent<Image>().color = new Color(0.025f, 0.06f, 0.065f, 0.88f);
            Frame(panel, 1488, 900, new Color(0.69f, 0.54f, 0.29f, 0.8f));
            PlaceText(eyebrow, 0, 397, 1300, 44, 28, Gold, false);
            eyebrow.characterSpacing = 5;
            PlaceText(title, 0, 300, 1360, 154, 116, Ivory, true);
            PlaceText(subtitle, 0, 191, 1310, 88, 34, Muted, false);
            PlaceText(detail, 0, 121, 1300, 50, 28, Gold, false);
            var divider = (RectTransform)panel.Find("Gold divider");
            Position(divider, 0, 247, 1120, 2);
            divider.GetComponent<Image>().color = new Color(Gold.r, Gold.g, Gold.b, 0.45f);
            var footer = panel.Find("Controls").GetComponent<TMP_Text>();
            PlaceText(footer, 0, -392, 1390, 80, 26, Muted, false);
            footer.text = "MOVE  Mouse / A-D / Arrows     SPELLS  1 or Space / 2 / 3\nSELECT  Mouse or arrows + Enter";
            BuildHud();
            StyleHotbar();
        }

        private void BuildMenuBackdrop()
        {
            overlay.GetComponent<Image>().color = new Color(0.015f, 0.025f, 0.035f, 1f);
            var artRect = CenterRect("Castle menu art", overlay.transform, 0, 0, 0, 0);
            Stretch(artRect);
            artRect.SetAsFirstSibling();
            var art = artRect.gameObject.AddComponent<RawImage>();
            if (background != null && background.sprite != null) art.texture = background.sprite.texture;
            art.color = art.texture != null ? Color.white : new Color(0.04f, 0.08f, 0.10f);
            art.raycastTarget = false;
            var wash = Box("Readability wash", artRect, 0, 0, 0, 0, new Color(0.015f, 0.03f, 0.04f, 0.58f));
            Stretch(wash);
            var shadow = Box("Panel depth", overlay.transform, 0, -14, 1512, 914, new Color(0, 0, 0, 0.45f));
            shadow.SetSiblingIndex(panel.GetSiblingIndex());
        }

        private void BuildLevelCard(Button button, LevelDefinition layout, float x)
        {
            StyleButton(button, x, -96, 416, 352, Gold);
            var label = button.transform.Find("Label").GetComponent<TMP_Text>();
            PlaceText(label, 0, -35, 388, 64, 42, Ivory, true);
            label.text = layout.Name;
            Text("Chapter", button.transform, 0, 135, 370, 34, 26, Gold, false).text = $"LEVEL {layout.Number:00}";
            Text("Brick count", button.transform, 0, -103, 370, 40, 28, Muted, false).text = $"{layout.BrickCount} BRICKS";
            Box("Card divider", button.transform, 0, -73, 320, 1, new Color(Gold.r, Gold.g, Gold.b, 0.35f));
            // Read the actual catalog so previews cannot drift from their playable formations.
            for (int row = 0; row < layout.Rows; row++)
            for (int col = 0; col < layout.Columns; col++)
            {
                if (!layout.HasBrick(row, col)) continue;
                float bx = (col - (layout.Columns - 1) * 0.5f) * 38;
                float by = 57 + ((layout.Rows - 1) * 0.5f - row) * 15;
                Color tint = Color.Lerp(Jade, Gold, row / (float)Mathf.Max(1, layout.Rows - 1));
                Box($"Preview {row}-{col}", button.transform, bx, by, 32, 10, tint);
            }
        }

        private void BuildDifficultyCard(Button button, int index, float x)
        {
            Color accent = index == 0 ? Jade : index == 1 ? Gold : Coral;
            StyleButton(button, x, -73, 416, 292, accent);
            var label = button.transform.Find("Label").GetComponent<TMP_Text>();
            PlaceText(label, 0, -15, 382, 62, 48, Ivory, true);
            label.text = index == 0 ? "EASY" : index == 1 ? "MEDIUM" : "HARD";
            Text("Difficulty crest", button.transform, 0, 77, 350, 84, 72, accent, true).text =
                index == 0 ? "I" : index == 1 ? "II" : "III";
            Text("Pace", button.transform, 0, -79, 380, 44, 30, accent, false).text =
                index == 0 ? "A slower pace" : index == 1 ? "Find your rhythm" : "Test your reflexes";
        }

        private void StyleAction(Button button, string label, float width = 660, float height = 92, float fontSize = 34)
        {
            StyleButton(button, 0, 0, width, height, Gold);
            var text = button.transform.Find("Label").GetComponent<TMP_Text>();
            PlaceText(text, 0, 0, width - 40, height - 12, fontSize, Ivory, false);
            text.text = label;
        }

        private void StyleButton(Button button, float x, float y, float width, float height, Color accent)
        {
            var rect = (RectTransform)button.transform;
            Position(rect, x, y, width, height);
            button.image.color = Color.white;
            var colors = button.colors;
            colors.normalColor = new Color(0.065f, 0.115f, 0.12f, 0.97f);
            colors.highlightedColor = colors.selectedColor = new Color(0.15f, 0.25f, 0.235f, 1f);
            colors.pressedColor = new Color(0.28f, 0.28f, 0.16f, 1f);
            colors.fadeDuration = 0.10f;
            button.colors = colors;
            Frame(rect, width, height, new Color(accent.r, accent.g, accent.b, 0.65f));
            Box("Accent seam", rect, 0, height * 0.5f - 2, width - 2, 4, accent);
            button.gameObject.AddComponent<MedievalButtonMotion>();
        }

        private void BuildHud()
        {
            // Score's resting pose is serialized in the scene before its feedback captures it.
            // LevelSession continues to own both original score/lives text values.
            var livesPlate = Box("Lives plaque - UI02", hudRoot, 0, 0, 470, 104,
                new Color(0.035f, 0.045f, 0.065f, 0.96f));
            livesPlate.anchorMin = livesPlate.anchorMax = livesPlate.pivot = new Vector2(1, 1);
            livesPlate.anchoredPosition = new Vector2(-48, -30);
            livesPlate.SetSiblingIndex(livesText.transform.GetSiblingIndex());
            Frame(livesPlate, 470, 104, new Color(0.4f, 0.31f, 0.16f));
            Box("Life accent", livesPlate, 233, 0, 4, 104, Coral);

            var readout = Box("Level plaque - UI02", menuRoot, 0, 0, 720, 104,
                new Color(0.035f, 0.045f, 0.065f, 0.96f));
            readout.anchorMin = readout.anchorMax = readout.pivot = new Vector2(0.5f, 1);
            readout.anchoredPosition = new Vector2(0, -30);
            Frame(readout, 720, 104, new Color(0.4f, 0.31f, 0.16f));
            levelReadout = readout.gameObject;
            levelTitle = Text("Level name", readout, 0, 17, 684, 52, 40, Ivory, true);
            difficultyTitle = Text("Difficulty", readout, 0, -29, 680, 34, 26, Gold, false);
            Transform gain = hudRoot.Find("Score plaque - UI01/Score gain - reused");
            if (gain != null) gain.GetComponent<TMP_Text>().fontSize = 32;
        }

        private void StyleHotbar()
        {
            var hotbar = hudRoot.Find("Ability hotbar") as RectTransform;
            if (hotbar == null) return;
            hotbar.sizeDelta = new Vector2(1248, 84);
            hotbar.anchoredPosition = new Vector2(0, 12);
            for (int i = 0; i < hotbar.childCount; i++)
            {
                var card = (RectTransform)hotbar.GetChild(i);
                Position(card, (i - 1) * 424, 0, 400, 84);
                Frame(card, 400, 84, new Color(0.4f, 0.31f, 0.16f));
                var spell = card.Find("Spell").GetComponent<TMP_Text>();
                var status = card.Find("Status").GetComponent<TMP_Text>();
                PlaceText(spell, 0, 21, 384, 32, 24, Ivory, false);
                PlaceText(status, 0, -9, 384, 29, 23, Gold, false);
                var track = (RectTransform)card.Find("Cooldown track");
                Position(track, 0, -33, 280, 5);
            }
        }

        private void RefreshMode()
        {
            lastMode = oldMode.text;
            string[] parts = lastMode.Split('|');
            string left = parts[0].Trim();
            int space = left.IndexOf(' ');
            levelTitle.text = left.StartsWith("L") && space > 1
                ? "LEVEL " + left.Substring(1, space - 1).PadLeft(2, '0') + "  /  " + left.Substring(space + 1)
                : left;
            string mode = parts.Length > 1 ? parts[1].Trim() : "";
            difficultyTitle.text = mode + " DIFFICULTY";
            difficultyTitle.color = mode == "EASY" ? Jade : mode == "HARD" ? Coral : Gold;
        }

        private void RefreshScreen()
        {
            if (!overlay.activeSelf) return;
            title.color = title.text == "GAME OVER" ? Coral : title.text == "VICTORY!" ? Gold : Ivory;
            bool result = retry.gameObject.activeSelf || returnToMenu.gameObject.activeSelf;
            detail.fontSize = result ? 52 : 28;
            if (!result) detail.rectTransform.anchoredPosition = new Vector2(0, 121);
            else detail.rectTransform.anchoredPosition = new Vector2(0, 100);
            SetPosition(back, 0, -292, 380, 68);
            SetPosition(retry, 0, -58, 660, 92);
            SetPosition(next, 0, -58, 660, 92);
            SetPosition(returnToMenu, 0, next.gameObject.activeSelf || retry.gameObject.activeSelf ? -176 : -100, 660, 92);
            RebuildNavigation();
        }

        private void RebuildNavigation()
        {
            // Cards are horizontal; connect both arrow directions so prior menu controls still work.
            var all = new Button[10];
            int count = 0;
            foreach (Button b in levels) if (b.gameObject.activeSelf) all[count++] = b;
            foreach (Button b in difficulties) if (b.gameObject.activeSelf) all[count++] = b;
            if (back.gameObject.activeSelf) all[count++] = back;
            if (retry.gameObject.activeSelf) all[count++] = retry;
            if (next.gameObject.activeSelf) all[count++] = next;
            if (returnToMenu.gameObject.activeSelf) all[count++] = returnToMenu;
            for (int i = 0; i < count; i++)
            {
                Button previous = all[(i + count - 1) % count];
                Button following = all[(i + 1) % count];
                all[i].navigation = new Navigation { mode = Navigation.Mode.Explicit,
                    selectOnLeft = previous, selectOnRight = following,
                    selectOnUp = previous, selectOnDown = following };
            }
        }

        private void FitBackground()
        {
            Camera camera = Camera.main;
            if (background == null || background.sprite == null || camera == null || !camera.orthographic) return;
            Bounds spriteBounds = background.sprite.bounds;
            Vector2 size = spriteBounds.size;
            float height = camera.orthographicSize * 2f;
            float scale = Mathf.Max(height / size.y, height * camera.aspect / size.x);
            background.transform.localScale = new Vector3(scale, scale, 1);
            // Imported artwork may use a corner pivot; center its bounds rather than its transform.
            background.transform.position = new Vector3(camera.transform.position.x - spriteBounds.center.x * scale,
                camera.transform.position.y - spriteBounds.center.y * scale, 5);
            background.sortingOrder = -100;
        }

        private void PlaceText(TMP_Text text, float x, float y, float width, float height, float size, Color color, bool display)
        {
            Position(text.rectTransform, x, y, width, height);
            text.font = display ? displayFont : bodyFont;
            text.fontSize = size;
            text.fontStyle = display ? FontStyles.Normal : FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.raycastTarget = false;
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private TMP_Text Text(string name, Transform parent, float x, float y, float width, float height, float size, Color color, bool display)
        {
            var rect = CenterRect(name, parent, x, y, width, height);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            PlaceText(text, x, y, width, height, size, color, display);
            return text;
        }

        private static RectTransform Box(string name, Transform parent, float x, float y, float width, float height, Color color)
        {
            var rect = CenterRect(name, parent, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static void Frame(RectTransform rect, float width, float height, Color color)
        {
            Box("Frame top", rect, 0, height * 0.5f - 0.5f, width, 1, color);
            Box("Frame bottom", rect, 0, -height * 0.5f + 0.5f, width, 1, color);
            Box("Frame left", rect, -width * 0.5f + 0.5f, 0, 1, height, color);
            Box("Frame right", rect, width * 0.5f - 0.5f, 0, 1, height, color);
        }

        private static RectTransform CenterRect(string name, Transform parent, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.gameObject.layer = 5;
            Position(rect, x, y, width, height);
            return rect;
        }

        private static void Position(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetPosition(Button button, float x, float y, float width, float height)
            => Position((RectTransform)button.transform, x, y, width, height);

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (displayFont == null || displayFont == bodyFont) return;
            if (displayFont.material != null) Destroy(displayFont.material);
            foreach (Texture2D atlas in displayFont.atlasTextures) if (atlas != null) Destroy(atlas);
            Destroy(displayFont);
        }
    }
}
