using System;
using System.Runtime.Serialization;

namespace Octopus.TaskTree
{
    [Serializable]
    public class TaskNodeCycleDetectedException : Exception
    {
        public ITaskNode NewTask { get; }
        public ITaskNode ParentTask { get; }

        public string MessageStr { get; private set; }

        public TaskNodeCycleDetectedException()
            : base("Cycle detected in the task tree.")
        {
        }

        public TaskNodeCycleDetectedException(ITaskNode newTask, ITaskNode parentTask)
            : base($"Task '{newTask?.Id}' was already added as a child to task tree of '{parentTask?.Id}'.")
        {
            this.NewTask = newTask;
            this.ParentTask = parentTask;
        }

        public TaskNodeCycleDetectedException(string message) : base(message)
        {
        }

        public TaskNodeCycleDetectedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TaskNodeCycleDetectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}