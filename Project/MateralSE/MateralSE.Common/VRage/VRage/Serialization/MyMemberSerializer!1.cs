namespace VRage.Serialization
{
    using System;
    using VRage.Library.Collections;

    public abstract class MyMemberSerializer<TOwner> : MyMemberSerializer
    {
        protected MyMemberSerializer()
        {
        }

        public abstract void Clone(ref TOwner original, ref TOwner clone);
        public sealed override void Clone(object original, object clone)
        {
            TOwner local = (TOwner) original;
            TOwner local2 = (TOwner) clone;
            this.Clone(local, local2);
        }

        public abstract bool Equals(ref TOwner a, ref TOwner b);
        public sealed override bool Equals(object a, object b)
        {
            TOwner local = (TOwner) a;
            TOwner local2 = (TOwner) b;
            return this.Equals(ref local, ref local2);
        }

        public abstract void Read(BitStream stream, ref TOwner obj, MySerializeInfo info);
        public sealed override void Read(BitStream stream, object obj, MySerializeInfo info)
        {
            TOwner local = (TOwner) obj;
            this.Read(stream, ref local, info);
        }

        public abstract void Write(BitStream stream, ref TOwner obj, MySerializeInfo info);
        public sealed override void Write(BitStream stream, object obj, MySerializeInfo info)
        {
            TOwner local = (TOwner) obj;
            this.Write(stream, ref local, info);
        }
    }
}

