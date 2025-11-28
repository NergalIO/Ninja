using System;
using System.Collections.Generic;
using System.Linq;
using Ninja.Managers;
using Ninja.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Ninja.Settings {
    public class SettingsManager : PersistentSingleton<SettingsManager> {
        private const string SettingsPath = "Settings";

        [Header("Menu")]
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private Button closeButton;

        [Header("Settings Container")]
        [SerializeField] private Transform settingsContainer;

        [Header("Setting Card Prefab")]
        [SerializeField] private GameObject settingCardPrefab;

        private Dictionary<string, GameSetting> settings = new(StringComparer.OrdinalIgnoreCase);
        private List<GameSetting> settingsList = new();
        private bool isMenuOpen = true;
        private bool wasPausedBeforeMenu = false;

        public IReadOnlyDictionary<string, GameSetting> Settings => settings;
        public IReadOnlyList<GameSetting> SettingsList => settingsList;
        public bool IsMenuOpen => isMenuOpen;

        public event Action<GameSetting, object> OnSettingChanged;
        public event Action OnSettingsMenuOpened;
        public event Action OnSettingsMenuClosed;

        protected override void OnSingletonInitialized() {
            closeButton.onClick.AddListener(OnCloseButton);
            LoadSettings();
            HideSettings();
        }

        private void LoadSettings() {
            settings.Clear();
            settingsList.Clear();

            var loadedSettings = Resources.LoadAll<GameSetting>(SettingsPath);
            
            foreach (var setting in loadedSettings) {
                if (setting == null) {
                    continue;
                }

                var key = string.IsNullOrEmpty(setting.SettingName) ? setting.name : setting.SettingName;
                
                if (settings.ContainsKey(key)) {
                    Debug.LogWarning($"[SettingsManager] Duplicate setting name '{key}' found. Skipping '{setting.name}'.");
                    continue;
                }

                settings[key] = setting;
                settingsList.Add(setting);
            }

            Debug.Log($"[SettingsManager] Loaded {settings.Count} settings from Resources/{SettingsPath}");
        }

        public GameSetting GetSetting(string settingName) {
            if (settings.TryGetValue(settingName, out var setting)) {
                return setting;
            }

            Debug.LogWarning($"[SettingsManager] Setting '{settingName}' not found.");
            return null;
        }

        public T GetSettingValue<T>(string settingName) {
            var setting = GetSetting(settingName);
            if (setting == null) {
                return default(T);
            }

            return setting.GetValue<T>();
        }

        public object GetSettingValue(string settingName) {
            var setting = GetSetting(settingName);
            if (setting == null) {
                return null;
            }

            return setting.GetValue();
        }

        public bool SetSettingValue(string settingName, object value) {
            var setting = GetSetting(settingName);
            if (setting == null) {
                return false;
            }

            setting.SetValue(value);
            OnSettingChanged?.Invoke(setting, value);
            return true;
        }

        public void ResetSetting(string settingName) {
            var setting = GetSetting(settingName);
            if (setting == null) {
                return;
            }

            var defaultValue = setting.GetDefaultValue();
            SetSettingValue(settingName, defaultValue);
        }

        public void ResetAllSettings() {
            foreach (var setting in settingsList) {
                var defaultValue = setting.GetDefaultValue();
                setting.SetValue(defaultValue);
                OnSettingChanged?.Invoke(setting, defaultValue);
            }
        }

        public void ReloadSettings() {
            LoadSettings();
        }

        public void ShowSettings() {
            if (isMenuOpen) {
                return;
            }

            isMenuOpen = true;

            // Сохраняем состояние паузы
            if (GameManager.Instance != null) {
                wasPausedBeforeMenu = GameManager.Instance.IsPaused;
                if (!wasPausedBeforeMenu) {
                    GameManager.Instance.PauseGame();
                }
            }

            // Показываем меню
            settingsMenu.SetActive(true);

            // Заполняем поля настроек
            PopulateSettingsUI();

            OnSettingsMenuOpened?.Invoke();
        }

        public void HideSettings() {
            if (!isMenuOpen) {
                return;
            }

            isMenuOpen = false;

            // Скрываем меню
            settingsMenu.SetActive(false);

            // Восстанавливаем состояние паузы
            if (GameManager.Instance != null && !wasPausedBeforeMenu) {
                GameManager.Instance.ResumeGame();
            }

            OnSettingsMenuClosed?.Invoke();
        }

        public void ToggleSettings() {
            if (isMenuOpen) {
                HideSettings();
            } else {
                ShowSettings();
            }
        }

        private void PopulateSettingsUI() {
            if (settingsContainer == null || settingCardPrefab == null) {
                Debug.LogWarning("[SettingsManager] Settings container or card prefab is not assigned. Cannot populate settings UI.");
                return;
            }

            // Очищаем существующие карточки
            foreach (Transform child in settingsContainer) {
                if (Application.isPlaying) {
                    Destroy(child.gameObject);
                } else {
                    DestroyImmediate(child.gameObject);
                }
            }

            // Создаем карточки для каждой настройки
            foreach (var setting in settingsList) {
                if (setting == null) {
                    continue;
                }

                var cardObject = Instantiate(settingCardPrefab, settingsContainer);
                var settingCard = cardObject.GetComponent<SettingCard>();
                
                if (settingCard == null) {
                    Debug.LogWarning($"[SettingsManager] SettingCard component not found on prefab for setting '{setting.SettingName}'. Adding component...");
                    settingCard = cardObject.AddComponent<SettingCard>();
                }

                settingCard.Initialize(setting);
            }
        }

        private void OnCloseButton()
        {
            HideSettings();
        }
    }
}

