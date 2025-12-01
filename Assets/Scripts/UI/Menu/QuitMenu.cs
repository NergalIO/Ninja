using Ninja.Systems.Loader;
using Ninja.UI.Loading;
using UnityEngine;
using UnityEngine.UI;

namespace Ninja.UI.Menu
{
    public class QuitMenu : MenuBase
    {
        [Header("Components")]
        [SerializeField] private Button confirmQuitButton;
        [SerializeField] private Button cancelQuitButton;

        [Header("Variables")]
        [SerializeField] private bool isApplicationQuit = false;

        private void Start()
        {
            confirmQuitButton.onClick.AddListener(OnConfirmQuit);
            cancelQuitButton.onClick.AddListener(OnCancelQuit);
        }

        private void OnConfirmQuit()
        {
            if (!IsFocused)
                return;

            if (isApplicationQuit)
            {
                Debug.Log("Quitting the game...");
                Application.Quit();
            }
            else
            {
                AsyncSceneLoader.Instance.LoadSceneAsyncWithProgress(
                    "Menu",
                    LoadingUI.Instance.OnSceneLoadProgress
                );
            }
        }

        private void OnCancelQuit()
        {
            if (!IsFocused)
                return;

            Close();
        }

        public override void OnEscPressed()
        {
            if (!IsFocused)
                return;

            Close();
        }
    }
}
