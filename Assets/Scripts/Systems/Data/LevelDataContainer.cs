using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ninja.Systems.Data
{
    [Serializable]
    public class LevelDataContainer
    {
        public Dictionary<string, LevelData> runs = new Dictionary<string, LevelData>();
    }
}

