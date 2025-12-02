using UnityEngine;
using Ninja.Input;
using Ninja.Gameplay.Interaction;

namespace Ninja.Gameplay.Player
{
    /// <summary>
    /// Контроллер взаимодействия игрока с объектами
    /// </summary>
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
        [SerializeField] private float facingAngleThreshold = 90f; // Угол в градусах для проверки направления взгляда
        
        [Header("Gizmos Settings")]
        [SerializeField] private float facingDirectionLineLength = 1.5f;
        [SerializeField] private Color facingDirectionColor = Color.blue;
        [SerializeField] private Color facingSectorColor = new Color(0f, 1f, 1f, 0.3f); // Полупрозрачный cyan

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

        /// <summary>
        /// Проверяет наличие объектов взаимодействия поблизости
        /// </summary>
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
                    continue;

                if (!interactable.CanInteract(transform))
                    continue;

                // Проверяем направление взгляда игрока
                if (!IsFacingObject(collider.transform))
                    continue;

                float distance = Vector2.Distance(interactionPoint.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestInteractable = interactable;
                    nearestOutline = collider.GetComponent<InteractableOutline>();
                }
            }

            // Обновляем обводку
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

        /// <summary>
        /// Проверяет, смотрит ли игрок на объект
        /// </summary>
        private bool IsFacingObject(Transform target)
        {
            // Получаем направление от игрока к объекту
            Vector2 directionToTarget = (target.position - transform.position).normalized;

            // Получаем направление взгляда игрока (forward direction)
            float playerAngle = transform.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 playerForward = new Vector2(Mathf.Cos(playerAngle), Mathf.Sin(playerAngle));

            // Вычисляем угол между направлением взгляда и направлением к объекту
            float dotProduct = Vector2.Dot(playerForward, directionToTarget);
            float angle = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f)) * Mathf.Rad2Deg;

            // Если угол меньше порога, игрок смотрит на объект
            return angle <= facingAngleThreshold;
        }

        /// <summary>
        /// Обрабатывает взаимодействие при нажатии кнопки
        /// </summary>
        private void HandleInteraction()
        {
            // Обрабатываем только момент нажатия
            if (inputController.IsInteractPressed && currentInteractable != null)
            {
                // Проверка направления взгляда уже выполнена в CheckForInteractables
                // Дополнительно проверяем, что объект все еще доступен
                MonoBehaviour interactableMono = currentInteractable as MonoBehaviour;
                if (interactableMono != null && IsFacingObject(interactableMono.transform))
                {
                    currentInteractable.Interact(transform);
                }
            }
        }

        /// <summary>
        /// Получить текущий объект взаимодействия
        /// </summary>
        public IInteractable GetCurrentInteractable()
        {
            return currentInteractable;
        }

        /// <summary>
        /// Визуализация зоны взаимодействия и направления взгляда в редакторе
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos)
                return;

            Vector3 point = interactionPoint != null ? interactionPoint.position : transform.position;
            
            // Рисуем зону взаимодействия
            Gizmos.color = currentInteractable != null ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(point, interactionRange);

            // Рисуем направление взгляда игрока
            DrawFacingDirection(point);
            
            // Рисуем сектор зоны взаимодействия
            DrawFacingSector(point);
        }

        /// <summary>
        /// Рисует линию направления взгляда игрока
        /// </summary>
        private void DrawFacingDirection(Vector3 origin)
        {
            // Получаем направление взгляда игрока
            float playerAngle = transform.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 playerForward = new Vector2(Mathf.Cos(playerAngle), Mathf.Sin(playerAngle));
            
            Vector3 direction = new Vector3(playerForward.x, playerForward.y, 0);
            Vector3 endPoint = origin + direction * facingDirectionLineLength;

            // Рисуем линию направления взгляда
            Gizmos.color = facingDirectionColor;
            Gizmos.DrawLine(origin, endPoint);

            // Рисуем стрелку на конце линии
            DrawArrow(origin, endPoint, 0.2f);
        }

        /// <summary>
        /// Рисует сектор зоны взаимодействия (конус направления взгляда)
        /// </summary>
        private void DrawFacingSector(Vector3 origin)
        {
            float playerAngle = transform.eulerAngles.z;
            float halfAngle = facingAngleThreshold * 0.5f;
            
            // Вычисляем углы границ сектора
            float leftAngle = (playerAngle - halfAngle) * Mathf.Deg2Rad;
            float rightAngle = (playerAngle + halfAngle) * Mathf.Deg2Rad;

            Vector2 leftDir = new Vector2(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle));
            Vector2 rightDir = new Vector2(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle));

            Vector3 leftEnd = origin + new Vector3(leftDir.x, leftDir.y, 0) * interactionRange;
            Vector3 rightEnd = origin + new Vector3(rightDir.x, rightDir.y, 0) * interactionRange;

            // Рисуем границы сектора
            Gizmos.color = facingSectorColor;
            Gizmos.DrawLine(origin, leftEnd);
            Gizmos.DrawLine(origin, rightEnd);

            // Рисуем дугу сектора
            DrawArc(origin, interactionRange, playerAngle - halfAngle, playerAngle + halfAngle, 20);
        }

        /// <summary>
        /// Рисует стрелку от start к end
        /// </summary>
        private void DrawArrow(Vector3 start, Vector3 end, float arrowHeadLength)
        {
            Vector3 direction = (end - start).normalized;
            Vector3 right = Vector3.Cross(Vector3.forward, direction).normalized;
            Vector3 arrowHead1 = end - direction * arrowHeadLength + right * arrowHeadLength * 0.5f;
            Vector3 arrowHead2 = end - direction * arrowHeadLength - right * arrowHeadLength * 0.5f;

            Gizmos.DrawLine(end, arrowHead1);
            Gizmos.DrawLine(end, arrowHead2);
        }

        /// <summary>
        /// Рисует дугу от startAngle до endAngle
        /// </summary>
        private void DrawArc(Vector3 center, float radius, float startAngle, float endAngle, int segments)
        {
            float angleStep = (endAngle - startAngle) / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (startAngle + angleStep * i) * Mathf.Deg2Rad;
                float angle2 = (startAngle + angleStep * (i + 1)) * Mathf.Deg2Rad;

                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;

                Gizmos.DrawLine(point1, point2);
            }
        }
    }
}

