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

        public void OnResumeButton()
        {
            Close();
        }

        public void OnSettingsButton()
        {
            settingsMenu.Open();
        }

        public void OnQuitButton()
        {
           quitMenu.Open();
        }

        private void OnEnable()
        {
            if (!GameManager.Instance.IsPaused)
                GameManager.Instance.TogglePause();
        }

        private void OnDisable()
        {
            if (GameManager.Instance.IsPaused)
                GameManager.Instance.TogglePause();
        }
    }
}