using UnityEngine;
using System.Collections.Generic;
using UnityEditor;


namespace Ninja.Gameplay.Enemy
{
    public class FieldOfView : MonoBehaviour
    {
        [Header("FOV Settings")]
        [SerializeField] private float viewRadius = 6f;
        [Range(0, 360)]
        [SerializeField] private float viewAngleHorizontal = 90f;
        [Range(0, 180)]
        [SerializeField] private float viewAngleVertical = 60f;

        [Header("Ray settings")]
        [SerializeField] private int rayCountHorizontal = 60;
        [SerializeField] private int rayCountVertical = 30;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstacleMask;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo;

        [Header("Rendering")]
        [SerializeField] private Material fovMaterial;
        [SerializeField] private MeshRenderer meshRenderer;

        private Transform target;
        private Mesh mesh;
        private Vector3 origin;
        private float startAngleHorizontal;
        private bool canSeeTarget;

        public float ViewRadius => viewRadius;
        public float ViewAngleHorizontal => viewAngleHorizontal;
        public float ViewAngleVertical => viewAngleVertical;
        public bool CanSeeTarget => canSeeTarget;

        private void Start()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            mesh = new Mesh();
            mesh.name = "FOV Mesh";
            meshFilter.mesh = mesh;

            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                }
            }

            if (fovMaterial == null && meshRenderer != null)
            {
                fovMaterial = meshRenderer.material;
                if (fovMaterial == null)
                {
                    fovMaterial = new Material(Shader.Find("Sprites/Default"));
                    fovMaterial.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                    meshRenderer.material = fovMaterial;
                }
            }
        }

        public void SetTarget(Transform targetTransform)
        {
            target = targetTransform;
        }

        private void LateUpdate()
        {
            origin = transform.position;
            float currentRotation = transform.eulerAngles.z;
            startAngleHorizontal = currentRotation - viewAngleHorizontal / 2f;

            GenerateMesh();
            DetectTarget();
            UpdateMeshColor();
        }

        private void GenerateMesh()
        {
            float angleStepHorizontal = viewAngleHorizontal / rayCountHorizontal;
            
            List<Vector3> viewPoints = new List<Vector3>();
            
            for (int h = 0; h <= rayCountHorizontal; h++)
            {
                float horizontalAngle = startAngleHorizontal + angleStepHorizontal * h;
                Vector3 dir = FieldOfViewUtils.DirFromAngle(horizontalAngle, 0f);
                FieldOfViewUtils.ViewCastInfo info = FieldOfViewUtils.ViewCast2D(origin, dir, viewRadius, obstacleMask, target);
                viewPoints.Add(info.point);
            }

            if (viewPoints.Count < 3)
                return;

            int vertexCount = viewPoints.Count + 1;
            Vector3[] vertices = new Vector3[vertexCount];
            List<int> triangles = new List<int>();

            vertices[0] = Vector3.zero;

            for (int i = 0; i < viewPoints.Count; i++)
            {
                vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            }

            for (int i = 1; i < viewPoints.Count; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }

        private void DetectTarget()
        {
            canSeeTarget = false;

            if (target == null)
                return;

            Vector3 directionToTarget = (target.position - origin).normalized;
            float distanceToTarget = Vector3.Distance(origin, target.position);

            if (distanceToTarget > viewRadius)
                return;

            Vector3 forward = FieldOfViewUtils.DirFromAngle(transform.eulerAngles.z, 0f);
            float angleToTarget = FieldOfViewUtils.CalculateAngleIn2D(forward, directionToTarget);
            
            if (angleToTarget == float.MaxValue || angleToTarget > viewAngleHorizontal / 2f)
                return;

            canSeeTarget = FieldOfViewUtils.CheckLineOfSight2D(origin, target.position, distanceToTarget, obstacleMask, target);
        }

        private void UpdateMeshColor()
        {
            if (fovMaterial == null)
            {
                if (meshRenderer != null && meshRenderer.material != null)
                {
                    fovMaterial = meshRenderer.material;
                }
                else
                {
                    return;
                }
            }

            Color meshColor = canSeeTarget ? new Color(1f, 0f, 0f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);
            
            if (meshRenderer != null)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", meshColor);
                meshRenderer.SetPropertyBlock(propBlock);
            }
            else
            {
                fovMaterial.color = meshColor;
            }
        }

        private void OnDrawGizmos()
        {
            if (!showDebugInfo)
                return;

            DrawFieldOfView();
        }

        private void OnDrawGizmosSelected()
        {
            DrawFieldOfView();
        }

        private void DrawFieldOfView()
        {
            Vector3 currentOrigin = Application.isPlaying ? origin : transform.position;
            float currentZRotation = transform.eulerAngles.z;
            float currentStartAngleHorizontal = Application.isPlaying ? startAngleHorizontal : currentZRotation - viewAngleHorizontal / 2f;

            bool playerInFOV = canSeeTarget;
            Color baseColor = playerInFOV ? new Color(1f, 0f, 0f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Color rayColor = playerInFOV ? new Color(1f, 0f, 0f, 0.5f) : new Color(0.6f, 0.6f, 0.6f, 0.4f);

            Gizmos.color = baseColor;
            DrawCircleGizmo(currentOrigin, viewRadius);

            int gizmoRayCount = Mathf.Max(20, rayCountHorizontal / 2);
            float angleStep = viewAngleHorizontal / gizmoRayCount;

            Vector3 leftBoundaryDir = FieldOfViewUtils.DirFromAngle(currentStartAngleHorizontal, 0f);
            Gizmos.color = rayColor;
            Gizmos.DrawRay(currentOrigin, leftBoundaryDir * viewRadius);

            Vector3 rightBoundaryDir = FieldOfViewUtils.DirFromAngle(currentStartAngleHorizontal + viewAngleHorizontal, 0f);
            Gizmos.DrawRay(currentOrigin, rightBoundaryDir * viewRadius);

            List<Vector3> arcPoints = new List<Vector3>();
            
            for (int i = 0; i <= gizmoRayCount; i++)
            {
                float horizontalAngle = currentStartAngleHorizontal + angleStep * i;
                Vector3 direction = FieldOfViewUtils.DirFromAngle(horizontalAngle, 0f);
                
                Vector3 endPoint;
                bool hitObstacle = false;
                
                if (Application.isPlaying)
                {
                    FieldOfViewUtils.ViewCastInfo info = FieldOfViewUtils.ViewCast2D(currentOrigin, direction, viewRadius, obstacleMask);
                    endPoint = info.point;
                    hitObstacle = info.hit;
                }
                else
                {
                    endPoint = currentOrigin + direction * viewRadius;
                }

                arcPoints.Add(endPoint);

                Gizmos.color = hitObstacle ? new Color(rayColor.r, rayColor.g, rayColor.b, rayColor.a * 0.5f) : rayColor;
                float rayLength = hitObstacle ? Vector3.Distance(currentOrigin, endPoint) : viewRadius;
                Gizmos.DrawRay(currentOrigin, direction * rayLength);
            }

            #if UNITY_EDITOR
            if (arcPoints.Count >= 2)
            {
                Vector3[] vertices = new Vector3[arcPoints.Count + 1];
                vertices[0] = currentOrigin;
                for (int i = 0; i < arcPoints.Count; i++)
                {
                    vertices[i + 1] = arcPoints[i];
                }

                Handles.color = baseColor;
                Handles.DrawAAConvexPolygon(vertices);
            }
            #endif

            for (int i = 1; i < arcPoints.Count; i++)
            {
                Gizmos.color = rayColor;
                Gizmos.DrawLine(arcPoints[i - 1], arcPoints[i]);
            }

            if (target != null)
            {
                Gizmos.color = canSeeTarget ? Color.red : new Color(0.5f, 0.5f, 0.5f, 0.3f);
                Gizmos.DrawLine(currentOrigin, target.position);
            }
        }

        private void DrawCircleGizmo(Vector3 center, float radius)
        {
            int segments = 32;
            float angleStep = 360f / segments;
            Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }

    }
}