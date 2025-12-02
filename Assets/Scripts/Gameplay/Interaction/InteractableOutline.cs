using UnityEngine;

namespace Ninja.Gameplay.Interaction
{
    /// <summary>
    /// Компонент для визуальной обводки интерактивных объектов
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class InteractableOutline : MonoBehaviour
    {
        [Header("Outline Settings")]
        [SerializeField] private Color outlineColor = Color.yellow;
        [SerializeField] private float outlineWidth = 0.1f;
        [SerializeField] private int segmentsPerCorner = 5;
        [SerializeField] private bool showOutline = false;

        private LineRenderer lineRenderer;
        private SpriteRenderer spriteRenderer;
        private IInteractable interactable;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            interactable = GetComponent<IInteractable>();

            // Создаем LineRenderer если его нет
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            SetupLineRenderer();
        }

        private void SetupLineRenderer()
        {
            lineRenderer.useWorldSpace = false;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = outlineColor;
            lineRenderer.endColor = outlineColor;
            lineRenderer.startWidth = outlineWidth;
            lineRenderer.endWidth = outlineWidth;
            lineRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            lineRenderer.enabled = false;
        }

        private void Update()
        {
            if (interactable == null)
                return;

            // Обновляем обводку в зависимости от состояния
            bool shouldShow = showOutline && interactable != null;
            lineRenderer.enabled = shouldShow;

            if (shouldShow)
            {
                UpdateOutline();
            }
        }

        private void UpdateOutline()
        {
            if (spriteRenderer.sprite == null)
                return;

            // Получаем границы спрайта в локальных координатах
            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            
            // Преобразуем размеры спрайта в локальные координаты с учетом масштаба
            Vector3 localSize = new Vector3(
                spriteBounds.size.x * transform.lossyScale.x,
                spriteBounds.size.y * transform.lossyScale.y,
                0
            );

            // Создаем прямоугольную обводку вокруг спрайта
            int totalSegments = segmentsPerCorner * 4;
            lineRenderer.positionCount = totalSegments + 1;

            float halfWidth = localSize.x * 0.5f;
            float halfHeight = localSize.y * 0.5f;

            int index = 0;

            // Верхняя сторона
            for (int i = 0; i < segmentsPerCorner; i++)
            {
                float t = (float)i / segmentsPerCorner;
                lineRenderer.SetPosition(index++, new Vector3(
                    Mathf.Lerp(-halfWidth, halfWidth, t),
                    halfHeight,
                    0
                ));
            }

            // Правая сторона
            for (int i = 0; i < segmentsPerCorner; i++)
            {
                float t = (float)i / segmentsPerCorner;
                lineRenderer.SetPosition(index++, new Vector3(
                    halfWidth,
                    Mathf.Lerp(halfHeight, -halfHeight, t),
                    0
                ));
            }

            // Нижняя сторона
            for (int i = 0; i < segmentsPerCorner; i++)
            {
                float t = (float)i / segmentsPerCorner;
                lineRenderer.SetPosition(index++, new Vector3(
                    Mathf.Lerp(halfWidth, -halfWidth, t),
                    -halfHeight,
                    0
                ));
            }

            // Левая сторона
            for (int i = 0; i < segmentsPerCorner; i++)
            {
                float t = (float)i / segmentsPerCorner;
                lineRenderer.SetPosition(index++, new Vector3(
                    -halfWidth,
                    Mathf.Lerp(-halfHeight, halfHeight, t),
                    0
                ));
            }

            // Замыкаем контур
            lineRenderer.SetPosition(index, new Vector3(-halfWidth, halfHeight, 0));
        }

        /// <summary>
        /// Показать обводку
        /// </summary>
        public void ShowOutline()
        {
            showOutline = true;
        }

        /// <summary>
        /// Скрыть обводку
        /// </summary>
        public void HideOutline()
        {
            showOutline = false;
        }

        /// <summary>
        /// Установить цвет обводки
        /// </summary>
        public void SetOutlineColor(Color color)
        {
            outlineColor = color;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
        }
    }
}

