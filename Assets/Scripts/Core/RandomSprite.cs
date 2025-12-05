using UnityEngine;

namespace Ninja.Core
{
    public static class RandomSprite
    {
        public static Sprite GetRandom(string path)
        {
            var sprites = Resources.LoadAll<Sprite>(path);
            return sprites.Length > 0 ? sprites[Random.Range(0, sprites.Length)] : null;
        }
    }
}
