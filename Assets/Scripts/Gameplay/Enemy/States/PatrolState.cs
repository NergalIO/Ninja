using System.Collections;
using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class PatrolState : EnemyStateBase
    {
        private bool isWaitingAtPoint = false;
        private Coroutine waitCoroutine;

        public PatrolState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            context.Agent.speed = context.PatrolSpeed;
            MoveToNextPatrolPoint();
        }

        public override void Update()
        {
            if (context.PatrolPoints == null || context.PatrolPoints.Length == 0)
                return;

            // Если ждем на точке, не проверяем расстояние
            if (isWaitingAtPoint)
                return;

            if (!context.Agent.pathPending && context.Agent.remainingDistance < EnemyStateContext.PATROL_DISTANCE_THRESHOLD)
            {
                StartWaitingAtPoint();
            }

            // Сохраняем позицию патруля для возврата
            if (!context.HasLastPatrolPosition)
            {
                context.LastPatrolPosition = context.Transform.position;
                context.HasLastPatrolPosition = true;
            }
        }

        public override void Exit()
        {
            if (waitCoroutine != null && context.CoroutineRunner != null)
            {
                context.CoroutineRunner.StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }
            isWaitingAtPoint = false;
        }

        private void StartWaitingAtPoint()
        {
            isWaitingAtPoint = true;
            context.Agent.speed = 0f;
            
            if (waitCoroutine != null && context.CoroutineRunner != null)
            {
                context.CoroutineRunner.StopCoroutine(waitCoroutine);
            }

            waitCoroutine = context.CoroutineRunner.StartCoroutine(WaitAtPoint());
        }

        private IEnumerator WaitAtPoint()
        {
            yield return new WaitForSeconds(context.WaitTimeAtPatrolPoint);
            
            isWaitingAtPoint = false;
            context.Agent.speed = context.PatrolSpeed;
            MoveToNextPatrolPoint();
            waitCoroutine = null;
        }

        private void MoveToNextPatrolPoint()
        {
            if (context.PatrolPoints == null || context.PatrolPoints.Length == 0)
                return;

            context.CurrentPatrolPointIndex = (context.CurrentPatrolPointIndex + 1) % context.PatrolPoints.Length;
            context.Agent.destination = context.PatrolPoints[context.CurrentPatrolPointIndex].position;
        }
    }
}

