using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
namespace Ninja.Gameplay.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Navigation Preferences")]
        [SerializeField] private NavMeshAgent agent;

        [Header("Player")]
        [SerializeField] private Transform playerPosition;

        [Header("Speed Preferences")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float shaseSpeed = 4f;

        [Header("Patrol Preferences")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private int currentPoint = 0;
        [SerializeField] private float viewDistance = 6f;
        [SerializeField] private float radius;
        [SerializeField] private float angle;
        [SerializeField] private float waitAtPoint = 2f;
        [SerializeField] private float waitTimer = 0f;

        [Header("Layer Masks")]
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstructionMask;

        [Header("Chase Preferences")]
        [SerializeField] private GameObject foundTarget;
        [SerializeField] private float loseTargetTime = 3f;
        [SerializeField] private float lastSeenTarget;

        [Header("Seach Preferences")]
        [SerializeField] private Transform[] searchPoints;
        [SerializeField] private float timeToForgetTarget = 5f;
        [SerializeField] private float startedSeachingTime;

        private EnemyState state = EnemyState.Patrol;
        private Coroutine ForgotenAfterCoroutine;

        public void Awake()
        {
            targetMask = LayerMask.GetMask("Target");
            obstructionMask = LayerMask.GetMask("Obstruction");
        }

        public void FixedUpdate()
        {
            switch (state)
            {
                case EnemyState.Patrol:
                    Patrol();
                    LookForPlayer();
                    break;
                case EnemyState.Chase:
                    ChasePlayer();
                    break;
                case EnemyState.Search:
                    SearchPlayer();
                    LookForPlayer();
                    break;
            }
        }

        private void Patrol()
        {
            agent.speed = patrolSpeed;

            if (!agent.pathPending && agent.remainingDistance < 0.25f)
            {
                waitTimer += Time.deltaTime;

                if (waitTimer >= waitAtPoint)
                {
                    GoToNextPatrolPoint();
                    waitTimer = 0f;
                }
            }
        }

        private void GoToNextPatrolPoint()
        {
            if (patrolPoints.Length == 0)
                return;
            
            agent.destination = patrolPoints[currentPoint].position;
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
        }

        private void SearchPlayer()
        {
            agent.speed = patrolSpeed;

            if (Time.fixedTime - lastSeenTarget > loseTargetTime)
            {
                state = EnemyState.Patrol;
                GoToNextPatrolPoint();
            }
            LookForPlayer();
        }

        private void LookForPlayer()
        {
            Collider2D[] rangeChecks = Physics2D.OverlapCircleAll(transform.position, radius, targetMask);
            foreach (var check in rangeChecks)
            {
                Debug.Log(check);
            }
            //OnFoundTarget();
        }

        private void ChasePlayer()
        {
            foundTarget = playerPosition.gameObject;
            agent.destination = foundTarget.transform.position;
        }

        private void OnFoundTarget()
        {
            state = EnemyState.Chase;
            agent.speed = shaseSpeed;
            agent.destination = foundTarget.transform.position;
            lastSeenTarget = Time.fixedTime;
        }

        private IEnumerator ForgotenAfter()
        {
            while (Time.fixedTime - lastSeenTarget < loseTargetTime)
            {
                yield return null;
            }
            foundTarget = null;
            state = EnemyState.Patrol;
            ForgotenAfterCoroutine = null;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("NoiseArea"))
            {
                foundTarget = collision.gameObject;
                OnFoundTarget();
                StartCoroutine(ForgotenAfter());
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
