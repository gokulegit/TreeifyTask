using Octopus.TaskTree;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Octopus.Sample
{
    public class TaskNodeViewModel : INotifyPropertyChanged
    {
        private readonly ITaskNode baseTaskNode;
        private ObservableCollection<TaskNodeViewModel> _childTasks;
        private TaskStatus _taskStatus;

        public TaskNodeViewModel(ITaskNode baseTaskNode)
        {
            this.baseTaskNode = baseTaskNode;
            PopulateChild(baseTaskNode);
            baseTaskNode.Reporting += BaseTaskNode_Reporting;
        }

        private void PopulateChild(ITaskNode baseTaskNode)
        {
            this._childTasks = new ObservableCollection<TaskNodeViewModel>();
            foreach (var ct in baseTaskNode.ChildTasks)
            {
                this._childTasks.Add(new TaskNodeViewModel(ct));
            }
        }

        private void BaseTaskNode_Reporting(object sender, ProgressReportingEventArgs eventArgs)
        {
            this.TaskStatus = eventArgs.TaskStatus;
        }

        public ObservableCollection<TaskNodeViewModel> ChildTasks =>
            _childTasks;

        public string Id
        {
            get => baseTaskNode.Id;
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

        public ITaskNode BaseTaskNode => baseTaskNode;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

