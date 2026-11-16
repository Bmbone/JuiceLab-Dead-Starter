using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JuiceStudy.BreakEffects
{
    /// <summary>Reuses visual-only brick fragments and impact labels after gameplay registers a break.</summary>
    [DisallowMultipleComponent]
    public sealed class StoneBreakFeedback : MonoBehaviour
    {
        [SerializeField] private Material stoneMaterial;
        [SerializeField] private Material flashMaterial;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField, Range(4, 20)] private int poolSize = 12;
        [SerializeField, Range(0.2f, 0.8f)] private float fragmentDuration = 0.48f;
        [SerializeField, Range(0.03f, 0.12f)] private float flashDuration = 0.065f;
        [SerializeField, Range(0.5f, 3f)] private float fragmentSpeed = 1.6f;
        [SerializeField, Range(0f, 8f)] private float fragmentGravity = 4.4f;
        [SerializeField, Range(0.3f, 1f)] private float popupDuration = 0.66f;
        [SerializeField, Range(28f, 56f)] private float popupFontSize = 42f;
        [SerializeField, Range(16f, 90f)] private float popupRise = 56f;
        [SerializeField, Range(1f, 1.35f)] private float popupPeakScale = 1.18f;

        // These four convex polygons tile the original unit square exactly.
        private static readonly Vector2[][] Shapes =
        {
            new[] { new Vector2(-0.5f, 0.5f), new Vector2(-0.06f, 0.5f), new Vector2(-0.04f, 0.03f), new Vector2(-0.5f, 0.12f) },
            new[] { new Vector2(-0.06f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.08f), new Vector2(-0.04f, 0.03f) },
            new[] { new Vector2(-0.04f, 0.03f), new Vector2(0.5f, -0.08f), new Vector2(0.5f, -0.5f), new Vector2(0.10f, -0.5f) },
            new[] { new Vector2(-0.5f, 0.12f), new Vector2(-0.04f, 0.03f), new Vector2(0.10f, -0.5f), new Vector2(-0.5f, -0.5f) }
        };
        private static readonly int TintId = Shader.PropertyToID("_StoneTint");
        private static readonly int RectId = Shader.PropertyToID("_SpriteRect");
        private static readonly int SizeId = Shader.PropertyToID("_PhysicalSize");
        private static readonly int TilesId = Shader.PropertyToID("_Tiles");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private static readonly int EmblemId = Shader.PropertyToID("_Emblem");
        private static readonly int CrenelsId = Shader.PropertyToID("_Crenels");
        private static readonly Color PopupColor = new Color(1f, 0.93f, 0.70f, 1f);

        private sealed class Fragment
        {
            public SpriteRenderer Renderer;
            public MaterialPropertyBlock Properties;
            public Vector2 Start;
            public Vector2 Velocity;
            public float Spin;
        }

        private sealed class Slot
        {
            public readonly Fragment[] Fragments = new Fragment[4];
            public SpriteRenderer Flash;
            public RectTransform Popup;
            public CanvasGroup PopupFade;
            public TMP_Text Label;
            public TMP_Text Shadow;
            public Vector2 Center;
            public Vector2 Size;
            public Vector2 Direction;
            public Color Tint;
            public float Elapsed;
            public int SpawnFrame;
            public bool Active;
            public bool HasReward;
        }

        private Slot[] slots;
        private readonly Sprite[] fragmentSprites = new Sprite[4];
        private readonly Vector2[] pivots = new Vector2[4];
        private Sprite flashSprite;
        private Transform visualRoot;
        private RectTransform popupCanvas;
        private int nextSlot;
        private int previousScore;
        private int pendingPoints;

        private void Awake()
        {
            if (stoneMaterial == null || flashMaterial == null || worldCamera == null || scoreText == null)
            {
                Debug.LogError("Assign both materials, camera and score text to StoneBreakFeedback.", this);
                enabled = false;
                return;
            }
            visualRoot = new GameObject("Stone break visual pool").transform;
            visualRoot.SetParent(transform, false);
            BuildSprites();
            BuildPopupCanvas();
            slots = new Slot[Mathf.Clamp(poolSize, 4, 20)];
            for (int i = 0; i < slots.Length; i++) slots[i] = BuildSlot(i);
            ClearAll();
        }

        private void OnEnable()
        {
            if (slots == null) return;
            string displayed = scoreText.text;
            int.TryParse(displayed.Substring(displayed.LastIndexOf(' ') + 1), out previousScore);
            pendingPoints = 0;
            GameEvents.ScoreChanged += OnScore;
            GameEvents.BrickDestroyed += OnBrick;
        }

        private void OnDisable()
        {
            GameEvents.ScoreChanged -= OnScore;
            GameEvents.BrickDestroyed -= OnBrick;
            ClearAll();
        }

        private void OnScore(int score)
        {
            int increase = score - previousScore;
            previousScore = score;
            if (score == 0 || increase < 0) { ClearAll(); return; }
            // LevelSession raises the actual new score immediately before BrickDestroyed.
            if (increase > 0) pendingPoints += increase;
        }

        private void OnBrick(ImpactInfo impact)
        {
            if (slots == null) return;
            Slot slot = slots[nextSlot];
            nextSlot = (nextSlot + 1) % slots.Length;
            Hide(slot);
            slot.Center = impact.Center;
            slot.Size = new Vector2(Mathf.Max(0.01f, impact.Size.x), Mathf.Max(0.01f, impact.Size.y));
            slot.Tint = impact.Color;
            Vector2 travel = impact.Center - impact.Point;
            if (travel.sqrMagnitude < 0.0001f) travel = -impact.Normal;
            slot.Direction = travel.sqrMagnitude > 0.0001f ? travel.normalized : Vector2.up;
            slot.Elapsed = 0;
            slot.SpawnFrame = Time.frameCount;
            slot.Active = true;
            slot.Flash.enabled = true;
            slot.Flash.transform.position = slot.Center;
            slot.Flash.transform.localScale = new Vector3(slot.Size.x, slot.Size.y, 1);
            slot.Flash.color = Color.Lerp(slot.Tint, Color.white, 0.86f);

            float seed = SeedAt(slot.Center);
            for (int i = 0; i < 4; i++)
            {
                Fragment part = slot.Fragments[i];
                Vector2 offset = Vector2.Scale(pivots[i], slot.Size);
                part.Start = slot.Center + offset;
                part.Velocity = offset.normalized * (fragmentSpeed * (0.9f + i * 0.06f)) +
                    slot.Direction * 0.65f + Vector2.up * 0.75f;
                part.Spin = i < 2 ? -135f + i * 60f : 75f + (i - 2) * 60f;
                part.Renderer.transform.position = part.Start;
                part.Renderer.transform.rotation = Quaternion.identity;
                part.Renderer.transform.localScale = new Vector3(slot.Size.x, slot.Size.y, 1);
                part.Properties.SetVector(RectId, new Vector4(-0.5f - pivots[i].x, -0.5f - pivots[i].y, 1, 1));
                part.Properties.SetVector(SizeId, new Vector4(slot.Size.x, slot.Size.y, 0, 0));
                part.Properties.SetVector(TilesId, new Vector4(1, 1, 0, 0));
                part.Properties.SetFloat(SeedId, seed);
                part.Properties.SetFloat(EmblemId, 1);
                part.Properties.SetFloat(CrenelsId, 0);
                part.Properties.SetColor(TintId, slot.Tint);
                part.Renderer.SetPropertyBlock(part.Properties);
                part.Renderer.enabled = true;
            }
            slot.HasReward = pendingPoints > 0;
            if (slot.HasReward)
            {
                slot.Label.SetText("+{0}", pendingPoints);
                slot.Shadow.SetText("+{0}", pendingPoints);
                slot.PopupFade.alpha = 1;
                slot.Popup.localScale = Vector3.one * popupPeakScale;
                slot.Popup.gameObject.SetActive(true);
            }
            pendingPoints = 0;
            UpdatePopup(slot);
        }

        private void LateUpdate()
        {
            if (slots == null) return;
            foreach (Slot slot in slots)
            {
                if (!slot.Active) continue;
                // Always show the first impact frame, even when a slow frame exceeds flashDuration.
                if (slot.SpawnFrame != Time.frameCount) slot.Elapsed += Time.deltaTime;
                AnimateBreak(slot);
                UpdatePopup(slot);
                if (slot.Elapsed >= Mathf.Max(fragmentDuration, popupDuration)) Hide(slot);
            }
        }

        private void AnimateBreak(Slot slot)
        {
            float duration = Mathf.Max(fragmentDuration, 0.05f);
            float t = Mathf.Clamp01(slot.Elapsed / duration);
            slot.Flash.enabled = slot.Elapsed < flashDuration;
            if (slot.Flash.enabled)
            {
                Color flash = Color.Lerp(slot.Tint, Color.white, 0.86f);
                flash.a *= 1f - Mathf.Clamp01(slot.Elapsed / Mathf.Max(0.01f, flashDuration));
                slot.Flash.color = flash;
                float expansion = 1f + Mathf.Sin(Mathf.PI * Mathf.Clamp01(slot.Elapsed / flashDuration)) * 0.045f;
                slot.Flash.transform.localScale = new Vector3(slot.Size.x, slot.Size.y, 1) * expansion;
            }
            float flight = Mathf.Max(0, slot.Elapsed - 0.025f);
            float recoil = Mathf.Sin(Mathf.PI * Mathf.Clamp01(slot.Elapsed / 0.08f)) * 0.035f;
            float fade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.35f, 1f, t));
            float shrink = 1f - 0.25f * Mathf.SmoothStep(0.35f, 1f, t);
            Color tint = slot.Tint;
            tint.a *= fade;
            foreach (Fragment part in slot.Fragments)
            {
                part.Renderer.enabled = t < 1;
                if (t >= 1) continue;
                Vector2 position = part.Start + part.Velocity * flight + slot.Direction * recoil +
                    Vector2.down * (0.5f * fragmentGravity * flight * flight);
                part.Renderer.transform.position = position;
                part.Renderer.transform.rotation = Quaternion.Euler(0, 0, part.Spin * flight);
                part.Renderer.transform.localScale = new Vector3(slot.Size.x, slot.Size.y, 1) * shrink;
                part.Properties.SetColor(TintId, tint);
                part.Renderer.SetPropertyBlock(part.Properties);
            }
        }

        private void UpdatePopup(Slot slot)
        {
            if (!slot.HasReward || worldCamera == null) return;
            float t = Mathf.Clamp01(slot.Elapsed / Mathf.Max(0.05f, popupDuration));
            if (t >= 1) { slot.Popup.gameObject.SetActive(false); return; }
            Vector3 view = worldCamera.WorldToViewportPoint(slot.Center);
            slot.Popup.gameObject.SetActive(view.z > 0);
            if (view.z <= 0) return;
            Vector2 size = popupCanvas.rect.size;
            float punch = 1f - Mathf.Clamp01(slot.Elapsed / 0.14f);
            float scale = 1f + (popupPeakScale - 1f) * punch * punch;
            slot.Popup.localScale = Vector3.one * scale;
            // Account for the scaled label bounds, the 134-unit HUD and 96-unit hotbar.
            float halfWidth = slot.Popup.sizeDelta.x * scale * 0.5f;
            float halfHeight = slot.Popup.sizeDelta.y * scale * 0.5f;
            float minY = 96 + halfHeight + 8;
            float maxY = Mathf.Max(minY, size.y - 134 - halfHeight - 8);
            float x = Mathf.Clamp(view.x * size.x, halfWidth + 8, Mathf.Max(halfWidth + 8, size.x - halfWidth - 8));
            float y = Mathf.Clamp(view.y * size.y - 14 + popupRise * t, minY, maxY);
            slot.Popup.anchoredPosition = new Vector2(x, y);
            slot.PopupFade.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1f, t));
        }

        private void BuildSprites()
        {
            Texture2D white = Texture2D.whiteTexture;
            var rect = new Rect(0, 0, white.width, white.height);
            flashSprite = Sprite.Create(white, rect, new Vector2(0.5f, 0.5f), white.width, 0, SpriteMeshType.FullRect);
            flashSprite.name = "Break flash - runtime";
            for (int i = 0; i < 4; i++)
            {
                Vector2 pivot = Vector2.zero;
                foreach (Vector2 point in Shapes[i]) pivot += point;
                pivots[i] = pivot / Shapes[i].Length;
                var vertices = new Vector2[Shapes[i].Length];
                // OverrideGeometry expects pixel coordinates in Sprite.rect, not local unit positions.
                // Its pivot/PPU conversion then recovers our centered local fragment vertices.
                for (int j = 0; j < vertices.Length; j++)
                    vertices[j] = (Shapes[i][j] - pivots[i] + Vector2.one * 0.5f) * white.width;
                Sprite sprite = Sprite.Create(white, rect, new Vector2(0.5f, 0.5f), white.width, 0, SpriteMeshType.FullRect);
                sprite.name = "Carved stone fragment " + i;
                sprite.OverrideGeometry(vertices, new ushort[] { 0, 1, 2, 0, 2, 3 });
                fragmentSprites[i] = sprite;
            }
        }

        private void BuildPopupCanvas()
        {
            var root = new GameObject("Impact scores - pooled", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.layer = 5;
            root.transform.SetParent(transform, false);
            popupCanvas = root.GetComponent<RectTransform>();
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 4; // Result/menu canvas stays on top at order 10.
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            // Deliberately no GraphicRaycaster: impact labels cannot capture mouse input.
        }

        private Slot BuildSlot(int index)
        {
            var slot = new Slot();
            slot.Flash = Renderer("Brick flash " + index, flashSprite, flashMaterial, 1);
            for (int i = 0; i < 4; i++)
            {
                slot.Fragments[i] = new Fragment
                {
                    Renderer = Renderer($"Brick piece {index}-{i}", fragmentSprites[i], stoneMaterial, -4 + i),
                    Properties = new MaterialPropertyBlock()
                };
            }
            var root = new GameObject("Impact reward " + index, typeof(RectTransform), typeof(CanvasGroup));
            root.layer = 5;
            root.transform.SetParent(popupCanvas, false);
            slot.Popup = root.GetComponent<RectTransform>();
            slot.Popup.anchorMin = slot.Popup.anchorMax = Vector2.zero;
            slot.Popup.pivot = new Vector2(0.5f, 0.5f);
            slot.Popup.sizeDelta = new Vector2(180, 72);
            slot.PopupFade = root.GetComponent<CanvasGroup>();
            slot.PopupFade.interactable = false;
            slot.PopupFade.blocksRaycasts = false;
            slot.Shadow = Label("Reward shadow", slot.Popup, new Vector2(2, -3), new Color(0.025f, 0.035f, 0.045f, 0.92f));
            slot.Label = Label("Reward", slot.Popup, Vector2.zero, PopupColor);
            return slot;
        }

        private TMP_Text Label(string name, RectTransform parent, Vector2 offset, Color tint)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.layer = 5;
            var rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(180, 72);
            var label = obj.AddComponent<TextMeshProUGUI>();
            label.font = scoreText.font;
            label.fontSize = popupFontSize;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = tint;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private SpriteRenderer Renderer(string name, Sprite sprite, Material material, int order)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(visualRoot, false);
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            renderer.enabled = false;
            return renderer;
        }

        private static float SeedAt(Vector2 position)
        {
            // Match StoneCourt's deterministic surface seed so the cut pieces retain their carving/grain.
            int key = unchecked(Mathf.RoundToInt(position.x * 100) * 73856093 ^ Mathf.RoundToInt(position.y * 100) * 19349663);
            return (key & 65535) / 65535f * 83f;
        }

        private static void Hide(Slot slot)
        {
            slot.Active = false;
            slot.HasReward = false;
            slot.Flash.enabled = false;
            foreach (Fragment part in slot.Fragments) part.Renderer.enabled = false;
            slot.Popup.gameObject.SetActive(false);
        }

        private void ClearAll()
        {
            pendingPoints = 0;
            nextSlot = 0;
            if (slots == null) return;
            foreach (Slot slot in slots) Hide(slot);
        }

        private void OnDestroy()
        {
            if (visualRoot != null) Destroy(visualRoot.gameObject);
            if (popupCanvas != null) Destroy(popupCanvas.gameObject);
            if (flashSprite != null) Destroy(flashSprite);
            foreach (Sprite sprite in fragmentSprites) if (sprite != null) Destroy(sprite);
        }
    }
}
