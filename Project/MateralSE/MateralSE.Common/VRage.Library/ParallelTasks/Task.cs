namespace ParallelTasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Task
    {
        public bool valid;
        internal WorkItem Item { get; private set; }
        internal int ID { get; private set; }
        public bool IsComplete =>
            (!this.valid || (this.Item.RunCount != this.ID));
        public Exception[] Exceptions =>
            (!this.valid ? null : this.Item.GetExceptions(this.ID));
        internal Task(WorkItem item)
        {
            this = new Task();
            this.ID = item.RunCount;
            this.Item = item;
            this.valid = true;
        }

        public void WaitOrExecute(bool blocking = false)
        {
            if (this.valid)
            {
                this.AssertNotOperatingOnItself();
                this.Item.WaitOrExecute(this.ID, blocking);
            }
        }

        public void Wait(bool blocking = false)
        {
            if (this.valid)
            {
                this.AssertNotOperatingOnItself();
                this.Item.Wait(this.ID, blocking);
            }
        }

        public void Execute()
        {
            if (this.valid)
            {
                this.AssertNotOperatingOnItself();
                this.Item.Execute(this.ID);
            }
        }

        internal void DoWork()
        {
            if (this.valid)
            {
                this.Item.DoWork(this.ID);
            }
        }

        private void AssertNotOperatingOnItself()
        {
            Task? currentTask = WorkItem.CurrentTask;
            if (((currentTask != null) && ReferenceEquals(currentTask.Value.Item, this.Item)) && (currentTask.Value.ID == this.ID))
            {
                throw new InvalidOperationException("A task cannot perform this operation on itself.");
            }
        }
    }
}

