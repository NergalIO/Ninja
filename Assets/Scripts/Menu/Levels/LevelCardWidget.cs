using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Ninja.Levels
{
    public class LevelCardWidget : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private LevelController controller;

        [Header("Level")]
        [SerializeField] private Level level;

        [Header("UI References")]
        [SerializeField] private TMP_Text levelName;
        [SerializeField] private Button loadButton;

        public Level Level => level;
        public void Awake()
        {
            loadButton.onClick.AddListener(OnLoadButton);
        }
        public void SetController(LevelController controller)
        {
            this.controller = controller;
        }
        public void SetLevel(Level level)
        {
            this.level = level;
            UpdateView();
        }

        private void UpdateView()
        {
            levelName.text = level.LevelName;
        }

        private void OnLoadButton()
        {
            controller.LoadLevel(level.Id);
        }
    }
}