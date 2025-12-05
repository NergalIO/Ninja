using UnityEngine;
using Ninja.Core.Events;

namespace Ninja.Gameplay.Enemy
{
    /// <summary>
    /// Система обнаружения игрока с учётом расстояния и освещения
    /// </summary>
    public class DetectionSystem : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float maxDetectionTime = 2f;
        [SerializeField] private float detectionDecayRate = 1.5f;
        [SerializeField] private AnimationCurve distanceCurve = AnimationCurve.Linear(0, 2, 1, 0.5f);
        [SerializeField, Range(0f, 1f)] private float alertThreshold = 0.5f;
        
        [Header("Light Modifiers")]
        [SerializeField] private float lightDetectionMultiplier = 1.5f;
        [SerializeField] private float shadowDetectionMultiplier = 0.3f;
        
        [Header("References")]
        [SerializeField] private FieldOfView fieldOfView;
        [SerializeField] private Transform target;
        
        [Header("Debug")]
        [SerializeField] private bool showDebug = false;
        
        private float currentDetection = 0f;
        private bool targetInShadow = false;
        
        /// <summary>
        /// Текущий уровень обнаружения (0-1)
        /// </summary>
        public float DetectionLevel => Mathf.Clamp01(currentDetection / maxDetectionTime);
        
        /// <summary>
        /// Полностью обнаружен (100%)
        /// </summary>
        public bool IsFullyDetected => currentDetection >= maxDetectionTime;
        
        /// <summary>
        /// Достигнут порог настороженности (50%)
        /// </summary>
        public bool IsAlerted => DetectionLevel >= alertThreshold;
        
        /// <summary>
        /// Враг видит цель
        /// </summary>
        public bool CanSeeTarget => fieldOfView != null && fieldOfView.CanSeeTarget;
        
        /// <summary>
        /// Цель в тени
        /// </summary>
        public bool IsTargetInShadow => targetInShadow;
        
        /// <summary>
        /// Событие при полном обнаружении (100%)
        /// </summary>
        public event System.Action OnTargetDetected;
        
        /// <summary>
        /// Событие при достижении порога настороженности (50%)
        /// </summary>
        public event System.Action OnAlertTriggered;
        
        /// <summary>
        /// Событие при потере цели из виду
        /// </summary>
        public event System.Action OnTargetLost;

        private bool wasDetected = false;
        private bool wasAlerted = false;
        private bool wasVisible = false;
        private bool isPaused = false;

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

        private void OnGamePaused(EventArgs e) => isPaused = true;
        private void OnGameResumed(EventArgs e) => isPaused = false;

        public void SetTarget(Transform t)
        {
            target = t;
            if (fieldOfView != null)
                fieldOfView.SetTarget(t);
        }

        public void SetTargetInShadow(bool inShadow)
        {
            targetInShadow = inShadow;
        }

        private void Update()
        {
            if (isPaused || target == null || fieldOfView == null)
                return;

            bool canSee = fieldOfView.CanSeeTarget;

            if (canSee)
            {
                float detectionRate = CalculateDetectionRate();
                currentDetection += detectionRate * Time.deltaTime;
                currentDetection = Mathf.Min(currentDetection, maxDetectionTime);
                
                if (!wasVisible)
                {
                    wasVisible = true;
                }

                // Проверка на Alert (50%)
                if (DetectionLevel >= alertThreshold && !wasAlerted && !wasDetected)
                {
                    wasAlerted = true;
                    OnAlertTriggered?.Invoke();
                    
                    if (showDebug)
                        Debug.Log($"[DetectionSystem] Alert triggered at {DetectionLevel:P0}!");
                }

                // Проверка на полное обнаружение (100%)
                if (currentDetection >= maxDetectionTime && !wasDetected)
                {
                    wasDetected = true;
                    OnTargetDetected?.Invoke();
                    
                    if (showDebug)
                        Debug.Log($"[DetectionSystem] Target fully detected!");
                }
            }
            else
            {
                // Постепенно забываем
                currentDetection -= detectionDecayRate * Time.deltaTime;
                currentDetection = Mathf.Max(0, currentDetection);
                
                if (wasVisible)
                {
                    wasVisible = false;
                    OnTargetLost?.Invoke();
                }

                // Сброс флагов когда обнаружение упало
                if (currentDetection <= 0)
                {
                    wasDetected = false;
                    wasAlerted = false;
                }
            }

            // Обновляем цвет FOV
            if (fieldOfView != null)
            {
                fieldOfView.SetDetectionLevel(DetectionLevel);
            }

            if (showDebug && canSee)
            {
                Debug.Log($"[DetectionSystem] Detection: {DetectionLevel:P0}, InShadow: {targetInShadow}");
            }
        }

        private float CalculateDetectionRate()
        {
            float distance = Vector2.Distance(transform.position, target.position);
            float normalizedDistance = Mathf.Clamp01(distance / fieldOfView.ViewRadius);
            
            // Базовая скорость обнаружения из кривой (ближе = быстрее)
            float baseRate = distanceCurve.Evaluate(normalizedDistance);
            
            // Модификатор освещения
            float lightModifier = targetInShadow ? shadowDetectionMultiplier : lightDetectionMultiplier;
            
            return baseRate * lightModifier;
        }

        /// <summary>
        /// Сбросить обнаружение
        /// </summary>
        public void ResetDetection()
        {
            currentDetection = 0f;
            wasDetected = false;
            wasAlerted = false;
            wasVisible = false;
            
            if (fieldOfView != null)
                fieldOfView.SetDetectionLevel(0);
        }

        /// <summary>
        /// Мгновенно обнаружить (например при шуме)
        /// </summary>
        public void InstantDetect()
        {
            currentDetection = maxDetectionTime;
            
            if (!wasDetected)
            {
                wasDetected = true;
                OnTargetDetected?.Invoke();
            }
            
            if (fieldOfView != null)
                fieldOfView.SetDetectionLevel(1f);
        }

        /// <summary>
        /// Обработка шума - обнаружение подскакивает до порога настороженности (50%)
        /// </summary>
        public void OnNoiseHeard(Vector3 noisePosition)
        {
            // Подскакиваем до 50% если ниже
            float alertLevel = maxDetectionTime * alertThreshold;
            if (currentDetection < alertLevel)
            {
                currentDetection = alertLevel;
                
                if (!wasAlerted)
                {
                    wasAlerted = true;
                    OnAlertTriggered?.Invoke();
                    
                    if (showDebug)
                        Debug.Log($"[DetectionSystem] Alert triggered by noise at {noisePosition}!");
                }
                
                if (fieldOfView != null)
                    fieldOfView.SetDetectionLevel(DetectionLevel);
            }
        }
    }
}
