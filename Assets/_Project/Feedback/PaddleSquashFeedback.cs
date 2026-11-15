using UnityEngine;

namespace JuiceStudy
{
    public sealed class PaddleSquashFeedback : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField, Range(0.02f, 0.35f)] private float squashAmount = 0.18f;
        [SerializeField, Range(0.08f, 0.5f)] private float duration = 0.22f;
        private Vector3 restScale;
        private float elapsed;
        private bool reacting;

        private void Awake()
        {
            if (visual == null || visual.GetComponentsInChildren<Collider2D>(true).Length > 0 ||
                visual.GetComponent<Rigidbody2D>() != null)
            {
                Debug.LogError("Assign only Paddle/Visual, never a physics transform.", this);
                enabled = false;
                return;
            }
            restScale = visual.localScale;
        }

        private void OnEnable() => GameEvents.BallHitPaddle += OnHit;
        private void OnDisable()
        {
            GameEvents.BallHitPaddle -= OnHit;
            reacting = false;
            if (visual != null && restScale != Vector3.zero) visual.localScale = restScale;
        }

        private void OnHit(ImpactInfo _)
        {
            elapsed = 0;
            reacting = true;
            ApplySquash(squashAmount);
        }

        private void Update()
        {
            if (!reacting) return;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
            // Immediate compression, gentle rebound, exact return; rapid hits restart, not multiply.
            float amount = squashAmount * Mathf.Cos(t * Mathf.PI * 1.5f) * (1 - t) * (1 - t);
            ApplySquash(amount);
            if (t >= 1)
            {
                visual.localScale = restScale;
                reacting = false;
            }
        }

        private void ApplySquash(float amount) =>
            visual.localScale = Vector3.Scale(restScale, new Vector3(1 + amount, 1 - amount, 1));
    }
}
