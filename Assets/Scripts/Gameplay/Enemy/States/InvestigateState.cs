namespace Ninja.Gameplay.Enemy
{
    public class InvestigateState : EnemyStateBase
    {
        public InvestigateState(EnemyStateContext context) : base(context) { }

        public override void Enter() => ctx.Agent.speed = ctx.InvestigateSpeed;

        public override void Update()
        {
            if (!ctx.HasNoisePosition)
            {
                ctx.ChangeState?.Invoke(EnemyState.Return);
                return;
            }

            ctx.Agent.destination = ctx.NoisePosition;

            if (ctx.ReachedDestination())
                ctx.ChangeState?.Invoke(EnemyState.Scan);
        }
    }
}
