using System;
using System.Collections.Generic;
using Ninja.Utils;
using UnityEngine;

namespace Ninja.Managers {
    public class SoundManager : PersistentSingleton<SoundManager> {
        private const string MusicSourceName = "Music Source";

        private static class PrefKeys {
            public const string GlobalVolume = "sound_global_volume";
            public const string MusicVolume = "sound_music_volume";
            public const string SfxVolume = "sound_sfx_volume";
            public const string MuteAll = "sound_mute_all";
            public const string MuteMusic = "sound_mute_music";
            public const string MuteSfx = "sound_mute_sfx";

            public const string RuntimeMusicVolume = "sound_runtime_music_volume";
            public const string RuntimeSfxVolume = "sound_runtime_sfx_volume";
            public const string RuntimeMusicMute = "sound_runtime_music_mute";
            public const string RuntimeSfxMute = "sound_runtime_sfx_mute";
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

        public IReadOnlyDictionary<string, AudioClip> MusicClips => musicClips;
        public IReadOnlyDictionary<string, AudioClip> SfxClips => sfxClips;

        protected override void OnSingletonInitialized() {
            base.OnSingletonInitialized();
            dynamicSourcesRoot ??= transform;
            LoadClipsFromResources();
            BuildClipDictionaries();
            LoadSettings();
            InitializeMusicSource();
            ApplySettingsToRuntimeSources(savePreferences: true);

            if (autoPlayRandomMusic) {
                StartRandomMusicPlaylist();
            }
        }

        private void LateUpdate() {
            CleanupFinishedSfxSources();
        }

        #region Public API

        public void SetGlobalVolume(float value) {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(globalVolume, value)) {
                return;
            }

            globalVolume = value;
            PlayerPrefs.SetFloat(PrefKeys.GlobalVolume, globalVolume);
            ApplySettingsToRuntimeSources();
        }

        public void SetMusicVolume(float value) {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(musicVolume, value)) {
                return;
            }

            musicVolume = value;
            PlayerPrefs.SetFloat(PrefKeys.MusicVolume, musicVolume);
            ApplySettingsToRuntimeSources();
        }

        public void SetSfxVolume(float value) {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(sfxVolume, value)) {
                return;
            }

