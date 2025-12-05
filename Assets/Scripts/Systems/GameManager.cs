using UnityEngine;
using UnityEngine.SceneManagement;
using Ninja.Core;
using Ninja.Core.Events;

namespace Ninja.Systems
{
    /// <summary>
    /// Главный менеджер игры - управляет паузой и временем уровня
    /// </summary>
    public class GameManager : PersistentSingleton<GameManager>
    {
        [SerializeField] private bool pauseOnStart = false;

        private bool isPaused;
        private float savedTimeScale = 1f;
        private float levelStartTime;

        public bool IsPaused => isPaused;
        public float LevelTime => Time.time - levelStartTime;
        public string CurrentScene => SceneManager.GetActiveScene().name;

        protected override void OnSingletonInitialized()
        {
            base.OnSingletonInitialized();
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitLevel();
            
            if (pauseOnStart) Pause();
            else Resume();
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            base.OnDestroy();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InitLevel();

        private void InitLevel()
        {
            if (CurrentScene == "Menu" || string.IsNullOrEmpty(CurrentScene))
                return;

            levelStartTime = Time.time;
            Events.Trigger(GameEvents.LevelStarted, new LevelEventArgs(CurrentScene));
        }

        public void Pause()
        {
            if (isPaused) return;

            savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPaused = true;
            Events.Trigger(GameEvents.GamePaused);
        }

        public void Resume()
        {
            if (!isPaused) return;

            Time.timeScale = savedTimeScale;
            isPaused = false;
            Events.Trigger(GameEvents.GameResumed);
        }

        public void TogglePause()
        {
            if (isPaused) Resume();
            else Pause();
        }

        public void SetTimeScale(float scale)
        {
            scale = Mathf.Clamp01(scale);
            if (isPaused) savedTimeScale = scale;
            else Time.timeScale = scale;
        }
    }
}
