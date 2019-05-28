namespace ParallelTasks
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Future<T>
    {
        private Task task;
        private FutureWork<T> work;
        private int id;
        public bool IsComplete =>
            this.task.IsComplete;
        public Exception[] Exceptions =>
            this.task.Exceptions;
        internal Future(Task task, FutureWork<T> work)
        {
            this.task = task;
            this.work = work;
            this.id = work.ID;
        }

        public T GetResult()
        {
            if ((this.work == null) || (this.work.ID != this.id))
            {
                throw new InvalidOperationException("The result of a future can only be retrieved once.");
            }
            this.task.WaitOrExecute(false);
            T result = this.work.Result;
            this.work.ReturnToPool();
            this.work = null;
            return result;
        }
    }
}

