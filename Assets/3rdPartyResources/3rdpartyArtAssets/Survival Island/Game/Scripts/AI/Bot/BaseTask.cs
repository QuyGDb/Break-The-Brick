using UnityEngine;
using Watermelon.AI;

namespace Watermelon
{
    public abstract class BaseTask
    {
        protected HelperTaskType type;
        public HelperTaskType Type => type;

        protected int priority;
        public int Priority => priority;

        protected bool isActive;
        public bool IsActive => isActive;

        protected bool isTaken;

        protected bool skipIfTaken;

        protected Transform targetTransform;
        public Transform TargetTransform => targetTransform;

        protected float offsetRadius;
        public float OffsetRadius => offsetRadius;

        private TaskHandler taskHandler;

        public BaseTask(HelperTaskType type, Transform targetTransform, int priority = 0, bool skipIfTaken = true, float offsetRadius = 0)
        {
            this.type = type;
            this.priority = priority;
            this.targetTransform = targetTransform;
            this.skipIfTaken = skipIfTaken;
            this.offsetRadius = offsetRadius;

            isActive = false;
            isTaken = false;
        }

        public void Activate()
        {
            isActive = true;
            isTaken = false;

            OnTaskActivated();
        }

        public void Disable()
        {
            isActive = false;
            isTaken = false;

            OnTaskDisabled();
        }

        public void Take(HelperBehavior helperBehavior)
        {
            if (!isActive) return;

            isTaken = true;

            OnTaskTaken(helperBehavior);
        }

        public void Reset()
        {
            isTaken = false;

            OnTaskReseted();
        }

        public void Register(TaskHandler taskHandler)
        {
            this.taskHandler = taskHandler;

            taskHandler.RegisterTask(this);
        }

        public void Destroy()
        {
            if (taskHandler != null)
                taskHandler.RemoveTask(this);
        }

        public virtual bool IsInRange(HelperBehavior helperBehavior)
        {
            float maxTaskDistance = helperBehavior.TasksDistance;
            if(maxTaskDistance <= 0 || Vector3.Distance(helperBehavior.GetRestPosition(), targetTransform.position) <= maxTaskDistance)
            {
                return true;
            }

            return false;
        }

        public virtual bool IsPathExists(HelperBehavior helperBehavior)
        {
            Transform taskTransform = targetTransform;
            if (taskTransform != null)
            {
                if(offsetRadius > 0)
                {
                    Vector3 direction = (helperBehavior.transform.position - taskTransform.position).normalized;

                    return helperBehavior.NavMeshAgentBehaviour.PathExists(taskTransform.position + (direction * offsetRadius));
                }

                return helperBehavior.NavMeshAgentBehaviour.PathExists(taskTransform.position);
            }

            return false;
        }

        public bool IsTypeAvailable(HelperTaskType availableTasks)
        {
            return type != 0 && (availableTasks & type) == type;
        }

        public virtual bool IsTaken()
        {
            if (!skipIfTaken)
                return false;

            return isTaken;
        }

        public virtual void Unload() { }

        protected abstract void OnTaskActivated();
        protected abstract void OnTaskDisabled();
        protected abstract void OnTaskTaken(HelperBehavior helperBehavior);
        protected abstract void OnTaskReseted();

        public virtual bool Validate(HelperBehavior helperBehavior)
        {
            return true;
        }

        public override string ToString()
        {
            return string.Format("{0} (P:{1}) IsActive:{2}; IsTaken:{3};", GetType(), priority, isActive, IsTaken());
        }

        public virtual bool GetStateMachineState(out HelperStateMachine.State nextState)
        {
            nextState = HelperStateMachine.State.WaitingForTask;

            return true;
        }

        public virtual int GetPriority(HelperBehavior helperBehavior)
        {
            return priority;
        }
    }

    public static class TaskExtensions
    {
        public static bool IsTypeAvailable(this HelperTaskType availableTasks, HelperTaskType task)
        {
            return task != 0 && (availableTasks & task) == task;
        }
    }
}