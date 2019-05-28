namespace VRage.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NoSerializeAttribute : Attribute
    {
    }
}

