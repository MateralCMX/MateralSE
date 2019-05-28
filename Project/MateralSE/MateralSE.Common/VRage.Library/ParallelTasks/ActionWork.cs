namespace ParallelTasks
{
    using System;
    using System.Runtime.InteropServices;

    public class ActionWork : AbstractWork
    {
        public readonly Action<WorkData> _Action;

        public ActionWork(Action<WorkData> action) : this(action, Parallel.DefaultOptions)
        {
        }

        public ActionWork(Action<WorkData> action, WorkOptions options)
        {
            this._Action = action;
            this.Options = options;
        }

        public override void DoWork(WorkData workData = null)
        {
            this._Action(workData);
        }
    }
}

