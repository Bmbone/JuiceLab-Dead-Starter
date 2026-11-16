using UnityEngine;

namespace JuiceStudy.Arcade
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class ArcadeBall : MonoBehaviour
    {
        [SerializeField] private ArcadeSession session;
        [SerializeField] private float maxBounceAngle = 60f;
        [SerializeField] private float minVerticalRatio = 0.25f;
        private Rigidbody2D body;
        private Vector2 startPosition;
        private float speed;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            startPosition = transform.position;
            Hold();
        }

        public void Hold()
        {
            if (body == null) return;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0;
            body.simulated = false;
        }

        public void Launch(float selectedSpeed)
        {
            Hold();
            speed = selectedSpeed;
            transform.position = startPosition;
            body.position = startPosition;
            body.simulated = true;
            Vector2 direction = Quaternion.Euler(0, 0, Random.Range(-45f, 45f)) * Vector2.up;
            body.linearVelocity = direction * speed;
        }

        public void Lose() => session.LoseLife();

        private void FixedUpdate()
        {
            if (!session.IsPlaying || !body.simulated) return;
            Vector2 velocity = body.linearVelocity;
            if (velocity.sqrMagnitude < 0.0001f) return;
            Vector2 direction = velocity.normalized;
            if (Mathf.Abs(direction.y) < minVerticalRatio)
            {
                direction.y = (direction.y >= 0 ? 1 : -1) * minVerticalRatio;
                direction.Normalize();
            }
            body.linearVelocity = direction * speed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!session.IsPlaying || collision.contactCount == 0) return;
            ContactPoint2D contact = collision.GetContact(0);
            var impact = new ImpactInfo(contact.point, contact.normal);
            if (collision.gameObject.TryGetComponent(out Paddle paddle))
            {
                float offset = Mathf.Clamp((transform.position.x - paddle.transform.position.x) /
                    Mathf.Max(0.01f, paddle.Width * 0.5f), -1, 1);
                Vector2 direction = Quaternion.Euler(0, 0, -offset * maxBounceAngle) * Vector2.up;
                body.linearVelocity = direction * speed;
                session.HitPaddle();
                GameEvents.RaiseBallHitPaddle(impact);
            }
            else if (!collision.gameObject.TryGetComponent(out ArcadeBrick _))
            {
                GameEvents.RaiseBallHitWall(impact);
            }
        }

        private void OnDisable() => Hold();
    }
}
