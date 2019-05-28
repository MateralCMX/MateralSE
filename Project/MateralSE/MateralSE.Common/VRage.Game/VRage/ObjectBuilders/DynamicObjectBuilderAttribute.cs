namespace VRage.ObjectBuilders
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Serialization;

    public class DynamicObjectBuilderAttribute : DynamicAttribute
    {
        public DynamicObjectBuilderAttribute(bool defaultTypeCommon = false) : base(typeof(MyObjectBuilderDynamicSerializer), defaultTypeCommon)
        {
        }
    }
}

