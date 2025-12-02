using UnityEngine;

namespace Ninja.Core
{
    public static class GizmosUtils
    {
        public static void DrawArc(Vector3 center, float radius, float startAngle, float endAngle, int segments = 20)
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

        public static void DrawArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.2f)
        {
            Vector3 direction = (end - start).normalized;
            Vector3 right = Vector3.Cross(Vector3.forward, direction).normalized;
            Vector3 arrowHead1 = end - direction * arrowHeadLength + right * arrowHeadLength * 0.5f;
            Vector3 arrowHead2 = end - direction * arrowHeadLength - right * arrowHeadLength * 0.5f;

            Gizmos.DrawLine(end, arrowHead1);
            Gizmos.DrawLine(end, arrowHead2);
        }

        public static void DrawCircle(Vector3 center, float radius, int segments = 32)
        {
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

        public static void DrawSector(Vector3 origin, float radius, float centerAngle, float angleRange, int segments = 20)
        {
            float halfAngle = angleRange * 0.5f;
            float leftAngle = centerAngle - halfAngle;
            float rightAngle = centerAngle + halfAngle;

            float leftAngleRad = leftAngle * Mathf.Deg2Rad;
            float rightAngleRad = rightAngle * Mathf.Deg2Rad;

            Vector2 leftDir = new Vector2(Mathf.Cos(leftAngleRad), Mathf.Sin(leftAngleRad));
            Vector2 rightDir = new Vector2(Mathf.Cos(rightAngleRad), Mathf.Sin(rightAngleRad));

            Vector3 leftEnd = origin + new Vector3(leftDir.x, leftDir.y, 0) * radius;
            Vector3 rightEnd = origin + new Vector3(rightDir.x, rightDir.y, 0) * radius;

            Gizmos.DrawLine(origin, leftEnd);
            Gizmos.DrawLine(origin, rightEnd);

            DrawArc(origin, radius, leftAngle, rightAngle, segments);
        }
    }
}

