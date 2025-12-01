using UnityEngine;
using UnityEngine.UI;

namespace Ninja.UI.Menu
{
    public class MenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MenuBase playMenu;
        [SerializeField] private MenuBase settingsMenu;
        [SerializeField] private MenuBase quitMenu;

        [Header("Components")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            playButton.onClick.AddListener(() => OpenMenu(playMenu));
            settingsButton.onClick.AddListener(() => OpenMenu(settingsMenu));
            quitButton.onClick.AddListener(() => OpenMenu(quitMenu));

            CloseAllMenus();
        }

        private void OpenMenu(MenuBase menuToOpen)
        {
            menuToOpen.Open();
            UIController.Instance.FocusMenu(menuToOpen);
        }

        private void CloseMenu(MenuBase menuToClose)
        {
            menuToClose.Close();
            UIController.Instance.UnfocusMenu(menuToClose);
        }

        private void CloseAllMenus()
        {
            CloseMenu(playMenu);
            CloseMenu(settingsMenu);
            CloseMenu(quitMenu);
        }
    }
}
