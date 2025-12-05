using System.Collections.Generic;
using UnityEngine;

using Ninja.Core.Events;
using Ninja.Gameplay.Interaction;
using Ninja.Input;

namespace Ninja.Gameplay.Player
{
    /// <summary>
    /// Контроллер взаимодействия игрока с интерактивными объектами
    /// </summary>
    public class PlayerInteractionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputController inputController;
        
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 1.5f;
        [SerializeField] private LayerMask interactableLayer = -1;
        [SerializeField, Range(0f, 360f)] private float viewAngle = 120f;
        [SerializeField] private bool requireLineOfSight = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private bool showDebugLogs = false;
        [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);
        
        private IInteractable currentInteractable;
        private readonly List<IInteractable> interactablesInRange = new();
        private readonly Collider2D[] colliderBuffer = new Collider2D[16];
        
        /// <summary>
        /// Текущий интерактивный объект в фокусе
        /// </summary>
        public IInteractable CurrentInteractable => currentInteractable;
        
        /// <summary>
        /// Есть ли объект для взаимодействия
        /// </summary>
        public bool HasInteractable => currentInteractable != null && currentInteractable.CanInteract;
        
        /// <summary>
        /// Подсказка текущего интерактивного объекта
        /// </summary>
        public string CurrentHint => currentInteractable?.InteractionHint ?? string.Empty;
        
        private void Awake()
        {
            if (inputController == null)
                inputController = GetComponentInChildren<PlayerInputController>();
            
            if (inputController == null)
                inputController = GetComponentInParent<PlayerInputController>();
            
            if (inputController == null)
                inputController = GetComponent<PlayerInputController>();
                
            if (inputController == null)
                Debug.LogError($"[PlayerInteractionController] PlayerInputController не найден на {gameObject.name}! " +
                    "Назначьте его вручную в инспекторе.", this);
        }
        
        private void Update()
        {
            DetectInteractables();
            UpdateCurrentInteractable();
            HandleInteraction();
        }
        
        private void DetectInteractables()
        {
            interactablesInRange.Clear();
            
            int count = Physics2D.OverlapCircleNonAlloc(
                transform.position, 
                detectionRadius, 
                colliderBuffer, 
                interactableLayer
            );
            
            for (int i = 0; i < count; i++)
            {
                var interactable = colliderBuffer[i].GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    interactablesInRange.Add(interactable);
                }
            }
        }
        
        private void UpdateCurrentInteractable()
        {
            IInteractable closest = null;
            float closestDistance = float.MaxValue;
            
            Vector2 playerForward = transform.right; // В 2D "вперёд" часто это right
            float halfViewAngle = viewAngle * 0.5f;
            
            foreach (var interactable in interactablesInRange)
            {
                if (interactable?.Transform == null)
                    continue;
                    
                Vector2 toInteractable = (Vector2)interactable.Transform.position - (Vector2)transform.position;
                float distance = toInteractable.magnitude;
                
                if (distance < 0.01f)
                    continue;
                
                // Проверяем угол обзора
                if (requireLineOfSight)
                {
                    float dot = Vector2.Dot(playerForward, toInteractable.normalized);
                    float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
                    
                    // Если объект вне угла обзора - пропускаем
                    if (angle > halfViewAngle)
                        continue;
                }
                
                // Выбираем ближайший объект
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                }
            }
            
            if (closest != currentInteractable)
            {
                if (currentInteractable != null)
                {
                    currentInteractable.OnUnfocus(gameObject);
                    TriggerInteractionEvent(GameEvents.InteractionUnfocused, currentInteractable);
                }
                
                currentInteractable = closest;
                
                if (currentInteractable != null)
                {
                    currentInteractable.OnFocus(gameObject);
                    TriggerInteractionEvent(GameEvents.InteractionFocused, currentInteractable);
                }
            }
        }
        
        private bool wasInteracting = false;
        
        private void HandleInteraction()
        {
            if (inputController == null)
                return;
            
            bool isInteracting = inputController.IsInteracting;
            bool interactInput = isInteracting && !wasInteracting;
            wasInteracting = isInteracting;
            
            if (showDebugLogs && interactInput)
                Debug.Log($"[PlayerInteractionController] Interact input! Current: {currentInteractable?.Transform?.name ?? "none"}");

            if (interactInput && currentInteractable != null && currentInteractable.CanInteract)
            {
                if (showDebugLogs)
                    Debug.Log($"[PlayerInteractionController] Interacting with {currentInteractable.Transform.name}");
                    
                currentInteractable.OnInteract(gameObject);
                TriggerInteractionEvent(GameEvents.InteractionPerformed, currentInteractable);
            }
        }
        
        private void TriggerInteractionEvent(string eventName, IInteractable interactable)
        {
            var args = new InteractionEventArgs(
                gameObject,
                interactable?.Transform?.gameObject,
                interactable?.InteractionHint
            );
            Events.Trigger(eventName, args);
        }
        
        /// <summary>
        /// Принудительно взаимодействовать с указанным объектом
        /// </summary>
        public void ForceInteract(IInteractable interactable)
        {
            if (interactable != null && interactable.CanInteract)
            {
                interactable.OnInteract(gameObject);
                TriggerInteractionEvent(GameEvents.InteractionPerformed, interactable);
            }
        }
        
        /// <summary>
        /// Очистить текущий интерактивный объект
        /// </summary>
        public void ClearCurrentInteractable()
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnUnfocus(gameObject);
                TriggerInteractionEvent(GameEvents.InteractionUnfocused, currentInteractable);
                currentInteractable = null;
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos)
                return;
            
            Vector3 pos = transform.position;
            Vector3 forward = transform.right;
            
            // Рисуем радиус обнаружения
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(pos, detectionRadius);
            
            // Рисуем угол обзора
            if (requireLineOfSight)
            {
                Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
                float halfAngle = viewAngle * 0.5f * Mathf.Deg2Rad;
                
                Vector3 rightDir = new Vector3(
                    forward.x * Mathf.Cos(halfAngle) - forward.y * Mathf.Sin(halfAngle),
                    forward.x * Mathf.Sin(halfAngle) + forward.y * Mathf.Cos(halfAngle),
                    0
                );
                Vector3 leftDir = new Vector3(
                    forward.x * Mathf.Cos(-halfAngle) - forward.y * Mathf.Sin(-halfAngle),
                    forward.x * Mathf.Sin(-halfAngle) + forward.y * Mathf.Cos(-halfAngle),
                    0
                );
                
                Gizmos.DrawLine(pos, pos + rightDir * detectionRadius);
                Gizmos.DrawLine(pos, pos + leftDir * detectionRadius);
                
                // Дуга
                int segments = 20;
                float angleStep = viewAngle / segments * Mathf.Deg2Rad;
                float startAngle = Mathf.Atan2(forward.y, forward.x) - halfAngle;
                
                for (int i = 0; i < segments; i++)
                {
                    float a1 = startAngle + angleStep * i;
                    float a2 = startAngle + angleStep * (i + 1);
                    Vector3 p1 = pos + new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0) * detectionRadius;
                    Vector3 p2 = pos + new Vector3(Mathf.Cos(a2), Mathf.Sin(a2), 0) * detectionRadius;
                    Gizmos.DrawLine(p1, p2);
                }
            }
            
            // Линия к текущему объекту
            if (currentInteractable?.Transform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pos, currentInteractable.Transform.position);
            }
        }
#endif
    }
}
