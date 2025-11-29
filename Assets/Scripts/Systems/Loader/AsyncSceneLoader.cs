using UnityEngine;


namespace Ninja.Systems.Loader
{
    public class AsyncSceneLoader : PersistentSingleton<AsyncSceneLoader>
    {
        [SerializeField] private float loadingDelay = 0.1f;

        public void LoadSceneAsync(string sceneName)
        {
            StartCoroutine(LoadSceneAsyncRoutine(sceneName));
        }

        public void LoadSceneAsyncWithProgress(string sceneName, System.Action<float> onProgress = null)
        {
            StartCoroutine(LoadSceneAsyncWithProgressRoutine(sceneName, onProgress));
        }

        private IEnumerator LoadSceneAsyncRoutine(string sceneName)
        {
            yield return new WaitForSeconds(loadingDelay);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private IEnumerator LoadSceneAsyncWithProgressRoutine(string sceneName, System.Action<float> onProgress)
        {
            yield return new WaitForSeconds(loadingDelay);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                onProgress?.Invoke(asyncLoad.progress);
                yield return null;
            }
            
            onProgress?.Invoke(1f);
        }
    }
}