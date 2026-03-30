using System.Collections.Generic;
using UnityEngine;

namespace Util_Patten.FSM
{
    public abstract class Context
    {
        
    }

    public abstract class StateMachine<T, TState> : MonoBehaviour 
        where T : Context
        where TState : StateSO<T>
    {
        [Header("FSM Settings")]
        public TState startState;

        [SerializeField] protected T context;
        [SerializeField] protected TState currentState;


        protected virtual void Start()
        {
            if (startState != null && context != null)
            {
                currentState = startState;
                startState.OnEnter(context);
            }
            else
                Debug.LogWarning($"{this.gameObject.name}: has Not context or StartState");
        }

        protected virtual void Update()
        {
            if (currentState == null) return;

            var nextState = currentState.UpdateState(context) as TState;

            if (nextState != currentState)
                ForeChangeState(nextState);

        }

        protected virtual void ForeChangeState(TState state)
        {
            currentState.OnExit(context);
            currentState = state;
            currentState.OnEnter(context);
        }
    }
}