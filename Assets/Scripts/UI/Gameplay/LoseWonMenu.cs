using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Ninja.Core;
using Ninja.Systems;
using Ninja.Systems.Loader;
using Ninja.UI.Loading;

namespace Ninja.UI.Gameplay
{
    public class LoseWonMenu : MenuBase
    {
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text aliveTimeText;
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private Button restartButton;
        
        public bool isWon;

        protected override void Awake()
        {
            base.Awake();
            backToMenuButton?.onClick.AddListener(BackToMenu);
            restartButton?.onClick.AddListener(Restart);
        }

        public void UpdateStatistics()
        {
            resultText.text = isWon ? "You Won!" : "You Lose!";
            
            if (GameManager.Instance != null)
                aliveTimeText.text = $"Survived Time: {TimeUtils.FormatTime(GameManager.Instance.LevelTime)}";
            else
                aliveTimeText.text = "Survived Time: --";
        }

        private void BackToMenu() =>
            AsyncSceneLoader.Instance?.LoadSceneAsyncWithProgress("Menu", LoadingUI.Instance.OnSceneLoadProgress);

        private void Restart() =>
            AsyncSceneLoader.Instance?.LoadSceneAsyncWithProgress(
                SceneManager.GetActiveScene().name, 
                LoadingUI.Instance.OnSceneLoadProgress);
    }
}
