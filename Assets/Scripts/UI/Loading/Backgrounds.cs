using UnityEngine;


namespace Ninja.UI.Loading
{
    [CreateAssetMenu(fileName = "Backgrounds", menuName = "Ninja/UI/Loading/Backgrounds")]
    public class Backgrounds : ScriptableObject
    {
        [SerializeField] private Sprite[] backgrounds;

        public Sprite GetRandomBackground()
        {
            if (backgrounds.Length == 0)
            {
                return null;
            }

            int index = Random.Range(0, backgrounds.Length);
            return backgrounds[index];
        }
    }
}