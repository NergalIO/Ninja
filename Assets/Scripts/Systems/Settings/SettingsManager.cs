using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Ninja.Core;
using System;


namespace Ninja.Systems.Settings
{
    public class SettingsManager : PersistentSingleton<SettingsManager>
    {
        [SerializeField] private bool debugMode = true;
        
        private Dictionary<string, SettingsObject> settingsObjects = new Dictionary<string, SettingsObject>();
        private Dictionary<string, List<System.Action<object>>> actionCallbacks = new Dictionary<string, List<System.Action<object>>>();

        protected override void OnSingletonInitialized()
        {
            if (debugMode)
            {
                Debug.Log("[SettingsManager] Initializing SettingsManager...");
            }
            LoadSettingsObjects();
        }

        #region Private Methods

        private void LoadSettingsObjects()
        {
            var loadedSettings = Resources.LoadAll<SettingsObject>("Settings");
            
            if (debugMode)
            {
                Debug.Log($"[SettingsManager] Loading {loadedSettings.Length} SettingsObjects from Resources/Settings");
            }

            foreach (var settingsObject in loadedSettings)
            {
                if (settingsObject == null)
                {
                    Debug.LogError("[SettingsManager] Attempted to load a null SettingsObject.");
                    continue;
                }

                settingsObject.OnValueChanged += OnSettingValueChanged;
                settingsObject.LoadFromPlayerPrefs();
                settingsObjects[settingsObject.SettingKey] = settingsObject;

                if (debugMode)
                {
                    Debug.Log($"[SettingsManager] Loaded setting: '{settingsObject.SettingKey}' = '{settingsObject.GetValue()}' (Type: {settingsObject.Type})");
                }
            }

            if (debugMode)
            {
                Debug.Log($"[SettingsManager] Successfully loaded {settingsObjects.Count} settings");
            }
        }

        private SettingsObject GetSettingsObject(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("[SettingsManager] Attempted to get SettingsObject with null or empty key.");
                return null;
            }

            if (settingsObjects.TryGetValue(key, out var settingsObject))
            {
                return settingsObject;
            }

            Debug.LogWarning($"[SettingsManager] No SettingsObject found with key '{key}'.");
            return null;
        }

        private void OnSettingValueChanged(SettingsObject settingsObject, object newValue)
        {
            if (settingsObject == null)
            {
                Debug.LogError("[SettingsManager] Attempted to change value of a null SettingsObject.");
                return;
            }

            if (debugMode)
            {
                Debug.Log($"[SettingsManager] Setting changed: '{settingsObject.SettingKey}' = '{newValue}' (Type: {settingsObject.Type})");
            }

            // Вызываем зарегистрированные callbacks
            if (actionCallbacks.TryGetValue(settingsObject.SettingKey, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    try
                    {
                        callback?.Invoke(newValue);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[SettingsManager] Error invoking callback for '{settingsObject.SettingKey}': {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void RegisterAction(string key, System.Action action)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("[SettingsManager] Attempted to register action with null or empty key.");
                return;
            }

            if (action == null)
            {
                Debug.LogError($"[SettingsManager] Attempted to register null action for key '{key}'.");
                return;
            }

            var settingsObject = GetSettingsObject(key);
            if (settingsObject != null)
            {
                if (!actionCallbacks.ContainsKey(key))
                {
                    actionCallbacks[key] = new List<System.Action<object>>();
                }

                actionCallbacks[key].Add(value => action());

                if (debugMode)
                {
                    Debug.Log($"[SettingsManager] Registered action callback for '{key}'");
                }
            }
            else
            {
                Debug.LogWarning($"[SettingsManager] Cannot register action for non-existent key '{key}'.");
            }
        }

        public void RegisterAction<T>(string key, System.Action<T> action)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("[SettingsManager] Attempted to register action with null or empty key.");
                return;
            }

            if (action == null)
            {
                Debug.LogError($"[SettingsManager] Attempted to register null action for key '{key}'.");
                return;
            }

            var settingsObject = GetSettingsObject(key);
            if (settingsObject != null)
            {
                if (!actionCallbacks.ContainsKey(key))
                {
                    actionCallbacks[key] = new List<System.Action<object>>();
                }

                actionCallbacks[key].Add(value =>
                {
                    try
                    {
                        if (value is T typedValue)
                        {
                            action(typedValue);
                        }
                        else
                        {
                            action((T)System.Convert.ChangeType(value, typeof(T)));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[SettingsManager] Error converting value for callback '{key}': {ex.Message}");
                    }
                });

                if (debugMode)
                {
                    Debug.Log($"[SettingsManager] Registered typed action callback for '{key}' with type {typeof(T).Name}");
                }
            }
            else
            {
                Debug.LogWarning($"[SettingsManager] Cannot register action for non-existent key '{key}'.");
            }
        }

        public T GetSettingValue<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("[SettingsManager] Attempted to get setting with null or empty key.");
                return default;
            }

            var settingsObject = GetSettingsObject(key);
            if (settingsObject != null)
            {
                var value = settingsObject.GetValue<T>();

                if (debugMode)
                {
                    Debug.Log($"[SettingsManager] Retrieved setting: '{key}' of type {typeof(T).Name} = {value}");
                }

                return value;
            }

            if (debugMode)
            {
                Debug.LogWarning($"[SettingsManager] Failed to retrieve setting '{key}' - returning default value");
            }

            return default;
        }

        public void SetSettingValue<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("[SettingsManager] Attempted to set setting with null or empty key.");
                return;
            }

            var settingsObject = GetSettingsObject(key);
            if (settingsObject != null)
            {
                if (debugMode)
                {
                    Debug.Log($"[SettingsManager] Setting value: '{key}' of type {typeof(T).Name} = {value}");
                }

                settingsObject.SetValue(value);
            }
            else
            {
                Debug.LogError($"[SettingsManager] Cannot set value for non-existent key '{key}'.");
            }
        }

        public object GetSettingValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("[SettingsManager] Attempted to get setting with null or empty key.");
                return null;
            }

            var settingsObject = GetSettingsObject(key);
            if (settingsObject != null)
            {
                var value = settingsObject.GetValue();

                if (debugMode)
                {
                    Debug.Log($"[SettingsManager] Retrieved setting: '{key}' = {value}");
                }

                return value;
            }

            return null;
        }

        public void ResetToDefault(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("[SettingsManager] Attempted to reset setting with null or empty key.");
                return;
            }

            var settingsObject = GetSettingsObject(key);
            if (settingsObject != null)
            {
                if (debugMode)
                {
                    Debug.Log($"[SettingsManager] Resetting setting '{key}' to default value");
                }

                settingsObject.ResetToDefault();
            }
            else
            {
                Debug.LogError($"[SettingsManager] Cannot reset non-existent key '{key}'.");
            }
        }

        public bool HasSetting(string key)
        {
            return settingsObjects.ContainsKey(key);
        }

        internal void GetAllSettings(out List<SettingsObject> settingsObjects)
        {
            settingsObjects = new List<SettingsObject>();
            foreach (SettingsObject settingsObject in this.settingsObjects.Values)
            {
                settingsObjects.Add(settingsObject);
            }
        }

        #endregion
    }
}