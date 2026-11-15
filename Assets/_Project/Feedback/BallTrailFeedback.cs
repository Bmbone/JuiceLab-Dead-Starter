using UnityEngine;

namespace JuiceStudy
{
    /// <summary>Short visual-only history, explicitly cleared after the gameplay relaunch.</summary>
    public sealed class BallTrailFeedback : MonoBehaviour
    {
        [SerializeField] private Transform ball;
        [SerializeField] private Material trailMaterial;
        [SerializeField, Range(0.03f, 0.25f)] private float trailDuration = 0.12f;
        [SerializeField, Range(0.02f, 0.4f)] private float width = 0.16f;
        [SerializeField] private Color color = new Color(0.45f, 0.9f, 1, 0.6f);
        private TrailRenderer trail;
        private bool clearAfterRelaunch;

        private void Awake()
        {
            if (ball == null || trailMaterial == null)
            {
                Debug.LogError("Assign Ball and the Feedback material to BallTrailFeedback.", this);
                enabled = false;
                return;
            }
            // Separate identity-scaled object avoids inheriting the ball's 0.5 scale.
            var visual = new GameObject("Ball motion trail");
            visual.transform.SetParent(transform, false);
            visual.transform.position = ball.position;
            trail = visual.AddComponent<TrailRenderer>();
            trail.sharedMaterial = trailMaterial;
            trail.time = trailDuration;
            trail.startWidth = width;
            trail.endWidth = 0;
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0);
            trail.minVertexDistance = 0.025f;
            trail.numCapVertices = 3;
            trail.sortingOrder = -1;
            trail.emitting = false;
        }

        private void OnEnable()
        {
            clearAfterRelaunch = true;
            GameEvents.BallLost += OnLost;
        }
        private void OnDisable()
        {
            GameEvents.BallLost -= OnLost;
            if (trail == null) return;
            trail.emitting = false;
            trail.Clear();
        }
        private void OnLost()
        {
            // BallLost is raised BEFORE Ball.Launch; wait until LateUpdate to reposition and clear.
            clearAfterRelaunch = true;
            if (trail != null) trail.emitting = false;
        }
        private void LateUpdate()
        {
            if (trail == null || ball == null) return;
            trail.transform.position = ball.position;
            trail.time = Mathf.Max(0.01f, trailDuration);
            trail.startWidth = Mathf.Max(0.01f, width);
            if (clearAfterRelaunch)
            {
                trail.Clear();
                clearAfterRelaunch = false;
            }
            trail.emitting = true;
        }
    }
}
