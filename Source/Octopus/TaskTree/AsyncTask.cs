using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Octopus.TaskTree
{
    public class AsyncTask : IAsyncTask
    {
        private static Random rnd = new Random();
        private readonly List<Task> taskObjects = new();
        private readonly List<IAsyncTask> childTasks = new();
        private bool hasCustomAction;
        private Func<IProgressReporter, CancellationToken, Task> action =
            async (rep, tok) => await Task.Yield();
        public event ProgressReportingEventHandler Reporting;
        private bool seriesRunnerIsBusy;
        private bool concurrentRunnerIsBusy;

        public AsyncTask()
        {
            this.Id = rnd.Next() + string.Empty;
            this.Reporting += OnSelfReporting;
        }

        public AsyncTask(string Id)
            : this()
        {
            this.Id = Id ?? rnd.Next() + string.Empty;
        }

        public AsyncTask(string Id, Func<IProgressReporter, CancellationToken, Task> cancellableProgressReportingAsyncFunction)
            : this(Id)
        {
            this.SetAction(cancellableProgressReportingAsyncFunction);
        }

        #region Props

        public string Id { get; set; }
        public double ProgressValue { get; private set; }
        public object ProgressState { get; private set; }
        public TaskStatus TaskStatus { get; private set; }
        public IAsyncTask Parent { get; set; }
        public IEnumerable<IAsyncTask> ChildTasks =>
            this.childTasks;

        #endregion Props

        public void AddChild(IAsyncTask childTask)
        {
            childTask = childTask ?? throw new ArgumentNullException(nameof(childTask));

            EnsureNoCycles(childTask);

            childTask.Parent = this;
            childTask.Reporting += OnChildReporting;
            childTasks.Add(childTask);
        }

        private class ActionReport
        {
            public ActionReport()
            {
                this.TaskStatus = TaskStatus.NotStarted;
                this.ProgressValue = 0;
                this.ProgressState = null;
            }

            public ActionReport(IAsyncTask task)
            {
                this.Id = task.Id;
                this.TaskStatus = task.TaskStatus;
                this.ProgressState = task.ProgressState;
                this.ProgressValue = task.ProgressValue;
            }
            public string Id { get; set; }
            public TaskStatus TaskStatus { get; set; }
            public double ProgressValue { get; set; }
            public object ProgressState { get; set; }
            public override string ToString()
            {
                return $"Id={Id},({TaskStatus}, {ProgressValue}, {ProgressState})";
            }
        }

        private ActionReport selfActionReport = new();

        private void OnSelfReporting(object sender, ProgressReportingEventArgs eventArgs)
        {
            TaskStatus = selfActionReport.TaskStatus = eventArgs.TaskStatus;
            ProgressValue = selfActionReport.ProgressValue = eventArgs.ProgressValue;
            ProgressState = selfActionReport.ProgressState = eventArgs.ProgressState;
        }

        private void OnChildReporting(object sender, ProgressReportingEventArgs eventArgs)
        {
            // Child task that reports 
            var cTask = sender as IAsyncTask;

            var allReports = childTasks.Select(t => new ActionReport(t));
            if (hasCustomAction)
            {
                allReports = allReports.Append(selfActionReport);
            }

            this.TaskStatus = allReports.Any(v => v.TaskStatus == TaskStatus.InDeterminate) ? TaskStatus.InDeterminate : TaskStatus.InProgress;
            this.TaskStatus = allReports.Any(v => v.TaskStatus == TaskStatus.Failed) ? TaskStatus.Failed : this.TaskStatus;

            if (this.TaskStatus == TaskStatus.Failed)
            {
                this.ProgressState = new AggregateException($"{Id}: One or more error occurred in child tasks.",
                    childTasks.Where(v => v.TaskStatus == TaskStatus.Failed && v.ProgressState is Exception)
                        .Select(c => c.ProgressState as Exception));
            }

            this.ProgressValue = allReports.Select(t => t.ProgressValue).Average();

            SafeRaiseReportingEvent(this, new ProgressReportingEventArgs
            {
                ProgressValue = this.ProgressValue,
                TaskStatus = this.TaskStatus,
                ChildTasksRunningInParallel = concurrentRunnerIsBusy,
                ProgressState = seriesRunnerIsBusy ? cTask.ProgressState : this.ProgressState
            });
        }

        public async Task ExecuteConcurrently(CancellationToken cancellationToken, bool throwOnError)
        {
            if (concurrentRunnerIsBusy || seriesRunnerIsBusy) return;
            concurrentRunnerIsBusy = true;

            ResetChildrenProgressValues();
            foreach (var child in childTasks)
            {
                taskObjects.Add(child.ExecuteConcurrently(cancellationToken, throwOnError));
            }

            taskObjects.Add(ExceptionHandledAction(cancellationToken, throwOnError));
            if (taskObjects.Any())
            {
                await Task.WhenAll(taskObjects);
            }

            if (throwOnError && taskObjects.Any(t => t.IsFaulted))
            {
                var exs = taskObjects.Where(t => t.IsFaulted).Select(t => t.Exception);
                throw new AggregateException($"Internal error occurred while executing task - {Id}.", exs);
            }
            concurrentRunnerIsBusy = false;
            if (TaskStatus != TaskStatus.Failed)
            {
                if (cancellationToken.IsCancellationRequested)
                    Report(TaskStatus.Cancelled, 0);
                else
                    Report(TaskStatus.Completed, 100);
            }
        }

        private async Task ExceptionHandledAction(CancellationToken cancellationToken, bool throwOnError)
        {
            try
            {
                await action(this, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Don't throw this as an error as we have to come out of await.
            }
            catch (Exception ex)
            {
                this.Report(TaskStatus.Failed, this.ProgressValue, ex);
                if (throwOnError)
                {
                    throw new AggregateException($"Internal error occurred while executing the action of task - {Id}.", ex);
                }
            }
        }

        public async Task ExecuteInSeries(CancellationToken cancellationToken, bool throwOnError)
        {
            if (seriesRunnerIsBusy || concurrentRunnerIsBusy) return;
            seriesRunnerIsBusy = true;

            ResetChildrenProgressValues();
            try
            {
                foreach (var child in childTasks)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    await child.ExecuteInSeries(cancellationToken, throwOnError);
                }
                await ExceptionHandledAction(cancellationToken, throwOnError);
            }
            catch (Exception ex)
            {
                if (throwOnError)
                {
                    throw new AggregateException($"Internal error occurred while executing task - {Id}.", ex);
                }
            }
            seriesRunnerIsBusy = false;
            if (TaskStatus != TaskStatus.Failed)
            {
                if (cancellationToken.IsCancellationRequested)
                    Report(TaskStatus.Cancelled, 0);
                else
                    Report(TaskStatus.Completed, 100);
            }
        }

        public IEnumerable<IAsyncTask> ToFlatList()
        {
            return FlatList(this);
        }

        private void SafeRaiseReportingEvent(object sender, ProgressReportingEventArgs args)
        {
            this.Reporting?.Invoke(sender, args);
        }

        private void ResetChildrenProgressValues()
        {
            taskObjects.Clear();
            foreach (var task in childTasks)
            {
                task.ResetStatus();
            }
        }

        private void EnsureNoCycles(IAsyncTask newTask)
        {
            var root = this as IAsyncTask;
            while (true)
            {
                if (root.Parent is null)
                {
                    break;
                }
                root = root.Parent;
            }
            var existingTask = FlatList(root).FirstOrDefault(t => t == newTask);
            if (existingTask != null)
            {
                throw new AsyncTasksCycleDetectedException(newTask, existingTask.Parent);
            }
        }

        private IEnumerable<IAsyncTask> FlatList(IAsyncTask root)
        {
            yield return root;
            foreach(var ct in root.ChildTasks)
            {
                foreach (var item in FlatList(ct))
                    yield return item;
            }
        }

        public void RemoveChild(IAsyncTask childTask)
        {
            childTask.Reporting -= OnChildReporting;
            childTasks.Remove(childTask);
        }

        public void Report(TaskStatus taskStatus, double progressValue, object progressState = null)
        {
            SafeRaiseReportingEvent(this, new ProgressReportingEventArgs
            {
                ChildTasksRunningInParallel = concurrentRunnerIsBusy,
                TaskStatus = taskStatus,
                ProgressValue = progressValue,
                ProgressState = progressState
            });
        }

        public void SetAction(Func<IProgressReporter, CancellationToken, Task> cancellableProgressReportingAction)
        {
            cancellableProgressReportingAction = cancellableProgressReportingAction ??
                throw new ArgumentNullException(nameof(cancellableProgressReportingAction));
            hasCustomAction = true;
            action = cancellableProgressReportingAction;
        }

        public void ResetStatus()
        {
            this.TaskStatus = TaskStatus.NotStarted;
            this.ProgressState = null;
            this.ProgressValue = 0;
        }

        public override string ToString()
        {
            return $"Id={Id},({TaskStatus}, {ProgressValue}, {ProgressState})";
        }
    }
}
