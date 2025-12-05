using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class SearchState : EnemyStateBase
    {
        public SearchState(EnemyStateContext context) : base(context) { }

        public override void Enter() => ctx.Agent.speed = ctx.PatrolSpeed;

        public override void Update()
        {
            if (Time.time - ctx.SearchStartTime > ctx.ForgetTime)
                ctx.ChangeState?.Invoke(EnemyState.Return);
        }
    }
}
