using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class ScanState : EnemyStateBase
    {
        public ScanState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            context.Agent.speed = 0f;
            context.ScanStartTime = Time.time;
        }

        public override void Update()
        {
            // Проверяем видимость игрока во время сканирования
            if (context.FieldOfView != null && context.FieldOfView.CanSeeTarget)
            {
                context.OnPlayerDetected?.Invoke();
                return;
            }

            // Если время сканирования истекло, возвращаемся к патрулю
            if (Time.time - context.ScanStartTime > context.ScanDuration)
            {
                context.OnStateChange?.Invoke(EnemyState.Return);
            }
        }

        public override void Exit() { }
    }
}

