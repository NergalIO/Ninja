using UnityEngine;
using System.Collections.Generic;

namespace Ninja.Gameplay.Enemy
{
    public class FieldOfView : MonoBehaviour
    {
        [Header("FOV Settings")]
        [SerializeField] private float viewRadius = 6f;
        [Range(0, 360)]
        [SerializeField] private float viewAngle = 90f;

        [Header("Ray settings")]
        [SerializeField] private int rayCount = 60;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstacleMask;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo;

        private Transform target;
        private Mesh mesh;
        private Vector3 origin;
        private float startAngle;
        private bool canSeeTarget;

        public float ViewRadius => viewRadius;
        public float ViewAngle => viewAngle;
        public bool CanSeeTarget => canSeeTarget;

        private void Start()
        {
            mesh = new Mesh();
            mesh.name = "FOV Mesh";
            GetComponent<MeshFilter>().mesh = mesh;
        }

        public void SetTarget(Transform targetTransform)
        {
            target = targetTransform;
        }

        private void LateUpdate()
        {
            origin = transform.position;
            startAngle = transform.eulerAngles.y - viewAngle / 2f;

            GenerateMesh();
            DetectTarget();
        }

        private void GenerateMesh()
        {
            float angleStep = viewAngle / rayCount;

            List<Vector3> viewPoints = new List<Vector3>();
            for (int i = 0; i <= rayCount; i++)
            {
                float angle = startAngle + angleStep * i;
                ViewCastInfo info = ViewCast(angle);
                viewPoints.Add(info.point);
            }

            int vertexCount = viewPoints.Count + 1;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[(vertexCount - 2) * 3];

            vertices[0] = Vector3.zero;

            for (int i = 0; i < viewPoints.Count; i++)
            {
                vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            }

            int triIndex = 0;
            for (int i = 1; i < vertexCount - 1; i++)
            {
                triangles[triIndex++] = 0;
                triangles[triIndex++] = i;
                triangles[triIndex++] = i + 1;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }

        private ViewCastInfo ViewCast(float angle)
        {
            Vector3 dir = DirFromAngle(angle);

            if (Physics.Raycast(origin, dir, out RaycastHit hit, viewRadius, obstacleMask))
            {
                return new ViewCastInfo(true, hit.point, hit.distance, angle);
            }
            else
            {
                return new ViewCastInfo(false, origin + dir * viewRadius, viewRadius, angle);
            }
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

            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (angleToTarget > viewAngle / 2f)
                return;

            // Check if there's an obstacle blocking the view to the target
            if (Physics.Raycast(origin, directionToTarget, out RaycastHit hit, distanceToTarget, obstacleMask))
            {
                // Obstacle is blocking the view
                canSeeTarget = false;
            }
            else
            {
                // No obstacle, can see the target
                canSeeTarget = true;
            }
        }

        public Vector3 DirFromAngle(float angleInDegrees)
        {
            float rad = (angleInDegrees) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
        }

        public struct ViewCastInfo
        {
            public bool hit;
            public Vector3 point;
            public float distance;
            public float angle;

            public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
            {
                this.hit = hit;
                this.point = point;
                this.distance = distance;
                this.angle = angle;
            }
        }
    }
}