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

        public void OnEnable()
        {
            Refresh();
        }

        public void OnDisable()
        {
            Clear();
        }

        public void Refresh()
        {
            Clear();
            SettingsManager.Instance.GetAllSettings(out List<SettingsObject> settingsObjects);
            foreach (SettingsObject localSettingsObject in settingsObjects)
            {
                SettingsGroup group = GetOrCreateSettingsGroup(localSettingsObject.Group);
                group.AddSettingObject(localSettingsObject);
            }
        }

        public SettingsGroup GetOrCreateSettingsGroup(string groupName)
        {
            foreach (SettingsGroup group in settingsGroups)
            {
                if (group.GroupName == groupName)
                {
                    return group;
                }
            }

            GameObject groupObj = Instantiate(settingsGroupPrefab, container);
            SettingsGroup newGroup = groupObj.GetComponent<SettingsGroup>();
            newGroup.SetTitle(groupName);
            settingsGroups.Add(newGroup);
            return newGroup;
        }

        public void Clear()
        {
            foreach (SettingsGroup card in settingsGroups)
            {
                Destroy(card.gameObject);
            }
            settingsGroups.Clear();
        }
    }
}