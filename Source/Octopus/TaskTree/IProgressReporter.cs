namespace Octopus.TaskTree
{
    public interface IProgressReporter
    {
        void Report(TaskStatus taskStatus, double progressValue, object progressState = default);
        event ProgressReportingEventHandler Reporting;
    }
}
