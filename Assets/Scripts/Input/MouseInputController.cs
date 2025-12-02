using UnityEngine;
using UnityEngine.InputSystem;

using Ninja.Input;


namespace Ninja.Input
{
    public class MouseInputController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputController inputController;

        public Vector2 LookDirection => inputController?.Look ?? Vector2.zero;
        
        public Vector2 MousePosition
        {
            get
            {
                // Используем UIInputController.Instance (Singleton из другой сцены)
                // Теперь Point возвращает абсолютную позицию мыши, а не дельту
                UIInputController uiInputController = UIInputController.Instance;
                Vector2 position = uiInputController?.Point ?? Vector2.zero;
                
                // Если позиция равна нулю или UIInputController недоступен, используем прямое чтение из Input System
                if (position.magnitude < 0.1f)
                {
                    if (Mouse.current != null)
                    {
                        position = Mouse.current.position.ReadValue();
                    }
                    else
                    {
                        // Fallback на старый Input
                        position = UnityEngine.Input.mousePosition;
                    }
                }
                
                return position;
            }
        }
        
        public bool IsClicking => UIInputController.Instance?.Click ?? false;
        public bool IsRightClicking => UIInputController.Instance?.RightClick ?? false;
        public bool IsMiddleClicking => UIInputController.Instance?.MiddleClick ?? false;
        public Vector2 ScrollDelta => UIInputController.Instance?.ScrollWheel ?? Vector2.zero;
    }
}

