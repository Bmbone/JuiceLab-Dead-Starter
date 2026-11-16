using JuiceStudy.Levels;
using UnityEngine;

namespace JuiceStudy.FireballVfx
{
    /// <summary>Observes the existing reused projectile. This component owns presentation, never spell rules.</summary>
    [DefaultExecutionOrder(400), DisallowMultipleComponent]
    public sealed class FireballVisualFeedback : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private Transform abilityRoot;
        [SerializeField] private Material flameMaterial;
        [SerializeField] private Material glowMaterial;
        [SerializeField] private Material ribbonMaterial;
        [SerializeField] private Material ringMaterial;
        [SerializeField] private bool enableEmbers = true;
        [SerializeField, Range(0f, 1f)] private float emberOpacity = 0.70f;
        [SerializeField, Range(0f, 0.4f)] private float haloOpacity = 0.20f;
        private const int EmberCapacity = 16;
        private const float EmberInterval = 1f / 32;
        private sealed class Ember
        {
            public SpriteRenderer Renderer;
            public MaterialPropertyBlock Properties;
            public Vector2 Position;
            public Vector2 Velocity;
            public float Age = 999;
            public float Lifetime;
            public float Size;
            public float Rotation;
        }
        private readonly Ember[] embers = new Ember[EmberCapacity];
        private readonly System.Random cosmetic = new System.Random(40212);
        private Transform sourceActor;
        private Renderer[] originals;
        private bool[] originalHidden;
        private bool ownsSources;
        private Transform visualRoot;
        private Sprite quad;
        private SpriteRenderer flame;
        private SpriteRenderer halo;
        private SpriteRenderer ring;
        private TrailRenderer ribbon;
        private MaterialPropertyBlock flameProperties;
        private MaterialPropertyBlock haloProperties;
        private MaterialPropertyBlock ringProperties;
        private bool bound;
        private bool wasActive;
        private bool skipCastPulse;
        private int ignoreThroughFrame = -1;
        private int ringFrame = -1;
        private int nextEmber;
        private float flightAge;
        private float emissionAge;
        private float ringAge = 999;
        private Vector3 ringPosition;
        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int AgeId = Shader.PropertyToID("_Age");

        private void Awake()
        {
            if (session == null || abilityRoot == null || flameMaterial == null || glowMaterial == null ||
                ribbonMaterial == null || ringMaterial == null)
            {
                Debug.LogError("Assign the session, ability root and four FireballVfx materials.", this);
                enabled = false;
                return;
            }
            Texture2D white = Texture2D.whiteTexture;
            quad = Sprite.Create(white, new Rect(0, 0, white.width, white.height),
                new Vector2(0.5f, 0.5f), white.width, 0, SpriteMeshType.FullRect);
            quad.name = "Fireball VFX quad - owned";
            visualRoot = new GameObject("Fireball presentation - reused").transform;
            visualRoot.SetParent(transform, false);
            flame = MakeSprite("Flame head", flameMaterial, 7);
            halo = MakeSprite("Warm halo", glowMaterial, 1);
            ring = MakeSprite("Cast ring", ringMaterial, 2);
            flameProperties = new MaterialPropertyBlock();
            haloProperties = new MaterialPropertyBlock();
            ringProperties = new MaterialPropertyBlock();
            var trailObject = new GameObject("Soft fire ribbon");
            trailObject.transform.SetParent(visualRoot, false);
            ribbon = trailObject.AddComponent<TrailRenderer>();
            ribbon.sharedMaterial = ribbonMaterial;
            ribbon.time = 0.13f;
            ribbon.startWidth = 0.24f;
            ribbon.endWidth = 0;
            ribbon.startColor = new Color(1, 0.73f, 0.20f, 0.78f);
            ribbon.endColor = new Color(0.84f, 0.12f, 0.015f, 0);
            ribbon.minVertexDistance = 0.025f;
            ribbon.numCapVertices = 4;
            ribbon.numCornerVertices = 2;
            ribbon.textureMode = LineTextureMode.Stretch;
            ribbon.sortingOrder = 2;
            ribbon.emitting = false;
            for (int i = 0; i < embers.Length; i++)
                embers[i] = new Ember { Renderer = MakeSprite("Ember " + i, glowMaterial, 3), Properties = new MaterialPropertyBlock() };
            ClearVisuals();
        }

