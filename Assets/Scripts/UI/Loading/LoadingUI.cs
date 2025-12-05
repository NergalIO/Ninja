using System.Collections;
using UnityEngine;
using Ninja.Core;
using Ninja.Systems;
using Ninja.Systems.Loader;

namespace Ninja.UI.Loading
{
    public class LoadingUI : PersistentSingleton<LoadingUI>
    {
        [SerializeField] private GameObject loadingPanelPrefab;
        private LoadingPanel panel;

        protected override void OnSingletonInitialized()
        {
            base.OnSingletonInitialized();
            StartCoroutine(SubscribeToLoader());
        }

        private IEnumerator SubscribeToLoader()
        {
            while (AsyncSceneLoader.Instance == null)
                yield return null;

            AsyncSceneLoader.Instance.OnSceneLoadStarted += OnLoadStarted;
            AsyncSceneLoader.Instance.OnSceneLoadCompleted += OnLoadCompleted;
        }

        protected override void OnDestroy()
        {
            if (AsyncSceneLoader.Instance != null)
            {
                AsyncSceneLoader.Instance.OnSceneLoadStarted -= OnLoadStarted;
                AsyncSceneLoader.Instance.OnSceneLoadCompleted -= OnLoadCompleted;
            }
            base.OnDestroy();
        }

        public void OnSceneLoadProgress(float progress) => panel?.OnProgress(progress);

        private void OnLoadStarted()
        {
            panel = Instantiate(loadingPanelPrefab, transform).GetComponent<LoadingPanel>();
            panel.Open();
        }

        private void OnLoadCompleted()
        {
            if (GameManager.Instance && GameManager.Instance.IsPaused)
                GameManager.Instance.TogglePause();

            panel?.Close();
            if (panel != null) Destroy(panel.gameObject);
        }
    }
}
