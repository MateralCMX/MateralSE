namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class DynamicKeyAttribute : SerializeAttribute
    {
        public DynamicKeyAttribute(Type dynamicSerializerType, bool defaultTypeCommon = false)
        {
            this.Flags = defaultTypeCommon ? MyObjectFlags.DynamicDefault : MyObjectFlags.Dynamic;
            base.DynamicSerializerType = dynamicSerializerType;
            base.Kind = MySerializeKind.Key;
        }
    }
}

