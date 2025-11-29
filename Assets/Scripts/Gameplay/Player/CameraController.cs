using UnityEngine;
namespace Ninja.Gameplay.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera followingCamera;
        [SerializeField] private Transform objectToLook;

        [Header("Preferences")]
        [SerializeField] private float currentSpeed = 0f;
        [SerializeField] private Vector2 availableSpeed = new();

        private void Awake()
        {
            if (followingCamera == null)
            {
                followingCamera = Camera.main;
            }
            
            if (objectToLook == null)
            {
                Debug.LogWarning("CameraController: Object to look no setted!");
            }
        }

        private void FixedUpdate()
        {
            if (objectToLook == null) return;

            Vector2 currentPosition = transform.position;
            Vector2 targetPosition = objectToLook.position;
            Vector2 direction = (targetPosition - currentPosition).normalized;
            float distance = Vector2.Distance(currentPosition, targetPosition);

            currentSpeed = Mathf.Clamp(distance, availableSpeed.x, availableSpeed.y);

            transform.position += (Vector3)direction * currentSpeed * Time.fixedDeltaTime;
        }
    }
}
