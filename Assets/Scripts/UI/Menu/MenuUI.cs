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

        public void Start()
        {
            playButton.onClick.AddListener(() => OpenMenu(playMenu));
            settingsButton.onClick.AddListener(() => OpenMenu(settingsMenu));
            quitButton.onClick.AddListener(() => OpenMenu(quitMenu));

            CloseAllMenus();
        }

        public void OpenMenu(MenuBase menuToOpen)
        {
            CloseAllMenus();
            menuToOpen.Open();
        }

        public void CloseMenu(MenuBase menuToClose)
        {
            menuToClose.Close();
        }

        public void CloseAllMenus()
        {
            playMenu.Close();
            settingsMenu.Close();
            quitMenu.Close();
        }
    }
}