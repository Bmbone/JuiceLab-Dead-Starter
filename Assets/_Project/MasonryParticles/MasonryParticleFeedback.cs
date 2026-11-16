using UnityEngine;

namespace JuiceStudy.MasonryParticles
{
    /// <summary>Three bounded, event-driven particle layers complement the existing stone break animation.</summary>
    [DisallowMultipleComponent]
    public sealed class MasonryParticleFeedback : MonoBehaviour
    {
        [SerializeField] private Material chipMaterial;
        [SerializeField] private Material dustMaterial;
        [SerializeField] private Material sparkMaterial;
        [SerializeField, Range(2, 12)] private int poolSize = 8;
        [SerializeField, Range(0, 12)] private int chipCount = 8;
        [SerializeField, Range(0, 4)] private int dustCount = 3;
        [SerializeField, Range(0, 6)] private int sparkCount = 4;
        [SerializeField, Range(0.5f, 1.5f)] private float intensity = 1f;
        [SerializeField, Range(0.04f, 0.25f)] private float dustOpacity = 0.16f;
        private sealed class Burst
        {
            public ParticleSystem Chips;
            public ParticleSystem Dust;
            public ParticleSystem Sparks;
        }
        private Burst[] pool;
        private Transform root;
        private int next;
        // Cosmetic randomness must never advance the ball's UnityEngine.Random sequence.
        private readonly System.Random random = new System.Random(40205);

