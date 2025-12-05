using UnityEngine;

namespace Ninja.Core.Events
{
    /// <summary>
    /// Базовый класс аргументов события
    /// </summary>
    public class EventArgs
    {
        public object Sender { get; set; }
        public float Timestamp { get; }

        public EventArgs() => Timestamp = Time.time;
    }

    /// <summary>
    /// Аргументы события игрока
    /// </summary>
    public class PlayerEventArgs : EventArgs
    {
        public Vector3 Position { get; set; }
        public Transform Transform { get; set; }

        public PlayerEventArgs() { }
        public PlayerEventArgs(Vector3 position) => Position = position;
        public PlayerEventArgs(Transform transform)
        {
            Transform = transform;
            Position = transform ? transform.position : Vector3.zero;
        }
    }

    /// <summary>
    /// Аргументы события обнаружения игрока
    /// </summary>
    public class PlayerDetectedEventArgs : PlayerEventArgs
    {
        public Vector3 DetectorPosition { get; set; }
        public GameObject Detector { get; set; }

        public PlayerDetectedEventArgs(Vector3 playerPos, Vector3 detectorPos, GameObject detector = null)
        {
            Position = playerPos;
            DetectorPosition = detectorPos;
            Detector = detector;
        }
    }

    /// <summary>
    /// Аргументы события шума
    /// </summary>
    public class NoiseEventArgs : EventArgs
    {
        public Vector3 Position { get; set; }
        public float Level { get; set; }

        public NoiseEventArgs(Vector3 position, float level = 1f)
        {
            Position = position;
            Level = level;
        }
    }

    /// <summary>
    /// Аргументы события уровня
    /// </summary>
    public class LevelEventArgs : EventArgs
    {
        public string LevelId { get; set; }
        public float CompletionTime { get; set; }
        public bool Success { get; set; }

        public LevelEventArgs(string levelId) => LevelId = levelId;
        public LevelEventArgs(string levelId, float time, bool success)
        {
            LevelId = levelId;
            CompletionTime = time;
            Success = success;
        }
    }

    /// <summary>
    /// Аргументы события врага
    /// </summary>
    public class EnemyEventArgs : EventArgs
    {
        public GameObject Enemy { get; set; }
        public Vector3 Position { get; set; }
        public string PreviousState { get; set; }
        public string NewState { get; set; }

        public EnemyEventArgs(GameObject enemy)
        {
            Enemy = enemy;
            Position = enemy ? enemy.transform.position : Vector3.zero;
        }

        public EnemyEventArgs(GameObject enemy, string prevState, string newState) : this(enemy)
        {
            PreviousState = prevState;
            NewState = newState;
        }
    }

    /// <summary>
    /// Аргументы события взаимодействия
    /// </summary>
    public class InteractionEventArgs : EventArgs
    {
        public GameObject Interactor { get; set; }
        public GameObject Interactable { get; set; }
        public string InteractionHint { get; set; }
        public Vector3 Position { get; set; }

        public InteractionEventArgs(GameObject interactor, GameObject interactable, string hint = null)
        {
            Interactor = interactor;
            Interactable = interactable;
            InteractionHint = hint ?? string.Empty;
            Position = interactable ? interactable.transform.position : Vector3.zero;
        }
    }
}
