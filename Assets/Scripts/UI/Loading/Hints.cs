using System.Collections.Generic;
using UnityEngine;

namespace Ninja.UI.Loading
{
    [CreateAssetMenu(fileName = "Hints", menuName = "Ninja/UI/Loading/Hints")]
    public class Hints : ScriptableObject
    {
        [SerializeField] private List<string> hints;

        public string GetRandomHint() =>
            hints?.Count > 0 ? hints[Random.Range(0, hints.Count)] : string.Empty;
    }
}
