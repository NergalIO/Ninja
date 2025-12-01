using System.Collections.Generic;
using Ninja.Systems.Settings;
using TMPro;
using UnityEngine;


namespace Ninja.UI.Menu
{
    public class SettingsGroup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject settingsCardPrefab;

        [Header("Components")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Transform container;

        [Header("Variables")]
        [SerializeField] private string groupName;
        [SerializeField] private List<SettingsObject> settingCards = new();
        [SerializeField] private List<SettingCard> cardInstances = new();

        public string GroupName => groupName;

        private void OnEnable()
        {
            Refresh();
        }

        private void OnDisable()
        {
            Clear();
        }

        public void SetTitle(string title)
        {
            groupName = title;
            titleText.text = title;
        }

        public void AddSettingObject(SettingsObject settingsObject)
        {
            settingCards.Add(settingsObject);
            Refresh();
        }

        public void Refresh()
        {
            Clear();
            foreach (SettingsObject settingsObject in settingCards)
            {
                SettingCard card = Instantiate(settingsCardPrefab, container).GetComponent<SettingCard>();
                card.SetObject(settingsObject);
                cardInstances.Add(card);
            }
        }

        public void Clear()
        {
            foreach (SettingCard card in cardInstances)
            {
                Destroy(card.gameObject);
            }
            cardInstances.Clear();
        }
    }
}