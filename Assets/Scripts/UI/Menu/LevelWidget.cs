using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Ninja.Gameplay.Levels;
using Ninja.Systems.Loader;
using Ninja.UI.Loading;


namespace Ninja.UI.Menu
{
    public class LevelWidget : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TMP_Text levelName;
        [SerializeField] private TMP_Text levelDescription;
        [SerializeField] private Button playButton;

        public void SetLevel(Level level)
        {
            levelName.text = level.LevelName;
            levelDescription.text = level.Description;

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => {
                Debug.Log($"Starting level: {level.LevelName}");
                AsyncSceneLoader.Instance.LoadSceneAsyncWithProgress(level.Id, LoadingUI.Instance.OnSceneLoadProgress);
            });
        }
    }
}