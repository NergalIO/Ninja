using UnityEngine;
using Ninja.InGame.UI;

namespace Ninja.InGame.InputSystem
{
    public class MouseInputController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputController inputController;
        [SerializeField] private UIInputController uiInputController;

        public Vector2 LookDirection => inputController?.Look ?? Vector2.zero;
        public Vector2 MousePosition => uiInputController?.Point ?? Vector2.zero;
        public bool IsClicking => uiInputController?.Click ?? false;
        public bool IsRightClicking => uiInputController?.RightClick ?? false;
        public bool IsMiddleClicking => uiInputController?.MiddleClick ?? false;
        public Vector2 ScrollDelta => uiInputController?.ScrollWheel ?? Vector2.zero;
    }
}
