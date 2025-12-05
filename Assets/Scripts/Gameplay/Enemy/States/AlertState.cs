using System.Collections;
using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    /// <summary>
    /// Состояние настороженности - враг заметил что-то подозрительное
    /// </summary>
    public class AlertState : EnemyStateBase
    {
        private Coroutine alertCoroutine;
        private Vector3 lastKnownPlayerPosition;
        private bool isLookingAround;

        public AlertState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            ctx.Agent.speed = 0f;
            ctx.Agent.ResetPath();
            
            // Запоминаем позицию игрока или шума
            if (ctx.HasNoisePosition)
                lastKnownPlayerPosition = ctx.NoisePosition;
            else if (ctx.Player != null)
                lastKnownPlayerPosition = ctx.Player.position;
            
            // Сразу поворачиваемся к позиции
            LookAtPosition(lastKnownPlayerPosition);
            
            isLookingAround = false;
            alertCoroutine = ctx.CoroutineRunner.StartCoroutine(AlertRoutine());
        }

        public override void Update()
        {
            // Проверяем полное обнаружение
            if (ctx.DetectionSystem != null && ctx.DetectionSystem.IsFullyDetected)
            {
                ctx.OnPlayerDetected?.Invoke();
                return;
            }

            // Поворачиваемся к игроку если видим, иначе к последней известной позиции
            if (ctx.DetectionSystem != null && ctx.DetectionSystem.CanSeeTarget && ctx.Player != null)
            {
                lastKnownPlayerPosition = ctx.Player.position;
                LookAtPosition(ctx.Player.position);
            }
            else if (!isLookingAround)
            {
                // Плавно поворачиваемся к позиции шума/последней позиции игрока
                LookAtPosition(lastKnownPlayerPosition);
            }
        }

        public override void Exit()
        {
            if (alertCoroutine != null)
                ctx.CoroutineRunner.StopCoroutine(alertCoroutine);
            
            ctx.HasNoisePosition = false;
        }

        private IEnumerator AlertRoutine()
        {
            // Ждём 3 секунды, смотря на игрока
            float waitTime = 3f;
            float elapsed = 0f;
            
            while (elapsed < waitTime)
            {
                // Если полностью обнаружили - выходим
                if (ctx.DetectionSystem != null && ctx.DetectionSystem.IsFullyDetected)
                    yield break;
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Если всё ещё в состоянии Alert и не обнаружили полностью
            if (ctx.DetectionSystem == null || !ctx.DetectionSystem.IsFullyDetected)
            {
                // Идём к последней известной позиции
                isLookingAround = true;
                ctx.Agent.speed = ctx.InvestigateSpeed;
                ctx.Agent.destination = lastKnownPlayerPosition;

                // Ждём пока дойдём
                while (ctx.Agent.pathPending || ctx.Agent.remainingDistance > EnemyStateContext.DISTANCE_THRESHOLD)
                {
                    if (ctx.DetectionSystem != null && ctx.DetectionSystem.IsFullyDetected)
                        yield break;
                    yield return null;
                }

                // Осматриваемся 3 секунды
                ctx.Agent.speed = 0f;
                yield return LookAroundRoutine(3f);

                // Возвращаемся к патрулю
                ctx.ChangeState?.Invoke(EnemyState.Return);
            }
        }

        private IEnumerator LookAroundRoutine(float duration)
        {
            float elapsed = 0f;
            float startAngle = ctx.Transform.eulerAngles.z;
            
            while (elapsed < duration)
            {
                if (ctx.DetectionSystem != null && ctx.DetectionSystem.IsFullyDetected)
                    yield break;

                // Поворачиваемся влево-вправо
                float angle = startAngle + Mathf.Sin(elapsed * 2f) * 45f;
                ctx.Transform.rotation = Quaternion.Euler(0, 0, angle);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void LookAtPosition(Vector3 position)
        {
            Vector2 direction = (position - ctx.Transform.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentAngle = ctx.Transform.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, 180f * Time.deltaTime);
            ctx.Transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
    }
}