            sfxVolume = value;
            PlayerPrefs.SetFloat(PrefKeys.SfxVolume, sfxVolume);
            ApplySettingsToRuntimeSources();
        }

        public void SetMuteAll(bool muted) {
            if (muteAll == muted) {
                return;
            }

            muteAll = muted;
            PlayerPrefs.SetInt(PrefKeys.MuteAll, BoolToInt(muteAll));
            ApplySettingsToRuntimeSources();
        }

        public void SetMuteMusic(bool muted) {
            if (muteMusic == muted) {
                return;
            }

            muteMusic = muted;
            PlayerPrefs.SetInt(PrefKeys.MuteMusic, BoolToInt(muteMusic));
            ApplySettingsToRuntimeSources();
        }

        public void SetMuteSfx(bool muted) {
            if (muteSFX == muted) {
                return;
            }

            muteSFX = muted;
            PlayerPrefs.SetInt(PrefKeys.MuteSfx, BoolToInt(muteSFX));
            ApplySettingsToRuntimeSources();
        }

        public bool PlayMusic(string clipId, bool restartIfSame = false) {
            // Останавливаем случайный плейлист, если запуск музыки происходит вручную
            StopRandomMusicPlaylist(stopCurrent: false);

            if (!musicClips.TryGetValue(clipId, out var clip) || clip == null) {
                Debug.LogWarning($"[SoundManager] Music clip '{clipId}' not found.");
                return false;
            }

            InitializeMusicSource();

            var audio = musicSource.AudioSource;
            if (!restartIfSame && audio.isPlaying && audio.clip == clip) {
                return true;
            }

            audio.clip = clip;
            audio.loop = true;
            audio.playOnAwake = false;
            audio.Play();
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
            return true;
        }

        /// <summary>
        /// Пропускает текущий трек и переходит к следующему случайному с плавным переходом.
        /// Если плейлист ещё не запущен – просто запускает его.
        /// </summary>
        public void SkipToNextRandomTrack() {
            if (!musicPlaylistActive) {
                StartRandomMusicPlaylist();
                return;
            }

            if (musicSource == null) {
                return;
            }

            requestNextRandomTrack = true;
        }

        /// <summary>
        /// Запускает бесконечный случайный плейлист музыки с плавными переходами между треками.
        /// </summary>
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
        }

        /// <summary>
        /// Останавливает случайный плейлист. Опционально глушит текущий трек.
        /// </summary>
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
                if (clip == null) {
                    continue;
                }

                musicLibrary.Add(new ClipEntry { id = clip.name, clip = clip });
            }

            foreach (var clip in sfxResources) {
                if (clip == null) {
                    continue;
                }

                sfxLibrary.Add(new ClipEntry { id = clip.name, clip = clip });
            }
        }

        private void BuildClipDictionaries() {
            musicClips.Clear();
            sfxClips.Clear();
            musicKeys.Clear();

            foreach (var entry in musicLibrary) {
                if (entry.clip == null) {
                    continue;
                }

                var key = string.IsNullOrWhiteSpace(entry.id) ? entry.clip.name : entry.id;
                musicClips[key] = entry.clip;
                musicKeys.Add(key);
            }

            foreach (var entry in sfxLibrary) {
                if (entry.clip == null) {
                    continue;
                }

                var key = string.IsNullOrWhiteSpace(entry.id) ? entry.clip.name : entry.id;
                sfxClips[key] = entry.clip;
            }
        }

        private void LoadSettings() {
            globalVolume = PlayerPrefs.GetFloat(PrefKeys.GlobalVolume, Mathf.Clamp01(globalVolume));
            musicVolume = PlayerPrefs.GetFloat(PrefKeys.MusicVolume, Mathf.Clamp01(musicVolume));
            sfxVolume = PlayerPrefs.GetFloat(PrefKeys.SfxVolume, Mathf.Clamp01(sfxVolume));

            muteAll = PlayerPrefs.GetInt(PrefKeys.MuteAll, BoolToInt(muteAll)) == 1;
            muteMusic = PlayerPrefs.GetInt(PrefKeys.MuteMusic, BoolToInt(muteMusic)) == 1;
            muteSFX = PlayerPrefs.GetInt(PrefKeys.MuteSfx, BoolToInt(muteSFX)) == 1;
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
            }

            musicSource.ConfigurePlayerPrefKeys(PrefKeys.RuntimeMusicVolume, PrefKeys.RuntimeMusicMute);
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
            source.ConfigurePlayerPrefKeys(PrefKeys.RuntimeSfxVolume, PrefKeys.RuntimeSfxMute);
            source.SetDestroyOnClipEnd(true);
            activeSfxSources.Add(source);
            return source;
        }

        #endregion

        #region Helpers

        private void ApplySettingsToRuntimeSources(bool savePreferences = true) {
            var musicMuted = muteAll || muteMusic;
            var sfxMuted = muteAll || muteSFX;

            var resolvedMusicVolume = musicMuted ? 0f : globalVolume * musicVolume;
            var resolvedSfxVolume = sfxMuted ? 0f : globalVolume * sfxVolume;

            PlayerPrefs.SetFloat(PrefKeys.RuntimeMusicVolume, resolvedMusicVolume);
            PlayerPrefs.SetFloat(PrefKeys.RuntimeSfxVolume, resolvedSfxVolume);
            PlayerPrefs.SetInt(PrefKeys.RuntimeMusicMute, BoolToInt(musicMuted));
            PlayerPrefs.SetInt(PrefKeys.RuntimeSfxMute, BoolToInt(sfxMuted));

            if (savePreferences) {
                PlayerPrefs.Save();
            }

            musicSource?.ForceSync();
            foreach (var source in activeSfxSources) {
                source?.ForceSync();
            }
        }

        private System.Collections.IEnumerator RandomMusicPlaylistCoroutine() {
            if (musicSource == null) {
                yield break;
            }

            var audio = musicSource.AudioSource;
            audio.loop = false;
            audio.playOnAwake = false;

            // Обновляем список доступных ключей на случай, если библиотека изменилась
            if (musicKeys.Count == 0) {
                yield break;
            }

            // Основной цикл плейлиста
            while (musicPlaylistActive) {
                if (musicKeys.Count == 0) {
                    yield break;
                }

                int index;
                if (musicKeys.Count == 1) {
                    index = 0;
                } else {
                    // Не повторяем один и тот же трек подряд
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

                // Плавно переходим к следующему треку
                yield return FadeToClipCoroutine(clip, musicFadeDuration);

                if (!musicPlaylistActive) {
                    yield break;
                }

                // Ждём почти до конца трека, оставляя время на затухание.
                // При запросе следующего трека выходим раньше и сразу переходим к следующему.
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

        private System.Collections.IEnumerator FadeToClipCoroutine(AudioClip newClip, float fadeDuration) {
            if (musicSource == null) {
                yield break;
            }

            var audio = musicSource.AudioSource;
            var initialVolume = audio.volume;

            // Затухание текущего трека
            if (audio.isPlaying && audio.clip != null && fadeDuration > 0f) {
                var t = 0f;
                while (t < fadeDuration) {
                    if (!musicPlaylistActive) {
                        yield break;
                    }

                    t += Time.deltaTime;
                    var k = Mathf.Clamp01(t / fadeDuration);
                    audio.volume = Mathf.Lerp(initialVolume, 0f, k);
                    yield return null;
                }
            }

            audio.Stop();
            audio.clip = newClip;
            audio.loop = false;

            if (newClip == null) {
                yield break;
            }

            // Включаем новый трек с нарастанием громкости
            audio.volume = 0f;
            audio.Play();

            if (fadeDuration <= 0f) {
                audio.volume = initialVolume;
                yield break;
            }

            var tFadeIn = 0f;
            while (tFadeIn < fadeDuration) {
                if (!musicPlaylistActive) {
                    yield break;
                }

                tFadeIn += Time.deltaTime;
                var k = Mathf.Clamp01(tFadeIn / fadeDuration);
                audio.volume = Mathf.Lerp(0f, initialVolume, k);
                yield return null;
            }

            audio.volume = initialVolume;
        }

        private void CleanupFinishedSfxSources() {
            for (int i = activeSfxSources.Count - 1; i >= 0; i--) {
                if (activeSfxSources[i] == null) {
                    activeSfxSources.RemoveAt(i);
                }
            }
        }

        private static int BoolToInt(bool value) => value ? 1 : 0;

        #endregion
    }
}
