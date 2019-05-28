namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class QueueExtensions
    {
        public static List<T> ToList<T>(this ConcurrentQueue<T> queue) => 
            QueueReflector<T>.ToList(queue);

        public static bool TryDequeue<T>(this Queue<T> queue, out T result)
        {
            if (queue.Count > 0)
            {
                result = queue.Dequeue();
                return true;
            }
            result = default(T);
            return false;
        }

        public static bool TryDequeueSync<T>(this Queue<T> queue, out T result)
        {
            object syncRoot = ((ICollection) queue).SyncRoot;
            lock (syncRoot)
            {
                return queue.TryDequeue<T>(out result);
            }
        }

        private static class QueueReflector<T>
        {
            public static Func<ConcurrentQueue<T>, List<T>> ToList;

            static QueueReflector()
            {
                MethodInfo method = typeof(ConcurrentQueue<T>).GetMethod("ToList", BindingFlags.NonPublic | BindingFlags.Instance);
                QueueExtensions.QueueReflector<T>.ToList = (Func<ConcurrentQueue<T>, List<T>>) Delegate.CreateDelegate(typeof(Func<ConcurrentQueue<T>, List<T>>), method);
            }
        }
    }
}

