namespace VRage.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class NullableAttribute : SerializeAttribute
    {
        public NullableAttribute()
        {
            base.Flags = MyObjectFlags.DefaultZero;
        }
    }
}

