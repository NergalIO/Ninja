using UnityEngine;
using UnityEngine.InputSystem;
using Keyboard;

namespace Ninja.Input
{
    [RequireComponent(typeof(MouseInputController), typeof(PlayerInputController))]
    public class InputController : MonoBehaviour, KeyboardBinds.IPlayerActions
    {
        private KeyboardBinds inputActions;

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool Interact { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool Crouch { get; private set; }
        public bool Sprint { get; private set; }

        private void Awake()
        {
            inputActions = new KeyboardBinds();
            inputActions.Player.AddCallbacks(this);
            inputActions.Enable();
        }

        private void OnDestroy()
        {
            inputActions?.Player.RemoveCallbacks(this);
            inputActions?.Dispose();
        }

        private void LateUpdate() => InteractPressed = false;

        public void OnMove(InputAction.CallbackContext ctx) => Move = ctx.ReadValue<Vector2>();
        public void OnLook(InputAction.CallbackContext ctx) => Look = ctx.ReadValue<Vector2>();
        public void OnCrouch(InputAction.CallbackContext ctx) => Crouch = ctx.ReadValue<float>() > 0.5f;
        public void OnSprint(InputAction.CallbackContext ctx) => Sprint = ctx.ReadValue<float>() > 0.5f;

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                Interact = true;
                InteractPressed = true;
            }
            else if (ctx.canceled)
            {
                Interact = false;
            }
        }
    }
}
