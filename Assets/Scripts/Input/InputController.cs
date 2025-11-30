using UnityEngine;
using Keyboard;
using UnityEngine.InputSystem;


namespace Ninja.Input
{
    [RequireComponent(typeof(MouseInputController), typeof(PlayerInputController))]
    public class InputController : MonoBehaviour, KeyboardBinds.IPlayerActions
    {
        private KeyboardBinds _inputActions;

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool Interact { get; private set; }
        public bool Crouch { get; private set; }
        public bool Sprint { get; private set; }

        private void Awake()
        {
            _inputActions = new KeyboardBinds();
            _inputActions.Player.AddCallbacks(this);
            _inputActions.Enable();
        }

        private void OnDestroy()
        {
            if (_inputActions != null)
            {
                _inputActions.Player.RemoveCallbacks(this);
                _inputActions.Dispose();
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Move = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Look = context.ReadValue<Vector2>();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            Interact = context.ReadValue<float>() > 0.5f;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            Crouch = context.ReadValue<float>() > 0.5f;
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            Sprint = context.ReadValue<float>() > 0.5f;
        }
    }
}

