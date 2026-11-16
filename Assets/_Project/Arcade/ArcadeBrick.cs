using UnityEngine;

namespace JuiceStudy.Arcade
{
    public sealed class ArcadeBrick : MonoBehaviour
    {
        [SerializeField] private int hitPoints = 1;
        [SerializeField] private int scoreValue = 10;
        private ArcadeSession session;
        private SpriteRenderer visual;
        private bool destroyed;

        private void Awake() => visual = GetComponent<SpriteRenderer>();
        public void Initialize(ArcadeSession owner) => session = owner;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (destroyed || session == null || !session.IsPlaying || collision.contactCount == 0 ||
                !collision.gameObject.TryGetComponent(out ArcadeBall _)) return;
            if (--hitPoints > 0) return;

            // Disable immediately: Unity defers Destroy until the end of the frame.
            destroyed = true;
            GetComponent<Collider2D>().enabled = false;
            ContactPoint2D contact = collision.GetContact(0);
            Color color = visual != null ? visual.color : Color.white;
            Vector2 size = visual != null ? (Vector2)visual.bounds.size : Vector2.one;
            session.BreakBrick(scoreValue, contact.point, contact.normal, color, transform.position, size);
            Destroy(gameObject);
        }
    }
}
