using UnityEngine;
using UnityEngine.AI;
using System;


namespace Ninja.Gameplay.Enemy
{
    [System.Serializable]
    public abstract class EnemyStateBase
    {
        protected EnemyStateContext context;

        public EnemyStateBase(EnemyStateContext context)
        {
            this.context = context;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }
}

