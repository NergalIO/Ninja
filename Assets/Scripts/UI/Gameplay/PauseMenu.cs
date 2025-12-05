using UnityEngine;
using UnityEngine.UI;
using Ninja.Systems;

namespace Ninja.UI.Gameplay
{
    public class PauseMenu : MenuBase
    {
        [SerializeField] private MenuBase settingsMenu;
        [SerializeField] private MenuBase quitMenu;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        protected override void Awake()
        {
            resumeButton?.onClick.AddListener(Close);
            settingsButton?.onClick.AddListener(OpenSettings);
            quitButton?.onClick.AddListener(OpenQuit);
            base.Awake();
        }

        private void OnEnable()
        {
            if (GameManager.Instance && !GameManager.Instance.IsPaused)
                GameManager.Instance.TogglePause();
            UIController.Instance?.FocusMenu(this);
        }

        private void OnDisable()
        {
            if (GameManager.Instance && GameManager.Instance.IsPaused)
                GameManager.Instance.TogglePause();
            UIController.Instance?.UnfocusMenu(this);
        }

        private void OpenSettings()
        {
            settingsMenu?.Open();
            UIController.Instance?.FocusMenu(settingsMenu);
        }

        private void OpenQuit()
        {
            quitMenu?.Open();
            UIController.Instance?.FocusMenu(quitMenu);
        }

        public override void OnEscPressed()
        {
            if (IsFocused) Close();
        }
    }
}
