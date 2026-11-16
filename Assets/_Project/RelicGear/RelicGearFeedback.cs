using JuiceStudy.Levels;
using UnityEngine;

namespace JuiceStudy.RelicGear
{
    /// <summary>Skins existing visual children while preserving original sprites/materials used by abilities.</summary>
    [DefaultExecutionOrder(150), DisallowMultipleComponent]
    public sealed class RelicGearFeedback : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private SpriteRenderer ballVisual;
        [SerializeField] private SpriteRenderer paddleVisual;
        [SerializeField] private Material orbMaterial;
        [SerializeField] private Material paddleMaterial;
        [SerializeField] private Material haloMaterial;
        [SerializeField] private Material contactMaterial;
        [SerializeField] private bool enableContactFeedback = true;
        [SerializeField, Range(0f, 1f)] private float contactStrength = 1;
        [SerializeField, Range(0f, 0.16f)] private float ballSquash = 0.10f;
        [SerializeField, Range(0.06f, 0.3f)] private float contactDuration = 0.14f;
        [SerializeField, Range(0f, 0.5f)] private float haloOpacity = 0.24f;

        private sealed class Contact
        {
            public SpriteRenderer Renderer;
            public MaterialPropertyBlock Properties;
            public Vector2 Position;
            public float Age = 999;
            public int Frame = -1;
        }
        private readonly Contact[] contacts = new Contact[4];
        private SpriteRenderer orb;
        private SpriteRenderer paddle;
        private SpriteRenderer halo;
        private Transform contactRoot;
        private MaterialPropertyBlock orbProperties;
        private MaterialPropertyBlock paddleProperties;
        private MaterialPropertyBlock haloProperties;
        private float ballAge = 999;
        private float paddleAge = 999;
        private float ballStrength;
        private Vector2 ballNormal = Vector2.up;
        private int ballFrame = -1;
        private int paddleFrame = -1;
        private int nextContact;
        private bool ownsSources;
        private bool ballWasHidden;
        private bool paddleWasHidden;
        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int RectId = Shader.PropertyToID("_SpriteRect");
        private static readonly int SizeId = Shader.PropertyToID("_Size");
        private static readonly int ImpactId = Shader.PropertyToID("_Impact");
        private static readonly int EmpoweredId = Shader.PropertyToID("_Empowered");

        private void Awake()
        {
            if (session == null || ballVisual == null || paddleVisual == null ||
                orbMaterial == null || paddleMaterial == null || haloMaterial == null || contactMaterial == null)
            {
                Debug.LogError("Assign the session, original visuals and four materials to RelicGearFeedback.", this);
                enabled = false;
                return;
            }
            orbProperties = new MaterialPropertyBlock();
            paddleProperties = new MaterialPropertyBlock();
            haloProperties = new MaterialPropertyBlock();
            orb = MakeVisual("Enchanted orb - visual only", ballVisual.transform, ballVisual.sprite, orbMaterial, 4);
            paddle = MakeVisual("Wood and brass paddle - visual only", paddleVisual.transform, paddleVisual.sprite, paddleMaterial, 0);
            halo = MakeVisual("Orb halo - visual only", ballVisual.transform, ballVisual.sprite, haloMaterial, -2);
            contactRoot = new GameObject("Contact rings - reused").transform;
            contactRoot.SetParent(transform, false);
            for (int i = 0; i < contacts.Length; i++)
            {
                contacts[i] = new Contact
                {
                    Renderer = MakeVisual("Contact ring " + i, contactRoot, ballVisual.sprite, contactMaterial, 2),
                    Properties = new MaterialPropertyBlock()
                };
            }
            ResetFeedback();
        }

        private void OnEnable()
        {
            if (orb == null) return;
            ballWasHidden = ballVisual.forceRenderingOff;
            paddleWasHidden = paddleVisual.forceRenderingOff;
            ownsSources = true;
            ballVisual.forceRenderingOff = true;
            paddleVisual.forceRenderingOff = true;
            orb.gameObject.SetActive(true);
            paddle.gameObject.SetActive(true);
            halo.gameObject.SetActive(true);
            ResetFeedback();
            GameEvents.BallHitPaddle += OnPaddle;
            GameEvents.BallHitWall += OnWall;
            GameEvents.BallLost += OnLost;
            GameEvents.ScoreChanged += OnScore;
        }

        private void OnDisable()
        {
            GameEvents.BallHitPaddle -= OnPaddle;
            GameEvents.BallHitWall -= OnWall;
            GameEvents.BallLost -= OnLost;
            GameEvents.ScoreChanged -= OnScore;
            ResetFeedback();
            RestoreSources();
            if (orb != null) orb.gameObject.SetActive(false);
            if (paddle != null) paddle.gameObject.SetActive(false);
            if (halo != null) halo.gameObject.SetActive(false);
        }

        private void OnPaddle(ImpactInfo impact)
        {
            if (!FeedbackActive) return;
            PulseBall(impact.Normal, 1);
            paddleAge = 0;
            paddleFrame = Time.frameCount;
            Contact ring = contacts[nextContact];
            nextContact = (nextContact + 1) % contacts.Length;
            ring.Position = impact.Point;
            ring.Age = 0;
            ring.Frame = Time.frameCount;
            ring.Renderer.enabled = true;
        }

        private void OnWall(ImpactInfo impact)
        {
            if (FeedbackActive) PulseBall(impact.Normal, 0.55f);
        }

        // The observer on Ball reports real collider contacts; remote spell breaks do not pulse this orb.
        public void NotifyBrickContact(Vector2 normal)
        {
            if (FeedbackActive) PulseBall(normal, 0.8f);
        }

