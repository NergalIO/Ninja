using UnityEngine;

namespace Ninja.UI.Loading
{
    [CreateAssetMenu(fileName = "Backgrounds", menuName = "Ninja/UI/Loading/Backgrounds")]
    public class Backgrounds : ScriptableObject
    {
        [SerializeField] private Sprite[] backgrounds;

        public Sprite GetRandomBackground() =>
            backgrounds.Length > 0 ? backgrounds[Random.Range(0, backgrounds.Length)] : null;
    }
}
