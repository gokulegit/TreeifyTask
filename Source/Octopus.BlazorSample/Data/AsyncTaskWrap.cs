using Octopus.TaskTree;
using System.Collections.Generic;
using System.Linq;

namespace Octopus.BlazorSample
{
    public class AsyncTaskWrap
    {
        public AsyncTaskWrap(IAsyncTask asyncTask)
        {
            AsyncTask = asyncTask;
        }

        public IAsyncTask AsyncTask { get; }
        public HashSet<AsyncTaskWrap> Children =>
            new HashSet<AsyncTaskWrap>(AsyncTask.ChildTasks.Select(ct => new AsyncTaskWrap(ct)));
    }
}
