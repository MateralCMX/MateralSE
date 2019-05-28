namespace VRage.Generics
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    public interface IMyVariableStorage<T>
    {
        bool GetValue(MyStringId key, out T value);
        void SetValue(MyStringId key, T newValue);
    }
}

