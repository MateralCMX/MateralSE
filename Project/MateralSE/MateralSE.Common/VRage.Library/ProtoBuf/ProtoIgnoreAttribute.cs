namespace ProtoBuf
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class ProtoIgnoreAttribute : Attribute
    {
    }
}

