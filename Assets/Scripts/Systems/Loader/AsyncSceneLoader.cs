using System.Collections;

using UnityEngine.SceneManagement;
using UnityEngine;

using Ninja.Core;
using System;
using Ninja.UI.Loading;



namespace Ninja.Systems.Loader
{
    public class AsyncSceneLoader : PersistentSingleton<AsyncSceneLoader>
    {
        [Header("References")]

        [Header("Current State")]
        [SerializeField] private LoaderStates currentState = LoaderStates.Idle;

        [Header("Loading Settings")]
        [SerializeField] private float loadingDelay = 0.1f;


        public Action OnSceneLoadStarted = delegate { };
        public Action OnSceneLoadCompleted = delegate { };


        public void LoadSceneAsync(string sceneName)
        {
            StartCoroutine(LoadSceneAsyncRoutine(sceneName));
        }

        public void LoadSceneAsyncWithProgress(string sceneName, Action<float> onProgress = null)
        {
            StartCoroutine(LoadSceneAsyncWithProgressRoutine(sceneName, onProgress));
        }

        private IEnumerator LoadSceneAsyncRoutine(string sceneName)
        {
            yield return new WaitForSeconds(loadingDelay);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            currentState = LoaderStates.Loading;
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            currentState = LoaderStates.Idle;
        }

        private IEnumerator LoadSceneAsyncWithProgressRoutine(string sceneName, Action<float> onProgress)
        {
            Debug.Log("Loading scene " + sceneName);
            yield return new WaitForSecondsRealtime(loadingDelay);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            currentState = LoaderStates.Loading;
            OnSceneLoadStarted?.Invoke();
            
            while (!asyncLoad.isDone)
            {
                onProgress?.Invoke(asyncLoad.progress);
                yield return null;
            }
            
            onProgress?.Invoke(1f);
            OnSceneLoadCompleted?.Invoke();
            currentState = LoaderStates.Idle;
        }
    }
}