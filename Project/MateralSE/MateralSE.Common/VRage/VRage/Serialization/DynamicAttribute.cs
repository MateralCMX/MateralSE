namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class DynamicAttribute : SerializeAttribute
    {
        public DynamicAttribute(Type dynamicSerializerType, bool defaultTypeCommon = false)
        {
            this.Flags = defaultTypeCommon ? MyObjectFlags.DynamicDefault : MyObjectFlags.Dynamic;
            base.DynamicSerializerType = dynamicSerializerType;
        }
    }
}

