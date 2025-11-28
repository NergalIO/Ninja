using Ninja.InGame.InputSystem;
using UnityEngine;


namespace Ninja.InGame.Player
{
    public class MovementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputController controller;
        [SerializeField] private Rigidbody2D rigidbody2d;

        [Header("Settings")]
        [SerializeField] private float defaultSpeed = 5f;
        [SerializeField] private float crouchSpeed = 3f;
        [SerializeField] private float sprintSpeed = 7f;

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
        }

        private void FixedUpdate()
        {
            float currentSpeed = defaultSpeed;
            if (controller.IsCrouching)
                currentSpeed = crouchSpeed;
            else if (controller.IsSprinting)
                currentSpeed = sprintSpeed;
            rigidbody2d.linearVelocity = controller.MoveDirection * currentSpeed * Time.fixedDeltaTime;
        }
    }
}
