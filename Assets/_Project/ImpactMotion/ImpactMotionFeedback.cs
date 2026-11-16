using JuiceStudy.Levels;
using UnityEngine;
using UnityEngine.Rendering;

namespace JuiceStudy.ImpactMotion
{
    /// <summary>Bounded render-only camera motion and shared controls for collider-free brick visuals.</summary>
    [DefaultExecutionOrder(-200), DisallowMultipleComponent]
    public sealed class ImpactMotionFeedback : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private LevelSession session;
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private bool enableBrickReactions = true;
        [SerializeField, Range(0f, 1f)] private float motionStrength = 1f;
        [SerializeField, Range(0f, 0.08f)] private float brickShakeAmplitude = 0.018f;
        [SerializeField, Range(0.04f, 0.25f)] private float brickShakeDuration = 0.11f;
        [SerializeField, Range(0f, 0.18f)] private float lifeLossAmplitude = 0.075f;
        [SerializeField, Range(0.1f, 0.5f)] private float lifeLossDuration = 0.26f;
        [SerializeField, Range(1f, 4f)] private float neighborRadius = 2.6f;
        [SerializeField, Range(0f, 0.05f)] private float neighborAmplitude = 0.035f;
        [SerializeField, Range(0.08f, 0.4f)] private float neighborDuration = 0.20f;
        [SerializeField, Range(0f, 0.04f)] private float neighborCompression = 0.016f;

        public bool BrickMotionEnabled => isActiveAndEnabled && enableBrickReactions && motionStrength > 0 &&
            session != null && session.IsPlaying;
        public float NeighborRadius => Mathf.Max(0.01f, neighborRadius);
        public float NeighborAmplitude => Mathf.Clamp(neighborAmplitude, 0, 0.05f) * Mathf.Clamp01(motionStrength);
        public float NeighborDuration => Mathf.Max(0.01f, neighborDuration);
        public float NeighborCompression => Mathf.Clamp(neighborCompression, 0, 0.04f) * Mathf.Clamp01(motionStrength);
        public int ResetVersion { get; private set; }
        private bool CameraMotionEnabled => isActiveAndEnabled && enableCameraShake && motionStrength > 0;
        private float CameraLimit => Mathf.Max(Mathf.Clamp(brickShakeAmplitude, 0, 0.08f),
            Mathf.Clamp(lifeLossAmplitude, 0, 0.18f)) * Mathf.Clamp01(motionStrength);
        private float brickAge = 999;
        private float lifeAge = 999;
        private float brickPhase;
        private int brickFrame = -1;
        private int lifeFrame = -1;
        private bool viewOverridden;
        private bool wasPlaying;

