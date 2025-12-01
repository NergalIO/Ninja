using System;
using System.Collections.Generic;
using UnityEngine;
using Ninja.Core;
using Data = Ninja.Systems.Data;
using LevelData = Ninja.Systems.Data.LevelData;
using Ninja.Gameplay.Levels;
using UnityEngine.SceneManagement;


namespace Ninja.Systems {
    public class GameManager : PersistentSingleton<GameManager> {
        [Header("Pause Settings")]
        [SerializeField] private bool pauseOnStart = false;

        private bool isPaused = false;
        private float previousTimeScale = 1f;
        private Data.LevelDataCollector dataCollector = new Data.LevelDataCollector();
        private float levelStartTime = 0f;

        public bool IsPaused => isPaused;
        public Data.LevelDataCollector DataCollector => dataCollector;
        public float LevelStartTime => levelStartTime;
        public float CurrentLevelTime => Time.time - levelStartTime;

        public string CurrentScene => SceneManager.GetActiveScene().name;

        // Game state events
        public event Action OnGamePaused;
        public event Action OnGameResumed;

        // Gameplay events
        public event Action OnPlayerEscapeTrigger;
        public event Action OnPlayerCatched;
        public event Action OnPlayerFound;
        public event Action<Vector3> OnPlayerHeard;

        protected override void OnSingletonInitialized() {
            base.OnSingletonInitialized();
            
            // Загружаем сохраненные данные
            dataCollector.LoadData();
            
            // Подключаем события к сбору данных
            OnPlayerCatched += HandlePlayerCatched;
            OnPlayerFound += HandlePlayerFound;
            OnPlayerEscapeTrigger += HandlePlayerEscape;
            
            // Подписываемся на загрузку сцен
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Инициализируем текущий уровень, если уже в игровой сцене
            InitializeCurrentLevel();
            
            if (pauseOnStart) {
                PauseGame();
            } else {
                ResumeGame();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InitializeCurrentLevel();
        }

        private void InitializeCurrentLevel()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            // Если это не меню, устанавливаем текущий уровень
            if (sceneName != "Menu" && !string.IsNullOrEmpty(sceneName))
            {
                SetCurrentLevel(sceneName);
            }
        }

        protected override void OnDestroy()
        {
            // Отписываемся от событий
            SceneManager.sceneLoaded -= OnSceneLoaded;
            
            // Сохраняем данные перед выходом только если уровень не завершен
            if (!string.IsNullOrEmpty(CurrentScene) && CurrentScene != "Menu")
            {
                dataCollector.SaveData(CurrentScene);
            }
            
            base.OnDestroy();
            // Отключаем события при уничтожении
            OnPlayerCatched -= HandlePlayerCatched;
            OnPlayerFound -= HandlePlayerFound;
            OnPlayerEscapeTrigger -= HandlePlayerEscape;
        }

        private void OnApplicationQuit()
        {
            // Сохраняем данные при выходе из приложения только если уровень не завершен
            if (!string.IsNullOrEmpty(CurrentScene) && CurrentScene != "Menu")
            {
                dataCollector.SaveData(CurrentScene);
            }
        }

        public void PauseGame() {
            if (isPaused) {
                return;
            }

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPaused = true;
            OnGamePaused?.Invoke();
        }

        public void ResumeGame() {
            if (!isPaused) {
                return;
            }

            Time.timeScale = previousTimeScale;
            isPaused = false;
            OnGameResumed?.Invoke();
        }

        public void TogglePause() {
            if (isPaused) {
                ResumeGame();
            } else {
                PauseGame();
            }
        }

        public void SetTimeScale(float timeScale) {
            if (isPaused) {
                previousTimeScale = Mathf.Clamp(timeScale, 0f, 1f);
            } else {
                Time.timeScale = Mathf.Clamp(timeScale, 0f, 1f);
            }
        }

        #region Level Data Collection
        public void SetCurrentLevel(string levelId)
        {
            dataCollector.SetCurrentLevel(levelId);
            levelStartTime = Time.time; // Запоминаем время начала уровня
        }

        public void SetPlayerTransform(Transform player)
        {
            dataCollector.SetPlayerTransform(player);
        }

        public void NotifyPlayerHeard(Vector3 noisePosition)
        {
            dataCollector.RecordPlayerHeard(noisePosition, CurrentScene);
            OnPlayerHeard?.Invoke(noisePosition);
        }

        public void NotifyPlayerFound()
        {
            OnPlayerFound?.Invoke();
        }

        public void NotifyPlayerCatched()
        {
            OnPlayerCatched?.Invoke();
        }

        public void NotifyPlayerEscape()
        {
            OnPlayerEscapeTrigger?.Invoke();
        }

        private void HandlePlayerCatched()
        {
            dataCollector.RecordPlayerCaught();
            // Сохраняем данные только в конце уровня
            dataCollector.SaveData(CurrentScene);
        }

        private void HandlePlayerFound()
        {
            dataCollector.RecordPlayerDetected();
            // Не сохраняем при обнаружении, только в конце уровня
        }

        private void HandlePlayerEscape()
        {
            dataCollector.RecordPlayerEscape(CurrentScene);
            // Сохраняем данные только в конце уровня
            dataCollector.SaveData(CurrentScene);
        }

        public void SaveLevelData()
        {
            dataCollector.SaveData(CurrentScene);
        }

        public void LoadLevelData()
        {
            dataCollector.LoadData();
        }

        // Удобные методы для доступа к данным из любого скрипта
        public static LevelData GetLevelData(string levelId)
        {
            if (Instance != null)
            {
                return Instance.dataCollector.GetLevelData(levelId);
            }
            return null;
        }

        public static LevelData GetCurrentLevelData()
        {
            if (Instance != null)
            {
                return Instance.dataCollector.GetCurrentLevelData();
            }
            return null;
        }

        public static IReadOnlyDictionary<string, LevelData> GetAllLevelData()
        {
            if (Instance != null)
            {
                return Instance.dataCollector.GetAllLevelData();
            }
            return new Dictionary<string, LevelData>();
        }

        public static int GetLevelTimesPlayed(string levelId)
        {
            var data = GetLevelData(levelId);
            return data?.TimesPlayed ?? 0;
        }

        public static int GetLevelTimesCaught(string levelId)
        {
            var data = GetLevelData(levelId);
            return data?.TimesCaught ?? 0;
        }

        public static int GetLevelTimesDetected(string levelId)
        {
            var data = GetLevelData(levelId);
            return data?.TimesDetected ?? 0;
        }

        public static int GetLevelTimesHeard(string levelId)
        {
            var data = GetLevelData(levelId);
            return data?.TimesHeard ?? 0;
        }

        public static int GetLevelTimesCompleted(string levelId)
        {
            var data = GetLevelData(levelId);
            return data?.TimesCompleted ?? 0;
        }
        #endregion
    }
}


