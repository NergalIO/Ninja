using System;
using UnityEngine;
using UnityEngine.AI;

namespace Ninja.Gameplay.Enemy
{
    public class EnemyStateContext
    {
        public const float DISTANCE_THRESHOLD = 0.25f;

        // Core
        public NavMeshAgent Agent;
        public Transform Transform;
        public Transform Player;
        public MonoBehaviour CoroutineRunner;
        
        // Callbacks
        public Action<EnemyState> ChangeState;
        public Action OnPlayerDetected;

        // Patrol
        public Transform[] PatrolPoints;
        public float PatrolSpeed;
        public float WaitTime;
        public int CurrentPatrolIndex;
        public Vector3 LastPatrolPosition;
        public bool HasLastPatrolPosition;

        // Chase
        public float ChaseSpeed;
        public float LoseTargetTime;
        public float LastSeenTime;

        // Search
        public float ForgetTime;
        public float SearchStartTime;

        // Investigate
        public float InvestigateSpeed;
        public Vector3 NoisePosition;
        public bool HasNoisePosition;

        // Scan / Detection
        public FieldOfView FieldOfView;
        public DetectionSystem DetectionSystem;
        public float ScanDuration;
        public float ScanStartTime;

        // Return
        public float ReturnSpeed;

        public void NextPatrolPoint()
        {
            if (PatrolPoints == null || PatrolPoints.Length == 0) return;
            CurrentPatrolIndex = (CurrentPatrolIndex + 1) % PatrolPoints.Length;
            Agent.destination = PatrolPoints[CurrentPatrolIndex].position;
        }

        public bool ReachedDestination()
        {
            return !Agent.pathPending && Agent.remainingDistance < DISTANCE_THRESHOLD;
        }
    }
}
