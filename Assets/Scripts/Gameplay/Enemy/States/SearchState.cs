using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class SearchState : EnemyStateBase
    {
        public SearchState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            context.Agent.speed = context.PatrolSpeed;
        }

        public override void Update()
        {
            if (Time.time - context.SearchStartTime > context.TimeToForgetTarget)
            {
                context.OnStateChange?.Invoke(EnemyState.Return);
            }
        }

        public override void Exit() { }
    }
}

