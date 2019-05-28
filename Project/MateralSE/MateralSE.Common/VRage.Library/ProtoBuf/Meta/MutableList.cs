namespace ProtoBuf.Meta
{
    using System;
    using System.Reflection;

    internal sealed class MutableList : BasicList
    {
        public void RemoveLast()
        {
            base.head.RemoveLastWithMutate();
        }

        public object this[int index]
        {
            get => 
                base.head[index];
            set => 
                (base.head[index] = value);
        }
    }
}

