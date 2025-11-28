using UnityEngine;

namespace Ninja.Settings {
    public enum SettingDataType {
        Float,
        Int,
        Bool,
        String
    }

    [CreateAssetMenu(fileName = "GameSetting", menuName = "Ninja/Settings/Game Setting")]
    public class GameSetting : ScriptableObject {
        [Header("Basic Info")]
        [SerializeField] private string settingName;
        [SerializeField] private string displayText;
        [SerializeField] private SettingDataType dataType;

        [Header("Numeric Settings")]
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;

        [Header("Default Value")]
        [SerializeField] private float defaultValueFloat = 0f;
        [SerializeField] private int defaultValueInt = 0;
        [SerializeField] private bool defaultValueBool = false;
        [SerializeField] private string defaultValueString = "";

        [Header("PlayerPrefs")]
        [SerializeField] private string playerPrefsKey;

        public string SettingName => settingName;
        public string DisplayText => displayText;
        public SettingDataType DataType => dataType;
        public float MinValue => minValue;
        public float MaxValue => maxValue;
        public string PlayerPrefsKey => string.IsNullOrEmpty(playerPrefsKey) ? settingName : playerPrefsKey;

        public object GetDefaultValue() {
            return dataType switch {
                SettingDataType.Float => defaultValueFloat,
                SettingDataType.Int => defaultValueInt,
                SettingDataType.Bool => defaultValueBool,
                SettingDataType.String => defaultValueString,
                _ => defaultValueFloat
            };
        }

        public void SetValue(object value) {
            switch (dataType) {
                case SettingDataType.Float:
                    float floatVal = Mathf.Clamp((float)value, minValue, maxValue);
                    PlayerPrefs.SetFloat(PlayerPrefsKey, floatVal);
                    break;
                case SettingDataType.Int:
                    int intVal = Mathf.RoundToInt(Mathf.Clamp((float)value, minValue, maxValue));
                    PlayerPrefs.SetInt(PlayerPrefsKey, intVal);
                    break;
                case SettingDataType.Bool:
                    PlayerPrefs.SetInt(PlayerPrefsKey, (bool)value ? 1 : 0);
                    break;
                case SettingDataType.String:
                    PlayerPrefs.SetString(PlayerPrefsKey, (string)value);
                    break;
            }
            PlayerPrefs.Save();
        }

        public object GetValue() {
            return dataType switch {
                SettingDataType.Float => PlayerPrefs.GetFloat(PlayerPrefsKey, defaultValueFloat),
                SettingDataType.Int => PlayerPrefs.GetInt(PlayerPrefsKey, defaultValueInt),
                SettingDataType.Bool => PlayerPrefs.GetInt(PlayerPrefsKey, defaultValueBool ? 1 : 0) == 1,
                SettingDataType.String => PlayerPrefs.GetString(PlayerPrefsKey, defaultValueString),
                _ => GetDefaultValue()
            };
        }

        public T GetValue<T>() {
            var value = GetValue();
            if (value is T typedValue) {
                return typedValue;
            }
            return default(T);
        }
    }
}

