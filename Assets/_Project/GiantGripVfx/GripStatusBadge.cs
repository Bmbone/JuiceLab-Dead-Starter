using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace JuiceStudy.GiantGripVfx
{
    /// <summary>Small state badge in the left gutter; never covers the court or takes input.</summary>
    public sealed class GripStatusBadge : IDisposable
    {
        private readonly GameObject root;
        private readonly Canvas canvas;
        private readonly CanvasGroup group;
        private readonly TMP_Text state;
        private readonly Image accent;
        private bool lastRestored;
        private bool initialized;

        public GripStatusBadge(Transform parent, TMP_FontAsset font)
        {
            root = new GameObject("Giant grip state - no input", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.layer = 5;
            root.transform.SetParent(parent, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            RectTransform panel = Rect("Grip status plaque", root.transform, 132, 155, 232, 94);
            panel.anchorMin = panel.anchorMax = new Vector2(0, 0);
            Box(panel, new Color(0.025f, 0.065f, 0.075f, 0.98f));
            Color gold = new Color(0.89f, 0.75f, 0.44f);
            Box(Rect("Top trim", panel, 0, 46, 232, 2), gold);
            Box(Rect("Bottom trim", panel, 0, -46, 232, 2), gold);
            Box(Rect("Left trim", panel, -115, 0, 2, 94), gold);
            Box(Rect("Right trim", panel, 115, 0, 2, 94), gold);
            accent = Box(Rect("State accent", panel, -112, 0, 4, 88), gold);
            Text("Ability", panel, 0, 25, 208, 30, 22, font, gold).text = "GIANT'S GRIP";
            state = Text("State", panel, 0, -16, 208, 44, 32, font, Color.white);
            group = panel.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            Hide();
        }

        public void Show(bool restored, float alpha, float leftCourtEdgePixels)
        {
            bool safe = leftCourtEdgePixels / Mathf.Max(0.001f, canvas.scaleFactor) >= 272;
            group.alpha = safe ? Mathf.Clamp01(alpha) : 0;
            if (!initialized || restored != lastRestored)
            {
                Color color = restored ? new Color(1, 0.85f, 0.43f) : new Color(0.43f, 0.90f, 0.73f);
                state.text = restored ? "RESTORED" : "ENLARGED";
                state.color = accent.color = color;
                lastRestored = restored;
                initialized = true;
            }
        }

        public void Hide() => group.alpha = 0;

        private static TMP_Text Text(string name, Transform parent, float x, float y, float w, float h, float size, TMP_FontAsset font, Color color)
        {
            var text = Rect(name, parent, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.raycastTarget = false;
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        private static Image Box(RectTransform rect, Color color)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
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

        public void Dispose()
        {
            if (root != null) Object.Destroy(root);
            // The HUD owns the borrowed font.
        }
    }
}
