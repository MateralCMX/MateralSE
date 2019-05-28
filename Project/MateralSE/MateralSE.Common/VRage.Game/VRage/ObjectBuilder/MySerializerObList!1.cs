namespace VRage.ObjectBuilder
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;
    using VRage.Serialization;

    internal class MySerializerObList<TItem> : MySerializer<MySerializableList<TItem>>
    {
        private MySerializer<TItem> m_itemSerializer;

        public MySerializerObList()
        {
            this.m_itemSerializer = MyFactory.GetSerializer<TItem>();
        }

        public override void Clone(ref MySerializableList<TItem> value)
        {
            MySerializableList<TItem> list = new MySerializableList<TItem>(value.Count);
            for (int i = 0; i < value.Count; i++)
            {
                TItem local = value[i];
                this.m_itemSerializer.Clone(ref local);
                list.Add(local);
            }
            value = list;
        }

        public override bool Equals(ref MySerializableList<TItem> a, ref MySerializableList<TItem> b)
        {
            if (a != b)
            {
                if (AnyNull(a, b))
                {
                    return false;
                }
                if (a.Count != b.Count)
                {
                    return false;
                }
                for (int i = 0; i < a.Count; i++)
                {
                    TItem local = a[i];
                    TItem local2 = b[i];
                    if (!this.m_itemSerializer.Equals(ref local, ref local2))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override unsafe void Read(BitStream stream, out MySerializableList<TItem> value, MySerializeInfo info)
        {
            int capacity = (int) stream.ReadUInt32Variant();
            value = new MySerializableList<TItem>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                TItem local;
                TItem* localPtr1;
                MySerializationHelpers.CreateAndRead<TItem>(stream, out localPtr1, this.m_itemSerializer, info.ItemInfo ?? MySerializeInfo.Default);
                localPtr1 = ref local;
                value.Add(local);
            }
        }

        public override void Write(BitStream stream, ref MySerializableList<TItem> value, MySerializeInfo info)
        {
            int count = value.Count;
            stream.WriteVariant((uint) count);
            for (int i = 0; i < count; i++)
            {
                TItem local = value[i];
                MySerializationHelpers.Write<TItem>(stream, ref local, this.m_itemSerializer, info.ItemInfo ?? MySerializeInfo.Default);
            }
        }
    }
}

