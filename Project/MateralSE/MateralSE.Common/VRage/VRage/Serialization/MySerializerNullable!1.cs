namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerNullable<T> : MySerializer<T?> where T: struct
    {
        private MySerializer<T> m_serializer;

        public MySerializerNullable()
        {
            this.m_serializer = MyFactory.GetSerializer<T>();
        }

        public override void Clone(ref T? value)
        {
            if (value != 0)
            {
                T local = value.Value;
                this.m_serializer.Clone(ref local);
                value = new T?(local);
            }
        }

        public override bool Equals(ref T? a, ref T? b)
        {
            if ((a != 0) != (b != 0))
            {
                return false;
            }
            if (a == 0)
            {
                return true;
            }
            T local = a.Value;
            T local2 = b.Value;
            return this.m_serializer.Equals(ref local, ref local2);
        }

        public override void Read(BitStream stream, out T? value, MySerializeInfo info)
        {
            if (!stream.ReadBool())
            {
                value = 0;
            }
            else
            {
                T local;
                this.m_serializer.Read(stream, out local, info);
                value = new T?(local);
            }
        }

        public override void Write(BitStream stream, ref T? value, MySerializeInfo info)
        {
            if (value == 0)
            {
                stream.WriteBool(false);
            }
            else
            {
                T local = value.Value;
                stream.WriteBool(true);
                this.m_serializer.Write(stream, ref local, info);
            }
        }
    }
}

