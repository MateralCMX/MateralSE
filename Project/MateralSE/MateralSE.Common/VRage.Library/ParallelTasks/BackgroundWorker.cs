namespace ParallelTasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class BackgroundWorker
    {
        private static Stack<BackgroundWorker> idleWorkers = new Stack<BackgroundWorker>();
        private Thread thread;
        private AutoResetEvent resetEvent = new AutoResetEvent(false);
        private Task work;

        public BackgroundWorker()
        {
            this.thread = new Thread(new ThreadStart(this.WorkLoop));
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        private void Start(Task work)
        {
            this.work = work;
            this.resetEvent.Set();
        }

        public static void StartWork(Task work)
        {
            BackgroundWorker worker = null;
            Stack<BackgroundWorker> idleWorkers = BackgroundWorker.idleWorkers;
            lock (idleWorkers)
            {
                if (BackgroundWorker.idleWorkers.Count > 0)
                {
                    worker = BackgroundWorker.idleWorkers.Pop();
                }
            }
            if (worker == null)
            {
                worker = new BackgroundWorker();
            }
            worker.Start(work);
        }

        private void WorkLoop()
        {
            while (true)
            {
                this.resetEvent.WaitOne();
                this.work.DoWork();
                Stack<BackgroundWorker> idleWorkers = BackgroundWorker.idleWorkers;
                lock (idleWorkers)
                {
                    BackgroundWorker.idleWorkers.Push(this);
                }
            }
        }
    }
}

