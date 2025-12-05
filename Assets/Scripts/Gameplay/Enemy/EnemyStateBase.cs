namespace Ninja.Gameplay.Enemy
{
    public abstract class EnemyStateBase
    {
        protected EnemyStateContext ctx;

        protected EnemyStateBase(EnemyStateContext context) => ctx = context;

        public abstract void Enter();
        public abstract void Update();
        public virtual void Exit() { }
    }
}
