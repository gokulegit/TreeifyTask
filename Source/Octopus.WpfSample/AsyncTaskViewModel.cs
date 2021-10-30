using Octopus.TaskTree;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Octopus.Sample
{
    public class AsyncTaskViewModel : INotifyPropertyChanged
    {
        private readonly IAsyncTask baseAsyncTask;
        private ObservableCollection<AsyncTaskViewModel> _childTasks;
        private TaskStatus _taskStatus;

        public AsyncTaskViewModel(IAsyncTask baseAsyncTask)
        {
            this.baseAsyncTask = baseAsyncTask;
            PopulateChild(baseAsyncTask);
            baseAsyncTask.Reporting += BaseAsyncTask_Reporting;
        }

        private void PopulateChild(IAsyncTask baseAsyncTask)
        {
            this._childTasks = new ObservableCollection<AsyncTaskViewModel>();
            foreach (var ct in baseAsyncTask.ChildTasks)
            {
                this._childTasks.Add(new AsyncTaskViewModel(ct));
            }
        }

        private void BaseAsyncTask_Reporting(object sender, ProgressReportingEventArgs eventArgs)
        {
            this.TaskStatus = eventArgs.TaskStatus;
        }

        public ObservableCollection<AsyncTaskViewModel> ChildTasks =>
            _childTasks;

        public string Id
        {
            get => baseAsyncTask.Id;
        }

        public TaskStatus TaskStatus
        {
            get => _taskStatus;
            set
            {
                _taskStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TaskStatus)));
            }
        }

        public IAsyncTask BaseAsyncTask => baseAsyncTask;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

