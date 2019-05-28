namespace VRage.Serialization
{
    using System;
    using System.Reflection;
    using VRage.Library.Collections;

    public sealed class MyMemberSerializer<TOwner, TMember> : MyMemberSerializer<TOwner>
    {
        private Getter<TOwner, TMember> m_getter;
        private Setter<TOwner, TMember> m_setter;
        private MySerializer<TMember> m_serializer;
        private MemberInfo m_memberInfo;

        public override void Clone(ref TOwner original, ref TOwner clone)
        {
            TMember local;
            this.m_getter(ref original, out local);
            this.m_serializer.Clone(ref local);
            this.m_setter(ref clone, ref local);
        }

        public override bool Equals(ref TOwner a, ref TOwner b)
        {
            TMember local;
            TMember local2;
            this.m_getter(ref a, out local);
            this.m_getter(ref b, out local2);
            return this.m_serializer.Equals(ref local, ref local2);
        }

        public sealed override void Init(MemberInfo memberInfo, MySerializeInfo info)
        {
            if (this.m_serializer != null)
            {
                throw new InvalidOperationException("Already initialized");
            }
            this.m_getter = memberInfo.CreateGetterRef<TOwner, TMember>();
            this.m_setter = memberInfo.CreateSetterRef<TOwner, TMember>();
            this.m_serializer = MyFactory.GetSerializer<TMember>();
            base.m_info = info;
            this.m_memberInfo = memberInfo;
        }

        public sealed override unsafe void Read(BitStream stream, ref TOwner obj, MySerializeInfo info)
        {
            TMember* localPtr1;
            if (MySerializationHelpers.CreateAndRead<TMember>(stream, out localPtr1, this.m_serializer, info ?? base.m_info))
            {
                TMember local;
                localPtr1 = ref local;
                this.m_setter(ref obj, ref local);
            }
        }

        public override string ToString() => 
            string.Format("{2} {0}.{1}", this.m_memberInfo.DeclaringType.Name, this.m_memberInfo.Name, this.m_memberInfo.GetMemberType().Name);

        public sealed override void Write(BitStream stream, ref TOwner obj, MySerializeInfo info)
        {
            try
            {
                TMember local;
                this.m_getter(ref obj, out local);
                MySerializationHelpers.Write<TMember>(stream, ref local, this.m_serializer, info ?? base.m_info);
            }
            catch (MySerializeException exception)
            {
                MySerializeErrorEnum error = exception.Error;
                string message = (error == MySerializeErrorEnum.NullNotAllowed) ? $"Error serializing {this.m_memberInfo.DeclaringType.Name}.{this.m_memberInfo.Name}, member contains null, but it's not allowed, consider adding attribute [Serialize(MyObjectFlags.Nullable)]" : ((error != MySerializeErrorEnum.DynamicNotAllowed) ? "Unknown serialization error" : $"Error serializing {this.m_memberInfo.DeclaringType.Name}.{this.m_memberInfo.Name}, member contains inherited type, but it's not allowed, consider adding attribute [Serialize(MyObjectFlags.Dynamic)]");
                throw new InvalidOperationException(message, exception);
            }
        }
    }
}