        private SpriteRenderer MakeSprite(string label, Material material, int order)
        {
            var child = new GameObject(label);
            child.transform.SetParent(visualRoot, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = quad;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void Start()
        {
            sourceActor = abilityRoot.Find("Fireball - reused");
            if (sourceActor == null)
            {
                Debug.LogError("The existing AbilityEffects fireball hierarchy was not found.", this);
                enabled = false;
                return;
            }
            originals = sourceActor.GetComponentsInChildren<Renderer>(true);
            originalHidden = new bool[originals.Length];
            bound = true;
            OwnSources();
            skipCastPulse = sourceActor.gameObject.activeInHierarchy;
        }

        private void OnEnable()
        {
            ClearVisuals();
            if (bound)
            {
                OwnSources();
                skipCastPulse = sourceActor != null && sourceActor.gameObject.activeInHierarchy;
            }
            GameEvents.BallLost += OnLost;
            GameEvents.ScoreChanged += OnScore;
        }

        private void OnDisable()
        {
            GameEvents.BallLost -= OnLost;
            GameEvents.ScoreChanged -= OnScore;
            ClearVisuals();
            RestoreSources();
        }

        private void LateUpdate()
        {
            if (!bound || sourceActor == null) return;
            if (!session.IsPlaying || Time.frameCount <= ignoreThroughFrame) { ClearVisuals(); return; }
            float dt = Mathf.Max(0, Time.deltaTime);
            bool active = sourceActor.gameObject.activeInHierarchy;
            if (active)
            {
                if (!wasActive)
                {
                    BeginFlight(!skipCastPulse);
                    skipCastPulse = false;
                }
                flightAge += dt;
                FollowSource();
            }
            else if (wasActive)
            {
                flame.enabled = halo.enabled = false;
                ribbon.emitting = false; // Normal ceiling expiry lets the short tail finish naturally.
            }
            AdvanceEmbers(dt);
            if (active && enableEmbers && dt > 0)
            {
                emissionAge += dt;
                int emitted = 0;
                while (emissionAge >= EmberInterval && emitted < 4)
                {
                    emissionAge -= EmberInterval;
                    EmitEmber();
                    emitted++;
                }
                if (emissionAge >= EmberInterval) emissionAge %= EmberInterval;
            }
            UpdateRing(dt);
            wasActive = active;
        }

        private void BeginFlight(bool castPulse)
        {
            flightAge = emissionAge = 0;
            ribbon.emitting = false;
            ribbon.transform.position = sourceActor.position;
            ribbon.Clear();
            ribbon.emitting = true;
            if (castPulse)
            {
                ringAge = 0;
                ringFrame = Time.frameCount;
                ringPosition = sourceActor.position;
            }
        }

        private void FollowSource()
        {
            Vector3 position = sourceActor.position;
            flame.enabled = halo.enabled = true;
            // UV head center is 0.67 in a 0.90-unit quad: this offset keeps it on the gameplay center.
            flame.transform.position = position + Vector3.down * 0.153f;
            flame.transform.localScale = new Vector3(0.60f, 0.90f, 1);
            flameProperties.SetFloat(AgeId, flightAge);
            flameProperties.SetColor(TintId, Color.white);
            flame.SetPropertyBlock(flameProperties);
            halo.transform.position = position;
            halo.transform.localScale = new Vector3(0.90f, 0.90f, 1);
            haloProperties.SetColor(TintId, new Color(1, 0.38f, 0.035f, Mathf.Clamp(haloOpacity, 0, 0.4f)));
            halo.SetPropertyBlock(haloProperties);
            ribbon.transform.position = position;
        }

        private void EmitEmber()
        {
            Ember ember = embers[nextEmber];
            nextEmber = (nextEmber + 1) % embers.Length;
            ember.Position = (Vector2)sourceActor.position + new Vector2((Next01() - 0.5f) * 0.14f, -0.13f);
            ember.Velocity = new Vector2((Next01() - 0.5f) * 0.6f, -0.55f - Next01() * 0.6f);
            ember.Lifetime = 0.16f + Next01() * 0.14f;
            ember.Size = 0.045f + Next01() * 0.03f;
            ember.Rotation = Next01() * 70 - 35;
            ember.Age = 0;
            DrawEmber(ember);
        }

        private float Next01() => (float)cosmetic.NextDouble();

        private void AdvanceEmbers(float delta)
        {
            foreach (Ember ember in embers)
            {
                if (!enableEmbers) { ember.Renderer.enabled = false; ember.Age = 999; continue; }
                ember.Age += delta;
                if (ember.Age >= ember.Lifetime) { ember.Renderer.enabled = false; continue; }
                ember.Velocity += Vector2.down * (0.8f * delta);
                ember.Position += ember.Velocity * delta;
                DrawEmber(ember);
            }
        }

        private void DrawEmber(Ember ember)
        {
            float t = Mathf.Clamp01(ember.Age / Mathf.Max(0.001f, ember.Lifetime));
            ember.Renderer.enabled = true;
            ember.Renderer.transform.position = ember.Position;
            ember.Renderer.transform.localRotation = Quaternion.Euler(0, 0, ember.Rotation);
            ember.Renderer.transform.localScale = new Vector3(ember.Size, ember.Size * 1.8f, 1) * (1 - t * 0.45f);
            Color tint = Color.Lerp(new Color(1, 0.83f, 0.27f), new Color(0.92f, 0.18f, 0.025f), t);
            tint.a = Mathf.Clamp01(emberOpacity) * (1 - t) * (1 - t);
            ember.Properties.SetColor(TintId, tint);
            ember.Renderer.SetPropertyBlock(ember.Properties);
        }

        private void UpdateRing(float delta)
        {
            if (ringFrame != Time.frameCount) ringAge += delta;
            float t = Mathf.Clamp01(ringAge / 0.16f);
            ring.enabled = t < 1;
            if (!ring.enabled) return;
            ring.transform.position = ringPosition;
            float size = Mathf.Lerp(0.14f, 0.84f, 1 - (1 - t) * (1 - t));
            ring.transform.localScale = new Vector3(size, size, 1);
            ringProperties.SetColor(TintId, new Color(1, 0.79f, 0.29f, 0.6f * (1 - t) * (1 - t)));
            ring.SetPropertyBlock(ringProperties);
        }

        private void OnLost() => ResetRound();
        private void OnScore(int score) { if (score == 0) ResetRound(); }
        private void ResetRound() { ClearVisuals(); skipCastPulse = false; ignoreThroughFrame = Time.frameCount; }

        private void ClearVisuals()
        {
            wasActive = false;
            flightAge = emissionAge = 0;
            nextEmber = 0;
            ringAge = 999;
            ringFrame = -1;
            if (flame != null) flame.enabled = false;
            if (halo != null) halo.enabled = false;
            if (ring != null) ring.enabled = false;
            if (ribbon != null) { ribbon.emitting = false; ribbon.Clear(); }
            foreach (Ember ember in embers)
                if (ember != null) { ember.Age = 999; ember.Renderer.enabled = false; }
        }

        private void OwnSources()
        {
            if (ownsSources || originals == null) return;
            for (int i = 0; i < originals.Length; i++)
            {
                originalHidden[i] = originals[i].forceRenderingOff;
                originals[i].forceRenderingOff = true;
            }
            ownsSources = true;
        }

        private void RestoreSources()
        {
            if (!ownsSources) return;
            for (int i = 0; i < originals.Length; i++)
                if (originals[i] != null) originals[i].forceRenderingOff = originalHidden[i];
            ownsSources = false;
        }

        private void OnDestroy()
        {
            GameEvents.BallLost -= OnLost;
            GameEvents.ScoreChanged -= OnScore;
            RestoreSources();
            if (visualRoot != null) Destroy(visualRoot.gameObject);
            if (quad != null) Destroy(quad); // The built-in white texture and material assets are not owned here.
        }
    }
}
