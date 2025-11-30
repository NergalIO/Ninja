using UnityEngine;


namespace Ninja.Input
{
    public class PlayerInputController : MonoBehaviour
    {
        private InputController _inputController;

        public Vector2 MoveDirection => _inputController?.Move ?? Vector2.zero;
        public Vector2 LookDirection => _inputController?.Look ?? Vector2.zero;
        public bool IsInteracting => _inputController?.Interact ?? false;
        public bool IsCrouching => _inputController?.Crouch ?? false;
        public bool IsSprinting => _inputController?.Sprint ?? false;

        private void Awake()
        {
            _inputController = GetComponent<InputController>();
        }
    }
}

