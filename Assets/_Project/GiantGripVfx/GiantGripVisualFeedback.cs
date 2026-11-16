using JuiceStudy.Abilities;
using JuiceStudy.Levels;
using TMPro;
using UnityEngine;

namespace JuiceStudy.GiantGripVfx
{
    /// <summary>Decorates the existing expanded collider; never resizes or moves the paddle.</summary>
    [DefaultExecutionOrder(450), DisallowMultipleComponent]
    public sealed class GiantGripVisualFeedback : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private AbilityPaddle paddle;
        [SerializeField] private BoxCollider2D paddleCollider;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Collider2D leftWall;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Material auraMaterial;
        [SerializeField] private Material runeMaterial;
        [SerializeField] private Material moteMaterial;
        [SerializeField, Range(0f, 1f)] private float auraOpacity = 0.65f;
        [SerializeField] private bool enableMotes = true;
        private const int MoteCount = 8;
        private sealed class Mote
        {
            public SpriteRenderer Renderer;
            public MaterialPropertyBlock Properties;
            public Vector3 Position;
            public Vector3 Velocity;
            public Color Color;
            public float Age = 999;
            public float Lifetime;
            public float Size;
        }
        private readonly Mote[] motes = new Mote[MoteCount];
        private readonly System.Random cosmetic = new System.Random(40214);
        private float normalWidth;
        private bool ready;
        private bool wasExpanded;
        private float activeAge;
        private float restoredAge = 999;
        private int transitionFrame = -1;
        private int ignoreThroughFrame = -1;
        private Transform visualRoot;
        private Sprite quad;
        private SpriteRenderer aura;
        private SpriteRenderer leftRune;
        private SpriteRenderer rightRune;
        private MaterialPropertyBlock auraProperties;
        private MaterialPropertyBlock runeProperties;
        private GripStatusBadge badge;
        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int BodySizeId = Shader.PropertyToID("_BodySize");
        private static readonly int QuadSizeId = Shader.PropertyToID("_QuadSize");
        private static readonly int AgeId = Shader.PropertyToID("_Age");
        private static readonly int PulseId = Shader.PropertyToID("_Pulse");

        private void Awake()
        {
            if (session == null || paddle == null || paddleCollider == null || worldCamera == null ||
                leftWall == null || scoreText == null || auraMaterial == null || runeMaterial == null || moteMaterial == null ||
                paddleCollider.gameObject != paddle.gameObject || paddle.Width <= 0)
            {
                Debug.LogError("Assign the Giant's Grip session, initialized paddle/collider, camera, wall, font and materials.", this);
                enabled = false;
                return;
            }
            normalWidth = paddle.Width;
            Texture2D white = Texture2D.whiteTexture;
            quad = Sprite.Create(white, new Rect(0, 0, white.width, white.height), new Vector2(0.5f, 0.5f),
                white.width, 0, SpriteMeshType.FullRect);
            quad.name = "Giant grip quad - owned";
            visualRoot = new GameObject("Giant grip presentation - reused").transform;
            visualRoot.SetParent(transform, false);
            aura = MakeSprite("Emerald reach aura", auraMaterial, -1);
            leftRune = MakeSprite("Left gold grip rune", runeMaterial, 1);
            rightRune = MakeSprite("Right gold grip rune", runeMaterial, 1);
            auraProperties = new MaterialPropertyBlock();
            runeProperties = new MaterialPropertyBlock();
            for (int i = 0; i < motes.Length; i++)
                motes[i] = new Mote { Renderer = MakeSprite("Grip mote " + i, moteMaterial, 2), Properties = new MaterialPropertyBlock() };
            badge = new GripStatusBadge(transform, scoreText.font);
            ready = true;
            ClearVisuals();
        }

