namespace VRage.Game.Entity.UseObject
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyActionDescription
    {
        public MyStringId Text;
        public object[] FormatParams;
        public bool IsTextControlHint;
        public MyStringId? JoystickText;
        public object[] JoystickFormatParams;
    }
}

