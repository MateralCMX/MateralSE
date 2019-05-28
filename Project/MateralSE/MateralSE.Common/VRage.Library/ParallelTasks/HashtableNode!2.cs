namespace ParallelTasks
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct HashtableNode<TKey, TData>
    {
        public TKey Key;
        public TData Data;
        public HashtableToken Token;
        public HashtableNode(TKey key, TData data, HashtableToken token)
        {
            this.Key = key;
            this.Data = data;
            this.Token = token;
        }
    }
}

