namespace VRage.ObjectBuilder
{
    using System;
    using System.Collections.Generic;

    public class MySerializableList<TItem> : List<TItem>
    {
        public MySerializableList()
        {
        }

        public MySerializableList(IEnumerable<TItem> collection) : base(collection)
        {
        }

        public MySerializableList(int capacity) : base(capacity)
        {
        }

        public void Add(TItem item)
        {
            if (item != null)
            {
                base.Add(item);
            }
        }
    }
}

