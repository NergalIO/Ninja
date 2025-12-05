using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ninja.Core;

namespace Ninja.Systems.Loader
{
    public class AsyncSceneLoader : PersistentSingleton<AsyncSceneLoader>
    {
        [SerializeField] private float loadingDelay = 0.1f;

        public event Action OnSceneLoadStarted;
        public event Action OnSceneLoadCompleted;

        public void LoadScene(string sceneName) =>
            StartCoroutine(LoadSceneRoutine(sceneName, null));

        public void LoadSceneAsyncWithProgress(string sceneName, Action<float> onProgress = null) =>
            StartCoroutine(LoadSceneRoutine(sceneName, onProgress));

        private IEnumerator LoadSceneRoutine(string sceneName, Action<float> onProgress)
        {
            yield return new WaitForSecondsRealtime(loadingDelay);

            OnSceneLoadStarted?.Invoke();

            var op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
            {
                onProgress?.Invoke(op.progress);
                yield return null;
            }

            onProgress?.Invoke(1f);
            OnSceneLoadCompleted?.Invoke();
        }
    }
}
