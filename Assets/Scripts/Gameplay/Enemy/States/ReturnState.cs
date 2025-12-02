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
            if (context.HasLastPatrolPosition)
            {
                context.Agent.destination = context.LastPatrolPosition;

                if (!context.Agent.pathPending && context.Agent.remainingDistance < EnemyStateContext.PATROL_DISTANCE_THRESHOLD)
                {
                    context.HasLastPatrolPosition = false;
                    context.MoveToNextPatrolPoint?.Invoke();
                    context.OnStateChange?.Invoke(EnemyState.Patrol);
                }
            }
            else
            {
                context.MoveToNextPatrolPoint?.Invoke();
                context.OnStateChange?.Invoke(EnemyState.Patrol);
            }
        }

        public override void Exit() { }
    }
}

