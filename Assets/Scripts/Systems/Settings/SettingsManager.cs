using System;
using System.Collections.Generic;
using UnityEngine;
using Ninja.Core;

namespace Ninja.Systems.Settings
{
    public class SettingsManager : PersistentSingleton<SettingsManager>
    {
        [SerializeField] private bool debugMode = true;
        
        private readonly Dictionary<string, SettingsObject> settingsObjects = new Dictionary<string, SettingsObject>();
        private readonly Dictionary<string, List<Action<object>>> actionCallbacks = new Dictionary<string, List<Action<object>>>();

        protected override void OnSingletonInitialized()
        {
            LogDebug("Initializing SettingsManager...");
            LoadSettingsObjects();
        }

        #region Private Methods

        private void LoadSettingsObjects()
        {
            var loadedSettings = Resources.LoadAll<SettingsObject>("Settings");
            LogDebug($"Loading {loadedSettings.Length} SettingsObjects from Resources/Settings");

            foreach (var settingsObject in loadedSettings)
            {
                if (settingsObject == null)
                {
                    LogError("Attempted to load a null SettingsObject.");
                    continue;
                }

                settingsObject.OnValueChanged += OnSettingValueChanged;
                settingsObject.LoadFromPlayerPrefs();
                settingsObjects[settingsObject.SettingKey] = settingsObject;

                LogDebug($"Loaded setting: '{settingsObject.SettingKey}' = '{settingsObject.GetValue()}' (Type: {settingsObject.Type})");
            }

            LogDebug($"Successfully loaded {settingsObjects.Count} settings");
        }

        private SettingsObject GetSettingsObject(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                LogError("Attempted to get SettingsObject with null or empty key.");
                return null;
            }

            if (settingsObjects.TryGetValue(key, out var settingsObject))
            {
                return settingsObject;
            }

            LogWarning($"No SettingsObject found with key '{key}'.");
            return null;
        }

        private void OnSettingValueChanged(SettingsObject settingsObject, object newValue)
        {
            if (settingsObject == null)
            {
                LogError("Attempted to change value of a null SettingsObject.");
                return;
            }

            LogDebug($"Setting changed: '{settingsObject.SettingKey}' = '{newValue}' (Type: {settingsObject.Type})");

            if (actionCallbacks.TryGetValue(settingsObject.SettingKey, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    try
                    {
                        callback?.Invoke(newValue);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error invoking callback for '{settingsObject.SettingKey}': {ex.Message}");
                    }
                }
            }
        }

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[SettingsManager] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SettingsManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SettingsManager] {message}");
        }

        #endregion

        #region Public Methods

        public void RegisterAction(string key, Action action)
        {
            if (!ValidateKey(key) || !ValidateAction(action, key))
                return;

            var settingsObject = GetSettingsObject(key);
            if (settingsObject == null)
            {
                LogWarning($"Cannot register action for non-existent key '{key}'.");
                return;
            }

            EnsureCallbackList(key);
            actionCallbacks[key].Add(value => action());
            LogDebug($"Registered action callback for '{key}'");
        }

        public void RegisterAction<T>(string key, Action<T> action)
        {
            if (!ValidateKey(key) || !ValidateAction(action, key))
                return;

            var settingsObject = GetSettingsObject(key);
            if (settingsObject == null)
            {
                LogWarning($"Cannot register action for non-existent key '{key}'.");
                return;
            }

            EnsureCallbackList(key);
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
                        T convertedValue = ConvertValue<T>(value);
                        action(convertedValue);
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error converting value for callback '{key}': {ex.Message}");
                }
            });

            LogDebug($"Registered typed action callback for '{key}' with type {typeof(T).Name}");
        }

        public T GetSettingValue<T>(string key)
        {
            if (!ValidateKey(key))
                return default;

            var settingsObject = GetSettingsObject(key);
            if (settingsObject == null)
            {
                LogWarning($"Failed to retrieve setting '{key}' - returning default value");
                return default;
            }

            var value = settingsObject.GetValue<T>();
            LogDebug($"Retrieved setting: '{key}' of type {typeof(T).Name} = {value}");
            return value;
        }

        public void SetSettingValue<T>(string key, T value)
        {
            if (!ValidateKey(key))
                return;

            var settingsObject = GetSettingsObject(key);
            if (settingsObject == null)
            {
                LogError($"Cannot set value for non-existent key '{key}'.");
                return;
            }

            LogDebug($"Setting value: '{key}' of type {typeof(T).Name} = {value}");
            settingsObject.SetValue(value);
        }

        public object GetSettingValue(string key)
        {
            if (!ValidateKey(key))
                return null;

            var settingsObject = GetSettingsObject(key);
            if (settingsObject == null)
                return null;

            var value = settingsObject.GetValue();
            LogDebug($"Retrieved setting: '{key}' = {value}");
            return value;
        }

        public void ResetToDefault(string key)
        {
            if (!ValidateKey(key))
                return;

            var settingsObject = GetSettingsObject(key);
            if (settingsObject == null)
            {
                LogError($"Cannot reset non-existent key '{key}'.");
                return;
            }

            LogDebug($"Resetting setting '{key}' to default value");
            settingsObject.ResetToDefault();
        }

        public bool HasSetting(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && settingsObjects.ContainsKey(key);
        }

        public List<SettingsObject> GetAllSettings()
        {
            return new List<SettingsObject>(settingsObjects.Values);
        }

        public void UnregisterAllActions(string key)
        {
            if (actionCallbacks.ContainsKey(key))
            {
                actionCallbacks.Remove(key);
                LogDebug($"Unregistered all actions for key '{key}'");
            }
        }

        #endregion

        #region Validation

        private bool ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                LogError("Attempted to use null or empty key.");
                return false;
            }
            return true;
        }

        private bool ValidateAction<T>(T action, string key) where T : class
        {
            if (action == null)
            {
                LogError($"Attempted to register null action for key '{key}'.");
                return false;
            }
            return true;
        }

        private void EnsureCallbackList(string key)
        {
            if (!actionCallbacks.ContainsKey(key))
            {
                actionCallbacks[key] = new List<Action<object>>();
            }
        }

        private T ConvertValue<T>(object value)
        {
            if (value == null)
                return default;

            Type targetType = typeof(T);
            Type valueType = value.GetType();

            if (targetType.IsAssignableFrom(valueType))
            {
                return (T)value;
            }

            if (targetType == typeof(float) && valueType == typeof(double))
            {
                return (T)(object)(float)(double)value;
            }

            if (targetType == typeof(float) && valueType == typeof(int))
            {
                return (T)(object)(float)(int)value;
            }

            if (targetType == typeof(int) && valueType == typeof(float))
            {
                return (T)(object)(int)(float)value;
            }

            if (value is IConvertible)
            {
                return (T)Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
            }

            return (T)value;
        }

        #endregion
    }
}