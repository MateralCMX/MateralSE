namespace VRage.Meta
{
    using System;

    public interface IMyTypeIndexer : IMyMetadataIndexer
    {
        void Index(Type type);
    }
}

