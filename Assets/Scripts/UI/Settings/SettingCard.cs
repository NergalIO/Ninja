using Ninja.Systems.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ninja.UI.Menu
{
    public class SettingCard : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TMP_Text settingNameText;
        [SerializeField] private Toggle toggle;
        [SerializeField] private Slider slider;

        private SettingsObject settingsObject;
        private bool isInitialized = false;

        private void OnEnable()
        {
            if (isInitialized)
            {
                RefreshUI();
            }
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        public void SetObject(SettingsObject settingsObject)
        {
            this.settingsObject = settingsObject;
            settingNameText.text = settingsObject.SettingName;
            
            RemoveListeners();
            SetupUI();
            isInitialized = true;
        }

        private void SetupUI()
        {
            switch (settingsObject.Type)
            {
                case SettingsObject.ValueType.Bool:
                    SetupToggle();
                    break;

                case SettingsObject.ValueType.Float:
                    SetupSlider();
                    break;

                default:
                    toggle.gameObject.SetActive(false);
                    slider.gameObject.SetActive(false);
                    break;
            }
        }

        private void SetupToggle()
        {
            toggle.gameObject.SetActive(true);
            slider.gameObject.SetActive(false);
            
            bool boolValue = SettingsManager.Instance.GetSettingValue<bool>(settingsObject.SettingKey);
            toggle.SetIsOnWithoutNotify(boolValue);
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void SetupSlider()
        {
            toggle.gameObject.SetActive(false);
            slider.gameObject.SetActive(true);
            
            float floatValue = SettingsManager.Instance.GetSettingValue<float>(settingsObject.SettingKey);
            slider.minValue = settingsObject.MinValue;
            slider.maxValue = settingsObject.MaxValue;
            slider.SetValueWithoutNotify(floatValue);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void RefreshUI()
        {
            if (settingsObject == null)
                return;

            switch (settingsObject.Type)
            {
                case SettingsObject.ValueType.Bool:
                    bool boolValue = SettingsManager.Instance.GetSettingValue<bool>(settingsObject.SettingKey);
                    toggle.SetIsOnWithoutNotify(boolValue);
                    break;

                case SettingsObject.ValueType.Float:
                    float floatValue = SettingsManager.Instance.GetSettingValue<float>(settingsObject.SettingKey);
                    slider.SetValueWithoutNotify(floatValue);
                    break;
            }
        }

        private void OnToggleValueChanged(bool value)
        {
            if (settingsObject != null)
            {
                SettingsManager.Instance.SetSettingValue(settingsObject.SettingKey, value);
            }
        }

        private void OnSliderValueChanged(float value)
        {
            if (settingsObject != null)
            {
                SettingsManager.Instance.SetSettingValue(settingsObject.SettingKey, value);
            }
        }

        private void RemoveListeners()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }

            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }
    }
}