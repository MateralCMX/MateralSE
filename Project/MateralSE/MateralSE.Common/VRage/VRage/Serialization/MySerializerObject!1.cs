namespace VRage.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Library.Collections;

    public class MySerializerObject<T> : MySerializer<T>
    {
        private MyMemberSerializer<T>[] m_memberSerializers;

        public MySerializerObject()
        {
            IEnumerable<MemberInfo> enumerable = (from s in typeof(T).GetDataMembers(true, true, true, true, false, true, true, true)
                where !Attribute.IsDefined(s, typeof(NoSerializeAttribute))
                where Attribute.IsDefined(s, typeof(SerializeAttribute)) || s.IsMemberPublic()
                select s).Where<MemberInfo>(new Func<MemberInfo, bool>(this.Filter));
            this.m_memberSerializers = (from s in enumerable select MyFactory.CreateMemberSerializer<T>(s)).ToArray<MyMemberSerializer<T>>();
        }

        public override void Clone(ref T value)
        {
            T clone = Activator.CreateInstance<T>();
            MyMemberSerializer<T>[] memberSerializers = this.m_memberSerializers;
            for (int i = 0; i < memberSerializers.Length; i++)
            {
                memberSerializers[i].Clone(ref value, ref clone);
            }
            value = clone;
        }

        public override bool Equals(ref T a, ref T b)
        {
            if (((T) a) != ((T) b))
            {
                if (AnyNull((T) a, (T) b))
                {
                    return false;
                }
                MyMemberSerializer<T>[] memberSerializers = this.m_memberSerializers;
                for (int i = 0; i < memberSerializers.Length; i++)
                {
                    if (!memberSerializers[i].Equals(ref a, ref b))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool Filter(MemberInfo info)
        {
            if (info.MemberType == MemberTypes.Field)
            {
                return true;
            }
            if (info.MemberType != MemberTypes.Property)
            {
                return false;
            }
            PropertyInfo info2 = (PropertyInfo) info;
            return (info2.CanRead && (info2.CanWrite && (info2.GetIndexParameters().Length == 0)));
        }

        public override void Read(BitStream stream, out T value, MySerializeInfo info)
        {
            value = Activator.CreateInstance<T>();
            MyMemberSerializer<T>[] memberSerializers = this.m_memberSerializers;
            for (int i = 0; i < memberSerializers.Length; i++)
            {
                memberSerializers[i].Read(stream, ref value, info.ItemInfo);
            }
        }

        public override void Write(BitStream stream, ref T value, MySerializeInfo info)
        {
            MyMemberSerializer<T>[] memberSerializers = this.m_memberSerializers;
            for (int i = 0; i < memberSerializers.Length; i++)
            {
                memberSerializers[i].Write(stream, ref value, info.ItemInfo);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySerializerObject<T>.<>c <>9;
            public static Func<MemberInfo, bool> <>9__1_0;
            public static Func<MemberInfo, bool> <>9__1_1;
            public static Func<MemberInfo, MyMemberSerializer<T>> <>9__1_2;

            static <>c()
            {
                MySerializerObject<T>.<>c.<>9 = new MySerializerObject<T>.<>c();
            }

            internal bool <.ctor>b__1_0(MemberInfo s) => 
                !Attribute.IsDefined(s, typeof(NoSerializeAttribute));

            internal bool <.ctor>b__1_1(MemberInfo s) => 
                (Attribute.IsDefined(s, typeof(SerializeAttribute)) || s.IsMemberPublic());

            internal MyMemberSerializer<T> <.ctor>b__1_2(MemberInfo s) => 
                MyFactory.CreateMemberSerializer<T>(s);
        }
    }
}

