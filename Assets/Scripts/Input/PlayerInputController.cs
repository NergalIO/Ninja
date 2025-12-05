using UnityEngine;

namespace Ninja.Input
{
    public class PlayerInputController : MonoBehaviour
    {
        private InputController input;
        private MouseInputController mouse;

        public Vector2 MoveDirection => input?.Move ?? Vector2.zero;
        public Vector2 LookDirection => input?.Look ?? Vector2.zero;
        public Vector2 MousePosition => mouse?.MousePosition ?? Vector2.zero;
        public bool IsInteracting => input?.Interact ?? false;
        public bool IsInteractPressed => input?.InteractPressed ?? false;
        public bool IsCrouching => input?.Crouch ?? false;
        public bool IsSprinting => input?.Sprint ?? false;

        private void Awake()
        {
            input = GetComponent<InputController>();
            mouse = GetComponent<MouseInputController>();
        }
    }
}
