namespace VRage.Trace
{
    using System;
    using System.Runtime.InteropServices;

    internal class MyNullTrace : ITrace
    {
        public void Send(string msg, string comment = null)
        {
        }

        public void Watch(string name, object value)
        {
        }
    }
}

