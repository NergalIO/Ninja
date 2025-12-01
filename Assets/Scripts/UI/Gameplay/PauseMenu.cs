using Ninja.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Ninja.UI.Gameplay
{
    public class PauseMenu : MenuBase
    {
        [Header("References")]
        [SerializeField] private MenuBase settingsMenu;
        [SerializeField] private MenuBase quitMenu;

        [Header("Components")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        protected override void Awake()
        {
            resumeButton.onClick.AddListener(OnResumeButton);
            settingsButton.onClick.AddListener(OnSettingsButton);
            quitButton.onClick.AddListener(OnQuitButton);

            base.Awake();
        }

        private void OnEnable()
        {
            if (!GameManager.Instance.IsPaused)
                GameManager.Instance.TogglePause();

            UIController.Instance.FocusMenu(this);
        }

        private void OnDisable()
        {
            if (GameManager.Instance.IsPaused)
                GameManager.Instance.TogglePause();

            UIController.Instance.UnfocusMenu(this);
        }

        private void OnResumeButton()
        {
            Close();
        }

        private void OnSettingsButton()
        {
            settingsMenu.Open();
            UIController.Instance.FocusMenu(settingsMenu);
        }

        private void OnQuitButton()
        {
            quitMenu.Open();
            UIController.Instance.FocusMenu(quitMenu);
        }

        public override void OnEscPressed()
        {
            if (!IsFocused)
                return;

            Close();
        }
    }
}
