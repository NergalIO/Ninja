using UnityEngine;
using System.Collections.Generic;
using Ninja.Core.Events;

namespace Ninja.Gameplay.Player
{
    public class NoiseController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MovementController controller;

        [Header("Noise Settings")]
        [SerializeField] private float defaultNoise = 0.5f;
        [SerializeField] private float lerpSpeed = 5f;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private int rayCount = 36;

        [Header("Rendering")]
        [SerializeField] private Material noiseMaterial;
        [SerializeField] private Color noiseColor = new Color(0.3f, 0.6f, 1f, 0.15f);

        [Header("Collider")]
        [SerializeField] private CircleCollider2D noiseCollider;

        private float currentRadius;
        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private List<Vector3> meshPoints = new();
        private bool isPaused;

        public float CurrentRadius => currentRadius;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponentInChildren<MovementController>();

            SetupCollider();
            SetupMesh();
        }

        private void OnEnable()
        {
            Events.Subscribe(GameEvents.GamePaused, OnGamePaused);
            Events.Subscribe(GameEvents.GameResumed, OnGameResumed);
        }

        private void OnDisable()
        {
            Events.Unsubscribe(GameEvents.GamePaused, OnGamePaused);
            Events.Unsubscribe(GameEvents.GameResumed, OnGameResumed);
        }

        private void OnGamePaused(EventArgs e) => isPaused = true;
        private void OnGameResumed(EventArgs e) => isPaused = false;

        private void SetupCollider()
        {
            if (noiseCollider == null)
            {
                var colliderGO = new GameObject("Noise Area");
                colliderGO.transform.SetParent(transform);
                colliderGO.transform.localPosition = Vector3.zero;
                noiseCollider = colliderGO.AddComponent<CircleCollider2D>();
                noiseCollider.isTrigger = true;
            }
            
            // Устанавливаем тег в любом случае
            noiseCollider.gameObject.tag = "NoiseArea";
            noiseCollider.isTrigger = true;
        }

        private void SetupMesh()
        {
            var filter = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            mesh = new Mesh { name = "Noise Mesh" };
            filter.mesh = mesh;

            meshRenderer = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();

            if (noiseMaterial == null)
            {
                noiseMaterial = new Material(Shader.Find("Sprites/Default")) { color = noiseColor };
            }
            meshRenderer.material = noiseMaterial;
        }

        private void Update()
        {
            if (isPaused) return;
            
            float targetRadius = defaultNoise * controller.CurrentSpeed.magnitude;
            currentRadius = Mathf.Lerp(currentRadius, targetRadius, lerpSpeed * Time.deltaTime);

            if (noiseCollider != null)
                noiseCollider.radius = currentRadius;
        }

        private void LateUpdate()
        {
            if (isPaused) return;
            
            GenerateMesh();
            UpdateColor();
        }

        private void GenerateMesh()
        {
            if (currentRadius < 0.01f)
            {
                mesh.Clear();
                meshPoints.Clear();
                return;
            }

            meshPoints.Clear();
            meshPoints.Add(Vector3.zero); // Центр

            float angleStep = 360f / rayCount;
            Vector3 origin = transform.position;

            for (int i = 0; i <= rayCount; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                var hit = Physics2D.Raycast(origin, dir, currentRadius, obstacleMask);
                Vector3 point;

                if (hit.collider != null)
                    point = (Vector3)hit.point;
                else
                    point = origin + (Vector3)(dir * currentRadius);

                meshPoints.Add(transform.InverseTransformPoint(point));
            }

            var triangles = new List<int>();
            for (int i = 1; i < meshPoints.Count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            mesh.Clear();
            mesh.SetVertices(meshPoints);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
        }

        private void UpdateColor()
        {
            if (meshRenderer == null) return;

            var block = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(block);
            block.SetColor("_Color", noiseColor);
            meshRenderer.SetPropertyBlock(block);
        }

        /// <summary>
        /// Проверить, слышен ли звук в указанной позиции
        /// </summary>
        public bool CanBeHeardAt(Vector3 listenerPosition)
        {
            if (currentRadius < 0.01f)
                return false;

            Vector2 direction = listenerPosition - transform.position;
            float distance = direction.magnitude;

            // Слишком далеко
            if (distance > currentRadius)
                return false;

            // Raycast к слушателю
            var hit = Physics2D.Raycast(transform.position, direction.normalized, distance, obstacleMask);

            // Если ничего не попало - звук слышен
            return hit.collider == null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = noiseColor;
                Gizmos.DrawWireSphere(transform.position, defaultNoise);
                return;
            }

            if (currentRadius < 0.01f)
                return;

            // Рисуем зону звука с учётом препятствий
            Gizmos.color = noiseColor;

            float angleStep = 360f / rayCount;
            Vector3 origin = transform.position;
            Vector3 prevPoint = GetNoisePoint(origin, 0);

            for (int i = 1; i <= rayCount; i++)
            {
                Vector3 point = GetNoisePoint(origin, angleStep * i);
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            // Соединяем с первой точкой
            Gizmos.DrawLine(prevPoint, GetNoisePoint(origin, 0));
        }

        private Vector3 GetNoisePoint(Vector3 origin, float angleDeg)
        {
            float angle = angleDeg * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var hit = Physics2D.Raycast(origin, dir, currentRadius, obstacleMask);

            if (hit.collider != null)
                return hit.point;

            return origin + (Vector3)(dir * currentRadius);
        }
#endif
    }
}
