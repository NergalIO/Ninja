using UnityEngine;

namespace Ninja.Core {
    public static class ComponentExtensions {
        public static T GetOrAddComponent<T>(this Component component) where T : Component {
            if (component == null) {
                return null;
            }

            return component.gameObject.GetOrAddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
            if (gameObject == null) {
                return null;
            }

            if (gameObject.TryGetComponent<T>(out var existing)) {
                return existing;
            }

            return gameObject.AddComponent<T>();
        }
    }
}

