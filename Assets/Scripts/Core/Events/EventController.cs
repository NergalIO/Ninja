using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ninja.Core.Events
{
    /// <summary>
    /// Централизованный контроллер событий.
    /// Использует событийно-ориентированный подход вместо постоянных проверок в Update().
    /// </summary>
    public class EventController : PersistentSingleton<EventController>
    {
        [SerializeField] private bool enableLogging = false;

        private readonly Dictionary<string, List<Action<EventArgs>>> subscribers = new();
        private int eventsTriggered = 0;

        public void Subscribe(string eventName, Action<EventArgs> handler)
        {
            if (!subscribers.ContainsKey(eventName))
                subscribers[eventName] = new List<Action<EventArgs>>();

            if (!subscribers[eventName].Contains(handler))
            {
                subscribers[eventName].Add(handler);
                Log($"Подписка на '{eventName}'");
            }
        }

        public void Unsubscribe(string eventName, Action<EventArgs> handler)
        {
            if (subscribers.TryGetValue(eventName, out var handlers))
            {
                handlers.Remove(handler);
                Log($"Отписка от '{eventName}'");
            }
        }

        public void TriggerEvent(string eventName, EventArgs args = null)
        {
            if (!subscribers.TryGetValue(eventName, out var handlers))
                return;

            eventsTriggered++;
            var handlersCopy = new List<Action<EventArgs>>(handlers);

            foreach (var handler in handlersCopy)
            {
                try
                {
                    handler?.Invoke(args);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Events] Ошибка '{eventName}': {e.Message}");
                }
            }

            Log($"'{eventName}' ({handlersCopy.Count} handlers)");
        }

        public bool HasSubscribers(string eventName) =>
            subscribers.TryGetValue(eventName, out var handlers) && handlers.Count > 0;

        public void Clear()
        {
            subscribers.Clear();
            Log("Все подписки очищены");
        }

        public void Clear(string eventName)
        {
            if (subscribers.ContainsKey(eventName))
            {
                subscribers[eventName].Clear();
                Log($"'{eventName}' очищено");
            }
        }

        public string GetStats()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Events triggered: {eventsTriggered}");
            sb.AppendLine($"Active events: {subscribers.Count}");
            foreach (var kvp in subscribers)
                sb.AppendLine($"  {kvp.Key}: {kvp.Value.Count}");
            return sb.ToString();
        }

        private void Log(string message)
        {
            if (enableLogging)
                Debug.Log($"[Events] {message}");
        }

        protected override void OnDestroy()
        {
            Clear();
            base.OnDestroy();
        }
    }

    /// <summary>
    /// Статический API для работы с событиями
    /// </summary>
    public static class Events
    {
        public static void Subscribe(string eventName, Action<EventArgs> handler) =>
            EventController.Instance?.Subscribe(eventName, handler);

        public static void Unsubscribe(string eventName, Action<EventArgs> handler) =>
            EventController.Instance?.Unsubscribe(eventName, handler);

        public static void Trigger(string eventName, EventArgs args = null) =>
            EventController.Instance?.TriggerEvent(eventName, args);

        public static bool HasSubscribers(string eventName) =>
            EventController.Instance?.HasSubscribers(eventName) ?? false;
    }
}