        private void Awake()
        {
            if (chipMaterial == null || dustMaterial == null || sparkMaterial == null)
            {
                Debug.LogError("Assign the chip, dust and spark materials to MasonryParticleFeedback.", this);
                enabled = false;
                return;
            }
            root = new GameObject("Masonry particle pool").transform;
            root.SetParent(transform, false);
            pool = new Burst[Mathf.Clamp(poolSize, 2, 12)];
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i] = new Burst
                {
                    Chips = BuildSystem("Stone chips " + i, chipMaterial, 12, 0.65f, -1, 0, i),
                    Dust = BuildSystem("Powder dust " + i, dustMaterial, 4, 0f, -5, 1, i),
                    Sparks = BuildSystem("Brass sparks " + i, sparkMaterial, 6, 0.12f, 3, 2, i)
                };
            }
        }

        private void OnEnable()
        {
            if (pool == null) return;
            GameEvents.BrickDestroyed += OnBrick;
            GameEvents.ScoreChanged += OnScore;
        }

        private void OnDisable()
        {
            GameEvents.BrickDestroyed -= OnBrick;
            GameEvents.ScoreChanged -= OnScore;
            ClearAll();
        }

        private void OnScore(int score)
        {
            if (score == 0) ClearAll();
        }

        private void OnBrick(ImpactInfo impact)
        {
            if (pool == null) return;
            Burst burst = pool[next];
            next = (next + 1) % pool.Length;
            Clear(burst);
            // Spray from the struck face; this also works for side hits and swept fireball hits.
            Vector2 outward = impact.Point - impact.Center;
            if (outward.sqrMagnitude < 0.0001f) outward = impact.Normal;
            outward = outward.sqrMagnitude > 0.0001f ? outward.normalized : Vector2.down;
            Vector2 tangent = new Vector2(-outward.y, outward.x);
            float spread = Mathf.Min(impact.Size.x, impact.Size.y) * 0.3f;
            float power = Mathf.Clamp(intensity, 0.5f, 1.5f);
            EmitChips(burst.Chips, impact, outward, tangent, spread, power);
            EmitDust(burst.Dust, impact, power);
            EmitSparks(burst.Sparks, impact, outward, tangent, spread, power);
        }

        private void EmitChips(ParticleSystem particles, ImpactInfo hit, Vector2 outward, Vector2 tangent, float spread, float power)
        {
            if (chipCount <= 0) return;
            particles.Play(false);
            Color baseColor = Color.Lerp(hit.Color, new Color(0.48f, 0.50f, 0.49f, 1), 0.22f);
            baseColor.a = 1;
            int count = Mathf.Clamp(chipCount, 0, 12);
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Rotate(outward, Sample(-72, 72)) * (Sample(1.2f, 2.7f) * power) + Vector2.up * 0.35f;
                Color tint = baseColor * Sample(0.84f, 1.08f);
                tint.a = 1;
                var particle = new ParticleSystem.EmitParams
                {
                    position = hit.Point + tangent * Sample(-spread, spread) + outward * 0.02f,
                    velocity = velocity,
                    startColor = tint,
                    startSize = Sample(0.045f, 0.105f) * power,
                    startLifetime = Sample(0.24f, 0.40f),
                    rotation = Sample(0, 360)
                };
                particles.Emit(particle, 1);
            }
        }

        private void EmitDust(ParticleSystem particles, ImpactInfo hit, float power)
        {
            if (dustCount <= 0) return;
            particles.Play(false);
            Color dust = Color.Lerp(hit.Color, new Color(0.77f, 0.73f, 0.64f, 1), 0.7f);
            int count = Mathf.Clamp(dustCount, 0, 4);
            Vector2 center = Vector2.Lerp(hit.Point, hit.Center, 0.65f);
            for (int i = 0; i < count; i++)
            {
                Color tint = dust;
                tint.a = Mathf.Clamp(dustOpacity, 0.04f, 0.25f) * Sample(0.72f, 1f);
                var particle = new ParticleSystem.EmitParams
                {
                    position = center + new Vector2(Sample(-0.16f, 0.16f) * hit.Size.x, Sample(-0.12f, 0.12f) * hit.Size.y),
                    velocity = new Vector3(Sample(-0.35f, 0.35f), Sample(0.25f, 0.6f), 0),
                    startColor = tint,
                    startSize = Sample(0.20f, 0.34f) * power,
                    startLifetime = Sample(0.28f, 0.42f),
                    rotation = Sample(0, 360)
                };
                particles.Emit(particle, 1);
            }
        }

        private void EmitSparks(ParticleSystem particles, ImpactInfo hit, Vector2 outward, Vector2 tangent, float spread, float power)
        {
            if (sparkCount <= 0) return;
            particles.Play(false);
            int count = Mathf.Clamp(sparkCount, 0, 6);
            for (int i = 0; i < count; i++)
            {
                var particle = new ParticleSystem.EmitParams
                {
                    position = hit.Point + tangent * Sample(-spread * 0.65f, spread * 0.65f),
                    velocity = Rotate(outward, Sample(-58, 58)) * (Sample(2.2f, 4f) * power),
                    startColor = Color.Lerp(new Color(1f, 0.72f, 0.35f, 1), new Color(1, 0.92f, 0.7f, 1), Sample(0, 1)),
                    startSize = Sample(0.035f, 0.055f) * power,
                    startLifetime = Sample(0.13f, 0.23f),
                    rotation = 0
                };
                particles.Emit(particle, 1);
            }
        }

        private ParticleSystem BuildSystem(string name, Material material, int capacity, float gravity, int order, int kind, int index)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(root, false);
            obj.SetActive(false);
            var particles = obj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1;
            main.startSpeed = 0;
            main.startLifetime = 1;
            main.startSize = 0.1f;
            main.startColor = Color.white;
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = capacity;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            main.stopAction = ParticleSystemStopAction.None;
            var emission = particles.emission;
            emission.enabled = false;
            var shape = particles.shape;
            shape.enabled = false;
            particles.useAutoRandomSeed = false;
            particles.randomSeed = (uint)(40205 + index * 71 + kind * 193);

            var colors = particles.colorOverLifetime;
            colors.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(kind == 0 ? new Color(0.70f, 0.73f, 0.75f) : Color.white, 1) },
                kind == 1 ? new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.18f), new GradientAlphaKey(0, 1) }
                          : new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.25f), new GradientAlphaKey(0, 1) });
            colors.color = gradient;
            var size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1,
                kind == 1 ? AnimationCurve.EaseInOut(0, 0.65f, 1, 1.7f) : AnimationCurve.EaseInOut(0, 1, 1, kind == 0 ? 0.42f : 0));
            if (kind == 0)
            {
                var spin = particles.rotationOverLifetime;
                spin.enabled = true;
                spin.z = new ParticleSystem.MinMaxCurve(-4.5f, 4.5f);
            }
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            renderer.alignment = ParticleSystemRenderSpace.View;
            if (kind == 2)
            {
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.cameraVelocityScale = 0;
                renderer.velocityScale = 0.04f;
                renderer.lengthScale = 1.4f;
            }
            else renderer.renderMode = ParticleSystemRenderMode.Billboard;
            obj.SetActive(true);
            particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particles;
        }

        private float Sample(float minimum, float maximum) => minimum + (maximum - minimum) * (float)random.NextDouble();

        private static Vector2 Rotate(Vector2 direction, float degrees)
        {
            float angle = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(angle), cos = Mathf.Cos(angle);
            return new Vector2(direction.x * cos - direction.y * sin, direction.x * sin + direction.y * cos);
        }

        private static void Clear(Burst burst)
        {
            burst.Chips.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            burst.Dust.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            burst.Sparks.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void ClearAll()
        {
            next = 0;
            if (pool == null) return;
            foreach (Burst burst in pool) Clear(burst);
        }

        private void OnDestroy()
        {
            if (root != null) Destroy(root.gameObject);
        }
    }
}
