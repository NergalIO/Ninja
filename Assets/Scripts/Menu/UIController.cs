using Ninja.Settings;
using UnityEngine;
using UnityEngine.UI;


namespace Ninja.UI.Menu
{
    public class UIController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("Windows")]
        [SerializeField] private GameObject playWindow;
        [SerializeField] private GameObject exitWindow;

        private void Awake()
        {
            Debug.Log("UIController: Awake");
            CloseAllWindows();
            playButton.onClick.AddListener(OnPlayButton);
            settingsButton.onClick.AddListener(OnSettingsButton);
            exitButton.onClick.AddListener(OnExitButton);
        }

        private void OnPlayButton()
        {
            Debug.Log("UIController: OnPlayButton");
            CloseAllWindows();
            playWindow.SetActive(true);
        }

        private void OnSettingsButton()
        {
            Debug.Log("UIController: OnSettingsButton");
            CloseAllWindows();
            if (SettingsManager.Instance.IsMenuOpen)
                SettingsManager.Instance.HideSettings();
            else
                SettingsManager.Instance.ShowSettings();
        }

        private void OnExitButton()
        {
            Debug.Log("UIController: OnExitButton");
            CloseAllWindows();
            exitWindow.SetActive(true);
        }

        public void CloseAllWindows()
        {
            Debug.Log("UIController: CloseAllWindows");
            playWindow.SetActive(false);
            SettingsManager.Instance.HideSettings();
            exitWindow.SetActive(false);
        }
    }
}