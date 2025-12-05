using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class ScanState : EnemyStateBase
    {
        public ScanState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            ctx.Agent.speed = 0f;
            ctx.ScanStartTime = Time.time;
        }

        public override void Update()
        {
            // Используем DetectionSystem если есть, иначе прямую проверку
            if (ctx.DetectionSystem != null)
            {
                if (ctx.DetectionSystem.IsFullyDetected)
                {
                    ctx.OnPlayerDetected?.Invoke();
                    return;
                }
            }
            else if (ctx.FieldOfView != null && ctx.FieldOfView.CanSeeTarget)
            {
                ctx.OnPlayerDetected?.Invoke();
                return;
            }

            if (Time.time - ctx.ScanStartTime > ctx.ScanDuration)
                ctx.ChangeState?.Invoke(EnemyState.Return);
        }
    }
}
