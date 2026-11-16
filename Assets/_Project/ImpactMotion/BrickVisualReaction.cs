using UnityEngine;

namespace JuiceStudy.ImpactMotion
{
    /// <summary>Moves a child renderer only; the original sprite remains available to gameplay for tint/bounds.</summary>
    [DefaultExecutionOrder(100), DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class BrickVisualReaction : MonoBehaviour
    {
        private SpriteRenderer source;
        private SpriteRenderer proxy;
        private Collider2D hitbox;
        private ImpactMotionFeedback controls;
        private MaterialPropertyBlock properties;
        private Vector2 direction;
        private float magnitude;
        private float age = 999;
        private int spawnFrame = -1;
        private int resetVersion;
        private bool originalForceOff;

        private void Awake()
        {
            source = GetComponent<SpriteRenderer>();
            hitbox = GetComponent<Collider2D>();
            properties = new MaterialPropertyBlock();
            var child = new GameObject("Brick reaction visual - collider free");
            child.layer = gameObject.layer;
            child.transform.SetParent(transform, false);
            proxy = child.AddComponent<SpriteRenderer>();
            CopyAppearance();
        }

        private void OnEnable()
        {
            if (source == null || proxy == null) return;
            originalForceOff = source.forceRenderingOff;
            source.forceRenderingOff = true;
            proxy.gameObject.SetActive(true);
            ResetPose();
            GameEvents.BrickDestroyed += OnBrick;
        }

        private void Start()
        {
            // One lookup per spawned brick; every stage uses one active LevelSession/feedback hierarchy.
            controls = Object.FindFirstObjectByType<ImpactMotionFeedback>();
            if (controls != null) resetVersion = controls.ResetVersion;
        }

        private void OnDisable()
        {
            GameEvents.BrickDestroyed -= OnBrick;
            ResetPose();
            if (source != null) source.forceRenderingOff = originalForceOff;
            if (proxy != null) proxy.gameObject.SetActive(false);
        }

        private void OnBrick(ImpactInfo impact)
        {
            if (controls == null || !controls.BrickMotionEnabled || hitbox == null || !hitbox.enabled) return;
            SyncReset();
            Vector2 delta = (Vector2)transform.position - impact.Center;
            float distance = delta.magnitude;
            if (distance < 0.001f || distance >= controls.NeighborRadius) return;
            float falloff = Mathf.Sqrt(Mathf.Clamp01(1 - distance / controls.NeighborRadius));
            magnitude = falloff;
            direction = delta / distance;
            age = 0;
            spawnFrame = Time.frameCount;
        }

        private void LateUpdate()
        {
            if (proxy == null || source == null) return;
            // Runs after MedievalStoneSurface so copied property blocks always reflect the current root tint/size.
            CopyAppearance();
            if (controls == null || !controls.BrickMotionEnabled || hitbox == null || !hitbox.enabled)
            {
                ResetPose();
                return;
            }
            SyncReset();
            if (spawnFrame != Time.frameCount) age += Time.deltaTime;
            float t = Mathf.Clamp01(age / controls.NeighborDuration);
            if (t >= 1) { ResetPose(); return; }
            float envelope = (1 - t) * (1 - t);
            Vector2 sideways = new Vector2(-direction.y, direction.x);
            Vector2 oscillation = direction * Mathf.Cos(age * 80) + sideways * (0.35f * Mathf.Sin(age * 103));
            Vector2 worldOffset = Vector2.ClampMagnitude(oscillation, 1) *
                (controls.NeighborAmplitude * magnitude * envelope);
            proxy.transform.localPosition = transform.InverseTransformVector(worldOffset);
            float compression = controls.NeighborCompression * magnitude * envelope * Mathf.Sin(Mathf.PI * t);
            proxy.transform.localScale = Vector3.one * (1 - compression);
        }

        private void CopyAppearance()
        {
            proxy.sprite = source.sprite;
            proxy.sharedMaterial = source.sharedMaterial;
            proxy.color = source.color;
            proxy.flipX = source.flipX;
            proxy.flipY = source.flipY;
            proxy.sortingLayerID = source.sortingLayerID;
            proxy.sortingOrder = source.sortingOrder;
            proxy.maskInteraction = source.maskInteraction;
            proxy.enabled = source.enabled && !originalForceOff;
            source.GetPropertyBlock(properties);
            proxy.SetPropertyBlock(properties);
        }

        private void SyncReset()
        {
            if (resetVersion == controls.ResetVersion) return;
            resetVersion = controls.ResetVersion;
            ResetPose();
        }

        private void ResetPose()
        {
            age = 999;
            magnitude = 0;
            spawnFrame = -1;
            if (proxy == null) return;
            proxy.transform.localPosition = Vector3.zero;
            proxy.transform.localRotation = Quaternion.identity;
            proxy.transform.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            if (source != null) source.forceRenderingOff = originalForceOff;
            if (proxy != null) Destroy(proxy.gameObject);
        }
    }
}
