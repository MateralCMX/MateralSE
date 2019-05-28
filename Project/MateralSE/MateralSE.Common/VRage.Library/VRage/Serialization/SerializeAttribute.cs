namespace VRage.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeAttribute : Attribute
    {
        public MyObjectFlags Flags;
        public MyPrimitiveFlags PrimitiveFlags;
        public ushort FixedLength;
        public Type DynamicSerializerType;
        public MySerializeKind Kind;

        public SerializeAttribute()
        {
        }

        public SerializeAttribute(MyObjectFlags flags)
        {
            this.Flags = flags;
        }

        public SerializeAttribute(MyPrimitiveFlags flags)
        {
            this.PrimitiveFlags = flags;
        }

        public SerializeAttribute(MyObjectFlags flags, Type dynamicResolverType)
        {
            this.Flags = flags;
            this.DynamicSerializerType = dynamicResolverType;
        }

        public SerializeAttribute(MyObjectFlags flags, ushort fixedLength)
        {
            this.Flags = flags;
            this.FixedLength = fixedLength;
        }

        public SerializeAttribute(MyPrimitiveFlags flags, ushort fixedLength)
        {
            this.PrimitiveFlags = flags;
            this.FixedLength = fixedLength;
        }
    }
}

