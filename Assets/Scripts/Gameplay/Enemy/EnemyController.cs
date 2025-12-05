using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Ninja.Core.Events;
using Ninja.Gameplay.Environment;
using Ninja.Gameplay.Player;

namespace Ninja.Gameplay.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private const float CATCH_DISTANCE = 1.2f;
        private const float MOVE_THRESHOLD = 0.1f;

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
        [SerializeField] private DetectionSystem detectionSystem;

        [Header("Search")]
        [SerializeField] private float forgetTime = 5f;

        [Header("Investigate")]
        [SerializeField] private float investigateSpeed = 3f;

        [Header("Scan")]
        [SerializeField] private float scanDuration = 3f;

        [Header("Return")]
        [SerializeField] private float returnSpeed = 2f;

        private EnemyStateContext context;
        private Dictionary<EnemyState, EnemyStateBase> states;
        private EnemyStateBase currentState;
        private EnemyState currentStateType = EnemyState.Patrol;
        private bool playerCaught;
        private bool isPaused;

        public float DetectionLevel => detectionSystem != null ? detectionSystem.DetectionLevel : 0f;

        private void Awake()
        {
            SetupAgent();
            SetupDetection();
            InitContext();
            InitStates();
        }

        private void OnEnable()
        {
            Events.Subscribe(GameEvents.GamePaused, OnGamePaused);
            Events.Subscribe(GameEvents.GameResumed, OnGameResumed);
        }

        private void OnDisable()
        {
            Events.Unsubscribe(GameEvents.GamePaused, OnGamePaused);
            Events.Unsubscribe(GameEvents.GameResumed, OnGameResumed);
        }

        private void OnGamePaused(EventArgs e)
        {
            isPaused = true;
            if (agent != null && agent.isOnNavMesh)
                agent.isStopped = true;
        }

        private void OnGameResumed(EventArgs e)
        {
            isPaused = false;
            if (agent != null && agent.isOnNavMesh)
                agent.isStopped = false;
        }
        
        private void SetupDetection()
        {
            if (fieldOfView != null && player != null)
                fieldOfView.SetTarget(player);
            
            // Создаём DetectionSystem если не назначен
            if (detectionSystem == null)
                detectionSystem = GetComponent<DetectionSystem>();
            
            if (detectionSystem != null)
            {
                detectionSystem.SetTarget(player);
                detectionSystem.OnAlertTriggered += HandlePlayerAlert;
                detectionSystem.OnTargetDetected += HandlePlayerDetected;
            }
        }

        private void HandlePlayerAlert()
        {
            // Переходим в состояние Alert только если не в погоне
            if (currentStateType != EnemyState.Chase && currentStateType != EnemyState.Alert)
            {
                ChangeState(EnemyState.Alert);
            }
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

        private void Update()
        {
            if (isPaused) return;
            
            if (detectionSystem != null && player != null)
                detectionSystem.SetTargetInShadow(ShadowZone.IsInShadow(player));

            UpdateRotation();
            CheckCatch();
            currentState?.Update();
        }

        private void SetupAgent()
        {
            if (agent == null) return;
            agent.updateRotation = false;
            agent.enabled = true;
        }

        private void InitContext()
        {
            context = new EnemyStateContext
            {
                Agent = agent,
                Transform = transform,
                Player = player,
                PatrolPoints = patrolPoints,
                PatrolSpeed = patrolSpeed,
                WaitTime = waitTime,
                ChaseSpeed = chaseSpeed,
                LoseTargetTime = loseTargetTime,
                FieldOfView = fieldOfView,
                DetectionSystem = detectionSystem,
                ForgetTime = forgetTime,
                InvestigateSpeed = investigateSpeed,
                ScanDuration = scanDuration,
                ReturnSpeed = returnSpeed,
                CoroutineRunner = this,
                ChangeState = ChangeState,
                OnPlayerDetected = HandlePlayerDetected
            };
        }

        private void InitStates()
        {
            states = new Dictionary<EnemyState, EnemyStateBase>
            {
                { EnemyState.Patrol, new PatrolState(context) },
                { EnemyState.Alert, new AlertState(context) },
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

        private void UpdateRotation()
        {
            if (agent == null) return;

            var dir = agent.desiredVelocity;
            if (dir.magnitude <= MOVE_THRESHOLD) return;

            float target = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float newAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, target, rotationSpeed * Time.deltaTime * 60f);
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

        private void OnTriggerEnter2D(Collider2D col) => HandleNoiseTrigger(col);
        
        private void OnTriggerStay2D(Collider2D col) => HandleNoiseTrigger(col);

        private void HandleNoiseTrigger(Collider2D col)
        {
            if (!col.CompareTag("NoiseArea")) return;
            
            // Уже в погоне или Alert - не реагируем повторно
            if (currentStateType == EnemyState.Chase || currentStateType == EnemyState.Alert)
                return;

            // Получаем NoiseController и проверяем слышимость
            var noiseController = col.GetComponentInParent<NoiseController>();
            if (noiseController != null && !noiseController.CanBeHeardAt(transform.position))
                return;
            
            var pos = col.transform.position;
            Events.Trigger(GameEvents.PlayerHeard, new NoiseEventArgs(pos));

            context.NoisePosition = pos;
            context.HasNoisePosition = true;
            
            // Шум повышает уровень обнаружения до 50% и переводит в Alert
            if (detectionSystem != null)
            {
                detectionSystem.OnNoiseHeard(pos);
                ChangeState(EnemyState.Alert);
            }
            else
            {
                ChangeState(EnemyState.Investigate);
            }
        }

        private void HandlePlayerDetected()
        {
            if (currentStateType == EnemyState.Chase) return;

            var playerPos = player ? player.position : Vector3.zero;
            Events.Trigger(GameEvents.PlayerDetected, new PlayerDetectedEventArgs(playerPos, transform.position, gameObject));
            Events.Trigger(GameEvents.ChaseStarted, new EnemyEventArgs(gameObject));
            ChangeState(EnemyState.Chase);
        }

        private void ChangeState(EnemyState newState)
        {
            if (currentStateType == newState && currentState != null) return;

            if (currentStateType == EnemyState.Chase && newState != EnemyState.Chase)
                Events.Trigger(GameEvents.ChaseEnded, new EnemyEventArgs(gameObject));

            currentState?.Exit();

            if (states.TryGetValue(newState, out var nextState))
            {
                currentStateType = newState;
                currentState = nextState;
                currentState.Enter();
            }
        }
    }
}
