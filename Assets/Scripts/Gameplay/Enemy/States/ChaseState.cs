using System.Collections;
using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class ChaseState : EnemyStateBase
    {
        private Coroutine forgetTargetCoroutine;

        public ChaseState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            context.Agent.speed = context.ChaseSpeed;
            context.LastSeenTargetTime = Time.time;

            if (forgetTargetCoroutine != null && context.CoroutineRunner != null)
            {
                context.CoroutineRunner.StopCoroutine(forgetTargetCoroutine);
            }

            forgetTargetCoroutine = context.CoroutineRunner.StartCoroutine(ForgetTargetAfterDelay());
        }

        public override void Update()
        {
            if (context.Player == null)
                return;

            context.Agent.destination = context.Player.position;
            context.LastSeenTargetTime = Time.time;
        }

        public override void Exit()
        {
            if (forgetTargetCoroutine != null && context.CoroutineRunner != null)
            {
                context.CoroutineRunner.StopCoroutine(forgetTargetCoroutine);
            }
        }

        private IEnumerator ForgetTargetAfterDelay()
        {
            while (Time.time - context.LastSeenTargetTime < context.LoseTargetTime)
            {
                yield return null;
            }

            context.SearchStartTime = Time.time;
            context.OnStateChange?.Invoke(EnemyState.Search);
        }
    }
}

