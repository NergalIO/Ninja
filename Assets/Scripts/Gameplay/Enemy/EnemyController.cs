using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Ninja.Systems;

namespace Ninja.Gameplay.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private const float CATCH_DISTANCE_THRESHOLD = 1.2f;
        private const float MOVEMENT_THRESHOLD = 0.1f;

        [Header("State Machine")]
        [SerializeField] private EnemyState state;
        
        [Header("Navigation")]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float rotationSpeed = 5f;

        [Header("Player")]
        [SerializeField] private Transform player;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float waitTimeAtPatrolPoint = 0.75f;

        [Header("Chase")]
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float loseTargetTime = 3f;

        [Header("View Settings")]
        [SerializeField] private FieldOfView fieldOfView;

        [Header("Search")]
        [SerializeField] private float timeToForgetTarget = 5f;

        [Header("Investigate")]
        [SerializeField] private float investigateSpeed = 3f;

        [Header("Scan")]
        [SerializeField] private float scanDuration = 3f;

        [Header("Return")]
        [SerializeField] private float returnSpeed = 2f;

        private EnemyStateContext context;
        private EnemyStateBase currentState;
        private EnemyState currentStateType = EnemyState.Patrol;
        private bool playerCaughtTriggered = false;
        private bool hasInitializedState = false;
        private Dictionary<EnemyState, EnemyStateBase> stateInstances;

        private void Awake()
        {
            InitializeContext();
            InitializeStates();
            
            if (agent != null)
            {
                agent.updateRotation = false;
                agent.enabled = true;
            }

            if (fieldOfView != null && player != null)
            {
                fieldOfView.SetTarget(player);
            }
        }

        private void Start()
        {
            if (agent == null || patrolPoints == null || patrolPoints.Length == 0)
                return;

            if (!agent.isOnNavMesh)
            {
                agent.Warp(transform.position);
                StartCoroutine(DelayedPatrolStart());
                return;
            }

            ChangeState(EnemyState.Patrol);
        }

        private System.Collections.IEnumerator DelayedPatrolStart()
        {
            yield return null;
            if (agent.isOnNavMesh)
            {
                ChangeState(EnemyState.Patrol);
            }
        }

        private void InitializeContext()
        {
            context = new EnemyStateContext
            {
                Agent = agent,
                RotationSpeed = rotationSpeed,
                Transform = transform,
                Player = player,
                PatrolPoints = patrolPoints,
                PatrolSpeed = patrolSpeed,
                CurrentPatrolPointIndex = 0,
                WaitTimeAtPatrolPoint = waitTimeAtPatrolPoint,
                ChaseSpeed = chaseSpeed,
                LoseTargetTime = loseTargetTime,
                FieldOfView = fieldOfView,
                TimeToForgetTarget = timeToForgetTarget,
                InvestigateSpeed = investigateSpeed,
                ScanDuration = scanDuration,
                ReturnSpeed = returnSpeed,
                CoroutineRunner = this,
                OnStateChange = ChangeState,
                GetCurrentState = () => currentStateType,
                OnPlayerDetected = OnPlayerDetected,
                MoveToNextPatrolPoint = MoveToNextPatrolPoint
            };
        }

        private void MoveToNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return;

            context.CurrentPatrolPointIndex = (context.CurrentPatrolPointIndex + 1) % patrolPoints.Length;
            agent.destination = patrolPoints[context.CurrentPatrolPointIndex].position;
        }

        private void InitializeStates()
        {
            stateInstances = new Dictionary<EnemyState, EnemyStateBase>
            {
                { EnemyState.Patrol, new PatrolState(context) },
                { EnemyState.Chase, new ChaseState(context) },
                { EnemyState.Search, new SearchState(context) },
                { EnemyState.Investigate, new InvestigateState(context) },
                { EnemyState.Scan, new ScanState(context) },
                { EnemyState.Return, new ReturnState(context) }
            };
        }

        private void FixedUpdate()
        {
            CheckForPlayer();
            RotateTowardsMovement();
            CheckPlayerCatch();
            currentState?.Update();
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

        #region Rotation
        private void RotateTowardsMovement()
        {
            if (agent == null)
                return;

            Vector3 direction = agent.desiredVelocity;

            if (direction.magnitude > MOVEMENT_THRESHOLD)
            {
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
                float newAngle = currentAngle + angleDifference * rotationSpeed * Time.fixedDeltaTime;
                
                Vector3 currentEuler = transform.eulerAngles;
                transform.rotation = Quaternion.Euler(currentEuler.x, currentEuler.y, newAngle);
            }
        }
        #endregion

        #region Noise Reaction
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag("NoiseArea"))
            {
                OnNoiseDetected(collider.transform.position);
            }
        }

        private void OnNoiseDetected(Vector3 noisePos)
        {
            GameManager.Instance?.NotifyPlayerHeard(noisePos);

            if (currentStateType == EnemyState.Chase)
            {
                OnPlayerDetected();
                return;
            }

            context.NoisePosition = noisePos;
            context.HasNoisePosition = true;
            ChangeState(EnemyState.Investigate);
        }
        #endregion

        #region State Management
        private void OnPlayerDetected()
        {
            if (currentStateType != EnemyState.Chase)
            {
                GameManager.Instance?.NotifyPlayerFound();
                ChangeState(EnemyState.Chase);
            }
        }

        private void CheckPlayerCatch()
        {
            if (playerCaughtTriggered || player == null)
                return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer < CATCH_DISTANCE_THRESHOLD)
            {
                playerCaughtTriggered = true;
                GameManager.Instance?.NotifyPlayerCatched();
            }
        }

        private void ChangeState(EnemyState newState)
        {
            if (currentStateType == newState && hasInitializedState)
                return;

            currentState?.Exit();
            currentStateType = newState;

            if (stateInstances != null && stateInstances.TryGetValue(newState, out EnemyStateBase nextState))
            {
                currentState = nextState;
            }
            else
            {
                return;
            }

            state = newState;
            hasInitializedState = true;
            currentState?.Enter();
        }
        #endregion
    }
}
