using UnityEngine;

namespace Watermelon
{
    public class StateBehavior<T> : IStateBehavior where T : MonoBehaviour
    {
        protected T target;
        public T Target => target;

        protected Vector3 Position => Target.transform.position;

        public event SimpleCallback OnFinished;
        protected void InvokeOnFinished()
        {
            OnFinished?.Invoke();
        }

        public StateBehavior(T target)
        {
            this.target = target;
        }

        public virtual void OnStart()
        {

        }

        public virtual void OnEnd()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public void SubscribeOnFinish(SimpleCallback callback)
        {
            OnFinished += callback;
        }

        public void UnsubscribeFromFinish(SimpleCallback callback)
        {
            OnFinished -= callback;
        }
    }

    public class StateTransition<T> where T : System.Enum
    {
        public StateTransitionType transitionType;
        public delegate bool EvaluateDelegate(out T nextState);
        public EvaluateDelegate Evaluate { get; set; }

        public StateTransition(EvaluateDelegate evaluate, StateTransitionType transitionType = StateTransitionType.Independent)
        {
            this.transitionType = transitionType;
            Evaluate = evaluate;
        }
    }

    public enum StateTransitionType
    {
        Independent,
        OnFinish,
    }

    public interface IStateBehavior
    {
        void OnUpdate();
        void OnEnd();
        void OnStart();

        void SubscribeOnFinish(SimpleCallback callback);
        void UnsubscribeFromFinish(SimpleCallback callback);
    }
}