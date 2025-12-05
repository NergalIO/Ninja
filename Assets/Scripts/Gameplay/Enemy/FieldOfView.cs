using UnityEngine;
using System.Collections.Generic;
using Ninja.Core.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ninja.Gameplay.Enemy
{
    public class FieldOfView : MonoBehaviour
    {
        [Header("FOV Settings")]
        [SerializeField] private float viewRadius = 6f;
        [SerializeField, Range(0, 360)] private float viewAngle = 90f;
        [SerializeField] private int rayCount = 60;
        [SerializeField] private LayerMask obstacleMask;

        [Header("Rendering")]
        [SerializeField] private Material fovMaterial;
        [SerializeField] private Color normalColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private Color alertColor = new Color(1f, 0f, 0f, 0.3f);

        private Transform target;
        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private Vector3 origin;
        private float startAngle;
        private bool canSeeTarget;
        private float detectionLevel = 0f;
        private bool isPaused;

        public float ViewRadius => viewRadius;
        public float ViewAngle => viewAngle;
        public bool CanSeeTarget => canSeeTarget;

        public void SetTarget(Transform t) => target = t;
        
        /// <summary>
        /// Установить уровень обнаружения для плавного изменения цвета (0-1)
        /// </summary>
        public void SetDetectionLevel(float level) => detectionLevel = Mathf.Clamp01(level);

        private void Start()
        {
            var filter = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            mesh = new Mesh { name = "FOV Mesh" };
            filter.mesh = mesh;

            meshRenderer = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
            
            if (fovMaterial == null)
            {
                fovMaterial = new Material(Shader.Find("Sprites/Default")) { color = normalColor };
                meshRenderer.material = fovMaterial;
            }
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

        private void LateUpdate()
        {
            if (isPaused) return;
            
            origin = transform.position;
            startAngle = transform.eulerAngles.z - viewAngle / 2f;

            GenerateMesh();
            DetectTarget();
            UpdateColor();
        }

        private void GenerateMesh()
        {
            float angleStep = viewAngle / rayCount;
            var points = new List<Vector3> { Vector3.zero };

            for (int i = 0; i <= rayCount; i++)
            {
                float angle = (startAngle + angleStep * i) * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                
                var hit = Physics2D.Raycast(origin, dir, viewRadius, obstacleMask);
                var point = hit.collider != null && hit.transform != target
                    ? (Vector3)hit.point
                    : origin + (Vector3)(dir * viewRadius);

                points.Add(transform.InverseTransformPoint(point));
            }

            var triangles = new List<int>();
            for (int i = 1; i < points.Count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            mesh.Clear();
            mesh.SetVertices(points);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
        }

        private void DetectTarget()
        {
            canSeeTarget = false;
            if (target == null) return;

            var toTarget = target.position - origin;
            float dist = toTarget.magnitude;

            if (dist > viewRadius) return;

            float forward = transform.eulerAngles.z * Mathf.Deg2Rad;
            var forwardDir = new Vector2(Mathf.Cos(forward), Mathf.Sin(forward));
            float angle = Vector2.Angle(forwardDir, toTarget.normalized);

            if (angle > viewAngle / 2f) return;

            var hit = Physics2D.Raycast(origin, toTarget.normalized, dist, obstacleMask);
            canSeeTarget = hit.collider == null || hit.transform == target;
        }

        private void UpdateColor()
        {
            if (meshRenderer == null) return;
            
            // Плавная интерполяция цвета по уровню обнаружения
            Color currentColor = Color.Lerp(normalColor, alertColor, detectionLevel);
            
            var block = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(block);
            block.SetColor("_Color", currentColor);
            meshRenderer.SetPropertyBlock(block);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var pos = Application.isPlaying ? origin : transform.position;
            float start = Application.isPlaying ? startAngle : transform.eulerAngles.z - viewAngle / 2f;

            // Цвет по уровню обнаружения
            Gizmos.color = Color.Lerp(normalColor, alertColor, detectionLevel);
            
            // Draw arc
            float step = viewAngle / 20f;
            Vector3 prevPoint = pos + DirFromAngle(start) * viewRadius;
            
            for (float a = step; a <= viewAngle; a += step)
            {
                var point = pos + DirFromAngle(start + a) * viewRadius;
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            // Draw boundaries
            Gizmos.DrawLine(pos, pos + DirFromAngle(start) * viewRadius);
            Gizmos.DrawLine(pos, pos + DirFromAngle(start + viewAngle) * viewRadius);

            if (target != null)
            {
                Gizmos.color = canSeeTarget ? Color.red : Color.gray;
                Gizmos.DrawLine(pos, target.position);
            }
        }

        private Vector3 DirFromAngle(float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
        }
#endif
    }
}
