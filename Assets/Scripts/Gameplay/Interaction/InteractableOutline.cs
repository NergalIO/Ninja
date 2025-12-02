using UnityEngine;

namespace Ninja.Gameplay.Interaction
{
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

            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            SetupLineRenderer();
        }

        private void Start()
        {
            if (spriteRenderer == null)
            {
                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                {
                    Debug.LogWarning($"[InteractableOutline] На объекте {gameObject.name} нет SpriteRenderer. Используются размеры коллайдера.");
                }
            }
        }

        private void SetupLineRenderer()
        {
            lineRenderer.useWorldSpace = false;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = outlineColor;
            lineRenderer.endColor = outlineColor;
            lineRenderer.startWidth = outlineWidth;
            lineRenderer.endWidth = outlineWidth;
            
            if (spriteRenderer != null)
            {
                lineRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            else
            {
                lineRenderer.sortingOrder = 100;
            }
            
            lineRenderer.enabled = false;
        }

        private void Update()
        {
            if (interactable == null)
                return;

            bool shouldShow = showOutline && interactable != null;
            lineRenderer.enabled = shouldShow;

            if (shouldShow)
            {
                UpdateOutline();
            }
        }

        private void UpdateOutline()
        {
            Vector3 localSize;
            
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Bounds spriteBounds = spriteRenderer.sprite.bounds;
                
                localSize = new Vector3(
                    spriteBounds.size.x * transform.lossyScale.x,
                    spriteBounds.size.y * transform.lossyScale.y,
                    0
                );
            }
            else
            {
                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                {
                    Bounds bounds = col.bounds;
                    localSize = transform.InverseTransformVector(bounds.size);
                }
                else
                {
                    localSize = Vector3.one;
                }
            }

            int totalSegments = segmentsPerCorner * 4;
            lineRenderer.positionCount = totalSegments + 1;

            float halfWidth = localSize.x * 0.5f;
            float halfHeight = localSize.y * 0.5f;

            int index = 0;

            for (int i = 0; i < segmentsPerCorner; i++)
            {
                float t = (float)i / segmentsPerCorner;
                lineRenderer.SetPosition(index++, new Vector3(
                    Mathf.Lerp(-halfWidth, halfWidth, t),
                    halfHeight,
                    0
                ));
            }

            for (int i = 0; i < segmentsPerCorner; i++)
            {
                float t = (float)i / segmentsPerCorner;
                lineRenderer.SetPosition(index++, new Vector3(
                    halfWidth,
                    Mathf.Lerp(halfHeight, -halfHeight, t),
                    0
                ));
            }

            for (int i = 0; i < segmentsPerCorner; i++)
            {
                float t = (float)i / segmentsPerCorner;
                lineRenderer.SetPosition(index++, new Vector3(
                    Mathf.Lerp(halfWidth, -halfWidth, t),
                    -halfHeight,
                    0
                ));
            }

            for (int i = 0; i < segmentsPerCorner; i++)
            {
                float t = (float)i / segmentsPerCorner;
                lineRenderer.SetPosition(index++, new Vector3(
                    -halfWidth,
                    Mathf.Lerp(-halfHeight, halfHeight, t),
                    0
                ));
            }

            lineRenderer.SetPosition(index, new Vector3(-halfWidth, halfHeight, 0));
        }

        public void ShowOutline()
        {
            showOutline = true;
        }

        public void HideOutline()
        {
            showOutline = false;
        }

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
