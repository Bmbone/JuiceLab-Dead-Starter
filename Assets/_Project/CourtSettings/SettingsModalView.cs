using System;
using JuiceStudy.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace JuiceStudy.CourtSettings
{
    /// <summary>One retained modal hierarchy, with large labels and explicit keyboard navigation.</summary>
    public sealed class SettingsModalView : IDisposable
    {
        private static readonly Color Gold = new Color(0.86f, 0.71f, 0.40f);
        private static readonly Color Ivory = new Color(0.95f, 0.92f, 0.83f);
        private static readonly Color Jade = new Color(0.38f, 0.84f, 0.67f);
        private readonly GameObject root;
        private readonly GameObject blocker;
        private readonly TMP_FontAsset bodyFont;
        private readonly TMP_FontAsset displayFont;
        private readonly Button entry;
        private readonly TMP_Text entryText;
        private readonly TMP_Text title;
        private readonly TMP_Text subtitle;
        private readonly Button close;
        private readonly Button menu;
        private readonly Button defaults;
        private readonly Slider music;
        private readonly Slider sfx;
        private readonly TMP_Text musicValue;
        private readonly TMP_Text sfxValue;
        private readonly Toggle camera;
        private readonly Toggle bricks;
        private readonly TMP_Text cameraValue;
        private readonly TMP_Text brickValue;
        private bool lastPlaying;
        private bool entryInitialized;

        public SettingsModalView(Transform parent, TMP_FontAsset body, Font heading,
            Action onOpen, Action onClose, Action onMenu, Action<float> onMusic, Action<float> onSfx,
            Action<bool> onCamera, Action<bool> onBricks, Action onDefaults)
        {
            bodyFont = body;
            displayFont = heading != null ? TMP_FontAsset.CreateFontAsset(heading) : bodyFont;
            if (displayFont == null) displayFont = bodyFont;
            root = new GameObject("Court settings UI - retained", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.layer = 5;
            root.transform.SetParent(parent, false);
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            entry = Button("Pause or settings", root.transform, 0, 0, 264, 64, "SETTINGS", 27, onOpen);
            var entryRect = (RectTransform)entry.transform;
            entryRect.anchorMin = entryRect.anchorMax = entryRect.pivot = Vector2.zero;
            entryRect.anchoredPosition = new Vector2(36, 24);
            entry.navigation = new Navigation { mode = Navigation.Mode.None };
            entryText = entry.transform.Find("Label").GetComponent<TMP_Text>();

            RectTransform shade = Rect("Modal input blocker", root.transform, 0, 0, 0, 0);
            Stretch(shade);
            Image(shade, new Color(0.01f, 0.018f, 0.027f, 0.92f), true);
            blocker = shade.gameObject;
            RectTransform panel = Rect("Court options panel", shade, 0, 0, 1000, 860);
            Image(panel, new Color(0.035f, 0.065f, 0.075f, 0.99f));
            Frame(panel, 1000, 860, Gold);
            Box("Header accent", panel, 0, 427, 996, 4, Jade);
            Text("Eyebrow", panel, 0, 368, 880, 36, 23, Gold).text = "BRICKKEEP  /  COURT OPTIONS";
            title = Text("Title", panel, 0, 294, 900, 94, 76, Ivory, true);
            subtitle = Text("Subtitle", panel, 0, 224, 900, 44, 28, Ivory);
            Box("Header rule", panel, 0, 186, 856, 2, new Color(0.46f, 0.37f, 0.20f));
            music = SliderRow(panel, "MUSIC", 124, onMusic, out musicValue);
            sfx = SliderRow(panel, "SOUND EFFECTS", 34, onSfx, out sfxValue);
            camera = ToggleRow(panel, "SCREEN SHAKE", -66, onCamera, out cameraValue);
            bricks = ToggleRow(panel, "BRICK MOTION", -148, onBricks, out brickValue);
            Text("Keyboard hint", panel, 0, -220, 900, 36, 23, Gold).text = "Up/Down: choose   Left/Right: adjust   Enter: select   Esc: close";
            close = Button("Resume or close", panel, -204, -294, 376, 74, "RESUME", 32, onClose);
            menu = Button("Return to menu", panel, 204, -294, 376, 74, "MAIN MENU", 32, onMenu);
            defaults = Button("Restore defaults", panel, 0, -380, 392, 48, "RESTORE DEFAULTS", 24, onDefaults);
            Hide();
        }

        public void SetEntryContext(bool playing)
        {
            if (entryInitialized && playing == lastPlaying) return;
            entryText.text = playing ? "PAUSE / ESC" : "SETTINGS";
            lastPlaying = playing;
            entryInitialized = true;
        }

        public void SetValues(float musicGain, float sfxGain, bool shake, bool brickMotion)
        {
            music.SetValueWithoutNotify(musicGain);
            sfx.SetValueWithoutNotify(sfxGain);
            musicValue.text = Percent(musicGain);
            sfxValue.text = Percent(sfxGain);
            camera.SetIsOnWithoutNotify(shake);
            bricks.SetIsOnWithoutNotify(brickMotion);
            cameraValue.text = shake ? "ON" : "OFF";
            brickValue.text = brickMotion ? "ON" : "OFF";
        }

        public void Show(bool playing)
        {
            title.text = playing ? "PAUSED" : "SETTINGS";
            subtitle.text = playing ? "Take a breath. Your run will wait." : "Set the sound and motion of your court.";
            close.transform.Find("Label").GetComponent<TMP_Text>().text = playing ? "RESUME" : "CLOSE";
            ((RectTransform)close.transform).anchoredPosition = new Vector2(playing ? -204 : 0, -294);
            menu.gameObject.SetActive(playing);
            Selectable[] controls = playing
                ? new Selectable[] { music, sfx, camera, bricks, close, menu, defaults }
                : new Selectable[] { music, sfx, camera, bricks, close, defaults };
            for (int i = 0; i < controls.Length; i++)
            {
                controls[i].navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = controls[(i + controls.Length - 1) % controls.Length],
                    selectOnDown = controls[(i + 1) % controls.Length]
                };
            }
            blocker.SetActive(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(close.gameObject);
        }

        public void Hide() => blocker.SetActive(false);
        public void SetAvailable(bool available) => root.SetActive(available);
        private static string Percent(float gain) => Mathf.RoundToInt(gain * 100) + "%";

        private Slider SliderRow(Transform parent, string label, float y, Action<float> callback, out TMP_Text valueText)
        {
            Text(label + " label", parent, -247, y, 360, 54, 33, Ivory).text = label;
            RectTransform row = Rect(label + " slider", parent, 108, y, 356, 60);
            Image(row, new Color(0.05f, 0.09f, 0.10f), true);
            Slider slider = row.gameObject.AddComponent<Slider>();
            Box("Track", row, 0, 0, 328, 14, new Color(0.10f, 0.19f, 0.20f));
            RectTransform fillArea = Rect("Fill area", row, 0, 0, 328, 14);
            RectTransform fill = Rect("Fill", fillArea, 0, 0, 0, 0);
            Stretch(fill);
            Image(fill, Gold);
            RectTransform handleArea = Rect("Handle area", row, 0, 0, 328, 60);
            RectTransform handle = Rect("Handle", handleArea, 0, 0, 24, 44);
            UnityEngine.UI.Image handleImage = Image(handle, Ivory, true);
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.wholeNumbers = false;
            slider.direction = Slider.Direction.LeftToRight;
            SetColors(slider);
            TMP_Text readout = Text(label + " value", parent, 366, y, 114, 54, 33, Jade);
            valueText = readout;
            slider.onValueChanged.AddListener(value => { readout.text = Percent(value); callback(value); });
            return slider;
        }

        private Toggle ToggleRow(Transform parent, string label, float y, Action<bool> callback, out TMP_Text valueText)
        {
            RectTransform row = Rect(label + " toggle", parent, 0, y, 856, 64);
            UnityEngine.UI.Image background = Image(row, new Color(0.065f, 0.12f, 0.13f), true);
            Toggle toggle = row.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            SetColors(toggle);
            Text("Label", row, -148, 0, 520, 52, 33, Ivory).text = label;
            TMP_Text state = Text("State", row, 276, 0, 92, 48, 27, Gold);
            valueText = state;
            RectTransform well = Rect("Switch well", row, 367, 0, 72, 40);
            Image(well, new Color(0.025f, 0.05f, 0.06f));
            Frame(well, 72, 40, Gold);
            toggle.graphic = Image(Rect("Enabled mark", well, 0, 0, 52, 22), Jade);
            toggle.onValueChanged.AddListener(value => { state.text = value ? "ON" : "OFF"; callback(value); });
            return toggle;
        }

        private Button Button(string name, Transform parent, float x, float y, float w, float h, string label, float size, Action callback)
        {
            RectTransform rect = Rect(name, parent, x, y, w, h);
            UnityEngine.UI.Image background = Image(rect, new Color(0.08f, 0.16f, 0.16f), true);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            SetColors(button);
            Frame(rect, w, h, Gold);
            Text("Label", rect, 0, 0, w - 20, h - 8, size, Ivory).text = label;
            button.onClick.AddListener(() => callback());
            rect.gameObject.AddComponent<MedievalButtonMotion>();
            return button;
        }

        private static void SetColors(Selectable selectable)
        {
            ColorBlock colors = selectable.colors;
            Color resting = selectable.targetGraphic.color;
            selectable.targetGraphic.color = Color.white;
            colors.normalColor = resting;
            colors.highlightedColor = colors.selectedColor = resting.grayscale > 0.6f ? Gold : new Color(0.19f, 0.32f, 0.28f);
            colors.pressedColor = new Color(0.31f, 0.34f, 0.22f);
            colors.fadeDuration = 0.08f;
            selectable.colors = colors;
        }

        private TMP_Text Text(string name, Transform parent, float x, float y, float w, float h, float size, Color color, bool display = false)
        {
            RectTransform rect = Rect(name, parent, x, y, w, h);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = display ? displayFont : bodyFont;
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static UnityEngine.UI.Image Image(RectTransform rect, Color color, bool raycast = false)
        {
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        private static void Box(string name, Transform parent, float x, float y, float w, float h, Color color)
            => Image(Rect(name, parent, x, y, w, h), color);

        private static void Frame(RectTransform parent, float w, float h, Color color)
        {
            Box("Top trim", parent, 0, h * 0.5f - 1, w, 2, color);
            Box("Bottom trim", parent, 0, -h * 0.5f + 1, w, 2, color);
            Box("Left trim", parent, -w * 0.5f + 1, 0, 2, h, color);
            Box("Right trim", parent, w * 0.5f - 1, 0, 2, h, color);
        }

        private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.gameObject.layer = 5;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        public void Dispose()
        {
            if (root != null) Object.Destroy(root);
            if (displayFont == null || displayFont == bodyFont) return;
            if (displayFont.material != null) Object.Destroy(displayFont.material);
            foreach (Texture2D atlas in displayFont.atlasTextures) if (atlas != null) Object.Destroy(atlas);
            Object.Destroy(displayFont);
        }
    }
}
