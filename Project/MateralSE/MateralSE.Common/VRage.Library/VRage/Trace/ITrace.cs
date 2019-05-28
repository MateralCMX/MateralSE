namespace VRage.Trace
{
    using System;
    using System.Runtime.InteropServices;

    public interface ITrace
    {
        void Send(string msg, string comment = null);
        void Watch(string name, object value);
    }
}

