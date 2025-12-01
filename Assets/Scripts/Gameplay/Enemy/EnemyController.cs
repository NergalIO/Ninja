using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Ninja.Gameplay.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private NavMeshAgent agent;

        [Header("Player")]
        [SerializeField] private Transform player;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        private int currentPoint = 0;
        [SerializeField] private float patrolSpeed = 2f;

        [Header("Chase")]
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float loseTargetTime = 3f;
        private float lastSeenTarget;

        [Header("View Settings")]
        [SerializeField] private FieldOfView fov;
        [SerializeField] private float viewRadius = 6f;
        [SerializeField] private float viewAngle = 60f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstructionMask;

        [Header("Search")]
        [SerializeField] private float timeToForgetTarget = 5f;
        private float startedSearchingTime;

        private EnemyState state = EnemyState.Patrol;
        private Coroutine forgetCoroutine;

        private void Awake()
        {
            targetMask = LayerMask.GetMask("Target");
            obstructionMask = LayerMask.GetMask("Obstruction");
            fov.player = player;
        }

        private void FixedUpdate()
        {
            switch (state)
            {
                case EnemyState.Patrol:
                    Patrol();
                    LookForPlayer();
                    break;

                case EnemyState.Chase:
                    ChasePlayer();
                    LookForPlayer();
                    break;

                case EnemyState.Search:
                    SearchPlayer();
                    LookForPlayer();
                    break;
            }
        }

        #region Patrol
        private void Patrol()
        {
            agent.speed = patrolSpeed;

            if (patrolPoints.Length == 0)
                return;

            if (!agent.pathPending && agent.remainingDistance < 0.25f)
            {
                GoToNextPatrolPoint();
            }
        }

        private void GoToNextPatrolPoint()
        {
            agent.destination = patrolPoints[currentPoint].position;
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
        }
        #endregion

        #region Vision
        /// <summary>
        /// Проверяет, находится ли игрок в зоне FOV.
        /// </summary>
        private void LookForPlayer()
        {
            if (fov.canSeePlayer)
                OnFoundPlayer();
            return;

            if (player == null) return;

            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            if (Vector3.Distance(transform.position, player.position) > viewRadius)
                return;

            if (Vector3.Angle(transform.forward, dirToPlayer) > viewAngle / 2f)
                return;

            if (Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, viewRadius))
            {
                if (((1 << hit.collider.gameObject.layer) & obstructionMask) != 0)
                    return;
            }

            OnFoundPlayer();
        }
        #endregion

        #region Chase
        private void ChasePlayer()
        {
            if (player == null) return;

            agent.speed = chaseSpeed;
            agent.destination = player.position;
        }

        private void OnFoundPlayer()
        {
            if (state != EnemyState.Chase)
            {
                state = EnemyState.Chase;
                agent.speed = chaseSpeed;
            }

            lastSeenTarget = Time.time;

            if (forgetCoroutine != null)
                StopCoroutine(forgetCoroutine);

            forgetCoroutine = StartCoroutine(ForgetPlayerAfterDelay());
        }

        private IEnumerator ForgetPlayerAfterDelay()
        {
            while (Time.time - lastSeenTarget < loseTargetTime)
                yield return null;

            state = EnemyState.Search;
            startedSearchingTime = Time.time;
            forgetCoroutine = null;
        }
        #endregion

        #region Noise Reaction

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag("NoiseArea"))
            {
                OnFoundPlayer();
            }
        }

        #endregion

        #region Search
        private void SearchPlayer()
        {
            agent.speed = patrolSpeed;

            if (Time.time - startedSearchingTime > timeToForgetTarget)
            {
                state = EnemyState.Patrol;
                GoToNextPatrolPoint();
            }
        }
        #endregion
    }
}
