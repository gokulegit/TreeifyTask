using Octopus.TaskTree;
using System.Collections.Generic;
using System.Linq;

namespace Octopus.BlazorSample
{
    public class TaskNodeWrap
    {
        public TaskNodeWrap(ITaskNode taskNode)
        {
            TaskNode = taskNode;
        }

        public ITaskNode TaskNode { get; }
        public HashSet<TaskNodeWrap> Children =>
            new HashSet<TaskNodeWrap>(TaskNode.ChildTasks.Select(ct => new TaskNodeWrap(ct)));
    }
}
