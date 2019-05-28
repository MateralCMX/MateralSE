namespace VRage.Meta
{
    using System;

    public interface IMyMetadataIndexer
    {
        void Activate();
        void Close();
        void Process();
        void SetParent(IMyMetadataIndexer indexer);
    }
}

