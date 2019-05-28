namespace VRage.Game.Voxels
{
    using ParallelTasks;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public abstract class MyPrecalcJob
    {
        public readonly Action OnCompleteDelegate;
        public bool IsValid;
        public volatile bool Started;

        protected MyPrecalcJob(bool enableCompletionCallback)
        {
            if (enableCompletionCallback)
            {
                this.OnCompleteDelegate = new Action(this.OnComplete);
            }
        }

        public abstract void Cancel();
        public virtual void DebugDraw(Color c)
        {
        }

        public abstract void DoWork();
        public void DoWorkInternal()
        {
            this.Started = true;
            this.DoWork();
        }

        protected virtual void OnComplete()
        {
        }

        public virtual bool IsCanceled =>
            false;

        public WorkOptions Options =>
            Parallel.DefaultOptions;

        public virtual int Priority =>
            0;
    }
}

