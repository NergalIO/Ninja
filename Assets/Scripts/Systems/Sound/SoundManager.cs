using System.Collections;
using System.Collections.Generic;
using Ninja.Systems.Settings;
using Ninja.Core;
using UnityEngine;

namespace Ninja.Audio
{
    public class SoundManager : PersistentSingleton<SoundManager>
    {
        private static class Settings
        {
            public const string GlobalVolume = "sound_global_volume";
            public const string MusicVolume = "sound_music_volume";
            public const string SfxVolume = "sound_sfx_volume";
            public const string MuteAll = "sound_mute_all";
            public const string MuteMusic = "sound_mute_music";
            public const string MuteSfx = "sound_mute_sfx";
        }

        [Header("Volumes")]
        [SerializeField, Range(0f, 1f)] private float globalVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        [Header("Mute")]
        [SerializeField] private bool muteAll;
        [SerializeField] private bool muteMusic;
        [SerializeField] private bool muteSfx;

        [Header("Runtime")]
        [SerializeField] private SoundSource musicSource;
        [SerializeField] private int maxSfxSources = 16;

        [Header("Playlist")]
        [SerializeField] private bool autoPlayMusic;
        [SerializeField, Range(0.1f, 5f)] private float fadeDuration = 1f;

        private readonly Dictionary<string, AudioClip> musicClips = new();
        private readonly Dictionary<string, AudioClip> sfxClips = new();
        private readonly List<SoundSource> sfxSources = new();
        private readonly List<string> musicKeys = new();
        
        private Coroutine playlistCoroutine;
        private int lastTrackIndex = -1;

        public float GlobalVolume => globalVolume;
        public float MusicVolume => musicVolume;
        public float SfxVolume => sfxVolume;

        protected override void OnSingletonInitialized()
        {
            base.OnSingletonInitialized();
            
            LoadClips();
            InitMusicSource();
            LoadSettings();
            
            if (autoPlayMusic)
                StartPlaylist();
        }

        private void LateUpdate() => CleanupSfxSources();

        private void LoadClips()
        {
            foreach (var clip in Resources.LoadAll<AudioClip>("Sounds/Music"))
            {
                musicClips[clip.name] = clip;
                musicKeys.Add(clip.name);
            }

            foreach (var clip in Resources.LoadAll<AudioClip>("Sounds/SFX"))
                sfxClips[clip.name] = clip;
        }

        private void InitMusicSource()
        {
            if (musicSource != null) return;
            
            musicSource = GetComponentInChildren<SoundSource>(true);
            if (musicSource != null) return;

            var go = new GameObject("MusicSource");
            go.transform.SetParent(transform);
            musicSource = go.AddComponent<SoundSource>();
            musicSource.SetDestroyOnClipEnd(false);
            musicSource.AudioSource.loop = true;
        }

        private void LoadSettings()
        {
            var sm = SettingsManager.Instance;
            if (sm == null) return;

            globalVolume = sm.GetSettingValue<float>(Settings.GlobalVolume);
            musicVolume = sm.GetSettingValue<float>(Settings.MusicVolume);
            sfxVolume = sm.GetSettingValue<float>(Settings.SfxVolume);
            muteAll = sm.GetSettingValue<bool>(Settings.MuteAll);
            muteMusic = sm.GetSettingValue<bool>(Settings.MuteMusic);
            muteSfx = sm.GetSettingValue<bool>(Settings.MuteSfx);

            sm.RegisterAction<float>(Settings.GlobalVolume, v => { globalVolume = v; ApplyVolumes(); });
            sm.RegisterAction<float>(Settings.MusicVolume, v => { musicVolume = v; ApplyVolumes(); });
            sm.RegisterAction<float>(Settings.SfxVolume, v => { sfxVolume = v; ApplyVolumes(); });
            sm.RegisterAction<bool>(Settings.MuteAll, v => { muteAll = v; ApplyVolumes(); });
            sm.RegisterAction<bool>(Settings.MuteMusic, v => { muteMusic = v; ApplyVolumes(); });
            sm.RegisterAction<bool>(Settings.MuteSfx, v => { muteSfx = v; ApplyVolumes(); });

            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            float music = (muteAll || muteMusic) ? 0f : globalVolume * musicVolume;
            float sfx = (muteAll || muteSfx) ? 0f : globalVolume * sfxVolume;

            musicSource?.SetVolume(music);
            foreach (var s in sfxSources)
                s?.SetVolume(sfx);
        }

