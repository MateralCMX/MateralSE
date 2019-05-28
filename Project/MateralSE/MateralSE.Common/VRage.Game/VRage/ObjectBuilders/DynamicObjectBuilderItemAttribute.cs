namespace VRage.ObjectBuilders
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Serialization;

    public class DynamicObjectBuilderItemAttribute : DynamicItemAttribute
    {
        public DynamicObjectBuilderItemAttribute(bool defaultTypeCommon = false) : base(typeof(MyObjectBuilderDynamicSerializer), defaultTypeCommon)
        {
        }
    }
}

