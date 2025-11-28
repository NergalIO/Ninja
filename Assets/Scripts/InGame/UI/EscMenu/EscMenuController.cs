using UnityEngine;
using UnityEngine.UI;
using Ninja.Managers;

namespace Ninja.InGame.UI
{
    public class EscMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject gameUI;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            playButton.onClick.AddListener(OnPlayButton);
            settingsButton.onClick.AddListener(OnSettingsButton);
            exitButton.onClick.AddListener(OnExitButton);
        }

        private void OnEnable()
        {
            gameUI.SetActive(false);
            Time.timeScale = 0;           
        }

        private void OnDisable()
        {
            gameUI.SetActive(true);
            Time.timeScale = 1;
        }

        private void OnExitButton()
        {
            AsyncSceneLoader.Instance.LoadScene("Menu");
        }

        private void OnPlayButton()
        {
            gameObject.SetActive(false);
        }

        private void OnSettingsButton()
        {
            Debug.Log("EscMenuController: Settings menu not implemented!");
        }
    }
}

