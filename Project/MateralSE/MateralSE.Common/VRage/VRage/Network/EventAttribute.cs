namespace VRage.Network
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : Attribute
    {
        public readonly int Order;
        public readonly string Serialization;

        public EventAttribute(string serializationMethod = null, [CallerLineNumber] int order = 0)
        {
            this.Order = order;
            this.Serialization = serializationMethod;
        }
    }
}

