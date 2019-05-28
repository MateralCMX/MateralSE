namespace VRage.Network
{
    using System;

    public interface IMyNetObject : IMyEventOwner
    {
        bool IsValid { get; }
    }
}

