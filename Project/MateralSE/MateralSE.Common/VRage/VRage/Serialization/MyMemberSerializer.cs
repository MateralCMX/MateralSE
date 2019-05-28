namespace VRage.Serialization
{
    using System;
    using System.Reflection;
    using VRage.Library.Collections;

    public abstract class MyMemberSerializer
    {
        protected MySerializeInfo m_info;

        protected MyMemberSerializer()
        {
        }

        public abstract void Clone(object original, object clone);
        public abstract bool Equals(object a, object b);
        public abstract void Init(MemberInfo memberInfo, MySerializeInfo info);
        public abstract void Read(BitStream stream, object obj, MySerializeInfo info);
        public abstract void Write(BitStream stream, object obj, MySerializeInfo info);

        public MySerializeInfo Info =>
            this.m_info;
    }
}

