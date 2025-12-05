using UnityEngine;
using UnityEngine.InputSystem;

namespace Ninja.Input
{
    public class MouseInputController : MonoBehaviour
    {
        [SerializeField] private InputController inputController;

        public Vector2 LookDirection => inputController?.Look ?? Vector2.zero;

        public Vector2 MousePosition
        {
            get
            {
                var pos = UIInputController.Instance?.Point ?? Vector2.zero;
                if (pos.magnitude < 0.1f)
                    pos = Mouse.current?.position.ReadValue() ?? (Vector2)UnityEngine.Input.mousePosition;
                return pos;
            }
        }

        public bool IsClicking => UIInputController.Instance?.Click ?? false;
        public bool IsRightClicking => UIInputController.Instance?.RightClick ?? false;
        public bool IsMiddleClicking => UIInputController.Instance?.MiddleClick ?? false;
        public Vector2 ScrollDelta => UIInputController.Instance?.ScrollWheel ?? Vector2.zero;
    }
}
