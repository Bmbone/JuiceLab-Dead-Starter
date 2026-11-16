using JuiceStudy.Levels;
using TMPro;
using UnityEngine;

namespace JuiceStudy.RuneShieldVfx
{
    /// <summary>Reads the existing shield line's visibility/flash; never arms or reflects a shield itself.</summary>
    [DefaultExecutionOrder(400), DisallowMultipleComponent]
    public sealed class RuneShieldVisualFeedback : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private Transform abilityRoot;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Collider2D rightWall;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Material wardMaterial;
        [SerializeField] private Material anchorMaterial;
        [SerializeField] private Material shardMaterial;
        [SerializeField, Range(0f, 1f)] private float wardOpacity = 0.75f;
        [SerializeField] private bool enableShards = true;
        private const float NativeWidth = 0.065f;
        private const float NativeFlashWidth = 0.15f;
        private const int ShardCount = 12;
        private sealed class Shard
        {
            public SpriteRenderer Renderer;
            public MaterialPropertyBlock Properties;
            public Vector3 Position;
            public Vector3 Velocity;
            public float Age = 999;
            public float Lifetime;
            public float Size;
            public float Angle;
        }
        private readonly Shard[] shards = new Shard[ShardCount];
        private readonly System.Random cosmetic = new System.Random(40213);
        private LineRenderer source;
        private bool originalHidden;
        private bool ownsSource;
        private bool bound;
        private bool wasVisible;
        private bool wasPulse;
        private bool spent;
        private int ignoreThroughFrame = -1;
        private int deflectFrame = -1;
        private float wardAge;
        private float deflectAge = 999;
        private Transform visualRoot;
        private Sprite quad;
        private SpriteRenderer ward;
        private SpriteRenderer leftAnchor;
        private SpriteRenderer rightAnchor;
        private MaterialPropertyBlock wardProperties;
        private MaterialPropertyBlock anchorProperties;
        private RuneShieldBadge badge;
        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int WidthId = Shader.PropertyToID("_Width");
        private static readonly int AgeId = Shader.PropertyToID("_Age");
        private static readonly int PulseId = Shader.PropertyToID("_Pulse");

        private void Awake()
        {
            if (session == null || abilityRoot == null || worldCamera == null || rightWall == null || scoreText == null ||
                wardMaterial == null || anchorMaterial == null || shardMaterial == null)
            {
                Debug.LogError("Assign all RuneShieldVisualFeedback scene and material references.", this);
                enabled = false;
                return;
            }
            Texture2D white = Texture2D.whiteTexture;
            quad = Sprite.Create(white, new Rect(0, 0, white.width, white.height), new Vector2(0.5f, 0.5f),
                white.width, 0, SpriteMeshType.FullRect);
            quad.name = "Rune shield VFX quad - owned";
            visualRoot = new GameObject("Rune shield presentation - reused").transform;
            visualRoot.SetParent(transform, false);
            ward = MakeSprite("Runic ward", wardMaterial);
            leftAnchor = MakeSprite("Left rune anchor", anchorMaterial);
            rightAnchor = MakeSprite("Right rune anchor", anchorMaterial);
            wardProperties = new MaterialPropertyBlock();
            anchorProperties = new MaterialPropertyBlock();
            for (int i = 0; i < shards.Length; i++)
                shards[i] = new Shard { Renderer = MakeSprite("Deflection shard " + i, shardMaterial), Properties = new MaterialPropertyBlock() };
            badge = new RuneShieldBadge(transform, scoreText.font);
            ClearVisuals();
        }

