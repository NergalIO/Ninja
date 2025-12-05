using UnityEngine;
using UnityEngine.UI;

namespace Ninja.UI
{
    public class MenuBase : MonoBehaviour
    {
        public enum MenuState { Open, Closed }

        [SerializeField] private Button closeButton;
        [SerializeField] private MenuState state = MenuState.Closed;
        [SerializeField] public bool closeOnEsc = true;
        public bool CloseOnFocusLost = false;

        public MenuState CurrentState => state;
        public bool IsFocused => UIController.Instance?.FocusedMenu == this;

        protected virtual void Awake()
        {
            closeButton?.onClick.AddListener(Close);
            if (state == MenuState.Closed)
                gameObject.SetActive(false);
        }

        protected virtual void OnDestroy() =>
            closeButton?.onClick.RemoveListener(Close);

        public virtual void Update() { }

        public void Toggle()
        {
            if (state == MenuState.Open) Close();
            else Open();
        }

        public virtual void Open()
        {
            state = MenuState.Open;
            gameObject.SetActive(true);
            UIController.Instance?.FocusMenu(this);
        }

        public virtual void Close()
        {
            state = MenuState.Closed;
            gameObject.SetActive(false);
            UIController.Instance?.UnfocusMenu(this);
        }

        public virtual void OnEscPressed()
        {
            if (closeOnEsc) Close();
        }
    }
}
