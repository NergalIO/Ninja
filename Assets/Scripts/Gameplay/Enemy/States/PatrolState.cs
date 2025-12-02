using System.Collections;
using UnityEngine;

namespace Ninja.Gameplay.Enemy
{
    public class PatrolState : EnemyStateBase
    {
        private bool isWaitingAtPoint = false;
        private Coroutine waitCoroutine;
        private Coroutine initializationCoroutine;
        private bool hasSetInitialDestination = false;

        public PatrolState(EnemyStateContext context) : base(context) { }

        public override void Enter()
        {
            if (context.Agent == null || context.PatrolPoints == null || context.PatrolPoints.Length == 0)
                return;

            if (!context.Agent.enabled)
            {
                context.Agent.enabled = true;
            }

            context.Agent.speed = context.PatrolSpeed;
            hasSetInitialDestination = false;

            if (context.Agent.isOnNavMesh)
            {
                MoveToCurrentPatrolPoint();
                hasSetInitialDestination = true;
            }
            else
            {
                if (initializationCoroutine != null && context.CoroutineRunner != null)
                {
                    context.CoroutineRunner.StopCoroutine(initializationCoroutine);
                }

                initializationCoroutine = context.CoroutineRunner.StartCoroutine(InitializePatrol());
            }
        }

        public override void Update()
        {
            if (context.PatrolPoints == null || context.PatrolPoints.Length == 0)
                return;

            if (context.Agent == null || !context.Agent.enabled || !hasSetInitialDestination)
                return;

            if (isWaitingAtPoint)
                return;

            if (!context.Agent.pathPending && context.Agent.hasPath)
            {
                if (context.Agent.remainingDistance < EnemyStateContext.PATROL_DISTANCE_THRESHOLD)
                {
                    StartWaitingAtPoint();
                }
            }
            else if (!context.Agent.pathPending && !context.Agent.hasPath && hasSetInitialDestination)
            {
                MoveToCurrentPatrolPoint();
            }

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

            if (initializationCoroutine != null && context.CoroutineRunner != null)
            {
                context.CoroutineRunner.StopCoroutine(initializationCoroutine);
                initializationCoroutine = null;
            }

            isWaitingAtPoint = false;
            hasSetInitialDestination = false;
        }

        private IEnumerator InitializePatrol()
        {
            yield return null;

            if (context.Agent == null || !context.Agent.enabled)
                yield break;

            if (!context.Agent.isOnNavMesh)
            {
                context.Agent.Warp(context.Transform.position);
                yield return null;

                if (!context.Agent.isOnNavMesh)
                    yield break;
            }

            MoveToCurrentPatrolPoint();
            hasSetInitialDestination = true;
            initializationCoroutine = null;
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

        private void MoveToCurrentPatrolPoint()
        {
            if (context.PatrolPoints == null || context.PatrolPoints.Length == 0 || context.Agent == null)
                return;

            if (context.CurrentPatrolPointIndex < 0 || context.CurrentPatrolPointIndex >= context.PatrolPoints.Length)
                return;

            Transform targetPoint = context.PatrolPoints[context.CurrentPatrolPointIndex];
            if (targetPoint == null)
                return;

            context.Agent.destination = targetPoint.position;
        }

        private void MoveToNextPatrolPoint()
        {
            if (context.PatrolPoints == null || context.PatrolPoints.Length == 0 || context.Agent == null)
                return;

            context.CurrentPatrolPointIndex = (context.CurrentPatrolPointIndex + 1) % context.PatrolPoints.Length;
            
            Transform targetPoint = context.PatrolPoints[context.CurrentPatrolPointIndex];
            if (targetPoint == null)
                return;

            context.Agent.destination = targetPoint.position;
        }
    }
}

