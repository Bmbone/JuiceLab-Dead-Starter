using UnityEngine;

namespace JuiceStudy.Abilities
{
    /// <summary>One reused fireball, one rune line and three bounded audio voices.</summary>
    public sealed class AbilityEffects
    {
        private readonly GameObject fireball;
        private readonly TrailRenderer trail;
        private readonly LineRenderer shield;
        private readonly AudioSource[] voices = new AudioSource[3];
        private readonly AudioClip[] clips = new AudioClip[4];
        private int voiceIndex;
        private float shieldFlash;

        public AbilityEffects(Transform parent, SpriteRenderer reference, Material material, float shieldY)
        {
            fireball = new GameObject("Fireball - reused");
            fireball.transform.SetParent(parent, false);
            Orb("Flame", fireball.transform, reference, 0.44f, new Color(1, 0.27f, 0.04f), 6);
            Orb("Hot core", fireball.transform, reference, 0.24f, new Color(1, 0.91f, 0.51f), 7);
            trail = fireball.AddComponent<TrailRenderer>();
            trail.sharedMaterial = material;
            trail.time = 0.16f;
            trail.startWidth = 0.32f;
            trail.endWidth = 0;
            trail.startColor = new Color(1, 0.47f, 0.08f, 0.9f);
            trail.endColor = new Color(0.8f, 0.08f, 0.01f, 0);
            trail.minVertexDistance = 0.025f;
            trail.numCapVertices = 4;
            trail.sortingOrder = 5;
            StopFireball();

            var rune = new GameObject("Rune shield");
            rune.transform.SetParent(parent, false);
            shield = rune.AddComponent<LineRenderer>();
            shield.sharedMaterial = material;
            shield.useWorldSpace = true;
            shield.positionCount = 2;
            shield.SetPosition(0, new Vector3(-7.45f, shieldY, 0));
            shield.SetPosition(1, new Vector3(7.4f, shieldY, 0));
            shield.numCapVertices = 4;
            shield.sortingOrder = 5;
            shield.enabled = false;
            for (int i = 0; i < voices.Length; i++)
            {
                var child = new GameObject("Spell audio voice " + i);
                child.transform.SetParent(parent, false);
                voices[i] = child.AddComponent<AudioSource>();
                voices[i].playOnAwake = false;
                voices[i].spatialBlend = 0;
                voices[i].volume = 0.28f;
            }
            clips[0] = MakeCue("Fireball whoosh", 180, 480, true);
            clips[1] = MakeCue("Giant's grip chime", 330, 660, false);
            clips[2] = MakeCue("Rune shield cast", 440, 880, false);
            clips[3] = MakeCue("Rune shield deflect", 900, 280, false);
        }

        private static void Orb(string name, Transform parent, SpriteRenderer reference, float diameter, Color color, int order)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = reference.sprite;
            renderer.sharedMaterial = reference.sharedMaterial;
            renderer.color = color;
            renderer.sortingOrder = order;
            child.transform.localScale = Vector3.one * (diameter / Mathf.Max(0.01f, reference.sprite.bounds.size.x));
        }

        public void StartFireball(Vector2 position)
        {
            fireball.transform.position = position;
            fireball.SetActive(true);
            trail.Clear();
            trail.emitting = true;
        }

        public void MoveFireball(Vector2 position) => fireball.transform.position = position;

        public void StopFireball()
        {
            trail.emitting = false;
            trail.Clear();
            fireball.SetActive(false);
        }

        public void PlayCue(int index)
        {
            AudioSource voice = voices[voiceIndex++ % voices.Length];
            voice.Stop();
            voice.clip = clips[index];
            voice.Play();
        }

        public void ShieldHit()
        {
            shieldFlash = 0.22f;
            PlayCue(3);
        }

        public void Tick(float delta, bool shieldActive)
        {
            shieldFlash = Mathf.Max(0, shieldFlash - delta);
            shield.enabled = shieldActive || shieldFlash > 0;
            float flash = shieldFlash / 0.22f;
            shield.startWidth = shield.endWidth = 0.065f + flash * 0.15f;
            var color = new Color(0.3f, 0.85f, 1, shieldActive ? 0.9f : flash);
            shield.startColor = shield.endColor = color;
        }

        public void Reset()
        {
            StopFireball();
            shieldFlash = 0;
            shield.enabled = false;
            foreach (AudioSource voice in voices) if (voice != null) voice.Stop();
        }

        private static AudioClip MakeCue(string name, float startHz, float endHz, bool noisy)
        {
            const int rate = 22050;
            const float seconds = 0.28f;
            var samples = new float[(int)(rate * seconds)];
            var random = new System.Random(402);
            double phase = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / samples.Length;
                phase += 2 * System.Math.PI * Mathf.Lerp(startHz, endHz, t) / rate;
                double signal = System.Math.Sin(phase) + 0.18 * System.Math.Sin(phase * 2);
                if (noisy) signal += (random.NextDouble() * 2 - 1) * 0.65;
                float envelope = Mathf.Min(1, t / 0.03f) * (1 - t) * (1 - t);
                samples[i] = (float)signal * envelope * 0.4f;
            }
            var clip = AudioClip.Create(name, samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void Dispose()
        {
            foreach (AudioClip clip in clips) if (clip != null) Object.Destroy(clip);
        }
    }
}
