namespace VRage.ObjectBuilders
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Serialization;

    public class DynamicNullableObjectBuilderItemAttribute : DynamicNullableItemAttribute
    {
        public DynamicNullableObjectBuilderItemAttribute(bool defaultTypeCommon = false) : base(typeof(MyObjectBuilderDynamicSerializer), defaultTypeCommon)
        {
        }
    }
}