        // === Public API ===

        public void SetGlobalVolume(float v) => SettingsManager.Instance?.SetSettingValue(Settings.GlobalVolume, Mathf.Clamp01(v));
        public void SetMusicVolume(float v) => SettingsManager.Instance?.SetSettingValue(Settings.MusicVolume, Mathf.Clamp01(v));
        public void SetSfxVolume(float v) => SettingsManager.Instance?.SetSettingValue(Settings.SfxVolume, Mathf.Clamp01(v));
        public void SetMuteAll(bool m) => SettingsManager.Instance?.SetSettingValue(Settings.MuteAll, m);
        public void SetMuteMusic(bool m) => SettingsManager.Instance?.SetSettingValue(Settings.MuteMusic, m);
        public void SetMuteSfx(bool m) => SettingsManager.Instance?.SetSettingValue(Settings.MuteSfx, m);

        public bool PlayMusic(string id, bool restart = false)
        {
            StopPlaylist();
            
            if (!musicClips.TryGetValue(id, out var clip))
                return false;

            var audio = musicSource.AudioSource;
            if (!restart && audio.isPlaying && audio.clip == clip)
                return true;

            audio.clip = clip;
            audio.loop = true;
            audio.Play();
            ApplyVolumes();
            return true;
        }

        public void StopMusic()
        {
            StopPlaylist();
            musicSource?.AudioSource.Stop();
        }

        public void StartPlaylist()
        {
            if (musicKeys.Count == 0) return;
            
            StopPlaylist();
            playlistCoroutine = StartCoroutine(PlaylistLoop());
        }

        public void StopPlaylist()
        {
            if (playlistCoroutine != null)
            {
                StopCoroutine(playlistCoroutine);
                playlistCoroutine = null;
            }
        }

        public void NextTrack()
        {
            StopPlaylist();
            StartPlaylist();
        }

        public SoundSource PlaySfx(string id, Vector3? position = null, float spatialBlend = 0f)
        {
            if (!sfxClips.TryGetValue(id, out var clip))
                return null;

            CleanupSfxSources();
            
            if (sfxSources.Count >= maxSfxSources && sfxSources.Count > 0)
            {
                Destroy(sfxSources[0]?.gameObject);
                sfxSources.RemoveAt(0);
            }

            var go = new GameObject($"SFX_{id}");
            go.transform.SetParent(transform);
            go.transform.position = position ?? transform.position;

            var source = go.AddComponent<SoundSource>();
            source.SetDestroyOnClipEnd(true);
            
            var audio = source.AudioSource;
            audio.clip = clip;
            audio.spatialBlend = spatialBlend;
            audio.Play();

            sfxSources.Add(source);
            ApplyVolumes();
            return source;
        }

        private IEnumerator PlaylistLoop()
        {
            var audio = musicSource.AudioSource;
            audio.loop = false;

            while (true)
            {
                int index;
                do { index = Random.Range(0, musicKeys.Count); }
                while (index == lastTrackIndex && musicKeys.Count > 1);
                
                lastTrackIndex = index;
                var clip = musicClips[musicKeys[index]];

                // Fade out
                if (audio.isPlaying)
                    yield return Fade(audio, 0f, fadeDuration);

                audio.clip = clip;
                audio.volume = 0f;
                audio.Play();

                // Fade in
                yield return Fade(audio, (muteAll || muteMusic) ? 0f : globalVolume * musicVolume, fadeDuration);

                // Wait for track to end
                yield return new WaitForSeconds(clip.length - fadeDuration);
            }
        }

        private IEnumerator Fade(AudioSource audio, float target, float duration)
        {
            float start = audio.volume;
            float t = 0f;
            
            while (t < duration)
            {
                t += Time.deltaTime;
                audio.volume = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            
            audio.volume = target;
        }

        private void CleanupSfxSources()
        {
            for (int i = sfxSources.Count - 1; i >= 0; i--)
                if (sfxSources[i] == null)
                    sfxSources.RemoveAt(i);
        }
    }
}
