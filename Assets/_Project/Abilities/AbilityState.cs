using System;

namespace JuiceStudy.Abilities
{
    public enum Spell { Fireball, WidePaddle, RuneShield }

    /// <summary>Timers and one-shot shield rules, independent of Unity and tested directly.</summary>
    public sealed class AbilityState
    {
        private readonly float[] cooldown = new float[3];
        private readonly float[] active = new float[3];
        public bool Enabled { get; private set; }

        public static float CooldownLength(Spell spell)
        {
            switch (spell)
            {
                case Spell.Fireball: return 5;
                case Spell.WidePaddle: return 12;
                case Spell.RuneShield: return 14;
                default: throw new ArgumentOutOfRangeException(nameof(spell));
            }
        }

        public static float EffectLength(Spell spell)
        {
            switch (spell)
            {
                case Spell.Fireball: return 0;
                case Spell.WidePaddle: return 6;
                case Spell.RuneShield: return 8;
                default: throw new ArgumentOutOfRangeException(nameof(spell));
            }
        }

        private static int Index(Spell spell)
        {
            int index = (int)spell;
            if (index < 0 || index >= 3) throw new ArgumentOutOfRangeException(nameof(spell));
            return index;
        }

        public float CooldownRemaining(Spell spell) => cooldown[Index(spell)];
        public float ActiveRemaining(Spell spell) => active[Index(spell)];
        public bool IsReady(Spell spell) => Enabled && CooldownRemaining(spell) <= 0;

        public bool TryCast(Spell spell)
        {
            int index = Index(spell);
            if (!IsReady(spell)) return false;
            cooldown[index] = CooldownLength(spell);
            active[index] = EffectLength(spell);
            return true;
        }

        public void Tick(float seconds)
        {
            if (seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds))
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (!Enabled) return;
            for (int i = 0; i < cooldown.Length; i++)
            {
                cooldown[i] = Math.Max(0, cooldown[i] - seconds);
                active[i] = Math.Max(0, active[i] - seconds);
            }
        }

        public bool TryBlock(float currentBottom, float nextBottom, float shieldY)
        {
            if (!Enabled || ActiveRemaining(Spell.RuneShield) <= 0 || nextBottom >= currentBottom ||
                currentBottom < shieldY || nextBottom > shieldY) return false;
            active[Index(Spell.RuneShield)] = 0;
            return true;
        }

        public void CancelEffects() => Array.Clear(active, 0, active.Length);

        public void Reset(bool enabled)
        {
            Enabled = enabled;
            Array.Clear(cooldown, 0, cooldown.Length);
            CancelEffects();
        }
    }

    /// <summary>A ball and fireball hitting the same brick can award its destruction only once.</summary>
    public sealed class BrickHealth
    {
        private int remaining;

        public BrickHealth(int hitPoints)
        {
            if (hitPoints < 1) throw new ArgumentOutOfRangeException(nameof(hitPoints));
            remaining = hitPoints;
        }

        public bool TryDestroyWithHit()
        {
            if (remaining <= 0) return false;
            return --remaining == 0;
        }
    }
}
