using UnityEngine;
using System.Collections.Generic;

namespace Ninja.Gameplay.Environment
{
    /// <summary>
    /// Зона тени - игрок в этой зоне хуже обнаруживается врагами
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShadowZone : MonoBehaviour
    {
        private static HashSet<Transform> objectsInShadow = new();
        
        /// <summary>
        /// Проверить находится ли объект в тени
        /// </summary>
        public static bool IsInShadow(Transform target)
        {
            return target != null && objectsInShadow.Contains(target);
        }

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                objectsInShadow.Add(other.transform);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                objectsInShadow.Remove(other.transform);
            }
        }

        private void OnDestroy()
        {
            // Очищаем при уничтожении сцены
            objectsInShadow.Clear();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var col = GetComponent<Collider2D>();
            if (col == null) return;
            
            Gizmos.color = new Color(0.2f, 0.2f, 0.4f, 0.3f);
            
            if (col is BoxCollider2D box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
#endif
    }
}
