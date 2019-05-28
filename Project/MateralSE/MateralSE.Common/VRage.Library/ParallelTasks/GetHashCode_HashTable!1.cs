namespace ParallelTasks
{
    using System;

    public class GetHashCode_HashTable<TKey>
    {
        public static int GetHashCode(TKey v) => 
            v.GetHashCode();
    }
}

