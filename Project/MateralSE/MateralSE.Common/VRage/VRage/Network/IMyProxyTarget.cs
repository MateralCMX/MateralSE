namespace VRage.Network
{
    public interface IMyProxyTarget : IMyNetObject, IMyEventOwner
    {
        IMyEventProxy Target { get; }
    }
}

