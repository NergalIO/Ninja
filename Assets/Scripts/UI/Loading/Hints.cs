using System.Collections.Generic;
using UnityEngine;


namespace Ninja.UI.Loading
{
    [CreateAssetMenu(fileName = "Hints", menuName = "Ninja/UI/Loading/Hints")]
    public class Hints : ScriptableObject
    {
        [SerializeField] private List<string> hints;

        public string GetRandomHint()
        {
            if (hints == null || hints.Count == 0)
            {
                return string.Empty;
            }

            int randomIndex = Random.Range(0, hints.Count);
            return hints[randomIndex];
        }
    }
}