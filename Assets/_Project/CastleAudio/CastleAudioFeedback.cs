using JuiceStudy.Levels;
using UnityEngine;

namespace JuiceStudy.CastleAudio
{
    /// <summary>Presentation-only event audio. Outcome cues have a voice collision sounds cannot steal.</summary>
    [DisallowMultipleComponent]
    public sealed class CastleAudioFeedback : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private AudioClip paddleWood;
        [SerializeField] private AudioClip wallStone;
        [SerializeField] private AudioClip[] brickBreaks;
        [SerializeField] private AudioClip lifeLost;
        [SerializeField] private AudioClip victoryFanfare;
        [SerializeField] private AudioClip gameOverToll;
        [SerializeField] private bool muted;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 0.65f;
        [SerializeField, Range(0f, 1f)] private float impactVolume = 0.50f;
        [SerializeField, Range(0f, 1f)] private float lifeLossVolume = 0.65f;
        [SerializeField, Range(0f, 1f)] private float resultVolume = 0.75f;
        [SerializeField, Range(1, 8)] private int collisionVoices = 4;
        [SerializeField, Range(0f, 0.4f)] private float stereoWidth = 0.22f;
        private AudioSource[] voices;
        private float[] voiceGains;
        private AudioSource importantVoice;
        private bool importantIsResult;
        private bool roundArmed;
        private bool resultHandled;
        private int nextVoice;
        private int nextBrick;
        private float lastPaddle = float.NegativeInfinity;
        private float lastWall = float.NegativeInfinity;
        private float lastBrick = float.NegativeInfinity;

        private void Awake()
        {
            bool ready = session != null && paddleWood != null && wallStone != null &&
                lifeLost != null && victoryFanfare != null && gameOverToll != null &&
                brickBreaks != null && brickBreaks.Length > 0;
            if (brickBreaks != null)
                foreach (AudioClip clip in brickBreaks) ready &= clip != null;
            if (!ready)
            {
                Debug.LogError("Assign LevelSession and every CastleAudio clip before playing.", this);
                enabled = false;
                return;
            }
            int count = Mathf.Clamp(collisionVoices, 1, 8);
            voices = new AudioSource[count];
            voiceGains = new float[count];
            for (int i = 0; i < count; i++) voices[i] = MakeVoice("Castle impact voice " + i, 96);
            importantVoice = MakeVoice("Castle life and result voice", 32);
        }

        private AudioSource MakeVoice(string label, int priority)
        {
            var child = new GameObject(label);
            child.transform.SetParent(transform, false);
            var voice = child.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.loop = false;
            voice.spatialBlend = 0;
            voice.dopplerLevel = 0;
            voice.priority = priority;
            return voice;
        }

        private void OnEnable()
        {
            if (importantVoice == null) return;
            ResetAudio();
            GameEvents.BallHitPaddle += OnPaddle;
            GameEvents.BallHitWall += OnWall;
            GameEvents.BrickDestroyed += OnBrick;
            GameEvents.BallLost += OnLoss;
            GameEvents.ScoreChanged += OnScore;
        }

        private void OnDisable()
        {
            GameEvents.BallHitPaddle -= OnPaddle;
            GameEvents.BallHitWall -= OnWall;
            GameEvents.BrickDestroyed -= OnBrick;
            GameEvents.BallLost -= OnLoss;
            GameEvents.ScoreChanged -= OnScore;
            StopImpacts();
            if (importantVoice != null) importantVoice.Stop();
            roundArmed = false;
        }

        private bool CanImpact => roundArmed && !resultHandled && session != null && session.IsPlaying;
        private float Master => muted ? 0 : Mathf.Clamp01(masterVolume);
        private float ImportantGain => importantIsResult ? Mathf.Clamp01(resultVolume) : Mathf.Clamp01(lifeLossVolume);
        private float ImpactDuck => importantVoice != null && importantVoice.isPlaying ? 0.35f : 1;

        private void OnPaddle(ImpactInfo impact)
        {
            if (!CanImpact || Time.unscaledTime - lastPaddle < 0.03f) return;
            lastPaddle = Time.unscaledTime;
            PlayImpact(paddleWood, 0.85f, 1, impact.Point.x);
        }

