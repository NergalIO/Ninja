using UnityEngine;

namespace Ninja.Core
{
    public static class ComponentExtensions
    {
        public static T GetOrAddComponent<T>(this Component component) where T : Component =>
            component == null ? null : component.gameObject.GetOrAddComponent<T>();

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component =>
            go == null ? null : go.TryGetComponent<T>(out var c) ? c : go.AddComponent<T>();
    }
}