        private bool FeedbackActive => isActiveAndEnabled && enableContactFeedback && contactStrength > 0 && session != null && session.IsPlaying;

        private void PulseBall(Vector2 normal, float strength)
        {
            ballAge = 0;
            ballFrame = Time.frameCount;
            ballStrength = Mathf.Clamp01(strength);
            ballNormal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector2.up;
        }

        private void OnLost() => ResetFeedback();
        private void OnScore(int score) { if (score == 0) ResetFeedback(); }

        private void LateUpdate()
        {
            if (orb == null) return;
            if (!FeedbackActive) ResetFeedback();
            if (ballFrame != Time.frameCount) ballAge += Time.deltaTime;
            if (paddleFrame != Time.frameCount) paddleAge += Time.deltaTime;
            float strength = Mathf.Clamp01(contactStrength);
            float ballPulse = Decay(ballAge) * ballStrength * strength;
            float paddlePulse = Decay(paddleAge) * strength;
            UpdateSurface(orb, ballVisual, orbProperties, ballPulse, 0);
            float powered = paddleVisual.color.g - Mathf.Max(paddleVisual.color.r, paddleVisual.color.b) > 0.12f ? 1 : 0;
            UpdateSurface(paddle, paddleVisual, paddleProperties, paddlePulse, powered);

            float axis = ballNormal.y * ballNormal.y - ballNormal.x * ballNormal.x;
            float amount = Mathf.Clamp(ballSquash, 0, 0.16f) * ballPulse;
            orb.transform.localScale = new Vector3(1 + axis * amount, 1 - axis * amount, 1);
            UpdateSurface(halo, ballVisual, haloProperties, 0, 0);
            Color glow = new Color(1, 0.80f, 0.39f, Mathf.Clamp(haloOpacity, 0, 0.5f) * (1 + ballPulse * 0.6f));
            haloProperties.SetColor(TintId, glow);
            halo.SetPropertyBlock(haloProperties);
            halo.transform.localScale = Vector3.one * (1.6f + ballPulse * 0.18f);
            for (int i = 0; i < contacts.Length; i++) UpdateContact(contacts[i], strength);
        }

        private float Decay(float age)
        {
            float t = Mathf.Clamp01(age / Mathf.Max(0.01f, contactDuration));
            return (1 - t) * (1 - t);
        }

        private void UpdateSurface(SpriteRenderer target, SpriteRenderer source, MaterialPropertyBlock properties, float impact, float empowered)
        {
            target.sprite = source.sprite;
            target.flipX = source.flipX;
            target.flipY = source.flipY;
            target.sortingLayerID = source.sortingLayerID;
            target.enabled = source.enabled && (source != ballVisual || !ballWasHidden) && (source != paddleVisual || !paddleWasHidden);
            Bounds bounds = source.sprite.bounds;
            Vector3 scale = source.transform.lossyScale;
            properties.SetVector(RectId, new Vector4(bounds.min.x, bounds.min.y,
                1 / Mathf.Max(bounds.size.x, 0.001f), 1 / Mathf.Max(bounds.size.y, 0.001f)));
            properties.SetVector(SizeId, new Vector4(Mathf.Abs(bounds.size.x * scale.x), Mathf.Abs(bounds.size.y * scale.y), 0, 0));
            properties.SetColor(TintId, source.color);
            properties.SetFloat(ImpactId, impact);
            properties.SetFloat(EmpoweredId, empowered);
            target.SetPropertyBlock(properties);
        }

        private void UpdateContact(Contact ring, float strength)
        {
            if (ring == null) return;
            if (ring.Frame != Time.frameCount) ring.Age += Time.deltaTime;
            float t = Mathf.Clamp01(ring.Age / 0.16f);
            ring.Renderer.enabled = FeedbackActive && t < 1;
            if (!ring.Renderer.enabled) return;
            Bounds bounds = ballVisual.sprite.bounds;
            float diameter = Mathf.Lerp(0.10f, 0.72f, 1 - (1 - t) * (1 - t));
            ring.Renderer.transform.position = ring.Position;
            ring.Renderer.transform.localScale = new Vector3(diameter / bounds.size.x, diameter / bounds.size.y, 1);
            ring.Properties.SetVector(RectId, new Vector4(bounds.min.x, bounds.min.y,
                1 / Mathf.Max(bounds.size.x, 0.001f), 1 / Mathf.Max(bounds.size.y, 0.001f)));
            ring.Properties.SetColor(TintId, new Color(1, 0.84f, 0.46f, 0.60f * (1 - t) * (1 - t) * strength));
            ring.Renderer.SetPropertyBlock(ring.Properties);
        }

        private static SpriteRenderer MakeVisual(string name, Transform parent, Sprite sprite, Material material, int order)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void ResetFeedback()
        {
            ballAge = paddleAge = 999;
            ballFrame = paddleFrame = -1;
            ballStrength = 0;
            nextContact = 0;
            if (orb != null) orb.transform.localScale = Vector3.one;
            foreach (Contact ring in contacts)
            {
                if (ring == null) continue;
                ring.Age = 999;
                ring.Frame = -1;
                ring.Renderer.enabled = false;
            }
        }

        private void RestoreSources()
        {
            if (!ownsSources) return;
            if (ballVisual != null) ballVisual.forceRenderingOff = ballWasHidden;
            if (paddleVisual != null) paddleVisual.forceRenderingOff = paddleWasHidden;
            ownsSources = false;
        }

        private void OnDestroy()
        {
            RestoreSources();
            if (orb != null) Destroy(orb.gameObject);
            if (paddle != null) Destroy(paddle.gameObject);
            if (halo != null) Destroy(halo.gameObject);
            if (contactRoot != null) Destroy(contactRoot.gameObject);
        }
    }
}
