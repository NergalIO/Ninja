using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ninja.UI.Loading;
using Ninja.Systems.Loader;
using Ninja.Core;
using UnityEngine.SceneManagement;
using Ninja.Systems;
using Ninja.Systems.Data;


namespace Ninja.UI.Gameplay
{
    public class LoseWonMenu : MenuBase
    {
        [Header("References")]
        [Header("Text")]
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text aliveTimeText;
        [SerializeField] private TMP_Text detectedTimesText;
        [SerializeField] private TMP_Text heardTimesText;

        [Header("Buttons")]
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private Button restartButton;

        [Header("Variables")]
        [SerializeField] public bool isWon;

        protected override void Awake()
        {
            base.Awake();
            backToMenuButton.onClick.AddListener(OnBackToMenu);
            restartButton.onClick.AddListener(OnRestart);
        }

        public void UpdateStatistics()
        {
            LevelData currentData = GameManager.GetCurrentLevelData();
            
            resultText.text = isWon ? "You Won!" : "You Lose!";
            if (currentData != null && GameManager.Instance != null)
            {
                float survivedTime = GameManager.Instance.CurrentLevelTime;
                aliveTimeText.text = $"Survived Time: {TimeUtils.FormatTime(survivedTime)}";
                
                detectedTimesText.text = $"Detected Times: {currentData.TimesDetected}";

                heardTimesText.text = $"Heard Times: {currentData.TimesHeard}";
            }
            else
            {
                aliveTimeText.text = "Survived Time: --";
                detectedTimesText.text = "Detected Times: --";
                heardTimesText.text = "Heard Times: --";
            }
        }

        public void OnBackToMenu()
        {
            AsyncSceneLoader.Instance.LoadSceneAsyncWithProgress(
                sceneName: "Menu",
                onProgress: LoadingUI.Instance.OnSceneLoadProgress
            );
        }

        public void OnRestart()
        {
            AsyncSceneLoader.Instance.LoadSceneAsyncWithProgress(
                sceneName: SceneManager.GetActiveScene().name,
                onProgress: LoadingUI.Instance.OnSceneLoadProgress
            );
        }
    }
}