using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class ReturnState : EnemyStateBase
    {
        public ReturnState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            context.Agent.speed = context.ReturnSpeed;
        }

        public override void Update()
        {
            // Если есть сохраненная позиция патруля, идем к ней
            if (context.HasLastPatrolPosition)
            {
                context.Agent.destination = context.LastPatrolPosition;

                // Если достигли позиции, возвращаемся к патрулю
                if (!context.Agent.pathPending && context.Agent.remainingDistance < EnemyStateContext.PATROL_DISTANCE_THRESHOLD)
                {
                    context.HasLastPatrolPosition = false;
                    context.MoveToNextPatrolPoint?.Invoke();
                    context.OnStateChange?.Invoke(EnemyState.Patrol);
                }
            }
            else
            {
                // Если нет сохраненной позиции, просто переходим к патрулю
                context.MoveToNextPatrolPoint?.Invoke();
                context.OnStateChange?.Invoke(EnemyState.Patrol);
            }
        }

        public override void Exit() { }
    }
}

