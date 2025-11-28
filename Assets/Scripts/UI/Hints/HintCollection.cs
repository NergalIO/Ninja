using System.Collections.Generic;
using UnityEngine;


namespace Ninja.Hints
{
    [CreateAssetMenu(fileName = "HintCollection", menuName = "Ninja/Hints/Collection")]
    public class HintCollection : ScriptableObject
    {
        [SerializeField] private List<Hint> hints;

        public string Hint { get { return hints[Random.Range(0, hints.Count - 1)].Text; } }
    }
}