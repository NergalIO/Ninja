using System;

using UnityEngine;

using Ninja.Core;


namespace Ninja.Systems {
    public class GameManager : PersistentSingleton<GameManager> {
        [Header("Pause Settings")]
        [SerializeField] private bool pauseOnStart = false;

        private bool isPaused = false;
        private float previousTimeScale = 1f;

        public bool IsPaused => isPaused;

        public event Action OnGamePaused;
        public event Action OnGameResumed;

        protected override void OnSingletonInitialized() {
            base.OnSingletonInitialized();
            
            if (pauseOnStart) {
                PauseGame();
            } else {
                ResumeGame();
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
    }
}


