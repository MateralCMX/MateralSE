namespace VRage.Scripting
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Script
    {
        public readonly string Name;
        public readonly string Code;
        public Script(string name, string code)
        {
            this.Name = name;
            this.Code = code;
        }
    }
}

