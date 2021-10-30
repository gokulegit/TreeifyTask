using System;

namespace Octopus.TaskTree
{
    public class ProgressReportingEventArgs : EventArgs
    {
        public TaskStatus TaskStatus { get; set; }
        public double ProgressValue { get; set; }
        public object ProgressState { get; set; }
        public bool ChildTasksRunningInParallel { get; set; }
    }

    public delegate void ProgressReportingEventHandler(object sender, ProgressReportingEventArgs eventArgs);
}