        private SpriteRenderer MakeSprite(string name, Material material)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(visualRoot, false);
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = quad;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = -1; // Every world effect stays behind the regular ball and paddle.
            return renderer;
        }

        private void Start()
        {
            Transform node = abilityRoot.Find("Rune shield");
            if (node == null || !node.TryGetComponent(out source) || source.positionCount < 2)
            {
                Debug.LogError("The existing AbilityEffects Rune shield line was not found.", this);
                enabled = false;
                return;
            }
            bound = true;
            OwnSource();
            CaptureCurrentState();
        }

        private void OnEnable()
        {
            ClearVisuals();
            if (bound && source != null) { OwnSource(); CaptureCurrentState(); }
            GameEvents.BallLost += OnLost;
            GameEvents.ScoreChanged += OnScore;
        }

        private void OnDisable()
        {
            GameEvents.BallLost -= OnLost;
            GameEvents.ScoreChanged -= OnScore;
            ClearVisuals();
            RestoreSource();
        }

        private bool SourceVisible => source != null && source.enabled && source.gameObject.activeInHierarchy;
        private float Pulse => Mathf.Clamp01((source.startWidth - NativeWidth) / NativeFlashWidth);

        private void CaptureCurrentState()
        {
            // A mid-ability enable resumes its appearance without replaying an activation/break burst.
            wasVisible = SourceVisible;
            wasPulse = wasVisible && Pulse > 0.001f;
            spent = wasPulse;
            wardAge = wasVisible ? 0.18f : 0;
        }

        private void LateUpdate()
        {
            if (!bound || source == null) return;
            if (!session.IsPlaying || Time.frameCount <= ignoreThroughFrame) { ClearVisuals(); return; }
            float dt = Mathf.Max(0, Time.deltaTime);
            bool visible = SourceVisible;
            float pulse = visible ? Pulse : 0;
            bool flashing = visible && pulse > 0.001f;
            if (visible && !wasVisible) { wardAge = 0; spent = false; }
            if (flashing && !wasPulse)
            {
                spent = true;
                deflectAge = 0;
                deflectFrame = Time.frameCount;
                if (enableShards) EmitShards();
            }
            if (visible)
            {
                wardAge += dt;
                DrawWard(pulse);
            }
            else HideWard();
            if (deflectFrame != Time.frameCount) deflectAge += dt;
            AdvanceShards(dt);
            bool deflected = (visible && spent) || deflectAge < 0.45f;
            if (visible || deflected)
            {
                float alpha = visible ? 1 : Mathf.Clamp01((0.45f - deflectAge) / 0.15f);
                float edge = worldCamera.WorldToScreenPoint(new Vector3(rightWall.bounds.min.x, 0, 0)).x;
                badge.Show(deflected, alpha, edge);
            }
            else badge.Hide();
            wasVisible = visible;
            wasPulse = flashing;
        }

        private void ReadPlane(out Vector3 a, out Vector3 b, out Vector3 up, out Quaternion rotation)
        {
            a = source.GetPosition(0);
            b = source.GetPosition(1);
            if (!source.useWorldSpace) { a = source.transform.TransformPoint(a); b = source.transform.TransformPoint(b); }
            Vector3 along = (b - a).normalized;
            up = new Vector3(-along.y, along.x, 0);
            rotation = Quaternion.Euler(0, 0, Mathf.Atan2(along.y, along.x) * Mathf.Rad2Deg);
        }

        private void DrawWard(float pulse)
        {
            ReadPlane(out Vector3 a, out Vector3 b, out Vector3 up, out Quaternion rotation);
            float opacity = Mathf.Clamp01(wardOpacity) * (spent ? Mathf.Max(0.05f, pulse) : 1);
            ward.enabled = leftAnchor.enabled = rightAnchor.enabled = true;
            ward.transform.position = (a + b) * 0.5f + up * 0.23f;
            ward.transform.rotation = rotation;
            ward.transform.localScale = new Vector3(Vector3.Distance(a, b), 0.46f, 1);
            wardProperties.SetColor(TintId, new Color(1, 1, 1, opacity));
            wardProperties.SetFloat(WidthId, Vector3.Distance(a, b));
            wardProperties.SetFloat(AgeId, wardAge);
            wardProperties.SetFloat(PulseId, pulse);
            ward.SetPropertyBlock(wardProperties);
            anchorProperties.SetColor(TintId, new Color(1, 1, 1, opacity));
            anchorProperties.SetFloat(PulseId, pulse);
            PlaceAnchor(leftAnchor, a + up * 0.22f, rotation);
            PlaceAnchor(rightAnchor, b + up * 0.22f, rotation);
        }

        private void PlaceAnchor(SpriteRenderer renderer, Vector3 position, Quaternion rotation)
        {
            renderer.transform.position = position;
            renderer.transform.rotation = rotation;
            renderer.transform.localScale = new Vector3(0.20f, 0.62f, 1);
            renderer.SetPropertyBlock(anchorProperties);
        }

        private void EmitShards()
        {
            ReadPlane(out Vector3 a, out Vector3 b, out Vector3 up, out Quaternion rotation);
            Vector3 across = (b - a).normalized;
            for (int i = 0; i < shards.Length; i++)
            {
                Shard shard = shards[i];
                shard.Position = Vector3.Lerp(a, b, (i + 0.5f) / shards.Length) + up * 0.30f;
                shard.Velocity = up * (0.9f + Next01() * 0.8f) + across * ((Next01() - 0.5f) * 0.5f);
                shard.Lifetime = 0.22f + Next01() * 0.10f;
                shard.Size = 0.035f + Next01() * 0.025f;
                shard.Angle = rotation.eulerAngles.z + (Next01() - 0.5f) * 70;
                shard.Age = 0;
                DrawShard(shard);
            }
        }

        private float Next01() => (float)cosmetic.NextDouble();

        private void AdvanceShards(float delta)
        {
            foreach (Shard shard in shards)
            {
                if (!enableShards) { shard.Age = 999; shard.Renderer.enabled = false; continue; }
                if (shard.Age >= shard.Lifetime) { shard.Renderer.enabled = false; continue; }
                // Do not advance newly emitted shards before their first render.
                if (deflectFrame != Time.frameCount)
                {
                    shard.Age += delta;
                    shard.Velocity += Vector3.down * (0.8f * delta);
                    shard.Position += shard.Velocity * delta;
                }
                if (shard.Age >= shard.Lifetime) shard.Renderer.enabled = false;
                else DrawShard(shard);
            }
        }

        private void DrawShard(Shard shard)
        {
            float t = Mathf.Clamp01(shard.Age / Mathf.Max(0.001f, shard.Lifetime));
            shard.Renderer.enabled = true;
            shard.Renderer.transform.position = shard.Position;
            shard.Renderer.transform.rotation = Quaternion.Euler(0, 0, shard.Angle);
            shard.Renderer.transform.localScale = new Vector3(shard.Size, shard.Size * 2.6f, 1) * (1 - t * 0.5f);
            shard.Properties.SetColor(TintId, new Color(1, 0.88f, 0.49f, 0.8f * (1 - t) * (1 - t)));
            shard.Renderer.SetPropertyBlock(shard.Properties);
        }

        private void HideWard()
        {
            if (ward != null) ward.enabled = false;
            if (leftAnchor != null) leftAnchor.enabled = false;
            if (rightAnchor != null) rightAnchor.enabled = false;
        }

        private void ClearVisuals()
        {
            HideWard();
            badge?.Hide();
            wasVisible = wasPulse = spent = false;
            wardAge = 0;
            deflectAge = 999;
            deflectFrame = -1;
            foreach (Shard shard in shards)
                if (shard != null) { shard.Age = 999; shard.Renderer.enabled = false; }
        }

        private void OnLost() => ResetRound();
        private void OnScore(int score) { if (score == 0) ResetRound(); }
        private void ResetRound() { ClearVisuals(); ignoreThroughFrame = Time.frameCount; }

        private void OwnSource()
        {
            if (ownsSource || source == null) return;
            originalHidden = source.forceRenderingOff;
            source.forceRenderingOff = true;
            ownsSource = true;
        }

        private void RestoreSource()
        {
            if (!ownsSource) return;
            if (source != null) source.forceRenderingOff = originalHidden;
            ownsSource = false;
        }

        private void OnDestroy()
        {
            GameEvents.BallLost -= OnLost;
            GameEvents.ScoreChanged -= OnScore;
            RestoreSource();
            badge?.Dispose();
            if (visualRoot != null) Destroy(visualRoot.gameObject);
            if (quad != null) Destroy(quad);
        }
    }
}
