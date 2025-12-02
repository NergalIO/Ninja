using UnityEngine;


namespace Ninja.Core
{

    public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance => instance;

        protected virtual void Awake()
        {
            if (instance != null && instance.Equals(null))
                instance = null;

            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this as T;
            DontDestroyOnLoad(gameObject);
            OnSingletonInitialized();
        }

        protected virtual void OnSingletonInitialized()
        {
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}

