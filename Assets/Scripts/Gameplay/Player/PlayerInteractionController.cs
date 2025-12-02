using UnityEngine;
using Ninja.Input;
using Ninja.Gameplay.Interaction;
using Ninja.Core;

namespace Ninja.Gameplay.Player
{
    [RequireComponent(typeof(PlayerInputController))]
    public class PlayerInteractionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputController inputController;
        [SerializeField] private Transform interactionPoint;

        [Header("Settings")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactionLayer = -1;
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private float facingAngleThreshold = 90f;
        [SerializeField] private bool debugLog = false;
        
        [Header("Gizmos Settings")]
        [SerializeField] private float facingDirectionLineLength = 1.5f;
        [SerializeField] private Color facingDirectionColor = Color.blue;
        [SerializeField] private Color facingSectorColor = new Color(0f, 1f, 1f, 0.3f);

        private IInteractable currentInteractable;
        private InteractableOutline currentOutline;

        private void Awake()
        {
            if (inputController == null)
            {
                inputController = GetComponent<PlayerInputController>();
            }

            if (interactionPoint == null)
            {
                interactionPoint = transform;
            }
        }

        private void Update()
        {
            CheckForInteractables();
            HandleInteraction();
        }

        private void CheckForInteractables()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                interactionPoint.position,
                interactionRange,
                interactionLayer
            );

            IInteractable nearestInteractable = null;
            InteractableOutline nearestOutline = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider2D collider in colliders)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    if (debugLog)
                        Debug.Log($"[Interaction] Объект {collider.name} не имеет компонента IInteractable");
                    continue;
                }

                if (!interactable.CanInteract(transform))
                {
                    if (debugLog)
                        Debug.Log($"[Interaction] Объект {collider.name} не может взаимодействовать (дистанция или состояние)");
                    continue;
                }

                if (!IsFacingObject(collider.transform))
                {
                    if (debugLog)
                        Debug.Log($"[Interaction] Игрок не смотрит на объект {collider.name}");
                    continue;
                }

                float distance = Vector2.Distance(interactionPoint.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestInteractable = interactable;
                    nearestOutline = collider.GetComponent<InteractableOutline>();
                    
                    if (debugLog)
                        Debug.Log($"[Interaction] Найден интерактивный объект: {collider.name}, расстояние: {distance}, обводка: {(nearestOutline != null ? "есть" : "нет")}");
                }
            }
            
            if (debugLog && colliders.Length > 0 && nearestInteractable == null)
            {
                Debug.Log($"[Interaction] Найдено {colliders.Length} коллайдеров, но ни один не прошел проверки");
            }

            if (currentOutline != null && currentOutline != nearestOutline)
            {
                currentOutline.HideOutline();
            }

            currentInteractable = nearestInteractable;
            currentOutline = nearestOutline;

            if (currentOutline != null)
            {
                currentOutline.ShowOutline();
            }
        }

        private bool IsFacingObject(Transform target)
        {
            Vector2 directionToTarget = (target.position - transform.position).normalized;

            float playerAngle = transform.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 playerForward = new Vector2(Mathf.Cos(playerAngle), Mathf.Sin(playerAngle));

            float dotProduct = Vector2.Dot(playerForward, directionToTarget);
            float angle = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f)) * Mathf.Rad2Deg;

            return angle <= facingAngleThreshold;
        }

        private void HandleInteraction()
        {
            if (inputController.IsInteractPressed)
            {
                if (currentInteractable == null)
                {
                    if (debugLog)
                        Debug.Log("[Interaction] Нажата E, но нет текущего объекта взаимодействия");
                    return;
                }
                
                MonoBehaviour interactableMono = currentInteractable as MonoBehaviour;
                if (interactableMono != null && IsFacingObject(interactableMono.transform))
                {
                    if (debugLog)
                        Debug.Log($"[Interaction] Взаимодействие с {interactableMono.name}");
                    currentInteractable.Interact(transform);
                }
                else if (debugLog)
                {
                    Debug.Log($"[Interaction] Не удалось взаимодействовать с объектом");
                }
            }
        }

        public IInteractable GetCurrentInteractable()
        {
            return currentInteractable;
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos)
                return;

            Vector3 point = interactionPoint != null ? interactionPoint.position : transform.position;
            
            Gizmos.color = currentInteractable != null ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(point, interactionRange);

            DrawFacingDirection(point);
            DrawFacingSector(point);
        }

        private void DrawFacingDirection(Vector3 origin)
        {
            float playerAngle = transform.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 playerForward = new Vector2(Mathf.Cos(playerAngle), Mathf.Sin(playerAngle));
            
            Vector3 direction = new Vector3(playerForward.x, playerForward.y, 0);
            Vector3 endPoint = origin + direction * facingDirectionLineLength;

            Gizmos.color = facingDirectionColor;
            Gizmos.DrawLine(origin, endPoint);

            GizmosUtils.DrawArrow(origin, endPoint, 0.2f);
        }

        private void DrawFacingSector(Vector3 origin)
        {
            float playerAngle = transform.eulerAngles.z;
            float halfAngle = facingAngleThreshold * 0.5f;

            Gizmos.color = facingSectorColor;
            GizmosUtils.DrawSector(origin, interactionRange, playerAngle, facingAngleThreshold, 20);
        }
    }
}
У