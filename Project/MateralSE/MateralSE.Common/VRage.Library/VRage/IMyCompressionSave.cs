namespace VRage
{
    using System;

    public interface IMyCompressionSave : IDisposable
    {
        void Add(byte[] value);
        void Add(byte value);
        void Add(int value);
        void Add(float value);
        void Add(byte[] value, int count);
    }
}

