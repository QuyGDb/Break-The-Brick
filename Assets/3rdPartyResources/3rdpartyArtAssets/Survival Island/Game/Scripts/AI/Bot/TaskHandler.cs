using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Watermelon
{
    public class TaskHandler
    {
        private List<BaseTask> tasks = new List<BaseTask>();
        public List<BaseTask> Tasks => tasks;

        public void Initialise()
        {

        }

        public void Unload()
        {
            foreach(var task in tasks)
            {
                task.Unload();
            }

            tasks.Clear();
        }

        public void RegisterTask(BaseTask task)
        {
            tasks.Add(task);
        }

        public void RemoveTask(BaseTask task)
        {
            tasks.Remove(task);
        }

        public BaseTask GetAvailableTask(HelperBehavior helperBehavior)
        {
            HelperTaskType taskType = helperBehavior.AvailableTaskTypes;

            IEnumerable<BaseTask> filteredTasks = tasks.Where(x => x.IsActive && !x.IsTaken() && x.IsTypeAvailable(taskType) && x.IsInRange(helperBehavior));

            // Randomize list
            filteredTasks = filteredTasks.OrderBy(x => Random.value);

            // Order by priority
            filteredTasks = filteredTasks.OrderByDescending(x => x.GetPriority(helperBehavior));

            // Validate
            filteredTasks = filteredTasks.Where(x => x.Validate(helperBehavior));

            // Get first task with path or null
            return filteredTasks.FirstOrDefault(x => x.IsPathExists(helperBehavior));
        }

        public void DebugPrint()
        {
            tasks.Display(x => x.ToString());
        }
    }
}