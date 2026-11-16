using System.Collections.Generic;
using JuiceStudy.ImpactMotion;
using JuiceStudy.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace JuiceStudy.CourtSettings
{
    /// <summary>Scene-local modal settings and an owned pause; existing gameplay modules remain enabled.</summary>
    [DefaultExecutionOrder(-10000), DisallowMultipleComponent]
    public sealed class CourtSettingsPanel : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private Transform boardRoot;
        [SerializeField] private Transform musicRoot;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private ImpactMotionFeedback impactMotion;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Font headingTypeface;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup musicBus;
        [SerializeField] private AudioMixerGroup sfxBus;
        private const string Prefix = "Brickkeep.Scene10.";
        private readonly Dictionary<AudioSource, AudioMixerGroup> originalRoutes = new Dictionary<AudioSource, AudioMixerGroup>();
        private readonly List<Transform> brickVisuals = new List<Transform>();
        private SettingsModalView view;
        private GameObject previousSelection;
        private float music = 1;
        private float sfx = 1;
        private bool cameraShake = true;
        private bool brickMotion = true;
        private bool open;
        private bool openedDuringPlay;
        private bool ownsPause;
        private float previousTimeScale;
        private bool ready;
        private bool refreshPending;
        private bool reorderCamera;
        private int cameraVersion = -1;
        private int suppressThroughFrame = -1;
        private bool waitForSpellRelease;

        private void Start()
        {
            if (session == null || boardRoot == null || musicRoot == null || worldCamera == null ||
                impactMotion == null || scoreText == null || mixer == null || musicBus == null || sfxBus == null)
            {
                Debug.LogError("Assign all CourtSettingsPanel scene, mixer and font references.", this);
                enabled = false;
                return;
            }
            music = ReadVolume("Music");
            sfx = ReadVolume("SFX");
            cameraShake = PlayerPrefs.GetInt(Prefix + "CameraShake", 1) != 0;
            brickMotion = PlayerPrefs.GetInt(Prefix + "BrickMotion", 1) != 0;
            view = new SettingsModalView(transform, scoreText.font, headingTypeface, Open, Close, ReturnToMenu,
                SetMusic, SetSfx, SetCameraShake, SetBrickMotion, RestoreDefaults);
            view.SetValues(music, sfx, cameraShake, brickMotion);
            ready = true;
            RefreshRuntimeSettings();
        }

        private static float ReadVolume(string key)
        {
            float value = PlayerPrefs.GetFloat(Prefix + key, 1);
            return float.IsNaN(value) || float.IsInfinity(value) ? 1 : Mathf.Clamp01(value);
        }

        private void OnEnable()
        {
            GameEvents.ScoreChanged += OnScore;
            refreshPending = true;
            reorderCamera = true;
            view?.SetAvailable(true);
        }

        private void Update()
        {
            if (!ready) return;
            if (refreshPending) RefreshRuntimeSettings();
            bool escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            if (escape)
            {
                if (open) Close();
                else Open();
            }
            // The existing game uses legacy keyboard reads, while its UI uses InputSystemUIInputModule.
            // Reset only the legacy frame before those Update methods run; modal raycasts block hotbar clicks.
            if (waitForSpellRelease && !SpellKeyHeld()) waitForSpellRelease = false;
            if (open || waitForSpellRelease || Time.frameCount <= suppressThroughFrame) UnityEngine.Input.ResetInputAxes();
            if (open && openedDuringPlay && !session.IsPlaying) Close();
            view.SetEntryContext(session.IsPlaying);
        }

        private static bool SpellKeyHeld()
        {
            Keyboard keys = Keyboard.current;
            return keys != null && (keys.spaceKey.isPressed || keys.digit1Key.isPressed || keys.digit2Key.isPressed ||
                keys.digit3Key.isPressed || keys.numpad1Key.isPressed || keys.numpad2Key.isPressed || keys.numpad3Key.isPressed);
        }

        private void LateUpdate()
        {
            if (!ready) return;
            // GameFeel may re-subscribe its render callback on round resets. Reorder only when needed.
            if (reorderCamera || cameraVersion != impactMotion.ResetVersion)
            {
                RenderPipelineManager.beginCameraRendering -= BeginCamera;
                RenderPipelineManager.beginCameraRendering += BeginCamera;
                cameraVersion = impactMotion.ResetVersion;
                reorderCamera = false;
            }
        }

        private void BeginCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera != worldCamera) return;
            if (!cameraShake || open) camera.ResetWorldToCameraMatrix();
            if (!brickMotion)
            {
                // The existing reaction writes its child in LateUpdate. Suppress only that visual pose.
                foreach (Transform visual in brickVisuals)
                {
                    if (visual == null) continue;
                    visual.localPosition = Vector3.zero;
                    visual.localScale = Vector3.one;
                }
            }
        }

        private void Open()
        {
            if (!ready || open) return;
            open = true;
            openedDuringPlay = session.IsPlaying;
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            previousTimeScale = Time.timeScale;
            ownsPause = openedDuringPlay;
            if (ownsPause) Time.timeScale = 0;
            reorderCamera = true;
            suppressThroughFrame = Time.frameCount;
            UnityEngine.Input.ResetInputAxes();
            view.Show(openedDuringPlay);
        }

        private void Close()
        {
            if (!open) return;
            open = false;
            waitForSpellRelease = true;
            RestorePause();
            suppressThroughFrame = Time.frameCount;
            UnityEngine.Input.ResetInputAxes();
            view?.Hide();
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(!session.IsPlaying && previousSelection != null && previousSelection.activeInHierarchy
                    ? previousSelection : null);
            PlayerPrefs.Save();
            reorderCamera = true;
        }

        private void ReturnToMenu()
        {
            if (!open || !openedDuringPlay) return;
            Close();
            session.ReturnToMainMenu();
        }

        private void RestorePause()
        {
            if (!ownsPause) return;
            Time.timeScale = previousTimeScale;
            ownsPause = false;
        }

        private void SetMusic(float value)
        {
            music = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(Prefix + "Music", music);
            ApplyMix();
        }

        private void SetSfx(float value)
        {
            sfx = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(Prefix + "SFX", sfx);
            ApplyMix();
        }

        private void SetCameraShake(bool value)
        {
            cameraShake = value;
            PlayerPrefs.SetInt(Prefix + "CameraShake", value ? 1 : 0);
            reorderCamera = true;
        }

        private void SetBrickMotion(bool value)
        {
            brickMotion = value;
            PlayerPrefs.SetInt(Prefix + "BrickMotion", value ? 1 : 0);
            CacheBrickVisuals();
        }

        private void RestoreDefaults()
        {
            SetMusic(1);
            SetSfx(1);
            SetCameraShake(true);
            SetBrickMotion(true);
            view.SetValues(music, sfx, cameraShake, brickMotion);
        }

        private static float Decibels(float value) => value <= 0.0001f ? -80 : 20 * Mathf.Log10(value);

        private void ApplyMix()
        {
            // Start/Update/UI callbacks only: AudioMixer.SetFloat must not run in Awake/OnEnable.
            bool musicReady = mixer.SetFloat("MusicVolume", Decibels(music));
            bool sfxReady = mixer.SetFloat("SFXVolume", Decibels(sfx));
            if (!musicReady || !sfxReady)
                Debug.LogError("CourtSettings mixer exposed volume parameters are missing.", this);
        }

        private void RefreshRuntimeSettings()
        {
            refreshPending = false;
            foreach (AudioSource source in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (source.gameObject.scene != gameObject.scene || originalRoutes.ContainsKey(source)) continue;
                originalRoutes.Add(source, source.outputAudioMixerGroup);
                source.outputAudioMixerGroup = source.transform.IsChildOf(musicRoot) ? musicBus : sfxBus;
            }
            ApplyMix();
            CacheBrickVisuals();
            reorderCamera = true;
        }

        private void CacheBrickVisuals()
        {
            brickVisuals.Clear();
            foreach (BrickVisualReaction reaction in boardRoot.GetComponentsInChildren<BrickVisualReaction>())
            {
                Transform visual = reaction.transform.Find("Brick reaction visual - collider free");
                if (visual != null) brickVisuals.Add(visual);
            }
        }

        private void OnScore(int score)
        {
            if (score != 0 || !ready) return;
            // New bricks exist before this event; discard destroyed-round references before caching them.
            CacheBrickVisuals();
            reorderCamera = true;
        }

        private void OnDisable()
        {
            GameEvents.ScoreChanged -= OnScore;
            RenderPipelineManager.beginCameraRendering -= BeginCamera;
            Close();
            RestorePause();
            foreach (var item in originalRoutes) if (item.Key != null) item.Key.outputAudioMixerGroup = item.Value;
            originalRoutes.Clear();
            brickVisuals.Clear();
            view?.SetAvailable(false);
        }

        private void OnDestroy()
        {
            GameEvents.ScoreChanged -= OnScore;
            RenderPipelineManager.beginCameraRendering -= BeginCamera;
            RestorePause();
            view?.Dispose();
        }
    }
}
