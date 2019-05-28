namespace VRage.Serialization
{
    using System;
    using VRage.Library.Collections;

    public interface INetObjectResolver
    {
        void Resolve<T>(BitStream stream, ref T obj) where T: class, IMyNetObject;
    }
}

