namespace VRage.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class NullableItemAttribute : SerializeAttribute
    {
        public NullableItemAttribute()
        {
            base.Flags = MyObjectFlags.DefaultZero;
            base.Kind = MySerializeKind.Item;
        }
    }
}

