using UnityEngine;

namespace JuiceStudy
{
    /// <summary>A bounded pool of short, brick-colored bursts. No gameplay objects are changed.</summary>
    public sealed class BrickBurstFeedback : MonoBehaviour
    {
        [SerializeField] private Material particleMaterial;
        [SerializeField, Range(1, 16)] private int poolSize = 8;
        [SerializeField, Range(4, 40)] private int particleCount = 16;
        [SerializeField, Range(0.1f, 1f)] private float lifetime = 0.42f;
        [SerializeField, Range(0.5f, 8f)] private float speed = 3.2f;
        [SerializeField, Range(0.02f, 0.3f)] private float size = 0.12f;

        private ParticleSystem[] pool;
        private int next;
        // Feedback must not consume UnityEngine.Random, which the ball uses to choose launch angles.
        private readonly System.Random random = new System.Random(402);

        private void Awake()
        {
            if (particleMaterial == null)
            {
                Debug.LogError("Assign the Feedback material to BrickBurstFeedback.", this);
                enabled = false;
                return;
            }
            pool = new ParticleSystem[Mathf.Clamp(poolSize, 1, 16)];
            for (int i = 0; i < pool.Length; i++)
            {
                var child = new GameObject("Brick burst " + i);
                child.transform.SetParent(transform, false);
                child.SetActive(false);
                var particles = child.AddComponent<ParticleSystem>();
                var main = particles.main;
                main.playOnAwake = false;
                main.loop = false;
                main.duration = 1f;
                main.startSpeed = 0f;
                main.gravityModifier = 0f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 40;
                main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
                var emission = particles.emission;
                emission.enabled = false;
                var shape = particles.shape;
                shape.enabled = false;
                var color = particles.colorOverLifetime;
                color.enabled = true;
                var fade = new Gradient();
                fade.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                    new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
                color.color = fade;
                var shrink = particles.sizeOverLifetime;
                shrink.enabled = true;
                shrink.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, 1, 1, 0));
                particles.useAutoRandomSeed = false;
                particles.randomSeed = (uint)(402 + i);
                var renderer = particles.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = particleMaterial;
                renderer.sortingOrder = 5;
                child.SetActive(true);
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                pool[i] = particles;
            }
        }

        private void OnEnable() => GameEvents.BrickDestroyed += OnBrick;
        private void OnDisable()
        {
            GameEvents.BrickDestroyed -= OnBrick;
            if (pool == null) return;
            foreach (var particles in pool)
                if (particles != null) particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnBrick(ImpactInfo impact)
        {
            if (pool == null) return;
            var particles = pool[next];
            next = (next + 1) % pool.Length;
            // Recycle instead of accumulating GameObjects when many bricks break together.
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.transform.position = impact.Point;
            particles.Play();
            int count = Mathf.Clamp(particleCount, 4, 40);
            for (int i = 0; i < count; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2;
                float velocity = Mathf.Max(0, speed) * (0.5f + (float)random.NextDouble() * 0.5f);
                var particle = new ParticleSystem.EmitParams
                {
                    position = impact.Point,
                    velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * velocity,
                    startColor = impact.Color,
                    startSize = Mathf.Max(0.01f, size) * (0.65f + (float)random.NextDouble() * 0.35f),
                    startLifetime = Mathf.Max(0.05f, lifetime) * (0.7f + (float)random.NextDouble() * 0.3f),
                    rotation = (float)random.NextDouble() * 360
                };
                particles.Emit(particle, 1);
            }
        }
    }
}
