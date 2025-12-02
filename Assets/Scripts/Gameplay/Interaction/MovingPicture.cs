using UnityEngine;

namespace Ninja.Gameplay.Interaction
{
    /// <summary>
    /// Скрипт для картин, которые можно отодвигать
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MovingPicture : InteractableObject
    {
        [Header("Picture Settings")]
        [SerializeField] private Vector2 moveDirection = Vector2.right;
        [SerializeField] private float moveDistance = 2f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private bool canReturn = true;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip moveSound;

        private Vector2 startPosition;
        private Vector2 targetPosition;
        private bool isMoved = false;
        private bool isAnimating = false;

        private void Start()
        {
            startPosition = transform.position;
            moveDirection = moveDirection.normalized;
            targetPosition = startPosition + moveDirection * moveDistance;
        }

        protected override void OnInteract(Transform interactor)
        {
            if (isAnimating)
                return;

            if (isMoved && !canReturn)
                return;

            isMoved = !isMoved;
            StartCoroutine(AnimatePicture());
            PlaySound(moveSound);
        }

        private System.Collections.IEnumerator AnimatePicture()
        {
            isAnimating = true;
            Vector2 target = isMoved ? targetPosition : startPosition;
            Vector2 start = transform.position;

            float distance = Vector2.Distance(start, target);
            float duration = distance / moveSpeed;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.position = Vector2.Lerp(start, target, t);
                yield return null;
            }

            transform.position = target;
            isAnimating = false;
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
            if (isMoved && !canReturn)
            {
                return "Картина уже отодвинута";
            }

            return isMoved ? "Вернуть картину [E]" : "Отодвинуть картину [E]";
        }

        public bool IsMoved => isMoved;

        /// <summary>
        /// Визуализация направления движения в редакторе
        /// </summary>
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Vector2 start = Application.isPlaying ? startPosition : (Vector2)transform.position;
            Vector2 end = start + moveDirection.normalized * moveDistance;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, 0.2f);
        }
    }
}

