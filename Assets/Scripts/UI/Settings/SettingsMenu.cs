using System.Collections.Generic;
using Ninja.Input;
using Ninja.Systems.Settings;
using UnityEngine;

namespace Ninja.UI.Menu
{
    public class SettingsMenu : MenuBase
    {
        [Header("References")]
        [SerializeField] private GameObject settingsGroupPrefab;

        [Header("Components")]
        [SerializeField] private Transform container;

        [Header("Variables")]
        [SerializeField] private List<SettingsGroup> settingsGroups = new();

        private void OnEnable()
        {
            UIController.Instance.FocusMenu(this);
            Refresh();
        }

        private void OnDisable()
        {
            Clear();
            UIController.Instance.UnfocusMenu(this);
        }

        private void Refresh()
        {
            Clear();
            SettingsManager.Instance.GetAllSettings(out List<SettingsObject> settingsObjects);

            foreach (SettingsObject localSettingsObject in settingsObjects)
            {
                SettingsGroup group = GetOrCreateSettingsGroup(localSettingsObject.Group);
                group.AddSettingObject(localSettingsObject);
            }
        }

        private SettingsGroup GetOrCreateSettingsGroup(string groupName)
        {
            foreach (SettingsGroup group in settingsGroups)
            {
                if (group.GroupName == groupName)
                    return group;
            }

            GameObject groupObj = Instantiate(settingsGroupPrefab, container);
            SettingsGroup newGroup = groupObj.GetComponent<SettingsGroup>();
            newGroup.SetTitle(groupName);
            settingsGroups.Add(newGroup);
            return newGroup;
        }

        private void Clear()
        {
            foreach (SettingsGroup card in settingsGroups)
                Destroy(card.gameObject);

            settingsGroups.Clear();
        }

        public override void OnEscPressed()
        {
            if (!IsFocused)
                return;

            Close();
        }
    }
}
