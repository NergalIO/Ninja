using UnityEngine;


namespace Ninja.Audio {
    [RequireComponent(typeof(AudioSource))]
    public class SoundSource : MonoBehaviour {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float volumeMultiplier = 1f;
        [SerializeField] private bool destroyOnClipEnd = false;

        private float baseVolume = 1f;

        public AudioSource AudioSource => audioSource;

        private void OnEnable() {
            if (audioSource == null) {
                audioSource = GetComponent<AudioSource>();
            }

            baseVolume = audioSource.volume;
        }

        private void Update() {
            if (destroyOnClipEnd && audioSource != null && !audioSource.isPlaying && audioSource.clip != null) {
                Destroy(gameObject);
            }
        }

        public void SetVolume(float volume) {
            if (audioSource == null) {
                return;
            }

            baseVolume = Mathf.Clamp01(volume);
            audioSource.volume = baseVolume * volumeMultiplier;
        }

        public void SetMute(bool muted) {
            if (audioSource == null) {
                return;
            }

            audioSource.mute = muted;
        }

        public void SetDestroyOnClipEnd(bool destroy) {
            destroyOnClipEnd = destroy;
        }

        public float GetVolume() {
            return audioSource != null ? audioSource.volume : 0f;
        }

        public bool IsMuted() {
            return audioSource != null && audioSource.mute;
        }
    }
}
