using System;
using UnityEngine;


namespace Ninja.Systems.Settings
{
    [CreateAssetMenu(fileName = "SettingsObject", menuName = "Ninja/Settings/Settings Object")]
    public class SettingsObject : ScriptableObject
    {
        public enum ValueType
        {
            String,
            Int,
            Float,
            Bool,
            Color
        }

        [Header("Identification")]
        [SerializeField] private string settingKey;
        [SerializeField] private string settingName;
        [SerializeField] private string group;

        [Header("Value Settings")]
        [SerializeField] private ValueType valueType = ValueType.String;
        [SerializeField] private string defaultValue;
        [SerializeField] private string currentValue;
        [SerializeField] private float maxValue;
        [SerializeField] private float minValue;

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;

        public string SettingKey => settingKey;
        public string SettingName => settingName;
        public string Group => group;
        public ValueType Type => valueType;
        public float MaxValue => maxValue;
        public float MinValue => minValue;
        public event Action<SettingsObject, object> OnValueChanged;

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(currentValue))
            {
                currentValue = defaultValue;

                if (debugMode)
                {
                    Debug.Log($"[SettingsObject] '{settingKey}' initialized with default value: '{defaultValue}' (Type: {valueType})");
                }
            }
        }

        #region Loading & Saving

        public void LoadFromPlayerPrefs()
        {
            if (string.IsNullOrWhiteSpace(settingKey))
            {
                Debug.LogError("[SettingsObject] Cannot load from PlayerPrefs: SettingKey is null or empty.");
                return;
            }

            if (PlayerPrefs.HasKey(settingKey))
            {
                var previousValue = currentValue;
                currentValue = PlayerPrefs.GetString(settingKey);

                if (debugMode)
                {
                    Debug.Log($"[SettingsObject] '{settingKey}' loaded from PlayerPrefs: '{previousValue}' -> '{currentValue}' (Type: {valueType})");
                }
            }
            else
            {
                currentValue = defaultValue;

                if (debugMode)
                {
                    Debug.Log($"[SettingsObject] '{settingKey}' not found in PlayerPrefs, using default: '{defaultValue}' (Type: {valueType})");
                }
            }
        }

        public void SaveToPlayerPrefs()
        {
            if (string.IsNullOrWhiteSpace(settingKey))
            {
                Debug.LogError("[SettingsObject] Cannot save to PlayerPrefs: SettingKey is null or empty.");
                return;
            }

            PlayerPrefs.SetString(settingKey, currentValue);
            PlayerPrefs.Save();

            if (debugMode)
            {
                Debug.Log($"[SettingsObject] '{settingKey}' saved to PlayerPrefs: '{currentValue}' (Type: {valueType})");
            }
        }

        #endregion

        #region Getting Values

        public object GetValue()
        {
            try
            {
                var result = ConvertStringToValue(currentValue);

                if (debugMode)
                {
                    Debug.Log($"[SettingsObject] '{settingKey}' retrieved value: {result} (Type: {valueType})");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SettingsObject] Cannot convert '{currentValue}' to type {valueType} for key '{settingKey}'. Error: {ex.Message}");
                return GetDefaultValue();
            }
        }

        public T GetValue<T>()
        {
            try
            {
                var value = ConvertStringToValue(currentValue);

                if (value is T typedValue)
                {
                    if (debugMode)
                    {
                        Debug.Log($"[SettingsObject] '{settingKey}' converted to {typeof(T).Name}: {typedValue}");
                    }

                    return typedValue;
                }

                // Попытка конвертировать если типы не совпадают
                var result = (T)Convert.ChangeType(value, typeof(T));

                if (debugMode)
                {
                    Debug.Log($"[SettingsObject] '{settingKey}' converted to {typeof(T).Name}: {result}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SettingsObject] Cannot convert '{currentValue}' to type {typeof(T).Name} for key '{settingKey}'. Error: {ex.Message}");
                return default;
            }
        }

        public string GetStringValue()
        {
            if (debugMode)
            {
                Debug.Log($"[SettingsObject] Retrieved string value for '{settingKey}': '{currentValue}'");
            }

            return currentValue;
        }

        #endregion

        #region Setting Values

        public void SetValue(object value)
        {
            if (value == null)
            {
                Debug.LogWarning($"[SettingsObject] Attempted to set null value for key '{settingKey}'.");
                return;
            }

            string newValue = ConvertValueToString(value);

            if (currentValue == newValue)
            {
                if (debugMode)
                {
                    Debug.Log($"[SettingsObject] '{settingKey}' value unchanged: '{newValue}' (Type: {valueType})");
                }

                return;
            }

            var previousValue = currentValue;
            currentValue = newValue;

            if (debugMode)
            {
                Debug.Log($"[SettingsObject] '{settingKey}' value changed: '{previousValue}' -> '{newValue}' (Type: {valueType})");
            }

            SaveToPlayerPrefs();
            OnValueChanged?.Invoke(this, GetValue());

            if (debugMode)
            {
                Debug.Log($"[SettingsObject] '{settingKey}' OnValueChanged event triggered with value: {GetValue()}");
            }
        }

        public void SetValue<T>(T value)
        {
            SetValue((object)value);
        }

        #endregion

        #region Reset

        public void ResetToDefault()
        {
            if (string.IsNullOrWhiteSpace(settingKey))
            {
                Debug.LogError("[SettingsObject] Cannot reset: SettingKey is null or empty.");
                return;
            }

            var previousValue = currentValue;
            currentValue = defaultValue;

            if (debugMode)
            {
                Debug.Log($"[SettingsObject] '{settingKey}' reset to default: '{previousValue}' -> '{defaultValue}' (Type: {valueType})");
            }

            PlayerPrefs.DeleteKey(settingKey);
            PlayerPrefs.Save();

            if (debugMode)
            {
                Debug.Log($"[SettingsObject] '{settingKey}' removed from PlayerPrefs");
            }

            OnValueChanged?.Invoke(this, GetValue());
        }

        #endregion

        #region Type Conversion

        private string ConvertValueToString(object value)
        {
            return valueType switch
            {
                ValueType.String => value.ToString(),
                ValueType.Int => ((int)value).ToString(),
                ValueType.Float => ((float)value).ToString("F6"),
                ValueType.Bool => ((bool)value).ToString().ToLower(),
                ValueType.Color => ColorUtility.ToHtmlStringRGBA((Color)value),
                _ => value.ToString()
            };
        }

        private object ConvertStringToValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return GetDefaultValue();
            }

            return valueType switch
            {
                ValueType.String => value,
                ValueType.Int => int.Parse(value),
                ValueType.Float => float.Parse(value),
                ValueType.Bool => bool.Parse(value),
                ValueType.Color => ColorFromHtml(value),
                _ => value
            };
        }

        private object GetDefaultValue()
        {
            return valueType switch
            {
                ValueType.String => string.Empty,
                ValueType.Int => 0,
                ValueType.Float => 0f,
                ValueType.Bool => false,
                ValueType.Color => Color.white,
                _ => null
            };
        }

        private Color ColorFromHtml(string htmlColor)
        {
            if (ColorUtility.TryParseHtmlString("#" + htmlColor, out Color color))
            {
                return color;
            }

            Debug.LogWarning($"[SettingsObject] Invalid color format: {htmlColor}, returning white");
            return Color.white;
        }

        #endregion
    }
}