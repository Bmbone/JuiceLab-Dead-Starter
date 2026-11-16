using JuiceStudy.Abilities;
using TMPro;
using UnityEngine;

namespace JuiceStudy.Levels
{
    public sealed class LevelAbilityController : MonoBehaviour
    {
        private const float ShieldY = -5.15f;
        private const float FireballRadius = 0.22f;
        private const float FireballSpeed = 16f;
        private const float FireballCeiling = 4.7f;
        [SerializeField] private LevelSession session;
        [SerializeField] private AbilityPaddle paddle;
        [SerializeField] private SpriteRenderer ballVisual;
        [SerializeField] private SpriteRenderer paddleVisual;
        [SerializeField] private RectTransform hudParent;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private Material effectMaterial;
        private readonly AbilityState state = new AbilityState();
        private readonly RaycastHit2D[] hits = new RaycastHit2D[64];
        private ContactFilter2D filter;
        private AbilityEffects effects;
        private AbilityHud hud;
        private Color restColor;
        private Vector2 fireballPosition;
        private float fireballLifetime;
        private bool fireballActive;
        private int firstPlayFrame;

        private void Awake()
        {
            restColor = paddleVisual.color;
            filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.DefaultRaycastLayers);
            filter.useTriggers = false;
            effects = new AbilityEffects(transform, ballVisual, effectMaterial, ShieldY);
            hud = new AbilityHud(hudParent, font, Cast);
        }

        public void BeginRound()
        {
            EndRound();
            state.Reset(true);
            firstPlayFrame = Time.frameCount;
            Refresh();
        }

        public void EndRound()
        {
            state.Reset(false);
            fireballActive = false;
            if (effects == null) return;
            effects.Reset();
            Refresh();
        }

        public void OnLifeLost()
        {
            // A lost life ends temporary effects, but does not refill any cooldown.
            state.CancelEffects();
            fireballActive = false;
            effects.StopFireball();
            Refresh();
        }

        public void Cast(Spell spell)
        {
            if (!session.IsPlaying || !state.TryCast(spell)) return;
            if (spell == Spell.Fireball)
            {
                float top = paddle.GetComponent<BoxCollider2D>().bounds.max.y;
                fireballPosition = new Vector2(paddle.transform.position.x, top + FireballRadius + 0.05f);
                fireballLifetime = 1.2f;
                fireballActive = true;
                effects.StartFireball(fireballPosition);
            }
            effects.PlayCue((int)spell);
            Refresh();
        }

        private void Update()
        {
            if (!session.IsPlaying) return;
            state.Tick(Time.deltaTime);
            if (Time.frameCount > firstPlayFrame)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Space)) Cast(Spell.Fireball);
                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Cast(Spell.WidePaddle);
                if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) Cast(Spell.RuneShield);
            }
            Refresh();
            effects.Tick(Time.deltaTime, state.ActiveRemaining(Spell.RuneShield) > 0);
        }

        private void Refresh()
        {
            bool wide = state.ActiveRemaining(Spell.WidePaddle) > 0;
            paddle.SetExpanded(wide);
            paddleVisual.color = wide ? new Color(0.4f, 1, 0.68f) : restColor;
            hud.Refresh(state);
        }

        private void FixedUpdate()
        {
            if (!session.IsPlaying || !fireballActive) return;
            float distance = Mathf.Min(FireballSpeed * Time.fixedDeltaTime, FireballCeiling - fireballPosition.y);
            if (distance <= 0) { StopFireball(); return; }
            // Sweep the whole movement segment; the projectile cannot tunnel through thin bricks.
            int count = Physics2D.CircleCast(fireballPosition, FireballRadius, Vector2.up, filter, hits, distance);
            for (int i = 0; i < count && session.IsPlaying; i++)
            {
                if (hits[i].collider != null && hits[i].collider.TryGetComponent(out LevelBrick brick))
                    brick.TakeHit(hits[i].point, hits[i].normal);
            }
            // The final brick may have ended the round and cleared this projectile during TakeHit.
            if (!session.IsPlaying || !fireballActive) return;
            fireballPosition += Vector2.up * distance;
            fireballLifetime -= Time.fixedDeltaTime;
            effects.MoveFireball(fireballPosition);
            if (fireballPosition.y >= FireballCeiling || fireballLifetime <= 0) StopFireball();
        }

        private void StopFireball()
        {
            fireballActive = false;
            effects.StopFireball();
        }

        public bool TryShieldBounce(Vector2 current, Vector2 next, float radius, out Vector2 contact)
        {
            contact = current;
            if (!session.IsPlaying || !state.TryBlock(current.y - radius, next.y - radius, ShieldY)) return false;
            float fraction = Mathf.Clamp01((current.y - radius - ShieldY) / (current.y - next.y));
            contact = new Vector2(Mathf.Lerp(current.x, next.x, fraction), ShieldY + radius + 0.01f);
            effects.ShieldHit();
            return true;
        }

        private void OnDisable() => EndRound();
        private void OnDestroy() => effects?.Dispose();
    }
}
