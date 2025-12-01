using UnityEngine;

using Ninja.Input;
using Ninja.Systems;


namespace Ninja.Gameplay.Player
{
    public class MovementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputController controller;
        [SerializeField] private Rigidbody2D rigidbody2d;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private MouseInputController mouseInputController;

        [Header("Settings")]
        [SerializeField] private float defaultSpeed = 5f;
        [SerializeField] private float crouchSpeed = 3f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float rotationSpeed = 10f;

        public bool IsCrouching => controller.IsCrouching;
        public bool IsSprinting => controller.IsSprinting;
        public Vector2 CurrentSpeed => rigidbody2d.linearVelocity;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponentInChildren<PlayerInputController>();
            }

            if (rigidbody2d == null)
            {
                rigidbody2d = GetComponent<Rigidbody2D>();
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mouseInputController == null)
            {
                mouseInputController = GetComponentInChildren<MouseInputController>();
            }
        }

        private void FixedUpdate()
        {
            float currentSpeed = defaultSpeed;
            if (controller.IsCrouching)
                currentSpeed = crouchSpeed;
            else if (controller.IsSprinting)
                currentSpeed = sprintSpeed;
            rigidbody2d.linearVelocity = controller.MoveDirection * currentSpeed * Time.fixedDeltaTime;

            RotateTowardsMouse();
        }

        private void RotateTowardsMouse()
        {
            if (mainCamera == null || mouseInputController == null)
                return;

            Vector2 mouseScreenPosition = mouseInputController.MousePosition;
            
            if (mouseScreenPosition.magnitude < 0.1f)
                return;

            float distanceFromCamera = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, distanceFromCamera));
            mouseWorldPosition.z = transform.position.z;

            Vector3 directionToMouse = (mouseWorldPosition - transform.position).normalized;
            
            if (directionToMouse.magnitude < 0.1f)
                return;

            float targetAngle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;
            
            float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
            float newAngle = currentAngle + angleDifference * rotationSpeed * Time.fixedDeltaTime;
            
            transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        }

        // WinZone теперь обрабатывается через скрипт WinZone на самом объекте WinZone
        // Это предотвращает ложные срабатывания от дочерних объектов игрока (например, NoiseArea)
    }
}

