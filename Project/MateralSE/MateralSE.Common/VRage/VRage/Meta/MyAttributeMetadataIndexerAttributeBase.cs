namespace VRage.Meta
{
    using System;

    public abstract class MyAttributeMetadataIndexerAttributeBase : Attribute
    {
        protected MyAttributeMetadataIndexerAttributeBase()
        {
        }

        public abstract Type AttributeType { get; }

        public abstract Type TargetType { get; }
    }
}

