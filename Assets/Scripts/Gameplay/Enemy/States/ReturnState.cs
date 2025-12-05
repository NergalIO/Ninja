namespace Ninja.Gameplay.Enemy
{
    public class ReturnState : EnemyStateBase
    {
        public ReturnState(EnemyStateContext context) : base(context) { }

        public override void Enter() => ctx.Agent.speed = ctx.ReturnSpeed;

        public override void Update()
        {
            if (!ctx.HasLastPatrolPosition)
            {
                ctx.NextPatrolPoint();
                ctx.ChangeState?.Invoke(EnemyState.Patrol);
                return;
            }

            ctx.Agent.destination = ctx.LastPatrolPosition;

            if (ctx.ReachedDestination())
            {
                ctx.HasLastPatrolPosition = false;
                ctx.NextPatrolPoint();
                ctx.ChangeState?.Invoke(EnemyState.Patrol);
            }
        }
    }
}
