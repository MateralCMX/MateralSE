namespace ParallelTasks
{
    using System;
    using System.Threading;

    public abstract class Singleton<T> where T: class, new()
    {
        private static T instance;

        protected Singleton()
        {
        }

        public static T Instance
        {
            get
            {
                if (Singleton<T>.instance == null)
                {
                    T local = Activator.CreateInstance<T>();
                    T comparand = default(T);
                    Interlocked.CompareExchange<T>(ref Singleton<T>.instance, local, comparand);
                }
                return Singleton<T>.instance;
            }
        }
    }
}

