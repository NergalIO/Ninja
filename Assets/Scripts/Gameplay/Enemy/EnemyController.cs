using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Ninja.Gameplay.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private const float PATROL_DISTANCE_THRESHOLD = 0.25f;

        [Header("Navigation")]
        [SerializeField] private NavMeshAgent agent;

        [Header("Player")]
        [SerializeField] private Transform player;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolSpeed = 2f;
        private int currentPatrolPointIndex = 0;

        [Header("Chase")]
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float loseTargetTime = 3f;
        private float lastSeenTargetTime;

        [Header("View Settings")]
        [SerializeField] private FieldOfView fieldOfView;

        [Header("Search")]
        [SerializeField] private float timeToForgetTarget = 5f;
        private float searchStartTime;

        private EnemyState currentState = EnemyState.Patrol;
        private Coroutine forgetTargetCoroutine;

        private void Awake()
        {
            if (fieldOfView != null && player != null)
            {
                fieldOfView.SetTarget(player);
            }
        }

        private void FixedUpdate()
        {
            CheckForPlayer();

            switch (currentState)
            {
                case EnemyState.Patrol:
                    HandlePatrol();
                    break;

                case EnemyState.Chase:
                    HandleChase();
                    break;

                case EnemyState.Search:
                    HandleSearch();
                    break;
            }
        }

        #region Vision
        private void CheckForPlayer()
        {
            if (fieldOfView != null && fieldOfView.CanSeeTarget)
            {
                OnPlayerDetected();
            }
        }
        #endregion

        #region Patrol
        private void HandlePatrol()
        {
            agent.speed = patrolSpeed;

            if (patrolPoints == null || patrolPoints.Length == 0)
                return;

            if (!agent.pathPending && agent.remainingDistance < PATROL_DISTANCE_THRESHOLD)
            {
                MoveToNextPatrolPoint();
            }
        }

        private void MoveToNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return;

            agent.destination = patrolPoints[currentPatrolPointIndex].position;
            currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
        }
        #endregion

        #region Chase
        private void HandleChase()
        {
            if (player == null)
                return;

            agent.speed = chaseSpeed;
            agent.destination = player.position;
        }

        private void OnPlayerDetected()
        {
            if (currentState != EnemyState.Chase)
            {
                ChangeState(EnemyState.Chase);
            }

            lastSeenTargetTime = Time.time;

            if (forgetTargetCoroutine != null)
            {
                StopCoroutine(forgetTargetCoroutine);
            }

            forgetTargetCoroutine = StartCoroutine(ForgetTargetAfterDelay());
        }

        private IEnumerator ForgetTargetAfterDelay()
        {
            while (Time.time - lastSeenTargetTime < loseTargetTime)
            {
                yield return null;
            }

            ChangeState(EnemyState.Search);
            searchStartTime = Time.time;
            forgetTargetCoroutine = null;
        }
        #endregion

        #region Search
        private void HandleSearch()
        {
            agent.speed = patrolSpeed;

            if (Time.time - searchStartTime > timeToForgetTarget)
            {
                ChangeState(EnemyState.Patrol);
                MoveToNextPatrolPoint();
            }
        }
        #endregion

        #region Noise Reaction
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag("NoiseArea"))
            {
                OnPlayerDetected();
            }
        }
        #endregion

        #region State Management
        private void ChangeState(EnemyState newState)
        {
            if (currentState == newState)
                return;

            currentState = newState;

            switch (newState)
            {
                case EnemyState.Chase:
                    agent.speed = chaseSpeed;
                    break;
                case EnemyState.Patrol:
                case EnemyState.Search:
                    agent.speed = patrolSpeed;
                    break;
            }
        }
        #endregion
    }
}
