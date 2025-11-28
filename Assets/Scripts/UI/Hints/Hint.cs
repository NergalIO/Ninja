using UnityEngine;


namespace Ninja.Hints
{
    [CreateAssetMenu(fileName = "Hint", menuName = "Ninja/Hints/Hint")]
    public class Hint : ScriptableObject
    {
        [SerializeField] private string text;

        public string Text => text;
    }
}