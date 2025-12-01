using UnityEngine;
using UnityEngine.AI;

namespace Ninja.Gameplay.Enemy
{
    public class EnemyStateContext
    {
        public const float PATROL_DISTANCE_THRESHOLD = 0.25f;

        // Navigation
        public NavMeshAgent Agent { get; set; }
        public float RotationSpeed { get; set; }
        public Transform Transform { get; set; }

        // Player
        public Transform Player { get; set; }

        // Patrol
        public Transform[] PatrolPoints { get; set; }
        public float PatrolSpeed { get; set; }
        public int CurrentPatrolPointIndex { get; set; }
        public float WaitTimeAtPatrolPoint { get; set; }

        // Chase
        public float ChaseSpeed { get; set; }
        public float LoseTargetTime { get; set; }
        public float LastSeenTargetTime { get; set; }

        // View
        public FieldOfView FieldOfView { get; set; }

        // Search
        public float TimeToForgetTarget { get; set; }
        public float SearchStartTime { get; set; }

        // Investigate
        public float InvestigateSpeed { get; set; }
        public Vector3 NoisePosition { get; set; }
        public bool HasNoisePosition { get; set; }

        // Scan
        public float ScanDuration { get; set; }
        public float ScanStartTime { get; set; }

        // Return
        public float ReturnSpeed { get; set; }
        public Vector3 LastPatrolPosition { get; set; }
        public bool HasLastPatrolPosition { get; set; }

        // State management
        public System.Action<EnemyState> OnStateChange { get; set; }
        public System.Func<EnemyState> GetCurrentState { get; set; }
        public System.Action OnPlayerDetected { get; set; }
        public System.Action MoveToNextPatrolPoint { get; set; }
        public MonoBehaviour CoroutineRunner { get; set; }
    }
}

