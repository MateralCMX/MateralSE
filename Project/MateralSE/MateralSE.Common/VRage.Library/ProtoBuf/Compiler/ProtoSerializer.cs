namespace ProtoBuf.Compiler
{
    using ProtoBuf;
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void ProtoSerializer(object value, ProtoWriter dest);
}

