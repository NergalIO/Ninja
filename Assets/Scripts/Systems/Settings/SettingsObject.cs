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
                LogDebug($"'{settingKey}' initialized with default value: '{defaultValue}' (Type: {valueType})");
            }
        }

        #region Loading & Saving

        public void LoadFromPlayerPrefs()
        {
            if (!ValidateKey())
                return;

            if (PlayerPrefs.HasKey(settingKey))
            {
                var previousValue = currentValue;
                string loadedValue = PlayerPrefs.GetString(settingKey);
                
                if (valueType == ValueType.Float)
                {
                    if (float.TryParse(loadedValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float floatValue))
                    {
                        if (floatValue >= minValue && floatValue <= maxValue)
                        {
                            currentValue = loadedValue;
                        }
                        else
                        {
                            LogWarning($"Loaded value {floatValue} is out of range [{minValue}, {maxValue}], using default");
                            currentValue = defaultValue;
                        }
                    }
                    else
                    {
                        LogWarning($"Failed to parse loaded value '{loadedValue}' as float, using default");
                        currentValue = defaultValue;
                    }
                }
                else
                {
                    currentValue = loadedValue;
                }
                
                LogDebug($"'{settingKey}' loaded from PlayerPrefs: '{previousValue}' -> '{currentValue}' (Type: {valueType})");
            }
            else
            {
                currentValue = defaultValue;
                LogDebug($"'{settingKey}' not found in PlayerPrefs, using default: '{defaultValue}' (Type: {valueType})");
            }
        }

        public void SaveToPlayerPrefs()
        {
            if (!ValidateKey())
                return;

            PlayerPrefs.SetString(settingKey, currentValue);
            PlayerPrefs.Save();
            LogDebug($"'{settingKey}' saved to PlayerPrefs: '{currentValue}' (Type: {valueType})");
        }

        #endregion

        #region Getting Values

        public object GetValue()
        {
            try
            {
                var result = ConvertStringToValue(currentValue);
                LogDebug($"'{settingKey}' retrieved value: {result} (Type: {valueType})");
                return result;
            }
            catch (Exception ex)
            {
                LogError($"Cannot convert '{currentValue}' to type {valueType} for key '{settingKey}'. Error: {ex.Message}");
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
                    LogDebug($"'{settingKey}' converted to {typeof(T).Name}: {typedValue}");
                    return typedValue;
                }

                if (typeof(T) == typeof(float) && value is double doubleValue)
                {
                    return (T)(object)(float)doubleValue;
                }

                if (typeof(T) == typeof(float) && value is int intValue)
                {
                    return (T)(object)(float)intValue;
                }

                var result = (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
                LogDebug($"'{settingKey}' converted to {typeof(T).Name}: {result}");
                return result;
            }
            catch (Exception ex)
            {
                LogError($"Cannot convert '{currentValue}' to type {typeof(T).Name} for key '{settingKey}'. Error: {ex.Message}");
                return default;
            }
        }

        public string GetStringValue()
        {
            LogDebug($"Retrieved string value for '{settingKey}': '{currentValue}'");
            return currentValue;
        }

        #endregion

        #region Setting Values

        public void SetValue(object value)
        {
            if (value == null)
            {
                LogWarning($"Attempted to set null value for key '{settingKey}'.");
                return;
            }

            if (!ValidateValue(value))
                return;

            object convertedValue = value;
            if (valueType == ValueType.Float && value is not float)
            {
                try
                {
                    convertedValue = Convert.ToSingle(value);
                }
                catch (Exception ex)
                {
                    LogError($"Failed to convert value to float: {ex.Message}");
                    return;
                }
            }

            if (IsValueEqual(convertedValue))
            {
                LogDebug($"'{settingKey}' value unchanged (Type: {valueType})");
                return;
            }

            string newValue = ConvertValueToString(convertedValue);
            var previousValue = currentValue;
            currentValue = newValue;

            LogDebug($"'{settingKey}' value changed: '{previousValue}' -> '{newValue}' (Type: {valueType}, Original: {value})");

            SaveToPlayerPrefs();
            
            object finalValue = GetValue();
            OnValueChanged?.Invoke(this, finalValue);

            LogDebug($"'{settingKey}' OnValueChanged event triggered with value: {finalValue}");
        }

        public void SetValue<T>(T value)
        {
            SetValue((object)value);
        }

        #endregion

        #region Reset

        public void ResetToDefault()
        {
            if (!ValidateKey())
                return;

            var previousValue = currentValue;
            currentValue = defaultValue;

            LogDebug($"'{settingKey}' reset to default: '{previousValue}' -> '{defaultValue}' (Type: {valueType})");

            PlayerPrefs.DeleteKey(settingKey);
            PlayerPrefs.Save();

            LogDebug($"'{settingKey}' removed from PlayerPrefs");

            OnValueChanged?.Invoke(this, GetValue());
        }

        #endregion

        #region Type Conversion

        private string ConvertValueToString(object value)
        {
            try
            {
                return valueType switch
                {
                    ValueType.String => value.ToString(),
                    ValueType.Int => ((int)value).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ValueType.Float => ((float)value).ToString("F6", System.Globalization.CultureInfo.InvariantCulture),
                    ValueType.Bool => ((bool)value).ToString().ToLower(),
                    ValueType.Color => ColorUtility.ToHtmlStringRGBA((Color)value),
                    _ => value.ToString()
                };
            }
            catch (Exception ex)
            {
                LogError($"Failed to convert value to string: {ex.Message}, Value: {value}, Type: {value.GetType()}");
                return value.ToString();
            }
        }

        private object ConvertStringToValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return GetDefaultValue();
            }

            try
            {
                object result = valueType switch
                {
                    ValueType.String => value,
                    ValueType.Int => int.Parse(value, System.Globalization.CultureInfo.InvariantCulture),
                    ValueType.Float => float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture),
                    ValueType.Bool => bool.Parse(value),
                    ValueType.Color => ColorFromHtml(value),
                    _ => value
                };
                
                if (valueType == ValueType.Float && result is float floatResult)
                {
                    if (floatResult < minValue || floatResult > maxValue)
                    {
                        LogWarning($"Parsed float value {floatResult} is out of range [{minValue}, {maxValue}], clamping");
                        floatResult = Mathf.Clamp(floatResult, minValue, maxValue);
                        result = floatResult;
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogError($"Failed to parse value '{value}' as {valueType}: {ex.Message}");
                return GetDefaultValue();
            }
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

            LogWarning($"Invalid color format: {htmlColor}, returning white");
            return Color.white;
        }

        #endregion

        #region Validation

        private bool ValidateKey()
        {
            if (string.IsNullOrWhiteSpace(settingKey))
            {
                LogError("SettingKey is null or empty.");
                return false;
            }
            return true;
        }

        private bool ValidateValue(object value)
        {
            if (valueType == ValueType.Int || valueType == ValueType.Float)
            {
                if (value is IConvertible convertible)
                {
                    try
                    {
                        float numValue = Convert.ToSingle(value);
                        if (numValue < minValue || numValue > maxValue)
                        {
                            LogWarning($"Value {numValue} is out of range [{minValue}, {maxValue}] for key '{settingKey}'");
                            return false;
                        }
                    }
                    catch
                    {
                        LogWarning($"Cannot convert value to number for key '{settingKey}'");
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsValueEqual(object newValue)
        {
            if (string.IsNullOrEmpty(currentValue))
                return false;

            try
            {
                var currentObj = ConvertStringToValue(currentValue);
                
                if (valueType == ValueType.Float)
                {
                    float currentFloat = Convert.ToSingle(currentObj);
                    float newFloat = Convert.ToSingle(newValue);
                    const float epsilon = 0.001f;
                    bool isEqual = Mathf.Abs(currentFloat - newFloat) < epsilon;
                    
                    if (isEqual)
                    {
                        LogDebug($"Values are approximately equal: {currentFloat} ≈ {newFloat}");
                    }
                    
                    return isEqual;
                }
                else if (valueType == ValueType.Int)
                {
                    int currentInt = Convert.ToInt32(currentObj);
                    int newInt = Convert.ToInt32(newValue);
                    return currentInt == newInt;
                }
                else if (valueType == ValueType.Bool)
                {
                    bool currentBool = Convert.ToBoolean(currentObj);
                    bool newBool = Convert.ToBoolean(newValue);
                    return currentBool == newBool;
                }
                else
                {
                    return currentObj.Equals(newValue);
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Error comparing values: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Logging

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[SettingsObject] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SettingsObject] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SettingsObject] {message}");
        }

        #endregion
    }
}