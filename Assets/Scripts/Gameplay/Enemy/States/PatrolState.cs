using System.Collections;
using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class PatrolState : EnemyStateBase
    {
        private Coroutine waitCoroutine;
        private bool isWaiting;

        public PatrolState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            if (ctx.Agent == null || ctx.PatrolPoints == null || ctx.PatrolPoints.Length == 0)
                return;

            ctx.Agent.speed = ctx.PatrolSpeed;
            MoveToCurrentPoint();
        }

        public override void Update()
        {
            if (ctx.PatrolPoints == null || ctx.PatrolPoints.Length == 0 || isWaiting)
                return;

            if (ctx.ReachedDestination())
            {
                isWaiting = true;
                ctx.Agent.speed = 0f;
                waitCoroutine = ctx.CoroutineRunner.StartCoroutine(WaitAndMove());
            }

            ctx.LastPatrolPosition = ctx.Transform.position;
            ctx.HasLastPatrolPosition = true;
        }

        public override void Exit()
        {
            if (waitCoroutine != null)
                ctx.CoroutineRunner.StopCoroutine(waitCoroutine);
            isWaiting = false;
        }

        private IEnumerator WaitAndMove()
        {
            yield return new WaitForSeconds(ctx.WaitTime);
            isWaiting = false;
            ctx.Agent.speed = ctx.PatrolSpeed;
            ctx.NextPatrolPoint();
        }

        private void MoveToCurrentPoint()
        {
            if (ctx.CurrentPatrolIndex >= ctx.PatrolPoints.Length)
                ctx.CurrentPatrolIndex = 0;
            ctx.Agent.destination = ctx.PatrolPoints[ctx.CurrentPatrolIndex].position;
        }
    }
}
