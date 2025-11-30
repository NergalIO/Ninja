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

        public void OnEnable()
        {
            toggle.onValueChanged.AddListener((value) => OnTogleValueChanged());
            slider.onValueChanged.AddListener((value) => OnSliderValueChanged());
        }

        public void OnDisable()
        {
            toggle.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.RemoveAllListeners();
        }

        public void OnTogleValueChanged()
        {
            SettingsManager.Instance.SetSettingValue(settingsObject.SettingKey, toggle.isOn);
        }

        public void OnSliderValueChanged()
        {
            SettingsManager.Instance.SetSettingValue(settingsObject.SettingKey, slider.value);
        }

        public void SetObject(SettingsObject settingsObject)
        {
            this.settingsObject = settingsObject;
            settingNameText.text = settingsObject.SettingName;
            switch (settingsObject.Type)
            {
                case SettingsObject.ValueType.Bool:
                    toggle.gameObject.SetActive(true);
                    slider.gameObject.SetActive(false);
                    bool boolValue = SettingsManager.Instance.GetSettingValue<bool>(settingsObject.SettingKey);
                    toggle.isOn = boolValue;
                    toggle.onValueChanged.AddListener((value) => {
                        SettingsManager.Instance.SetSettingValue(settingsObject.SettingKey, value);
                    });
                    break;

                case SettingsObject.ValueType.Float:
                    toggle.gameObject.SetActive(false);
                    slider.gameObject.SetActive(true);
                    float floatValue = SettingsManager.Instance.GetSettingValue<float>(settingsObject.SettingKey);
                    slider.value = floatValue;
                    slider.maxValue = settingsObject.MaxValue;
                    slider.minValue = settingsObject.MinValue;
                    slider.onValueChanged.AddListener((value) => {
                        SettingsManager.Instance.SetSettingValue(settingsObject.SettingKey, value);
                    });
                    break;
            }
        }
    }
}