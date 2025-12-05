using UnityEngine;
using Ninja.Core;
using Ninja.Input;
using Ninja.UI.Gameplay;

namespace Ninja.UI
{
    public class UIController : PersistentSingleton<UIController>
    {
        [SerializeField] private MenuBase pauseMenu;
        [SerializeField] private MenuBase previousMenu;
        [SerializeField] private MenuBase focusedMenu;

        public MenuBase FocusedMenu => focusedMenu;

        public void FocusMenu(MenuBase menu)
        {
            if (menu == null || focusedMenu == menu) return;

            if (focusedMenu != null)
            {
                previousMenu = focusedMenu;
                if (focusedMenu.CloseOnFocusLost)
                    focusedMenu.Close();
            }

            focusedMenu = menu;
            if (focusedMenu.CurrentState == MenuBase.MenuState.Closed)
                focusedMenu.Open();
        }

        public void UnfocusMenu(MenuBase menu)
        {
            if (menu == null || focusedMenu != menu) return;

            focusedMenu = null;
            if (previousMenu != null)
            {
                FocusMenu(previousMenu);
                previousMenu = null;
            }
        }

        private void Update()
        {
            if (!UIInputController.Instance.EscMenu) return;

            if (focusedMenu == null)
            {
                pauseMenu ??= FindAnyObjectByType<PauseMenu>(FindObjectsInactive.Include);
                pauseMenu?.Open();
                return;
            }

            focusedMenu.OnEscPressed();
        }
    }
}
