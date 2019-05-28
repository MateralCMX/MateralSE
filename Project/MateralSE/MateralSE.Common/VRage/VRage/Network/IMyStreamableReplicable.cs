namespace VRage.Network
{
    using System;
    using System.Collections.Generic;
    using VRage.Library.Collections;

    public interface IMyStreamableReplicable
    {
        void CreateStreamingStateGroup();
        IMyStateGroup GetStreamingStateGroup();
        void LoadCancel();
        void LoadDone(BitStream stream);
        void OnLoadBegin(Action<bool> loadingDoneHandler);
        void Serialize(BitStream stream, HashSet<string> cachedData, Endpoint forClient, Action writeData);

        bool NeedsToBeStreamed { get; }
    }
}