        private void OnWall(ImpactInfo impact)
        {
            if (!CanImpact || Time.unscaledTime - lastWall < 0.065f) return;
            lastWall = Time.unscaledTime;
            PlayImpact(wallStone, 0.45f, 1, impact.Point.x);
        }

        private void OnBrick(ImpactInfo impact)
        {
            if (!roundArmed || resultHandled || session == null) return;
            // LevelSession commits the winning state BEFORE BrickDestroyed, including a Fireball's last hit.
            // Resolve victory before the rate limit so a rapid final break never suppresses its fanfare.
            if (!session.IsPlaying)
            {
                PlayResult(victoryFanfare);
                return;
            }
            if (Time.unscaledTime - lastBrick < 0.025f) return;
            lastBrick = Time.unscaledTime;
            AudioClip clip = brickBreaks[nextBrick];
            nextBrick = (nextBrick + 1) % brickBreaks.Length;
            float pitch = 1 + Mathf.Clamp(impact.Combo, 0, 6) * 0.015f;
            PlayImpact(clip, 1, pitch, impact.Point.x);
        }

        private void OnLoss()
        {
            if (!roundArmed || resultHandled || session == null) return;
            // BallLost follows TryLoseLife: the final life is already GameOver at this point.
            if (!session.IsPlaying) PlayResult(gameOverToll);
            else
            {
                StopImpacts();
                PlayImportant(lifeLost, false);
            }
        }

        private void PlayResult(AudioClip clip)
        {
            resultHandled = true;
            roundArmed = false;
            StopImpacts();
            PlayImportant(clip, true);
        }

        private void PlayImportant(AudioClip clip, bool result)
        {
            importantIsResult = result;
            importantVoice.Stop();
            importantVoice.clip = clip;
            importantVoice.pitch = 1;
            importantVoice.panStereo = 0;
            importantVoice.volume = Master * ImportantGain;
            importantVoice.Play();
        }

        private void PlayImpact(AudioClip clip, float gain, float pitch, float x)
        {
            int index = nextVoice;
            for (int offset = 0; offset < voices.Length; offset++)
            {
                int candidate = (nextVoice + offset) % voices.Length;
                if (!voices[candidate].isPlaying) { index = candidate; break; }
            }
            nextVoice = (index + 1) % voices.Length;
            AudioSource voice = voices[index];
            voice.Stop();
            voice.clip = clip;
            voiceGains[index] = gain;
            voice.volume = Master * Mathf.Clamp01(impactVolume) * gain * ImpactDuck;
            voice.pitch = pitch;
            voice.panStereo = Mathf.Clamp(x / 7.5f, -1, 1) * Mathf.Clamp(stereoWidth, 0, 0.4f);
            voice.Play();
        }

        private void Update()
        {
            // Inspector mute/volume changes affect sounds already playing without creating new voices.
            if (voices == null) return;
            float gain = Master * Mathf.Clamp01(impactVolume) * ImpactDuck;
            for (int i = 0; i < voices.Length; i++) voices[i].volume = gain * voiceGains[i];
            if (importantVoice != null) importantVoice.volume = Master * ImportantGain;
        }

        private void OnScore(int score)
        {
            if (score == 0) ResetAudio();
        }

        private void ResetAudio()
        {
            StopImpacts();
            if (importantVoice != null) importantVoice.Stop();
            roundArmed = session != null && session.IsPlaying;
            resultHandled = false;
            importantIsResult = false;
            nextVoice = nextBrick = 0;
            lastPaddle = lastWall = lastBrick = float.NegativeInfinity;
        }

        private void StopImpacts()
        {
            if (voices == null) return;
            foreach (AudioSource voice in voices) if (voice != null) voice.Stop();
        }

        private void OnDestroy()
        {
            if (voices != null)
                foreach (AudioSource voice in voices) if (voice != null) Destroy(voice.gameObject);
            if (importantVoice != null) Destroy(importantVoice.gameObject);
            // Imported clip assets belong to the project, not this component.
        }
    }
}
