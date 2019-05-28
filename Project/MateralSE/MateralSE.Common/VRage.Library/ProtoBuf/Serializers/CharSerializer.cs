namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Meta;
    using System;

    internal sealed class CharSerializer : UInt16Serializer
    {
        private static readonly Type expectedType = typeof(char);

        public CharSerializer(TypeModel model) : base(model)
        {
        }

        public override object Read(object value, ProtoReader source) => 
            ((char) source.ReadUInt16());

        public override void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteUInt16((char) value, dest);
        }

        public override Type ExpectedType =>
            expectedType;
    }
}