        private SpriteRenderer MakeSprite(string name, Material material, int order)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(visualRoot, false);
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = quad;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order; // Ball stays at order 4; brass paddle stays at 0.
            return renderer;
        }

        private bool Expanded => paddle.Width > normalWidth * 1.3f;

        private void OnEnable()
        {
            ClearVisuals();
            // Re-enable during an existing spell resumes its appearance without a second cast burst.
            if (ready && session.IsPlaying) { wasExpanded = Expanded; activeAge = 0.3f; }
            GameEvents.BallLost += OnLost;
            GameEvents.ScoreChanged += OnScore;
        }

        private void LateUpdate()
        {
            if (!ready) return;
            if (!session.IsPlaying || Time.frameCount <= ignoreThroughFrame ||
                !paddle.gameObject.activeInHierarchy || !paddleCollider.enabled)
            {
                ClearVisuals();
                return;
            }
            float dt = Mathf.Max(0, Time.deltaTime);
            bool expanded = Expanded;
            if (expanded != wasExpanded)
            {
                transitionFrame = Time.frameCount;
                if (expanded) { activeAge = 0; restoredAge = 999; }
                else restoredAge = 0;
                if (enableMotes) EmitMotes(expanded);
            }
            if (transitionFrame != Time.frameCount)
            {
                activeAge += dt;
                restoredAge += dt;
            }
            if (expanded) DrawEmpowered();
            else HideEmpowered();
            AdvanceMotes(dt);
            if (expanded || restoredAge < 0.4f)
            {
                float alpha = expanded ? 1 : Mathf.Clamp01((0.4f - restoredAge) / 0.15f);
                float edge = worldCamera.WorldToScreenPoint(new Vector3(leftWall.bounds.max.x, 0, 0)).x;
                badge.Show(!expanded, alpha, edge);
            }
            else badge.Hide();
            wasExpanded = expanded;
        }

        private void DrawEmpowered()
        {
            Bounds bounds = ReadPaddleBounds();
            float pulse = 1 - Mathf.Clamp01(activeAge / 0.28f);
            pulse *= pulse;
            Vector3 size = new Vector3(bounds.size.x + 0.20f, bounds.size.y + 0.20f, 1);
            aura.enabled = leftRune.enabled = rightRune.enabled = true;
            aura.transform.position = bounds.center;
            aura.transform.localScale = size;
            auraProperties.SetVector(BodySizeId, new Vector4(bounds.size.x, bounds.size.y, 0, 0));
            auraProperties.SetVector(QuadSizeId, new Vector4(size.x, size.y, 0, 0));
            auraProperties.SetColor(TintId, new Color(1, 1, 1, Mathf.Clamp01(auraOpacity)));
            auraProperties.SetFloat(AgeId, activeAge);
            auraProperties.SetFloat(PulseId, pulse);
            aura.SetPropertyBlock(auraProperties);
            runeProperties.SetColor(TintId, new Color(1, 0.9f, 0.6f, 0.82f));
            runeProperties.SetFloat(PulseId, pulse);
            PlaceRune(leftRune, new Vector3(bounds.min.x + 0.09f, bounds.center.y, bounds.center.z));
            PlaceRune(rightRune, new Vector3(bounds.max.x - 0.09f, bounds.center.y, bounds.center.z));
        }

        private Bounds ReadPaddleBounds()
        {
            // AutoSyncTransforms is off in this project. Read the actual local box through the
            // current render transform rather than relying on last physics step's cached bounds.
            Vector2 half = paddleCollider.size * 0.5f;
            Vector2 offset = paddleCollider.offset;
            Transform pose = paddleCollider.transform;
            var bounds = new Bounds(pose.TransformPoint(offset - half), Vector3.zero);
            bounds.Encapsulate(pose.TransformPoint(offset + half));
            bounds.Encapsulate(pose.TransformPoint(offset + new Vector2(-half.x, half.y)));
            bounds.Encapsulate(pose.TransformPoint(offset + new Vector2(half.x, -half.y)));
            return bounds;
        }

        private void PlaceRune(SpriteRenderer renderer, Vector3 position)
        {
            renderer.transform.position = position;
            renderer.transform.localScale = new Vector3(0.14f, 0.34f, 1);
            renderer.SetPropertyBlock(runeProperties);
        }

        private float Next01() => (float)cosmetic.NextDouble();

        private void EmitMotes(bool activating)
        {
            Bounds bounds = ReadPaddleBounds();
            for (int i = 0; i < motes.Length; i++)
            {
                Mote mote = motes[i];
                float fraction = (i + 0.5f) / motes.Length;
                mote.Position = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, fraction), bounds.max.y + 0.04f, bounds.center.z);
                mote.Velocity = new Vector3((fraction - 0.5f) * (activating ? 0.5f : -0.35f),
                    activating ? 0.65f + Next01() * 0.45f : 0.10f + Next01() * 0.15f, 0);
                mote.Lifetime = activating ? 0.32f + Next01() * 0.12f : 0.22f + Next01() * 0.08f;
                mote.Size = 0.035f + Next01() * 0.025f;
                mote.Color = activating ? new Color(0.52f, 1, 0.78f) : new Color(1, 0.86f, 0.48f);
                mote.Age = 0;
                DrawMote(mote);
            }
        }

        private void AdvanceMotes(float delta)
        {
            foreach (Mote mote in motes)
            {
                if (!enableMotes || mote.Age >= mote.Lifetime) { mote.Age = 999; mote.Renderer.enabled = false; continue; }
                if (transitionFrame != Time.frameCount)
                {
                    mote.Age += delta;
                    mote.Velocity += Vector3.down * (0.7f * delta);
                    mote.Position += mote.Velocity * delta;
                }
                if (mote.Age >= mote.Lifetime) mote.Renderer.enabled = false;
                else DrawMote(mote);
            }
        }

        private void DrawMote(Mote mote)
        {
            float t = Mathf.Clamp01(mote.Age / Mathf.Max(0.001f, mote.Lifetime));
            mote.Renderer.enabled = true;
            mote.Renderer.transform.position = mote.Position;
            mote.Renderer.transform.localScale = new Vector3(mote.Size, mote.Size * 2, 1) * (1 - t * 0.5f);
            Color color = mote.Color;
            color.a = 0.8f * (1 - t) * (1 - t);
            mote.Properties.SetColor(TintId, color);
            mote.Renderer.SetPropertyBlock(mote.Properties);
        }

        private void HideEmpowered()
        {
            if (aura != null) aura.enabled = false;
            if (leftRune != null) leftRune.enabled = false;
            if (rightRune != null) rightRune.enabled = false;
        }

        private void ClearVisuals()
        {
            HideEmpowered();
            badge?.Hide();
            wasExpanded = false;
            activeAge = 0;
            restoredAge = 999;
            transitionFrame = -1;
            foreach (Mote mote in motes)
                if (mote != null) { mote.Age = 999; mote.Renderer.enabled = false; }
        }

        private void OnLost() => ResetRound();
        private void OnScore(int score) { if (score == 0) ResetRound(); }
        private void ResetRound() { ClearVisuals(); ignoreThroughFrame = Time.frameCount; }

        private void OnDisable()
        {
            GameEvents.BallLost -= OnLost;
            GameEvents.ScoreChanged -= OnScore;
            ClearVisuals();
        }

        private void OnDestroy()
        {
            GameEvents.BallLost -= OnLost;
            GameEvents.ScoreChanged -= OnScore;
            badge?.Dispose();
            if (visualRoot != null) Destroy(visualRoot.gameObject);
            if (quad != null) Destroy(quad);
        }
    }
}
