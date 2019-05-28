namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class DynamicNullableItemAttribute : SerializeAttribute
    {
        public DynamicNullableItemAttribute(Type dynamicSerializerType, bool defaultTypeCommon = false)
        {
            this.Flags = defaultTypeCommon ? MyObjectFlags.DynamicDefault : MyObjectFlags.Dynamic;
            base.Flags |= MyObjectFlags.DefaultZero;
            base.DynamicSerializerType = dynamicSerializerType;
            base.Kind = MySerializeKind.Item;
        }
    }
}

