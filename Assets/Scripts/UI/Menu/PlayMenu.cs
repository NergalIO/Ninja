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

        private void OnEnable()
        {
            Refresh();
            UIController.Instance.FocusMenu(this);
        }

        private void OnDisable()
        {
            Clear();
            UIController.Instance.UnfocusMenu(this);
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

        public override void OnEscPressed()
        {
            if (!IsFocused)
                return;

            Close();
        }
    }
}
