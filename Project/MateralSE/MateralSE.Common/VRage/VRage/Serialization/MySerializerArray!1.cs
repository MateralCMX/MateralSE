namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerArray<TItem> : MySerializer<TItem[]>
    {
        private MySerializer<TItem> m_itemSerializer;

        public MySerializerArray()
        {
            this.m_itemSerializer = MyFactory.GetSerializer<TItem>();
        }

        public override void Clone(ref TItem[] value)
        {
            value = (TItem[]) value.Clone();
            for (int i = 0; i < value.Length; i++)
            {
                this.m_itemSerializer.Clone(ref value[i]);
            }
        }

        public override bool Equals(ref TItem[] a, ref TItem[] b)
        {
            if (a != b)
            {
                if (AnyNull(a, b))
                {
                    return false;
                }
                if (a.Length != b.Length)
                {
                    return false;
                }
                for (int i = 0; i < a.Length; i++)
                {
                    if (!this.m_itemSerializer.Equals(ref a[i], ref b[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void Read(BitStream stream, out TItem[] value, MySerializeInfo info)
        {
            int num = (int) stream.ReadUInt32Variant();
            value = new TItem[num];
            for (int i = 0; i < value.Length; i++)
            {
                MySerializationHelpers.CreateAndRead<TItem>(stream, out value[i], this.m_itemSerializer, info.ItemInfo ?? MySerializeInfo.Default);
            }
        }

        public override void Write(BitStream stream, ref TItem[] value, MySerializeInfo info)
        {
            stream.WriteVariant((uint) value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                MySerializationHelpers.Write<TItem>(stream, ref value[i], this.m_itemSerializer, info.ItemInfo ?? MySerializeInfo.Default);
            }
        }
    }
}

