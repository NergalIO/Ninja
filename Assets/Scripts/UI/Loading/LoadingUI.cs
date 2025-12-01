using UnityEngine;

using Ninja.Systems.Loader;
using Ninja.Core;
using System.Collections;
using Ninja.Systems;


namespace Ninja.UI.Loading
{
    public class LoadingUI : PersistentSingleton<LoadingUI>
    {
        [Header("References")]
        [SerializeField] private GameObject loadingPanelPrefab;

        private LoadingPanel loadingPanelInstance;

        protected override void OnSingletonInitialized()
        {
            base.OnSingletonInitialized();
            if (AsyncSceneLoader.Instance != null)
            {
                AsyncSceneLoader.Instance.OnSceneLoadStarted += HandleSceneLoadStarted;
                AsyncSceneLoader.Instance.OnSceneLoadCompleted += HandleSceneLoadCompleted;
            }
            else
            {
                StartCoroutine(WaitForLoader());
            }
        }

        private IEnumerator WaitForLoader()
        {
            while (AsyncSceneLoader.Instance == null)
                yield return null;

            AsyncSceneLoader.Instance.OnSceneLoadStarted += HandleSceneLoadStarted;
            AsyncSceneLoader.Instance.OnSceneLoadCompleted += HandleSceneLoadCompleted;
        }

        protected override void OnDestroy()
        {
            if (AsyncSceneLoader.Instance != null)
            {
                AsyncSceneLoader.Instance.OnSceneLoadStarted -= HandleSceneLoadStarted;
                AsyncSceneLoader.Instance.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
            }
            base.OnDestroy();
        }

        public void OnSceneLoadProgress(float progress)
        {
            loadingPanelInstance.OnProgress(progress);
        }

        private void HandleSceneLoadStarted()
        {
            loadingPanelInstance = Instantiate(loadingPanelPrefab, transform).GetComponent<LoadingPanel>();
            loadingPanelInstance.Open();
        }

        private void HandleSceneLoadCompleted()
        {
            if (GameManager.Instance.IsPaused)
                GameManager.Instance.TogglePause();
            loadingPanelInstance.Close();
            Destroy(loadingPanelInstance.gameObject);
        }
    }
}