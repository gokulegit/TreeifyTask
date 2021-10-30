using Microsoft.AspNetCore.Components;
using MudBlazor;
using Octopus.TaskTree;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octopus.BlazorSample;

namespace Octopus.BlazorSample.Pages
{
    public partial class TreeViewSamplePage : ComponentBase
    {
        private HashSet<AsyncTaskWrap> Tasks { get; set; }

        private AsyncTaskWrap ActivatedValue { get; set; }

        private HashSet<AsyncTaskWrap> SelectedValues { get; set; }

        protected override void OnInitialized()
        {
            var rootTask = new AsyncTaskWrap(new AsyncTask("Root"));
            var task1 = new AsyncTask("Task-1");            
            var task1_1 = new AsyncTask("Task-1.1");
            task1.AddChild(task1_1);
            var task2 = new AsyncTask("Task-2");

            rootTask.AsyncTask.AddChild(task1);
            rootTask.AsyncTask.AddChild(task2);           

            Tasks = new HashSet<AsyncTaskWrap>();
            Tasks.Add(rootTask);
        }
    }
}
