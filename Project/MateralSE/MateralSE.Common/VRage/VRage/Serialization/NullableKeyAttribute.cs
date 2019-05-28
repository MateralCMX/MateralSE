namespace VRage.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class NullableKeyAttribute : SerializeAttribute
    {
        public NullableKeyAttribute()
        {
            base.Flags = MyObjectFlags.DefaultZero;
            base.Kind = MySerializeKind.Key;
        }
    }
}

