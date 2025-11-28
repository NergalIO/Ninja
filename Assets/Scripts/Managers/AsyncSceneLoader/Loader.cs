using System.Collections;
using Ninja.UI;
using Ninja.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Ninja.Managers
{
    public class AsyncSceneLoader : PersistentSingleton<AsyncSceneLoader>
    {
        [SerializeField] private LoaderStates state = LoaderStates.Idle;
        [SerializeField] private float progress;

        public LoaderStates State => state;
        public float Progress => progress;

        public void LoadScene(string sceneName)
        {
            LoaderMenu loaderMenu = LoaderMenu.Instance;
            Debug.Log("Level<" + sceneName + ">: Loading level");
            StartCoroutine(LoadAsync(sceneName));
            loaderMenu.gameObject.SetActive(true);
        }

        public IEnumerator LoadAsync(string sceneName)
        {
            state = LoaderStates.Loading;
            AsyncOperation asyncLoader = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoader.isDone)
            {
                progress = Mathf.Clamp01(progress / .9f);
                yield return null;
            }

            state = LoaderStates.Idle;
        }
    }
}
