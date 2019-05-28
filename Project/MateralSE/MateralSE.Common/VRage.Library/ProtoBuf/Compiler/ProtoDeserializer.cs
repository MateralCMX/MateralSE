namespace ProtoBuf.Compiler
{
    using ProtoBuf;
    using System;
    using System.Runtime.CompilerServices;

    internal delegate object ProtoDeserializer(object value, ProtoReader source);
}

