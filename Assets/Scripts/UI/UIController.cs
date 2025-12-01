using UnityEngine;
using Ninja.Core;
using Ninja.Input;
using Ninja.UI.Menu;
using Ninja.UI.Gameplay;

namespace Ninja.UI
{
    public class UIController : PersistentSingleton<UIController>
    {
        [Header("References")]
        [SerializeField] private MenuBase pauseMenu;

        [Header("Debug")]
        [SerializeField] private MenuBase previousMenu;
        [SerializeField] private MenuBase focusedMenu;

        public MenuBase FocusedMenu => focusedMenu;

        public void FocusMenu(MenuBase menu)
        {
            if (menu == null)
                return;

            // Не меняем, если пытаемся сфокусировать то же самое
            if (focusedMenu == menu)
                return;

            // Если есть фокус, сохраняем его как previous
            if (focusedMenu != null)
                previousMenu = focusedMenu;

            // Снимаем фокус с предыдущего (если требуется)
            if (focusedMenu != null && focusedMenu.CloseOnFocusLost)
                focusedMenu.Close();

            focusedMenu = menu;

            // Если меню закрыто — открываем
            if (focusedMenu.CurrentState == MenuBase.MenuState.Closed)
                focusedMenu.Open();
        }

        public void UnfocusMenu(MenuBase menu)
        {
            if (menu == null)
                return;

            // Если снимаем фокус не с текущего меню — игнорируем
            if (focusedMenu != menu)
                return;

            // Снимаем фокус
            focusedMenu = null;

            // Если есть предыдущее меню — ставим в фокус его
            if (previousMenu != null)
            {
                FocusMenu(previousMenu);
                previousMenu = null;
            }
        }

        private void Update()
        {
            if (!UIInputController.Instance.EscMenu)
                return;

            if (focusedMenu == null)
            {
                if (pauseMenu == null)
                    pauseMenu = FindAnyObjectByType<PauseMenu>(FindObjectsInactive.Include);

                if (pauseMenu != null)
                    pauseMenu.Open();

                return;
            }

            focusedMenu.OnEscPressed();
        }
    }
}
