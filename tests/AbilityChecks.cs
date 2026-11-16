using System;
using JuiceStudy.Abilities;
using JuiceStudy.Arcade;

internal static class AbilityChecks
{
    private static int checks;
    private static void Check(bool condition, string message)
    {
        checks++;
        if (!condition) throw new Exception(message);
    }
    private static void Reject(Action action)
    {
        bool rejected = false;
        try { action(); }
        catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Invalid input must be rejected.");
    }
    private static bool Near(float a, float b) => Math.Abs(a - b) < 0.001f;

    public static int Main()
    {
        var state = new AbilityState();
        foreach (Spell spell in Enum.GetValues(typeof(Spell)))
            Check(!state.TryCast(spell), "Spells cannot be used outside a run.");
        state.Reset(true);
        Check(state.TryCast(Spell.Fireball) && state.TryCast(Spell.WidePaddle) && state.TryCast(Spell.RuneShield),
            "All three independent abilities start ready and can be combined.");
        Check(!state.TryCast(Spell.Fireball) && state.CooldownRemaining(Spell.Fireball) == 5,
            "Repeated input cannot restart or bypass the fireball cooldown.");
        state.Tick(1);
        Check(Near(state.CooldownRemaining(Spell.Fireball), 4), "Fireball cooldown counts down.");
        Check(Near(state.ActiveRemaining(Spell.WidePaddle), 5), "Wide paddle lasts six seconds.");
        Check(Near(state.ActiveRemaining(Spell.RuneShield), 7), "Shield lasts at most eight seconds.");
        Check(!state.TryBlock(-5.4f, -5.0f, -5.15f), "Upward-moving ball does not consume a shield.");
        Check(!state.TryBlock(-5.3f, -5.5f, -5.15f), "Casting after passing the shield cannot teleport-rescue a ball.");
        Check(!state.TryBlock(-4.8f, -5.0f, -5.15f), "Shield stays armed until an actual crossing.");
        Check(state.TryBlock(-5.0f, -5.3f, -5.15f), "Downward crossing consumes the shield.");
        Check(!state.TryBlock(-5.0f, -5.3f, -5.15f), "One cast blocks only one crossing.");
        Check(state.CooldownRemaining(Spell.RuneShield) == 13, "Using a shield does not refill its cooldown.");
        state.Tick(4);
        Check(state.IsReady(Spell.Fireball) && !state.IsReady(Spell.WidePaddle), "Cooldowns progress independently.");
        Check(state.TryCast(Spell.Fireball), "Fireball becomes castable at five seconds.");
        state.Tick(1);
        Check(state.ActiveRemaining(Spell.WidePaddle) == 0 && !state.TryCast(Spell.WidePaddle),
            "Wide effect expires before its cooldown; it cannot stack.");
        state.Tick(6);
        Check(state.TryCast(Spell.WidePaddle), "Wide paddle is ready after twelve seconds.");
        Check(!state.TryCast(Spell.RuneShield), "Shield still needs its longer cooldown.");
        state.Tick(2);
        Check(state.TryCast(Spell.RuneShield), "Shield becomes available after fourteen seconds.");

        float wideCooldown = state.CooldownRemaining(Spell.WidePaddle);
        float shieldCooldown = state.CooldownRemaining(Spell.RuneShield);
        state.CancelEffects();
        Check(state.ActiveRemaining(Spell.WidePaddle) == 0 && state.ActiveRemaining(Spell.RuneShield) == 0,
            "Life loss cancels buffs.");
        Check(state.CooldownRemaining(Spell.WidePaddle) == wideCooldown &&
              state.CooldownRemaining(Spell.RuneShield) == shieldCooldown, "Life loss cannot be used to refill spells.");
        Reject(() => state.Tick(-1));
        Reject(() => state.Tick(float.NaN));
        Reject(() => state.Tick(float.PositiveInfinity));
        Reject(() => state.TryCast((Spell)99));
        Reject(() => new BrickHealth(0));

        var health = new BrickHealth(2);
        Check(!health.TryDestroyWithHit() && health.TryDestroyWithHit() && !health.TryDestroyWithHit(),
            "Multi-hit bricks are destroyed exactly once.");
        foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
        {
            var round = new ArcadeRound();
            round.Start(difficulty, 28);
            var bricks = new BrickHealth[28];
            for (int i = 0; i < bricks.Length; i++) bricks[i] = new BrickHealth(1);
            // A piercing fireball crosses a column, then the normal ball hits those same bricks.
            for (int i = 0; i < 4; i++) if (bricks[i].TryDestroyWithHit()) round.TryBreakBrick(10);
            for (int i = 0; i < 4; i++) if (bricks[i].TryDestroyWithHit()) round.TryBreakBrick(10);
            Check(round.Score == 40 && round.RemainingBricks == 24, "Ball/fireball race must not award duplicate score.");
            state.Reset(true);
            state.TryCast(Spell.RuneShield);
            if (!state.TryBlock(-5.0f, -5.15f, -5.15f)) round.TryLoseLife();
            Check(round.Lives == 3 && round.BallSpeed == ArcadeRound.SpeedFor(difficulty),
                "Shield preserves lives and the selected speed.");
            for (int i = 4; i < 28; i++) if (bricks[i].TryDestroyWithHit()) round.TryBreakBrick(10);
            Check(round.Phase == RoundPhase.Won && round.Score == 280, "Spell damage can finish a round normally.");
            state.Reset(false);
            Check(!state.TryCast(Spell.Fireball) && !round.TryBreakBrick(10), "Victory cannot accept more casts or damage.");
        }

        var random = new Random(402);
        for (int run = 0; run < 100; run++)
        {
            state.Reset(true);
            var casts = new int[3];
            float elapsed = 0;
            for (int tick = 0; tick < 500; tick++)
            {
                state.Tick(0.1f);
                elapsed += 0.1f;
                foreach (Spell spell in Enum.GetValues(typeof(Spell)))
                {
                    if (random.Next(3) == 0 && state.TryCast(spell)) casts[(int)spell]++;
                    float cooldown = state.CooldownRemaining(spell), active = state.ActiveRemaining(spell);
                    Check(cooldown >= 0 && cooldown <= AbilityState.CooldownLength(spell), "Cooldown remains bounded.");
                    Check(active >= 0 && active <= AbilityState.EffectLength(spell), "Effects cannot stack or run forever.");
                    Check(casts[(int)spell] <= 1 + (int)((elapsed + 0.01f) / AbilityState.CooldownLength(spell)),
                        "Repeated key/button input cannot cast faster than the cooldown.");
                }
                if (random.Next(20) == 0) state.CancelEffects();
            }
            state.Reset(false);
            state.Tick(100);
            foreach (Spell spell in Enum.GetValues(typeof(Spell)))
                Check(!state.IsReady(spell) && state.ActiveRemaining(spell) == 0 && state.CooldownRemaining(spell) == 0,
                    "Menus/results clear all active state.");
            state.Reset(true);
            foreach (Spell spell in Enum.GetValues(typeof(Spell)))
                Check(state.IsReady(spell), "Retry/new difficulty starts with every ability ready.");
        }
        Console.WriteLine($"PASS: {checks} ability assertions, cooldown stress, shield crossings and duplicate-hit scoring.");
        return 0;
    }
}
