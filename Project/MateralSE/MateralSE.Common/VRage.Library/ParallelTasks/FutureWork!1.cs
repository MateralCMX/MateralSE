namespace ParallelTasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class FutureWork<T> : AbstractWork
    {
        public override void DoWork(WorkData workData = null)
        {
            this.Result = this.Function();
        }

        public static FutureWork<T> GetInstance() => 
            Singleton<Pool<FutureWork<T>>>.Instance.Get(Thread.CurrentThread);

        public void ReturnToPool()
        {
            if (this.ID < 0x7fffffff)
            {
                int iD = this.ID;
                this.ID = iD + 1;
                this.Function = null;
                T local = default(T);
                this.Result = local;
                Singleton<Pool<FutureWork<T>>>.Instance.Return(Thread.CurrentThread, (FutureWork<T>) this);
            }
        }

        public int ID { get; private set; }

        public Func<T> Function { get; set; }

        public T Result { get; set; }
    }
}

