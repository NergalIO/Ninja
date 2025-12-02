using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class InvestigateState : EnemyStateBase
    {
        public InvestigateState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            context.Agent.speed = context.InvestigateSpeed;
        }

        public override void Update()
        {
            if (!context.HasNoisePosition)
            {
                context.OnStateChange?.Invoke(EnemyState.Return);
                return;
            }

            context.Agent.destination = context.NoisePosition;

            if (!context.Agent.pathPending && context.Agent.remainingDistance < EnemyStateContext.PATROL_DISTANCE_THRESHOLD)
            {
                context.OnStateChange?.Invoke(EnemyState.Scan);
            }
        }

        public override void Exit() { }
    }
}

