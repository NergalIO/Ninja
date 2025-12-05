using UnityEngine;
using Ninja.Input;
using Ninja.Core.Events;

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
        [SerializeField] private float rotationSpeed = 360f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private bool isPaused;

        public bool IsCrouching => controller.IsCrouching;
        public bool IsSprinting => controller.IsSprinting;
        public Vector2 CurrentSpeed => rigidbody2d.linearVelocity;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponentInChildren<PlayerInputController>();

            if (rigidbody2d == null)
                rigidbody2d = GetComponent<Rigidbody2D>();

            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mouseInputController == null)
                mouseInputController = GetComponentInChildren<MouseInputController>();
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

        private void OnGamePaused(EventArgs e)
        {
            isPaused = true;
            rigidbody2d.linearVelocity = Vector2.zero;
        }

        private void OnGameResumed(EventArgs e)
        {
            isPaused = false;
        }

        private void Update()
        {
            if (isPaused) return;
            
            if (mainCamera == null)
                mainCamera = Camera.main;

            Move();
            RotateTowardsMouse();
        }

        private void Move()
        {
            float currentSpeed = defaultSpeed;
            if (controller.IsCrouching)
                currentSpeed = crouchSpeed;
            else if (controller.IsSprinting)
                currentSpeed = sprintSpeed;
                
            Vector2 velocity = controller.MoveDirection * currentSpeed;
            rigidbody2d.linearVelocity = velocity;
        }

        private void RotateTowardsMouse()
        {
            if (mainCamera == null)
                return;

            Vector2 mouseScreen = controller.MousePosition;
            if (mouseScreen.sqrMagnitude < 0.01f)
                return;

            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
            Vector2 direction = (Vector2)mouseWorld - (Vector2)transform.position;
            
            if (direction.sqrMagnitude < 0.0001f)
                return;

            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float newAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
            
            if (showDebugInfo)
                Debug.Log($"Mouse: {mouseScreen} -> {mouseWorld}, Angle: {targetAngle:F1}°");
        }

    }
}