        private void Awake()
        {
            if (worldCamera == null || session == null)
            {
                Debug.LogError("Assign the camera and LevelSession to ImpactMotionFeedback.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (worldCamera == null || session == null) return;
            ResetMotion();
            GameEvents.BrickDestroyed += OnBrick;
            GameEvents.BallLost += OnBallLost;
            GameEvents.ScoreChanged += OnScore;
            RenderPipelineManager.beginCameraRendering += BeginCamera;
            RenderPipelineManager.endCameraRendering += EndCamera;
        }

        private void OnDisable()
        {
            GameEvents.BrickDestroyed -= OnBrick;
            GameEvents.BallLost -= OnBallLost;
            GameEvents.ScoreChanged -= OnScore;
            RenderPipelineManager.beginCameraRendering -= BeginCamera;
            RenderPipelineManager.endCameraRendering -= EndCamera;
            ResetMotion();
            FitBackdrop(false);
        }

        private void OnBrick(ImpactInfo impact)
        {
            if (!CameraMotionEnabled || !session.IsPlaying) return;
            brickAge = 0;
            brickFrame = Time.frameCount;
            brickPhase = impact.Center.x * 0.73f + impact.Center.y * 0.41f;
        }

        private void OnBallLost()
        {
            // Clear neighboring brick motion before the new launch; final results retain their existing timing.
            ResetVersion = unchecked(ResetVersion + 1);
            if (!CameraMotionEnabled || !session.IsPlaying) return;
            lifeAge = 0;
            lifeFrame = Time.frameCount;
        }

        private void OnScore(int score)
        {
            if (score == 0) ResetMotion();
        }

        private void FixedUpdate() => RestoreView();

        private void Update()
        {
            // A fallback restore runs before the existing mouse-input Update, even if rendering was interrupted.
            RestoreView();
            bool playing = session != null && session.IsPlaying;
            if (!playing)
            {
                if (wasPlaying) ResetMotion();
                brickAge = lifeAge = 999;
            }
            else if (!CameraMotionEnabled) brickAge = lifeAge = 999;
            else
            {
                if (brickFrame != Time.frameCount) brickAge += Time.deltaTime;
                if (lifeFrame != Time.frameCount) lifeAge += Time.deltaTime;
            }
            wasPlaying = playing;
        }

        private Vector2 Offset()
        {
            float brickEnvelope = Envelope(brickAge, brickShakeDuration);
            float lossEnvelope = Envelope(lifeAge, lifeLossDuration);
            Vector2 brick = new Vector2(Mathf.Cos(brickAge * 91 + brickPhase),
                0.65f * Mathf.Sin(brickAge * 117 + brickPhase + 0.5f));
            Vector2 loss = new Vector2(0.7f * Mathf.Sin(lifeAge * 81), -0.85f * Mathf.Cos(lifeAge * 69));
            brick = Vector2.ClampMagnitude(brick, 1) * (Mathf.Clamp(brickShakeAmplitude, 0, 0.08f) * brickEnvelope);
            loss = Vector2.ClampMagnitude(loss, 1) * (Mathf.Clamp(lifeLossAmplitude, 0, 0.18f) * lossEnvelope);
            // Repeated events refresh a channel; the sum never exceeds the configured strongest pulse.
            return Vector2.ClampMagnitude((brick + loss) * Mathf.Clamp01(motionStrength), CameraLimit);
        }

        private static float Envelope(float age, float duration)
        {
            float t = Mathf.Clamp01(age / Mathf.Max(0.01f, duration));
            return (1 - t) * (1 - t);
        }

        private void BeginCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera != worldCamera) return;
            RestoreView();
            FitBackdrop(CameraMotionEnabled);
            if (!CameraMotionEnabled || session == null || !session.IsPlaying) return;
            Vector2 offset = Offset();
            if (offset.sqrMagnitude < 0.00000001f) return;
            // Only the render pass sees this matrix. The Camera transform, input mapping and physics stay fixed.
            Matrix4x4 view = worldCamera.worldToCameraMatrix;
            worldCamera.worldToCameraMatrix = Matrix4x4.Translate(new Vector3(offset.x, offset.y, 0)) * view;
            viewOverridden = true;
        }

        private void EndCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera == worldCamera) RestoreView();
        }

        private void RestoreView()
        {
            if (!viewOverridden) return;
            if (worldCamera != null) worldCamera.ResetWorldToCameraMatrix();
            viewOverridden = false;
        }

        private void ResetMotion()
        {
            RestoreView();
            brickAge = lifeAge = 999;
            brickFrame = lifeFrame = -1;
            ResetVersion = unchecked(ResetVersion + 1);
        }

        private void FitBackdrop(bool overscan)
        {
            if (worldCamera == null || !worldCamera.orthographic || background == null || background.sprite == null) return;
            Bounds bounds = background.sprite.bounds;
            float height = worldCamera.orthographicSize * 2;
            float width = height * worldCamera.aspect;
            float scale = Mathf.Max(height / bounds.size.y, width / bounds.size.x);
            // Use a constant margin based on configured strength, so the background does not zoom on each impact.
            if (overscan) scale *= 1 + 2 * (CameraLimit + 0.025f) / Mathf.Max(0.01f, Mathf.Min(width, height));
            background.transform.localScale = new Vector3(scale, scale, 1);
            background.transform.position = new Vector3(worldCamera.transform.position.x - bounds.center.x * scale,
                worldCamera.transform.position.y - bounds.center.y * scale, 5);
        }
    }
}
