using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Ninja.Core.Events;

namespace Ninja.Gameplay.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private const float CATCH_DISTANCE = 1.2f;
        private const float MOVE_THRESHOLD = 0.1f;

        [Header("State")]
        [SerializeField] private EnemyState state;

        [Header("Navigation")]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float rotationSpeed = 5f;

        [Header("Player")]
        [SerializeField] private Transform player;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float waitTime = 0.75f;

        [Header("Chase")]
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float loseTargetTime = 3f;

        [Header("Vision")]
        [SerializeField] private FieldOfView fieldOfView;

        [Header("Search")]
        [SerializeField] private float forgetTime = 5f;

        [Header("Investigate")]
        [SerializeField] private float investigateSpeed = 3f;

        [Header("Scan")]
        [SerializeField] private float scanDuration = 3f;

        [Header("Return")]
        [SerializeField] private float returnSpeed = 2f;

        private EnemyStateContext context;
        private EnemyStateBase currentState;
        private EnemyState currentStateType = EnemyState.Patrol;
        private Dictionary<EnemyState, EnemyStateBase> states;
        private bool playerCaught;
        private bool initialized;

        private void Awake()
        {
            InitContext();
            InitStates();
            SetupAgent();
            SetupFieldOfView();
        }

        private void Start()
        {
            if (agent == null || patrolPoints == null || patrolPoints.Length == 0)
                return;

            if (!agent.isOnNavMesh)
            {
                agent.Warp(transform.position);
                StartCoroutine(DelayedStart());
                return;
            }

            ChangeState(EnemyState.Patrol);
        }

        private void FixedUpdate()
        {
            CheckVision();
            UpdateRotation();
            CheckCatch();
            currentState?.Update();
        }

        #region Initialization
        private void SetupAgent()
        {
            if (agent == null) return;
            agent.updateRotation = false;
            agent.enabled = true;
        }

        private void SetupFieldOfView()
        {
            if (fieldOfView != null && player != null)
                fieldOfView.SetTarget(player);
        }

        private void InitContext()
        {
            context = new EnemyStateContext
            {
                Agent = agent,
                RotationSpeed = rotationSpeed,
                Transform = transform,
                Player = player,
                PatrolPoints = patrolPoints,
                PatrolSpeed = patrolSpeed,
                WaitTimeAtPatrolPoint = waitTime,
                ChaseSpeed = chaseSpeed,
                LoseTargetTime = loseTargetTime,
                FieldOfView = fieldOfView,
                TimeToForgetTarget = forgetTime,
                InvestigateSpeed = investigateSpeed,
                ScanDuration = scanDuration,
                ReturnSpeed = returnSpeed,
                CoroutineRunner = this,
                OnStateChange = ChangeState,
                GetCurrentState = () => currentStateType,
                OnPlayerDetected = HandlePlayerDetected,
                MoveToNextPatrolPoint = NextPatrolPoint
            };
        }

        private void InitStates()
        {
            states = new Dictionary<EnemyState, EnemyStateBase>
            {
                { EnemyState.Patrol, new PatrolState(context) },
                { EnemyState.Chase, new ChaseState(context) },
                { EnemyState.Search, new SearchState(context) },
                { EnemyState.Investigate, new InvestigateState(context) },
                { EnemyState.Scan, new ScanState(context) },
                { EnemyState.Return, new ReturnState(context) }
            };
        }

        private IEnumerator DelayedStart()
        {
            yield return null;
            if (agent.isOnNavMesh)
                ChangeState(EnemyState.Patrol);
        }
        #endregion

        #region Core Logic
        private void CheckVision()
        {
            if (fieldOfView != null && fieldOfView.CanSeeTarget)
                HandlePlayerDetected();
        }

        private void UpdateRotation()
        {
            if (agent == null) return;

            var dir = agent.desiredVelocity;
            if (dir.magnitude <= MOVE_THRESHOLD) return;

            float target = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float current = transform.eulerAngles.z;
            float delta = Mathf.DeltaAngle(current, target);
            float newAngle = current + delta * rotationSpeed * Time.fixedDeltaTime;

            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }

        private void CheckCatch()
        {
            if (playerCaught || player == null) return;

            if (Vector3.Distance(transform.position, player.position) < CATCH_DISTANCE)
            {
                playerCaught = true;
                Events.Trigger(GameEvents.PlayerCaught, new PlayerEventArgs(player.position));
            }
        }

        private void NextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;
            context.CurrentPatrolPointIndex = (context.CurrentPatrolPointIndex + 1) % patrolPoints.Length;
            agent.destination = patrolPoints[context.CurrentPatrolPointIndex].position;
        }
        #endregion

        #region Events
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("NoiseArea"))
                HandleNoise(col.transform.position);
        }

        private void HandleNoise(Vector3 pos)
        {
            Events.Trigger(GameEvents.PlayerHeard, new NoiseEventArgs(pos));

            if (currentStateType == EnemyState.Chase)
            {
                HandlePlayerDetected();
                return;
            }

            context.NoisePosition = pos;
            context.HasNoisePosition = true;
            ChangeState(EnemyState.Investigate);
        }

        private void HandlePlayerDetected()
        {
            if (currentStateType == EnemyState.Chase) return;

            var playerPos = player ? player.position : Vector3.zero;
            Events.Trigger(GameEvents.PlayerDetected, new PlayerDetectedEventArgs(playerPos, transform.position, gameObject));
            Events.Trigger(GameEvents.ChaseStarted, new EnemyEventArgs(gameObject));
            ChangeState(EnemyState.Chase);
        }
        #endregion

        #region State Management
        private void ChangeState(EnemyState newState)
        {
            if (currentStateType == newState && initialized) return;

            // Событие окончания погони
            if (currentStateType == EnemyState.Chase && newState != EnemyState.Chase)
                Events.Trigger(GameEvents.ChaseEnded, new EnemyEventArgs(gameObject));

            currentState?.Exit();

            if (states.TryGetValue(newState, out var nextState))
            {
                currentStateType = newState;
                state = newState;
                currentState = nextState;
                initialized = true;
                currentState.Enter();
            }
        }
        #endregion
    }
}
