using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Octopus.TaskTree
{
    public interface IAsyncTask : IProgressReporter
    {
        string Id { get; set; }
        double ProgressValue { get; }
        object ProgressState { get; }
        IAsyncTask Parent { get; set; }
        IEnumerable<IAsyncTask> ChildTasks { get; }
        TaskStatus TaskStatus { get; }
        void SetAction(Func<IProgressReporter, CancellationToken, Task> cancellableProgressReportingAsyncFunction);
        Task ExecuteInSeries(CancellationToken cancellationToken, bool throwOnError);
        Task ExecuteConcurrently(CancellationToken cancellationToken, bool throwOnError);
        void AddChild(IAsyncTask childTask);
        void RemoveChild(IAsyncTask childTask);
        void ResetStatus();
        IEnumerable<IAsyncTask> ToFlatList();
    }
}
