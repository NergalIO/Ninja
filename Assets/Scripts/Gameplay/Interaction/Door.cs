using UnityEngine;

namespace Ninja.Gameplay.Interaction
{
    [RequireComponent(typeof(Collider2D))]
    public class Door : InteractableObject
    {
        [Header("Door Settings")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 2f;
        [SerializeField] private bool requiresKey = false;
        [SerializeField] private string keyName = "";

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;

        private Quaternion closedRotation;
        private Quaternion openRotation;
        private bool isAnimating = false;

        private void Start()
        {
            closedRotation = transform.rotation;
            openRotation = closedRotation * Quaternion.Euler(0, 0, openAngle);

            if (isOpen)
            {
                transform.rotation = openRotation;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        protected override void OnInteract(Transform interactor)
        {
            if (isAnimating)
                return;

            if (requiresKey && !HasKey())
            {
                PlaySound(lockedSound);
                Debug.Log($"Дверь заперта! Нужен ключ: {keyName}");
                return;
            }

            isOpen = !isOpen;
            StartCoroutine(AnimateDoor());
            PlaySound(isOpen ? openSound : closeSound);
        }

        private System.Collections.IEnumerator AnimateDoor()
        {
            isAnimating = true;
            Quaternion targetRotation = isOpen ? openRotation : closedRotation;
            Quaternion startRotation = transform.rotation;

            float elapsed = 0f;
            float duration = 1f / openSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                yield return null;
            }

            transform.rotation = targetRotation;
            isAnimating = false;
        }

        private bool HasKey()
        {
            return false;
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        public override string GetInteractionPrompt()
        {
            if (requiresKey && !HasKey())
            {
                return $"Заперто ({keyName})";
            }

            return isOpen ? "Закрыть дверь [E]" : "Открыть дверь [E]";
        }

        public bool IsOpen => isOpen;
    }
}
