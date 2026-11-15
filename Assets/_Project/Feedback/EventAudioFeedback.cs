using UnityEngine;

namespace JuiceStudy
{
    /// <summary>Original synthesized arcade cues; a fixed voice pool prevents unbounded overlap.</summary>
    public sealed class EventAudioFeedback : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private float masterVolume = 0.65f;
        [SerializeField, Range(0, 1)] private float paddleVolume = 0.32f;
        [SerializeField, Range(0, 1)] private float brickVolume = 0.25f;
        [SerializeField, Range(0, 1)] private float lossVolume = 0.4f;
        [SerializeField, Range(1, 8)] private int collisionVoices = 4;
        private AudioSource[] voices;
        private AudioSource lossVoice;
        private AudioClip paddleClip;
        private AudioClip brickClip;
        private AudioClip lossClip;
        private float lastPaddle = float.NegativeInfinity;
        private float lastBrick = float.NegativeInfinity;

        private void Awake()
        {
            voices = new AudioSource[Mathf.Clamp(collisionVoices, 1, 8)];
            for (int i = 0; i < voices.Length; i++) voices[i] = CreateVoice("Collision voice " + i);
            lossVoice = CreateVoice("Ball loss voice");
            paddleClip = MakeCue("Paddle - soft pluck", 0.09f, 620, 340, false);
            brickClip = MakeCue("Brick - bright chime", 0.13f, 950, 1250, false);
            lossClip = MakeCue("Loss - falling two-tone", 0.38f, 300, 95, true);
        }

        private AudioSource CreateVoice(string label)
        {
            var child = new GameObject(label);
            child.transform.SetParent(transform, false);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.loop = false;
            return source;
        }

        private void OnEnable()
        {
            lastPaddle = lastBrick = float.NegativeInfinity;
            GameEvents.BallHitPaddle += OnPaddle;
            GameEvents.BrickDestroyed += OnBrick;
            GameEvents.BallLost += OnLoss;
        }
        private void OnDisable()
        {
            GameEvents.BallHitPaddle -= OnPaddle;
            GameEvents.BrickDestroyed -= OnBrick;
            GameEvents.BallLost -= OnLoss;
            if (voices != null) foreach (var voice in voices) if (voice != null) voice.Stop();
            if (lossVoice != null) lossVoice.Stop();
        }

        private void OnPaddle(ImpactInfo _)
        {
            if (Time.unscaledTime - lastPaddle < 0.025f) return;
            lastPaddle = Time.unscaledTime;
            PlayCollision(paddleClip, paddleVolume, 1);
        }
        private void OnBrick(ImpactInfo impact)
        {
            if (Time.unscaledTime - lastBrick < 0.025f) return;
            lastBrick = Time.unscaledTime;
            PlayCollision(brickClip, brickVolume, 1 + Mathf.Min(impact.Combo, 5) * 0.025f);
        }
        private void PlayCollision(AudioClip clip, float volume, float pitch)
        {
            if (voices == null) return;
            // Prefer a free voice; otherwise recycle the one furthest through its short cue.
            var selected = voices[0];
            foreach (var voice in voices)
            {
                if (!voice.isPlaying) { selected = voice; break; }
                if (voice.time > selected.time) selected = voice;
            }
            selected.Stop();
            selected.clip = clip;
            selected.volume = Mathf.Clamp01(volume * masterVolume);
            selected.pitch = pitch;
            selected.Play();
        }
        private void OnLoss()
        {
            if (lossVoice == null) return;
            foreach (var voice in voices) voice.Stop();
            lossVoice.clip = lossClip;
            lossVoice.volume = Mathf.Clamp01(lossVolume * masterVolume);
            lossVoice.Play();
        }

        private static AudioClip MakeCue(string label, float seconds, float startHz, float endHz, bool loss)
        {
            const int rate = 44100;
            var samples = new float[Mathf.CeilToInt(seconds * rate)];
            double phase = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                float age = t / seconds;
                float hz = Mathf.Lerp(startHz, endHz, age);
                phase += 2 * System.Math.PI * hz / rate;
                float attack = Mathf.Clamp01(t / 0.005f);
                float envelope = attack * Mathf.Pow(1 - age, loss ? 1.4f : 2.4f);
                double harmonic = loss ? 0.2 * System.Math.Sin(phase * 0.5) : 0.15 * System.Math.Sin(phase * 2);
                samples[i] = (float)((System.Math.Sin(phase) + harmonic) * envelope * 0.5);
            }
            var clip = AudioClip.Create(label, samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (paddleClip != null) Destroy(paddleClip);
            if (brickClip != null) Destroy(brickClip);
            if (lossClip != null) Destroy(lossClip);
        }
    }
}
