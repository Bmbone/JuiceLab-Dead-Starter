using JuiceStudy.Abilities;
using JuiceStudy.Arcade;
using UnityEngine;

namespace JuiceStudy.Levels
{
    public sealed class LevelBrick : MonoBehaviour
    {
        [SerializeField] private int hitPoints = 1;
        [SerializeField] private int scoreValue = 10;
        private LevelSession session;
        private SpriteRenderer visual;
        private BrickHealth health;

        private void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
            health = new BrickHealth(hitPoints);
        }
        public void Initialize(LevelSession owner) => session = owner;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (session == null || !session.IsPlaying || collision.contactCount == 0 ||
                !collision.gameObject.TryGetComponent(out LevelBall _)) return;
            ContactPoint2D contact = collision.GetContact(0);
            TakeHit(contact.point, contact.normal);
        }

        public bool TakeHit(Vector2 point, Vector2 normal)
        {
            if (session == null || !session.IsPlaying || !health.TryDestroyWithHit()) return false;
            // Disable immediately: Unity defers Destroy until the end of the frame.
            GetComponent<Collider2D>().enabled = false;
            Color color = visual != null ? visual.color : Color.white;
            Vector2 size = visual != null ? (Vector2)visual.bounds.size : Vector2.one;
            session.BreakBrick(scoreValue, point, normal, color, transform.position, size);
            Destroy(gameObject);
            return true;
        }
    }
}
