namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    public interface ISerializer<T>
    {
        void Deserialize(ByteStream source, out T data);
        void Serialize(ByteStream destination, ref T data);
    }
}

