using UnityEngine;
using UnityEngine.InputSystem;
using Ninja.Core;
using Keyboard;

namespace Ninja.Input
{
    public class UIInputController : PersistentSingleton<UIInputController>, KeyboardBinds.IUIActions
    {
        private KeyboardBinds inputActions;
        private bool escPressed;

        public Vector2 Navigate { get; private set; }
        public bool Submit { get; private set; }
        public bool Cancel { get; private set; }
        public Vector2 Point { get; private set; }
        public bool Click { get; private set; }
        public bool RightClick { get; private set; }
        public bool MiddleClick { get; private set; }
        public Vector2 ScrollWheel { get; private set; }
        public Vector3 TrackedDevicePosition { get; private set; }
        public Quaternion TrackedDeviceOrientation { get; private set; }

        public bool EscMenu
        {
            get
            {
                bool val = escPressed;
                escPressed = false;
                return val;
            }
        }

        protected override void OnSingletonInitialized()
        {
            inputActions = new KeyboardBinds();
            inputActions.UI.AddCallbacks(this);
            inputActions.Enable();
        }

        private void Update()
        {
            if (inputActions?.UI.Point != null)
                Point = inputActions.UI.Point.ReadValue<Vector2>();
        }

        protected override void OnDestroy()
        {
            inputActions?.UI.RemoveCallbacks(this);
            inputActions?.Dispose();
            base.OnDestroy();
        }

        public void OnNavigate(InputAction.CallbackContext ctx) => Navigate = ctx.ReadValue<Vector2>();
        public void OnSubmit(InputAction.CallbackContext ctx) => Submit = ctx.ReadValue<float>() > 0.5f;
        public void OnCancel(InputAction.CallbackContext ctx) => Cancel = ctx.ReadValue<float>() > 0.5f;
        public void OnPoint(InputAction.CallbackContext ctx) => Point = ctx.ReadValue<Vector2>();
        public void OnClick(InputAction.CallbackContext ctx) => Click = ctx.ReadValue<float>() > 0.5f;
        public void OnRightClick(InputAction.CallbackContext ctx) => RightClick = ctx.ReadValue<float>() > 0.5f;
        public void OnMiddleClick(InputAction.CallbackContext ctx) => MiddleClick = ctx.ReadValue<float>() > 0.5f;
        public void OnScrollWheel(InputAction.CallbackContext ctx) => ScrollWheel = ctx.ReadValue<Vector2>();
        public void OnTrackedDevicePosition(InputAction.CallbackContext ctx) => TrackedDevicePosition = ctx.ReadValue<Vector3>();
        public void OnTrackedDeviceOrientation(InputAction.CallbackContext ctx) => TrackedDeviceOrientation = ctx.ReadValue<Quaternion>();
        public void OnESCMenu(InputAction.CallbackContext ctx) => escPressed = ctx.canceled;
    }
}
