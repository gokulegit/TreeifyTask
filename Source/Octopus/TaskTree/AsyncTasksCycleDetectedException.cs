using System;
using System.Runtime.Serialization;

namespace Octopus.TaskTree
{
    [Serializable]
    public class AsyncTasksCycleDetectedException : Exception
    {
        public IAsyncTask NewTask { get; }
        public IAsyncTask ParentTask { get; }

        public string MessageStr { get; private set; }

        public AsyncTasksCycleDetectedException()
            : base("Cycle detected in the task tree.")
        {
        }

        public AsyncTasksCycleDetectedException(IAsyncTask newTask, IAsyncTask parentTask)
            : base($"Task '{newTask?.Id}' was already added as a child to task tree of '{parentTask?.Id}'.")
        {
            this.NewTask = newTask;
            this.ParentTask = parentTask;
        }

        public AsyncTasksCycleDetectedException(string message) : base(message)
        {
        }

        public AsyncTasksCycleDetectedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AsyncTasksCycleDetectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}