using UnityEngine;
using Ninja.Managers;


namespace Ninja.Levels
{
    [CreateAssetMenu(fileName = "Level", menuName = "Ninja/Level")]
    public class Level : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string levelName;

        public string Id => id;
        public string LevelName => levelName;
    }
}