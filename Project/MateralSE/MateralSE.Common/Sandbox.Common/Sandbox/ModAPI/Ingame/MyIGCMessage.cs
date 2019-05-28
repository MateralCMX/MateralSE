namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyIGCMessage
    {
        public readonly object Data;
        public readonly string Tag;
        public readonly long Source;
        public MyIGCMessage(object data, string tag, long source)
        {
            this.Tag = tag;
            this.Data = data;
            this.Source = source;
        }

        public TData As<TData>() => 
            ((TData) this.Data);
    }
}

