using UnityEngine;
using UnityEngine.UI;

namespace Ninja.Settings {
    [RequireComponent(typeof(RectTransform))]
    public class SettingCard : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private Component titleText; // Text или TextMeshProUGUI
        [SerializeField] private Component valueText; // Text или TextMeshProUGUI
        [SerializeField] private Slider slider;
        [SerializeField] private Toggle toggle;
        [SerializeField] private Component inputField; // InputField или TMP_InputField

        private GameSetting currentSetting;
        private bool isInitialized = false;

        public void Initialize(GameSetting setting) {
            if (setting == null) {
                Debug.LogError("[SettingCard] Cannot initialize with null setting.");
                return;
            }

            currentSetting = setting;
            isInitialized = true;

            // Устанавливаем заголовок
            SetText(titleText, setting.DisplayText);

            // Настраиваем UI в зависимости от типа данных
            SetupUIForDataType(setting.DataType);

            // Загружаем текущее значение
            LoadCurrentValue();
        }

        private void SetupUIForDataType(SettingDataType dataType) {
            // Скрываем все элементы UI
            if (slider != null) slider.gameObject.SetActive(false);
            if (toggle != null) toggle.gameObject.SetActive(false);
            if (inputField != null) inputField.gameObject.SetActive(false);

            switch (dataType) {
                case SettingDataType.Float:
                    if (slider != null) {
                        slider.gameObject.SetActive(true);
                        slider.wholeNumbers = false;
                        slider.minValue = currentSetting.MinValue;
                        slider.maxValue = currentSetting.MaxValue;
                        slider.onValueChanged.RemoveAllListeners();
                        slider.onValueChanged.AddListener(OnSliderValueChanged);
                    }
                    break;

                case SettingDataType.Int:
                    if (slider != null) {
                        slider.gameObject.SetActive(true);
                        slider.wholeNumbers = true;
                        slider.minValue = currentSetting.MinValue;
                        slider.maxValue = currentSetting.MaxValue;
                        slider.onValueChanged.RemoveAllListeners();
                        slider.onValueChanged.AddListener(OnSliderValueChanged);
                    }
                    break;

                case SettingDataType.Bool:
                    if (toggle != null) {
                        toggle.gameObject.SetActive(true);
                        toggle.onValueChanged.RemoveAllListeners();
                        toggle.onValueChanged.AddListener(OnToggleValueChanged);
                    }
                    break;

                case SettingDataType.String:
                    if (inputField != null) {
                        inputField.gameObject.SetActive(true);
                        SetupInputField(inputField);
                    }
                    break;
            }
        }

        private void SetupInputField(Component inputFieldComponent) {
            // Пытаемся найти метод onValueChanged или onEndEdit
            var inputFieldType = inputFieldComponent.GetType();
            
            // Для стандартного InputField
            if (inputFieldType == typeof(InputField)) {
                var inputField = inputFieldComponent as InputField;
                if (inputField != null) {
                    inputField.onValueChanged.RemoveAllListeners();
                    inputField.onEndEdit.AddListener(OnInputFieldValueChanged);
                }
            }
            // Для TMP_InputField (если доступен)
            else if (inputFieldType.Name == "TMP_InputField") {
                var onEndEditEvent = inputFieldType.GetEvent("onEndEdit");
                if (onEndEditEvent != null) {
                    var method = typeof(SettingCard).GetMethod("OnInputFieldValueChanged", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (method != null) {
                        var delegateType = onEndEditEvent.EventHandlerType;
                        var handler = System.Delegate.CreateDelegate(delegateType, this, method);
                        onEndEditEvent.RemoveEventHandler(inputFieldComponent, handler);
                        onEndEditEvent.AddEventHandler(inputFieldComponent, handler);
                    }
                }
            }
        }

        private void LoadCurrentValue() {
            if (currentSetting == null || !isInitialized) {
                return;
            }

            var value = currentSetting.GetValue();

            switch (currentSetting.DataType) {
                case SettingDataType.Float:
                    if (slider != null) {
                        slider.value = (float)value;
                    }
                    UpdateValueText(value);
                    break;

                case SettingDataType.Int:
                    if (slider != null) {
                        slider.value = (int)value;
                    }
                    UpdateValueText(value);
                    break;

                case SettingDataType.Bool:
                    if (toggle != null) {
                        toggle.isOn = (bool)value;
                    }
                    UpdateValueText(value);
                    break;

                case SettingDataType.String:
                    if (inputField != null) {
                        SetInputFieldText(inputField, (string)value);
                    }
                    UpdateValueText(value);
                    break;
            }
        }

        private void OnSliderValueChanged(float value) {
            if (currentSetting == null || !isInitialized) {
                return;
            }

            object valueToSet = currentSetting.DataType == SettingDataType.Int
                ? Mathf.RoundToInt(value)
                : value;

            SettingsManager.Instance?.SetSettingValue(currentSetting.SettingName, valueToSet);
            UpdateValueText(valueToSet);
        }

        private void OnToggleValueChanged(bool value) {
            if (currentSetting == null || !isInitialized) {
                return;
            }

            SettingsManager.Instance?.SetSettingValue(currentSetting.SettingName, value);
            UpdateValueText(value);
        }

        private void OnInputFieldValueChanged(string value) {
            if (currentSetting == null || !isInitialized) {
                return;
            }

            SettingsManager.Instance?.SetSettingValue(currentSetting.SettingName, value);
            UpdateValueText(value);
        }

        private void UpdateValueText(object value) {
            string text = "";
            if (value is float floatValue) {
                text = floatValue.ToString("F2");
            } else if (value is int intValue) {
                text = intValue.ToString();
            } else if (value is bool boolValue) {
                text = boolValue ? "On" : "Off";
            } else {
                text = value?.ToString() ?? "";
            }
            
            SetText(valueText, text);
        }

        private void SetText(Component textComponent, string text) {
            if (textComponent == null) return;
            
            var textType = textComponent.GetType();
            
            // Для стандартного Text
            if (textType == typeof(Text)) {
                var unityText = textComponent as Text;
                if (unityText != null) {
                    unityText.text = text;
                }
            }
            // Для TextMeshProUGUI (если доступен)
            else if (textType.Name == "TextMeshProUGUI") {
                var textProperty = textType.GetProperty("text");
                if (textProperty != null) {
                    textProperty.SetValue(textComponent, text);
                }
            }
        }

        private void SetInputFieldText(Component inputFieldComponent, string text) {
            if (inputFieldComponent == null) return;
            
            var inputFieldType = inputFieldComponent.GetType();
            
            // Для стандартного InputField
            if (inputFieldType == typeof(InputField)) {
                var inputField = inputFieldComponent as InputField;
                if (inputField != null) {
                    inputField.text = text;
                }
            }
            // Для TMP_InputField (если доступен)
            else if (inputFieldType.Name == "TMP_InputField") {
                var textProperty = inputFieldType.GetProperty("text");
                if (textProperty != null) {
                    textProperty.SetValue(inputFieldComponent, text);
                }
            }
        }

        private void OnEnable() {
            // Перезагружаем значение при включении (на случай, если настройка изменилась извне)
            if (isInitialized && currentSetting != null) {
                LoadCurrentValue();
            }
        }
    }
}
