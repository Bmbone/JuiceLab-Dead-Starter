using JuiceStudy.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace JuiceStudy.CastleMusic
{
    /// <summary>Two synchronized arrangements; scene-local music survives round feedback resets.</summary>
    [DisallowMultipleComponent]
    public sealed class CastleMusicController : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private AudioClip menuTheme;
        [SerializeField] private AudioClip playTheme;
        [SerializeField] private bool muted;
        [SerializeField, Range(0f, 0.5f)] private float musicVolume = 0.22f;
        [SerializeField, Range(0.1f, 2f)] private float crossfadeSeconds = 0.8f;
        [SerializeField, Range(0.1f, 2f)] private float fadeInSeconds = 0.7f;
        [SerializeField, Range(0.05f, 0.5f)] private float resultFadeSeconds = 0.18f;
        [SerializeField, Range(0f, 1f)] private float lifeDuckGain = 0.35f;
        [SerializeField, Range(0.2f, 1.2f)] private float lifeDuckSeconds = 0.70f;
        private enum Phase { Menu, Playing, Result }
        private Phase phase;
        private AudioSource menuVoice;
        private AudioSource playVoice;
        private Button nextLevelButton;
        private bool phaseInitialized;
        private bool scheduled;
        private float blend;
        private float envelope;
        private float duck = 1;
        private float duckUntil = float.NegativeInfinity;

        private void Awake()
        {
            if (session == null || menuTheme == null || playTheme == null)
            {
                Debug.LogError("Assign LevelSession and the two CastleMusic arrangements.", this);
                enabled = false;
                return;
            }
            menuVoice = MakeVoice("Courtyard menu arrangement", menuTheme);
            playVoice = MakeVoice("Courtyard play arrangement", playTheme);
            RequestAudioData();
        }

        private AudioSource MakeVoice(string label, AudioClip clip)
        {
            var child = new GameObject(label);
            child.transform.SetParent(transform, false);
            var voice = child.AddComponent<AudioSource>();
            voice.clip = clip;
            voice.playOnAwake = false;
            voice.loop = true;
            voice.spatialBlend = 0;
            voice.dopplerLevel = 0;
            voice.pitch = 1;
            voice.priority = 160;
            voice.volume = 0;
            return voice;
        }

        private void OnEnable()
        {
            if (menuVoice == null) return;
            if (!phaseInitialized || session.IsPlaying)
            {
                phase = session.IsPlaying ? Phase.Playing : Phase.Menu;
                phaseInitialized = true;
            }
            // Refresh subscriptions idempotently, also when Enter Play Mode skips a domain reload.
            DetachStateEvents();
            GameEvents.ScoreChanged += OnScore;
            GameEvents.BrickDestroyed += OnBrick;
            GameEvents.BallLost += OnLoss;
            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            blend = phase == Phase.Playing ? 1 : 0;
            envelope = 0;
            duck = 1;
            RequestAudioData();
            TrySchedule();
        }

        private void OnDisable()
        {
            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
            StopLoops();
            // Keep state-only game events attached until destruction, so menu/round transitions made
            // while music is disabled are remembered. These handlers never start or schedule audio.
        }

        private void RequestAudioData()
        {
            if (menuTheme != null && menuTheme.loadState == AudioDataLoadState.Unloaded) menuTheme.LoadAudioData();
            if (playTheme != null && playTheme.loadState == AudioDataLoadState.Unloaded) playTheme.LoadAudioData();
        }

        private bool TrySchedule()
        {
            if (scheduled) return true;
            if (menuVoice == null || playVoice == null) return false;
            if (menuTheme.loadState == AudioDataLoadState.Failed || playTheme.loadState == AudioDataLoadState.Failed)
            {
                Debug.LogError("A CastleMusic arrangement failed to load.", this);
                enabled = false;
                return false;
            }
            if (menuTheme.loadState != AudioDataLoadState.Loaded || playTheme.loadState != AudioDataLoadState.Loaded) return false;
            if (menuTheme.samples != playTheme.samples || menuTheme.frequency != playTheme.frequency)
            {
                Debug.LogError("CastleMusic arrangements must have identical sample counts and sample rates.", this);
                enabled = false;
                return false;
            }
            double start = AudioSettings.dspTime + 0.15;
            menuVoice.timeSamples = playVoice.timeSamples = 0;
            menuVoice.PlayScheduled(start);
            playVoice.PlayScheduled(start);
            scheduled = true;
            return true;
        }

        private void Update()
        {
            if (!TrySchedule()) return;
            float dt = Mathf.Max(0, Time.unscaledDeltaTime);
            float targetBlend = phase == Phase.Menu ? 0 : phase == Phase.Playing ? 1 : blend;
            blend = Mathf.MoveTowards(blend, targetBlend, dt / Mathf.Max(0.01f, crossfadeSeconds));
            float targetEnvelope = phase == Phase.Result ? 0 : 1;
            float fade = phase == Phase.Result ? resultFadeSeconds : fadeInSeconds;
            envelope = Mathf.MoveTowards(envelope, targetEnvelope, dt / Mathf.Max(0.01f, fade));
            float targetDuck = phase == Phase.Playing && Time.unscaledTime < duckUntil ? Mathf.Clamp01(lifeDuckGain) : 1;
            duck = Mathf.MoveTowards(duck, targetDuck, dt / (targetDuck < duck ? 0.045f : 0.35f));
            float gain = (muted ? 0 : Mathf.Clamp(musicVolume, 0, 0.5f)) * envelope * duck;
            // Linear weights preserve the common backing rather than doubling it at the midpoint.
            menuVoice.volume = gain * (1 - blend);
            playVoice.volume = gain * blend;
        }

        private void LateUpdate()
        {
            // LevelMenuView builds its buttons in Start. Bind once after that hierarchy exists.
            if (nextLevelButton != null || session == null) return;
            Transform button = session.transform.Find("Level selection UI/Menu overlay/Menu panel/NEXT LEVEL");
            if (button == null || !button.TryGetComponent(out nextLevelButton)) return;
            nextLevelButton.onClick.AddListener(OnNextLevel);
        }

        private void OnScore(int score)
        {
            if (score != 0 || session == null) return;
            phase = session.IsPlaying ? Phase.Playing : Phase.Menu;
            phaseInitialized = true;
            duckUntil = float.NegativeInfinity;
        }

        private void OnBrick(ImpactInfo _)
        {
            // The existing session commits its final-brick win before publishing this event.
            if (phase == Phase.Playing && session != null && !session.IsPlaying)
                phase = Phase.Result;
        }

        private void OnLoss()
        {
            if (phase != Phase.Playing || session == null) return;
            if (session.IsPlaying) duckUntil = Time.unscaledTime + Mathf.Max(0, lifeDuckSeconds);
            else phase = Phase.Result;
        }

        private void OnNextLevel()
        {
            // This existing button opens difficulty selection without emitting a score reset.
            if (session == null || session.IsPlaying) return;
            phase = Phase.Menu;
            duckUntil = float.NegativeInfinity;
        }

        private void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            if (!isActiveAndEnabled) return;
            StopLoops();
            envelope = 0;
            RequestAudioData();
            TrySchedule();
        }

        private void StopLoops()
        {
            scheduled = false;
            if (menuVoice != null) { menuVoice.Stop(); menuVoice.volume = 0; }
            if (playVoice != null) { playVoice.Stop(); playVoice.volume = 0; }
        }

        private void DetachStateEvents()
        {
            GameEvents.ScoreChanged -= OnScore;
            GameEvents.BrickDestroyed -= OnBrick;
            GameEvents.BallLost -= OnLoss;
        }

        private void OnDestroy()
        {
            DetachStateEvents();
            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
            if (nextLevelButton != null) nextLevelButton.onClick.RemoveListener(OnNextLevel);
            StopLoops();
            if (menuVoice != null) Destroy(menuVoice.gameObject);
            if (playVoice != null) Destroy(playVoice.gameObject);
        }
    }
}
