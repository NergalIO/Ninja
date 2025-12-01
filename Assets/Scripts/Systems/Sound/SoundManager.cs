using System;
using System.Collections;
using System.Collections.Generic;
using Ninja.Systems.Settings;
using Ninja.Core;
using UnityEngine;
using Unity.VisualScripting;

namespace Ninja.Audio {
    public class SoundManager : PersistentSingleton<SoundManager> {
        private const string MusicSourceName = "Music Source";

        private static class SettingKeys {
            public const string GlobalVolume = "sound_global_volume";
            public const string MusicVolume = "sound_music_volume";
            public const string SfxVolume = "sound_sfx_volume";
            public const string MuteAll = "sound_mute_all";
            public const string MuteMusic = "sound_mute_music";
            public const string MuteSfx = "sound_mute_sfx";
        }

        [Serializable]
        private struct ClipEntry {
            public string id;
            public AudioClip clip;
        }

        [Header("Volumes")]
        [Range(0f, 1f)][SerializeField] private float globalVolume = 1f;
        [Range(0f, 1f)][SerializeField] private float musicVolume = 1f;
        [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

        [Header("Mute")]
        [SerializeField] private bool muteAll;
        [SerializeField] private bool muteMusic;
        [SerializeField] private bool muteSFX;

        [Header("Runtime")]
        [SerializeField] private SoundSource musicSource;
        [SerializeField] private Transform dynamicSourcesRoot;
        [SerializeField] private int maxSFXSources = 16;
        [SerializeField] private bool debugMode = true;

        [Header("Music Playlist")]
        [SerializeField] private bool autoPlayRandomMusic = false;
        [SerializeField] [Range(0.05f, 10f)] private float musicFadeDuration = 1f;
        [SerializeField] [Range(0f, 5f)] private float gapBetweenTracks = 0.1f;

        [Header("Libraries")]
        [SerializeField] private List<ClipEntry> musicLibrary = new();
        [SerializeField] private List<ClipEntry> sfxLibrary = new();

        private readonly Dictionary<string, AudioClip> musicClips = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AudioClip> sfxClips = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<SoundSource> activeSfxSources = new();

        // Состояние плейлиста музыки
        private Coroutine musicPlaylistCoroutine;
        private bool musicPlaylistActive;
        private readonly List<string> musicKeys = new();
        private System.Random musicRandom = new System.Random();
        private int lastRandomMusicIndex = -1;
        private bool requestNextRandomTrack;

        private SettingsManager settingsManager;

        public IReadOnlyDictionary<string, AudioClip> MusicClips => musicClips;
        public IReadOnlyDictionary<string, AudioClip> SfxClips => sfxClips;

        protected override void OnSingletonInitialized() {
            base.OnSingletonInitialized();
            
            if (debugMode)
            {
                Debug.Log("[SoundManager] Initializing SoundManager...");
            }

            dynamicSourcesRoot ??= transform;
            settingsManager = SettingsManager.Instance;
            
            LoadClipsFromResources();
            BuildClipDictionaries();
            LoadSettingsFromManager();
            InitializeMusicSource();
            RegisterSettingsCallbacks();
            ApplySettingsToRuntimeSources();

            if (autoPlayRandomMusic) {
                StartRandomMusicPlaylist();
            }

            if (debugMode)
            {
                Debug.Log("[SoundManager] Initialization complete");
            }
        }

        protected override void OnDestroy()
        {
            UnregisterSettingsCallbacks();
            base.OnDestroy();
        }

        private void LateUpdate() {
            CleanupFinishedSfxSources();
        }

        #region Settings Management

        private void LoadSettingsFromManager()
        {
            if (settingsManager == null)
            {
                Debug.LogWarning("[SoundManager] SettingsManager is not initialized. Waiting for initialization...");
                StartCoroutine(WaitForSettingsManager());
                return;
            }

            globalVolume = settingsManager.GetSettingValue<float>(SettingKeys.GlobalVolume);
            musicVolume = settingsManager.GetSettingValue<float>(SettingKeys.MusicVolume);
            sfxVolume = settingsManager.GetSettingValue<float>(SettingKeys.SfxVolume);
            muteAll = settingsManager.GetSettingValue<bool>(SettingKeys.MuteAll);
            muteMusic = settingsManager.GetSettingValue<bool>(SettingKeys.MuteMusic);
            muteSFX = settingsManager.GetSettingValue<bool>(SettingKeys.MuteSfx);

            if (debugMode)
            {
                Debug.Log($"[SoundManager] Loaded settings - Global: {globalVolume}, Music: {musicVolume}, SFX: {sfxVolume}");
            }
            ApplySettingsToRuntimeSources();
        }

        private IEnumerator WaitForSettingsManager()
        {
            while (SettingsManager.Instance == null)
                yield return null;

            settingsManager = SettingsManager.Instance;
            LoadSettingsFromManager();
            RegisterSettingsCallbacks();
            ApplySettingsToRuntimeSources();
        }

        private void RegisterSettingsCallbacks()
        {
            if (settingsManager == null) return;

            settingsManager.RegisterAction<float>(SettingKeys.GlobalVolume, OnGlobalVolumeChanged);
            settingsManager.RegisterAction<float>(SettingKeys.MusicVolume, OnMusicVolumeChanged);
            settingsManager.RegisterAction<float>(SettingKeys.SfxVolume, OnSfxVolumeChanged);
            settingsManager.RegisterAction<bool>(SettingKeys.MuteAll, OnMuteAllChanged);
            settingsManager.RegisterAction<bool>(SettingKeys.MuteMusic, OnMuteMusicChanged);
            settingsManager.RegisterAction<bool>(SettingKeys.MuteSfx, OnMuteSfxChanged);

            if (debugMode)
            {
                Debug.Log("[SoundManager] Registered all settings callbacks");
            }
        }

        private void UnregisterSettingsCallbacks()
        {
            // Примечание: Если нужна отписка, добавьте список зарегистрированных callbacks
            // и удаляйте их здесь
        }

        private void OnGlobalVolumeChanged(float newValue)
        {
            globalVolume = newValue;
            if (debugMode) Debug.Log($"[SoundManager] Global volume changed: {newValue}");
            ApplySettingsToRuntimeSources();
        }

        private void OnMusicVolumeChanged(float newValue)
        {
            musicVolume = newValue;
            if (debugMode) Debug.Log($"[SoundManager] Music volume changed: {newValue}");
            ApplySettingsToRuntimeSources();
        }

        private void OnSfxVolumeChanged(float newValue)
        {
            sfxVolume = newValue;
            if (debugMode) Debug.Log($"[SoundManager] SFX volume changed: {newValue}");
            ApplySettingsToRuntimeSources();
        }

        private void OnMuteAllChanged(bool newValue)
        {
            muteAll = newValue;
            if (debugMode) Debug.Log($"[SoundManager] Mute all changed: {newValue}");
            ApplySettingsToRuntimeSources();
        }

        private void OnMuteMusicChanged(bool newValue)
        {
            muteMusic = newValue;
            if (debugMode) Debug.Log($"[SoundManager] Mute music changed: {newValue}");
            ApplySettingsToRuntimeSources();
        }

        private void OnMuteSfxChanged(bool newValue)
        {
            muteSFX = newValue;
            if (debugMode) Debug.Log($"[SoundManager] Mute SFX changed: {newValue}");
            ApplySettingsToRuntimeSources();
        }

        #endregion

        #region Public API

        public void SetGlobalVolume(float value) {
            value = Mathf.Clamp01(value);
            settingsManager.SetSettingValue(SettingKeys.GlobalVolume, value);
        }

        public void SetMusicVolume(float value) {
            value = Mathf.Clamp01(value);
            settingsManager.SetSettingValue(SettingKeys.MusicVolume, value);
        }

        public void SetSfxVolume(float value) {
            value = Mathf.Clamp01(value);
            settingsManager.SetSettingValue(SettingKeys.SfxVolume, value);
        }

        public void SetMuteAll(bool muted) {
            settingsManager.SetSettingValue(SettingKeys.MuteAll, muted);
        }

        public void SetMuteMusic(bool muted) {
            settingsManager.SetSettingValue(SettingKeys.MuteMusic, muted);
        }

        public void SetMuteSfx(bool muted) {
            settingsManager.SetSettingValue(SettingKeys.MuteSfx, muted);
        }

        public float GetGlobalVolume() => settingsManager.GetSettingValue<float>(SettingKeys.GlobalVolume);
        public float GetMusicVolume() => settingsManager.GetSettingValue<float>(SettingKeys.MusicVolume);
        public float GetSfxVolume() => settingsManager.GetSettingValue<float>(SettingKeys.SfxVolume);
        public bool GetMuteAll() => settingsManager.GetSettingValue<bool>(SettingKeys.MuteAll);
        public bool GetMuteMusic() => settingsManager.GetSettingValue<bool>(SettingKeys.MuteMusic);
        public bool GetMuteSfx() => settingsManager.GetSettingValue<bool>(SettingKeys.MuteSfx);

        public bool PlayMusic(string clipId, bool restartIfSame = false) {
            StopRandomMusicPlaylist(stopCurrent: false);

            if (!musicClips.TryGetValue(clipId, out var clip) || clip == null) {
                Debug.LogWarning($"[SoundManager] Music clip '{clipId}' not found.");
                return false;
            }

            InitializeMusicSource();

            var audio = musicSource.AudioSource;
            if (!restartIfSame && audio.isPlaying && audio.clip == clip) {
                ApplySettingsToRuntimeSources();
                return true;
            }

            audio.clip = clip;
            audio.loop = true;
            audio.playOnAwake = false;
            audio.Play();

            ApplySettingsToRuntimeSources();

            if (debugMode)
            {
                Debug.Log($"[SoundManager] Playing music: '{clipId}'");
            }

            return true;
        }

        public bool StopMusic() {
            if (musicSource == null) {
                return false;
            }

            var audio = musicSource.AudioSource;
            if (!audio.isPlaying) {
                return false;
            }

            audio.Stop();

            if (debugMode)
            {
                Debug.Log("[SoundManager] Music stopped");
            }

            return true;
        }

        public void SkipToNextRandomTrack() {
            if (!musicPlaylistActive) {
                StartRandomMusicPlaylist();
                return;
            }

            if (musicSource == null) {
                return;
            }

            requestNextRandomTrack = true;

            if (debugMode)
            {
                Debug.Log("[SoundManager] Skipping to next random track");
            }
        }

        public void StartRandomMusicPlaylist() {
            if (musicClips.Count == 0) {
                Debug.LogWarning("[SoundManager] Cannot start random music playlist: music library is empty.");
                return;
            }

            InitializeMusicSource();

            if (musicPlaylistCoroutine != null) {
                StopCoroutine(musicPlaylistCoroutine);
            }

            musicPlaylistActive = true;
            musicPlaylistCoroutine = StartCoroutine(RandomMusicPlaylistCoroutine());

            if (debugMode)
            {
                Debug.Log("[SoundManager] Random music playlist started");
            }
        }

        public void StopRandomMusicPlaylist(bool stopCurrent = true) {
            if (!musicPlaylistActive && musicPlaylistCoroutine == null) {
                return;
            }

            musicPlaylistActive = false;
            if (musicPlaylistCoroutine != null) {
                StopCoroutine(musicPlaylistCoroutine);
                musicPlaylistCoroutine = null;
            }

            if (!stopCurrent || musicSource == null) {
                return;
            }

            var audio = musicSource.AudioSource;
            audio.Stop();

            if (debugMode)
            {
                Debug.Log("[SoundManager] Random music playlist stopped");
            }
        }

        public SoundSource PlaySfx(string clipId, Vector3 position, float spatialBlend = 0f) {
            if (!sfxClips.TryGetValue(clipId, out var clip) || clip == null) {
                Debug.LogWarning($"[SoundManager] SFX clip '{clipId}' not found.");
                return null;
            }

            CleanupFinishedSfxSources();
            if (activeSfxSources.Count >= maxSFXSources && activeSfxSources.Count > 0) {
                var oldest = activeSfxSources[0];
                if (oldest != null) {
                    Destroy(oldest.gameObject);
                }

                activeSfxSources.RemoveAt(0);
            }

            var sfxSource = CreateSfxSource(clipId, position);
            var audio = sfxSource.AudioSource;
            audio.clip = clip;
            audio.loop = false;
            audio.playOnAwake = false;
            audio.spatialBlend = Mathf.Clamp01(spatialBlend);
            audio.Play();

            if (debugMode)
            {
                Debug.Log($"[SoundManager] Playing SFX: '{clipId}' at position {position}");
            }

            return sfxSource;
        }

        public SoundSource PlaySfx(string clipId) {
            return PlaySfx(clipId, transform.position);
        }

        #endregion

        #region Initialization

        private void LoadClipsFromResources() {
            musicLibrary.Clear();
            sfxLibrary.Clear();

            var musicResources = Resources.LoadAll<AudioClip>("Sounds/Music");
            var sfxResources = Resources.LoadAll<AudioClip>("Sounds/SFX");

            foreach (var clip in musicResources) {
                if (clip == null) continue;
                musicLibrary.Add(new ClipEntry { id = clip.name, clip = clip });
            }

            foreach (var clip in sfxResources) {
                if (clip == null) continue;
                sfxLibrary.Add(new ClipEntry { id = clip.name, clip = clip });
            }

            if (debugMode)
            {
                Debug.Log($"[SoundManager] Loaded {musicResources.Length} music clips and {sfxResources.Length} SFX clips");
            }
        }

        private void BuildClipDictionaries() {
            musicClips.Clear();
            sfxClips.Clear();
            musicKeys.Clear();

            foreach (var entry in musicLibrary) {
                if (entry.clip == null) continue;
                var key = string.IsNullOrWhiteSpace(entry.id) ? entry.clip.name : entry.id;
                musicClips[key] = entry.clip;
                musicKeys.Add(key);
            }

            foreach (var entry in sfxLibrary) {
                if (entry.clip == null) continue;
                var key = string.IsNullOrWhiteSpace(entry.id) ? entry.clip.name : entry.id;
                sfxClips[key] = entry.clip;
            }
        }

        private void InitializeMusicSource() {
            if (musicSource == null) {
                musicSource = GetComponentInChildren<SoundSource>(includeInactive: true);
            }

            if (musicSource == null) {
                var existing = transform.Find(MusicSourceName);
                if (existing != null) {
                    musicSource = existing.GetComponent<SoundSource>();
                }
            }

            if (musicSource == null) {
                var go = new GameObject(MusicSourceName);
                go.transform.SetParent(transform);
                musicSource = go.AddComponent<SoundSource>();

                if (debugMode)
                {
                    Debug.Log("[SoundManager] Created new music source");
                }
            }

            musicSource.SetDestroyOnClipEnd(false);
            var audio = musicSource.AudioSource;
            audio.loop = true;
            audio.playOnAwake = false;
        }

        private SoundSource CreateSfxSource(string clipId, Vector3 position) {
            var go = new GameObject($"SFX_{clipId}_{Time.frameCount}");
            go.transform.SetParent(dynamicSourcesRoot, false);
            go.transform.position = position;

            var source = go.AddComponent<SoundSource>();
            source.SetDestroyOnClipEnd(true);
            activeSfxSources.Add(source);
            return source;
        }

        #endregion

        #region Helpers

        private void ApplySettingsToRuntimeSources() {
            var musicMuted = muteAll || muteMusic;
            var sfxMuted = muteAll || muteSFX;

            var resolvedMusicVolume = musicMuted ? 0f : globalVolume * musicVolume;
            var resolvedSfxVolume = sfxMuted ? 0f : globalVolume * sfxVolume;

            musicSource?.SetVolume(resolvedMusicVolume);
            musicSource?.SetMute(musicMuted);
            
            foreach (var source in activeSfxSources) {
                source?.SetVolume(resolvedSfxVolume);
                source?.SetMute(sfxMuted);
            }

            if (debugMode)
            {
                Debug.Log($"[SoundManager] Applied settings - Music Volume: {resolvedMusicVolume}, SFX Volume: {resolvedSfxVolume}");
            }
        }

        private IEnumerator RandomMusicPlaylistCoroutine() {
            if (musicSource == null) {
                yield break;
            }

            var audio = musicSource.AudioSource;
            audio.loop = false;
            audio.playOnAwake = false;

            if (musicKeys.Count == 0) {
                yield break;
            }

            while (musicPlaylistActive) {
                if (musicKeys.Count == 0) {
                    yield break;
                }

                int index;
                if (musicKeys.Count == 1) {
                    index = 0;
                } else {
                    do {
                        index = musicRandom.Next(musicKeys.Count);
                    } while (index == lastRandomMusicIndex && musicKeys.Count > 1);
                }

                lastRandomMusicIndex = index;
                var clipId = musicKeys[index];

                if (!musicClips.TryGetValue(clipId, out var clip) || clip == null) {
                    yield return null;
                    continue;
                }

                yield return FadeToClipCoroutine(clip, musicFadeDuration);

                if (!musicPlaylistActive) {
                    yield break;
                }

                var waitTime = Mathf.Max(0f, clip.length - musicFadeDuration);
                var elapsed = 0f;
                while (musicPlaylistActive && elapsed < waitTime && audio.clip == clip && audio.isPlaying && !requestNextRandomTrack) {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (!musicPlaylistActive) {
                    yield break;
                }

                if (requestNextRandomTrack) {
                    requestNextRandomTrack = false;
                    continue;
                }

                if (gapBetweenTracks > 0f) {
                    var gapElapsed = 0f;
                    while (musicPlaylistActive && gapElapsed < gapBetweenTracks && !requestNextRandomTrack) {
                        gapElapsed += Time.deltaTime;
                        yield return null;
                    }

                    if (!musicPlaylistActive) {
                        yield break;
                    }

                    if (requestNextRandomTrack) {
                        requestNextRandomTrack = false;
                    }
                }
            }
        }

        private IEnumerator FadeToClipCoroutine(AudioClip newClip, float fadeDuration) {
            if (musicSource == null) {
                yield break;
            }

            ApplySettingsToRuntimeSources();

            var audio = musicSource.AudioSource;
            var musicMuted = muteAll || muteMusic;
            var resolvedMusicVolume = musicMuted ? 0f : globalVolume * musicVolume;

            if (audio.isPlaying && audio.clip != null && fadeDuration > 0f) {
                var t = 0f;
                while (t < fadeDuration) {
                    if (!musicPlaylistActive) {
                        yield break;
                    }

                    t += Time.deltaTime;
                    var k = Mathf.Clamp01(t / fadeDuration);
                    audio.volume = Mathf.Lerp(resolvedMusicVolume, 0f, k);
                    yield return null;
                }
            }

            audio.Stop();
            audio.clip = newClip;
            audio.loop = false;

            if (newClip == null) {
                yield break;
            }

            audio.volume = 0f;
            audio.Play();

            if (fadeDuration <= 0f) {
                ApplySettingsToRuntimeSources();
                yield break;
            }

            var tFadeIn = 0f;
            while (tFadeIn < fadeDuration) {
                if (!musicPlaylistActive) {
                    yield break;
                }

                tFadeIn += Time.deltaTime;
                var k = Mathf.Clamp01(tFadeIn / fadeDuration);
                audio.volume = Mathf.Lerp(0f, resolvedMusicVolume, k);
                yield return null;
            }

            ApplySettingsToRuntimeSources();
        }

        private void CleanupFinishedSfxSources() {
            for (int i = activeSfxSources.Count - 1; i >= 0; i--) {
                if (activeSfxSources[i] == null) {
                    activeSfxSources.RemoveAt(i);
                }
            }
        }

        #endregion
    }
}

