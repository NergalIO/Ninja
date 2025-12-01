using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public static class FieldOfViewUtils
    {
        public static Vector3 DirFromAngle(float horizontalAngleInDegrees, float verticalAngleInDegrees = 0f)
        {
            float horizontalRad = horizontalAngleInDegrees * Mathf.Deg2Rad;
            float verticalRad = verticalAngleInDegrees * Mathf.Deg2Rad;

            float x = Mathf.Cos(horizontalRad) * Mathf.Cos(verticalRad);
            float y = Mathf.Sin(horizontalRad) * Mathf.Cos(verticalRad);
            float z = Mathf.Sin(verticalRad);

            return new Vector3(x, y, z);
        }

        public static ViewCastInfo ViewCast2D(Vector3 origin, Vector3 direction, float maxDistance, LayerMask obstacleMask, Transform target = null)
        {
            LayerMask maskToUse = GetObstacleMask(obstacleMask);
            
            Vector2 origin2D = new Vector2(origin.x, origin.y);
            Vector2 direction2D = new Vector2(direction.x, direction.y);
            
            RaycastHit2D hit = Physics2D.Raycast(origin2D, direction2D, maxDistance, maskToUse);
            
            if (hit.collider != null)
            {
                if (target != null && hit.collider.transform == target)
                {
                    return new ViewCastInfo(false, origin + direction * maxDistance, maxDistance, 0f);
                }
                Vector3 hitPoint = new Vector3(hit.point.x, hit.point.y, origin.z);
                return new ViewCastInfo(true, hitPoint, hit.distance, 0f);
            }
            else
            {
                return new ViewCastInfo(false, origin + direction * maxDistance, maxDistance, 0f);
            }
        }

        public static bool CheckLineOfSight2D(Vector3 origin, Vector3 targetPosition, float maxDistance, LayerMask obstacleMask, Transform target = null)
        {
            Vector3 directionToTarget = (targetPosition - origin).normalized;
            float distanceToTarget = Vector3.Distance(origin, targetPosition);

            if (distanceToTarget > maxDistance)
                return false;

            LayerMask maskToUse = GetObstacleMask(obstacleMask);
            Vector2 origin2D = new Vector2(origin.x, origin.y);
            Vector2 dirToTarget2D = new Vector2(directionToTarget.x, directionToTarget.y);
            
            RaycastHit2D hit = Physics2D.Raycast(origin2D, dirToTarget2D, distanceToTarget, maskToUse);
            
            if (hit.collider != null)
            {
                if (hit.collider.transform == target)
                {
                    return true;
                }
                float hitDistance = hit.distance;
                return hitDistance >= distanceToTarget - 0.1f;
            }
            
            return true;
        }

        public static float CalculateAngleIn2D(Vector3 forward, Vector3 directionToTarget)
        {
            Vector3 forward2D = new Vector3(forward.x, forward.y, 0f).normalized;
            Vector3 directionToTarget2D = new Vector3(directionToTarget.x, directionToTarget.y, 0f).normalized;
            
            if (forward2D.magnitude < 0.1f || directionToTarget2D.magnitude < 0.1f)
                return float.MaxValue;
            
            return Vector3.Angle(forward2D, directionToTarget2D);
        }

        public static LayerMask GetObstacleMask(LayerMask obstacleMask)
        {
            if (obstacleMask.value == 0)
            {
                return ~(1 << 4);
            }
            return obstacleMask;
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

