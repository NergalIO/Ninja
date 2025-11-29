using Ninja.Utils;
using UnityEngine;
namespace Ninja.Audio {

    [RequireComponent(typeof(AudioSource))]
    public class SoundSource : MonoBehaviour {
        private const string DefaultVolumeKey = "sound_volume";
        private const string DefaultMuteKey = "sound_muted";

        [Header("PlayerPrefs keys")]
        [SerializeField] private string volumePrefKey = DefaultVolumeKey;
        [SerializeField] private string mutePrefKey = DefaultMuteKey;

        [Header("Defaults")]
        [Range(0f, 1f)][SerializeField] private float defaultVolume = 1f;
        [SerializeField] private bool defaultMute = false;

        [Header("Behaviour")]
        [Tooltip("How often (seconds) PlayerPrefs are polled for changes.")]
        [SerializeField] private float prefsPollInterval = 0.2f;
        [Tooltip("Destroy the GameObject after a non-looping clip finishes.")]
        [SerializeField] private bool destroyOnClipEnd = true;

        private AudioSource _audioSource;
        private float _cachedVolume = float.NaN;
        private bool _cachedMute;
        private bool _hasStartedPlaying;
        private float _nextPrefsRefreshTime;

        private void Awake() {
            _audioSource = this.GetOrAddComponent<AudioSource>();
            SyncFromPrefs(force: true);
        }

        private void OnEnable() {
            SyncFromPrefs(force: true);
        }

        private void Update() {
            if (_audioSource == null) {
                enabled = false;
                return;
            }

            SyncFromPrefs();
            TrackLifetime();
        }

        public AudioSource AudioSource {
            get {
                if (_audioSource == null) {
                    _audioSource = this.GetOrAddComponent<AudioSource>();
                }
                return _audioSource;
            }
        }

        public void ConfigurePlayerPrefKeys(string volumeKey, string muteKey, float fallbackVolume = 1f, bool fallbackMute = false) {
            volumePrefKey = string.IsNullOrEmpty(volumeKey) ? DefaultVolumeKey : volumeKey;
            mutePrefKey = string.IsNullOrEmpty(muteKey) ? DefaultMuteKey : muteKey;
            defaultVolume = Mathf.Clamp01(fallbackVolume);
            defaultMute = fallbackMute;
            
            // Инициализируем AudioSource если он еще не инициализирован
            if (_audioSource == null) {
                _audioSource = this.GetOrAddComponent<AudioSource>();
            }
            
            SyncFromPrefs(force: true);
        }

        public void SetDestroyOnClipEnd(bool value) {
            destroyOnClipEnd = value;
        }

        public void ForceSync() {
            SyncFromPrefs(force: true);
        }

        private void SyncFromPrefs(bool force = false) {
            if (_audioSource == null) {
                _audioSource = this.GetOrAddComponent<AudioSource>();
                if (_audioSource == null) {
                    return;
                }
            }
            
            if (!force && Time.unscaledTime < _nextPrefsRefreshTime) {
                return;
            }

            _nextPrefsRefreshTime = Time.unscaledTime + Mathf.Max(0.02f, prefsPollInterval);

            float prefVolume = PlayerPrefs.GetFloat(volumePrefKey, defaultVolume);
            bool prefMute = PlayerPrefs.GetInt(mutePrefKey, defaultMute ? 1 : 0) == 1;

            if (force || !Mathf.Approximately(prefVolume, _cachedVolume)) {
                _cachedVolume = prefVolume;
                _audioSource.volume = prefVolume;
            }

            if (force || prefMute != _cachedMute) {
                _cachedMute = prefMute;
                _audioSource.mute = prefMute;
            }
        }

        private void TrackLifetime() {
            if (_audioSource.isPlaying) {
                _hasStartedPlaying = true;
            }

            if (!destroyOnClipEnd || _audioSource.loop) {
                return;
            }

            if (_hasStartedPlaying && !_audioSource.isPlaying) {
                Destroy(gameObject);
            }
        }
    }
}
