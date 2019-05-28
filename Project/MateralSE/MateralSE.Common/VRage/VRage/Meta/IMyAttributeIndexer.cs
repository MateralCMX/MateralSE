namespace VRage.Meta
{
    using System;

    public interface IMyAttributeIndexer : IMyMetadataIndexer
    {
        void Observe(Attribute attribute, Type type);
    }
}

