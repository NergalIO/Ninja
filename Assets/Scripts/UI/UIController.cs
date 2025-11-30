using UnityEngine;
using Ninja.Core;
using Ninja.Input;


namespace Ninja.UI
{
    public class UIController : PersistentSingleton<UIController>
    {
        [Header("References")]
        [SerializeField] private MenuBase focusedMenu;

        public MenuBase FocusedMenu => focusedMenu;

        public void FocusMenu(MenuBase menu)
        {
            if (focusedMenu == menu)
                return;

            if (focusedMenu != null && focusedMenu.CloseOnFocusLost)
                focusedMenu.Close();

            focusedMenu = menu;

            if (focusedMenu != null && focusedMenu.CurrentState == MenuBase.MenuState.Closed)
                focusedMenu.Open();
        }

        public void UnfocusMenu(MenuBase menu)
        {
            if (focusedMenu == menu)
                focusedMenu = null;
        }

        private void Update()
        {
            if (focusedMenu == null)
                return;

            if (UIInputController.Instance.EscMenu)
            {
                focusedMenu.OnEscPressed();
            }
        }
    }
}
