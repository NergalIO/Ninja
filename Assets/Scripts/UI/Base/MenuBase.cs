using UnityEngine;
using UnityEngine.UI;

namespace Ninja.UI
{
    public class MenuBase : MonoBehaviour
    {
        public enum MenuState
        {
            Open,
            Closed
        }

        [Header("Components")]
        [SerializeField] private Button closeButton;

        [Header("Variables")]
        [SerializeField] private MenuState state = MenuState.Closed;
        [SerializeField] public bool closeOnEsc = true;

        [Tooltip("Если true — меню закрывается при потере фокуса")]
        public bool CloseOnFocusLost = false;

        public MenuState CurrentState => state;

        protected virtual void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (state == MenuState.Closed)
                gameObject.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);
        }

        public bool IsFocused =>
            UIController.Instance.FocusedMenu == this;

        public virtual void Update()
        {
            if (!IsFocused)
                return;
        }

        public void Toggle()
        {
            if (state == MenuState.Open) Close();
            else Open();
        }

        public virtual void Open()
        {
            state = MenuState.Open;
            gameObject.SetActive(true);

            UIController.Instance.FocusMenu(this);
        }

        public virtual void Close()
        {
            state = MenuState.Closed;
            gameObject.SetActive(false);

            UIController.Instance.UnfocusMenu(this);
        }

        public virtual void OnEscPressed()
        {
            if (closeOnEsc)
                Close();
        }
    }
}

