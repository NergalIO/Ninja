using UnityEngine;
using Keyboard;
using UnityEngine.InputSystem;

using Ninja.Core;


namespace Ninja.Input
{
    public class UIInputController : PersistentSingleton<UIInputController>, KeyboardBinds.IUIActions
    {
        private KeyboardBinds _inputActions;

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
        private bool escReleasedFrame;
        public bool EscMenu 
        {
            get 
            { 
                bool tmp = escReleasedFrame;
                escReleasedFrame = false;
                return tmp;
            }
            private set 
            { 
                escReleasedFrame = value; 
            }
        }

        protected override void OnSingletonInitialized()
        {
            _inputActions = new KeyboardBinds();
            _inputActions.UI.AddCallbacks(this);
            _inputActions.Enable();
        }

        protected override void OnDestroy()
        {
            if (_inputActions != null)
            {
                _inputActions.UI.RemoveCallbacks(this);
                _inputActions.Dispose();
            }
            base.OnDestroy();
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
            Navigate = context.ReadValue<Vector2>();
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            Submit = context.ReadValue<float>() > 0.5f;
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            Cancel = context.ReadValue<float>() > 0.5f;
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            Point = context.ReadValue<Vector2>();
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            Click = context.ReadValue<float>() > 0.5f;
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            RightClick = context.ReadValue<float>() > 0.5f;
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
            MiddleClick = context.ReadValue<float>() > 0.5f;
        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
            ScrollWheel = context.ReadValue<Vector2>();
        }

        public void OnTrackedDevicePosition(InputAction.CallbackContext context)
        {
            TrackedDevicePosition = context.ReadValue<Vector3>();
        }

        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
        {
            TrackedDeviceOrientation = context.ReadValue<Quaternion>();
        }

        public void OnESCMenu(InputAction.CallbackContext context)
        {
            EscMenu = context.canceled;
        }
    }
}

