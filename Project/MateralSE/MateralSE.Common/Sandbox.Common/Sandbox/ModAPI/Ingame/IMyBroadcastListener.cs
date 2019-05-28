namespace Sandbox.ModAPI.Ingame
{
    using System;

    public interface IMyBroadcastListener : IMyMessageProvider
    {
        string Tag { get; }

        bool IsActive { get; }
    }
}

