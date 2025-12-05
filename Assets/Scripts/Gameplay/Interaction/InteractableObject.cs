using UnityEngine;
using UnityEngine.Events;

namespace Ninja.Gameplay.Interaction
{
    /// <summary>
    /// Базовый класс для интерактивных объектов
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private bool canInteract = true;
        [SerializeField] private string interactionHint = "Нажмите E";
        [SerializeField] private bool singleUse = false;
        
        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 1f);
        [SerializeField] private bool useHighlight = true;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onInteract;
        [SerializeField] private UnityEvent onFocus;
        [SerializeField] private UnityEvent onUnfocus;
        
        private Color originalColor;
        private bool hasBeenUsed = false;
        private bool isFocused = false;
        
        public bool CanInteract => canInteract && (!singleUse || !hasBeenUsed);
        public string InteractionHint => interactionHint;
        public Transform Transform => transform;
        public bool IsFocused => isFocused;
        
        protected virtual void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
                
            // Убедимся что есть триггер коллайдер для обнаружения
            var collider = GetComponent<Collider2D>();
            if (collider != null && !collider.isTrigger)
            {
                Debug.LogWarning($"[InteractableObject] Collider на {gameObject.name} не является триггером. " +
                    "Рекомендуется использовать триггер для зоны взаимодействия.", this);
            }
        }
        
        public virtual void OnInteract(GameObject interactor)
        {
            if (!CanInteract)
                return;
                
            if (singleUse)
                hasBeenUsed = true;
                
            onInteract?.Invoke();
            
            Debug.Log($"[InteractableObject] {interactor.name} взаимодействует с {gameObject.name}");
        }
        
        public virtual void OnFocus(GameObject interactor)
        {
            if (!CanInteract)
                return;
                
            isFocused = true;
            
            if (useHighlight && spriteRenderer != null)
                spriteRenderer.color = highlightColor;
                
            onFocus?.Invoke();
        }
        
        public virtual void OnUnfocus(GameObject interactor)
        {
            isFocused = false;
            
            if (useHighlight && spriteRenderer != null)
                spriteRenderer.color = originalColor;
                
            onUnfocus?.Invoke();
        }
        
        /// <summary>
        /// Сбросить состояние объекта (для переиспользования)
        /// </summary>
        public virtual void ResetInteractable()
        {
            hasBeenUsed = false;
            canInteract = true;
        }
        
        /// <summary>
        /// Установить возможность взаимодействия
        /// </summary>
        public void SetCanInteract(bool value)
        {
            canInteract = value;
            
            // Если объект сейчас в фокусе но стал недоступен - снимаем подсветку
            if (!canInteract && isFocused && useHighlight && spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }
        
        /// <summary>
        /// Установить подсказку взаимодействия
        /// </summary>
        public void SetInteractionHint(string hint)
        {
            interactionHint = hint;
        }
        
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
#endif
    }
}
