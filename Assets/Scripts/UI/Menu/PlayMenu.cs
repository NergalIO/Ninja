using System.Collections.Generic;
using Ninja.Gameplay.Levels;
using UnityEngine;


namespace Ninja.UI.Menu
{
    public class PlayMenu : MenuBase
    {
        [Header("References")]
        [SerializeField] private LevelController levelController;
        [SerializeField] private GameObject levelWidgetPrefab;
        [SerializeField] private Transform container;

        [Header("Variables")]
        [SerializeField] private List<LevelWidget> levelWidgets = new();

        public IReadOnlyCollection<LevelWidget> Levels => levelWidgets;

        public void OnEnable()
        {
            Refresh();            
        }

        public void OnDisable()
        {
            Clear();
        }

        private void Refresh()
        {
            Clear();
            foreach (Level level in levelController.Levels)
            {
                GameObject widgetObj = Instantiate(levelWidgetPrefab, container);
                LevelWidget widget = widgetObj.GetComponent<LevelWidget>();
                widget.SetLevel(level);
                levelWidgets.Add(widget);
            }
        }

        private void Clear()
        {
            foreach (LevelWidget widget in levelWidgets)
            {
                Destroy(widget.gameObject);
            }
            levelWidgets.Clear();
        }
    }
}