using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ninja.Managers;
using Ninja.Utils;


namespace Ninja.Levels
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private List<Level> levels = new();

        public List<Level> Levels => levels;

        public void Awake()
        {
            LoadLevels();
        }

        private void LoadLevels()
        {
            foreach (Level level in Resources.LoadAll<Level>("Levels"))
            {
                levels.Add(level);
            }
        }

        public bool TryGetLevel(string levelId, out Level level)
        {
            level = levels.FirstOrDefault(level => level.Id == levelId);
            if (level == null)
            {
                level = default;
                return false;
            }
            return true;
        }

        public void LoadLevel(string levelId)
        {
            Level level;
            if (TryGetLevel(levelId, out level))
            {
                AsyncSceneLoader.Instance.LoadScene(level.LevelName);
            }
        }
    }
}