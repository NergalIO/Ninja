using System.Collections;
using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class ChaseState : EnemyStateBase
    {
        private Coroutine loseTargetCoroutine;

        public ChaseState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            ctx.Agent.speed = ctx.ChaseSpeed;
            ctx.LastSeenTime = Time.time;
            loseTargetCoroutine = ctx.CoroutineRunner.StartCoroutine(CheckLoseTarget());
        }

        public override void Update()
        {
            if (ctx.Player == null) return;

            ctx.Agent.destination = ctx.Player.position;
            ctx.LastSeenTime = Time.time;
        }

        public override void Exit()
        {
            if (loseTargetCoroutine != null)
                ctx.CoroutineRunner.StopCoroutine(loseTargetCoroutine);
        }

        private IEnumerator CheckLoseTarget()
        {
            while (Time.time - ctx.LastSeenTime < ctx.LoseTargetTime)
                yield return null;

            ctx.SearchStartTime = Time.time;
            ctx.ChangeState?.Invoke(EnemyState.Search);
        }
    }
}
