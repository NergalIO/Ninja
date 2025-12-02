using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ninja.Core
{
    public static class RandomSprite
    {
        public static Sprite GetRandomSpriteFromResources(string path)
        {
            List<Sprite> images = Resources.LoadAll<Sprite>(path).ToList();
            if (images.Count == 0)
            {
                return default;
            }
            return images[Random.Range(0, images.Count - 1)];
        }

        public static Sprite RandomSpriteFromResources
                => GetRandomSpriteFromResources("Images/Background");
    }
}

