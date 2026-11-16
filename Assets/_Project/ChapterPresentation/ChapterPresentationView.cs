using System;
using JuiceStudy.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace JuiceStudy.Chapters
{
    /// <summary>Retained, noninteractive UI decoration; borrowed fonts and original score text stay with their owners.</summary>
    public sealed class ChapterPresentationView : IDisposable
    {
        private static readonly Color Gold = new Color(0.94f, 0.79f, 0.46f);
        private static readonly Color Ivory = new Color(0.97f, 0.94f, 0.85f);
        private static readonly Color Jade = new Color(0.43f, 0.85f, 0.68f);
        private static readonly Color Coral = new Color(1f, 0.58f, 0.49f);
        private static readonly Color Ink = new Color(0.025f, 0.06f, 0.07f, 0.98f);
        private readonly GameObject introRoot;
        private readonly Canvas introCanvas;
        private readonly RectTransform introCard;
        private readonly CanvasGroup introGroup;
        private readonly TMP_Text introChapter;
        private readonly TMP_Text introTitle;
        private readonly TMP_Text introMode;
        private readonly GameObject resultRoot;
        private readonly CanvasGroup resultGroup;
        private readonly Image scorePlate;
        private readonly Image[] accentImages;
        private readonly TMP_Text resultSeal;
        private readonly TMP_Text resultCue;
        private readonly TMP_FontAsset body;
        private readonly TMP_FontAsset display;
        private float resultAge;

        public ChapterPresentationView(Transform parent, RectTransform panel, Transform scoreOwner,
            TMP_FontAsset bodyFont, TMP_FontAsset displayFont)
        {
            body = bodyFont;
            display = displayFont != null ? displayFont : bodyFont;
            introRoot = new GameObject("Chapter intro canvas - no input", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            introRoot.layer = 5;
            introRoot.transform.SetParent(parent, false);
            introCanvas = introRoot.GetComponent<Canvas>();
            introCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            introCanvas.sortingOrder = 8;
            CanvasScaler scaler = introRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            introCard = Rect("Chapter banner - outside court", introRoot.transform, 132, 0, 232, 300);
            introCard.anchorMin = introCard.anchorMax = new Vector2(0, 0.67f);
            Box(introCard, Ink);
            Frame(introCard, 232, 300, Gold);
            introGroup = NoInput(introCard.gameObject);
            introChapter = Text("Chapter", introCard, 0, 126, 208, 30, 24, Gold);
            Crest("Court crest", introCard, 0, 60, 1, Gold, new Color(0.055f, 0.18f, 0.16f));
            introTitle = Text("Level name", introCard, 0, -44, 208, 84, 32, Ivory, true);
            introMode = Text("Difficulty", introCard, 0, -126, 208, 32, 24, Jade);

            RectTransform result = Rect("Chapter result decoration - no input", panel, 0, 0, 1488, 900);
            resultRoot = result.gameObject;
            resultGroup = NoInput(resultRoot);
            // The original score remains a sibling drawn above its new plaque.
            result.SetSiblingIndex(scoreOwner.GetSiblingIndex());
            RectTransform plaque = Rect("Final score plaque", result, 0, 100, 920, 92);
            scorePlate = Box(plaque, new Color(0.055f, 0.13f, 0.13f, 0.98f));
            Frame(plaque, 920, 92, Gold);
            Box(Rect("Score accent", plaque, -456, 0, 6, 88), Gold);
            Crest("Score seal", plaque, -378, 5, 0.58f, Gold, Ink);
            resultSeal = Text("Outcome stamp", plaque, 374, 0, 132, 48, 28, Gold);
            Crest("Left heraldry", result, -606, 306, 1.4f, Gold, new Color(0.055f, 0.18f, 0.16f));
            Crest("Right heraldry", result, 606, 306, 1.4f, Gold, new Color(0.055f, 0.18f, 0.16f));
            resultCue = Text("Next step", result, 0, -292, 1120, 58, 28, Gold);
            // Only the original gold ornament images are recolored; dark fills keep their contrast.
            Image[] all = result.GetComponentsInChildren<Image>();
            int count = 0;
            foreach (Image item in all) if (item.color == Gold) count++;
            accentImages = new Image[count];
            int index = 0;
            foreach (Image item in all) if (item.color == Gold) accentImages[index++] = item;
            HideIntro();
            HideResult();
        }

        public void SetIntro(LevelDefinition layout, string difficulty)
        {
            introChapter.text = "LEVEL " + layout.Number.ToString("00");
            introTitle.text = layout.Id == LevelId.TwinTowers ? "Twin\nTowers" : layout.Name;
            introMode.text = difficulty;
            introMode.color = difficulty == "HARD" ? Coral : difficulty == "MEDIUM" ? Gold : Jade;
        }

        public void UpdateIntro(float alpha, float leftCourtEdgePixels)
        {
            // 16-unit outer margin + 232-unit banner + 24-unit safety gap.
            bool safe = leftCourtEdgePixels / Mathf.Max(0.001f, introCanvas.scaleFactor) >= 272;
            introCard.gameObject.SetActive(safe && alpha > 0);
            introGroup.alpha = Mathf.Clamp01(alpha);
        }

        public void HideIntro()
        {
            introCard.gameObject.SetActive(false);
            introGroup.alpha = 0;
        }

        public void ShowResult(bool won, string cue)
        {
            Color accent = won ? Gold : Coral;
            foreach (Image item in accentImages) item.color = accent;
            scorePlate.color = won ? new Color(0.055f, 0.13f, 0.13f, 0.98f) : new Color(0.14f, 0.07f, 0.075f, 0.98f);
            resultSeal.text = won ? "CLEAR" : "RETRY";
            resultSeal.color = accent;
            resultCue.text = cue;
            resultCue.color = won ? Gold : Ivory;
            resultAge = 0;
            resultGroup.alpha = 0;
            resultRoot.SetActive(true);
        }

        public void TickResult(float delta)
        {
            resultAge += Mathf.Max(0, delta);
            resultGroup.alpha = Mathf.SmoothStep(0, 1, resultAge / 0.18f);
        }

        public void HideResult()
        {
            resultRoot.SetActive(false);
            resultGroup.alpha = 0;
        }

        private static CanvasGroup NoInput(GameObject obj)
        {
            CanvasGroup group = obj.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            return group;
        }

        private static void Crest(string name, Transform parent, float x, float y, float scale, Color trim, Color fill)
        {
            RectTransform root = Rect(name, parent, x, y, 76, 96);
            root.localScale = Vector3.one * scale;
            RectTransform backTip = Rect("Shield point trim", root, 0, -18, 54, 54);
            backTip.localRotation = Quaternion.Euler(0, 0, 45);
            Box(backTip, trim);
            Box(Rect("Shield top trim", root, 0, 10, 76, 60), trim);
            RectTransform tip = Rect("Shield point", root, 0, -18, 48, 48);
            tip.localRotation = Quaternion.Euler(0, 0, 45);
            Box(tip, fill);
            Box(Rect("Shield face", root, 0, 10, 68, 52), fill);
            Box(Rect("Left tower", root, -20, 8, 10, 24), trim);
            Box(Rect("Right tower", root, 20, 8, 10, 24), trim);
            Box(Rect("Keep", root, 0, 15, 12, 38), trim);
            Box(Rect("Bridge", root, 0, 0, 42, 9), trim);
            Box(Rect("Gate", root, 0, 2, 6, 13), fill);
        }

        private TMP_Text Text(string name, Transform parent, float x, float y, float w, float h, float size, Color color, bool heading = false)
        {
            var text = Rect(name, parent, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = heading ? display : body;
            text.fontSize = size;
            text.fontStyle = heading ? FontStyles.Normal : FontStyles.Bold;
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

        private static void Frame(RectTransform rect, float w, float h, Color color)
        {
            Box(Rect("Top trim", rect, 0, h * 0.5f - 1, w, 2), color);
            Box(Rect("Bottom trim", rect, 0, -h * 0.5f + 1, w, 2), color);
            Box(Rect("Left trim", rect, -w * 0.5f + 1, 0, 2, h), color);
            Box(Rect("Right trim", rect, w * 0.5f - 1, 0, 2, h), color);
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
            if (introRoot != null) Object.Destroy(introRoot);
            if (resultRoot != null) Object.Destroy(resultRoot);
            // Fonts belong to the existing HUD/interface, and are never destroyed here.
        }
    }
}
